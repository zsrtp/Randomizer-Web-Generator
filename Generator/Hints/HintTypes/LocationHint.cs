namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class LocationHint : Hint
    {
        private static readonly object privateObj = new();

        public enum Status : byte
        {
            Bad = 0,
            Good = 1,
            Required = 2,
        }

        string checkName;
        bool vague;
        Status status;
        bool markAsSometimes;

        // derived
        Item contents;

        public static LocationHint Create(
            HintGenData genData,
            string checkName,
            bool vague = false,
            bool markAsSometimes = false
        )
        {
            Status status = CalcStatus(genData, checkName);

            LocationHint hint = new(checkName, vague, status, markAsSometimes);
            return hint;
        }

        private LocationHint(
            string checkName,
            bool vague,
            Status status,
            bool markAsSometimes = false,
            Dictionary<int, byte> itemPlacements = null,
            object privateRef = null
        )
        {
            this.type = HintType.Location;

            // Only allow itemPlacements if constructor is called from within
            // this class.
            if (privateObj != privateRef)
                itemPlacements = null;

            this.checkName = checkName;
            this.vague = vague;
            this.status = status;
            this.markAsSometimes = markAsSometimes;

            CalcDerived(itemPlacements);
        }

        private void CalcDerived(Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                contents = HintUtils.getCheckContents(checkName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                contents = HintUtils.getCheckContents(checkName);
            }
        }

        public static Status CalcStatus(HintGenData genData, string checkName)
        {
            Status status = Status.Bad;
            if (genData.requiredChecks.Contains(checkName))
                status = Status.Required;
            else if (genData.CheckIsGood(checkName))
                status = Status.Good;
            return status;
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();

            string statusText = "";

            if (vague)
            {
                if (status != Status.Bad)
                    hintText.text = $"They say that {{{checkName}}} has {{something good}}.";
                else
                    hintText.text = $"They say that {{{checkName}}} has {{nothing important}}.";
            }
            else
            {
                if (HintUtils.IsTradeItem(contents))
                {
                    switch (status)
                    {
                        case Status.Bad:
                            statusText = " (not useful)";
                            break;
                        case Status.Good:
                        case Status.Required:
                            statusText = " (good)";
                            break;
                    }
                }

                hintText.text = $"They say that {{{checkName}}} has {{{contents}{statusText}}}.";
            }
            return new List<HintText> { hintText };
        }

        // Only need to encode the checkName since we can grab the contents when
        // decoding.
        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(checkName),
                bitLengths.checkId
            );
            result += vague ? "1" : "0";
            result += SettingsEncoder.EncodeNumAsBits((byte)status, 2);
            result += markAsSometimes ? "1" : "0";
            return result;
        }

        public static LocationHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            int checkId = processor.NextInt(bitLengths.checkId);
            bool vague = processor.NextBool();
            Status status = (Status)processor.NextInt(2);
            bool markAsSometimes = processor.NextBool();

            string checkName = CheckIdClass.GetCheckName(checkId);
            return new LocationHint(
                checkName,
                vague,
                status,
                markAsSometimes,
                itemPlacements,
                privateObj
            );
        }
    }
}
