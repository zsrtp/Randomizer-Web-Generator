namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SSettings.Enums;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class HintGenData
    {
        public Random rnd { get; private set; }
        public SharedSettings sSettings { get; private set; }
        public PlaythroughSpheres playthroughSpheres { get; private set; }
        public Room startingRoom { get; private set; }
        public HintedThings3 hinted { get; }
        public HintVars vars { get; }

        public Dictionary<Goal, List<string>> goalToRequiredChecks { get; set; }
        public HashSet<string> requiredChecks { get; set; }
        public HashSet<Item> preventBarrenItems { get; private set; }
        public HashSet<string> allowBarrenChecks { get; private set; }
        public Dictionary<Item, List<string>> itemToChecksList { get; set; }
        public Dictionary<string, Item> tradeChainStartToReward = new();
        public Dictionary<Item, string> tradeItemToChainEndCheck = new();
        public Dictionary<AreaId, HashSet<Item>> areaIdToAllowBarrenItems { get; private set; }

        public HintGenData(
            Random rnd,
            SharedSettings sSettings,
            PlaythroughSpheres playthroughSpheres,
            Room startingRoom
        )
        {
            this.rnd = rnd;
            this.sSettings = sSettings;
            this.playthroughSpheres = playthroughSpheres;
            this.startingRoom = startingRoom;
            hinted = new HintedThings3();
            vars = new HintVars();

            areaIdToAllowBarrenItems = prepareAreaIdToAllowBarrenItems();
            itemToChecksList = calcItemToChecksList();
            prepareTradeItemData();

            if (sSettings.logicRules != LogicRules.No_Logic)
            {
                goalToRequiredChecks = HintUtils.calculateGoalsRequiredChecks(
                    startingRoom,
                    playthroughSpheres.spheres,
                    sSettings
                );

                // We need to calculate `requiredChecks` separately from
                // `goalToRequiredChecks` because the goal ones might be
                // calculated assuming you start with big keys so that the path
                // hints are not all super big key-based.
                requiredChecks = HintUtils.calculateRequiredChecks(
                    startingRoom,
                    playthroughSpheres.spheres
                );
            }
            else
            {
                goalToRequiredChecks = new();
                requiredChecks = new();
            }

            allowBarrenChecks = prepareAllowBarrenChecks();
        }

        public void updateFromHintSettings(HintSettings hintSettings)
        {
            preventBarrenItems = genPreventBarrenItemSet(hintSettings);
        }

        private HashSet<Item> genPreventBarrenItemSet(HintSettings hintSettings)
        {
            // Intentionally not including shields. Note that hard-required
            // checks will always preventBarren.
            HashSet<Item> itemSet =
                new()
                {
                    // Item Wheel
                    Item.Progressive_Clawshot,
                    Item.Progressive_Dominion_Rod,
                    Item.Ball_and_Chain,
                    Item.Spinner,
                    Item.Progressive_Bow,
                    Item.Iron_Boots,
                    Item.Boomerang,
                    Item.Lantern,
                    Item.Progressive_Fishing_Rod,
                    Item.Filled_Bomb_Bag,
                    Item.Aurus_Memo,
                    // Other
                    Item.Progressive_Sword,
                    Item.Zora_Armor,
                    Item.Shadow_Crystal,
                    Item.Gate_Keys,
                    Item.North_Faron_Woods_Gate_Key,
                    Item.Gerudo_Desert_Bulblin_Camp_Key,
                };

            // Handle dungeonRewards if shuffled.
            if (sSettings.shuffleRewards)
            {
                bool noReasonToEnterPot =
                    sSettings.barrenDungeons && !HintUtils.DungeonIsRequired("Palace of Twilight");

                if (
                    (
                        !noReasonToEnterPot
                        && sSettings.palaceRequirements == PalaceRequirements.Fused_Shadows
                    )
                    || sSettings.castleRequirements == CastleRequirements.Fused_Shadows
                )
                    itemSet.Add(Item.Progressive_Fused_Shadow);

                if (
                    (
                        !noReasonToEnterPot
                        && sSettings.palaceRequirements == PalaceRequirements.Mirror_Shards
                    )
                    || sSettings.castleRequirements == CastleRequirements.Mirror_Shards
                )
                    itemSet.Add(Item.Progressive_Mirror_Shard);
            }

            if (sSettings.logicRules != LogicRules.Glitchless)
            {
                itemSet.Add(Item.Magic_Armor);
                itemSet.Add(Item.Progressive_Hidden_Skill);
            }

            if (sSettings.damageMagnification == DamageMagnification.OHKO)
            {
                itemSet.Add(Item.Coro_Bottle);
                itemSet.Add(Item.Empty_Bottle);
                itemSet.Add(Item.Jovani_Bottle);
                itemSet.Add(Item.Sera_Bottle);
            }

            if (!sSettings.skipPrologue)
                itemSet.Add(Item.North_Faron_Woods_Gate_Key);

            if (sSettings.smallKeySettings != SmallKeySettings.Keysy)
                itemSet.Add(Item.Gate_Keys);

            // For keysy, camp_key gets added to starting items currently, so it
            // will automatically get handled by allowBarrenChecks.
            if (!sSettings.skipArbitersEntrance)
                itemSet.Add(Item.Gerudo_Desert_Bulblin_Camp_Key);

            // Add all of the dungeon stuff.

            bool bigKeysPreventBarren =
                sSettings.bigKeySettings == BigKeySettings.Anywhere
                || sSettings.bigKeySettings == BigKeySettings.Any_Dungeon;

            bool smallKeysPreventBarren =
                sSettings.smallKeySettings == SmallKeySettings.Anywhere
                || sSettings.smallKeySettings == SmallKeySettings.Any_Dungeon;

            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Forest Temple"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Forest_Temple_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Forest_Temple_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Goron Mines"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Goron_Mines_Key_Shard);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Goron_Mines_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Lakebed Temple"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Lakebed_Temple_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Lakebed_Temple_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Arbiter's Grounds"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Arbiters_Grounds_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Arbiters_Grounds_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Snowpeak Ruins"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Snowpeak_Ruins_Bedroom_Key);
                if (smallKeysPreventBarren)
                {
                    itemSet.Add(Item.Snowpeak_Ruins_Small_Key);
                    itemSet.Add(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
                    itemSet.Add(Item.Snowpeak_Ruins_Ordon_Pumpkin);
                }
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Temple of Time"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Temple_of_Time_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Temple_of_Time_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("City in the Sky"))
            {
                if (!sSettings.skipCityEntrance)
                    itemSet.Add(Item.Progressive_Sky_Book);

                if (bigKeysPreventBarren)
                    itemSet.Add(Item.City_in_The_Sky_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.City_in_The_Sky_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Palace of Twilight"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Palace_of_Twilight_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Palace_of_Twilight_Small_Key);
            }

            if (bigKeysPreventBarren)
                itemSet.Add(Item.Hyrule_Castle_Big_Key);
            if (smallKeysPreventBarren)
                itemSet.Add(Item.Hyrule_Castle_Small_Key);

            if (sSettings.logicRules == LogicRules.No_Logic)
            {
                // Note for other logics, one of these items only preventBarren
                // when there is a logically required check which rewards that
                // item. This way we do not have to worry about the desert or a
                // dungeon being not barren-hintable when it has an obviously
                // skippable slingshot, etc. This does not impact the chance
                // that these items get hinted required (about 0.1% for
                // slingshot). This also prevents bugs and sketch which lead to
                // these items from preventing barren when pointless.
                itemSet.Add(Item.Slingshot);
                itemSet.Add(Item.Wooden_Shield);
                itemSet.Add(Item.Ordon_Shield);
                itemSet.Add(Item.Hylian_Shield);

                // Can address this more when work is done around shop prices
                if (
                    !HintUtils.checkIsPlayerKnownStatus("Castle Town Malo Mart Magic Armor")
                    && !sSettings.increaseWallet
                )
                    itemSet.Add(Item.Progressive_Wallet);
            }

            // Apply majorItem changes from hintSettings.
            if (hintSettings.addItems.ContainsKey("majorItems"))
            {
                foreach (Item addedItem in hintSettings.addItems["majorItems"])
                {
                    itemSet.Add(addedItem);
                }
            }

            if (sSettings.logicRules != LogicRules.No_Logic)
            {
                // For now, do not allow people to remove preventBarrenItems for
                // no-logic since we have no way to protect against people
                // letting a check preventBarren when it is required. We do not
                // know what is "required" or not since you need logic to
                // determine that.
                if (hintSettings.removeItems.ContainsKey("majorItems"))
                {
                    foreach (Item removedItem in hintSettings.removeItems["majorItems"])
                    {
                        itemSet.Remove(removedItem);
                    }
                }
            }

            // Handle tradeItems (bugs and sketch)

            // A tradeItem only prevents barren if it eventually leads to a
            // preventBarren item at the end of the chain. We do not want to
            // have to worry about Ashei's Sketch or a specific bug showing up
            // in the middle of a bug chain that leads to a Purple Rupee.
            // TradeItem hint calculations assume only the check at the end of
            // the trade chain is what is important, and I would rather not make
            // that stuff more complicated for a use case that would just lead
            // to confusing behavior for players. - isaac
            foreach (KeyValuePair<Item, string> pair in HintUtils.tradeItemToRewardCheck)
            {
                itemSet.Remove(pair.Key);
            }

            // Any tradeItems which lead to a preventBarren item or required
            // check also prevent barren.
            foreach (KeyValuePair<Item, string> pair in HintUtils.tradeItemToRewardCheck)
            {
                Item tradeItem = pair.Key;
                if (tradeItemToChainEndCheck.ContainsKey(tradeItem))
                {
                    string chainEndCheckName = tradeItemToChainEndCheck[tradeItem];
                    Item chainEndItem = HintUtils.getCheckContents(chainEndCheckName);
                    // Prevents barren if chainEnd is required or a
                    // preventBarren item which is not on an allowBarren check.
                    // For example, if the player started with 2 clawshots and
                    // there was a 3rd clawshot on Agitha, the tradeItems that
                    // lead to the 3rd clawshot would not preventBarren. This is
                    // basically the same check as if a check is "good".
                    if (
                        requiredChecks.Contains(chainEndCheckName)
                        || (
                            itemSet.Contains(chainEndItem)
                            && !allowBarrenChecks.Contains(chainEndCheckName)
                        )
                    )
                        itemSet.Add(tradeItem);
                }
            }

            return itemSet;
        }

        private HashSet<string> prepareAllowBarrenChecks()
        {
            HashSet<string> allowBarrenCheckSet = new();

            Dictionary<Item, int> itemToProgCount =
                new()
                {
                    // __Item Wheel__
                    { Item.Progressive_Clawshot, 2 },
                    { Item.Progressive_Dominion_Rod, 2 },
                    { Item.Ball_and_Chain, 1 },
                    { Item.Spinner, 1 },
                    { Item.Progressive_Bow, 1 },
                    { Item.Iron_Boots, 1 },
                    { Item.Boomerang, 1 },
                    { Item.Lantern, 1 },
                    { Item.Slingshot, 1 },
                    { Item.Progressive_Fishing_Rod, 2 },
                    { Item.Filled_Bomb_Bag, 1 },
                    // - handle bottles in the future if needed. Will be easier
                    //   to handle after Coro bottle can always be dumped, so
                    //   waiting on that rather than adding a temporary complex
                    //   implementation. Not expecting it to be noticeable
                    //   either way at the moment.
                    { Item.Asheis_Sketch, 1 },
                    { Item.Progressive_Sky_Book, 7 },
                    { Item.Aurus_Memo, 1 },
                    // __Collection Screen__
                    { Item.Progressive_Sword, 4 },
                    // - shields handled separately
                    { Item.Zora_Armor, 1 },
                    { Item.Magic_Armor, 1 },
                    // __Bugs__
                    { Item.Female_Ant, 1 },
                    { Item.Female_Beetle, 1 },
                    { Item.Female_Butterfly, 1 },
                    { Item.Female_Dayfly, 1 },
                    { Item.Female_Dragonfly, 1 },
                    { Item.Female_Grasshopper, 1 },
                    { Item.Female_Ladybug, 1 },
                    { Item.Female_Mantis, 1 },
                    { Item.Female_Phasmid, 1 },
                    { Item.Female_Pill_Bug, 1 },
                    { Item.Female_Snail, 1 },
                    { Item.Female_Stag_Beetle, 1 },
                    { Item.Male_Ant, 1 },
                    { Item.Male_Beetle, 1 },
                    { Item.Male_Butterfly, 1 },
                    { Item.Male_Dayfly, 1 },
                    { Item.Male_Dragonfly, 1 },
                    { Item.Male_Grasshopper, 1 },
                    { Item.Male_Ladybug, 1 },
                    { Item.Male_Mantis, 1 },
                    { Item.Male_Phasmid, 1 },
                    { Item.Male_Pill_Bug, 1 },
                    { Item.Male_Snail, 1 },
                    { Item.Male_Stag_Beetle, 1 },
                    // __Dungeon Keys__
                    { Item.Forest_Temple_Big_Key, 1 },
                    { Item.Forest_Temple_Small_Key, 4 },
                    { Item.Goron_Mines_Key_Shard, 3 },
                    { Item.Goron_Mines_Small_Key, 3 },
                    { Item.Lakebed_Temple_Big_Key, 1 },
                    { Item.Lakebed_Temple_Small_Key, 3 },
                    { Item.Arbiters_Grounds_Big_Key, 1 },
                    { Item.Arbiters_Grounds_Small_Key, 5 },
                    { Item.Snowpeak_Ruins_Bedroom_Key, 1 },
                    { Item.Snowpeak_Ruins_Small_Key, 3 },
                    { Item.Snowpeak_Ruins_Ordon_Goat_Cheese, 1 },
                    { Item.Snowpeak_Ruins_Ordon_Pumpkin, 1 },
                    { Item.Temple_of_Time_Big_Key, 1 },
                    { Item.Temple_of_Time_Small_Key, 3 },
                    { Item.City_in_The_Sky_Big_Key, 1 },
                    { Item.City_in_The_Sky_Small_Key, 1 },
                    { Item.Palace_of_Twilight_Big_Key, 1 },
                    { Item.Palace_of_Twilight_Small_Key, 7 },
                    { Item.Hyrule_Castle_Big_Key, 1 },
                    { Item.Hyrule_Castle_Small_Key, 3 },
                    // __Other__
                    { Item.Shadow_Crystal, 1 },
                    { Item.Gate_Keys, 1 },
                    { Item.North_Faron_Woods_Gate_Key, 1 },
                    { Item.Gerudo_Desert_Bulblin_Camp_Key, 1 },
                    { Item.Progressive_Fused_Shadow, 3 },
                    { Item.Progressive_Mirror_Shard, 3 },
                };

            Dictionary<Item, int> itemToInflexibleCount = new();
            foreach (string checkName in requiredChecks)
            {
                Item item = HintUtils.getCheckContents(checkName);
                if (!itemToInflexibleCount.ContainsKey(item))
                    itemToInflexibleCount[item] = 0;
                itemToInflexibleCount[item] += 1;
            }

            foreach (Item item in sSettings.startingItems)
            {
                if (!itemToInflexibleCount.ContainsKey(item))
                    itemToInflexibleCount[item] = 0;
                itemToInflexibleCount[item] += 1;
            }

            // TODO: handle items with different IDs which are functionally
            // equivalent separately such as shields. Waiting on Coro bottle
            // dumping code before messing with bottles.

            foreach (KeyValuePair<Item, int> pair in itemToProgCount)
            {
                Item item = pair.Key;
                int progCount = pair.Value;

                if (itemToInflexibleCount.ContainsKey(item) && itemToChecksList.ContainsKey(item))
                {
                    int inflexibleCount = itemToInflexibleCount[item];
                    if (inflexibleCount >= progCount)
                    {
                        // Mark any unrequired checks with this item as
                        // allowBarren.
                        List<string> checksForItem = itemToChecksList[item];
                        foreach (string checkName in checksForItem)
                        {
                            if (!requiredChecks.Contains(checkName))
                                allowBarrenCheckSet.Add(checkName);
                        }
                    }
                }
            }

            return allowBarrenCheckSet;
        }

        private void prepareTradeItemData()
        {
            foreach (KeyValuePair<Item, string> pair in HintUtils.tradeItemToRewardCheck)
            {
                Item tradeItem = pair.Key;
                if (itemToChecksList.ContainsKey(tradeItem))
                {
                    List<string> checkNames = itemToChecksList[tradeItem];

                    string finalCheck = HintUtils.GetTradeChainFinalCheck(checkNames[0]);
                    // If circular, we do not make any changes to data
                    // structures. At the end, we can figure out which
                    // tradeItems should prevent barren by iterating over the
                    // tradeItems (any Items which do not have a key clearly do
                    // not prevent barren).
                    if (finalCheck == null)
                        continue;

                    tradeItemToChainEndCheck[tradeItem] = finalCheck;

                    Item finalReward = HintUtils.getCheckContents(finalCheck);
                    foreach (string checkName in checkNames)
                    {
                        // Skip over any checks which are tradeItemReward checks
                        // (and therefore not chain starters). Note we also
                        // never hint chains that start with a tradeItem that
                        // the player has in their startingItems.
                        if (!HintUtils.CheckIsTradeItemReward(checkName))
                            tradeChainStartToReward[checkName] = finalReward;
                    }
                }
            }
        }

        private bool recursiveCheckSingleItemPreventBarren(
            Item item,
            HashSet<Item> preventBarrenItemSet
        )
        {
            if (!HintConstants.singleCheckItems.ContainsKey(item))
                throw new Exception(
                    $"`recursiveCheckSingleItemPreventBarren` called with invalid item `{item}`."
                );

            string rewardCheck = HintConstants.singleCheckItems[item];
            if (HintUtils.checkIsExcluded(rewardCheck))
                return false;

            Item rewardContents = HintUtils.getCheckContents(rewardCheck);

            if (preventBarrenItemSet.Contains(rewardContents))
                return true;
            else if (HintConstants.singleCheckItems.ContainsKey(rewardContents))
                return recursiveCheckSingleItemPreventBarren(rewardContents, preventBarrenItemSet);

            return false;
        }

        private bool getItemHardRequired(Item item)
        {
            foreach (string checkName in requiredChecks)
            {
                if (HintUtils.getCheckContents(checkName) == item)
                    return true;
            }

            return false;
        }

        private Dictionary<AreaId, HashSet<Item>> prepareAreaIdToAllowBarrenItems()
        {
            Dictionary<AreaId, HashSet<Item>> ret = new();

            HashSet<Item> baseAllowedForDungeons = new() { };

            if (!sSettings.shuffleRewards)
            {
                baseAllowedForDungeons.Add(Item.Progressive_Fused_Shadow);
                baseAllowedForDungeons.Add(Item.Progressive_Mirror_Shard);
            }

            ret[AreaId.Zone(Zone.Forest_Temple)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Goron_Mines)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Lakebed_Temple)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Arbiters_Grounds)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Snowpeak_Ruins)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Temple_of_Time)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.City_in_the_Sky)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Palace_of_Twilight)] = new(baseAllowedForDungeons);
            ret[AreaId.Zone(Zone.Hyrule_Castle)] = new(baseAllowedForDungeons);
            ret[AreaId.Province(Province.Dungeon)] = new(baseAllowedForDungeons);

            if (sSettings.smallKeySettings == SmallKeySettings.Own_Dungeon)
            {
                ret[AreaId.Zone(Zone.Forest_Temple)].Add(Item.Forest_Temple_Small_Key);
                ret[AreaId.Zone(Zone.Goron_Mines)].Add(Item.Goron_Mines_Small_Key);
                ret[AreaId.Zone(Zone.Lakebed_Temple)].Add(Item.Lakebed_Temple_Small_Key);
                ret[AreaId.Zone(Zone.Arbiters_Grounds)].Add(Item.Arbiters_Grounds_Small_Key);
                ret[AreaId.Zone(Zone.Snowpeak_Ruins)].UnionWith(
                    new HashSet<Item>()
                    {
                        Item.Snowpeak_Ruins_Small_Key,
                        Item.Snowpeak_Ruins_Ordon_Pumpkin,
                        Item.Snowpeak_Ruins_Ordon_Goat_Cheese,
                    }
                );
                ret[AreaId.Zone(Zone.Temple_of_Time)].Add(Item.Temple_of_Time_Small_Key);
                ret[AreaId.Zone(Zone.City_in_the_Sky)].Add(Item.City_in_The_Sky_Small_Key);
                ret[AreaId.Zone(Zone.Palace_of_Twilight)].Add(Item.Palace_of_Twilight_Small_Key);
                ret[AreaId.Zone(Zone.Hyrule_Castle)].Add(Item.Hyrule_Castle_Small_Key);

                ret[AreaId.Province(Province.Dungeon)].UnionWith(
                    new HashSet<Item>()
                    {
                        Item.Forest_Temple_Small_Key,
                        Item.Goron_Mines_Small_Key,
                        Item.Lakebed_Temple_Small_Key,
                        Item.Arbiters_Grounds_Small_Key,
                        Item.Snowpeak_Ruins_Small_Key,
                        Item.Snowpeak_Ruins_Ordon_Pumpkin,
                        Item.Snowpeak_Ruins_Ordon_Goat_Cheese,
                        Item.Temple_of_Time_Small_Key,
                        Item.City_in_The_Sky_Small_Key,
                        Item.Palace_of_Twilight_Small_Key,
                        Item.Hyrule_Castle_Small_Key,
                    }
                );
            }

            if (sSettings.bigKeySettings == BigKeySettings.Own_Dungeon)
            {
                ret[AreaId.Zone(Zone.Forest_Temple)].Add(Item.Forest_Temple_Big_Key);
                ret[AreaId.Zone(Zone.Goron_Mines)].Add(Item.Goron_Mines_Key_Shard);
                ret[AreaId.Zone(Zone.Lakebed_Temple)].Add(Item.Lakebed_Temple_Big_Key);
                ret[AreaId.Zone(Zone.Arbiters_Grounds)].Add(Item.Arbiters_Grounds_Big_Key);
                ret[AreaId.Zone(Zone.Snowpeak_Ruins)].Add(Item.Snowpeak_Ruins_Bedroom_Key);
                ret[AreaId.Zone(Zone.Temple_of_Time)].Add(Item.Temple_of_Time_Big_Key);
                ret[AreaId.Zone(Zone.City_in_the_Sky)].Add(Item.City_in_The_Sky_Big_Key);
                ret[AreaId.Zone(Zone.Palace_of_Twilight)].Add(Item.Palace_of_Twilight_Big_Key);
                ret[AreaId.Zone(Zone.Hyrule_Castle)].Add(Item.Hyrule_Castle_Big_Key);

                ret[AreaId.Province(Province.Dungeon)].UnionWith(
                    new HashSet<Item>()
                    {
                        Item.Forest_Temple_Big_Key,
                        Item.Goron_Mines_Key_Shard,
                        Item.Lakebed_Temple_Big_Key,
                        Item.Arbiters_Grounds_Big_Key,
                        Item.Snowpeak_Ruins_Bedroom_Key,
                        Item.Temple_of_Time_Big_Key,
                        Item.City_in_The_Sky_Big_Key,
                        Item.Palace_of_Twilight_Big_Key,
                        Item.Hyrule_Castle_Big_Key,
                    }
                );
            }

            return ret;
        }

        public bool ItemAllowsBarrenForArea(Item item, AreaId areaId)
        {
            return areaIdToAllowBarrenItems.ContainsKey(areaId)
                && areaIdToAllowBarrenItems[areaId].Contains(item);
        }

        private Dictionary<Item, List<string>> calcItemToChecksList()
        {
            Dictionary<Item, List<string>> itemToChecks = new();

            foreach (KeyValuePair<string, Check> pair in Randomizer.Checks.CheckDict)
            {
                Check check = pair.Value;
                Item contents = check.itemId;
                if (!itemToChecks.ContainsKey(contents))
                {
                    itemToChecks[contents] = new();
                }
                List<string> checkNameList = itemToChecks[contents];
                checkNameList.Add(pair.Value.checkName);
            }

            return itemToChecks;
        }

        public bool isCheckSphere0(string checkName)
        {
            return playthroughSpheres.sphere0Checks.Contains(checkName);
        }

        // For almost all cases, you should not bypassIgnoredChecks. Currently the only reason to bypassIgnoredChecks is when

        // The purpose
        // of this is for checking if the final bug reward in a tradeChain would
        // be considered good (ignoring the fact that it should be ignored by
        // basically every hintType when Agitha rewards are hinted).
        public bool checkCanBeHintedSpol(string checkName, bool bypassIgnoredChecks = false)
        {
            Item contents = HintUtils.getCheckContents(checkName);
            if (
                HintUtils.checkIsPlayerKnownStatus(checkName)
                || !requiredChecks.Contains(checkName)
                || HintConstants.invalidSpolItems.Contains(contents)
                || hinted.alreadyCheckContentsHinted.Contains(checkName)
                || hinted.alreadyCheckDirectedToward.Contains(checkName)
            )
                return false;

            if (hinted.hintsShouldIgnoreChecks.Contains(checkName))
            {
                if (!bypassIgnoredChecks)
                    return false;

                // If ignored Agitha check, only valid during bypass if the
                // checkContents are not a different tradeItem (which would
                // indicate we are in the middle of a chain).
                return !HintUtils.IsTradeItem(contents);
            }

            return true;
        }

        public bool checkCanBeLocationHinted(string checkName)
        {
            // We should ignore checks which are directed toward. We may want to
            // not hint toward any checks in a zone which is SpoL? This was the
            // previous behavior. Probably fine to list them in a SpoL zone.
            // This way you can know that specific check wasn't the SpoL one.
            // Can adjust later if doesn't make sense.
            return !hinted.alreadyCheckKnownBarren.Contains(checkName)
                && !hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !hinted.alreadyCheckDirectedToward.Contains(checkName)
                && !hinted.hintsShouldIgnoreChecks.Contains(checkName)
                && !HintUtils.checkIsPlayerKnownStatus(checkName);
        }

        public List<AreaId> ResolveToAreaIds(string name, HashSet<AreaId.AreaType> validAreaTypes)
        {
            if (HintSettingUtils.IsVarDefinition(name))
            {
                return vars.ResolveToAreaIds(name);
            }
            else if (HintSettingUtils.IsAreaDefinition(name))
            {
                AreaId areaId = AreaId.ParseString(name);
                if (areaId == null)
                    throw new Exception($"Failed to resolve '{name}' to an areaId.");
                return new() { areaId };
            }

            throw new Exception($"Failed to resolve '{name}' to areaIds.");
        }

        public HashSet<string> ResolveToChecks(string name)
        {
            if (HintSettingUtils.IsVarDefinition(name))
            {
                // Resolve as var.
                return vars.ResolveDefToChecks(name);
            }
            else if (HintSettingUtils.IsAreaDefinition(name))
            {
                // Resolve as an area.
                AreaId areaId = AreaId.ParseString(name);
                return areaId.ResolveToChecks();
            }

            // Resolve as checkName.
            if (!CheckIdClass.IsValidCheckName(name))
                throw new Exception($"Failed to resolve '{name}' as a checkName.");
            return new() { name };
        }

        public bool CheckShouldBeIgnored(string checkName)
        {
            return (
                hinted.hintsShouldIgnoreChecks.Contains(checkName)
                || HintUtils.checkIsPlayerKnownStatus(checkName)
            );
        }

        public CheckStatus CalcCheckStatus(string checkName)
        {
            CheckStatus status = CheckStatus.Bad;
            if (requiredChecks.Contains(checkName))
                status = CheckStatus.Required;
            else if (CheckIsGood(checkName))
                status = CheckStatus.Good;
            return status;
        }

        // For almost all cases, you should not bypassIgnoredChecks. The purpose
        // of this is for checking if the final bug reward in a tradeChain would
        // be considered good (ignoring the fact that it should be ignored by
        // basically every hintType when Agitha rewards are hinted).
        public bool CheckIsGood(string checkName, bool bypassIgnoredChecks = false)
        {
            // Could potentially determine all "good" checks at one time, but we
            // would have to do it after we generate Agitha hints since this
            // modifies hintsShouldIgnoreChecks, but it would be more fragile.
            // Can worry about slight performance improvements in the future
            // (path stuff is the real bottleneck). "premature optimization is
            // the root of all evil"

            // Order this is checked is important.
            if (HintUtils.checkIsPlayerKnownStatus(checkName))
            {
                // For example, don't want to say a vanilla big key is good if
                // the check is required. Hinting it when the player knows it is
                // vanilla is not helpful (such as BeyondThisPoint hint).
                return false;
            }
            if (!bypassIgnoredChecks && hinted.hintsShouldIgnoreChecks.Contains(checkName))
                return false;
            if (requiredChecks.Contains(checkName))
                return true;
            if (allowBarrenChecks.Contains(checkName))
                return false;

            Item item = HintUtils.getCheckContents(checkName);
            // Note that this handles tradeItems correctly because any
            // tradeItems that lead to a required check or a preventBarren item
            // which is not an an allowBarren check are said to preventBarren.
            // Therefore when you ask it a check which reward Male_Ant for
            // example is good, you will receive an accurate response. We do not
            // allow players to manually determine if tradeItems preventBarren
            // or not (they are derived based off of other items).
            return preventBarrenItems.Contains(item);
        }

        public bool CheckIsRequired(string checkName)
        {
            return requiredChecks.Contains(checkName);
        }

        public CheckStatus CalcCheckStatus(string checkName, bool bypassIgnoredChecks = false)
        {
            if (CheckIsRequired(checkName))
                return CheckStatus.Required;
            else if (CheckIsGood(checkName, bypassIgnoredChecks))
                return CheckStatus.Good;
            else
                return CheckStatus.Bad;
        }

        public bool ItemUsesDefArticle(Item item)
        {
            if (itemToChecksList.TryGetValue(item, out List<string> checksGivingItem))
                return checksGivingItem.Count == 1;
            return false;
        }

        public AreaId GetRecommendedAreaId(string checkName)
        {
            Item item = HintUtils.getCheckContents(checkName);
            Province province = HintUtils.checkNameToHintProvince(checkName);
            string zoneName = HintUtils.checkNameToHintZone(checkName);
            if (
                zoneName == "Agitha's Castle"
                || province == Province.Dungeon
                || itemToChecksList[item].Count > 1
            )
                return AreaId.ZoneStr(zoneName);
            else
                return AreaId.Province(province);
        }

        public AreaId.AreaType GetRecommendedAreaIdType(string startCheckName, Item item)
        {
            Province province = HintUtils.checkNameToHintProvince(startCheckName);
            string zoneName = HintUtils.checkNameToHintZone(startCheckName);
            if (
                zoneName == "Agitha's Castle"
                || province == Province.Dungeon
                || itemToChecksList[item].Count > 1
            )
                return AreaId.AreaType.Zone;
            else
                return AreaId.AreaType.Province;
        }

        public double GetAreaWothWeight(AreaId areaId)
        {
            // This weighting takes a few factors into consideration. For large
            // areas which the player will always check anyway (such as
            // dungeons, desert, LLC), we give a weight of 1. Tiny areas which
            // are all sphere0 get a weight of 1.5. Other areas have an average
            // weight of 3ish. The following give bonus weight: (1) having more
            // non-sphere0 checks and (2) having a required non-sphere0 check
            // while having sphere0 checks. With how the weights play out, we
            // expect to usually pick 1 or 0 boring areas (dungeons, desert,
            // LLC) after the first 3 picks. All of the adjustments are designed
            // to produce more interesting hints on average.

            int numChecks = 0;
            int numSphere0Checks = 0;
            bool hasSphereLater = false;

            HashSet<string> checkNames = areaId.ResolveToChecks();
            foreach (string checkName in checkNames)
            {
                if (
                    HintUtils.checkIsPlayerKnownStatus(checkName)
                    || hinted.hintsShouldIgnoreChecks.Contains(checkName)
                    || hinted.alreadyCheckContentsHinted.Contains(checkName)
                    || hinted.alreadyCheckDirectedToward.Contains(checkName)
                )
                    continue;

                numChecks += 1;
                if (isCheckSphere0(checkName))
                    numSphere0Checks += 1;
                else if (checkCanBeHintedSpol(checkName))
                    hasSphereLater = true;
            }

            double percentSphere0 = (double)numSphere0Checks / numChecks;

            if (numChecks < 1)
                return 0;
            else if (numChecks >= 12)
                return 1;
            else if (numChecks <= 2 && percentSphere0 <= 0)
                return 1.5;

            double weight = 2 + Math.Pow(1 - percentSphere0, 2);

            if (numSphere0Checks > 0 && hasSphereLater)
                weight += 2 * Math.Pow((double)1 / (numChecks - 2), 0.25);

            return weight;
        }
    }

    public class HintedThings3
    {
        public readonly HashSet<string> alreadyCheckContentsHinted = new();
        public readonly HashSet<string> alreadyCheckDirectedToward = new();
        public readonly HashSet<string> hintsShouldIgnoreChecks = new();
        public readonly HashSet<string> alreadyCheckKnownBarren = new();
        public readonly HashSet<TradeGroup> hintedTradeGroups = new();
        public readonly HashSet<string> alwaysHintedChecks = new();
        public readonly HashSet<Zone> hintedBarrenZones = new();
        public readonly HashSet<AreaId> hintedWothAreas = new();
        public bool agithaHintedDead = false;

        // private
        private readonly Dictionary<int, int> hintedBarrenDungeonCache = new();
        private readonly Dictionary<int, int> hintedWothDungeonCache = new();
        private readonly HashSet<string> ignoreForBarrenWeighting = new();

        public int GetNumHintedBarrenDungeons()
        {
            if (ListUtils.isEmpty(hintedBarrenZones))
                return 0;

            int key = hintedBarrenZones.Count;

            if (hintedBarrenDungeonCache.ContainsKey(key))
                return hintedBarrenDungeonCache[key];

            int count = 0;
            foreach (Zone zone in hintedBarrenZones)
            {
                if (HintUtils.hintZoneIsDungeon(ZoneUtils.IdToString(zone)))
                    count += 1;
            }

            hintedBarrenDungeonCache.Clear();
            hintedBarrenDungeonCache[key] = count;

            return count;
        }

        public int GetNumHintedWothDungeons()
        {
            if (ListUtils.isEmpty(hintedWothAreas))
                return 0;

            int key = hintedWothAreas.Count;

            if (hintedWothDungeonCache.ContainsKey(key))
                return hintedWothDungeonCache[key];

            int count = 0;
            foreach (AreaId areaId in hintedWothAreas)
            {
                if (
                    areaId.type == AreaId.AreaType.Zone
                    && HintUtils.hintZoneIsDungeon(areaId.stringId)
                )
                    count += 1;
            }

            hintedWothDungeonCache.Clear();
            hintedWothDungeonCache[key] = count;

            return count;
        }

        public void AddNonWeightedBarrenCheck(string checkName)
        {
            if (StringUtils.isEmpty(checkName))
                return;

            if (!alreadyCheckKnownBarren.Contains(checkName))
                ignoreForBarrenWeighting.Add(checkName);
            alreadyCheckKnownBarren.Add(checkName);
        }

        public void AddHintedBarrenCheck(string checkName)
        {
            if (StringUtils.isEmpty(checkName))
                return;
            alreadyCheckKnownBarren.Add(checkName);
            ignoreForBarrenWeighting.Remove(checkName);
        }

        public void AddHintedBarrenChecks(ICollection<string> checkNames)
        {
            if (ListUtils.isEmpty(checkNames))
                return;

            alreadyCheckKnownBarren.UnionWith(checkNames);
            ignoreForBarrenWeighting.RemoveWhere(
                (checkName) => checkName != null && checkNames.Contains(checkName)
            );
        }

        public bool IsIgnoreCheckForBarrenWeighting(string checkName)
        {
            if (StringUtils.isEmpty(checkName))
                return false;
            return ignoreForBarrenWeighting.Contains(checkName);
        }
    }

    public class HintVars
    {
        private Dictionary<string, List<Hint>> varNameToHints = new();

        public void SaveToVar(string varName, List<Hint> hints)
        {
            if (StringUtils.isEmpty(varName))
                throw new Exception("Cannot save to empty varName.");

            if (ListUtils.isEmpty(hints))
                return;

            if (!varNameToHints.ContainsKey(varName))
                varNameToHints[varName] = new();

            List<Hint> hintsForVar = varNameToHints[varName];
            foreach (Hint hint in hints)
            {
                if (hint != null)
                    hintsForVar.Add(hint);
            }
        }

        public List<AreaId> ResolveToAreaIds(string varName)
        {
            List<AreaId> results = new();
            if (StringUtils.isEmpty(varName) || !varNameToHints.ContainsKey(varName))
                return results;

            List<Hint> hints = varNameToHints[varName];
            if (ListUtils.isEmpty(hints))
                return results;

            foreach (Hint hint in hints)
            {
                IAreaHinter areaHinter = hint as IAreaHinter;
                if (areaHinter != null)
                    results.Add(areaHinter.GetAreaId());
            }

            return results;
        }

        private List<Hint> GetHintsForVarName(string varName)
        {
            if (StringUtils.isEmpty(varName) || !varNameToHints.ContainsKey(varName))
                return new();

            List<Hint> hints = varNameToHints[varName];

            if (ListUtils.isEmpty(hints))
                return new();
            return hints;
        }

        private List<AreaId> HintsToAreaIds(List<Hint> hints, int? max = null)
        {
            List<AreaId> results = new();
            if (!ListUtils.isEmpty(hints))
            {
                int currentIndex = 0;
                foreach (Hint hint in hints)
                {
                    if (max != null && currentIndex >= max)
                        break;

                    IAreaHinter areaHinter = hint as IAreaHinter;
                    if (areaHinter != null)
                    {
                        AreaId areaId = areaHinter.GetAreaId();
                        if (areaId != null)
                            results.Add(areaId);
                    }
                    currentIndex += 1;
                }
            }
            return results;
        }

        public HashSet<string> ResolveDefToChecks(string varDef)
        {
            KeyValuePair<string, string> varParts = HintSettingUtils.ParseVarDefinition(varDef);
            string varName = varParts.Key;
            string property = varParts.Value;

            List<Hint> hints = GetHintsForVarName(varName);
            if (ListUtils.isEmpty(hints))
                return new();

            switch (property)
            {
                case "areaIds":
                {
                    HashSet<string> checkNames = new();
                    List<AreaId> areaIds = HintsToAreaIds(hints);
                    foreach (AreaId areaId in areaIds)
                    {
                        HashSet<string> partial = areaId.ResolveToChecks();
                        checkNames.UnionWith(partial);
                    }
                    return checkNames;
                }
                case "areaId":
                {
                    List<AreaId> areaIds = HintsToAreaIds(hints, 1);
                    if (!ListUtils.isEmpty(areaIds))
                        return areaIds[0].ResolveToChecks();
                    return new();
                }
                default:
                    throw new Exception($"Failed to reesolve property of '{varDef}'.");
            }
        }
    }
}
