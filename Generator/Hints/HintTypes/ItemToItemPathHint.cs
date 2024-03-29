namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class ItemToItemPathHint : Hint
    {
        public Item srcItem { get; }
        public string tgtCheckName { get; }

        // derived
        public Item tgtItem { get; private set; }

        public ItemToItemPathHint(Item srcItem, string tgtCheckName)
        {
            this.type = HintType.ItemToItemPath;
            this.srcItem = srcItem;
            this.tgtCheckName = tgtCheckName;

            CalcDerived(null);
        }

        private ItemToItemPathHint(
            Item srcItem,
            string tgtCheckName,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.type = HintType.ItemToItemPath;
            this.srcItem = srcItem;
            this.tgtCheckName = tgtCheckName;

            CalcDerived(itemPlacements);
        }

        private void CalcDerived(Dictionary<int, byte> itemPlacements)
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
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();
            hintText.text = $"They say that {{{srcItem}}} is on the path to {{{tgtItem}}}.";
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

            return new ItemToItemPathHint(srcItem, destCheckName, itemPlacements);
        }
    }
}
