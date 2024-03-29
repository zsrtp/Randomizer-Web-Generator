namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class NullHint : Hint
    {
        public HintType intendedHintType { get; }

        public NullHint(HintType intendedHintType)
        {
            this.intendedHintType = intendedHintType;
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();
            hintText.text = $"Hint should have been {intendedHintType.ToString()}.";
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits((int)intendedHintType, bitLengths.hintType);
            return result;
        }

        public static NullHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            HintType intededHintType = (HintType)processor.NextInt(bitLengths.hintType);
            return new NullHint(intededHintType);
        }
    }
}
