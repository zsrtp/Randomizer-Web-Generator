namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.Marshalling;
    using TPRandomizer.Util;

    public class BeyondPointHint : Hint
    {
        public enum BeyondPointType
        {
            Nothing = 0,
            Good = 1,
            Only_Big_Keys = 2,
            Good_No_Big_Keys = 3,
            Good_And_Big_Keys = 4,
        }

        public override HintType type { get; } = HintType.BeyondPoint;
        public BeyondPointType beyondPointType { get; }

        public BeyondPointHint(BeyondPointType beyondPointType)
        {
            this.beyondPointType = beyondPointType;
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            string context = "";
            switch (beyondPointType)
            {
                case BeyondPointType.Good:
                    context = "good";
                    break;
                case BeyondPointType.Only_Big_Keys:
                    context = "only-big-keys";
                    break;
                case BeyondPointType.Good_No_Big_Keys:
                    context = "good-no-big-keys";
                    break;
                case BeyondPointType.Good_And_Big_Keys:
                    context = "good-and-big-keys";
                    break;
            }

            string text = Res.LangSpecificNormalize(
                Res.Msg("hint-type.beyond-point", new() { { "context", context } }).Substitute(null)
            );

            HintText hintText = new HintText();
            hintText.text = text;
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits((byte)beyondPointType, 3);
            return result;
        }

        public static BeyondPointHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            BeyondPointType beyondPointType = (BeyondPointType)processor.NextInt(3);
            return new BeyondPointHint(beyondPointType);
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            return hintInfo;
        }
    }
}
