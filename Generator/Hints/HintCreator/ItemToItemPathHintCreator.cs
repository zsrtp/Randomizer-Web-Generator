namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class ItemToItemPathHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.ItemToItemPath;

        private ItemToItemPathHintCreator() { }

        new public static ItemToItemPathHintCreator fromJObject(JObject obj)
        {
            return new ItemToItemPathHintCreator();
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            // Hard-required unique items which can be hinted.
            Dictionary<Item, string> uniqReqItemToCheck = getItemToItemPathGoalItems(genData);

            if (uniqReqItemToCheck.Count < 1)
                return null;

            HashSet<Goal> goals = new();
            foreach (KeyValuePair<Item, string> pair in uniqReqItemToCheck)
            {
                goals.Add(new Goal(GoalEnum.Invalid, Goal.Type.Check, pair.Value));
            }

            List<Item> baseSrcItems =
                new()
                {
                    Item.Boomerang,
                    Item.Lantern,
                    Item.Progressive_Fishing_Rod,
                    Item.Iron_Boots,
                    Item.Progressive_Bow,
                    Item.Filled_Bomb_Bag,
                    Item.Progressive_Clawshot,
                    Item.Aurus_Memo,
                    Item.Spinner,
                    Item.Ball_and_Chain,
                    Item.Progressive_Dominion_Rod,
                };

            // Remove any baseSrcItems which have a check that is already
            // directedToward. Similar to with tradeItems, we do not want to
            // double up with other hints. For example, I had a seed where Ordon
            // was path to LBT and also Spinner is path to Zora Armor, and the
            // very first chest had a Spinner in it. In this case, the normal
            // Path hint directed toward the Ordon check, and the ItemToItemPath
            // hint directed toward the ZA check in Stalfos Grotto. Technically
            // there was nothing wrong, but it definitely felt wrong and like a
            // waste. We were already requiring that the srcChecks not be
            // directedToward for tradeItems, so the change was made to have
            // that be the behavior for everything. -isaac
            for (int i = baseSrcItems.Count - 1; i >= 0; i--)
            {
                Item item = baseSrcItems[i];
                if (!genData.itemToChecksList.TryGetValue(item, out List<string> srcCheckNames))
                {
                    baseSrcItems.RemoveAt(i);
                }
                else
                {
                    // Check that none of the checkNames are already hinted.
                    bool srcItemNotHinted = srcCheckNames.All(
                        (checkName) =>
                            !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
                            && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                    );
                    if (!srcItemNotHinted)
                        baseSrcItems.RemoveAt(i);
                }
            }

            // Add any chainStarter tradeItems that lead to a required
            // uniqueItem as long as there is no checkName rewarding that
            // tradeItem that is already directedToward. For example, if KGY is
            // path to Armogohma for Rutela's Blessing which gives a
            // FemaleStagBeetle, then it is not valid to also create an
            // ItemToItemPath hint such as "FemaleStagBeetle path to Lantern"
            // even if the tgtItem is different. A lot of the time having hints
            // like this that overlap to varying degrees just feels wrong (as
            // in, you have to work to convince yourself that it makes sense; we
            // don't want every player to have to work to convince themselves
            // that it makes sense either), and it is not like we cannot just
            // hint a different srcItem or tgtItem which feels completely
            // natural. -isaac
            foreach (KeyValuePair<string, Item> pair in genData.tradeChainStartToReward)
            {
                Item chainReward = pair.Value;
                if (uniqReqItemToCheck.ContainsKey(chainReward))
                {
                    string chainStart = pair.Key;
                    Item startItem = HintUtils.getCheckContents(chainStart);

                    List<string> checks = genData.itemToChecksList[startItem];

                    // Check that none of the checkNames are already hinted.
                    bool srcItemNotHinted = checks.All(
                        (checkName) =>
                            !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
                            && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                    );

                    if (srcItemNotHinted)
                        baseSrcItems.Add(startItem);
                }
            }

            Dictionary<Item, Dictionary<Goal, bool>> res = HintUtils.checkGoalsWithoutItems(
                genData.startingRoom,
                baseSrcItems,
                goals
            );

            Dictionary<Item, HashSet<Item>> destItemToSrcItem = new();

            foreach (KeyValuePair<Item, Dictionary<Goal, bool>> pair in res)
            {
                Item srcItem = pair.Key;
                // For each dest check, determine the destItem
                foreach (KeyValuePair<Goal, bool> goalPair in pair.Value)
                {
                    if (!goalPair.Value)
                    {
                        // Goal failed when missing srcItem
                        string destCheckName = goalPair.Key.id;
                        Item destItem = HintUtils.getCheckContents(destCheckName);
                        if (!destItemToSrcItem.ContainsKey(destItem))
                            destItemToSrcItem[destItem] = new();
                        destItemToSrcItem[destItem].Add(srcItem);
                    }
                }
            }

            if (destItemToSrcItem.Count < 1)
                return null;

            List<KeyValuePair<double, Item>> weightedList = new();
            Dictionary<Item, int> srcItemCounts = new();
            foreach (KeyValuePair<Item, HashSet<Item>> pair in destItemToSrcItem)
            {
                HashSet<Item> srcItemsForTgtItem = pair.Value;
                // Tally how often a srcItem is used for a destItem. Used for
                // weighting later.
                foreach (Item srcItem in srcItemsForTgtItem)
                {
                    if (!srcItemCounts.ContainsKey(srcItem))
                        srcItemCounts[srcItem] = 0;
                    srcItemCounts[srcItem] += 1;
                }

                // 1 is 1; 2 is 1.41; 3 is 1.59; 4 is 1.69; 5 is 1.75;
                // Approaches 2 as srcItemCount approaches infinity
                double weight = 4 * Math.Atan(srcItemsForTgtItem.Count) / Math.PI;
                // ^ We want to punish target items which would resolve in a
                // simple and uninteresting way (only 1 srcItem), and we want to
                // improve the chances of a hint that might take a while to
                // resolve. For example, in the case of Lantern to Zora Armor
                // and ZA is in a sphere1 Lantern check, it is not that unlikely
                // that someone would find Lantern and get ZA before they ever
                // even come across the ItemToItemPath hint.

                weightedList.Add(new(weight, pair.Key));
            }

            VoseInstance<Item> voseInst = VoseAlgorithm.createInstance(weightedList);
            Item selectedDestItem = voseInst.NextAndKeep(genData.rnd);

            HashSet<Item> srcItemsForDest = destItemToSrcItem[selectedDestItem];
            List<KeyValuePair<double, Item>> srcWeightedList = new();
            foreach (Item srcItem in srcItemsForDest)
            {
                // Prefer hinting a srcItem which is not used for several
                // targetItems. The hint is more helpful when the srcItem and
                // targetItem are more closely tied together logically.
                // Numerator is 10 since it makes the weights easier to
                // understand, but the ratios are the same regardless.
                double weight = 10.0 / Math.Sqrt(srcItemCounts[srcItem]);
                srcWeightedList.Add(new(weight, srcItem));
            }

            VoseInstance<Item> srcItemVoseInst = VoseAlgorithm.createInstance(srcWeightedList);
            Item selectedSrcItem = srcItemVoseInst.NextAndKeep(genData.rnd);

            // Only ever one target checkName since target items are unique.
            string tgtCheckName = genData.itemToChecksList[selectedDestItem][0];

            ItemToItemPathHint hint = ItemToItemPathHint.Create(
                genData,
                selectedSrcItem,
                tgtCheckName
            );

            // Update hinted
            genData.hinted.alreadyCheckDirectedToward.Add(tgtCheckName);

            // Mark all checks which reward this item as directedToward so that
            // we do not get other hints that directToward the same item (path,
            // tradeChain, etc.).
            List<string> checkNames = genData.itemToChecksList[selectedSrcItem];
            foreach (string checkName in checkNames)
            {
                genData.hinted.alreadyCheckDirectedToward.Add(checkName);
            }

            // TODO: do iterations instead of returning a single hint.
            return new() { hint };
        }

        private Dictionary<Item, string> getItemToItemPathGoalItems(HintGenData genData)
        {
            Dictionary<Item, string> uniqueItemsToCheckName = new();

            // Note that for this you are NEVER considered to start with
            // bigKeys. That is only for 'Path' hints since those are mostly
            // directed at bosses which are always locked behind big Keys.
            foreach (string checkName in genData.requiredChecks)
            {
                if (genData.isCheckSphere0(checkName))
                    continue;

                Item contents = HintUtils.getCheckContents(checkName);
                if (genData.itemToChecksList.ContainsKey(contents))
                {
                    List<string> checksList = genData.itemToChecksList[contents];
                    if (
                        checksList != null
                        && checksList.Count == 1
                        && genData.checkCanBeHintedSpol(checkName, true)
                    )
                    {
                        uniqueItemsToCheckName[contents] = checkName;
                    }
                }
            }

            return uniqueItemsToCheckName;
        }
    }
}
