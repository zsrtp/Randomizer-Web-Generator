namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class JovaniRewardsHint : Hint
    {
        public override HintType type { get; } = HintType.JovaniRewards;

        public List<JovaniCheckInfo> checkInfoList { get; }

        public JovaniRewardsHint(List<JovaniCheckInfo> checkInfoList)
        {
            this.checkInfoList = checkInfoList;

            if (!ListUtils.isEmpty(checkInfoList) && checkInfoList.Count > 16)
            {
                // Only allow up to 16 unique Jovani reward thresholds. Probably
                // more than we will ever need.
                throw new Exception(
                    $"Cannot create JovaniRewardsHint with more than 16 thresholds, but was '{checkInfoList.Count}'."
                );
            }

            if (ListUtils.isEmpty(this.checkInfoList))
                this.checkInfoList = new();
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            result += SettingsEncoder.EncodeNumAsBits(checkInfoList.Count, 4);

            foreach (JovaniCheckInfo jovaniCheckInfo in checkInfoList)
            {
                result += jovaniCheckInfo.encodeAsBits(bitLengths);
            }

            return result;
        }

        public static JovaniRewardsHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            int listSize = processor.NextInt(4);

            List<JovaniCheckInfo> list = new(listSize);

            for (int i = 0; i < listSize; i++)
            {
                list.Add(JovaniCheckInfo.decode(bitLengths, processor, itemPlacements));
            }

            return new JovaniRewardsHint(list);
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            string text = "";

            if (ListUtils.isEmpty(checkInfoList))
            {
                // Not expecting to ever see this. Can fill out if we ever
                // support something like a random number of Jovani rewards
                // which the user does not know ahead of time, and we happened
                // to generate that there were no Jovani rewards.
                text = "empty...";
            }
            else
            {
                for (int i = 0; i < checkInfoList.Count; i++)
                {
                    JovaniCheckInfo checkInfo = checkInfoList[i];

                    if (i > 0)
                        text += "\n\n";

                    string countStr = checkInfo.soulsThreshold.ToString();

                    Res.Result res = Res.Msg(
                        "hint-type.jovani-rewards.reward",
                        new() { { "count", countStr } }
                    );

                    string itemText;
                    if (checkInfo.unhinted)
                    {
                        itemText = Res.Msg("noun.jovani-unhinted")
                            .ResolveWithColor(CustomMessages.messageColorOrange);
                    }
                    else
                    {
                        // Leaving def/indef out for now. Might need it or
                        // 'capitalize' to be based on meta from the
                        // 'hint-type.jovani-rewards.reward' line.
                        itemText = customMsgData.GenItemText3(
                            out _,
                            checkInfo.item,
                            checkInfo.checkStatus,
                            // contextIn: checkInfo.useDefArticle ? "def" : "indef",
                            checkStatusDisplay: checkInfo.checkStatusDisplay,
                            capitalize: true,
                            isLogicalItem: checkInfo.isLogicalItem
                        );
                    }

                    string rowText = res.Substitute(
                        new() { { "count", countStr }, { "item", itemText } }
                    );
                    // 41 based on smaller font size. See AgithaRewardsHint as
                    // well.
                    text += Res.LangSpecificNormalize(rowText, Res.IsCultureJa() ? 30 : 41);
                }
            }

            // Smaller font size.
            text = "\x1A\x07\xFF\x00\x01\x00\x48" + text;

            HintText hintText = new HintText();
            hintText.text = text;
            return new List<HintText>() { hintText };
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);

            foreach (JovaniCheckInfo checkInfo in checkInfoList)
            {
                if (!checkInfo.unhinted)
                {
                    hintInfo.hintedChecks.Add(checkInfo.checkName);
                    hintInfo.hintedItems.Add(checkInfo.item);
                }
            }

            return hintInfo;
        }

        public class JovaniCheckInfo
        {
            public string checkName { get; }
            public byte soulsThreshold { get; } // Planning ahead for configurable thresholds
            public bool unhinted { get; }
            public CheckStatus checkStatus { get; }
            public CheckStatusDisplay checkStatusDisplay { get; }

            // derived and encoded
            public bool useDefArticle { get; private set; }
            public bool isLogicalItem { get; private set; }

            // derived and not encoded
            public Item item { get; private set; }

            public JovaniCheckInfo(
                HintGenData genData,
                string checkName,
                byte soulsThreshold,
                bool unhinted,
                CheckStatus checkStatus,
                CheckStatusDisplay checkStatusDisplay,
                bool useDefArticle = false,
                bool isLogicalItem = false,
                Dictionary<int, byte> itemPlacements = null
            )
            {
                this.checkName = checkName;
                this.soulsThreshold = soulsThreshold;
                this.unhinted = unhinted;
                this.checkStatus = checkStatus;
                this.checkStatusDisplay = checkStatusDisplay;
                this.useDefArticle = useDefArticle;
                this.isLogicalItem = isLogicalItem;

                CalcDerived(genData, itemPlacements);
            }

            private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
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

                // When creating the hint during generation, we calculate rather
                // than use input value.
                if (genData != null)
                {
                    if (genData.logicalItems.Contains(item))
                        isLogicalItem = true;

                    useDefArticle = genData.ItemUsesDefArticle(item);
                }
            }

            public string encodeAsBits(HintEncodingBitLengths bitLengths)
            {
                string result = SettingsEncoder.EncodeNumAsBits(
                    CheckIdClass.GetCheckIdNum(checkName),
                    bitLengths.checkId
                );
                result += SettingsEncoder.EncodeNumAsBits(soulsThreshold, 8);
                result += unhinted ? "1" : "0";
                result += SettingsEncoder.EncodeNumAsBits((byte)checkStatus, 2);
                result += SettingsEncoder.EncodeNumAsBits((byte)checkStatusDisplay, 2);
                result += useDefArticle ? "1" : "0";
                result += isLogicalItem ? "1" : "0";
                return result;
            }

            public static JovaniCheckInfo decode(
                HintEncodingBitLengths bitLengths,
                BitsProcessor processor,
                Dictionary<int, byte> itemPlacements
            )
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                string checkName = CheckIdClass.GetCheckName(checkId);

                byte soulsThreshold = processor.NextByte();
                bool unhinted = processor.NextBool();
                CheckStatus status = (CheckStatus)processor.NextInt(2);
                CheckStatusDisplay display = (CheckStatusDisplay)processor.NextInt(2);
                bool useDefArticle = processor.NextBool();
                bool isLogicalItem = processor.NextBool();

                return new JovaniCheckInfo(
                    null,
                    checkName,
                    soulsThreshold,
                    unhinted,
                    status,
                    display,
                    useDefArticle,
                    isLogicalItem,
                    itemPlacements
                );
            }
        }
    }
}
