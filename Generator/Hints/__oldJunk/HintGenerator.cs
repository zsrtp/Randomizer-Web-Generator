// namespace TPRandomizer.Hints
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//     using SSettings.Enums;

//     public class HintGenerator
//     {
//         public static HintResults Generate(
//             Random rnd,
//             SharedSettings sSettings,
//             // List<List<KeyValuePair<int, Item>>> spheres,
//             PlaythroughSpheres playthroughSpheres,
//             Room startingRoom
//         )
//         {
//             HintGeneratorInternal generator = new HintGeneratorInternal(
//                 sSettings,
//                 playthroughSpheres.spheres
//             );
//             return generator.Generate(rnd, startingRoom);
//         }

//         public static List<string> GetBaseHintSpotNames(
//             SharedSettings sharedSettings,
//             Dictionary<string, string[]> zoneToChecksMap = null
//         )
//         {
//             Dictionary<string, string[]> hintZoneToChecksMap = zoneToChecksMap;
//             if (hintZoneToChecksMap == null)
//             {
//                 hintZoneToChecksMap = HintUtils.getHintZoneToChecksMap(sharedSettings);
//             }

//             HashSet<string> invalidHintZoneNames =
//                 new()
//                 {
//                     "Agitha",
//                     "Hero's Spirit",
//                     "Cave of Ordeals",
//                     "Forest Temple",
//                     "Goron Mines",
//                     "Lakebed Temple",
//                     "Arbiter's Grounds",
//                     "Snowpeak Ruins",
//                     "Temple of Time",
//                     "City in the Sky",
//                     "Palace of Twilight",
//                     "Hyrule Castle",
//                 };

//             List<string> zoneNames = new();

//             foreach (KeyValuePair<string, string[]> kv in hintZoneToChecksMap)
//             {
//                 string zoneName = kv.Key;
//                 if (!invalidHintZoneNames.Contains(zoneName))
//                 {
//                     zoneNames.Add(zoneName);
//                 }
//             }

//             return zoneNames;
//         }

//         private static Hint genCheckHint(string checkName, HintType hintType, byte count = 0)
//         {
//             return new Hint(
//                 checkName,
//                 Randomizer.Checks.CheckDict[checkName].itemId,
//                 hintType,
//                 count
//             );
//         }

//         private static void ShuffleListInPlace<T>(Random rnd, IList<T> list)
//         {
//             int n = list.Count;
//             while (n > 1)
//             {
//                 n--;
//                 int k = rnd.Next(n + 1);
//                 T value = list[k];
//                 list[k] = list[n];
//                 list[n] = value;
//             }
//         }

//         internal class HintItemNameComparer : IComparer<Hint>
//         {
//             int IComparer<Hint>.Compare(Hint a, Hint b)
//             {
//                 string aItemName = a.contents.ToString();
//                 string bItemName = b.contents.ToString();

//                 return String.CompareOrdinal(aItemName, bItemName);
//             }
//         }

//         private class HintGeneratorInternal
//         {
//             Dictionary<string, string[]> hintZoneToChecksMap;
//             Dictionary<string, string> checkToHintZoneMap;
//             SharedSettings sSettings;
//             List<List<KeyValuePair<int, Item>>> spheres;

//             // TODO: don't hardcode
//             int desiredSometimesHintsCount = 4;

//             public HintGeneratorInternal(
//                 SharedSettings sSettings,
//                 List<List<KeyValuePair<int, Item>>> spheres
//             )
//             {
//                 this.sSettings = sSettings;
//                 this.spheres = spheres;

//                 // Load hint zone definitions. Will be based on settings in the future.
//                 hintZoneToChecksMap = HintUtils.getHintZoneToChecksMap(sSettings);
//                 checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(sSettings);
//             }

//             public HintResults Generate(Random rnd, Room startingRoom)
//             {
//                 Console.WriteLine("Calculating Hints.");

//                 List<Hint> alwaysHints = genAlwaysHints();
//                 List<Hint> agithaHints = genAgithaHints(spheres);
//                 List<Hint> spolHints = genSpolHints(rnd, startingRoom, alwaysHints);
//                 List<Hint> barrenHints = genBarrenHints(rnd, spolHints);
//                 List<Hint> sometimesHints = new();
//                 if (!sSettings.hintSettings.simulateInGameHints)
//                 {
//                     sometimesHints = genSometimesHints(rnd, spolHints, barrenHints);
//                 }

//                 List<Hint> hints = new List<Hint>()
//                     .Concat(alwaysHints)
//                     .Concat(agithaHints)
//                     .Concat(spolHints)
//                     .Concat(barrenHints)
//                     .Concat(sometimesHints)
//                     .ToList();

//                 HintResults hintResults = new();
//                 hintResults.hints = hints;

//                 if (sSettings.hintSettings.simulateInGameHints)
//                 {
//                     List<Hint> endOfGameHints = genEndOfGameHints(rnd, sSettings);
//                     hintResults.hints = hintResults.hints.Concat(endOfGameHints).ToList();

//                     FillInHintSpots(rnd, hintResults);
//                 }

//                 return hintResults;
//             }

//             private List<Hint> genAlwaysHints()
//             {
//                 List<Hint> alwaysHints = new();

//                 if (sSettings.hintSettings.alwaysHints != null)
//                 {
//                     foreach (String checkName in sSettings.hintSettings.alwaysHints)
//                     {
//                         alwaysHints.Add(genCheckHint(checkName, HintType.Always));
//                     }
//                 }

//                 return alwaysHints;
//             }

//             private List<Hint> genBarrenHints(Random rnd, List<Hint> spolHints)
//             {
//                 List<Hint> barrenHints = new();

//                 // Hint zones which cannot be hinted as barren. Death Mountain will
//                 // always be excluded, and other zones will not be hinted based on
//                 // settings.
//                 // TODO: Agitha should only be excluded based on settings.
//                 HashSet<string> excludedZones = new() { "Death Mountain", "Agitha" };

//                 // Exclude any zones already marked as SpoL.
//                 foreach (Hint spolHint in spolHints)
//                 {
//                     excludedZones.Add(checkToHintZoneMap[spolHint.checkName]);
//                 }

//                 // Prevent unrequiredBarren dungeons from being hinted barren when
//                 // unrequiredBarren setting is on.
//                 if (sSettings.barrenDungeons)
//                 {
//                     // If unrequiredBarren, filter out unrequired dungeons.
//                     foreach (
//                         KeyValuePair<string, byte> kv in HintConstants.dungeonZonesToRequiredMaskMap
//                     )
//                     {
//                         string zoneName = kv.Key;
//                         if (!HintUtils.DungeonIsRequired(zoneName))
//                         {
//                             excludedZones.Add(zoneName);
//                         }
//                         // byte mask = dungeonZonesToRequiredMaskMap.GetValueOrDefault(
//                         //     zoneName,
//                         //     (byte)0
//                         // );
//                         // if ((Randomizer.RequiredDungeons & mask) == 0)
//                         // {
//                         //     excludedZones.Add(zoneName);
//                         // }
//                     }
//                 }

//                 List<string> potentialBarrenZones = new();

//                 foreach (KeyValuePair<string, string[]> kv in hintZoneToChecksMap)
//                 {
//                     string zoneName = kv.Key;
//                     if (excludedZones.Contains(zoneName))
//                     {
//                         continue;
//                     }

//                     // Filter out any zones which contain checks that are all
//                     // "Excluded" or "Excluded-Unrequired" or "Vanilla". A zone
//                     // which only contains checks with these statuses will always
//                     // contain some combination of junk and things the player
//                     // already knows. That being the case, providing a Barren hint
//                     // for such a zone would not help the player at all.
//                     bool zoneCanBeHintedBarren = false;
//                     string[] checkNames = kv.Value;
//                     for (int i = 0; i < checkNames.Length; i++)
//                     {
//                         string checkName = checkNames[i];
//                         string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;

//                         if (
//                             !HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(
//                                 checkStatus
//                             )
//                         )
//                         {
//                             zoneCanBeHintedBarren = true;
//                             break;
//                         }
//                     }

//                     if (zoneCanBeHintedBarren)
//                     {
//                         potentialBarrenZones.Add(zoneName);
//                     }
//                 }

//                 // Randomize order of potential zones, then iterate through them.
//                 // Can mark a zone as barren (do up to X times) if none of its checks
//                 // have preventBarrenItems as their contents.
//                 // If an area does have an item to prevent barren, then keep going.
//                 // When picking an area to be barren, just pick the first item and add that as a hint.

//                 ShuffleListInPlace(rnd, potentialBarrenZones);

//                 HashSet<Item> preventBarrenItems = genPreventBarrenItemSet();

//                 for (
//                     int i = 0;
//                     i < potentialBarrenZones.Count
//                         && barrenHints.Count < sSettings.hintSettings.barrenHintCount;
//                     i++
//                 )
//                 {
//                     string zoneName = potentialBarrenZones[i];

//                     bool zoneIsBarren = true;
//                     string[] checksForZone = hintZoneToChecksMap[zoneName];
//                     for (int j = 0; j < checksForZone.Length; j++)
//                     {
//                         string checkName = checksForZone[j];
//                         Item contents = Randomizer.Checks.CheckDict[checkName].itemId;

//                         if (preventBarrenItems.Contains(contents))
//                         {
//                             zoneIsBarren = false;
//                             break;
//                         }
//                     }

//                     if (zoneIsBarren)
//                     {
//                         barrenHints.Add(genCheckHint(checksForZone[0], HintType.Barren));
//                     }
//                 }

//                 return barrenHints;
//             }

//             private HashSet<Item> genPreventBarrenItemSet()
//             {
//                 HashSet<Item> preventBarrenItemSet =
//                     new()
//                     {
//                         Item.Progressive_Sword,
//                         Item.Boomerang,
//                         Item.Lantern,
//                         Item.Slingshot,
//                         Item.Progressive_Fishing_Rod,
//                         Item.Iron_Boots,
//                         Item.Progressive_Bow,
//                         Item.Filled_Bomb_Bag,
//                         Item.Progressive_Clawshot,
//                         Item.Aurus_Memo,
//                         Item.Spinner,
//                         Item.Ball_and_Chain,
//                         Item.Progressive_Dominion_Rod,
//                     };

//                 // Don't use invalid SpoL items. For example, Big Keys should
//                 // still prevent barren if Big Key sanity is on.

//                 // Include any bugs and sketch if their reward check is not
//                 // excluded or vanilla.
//                 foreach (KeyValuePair<Item, string> kv in HintConstants.singleCheckItems)
//                 {
//                     if (
//                         !HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(
//                             Randomizer.Checks.CheckDict[kv.Value].checkStatus
//                         )
//                     )
//                     {
//                         preventBarrenItemSet.Add(kv.Key);
//                     }
//                 }

//                 // Add items which show up in the spheres.
//                 foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
//                 {
//                     if (spherePairs.Count > 0)
//                     {
//                         Dictionary<string, string> spherePairsMap = new();

//                         foreach (KeyValuePair<int, Item> pair in spherePairs)
//                         {
//                             preventBarrenItemSet.Add(pair.Value);
//                         }
//                     }
//                 }

//                 // Remove items from set depending on settings.
//                 preventBarrenItemSet.Remove(Item.Progressive_Hidden_Skill);
//                 preventBarrenItemSet.Remove(Item.Poe_Soul);

//                 preventBarrenItemSet.Remove(Item.Progressive_Mirror_Shard);
//                 preventBarrenItemSet.Remove(Item.Mirror_Piece_3);
//                 preventBarrenItemSet.Remove(Item.Mirror_Piece_4);
//                 preventBarrenItemSet.Remove(Item.Progressive_Fused_Shadow);
//                 preventBarrenItemSet.Remove(Item.Fused_Shadow_2);
//                 preventBarrenItemSet.Remove(Item.Fused_Shadow_3);
//                 preventBarrenItemSet.Remove(Item.Poe_Scent);
//                 preventBarrenItemSet.Remove(Item.Reekfish_Scent);

//                 // Big Keys only prevent barren if Keysanity or Any_Dungeon
//                 if (
//                     sSettings.bigKeySettings != BigKeySettings.Anywhere
//                     && sSettings.bigKeySettings != BigKeySettings.Any_Dungeon
//                 )
//                 {
//                     preventBarrenItemSet.Remove(Item.Forest_Temple_Big_Key);
//                     preventBarrenItemSet.Remove(Item.Goron_Mines_Big_Key);
//                     preventBarrenItemSet.Remove(Item.Goron_Mines_Key_Shard);
//                     preventBarrenItemSet.Remove(Item.Goron_Mines_Key_Shard_Second);
//                     preventBarrenItemSet.Remove(Item.Goron_Mines_Key_Shard_3);
//                     preventBarrenItemSet.Remove(Item.Lakebed_Temple_Big_Key);
//                     preventBarrenItemSet.Remove(Item.Arbiters_Grounds_Big_Key);
//                     preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Bedroom_Key);
//                     preventBarrenItemSet.Remove(Item.Temple_of_Time_Big_Key);
//                     preventBarrenItemSet.Remove(Item.City_in_The_Sky_Big_Key);
//                     preventBarrenItemSet.Remove(Item.Palace_of_Twilight_Big_Key);
//                     preventBarrenItemSet.Remove(Item.Hyrule_Castle_Big_Key);
//                 }

//                 if (
//                     sSettings.smallKeySettings != SmallKeySettings.Anywhere
//                     && sSettings.smallKeySettings != SmallKeySettings.Any_Dungeon
//                 )
//                 {
//                     preventBarrenItemSet.Remove(Item.Forest_Temple_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Goron_Mines_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Lakebed_Temple_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Arbiters_Grounds_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Ordon_Goat_Cheese);
//                     preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Ordon_Pumpkin);
//                     preventBarrenItemSet.Remove(Item.Snowpeak_Ruins_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Temple_of_Time_Small_Key);
//                     preventBarrenItemSet.Remove(Item.City_in_The_Sky_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Palace_of_Twilight_Small_Key);
//                     preventBarrenItemSet.Remove(Item.Hyrule_Castle_Small_Key);
//                 }

//                 return preventBarrenItemSet;
//             }

//             private List<Hint> genSpolHints(Random rnd, Room startingRoom, List<Hint> alwaysHints)
//             {
//                 HashSet<string> potentialSpolCheckNames = getPotentialSpolCheckNames(
//                     startingRoom,
//                     alwaysHints
//                 );

//                 HashSet<string> potentialSpolHintZones = new();
//                 foreach (string checkName in potentialSpolCheckNames)
//                 {
//                     potentialSpolHintZones.Add(checkToHintZoneMap[checkName]);
//                 }

//                 List<string> potentialSpolHintZonesList = potentialSpolHintZones.ToList();

//                 List<Hint> spolHints = new();

//                 ShuffleListInPlace(rnd, potentialSpolHintZonesList);

//                 for (
//                     int i = 0;
//                     i < sSettings.hintSettings.spolHintCount
//                         && i < potentialSpolHintZonesList.Count;
//                     i++
//                 )
//                 {
//                     // Get checkname for that zone
//                     string zoneName = potentialSpolHintZonesList[i];

//                     // Pick a random check from the list of potentialSpolChecks
//                     // which belong to that zone.
//                     List<string> checkNamesForZone = new();
//                     foreach (string potentialSpolCheckName in potentialSpolCheckNames)
//                     {
//                         if (checkToHintZoneMap[potentialSpolCheckName] == zoneName)
//                         {
//                             checkNamesForZone.Add(potentialSpolCheckName);
//                         }
//                     }
//                     ShuffleListInPlace(rnd, checkNamesForZone);
//                     string checkName = checkNamesForZone[0];

//                     spolHints.Add(genCheckHint(checkName, HintType.SpiritOfLight));
//                 }

//                 return spolHints;
//             }

//             private HashSet<string> getPotentialSpolCheckNames(
//                 Room startingRoom,
//                 List<Hint> alwaysHints
//             )
//             {
//                 HashSet<string> potentialSpolCheckNames = new();

//                 // Don't consider a check for SpoL if already covered by Always
//                 // hints
//                 HashSet<string> skipCheckNames = new();
//                 foreach (Hint alwaysHint in alwaysHints)
//                 {
//                     skipCheckNames.Add(alwaysHint.checkName);
//                 }

//                 // TODO: don't hardcode Agitha; only include if AgithaRewards
//                 // hint is turned on.
//                 HashSet<string> preventSpolHintZones = new() { "Agitha" };

//                 foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
//                 {
//                     if (spherePairs.Count > 0)
//                     {
//                         Dictionary<string, string> spherePairsMap = new();

//                         foreach (KeyValuePair<int, Item> pair in spherePairs)
//                         {
//                             Item checkContents = pair.Value;
//                             string checkName = CheckIdClass.GetCheckName(pair.Key);
//                             string checkHintZone = checkToHintZoneMap[checkName];

//                             if (
//                                 !HintConstants.invalidSpolItems.Contains(checkContents)
//                                 && !skipCheckNames.Contains(checkName)
//                                 && !preventSpolHintZones.Contains(checkHintZone)
//                             )
//                             {
//                                 potentialSpolCheckNames.Add(checkName);
//                             }
//                         }
//                     }
//                 }

//                 potentialSpolCheckNames = filterPotentialSpolWithPlaythroughTests(
//                     startingRoom,
//                     potentialSpolCheckNames,
//                     alwaysHints
//                 );

//                 return potentialSpolCheckNames;
//             }

//             // Return filtered list of checkNames, removing any checkNames for
//             // checks which are not necessary to logically complete the seed.
//             private HashSet<string> filterPotentialSpolWithPlaythroughTests(
//                 Room startingRoom,
//                 HashSet<string> unfilteredCheckNames,
//                 List<Hint> alwaysHints
//             )
//             {
//                 HashSet<string> filteredCheckNames = new();

//                 // I think it is safe to only generate the item pool once up front.
//                 Randomizer.Items.GenerateItemPool();

//                 // After we get potential checks, filter out any which can be
//                 // removed in isolation and the playthrough is still valid.
//                 foreach (string checkName in unfilteredCheckNames)
//                 {
//                     // Replace check contents with a green rupee. If the playthrough
//                     // is still beatable, then that item cannot be considered for
//                     // SpoL hints.
//                     Item originalContents = Randomizer.Checks.CheckDict[checkName].itemId;
//                     Randomizer.Checks.CheckDict[checkName].itemId = Item.Green_Rupee;

//                     bool successWithoutCheck = BackendFunctions.emulatePlaythrough(startingRoom);

//                     if (!successWithoutCheck)
//                     {
//                         Console.WriteLine($"Needed for SpoL: {originalContents}: {checkName}");
//                         // Check required to beat the seed, so add it for
//                         // consideration for SpoL hints.
//                         filteredCheckNames.Add(checkName);
//                     }
//                     else
//                     {
//                         Console.WriteLine($"Not needed for SpoL: {originalContents}: {checkName}");
//                     }

//                     // Put the original item back.
//                     Randomizer.Checks.CheckDict[checkName].itemId = originalContents;
//                 }

//                 return filteredCheckNames;
//             }

//             private List<Hint> genSometimesHints(
//                 Random rnd,
//                 List<Hint> spolHints,
//                 List<Hint> barrenHints
//             )
//             {
//                 HashSet<string> zoneNamesToAvoid = new();
//                 foreach (Hint hint in spolHints)
//                 {
//                     zoneNamesToAvoid.Add(checkToHintZoneMap[hint.checkName]);
//                 }
//                 foreach (Hint hint in barrenHints)
//                 {
//                     zoneNamesToAvoid.Add(checkToHintZoneMap[hint.checkName]);
//                 }

//                 List<string> baseCheckNames = genBaseSometimesCheckNames();

//                 List<string> goodCheckNames = new();
//                 List<string> mehCheckNames = new();
//                 foreach (string checkName in baseCheckNames)
//                 {
//                     if (zoneNamesToAvoid.Contains(checkToHintZoneMap[checkName]))
//                     {
//                         mehCheckNames.Add(checkName);
//                     }
//                     else
//                     {
//                         goodCheckNames.Add(checkName);
//                     }
//                 }

//                 ShuffleListInPlace(rnd, goodCheckNames);
//                 ShuffleListInPlace(rnd, mehCheckNames);

//                 List<string> checkNames = new List<string>()
//                     .Concat(goodCheckNames)
//                     .Concat(mehCheckNames)
//                     .ToList();

//                 List<Hint> sometimesHints = new();

//                 for (
//                     int i = 0;
//                     i < checkNames.Count
//                         && sometimesHints.Count < sSettings.hintSettings.sometimesHintCount;
//                     i++
//                 )
//                 {
//                     sometimesHints.Add(genCheckHint(checkNames[i], HintType.Sometimes));
//                 }

//                 return sometimesHints;
//             }

//             private List<string> genBaseSometimesCheckNames()
//             {
//                 List<string> baseSometimesCheckNames = new();

//                 if (sSettings.hintSettings.sometimesHintPool == null)
//                 {
//                     return baseSometimesCheckNames;
//                 }

//                 // Iterate through checks
//                 foreach (string checkName in sSettings.hintSettings.sometimesHintPool)
//                 {
//                     string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
//                     // Don't hint Excluded, Vanilla, etc.
//                     if (HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(checkStatus))
//                     {
//                         continue;
//                     }

//                     // Don't hint dungeon or post-dungeon if dungeon is
//                     // unrequired barren for this seed.
//                     string dependentDungeonName = HintUtils.getDependentDungeonForCheckName(
//                         checkName
//                     );
//                     if (
//                         dependentDungeonName != null
//                         && sSettings.barrenDungeons
//                         && !HintUtils.DungeonIsRequired(dependentDungeonName)
//                     )
//                     {
//                         continue;
//                     }

//                     baseSometimesCheckNames.Add(checkName);
//                 }

//                 return baseSometimesCheckNames;
//             }

//             private static List<Hint> genAgithaHints(List<List<KeyValuePair<int, Item>>> spheres)
//             {
//                 HashSet<Item> interestingItems =
//                     new()
//                     {
//                         // Collection screen
//                         Item.Progressive_Sword,
//                         Item.Progressive_Wallet,
//                         Item.Ordon_Shield,
//                         Item.Hylian_Shield,
//                         Item.Zora_Armor,
//                         Item.Magic_Armor,
//                         Item.Shadow_Crystal,
//                         // Item wheel
//                         Item.Boomerang,
//                         Item.Lantern,
//                         Item.Slingshot,
//                         Item.Progressive_Fishing_Rod,
//                         Item.Iron_Boots,
//                         Item.Progressive_Bow,
//                         Item.Filled_Bomb_Bag,
//                         Item.Progressive_Clawshot,
//                         Item.Aurus_Memo,
//                         Item.Asheis_Sketch,
//                         Item.Spinner,
//                         Item.Ball_and_Chain,
//                         Item.Progressive_Dominion_Rod,
//                         Item.Progressive_Sky_Book,
//                         Item.Renados_Letter,
//                         Item.Invoice,
//                         Item.Wooden_Statue,
//                         Item.Ilias_Charm,
//                         Item.Horse_Call,
//                         Item.Gate_Keys,
//                         // Item.Empty_Bottle,
//                         // Item.Progressive_Hidden_Skill,
//                     };

//                 HashSet<Item> sphereItems = getSphereItems(spheres);

//                 HashSet<Item> bugsOfInterest = new();

//                 foreach (KeyValuePair<Item, string> kv in HintConstants.bugsToRewardChecksMap)
//                 {
//                     string checkName = kv.Value;
//                     string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;

//                     if (!HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(checkStatus))
//                     {
//                         bugsOfInterest.Add(kv.Key);
//                     }
//                 }

//                 HashSet<Item> possibleHintItems = interestingItems
//                     .Concat(sphereItems)
//                     .Concat(bugsOfInterest)
//                     .ToHashSet();
//                 possibleHintItems.Remove(Item.Progressive_Hidden_Skill);
//                 possibleHintItems.Remove(Item.Poe_Soul);

//                 List<Hint> agithaHints = new();

//                 // Okay to loop over all bugs since excluded ones will never
//                 // have anything meaningful.
//                 foreach (KeyValuePair<Item, string> kv in HintConstants.bugsToRewardChecksMap)
//                 {
//                     string checkName = kv.Value;
//                     Item contents = Randomizer.Checks.CheckDict[checkName].itemId;

//                     if (possibleHintItems.Contains(contents))
//                     {
//                         agithaHints.Add(genCheckHint(checkName, HintType.AgithaRewards));
//                     }
//                 }

//                 // Sort list alphabetically so players can't try to infer anything
//                 // based on the order.
//                 agithaHints.Sort(new HintItemNameComparer());

//                 return agithaHints;
//             }

//             private static HashSet<Item> getSphereItems(List<List<KeyValuePair<int, Item>>> spheres)
//             {
//                 HashSet<Item> sphereItems = new();

//                 foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
//                 {
//                     if (spherePairs.Count > 0)
//                     {
//                         Dictionary<string, string> spherePairsMap = new();

//                         foreach (KeyValuePair<int, Item> pair in spherePairs)
//                         {
//                             sphereItems.Add(pair.Value);
//                         }
//                     }
//                 }

//                 return sphereItems;
//             }

//             private void FillInHintSpots(Random rnd, HintResults hintResults)
//             {
//                 int midnaSpolHintIndex = -1;
//                 int midnaBarrenHintIndex = -1;

//                 List<int> hintIndexesForHintSpots = new();

//                 for (int i = 0; i < hintResults.hints.Count; i++)
//                 {
//                     Hint hint = hintResults.hints[i];
//                     if (midnaSpolHintIndex < 0 && hint.hintType == HintType.SpiritOfLight)
//                     {
//                         midnaSpolHintIndex = i;
//                     }
//                     else if (midnaBarrenHintIndex < 0 && hint.hintType == HintType.Barren)
//                     {
//                         midnaBarrenHintIndex = i;
//                     }
//                     else if (
//                         hint.hintType != HintType.AgithaRewards
//                         && hint.hintType != HintType.EndOfGame
//                     )
//                     {
//                         hintIndexesForHintSpots.Add(i);
//                     }
//                 }

//                 if (midnaSpolHintIndex >= 0)
//                 {
//                     hintResults.midnaHintSpot.hintIndexArr.Add(midnaSpolHintIndex);
//                 }
//                 if (midnaBarrenHintIndex >= 0)
//                 {
//                     hintResults.midnaHintSpot.hintIndexArr.Add(midnaBarrenHintIndex);
//                 }
//                 hintResults.midnaHintSpot.hintCount = (byte)hintResults
//                     .midnaHintSpot
//                     .hintIndexArr
//                     .Count;

//                 // Create list of hint spots, and fill in their hint counts.
//                 // Keep track of the total number of hint counts.

//                 byte hintsPerWorldHintSpot = 2;

//                 hintResults.hintSpots = GenBaseHintSpots(hintResults, hintsPerWorldHintSpot);

//                 // Add 3 copies of the SpoL, Barren, and Always hints.
//                 hintIndexesForHintSpots = hintIndexesForHintSpots
//                     .Concat(hintIndexesForHintSpots)
//                     .Concat(hintIndexesForHintSpots)
//                     .ToList();

//                 List<Hint> sometimesHints = genShuffledValidSometimesHintsList(
//                     rnd,
//                     hintResults.hints
//                 );
//                 HintProvider sometimesHintProvider = new HintProvider(sometimesHints);

//                 JunkHints.Generator junkHintsGenerator = new JunkHints.Generator(rnd);
//                 int firstJunkHintIndex = -1;

//                 while (true)
//                 {
//                     // Get list of not full-junked HintSpots which need more hints.
//                     List<HintSpot> spotsMissingHints = new();
//                     for (int i = 0; i < hintResults.hintSpots.Count; i++)
//                     {
//                         HintSpot hintSpot = hintResults.hintSpots[i];
//                         if (hintSpot.hintIndexArr.Count < hintSpot.hintCount)
//                         {
//                             spotsMissingHints.Add(hintSpot);
//                         }
//                     }

//                     if (spotsMissingHints.Count == 0)
//                     {
//                         break;
//                     }

//                     ShuffleListInPlace(rnd, spotsMissingHints);

//                     // Do one pass of adding a hintIndex to each spot. If
//                     // necessary, add more Random or Junk hints to
//                     // `hintResults.hints`.
//                     foreach (HintSpot spotMissingHints in spotsMissingHints)
//                     {
//                         // Scan through the `hintIndexesForHintSpots` from the
//                         // start until find a number which is not already in
//                         // this hintSpots's list.
//                         bool addedHintIndex = false;
//                         for (int i = 0; i < hintIndexesForHintSpots.Count; i++)
//                         {
//                             int hintIndex = hintIndexesForHintSpots[i];
//                             if (!spotMissingHints.hintIndexArr.Contains(hintIndex))
//                             {
//                                 hintIndexesForHintSpots.RemoveAt(i);
//                                 spotMissingHints.hintIndexArr.Add(hintIndex);
//                                 addedHintIndex = true;
//                                 // break out of inner loop
//                                 break;
//                             }
//                         }
//                         if (addedHintIndex)
//                         {
//                             continue;
//                         }

//                         // If can't find anything, grab another random hint
//                         // (guaranteed to be a new hint; needs to be added to
//                         // hintList).
//                         if (sometimesHintProvider.HasMoreHints())
//                         {
//                             Hint sometimesHint = sometimesHintProvider.NextHint();
//                             // Add new hint to hint list of hintResults.
//                             hintResults.hints.Add(sometimesHint);
//                             int newSometimesHintIndex = hintResults.hints.Count - 1;
//                             // Make hintSpot point to the index of this new hint
//                             // (always at the end of the list).
//                             spotMissingHints.hintIndexArr.Add(newSometimesHintIndex);
//                             // Add index to `hintIndexesForHintSpots`. This is
//                             // for the 2nd copy of this Sometimes hint.
//                             hintIndexesForHintSpots.Add(newSometimesHintIndex);
//                             continue;
//                         }

//                         // If no more random hints available, grab a junk hint.
//                         // Junk hints are guaranteed to be unique until made it
//                         // through the list once. Note that since we don't add
//                         // junk hints until we have already exhausted all of the
//                         // other hint types, the junk hints will always all be
//                         // grouped at the end of the `hintResults.hints` list.
//                         // So it is safe to keep track of the first junk index
//                         // and determine the hint's index in the hintResults
//                         // based on that starting point + index returned from
//                         // the junk hints generator.
//                         JunkHints.Result junkHintResult = junkHintsGenerator.GetNextHint();
//                         if (firstJunkHintIndex < 0)
//                         {
//                             firstJunkHintIndex = hintResults.hints.Count;
//                         }
//                         if (junkHintResult.isNewHint)
//                         {
//                             Hint junkHint = junkHintResult.hint;
//                             hintResults.hints.Add(junkHint);
//                         }
//                         spotMissingHints.hintIndexArr.Add(
//                             firstJunkHintIndex + junkHintResult.index
//                         );

//                         // After made it through the list once, only way to not
//                         // be able to get a junk hint for a spot is if every
//                         // single junk hint is already on that spot. Since we
//                         // have a cap of 255 hints on a spot, we need to provide
//                         // at least this many junk hints. Probably the actual
//                         // cap of 255 would be decreased once we figure out the
//                         // actual number. Since all of the spots only have 2
//                         // hints on them at the moment and we have 64 junk
//                         // hints, it should be impossible to not be able to find
//                         // a junk hint to put on a specific spot, so not
//                         // worrying about this edge case at the moment.
//                     }
//                 }

//                 if (firstJunkHintIndex < 0)
//                 {
//                     firstJunkHintIndex = hintResults.hints.Count;
//                 }

//                 // Fill any spots with 0 hintCount to have a single junk hint.
//                 foreach (HintSpot hintSpot in hintResults.hintSpots)
//                 {
//                     if (hintSpot.hintCount == 0)
//                     {
//                         hintSpot.hintCount = hintsPerWorldHintSpot;
//                         JunkHints.Result junkHintResult = junkHintsGenerator.GetNextHint();
//                         if (junkHintResult.isNewHint)
//                         {
//                             Hint junkHint = junkHintResult.hint;
//                             hintResults.hints.Add(junkHint);
//                         }
//                         hintSpot.hintIndexArr.Add(firstJunkHintIndex + junkHintResult.index);
//                     }
//                 }
//             }

//             private List<Hint> genShuffledValidSometimesHintsList(Random rnd, List<Hint> hints)
//             {
//                 List<Hint> spolHints = new();
//                 List<Hint> barrenHints = new();
//                 foreach (Hint hint in hints)
//                 {
//                     switch (hint.hintType)
//                     {
//                         case HintType.SpiritOfLight:
//                             spolHints.Add(hint);
//                             break;
//                         case HintType.Barren:
//                             barrenHints.Add(hint);
//                             break;
//                     }
//                 }

//                 HashSet<string> checksToAvoid = new();

//                 foreach (string checkName in sSettings.hintSettings.alwaysHints)
//                 {
//                     checksToAvoid.Add(checkName);
//                 }

//                 HashSet<string> zonesToAvoid = new();
//                 foreach (Hint barrenHint in barrenHints)
//                 {
//                     zonesToAvoid.Add(checkToHintZoneMap[barrenHint.checkName]);
//                 }
//                 if (sSettings.hintSettings.agithaRewardsHintsEnabled)
//                 {
//                     zonesToAvoid.Add("Agitha");
//                 }

//                 // Get List of all possible random hint checks.
//                 List<string> possibleHintCheckNames = new();

//                 foreach (string checkName in sSettings.hintSettings.sometimesHintPool)
//                 {
//                     // Filter out already covered hints by Always and Sometimes.
//                     if (checksToAvoid.Contains(checkName))
//                     {
//                         continue;
//                     }

//                     // Filter out any checks belonging to hinted Barren zones.
//                     string hintZoneName = checkToHintZoneMap[checkName];
//                     if (zonesToAvoid.Contains(hintZoneName))
//                     {
//                         continue;
//                     }

//                     // Filter out checks which are invalid based on settings
//                     // (post-dungeon, invalid dungeon, poes, etc.).
//                     string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
//                     // Don't hint Excluded, Vanilla, etc.
//                     if (HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(checkStatus))
//                     {
//                         continue;
//                     }

//                     // Don't hint dungeon or post-dungeon if dungeon is
//                     // unrequired barren for this seed.
//                     string dependentDungeonName = HintUtils.getDependentDungeonForCheckName(
//                         checkName
//                     );
//                     if (
//                         dependentDungeonName != null
//                         && sSettings.barrenDungeons
//                         && !HintUtils.DungeonIsRequired(dependentDungeonName)
//                     )
//                     {
//                         continue;
//                     }

//                     // Filter out Checks with "Dungeon Reward" in their "category".
//                     if (Randomizer.Checks.CheckDict[checkName].category.Contains("Dungeon Reward"))
//                     {
//                         continue;
//                     }

//                     // Add to list if passes all checks.
//                     possibleHintCheckNames.Add(checkName);
//                 }

//                 ShuffleListInPlace(rnd, possibleHintCheckNames);

//                 List<Hint> retList = new();
//                 foreach (string checkName in possibleHintCheckNames)
//                 {
//                     retList.Add(genCheckHint(checkName, HintType.Sometimes));
//                 }

//                 return retList;
//             }

//             private List<HintSpot> GenBaseHintSpots(HintResults hintResults, byte hintsPerSpot)
//             {
//                 List<string> zoneNames = GetBaseHintSpotNames(this.sSettings, hintZoneToChecksMap);

//                 // HashSet<string> invalidHintZoneNames =
//                 //     new()
//                 //     {
//                 //         "Agitha",
//                 //         "Hero's Spirit",
//                 //         "Cave of Ordeals",
//                 //         "Forest Temple",
//                 //         "Goron Mines",
//                 //         "Lakebed Temple",
//                 //         "Arbiter's Grounds",
//                 //         "Snowpeak Ruins",
//                 //         "Temple of Time",
//                 //         "City in the Sky",
//                 //         "Palace of Twilight",
//                 //         "Hyrule Castle",
//                 //     };

//                 // List<HintSpot> hintSpots = new();

//                 // foreach (KeyValuePair<string, string[]> kv in hintZoneToChecksMap)
//                 // {
//                 //     string zoneName = kv.Key;
//                 //     if (!invalidHintZoneNames.Contains(zoneName))
//                 //     {
//                 //         HintSpot hintSpot = new HintSpot(zoneName);
//                 //         hintSpot.hintCount = (byte)(
//                 //             HintZoneAllBoringChecks(zoneName) ? 0 : hintsPerSpot
//                 //         );
//                 //         hintSpots.Add(hintSpot);
//                 //     }
//                 // }

//                 HashSet<string> zonesHintedBarren = new();
//                 foreach (Hint hint in hintResults.hints)
//                 {
//                     if (hint.hintType == HintType.Barren)
//                     {
//                         string hintedBarrenZone = checkToHintZoneMap[hint.checkName];
//                         zonesHintedBarren.Add(hintedBarrenZone);
//                     }
//                 }

//                 List<HintSpot> hintSpots = new();

//                 foreach (string zoneName in zoneNames)
//                 {
//                     HintSpot hintSpot = new HintSpot(zoneName);
//                     hintSpot.hintCount = (byte)(
//                         zonesHintedBarren.Contains(zoneName) || HintZoneAllBoringChecks(zoneName)
//                             ? 0
//                             : hintsPerSpot
//                     );
//                     hintSpots.Add(hintSpot);
//                 }

//                 return hintSpots;
//             }

//             private bool HintZoneAllBoringChecks(string hintZoneName)
//             {
//                 string[] checkNames = hintZoneToChecksMap[hintZoneName];
//                 if (checkNames == null)
//                 {
//                     return true;
//                 }

//                 foreach (string checkName in checkNames)
//                 {
//                     // Filter out checks which are invalid based on settings
//                     // (post-dungeon, invalid dungeon, poes, etc.).
//                     string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
//                     // Don't hint Excluded, Vanilla, etc.
//                     if (HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(checkStatus))
//                     {
//                         continue;
//                     }

//                     // Don't hint dungeon or post-dungeon if dungeon is
//                     // unrequired barren for this seed.
//                     string dependentDungeonName = HintUtils.getDependentDungeonForCheckName(
//                         checkName
//                     );
//                     if (
//                         dependentDungeonName != null
//                         && sSettings.barrenDungeons
//                         && !HintUtils.DungeonIsRequired(dependentDungeonName)
//                     )
//                     {
//                         continue;
//                     }

//                     return false;
//                 }

//                 return true;
//             }

//             private List<Hint> genEndOfGameHints(Random rnd, SharedSettings sSettings)
//             {
//                 List<Hint> endOfGameHints = new();

//                 HashSet<Item> endOfGameHintItems = new() { Item.Progressive_Sword };
//                 if (
//                     sSettings.bigKeySettings == BigKeySettings.Anywhere
//                     || sSettings.bigKeySettings == BigKeySettings.Any_Dungeon
//                 )
//                 {
//                     endOfGameHintItems.Add(Item.Hyrule_Castle_Big_Key);
//                 }

//                 foreach (KeyValuePair<string, Check> kv in Randomizer.Checks.CheckDict)
//                 {
//                     Check check = kv.Value;
//                     if (endOfGameHintItems.Contains(check.itemId))
//                     {
//                         endOfGameHints.Add(genCheckHint(check.checkName, HintType.EndOfGame));
//                     }
//                 }

//                 return endOfGameHints;
//             }
//         }
//     }

//     internal class HintProvider
//     {
//         int index = 0;
//         List<Hint> hints;

//         public HintProvider(List<Hint> hints)
//         {
//             this.hints = hints;
//         }

//         public Hint NextHint()
//         {
//             Hint hint = hints[index];
//             index += 1;
//             return hint;
//         }

//         public bool HasMoreHints()
//         {
//             if (hints == null)
//             {
//                 return false;
//             }
//             return index < hints.Count;
//         }
//     }
// }
