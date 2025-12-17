namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class TradeChainHint : Hint
    {
        public override HintType type { get; } = HintType.TradeChain;

        public enum AreaType
        {
            Zone = 0,
            Province = 1,
        }

        // Used during initial creation, but not stored.
        public AreaType areaType { get; }

        // stored
        public string srcCheckName { get; }
        public bool vaugeSourceItem { get; }
        public bool includeArea { get; }
        public DetailedCheckStatus checkStatus { get; }
        public CheckStatusDisplay checkStatusDisplay { get; }

        // derived but encoded
        public AreaId srcAreaId { get; private set; }
        public bool srcUseDefiniteArticle { get; private set; }
        public bool tgtUseDefiniteArticle { get; private set; }
        public bool tgtIsLogicalItem { get; private set; }

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
            DetailedCheckStatus checkStatus,
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
            DetailedCheckStatus checkStatus,
            CheckStatusDisplay checkStatusDisplay,
            AreaId srcAreaId = null,
            bool srcUseDefiniteArticle = false,
            bool tgtUseDefiniteArticle = false,
            bool tgtIsLogicalItem = false,
            Dictionary<int, int> itemPlacements = null
        )
        {
            this.srcCheckName = srcCheckName;
            this.vaugeSourceItem = vagueSourceItem;
            this.includeArea = includeArea;
            this.areaType = areaType;
            this.checkStatus = checkStatus;
            this.checkStatusDisplay = checkStatusDisplay;
            this.srcAreaId = srcAreaId;
            this.srcUseDefiniteArticle = srcUseDefiniteArticle;
            this.tgtUseDefiniteArticle = tgtUseDefiniteArticle;
            this.tgtIsLogicalItem = tgtIsLogicalItem;

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, int> itemPlacements)
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
                string srcZoneName = genData.GetZoneNameForCheck(srcCheckName);
                if (areaType == AreaType.Zone)
                    srcAreaId = AreaId.ZoneStr(srcZoneName);
                else
                {
                    Zone srcZone = ZoneUtils.StringToIdThrows(srcZoneName);
                    Province srcProvince = ProvinceUtils.ZoneToProvince(srcZone);
                    srcAreaId = AreaId.Province(srcProvince);
                }

                if (genData.logicalItems.Contains(tgtItem))
                    tgtIsLogicalItem = true;

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

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            // leads to "item name", "something good", "nothing", "is on the WotH"

            // The target check will have a status which we can calc and store ahead of time.

            // Normally we will say the item by name.
            // If it is by name, then we can use a CheckStatusDisplay.

            // However, if it is vague, then we have different ways of displaying the status:
            // Required, Good, Bad, or Unhelpful.

            // If a check is Required, then it is also Good and we may to show
            // that status instead of Required.

            Res.Result hintParsedRes = Res.Msg("hint-type.trade-chain");

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
                srcText = customMsgData.GenItemText4(
                    out srcItemMeta,
                    srcItem,
                    DetailedCheckStatus.Unknown,
                    srcUseDefiniteArticle ? "def" : "indef",
                    checkStatusDisplay: CheckStatusDisplay.None,
                    prefStartColor: CustomMessages.messageColorYellow
                );
            }

            string verb = CustomMsgData.GenVerb(hintParsedRes, srcItemMeta);

            Dictionary<string, string> metaForArea = new();
            foreach (KeyValuePair<string, string> pair in srcItemMeta)
            {
                // We are only ever hinting one instance of an item for this
                // hint, so the area should not be forced to plural. For
                // example, if we say "the Iron Boots" (not that we would use
                // this as a srcItem) then we should still say "a dungeon" and
                // not "the dungeons". We use this same logic for ItemHints
                // where it is more relevant.
                if (pair.Key != "plural")
                    metaForArea[pair.Key] = pair.Value;
            }

            string areaPhrase = "";
            if (includeArea)
            {
                if (areaLeadingSpace)
                    areaPhrase = " ";

                Res.Result tradeChainAreaRes = Res.Msg(
                    srcAreaId.GenResKey(),
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
                        srcAreaId,
                        metaForArea,
                        CustomMessages.messageColorRed
                    );
                }
            }

            string tgtText = customMsgData.GenItemText4(
                out Dictionary<string, string> tgtItemMeta,
                tgtItem,
                checkStatus,
                tgtUseDefiniteArticle ? "def" : "indef",
                checkStatusDisplay: checkStatusDisplay,
                isLogicalItem: tgtIsLogicalItem
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
            result += SettingsEncoder.EncodeNumAsBits((int)checkStatus, bitLengths.checkStatus);
            result += SettingsEncoder.EncodeNumAsBits((int)checkStatusDisplay, 2);
            result += srcAreaId.encodeAsBits(bitLengths);
            result += srcUseDefiniteArticle ? "1" : "0";
            result += tgtUseDefiniteArticle ? "1" : "0";
            result += tgtIsLogicalItem ? "1" : "0";
            return result;
        }

        public static TradeChainHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, int> itemPlacements
        )
        {
            int srcCheckId = processor.NextInt(bitLengths.checkId);
            string srcCheckName = CheckIdClass.GetCheckName(srcCheckId);

            bool vagueSource = processor.NextBool();
            bool includeArea = processor.NextBool();
            DetailedCheckStatus checkStatus = (DetailedCheckStatus)processor.NextInt(
                bitLengths.checkStatus
            );
            CheckStatusDisplay checkStatusDisplay = (CheckStatusDisplay)processor.NextInt(2);
            AreaId srcAreaId = AreaId.decode(bitLengths, processor);
            bool srcUseDefiniteArticle = processor.NextBool();
            bool tgtUseDefiniteArticle = processor.NextBool();
            bool tgtIsLogicalItem = processor.NextBool();

            // Note: we just pass AreaType.Zone since it isn't used when decoding. We don't bother
            // storing it for this reason.
            TradeChainHint hint =
                new(
                    null,
                    srcCheckName,
                    vagueSource,
                    includeArea,
                    AreaType.Zone,
                    checkStatus,
                    checkStatusDisplay,
                    srcAreaId,
                    srcUseDefiniteArticle,
                    tgtUseDefiniteArticle,
                    tgtIsLogicalItem,
                    itemPlacements
                );

            return hint;
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            hintInfo.sourceCheck = srcCheckName;
            hintInfo.sourceItem = srcItem;
            hintInfo.targetCheck = tgtCheckName;
            hintInfo.targetItem = tgtItem;

            return hintInfo;
        }
    }
}
