namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Transactions;
    using Microsoft.CodeAnalysis;
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
        public bool isRaceSeed { get; private set; }
        public HintedThings3 hinted { get; }
        public HintVars vars { get; }

        public Dictionary<Goal, List<string>> goalToRequiredChecks { get; set; }
        public HashSet<string> requiredChecks { get; set; }
        public HashSet<string> condReqChecks { get; set; } = new();
        public HashSet<string> notReqChecks { get; set; } = new();
        public bool? agithaRequired { get; set; }
        public HashSet<Item> preventBarrenItems { get; private set; }
        public HashSet<string> allowBarrenChecks { get; private set; }
        public HashSet<Item> majorItems { get; private set; }
        public HashSet<Item> logicalItems { get; private set; }
        public HashSet<Item> logicalItems2 { get; private set; }
        public Dictionary<Item, List<string>> itemToChecksList { get; set; }
        public Dictionary<string, Item> tradeChainStartToReward = new();
        public Dictionary<Item, string> tradeItemToChainEndCheck = new();
        public Dictionary<AreaId, HashSet<Item>> areaIdToAllowBarrenItems { get; private set; }
        public HashSet<string> unreachableChecks = new();
        private Dictionary<AreaId, AreaCheckInfo> areaToCheckInfo = new();
        private Dictionary<string, Zone> checkNameToZone = new();
        public Dictionary<Zone, HashSet<Zone>> dungeonEntrances = new(); // Entering Key sends to Value(s)

        private HintSettings hintSettings;
        private Dictionary<Item, int> multiToMaxItems = new();

        public HintGenData(
            Random rnd,
            SharedSettings sSettings,
            PlaythroughSpheres playthroughSpheres,
            Room startingRoom,
            bool isRaceSeed
        )
        {
            this.rnd = rnd;
            this.sSettings = sSettings;
            this.playthroughSpheres = playthroughSpheres;
            this.startingRoom = startingRoom;
            this.isRaceSeed = isRaceSeed;
            hinted = new HintedThings3();
            vars = new HintVars();

            unreachableChecks = calcUnreachableChecks();
            calcAreaToCheckInfo();
            dungeonEntrances = calcDungeonEntrances();
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

                foreach (string checkName in requiredChecks)
                {
                    Item contents = HintUtils.getCheckContents(checkName);
                    Console.WriteLine($"Required Check: {checkName} ({contents})");
                }

                agithaRequired = HintUtils.CalcAgithaRequired(startingRoom, sSettings);
            }
            else
            {
                goalToRequiredChecks = new();
                requiredChecks = new();
            }
        }

        public void updateFromHintSettings(HintSettings hintSettings)
        {
            this.hintSettings = hintSettings;
            // TODO: Important items must be a subset of major items for the hint, so a skippable
            // poe soul when they aren't all sometimes required would not be counted as important.
            // This is because poeSouls are only major if needed for HC, even if a Jovani reward is
            // required for example (and even if the poe soul is itself "required"; we would prevent
            // a barren hint from creating, but the IC might still be 0).

            majorItems = prepMajorItems();
            prepPreventBarrenAndLogicalItemSets(hintSettings);

            prepLogicalItemAndMultiMax();

            Dictionary<Item, int> itemToInflexibleCount = new();
            allowBarrenChecks = prepareAllowBarrenChecks(itemToInflexibleCount);

            if (sSettings.logicRules != LogicRules.No_Logic)
            {
                // Calculate conditionallyRequired checks. This depends on knowing the "logical
                // items" and "allowBarrenChecks", so has to wait until here.
                if (hintSettings.barren.blockerType == Barren.BlockerType.Important)
                {
                    HintCondReqCalc condReqCalc = new(this);
                    condReqChecks = condReqCalc.run(itemToInflexibleCount);
                }
            }

            // Potentially mark checks rewarding poeSouls as "not required" if poe souls do not
            // serve any purpose based on settings / Jovani rewards.
            if (
                sSettings.castleRequirements != CastleRequirements.Poe_Souls
                && sSettings.castleBKRequirements != CastleBKRequirements.Poe_Souls
            )
            {
                bool hasGoodJovaniReward = false;
                List<string> jovaniChecks =
                    new() { "Jovani 20 Poe Soul Reward", "Jovani 60 Poe Soul Reward", };
                foreach (string checkName in jovaniChecks)
                {
                    if (allowBarrenChecks.Contains(checkName))
                        continue;

                    Item contents = HintUtils.getCheckContents(checkName);

                    if (
                        hintSettings.barren.blockerType == Barren.BlockerType.NonJunk
                        && !HintConstants.junkItems.Contains(contents)
                    )
                    {
                        hasGoodJovaniReward = true;
                        break;
                    }

                    // If Jovani reward is a tradeItem
                    if (HintUtils.IsTradeItem(contents))
                    {
                        if (
                            !tradeItemToChainEndCheck.TryGetValue(
                                contents,
                                out string chainEndCheckName
                            )
                        )
                        {
                            // Chain is a loop, so not useful.
                            continue;
                        }

                        // We ignore PoeSouls here and below since Jovani giving you a chain ending
                        // in a poeSoul or a poeSoul directly is an unhelpful loop. For it to
                        // possibly be useful, a different Jovani check must be something useful
                        // other than a poe soul.
                        if (
                            CheckIsGoodOrSkippable(
                                chainEndCheckName,
                                allowVanillaChecks: true,
                                ignoreItemForSkippable: Item.Poe_Soul
                            )
                        )
                        {
                            hasGoodJovaniReward = true;
                            break;
                        }
                    }
                    else if (
                        // Basic handling for non-tradeItems.
                        CheckIsGoodOrSkippable(
                            checkName,
                            allowVanillaChecks: true,
                            ignoreItemForSkippable: Item.Poe_Soul
                        )
                    )
                    {
                        hasGoodJovaniReward = true;
                        break;
                    }
                }
                if (!hasGoodJovaniReward)
                {
                    // All skippable poeSoul checks can be marked as allowBarren. They are still
                    // considered logical so that a Location hint saying the checkStatus would
                    // indicate "not required". If it was not a logical item (such as an orange
                    // Rupee), then it would not indicate anything.
                    if (itemToChecksList.TryGetValue(Item.Poe_Soul, out List<string> checkNames))
                    {
                        foreach (string checkName in checkNames)
                        {
                            if (
                                !requiredChecks.Contains(checkName)
                                && !condReqChecks.Contains(checkName)
                            )
                            {
                                notReqChecks.Add(checkName);
                            }
                        }
                    }
                }
            }

            // We mark tradeItems as "not required" if they either have no chain end, the chain end
            // is unreachable, or the chain end is known bad, etc. We do this at this point after we
            // have finished all other additions to `notReqChecks`.
            foreach (KeyValuePair<Item, string> pair in HintUtils.tradeItemToRewardCheck)
            {
                Item tradeItem = pair.Key;
                if (tradeItemToChainEndCheck.TryGetValue(tradeItem, out string chainEndCheckName))
                {
                    if (!CheckIsGoodOrSkippable(chainEndCheckName, allowVanillaChecks: true))
                    {
                        // Mark all checks rewarding the tradeItem as "not required" if not already
                        // marked as "required", "sometimesRequired", or "allowBarren" (not expected
                        // to be in any of these, but just in case so we don't put it in multiple
                        // lists).
                        if (itemToChecksList.TryGetValue(tradeItem, out List<string> checksForItem))
                        {
                            foreach (string checkName in checksForItem)
                            {
                                if (
                                    !requiredChecks.Contains(checkName)
                                    && !condReqChecks.Contains(checkName)
                                    && !allowBarrenChecks.Contains(checkName)
                                )
                                    notReqChecks.Add(checkName);
                            }
                        }
                    }
                }
            }

            if (sSettings.logicRules != LogicRules.No_Logic)
            {
                if (hintSettings.barren.blockerType == Barren.BlockerType.Important)
                {
                    Dictionary<string, Item> importantChecks = new();
                    Dictionary<string, Item> majorItemChecks = new();
                    Dictionary<string, Item> noneOfAboveChecks = new();

                    // TODO: test code here. See if can calculate useful count for a basic S1 seed (for lake hylia).
                    // HashSet<string> lhChecks = AreaId.Zone(Zone.Lake_Hylia).ResolveToChecks();
                    HashSet<string> lhChecks = AreaId
                        .Zone(Zone.Gerudo_Desert)
                        .ResolveToChecks(this);
                    foreach (string lhCheckName in lhChecks)
                    {
                        bool added = false;
                        Item contents = HintUtils.getCheckContents(lhCheckName);
                        if (!unreachableChecks.Contains(lhCheckName))
                        {
                            if (majorItems.Contains(contents))
                            {
                                added = true;
                                majorItemChecks[lhCheckName] = contents;
                                if (
                                    requiredChecks.Contains(lhCheckName)
                                    || condReqChecks.Contains(lhCheckName)
                                )
                                {
                                    importantChecks[lhCheckName] = contents;
                                }
                            }
                        }
                        if (!added)
                        {
                            noneOfAboveChecks[lhCheckName] = contents;
                        }
                    }

                    int i = 0;
                }
            }
        }

        private void prepPreventBarrenAndLogicalItemSets(HintSettings hintSettings)
        {
            HashSet<Item> logicalItems = new(HintConstants.baseLogicalItems);

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
                };

            // Handle dungeonRewards
            bool noReasonToEnterPot =
                sSettings.barrenDungeons && !HintUtils.DungeonIsRequired("Palace of Twilight");

            if (
                (
                    !noReasonToEnterPot
                    && sSettings.palaceRequirements == PalaceRequirements.Fused_Shadows
                )
                || sSettings.castleRequirements == CastleRequirements.Fused_Shadows
            )
            {
                // This item is logical even if it does not prevent barren, but
                // only when it matter according to settings.
                logicalItems.Add(Item.Progressive_Fused_Shadow);
                if (sSettings.shuffleRewards)
                    itemSet.Add(Item.Progressive_Fused_Shadow);
            }

            if (
                (
                    !noReasonToEnterPot
                    && sSettings.palaceRequirements == PalaceRequirements.Mirror_Shards
                )
                || sSettings.castleRequirements == CastleRequirements.Mirror_Shards
            )
            {
                // This item is logical even if it does not prevent barren, but
                // only when it matter according to settings.
                logicalItems.Add(Item.Progressive_Fused_Shadow);
                if (sSettings.shuffleRewards)
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

            // Note: Coro's key should be handled by starting items with keysy
            // as well I think. Will be updating all of this logic with the
            // better algorithm in a few weeks. - isaac

            // Add all of the dungeon stuff.

            bool bigKeysPreventBarren =
                sSettings.bigKeySettings == BigKeySettings.Anywhere
                || sSettings.bigKeySettings == BigKeySettings.Any_Dungeon;

            bool smallKeysPreventBarren =
                sSettings.smallKeySettings == SmallKeySettings.Anywhere
                || sSettings.smallKeySettings == SmallKeySettings.Any_Dungeon;

            bool isBigKeysy = sSettings.bigKeySettings == BigKeySettings.Keysy;
            bool isSmallKeysy = sSettings.smallKeySettings == SmallKeySettings.Keysy;

            // Dungeon keys are logical even if they do not prevent barren for
            // dungeons that are not unrequiredBarren.
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Forest Temple"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Forest_Temple_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Forest_Temple_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.Forest_Temple_Big_Key);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.Forest_Temple_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Goron Mines"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Goron_Mines_Key_Shard);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Goron_Mines_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.Goron_Mines_Key_Shard);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.Goron_Mines_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Lakebed Temple"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Lakebed_Temple_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Lakebed_Temple_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.Lakebed_Temple_Big_Key);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.Lakebed_Temple_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Arbiter's Grounds"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Arbiters_Grounds_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Arbiters_Grounds_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.Arbiters_Grounds_Big_Key);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.Arbiters_Grounds_Small_Key);
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

                if (!isBigKeysy)
                    logicalItems.Add(Item.Snowpeak_Ruins_Bedroom_Key);
                if (!isSmallKeysy)
                {
                    logicalItems.Add(Item.Snowpeak_Ruins_Small_Key);
                    logicalItems.Add(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
                    logicalItems.Add(Item.Snowpeak_Ruins_Ordon_Pumpkin);
                }
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Temple of Time"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Temple_of_Time_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Temple_of_Time_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.Temple_of_Time_Big_Key);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.Temple_of_Time_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("City in the Sky"))
            {
                if (!sSettings.skipCityEntrance)
                    itemSet.Add(Item.Progressive_Sky_Book);

                if (bigKeysPreventBarren)
                    itemSet.Add(Item.City_in_The_Sky_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.City_in_The_Sky_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.City_in_The_Sky_Big_Key);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.City_in_The_Sky_Small_Key);
            }
            if (!sSettings.barrenDungeons || HintUtils.DungeonIsRequired("Palace of Twilight"))
            {
                if (bigKeysPreventBarren)
                    itemSet.Add(Item.Palace_of_Twilight_Big_Key);
                if (smallKeysPreventBarren)
                    itemSet.Add(Item.Palace_of_Twilight_Small_Key);

                if (!isBigKeysy)
                    logicalItems.Add(Item.Palace_of_Twilight_Big_Key);
                if (!isSmallKeysy)
                    logicalItems.Add(Item.Palace_of_Twilight_Small_Key);
            }

            if (bigKeysPreventBarren)
                itemSet.Add(Item.Hyrule_Castle_Big_Key);
            if (smallKeysPreventBarren)
                itemSet.Add(Item.Hyrule_Castle_Small_Key);

            if (!isBigKeysy)
                logicalItems.Add(Item.Hyrule_Castle_Big_Key);
            if (!isSmallKeysy)
                logicalItems.Add(Item.Hyrule_Castle_Small_Key);

            if (sSettings.logicRules == LogicRules.No_Logic)
            {
                // Note for other logics, one of these items only preventBarren
                // when there is a logically required check which rewards that
                // item. This way we do not have to worry about the desert or a
                // dungeon being not barren-hintable when it has an obviously
                // unrequired slingshot, etc. This does not impact the chance
                // that these items get hinted required (about 0.1% for
                // slingshot). This also prevents bugs and sketch which lead to
                // these items from preventing barren when pointless.
                itemSet.Add(Item.Slingshot);
                itemSet.Add(Item.Wooden_Shield);
                itemSet.Add(Item.Ordon_Shield);
                itemSet.Add(Item.Hylian_Shield);
            }

            // Can address this more when work is done around shop prices
            if (
                sSettings.walletSize == WalletSize.Reduced
                || (
                    !HintUtils.checkIsPlayerKnownStatus("Castle Town Malo Mart Magic Armor")
                    && (sSettings.walletSize <= WalletSize.HD)
                )
            )
                itemSet.Add(Item.Progressive_Wallet);

            // Mark all preventBarren items as logical items so we catch any
            // conditional ones (such as Magic Armor for non-Glitchless logic).
            // We do this here before making preventBarren adjustments from the
            // hint distribution.
            logicalItems.UnionWith(itemSet);

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
                        // && !allowBarrenChecks.Contains(chainEndCheckName)
                        )
                    )
                        // TODO: temp disabled allowBarrenChecks line above this since order stuff
                        // is in a weird state right now, and this function will probably go away
                        // anyway
                        itemSet.Add(tradeItem);
                }
            }

            this.preventBarrenItems = itemSet;
            this.logicalItems = logicalItems;
        }

        private HashSet<Item> prepMajorItems()
        {
            HashSet<Item> majorItems = new(HintConstants.baseMajorItems);

            // Filter out conditional majorItems as appropriate:

            if (!sSettings.shuffleRewards)
            {
                majorItems.Remove(Item.Progressive_Fused_Shadow);
                majorItems.Remove(Item.Progressive_Mirror_Shard);
            }

            if (
                sSettings.castleRequirements != CastleRequirements.Poe_Souls
                && sSettings.castleBKRequirements != CastleBKRequirements.Poe_Souls
            )
            {
                majorItems.Remove(Item.Poe_Soul);
            }

            if (
                sSettings.castleRequirements != CastleRequirements.Hearts
                && sSettings.castleBKRequirements != CastleBKRequirements.Hearts
            )
            {
                majorItems.Remove(Item.Heart_Container);
                majorItems.Remove(Item.Piece_of_Heart);
            }

            if (
                sSettings.smallKeySettings != SmallKeySettings.Any_Dungeon
                && sSettings.smallKeySettings != SmallKeySettings.Anywhere
            )
            {
                majorItems.Remove(Item.Forest_Temple_Small_Key);
                majorItems.Remove(Item.Goron_Mines_Small_Key);
                majorItems.Remove(Item.Lakebed_Temple_Small_Key);
                majorItems.Remove(Item.Arbiters_Grounds_Small_Key);
                majorItems.Remove(Item.Snowpeak_Ruins_Small_Key);
                majorItems.Remove(Item.Snowpeak_Ruins_Ordon_Pumpkin);
                majorItems.Remove(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
                majorItems.Remove(Item.Temple_of_Time_Small_Key);
                majorItems.Remove(Item.City_in_The_Sky_Small_Key);
                majorItems.Remove(Item.Palace_of_Twilight_Small_Key);
                majorItems.Remove(Item.Hyrule_Castle_Small_Key);
            }

            if (
                sSettings.bigKeySettings != BigKeySettings.Any_Dungeon
                && sSettings.bigKeySettings != BigKeySettings.Anywhere
            )
            {
                majorItems.Remove(Item.Forest_Temple_Big_Key);
                majorItems.Remove(Item.Goron_Mines_Key_Shard);
                majorItems.Remove(Item.Lakebed_Temple_Big_Key);
                majorItems.Remove(Item.Arbiters_Grounds_Big_Key);
                majorItems.Remove(Item.Temple_of_Time_Big_Key);
                majorItems.Remove(Item.Snowpeak_Ruins_Bedroom_Key);
                majorItems.Remove(Item.City_in_The_Sky_Big_Key);
                majorItems.Remove(Item.Palace_of_Twilight_Big_Key);
                majorItems.Remove(Item.Hyrule_Castle_Big_Key);
            }

            return majorItems;
        }

        private void prepLogicalItemAndMultiMax()
        {
            HashSet<Item> logicalItems = new(HintConstants.baseMajorItems);

            // From logicalItems, filter out any items which could not possibly serve a purpose
            // based on the settings:

            multiToMaxItems = new()
            {
                { Item.Progressive_Sword, 4 },
                { Item.Progressive_Fused_Shadow, 3 },
                { Item.Progressive_Mirror_Shard, 4 },
                { Item.Progressive_Hidden_Skill, 7 },
                { Item.Poe_Soul, 60 },
                { Item.Progressive_Clawshot, 2 },
                { Item.Progressive_Dominion_Rod, 2 },
                { Item.Progressive_Fishing_Rod, 2 },
                { Item.Progressive_Sky_Book, 7 },
                { Item.Forest_Temple_Small_Key, 4 },
                { Item.Goron_Mines_Small_Key, 3 },
                { Item.Lakebed_Temple_Small_Key, 3 },
                { Item.Arbiters_Grounds_Small_Key, 5 },
                { Item.Snowpeak_Ruins_Small_Key, 4 },
                { Item.Temple_of_Time_Small_Key, 3 },
                { Item.Palace_of_Twilight_Small_Key, 7 },
                { Item.Hyrule_Castle_Small_Key, 3 },
                { Item.Goron_Mines_Key_Shard, 3 },
            };

            // The number of wallets to logically max them out varies based on the size setting.
            switch (sSettings.walletSize)
            {
                case WalletSize.Reduced:
                    multiToMaxItems[Item.Progressive_Wallet] = 2;
                    break;
                case WalletSize.Vanilla:
                case WalletSize.HD:
                {
                    if (HintUtils.checkIsExcluded("Castle Town Malo Mart Magic Armor"))
                    {
                        multiToMaxItems[Item.Progressive_Wallet] = 0;
                        logicalItems.Remove(Item.Progressive_Wallet);
                    }
                    break;
                }
                case WalletSize.Large:
                    multiToMaxItems[Item.Progressive_Wallet] = 0;
                    logicalItems.Remove(Item.Progressive_Wallet);
                    break;
            }

            if (sSettings.logicRules == LogicRules.Glitchless)
            {
                logicalItems.Remove(Item.Magic_Armor);

                if (
                    !sSettings.bonksDoDamage
                    || sSettings.damageMagnification != DamageMagnification.OHKO
                )
                {
                    // Note: bottles can be used for step clips for glitched
                    logicalItems.Remove(Item.Coro_Bottle);
                    logicalItems.Remove(Item.Empty_Bottle);
                    logicalItems.Remove(Item.Jovani_Bottle);
                    logicalItems.Remove(Item.Sera_Bottle);
                }
            }

            if (
                sSettings.palaceRequirements != PalaceRequirements.Fused_Shadows
                && sSettings.castleRequirements != CastleRequirements.Fused_Shadows
                && sSettings.castleBKRequirements != CastleBKRequirements.Fused_Shadows
            )
            {
                logicalItems.Remove(Item.Progressive_Fused_Shadow);
            }

            if (
                sSettings.palaceRequirements != PalaceRequirements.Mirror_Shards
                && sSettings.castleRequirements != CastleRequirements.Mirror_Shards
                && sSettings.castleBKRequirements != CastleBKRequirements.Mirror_Shards
            )
            {
                logicalItems.Remove(Item.Progressive_Mirror_Shard);
            }

            if (
                sSettings.castleRequirements != CastleRequirements.Poe_Souls
                && sSettings.castleBKRequirements != CastleBKRequirements.Poe_Souls
                && HintUtils.checkIsExcluded("Jovani 20 Poe Soul Reward")
                && HintUtils.checkIsExcluded("Jovani 60 Poe Soul Reward")
            )
            {
                logicalItems.Remove(Item.Poe_Soul);
            }

            if (
                sSettings.castleRequirements != CastleRequirements.Hearts
                && sSettings.castleBKRequirements != CastleBKRequirements.Hearts
            )
            {
                logicalItems.Remove(Item.Heart_Container);
                logicalItems.Remove(Item.Piece_of_Heart);
            }

            if (sSettings.smallKeySettings == SmallKeySettings.Keysy)
            {
                logicalItems.Remove(Item.Forest_Temple_Small_Key);
                logicalItems.Remove(Item.Goron_Mines_Small_Key);
                logicalItems.Remove(Item.Lakebed_Temple_Small_Key);
                logicalItems.Remove(Item.Arbiters_Grounds_Small_Key);
                logicalItems.Remove(Item.Snowpeak_Ruins_Small_Key);
                logicalItems.Remove(Item.Snowpeak_Ruins_Ordon_Pumpkin);
                logicalItems.Remove(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
                logicalItems.Remove(Item.Temple_of_Time_Small_Key);
                logicalItems.Remove(Item.City_in_The_Sky_Small_Key);
                logicalItems.Remove(Item.Palace_of_Twilight_Small_Key);
                logicalItems.Remove(Item.Hyrule_Castle_Small_Key);
                // Note: after breaking OW keys out from the Dungeon small keysy setting, will need
                // to make adjustments here.
                logicalItems.Remove(Item.Faron_Woods_Coro_Key);
                logicalItems.Remove(Item.North_Faron_Woods_Gate_Key);
                logicalItems.Remove(Item.Gate_Keys);
                logicalItems.Remove(Item.Gerudo_Desert_Bulblin_Camp_Key);
            }

            if (sSettings.bigKeySettings == BigKeySettings.Keysy)
            {
                logicalItems.Remove(Item.Forest_Temple_Big_Key);
                logicalItems.Remove(Item.Goron_Mines_Key_Shard);
                logicalItems.Remove(Item.Lakebed_Temple_Big_Key);
                logicalItems.Remove(Item.Arbiters_Grounds_Big_Key);
                logicalItems.Remove(Item.Temple_of_Time_Big_Key);
                logicalItems.Remove(Item.Snowpeak_Ruins_Bedroom_Key);
                logicalItems.Remove(Item.City_in_The_Sky_Big_Key);
                logicalItems.Remove(Item.Palace_of_Twilight_Big_Key);
                logicalItems.Remove(Item.Hyrule_Castle_Big_Key);
            }

            if (sSettings.barrenDungeons)
            {
                if (!HintUtils.DungeonIsRequired("Forest Temple"))
                {
                    logicalItems.Remove(Item.Forest_Temple_Small_Key);
                    logicalItems.Remove(Item.Forest_Temple_Big_Key);
                }
                if (!HintUtils.DungeonIsRequired("Goron Mines"))
                {
                    logicalItems.Remove(Item.Goron_Mines_Small_Key);
                    logicalItems.Remove(Item.Goron_Mines_Key_Shard);
                }
                if (!HintUtils.DungeonIsRequired("Lakebed Temple"))
                {
                    logicalItems.Remove(Item.Lakebed_Temple_Small_Key);
                    logicalItems.Remove(Item.Lakebed_Temple_Big_Key);
                }
                if (!HintUtils.DungeonIsRequired("Arbiter's Grounds"))
                {
                    logicalItems.Remove(Item.Arbiters_Grounds_Small_Key);
                    logicalItems.Remove(Item.Arbiters_Grounds_Big_Key);
                }
                if (!HintUtils.DungeonIsRequired("Snowpeak Ruins"))
                {
                    logicalItems.Add(Item.Snowpeak_Ruins_Small_Key);
                    logicalItems.Add(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
                    logicalItems.Add(Item.Snowpeak_Ruins_Ordon_Pumpkin);
                    logicalItems.Add(Item.Snowpeak_Ruins_Bedroom_Key);
                }
                if (!HintUtils.DungeonIsRequired("Temple of Time"))
                {
                    logicalItems.Remove(Item.Temple_of_Time_Small_Key);
                    logicalItems.Remove(Item.Temple_of_Time_Big_Key);
                }
                if (!HintUtils.DungeonIsRequired("City in the Sky"))
                {
                    logicalItems.Remove(Item.City_in_The_Sky_Small_Key);
                    logicalItems.Remove(Item.City_in_The_Sky_Big_Key);
                }
                if (!HintUtils.DungeonIsRequired("Palace of Twilight"))
                {
                    logicalItems.Remove(Item.Palace_of_Twilight_Small_Key);
                    logicalItems.Remove(Item.Palace_of_Twilight_Big_Key);
                }
            }

            if (sSettings.skipArbitersEntrance)
                logicalItems.Remove(Item.Gerudo_Desert_Bulblin_Camp_Key);

            if (sSettings.skipCityEntrance && HintUtils.checkIsExcluded("Shad Dominion Rod"))
                logicalItems.Remove(Item.Progressive_Sky_Book);

            // For trade items, filter out if reward is excluded.
            foreach (KeyValuePair<Item, string> pair in HintUtils.tradeItemToRewardCheck)
            {
                Item tradeItem = pair.Key;
                string rewardCheckName = pair.Value;
                if (HintUtils.checkIsExcluded(rewardCheckName))
                    logicalItems.Remove(tradeItem);
            }

            // Filter out any items where you already start with enough copies to logically max it
            // out.
            Dictionary<Item, int> startingItemCounts = new();
            foreach (Item startingItem in sSettings.startingItems)
            {
                int count = 0;
                if (startingItemCounts.TryGetValue(startingItem, out int currCount))
                {
                    count = currCount;
                }
                startingItemCounts[startingItem] = count + 1;
            }
            foreach (Item item in new List<Item>(logicalItems))
            {
                // Skip these for now. Not relevant since cannot adjust starting hearts yet.
                if (item == Item.Heart_Container || item == Item.Piece_of_Heart)
                    continue;

                if (startingItemCounts.TryGetValue(item, out int startingCount))
                {
                    int countToMaxOut = 1;
                    if (multiToMaxItems.TryGetValue(item, out int count))
                        countToMaxOut = count;

                    if (startingCount >= countToMaxOut)
                        logicalItems.Remove(item);
                }
            }

            this.logicalItems2 = logicalItems;
        }

        private HashSet<string> prepareAllowBarrenChecks(
            Dictionary<Item, int> itemToInflexibleCount
        )
        {
            HashSet<string> allowBarrenCheckSet = new();

            Dictionary<Item, int> itemToProgCount = new();
            foreach (Item item in logicalItems2)
            {
                int countToMaxOut = 1;
                if (multiToMaxItems.TryGetValue(item, out int count))
                    countToMaxOut = count;
                itemToProgCount[item] = countToMaxOut;
            }

            // Calculate inflexible count. Note for no-logic, requiredChecks is empty since we need
            // logic for something to be "logically required".
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

            // TODO: handle items with different IDs which are functionally equivalent (bottles
            // only?). Only Hylian shield matters logically, so doesn't matter for the wooden/Ordon
            // shields.

            // Items already handled because they went over the inflexibleCount threshold.
            HashSet<Item> fullInflexibleItems = new();

            foreach (KeyValuePair<Item, int> pair in itemToProgCount)
            {
                Item item = pair.Key;
                int progCount = pair.Value;

                if (itemToInflexibleCount.ContainsKey(item) && itemToChecksList.ContainsKey(item))
                {
                    int inflexibleCount = itemToInflexibleCount[item];
                    if (inflexibleCount >= progCount)
                    {
                        fullInflexibleItems.Add(item);
                        // If inflexibly maxed out, mark any unrequired checks with this item as
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

            // For items which max out at a single copy, any copies of this item which cannot
            // logically be your first copy are not logically useful, so mark allowBarren. This most
            // often matters for bombs or bows, but you could also see this if there were 2 findable
            // Auru's Memos and one was in the desert for example.
            if (sSettings.logicRules != LogicRules.No_Logic)
            {
                foreach (KeyValuePair<Item, int> pair in itemToProgCount)
                {
                    Item item = pair.Key;
                    if (
                        pair.Value == 1
                        && !fullInflexibleItems.Contains(item)
                        && itemToChecksList.TryGetValue(item, out List<string> checksList)
                        && checksList.Count > 1
                    )
                    {
                        HashSet<string> blockedChecks = HintUtils.calcFindingItemBlocksItself(
                            startingRoom,
                            sSettings,
                            checksList
                        );
                        allowBarrenCheckSet.UnionWith(blockedChecks);
                    }
                }
            }

            // "allowBarren" should only be expanded for HiddenSkill like this when "barrenType" is
            // "important". Otherwise it is true that HiddenSkills have not maxed out at 1 copy, so
            // they should not be excluded.

            // This should happen even without doing Important checks? Well, the only time it is
            // relevant is if barren blockers are "important", and this requires that the importance
            // is calculated. Therefore the code can go in there.


            // Setting: - [] Only junk allows barren
            // - For players who wish to collect every heart, every shield, every bottle, etc.
            // - This setting causes all non-junk items to be considered barren-blockers by default.

            // Setting: - [] Hint importance
            // - WARNING: can cause a large increase in generation time depending on settings.
            // - Does nothing for no-logic or hint distributions which have this enabled by default
            //   (such as race distributions).
            // Calculates "sometimes required" checks, includes importance on hints where
            //   applicable, and prevents barren based on importance.

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

        private Dictionary<Zone, HashSet<Zone>> calcDungeonEntrances()
        {
            Dictionary<Zone, HashSet<Zone>> result = new();

            List<(string, string, Zone)> exitToDungeonList =
                new()
                {
                    ("North Faron Woods", "Forest Temple Entrance", Zone.Forest_Temple),
                    (
                        "Death Mountain Sumo Hall Goron Mines Tunnel",
                        "Goron Mines Entrance",
                        Zone.Goron_Mines
                    ),
                    (
                        "Lake Hylia Lakebed Temple Entrance",
                        "Lakebed Temple Entrance",
                        Zone.Lakebed_Temple
                    ),
                    (
                        "Outside Arbiters Grounds",
                        "Arbiters Grounds Entrance",
                        Zone.Arbiters_Grounds
                    ),
                    (
                        "Snowpeak Summit Lower Left Door",
                        "Snowpeak Ruins Left Door",
                        Zone.Snowpeak_Ruins
                    ),
                    (
                        "Snowpeak Summit Lower Right Door",
                        "Snowpeak Ruins Right Door",
                        Zone.Snowpeak_Ruins
                    ),
                    (
                        "Sacred Grove Past Behind Window",
                        "Temple of Time Entrance",
                        Zone.Temple_of_Time
                    ),
                    ("Lake Hylia", "City in The Sky Entrance", Zone.City_in_the_Sky),
                    (
                        "Mirror Chamber Portal",
                        "Palace of Twilight Entrance",
                        Zone.Palace_of_Twilight
                    ),
                    (
                        "Castle Town North Inside Barrier",
                        "Hyrule Castle Entrance",
                        Zone.Hyrule_Castle
                    ),
                };

            // Build quick lookups
            Dictionary<string, Zone> entranceToZone = new();
            foreach ((string, string, Zone) tuple in exitToDungeonList)
            {
                entranceToZone[tuple.Item2] = tuple.Item3;
            }

            foreach ((string, string, Zone) tuple in exitToDungeonList)
            {
                string srcRoom = tuple.Item1;
                string vanillaTargetRoom = tuple.Item2;
                Zone entranceZone = tuple.Item3;

                bool exitIsNotRandomized = false;
                Entrance exitToMatch = null;

                Room dungeonEntranceRoom = Randomizer.Rooms.RoomDict[srcRoom];
                foreach (Entrance exit in dungeonEntranceRoom.Exits)
                {
                    if (exit.OriginalConnectedArea == vanillaTargetRoom)
                    {
                        exitToMatch = exit.GetReplacedEntrance();
                        if (exitToMatch == null)
                        {
                            // If found the exit but it has no replacedEntrance,
                            // then this one was not randomized and the dungeon
                            // connection is vanilla.
                            exitIsNotRandomized = true;
                        }
                        break;
                    }
                }

                Zone dungeonUponEntering;

                if (exitIsNotRandomized)
                {
                    dungeonUponEntering = entranceZone;
                }
                else
                {
                    if (exitToMatch == null)
                    {
                        throw new Exception(
                            $"Failed to find vanillaTargetRoom '{vanillaTargetRoom}'."
                        );
                    }

                    string newTargetRoom = exitToMatch.OriginalConnectedArea;
                    if (!entranceToZone.TryGetValue(newTargetRoom, out Zone targetDungeon))
                        throw new Exception(
                            $"Failed to find dungeon zone for entrance '{newTargetRoom}'."
                        );

                    dungeonUponEntering = targetDungeon;
                }

                if (!result.TryGetValue(entranceZone, out HashSet<Zone> set))
                {
                    set = new();
                    result[entranceZone] = set;
                }
                set.Add(dungeonUponEntering);
            }

            return result;
        }

        private HashSet<string> calcUnreachableChecks()
        {
            HashSet<string> unreachableChecks = new();
            if (
                !BackendFunctions.ValidatePlaythrough(
                    startingRoom,
                    unreachableChecks: unreachableChecks
                )
            )
                throw new Exception(
                    $"Unexpected playthrough failure during hints calcUnreachableChecks."
                );

            return unreachableChecks;
        }

        private void calcAreaToCheckInfoInner<T>(
            Dictionary<AreaId, AreaCheckInfo> result,
            Dictionary<string, Zone> checkNameToZone,
            Dictionary<T, string[]> dict,
            Func<T, AreaId> func
        )
        {
            foreach (KeyValuePair<T, string[]> entry in dict)
            {
                AreaId areaId = func(entry.Key);
                AreaCheckInfo areaCheckInfo = new();
                result[areaId] = areaCheckInfo;

                Zone zone = Zone.Invalid;
                string keyAsStr = entry.Key as string;
                if (keyAsStr != null && areaId.type == AreaId.AreaType.Zone)
                {
                    zone = ZoneUtils.StringToIdThrows(keyAsStr);
                }

                // Filter out any unreachable checks. Vanilla/excluded is fine. But we can go ahead
                // and keep track of if it can be barren hinted or not.

                string[] baseCheckNames = entry.Value;
                foreach (string checkName in baseCheckNames)
                {
                    // Skip over hidden checks such as "Arbiters Grounds Stallord" and portals.
                    if (CheckIdClass.GetIsHideFromUiCheckName(checkName))
                        continue;

                    if (zone != Zone.Invalid)
                        checkNameToZone[checkName] = zone;

                    areaCheckInfo.fullCheckNames.Add(checkName);

                    if (!HintUtils.checkIsPlayerKnownStatus(checkName))
                        areaCheckInfo.nonVanillaExcludedCheckNames.Add(checkName);
                }
            }
        }

        private void calcAreaToCheckInfo()
        {
            // After more ER work is done, this function will calculate based on the room graph, but
            // this is not necessary for now. Ex: if grottos are shuffled, then a grotto could be
            // part of a different zone that normal.


            // Zones
            calcAreaToCheckInfoInner(
                areaToCheckInfo,
                checkNameToZone,
                ZoneUtils.zoneNameToChecks,
                (string zoneName) =>
                {
                    Zone zone = ZoneUtils.StringToIdThrows(zoneName);
                    AreaId areaId = AreaId.Zone(zone);
                    return areaId;
                }
            );

            // Verify all non-hidden checkNames can be mapped to a Zone. Since they can map to a
            // Zone, they can also map to a Province.
            foreach (KeyValuePair<string, Check> pair in Randomizer.Checks.CheckDict)
            {
                // Note: we skip over hidden checks such as "Arbiters Grounds Stallord" and portals.
                string checkName = pair.Value.checkName;
                if (
                    !CheckIdClass.GetIsHideFromUiCheckName(checkName)
                    && !checkNameToZone.ContainsKey(checkName)
                )
                    throw new Exception($"Did not find Zone for checkname '{checkName}'.");
            }

            // Hint categories
            calcAreaToCheckInfoInner(
                areaToCheckInfo,
                checkNameToZone,
                HintCategoryUtils.categoryToChecksMap,
                AreaId.Category
            );
        }

        public AreaCheckInfo GetAreaCheckInfoThrows(AreaId areaId)
        {
            if (!areaToCheckInfo.TryGetValue(areaId, out AreaCheckInfo areaCheckInfo))
                throw new Exception($"Failed to get checks for areaId '{areaId.stringId}'.");
            return areaCheckInfo;
        }

        public HashSet<string> GetChecksForZone(Zone zone)
        {
            // HashSet<string> checkNames = new();
            // Dictionary<string, string[]> zoneToChecks = getHintZoneToChecksMap();
            AreaCheckInfo areaCheckInfo = GetAreaCheckInfoThrows(AreaId.Zone(zone));
            return new(areaCheckInfo.fullCheckNames);

            // string zoneName = ZoneUtils.IdToString(zone);
            // string[] checks = zoneToChecks[zoneName];
            // foreach (string check in checks)
            // foreach (string check in areaCheckInfo.fullCheckNames)
            // {
            //     checkNames.Add(check);
            // }
            // return checkNames;
        }

        public string GetZoneNameForCheck(string checkName)
        {
            if (!checkNameToZone.TryGetValue(checkName, out Zone zone))
                throw new Exception($"Failed to find zone for checkName '{checkName}'.");
            return ZoneUtils.IdToString(zone);
        }

        public HashSet<string> GetChecksForProvince(Province province)
        {
            HashSet<string> checkNames = new();
            // Dictionary<string, string[]> zoneToChecks = getHintZoneToChecksMap();

            foreach (Zone zone in ProvinceUtils.ProvinceToZones(province))
            {
                // string zoneName = ZoneUtils.IdToString(zone);
                // string[] checks = zoneToChecks[zoneName];
                HashSet<string> checksForZone = GetChecksForZone(zone);
                foreach (string check in checksForZone)
                {
                    checkNames.Add(check);
                }
            }
            return checkNames;
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

        public bool checkCanBeLocationHinted(
            string checkName,
            bool canHintHintedBarrenChecks = false
        )
        {
            // We should ignore checks which are directed toward. We may want to
            // not hint toward any checks in a zone which is SpoL? This was the
            // previous behavior. Probably fine to list them in a SpoL zone.
            // This way you can know that specific check wasn't the SpoL one.
            // Can adjust later if doesn't make sense.
            return (
                    canHintHintedBarrenChecks || !hinted.alreadyCheckKnownBarren.Contains(checkName)
                )
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
                return vars.ResolveDefToChecks(this, name);
            }
            else if (HintSettingUtils.IsAreaDefinition(name))
            {
                // Resolve as an area.
                AreaId areaId = AreaId.ParseString(name);
                return areaId.ResolveToChecks(this);
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

            // Order of these if-statements is important.
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

            // TODO: update implementation. Good means preventsBarren. For "junk only", any check
            // where the contents are a baseMajorItem (or technically we should check against junk
            // specifically) is Good. For Major, any check where the contents are in `majorItems`
            // blocks barren. For advanced stuff, the check must either be "required" or
            // "conditionallyRequired". Note: "required" checks are always Good, but this is handled
            // above.

            //      NEXT TODO: !!!!!!!!!!!!!! go ahead and create the HintSettings.Barren.blockerType setting
            // so that we can use it in the code instead of leaving TODOs all over the place. This
            // is read from the distribution, then we potentially overwrite it from the main
            // sSettings if the user were to check a box saying "Non-Junk Items" prevents barren.
            // For balanced, it would default to Major, etc. For a race distribution, it might be
            // set to Important Items. Blocks Barren: "No Override", "Non-Junk", "Major Items"
            // "Important Items". Note: if they select "Important Items", then we will have to do
            // the condReq calcultions. If they are no-logic, then have to revert to "Major Items".

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

        private bool CheckIsGoodOrSkippable(
            string checkName,
            bool allowVanillaChecks = false,
            Item? ignoreItemForSkippable = null
        )
        {
            if (unreachableChecks.Contains(checkName))
                return false;
            if (!allowVanillaChecks && HintUtils.checkIsVanilla(checkName))
                return false;

            // Ignoring "isPlayerKnownStatus", returns true if the check would either be considered
            // a barren-blocker or "skippable" (logical + not "required"/"sometimesRequired"/"not
            // required").

            if (requiredChecks.Contains(checkName) || condReqChecks.Contains(checkName))
                return true;
            if (allowBarrenChecks.Contains(checkName))
                return false;

            Item contents = HintUtils.getCheckContents(checkName);

            if (hintSettings.barren.blockerType == Barren.BlockerType.NonJunk)
            {
                // Anything that could be considered as progress toward 100%-ing a seed is
                // considered as good in this case (including wooden shield, etc.).
                if (!HintConstants.junkItems.Contains(contents))
                    return true;
            }
            else if (notReqChecks.Contains(checkName))
                return false;

            if (ignoreItemForSkippable != null && ignoreItemForSkippable == contents)
                return false;

            // If logical, then status would be "skippable" at this point. Else returns false.
            return logicalItems2.Contains(contents);
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

        public AreaId GetZoneAreaId(string checkName)
        {
            string zoneName = GetZoneNameForCheck(checkName);
            return AreaId.ZoneStr(zoneName);
        }

        public AreaId GetProvinceAreaId(string checkName)
        {
            Province province = HintUtils.checkNameToHintProvince(checkName);
            return AreaId.Province(province);
        }

        public AreaId GetRecommendedAreaId(string checkName)
        {
            Item item = HintUtils.getCheckContents(checkName);
            Province province = HintUtils.checkNameToHintProvince(checkName);
            string zoneName = GetZoneNameForCheck(checkName);
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
            string zoneName = GetZoneNameForCheck(startCheckName);
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

            HashSet<string> checkNames = areaId.ResolveToChecks(this);
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
        private HashSet<uint> startingHintIds = new();

        public void OnPickedStartingHint(Hint hint)
        {
            startingHintIds.Add(hint.uniqueHintId);
        }

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

        private List<Hint> GetHints(string varName)
        {
            if (StringUtils.isEmpty(varName) || !varNameToHints.ContainsKey(varName))
                return new();

            List<Hint> hints = varNameToHints[varName];

            if (ListUtils.isEmpty(hints))
                return new();
            return hints;
        }

        public List<Hint> GetHintsForVarName(string varName, bool includeStartingHints = true)
        {
            List<Hint> baseHints = GetHints(varName);

            List<Hint> results = new(baseHints.Count);
            for (int i = 0; i < baseHints.Count; i++)
            {
                Hint hint = baseHints[i];
                // Potentially filter out starting hints
                if (includeStartingHints || !startingHintIds.Contains(hint.uniqueHintId))
                    results.Add(hint);
            }

            return results;
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

        public HashSet<string> ResolveDefToChecks(HintGenData genData, string varDef)
        {
            KeyValuePair<string, string> varParts = HintSettingUtils.ParseVarDefinition(varDef);
            string varName = varParts.Key;
            string property = varParts.Value;

            List<Hint> hints = GetHints(varName);
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
                        HashSet<string> partial = areaId.ResolveToChecks(genData);
                        checkNames.UnionWith(partial);
                    }
                    return checkNames;
                }
                case "areaId":
                {
                    List<AreaId> areaIds = HintsToAreaIds(hints, 1);
                    if (!ListUtils.isEmpty(areaIds))
                        return areaIds[0].ResolveToChecks(genData);
                    return new();
                }
                default:
                    throw new Exception($"Failed to reesolve property of '{varDef}'.");
            }
        }
    }

    public class BarrenHelper
    {
        // Results of calculating info about the zones / dependent checks:

        // (This would be done recursively, though the recursion can break out early once we know
        // the thing we are checking is blocked from being hinted barren period).

        // - How many checks are there that the player does not already know are barren for the area
        //   or checkList? (this cannot be cached since it depends on how much new info the hint
        //   would give us).

        // - Cached result: is the zone blocked from being hinted barren period.
    }

    public class AreaCheckInfo
    {
        public HashSet<string> fullCheckNames { get; } = new();
        public HashSet<string> nonVanillaExcludedCheckNames { get; } = new();
        public HashSet<AreaId> dependentAreaIds { get; } = new();
    }
}
