namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class ItemHint : Hint
    {
        public AreaId areaId { get; }
        public string checkName { get; }
        public bool isPositive { get; }

        public Item item { get; }

        public ItemHint(AreaId areaId, string checkName, Item item, bool isPositive)
        {
            this.type = HintType.Item;
            this.areaId = areaId;
            this.checkName = checkName;
            this.isPositive = isPositive;

            this.item = item;
        }

        public override List<HintText> toHintTextList()
        {
            // string negativeText = isPositive ? "" : " NOT";

            HintText hintText = new HintText();
            hintText.text =
                $"They say that {{{item}}} can be found at {{{areaId.tempToString()}}}.";
            // hintText.text = $"{item.ToString()} is {negativeText} at {id}.";
            return new List<HintText> { hintText };
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
            result += isPositive ? "1" : "0";
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
            string checkName = CheckIdClass.GetCheckName(checkId);
            bool isPositive = processor.NextBool();
            Item item = (Item)itemPlacements[checkId];
            return new ItemHint(areaId, checkName, item, isPositive);
        }
    }
}
