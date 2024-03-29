// namespace TPRandomizer.Hints
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//     using SSettings.Enums;
//     using TPRandomizer.Util;

//     public class HintGeneratorNew
//     {
//         private Random rnd;
//         private SharedSettings sSettings;
//         private PlaythroughSpheres playthroughSpheres;
//         private Room startingRoom;
//         private Dictionary<Goal, List<string>> goalToRequiredChecks;
//         private HashSet<string> requiredChecks;
//         private HashSet<Item> preventBarrenItems;
//         private Dictionary<Item, List<string>> itemToChecksList;

//         // Possible this could be written more literally as hinted as Always,
//         // but it is fine for now.
//         private HashSet<string> alreadyCheckContentsHinted = new();
//         private HashSet<string> alreadyCheckDirectedToward = new();
//         private HashSet<Item> alreadyItemHintedRequired = new();
//         private HashSet<string> hintsShouldIgnoreChecks = new();
//         private HashSet<string> alreadyCheckKnownBarren = new();
//         private HashSet<Item> bugsIndirectlyPointedTo = new();

//         public HintGeneratorNew(
//             Random rnd,
//             SharedSettings sSettings,
//             // List<List<KeyValuePair<int, Item>>> spheres,
//             PlaythroughSpheres playthroughSpheres,
//             Room startingRoom
//         )
//         {
//             this.rnd = rnd;
//             this.sSettings = sSettings;
//             this.playthroughSpheres = playthroughSpheres;
//             this.startingRoom = startingRoom;

//             goalToRequiredChecks = HintUtils.calculateGoalsRequiredChecks(
//                 startingRoom,
//                 playthroughSpheres.spheres
//             );

//             // We need to calculate `requiredChecks` separately from
//             // `goalToRequiredChecks` because the goal ones are calculated
//             // assuming you start with big keys so that the path hints are not
//             // all super big key-based.
//             requiredChecks = HintUtils.calculateRequiredChecks(
//                 startingRoom,
//                 playthroughSpheres.spheres
//             );

//             // requiredChecks = new();
//             // foreach (KeyValuePair<Goal, List<string>> pair in goalToRequiredChecks)
//             // {
//             //     if (pair.Value != null)
//             //     {
//             //         foreach (string checkName in pair.Value)
//             //         {
//             //             requiredChecks.Add(checkName);
//             //         }
//             //     }
//             // }

//             // requiredChecks = HintUtils.calculateRequiredChecks(
//             //     startingRoom,
//             //     playthroughSpheres.spheres
//             // );
//             preventBarrenItems = genPreventBarrenItemSet();
//             itemToChecksList = calcItemToChecksList();

//             // Calculate how many of each item in the item pool
//             // Can probably just iterate over all of the checks the same way we check their contents.
//         }

//         private bool getSlingshotHardRequired()
//         {
//             foreach (string checkName in requiredChecks)
//             {
//                 if (getCheckContents(checkName) == Item.Slingshot)
//                     return true;
//             }

//             return false;
//         }

//         private HashSet<Item> genPreventBarrenItemSet()
//         {
//             HashSet<Item> preventBarrenItemSet =
//                 new()
//                 {
//                     Item.Progressive_Sword,
//                     Item.Boomerang,
//                     Item.Lantern,
//                     Item.Progressive_Fishing_Rod,
//                     Item.Iron_Boots,
//                     Item.Progressive_Bow,
//                     Item.Filled_Bomb_Bag,
//                     Item.Progressive_Clawshot,
//                     Item.Aurus_Memo,
//                     Item.Spinner,
//                     Item.Ball_and_Chain,
//                     Item.Progressive_Dominion_Rod,
//                 };

//             // Don't use invalid SpoL items. For example, Big Keys should
//             // still prevent barren if Big Key sanity is on.

//             // Add items which show up in the spheres. This handles only adding
//             // items which are relevant based on the selected dungeons (such as
//             // big keys, sky chars, etc.)
//             foreach (List<KeyValuePair<int, Item>> spherePairs in playthroughSpheres.spheres)
//             {
//                 foreach (KeyValuePair<int, Item> pair in spherePairs)
//                 {
//                     Item item = pair.Value;
//                     // Single check items are handled separately later.
//                     if (!HintConstants.singleCheckItems.ContainsKey(item))
//                         preventBarrenItemSet.Add(item);
//                 }
//             }

//             // Slingshot only prevents barren if it is hard-required.
//             if (!getSlingshotHardRequired())
//                 preventBarrenItemSet.Remove(Item.Slingshot);

//             // Remove items from set depending on settings.
//             preventBarrenItemSet.Remove(Item.Progressive_Hidden_Skill);
//             preventBarrenItemSet.Remove(Item.Poe_Soul);

//             preventBarrenItemSet.Remove(Item.Progressive_Mirror_Shard);
//             preventBarrenItemSet.Remove(Item.Mirror_Piece_3);
//             preventBarrenItemSet.Remove(Item.Mirror_Piece_4);
//             preventBarrenItemSet.Remove(Item.Progressive_Fused_Shadow);
//             preventBarrenItemSet.Remove(Item.Fused_Shadow_2);
//             preventBarrenItemSet.Remove(Item.Fused_Shadow_3);
//             preventBarrenItemSet.Remove(Item.Poe_Scent);
//             preventBarrenItemSet.Remove(Item.Reekfish_Scent);

//             // Big Keys only prevent barren if Keysanity or Any_Dungeon
//             if (
//                 sSettings.bigKeySettings != BigKeySettings.Anywhere
//                 && sSettings.bigKeySettings != BigKeySettings.Any_Dungeon
//             )
//             {
//                 preventBarrenItemSet.Remove(Item.Forest_Temple_Big_Key);
//                 preventBarrenItemSet.Remove(Item.Goron_Mines_Big_Key);
//                 preventBarrenItemSet.Remove(Item.Goron_Mines_Key_Shard);
//                 preventBarrenItemSet.Remove(Item.Goron_Mines_Key_Shard_Second);
//                 preventBarrenItemSet.Remove(Item.Goron_Mines_Key_Shard_3);
//                 preventBarrenItemSet.Remove(Item.Lakebed_Temple_Big_Key);
//                 preventBarrenItemSet.Remove(Item.Arbiters_Grounds_Big_Key);
//                 preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Bedroom_Key);
//                 preventBarrenItemSet.Remove(Item.Temple_of_Time_Big_Key);
//                 preventBarrenItemSet.Remove(Item.City_in_The_Sky_Big_Key);
//                 preventBarrenItemSet.Remove(Item.Palace_of_Twilight_Big_Key);
//                 preventBarrenItemSet.Remove(Item.Hyrule_Castle_Big_Key);
//             }

//             if (
//                 sSettings.smallKeySettings != SmallKeySettings.Anywhere
//                 && sSettings.smallKeySettings != SmallKeySettings.Any_Dungeon
//             )
//             {
//                 preventBarrenItemSet.Remove(Item.Forest_Temple_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Goron_Mines_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Lakebed_Temple_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Arbiters_Grounds_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
//                 preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Ordon_Pumpkin);
//                 preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Temple_of_Time_Small_Key);
//                 preventBarrenItemSet.Remove(Item.City_in_The_Sky_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Palace_of_Twilight_Small_Key);
//                 preventBarrenItemSet.Remove(Item.Hyrule_Castle_Small_Key);
//             }

//             // Single-check items only prevent barren if the reward at the end
//             // of their reward chain prevents barren.
//             foreach (KeyValuePair<Item, string> kv in HintConstants.singleCheckItems)
//             {
//                 Item singleCheckItem = kv.Key;
//                 if (recursiveCheckSingleItemPreventBarren(singleCheckItem, preventBarrenItemSet))
//                 {
//                     preventBarrenItemSet.Add(singleCheckItem);
//                 }
//             }

//             return preventBarrenItemSet;
//         }

//         private bool recursiveCheckSingleItemPreventBarren(
//             Item item,
//             HashSet<Item> preventBarrenItemSet
//         )
//         {
//             if (!HintConstants.singleCheckItems.ContainsKey(item))
//                 throw new Exception(
//                     $"`recursiveCheckSingleItemPreventBarren` called with invalid item `{item}`."
//                 );

//             string rewardCheck = HintConstants.singleCheckItems[item];
//             if (HintUtils.checkIsExcluded(rewardCheck))
//                 return false;

//             Item rewardContents = getCheckContents(rewardCheck);

//             if (preventBarrenItemSet.Contains(rewardContents))
//                 return true;
//             else if (HintConstants.singleCheckItems.ContainsKey(rewardContents))
//                 return recursiveCheckSingleItemPreventBarren(rewardContents, preventBarrenItemSet);

//             return false;
//         }

//         private Dictionary<Item, List<string>> calcItemToChecksList()
//         {
//             Dictionary<Item, List<string>> itemToChecks = new();

//             foreach (KeyValuePair<string, Check> pair in Randomizer.Checks.CheckDict)
//             {
//                 Check check = pair.Value;
//                 Item contents = check.itemId;
//                 if (!itemToChecks.ContainsKey(contents))
//                 {
//                     itemToChecks[contents] = new();
//                 }
//                 List<string> checkNameList = itemToChecks[contents];
//                 checkNameList.Add(pair.Value.checkName);
//             }

//             return itemToChecks;
//         }

//         // private Check getCheckFromCheckName(string checkName)
//         // {
//         //     return Randomizer.Checks.CheckDict[checkName];
//         // }

//         private Item getCheckContents(string checkName)
//         {
//             return Randomizer.Checks.CheckDict[checkName].itemId;
//         }

//         private bool isCheckSphere0(string checkName)
//         {
//             return playthroughSpheres.sphere0Checks.Contains(checkName);
//         }

//         private bool checkPreventsBarren(string checkName)
//         {
//             return preventBarrenItems.Contains(getCheckContents(checkName));
//         }

//         public List<HintSpot2> Generate()
//         {
//             List<HintSpot2> prefilledHintSpots = new();

//             // Agitha must be done up front since it determines whether or not
//             // bugs should prevent barren.
//             HintSpot2 agithaHintSpot = getAgithaHintSpot();
//             if (agithaHintSpot != null)
//                 prefilledHintSpots.Add(agithaHintSpot);

//             HashSet<string> alwaysChecks =
//                 new()
//                 {
//                     "Goron Springwater Rush",
//                     "Iza Helping Hand",
//                     "Lake Hylia Shell Blade Grotto Chest",
//                     "Lanayru Ice Block Puzzle Cave Chest",
//                     "Plumm Fruit Balloon Minigame"
//                 };

//             List<HintAbs> alwaysHints = genAlwaysHints(alwaysChecks, 3);

//             // // Note: these are currently marked as directed toward. This is
//             // // probably good since it is not like the anti-sword casino hint
//             // // where you don't get to see it until the end. The consequence is
//             // // that sometimes hints, etc. will not point toward big keys. This
//             // // is probably good, except a hint pointing toward HC big key might
//             // // be good? Can fine-tune it within that method. Need to playtest.
//             // prefilledHintSpots.AddRange(getDungeonAntiCasinoHintSpots());
//             // Dictionary<string, List<string>> dungeonZoneToBigKeyChecks =

//             // Create hints for this.

//             // If is hinted, then we should not give any hints relating to
//             // Snowpeak or snowpeak province. (Other than the empty grotto hints
//             // which can always just refer to 5 provinces. No reason to make
//             // this more complicated.)
//             // bool snowpeakHintIsNothingBeyond = false;
//             bool snowpeakHintIsNothingBeyond = tryHintSnowpeakBeyondDead();

//             // HintAbs baseHintForSnowpeakSign = null;
//             // if (!HintUtils.DungeonIsRequired("Snowpeak Ruins"))
//             // {
//             //     if (!sSettings.skipSnowpeakEntrance)
//             //     {
//             //         // Add fishing rod hint if possible
//             //         List<HintAbs> hintList = genItemHintList(
//             //             new() { Item.Progressive_Fishing_Rod, }
//             //         );
//             //         if (hintList.Count > 0)
//             //         {
//             //             baseHintForSnowpeakSign = hintList[0];
//             //             tryMarkAlreadyKnownFromHint(baseHintForSnowpeakSign);
//             //         }
//             //     }
//             // }
//             // else
//             // {
//             // snowpeakHintIsNothingBeyond = tryHintSnowpeakBeyondDead();
//             // }

//             // List<HintSpot2> emptyGrottoHintSpots = getEmptyGrottoHintSpots();
//             // if (emptyGrottoHintSpots != null)
//             //     prefilledHintSpots.AddRange(emptyGrottoHintSpots);
//             // TODO: contents just here for seeing in Locals for now
//             // Item emptyGrottoContents = getCheckContents(emptyGrottoHintCheckName);

//             List<PathHint> pathHints = genPathHints(3);
//             // List<PathHint> pathHints = genPathHints(10);

//             // List<SpiritOfLightHint> spolHints = genSpolHints(3);

//             // Dictionary<string, Item> spolContents = new();
//             // foreach (SpiritOfLightHint spolHint in spolHints)
//             // {
//             //     string checkName = spolHint.checkName;
//             //     spolContents[checkName] = getCheckContents(checkName);
//             // }

//             Dictionary<string, BarrenHint> zoneToBarrenHintMap = genBarrenZoneHints(4);
//             // Create this since we mutate `zoneToBarrenHintMap`
//             HashSet<string> zonesWhichAreHintedBarren = new();
//             foreach (KeyValuePair<string, BarrenHint> pair in zoneToBarrenHintMap)
//             {
//                 zonesWhichAreHintedBarren.Add(pair.Key);
//             }

//             Dictionary<string, HintAbs> dungeonZoneToBeyondDeadHint = genDungeonToBeyondDeadMap();

//             // Dictionary<HintCategory, HintSpotLocation> categoryToNothingBeyondSpot =
//             //     new()
//             //     {
//             //         {
//             //             HintCategory.Goron_Mines_2nd_Part,
//             //             HintSpotLocation.Goron_Mines_Nothing_Beyond_Sign
//             //         },
//             //         {
//             //             HintCategory.Temple_of_Time_End_Part,
//             //             HintSpotLocation.Temple_of_Time_Nothing_Beyond_Sign
//             //         },
//             //         {
//             //             HintCategory.City_in_the_Sky_East_Wing,
//             //             HintSpotLocation.City_in_the_Sky_Nothing_Beyond_Sign
//             //         },
//             //         {
//             //             HintCategory.Lake_Lantern_Cave_2nd_Part,
//             //             HintSpotLocation.Lake_Lantern_Cave_Midway_Sign
//             //         },
//             //         {
//             //             HintCategory.Arbiters_Grounds_2nd_Half,
//             //             HintSpotLocation.Arbiters_Grounds_2nd_Half_Sign
//             //         },
//             //         {
//             //             HintCategory.Lakebed_Temple_2nd_Wing,
//             //             HintSpotLocation.Lakebed_Temple_2nd_Wing_Sign
//             //         }
//             //     };

//             // List<CategoryObject> nothingBeyondList = genCategoryHints(
//             //     null,
//             //     new()
//             //     {
//             //         // HintCategoryEnum.Goron_Mines_2nd_Part,
//             //         // HintCategoryEnum.Temple_of_Time_End_Part,
//             //         // HintCategoryEnum.City_in_the_Sky_East_Wing,
//             //         HintCategory.Lake_Lantern_Cave_2nd_Part,
//             //         // HintCategoryEnum.Arbiters_Grounds_2nd_Half,
//             //         // HintCategoryEnum.Lakebed_Temple_2nd_Wing
//             //     }
//             // );
//             // foreach (CategoryObject obj in nothingBeyondList)
//             // {
//             //     if (!obj.hasUseful && categoryToNothingBeyondSpot.ContainsKey(obj.category))
//             //     {
//             //         HintSpot2 spot = new HintSpot2(categoryToNothingBeyondSpot[obj.category]);
//             //         spot.hints.Add(new NothingBeyondHint());
//             //         prefilledHintSpots.Add(spot);

//             //         // Mark checks in this category as knownBarren.
//             //         string[] checks = HintCategoryUtils.categoryToChecksMap[obj.category];
//             //         foreach (string checkName in checks)
//             //         {
//             //             alreadyCheckKnownBarren.Add(checkName);
//             //         }
//             //     }
//             // }

//             // For each one which does not haveUseful, add a sign and mark the
//             // checks in that zone as known barren.

//             // Should do nothing beyond this point hints for dungeons before
//             // doing sometimes. Okay to do after the interesting hints?

//             // Calculate "interesting" hints.
//             List<
//                 KeyValuePair<HintAbs, int>
//             > priorityHintToQuantity = new();
//             // List<KeyValuePair<HintAbs, int>> secondaryHintToQuantity = new();

//             // Priority ones go into their own list (not shuffled).
//             // Other ones go into their own list which is shuffled.

//             // For picking hint at start, pick a random index which is in either
//             // list if you were to put them back to back.

//             // Remove the item from that list.

//             // Start adding from the priority list until no valid ones left to pick.
//             // Start adding form the 2nd list until no valid ones left to pick.

//             // If spots left, fill with a sometimes hint.

//             // Sometimes hints should first come from a priority list (interesting boss heart containers).
//             // Once that list is exhausted, we grab from the remaining list.
//             // If it runs out, then we put in null hints which say they were meant to be X type.

//             // NumItemInAreaHint numSwordsInDungeonsHint = genNumItemInDungeonsHint(
//             //     Item.Progressive_Sword
//             // );
//             // // priorityHintToQuantity.Add(new(numSwordsInDungeonsHint, 1));
//             // priorityHintToQuantity.Add(new(numSwordsInDungeonsHint, 2));

//             NumItemInAreaHint swordProvinceHint = genProvinceWithMostItemHint(
//                 Item.Progressive_Sword
//             );
//             if (swordProvinceHint != null)
//                 priorityHintToQuantity.Add(new(swordProvinceHint, 2));

//             // Start picking ones randomly
//             HintedThings priorityHintedThings = new HintedThings();
//             List<HintToGen> mainHintsToGen =
//                 new()
//                 {
//                     HintToGen.NumClawsInDungeons,
//                     HintToGen.ItemHint,
//                     HintToGen.BarrenBugGender,
//                     HintToGen.ItemToItemPath,
//                     HintToGen.BarrenCategory,
//                     // HintToGen.MistGoodOrNot,

//                     // // HintToGen.BarrenBugGender,
//                     // HintToGen.ItemHint,
//                     // HintToGen.ItemToItemPath,
//                     // // HintToGen.MistGoodOrNot,
//                 };
//             HintUtils.ShuffleListInPlace(rnd, mainHintsToGen);

//             List<KeyValuePair<HintAbs, int>> morePriorityHints = genMorePriorityHints(
//                 priorityHintedThings,
//                 mainHintsToGen
//             );
//             priorityHintToQuantity = priorityHintToQuantity.Concat(morePriorityHints).ToList();

//             // NumItemInAreaHint numClawsInDungeonsHint = genNumClawsInDungeonsHint();
//             // if (numClawsInDungeonsHint != null)
//             //     priorityHintToQuantity.Add(new(numClawsInDungeonsHint, 2));

//             // Want to iterate through all of the zones and get a map of ones
//             // which can be hinted to whether they are good or barren.
//             // Dictionary<string, bool> categoryGoodMap = genCategoryHints();
//             // List<CategoryObject> priorityCategoryObjs = genCategoryHints(
//             //     null,
//             //     new()
//             //     {
//             //         // HintCategoryEnum.Post_dungeon,
//             //         // HintCategoryEnum.Mist,
//             //         HintCategoryEnum.Lake_Lantern_Cave_Lantern_Checks
//             //     }
//             // );

//             // foreach (CategoryObject obj in priorityCategoryObjs)
//             // {
//             //     int numHints = obj.category == HintCategoryEnum.Post_dungeon ? 2 : 1;
//             //     HintAbs hint = obj.toHint(getCheckContents);
//             //     priorityHintToQuantity.Add(new(hint, numHints));

//             //     tryMarkAlreadyKnownFromHint(hint);
//             // }

//             // // create province hint for boomerang if possible
//             // List<HintAbs> boomerangHintList = genItemHintList(new() { Item.Boomerang, });
//             // if (boomerangHintList.Count > 0)
//             // {
//             //     HintAbs boomerangHint = boomerangHintList[0];
//             //     priorityHintToQuantity.Add(new(boomerangHint, 2));
//             //     tryMarkAlreadyKnownFromHint(boomerangHint);
//             // }

//             // // Pick priorityHintForEmptyGrotto
//             // int emptyGrottoRandomIndex = rnd.Next(priorityHintToQuantity.Count);
//             // HintAbs emptyGrottoHint = priorityHintToQuantity[emptyGrottoRandomIndex].Key;
//             // priorityHintToQuantity.RemoveAt(emptyGrottoRandomIndex);
//             // tryMarkAlreadyKnownFromHint(emptyGrottoHint);

//             // // String emptyGrottoHintCheck = getEmptyGrottoHintCheck();
//             // List<HintSpot2> emptyGrottoHintSpots = getEmptyGrottoHintSpots(
//             //     emptyGrottoHint,
//             //     zonesWhichAreHintedBarren,
//             //     snowpeakHintIsNothingBeyond
//             // );
//             // if (emptyGrottoHintSpots != null)
//             //     prefilledHintSpots.AddRange(emptyGrottoHintSpots);

//             // Secondary hints:

//             List<CategoryObject> categoryGoodList = genCategoryHints(
//                 priorityHintedThings,
//                 new()
//                 {
//                     HintCategory.Lake_Lantern_Cave_Lantern_Checks,
//                     HintCategory.Grotto,
//                     HintCategory.Owl_Statue,
//                     HintCategory.Underwater,
//                     HintCategory.Upper_Desert,
//                     HintCategory.Lower_Desert
//                 }
//             );

//             // TODO: since this list is already weighted and we don't want to go
//             // crazy on these ones, should maybe have a different list of lists
//             // which we pick randomly from. Each item in the list would be a
//             // different type of hint, like: [[a,b,c], [d,e,f], [g,h,i]]. For
//             // example, maybe d,e,f is the secondary category hints, and the
//             // a,b,c one is fishing rod or dominion rod hints. And the third one
//             // could be Lantern in this province hint if applicable.

//             // You could probably merge these into a single list up front to
//             // make picking from it easier. You just start at the front and
//             // iterate down the list until you find one that fits within the
//             // space you still have left (the sizes being 1 and 2, so if you had
//             // space for 3 left, then either would work. But if you only had
//             // space for a one-sign hint, then you would keep going until you
//             // found one which was a one-sign hint.)

//             // Maybe want to make sure we don't pick from the same list twice in
//             // a row when merging the list? Probably not needed.

//             // When picking the "interesting" hint to use in the start hints, we
//             // can make it a 50/50 (or as close as we can) to having it be
//             // between the primary list and secondary list.
//             List<KeyValuePair<HintAbs, int>> secondaryCategoryHintList = new();
//             foreach (CategoryObject obj in categoryGoodList)
//             {
//                 HintAbs hint = obj.toHint(getCheckContents);
//                 int timesHinted = hint.type == HintNewType.SomethingGood ? 1 : 2;
//                 secondaryCategoryHintList.Add(new(hint, timesHinted));
//             }

//             List<KeyValuePair<HintAbs, int>> itemHintList = new();

//             List<NamedItemHint> itemHints = genItemHintList(
//                 new()
//                 {
//                     // Item.Lantern,
//                     // Item.Iron_Boots,
//                     // Item.Ball_and_Chain,
//                     Item.Progressive_Bow,
//                     Item.Filled_Bomb_Bag,
//                     Item.Progressive_Dominion_Rod,
//                     Item.Progressive_Fishing_Rod,
//                 },
//                 priorityHintedThings
//             );
//             foreach (HintAbs itemHint in itemHints)
//             {
//                 itemHintList.Add(new(itemHint, 2));
//             }

//             // Item hints always go in the secondary list as 2 each.
//             List<KeyValuePair<HintAbs, int>> secondaryHintToQuantity = HintUtils.MergeListsRandomly(
//                 rnd,
//                 secondaryCategoryHintList,
//                 itemHintList
//             );
//             secondaryHintToQuantity = new();

//             HintAbs initialInterestingHint = null;

//             int val = rnd.Next(priorityHintToQuantity.Count);
//             // Pick randomly from the first list
//             int randomIndex = rnd.Next(priorityHintToQuantity.Count);
//             initialInterestingHint = priorityHintToQuantity[randomIndex].Key;
//             priorityHintToQuantity.RemoveAt(randomIndex);
//             tryMarkAlreadyKnownFromHint(initialInterestingHint);

//             // int val = rnd.Next(total);
//             // if (val < priorityHintToQuantity.Count)
//             // {
//             //     // Pick randomly from the first list
//             //     int randomIndex = rnd.Next(priorityHintToQuantity.Count);
//             //     initialInterestingHint = priorityHintToQuantity[randomIndex].Key;
//             //     priorityHintToQuantity.RemoveAt(randomIndex);
//             //     tryMarkAlreadyKnownFromHint(initialInterestingHint);
//             // }
//             // else
//             // {
//             //     // pick the first item from the secondary list
//             //     initialInterestingHint = secondaryHintToQuantity[0].Key;
//             //     secondaryHintToQuantity.RemoveAt(0);
//             // }

//             // Set starting hints
//             HintSpot2 startingHintSpot = new HintSpot2(HintSpotLocation.Midna_Talk_To);

//             // startingHintSpot.hints.Add(spolHints[0]);
//             // spolHints.RemoveAt(0);
//             startingHintSpot.hints.Add(pathHints[0]);
//             pathHints.RemoveAt(0);

//             KeyValuePair<string, BarrenHint> firstBarrenHintPair = RemoveFirstItemFromDictionary(
//                 zoneToBarrenHintMap
//             );
//             startingHintSpot.hints.Add(firstBarrenHintPair.Value);

//             startingHintSpot.hints.Add(initialInterestingHint);
//             prefilledHintSpots.Add(startingHintSpot);

//             List<HintSpot2> overworldHintSpots = new();

//             HashSet<string> zonesGivenBarrenJunkSign = new();

//             Dictionary<string, string[]> hintZoneToChecksMap = HintUtils.getHintZoneToChecksMap(
//                 sSettings
//             );

//             foreach (
//                 KeyValuePair<
//                     string,
//                     HintSpotLocation
//                 > pair in HintConstants.hintZoneToHintSpotLocation
//             )
//             {
//                 string hintZone = pair.Key;

//                 string[] checksOfZone = hintZoneToChecksMap[hintZone];
//                 bool areaAllKnown = HintUtils.checksAllPlayerKnownStatus(checksOfZone);
//                 // If area is completely know to player (such as excluded Hidden
//                 // Village), do not put a sign in that area.
//                 if (areaAllKnown)
//                     continue;

//                 // If hintZone is hinted Barren, fill with a single Junk hint.
//                 if (zonesWhichAreHintedBarren.Contains(hintZone))
//                 {
//                     HintSpot2 spot = new HintSpot2(pair.Value);
//                     spot.hints.Add(new JunkHint(rnd));
//                     prefilledHintSpots.Add(spot);
//                     // Mark that we need one less Barren hint for this.
//                     zonesGivenBarrenJunkSign.Add(hintZone);
//                 }
//                 // else if (snowpeakHintIsNothingBeyond && pair.Key == "Snowpeak")
//                 // {
//                 //     HintSpot2 spot = new HintSpot2(pair.Value);
//                 //     spot.hints.Add(new NothingBeyondHint());
//                 //     prefilledHintSpots.Add(spot);
//                 // }
//                 else
//                 {
//                     HintSpot2 spot = new HintSpot2(pair.Value);

//                     // if (snowpeakHintIsNothingBeyond && pair.Key == "Snowpeak")
//                     // {
//                     //     spot.hints.Add(new NothingBeyondHint());
//                     // }

//                     overworldHintSpots.Add(spot);
//                 }
//             }

//             HintUtils.ShuffleListInPlace(rnd, overworldHintSpots);

//             // Make list of Always, and remaining SpoL and Barren.
//             List<KeyValuePair<HintAbs, int>> baseHintsToCount = new();
//             // foreach (HintAbs alwaysHint in alwaysHints)
//             // {
//             //     baseHintsToCount.Add(new(alwaysHint, 2));
//             // }

//             List<List<HintAbs>> pairedAlwaysHints = createPairedAlwaysHints(alwaysHints);
//             foreach (List<HintAbs> hints in pairedAlwaysHints)
//             {
//                 if (overworldHintSpots.Count < 1)
//                     break;

//                 // Remove the first spot from the list that does not already
//                 // have hints (for example, Snowpeak sign with a fishing rod
//                 // hint) and add to prefilled.
//                 for (int i = 0; i < overworldHintSpots.Count; i++)
//                 {
//                     HintSpot2 spot = overworldHintSpots[i];
//                     // Skip over if spot already has hints.
//                     if (spot.hints.Count > 0)
//                         continue;

//                     overworldHintSpots.RemoveAt(i);

//                     spot.hints.AddRange(hints);
//                     prefilledHintSpots.Add(spot);
//                     break;
//                 }
//             }

//             // foreach (SpiritOfLightHint hint in spolHints)
//             foreach (PathHint hint in pathHints)
//             {
//                 baseHintsToCount.Add(new(hint, 2));
//             }
//             foreach (KeyValuePair<string, BarrenHint> pair in zoneToBarrenHintMap)
//             {
//                 // Reduce to 1 if the zone was already given a junk hint.
//                 int count = zonesGivenBarrenJunkSign.Contains(pair.Key) ? 1 : 2;
//                 baseHintsToCount.Add(new(pair.Value, count));
//             }

//             List<KeyValuePair<HintAbs, int>> combinedMandatoryHintToCount = new List<
//                 KeyValuePair<HintAbs, int>
//             >()
//                 .Concat(baseHintsToCount)
//                 .Concat(priorityHintToQuantity)
//                 .ToList();

//             List<CheckContentsHint> sometimesHintList = null;

//             // do first iteration through the overworld hint spots
//             int overworldHintSpotIndex = -1;
//             foreach (HintSpot2 spot in overworldHintSpots)
//             {
//                 overworldHintSpotIndex += 1;

//                 // if (spot.hints.Count > 0)
//                 // {
//                 //     // Skip over any spots which already have a first hint
//                 //     // placed in them. For example, the Snowpeak sign with a
//                 //     // Fishing Rod hint.
//                 //     continue;
//                 // }

//                 bool didFill = false;

//                 if (combinedMandatoryHintToCount.Count > 0)
//                 {
//                     KeyValuePair<HintAbs, int> pair = combinedMandatoryHintToCount[0];
//                     spot.hints.Add(pair.Key);
//                     tryMarkAlreadyKnownFromHint(pair.Key);

//                     if (pair.Value - 1 > 0)
//                         combinedMandatoryHintToCount[0] = new(pair.Key, pair.Value - 1);
//                     else
//                         combinedMandatoryHintToCount.RemoveAt(0);

//                     didFill = true;
//                 }
//                 else if (secondaryCategoryHintList.Count > 0)
//                 {
//                     int numSpotsToFill = overworldHintSpots.Count - overworldHintSpotIndex;

//                     foreach (KeyValuePair<HintAbs, int> pair in secondaryHintToQuantity)
//                     {
//                         if (pair.Value <= numSpotsToFill)
//                         {
//                             spot.hints.Add(pair.Key);
//                             tryMarkAlreadyKnownFromHint(pair.Key);
//                             if (pair.Value - 1 > 0)
//                                 secondaryHintToQuantity[0] = new(pair.Key, pair.Value - 1);
//                             else
//                                 secondaryHintToQuantity.RemoveAt(0);

//                             didFill = true;
//                             break;
//                         }
//                     }
//                 }

//                 if (didFill)
//                     continue;

//                 if (sometimesHintList == null)
//                 {
//                     // Do here so the sometimes list is generated after all
//                     // alreadyKnown stuff gets marked as certain hints are
//                     // selected.
//                     sometimesHintList = genSometimesCheckList(swordProvinceHint);
//                 }

//                 // Else pick a random sometimes hint.
//                 if (sometimesHintList.Count > 0)
//                 {
//                     spot.hints.Add(sometimesHintList[0]);
//                     sometimesHintList.RemoveAt(0);
//                 }
//                 else
//                 {
//                     spot.hints.Add(new NullHint(HintNewType.CheckContents));
//                 }
//             }

//             // For sometimes, we can do a "getNext" type thing.

//             // Then from here, we keep track of how many times we can still use
//             // the hint (sometimes 1, sometimes start at 2).

//             HintUtils.ShuffleListInPlace(rnd, overworldHintSpots);
//             // Do an iteration, filling with sometimes hints


//             if (sometimesHintList == null)
//             {
//                 // Do here so the sometimes list is generated after all
//                 // alreadyKnown stuff gets marked as certain hints are selected.
//                 sometimesHintList = genSometimesCheckList(swordProvinceHint);
//             }

//             // Add a 2nd
//             int sometimesHintCopiesRemaining = 0;
//             HintAbs currentSometimesHint = null;
//             foreach (HintSpot2 spot in overworldHintSpots)
//             {
//                 if (sometimesHintCopiesRemaining < 1)
//                 {
//                     if (sometimesHintList.Count > 0)
//                     {
//                         currentSometimesHint = sometimesHintList[0];
//                         sometimesHintList.RemoveAt(0);
//                     }
//                     else
//                     {
//                         currentSometimesHint = new NullHint(HintNewType.CheckContents);
//                     }
//                     sometimesHintCopiesRemaining = 2;
//                 }

//                 spot.hints.Add(currentSometimesHint);
//                 sometimesHintCopiesRemaining -= 1;
//             }

//             // Note: these are currently marked as directed toward. This is
//             // probably good since it is not like the anti-sword casino hint
//             // where you don't get to see it until the end. The consequence is
//             // that sometimes hints, etc. will not point toward big keys. This
//             // is probably good, except a hint pointing toward HC big key might
//             // be good? Can fine-tune it within that method. Need to playtest.
//             prefilledHintSpots.AddRange(
//                 getDungeonAntiCasinoHintSpots(
//                     dungeonZoneToBeyondDeadHint,
//                     zonesWhichAreHintedBarren
//                 )
//             );

//             // ^ Can create hints for dungeon known barren spots up front, then
//             // pass those hints to the antiCasinoHintSpot creation.


//             Item hcEastWingBalcony = getCheckContents("Hyrule Castle East Wing Balcony Chest");
//             Item hcEastWingBoomerang = getCheckContents(
//                 "Hyrule Castle East Wing Boomerang Puzzle Chest"
//             );
//             Item hcGySwitchChest1 = getCheckContents(
//                 "Hyrule Castle Graveyard Grave Switch Room Back Left Chest"
//             );
//             Item hcGySwitchChest2 = getCheckContents(
//                 "Hyrule Castle Graveyard Grave Switch Room Front Left Chest"
//             );
//             Item hcGySwitchChest3 = getCheckContents(
//                 "Hyrule Castle Graveyard Grave Switch Room Right Chest"
//             );
//             Item hcOwlStatueChest = getCheckContents("Hyrule Castle Graveyard Owl Statue Chest");

//             // TODO: should the barren bug gender one show up even when the
//             // things are not necessarily hard-required? Right now you seem to
//             // never see this hint, even though it seems valid even in the case
//             // that Agitha has a single item which is a sword. Even if not
//             // hard-required, a hint which cuts out 50% of the bugs would be
//             // nice. You can also know the hint exists since there is only 1
//             // item hinted (guaranteed to be a single gender).

//             // TODO: bug: the hint zone of the barren hint which is selected for
//             // the starting hints should have a junk sign. (No sign would be
//             // fine, but it is easier to tell people "hinted barren areas have a
//             // junk sign" and have it be accurate without asterisks). Right now
//             // it has a sign which acts as if the area is not hinted barren
//             // which is a problem.

//             return new List<HintSpot2>()
//                 .Concat(prefilledHintSpots)
//                 .Concat(overworldHintSpots)
//                 .ToList();
//         }

//         private List<HintAbs> genAlwaysHints(HashSet<string> alwaysChecksPool, int numToHint)
//         {
//             if (numToHint == 0)
//                 return new();
//             else if (numToHint < 0)
//                 throw new Exception($"Tried to generate {numToHint} always hints.");

//             if (alwaysChecksPool == null)
//                 alwaysChecksPool = new();

//             if (numToHint > alwaysChecksPool.Count)
//             {
//                 throw new Exception(
//                     $"Tried to generate {numToHint} always hints, but pool only had {alwaysChecksPool.Count} checks."
//                 );
//             }

//             List<string> requiredAlwaysChecks = new();
//             List<string> interestingAlwaysChecks = new();
//             List<string> boringAlwaysChecks = new();

//             foreach (string checkName in alwaysChecksPool)
//             {
//                 // No matter what, the player will always know which Always
//                 // checks they need to do, so they should all be marked as
//                 // hinted.
//                 alreadyCheckContentsHinted.Add(checkName);

//                 if (requiredChecks.Contains(checkName))
//                     requiredAlwaysChecks.Add(checkName);
//                 else if (preventBarrenItems.Contains(getCheckContents(checkName)))
//                     interestingAlwaysChecks.Add(checkName);
//                 else
//                     boringAlwaysChecks.Add(checkName);
//             }

//             // Determine if more than the max Always hints are hard-required. If
//             // they are, return a hint listing all of the hard-required checks.
//             if (requiredAlwaysChecks.Count > numToHint)
//             {
//                 return new() { new ChecksListHint(requiredAlwaysChecks) };
//             }

//             HintUtils.ShuffleListInPlace(rnd, requiredAlwaysChecks);
//             HintUtils.ShuffleListInPlace(rnd, interestingAlwaysChecks);
//             HintUtils.ShuffleListInPlace(rnd, boringAlwaysChecks);

//             List<string> checks = new List<string>()
//                 .Concat(requiredAlwaysChecks)
//                 .Concat(interestingAlwaysChecks)
//                 .Concat(boringAlwaysChecks)
//                 .ToList()
//                 .GetRange(0, numToHint);

//             List<HintAbs> result = new();
//             for (int i = 0; i < numToHint; i++)
//             {
//                 string checkName = checks[i];
//                 CheckContentsHint hint = CheckContentsHint.Create(null, checkName);
//                 result.Add(hint);
//             }

//             // Shuffle so that when we group the Always hints in pairs, etc., it
//             // is random how they show up.
//             HintUtils.ShuffleListInPlace(rnd, result);

//             return result;
//         }

//         private List<List<HintAbs>> createPairedAlwaysHints(List<HintAbs> hints)
//         {
//             List<List<HintAbs>> result = new();
//             if (hints == null || hints.Count < 1)
//                 return result;

//             if (hints.Count == 1)
//             {
//                 result.Add(hints);
//                 return result;
//             }

//             int numHints = hints.Count;
//             for (int i = 0; i < numHints; i++)
//             {
//                 List<HintAbs> groupedHints = new();
//                 groupedHints.Add(hints[i]);
//                 groupedHints.Add(hints[(i + 1) % numHints]);
//                 result.Add(groupedHints);
//             }

//             return result;
//         }

//         // Returns true if should have the Snowpeak hint be a NothingBeyond hint.
//         private bool tryHintSnowpeakBeyondDead()
//         {
//             HashSet<string> snowpeakBeyondChecks =
//                 new() { "Snowpeak Cave Ice Lantern Chest", "Snowpeak Freezard Grotto Chest", };

//             if (
//                 HintUtils.DungeonIsRequired("Snowpeak Ruins")
//                 || sSettings.shufflePoes
//                 || HintUtils.checksAllPlayerKnownStatus(snowpeakBeyondChecks)
//             )
//             {
//                 return false;
//             }

//             foreach (string checkName in snowpeakBeyondChecks)
//             {
//                 if (checkPreventsBarren(checkName))
//                 {
//                     return false;
//                 }
//             }

//             // Checks can be hinted barren, so we will add the beyond hint.
//             foreach (string checkName in snowpeakBeyondChecks)
//             {
//                 alreadyCheckKnownBarren.Add(checkName);
//             }

//             return true;
//         }

//         private bool checkCanBeHintedSpol(string checkName, HintedThings hintedThings = null)
//         {
//             Item contents = getCheckContents(checkName);
//             if (
//                 !requiredChecks.Contains(checkName)
//                 || HintConstants.invalidSpolItems.Contains(contents)
//                 || alreadyCheckContentsHinted.Contains(checkName)
//                 || alreadyCheckDirectedToward.Contains(checkName)
//                 || hintsShouldIgnoreChecks.Contains(checkName)
//             )
//                 return false;

//             if (
//                 hintedThings != null
//                 && (
//                     hintedThings.alreadyCheckContentsHinted.Contains(checkName)
//                     || hintedThings.alreadyCheckDirectedToward.Contains(checkName)
//                     || hintedThings.hintsShouldIgnoreChecks.Contains(checkName)
//                 )
//             )
//                 return false;

//             return true;
//         }

//         private List<HintSpot2> getEmptyGrottoHintSpots(
//             // String checkToHint,
//             HintAbs emptyGrottoHint,
//             HashSet<string> hintedBarrenZones,
//             bool snowpeakHintIsNothingBeyond
//         )
//         {
//             if (emptyGrottoHint == null)
//                 return null;

//             // Item item = getCheckContents(checkToHint);
//             // String checkZone = HintUtils.checkNameToHintZone(sSettings, checkToHint);
//             // Province checkProvince = HintUtils.checkNameToHintProvince(sSettings, checkToHint);

//             // For each grotto, check that it is not considered in a barren area
//             // (barren zone or special snowpeak logic).

//             // Any grotto which is invalid gets a junk sign.

//             // Select one grotto randomly from the valid ones.
//             // The other valid grottos point to the province of this grotto.
//             // This grotto gets the hint (Item is in province).

//             Dictionary<HintSpotLocation, string> locationToZone =
//                 new()
//                 {
//                     { HintSpotLocation.Faron_Field_Water_Grotto_Sign, "Faron Field" },
//                     { HintSpotLocation.Kakariko_Gorge_Grotto_Sign, "Kakariko Gorge" },
//                     { HintSpotLocation.Lanayru_Field_Chus_Grotto_Sign, "Lanayru Field" },
//                     { HintSpotLocation.Desert_Chus_Grotto_Sign, "Gerudo Desert" },
//                     { HintSpotLocation.Snowpeak_Rare_Chu_Grotto_Sign, "Snowpeak" },
//                 };

//             List<HintSpot2> spots = new();
//             List<KeyValuePair<HintSpotLocation, string>> validLocations = new();
//             List<KeyValuePair<HintSpotLocation, string>> invalidLocations = new();

//             foreach (KeyValuePair<HintSpotLocation, string> pair in locationToZone)
//             {
//                 string zone = pair.Value;
//                 if (
//                     hintedBarrenZones.Contains(zone)
//                     || (snowpeakHintIsNothingBeyond && zone == "Snowpeak")
//                 )
//                 {
//                     invalidLocations.Add(pair);
//                     // Add junk hint for grotto when zone of grotto is barren.
//                     // HintSpot2 spot = new HintSpot2(pair.Key);
//                     // spot.hints.Add(new JunkHint());
//                     // spots.Add(spot);
//                 }
//                 else
//                 {
//                     validLocations.Add(pair);
//                 }
//             }

//             if (validLocations.Count < 1)
//                 return null;

//             KeyValuePair<HintSpotLocation, string> selectedPair = HintUtils.RemoveRandomListItem(
//                 rnd,
//                 validLocations
//             );

//             // HintUtils.ShuffleListInPlace(rnd, validLocations);

//             // KeyValuePair<HintSpotLocation, string> selectedPair = validLocations[0];
//             // validLocations.RemoveAt(0);

//             // Add hint to selected spot
//             HintSpot2 validSpot = new HintSpot2(selectedPair.Key);
//             validSpot.hints.Add(emptyGrottoHint);
//             spots.Add(validSpot);

//             Province selectedProvince = HintConstants.zoneToProvince[selectedPair.Value];

//             foreach (KeyValuePair<HintSpotLocation, string> pair in validLocations)
//             {
//                 // Hint toward the selected province.
//                 HintSpot2 spot = new HintSpot2(pair.Key);
//                 HintAbs hint = new TryOtherGrottoHint(AreaId.Province(selectedProvince));
//                 spot.hints.Add(hint);
//                 spots.Add(spot);
//             }

//             List<Province> provinces =
//                 new()
//                 {
//                     Province.Faron,
//                     Province.Eldin,
//                     Province.Lanayru,
//                     Province.Desert,
//                     Province.Peak,
//                 };

//             foreach (KeyValuePair<HintSpotLocation, string> pair in invalidLocations)
//             {
//                 Province provinceOfSpot = HintConstants.zoneToProvince[pair.Value];
//                 HintUtils.ShuffleListInPlace(rnd, provinces);
//                 Province provinceToHint =
//                     provinces[0] == provinceOfSpot ? provinces[1] : provinces[0];

//                 // Hint toward a random province.
//                 HintSpot2 spot = new HintSpot2(pair.Key);
//                 HintAbs hint = new TryOtherGrottoHint(AreaId.Province(provinceToHint));
//                 spot.hints.Add(hint);
//                 spots.Add(spot);
//             }

//             return spots;
//         }

//         // Picks a unique item which could have been SpoL. Priority is given to
//         // non-sphere0 checks.
//         private String getEmptyGrottoHintCheck()
//         {
//             Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//                 sSettings
//             );

//             Dictionary<Item, string> uniqueRequiredItemsToCheckName = new();
//             foreach (string checkName in requiredChecks)
//             {
//                 Item contents = getCheckContents(checkName);
//                 if (itemToChecksList.ContainsKey(contents))
//                 {
//                     List<string> checksList = itemToChecksList[contents];
//                     // Make sure we don't pick a checkName belonging to a zone
//                     // which does not belong to a province such as "Hero's
//                     // Shade".
//                     Province checkProvince = HintUtils.checkNameToHintProvince(
//                         sSettings,
//                         checkName
//                     );

//                     if (
//                         checksList != null
//                         && checksList.Count == 1
//                         && checkCanBeHintedSpol(checkName)
//                         && checkProvince != Province.Invalid
//                     // && checkProvince != Province.MultiProvince
//                     // && checkProvince != Province.Dungeon
//                     )
//                     {
//                         uniqueRequiredItemsToCheckName[contents] = checkName;
//                     }
//                 }
//             }

//             HashSet<string> sphere0Checks = playthroughSpheres.sphere0Checks;

//             List<KeyValuePair<Item, string>> sphere0Items = new();
//             List<KeyValuePair<Item, string>> laterDungeonItems = new();
//             List<KeyValuePair<Item, string>> laterNonDungeonItems = new();
//             foreach (KeyValuePair<Item, string> pair in uniqueRequiredItemsToCheckName)
//             {
//                 if (sphere0Checks.Contains(pair.Value))
//                     sphere0Items.Add(pair);
//                 else if (HintUtils.checkNameIsDungeonCheck(sSettings, pair.Value))
//                     laterDungeonItems.Add(pair);
//                 else
//                     laterNonDungeonItems.Add(pair);
//             }

//             string selectedCheckName = null;

//             if (laterNonDungeonItems.Count > 0)
//                 selectedCheckName = pickEmptyGrottoCheck(laterNonDungeonItems);
//             else if (laterDungeonItems.Count > 0)
//                 selectedCheckName = pickEmptyGrottoCheck(laterDungeonItems);
//             else if (sphere0Checks.Count > 0)
//                 selectedCheckName = pickEmptyGrottoCheck(sphere0Items);

//             if (selectedCheckName != null)
//             {
//                 alreadyCheckDirectedToward.Add(selectedCheckName);
//                 alreadyItemHintedRequired.Add(getCheckContents(selectedCheckName));
//                 // return buildEmptyGrottoHintSpots(selectedCheckName);
//             }

//             // return null;

//             return selectedCheckName;
//         }

//         private string pickEmptyGrottoCheck(List<KeyValuePair<Item, string>> checks)
//         {
//             List<KeyValuePair<double, KeyValuePair<Item, string>>> weightedChecks = new();
//             foreach (KeyValuePair<Item, string> pair in checks)
//             {
//                 double weight = HintConstants.bugsToRewardChecksMap.ContainsKey(pair.Key) ? 3 : 1;
//                 weightedChecks.Add(
//                     new KeyValuePair<double, KeyValuePair<Item, string>>(weight, pair)
//                 );
//             }

//             VoseInstance<KeyValuePair<Item, string>> inst = VoseAlgorithm.createInstance(
//                 weightedChecks
//             );

//             KeyValuePair<Item, string> itemAndCheckName = inst.NextAndKeep(rnd);
//             return itemAndCheckName.Value;
//         }

//         // !! Not adjusted to handle that there are 8 instead of 6 provinces
//         // now, but not using at the moment so it's okay.
//         private List<HintSpot2> buildEmptyGrottoHintSpots(string checkName)
//         {
//             List<Province> provinces =
//                 new()
//                 {
//                     Province.Ordona,
//                     Province.Faron,
//                     Province.Eldin,
//                     Province.Lanayru,
//                     Province.Desert,
//                     Province.Peak,
//                 };

//             List<HintSpotLocation> locations =
//                 new()
//                 {
//                     HintSpotLocation.Kakariko_Gorge_Grotto_Sign,
//                     HintSpotLocation.Faron_Field_Water_Grotto_Sign,
//                     HintSpotLocation.Lanayru_Field_Chus_Grotto_Sign,
//                     HintSpotLocation.Desert_Chus_Grotto_Sign,
//                     HintSpotLocation.Snowpeak_Rare_Chu_Grotto_Sign,
//                 };

//             Province province = HintUtils.checkNameToHintProvince(sSettings, checkName);
//             if (!provinces.Contains(province))
//                 return null;

//             Item item = getCheckContents(checkName);

//             provinces.Remove(province);
//             HintUtils.ShuffleListInPlace(rnd, provinces);
//             HintUtils.ShuffleListInPlace(rnd, locations);

//             List<HintSpot2> spots = new();
//             for (int i = 0; i < provinces.Count; i++)
//             {
//                 int secondIndex = (i + 1) % provinces.Count;

//                 List<AreaId> areadIds = new();
//                 areadIds.Add(AreaId.Province(provinces[i]));
//                 areadIds.Add(AreaId.Province(provinces[secondIndex]));

//                 HintSpot2 spot = new HintSpot2(locations[i]);
//                 spot.hints.Add(new ItemMultiLocationsHint(item, areadIds, false));
//                 spots.Add(spot);
//             }

//             return spots;
//         }

//         // TODO: need to handle unhappy paths, such as no valid combinations of
//         // unique zones or only 2 of the expected 3 zones or something.
//         private List<PathHint> genPathHints(int numHintsDesired)
//         {
//             Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//                 sSettings
//             );

//             List<List<KeyValuePair<Goal, List<string>>>> lists = splitPathGoalsToLists();
//             List<KeyValuePair<Goal, List<string>>> primaryList = lists[0];
//             List<KeyValuePair<Goal, List<string>>> secondaryList = lists[1];

//             // Then we pick from the primary list. Up to the size of the list, max is desired count.
//             // Need primary list and desired hints (can be over max; will just return a certain count).
//             // Also will resort the list in place based on which ones were picked (not picked moved to end).
//             List<PathHint> pathHints = pickPrimaryPathHints(primaryList, numHintsDesired);

//             List<KeyValuePair<Goal, List<string>>> combinedList = new List<
//                 KeyValuePair<Goal, List<string>>
//             >()
//                 .Concat(primaryList)
//                 .Concat(secondaryList)
//                 .ToList();

//             Dictionary<Goal, HashSet<string>> goalToHintedZones = new();
//             Dictionary<GoalEnum, Goal> goalEnumToGoal = new();
//             foreach (KeyValuePair<Goal, List<string>> pair in combinedList)
//             {
//                 goalToHintedZones[pair.Key] = new();
//                 goalEnumToGoal[pair.Key.goalEnum] = pair.Key;
//             }

//             // Mark hinted checks and for each goal which zones have been hinted.
//             HashSet<string> hintedChecks = new();
//             foreach (PathHint hint in pathHints)
//             {
//                 hintedChecks.Add(hint.checkName);
//                 string zoneName = checkToHintZoneMap[hint.checkName];
//                 Goal goal = goalEnumToGoal[hint.goalEnum];
//                 goalToHintedZones[goal].Add(zoneName);
//             }

//             // For each goal, keep track of which zones have been hinted.
//             // When iterating on a goal, filter out checks which have been hinted.
//             // Reconstruct zone lists, giving priority to zones for that goal which have not been hinted.
//             // Pick random zone for that goal.
//             // Mark zone as hinted for that goal.
//             // Pick random check for that zone which has not been hinted.
//             // Continue.

//             // foreach (KeyValuePair<Goal, List<string>> pair in combinedList)
//             // {
//             //     List<string> filteredChecks = pair.Value.Where(checkName => !hintedChecks.Contains(checkName)).ToList();
//             //     pair.Value.Clear();
//             //     pair.Value.AddRange(filteredChecks);
//             // }

//             int currentIndex = pathHints.Count;
//             if (currentIndex >= combinedList.Count)
//                 currentIndex = 0;

//             // TODO: Need to rewrite at the zone level.

//             while (pathHints.Count < numHintsDesired && combinedList.Count > 0)
//             {
//                 KeyValuePair<Goal, List<string>> pair = combinedList[currentIndex];
//                 // Filter already hinted checks from list
//                 pair.Value.RemoveAll(checkName => hintedChecks.Contains(checkName));

//                 if (pair.Value.Count < 1)
//                 {
//                     // If no available checks for goal, filter out goal from options.
//                     combinedList.RemoveAt(currentIndex);
//                     if (currentIndex >= combinedList.Count)
//                         currentIndex = 0;
//                     continue;
//                 }

//                 // Condense remaining checks for checkList into priorityZones and not priority zones.
//                 HashSet<string> priorityZones = new();
//                 HashSet<string> secondaryZones = new();
//                 foreach (string checkName in pair.Value)
//                 {
//                     string hintZone = checkToHintZoneMap[checkName];
//                     if (goalToHintedZones[pair.Key].Contains(hintZone))
//                         secondaryZones.Add(hintZone);
//                     else
//                         priorityZones.Add(hintZone);
//                 }

//                 string selectedZone;
//                 if (priorityZones.Count > 0)
//                     selectedZone = HintUtils.RemoveRandomHashSetItem(rnd, priorityZones);
//                 else
//                     selectedZone = HintUtils.RemoveRandomHashSetItem(rnd, secondaryZones);

//                 // Pick random check for that zone
//                 List<string> checksForZone = pair.Value.FindAll(
//                     checkName => checkToHintZoneMap[checkName] == selectedZone
//                 );
//                 string selectedCheckName = HintUtils.RemoveRandomListItem(rnd, checksForZone);
//                 pair.Value.Remove(selectedCheckName);

//                 // Mark zone as hinted for that goal
//                 goalToHintedZones[pair.Key].Add(selectedZone);

//                 // Create path hint for this goal and check
//                 Item contents = getCheckContents(selectedCheckName);
//                 // Mark checkName as directed toward
//                 alreadyCheckDirectedToward.Add(selectedCheckName);
//                 hintedChecks.Add(selectedCheckName);

//                 PathHint hint = new PathHint(
//                     AreaId.ZoneStr(selectedZone),
//                     selectedCheckName,
//                     pair.Key.goalEnum
//                 );
//                 pathHints.Add(hint);

//                 currentIndex = (currentIndex + 1) % combinedList.Count;
//             }

//             // // TODO: pick up with picking more hints. Not needed for playtesting
//             // // current settings. (This error should be pushed down to the bottom
//             // // when we truly fail; not expected to ever really happen).
//             // if (primaryPathHints.Count < numHintsDesired)
//             //     throw new Exception(
//             //         $"Wanted {numHintsDesired} path hints, but only generated {primaryPathHints.Count}."
//             //     );

//             return pathHints;

//             // // calculate from goalToRequiredChecks
//             // Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//             //     sSettings
//             // );

//             // // Ideal, pick 1 hint from the required dungeons, up to the desired
//             // // count or the numDungeons. These should be different zones for
//             // // each. Then continue from the secondary list and loop back around
//             // // everything until there are no more hints to give or no more
//             // // desired.

//             // // If unable to pick unique zones from required dungeons, then skip
//             // // this step and begin looping through (might be partway through
//             // // primary list).

//             // // First we build the primary (dungeons with path checks) and
//             // // secondary lists (hyrule castle and ganondorf; filtered based on
//             // // previous checks).

//             // Dictionary<Goal, List<string>> goalToZones = new();

//             // foreach (KeyValuePair<Goal, List<string>> pair in goalToRequiredChecks)
//             // {
//             //     // if (!GoalConstants.IsDungeonGoal(pair.Key))
//             //     //     continue;

//             //     HashSet<string> zonesForGoal = new();

//             //     List<string> checkNames = pair.Value;
//             //     if (checkNames == null)
//             //         continue;

//             //     foreach (string checkName in checkNames)
//             //     {
//             //         if (!checkCanBeHintedSpol(checkName))
//             //             continue;

//             //         string zoneName = checkToHintZoneMap[checkName];
//             //         zonesForGoal.Add(zoneName);
//             //     }

//             //     if (zonesForGoal.Count > 0)
//             //     {
//             //         List<string> zonesForGoalList = new List<string>()
//             //             .Concat(zonesForGoal)
//             //             .ToList();
//             //         HintUtils.ShuffleListInPlace(rnd, zonesForGoalList);
//             //         goalToZones[pair.Key] = zonesForGoalList;
//             //     }
//             // }

//             // // if (goalToZones.Count != 3)
//             // if (goalToZones.Count < 3)
//             // {
//             //     throw new Exception($"Expected 3 goals but only found {goalToZones.Count}.");
//             // }

//             // List<KeyValuePair<Goal, List<string>>> goalToZonesList = new();
//             // foreach (KeyValuePair<Goal, List<string>> pair in goalToZones)
//             // {
//             //     goalToZonesList.Add(pair);
//             // }

//             // HashSet<string> results = new();

//             // Dictionary<string, double> zoneWeightings = getZoneWeightings();

//             // // commented out so can change function signature
//             // // recurPickUniqueZonesCombo(results, goalToZonesList, new(), new());

//             // if (results.Count == 0)
//             //     throw new Exception($"Expected some results, but there were none.");

//             // // From valid checks to hint, get a list of zones for each goal.

//             // // Then we need to find distributions for the first 3 dungeons which
//             // // are a different zone for each if possible.

//             // List<KeyValuePair<double, string>> weightedList = new();
//             // foreach (string comboId in results)
//             // {
//             //     string[] zones = comboId.Split("###");
//             //     List<double> weights = new();
//             //     double lowestWeight = 1000.0;

//             //     double avgWeight = 0.0;

//             //     for (int i = 0; i < zones.Length; i++)
//             //     {
//             //         string zone = zones[i];
//             //         double weight = zoneWeightings[zone];
//             //         if (weight < lowestWeight)
//             //         {
//             //             lowestWeight = weight;
//             //         }
//             //         avgWeight += weight;
//             //     }

//             //     avgWeight += lowestWeight * 7;
//             //     avgWeight /= zones.Length + 7;

//             //     weightedList.Add(new(avgWeight, comboId));
//             // }

//             // VoseInstance<string> voseInst = VoseAlgorithm.createInstance(weightedList);
//             // string selectedComboId = voseInst.NextAndKeep(rnd);

//             // string[] selectedZones = selectedComboId.Split("###");
//             // List<PathHint> pathHints = new();

//             // for (int i = 0; i < selectedZones.Length; i++)
//             // {
//             //     string desiredZoneName = selectedZones[i];
//             //     Goal goal = goalToZonesList[i].Key;
//             //     List<string> requiredChecksOfGoal = goalToRequiredChecks[goal];

//             //     List<string> requiredChecksInZone = new();
//             //     foreach (string checkName in requiredChecksOfGoal)
//             //     {
//             //         if (checkToHintZoneMap[checkName] == desiredZoneName)
//             //         {
//             //             requiredChecksInZone.Add(checkName);
//             //         }
//             //     }

//             //     HintUtils.ShuffleListInPlace(rnd, requiredChecksInZone);
//             //     string selectedCheckName = requiredChecksInZone[0];
//             //     Item contents = getCheckContents(selectedCheckName);
//             //     // Mark checkName as directed toward
//             //     alreadyCheckDirectedToward.Add(selectedCheckName);

//             //     PathHint hint = new PathHint(
//             //         TypedId.Zone(desiredZoneName),
//             //         selectedCheckName,
//             //         goal.goalEnum
//             //     );
//             //     pathHints.Add(hint);

//             //     // Create a hint from the selectedCheckName and the goal.
//             // }

//             // // Get every possible combination of zone selections for each which has no repeats.

//             // // This can probably be done recursively.
//             // return pathHints;
//         }

//         private List<List<KeyValuePair<Goal, List<string>>>> splitPathGoalsToLists()
//         {
//             Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//                 sSettings
//             );

//             List<KeyValuePair<Goal, List<string>>> primaryList = new();
//             List<KeyValuePair<Goal, List<string>>> secondaryList = new();

//             // Ideal, pick 1 hint from the required dungeons, up to the desired
//             // count or the numDungeons. These should be different zones for
//             // each. Then continue from the secondary list and loop back around
//             // everything until there are no more hints to give or no more
//             // desired.

//             // If unable to pick unique zones from required dungeons, then skip
//             // this step and begin looping through (might be partway through
//             // primary list).

//             // First we build the primary (dungeons with path checks) and
//             // secondary lists (hyrule castle and ganondorf; filtered based on
//             // previous checks).

//             HashSet<string> checkNamesForPrimaryGoals = new();

//             Dictionary<Goal, List<string>> goalToZones = new();

//             foreach (KeyValuePair<Goal, List<string>> pair in goalToRequiredChecks)
//             {
//                 List<string> checkNames = pair.Value;
//                 if (checkNames == null)
//                     continue;

//                 bool isPrimaryGoal = GoalConstants.IsDungeonGoal(pair.Key);
//                 List<string> canBeHintedCheckNames = new();

//                 foreach (string checkName in checkNames)
//                 {
//                     if (!checkCanBeHintedSpol(checkName))
//                         continue;

//                     canBeHintedCheckNames.Add(checkName);
//                     if (isPrimaryGoal)
//                     {
//                         checkNamesForPrimaryGoals.Add(checkName);
//                     }
//                 }

//                 if (canBeHintedCheckNames.Count > 0)
//                 {
//                     if (isPrimaryGoal)
//                         primaryList.Add(new(pair.Key, canBeHintedCheckNames));
//                     else
//                         secondaryList.Add(new(pair.Key, canBeHintedCheckNames));
//                 }
//             }
//             HintUtils.ShuffleListInPlace(rnd, primaryList);

//             // filter secondary list items to not include ones in primary
//             secondaryList = filterPathHintSecondaryList(secondaryList, checkNamesForPrimaryGoals);

//             return new() { primaryList, secondaryList };
//         }

//         private List<KeyValuePair<Goal, List<string>>> filterPathHintSecondaryList(
//             List<KeyValuePair<Goal, List<string>>> origList,
//             HashSet<string> checkNamesForPrimaryGoals
//         )
//         {
//             List<KeyValuePair<Goal, List<string>>> filteredSecondaryList = new();
//             HashSet<string> checkNamesForSecondaryGoals = new();

//             foreach (KeyValuePair<Goal, List<string>> pair in origList)
//             {
//                 List<string> filteredChecks = new();
//                 foreach (string checkName in pair.Value)
//                 {
//                     if (
//                         !checkNamesForPrimaryGoals.Contains(checkName)
//                         && !checkNamesForSecondaryGoals.Contains(checkName)
//                     )
//                     {
//                         filteredChecks.Add(checkName);
//                     }
//                 }

//                 if (filteredChecks.Count > 0)
//                 {
//                     foreach (string checkName in filteredChecks)
//                     {
//                         checkNamesForSecondaryGoals.Add(checkName);
//                     }

//                     filteredSecondaryList.Add(new(pair.Key, filteredChecks));
//                 }
//             }

//             return filteredSecondaryList;
//         }

//         private List<PathHint> pickPrimaryPathHints(
//             List<KeyValuePair<Goal, List<string>>> primaryList,
//             int numHintsDesired
//         )
//         {
//             if (numHintsDesired < 1)
//                 return new();

//             int numHintsToPick =
//                 numHintsDesired > primaryList.Count ? primaryList.Count : numHintsDesired;

//             List<List<int>> combinations = getCombinationIndexes(primaryList.Count, numHintsToPick);

//             reorderPathPrimaryList(primaryList);

//             return pickPrimaryPathHintsFromCombinations(primaryList, numHintsToPick, combinations);
//         }

//         private List<PathHint> pickPrimaryPathHintsFromCombinations(
//             List<KeyValuePair<Goal, List<string>>> primaryList,
//             int numHintsDesired,
//             List<List<int>> combinations
//         )
//         {
//             Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//                 sSettings
//             );

//             List<KeyValuePair<Goal, List<string>>> goalAndZonesList = new();

//             foreach (KeyValuePair<Goal, List<string>> pair in primaryList)
//             {
//                 HashSet<string> zonesForGoal = new();

//                 List<string> checkNames = pair.Value;
//                 if (checkNames == null)
//                     continue;

//                 foreach (string checkName in checkNames)
//                 {
//                     string zoneName = checkToHintZoneMap[checkName];
//                     zonesForGoal.Add(zoneName);
//                 }

//                 if (zonesForGoal.Count > 0)
//                 {
//                     List<string> zonesForGoalList = new List<string>()
//                         .Concat(zonesForGoal)
//                         .ToList();
//                     HintUtils.ShuffleListInPlace(rnd, zonesForGoalList);
//                     goalAndZonesList.Add(new(pair.Key, zonesForGoalList));
//                 }
//             }

//             Dictionary<string, int> results = new();
//             List<int> failedAttempts = new() { 0 };

//             for (int i = 0; i < combinations.Count; i++)
//             {
//                 List<int> combination = combinations[i];
//                 List<KeyValuePair<Goal, List<string>>> partialList = new(combination.Count);
//                 foreach (int index in combination)
//                 {
//                     partialList.Add(goalAndZonesList[index]);
//                 }

//                 recurPickUniqueZonesCombo(results, failedAttempts, i, partialList, new(), new());
//                 if (results.Count >= 100 || (results.Count > 0 && failedAttempts[0] >= 500))
//                     break;
//             }

//             return pickPathHintsFromResults(primaryList, results, combinations, goalAndZonesList);
//         }

//         private List<PathHint> pickPathHintsFromResults(
//             List<KeyValuePair<Goal, List<string>>> primaryList,
//             Dictionary<string, int> results,
//             List<List<int>> combinations,
//             List<KeyValuePair<Goal, List<string>>> goalAndZonesList
//         )
//         {
//             if (results.Count < 1)
//                 return new();

//             Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//                 sSettings
//             );
//             Dictionary<string, double> zoneWeightings = getZoneWeightings();

//             List<KeyValuePair<double, string>> weightedList = new();
//             foreach (KeyValuePair<string, int> pair in results)
//             {
//                 string[] zones = pair.Key.Split("###");
//                 List<double> weights = new();
//                 double lowestWeight = 1000.0;

//                 double avgWeight = 0.0;

//                 for (int i = 0; i < zones.Length; i++)
//                 {
//                     string zone = zones[i];
//                     double weight = zoneWeightings[zone];
//                     if (weight < lowestWeight)
//                     {
//                         lowestWeight = weight;
//                     }
//                     avgWeight += weight;
//                 }

//                 avgWeight += lowestWeight * 7;
//                 avgWeight /= zones.Length + 7;

//                 weightedList.Add(new(avgWeight, pair.Key));
//             }

//             VoseInstance<string> voseInst = VoseAlgorithm.createInstance(weightedList);
//             string selectedComboId = voseInst.NextAndKeep(rnd);

//             string[] selectedZones = selectedComboId.Split("###");
//             List<PathHint> pathHints = new();

//             int combinationIndex = results[selectedComboId];
//             List<int> combination = combinations[combinationIndex];

//             for (int i = 0; i < selectedZones.Length; i++)
//             {
//                 string desiredZoneName = selectedZones[i];
//                 int goalAndZonesListIndex = combination[i];
//                 Goal goal = goalAndZonesList[goalAndZonesListIndex].Key;
//                 List<string> requiredChecksOfGoal = goalToRequiredChecks[goal];

//                 List<string> requiredChecksInZone = new();
//                 foreach (string checkName in requiredChecksOfGoal)
//                 {
//                     if (checkToHintZoneMap[checkName] == desiredZoneName)
//                     {
//                         requiredChecksInZone.Add(checkName);
//                     }
//                 }

//                 HintUtils.ShuffleListInPlace(rnd, requiredChecksInZone);
//                 string selectedCheckName = requiredChecksInZone[0];
//                 Item contents = getCheckContents(selectedCheckName);
//                 // Mark checkName as directed toward
//                 alreadyCheckDirectedToward.Add(selectedCheckName);

//                 PathHint hint = new PathHint(
//                     AreaId.ZoneStr(desiredZoneName),
//                     selectedCheckName,
//                     goal.goalEnum
//                 );
//                 pathHints.Add(hint);
//             }

//             // Reorder primaryList so that the goals we selected are at the
//             // start of the list so the caller knows from which goal index it
//             // should resume selecting more path hints.
//             List<KeyValuePair<Goal, List<string>>> newPrimaryList = new();
//             List<KeyValuePair<Goal, List<string>>> newPrimaryListEnd = new();
//             HashSet<int> indexesToFront = new();
//             foreach (int index in combination)
//             {
//                 indexesToFront.Add(index);
//             }

//             for (int i = 0; i < primaryList.Count; i++)
//             {
//                 if (indexesToFront.Contains(i))
//                     newPrimaryList.Add(primaryList[i]);
//                 else
//                     newPrimaryListEnd.Add(primaryList[i]);
//             }

//             newPrimaryList.AddRange(newPrimaryListEnd);
//             primaryList.Clear();
//             primaryList.AddRange(newPrimaryList);

//             return pathHints;
//         }

//         private void reorderPathPrimaryList(List<KeyValuePair<Goal, List<string>>> primaryList)
//         {
//             List<KeyValuePair<double, int>> weightedIndexes = new();
//             for (int i = 0; i < primaryList.Count; i++)
//             {
//                 KeyValuePair<Goal, List<string>> pair = primaryList[i];
//                 double weight = pair.Value.Count;
//                 if (weight > 7)
//                     weight = 3;
//                 weightedIndexes.Add(new(Math.Sqrt(weight), i));
//             }

//             List<int> newOrder = new();

//             // Reorder based on weights. Slightly discourages goals which only
//             // require 1 or 2 things as well as goals which require a huge
//             // number of things.
//             VoseInstance<int> voseInst = VoseAlgorithm.createInstance(weightedIndexes);
//             while (voseInst.HasMore())
//             {
//                 int index = voseInst.NextAndRemove(rnd);
//                 newOrder.Add(index);
//             }

//             List<KeyValuePair<Goal, List<string>>> newList = new();
//             foreach (int index in newOrder)
//             {
//                 newList.Add(primaryList[index]);
//             }

//             primaryList.Clear();
//             primaryList.AddRange(newList);
//         }

//         private List<List<int>> getCombinationIndexes(int totalLength, int numToPick)
//         {
//             if (totalLength < 1)
//                 throw new Exception($"totalLength must be at least 1. Received '{totalLength}'.");
//             else if (numToPick > totalLength)
//                 throw new Exception(
//                     $"numToPick must not exceed totalLength. Received totalLength '{totalLength}' and numToPick '{numToPick}'."
//                 );

//             List<int> sourceArr = new(totalLength);
//             for (int i = 0; i < totalLength; i++)
//             {
//                 sourceArr.Add(i);
//             }

//             List<List<int>> results = new();
//             recurGetCombinationIndexes(numToPick, sourceArr, results);
//             return results;
//         }

//         private void recurGetCombinationIndexes(
//             int numToPick,
//             List<int> selectionSource,
//             List<List<int>> results,
//             int startIndex = 0,
//             List<int> pushedItems = null
//         )
//         {
//             if (pushedItems == null)
//                 pushedItems = new(numToPick);

//             for (int i = startIndex; i < selectionSource.Count; i++)
//             {
//                 if (pushedItems.Count + 1 < numToPick)
//                 {
//                     pushedItems.Add(selectionSource[i]);
//                     recurGetCombinationIndexes(
//                         numToPick,
//                         selectionSource,
//                         results,
//                         i + 1,
//                         pushedItems
//                     );
//                     pushedItems.RemoveAt(pushedItems.Count - 1);
//                 }
//                 else
//                 {
//                     List<int> b = new(pushedItems);
//                     b.Add(selectionSource[i]);
//                     results.Add(b);
//                 }
//             }
//         }

//         private void recurPickUniqueZonesCombo(
//             Dictionary<string, int> results,
//             List<int> failedAttempts,
//             int combinationIndex,
//             List<KeyValuePair<Goal, List<string>>> goalsAndZones,
//             HashSet<string> currentZones,
//             List<string> currentZonesList
//         )
//         {
//             List<string> currentColValues = goalsAndZones[currentZonesList.Count].Value;
//             for (int i = 0; i < currentColValues.Count; i++)
//             {
//                 string currZone = currentColValues[i];
//                 if (currentZones.Contains(currZone))
//                 {
//                     failedAttempts[0] = failedAttempts[0] + 1;
//                     continue;
//                 }

//                 if (currentZonesList.Count < goalsAndZones.Count - 1)
//                 {
//                     // Not on the leaf list
//                     currentZones.Add(currZone);
//                     currentZonesList.Add(currZone);
//                     recurPickUniqueZonesCombo(
//                         results,
//                         failedAttempts,
//                         combinationIndex,
//                         goalsAndZones,
//                         currentZones,
//                         currentZonesList
//                     );
//                     currentZonesList.RemoveAt(currentZonesList.Count - 1);
//                     currentZones.Remove(currZone);
//                 }
//                 else
//                 {
//                     // are on the leaf and this is a valid combination
//                     currentZonesList.Add(currZone);
//                     string key = string.Join("###", currentZonesList);
//                     if (!results.ContainsKey(key))
//                         results[key] = combinationIndex;
//                     else
//                         failedAttempts[0] = failedAttempts[0] + 1;
//                     currentZonesList.Remove(currZone);
//                 }
//                 // need to call recu
//             }

//             // Iterate over each row in this column since we are the last one.
//             // List<string> currentColValues2 = goalsAndZones[positions.Count].Value;
//             // // for (int i = 0; i < currentColValues2.Count; i++)
//             // // {
//             // //     positions.Add(i);
//             // //     recurPickUniqueZonesCombo(results, goalsAndZones, positions);
//             // //     positions.RemoveAt(positions.Count - 1);
//             // // }

//             // do nothing
//         }

//         private Dictionary<string, double> getZoneWeightings()
//         {
//             Dictionary<string, bool> zoneToHasSphereLater = new();

//             Dictionary<string, string> checkToHintZone = HintUtils.getCheckToHintZoneMap(sSettings);

//             // Get zones which can be hinted SpoL, and track which ones have a
//             // check which can be hinted SpoL which is not in sphere 0.
//             foreach (string checkName in requiredChecks)
//             {
//                 Item contents = getCheckContents(checkName);

//                 if (!checkCanBeHintedSpol(checkName))
//                 {
//                     continue;
//                 }

//                 string hintZone = checkToHintZone[checkName];
//                 bool hasSphereLater = false;
//                 zoneToHasSphereLater.TryGetValue(hintZone, out hasSphereLater);
//                 if (!hasSphereLater)
//                 {
//                     hasSphereLater = !isCheckSphere0(checkName);
//                 }
//                 zoneToHasSphereLater[hintZone] = hasSphereLater;

//                 // condense into list of zones.

//                 // pick zone based on numberOfNonSphere0 checks (large ones are
//                 // bad). single check ones are also bad. optimal size would
//                 // probably be

//                 // Don't count checks known to player

//                 // Dungeons, Desert, and LLC are hardcoded weaker?

//                 // Interesting hint is non-sphere0 ZD.
//                 // Also Lanayru Field
//                 // Also Sacred Grove
//                 // Faron Woods would be fine also
//                 // WoCT and SoCt are fine
//                 // Kak Gorge is fine
//                 // Snowpeak is super good if non-s0 check.

//                 // Boring:
//                 // Dungeons
//                 // LLC
//                 // Desert
//                 // Bulblin Camp
//                 // Sphere0 Kak GY (non-s0 is fine)
//                 // s0 Ordon check

//                 // ^ these are all boring because "I was going to do that
//                 // anyway" or if you find the hint later, then "I already did
//                 // that".

//                 //   "Death Mountain" 1 (0, 1), lol

//                 // Special ones:
//                 //   "Agitha", don't hint if gets own hint
//                 //   "Hidden Village", good if not excluded
//                 //   "Hero's Spirit", great if not excluded
//                 //   "Cave of Ordeals", great if not excluded

//                 //   "Castle Town" 4 (2, 2), PLEASE
//                 //   "Snowpeak" 3 (1, 2), great if not s0, else meh

//                 //   "Lanayru Field" 6 (0, 6), good
//                 //   "Sacred Grove" 6 (0, 6), good
//                 //   "Great Bridge of Hylia" 7 (0, 7), good
//                 //   "Kakariko Gorge" 9 (2, 7), good (better if not s0)
//                 //   "N Eldin Field" 8 (0, 8), good (no s0 at all)
//                 //   "Lanayru Spring" 7 (0, 7), good (no s0)
//                 //   "Faron Woods" 10 (3, 7), probably good since encourages glitches (3 s0, 7 not)

//                 //   "West of Castle Town" 5 (1, 4), pretty good (1 s0)
//                 //   "South of Castle Town" 6 (3, 3), pretty good
//                 //   "Lake Hylia" 10 (8, 2), not bad

//                 //   "Zora's Domain" 6 (3, 3), good if not s0. Okay if s0
//                 //   "Faron Field" 7 (4, 3), okay if not s0, but kind of meh since usually do
//                 //   "Kakariko Village" 8 (5, 3, 2), fine
//                 //   "S Eldin Field" 8 (4, 4), fine (better if not s0)
//                 //   "Kakariko Graveyard" 4 (2, 2), way better if not s0, else meh
//                 //   "Upper Zora's River" 3 (2, 1), okay

//                 //   "Bulblin Camp" 6 (0, 6), not amazing; excluded anyway

//                 // Special bad ones:
//                 //   "Ordon" 10 (7, 3), never great
//                 //   "Lake Lantern Cave" 15 (0, 15), HORRIBLE
//                 //   "Gerudo Desert" 16 (0, 16), HORRIBLE

//                 //   "Forest Temple" (all have a lot), all dungeons are really boring
//                 //   "Goron Mines",
//                 //   "Lakebed Temple",
//                 //   "Arbiter's Grounds",
//                 //   "Snowpeak Ruins",
//                 //   "Temple of Time",
//                 //   "City in the Sky",
//                 //   "Palace of Twilight",
//                 //   "Hyrule Castle,
//             }

//             // Now should iterate over each one determine percentage which is sphere0
//             // Dictionary<string, double> zoneToPercentSphere0 = new();
//             List<ZoneNumberData> zoneNumberDataList = new();

//             Dictionary<string, string[]> zoneToChecksMap = HintUtils.getHintZoneToChecksMap(
//                 sSettings
//             );

//             // Check hinted by name (always, sometimes)
//             // Check directed to (empty grotto)
//             // check hinted barren (snowpeak)

//             // SpoL: cannot point to a check already hinted by name or already directed to

//             // Barren: should not

//             foreach (KeyValuePair<string, bool> pair in zoneToHasSphereLater)
//             {
//                 string zoneName = pair.Key;
//                 string[] checks = zoneToChecksMap[zoneName];
//                 int total = 0;
//                 int numSphere0 = 0;

//                 foreach (string checkName in checks)
//                 {
//                     // Skip over any checks which should not be counted when
//                     // calculating how big the potential SpoL area is.
//                     if (
//                         HintUtils.checkIsPlayerKnownStatus(checkName)
//                         || alreadyCheckContentsHinted.Contains(checkName)
//                         || alreadyCheckKnownBarren.Contains(checkName)
//                     )
//                     {
//                         continue;
//                     }

//                     if (isCheckSphere0(checkName))
//                         numSphere0 += 1;
//                     total += 1;
//                 }

//                 if (total == 0)
//                     continue;

//                 double percent = (double)numSphere0 / (double)total;
//                 // zoneToPercentSphere0[zoneName] = percent;
//                 zoneNumberDataList.Add(new ZoneNumberData(zoneName, percent, total, pair.Value));
//             }

//             Dictionary<string, double> zoneToWeight = new();

//             List<KeyValuePair<double, string>> weightedZones = new();
//             foreach (ZoneNumberData data in zoneNumberDataList)
//             {
//                 double weight = data.calculateWeight();
//                 zoneToWeight[data.hintZone] = weight;
//                 // weightedZones.Add(new(weight, data.hintZone));
//             }

//             return zoneToWeight;
//         }

//         private List<SpiritOfLightHint> genSpolHints(int numSpolZones)
//         {
//             List<string> result = new();

//             if (numSpolZones < 1)
//                 return new();

//             // pick spol checks
//             Dictionary<string, bool> zoneToHasSphereLater = new();

//             Dictionary<string, string> checkToHintZone = HintUtils.getCheckToHintZoneMap(sSettings);

//             // Get zones which can be hinted SpoL, and track which ones have a
//             // check which can be hinted SpoL which is not in sphere 0.
//             foreach (string checkName in requiredChecks)
//             {
//                 Item contents = getCheckContents(checkName);

//                 if (!checkCanBeHintedSpol(checkName))
//                 {
//                     continue;
//                 }

//                 string hintZone = checkToHintZone[checkName];
//                 bool hasSphereLater = false;
//                 zoneToHasSphereLater.TryGetValue(hintZone, out hasSphereLater);
//                 if (!hasSphereLater)
//                 {
//                     hasSphereLater = !isCheckSphere0(checkName);
//                 }
//                 zoneToHasSphereLater[hintZone] = hasSphereLater;

//                 // condense into list of zones.

//                 // pick zone based on numberOfNonSphere0 checks (large ones are
//                 // bad). single check ones are also bad. optimal size would
//                 // probably be

//                 // Don't count checks known to player

//                 // Dungeons, Desert, and LLC are hardcoded weaker?

//                 // Interesting hint is non-sphere0 ZD.
//                 // Also Lanayru Field
//                 // Also Sacred Grove
//                 // Faron Woods would be fine also
//                 // WoCT and SoCt are fine
//                 // Kak Gorge is fine
//                 // Snowpeak is super good if non-s0 check.

//                 // Boring:
//                 // Dungeons
//                 // LLC
//                 // Desert
//                 // Bulblin Camp
//                 // Sphere0 Kak GY (non-s0 is fine)
//                 // s0 Ordon check

//                 // ^ these are all boring because "I was going to do that
//                 // anyway" or if you find the hint later, then "I already did
//                 // that".

//                 //   "Death Mountain" 1 (0, 1), lol

//                 // Special ones:
//                 //   "Agitha", don't hint if gets own hint
//                 //   "Hidden Village", good if not excluded
//                 //   "Hero's Spirit", great if not excluded
//                 //   "Cave of Ordeals", great if not excluded

//                 //   "Castle Town" 4 (2, 2), PLEASE
//                 //   "Snowpeak" 3 (1, 2), great if not s0, else meh

//                 //   "Lanayru Field" 6 (0, 6), good
//                 //   "Sacred Grove" 6 (0, 6), good
//                 //   "Great Bridge of Hylia" 7 (0, 7), good
//                 //   "Kakariko Gorge" 9 (2, 7), good (better if not s0)
//                 //   "N Eldin Field" 8 (0, 8), good (no s0 at all)
//                 //   "Lanayru Spring" 7 (0, 7), good (no s0)
//                 //   "Faron Woods" 10 (3, 7), probably good since encourages glitches (3 s0, 7 not)

//                 //   "West of Castle Town" 5 (1, 4), pretty good (1 s0)
//                 //   "South of Castle Town" 6 (3, 3), pretty good
//                 //   "Lake Hylia" 10 (8, 2), not bad

//                 //   "Zora's Domain" 6 (3, 3), good if not s0. Okay if s0
//                 //   "Faron Field" 7 (4, 3), okay if not s0, but kind of meh since usually do
//                 //   "Kakariko Village" 8 (5, 3, 2), fine
//                 //   "S Eldin Field" 8 (4, 4), fine (better if not s0)
//                 //   "Kakariko Graveyard" 4 (2, 2), way better if not s0, else meh
//                 //   "Upper Zora's River" 3 (2, 1), okay

//                 //   "Bulblin Camp" 6 (0, 6), not amazing; excluded anyway

//                 // Special bad ones:
//                 //   "Ordon" 10 (7, 3), never great
//                 //   "Lake Lantern Cave" 15 (0, 15), HORRIBLE
//                 //   "Gerudo Desert" 16 (0, 16), HORRIBLE

//                 //   "Forest Temple" (all have a lot), all dungeons are really boring
//                 //   "Goron Mines",
//                 //   "Lakebed Temple",
//                 //   "Arbiter's Grounds",
//                 //   "Snowpeak Ruins",
//                 //   "Temple of Time",
//                 //   "City in the Sky",
//                 //   "Palace of Twilight",
//                 //   "Hyrule Castle,
//             }

//             // Now should iterate over each one determine percentage which is sphere0
//             // Dictionary<string, double> zoneToPercentSphere0 = new();
//             List<ZoneNumberData> zoneNumberDataList = new();

//             Dictionary<string, string[]> zoneToChecksMap = HintUtils.getHintZoneToChecksMap(
//                 sSettings
//             );

//             // Check hinted by name (always, sometimes)
//             // Check directed to (empty grotto)
//             // check hinted barren (snowpeak)

//             // SpoL: cannot point to a check already hinted by name or already directed to

//             // Barren: should not

//             foreach (KeyValuePair<string, bool> pair in zoneToHasSphereLater)
//             {
//                 string zoneName = pair.Key;
//                 string[] checks = zoneToChecksMap[zoneName];
//                 int total = 0;
//                 int numSphere0 = 0;

//                 foreach (string checkName in checks)
//                 {
//                     // Skip over any checks which should not be counted when
//                     // calculating how big the potential SpoL area is.
//                     if (
//                         HintUtils.checkIsPlayerKnownStatus(checkName)
//                         || alreadyCheckContentsHinted.Contains(checkName)
//                         || alreadyCheckKnownBarren.Contains(checkName)
//                     )
//                     {
//                         continue;
//                     }

//                     if (isCheckSphere0(checkName))
//                         numSphere0 += 1;
//                     total += 1;
//                 }

//                 if (total == 0)
//                     continue;

//                 double percent = (double)numSphere0 / (double)total;
//                 // zoneToPercentSphere0[zoneName] = percent;
//                 zoneNumberDataList.Add(new ZoneNumberData(zoneName, percent, total, pair.Value));
//             }

//             List<KeyValuePair<double, string>> weightedZones = new();
//             foreach (ZoneNumberData data in zoneNumberDataList)
//             {
//                 double weight = data.calculateWeight();
//                 weightedZones.Add(new(weight, data.hintZone));
//             }

//             Dictionary<string, int> counts = new();

//             int horribleAreaPickedCount = 0;

//             HashSet<string> horribleZones =
//                 new()
//                 {
//                     "Forest Temple",
//                     "Goron Mines",
//                     "Lakebed Temple",
//                     "Arbiter's Grounds",
//                     "Snowpeak Ruins",
//                     "Temple of Time",
//                     "City in the Sky",
//                     "Palace of Twilight",
//                     "Gerudo Desert",
//                     "Lake Lantern Cave"
//                 };

//             for (int i = 0; i < 500; i++)
//             {
//                 bool incrementedHorribleArea = false;
//                 VoseInstance<string> voseInst = VoseAlgorithm.createInstance(weightedZones);
//                 List<string> selectedZones = new();
//                 while (voseInst.HasMore() && selectedZones.Count < numSpolZones)
//                 {
//                     string zone = voseInst.NextAndRemove(rnd);
//                     if (!incrementedHorribleArea && horribleZones.Contains(zone))
//                     {
//                         incrementedHorribleArea = true;
//                         horribleAreaPickedCount += 1;
//                     }

//                     selectedZones.Add(zone);
//                     int count = 0;
//                     counts.TryGetValue(zone, out count);
//                     counts[zone] = count + 1;
//                 }

//                 result = selectedZones;
//             }

//             Dictionary<string, double> percents = new();
//             foreach (KeyValuePair<string, int> pair in counts)
//             {
//                 percents.Add(pair.Key, (double)pair.Value / 500);
//             }

//             // The only checks which are suggested toward are the specific
//             // checks in each zone which were selected. Just pick a random valid
//             // one from the zone.
//             List<string> selectedChecks = new();

//             foreach (string zoneName in result)
//             {
//                 string[] checks = zoneToChecksMap[zoneName];

//                 List<string> possibleCheckNames = new();

//                 foreach (string check in checks)
//                 {
//                     if (checkCanBeHintedSpol(check))
//                         possibleCheckNames.Add(check);
//                 }

//                 HintUtils.ShuffleListInPlace(rnd, possibleCheckNames);
//                 string selectedCheck = possibleCheckNames[0];
//                 selectedChecks.Add(selectedCheck);
//                 alreadyCheckDirectedToward.Add(selectedCheck);
//             }

//             HintUtils.ShuffleListInPlace(rnd, selectedChecks);

//             List<SpiritOfLightHint> hints = new();
//             foreach (string checkName in selectedChecks)
//             {
//                 string hintZone = HintUtils.checkNameToHintZone(sSettings, checkName);
//                 hints.Add(new SpiritOfLightHint(AreaId.ZoneStr(hintZone), checkName));
//             }

//             return hints;
//         }

//         private Dictionary<string, BarrenHint> genBarrenZoneHints(int numBarrenZones)
//         {
//             // Iterate over each zone.
//             // Zone must contain at least one check which is not alreadyHinted or playerKnowsStatus.
//             // All valid checks in the zone must not contain a preventBarrenItem.
//             // Keep count of the percentSphere0. If 2 zones are the same size (such as 6), better
//             // to hint the one which has a higher percentage of sphere0. However, this should have a smaller effect
//             // than the raw size of the area.

//             // An area with 16 should have a much larger chance of hinted barren than a thing with 4.
//             // Doing square root to get 4 and 2 seems like not a good ratio. Kak GY and Desert are not the same at all.

//             //
//             Dictionary<string, string[]> zoneToChecksMap = HintUtils.getHintZoneToChecksMap(
//                 sSettings
//             );

//             HashSet<string> excludedZones = new() { "Hyrule Castle" };
//             if (sSettings.barrenDungeons)
//             {
//                 // If unrequiredBarren, filter out unrequired dungeons.
//                 foreach (
//                     KeyValuePair<string, byte> kv in HintConstants.dungeonZonesToRequiredMaskMap
//                 )
//                 {
//                     string zoneName = kv.Key;
//                     if (!HintUtils.DungeonIsRequired(zoneName))
//                         excludedZones.Add(zoneName);
//                 }
//             }

//             List<KeyValuePair<string, List<string>>> potentialBarrenZones = new();

//             foreach (KeyValuePair<string, string[]> pair in zoneToChecksMap)
//             {
//                 string zoneName = pair.Key;
//                 if (excludedZones.Contains(zoneName))
//                     continue;

//                 // Filter out any zones which contain checks that are all
//                 // "Excluded" or "Excluded-Unrequired" or "Vanilla". A zone
//                 // which only contains checks with these statuses will always
//                 // contain some combination of junk and things the player
//                 // already knows. That being the case, providing a Barren hint
//                 // for such a zone would not help the player at all.
//                 // bool zoneCanBeHintedBarren = false;
//                 List<string> unknownChecks = new();
//                 // int numUnknownChecks = 0;
//                 bool zoneCanBeHintedBarren = true;

//                 string[] checkNames = pair.Value;
//                 for (int i = 0; i < checkNames.Length; i++)
//                 {
//                     string checkName = checkNames[i];
//                     if (
//                         !alreadyCheckKnownBarren.Contains(checkName)
//                         && !alreadyCheckContentsHinted.Contains(checkName)
//                         && !hintsShouldIgnoreChecks.Contains(checkName)
//                         && !HintUtils.checkIsPlayerKnownStatus(checkName)
//                     )
//                     {
//                         // Important that the empty grotto check is still
//                         // checked in here such that it prevents the zone from
//                         // being hinted barren. We do not check against
//                         // `alreadyCheckDirectedToward` since this is what the
//                         // empty grotto and SpoL checks uses and we want those
//                         // to prevent barren.
//                         Item contents = getCheckContents(checkName);
//                         if (preventBarrenItems.Contains(contents))
//                         {
//                             zoneCanBeHintedBarren = false;
//                             break;
//                         }

//                         // numUnknownChecks += 1;
//                         unknownChecks.Add(checkName);
//                     }
//                 }

//                 // if (zoneCanBeHintedBarren && numUnknownChecks > 0)
//                 if (zoneCanBeHintedBarren && unknownChecks.Count > 0)
//                 {
//                     // potentialBarrenZones.Add(new(zoneName, numUnknownChecks));
//                     potentialBarrenZones.Add(new(zoneName, unknownChecks));
//                 }
//             }

//             List<KeyValuePair<string, List<string>>> selectedZones = new();

//             if (potentialBarrenZones.Count > 0)
//             {
//                 List<KeyValuePair<string, List<string>>> zonesToPickFrom = new();
//                 List<KeyValuePair<string, List<string>>> twoOrFewerChecks = new();
//                 foreach (KeyValuePair<string, List<string>> pair in potentialBarrenZones)
//                 {
//                     if (pair.Value.Count >= 3)
//                         zonesToPickFrom.Add(pair);
//                     else
//                         twoOrFewerChecks.Add(pair);
//                 }
//                 // Shuffle so not the same order every time. zonesToPickFrom
//                 // will already be randomly picked from, so it is fine as is.
//                 HintUtils.ShuffleListInPlace(rnd, twoOrFewerChecks);

//                 // while (zonesToPickFrom.Count < numBarrenZones && twoOrFewerChecks.Count > 0)
//                 // {
//                 //     KeyValuePair<string, int> pair = twoOrFewerChecks[twoOrFewerChecks.Count - 1];
//                 //     twoOrFewerChecks.RemoveAt(twoOrFewerChecks.Count - 1);
//                 //     zonesToPickFrom.Add(pair);
//                 // }

//                 List<KeyValuePair<double, KeyValuePair<string, List<string>>>> weightedList = new();
//                 foreach (KeyValuePair<string, List<string>> pair in zonesToPickFrom)
//                 {
//                     weightedList.Add(new(barrenWeightForNumUnknownChecks(pair.Value.Count), pair));
//                 }

//                 VoseInstance<KeyValuePair<string, List<string>>> inst =
//                     VoseAlgorithm.createInstance(weightedList);
//                 while (inst.HasMore() && selectedZones.Count < numBarrenZones)
//                 {
//                     KeyValuePair<string, List<string>> pair = inst.NextAndRemove(rnd);
//                     selectedZones.Add(pair);
//                 }

//                 while (selectedZones.Count < numBarrenZones && twoOrFewerChecks.Count > 0)
//                 {
//                     KeyValuePair<string, List<string>> pair = twoOrFewerChecks[
//                         twoOrFewerChecks.Count - 1
//                     ];
//                     twoOrFewerChecks.RemoveAt(twoOrFewerChecks.Count - 1);
//                     selectedZones.Add(pair);
//                 }

//                 // Shuffle since if we have to add small zones they will always
//                 // be at the end.
//                 HintUtils.ShuffleListInPlace(rnd, selectedZones);
//             }

//             Dictionary<string, BarrenHint> hintResults = new();

//             // For each selected zone, mark all contents as already hinted.
//             foreach (KeyValuePair<string, List<string>> pair in selectedZones)
//             {
//                 hintResults[pair.Key] = new BarrenHint(AreaId.ZoneStr(pair.Key));

//                 foreach (string check in pair.Value)
//                 {
//                     alreadyCheckKnownBarren.Add(check);
//                 }

//                 // string[] checks = zoneToChecksMap[selectedZone];
//                 // foreach (string check in checks)
//                 // {
//                 //     alreadyCheckKnownBarren.Add(check);
//                 // }
//             }

//             return hintResults;
//         }

//         private List<HintSpot2> getDungeonAntiCasinoHintSpots(
//             Dictionary<string, HintAbs> dungeonZoneToOtherHint,
//             HashSet<string> hintedBarrenZones
//         )
//         {
//             // Gather required Big Key checkNames.
//             Dictionary<string, List<string>> dungeonZoneToBigKeyCheckNames = new();
//             if (
//                 sSettings.bigKeySettings == SSettings.Enums.BigKeySettings.Anywhere
//                 || sSettings.bigKeySettings == SSettings.Enums.BigKeySettings.Any_Dungeon
//             )
//             {
//                 foreach (string checkName in requiredChecks)
//                 {
//                     Item contents = getCheckContents(checkName);
//                     if (HintConstants.bigKeyToDungeonZone.ContainsKey(contents))
//                     {
//                         string zoneName = HintConstants.bigKeyToDungeonZone[contents];
//                         if (!dungeonZoneToBigKeyCheckNames.ContainsKey(zoneName))
//                         {
//                             dungeonZoneToBigKeyCheckNames[zoneName] = new();
//                         }
//                         dungeonZoneToBigKeyCheckNames[zoneName].Add(checkName);
//                     }
//                 }
//             }

//             // Create hints for Big Keys.
//             Dictionary<string, List<HintAbs>> zoneToHints = new();
//             foreach (KeyValuePair<string, List<string>> pair in dungeonZoneToBigKeyCheckNames)
//             {
//                 string zoneName = pair.Key;
//                 List<string> checkNames = pair.Value;

//                 List<HintAbs> hints = new();

//                 if (checkNames.Count > 1)
//                 {
//                     // Add to HashSet first to merge duplicates.
//                     HashSet<string> zones = new();
//                     List<string> agithaCheckNames = new();
//                     foreach (string checkName in checkNames)
//                     {
//                         zones.Add(HintUtils.checkNameToHintZone(sSettings, checkName));
//                         string zoneOfCheck = HintUtils.checkNameToHintZone(sSettings, checkName);
//                         if (zoneOfCheck == "Agitha")
//                             agithaCheckNames.Add(checkName);
//                     }
//                     List<AreaId> areaIds = new();
//                     foreach (string zone in zones)
//                     {
//                         areaIds.Add(AreaId.ZoneStr(zone));
//                     }

//                     Item item = getCheckContents(checkNames[0]);
//                     hints.Add(new ItemMultiLocationsHint(item, areaIds, true));

//                     foreach (string checkName in agithaCheckNames)
//                     {
//                         hints.Add(CheckContentsHint.Create(null, checkName));
//                     }
//                 }
//                 else if (checkNames.Count == 1)
//                 {
//                     string checkName = checkNames[0];
//                     string zoneOfCheck = HintUtils.checkNameToHintZone(sSettings, checkName);
//                     if (zoneOfCheck == "Agitha")
//                     {
//                         // Mainly added for Hyrule Castle, but seeing how it
//                         // works on the other dungeons as well.
//                         hints.Add(CheckContentsHint.Create(null, checkName));
//                     }
//                     else
//                     {
//                         Province province = HintUtils.checkNameToHintProvince(sSettings, checkName);
//                         AreaId areaId;
//                         if (province == Province.Dungeon || zoneName == "Hyrule Castle")
//                             areaId = AreaId.ZoneStr(zoneOfCheck);
//                         else
//                             areaId = AreaId.Province(province);

//                         hints.Add(
//                             new NamedItemHint(
//                                 // TypedId.Zone(zoneOfCheck),
//                                 // TypedId.Province(province),
//                                 areaId,
//                                 checkName,
//                                 getCheckContents(checkName),
//                                 true
//                             )
//                         );
//                     }
//                 }

//                 if (hints.Count > 0)
//                 {
//                     if (!zoneToHints.ContainsKey(zoneName))
//                     {
//                         zoneToHints[zoneName] = new();
//                     }
//                     zoneToHints[zoneName].AddRange(hints);
//                 }
//             }

//             // Temp disabling HC sword hints
//             // // Add Sword zones hint to Hyrule Castle hints.
//             // if (itemToChecksList.ContainsKey(Item.Progressive_Sword))
//             // {
//             //     List<string> checkNames = itemToChecksList[Item.Progressive_Sword];
//             //     if (checkNames.Count > 0)
//             //     {
//             //         string hcZoneName = "Hyrule Castle";
//             //         if (!zoneToHints.ContainsKey(hcZoneName))
//             //             zoneToHints[hcZoneName] = new();

//             //         // Add to HashSet first to merge duplicates.
//             //         HashSet<string> zones = new();
//             //         foreach (string checkName in checkNames)
//             //         {
//             //             zones.Add(HintUtils.checkNameToHintZone(sSettings, checkName));
//             //         }
//             //         List<TypedId> typedIds = new();
//             //         foreach (string zone in zones)
//             //         {
//             //             typedIds.Add(TypedId.Zone(zone));
//             //         }
//             //         zoneToHints[hcZoneName].Add(
//             //             new ItemMultiLocationsHint(Item.Progressive_Sword, typedIds, true)
//             //         );
//             //     }
//             // }

//             Dictionary<string, string[]> hintZoneToChecksMap = HintUtils.getHintZoneToChecksMap(
//                 sSettings
//             );

//             foreach (KeyValuePair<string, List<HintAbs>> pair in zoneToHints)
//             {
//                 string zoneName = pair.Key;

//                 List<HintAbs> hintsToAddToZone = new();

//                 if (!hintZoneToChecksMap.ContainsKey(zoneName))
//                     throw new Exception(
//                         $"Failed to find zone '{zoneName}' in hintZoneToChecksMap."
//                     );

//                 bool addedBarrenHint = false;
//                 if (hintedBarrenZones.Contains(zoneName))
//                 {
//                     addedBarrenHint = true;
//                     hintsToAddToZone.Add(new BarrenHint(AreaId.ZoneStr(zoneName)));
//                 }

//                 // TODO: Here is where we append the hints (if any) for that
//                 // dungeon zone string name.
//                 // if (dungeonZoneToOtherHint.ContainsKey(zoneName))
//                 // {
//                 //     HintAbs hint = dungeonZoneToOtherHint[zoneName];
//                 //     // Do not add nothingBeyond hint if zone already hinted
//                 //     // itself as barren.
//                 //     if (
//                 //         !addedBarrenHint
//                 //         || !hint.GetType().IsAssignableTo(typeof(NothingBeyondHint))
//                 //     )
//                 //         hintsToAddToZone.Add(hint);
//                 // }

//                 if (hintsToAddToZone.Count > 0)
//                 {
//                     if (!zoneToHints.ContainsKey(zoneName))
//                         zoneToHints[zoneName] = new();
//                     zoneToHints[zoneName].AddRange(hintsToAddToZone);
//                 }

//                 // Commenting out previous hints (hinted about item in dungeon).

//                 // // Shuffle copy of list so we don't have certain checks always
//                 // // taking priority over other checks. For example, check A had
//                 // // this item which was hinted, therefore check B cannot possibly
//                 // // have this item.
//                 // List<string> checkNames = new List<string>(hintZoneToChecksMap[zoneName]);
//                 // HintUtils.ShuffleListInPlace(rnd, checkNames);

//                 // // Split into lists
//                 // Dictionary<Item, string> higherPriorityDict = new();
//                 // Dictionary<Item, string> middlePriorityDict = new();
//                 // Dictionary<Item, string> lowerPriorityDict = new();

//                 // foreach (string checkName in checkNames)
//                 // {
//                 //     Item contents = getCheckContents(checkName);
//                 //     if (preventBarrenItems.Contains(contents))
//                 //     {
//                 //         // if (!requiredChecks.Contains(checkName)
//                 //         // // || alreadyCheckContentsHinted.Contains(checkName)
//                 //         // // || alreadyCheckDirectedToward.Contains(checkName)
//                 //         // // || alreadyItemHintedRequired.Contains(contents)
//                 //         // )
//                 //         // {
//                 //         //     middlePriorityDict[contents] = checkName;
//                 //         // }
//                 //         // else
//                 //         // {

//                 //         if (HintConstants.invalidSpolItems.Contains(contents))
//                 //             lowerPriorityDict[contents] = checkName;
//                 //         else
//                 //             higherPriorityDict[contents] = checkName;

//                 //         // }
//                 //     }
//                 // }

//                 // // Merge the two
//                 // List<KeyValuePair<Item, string>> higherPriorityList = higherPriorityDict.ToList();
//                 // List<KeyValuePair<Item, string>> middlePriorityList = middlePriorityDict.ToList();
//                 // List<KeyValuePair<Item, string>> lowerPriorityList = lowerPriorityDict.ToList();

//                 // HintUtils.ShuffleListInPlace(rnd, higherPriorityList);
//                 // HintUtils.ShuffleListInPlace(rnd, middlePriorityList);
//                 // HintUtils.ShuffleListInPlace(rnd, lowerPriorityList);

//                 // List<KeyValuePair<Item, string>> combined = new List<KeyValuePair<Item, string>>()
//                 //     .Concat(higherPriorityList)
//                 //     .Concat(middlePriorityList)
//                 //     .Concat(lowerPriorityList)
//                 //     .ToList();

//                 // if (combined.Count > 0)
//                 // {
//                 //     // Pick random key from dictionary
//                 //     KeyValuePair<Item, string> selected = combined[0];

//                 //     // Hint that certain item is in the dungeon.
//                 //     NamedItemHint hint = new NamedItemHint(
//                 //         TypedId.Zone(zoneName),
//                 //         selected.Value,
//                 //         selected.Key,
//                 //         true
//                 //     );
//                 //     pair.Value.Add(hint);
//                 // }
//                 // else
//                 // {
//                 //     // Hint that nothing is in the dungeon.
//                 //     pair.Value.Add(new BarrenHint(TypedId.Zone(zoneName)));
//                 // }
//             }

//             // Build hintSpots
//             List<HintSpot2> hintSpots = new();
//             foreach (KeyValuePair<string, List<HintAbs>> pair in zoneToHints)
//             {
//                 string hintZone = pair.Key;
//                 if (HintConstants.dungeonZoneToSpotLocation.ContainsKey(hintZone))
//                 {
//                     HintSpot2 spot = new HintSpot2(
//                         HintConstants.dungeonZoneToSpotLocation[hintZone]
//                     );
//                     spot.hints.AddRange(pair.Value);
//                     hintSpots.Add(spot);
//                 }
//             }

//             return hintSpots;
//         }

//         private double barrenWeightForNumUnknownChecks(int numUnknownChecks)
//         {
//             if (numUnknownChecks <= 2)
//             {
//                 // This code is not expected to ever run. We have it here just
//                 // in case.
//                 return 0.1;
//             }
//             else if (numUnknownChecks <= 5 || numUnknownChecks >= 12)
//             {
//                 return numUnknownChecks;
//             }
//             else if (numUnknownChecks <= 7)
//             {
//                 return 7;
//             }
//             else if (numUnknownChecks >= 9)
//             {
//                 return 9;
//             }

//             // return 10 * Math.Pow(numUnknownChecks, 0.5);
//             // return numUnknownChecks;
//             return 7;
//         }

//         private HintSpot2 getAgithaHintSpot()
//         {
//             int numBugsInPool = 0;
//             List<string> interestingAgithaChecks = new();
//             List<Item> items = new();

//             foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
//             {
//                 string agithaRewardCheckName = pair.Value;
//                 hintsShouldIgnoreChecks.Add(agithaRewardCheckName);

//                 // If not included, skip over it
//                 if (HintUtils.checkIsPlayerKnownStatus(agithaRewardCheckName))
//                     continue;

//                 numBugsInPool += 1;

//                 Item contents = getCheckContents(agithaRewardCheckName);
//                 if (
//                     preventBarrenItems.Contains(contents)
//                     && !HintConstants.bugsToRewardChecksMap.ContainsKey(contents)
//                 )
//                 {
//                     // Interesting contents which are not a bug.
//                     interestingAgithaChecks.Add(agithaRewardCheckName);
//                     items.Add(getCheckContents(agithaRewardCheckName));
//                 }

//                 // if item is preventBarren and not a bug, then add to the list

//                 // Determine if there is anything interesting on Agitha. If
//                 // there isn't, then she is considered dead and bugs should not
//                 // prevent barren.

//                 // IMPORTANT: if Agitha has nothing, then bugs should not
//                 // prevent barren.
//             }

//             if (interestingAgithaChecks.Count < 1)
//             {
//                 // Bugs should no longer prevent barren.
//                 foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
//                 {
//                     preventBarrenItems.Remove(pair.Key);
//                 }
//             }

//             if (numBugsInPool < 1)
//                 return null;

//             // TODO: sort items list alphabetically. This should be done in hint
//             // implementation.
//             AgithaRewardsHint hint = new AgithaRewardsHint(
//                 numBugsInPool,
//                 interestingAgithaChecks,
//                 items
//             );

//             HintSpot2 spot = new HintSpot2(HintSpotLocation.Agithas_Castle_Sign);
//             spot.hints.Add(hint);
//             return spot;
//         }

//         private BugsUsefulHint genBarrenBugGenderHint(HintedThings hintedThings)
//         {
//             HashSet<Item> interestingMaleBugs = new();
//             HashSet<Item> interestingFemaleBugs = new();
//             bool hasInterestingBugNotHintedToward = false;

//             foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
//             {
//                 Item bug = pair.Key;
//                 string agithaRewardCheckName = pair.Value;

//                 if (itemToChecksList.ContainsKey(bug))
//                 {
//                     List<string> checkNames = itemToChecksList[bug];
//                     if (checkNames == null || checkNames.Count < 1)
//                         continue;

//                     // Check if what you trade the bug for is interesting.
//                     if (!preventBarrenItems.Contains(getCheckContents(agithaRewardCheckName)))
//                         continue;

//                     if (HintUtils.isItemMaleBug(bug))
//                         interestingMaleBugs.Add(bug);
//                     else if (HintUtils.isItemFemaleBug(bug))
//                         interestingFemaleBugs.Add(bug);
//                     else
//                         continue;

//                     // If all required bugs are already hinted by name, no
//                     // reason for this hint. For example, only 1 bug leads to
//                     // something and Male Mantis already hinted as a required
//                     // item.

//                     // Potentially should not give a hint in the case that every
//                     // interesting bug is already directed toward with a path
//                     // hint. Leaving for now though since those path hints do
//                     // not list the bugs by name (bug listed required by name in
//                     // a hint is already covered in places).
//                     if (
//                         !alreadyItemHintedRequired.Contains(bug)
//                         && (
//                             hintedThings == null
//                             || !hintedThings.alreadyItemHintedRequired.Contains(bug)
//                         )
//                     )
//                         hasInterestingBugNotHintedToward = true;
//                 }
//             }

//             bool hasInterestingMaleBugReward = interestingMaleBugs.Count > 0;
//             bool hasInterestingFemaleBugReward = interestingFemaleBugs.Count > 0;

//             // If has one not directed to and all interesting bugs are the same
//             // gender.
//             if (
//                 hasInterestingBugNotHintedToward
//                 && hasInterestingMaleBugReward != hasInterestingFemaleBugReward
//             )
//             {
//                 HashSet<Item> newIndirectlyPointedTo = new();
//                 BugsUsefulHint hint = null;

//                 if (hasInterestingFemaleBugReward)
//                 {
//                     newIndirectlyPointedTo = interestingFemaleBugs;
//                     hint = new BugsUsefulHint(BugsUsefulHint.BugType.Male, false);
//                 }
//                 else if (hasInterestingMaleBugReward)
//                 {
//                     newIndirectlyPointedTo = interestingMaleBugs;
//                     hint = new BugsUsefulHint(BugsUsefulHint.BugType.Female, false);
//                 }

//                 if (hint != null)
//                 {
//                     foreach (Item bug in newIndirectlyPointedTo)
//                     {
//                         if (!hintedThings.alreadyItemHintedRequired.Contains(bug))
//                             hintedThings.bugsIndirectlyPointedTo.Add(bug);
//                     }
//                     return hint;
//                 }
//             }

//             return null;
//         }

//         private List<Item> getHardRequiredBugs()
//         {
//             HashSet<Item> requiredBugs = new();
//             foreach (string checkName in requiredChecks)
//             {
//                 Item item = getCheckContents(checkName);
//                 if (HintConstants.bugsToRewardChecksMap.ContainsKey(item))
//                     requiredBugs.Add(item);
//             }
//             return requiredBugs.ToList();
//         }

//         private NumItemInAreaHint genNumItemInDungeonsHint(Item item)
//         {
//             // Iterate over all checks
//             // For each one which is a sword, determine if check is in a dungeon or not.
//             // Post-dungeon checks are not in a dungeon.
//             int numItemInDungeons = 0;

//             Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//                 sSettings
//             );

//             foreach (KeyValuePair<string, string> pair in checkToHintZoneMap)
//             {
//                 Item contents = getCheckContents(pair.Key);
//                 if (contents == item)
//                 {
//                     if (HintUtils.hintZoneIsDungeon(pair.Value))
//                     {
//                         numItemInDungeons += 1;
//                     }
//                 }
//             }

//             return new NumItemInAreaHint(
//                 numItemInDungeons,
//                 item,
//                 AreaId.Category(HintCategory.Dungeon)
//             );
//         }

//         private NumItemInAreaHint genNumClawsInDungeonsHint(HintedThings hintedThings)
//         {
//             if (!itemToChecksList.ContainsKey(Item.Progressive_Clawshot))
//                 return null;

//             List<string> checkNames = itemToChecksList[Item.Progressive_Clawshot];
//             if (checkNames == null)
//                 return null;

//             foreach (string checkName in checkNames)
//             {
//                 if (
//                     !alreadyCheckContentsHinted.Contains(checkName)
//                     && !alreadyCheckDirectedToward.Contains(checkName)
//                     && !hintedThings.alreadyCheckContentsHinted.Contains(checkName)
//                     && !hintedThings.alreadyCheckDirectedToward.Contains(checkName)
//                 )
//                     return genNumItemInDungeonsHint(Item.Progressive_Clawshot);
//             }

//             return null;
//         }

//         private class CategoryObject
//         {
//             public HintCategory category { get; }
//             public List<string> validChecks { get; }
//             public List<string> usefulChecks { get; }
//             public bool hasUseful
//             {
//                 get { return usefulChecks.Count > 0; }
//             }

//             public CategoryObject(
//                 HintCategory category,
//                 List<string> validChecks,
//                 List<string> usefulChecks
//             )
//             {
//                 this.category = category;
//                 this.validChecks = validChecks;
//                 this.usefulChecks = usefulChecks;
//             }

//             public HintAbs toHint(Func<string, Item> checkNameToContents)
//             {
//                 if (usefulChecks.Count > 0)
//                 {
//                     string checkName = usefulChecks[0];
//                     Item item = checkNameToContents(checkName);
//                     return new SomethingGoodHint(AreaId.Category(category), checkName, item);
//                 }
//                 else
//                     return new BarrenHint(AreaId.Category(category));
//             }
//         }

//         // private Dictionary<string, bool> genCategoryHints()
//         private List<CategoryObject> genCategoryHints(
//             HintedThings hintedThings,
//             HashSet<HintCategory> categories
//         )
//         {
//             List<CategoryObject> categoryGoodList = new();

//             if (categories == null || categories.Count < 1)
//                 return categoryGoodList;

//             // foreach (KeyValuePair<string, string[]> pair in HintCategory.categoryToChecksMap)
//             foreach (HintCategory category in categories)
//             {
//                 if (!HintCategoryUtils.categoryToChecksMap.ContainsKey(category))
//                     continue;

//                 string[] checks = HintCategoryUtils.categoryToChecksMap[category];

//                 List<string> validChecksInCategory = new();
//                 List<string> usefulChecks = new();

//                 foreach (string checkName in checks)
//                 {
//                     if (
//                         HintUtils.checkIsPlayerKnownStatus(checkName)
//                         || alreadyCheckKnownBarren.Contains(checkName)
//                         || alreadyCheckContentsHinted.Contains(checkName)
//                     )
//                         continue;

//                     if (
//                         hintedThings != null
//                         && (
//                             hintedThings.alreadyCheckKnownBarren.Contains(checkName)
//                             || hintedThings.alreadyCheckContentsHinted.Contains(checkName)
//                         )
//                     )
//                         continue;

//                     // canHint = true;
//                     Item contents = getCheckContents(checkName);
//                     if (preventBarrenItems.Contains(contents))
//                     {
//                         usefulChecks.Add(checkName);
//                         // break;
//                     }

//                     validChecksInCategory.Add(checkName);
//                 }

//                 if (validChecksInCategory.Count > 0)
//                 {
//                     categoryGoodList.Add(
//                         new CategoryObject(category, validChecksInCategory, usefulChecks)
//                     );
//                 }
//             }

//             // Order according to weights:

//             // ordered list of name and good
//             List<KeyValuePair<double, CategoryObject>> weightedList = new();

//             foreach (CategoryObject obj in categoryGoodList)
//             {
//                 double weight = 1;

//                 // weight = 5;
//                 // if (!obj.hasUseful && obj.numValidChecks >= 7)
//                 if (!obj.hasUseful)
//                     weight = Math.Log(obj.validChecks.Count) + 3;
//                 else
//                     weight = 1 + Math.PI / 2 - Math.Atan(0.5 * (obj.validChecks.Count - 6));

//                 weightedList.Add(new(weight, obj));
//             }

//             VoseInstance<CategoryObject> inst = VoseAlgorithm.createInstance(weightedList);

//             List<CategoryObject> orderedList = new();

//             while (inst.HasMore())
//             {
//                 orderedList.Add(inst.NextAndRemove(rnd));
//             }

//             // If the first desert hasUseful and the 2nd does not, swap their
//             // positions since not hasUseful is the more useful hint.
//             int firstDesertObjIndex = -1;
//             int secondDesertObjIndex = -1;

//             for (int i = 0; i < orderedList.Count; i++)
//             {
//                 CategoryObject obj = orderedList[i];
//                 if (
//                     obj.category == HintCategory.Upper_Desert
//                     || obj.category == HintCategory.Lower_Desert
//                 )
//                 {
//                     if (firstDesertObjIndex < 0)
//                     {
//                         firstDesertObjIndex = i;
//                     }
//                     else if (secondDesertObjIndex < 0)
//                     {
//                         secondDesertObjIndex = i;
//                         break;
//                     }
//                 }
//             }

//             if (firstDesertObjIndex >= 0 && secondDesertObjIndex >= 0)
//             {
//                 CategoryObject firstDesertObj = orderedList[firstDesertObjIndex];
//                 CategoryObject secondDesertObj = orderedList[secondDesertObjIndex];

//                 if (firstDesertObj.hasUseful && !secondDesertObj.hasUseful)
//                 {
//                     orderedList[firstDesertObjIndex] = secondDesertObj;
//                     orderedList[secondDesertObjIndex] = firstDesertObj;
//                 }
//             }

//             return orderedList;
//         }

//         private List<CategoryObject> pullOutHintsAndMutateList(
//             List<CategoryObject> listIn,
//             List<HintCategory> categoriesToPull
//         )
//         {
//             List<CategoryObject> pulled = new();

//             foreach (HintCategory category in categoriesToPull)
//             {
//                 for (int i = 0; i < listIn.Count; i++)
//                 {
//                     CategoryObject obj = listIn[i];
//                     if (obj.category == category)
//                     {
//                         // remove and break;
//                         pulled.Add(obj);
//                         listIn.RemoveAt(i);
//                         break;
//                     }
//                 }
//             }

//             return pulled;
//         }

//         private bool checkCanBeItemHinted(string checkName, HintedThings hintedThings)
//         {
//             Item contents = getCheckContents(checkName);
//             if (
//                 hintsShouldIgnoreChecks.Contains(checkName)
//                 || HintUtils.checkIsPlayerKnownStatus(checkName)
//                 || alreadyCheckContentsHinted.Contains(checkName)
//                 || alreadyCheckDirectedToward.Contains(checkName)
//                 || alreadyCheckKnownBarren.Contains(checkName)
//                 || bugsIndirectlyPointedTo.Contains(contents)
//             )
//             {
//                 return false;
//             }

//             if (hintedThings != null)
//             {
//                 if (
//                     hintedThings.alreadyCheckContentsHinted.Contains(checkName)
//                     || hintedThings.alreadyCheckContentsHinted.Contains(checkName)
//                     || hintedThings.alreadyCheckDirectedToward.Contains(checkName)
//                     || hintedThings.alreadyCheckKnownBarren.Contains(checkName)
//                     || hintedThings.bugsIndirectlyPointedTo.Contains(contents)
//                 )
//                 {
//                     return false;
//                 }
//             }

//             return true;
//         }

//         private List<NamedItemHint> genItemHintList(
//             List<Item> itemsToHint,
//             HintedThings hintedThings
//         )
//         {
//             List<NamedItemHint> result = new();

//             foreach (Item item in itemsToHint)
//             {
//                 if (!itemToChecksList.ContainsKey(item))
//                     continue;

//                 List<string> checkNames = itemToChecksList[item];
//                 if (checkNames.Count < 1)
//                 {
//                     continue;
//                 }
//                 else if (checkNames.Count == 1)
//                 {
//                     string checkName = checkNames[0];
//                     if (!checkCanBeItemHinted(checkName, hintedThings))
//                         continue;

//                     Province province = HintUtils.checkNameToHintProvince(sSettings, checkName);
//                     if (province == Province.Dungeon)
//                     {
//                         string zoneName = HintUtils.checkNameToHintZone(sSettings, checkName);
//                         result.Add(
//                             new NamedItemHint(
//                                 AreaId.ZoneStr(zoneName),
//                                 checkName,
//                                 getCheckContents(checkName),
//                                 true
//                             )
//                         );
//                     }
//                     else if (province != Province.Invalid)
//                     {
//                         result.Add(
//                             new NamedItemHint(
//                                 AreaId.Province(province),
//                                 checkName,
//                                 getCheckContents(checkName),
//                                 true
//                             )
//                         );
//                     }
//                 }
//                 else
//                 {
//                     // Zone hint when more than one of the item.
//                     List<string> combined = new();
//                     List<string> sphere0Checks = new();
//                     List<string> sphereLaterChecks = new();

//                     foreach (string checkName in checkNames)
//                     {
//                         if (!checkCanBeItemHinted(checkName, hintedThings))
//                             continue;

//                         if (playthroughSpheres.sphere0Checks.Contains(checkName))
//                             sphere0Checks.Add(checkName);
//                         else
//                             sphereLaterChecks.Add(checkName);
//                     }

//                     HintUtils.ShuffleListInPlace(rnd, sphere0Checks);
//                     HintUtils.ShuffleListInPlace(rnd, sphereLaterChecks);

//                     if (rnd.NextDouble() < 0.6)
//                     {
//                         // Prefer sphereLater
//                         combined = new List<string>()
//                             .Concat(sphereLaterChecks)
//                             .Concat(sphere0Checks)
//                             .ToList();
//                     }
//                     else
//                     {
//                         combined = new List<string>()
//                             .Concat(sphere0Checks)
//                             .Concat(sphereLaterChecks)
//                             .ToList();
//                     }

//                     if (combined.Count <= 0)
//                         continue;

//                     string selectedCheckName = combined[0];
//                     string hintZone = HintUtils.getCheckToHintZoneMap(sSettings)[selectedCheckName];
//                     result.Add(
//                         new NamedItemHint(
//                             AreaId.ZoneStr(hintZone),
//                             selectedCheckName,
//                             getCheckContents(selectedCheckName),
//                             true
//                         )
//                     );
//                 }
//             }

//             HintUtils.ShuffleListInPlace(rnd, result);

//             foreach (NamedItemHint hint in result)
//             {
//                 tryMarkAlreadyKnownFromHint(hint, hintedThings);
//             }

//             return result;
//         }

//         private static KeyValuePair<T, R> RemoveFirstItemFromDictionary<T, R>(
//             Dictionary<T, R> dictionary
//         )
//         {
//             if (dictionary.Count < 1)
//                 throw new IndexOutOfRangeException(
//                     "Failed to remove first item from empty dictionary"
//                 );

//             KeyValuePair<T, R> retPair = default(KeyValuePair<T, R>);
//             foreach (KeyValuePair<T, R> pair in dictionary)
//             {
//                 retPair = pair;
//                 break;
//             }
//             dictionary.Remove(retPair.Key);

//             return retPair;
//         }

//         class ZoneNumberData
//         {
//             public string hintZone;
//             public double percentSphere0;
//             public int numChecks;
//             public bool hasSphereLater;

//             public ZoneNumberData(
//                 string hintZone,
//                 double percentSphere0,
//                 int numChecks,
//                 bool hasSphereLater
//             )
//             {
//                 this.hintZone = hintZone;
//                 this.percentSphere0 = percentSphere0;
//                 this.numChecks = numChecks;
//                 this.hasSphereLater = hasSphereLater;
//             }

//             public double calculateWeight()
//             {
//                 if (numChecks >= 12)
//                 {
//                     return 1;
//                 }
//                 else if (numChecks <= 2)
//                 {
//                     return 6;
//                 }

//                 double val = 10;

//                 double extra = 5 * Math.Pow(1 - percentSphere0, 2);

//                 // if not all sL and has sL, should get a boost.
//                 // The boost should be really significant for 3 check areas,
//                 // and not as much for 11 check areas.

//                 if (percentSphere0 > 0 && hasSphereLater)
//                 {
//                     double moreExtra = 3 * Math.Pow(100 * ((double)3 / numChecks) + 1, 0.25);
//                     val += moreExtra;
//                 }

//                 val += extra;
//                 val += 0;

//                 // else if (numChecks <= 5)
//                 // {
//                 //     return 10;
//                 // }
//                 // else if (numChecks <= 7)
//                 // {
//                 //     return 10;
//                 // }
//                 // else
//                 // {
//                 //     return 10;
//                 // }

//                 // if (hasSphereLater)
//                 // {
//                 //     if (percentSphere0 > 0)
//                 //     {
//                 //         return 6;
//                 //     }
//                 //     return 5;
//                 // }

//                 // return 3;
//                 return val;
//             }
//         }

//         private bool checkCanBeSometimesHinted(string checkName)
//         {
//             // We should ignore checks which are directed toward. We may want to
//             // not hint toward any checks in a zone which is SpoL? This was the
//             // previous behavior. Probably fine to list them in a SpoL zone.
//             // This way you can know that specific check wasn't the SpoL one.
//             // Can adjust later if doesn't make sense.
//             return !alreadyCheckKnownBarren.Contains(checkName)
//                 && !alreadyCheckContentsHinted.Contains(checkName)
//                 && !alreadyCheckDirectedToward.Contains(checkName)
//                 && !hintsShouldIgnoreChecks.Contains(checkName)
//                 && !HintUtils.checkIsPlayerKnownStatus(checkName);
//         }

//         private string pickBoOrGoatsCheck()
//         {
//             List<string> boAndGoatsOptions = new() { "Herding Goats Reward", "Wrestling With Bo", };
//             List<string> filteredList = new();
//             foreach (string checkName in boAndGoatsOptions)
//             {
//                 if (checkCanBeSometimesHinted(checkName))
//                     filteredList.Add(checkName);
//             }

//             if (filteredList.Count < 1)
//                 return null;
//             else if (filteredList.Count == 1 || rnd.NextDouble() < 0.5)
//                 return filteredList[0];
//             else
//                 return filteredList[1];
//         }

//         private List<CheckContentsHint> genSometimesCheckList(NumItemInAreaHint swordProvinceHint)
//         {
//             HashSet<string> addedToList = new();
//             List<string> orderedCheckNames = new();

//             // Gen sword hint
//             if (swordProvinceHint != null)
//             {
//                 Province avoidProvince = (Province)ProvinceUtils.StringToId(
//                     swordProvinceHint.areaId.stringId
//                 );

//                 List<KeyValuePair<string, int>> swordChecksList = new();
//                 List<string> checkNames = itemToChecksList[Item.Progressive_Sword];
//                 foreach (string checkName in checkNames)
//                 {
//                     Province province = HintUtils.checkNameToHintProvince(sSettings, checkName);
//                     if (province != avoidProvince && checkCanBeSometimesHinted(checkName))
//                     {
//                         // For more complex priority, can use a bitmask. 0b100
//                         // is a higher priority groupBy than 0b10 than 0b1. Can
//                         // potentially break this out into a utility method.
//                         swordChecksList.Add(new(checkName, isCheckSphere0(checkName) ? 0 : 1));
//                     }
//                 }

//                 if (swordChecksList.Count > 0)
//                 {
//                     swordChecksList.Sort(
//                         (a, b) =>
//                         {
//                             return b.Value - a.Value;
//                         }
//                     );
//                     int largestVal = swordChecksList[0].Value;
//                     int lastIndex = swordChecksList.FindLastIndex(
//                         (pair) => pair.Value == largestVal
//                     );
//                     int randomIndex = rnd.Next(lastIndex + 1);
//                     string selectedCheck = swordChecksList[randomIndex].Key;

//                     orderedCheckNames.Add(selectedCheck);
//                     addedToList.Add(selectedCheck);
//                 }
//             }

//             string citsFirstWestChest = "City in The Sky West Wing First Chest";
//             if (
//                 !addedToList.Contains(citsFirstWestChest)
//                 && checkCanBeSometimesHinted(citsFirstWestChest)
//                 && rnd.NextDouble() < 0.5
//             )
//             {
//                 // 50% chance of including this hint when possible to
//                 orderedCheckNames.Add(citsFirstWestChest);
//                 addedToList.Add(citsFirstWestChest);
//             }

//             string boOrGoatsCheck = pickBoOrGoatsCheck();
//             if (boOrGoatsCheck != null)
//             {
//                 orderedCheckNames.Add(boOrGoatsCheck);
//                 addedToList.Add(boOrGoatsCheck);
//             }

//             // Make sure check can be hinted.

//             // Iterate through the heart containers first, then filter to any
//             // that are included and any that are interesting.

//             List<string> heartContainerCheckNames =
//                 new()
//                 {
//                     "Forest Temple Diababa Heart Container",
//                     "Goron Mines Fyrus Heart Container",
//                     "Lakebed Temple Morpheel Heart Container",
//                     "Snowpeak Ruins Blizzeta Heart Container",
//                     "Temple of Time Armogohma Heart Container",
//                     "City in The Sky Argorok Heart Container",
//                 };

//             List<string> interestingHeartContainerChecks = new();

//             foreach (string checkName in heartContainerCheckNames)
//             {
//                 if (
//                     !addedToList.Contains(checkName)
//                     && checkCanBeSometimesHinted(checkName)
//                     && preventBarrenItems.Contains(getCheckContents(checkName))
//                 )
//                 {
//                     interestingHeartContainerChecks.Add(checkName);
//                 }
//             }

//             HintUtils.ShuffleListInPlace(rnd, interestingHeartContainerChecks);
//             foreach (string checkName in interestingHeartContainerChecks)
//             {
//                 orderedCheckNames.Add(checkName);
//                 addedToList.Add(checkName);
//             }

//             List<string> sometimesChecks =
//                 new()
//                 {
//                     // "Links Basement Chest", // This is just a worse ranch grotto hint
//                     // Since both Ordon lantern checks are together, hinting one
//                     // dead just makes you less likely to check the other one
//                     // which could actually be unhelpful if it mattered.
//                     // "Ordon Ranch Grotto Lantern Chest",
//                     "Ordon Cat Rescue",
//                     "Lost Woods Lantern Chest",
//                     // "Sacred Grove Baba Serpent Grotto Chest", // takes slot for better hints
//                     "Sacred Grove Spinner Chest",
//                     "Faron Field Bridge Chest",
//                     // "Faron Woods Owl Statue Chest",
//                     "Eldin Lantern Cave Lantern Chest",
//                     "Kakariko Gorge Double Clawshot Chest",
//                     // "Kakariko Gorge Owl Statue Chest",
//                     "Eldin Spring Underwater Chest",
//                     // "Kakariko Village Bomb Rock Spire Heart Piece", // takes slot for better hints
//                     // "Kakariko Village Malo Mart Hawkeye",
//                     // "Kakariko Watchtower Alcove Chest", // not needed
//                     // "Talo Sharpshooting",
//                     "Gift From Ralis",
//                     "Kakariko Graveyard Lantern Chest",
//                     "Eldin Field Bomb Rock Chest",
//                     "Eldin Field Bomskit Grotto Lantern Chest",
//                     "Eldin Stockcave Lantern Chest",
//                     "Death Mountain Alcove Chest",
//                     // "Death Mountain Trail Poe",
//                     "Lanayru Field Behind Gate Underwater Chest",
//                     "Lanayru Field Skulltula Grotto Chest",
//                     "Lanayru Field Spinner Track Chest",
//                     // "Hyrule Field Amphitheater Owl Statue Chest",
//                     // "Outside South Castle Town Double Clawshot Chasm Chest", // no point
//                     "Outside South Castle Town Fountain Chest",
//                     "Outside South Castle Town Tightrope Chest",
//                     // "West Hyrule Field Helmasaur Grotto Chest", // takes away from better ones
//                     "STAR Prize 2",
//                     // "Lake Hylia Bridge Owl Statue Chest",
//                     // "Lake Hylia Bridge Vines Chest", // not great
//                     // "Lake Hylia Shell Blade Grotto Chest",
//                     "Lake Hylia Underwater Chest",
//                     // "Lake Hylia Water Toadpoli Grotto Chest", // not great
//                     // "Plumm Fruit Balloon Minigame",
//                     "Lanayru Spring Back Room Lantern Chest",
//                     "Zoras Domain Extinguish All Torches Chest",
//                     "Zoras Domain Light All Torches Chest",
//                     "Zoras Domain Underwater Goron",
//                     "Fishing Hole Bottle",
//                     // "Gerudo Desert Owl Statue Chest",
//                     "Gerudo Desert Rock Grotto Lantern Chest",
//                     "Outside Arbiters Grounds Lantern Chest",
//                     // "Snowboard Racing Prize",
//                     "Snowpeak Cave Ice Lantern Chest",
//                     "Forest Temple Gale Boomerang",
//                     "Lakebed Temple Deku Toad Chest",
//                     "Arbiters Grounds Death Sword Chest",
//                     "Snowpeak Ruins Chapel Chest",
//                     // "Temple of Time Lobby Lantern Chest", // never great when it shows up
//                     "City in The Sky Aeralfos Chest",
//                 };

//             HintUtils.ShuffleListInPlace(rnd, sometimesChecks);
//             foreach (string checkName in sometimesChecks)
//             {
//                 if (!addedToList.Contains(checkName) && checkCanBeSometimesHinted(checkName))
//                     orderedCheckNames.Add(checkName);
//             }

//             List<CheckContentsHint> hints = new();
//             foreach (string checkName in orderedCheckNames)
//             {
//                 CheckContentsHint hint = CheckContentsHint.Create(null, checkName);
//                 hints.Add(hint);
//             }

//             return hints;
//         }

//         // Mark info as known for variable hints only once they are for sure
//         // selected. The known info from these hints is used to avoid double
//         // hinting some things with sometimes hints.
//         private void tryMarkAlreadyKnownFromHint(HintAbs hint, HintedThings hintedThings = null)
//         {
//             if (hint == null)
//                 return;

//             switch (hint.type)
//             {
//                 case HintNewType.Barren:
//                     BarrenHint barrenHint = (BarrenHint)hint;
//                     if (barrenHint.areaId.type == AreaId.AreaType.Category)
//                     {
//                         HintCategory category = HintCategoryUtils.StringToId(
//                             barrenHint.areaId.stringId
//                         );
//                         string[] checkNames = HintCategoryUtils.categoryToChecksMap[category];
//                         foreach (string checkName in checkNames)
//                         {
//                             if (hintedThings != null)
//                                 hintedThings.alreadyCheckKnownBarren.Add(checkName);
//                             else
//                                 alreadyCheckKnownBarren.Add(checkName);
//                         }
//                     }
//                     break;
//                 case HintNewType.NamedItem:
//                     NamedItemHint itemHint = (NamedItemHint)hint;
//                     if (hintedThings != null)
//                     {
//                         hintedThings.alreadyItemHintedRequired.Add(itemHint.item);
//                         hintedThings.alreadyCheckDirectedToward.Add(itemHint.checkName);
//                     }
//                     else
//                     {
//                         alreadyItemHintedRequired.Add(itemHint.item);
//                         alreadyCheckDirectedToward.Add(itemHint.checkName);
//                     }
//                     break;
//                 case HintNewType.SomethingGood:
//                     SomethingGoodHint somethingGoodHint = (SomethingGoodHint)hint;
//                     if (hintedThings != null)
//                         hintedThings.alreadyCheckDirectedToward.Add(somethingGoodHint.checkName);
//                     else
//                         alreadyCheckDirectedToward.Add(somethingGoodHint.checkName);
//                     break;
//                 case HintNewType.BugsUseful:
//                     BugsUsefulHint bugsUsefulHint = (BugsUsefulHint)hint;

//                     HashSet<Item> bugsPointedTo = new();
//                     if (!bugsUsefulHint.isUseful)
//                     {
//                         if (bugsUsefulHint.bugType == BugsUsefulHint.BugType.Female)
//                         {
//                             bugsPointedTo = preventBarrenItems
//                                 .Where(item => HintUtils.isItemMaleBug(item))
//                                 .ToHashSet();
//                         }
//                         else if (bugsUsefulHint.bugType == BugsUsefulHint.BugType.Male)
//                         {
//                             bugsPointedTo = preventBarrenItems
//                                 .Where(item => HintUtils.isItemFemaleBug(item))
//                                 .ToHashSet();
//                         }
//                     }

//                     HashSet<Item> targetSet =
//                         hintedThings != null
//                             ? hintedThings.bugsIndirectlyPointedTo
//                             : bugsIndirectlyPointedTo;
//                     foreach (Item item in bugsPointedTo)
//                     {
//                         targetSet.Add(item);
//                     }
//                     break;
//                 case HintNewType.ItemToItemPath:
//                     ItemToItemPathHint itemToItemPathHint = (ItemToItemPathHint)hint;

//                     // If srcItem or destItem is a bug, mark bug as known required.
//                     if (HintConstants.bugsToRewardChecksMap.ContainsKey(itemToItemPathHint.srcItem))
//                     {
//                         if (hintedThings != null)
//                             hintedThings.alreadyItemHintedRequired.Add(itemToItemPathHint.srcItem);
//                         else
//                             alreadyItemHintedRequired.Add(itemToItemPathHint.srcItem);
//                     }
//                     if (HintConstants.bugsToRewardChecksMap.ContainsKey(itemToItemPathHint.tgtItem))
//                     {
//                         if (hintedThings != null)
//                             hintedThings.alreadyItemHintedRequired.Add(itemToItemPathHint.tgtItem);
//                         else
//                             alreadyItemHintedRequired.Add(itemToItemPathHint.tgtItem);
//                     }

//                     // Need to mark as directed toward so Sometimes hints don't
//                     // make this hint pointless. Had this happen: sign said "IB
//                     // to Memo" and the sign's 2nd hint was "Lanayru Field IB
//                     // check is Memo".
//                     if (hintedThings != null)
//                         hintedThings.alreadyCheckDirectedToward.Add(
//                             itemToItemPathHint.tgtCheckName
//                         );
//                     else
//                         alreadyCheckDirectedToward.Add(itemToItemPathHint.tgtCheckName);

//                     break;
//             }
//         }

//         private Dictionary<string, HintAbs> genDungeonToBeyondDeadMap()
//         {
//             Dictionary<string, HintAbs> dungeonZoneToBeyondDeadHint = new();
//             return dungeonZoneToBeyondDeadHint;

//             // Dictionary<string, HintCategory> zoneToCategoryEnum =
//             //     new()
//             //     {
//             //         { "Forest Temple", HintCategory.Forest_Temple_West_Wing },
//             //         { "Goron Mines", HintCategory.Goron_Mines_2nd_Part },
//             //         { "Lakebed Temple", HintCategory.Lakebed_Temple_2nd_Wing },
//             //         { "Arbiter's Grounds", HintCategory.Arbiters_Grounds_2nd_Half },
//             //         { "Snowpeak Ruins", HintCategory.Snowpeak_Ruins_2nd_Floor },
//             //         { "Temple of Time", HintCategory.Temple_of_Time_End_Part },
//             //         { "City in the Sky", HintCategory.City_in_the_Sky_East_Wing },
//             //         // { "Palace of Twilight", Province.Dungeon },
//             //         // { "Hyrule Castle", Province.Dungeon },
//             //     };

//             // foreach (KeyValuePair<string, HintCategory> pair in zoneToCategoryEnum)
//             // {
//             //     List<CategoryObject> nothingBeyondList = genCategoryHints(
//             //         null,
//             //         new() { pair.Value }
//             //     );
//             //     if (nothingBeyondList.Count > 0)
//             //     {
//             //         CategoryObject obj = nothingBeyondList[0];
//             //         if (!obj.hasUseful)
//             //         {
//             //             dungeonZoneToBeyondDeadHint[pair.Key] = new NothingBeyondHint();

//             //             // Mark checks in this category as knownBarren.
//             //             string[] checks = HintCategoryUtils.categoryToChecksMap[obj.category];
//             //             foreach (string checkName in checks)
//             //             {
//             //                 alreadyCheckKnownBarren.Add(checkName);
//             //             }
//             //         }
//             //     }
//             // }

//             // // if (zoneIsBarren("Hyrule Castle"))
//             // //     dungeonZoneToBeyondDeadHint["Hyrule Castle"] = new NothingBeyondHint();

//             // return dungeonZoneToBeyondDeadHint;
//         }

//         private NumItemInAreaHint genProvinceWithMostItemHint(Item item)
//         {
//             if (!itemToChecksList.ContainsKey(item))
//                 return null;

//             // For each province, assign to a dictionary of stringId to

//             Dictionary<Province, int> provinceToCount = new();

//             List<string> checkNames = itemToChecksList[item];
//             foreach (string checkName in checkNames)
//             {
//                 Province province = HintUtils.checkNameToHintProvince(sSettings, checkName);
//                 if (!provinceToCount.ContainsKey(province))
//                     provinceToCount[province] = 0;
//                 provinceToCount[province] += 1;
//             }

//             List<KeyValuePair<Province, int>> asList = provinceToCount.ToList();
//             asList.Sort(
//                 (a, b) =>
//                 {
//                     if (a.Value == b.Value)
//                     {
//                         int aVal = a.Key == Province.MultiProvince ? 0 : 1;
//                         int bVal = b.Key == Province.MultiProvince ? 0 : 1;
//                         return bVal - aVal;
//                     }

//                     return b.Value - a.Value;
//                 }
//             );

//             int largestPerProvince = asList[0].Value;

//             int lastIndex = asList.FindLastIndex((pair) => pair.Value == largestPerProvince);
//             int randomIndex = rnd.Next(lastIndex + 1);

//             Province selectedProvince = asList[randomIndex].Key;
//             int numSwords = provinceToCount[selectedProvince];

//             return new NumItemInAreaHint(numSwords, item, AreaId.Province(selectedProvince));
//         }

//         private List<KeyValuePair<HintAbs, int>> genMorePriorityHints(
//             HintedThings priorityHintedThings,
//             List<HintToGen> hintsToGen
//         )
//         {
//             List<KeyValuePair<HintAbs, int>> hintToQuantity = new();

//             if (hintsToGen == null || hintsToGen.Count < 1)
//                 return hintToQuantity;

//             bool generatedBarrenBugGenderHint = false;

//             foreach (HintToGen hintToGen in hintsToGen)
//             {
//                 switch (hintToGen)
//                 {
//                     case HintToGen.NumClawsInDungeons:
//                         NumItemInAreaHint numClawsInDungeonsHint = genNumClawsInDungeonsHint(
//                             priorityHintedThings
//                         );
//                         if (numClawsInDungeonsHint != null)
//                         {
//                             List<string> clawshotChecks = itemToChecksList[
//                                 Item.Progressive_Clawshot
//                             ];
//                             foreach (string checkName in clawshotChecks)
//                             {
//                                 priorityHintedThings.alreadyCheckDirectedToward.Add(checkName);
//                             }
//                             hintToQuantity.Add(new(numClawsInDungeonsHint, 2));
//                         }
//                         break;
//                     case HintToGen.ItemHint:
//                         List<Item> possibleHints =
//                             new()
//                             {
//                                 Item.Boomerang,
//                                 Item.Lantern,
//                                 Item.Progressive_Fishing_Rod,
//                                 Item.Iron_Boots,
//                                 Item.Progressive_Bow,
//                                 Item.Filled_Bomb_Bag,
//                                 Item.Progressive_Clawshot,
//                                 Item.Aurus_Memo,
//                                 Item.Spinner,
//                                 Item.Ball_and_Chain,
//                                 Item.Progressive_Dominion_Rod,
//                             };
//                         if (!generatedBarrenBugGenderHint)
//                         {
//                             possibleHints = possibleHints.Concat(getHardRequiredBugs()).ToList();
//                         }
//                         HintUtils.ShuffleListInPlace(rnd, possibleHints);

//                         for (int i = 0; i < possibleHints.Count; i++)
//                         {
//                             List<NamedItemHint> priorityItemHintList = genItemHintList(
//                                 new() { possibleHints[i] },
//                                 priorityHintedThings
//                             );
//                             if (priorityItemHintList.Count > 0)
//                             {
//                                 NamedItemHint itemHint = priorityItemHintList[0];
//                                 hintToQuantity.Add(new(itemHint, 2));
//                                 break;
//                             }
//                         }
//                         break;
//                     case HintToGen.BarrenBugGender:
//                         BugsUsefulHint barrenBugGenderHint = genBarrenBugGenderHint(
//                             priorityHintedThings
//                         );
//                         if (barrenBugGenderHint != null)
//                         {
//                             generatedBarrenBugGenderHint = true;
//                             hintToQuantity.Add(new(barrenBugGenderHint, 2));
//                         }
//                         break;
//                     case HintToGen.ItemToItemPath:
//                         // Hard-required unique items which can be hinted.
//                         Dictionary<Item, string> uniqReqItemToCheck = getItemToItemPathGoalItems(
//                             priorityHintedThings
//                         );

//                         if (uniqReqItemToCheck.Count < 1)
//                             break;

//                         HashSet<Goal> goals = new();
//                         foreach (KeyValuePair<Item, string> pair in uniqReqItemToCheck)
//                         {
//                             goals.Add(new Goal(GoalEnum.Invalid, Goal.Type.Check, pair.Value));
//                         }

//                         List<Item> srcItems =
//                             new()
//                             {
//                                 Item.Boomerang,
//                                 Item.Lantern,
//                                 Item.Progressive_Fishing_Rod,
//                                 Item.Iron_Boots,
//                                 Item.Progressive_Bow,
//                                 Item.Filled_Bomb_Bag,
//                                 Item.Progressive_Clawshot,
//                                 Item.Aurus_Memo,
//                                 Item.Spinner,
//                                 Item.Ball_and_Chain,
//                                 Item.Progressive_Dominion_Rod,
//                             };

//                         if (
//                             requiredChecks.Contains(
//                                 HintConstants.singleCheckItems[Item.Asheis_Sketch]
//                             )
//                         )
//                             srcItems.Add(Item.Asheis_Sketch);

//                         HashSet<Item> effectiveIndirectBugs = getEffectiveBugsIndirectlyPointedTo(
//                             priorityHintedThings
//                         );

//                         if (effectiveIndirectBugs.Count > 1)
//                         {
//                             srcItems.AddRange(effectiveIndirectBugs);
//                         }
//                         else if (effectiveIndirectBugs.Count == 0)
//                         {
//                             // Add all preventBarren bugs which are required.
//                             HashSet<Item> srcBugs = preventBarrenItems
//                                 .Where(
//                                     item =>
//                                         HintUtils.isItemGoldenBug(item)
//                                         && requiredChecks.Contains(
//                                             HintConstants.singleCheckItems[item]
//                                         )
//                                 )
//                                 .ToHashSet();
//                             srcItems.AddRange(srcBugs);
//                         }
//                         // Don't add the bug when there is exactly 1 pointed to
//                         // by the barrenBugHint. Hinting that bug in this way
//                         // would make the barrenBugHint pointless.


//                         // Add preventBarren bugs when haven't already hinted which bugs are good.
//                         Dictionary<Item, Dictionary<Goal, bool>> res =
//                             HintUtils.checkGoalsWithoutItems(startingRoom, srcItems, goals);

//                         Dictionary<Item, HashSet<Item>> destItemToSrcItem = new();

//                         foreach (KeyValuePair<Item, Dictionary<Goal, bool>> pair in res)
//                         {
//                             Item srcItem = pair.Key;
//                             // For each dest check, determine the destItem
//                             foreach (KeyValuePair<Goal, bool> goalPair in pair.Value)
//                             {
//                                 if (!goalPair.Value)
//                                 {
//                                     // Goal failed when missing srcItem
//                                     string destCheckName = goalPair.Key.id;
//                                     Item destItem = getCheckContents(destCheckName);
//                                     if (!destItemToSrcItem.ContainsKey(destItem))
//                                         destItemToSrcItem[destItem] = new();
//                                     destItemToSrcItem[destItem].Add(srcItem);
//                                 }
//                             }
//                         }

//                         if (destItemToSrcItem.Count < 1)
//                             break;

//                         List<KeyValuePair<double, Item>> weightedList = new();
//                         Dictionary<Item, int> srcItemCounts = new();
//                         foreach (KeyValuePair<Item, HashSet<Item>> pair in destItemToSrcItem)
//                         {
//                             foreach (Item srcItem in pair.Value)
//                             {
//                                 if (!srcItemCounts.ContainsKey(srcItem))
//                                     srcItemCounts[srcItem] = 0;
//                                 srcItemCounts[srcItem] += 1;
//                             }

//                             double weight = 3;
//                             if (!HintUtils.isItemGoldenBug(pair.Key))
//                             {
//                                 weight = (4 * Math.Atan(pair.Value.Count)) / Math.PI;
//                             }

//                             // double weight = HintUtils.isItemGoldenBug(pair.Key) ? 3 : 1;
//                             weightedList.Add(new(weight, pair.Key));
//                         }

//                         VoseInstance<Item> voseInst = VoseAlgorithm.createInstance(weightedList);
//                         Item selectedDestItem = voseInst.NextAndKeep(rnd);

//                         HashSet<Item> srcItemsForDest = destItemToSrcItem[selectedDestItem];
//                         List<KeyValuePair<double, Item>> srcWeightedList = new();
//                         foreach (Item srcItem in srcItemsForDest)
//                         {
//                             // double weight = 1 / Math.Sqrt(srcItemCounts[srcItem]);
//                             double weight = 1.0 / srcItemCounts[srcItem];
//                             srcWeightedList.Add(new(weight, srcItem));
//                         }

//                         VoseInstance<Item> srcItemVoseInst = VoseAlgorithm.createInstance(
//                             srcWeightedList
//                         );
//                         Item selectedSrcItem = srcItemVoseInst.NextAndKeep(rnd);
//                         string selectedSrcCheckName = itemToChecksList[selectedSrcItem][0];

//                         string destItemCheckName = itemToChecksList[selectedDestItem][0];
//                         priorityHintedThings.alreadyCheckDirectedToward.Add(destItemCheckName);

//                         ItemToItemPathHint itemToItemPathHint = new ItemToItemPathHint(
//                             selectedSrcItem,
//                             destItemCheckName
//                         );
//                         hintToQuantity.Add(new(itemToItemPathHint, 2));
//                         tryMarkAlreadyKnownFromHint(itemToItemPathHint, priorityHintedThings);
//                         break;
//                     case HintToGen.MistGoodOrNot:
//                         List<CategoryObject> mistHintList = genCategoryHints(
//                             priorityHintedThings,
//                             new() { HintCategory.Mist }
//                         );
//                         if (mistHintList != null && mistHintList.Count > 0)
//                         {
//                             CategoryObject mistObj = mistHintList[0];
//                             HintAbs hint = mistObj.toHint(getCheckContents);
//                             // hintToQuantity.Add(new(mistObj.toHint(), 1));
//                             hintToQuantity.Add(new(hint, 2));

//                             tryMarkAlreadyKnownFromHint(hint, priorityHintedThings);
//                         }
//                         break;
//                     case HintToGen.BarrenCategory:
//                         List<CategoryObject> barrenCatHintList = genCategoryHints(
//                             priorityHintedThings,
//                             new()
//                             {
//                                 HintCategory.Mist,
//                                 HintCategory.Lake_Lantern_Cave_Lantern_Checks,
//                                 HintCategory.Upper_Desert,
//                                 HintCategory.Lower_Desert,
//                                 // Below ones are split across multiple zones.
//                                 // Hopefully not too difficult for people.
//                                 HintCategory.Owl_Statue,
//                                 HintCategory.Underwater,
//                                 HintCategory.Grotto,
//                                 HintCategory.Post_dungeon,
//                             }
//                         );
//                         CategoryObject catObj = null;
//                         if (barrenCatHintList != null && barrenCatHintList.Count > 0)
//                         {
//                             foreach (CategoryObject categoryObject in barrenCatHintList)
//                             {
//                                 if (!categoryObject.hasUseful)
//                                 {
//                                     catObj = categoryObject;
//                                     break;
//                                 }
//                             }
//                         }
//                         if (catObj != null)
//                         {
//                             HintAbs hint = catObj.toHint(getCheckContents);
//                             hintToQuantity.Add(new(hint, 2));
//                             tryMarkAlreadyKnownFromHint(hint, priorityHintedThings);
//                         }
//                         break;
//                     default:
//                         break;
//                 }
//             }

//             if (hintToQuantity.Count > 4)
//                 return hintToQuantity.GetRange(0, 4);

//             return hintToQuantity;
//         }

//         private Dictionary<Item, string> getItemToItemPathGoalItems(HintedThings hintedThings)
//         {
//             Dictionary<Item, string> uniqueItemsToCheckName = new();
//             // foreach (Item item in preventBarrenItems)
//             // {
//             //     if (itemToChecksList.ContainsKey(item))
//             //     {
//             //         List<string> checksList = itemToChecksList[item];
//             //         if (checksList == null || checksList.Count != 1)
//             //             continue;

//             //         string checkName = checksList[0];
//             //         // Only can direct to a bug if the bug is hard-required.
//             //         if (HintUtils.isItemGoldenBug(item) && !requiredChecks.Contains(checkName))
//             //             continue;

//             //         if (checkCanBeHintedSpol(checkName, hintedThings))
//             //             uniqueItemsToCheckName[item] = checkName;
//             //     }
//             // }

//             HashSet<Item> effectiveBugsPointedTo = getEffectiveBugsIndirectlyPointedTo(
//                 hintedThings
//             );

//             foreach (string checkName in requiredChecks)
//             {
//                 if (isCheckSphere0(checkName))
//                     continue;

//                 Item contents = getCheckContents(checkName);
//                 if (effectiveBugsPointedTo.Count == 1 && effectiveBugsPointedTo.Contains(contents))
//                     continue;

//                 if (itemToChecksList.ContainsKey(contents))
//                 {
//                     List<string> checksList = itemToChecksList[contents];
//                     if (
//                         checksList != null
//                         && checksList.Count == 1
//                         && checkCanBeHintedSpol(checkName, hintedThings)
//                     )
//                     {
//                         uniqueItemsToCheckName[contents] = checkName;
//                     }
//                 }
//             }

//             return uniqueItemsToCheckName;
//         }

//         private HashSet<Item> getEffectiveBugsIndirectlyPointedTo(HintedThings hintedThings)
//         {
//             // Returns any bugs indirectly pointed to (preventBarren male bugs
//             // are pointed to when female bugs are hinted barren) and then
//             // removes any bugs which are hinted by name. For example, if there
//             // is a hint that "Lantern is path to Male Mantis" and also "female
//             // bugs are barren", then there must be a male bug required other
//             // than the Male Mantis. Otherwise, the "female bugs are barren"
//             // hint would serve no purpose. This applies to any hints that
//             // indicate that a bug is required, such as "Male Mantis is in
//             // Ordona Province" which indicates that Male Mantis is required.

//             HashSet<Item> effectiveSet = new();

//             if (hintedThings != null)
//             {
//                 foreach (Item bug in hintedThings.bugsIndirectlyPointedTo)
//                 {
//                     // Only add when the bug is not already known to be required
//                     // based on other hints.
//                     if (!hintedThings.alreadyItemHintedRequired.Contains(bug))
//                         effectiveSet.Add(bug);
//                 }
//             }
//             else
//             {
//                 foreach (Item bug in bugsIndirectlyPointedTo)
//                 {
//                     // Only add when the bug is not already known to be required
//                     // based on other hints.
//                     if (!alreadyItemHintedRequired.Contains(bug))
//                         effectiveSet.Add(bug);
//                 }
//             }

//             return effectiveSet;
//         }

//         private bool zoneIsBarren(string zoneName)
//         {
//             // Filter out any zones which contain checks that are all
//             // "Excluded" or "Excluded-Unrequired" or "Vanilla". A zone
//             // which only contains checks with these statuses will always
//             // contain some combination of junk and things the player
//             // already knows. That being the case, providing a Barren hint
//             // for such a zone would not help the player at all.
//             // bool zoneCanBeHintedBarren = false;
//             List<string> unknownChecks = new();
//             // int numUnknownChecks = 0;
//             bool zoneCanBeHintedBarren = true;

//             string[] checkNames = HintUtils.getHintZoneToChecksMap(sSettings)[zoneName];
//             for (int i = 0; i < checkNames.Length; i++)
//             {
//                 string checkName = checkNames[i];
//                 if (
//                     !alreadyCheckKnownBarren.Contains(checkName)
//                     && !alreadyCheckContentsHinted.Contains(checkName)
//                     && !hintsShouldIgnoreChecks.Contains(checkName)
//                     && !HintUtils.checkIsPlayerKnownStatus(checkName)
//                 )
//                 {
//                     // Important that the empty grotto check is still
//                     // checked in here such that it prevents the zone from
//                     // being hinted barren. We do not check against
//                     // `alreadyCheckDirectedToward` since this is what the
//                     // empty grotto and SpoL checks uses and we want those
//                     // to prevent barren.
//                     Item contents = getCheckContents(checkName);
//                     if (preventBarrenItems.Contains(contents))
//                     {
//                         zoneCanBeHintedBarren = false;
//                         break;
//                     }

//                     unknownChecks.Add(checkName);
//                 }
//             }

//             return zoneCanBeHintedBarren && unknownChecks.Count > 0;
//         }
//     }

//     class HintedThings
//     {
//         public HashSet<string> alreadyCheckContentsHinted = new();
//         public HashSet<string> alreadyCheckDirectedToward = new();
//         public HashSet<Item> alreadyItemHintedRequired = new();
//         public HashSet<string> hintsShouldIgnoreChecks = new();
//         public HashSet<string> alreadyCheckKnownBarren = new();
//         public HashSet<Item> bugsIndirectlyPointedTo = new();
//     }

//     enum HintToGen
//     {
//         NumClawsInDungeons,
//         ItemHint,
//         BarrenBugGender,
//         ItemToItemPath,
//         MistGoodOrNot,
//         BarrenCategory
//     }
// }
