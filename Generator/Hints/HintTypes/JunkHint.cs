namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class JunkHint : Hint
    {
        public override HintType type { get; } = HintType.Junk;

        public ushort idValue { get; private set; }
        public bool indicatesBarren { get; private set; }

        public JunkHint(Random rnd, bool indicatesBarren = false)
        {
            // this.idValue = (ushort)rnd.Next(ushort.MaxValue);
            this.idValue = 59;
            this.indicatesBarren = indicatesBarren;
        }

        // TODO: Temp public for testing
        public JunkHint(ushort idValue, bool indicatesBarren = false)
        {
            this.idValue = idValue;
            this.indicatesBarren = indicatesBarren;
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            string text = Res.GetJunkHintText(idValue);

            if (indicatesBarren)
            {
                text +=
                    " "
                    + Res.Msg("noun.barren-zone")
                        .ResolveWithColor(CustomMessages.messageColorPurple);
            }

            string normalizedText = Res.LangSpecificNormalize(text);

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(idValue, 16);
            result += indicatesBarren ? "1" : "0";
            return result;
        }

        public static JunkHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            ushort idValue = (ushort)processor.NextInt(16);
            bool indicatesBarren = processor.NextBool();
            return new JunkHint(idValue, indicatesBarren);
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            return hintInfo;
        }
    }
}
