namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class NumItemInAreaHint : Hint, IAreaHinter
    {
        public int count { get; }
        public Item item { get; }
        public AreaId areaId { get; }

        public NumItemInAreaHint(int count, Item item, AreaId areaId)
        {
            this.type = HintType.NumItemInArea;
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

        public override List<HintText> toHintTextList()
        {
            string itemText = CustomMsgData.GenItemText(
                out Dictionary<string, string> itemMeta,
                item,
                contextIn: "count",
                count: count.ToString(),
                prefStartColor: CustomMessages.messageColorGreen
            );

            // Construct hashSet of relevant contexts for a given key.

            // string areaResKey = areaId.GenResKey();

            // string areaResContext = "default";
            // if (!ListUtils.isEmpty(itemMeta))
            // {
            //     Dictionary<string, string> areaResContextDict = Res.FilterToRelevantContext(
            //         areaResKey,
            //         itemMeta
            //     );
            //     areaResContext = CustomMsgData.BuildContextFromMeta(areaResContextDict);
            // }

            Res.ParsedRes aa2 = Res.ParseValRelevantContext(areaId.GenResKey(), itemMeta);

            // Res.ParsedRes aa = Res.ParseVal(
            //     areaId.GenResKey(),
            //     new() { { "context", areaResContext } }
            // );
            string areaString = aa2.ResolveWithColors(
                CustomMessages.messageColorRed,
                CustomMessages.messageColorWhite
            );

            string context = CustomMsgData.BuildContextFromMeta(itemMeta);

            if (!aa2.meta.TryGetValue("ap", out string areaPhraseKey))
                areaPhraseKey = "default";

            Res.ParsedRes parsedRes2 = Res.ParseVal($"area-phrase.{areaPhraseKey}");
            string areaPhrase = parsedRes2.Substitute(new() { { "area", areaString } });

            Res.ParsedRes hintParsedRes = Res.ParseVal("hint-type.item");

            string verb = "";
            if (hintParsedRes.slotMeta.TryGetValue("verb", out Dictionary<string, string> verbMeta))
            {
                if (verbMeta.TryGetValue("name", out string verbName))
                {
                    verb = Res.Msg("verb." + verbName, new() { { "context", context } });
                }
            }

            string text = hintParsedRes.Substitute(
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
            // hintText.text =
            //     $"They say that {{{item}}} can be found at {{{areaId.tempToString()}}}.";
            return new List<HintText> { hintText };
        }
    }
}
