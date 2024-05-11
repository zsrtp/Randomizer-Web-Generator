namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class BarrenHint : Hint
    {
        public override HintType type { get; } = HintType.Barren;

        public AreaId areaId { get; }

        public BarrenHint(AreaId areaId)
        {
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

            string areaPhrase;

            // We first try to pick an area with barren context (some areas may
            // have a special way of being phrased for barren hints). To confirm
            // that we resolved specifically to "barren" context and not a
            // fallback, the resource should provide "barren" as true in its
            // meta. If we do not find one for "barren", then we fall back to
            // default behavior.
            Res.Result barrenAreaRes = Res.Msg(
                areaId.GenResKey(),
                new() { { "context", "barren" } }
            );
            if (
                barrenAreaRes.meta.TryGetValue("barren", out string barrenVal)
                && barrenVal == "true"
            )
            {
                areaPhrase = barrenAreaRes.ResolveWithColor(CustomMessages.messageColorPurple);
            }
            else
            {
                areaPhrase = CustomMsgData.GenAreaPhrase(
                    areaId,
                    new() { { "default", "true" } },
                    CustomMessages.messageColorPurple
                );
            }

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
