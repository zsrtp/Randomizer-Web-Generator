namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class LocationHint : Hint
    {
        public override HintType type { get; } = HintType.Location;

        string checkName;
        CheckStatusDisplay display;
        bool vague;
        bool markAsSometimes;

        // derived and stored
        CheckStatus status;
        bool useDefiniteArticle;

        // derived (not stored)
        Item contents;

        public static LocationHint Create(
            HintGenData genData,
            string checkName,
            bool vague = false,
            CheckStatusDisplay display = CheckStatusDisplay.Automatic,
            bool markAsSometimes = false
        )
        {
            CheckStatus status = CalcStatus(genData, checkName);

            LocationHint hint = new(genData, checkName, vague, status, display, markAsSometimes);
            return hint;
        }

        private LocationHint(
            HintGenData genData,
            string checkName,
            bool vague,
            CheckStatus status,
            CheckStatusDisplay display = CheckStatusDisplay.None,
            bool markAsSometimes = false,
            bool useDefiniteArticle = false,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.checkName = checkName;
            this.vague = vague;
            this.status = status;
            this.display = display;
            this.markAsSometimes = markAsSometimes;
            this.useDefiniteArticle = useDefiniteArticle;

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
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

            // When creating the hint during generation, we calculate rather
            // than use input value.
            if (genData != null)
            {
                if (
                    genData.itemToChecksList.TryGetValue(
                        contents,
                        out List<string> checksGivingItem
                    )
                )
                {
                    useDefiniteArticle = checksGivingItem.Count == 1;
                }
            }
        }

        public static CheckStatus CalcStatus(HintGenData genData, string checkName)
        {
            CheckStatus status = CheckStatus.Bad;
            if (genData.requiredChecks.Contains(checkName))
                status = CheckStatus.Required;
            else if (genData.CheckIsGood(checkName))
                status = CheckStatus.Good;
            return status;
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
            result += SettingsEncoder.EncodeNumAsBits((byte)display, 2);
            result += useDefiniteArticle ? "1" : "0";
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
            CheckStatus status = (CheckStatus)processor.NextInt(2);
            CheckStatusDisplay display = (CheckStatusDisplay)processor.NextInt(2);
            bool useDefiniteArticle = processor.NextBool();
            bool markAsSometimes = processor.NextBool();

            string checkName = CheckIdClass.GetCheckName(checkId);
            return new LocationHint(
                null,
                checkName,
                vague,
                status,
                display,
                markAsSometimes,
                useDefiniteArticle,
                itemPlacements
            );
        }

        public override List<HintText> toHintTextList()
        {
            HintText hintText = new HintText();

            // string statusText = "";
            string text = "";

            if (vague)
            {
                bool showVagueGoodText = false;
                bool showUnrequiredText = false;
                bool showVagueBadText = false;

                if (display == CheckStatusDisplay.Required_Or_Not)
                {
                    if (status == CheckStatus.Required)
                    {
                        Res.Result hintTypeRes = Res.Msg("hint-type.location.vague-required", null);

                        string checkNameStr =
                            CustomMessages.messageColorBlue
                            + checkName
                            + CustomMessages.messageColorWhite;

                        text = hintTypeRes.Substitute(new() { { "check-name", checkNameStr } });
                    }
                    else if (status == CheckStatus.Good)
                    {
                        showVagueGoodText = true;
                        showUnrequiredText = true;
                    }
                    else
                        showVagueBadText = true;
                }
                else
                {
                    if (status == CheckStatus.Bad)
                        showVagueBadText = true;
                    else
                        showVagueGoodText = true;
                }

                if (showVagueGoodText)
                {
                    Res.Result hintTypeRes = Res.Msg("hint-type.location.vague-good", null);

                    string checkNameStr =
                        CustomMessages.messageColorRed
                        + checkName
                        + CustomMessages.messageColorWhite;

                    string noun = CustomMsgData.GenResWithSlotName(
                        hintTypeRes,
                        "noun",
                        null,
                        CustomMessages.messageColorGreen
                    );

                    if (showUnrequiredText)
                        noun += " " + Res.SimpleMsg("description.unrequired-check", null);

                    text = hintTypeRes.Substitute(
                        new() { { "check-name", checkNameStr }, { "noun", noun } }
                    );
                }
                else if (showVagueBadText)
                {
                    Res.Result hintTypeRes = Res.Msg("hint-type.location.vague-bad", null);

                    string checkNameStr =
                        CustomMessages.messageColorRed
                        + checkName
                        + CustomMessages.messageColorWhite;

                    string noun = CustomMsgData.GenResWithSlotName(
                        hintTypeRes,
                        "noun",
                        null,
                        CustomMessages.messageColorPurple
                    );

                    text = hintTypeRes.Substitute(
                        new() { { "check-name", checkNameStr }, { "noun", noun } }
                    );
                }
            }
            else
            {
                Res.Result hintTypeRes = Res.Msg("hint-type.location", null);

                string checkNameStr =
                    CustomMessages.messageColorRed + checkName + CustomMessages.messageColorWhite;

                string verb = CustomMsgData.GenVerb(hintTypeRes, null);

                string itemText = CustomMsgData.GenItemText3(
                    out Dictionary<string, string> meta,
                    contents,
                    status,
                    contextIn: useDefiniteArticle ? "def" : "indef",
                    checkStatusDisplay: display
                );

                text = hintTypeRes.Substitute(
                    new() { { "check-name", checkNameStr }, { "verb", verb }, { "item", itemText } }
                );
            }

            // We either display as vague or not.

            // We always know the status of the check.

            // We always NEED to know the display type, which is one of:
            // - isRequiredOrNot
            // - unspecified (nothing, or if a tradeItem, then isGoodOrNot)

            // If vague, then we always display good or bad. We only display
            // "woth" if we are doing required or not. If we are doing required
            // or not, then we display "woth" or "unrequired".

            // if (vague)
            // {
            //     if (displayType == DisplayType.IsRequiredOrNot)
            //     {
            //         // Either woth or unrequired
            //     }
            //     else
            //     {
            //         // Either good or bad
            //     }

            //     if (status != CheckStatus.Bad)
            //         hintText.text = $"They say that {{{checkName}}} has {{something good}}.";
            //     else
            //         hintText.text = $"They say that {{{checkName}}} has {{nothing important}}.";
            // }
            // else
            // {
            //     if (HintUtils.IsTradeItem(contents))
            //     {
            //         switch (status)
            //         {
            //             case CheckStatus.Bad:
            //                 statusText = " (not useful)";
            //                 break;
            //             case CheckStatus.Good:
            //             case CheckStatus.Required:
            //                 statusText = " (good)";
            //                 break;
            //         }
            //     }

            //     // If not vague, we specify the exact item.

            //     // If terms of the color, this is dependent on some things:

            //     // If the status is required and we want Required or Not, then
            //     // if the status is required, the item text should be Blue and
            //     // we should display (required).

            //     // If the status is good or bad and we want RequiredOrNot, then
            //     // if the status is good or bad, the item text should be either
            //     // green for good or purple for bad.

            //     // If we want GoodOrBad or if the item is a tradeItem and it is
            //     // unspecified, then we should include the (good) or (bad) and
            //     // also the item color should be either green or purple.

            //     hintText.text = $"They say that {{{checkName}}} has {{{contents}{statusText}}}.";
            // }
            string normalizedText = Res.LangSpecificNormalize(text);
            hintText.text = normalizedText;
            return new List<HintText> { hintText };
        }
    }
}
