namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class BeyondPointHint : Hint
    {
        public override HintType type { get; } = HintType.BeyondPoint;

        public bool isPositive { get; }

        public BeyondPointHint(bool isPositive)
        {
            this.isPositive = isPositive;
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            string text = Res.LangSpecificNormalize(
                Res.Msg("hint-type.beyond-point", new() { { "context", isPositive ? "good" : "" } })
                    .Substitute(null)
            );

            HintText hintText = new HintText();
            hintText.text = text;
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

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            return hintInfo;
        }
    }
}
