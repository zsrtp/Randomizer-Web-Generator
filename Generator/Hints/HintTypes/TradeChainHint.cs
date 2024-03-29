namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
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

        // derived
        public string destCheckName { get; private set; }
        public Item srcItem { get; private set; }
        public Item destItem { get; private set; }

        public TradeChainHint(
            string srcCheckName,
            bool vagueSourceItem,
            bool includeArea,
            AreaType areaType,
            RewardVagueness rewardVagueness,
            RewardStatus rewardStatus
        )
        {
            this.type = HintType.TradeChain;
            this.srcCheckName = srcCheckName;
            this.vaugeSourceItem = vagueSourceItem;
            this.includeArea = includeArea;
            this.areaType = areaType;
            this.rewardVagueness = rewardVagueness;
            this.rewardStatus = rewardStatus;

            CalcDerived(null);
        }

        private TradeChainHint(
            string srcCheckName,
            bool vagueSourceItem,
            bool includeArea,
            AreaType areaType,
            RewardVagueness rewardVagueness,
            RewardStatus rewardStatus,
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

            CalcDerived(itemPlacements);
        }

        private void CalcDerived(Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                srcItem = HintUtils.getCheckContents(srcCheckName, itemPlacements);
                destCheckName = HintUtils.GetTradeChainFinalCheck(srcCheckName, itemPlacements);
                destItem = HintUtils.getCheckContents(destCheckName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                srcItem = HintUtils.getCheckContents(srcCheckName);
                destCheckName = HintUtils.GetTradeChainFinalCheck(srcCheckName);
                destItem = HintUtils.getCheckContents(destCheckName);
            }
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();

            string text = "They say that ";
            if (vaugeSourceItem && HintUtils.isItemGoldenBug(srcItem))
                text += "a {bug} ";
            else
                text += $"{{{srcItem}}} ";

            if (includeArea)
            {
                if (areaType == AreaType.Province)
                {
                    string provinceName = ProvinceUtils.IdToString(
                        HintUtils.checkNameToHintProvince(srcCheckName)
                    );
                    text += $"at {{Province:{provinceName}}} ";
                }
                else
                {
                    string zoneName = HintUtils.checkNameToHintZone(srcCheckName);
                    text += $"at {{Zone:{zoneName}}} ";
                }
            }

            // TODO: handle temp reward end stuff. Vagueness needs to be looked at.
            text += $"leads to {{{destItem}}}.";

            hintText.text = text;
            // $"They say that TradeItemChain {{{srcItem}}} is on the path to {{{destItem}}}.";
            return new List<HintText> { hintText };
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

            TradeChainHint hint =
                new(
                    srcCheckName,
                    vagueSource,
                    includeArea,
                    areaType,
                    rewardVagueness,
                    rewardStatus,
                    itemPlacements
                );

            return hint;
        }
    }
}
