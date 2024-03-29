namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class TradeGroupHint : Hint
    {
        private static readonly object privateObj = new();

        public enum Vagueness
        {
            Named = 0,
            Vague = 1,
            Required = 2,
            Unhelpful = 3,
        }

        public enum Status
        {
            Bad = 0,
            Important = 1,
            Required = 2,
        }

        // stored
        public TradeGroup tradeGroup { get; private set; }
        public bool hasCheckName { get; private set; }
        public string checkName { get; private set; }
        public Vagueness vagueness { get; private set; }
        public Status status { get; private set; }

        // derived
        public Item destItem { get; private set; }

        public TradeGroupHint(
            TradeGroup tradeGroup,
            Vagueness vagueness,
            Status status,
            string checkName,
            Dictionary<int, byte> itemPlacements = null,
            object privateRef = null
        )
        {
            this.type = HintType.TradeGroup;

            // Only allow itemPlacements if constructor is called from within
            // this class.
            if (privateObj != privateRef)
                itemPlacements = null;

            this.tradeGroup = tradeGroup;
            this.vagueness = vagueness;
            this.status = status;
            if (checkName == null)
            {
                this.hasCheckName = false;
                this.checkName = CheckIdClass.GetCheckName(0);
            }
            else
            {
                this.hasCheckName = true;
                this.checkName = checkName;
            }

            CalcDerived(itemPlacements);
        }

        private void CalcDerived(Dictionary<int, byte> itemPlacements)
        {
            if (itemPlacements != null)
            {
                // When decoding hint from string
                destItem = HintUtils.getCheckContents(checkName, itemPlacements);
            }
            else
            {
                // When creating hint during generation
                destItem = HintUtils.getCheckContents(checkName);
            }
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();

            string text = $"They say that {{{tradeGroup}}} ";
            if (status == Status.Required)
            {
                text += "is on the {way of the hero}.";
            }
            else if (status == Status.Important)
            {
                if (vagueness == Vagueness.Named)
                    text += $"lead to {{{destItem}}}.";
                else
                    text += "lead to {something good}.";
            }
            else if (status == Status.Bad)
            {
                text += "lead to {nothing}.";
            }

            hintText.text = text;
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits((int)tradeGroup, bitLengths.tradeGroupId);
            result += SettingsEncoder.EncodeNumAsBits((int)vagueness, 2);
            result += SettingsEncoder.EncodeNumAsBits((int)status, 2);
            result += hasCheckName ? "1" : "0";
            result += SettingsEncoder.EncodeNumAsBits(
                CheckIdClass.GetCheckIdNum(checkName),
                bitLengths.checkId
            );
            return result;
        }

        public static TradeGroupHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            TradeGroup tradeGroup = (TradeGroup)processor.NextInt(bitLengths.tradeGroupId);
            Vagueness vagueness = (Vagueness)processor.NextInt(2);
            Status status = (Status)processor.NextInt(2);
            bool hasCheckName = processor.NextBool();

            int srcCheckId = processor.NextInt(bitLengths.checkId);
            string encodedCheckName = CheckIdClass.GetCheckName(srcCheckId);

            string checkName = hasCheckName ? encodedCheckName : null;

            TradeGroupHint hint =
                new(tradeGroup, vagueness, status, checkName, itemPlacements, privateObj);

            return hint;
        }
    }
}
