namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class AgithaRewardsHint : Hint
    {
        public override HintType type { get; } = HintType.AgithaRewards;

        public int numBugsInPool { get; }
        public List<string> interestingAgithaChecks { get; }

        // Derived and encoded
        public List<bool> useDefArticleList { get; private set; }

        // derived and not encoded
        public List<Item> items { get; private set; }

        public AgithaRewardsHint(
            HintGenData genData,
            int numBugsInPool,
            List<string> interestingAgithaChecks,
            List<bool> useDefArticleList = null,
            Dictionary<int, byte> itemPlacements = null
        // List<Item> items
        )
        {
            this.numBugsInPool = numBugsInPool;
            this.interestingAgithaChecks = interestingAgithaChecks;
            this.useDefArticleList = useDefArticleList;

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
        {
            items = new();
            if (genData != null)
                useDefArticleList = new();

            foreach (string checkName in interestingAgithaChecks)
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

                // When creating the hint during generation, we calculate rather
                // than use input value.
                if (genData != null)
                {
                    if (
                        genData.itemToChecksList.TryGetValue(
                            item,
                            out List<string> checksGivingItem
                        )
                    )
                    {
                        useDefArticleList.Add(checksGivingItem.Count == 1);
                    }
                }
            }
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            bool hasItems = !ListUtils.isEmpty(items);

            string context = hasItems ? "good" : null;

            Res.Result res = Res.Msg("hint-type.agitha-rewards", new() { { "context", context } });

            string numBugsColor = hasItems
                ? CustomMessages.messageColorYellow
                : CustomMessages.messageColorPurple;
            string numBugsText = Res.Msg(
                    "noun.num-golden-bugs",
                    new() { { "count", numBugsInPool.ToString() } }
                )
                .Substitute(
                    new()
                    {
                        { "count", numBugsInPool.ToString() },
                        { "cs", numBugsColor },
                        { "ce", CustomMessages.messageColorWhite }
                    }
                );

            // Generate the "items" text.
            string itemsText = "";
            if (hasItems)
            {
                // Generate the items list by passing a list of strings. On the
                // outside we can set each one to a green color. The function
                // can be in Res and it can automatically detect the language.
                List<string> itemTexts = new();
                for (int i = 0; i < items.Count; i++)
                {
                    itemTexts.Add(
                        customMsgData.GenItemText3(
                            out _,
                            items[i],
                            CheckStatus.Good,
                            contextIn: useDefArticleList[i] ? "def" : "indef",
                            prefStartColor: "",
                            prefEndColor: ""
                        )
                    );
                }
                // TODO: there seems to be a maximum number of bytes that we can
                // put on the sign before it stops copying them over. So making
                // this entire section green to save space for now until we can
                // figure out another solution. Ideally we would maybe split
                // this sign into multiple text boxes.
                itemsText =
                    CustomMessages.messageColorGreen + Res.CreateAndList(res.langCode, itemTexts);
            }

            string text = res.Substitute(
                new() { { "num-bugs", numBugsText }, { "items", itemsText } }
            );

            // Set font size to 0x48.
            string normText = Res.LangSpecificNormalize("\x1A\x07\xFF\x00\x01\x00\x48" + text, 41);

            HintText hintText = new HintText();
            hintText.text = normText;
            return new List<HintText>() { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            // At most 24 bugs in the pool which can be traded in.
            result += SettingsEncoder.EncodeNumAsBits(numBugsInPool, 5);
            result += SettingsEncoder.EncodeAsVlq16((ushort)interestingAgithaChecks.Count);
            for (int i = 0; i < interestingAgithaChecks.Count; i++)
            {
                string checkName = interestingAgithaChecks[i];
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
                result += useDefArticleList[i] ? "1" : "0";
            }
            return result;
        }

        public static AgithaRewardsHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            int numBugsInPool = processor.NextInt(5);
            int numInterestingAgithaChecks = processor.NextVlq16();
            List<string> interestingAgithaChecks = new();
            List<bool> useDefArticleList = new();
            for (int i = 0; i < numInterestingAgithaChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                interestingAgithaChecks.Add(CheckIdClass.GetCheckName(checkId));
                bool useDefArticle = processor.NextBool();
                useDefArticleList.Add(useDefArticle);
            }
            return new AgithaRewardsHint(
                null,
                numBugsInPool,
                interestingAgithaChecks,
                useDefArticleList,
                itemPlacements
            );
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);
            hintInfo.hintedChecks.AddRange(interestingAgithaChecks);
            hintInfo.hintedItems.AddRange(items);

            return hintInfo;
        }
    }
}
