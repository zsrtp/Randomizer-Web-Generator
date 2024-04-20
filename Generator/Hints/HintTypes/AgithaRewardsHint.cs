namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class AgithaRewardsHint : Hint
    {
        public int numBugsInPool { get; }
        public List<string> interestingAgithaChecks { get; }

        public List<Item> items { get; }

        public AgithaRewardsHint(
            int numBugsInPool,
            List<string> interestingAgithaChecks,
            List<Item> items
        )
        {
            this.type = HintType.AgithaRewards;
            this.numBugsInPool = numBugsInPool;
            this.interestingAgithaChecks = interestingAgithaChecks;

            this.items = items;
        }

        public override List<HintText> toHintTextList()
        {
            string text = $"{numBugsInPool} bugs in pool.";

            if (items == null || items.Count < 1)
                text += " Nothing interesting.";
            else
                text += "\n" + string.Join(", ", items);

            HintText hintText = new HintText();
            hintText.text = Res.LangSpecificNormalize(text);
            return new List<HintText> { hintText };
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);
            // At most 24 bugs in the pool which can be traded in.
            result += SettingsEncoder.EncodeNumAsBits(numBugsInPool, 5);
            result += SettingsEncoder.EncodeAsVlq16((ushort)interestingAgithaChecks.Count);
            foreach (string checkName in interestingAgithaChecks)
            {
                int checkId = CheckIdClass.GetCheckIdNum(checkName);
                result += SettingsEncoder.EncodeNumAsBits(checkId, bitLengths.checkId);
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
            List<Item> items = new();
            for (int i = 0; i < numInterestingAgithaChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                interestingAgithaChecks.Add(CheckIdClass.GetCheckName(checkId));
                items.Add((Item)itemPlacements[checkId]);
            }
            return new AgithaRewardsHint(numBugsInPool, interestingAgithaChecks, items);
        }
    }
}
