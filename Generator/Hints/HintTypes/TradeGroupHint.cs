namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class TradeGroupHint : Hint
    {
        public override HintType type { get; } = HintType.TradeGroup;

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

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            HintText hintText = new HintText();

            string groupKey = TradeGroupUtils.GenResKey(tradeGroup);
            Res.Result groupRes = Res.Msg(groupKey, null);

            string text = "";
            if (status == Status.Required)
            {
                string group = groupRes.ResolveWithColor(CustomMessages.messageColorBlue);
                text = Res.SimpleMsg(
                    "hint-type.trade-group",
                    new() { { "group", group }, { "context", "woth" } }
                );
            }
            else if (status == Status.Important)
            {
                string group = groupRes.ResolveWithColor(CustomMessages.messageColorYellow);
                if (vagueness == Vagueness.Named)
                {
                    // Gen item text.
                    string itemText = CustomMsgData.GenItemText(
                        destItem,
                        // useDefiniteArticle ? "def" : "indef", // TODO: we DO need to actually know this.
                        "def",
                        prefStartColor: CustomMessages.messageColorGreen
                    );

                    text = Res.Msg("hint-type.trade-group", null)
                        .Substitute(new() { { "group", group }, { "item", itemText } });
                }
                else
                {
                    string somethingGood = Res.Msg("noun.something-good", null)
                        .ResolveWithColor(CustomMessages.messageColorGreen);

                    text = Res.SimpleMsg(
                        "hint-type.trade-group",
                        new()
                        {
                            { "context", "good" },
                            { "group", group },
                            { "something-good", somethingGood }
                        }
                    );
                }
            }
            else if (status == Status.Bad)
            {
                string group = groupRes.ResolveWithColor(CustomMessages.messageColorPurple);
                text = Res.SimpleMsg(
                    "hint-type.trade-group",
                    new() { { "group", group }, { "context", "barren" } }
                );
            }

            string normalizedText = Res.LangSpecificNormalize(text);
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            return hintInfo;
        }
    }
}
