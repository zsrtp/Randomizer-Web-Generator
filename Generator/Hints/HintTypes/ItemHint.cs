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

            // context depends on if only copy of item.

            // Need to get context based on how many of the item can be found.
            // We should use a static method which takes in the genData (as seen
            // in LocationHint). Also, let's get rid of isPositive and only add
            // it if we will use it which is probably not going to happen.

            string itemText = CustomMsgData.GenItemText2(
                out Dictionary<string, string> meta,
                item,
                "def"
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
