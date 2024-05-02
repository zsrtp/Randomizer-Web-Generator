namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class TradeChainHint : Hint
    {
        public enum RewardVagueness
        {
            Named = 0,
            Vague = 1,
            Required = 2,
            Unhelpful = 3,
        }

        public enum RewardStatus
        {
            Bad = 0,
            Good = 1,
            Required = 2,
        }

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
        public RewardVagueness rewardVagueness { get; }
        public RewardStatus rewardStatus { get; }

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
            RewardVagueness rewardVagueness,
            RewardStatus rewardStatus
        )
        {
            return new TradeChainHint(
                genData,
                srcCheckName,
                vagueSourceItem,
                includeArea,
                areaType,
                rewardVagueness,
                rewardStatus
            );
        }

        // public TradeChainHint(
        //     string srcCheckName,
        //     bool vagueSourceItem,
        //     bool includeArea,
        //     AreaType areaType,
        //     RewardVagueness rewardVagueness,
        //     RewardStatus rewardStatus
        // )
        // {
        //     this.type = HintType.TradeChain;
        //     this.srcCheckName = srcCheckName;
        //     this.vaugeSourceItem = vagueSourceItem;
        //     this.includeArea = includeArea;
        //     this.areaType = areaType;
        //     this.rewardVagueness = rewardVagueness;
        //     this.rewardStatus = rewardStatus;

        //     CalcDerived(null);
        // }

        private TradeChainHint(
            HintGenData genData,
            string srcCheckName,
            bool vagueSourceItem,
            bool includeArea,
            AreaType areaType,
            RewardVagueness rewardVagueness,
            RewardStatus rewardStatus,
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
            this.rewardVagueness = rewardVagueness;
            this.rewardStatus = rewardStatus;
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
            Res.Result hintParsedRes = Res.ParseVal("hint-type.trade-chain");

            bool areaLeadingSpace = hintParsedRes.SlotMetaHasVal("area-phrase", "space", "true");

            // Src is either the item name, or if it is vague and a bug, then we say "an insect".

            string srcText = CustomMsgData.GenItemText3(
                out Dictionary<string, string> srcItemMeta,
                srcItem,
                CheckStatus.Unknown,
                srcUseDefiniteArticle ? "def" : "indef",
                checkStatusDisplay: CheckStatusDisplay.None,
                prefStartColor: CustomMessages.messageColorYellow
            );

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
                areaPhrase += CustomMsgData.GenAreaPhrase(
                    areaId,
                    srcItemMeta,
                    CustomMessages.messageColorRed
                );
            }

            CheckStatus tgtStatus = CheckStatus.Unknown;
            if (rewardStatus == RewardStatus.Required)
                tgtStatus = CheckStatus.Required;
            else if (rewardStatus == RewardStatus.Good)
                tgtStatus = CheckStatus.Good;
            else if (rewardStatus == RewardStatus.Bad)
                tgtStatus = CheckStatus.Bad;

            string tgtText = CustomMsgData.GenItemText3(
                out Dictionary<string, string> tgtItemMeta,
                tgtItem,
                tgtStatus,
                tgtUseDefiniteArticle ? "def" : "indef",
                checkStatusDisplay: CheckStatusDisplay.None
            // prefStartColor: CustomMessages.messageColorBlue
            );

            string text2 = hintParsedRes.Substitute(
                new()
                {
                    { "source", srcText },
                    { "area-phrase", areaPhrase },
                    { "verb", verb },
                    { "target", tgtText }
                }
            );

            List<HintText> results = new();

            List<AreaId> areaIds =
                new()
                {
                    // zones
                    // AreaId.Zone(Zone.Ordon),
                    // AreaId.Zone(Zone.Sacred_Grove),
                    // AreaId.Zone(Zone.Faron_Field),
                    // AreaId.Zone(Zone.Faron_Woods),
                    // AreaId.Zone(Zone.Kakariko_Gorge),
                    // AreaId.Zone(Zone.Kakariko_Village),
                    // AreaId.Zone(Zone.Kakariko_Graveyard),
                    // AreaId.Zone(Zone.Eldin_Field),
                    // AreaId.Zone(Zone.North_Eldin),
                    // AreaId.Zone(Zone.Death_Mountain),
                    // AreaId.Zone(Zone.Hidden_Village),
                    // AreaId.Zone(Zone.Lanayru_Field),
                    // AreaId.Zone(Zone.Beside_Castle_Town),
                    // AreaId.Zone(Zone.South_of_Castle_Town),
                    // AreaId.Zone(Zone.Castle_Town),
                    // AreaId.Zone(Zone.Agithas_Castle),
                    // AreaId.Zone(Zone.Great_Bridge_of_Hylia),
                    // AreaId.Zone(Zone.Lake_Hylia),
                    // AreaId.Zone(Zone.Lake_Lantern_Cave),
                    // AreaId.Zone(Zone.Lanayru_Spring),
                    // AreaId.Zone(Zone.Zoras_Domain),
                    // AreaId.Zone(Zone.Upper_Zoras_River),
                    // AreaId.Zone(Zone.Gerudo_Desert),
                    // AreaId.Zone(Zone.Bulblin_Camp),
                    // AreaId.Zone(Zone.Snowpeak),
                    // AreaId.Zone(Zone.Cave_of_Ordeals),
                    // AreaId.Zone(Zone.Forest_Temple),
                    // AreaId.Zone(Zone.Goron_Mines),
                    // AreaId.Zone(Zone.Lakebed_Temple),
                    // AreaId.Zone(Zone.Arbiters_Grounds),
                    // AreaId.Zone(Zone.Snowpeak_Ruins),
                    // AreaId.Zone(Zone.Temple_of_Time),
                    // AreaId.Zone(Zone.City_in_the_Sky),
                    // AreaId.Zone(Zone.Palace_of_Twilight),
                    // AreaId.Zone(Zone.Hyrule_Castle),
                    // provinces
                    // AreaId.Province(Province.Ordona),
                    // AreaId.Province(Province.Faron),
                    // AreaId.Province(Province.Eldin),
                    // AreaId.Province(Province.Lanayru),
                    // AreaId.Province(Province.Desert),
                    // AreaId.Province(Province.Peak),
                    // AreaId.Province(Province.Dungeon),
                    // asdf
                    AreaId.Category(HintCategory.Grotto),
                    AreaId.Category(HintCategory.Mist),
                    AreaId.Category(HintCategory.Owl_Statue),
                    AreaId.Category(HintCategory.Llc_Lantern_Chests),
                    AreaId.Category(HintCategory.Underwater),
                    AreaId.Category(HintCategory.Upper_Desert),
                    AreaId.Category(HintCategory.Lower_Desert),
                    AreaId.Category(HintCategory.Golden_Wolf),
                };

            foreach (AreaId areaId1 in areaIds)
            {
                areaPhrase = "";
                if (includeArea)
                {
                    if (areaLeadingSpace)
                        areaPhrase = " ";

                    Res.Result tradeChainAreaRes = Res.Msg(
                        areaId1.GenResKey(),
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
                            areaId1,
                            srcItemMeta,
                            CustomMessages.messageColorRed
                        );
                    }
                }

                string textForArea = hintParsedRes.Substitute(
                    new()
                    {
                        { "source", srcText },
                        { "area-phrase", areaPhrase },
                        { "verb", verb },
                        { "target", tgtText }
                    }
                );

                HintText hintText = new HintText();
                hintText.text = Res.LangSpecificNormalize(textForArea);
                results.Add(hintText);
            }

            // HintText hintText = new HintText();

            // "Il est dit que {source} mène à {target}."
            // "Il est dit que {source} {area-phrase} mène à {target}."

            // string text = "They say that ";
            // if (vaugeSourceItem && HintUtils.isItemGoldenBug(srcItem))
            //     text += "a {bug} ";
            // else
            //     text += $"{{{srcItem}}} ";

            // if (includeArea)
            // {
            //     if (areaType == AreaType.Province)
            //     {
            //         string provinceName = ProvinceUtils.IdToString(
            //             HintUtils.checkNameToHintProvince(srcCheckName)
            //         );
            //         text += $"at {{Province:{provinceName}}} ";
            //     }
            //     else
            //     {
            //         string zoneName = HintUtils.checkNameToHintZone(srcCheckName);
            //         text += $"at {{Zone:{zoneName}}} ";
            //     }
            // }

            // // TODO: handle temp reward end stuff. Vagueness needs to be looked at.
            // text += $"leads to {{{tgtItem}}}.";

            // hintText.text = Res.LangSpecificNormalize(text2);

            // HintText hintText2 = new();
            // hintText2.text = "2nd hint text!";

            // return new List<HintText> { hintText, hintText2 };
            return results;
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
            result += SettingsEncoder.EncodeNumAsBits((int)rewardVagueness, 2);
            result += SettingsEncoder.EncodeNumAsBits((int)rewardStatus, 2);
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
            RewardVagueness rewardVagueness = (RewardVagueness)processor.NextInt(2);
            RewardStatus rewardStatus = (RewardStatus)processor.NextInt(2);
            bool srcUseDefiniteArticle = processor.NextBool();
            bool tgtUseDefiniteArticle = processor.NextBool();

            TradeChainHint hint =
                new(
                    null,
                    srcCheckName,
                    vagueSource,
                    includeArea,
                    areaType,
                    rewardVagueness,
                    rewardStatus,
                    srcUseDefiniteArticle,
                    tgtUseDefiniteArticle,
                    itemPlacements
                );

            return hint;
        }
    }
}
