namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class ItemToItemPathHint : Hint
    {
        public override HintType type { get; } = HintType.ItemToItemPath;

        public Item srcItem { get; }
        public string tgtCheckName { get; }

        // derived but encoded
        public bool srcUseDefiniteArticle { get; private set; }

        // always derived
        public Item tgtItem { get; private set; }

        public static ItemToItemPathHint Create(
            HintGenData genData,
            Item srcItem,
            string tgtCheckName
        )
        {
            ItemToItemPathHint hint = new(genData, srcItem, tgtCheckName);
            return hint;
        }

        private ItemToItemPathHint(
            HintGenData genData,
            Item srcItem,
            string tgtCheckName,
            bool srcUseDefiniteArticle = false,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.srcItem = srcItem;
            this.tgtCheckName = tgtCheckName;
            this.srcUseDefiniteArticle = srcUseDefiniteArticle;

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                tgtItem = HintUtils.getCheckContents(tgtCheckName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                tgtItem = HintUtils.getCheckContents(tgtCheckName);
            }

            // When creating the hint during generation, we calculate rather
            // than use input value.
            if (genData != null)
            {
                if (
                    genData.itemToChecksList.TryGetValue(srcItem, out List<string> checksGivingItem)
                )
                {
                    srcUseDefiniteArticle = checksGivingItem.Count == 1;
                }
            }
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            Res.Result hintParsedRes = Res.ParseVal("hint-type.item-to-item-path");

            string srcText = customMsgData.GenItemText3(
                out Dictionary<string, string> srcItemMeta,
                srcItem,
                CheckStatus.Unknown,
                srcUseDefiniteArticle ? "def" : "indef",
                checkStatusDisplay: CheckStatusDisplay.None,
                prefStartColor: CustomMessages.messageColorGreen
            );

            string verb = CustomMsgData.GenVerb(hintParsedRes, srcItemMeta);

            string tgtText = customMsgData.GenItemText3(
                out Dictionary<string, string> tgtItemMeta,
                tgtItem,
                CheckStatus.Required,
                "def",
                checkStatusDisplay: CheckStatusDisplay.None,
                prefStartColor: CustomMessages.messageColorBlue
            );

            string text = hintParsedRes.Substitute(
                new() { { "source", srcText }, { "verb", verb }, { "target", tgtText }, }
            );

            // string text = $"They say that {{{srcItem}}} is on the path to {{{tgtItem}}}.";

            HintText hintText = new HintText();
            // hintText.text = $"They say that {{{srcItem}}} is on the path to {{{tgtItem}}}.";
            hintText.text = Res.LangSpecificNormalize(text);
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits((int)srcItem, 8);
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(tgtCheckName),
                bitLengths.checkId
            );
            result += srcUseDefiniteArticle ? "1" : "0";
            return result;
        }

        public static ItemToItemPathHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            Item srcItem = (Item)processor.NextByte();

            int destCheckId = processor.NextInt(bitLengths.checkId);
            string destCheckName = CheckIdClass.GetCheckName(destCheckId);
            bool srcUseDefiniteArticle = processor.NextBool();

            return new ItemToItemPathHint(
                null,
                srcItem,
                destCheckName,
                srcUseDefiniteArticle,
                itemPlacements
            );
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            hintInfo.sourceItem = srcItem;
            hintInfo.targetCheck = tgtCheckName;
            hintInfo.targetItem = tgtItem;

            return hintInfo;
        }
    }
}
