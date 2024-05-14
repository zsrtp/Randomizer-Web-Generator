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
        public List<string> checkNames { get; }

        public NumItemInAreaHint(int count, Item item, AreaId areaId, List<string> checkNames)
        {
            this.count = count;
            this.item = item;
            this.areaId = areaId;
            this.checkNames = checkNames;

            if (this.checkNames == null)
                this.checkNames = new();
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeAsVlq16((ushort)count);
            result += SettingsEncoder.EncodeNumAsBits((int)item, 8);
            result += areaId.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeAsVlq16((ushort)checkNames.Count);
            for (int i = 0; i < checkNames.Count; i++)
            {
                string checkName = checkNames[i];
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
            }
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

            int numCheckNames = processor.NextVlq16();

            List<string> checkNames = new();
            for (int i = 0; i < numCheckNames; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                checkNames.Add(CheckIdClass.GetCheckName(checkId));
            }

            return new NumItemInAreaHint(count, item, areaId, checkNames);
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

            Res.Result hintTypeRes = Res.ParseVal("hint-type.num-item-in-area");

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
                new() { { "subject", itemText }, { "verb", verb }, { "area-phrase", areaPhrase }, }
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

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            hintInfo.hintedChecks.AddRange(checkNames);
            hintInfo.hintedItems.Add(item);
            return hintInfo;
        }
    }
}
