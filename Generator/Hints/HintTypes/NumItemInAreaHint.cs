namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class NumItemInAreaHint : Hint, IAreaHinter
    {
        public int count { get; }
        public Item item { get; }
        public AreaId areaId { get; }

        public NumItemInAreaHint(int count, Item item, AreaId areaId)
        {
            this.type = HintType.NumItemInArea;
            this.count = count;
            this.item = item;
            this.areaId = areaId;
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();
            hintText.text =
                $"They say that there are {count} {item.ToString()} at {{{areaId.tempToString()}}}.";
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeAsVlq16((ushort)count);
            result += SettingsEncoder.EncodeNumAsBits((int)item, 8);
            result += areaId.encodeAsBits(bitLengths);
            return result;
        }

        public static NumItemInAreaHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            int count = processor.NextVlq16();
            Item item = (Item)processor.NextByte();
            AreaId areaId = AreaId.decode(bitLengths, processor);
            return new NumItemInAreaHint(count, item, areaId);
        }

        public AreaId GetAreaId()
        {
            return areaId;
        }
    }
}
