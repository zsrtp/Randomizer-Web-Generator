namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class TradeChainHint : Hint
    {
        public enum AreaType
        {
            Zone = 0,
            Province = 1,
        }

        // stored
        public string srcCheckName { get; }
        public bool vaugeSourceItem { get; }
        public bool includeArea { get; }
        public AreaType areaType { get; }
        public CheckStatus checkStatus { get; }
        public CheckStatusDisplay checkStatusDisplay { get; }

        // derived but encoded
        public bool srcUseDefiniteArticle { get; private set; }
        public bool tgtUseDefiniteArticle { get; private set; }

        // derived and not encoded
        public string tgtCheckName { get; private set; }
        public Item srcItem { get; private set; }
        public Item tgtItem { get; private set; }

        public static TradeChainHint Create(
            HintGenData genData,
            string srcCheckName,
            bool vagueSourceItem,
            bool includeArea,
            AreaType areaType,
            CheckStatus checkStatus,
            CheckStatusDisplay checkStatusDisplay
        )
        {
            return new TradeChainHint(
                genData,
                srcCheckName,
                vagueSourceItem,
                includeArea,
                areaType,
                checkStatus,
                checkStatusDisplay
            );
        }

        private TradeChainHint(
            HintGenData genData,
            string srcCheckName,
            bool vagueSourceItem,
            bool includeArea,
            AreaType areaType,
            CheckStatus checkStatus,
            CheckStatusDisplay checkStatusDisplay,
            bool srcUseDefiniteArticle = false,
            bool tgtUseDefiniteArticle = false,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.type = HintType.TradeChain;
            this.srcCheckName = srcCheckName;
            this.vaugeSourceItem = vagueSourceItem;
            this.includeArea = includeArea;
            this.areaType = areaType;
            this.checkStatus = checkStatus;
            this.checkStatusDisplay = checkStatusDisplay;
            this.srcUseDefiniteArticle = srcUseDefiniteArticle;
            this.tgtUseDefiniteArticle = tgtUseDefiniteArticle;

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                srcItem = HintUtils.getCheckContents(srcCheckName, itemPlacements);
                tgtCheckName = HintUtils.GetTradeChainFinalCheck(srcCheckName, itemPlacements);
                tgtItem = HintUtils.getCheckContents(tgtCheckName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                srcItem = HintUtils.getCheckContents(srcCheckName);
                tgtCheckName = HintUtils.GetTradeChainFinalCheck(srcCheckName);
                tgtItem = HintUtils.getCheckContents(tgtCheckName);
            }

            // When creating the hint during generation, we calculate rather
            // than use input value.
            if (genData != null)
            {
                if (
                    genData.itemToChecksList.TryGetValue(
                        srcItem,
                        out List<string> checksGivingSrcItem
                    )
                )
                {
                    srcUseDefiniteArticle = checksGivingSrcItem.Count == 1;
                }

                if (
                    genData.itemToChecksList.TryGetValue(
                        tgtItem,
                        out List<string> checksGivingTgtItem
                    )
                )
                {
                    tgtUseDefiniteArticle = checksGivingTgtItem.Count == 1;
                }
            }
        }

        public override List<HintText> toHintTextList()
        {
            // leads to "item name", "something good", "nothing", "is on the WotH"

            // The target check will have a status which we can calc and store ahead of time.

            // Normally we will say the item by name.
            // If it is by name, then we can use a CheckStatusDisplay.

            // However, if it is vague, then we have different ways of displaying the status:
            // Required, Good, Bad, or Unhelpful.

            // If a check is Required, then it is also Good and we may to show
            // that status instead of Required.

            Res.Result hintParsedRes = Res.ParseVal("hint-type.trade-chain");

            bool areaLeadingSpace = hintParsedRes.SlotMetaHasVal("area-phrase", "space", "true");

            string srcText;
            Dictionary<string, string> srcItemMeta;

            if (vaugeSourceItem && HintUtils.isItemGoldenBug(srcItem))
            {
                Res.Result bugRes = Res.Msg("noun.a-bug", null);
                srcItemMeta = bugRes.meta;
                srcText = bugRes.ResolveWithColor(CustomMessages.messageColorYellow);
            }
            else
            {
                srcText = CustomMsgData.GenItemText3(
                    out srcItemMeta,
                    srcItem,
                    CheckStatus.Unknown,
                    srcUseDefiniteArticle ? "def" : "indef",
                    checkStatusDisplay: CheckStatusDisplay.None,
                    prefStartColor: CustomMessages.messageColorYellow
                );
            }

            string verb = CustomMsgData.GenVerb(hintParsedRes, srcItemMeta);

            AreaId areaId;

            if (areaType == AreaType.Province)
            {
                areaId = AreaId.Province(HintUtils.checkNameToHintProvince(srcCheckName));
            }
            else
            {
                areaId = AreaId.ZoneStr(HintUtils.checkNameToHintZone(srcCheckName));
            }

            string areaPhrase = "";
            if (includeArea)
            {
                if (areaLeadingSpace)
                    areaPhrase = " ";

                Res.Result tradeChainAreaRes = Res.Msg(
                    areaId.GenResKey(),
                    new() { { "context", "trade-chain" } }
                );
                if (tradeChainAreaRes.MetaHasVal("trade-chain", "true"))
                {
                    // Note: this treats "ap" as "none" since we are not
                    // doing more work. Can update the code to make use of
                    // "ap" meta (and use the subjectMeta) if needed.
                    areaPhrase += tradeChainAreaRes.ResolveWithColor(
                        CustomMessages.messageColorRed
                    );
                }
                else
                {
                    areaPhrase += CustomMsgData.GenAreaPhrase(
                        areaId,
                        srcItemMeta,
                        CustomMessages.messageColorRed
                    );
                }
            }

            string tgtText = CustomMsgData.GenItemText3(
                out Dictionary<string, string> tgtItemMeta,
                tgtItem,
                checkStatus,
                tgtUseDefiniteArticle ? "def" : "indef",
                checkStatusDisplay: checkStatusDisplay
            );

            string text = hintParsedRes.Substitute(
                new()
                {
                    { "source", srcText },
                    { "area-phrase", areaPhrase },
                    { "verb", verb },
                    { "target", tgtText }
                }
            );

            HintText hintText = new();
            hintText.text = Res.LangSpecificNormalize(text);
            return new() { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(srcCheckName),
                bitLengths.checkId
            );
            result += vaugeSourceItem ? "1" : "0";
            result += includeArea ? "1" : "0";
            result += SettingsEncoder.EncodeNumAsBits((int)areaType, 1);
            result += SettingsEncoder.EncodeNumAsBits((int)checkStatus, 2);
            result += SettingsEncoder.EncodeNumAsBits((int)checkStatusDisplay, 2);
            result += srcUseDefiniteArticle ? "1" : "0";
            result += tgtUseDefiniteArticle ? "1" : "0";
            return result;
        }

        public static TradeChainHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            int srcCheckId = processor.NextInt(bitLengths.checkId);
            string srcCheckName = CheckIdClass.GetCheckName(srcCheckId);

            bool vagueSource = processor.NextBool();
            bool includeArea = processor.NextBool();
            AreaType areaType = (AreaType)processor.NextInt(1);
            CheckStatus checkStatus = (CheckStatus)processor.NextInt(2);
            CheckStatusDisplay checkStatusDisplay = (CheckStatusDisplay)processor.NextInt(2);
            bool srcUseDefiniteArticle = processor.NextBool();
            bool tgtUseDefiniteArticle = processor.NextBool();

            TradeChainHint hint =
                new(
                    null,
                    srcCheckName,
                    vagueSource,
                    includeArea,
                    areaType,
                    checkStatus,
                    checkStatusDisplay,
                    srcUseDefiniteArticle,
                    tgtUseDefiniteArticle,
                    itemPlacements
                );

            return hint;
        }
    }
}
