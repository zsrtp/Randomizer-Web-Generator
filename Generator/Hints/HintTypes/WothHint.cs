namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class WothHint : Hint
    {
        public AreaId areaId { get; }

        public string checkName { get; }

        public WothHint(AreaId areaId, string checkName)
        {
            this.type = HintType.Woth;
            this.areaId = areaId;
            this.checkName = checkName;
        }

        // Need to encode the kind of id, the id, and the checkName. A single
        // checkName could be in multiple categories, so need the categoryId. It
        // is not possible to determine the checkName with just the area, so
        // need the checkId.
        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += areaId.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(checkName),
                bitLengths.checkId
            );
            return result;
        }

        public static WothHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            AreaId areaId = AreaId.decode(bitLengths, processor);
            int checkId = processor.NextInt(bitLengths.checkId);
            string checkName = CheckIdClass.GetCheckName(checkId);
            return new WothHint(areaId, checkName);
        }

        public override List<HintText> toHintTextList()
        {
            Res.Result ppp = Res.ParseVal(areaId.GenResKey());

            string verbContext = CustomMsgData.BuildContextFromMeta(ppp.meta);

            string areaText = ppp.ResolveWithColor(
                CustomMessages.messageColorYellow,
                CustomMessages.messageColorWhite
            );

            Res.Result hintParsedRes = Res.ParseVal("hint-type.woth");

            string verb = "";
            if (hintParsedRes.slotMeta.TryGetValue("verb", out Dictionary<string, string> verbMeta))
            {
                if (verbMeta.TryGetValue("name", out string verbName))
                {
                    verb = Res.SimpleMsgOld(
                        "verb." + verbName,
                        new() { { "context", verbContext } }
                    );
                }
            }

            string text = hintParsedRes.Substitute(
                new() { { "area", areaText }, { "verb", verb } }
            );

            string normalizedText = Res.LangSpecificNormalize(text);

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }
    }
}
