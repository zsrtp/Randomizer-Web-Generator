namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class PathHint : Hint
    {
        public override HintType type { get; } = HintType.Path;

        private static Dictionary<GoalEnum, string> goalToColor =
            new()
            {
                { GoalEnum.Diababa, CustomMessages.messageColorGreen },
                { GoalEnum.Fyrus, CustomMessages.messageColorRed },
                { GoalEnum.Morpheel, CustomMessages.messageColorBlue },
                { GoalEnum.Stallord, CustomMessages.messageColorOrange },
                { GoalEnum.Blizzeta, CustomMessages.messageColorLightBlue },
                { GoalEnum.Armogohma, CustomMessages.messageColorDarkGreen },
                { GoalEnum.Argorok, CustomMessages.messageColorYellow },
                { GoalEnum.Zant, CustomMessages.messageColorPurple },
                { GoalEnum.Hyrule_Castle, CustomMessages.messageColorSilver },
                { GoalEnum.Ganondorf, CustomMessages.messageColorSilver },
            };

        // AreaId expected to only ever be hintZone, but this is easier to
        // write and also future-proofs.
        public AreaId areaId { get; }
        public string checkName { get; }
        public GoalEnum goalEnum { get; }

        // always derived
        public Item item { get; private set; }

        public PathHint(
            AreaId areaId,
            string checkName,
            GoalEnum goalEnum,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.areaId = areaId;
            this.checkName = checkName;
            this.goalEnum = goalEnum;

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

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            Res.Result hintParsedRes = Res.ParseVal("hint-type.path");

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

            if (!goalToColor.TryGetValue(goalEnum, out string goalColor))
                throw new Exception($"Failed to pick color for unknown goalEnum '{goalEnum}'.");
            string goalResKey = "goal." + goalEnum.ToString().ToLowerInvariant();
            string goal = Res.Msg(goalResKey, null).ResolveWithColor(goalColor);

            string text = hintParsedRes.Substitute(
                new() { { "area", areaText }, { "verb", verb }, { "goal", goal }, }
            );

            HintText hintText = new HintText();
            hintText.text = Res.LangSpecificNormalize(text);
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += areaId.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(checkName),
                bitLengths.checkId
            );
            result += SettingsEncoder.EncodeNumAsBits((int)goalEnum, bitLengths.goalEnum);
            return result;
        }

        public static PathHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            AreaId areaId = AreaId.decode(bitLengths, processor);
            int checkId = processor.NextInt(bitLengths.checkId);
            string checkName = CheckIdClass.GetCheckName(checkId);
            GoalEnum goalEnum = (GoalEnum)processor.NextInt(bitLengths.goalEnum);
            return new PathHint(areaId, checkName, goalEnum, itemPlacements);
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
