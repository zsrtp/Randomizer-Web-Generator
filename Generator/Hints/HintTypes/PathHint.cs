namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class PathHint : Hint
    {
        // AreaId expected to only ever be hintZone, but this is easier to
        // write and also future-proofs.
        public AreaId areaId { get; }
        public string checkName { get; }
        public GoalEnum goalEnum { get; }

        public PathHint(AreaId areaId, string checkName, GoalEnum goalEnum)
        {
            this.type = HintType.Path;
            this.areaId = areaId;
            this.checkName = checkName;
            this.goalEnum = goalEnum;
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();
            hintText.text =
                $"They say that {{{areaId.tempToString()}}} is on the path to {{{goalEnum.ToString()}}}.";
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
            return new PathHint(areaId, checkName, goalEnum);
        }
    }
}
