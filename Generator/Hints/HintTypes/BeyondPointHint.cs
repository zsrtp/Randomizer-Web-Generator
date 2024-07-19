namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.Marshalling;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class BeyondPointHint : Hint
    {
        // public enum BeyondPointType
        // {
        //     Nothing = 0,
        //     Good = 1,
        //     Only_Big_Keys = 2,
        //     Good_No_Big_Keys = 3,
        //     Good_And_Big_Keys = 4,
        // }

        public override HintType type { get; } = HintType.BeyondPoint;

        // public BeyondPointType beyondPointType { get; }
        bool includeBigKeyInfo;
        List<string> goodChecks;
        List<string> bigKeyChecks;

        // derived and stored
        bool bigKeyUseDefiniteArticle;

        // derived (not stored)
        Dictionary<string, Item> checkNameToContents = new();

        public static BeyondPointHint Create(
            HintGenData genData,
            bool includeBigKeyInfo,
            List<string> goodChecks,
            List<string> bigKeyChecks
        )
        {
            return new BeyondPointHint(genData, includeBigKeyInfo, goodChecks, bigKeyChecks);
        }

        private BeyondPointHint(
            HintGenData genData,
            // BeyondPointType beyondPointType,
            bool includeBigKeyInfo,
            List<string> goodChecks,
            List<string> bigKeyChecks,
            bool bigKeyUseDefiniteArticle = false,
            Dictionary<int, byte> itemPlacements = null
        )
        {
            this.includeBigKeyInfo = includeBigKeyInfo;
            this.goodChecks = goodChecks;
            this.bigKeyChecks = bigKeyChecks;
            this.bigKeyUseDefiniteArticle = bigKeyUseDefiniteArticle;

            if (this.goodChecks == null)
                this.goodChecks = new();
            if (this.bigKeyChecks == null)
                this.bigKeyChecks = new();

            CalcDerived(genData, itemPlacements);
        }

        private void CalcDerived(HintGenData genData, Dictionary<int, byte> itemPlacements)
        {
            buildCheckToItemMappings(goodChecks, itemPlacements);
            buildCheckToItemMappings(bigKeyChecks, itemPlacements);

            // When creating the hint during generation, we calculate rather
            // than use input value.
            if (genData != null)
            {
                // If we have bigKeyChecks, then do the calc.
                if (!ListUtils.isEmpty(bigKeyChecks))
                {
                    Item bigKeyItem = checkNameToContents[bigKeyChecks[0]];

                    if (
                        genData.itemToChecksList.TryGetValue(
                            bigKeyItem,
                            out List<string> checksGivingItem
                        )
                    )
                    {
                        bigKeyUseDefiniteArticle = checksGivingItem.Count == 1;
                    }
                }
            }
        }

        private void buildCheckToItemMappings(
            List<string> checkNames,
            Dictionary<int, byte> itemPlacements
        )
        {
            if (!ListUtils.isEmpty(checkNames))
            {
                foreach (string checkName in checkNames)
                {
                    Item contents;
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
                    checkNameToContents[checkName] = contents;
                }
            }
        }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            string context;
            if (!ListUtils.isEmpty(goodChecks))
            {
                if (!includeBigKeyInfo)
                    context = "good";
                else if (!ListUtils.isEmpty(bigKeyChecks))
                    context = "good-and-big-keys";
                else
                    context = "good-no-big-keys";
            }
            else
            {
                if (includeBigKeyInfo && !ListUtils.isEmpty(bigKeyChecks))
                    context = "only-big-keys";
                else
                    context = "";
            }

            string bigKeysText = "";

            if (includeBigKeyInfo)
            {
                // In the future, we may need to check this differently
                // depending on how plurals work in different languages. We want
                // to prioritize the "count" one, but if we pass both "count"
                // and "context", then it will resolve to the "context" one
                // without the "count" first. For now, just comparing if there
                // are 2 or more since it works for both English and French.
                Dictionary<string, string> interpolation = new();
                if (bigKeyChecks.Count >= 2)
                    interpolation["count"] = bigKeyChecks.Count.ToString();
                else
                    interpolation["context"] = bigKeyUseDefiniteArticle ? "def" : "indef";

                bigKeysText = Res.Msg("hint-type.beyond-point.big-key", interpolation)
                    .ResolveWithColor(CustomMessages.messageColorOrange);
            }

            // Something good and the big key
            // Something good and a big key
            // Something good and big keys

            // Only the big key
            // Only a big key
            // Only big keys

            // If includeBigKey info, then try to generate the text based on how
            // many there are and if useDef.

            string text = Res.LangSpecificNormalize(
                Res.Msg("hint-type.beyond-point", new() { { "context", context } })
                    .Substitute(new() { { "big-key", bigKeysText } })
            );

            HintText hintText = new HintText();
            hintText.text = text;
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            // TODO: Probably don't need this one anymore
            // result += SettingsEncoder.EncodeNumAsBits((byte)beyondPointType, 3);
            result += includeBigKeyInfo ? "1" : "0";

            result += SettingsEncoder.EncodeAsVlq16((ushort)goodChecks.Count);
            for (int i = 0; i < goodChecks.Count; i++)
            {
                string checkName = goodChecks[i];
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
            }

            result += SettingsEncoder.EncodeAsVlq16((ushort)bigKeyChecks.Count);
            for (int i = 0; i < bigKeyChecks.Count; i++)
            {
                string checkName = bigKeyChecks[i];
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
            }

            result += bigKeyUseDefiniteArticle ? "1" : "0";

            return result;
        }

        public static BeyondPointHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            // BeyondPointType beyondPointType = (BeyondPointType)processor.NextInt(3);
            bool includeBigKeyInfo = processor.NextBool();
            List<string> goodChecks = new();
            List<string> bigKeyChecks = new();

            int numGoodChecks = processor.NextVlq16();
            for (int i = 0; i < numGoodChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                goodChecks.Add(CheckIdClass.GetCheckName(checkId));
            }

            int numBigKeyChecks = processor.NextVlq16();
            for (int i = 0; i < numBigKeyChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                bigKeyChecks.Add(CheckIdClass.GetCheckName(checkId));
            }

            bool bigKeyUseDefiniteArticle = processor.NextBool();

            return new BeyondPointHint(
                null,
                includeBigKeyInfo,
                goodChecks,
                bigKeyChecks,
                bigKeyUseDefiniteArticle,
                itemPlacements
            );
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);

            if (!ListUtils.isEmpty(goodChecks))
            {
                foreach (string checkName in goodChecks)
                {
                    hintInfo.referencedChecks.Add(checkName);
                    hintInfo.referencedItems.Add(checkNameToContents[checkName]);
                }
            }

            if (!ListUtils.isEmpty(bigKeyChecks))
            {
                foreach (string checkName in bigKeyChecks)
                {
                    hintInfo.referencedChecks.Add(checkName);
                    hintInfo.referencedItems.Add(checkNameToContents[checkName]);
                }
            }

            return hintInfo;
        }
    }
}
