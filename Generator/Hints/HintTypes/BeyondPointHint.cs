namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class BeyondPointHint : Hint
    {
        public bool isPositive { get; }

        public BeyondPointHint(bool isPositive)
        {
            this.type = HintType.BeyondPoint;
            this.isPositive = isPositive;
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();
            if (isPositive)
                hintText.text = "{Something good} beyond this point!";
            else
                hintText.text = "{Nothing} beyond this point!";
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += isPositive ? "1" : "0";
            return result;
        }

        public static BeyondPointHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            bool isPositive = processor.NextBool();
            return new BeyondPointHint(isPositive);
        }
    }
}
