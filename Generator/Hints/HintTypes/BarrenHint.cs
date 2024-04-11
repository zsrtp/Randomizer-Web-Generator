namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class BarrenHint : Hint
    {
        public AreaId areaId { get; }

        public BarrenHint(AreaId areaId)
        {
            this.type = HintType.Barren;
            this.areaId = areaId;
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += areaId.encodeAsBits(bitLengths);
            return result;
        }

        public static BarrenHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            AreaId areaId = AreaId.decode(bitLengths, processor);
            return new BarrenHint(areaId);
        }

        public override List<HintText> toHintTextList()
        {
            Res.Result hintParsedRes = Res.ParseVal("hint-type.barren");

            string areaPhrase = CustomMsgData.GenAreaPhrase(
                areaId,
                null,
                CustomMessages.messageColorPurple
            );

            string text = hintParsedRes.Substitute(new() { { "area-phrase", areaPhrase } });

            string normalizedText = Res.LangSpecificNormalize(text);

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            // hintText.text =
            //     $"They say that there is {{nothing}} to be found in {{{areaId.tempToString()}}}.";
            return new List<HintText> { hintText };
        }
    }
}
