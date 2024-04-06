namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
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
            string itemText = CustomMsgData.GenItemText2(
                out Dictionary<string, string> meta,
                item,
                useDefiniteArticle ? "def" : "indef"
            );

            string context = CustomMsgData.BuildContextFromMeta(meta);

            // get resource from context
            Res.ParsedRes parsedRes = Res.ParseVal(
                "hint-type.item",
                new() { { "context", context } }
            );

            string text = parsedRes.Substitute(new() { { "item", itemText } });

            HintText hintText = new HintText();
            hintText.text =
                $"They say that {{{item}}} can be found at {{{areaId.tempToString()}}}.";
            return new List<HintText> { hintText };
        }
    }
}
