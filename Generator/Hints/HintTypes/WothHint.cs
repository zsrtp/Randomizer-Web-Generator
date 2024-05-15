namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class WothHint : Hint
    {
        public override HintType type { get; } = HintType.Woth;

        public AreaId areaId { get; }
        public string checkName { get; }

        // always derived
        public Item item { get; private set; }

        public WothHint(
            AreaId areaId,
            string checkName,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.areaId = areaId;
            this.checkName = checkName;

            CalcDerived(itemPlacements);
        }

        private void CalcDerived(Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                item = HintUtils.getCheckContents(checkName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                item = HintUtils.getCheckContents(checkName);
            }
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
            return new WothHint(areaId, checkName, itemPlacements);
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            Res.Result hintParsedRes = Res.ParseVal("hint-type.woth");

            Res.Result areaRes = null;
            string areaText = "";
            bool didGenAreaPhrase = false;

            if (
                hintParsedRes.meta.TryGetValue("area-context", out string areaContextFromRes)
                && !StringUtils.isEmpty(areaContextFromRes)
            )
            {
                areaRes = Res.Msg(areaId.GenResKey(), new() { { "context", areaContextFromRes } });
                if (areaRes.MetaHasVal("area-context", areaContextFromRes))
                {
                    // Note: this treats "ap" as "none" since we are not
                    // doing more work. Can update the code to make use of
                    // "ap" meta (and use the subjectMeta) if needed.
                    areaText = areaRes.ResolveWithColor(CustomMessages.messageColorBlue);
                    didGenAreaPhrase = true;
                }
            }

            if (!didGenAreaPhrase)
            {
                areaRes = Res.Msg(areaId.GenResKey(), new() { { "context", "default" } });
                areaText = areaRes.ResolveWithColor(CustomMessages.messageColorBlue);
            }

            string verb = CustomMsgData.GenVerb(hintParsedRes, areaRes.meta);

            string text = hintParsedRes.Substitute(
                new() { { "area", areaText }, { "verb", verb } }
            );

            string normalizedText = Res.LangSpecificNormalize(text);

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            hintInfo.hintedCheck = checkName;
            hintInfo.hintedItems.Add(item);

            return hintInfo;
        }
    }
}
