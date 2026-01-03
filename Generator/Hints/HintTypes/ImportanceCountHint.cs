namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using System.Linq;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class ImportanceCountHint : Hint
    {
        public override HintType type { get; } = HintType.ImportanceCount;

        public AreaId areaId { get; private set; }
        public bool hasRelevantDependentChecks { get; private set; }
        public bool indicatesImportant { get; private set; }
        public HashSet<string> importantChecks { get; private set; }
        public HashSet<string> majorChecks { get; private set; }

        // derived and not encoded
        private List<string> combinedChecksList;
        public List<Item> items { get; private set; }

        public ImportanceCountHint(
            AreaId areaId,
            bool hasRelevantDependentChecks,
            bool indicatesImportant,
            HashSet<string> importantChecks,
            HashSet<string> majorChecks,
            Dictionary<int, int> itemPlacements = null
        )
        {
            this.areaId = areaId;
            this.hasRelevantDependentChecks = hasRelevantDependentChecks;
            this.indicatesImportant = indicatesImportant;
            this.importantChecks = importantChecks;
            this.majorChecks = majorChecks;

            CalcDerived(itemPlacements);
        }

        private void CalcDerived(Dictionary<int, int> itemPlacements)
        {
            items = new();
            combinedChecksList = importantChecks.Concat(majorChecks).ToList();

            foreach (string checkName in combinedChecksList)
            {
                Item item;
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
                items.Add(item);
            }
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += areaId.encodeAsBits(bitLengths);
            result += hasRelevantDependentChecks ? "1" : "0";
            result += indicatesImportant ? "1" : "0";
            result += SettingsEncoder.EncodeAsVlq16((ushort)importantChecks.Count);
            foreach (string checkName in importantChecks)
            {
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
            }
            result += SettingsEncoder.EncodeAsVlq16((ushort)majorChecks.Count);
            foreach (string checkName in majorChecks)
            {
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
            }

            return result;
        }

        public static ImportanceCountHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, int> itemPlacements
        )
        {
            HashSet<string> importantChecks = new();
            HashSet<string> majorChecks = new();

            AreaId areaId = AreaId.decode(bitLengths, processor);
            bool hasRelevantDependentChecks = processor.NextBool();
            bool indicatesImportant = processor.NextBool();
            int numImportantChecks = processor.NextVlq16();
            for (int i = 0; i < numImportantChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                importantChecks.Add(CheckIdClass.GetCheckName(checkId));
            }
            int numMajorChecks = processor.NextVlq16();
            for (int i = 0; i < numMajorChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                majorChecks.Add(CheckIdClass.GetCheckName(checkId));
            }

            return new ImportanceCountHint(
                areaId,
                hasRelevantDependentChecks,
                indicatesImportant,
                importantChecks,
                majorChecks,
                itemPlacements
            );
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            Res.Result hintParsedRes = Res.Msg("hint-type.importance-count");

            string subject;
            Dictionary<string, string> subjectMeta;

            if (indicatesImportant)
            {
                string color =
                    importantChecks.Count > 0
                        ? CustomMessages.messageColorGreen
                        : CustomMessages.messageColorPurple;

                Dictionary<string, string> interpolation =
                    new()
                    {
                        { "context", "count" },
                        { "count", importantChecks.Count.ToString() },
                    };

                Res.Result subjRes = Res.Msg(
                    "hint-type.importance-count.important-item",
                    interpolation,
                    interpolation
                );
                subjectMeta = subjRes.meta;

                if (subjRes.value.Contains("{cs}"))
                {
                    interpolation["cs"] = color;
                    interpolation["ce"] = CustomMessages.messageColorWhite;
                    subject = subjRes.Substitute(interpolation);
                }
                else
                {
                    subject =
                        color
                        + subjRes.Substitute(interpolation)
                        + CustomMessages.messageColorWhite;
                }

                if (majorChecks.Count > 0)
                {
                    int total = importantChecks.Count + majorChecks.Count;
                    string majorCountStr = Res.SimpleMsg(
                        "hint-type.importance-count.num-major",
                        new() { { "count", total.ToString() } }
                    );
                    subject += " " + majorCountStr;
                }
            }
            else
            {
                string color =
                    majorChecks.Count > 0
                        ? CustomMessages.messageColorGreen
                        : CustomMessages.messageColorPurple;

                Dictionary<string, string> interpolation =
                    new() { { "context", "count" }, { "count", majorChecks.Count.ToString() }, };

                Res.Result subjRes = Res.Msg(
                    "hint-type.importance-count.major-item",
                    interpolation,
                    interpolation
                );
                subjectMeta = subjRes.meta;

                if (subjRes.value.Contains("{cs}"))
                {
                    interpolation["cs"] = color;
                    interpolation["ce"] = CustomMessages.messageColorWhite;
                    subject = subjRes.Substitute(interpolation);
                }
                else
                {
                    subject =
                        color
                        + subjRes.Substitute(interpolation)
                        + CustomMessages.messageColorWhite;
                }
            }

            string areaPhrase = CustomGenAreaPhrase(
                areaId,
                subjectMeta,
                CustomMessages.messageColorRed
            );

            string verb = CustomMsgData.GenVerb(hintParsedRes, subjectMeta);

            string text = hintParsedRes.Substitute(
                new() { { "subject", subject }, { "verb", verb }, { "area-phrase", areaPhrase }, }
            );

            string normalizedText = Res.LangSpecificNormalize(text);

            if (hasRelevantDependentChecks)
            {
                // If footnote would be in next textbox, put '*' at end of main hint text.
                if (Res.IsLinesFillBasicSign(normalizedText))
                    normalizedText += "*";

                string footnoteText = Res.LangSpecificNormalize(
                    Res.SimpleMsg("hint-type.importance-count.footnote")
                );
                normalizedText += '\n' + CustomMessages.messageColorOrange + footnoteText;
            }

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }

        private static string CustomGenAreaPhrase(
            AreaId areaId,
            Dictionary<string, string> subjectMeta = null,
            string color = null
        )
        {
            Res.Result areaRes = Res.Msg(areaId.GenResKey(), null, subjectMeta);
            string areaString = areaRes.ResolveWithColor(color);

            if (!areaRes.meta.TryGetValue("ap", out string areaPhraseKey))
                areaPhraseKey = "default";

            Res.Result areaPhraseRes = Res.Msg($"area-phrase.{areaPhraseKey}");
            string areaPhrase = areaPhraseRes.Substitute(new() { { "area", areaString } });

            return areaPhrase;
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);

            for (int i = 0; i < combinedChecksList.Count; i++)
            {
                hintInfo.hintedChecks.Add(combinedChecksList[i]);
                hintInfo.hintedItems.Add(items[i]);
            }

            return hintInfo;
        }
    }
}
