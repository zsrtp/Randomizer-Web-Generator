namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class NumItemInAreaHint : Hint, IAreaHinter
    {
        public override HintType type { get; } = HintType.NumItemInArea;

        public int count { get; }
        public Item item { get; }
        public AreaId areaId { get; }

        public NumItemInAreaHint(int count, Item item, AreaId areaId)
        {
            this.count = count;
            this.item = item;
            this.areaId = areaId;
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeAsVlq16((ushort)count);
            result += SettingsEncoder.EncodeNumAsBits((int)item, 8);
            result += areaId.encodeAsBits(bitLengths);
            return result;
        }

        public static NumItemInAreaHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            int count = processor.NextVlq16();
            Item item = (Item)processor.NextByte();
            AreaId areaId = AreaId.decode(bitLengths, processor);
            return new NumItemInAreaHint(count, item, areaId);
        }

        public AreaId GetAreaId()
        {
            return areaId;
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            string itemText = CustomMsgData.GenItemText(
                out Dictionary<string, string> itemMeta,
                item,
                contextIn: "count",
                count: count,
                prefStartColor: CustomMessages.messageColorGreen
            );

            string areaPhrase = CustomMsgData.GenAreaPhrase(
                areaId,
                itemMeta,
                CustomMessages.messageColorRed
            );

            Res.Result hintTypeRes = Res.ParseVal("hint-type.item");

            string verb = "";
            if (hintTypeRes.slotMeta.TryGetValue("verb", out Dictionary<string, string> verbMeta))
            {
                if (verbMeta.TryGetValue("name", out string verbName))
                {
                    string verbResKey = "verb." + verbName;
                    verb = Res.SimpleMsg(verbResKey, null, itemMeta);
                }
            }

            string text = hintTypeRes.Substitute(
                new() { { "item", itemText }, { "verb", verb }, { "area-phrase", areaPhrase }, }
            );

            // Find how to pass in the languages
            string normalizedText = Res.LangSpecificNormalize(text);

            // Then we need to do language specific substitutions:

            // For example, if it is french for "hint-type.item", then we need
            // to go through and convert any "que un" or "que une" to "qu'un" or
            // "qu'une" as well as "du une" to "d'une", etc.

            // ^ Need to figure out how to preserve the positions of escaped
            // sequences when doing this.

            // Once we have the finalized text (including the colors), then we
            // can do the adding the line breaks part. This could potentially be
            // done per language as well, but it is more of a question of "is
            // the game JP" or not as opposed to the resource languages, etc.

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }
    }
}
