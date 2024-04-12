namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class ItemHint : Hint
    {
        public AreaId areaId { get; }
        public string checkName { get; }

        // derived but encoded
        public bool useDefiniteArticle { get; private set; }

        // always derived
        public Item item { get; private set; }

        public static ItemHint Create(HintGenData genData, AreaId areaId, string checkName)
        {
            ItemHint hint = new(areaId, checkName, genData: genData);
            return hint;
        }

        private ItemHint(
            AreaId areaId,
            string checkName,
            bool useDefiniteArticle = false,
            Dictionary<int, byte> itemPlacements = null,
            HintGenData genData = null
        )
        {
            this.type = HintType.Item;

            this.areaId = areaId;
            this.checkName = checkName;
            this.useDefiniteArticle = useDefiniteArticle;

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                item = HintUtils.getCheckContents(checkName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                item = HintUtils.getCheckContents(checkName);
            }

            // TEST CODE
            item = Item.Male_Snail;

            if (genData != null)
            {
                // Do calc rather than use input value.
                if (genData.itemToChecksList.TryGetValue(item, out List<string> checksGivingItem))
                {
                    useDefiniteArticle = checksGivingItem.Count == 1;
                }
            }
        }

        // We encode the checkId since we want it to show up in the spoiler log.
        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += areaId.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(checkName),
                bitLengths.checkId
            );
            result += useDefiniteArticle ? "1" : "0";
            return result;
        }

        public static ItemHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            AreaId areaId = AreaId.decode(bitLengths, processor);
            int checkId = processor.NextInt(bitLengths.checkId);
            bool useDefiniteArticle = processor.NextBool();
            string checkName = CheckIdClass.GetCheckName(checkId);
            return new ItemHint(areaId, checkName, useDefiniteArticle, itemPlacements);
        }

        public override List<HintText> toHintTextList()
        {
            string itemText = CustomMsgData.GenItemText(
                out Dictionary<string, string> meta,
                item,
                useDefiniteArticle ? "def" : "indef",
                prefStartColor: CustomMessages.messageColorGreen
            );

            string areaPhrase = CustomMsgData.GenAreaPhrase(
                areaId,
                meta,
                CustomMessages.messageColorRed
            );

            Res.Result hintParsedRes = Res.ParseVal("hint-type.item");

            string verb = CustomMsgData.GenVerb(hintParsedRes, meta);

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
            return new List<HintText> { hintText };
        }
    }
}
