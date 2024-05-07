namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public enum GoalEnum
    {
        Invalid = 0,
        Diababa = 1,
        Fyrus = 2,
        Morpheel = 3,
        Stallord = 4,
        Blizzeta = 5,
        Armogohma = 6,
        Argorok = 7,
        Zant = 8,
        Hyrule_Castle = 9,
        Ganondorf = 10,
    }

    public class Goal
    {
        public enum Type
        {
            Room,
            Check,
        }

        public GoalEnum goalEnum { get; }
        public Type type { get; }
        public string id { get; }

        public Goal(GoalEnum goalEnum, Type type, string id)
        {
            this.goalEnum = goalEnum;
            this.type = type;
            this.id = id;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                Goal g = (Goal)obj;
                return this.goalEnum == g.goalEnum && this.type == g.type && this.id == g.id;
            }
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (goalEnum, type, id).GetHashCode();
        }
    }

    public class GoalConstants
    {
        public static readonly byte NumBitsToEncode = 6;

        public static readonly Goal Diababa = new Goal(
            GoalEnum.Diababa,
            Goal.Type.Check,
            "Forest Temple Dungeon Reward"
        );
        public static readonly Goal Fyrus = new Goal(
            GoalEnum.Fyrus,
            Goal.Type.Check,
            "Goron Mines Dungeon Reward"
        );
        public static readonly Goal Morpheel = new Goal(
            GoalEnum.Morpheel,
            Goal.Type.Check,
            "Lakebed Temple Dungeon Reward"
        );
        public static readonly Goal Stallord = new Goal(
            GoalEnum.Stallord,
            Goal.Type.Check,
            "Arbiters Grounds Stallord Heart Container"
        );
        public static readonly Goal Blizzeta = new Goal(
            GoalEnum.Blizzeta,
            Goal.Type.Check,
            "Snowpeak Ruins Dungeon Reward"
        );
        public static readonly Goal Armogohma = new Goal(
            GoalEnum.Armogohma,
            Goal.Type.Check,
            "Temple of Time Dungeon Reward"
        );
        public static readonly Goal Argorok = new Goal(
            GoalEnum.Argorok,
            Goal.Type.Check,
            "City in The Sky Dungeon Reward"
        );

        public static readonly Goal Zant = new Goal(
            GoalEnum.Zant,
            Goal.Type.Check,
            "Palace of Twilight Zant Heart Container"
        );
        public static readonly Goal Hyrule_Castle = new Goal(
            GoalEnum.Hyrule_Castle,
            Goal.Type.Room,
            "Hyrule Castle Entrance"
        );
        public static readonly Goal Ganondorf = new Goal(
            GoalEnum.Ganondorf,
            Goal.Type.Room,
            "Ganondorf Castle"
        );

        public static readonly Dictionary<string, Goal> requiredDungeonHintZoneToGoal =
            new()
            {
                { "Forest Temple", Diababa },
                { "Goron Mines", Fyrus },
                { "Lakebed Temple", Morpheel },
                { "Arbiter's Grounds", Stallord },
                { "Snowpeak Ruins", Blizzeta },
                { "Temple of Time", Armogohma },
                { "City in the Sky", Argorok },
                { "Palace of Twilight", Zant },
            };

        public static bool IsDungeonGoal(Goal goal)
        {
            if (goal == null)
                return false;

            return requiredDungeonHintZoneToGoal.ContainsValue(goal);
        }
    }

    public class HintUtils
    {
        private static readonly HashSet<string> dungeonZones =
            new()
            {
                "Forest Temple",
                "Goron Mines",
                "Lakebed Temple",
                "Arbiter's Grounds",
                "Snowpeak Ruins",
                "Temple of Time",
                "City in the Sky",
                "Palace of Twilight",
                "Hyrule Castle",
            };

        public static readonly Dictionary<Item, string> tradeItemToRewardCheck =
            new()
            {
                { Item.Female_Ant, "Agitha Female Ant Reward" },
                { Item.Female_Beetle, "Agitha Female Beetle Reward" },
                { Item.Female_Butterfly, "Agitha Female Butterfly Reward" },
                { Item.Female_Dayfly, "Agitha Female Dayfly Reward" },
                { Item.Female_Dragonfly, "Agitha Female Dragonfly Reward" },
                { Item.Female_Grasshopper, "Agitha Female Grasshopper Reward" },
                { Item.Female_Ladybug, "Agitha Female Ladybug Reward" },
                { Item.Female_Mantis, "Agitha Female Mantis Reward" },
                { Item.Female_Phasmid, "Agitha Female Phasmid Reward" },
                { Item.Female_Pill_Bug, "Agitha Female Pill Bug Reward" },
                { Item.Female_Snail, "Agitha Female Snail Reward" },
                { Item.Female_Stag_Beetle, "Agitha Female Stag Beetle Reward" },
                { Item.Male_Ant, "Agitha Male Ant Reward" },
                { Item.Male_Beetle, "Agitha Male Beetle Reward" },
                { Item.Male_Butterfly, "Agitha Male Butterfly Reward" },
                { Item.Male_Dayfly, "Agitha Male Dayfly Reward" },
                { Item.Male_Dragonfly, "Agitha Male Dragonfly Reward" },
                { Item.Male_Grasshopper, "Agitha Male Grasshopper Reward" },
                { Item.Male_Ladybug, "Agitha Male Ladybug Reward" },
                { Item.Male_Mantis, "Agitha Male Mantis Reward" },
                { Item.Male_Phasmid, "Agitha Male Phasmid Reward" },
                { Item.Male_Pill_Bug, "Agitha Male Pill Bug Reward" },
                { Item.Male_Snail, "Agitha Male Snail Reward" },
                { Item.Male_Stag_Beetle, "Agitha Male Stag Beetle Reward" },
                { Item.Asheis_Sketch, "Gift From Ralis" },
            };

        public static readonly Dictionary<string, Item> tradeRewardCheckToSourceItem;

        private static readonly Dictionary<string, string> cachedCheckToHintZoneMap = new();

        static HintUtils()
        {
            tradeRewardCheckToSourceItem = new();
            foreach (KeyValuePair<Item, string> pair in tradeItemToRewardCheck)
            {
                tradeRewardCheckToSourceItem[pair.Value] = pair.Key;
            }

            //         { Item.Female_Ant, "Agitha Female Ant Reward" },
            //         { Item.Female_Beetle, "Agitha Female Beetle Reward" },
            //         { Item.Female_Butterfly, "Agitha Female Butterfly Reward" },
            //         { Item.Female_Dayfly, "Agitha Female Dayfly Reward" },
            //         { Item.Female_Dragonfly, "Agitha Female Dragonfly Reward" },
            //         { Item.Female_Grasshopper, "Agitha Female Grasshopper Reward" },
            //         { Item.Female_Ladybug, "Agitha Female Ladybug Reward" },
            //         { Item.Female_Mantis, "Agitha Female Mantis Reward" },
            //         { Item.Female_Phasmid, "Agitha Female Phasmid Reward" },
            //         { Item.Female_Pill_Bug, "Agitha Female Pill Bug Reward" },
            //         { Item.Female_Snail, "Agitha Female Snail Reward" },
            //         { Item.Female_Stag_Beetle, "Agitha Female Stag Beetle Reward" },
            //         { Item.Male_Ant, "Agitha Male Ant Reward" },
            //         { Item.Male_Beetle, "Agitha Male Beetle Reward" },
            //         { Item.Male_Butterfly, "Agitha Male Butterfly Reward" },
            //         { Item.Male_Dayfly, "Agitha Male Dayfly Reward" },
            //         { Item.Male_Dragonfly, "Agitha Male Dragonfly Reward" },
            //         { Item.Male_Grasshopper, "Agitha Male Grasshopper Reward" },
            //         { Item.Male_Ladybug, "Agitha Male Ladybug Reward" },
            //         { Item.Male_Mantis, "Agitha Male Mantis Reward" },
            //         { Item.Male_Phasmid, "Agitha Male Phasmid Reward" },
            //         { Item.Male_Pill_Bug, "Agitha Male Pill Bug Reward" },
            //         { Item.Male_Snail, "Agitha Male Snail Reward" },
            //         { Item.Male_Stag_Beetle, "Agitha Male Stag Beetle Reward" },
            //         { Item.Asheis_Sketch, "Gift From Ralis" },
        }

        public static string getDependentDungeonForCheckName(string checkName)
        {
            if (checkName == null || checkName == "")
            {
                return null;
            }

            Check check = Randomizer.Checks.CheckDict[checkName];
            if (check == null)
            {
                return null;
            }

            for (int i = 0; i < check.checkCategory.Count; i++)
            {
                string categoryName = check.checkCategory[i];
                if (HintConstants.jsonCategoryToDungeonZoneName.ContainsKey(categoryName))
                {
                    string dungeonZoneName = HintConstants.jsonCategoryToDungeonZoneName[
                        categoryName
                    ];
                    if (HintConstants.dungeonZonesToRequiredMaskMap.ContainsKey(dungeonZoneName))
                    {
                        return dungeonZoneName;
                    }
                }
            }

            // If no dungeonZone name, check if check is hardcoded post-dungeon check.
            if (HintConstants.postDungeonChecksToDungeonZone.ContainsKey(checkName))
            {
                return HintConstants.postDungeonChecksToDungeonZone[checkName];
            }

            return null;
        }

        public static Dictionary<string, string[]> getHintZoneToChecksMap()
        {
            return ZoneUtils.zoneNameToChecks;
        }

        public static Dictionary<string, string> getCheckToHintZoneMap()
        {
            if (cachedCheckToHintZoneMap.Count > 0)
                return cachedCheckToHintZoneMap;

            Dictionary<string, string[]> hintZoneToChecksMap = getHintZoneToChecksMap();
            Dictionary<string, string> checkToHintZoneMap = convertToCheckToHintZoneMap(
                hintZoneToChecksMap
            );

            foreach (KeyValuePair<string, string> pair in checkToHintZoneMap)
            {
                cachedCheckToHintZoneMap.Add(pair.Key, pair.Value);
            }

            return checkToHintZoneMap;
        }

        private static Dictionary<string, string> convertToCheckToHintZoneMap(
            Dictionary<string, string[]> hintZoneToChecksMap
        )
        {
            Dictionary<string, string> checkToHintZoneMap = new();

            foreach (KeyValuePair<string, string[]> kv in hintZoneToChecksMap)
            {
                string zoneName = kv.Key;
                string[] checksForZone = kv.Value;

                for (int i = 0; i < checksForZone.Length; i++)
                {
                    checkToHintZoneMap.Add(checksForZone[i], zoneName);
                }
            }

            return checkToHintZoneMap;
        }

        public static bool DungeonIsRequired(string dungeonHintZoneName)
        {
            return (
                    Randomizer.RequiredDungeons
                    & HintConstants.dungeonZonesToRequiredMaskMap[dungeonHintZoneName]
                ) != 0;
        }

        public static HashSet<string> getRequiredDungeonZones()
        {
            HashSet<string> result = new();

            foreach (KeyValuePair<string, byte> pair in HintConstants.dungeonZonesToRequiredMaskMap)
            {
                if ((Randomizer.RequiredDungeons & pair.Value) != 0)
                {
                    result.Add(pair.Key);
                }
            }

            return result;
        }

        public static HashSet<string> calculateRequiredChecks(
            Room startingRoom,
            List<List<KeyValuePair<int, Item>>> spheres
        )
        {
            HashSet<string> maybeRequiredCheckNames = new();

            foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
            {
                foreach (KeyValuePair<int, Item> pair in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(pair.Key);
                    maybeRequiredCheckNames.Add(checkName);
                }
            }

            // HashSet<string> requiredChecks = filterToRequiredChecksByPlaythroughs(
            //     startingRoom,
            //     maybeRequiredCheckNames
            // );
            // return requiredChecks;

            HashSet<Goal> goals = new() { GoalConstants.Ganondorf };

            Dictionary<Goal, List<string>> goalToRequiredChecks = filterToRequiredChecksOfGoals(
                startingRoom,
                maybeRequiredCheckNames,
                goals,
                false
            );

            HashSet<string> requiredChecks = new();
            foreach (KeyValuePair<Goal, List<string>> pair in goalToRequiredChecks)
            {
                List<string> checkNames = pair.Value;
                if (checkNames != null)
                {
                    foreach (string checkName in checkNames)
                    {
                        requiredChecks.Add(checkName);
                    }
                }
            }

            return requiredChecks;
        }

        public static Dictionary<Goal, List<string>> calculateGoalsRequiredChecks(
            Room startingRoom,
            List<List<KeyValuePair<int, Item>>> spheres
        )
        {
            HashSet<string> maybeRequiredCheckNames = new();

            foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
            {
                foreach (KeyValuePair<int, Item> pair in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(pair.Key);
                    maybeRequiredCheckNames.Add(checkName);
                }
            }

            HashSet<Goal> goalsFromDungeons = getGoalsBasedOnDungeons();

            return filterToRequiredChecksOfGoals(
                startingRoom,
                maybeRequiredCheckNames,
                goalsFromDungeons,
                true
            );
        }

        private static HashSet<Goal> getGoalsBasedOnDungeons()
        {
            HashSet<string> requiredDungeons = getRequiredDungeonZones();

            HashSet<Goal> result = new();
            foreach (string dungeonZone in requiredDungeons)
            {
                if (GoalConstants.requiredDungeonHintZoneToGoal.ContainsKey(dungeonZone))
                {
                    Goal goal = GoalConstants.requiredDungeonHintZoneToGoal[dungeonZone];
                    result.Add(goal);
                }
            }

            result.Add(GoalConstants.Hyrule_Castle);
            result.Add(GoalConstants.Ganondorf);

            return result;
        }

        // private static HashSet<string> filterToRequiredChecksByPlaythroughs(
        //     Room startingRoom,
        //     HashSet<string> maybeRequiredCheckNames
        // )
        // {
        //     HashSet<string> requiredChecks = new();

        //     Dictionary<Goal, List<string>> goalsToRequiredChecks = filterToRequiredChecksOfGoals(
        //         startingRoom,
        //         maybeRequiredCheckNames
        //     );
        //     foreach (KeyValuePair<Goal, List<string>> pair in goalsToRequiredChecks)
        //     {
        //         List<string> checkNames = pair.Value;
        //         if (checkNames != null)
        //         {
        //             foreach (string checkName in checkNames)
        //             {
        //                 requiredChecks.Add(checkName);
        //             }
        //         }
        //     }

        //     return requiredChecks;
        // }

        private static Dictionary<Goal, List<string>> filterToRequiredChecksOfGoals(
            Room startingRoom,
            HashSet<string> maybeRequiredCheckNames,
            HashSet<Goal> goals,
            bool startWithBigKeys
        )
        {
            Dictionary<Goal, List<string>> goalsToRequiredChecks = new();
            foreach (Goal goal in goals)
            {
                goalsToRequiredChecks[goal] = new();
            }

            // HashSet<string> filteredCheckNames = new();

            // I think it is safe to only generate the item pool once up front.
            Randomizer.Items.GenerateItemPool();

            // After we get potential checks, filter out any which can be
            // removed in isolation and the playthrough is still valid.
            foreach (string checkName in maybeRequiredCheckNames)
            {
                // Replace check contents with a green rupee. If the playthrough
                // is still beatable, then that item cannot be considered for
                // SpoL hints.
                Item originalContents = Randomizer.Checks.CheckDict[checkName].itemId;
                Randomizer.Checks.CheckDict[checkName].itemId = Item.Green_Rupee;

                // bool successWithoutCheck = BackendFunctions.emulatePlaythrough(startingRoom);
                Dictionary<Goal, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                    startingRoom,
                    goals,
                    startWithBigKeys
                );

                foreach (KeyValuePair<Goal, bool> pair in goalResults)
                {
                    if (pair.Value)
                    {
                        // Was able to complete goal after replacing check contents.
                        Console.WriteLine(
                            $"NOT needed for Goal {pair.Key.id}: {originalContents} in {checkName}"
                        );
                    }
                    else
                    {
                        // Was unable to complete goal after replacing check contents.
                        goalsToRequiredChecks[pair.Key].Add(checkName);
                        Console.WriteLine(
                            $"Needed for Goal {pair.Key.id}: {originalContents} in {checkName}"
                        );
                    }
                }

                // bool successWithoutCheck = true;

                // if (!successWithoutCheck)
                // {
                //     Console.WriteLine($"Needed for SpoL: {originalContents}: {checkName}");
                //     // Check required to beat the seed, so add it for
                //     // consideration for SpoL hints.
                //     filteredCheckNames.Add(checkName);
                // }
                // else
                // {
                //     Console.WriteLine($"Not needed for SpoL: {originalContents}: {checkName}");
                // }

                // Put the original item back.
                Randomizer.Checks.CheckDict[checkName].itemId = originalContents;
            }

            // return filteredCheckNames;
            return goalsToRequiredChecks;
        }

        private static Dictionary<Item, List<string>> calcItemToChecksList()
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

        public static Dictionary<Item, Dictionary<Goal, bool>> checkGoalsWithoutItems(
            Room startingRoom,
            List<Item> items,
            HashSet<Goal> goals
        )
        {
            Dictionary<Item, List<string>> itemToChecks = calcItemToChecksList();

            // I think it is safe to only generate the item pool once up front.
            Randomizer.Items.GenerateItemPool();

            Dictionary<Item, Dictionary<Goal, bool>> results = new();

            // After we get potential checks, filter out any which can be
            // removed in isolation and the playthrough is still valid.
            foreach (Item item in items)
            {
                // TODO: this needs to handle when the item does not show up.
                // For example, when the player starts with the only Boomerang.
                itemToChecks.TryGetValue(item, out List<string> checkNames);
                if (ListUtils.isEmpty(checkNames))
                {
                    results[item] = new();
                    checkNames = new();
                }
                List<Item> originalContentsList = new();

                foreach (string checkName in checkNames)
                {
                    // Replace check contents with a green rupee and see which
                    // goals are completable.
                    Item originalContents = Randomizer.Checks.CheckDict[checkName].itemId;
                    originalContentsList.Add(originalContents);
                    Randomizer.Checks.CheckDict[checkName].itemId = Item.Green_Rupee;
                }

                Dictionary<Goal, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                    startingRoom,
                    goals,
                    true
                );

                results[item] = goalResults;

                // Put the original items back.
                for (int i = 0; i < checkNames.Count; i++)
                {
                    string checkName = checkNames[i];
                    Item originalContents = originalContentsList[i];
                    Randomizer.Checks.CheckDict[checkName].itemId = originalContents;
                }
            }

            return results;
        }

        public static void ShuffleListInPlace<T>(Random rnd, IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<T> MergeListsRandomly<T>(Random rnd, params List<T>[] lists)
        {
            List<List<T>> newLists = new();
            foreach (List<T> list in lists)
            {
                List<T> listCopy = new(list);
                newLists.Add(listCopy);
            }

            List<T> outputList = new();

            while (true)
            {
                if (newLists.Count < 1)
                    break;

                int listIndex = rnd.Next(newLists.Count);
                List<T> list = newLists[listIndex];
                if (list.Count < 1)
                {
                    // remove this list and continue
                    newLists.RemoveAt(listIndex);
                    continue;
                }

                outputList.Add(list[0]);
                list.RemoveAt(0);
            }

            return outputList;
        }

        public static T RemoveRandomListItem<T>(Random rnd, List<T> list)
        {
            int index = rnd.Next(list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static T RemoveRandomHashSetItem<T>(Random rnd, HashSet<T> hashSet)
        {
            List<T> list = new(hashSet);
            int index = rnd.Next(list.Count);
            T item = list[index];
            hashSet.Remove(item);
            return item;
        }

        public static KeyValuePair<A, B> RemoveRandomDictionaryItem<A, B>(
            Random rnd,
            Dictionary<A, B> dict
        )
        {
            List<KeyValuePair<A, B>> list = new(dict);
            int index = rnd.Next(list.Count);
            KeyValuePair<A, B> item = list[index];
            dict.Remove(item.Key);
            return item;
        }

        public static KeyValuePair<A, B> PickRandomDictionaryPair<A, B>(
            Random rnd,
            Dictionary<A, B> dict
        )
        {
            int desiredIndex = rnd.Next(dict.Count);
            int i = 0;
            foreach (KeyValuePair<A, B> pair in dict)
            {
                if (i == desiredIndex)
                    return pair;
                i += 1;
            }
            throw new Exception("Failed to pick random dictionary key.");
        }

        public static T PickRandomListItem<T>(Random rnd, IList<T> list)
        {
            if (ListUtils.isEmpty(list))
                throw new Exception("Cannot pick a random item from null or empty list.");

            int randomIndex = rnd.Next(list.Count);
            return list[randomIndex];
        }

        public static string PickRandomCheckByGroup(
            Random rnd,
            IEnumerable<string> checksEnumerable,
            bool byProvince = false
        )
        {
            Dictionary<string, List<string>> checksByGroup = new();

            foreach (string checkName in checksEnumerable)
            {
                if (byProvince)
                {
                    Province province = checkNameToHintProvince(checkName);
                    string provinceName = ProvinceUtils.IdToString(province);
                    if (!checksByGroup.ContainsKey(provinceName))
                        checksByGroup[provinceName] = new();
                    checksByGroup[provinceName].Add(checkName);
                }
                else
                {
                    string zoneName = checkNameToHintZone(checkName);
                    if (!checksByGroup.ContainsKey(zoneName))
                        checksByGroup[zoneName] = new();
                    checksByGroup[zoneName].Add(checkName);
                }
            }

            if (checksByGroup.Count < 1)
                throw new Exception("checkByGroup is empty.");

            KeyValuePair<string, List<string>> groupAndChecks = PickRandomDictionaryPair(
                rnd,
                checksByGroup
            );

            List<string> checksOfGroup = groupAndChecks.Value;
            string selectedCheckName = RemoveRandomListItem(rnd, checksOfGroup);
            return selectedCheckName;
        }

        public static bool checkIsPlayerKnownStatus(string checkName)
        {
            string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
            return HintConstants.preventBarrenHintIfAllCheckStatusesAre.Contains(checkStatus);
        }

        public static bool checkIsExcluded(string checkName)
        {
            string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
            return HintConstants.excludedCheckStatuses.Contains(checkStatus);
        }

        public static bool checksAllPlayerKnownStatus(IEnumerable<string> checkNames)
        {
            foreach (string checkName in checkNames)
            {
                if (!checkIsPlayerKnownStatus(checkName))
                    return false;
            }
            return true;
        }

        public static bool isItemGoldenBug(Item item)
        {
            return HintConstants.bugsToRewardChecksMap.ContainsKey(item);
        }

        public static bool isItemMaleBug(Item item)
        {
            return isItemGoldenBug(item) && item.ToString().StartsWith("Male");
        }

        public static bool isItemFemaleBug(Item item)
        {
            return isItemGoldenBug(item) && item.ToString().StartsWith("Female");
        }

        public static bool hintZoneIsDungeon(string hintZone)
        {
            return HintConstants.dungeonZones.Contains(hintZone);
        }

        public static string checkNameToHintZone(string checkName)
        {
            return getCheckToHintZoneMap()[checkName];
        }

        public static Province checkNameToHintProvince(string checkName)
        {
            string hintZone = checkNameToHintZone(checkName);

            if (HintConstants.zoneToProvince.ContainsKey(hintZone))
                return HintConstants.zoneToProvince[hintZone];

            return Province.Invalid;
        }

        public static Item getCheckContents(string checkName)
        {
            return Randomizer.Checks.CheckDict[checkName].itemId;
        }

        public static Item getCheckContents(string checkName, Dictionary<int, byte> itemPlacements)
        {
            int srcCheckId = CheckIdClass.GetCheckIdNum(checkName);
            return (Item)itemPlacements[srcCheckId];
        }

        public static HashSet<string> GetChecksForProvince(Province province)
        {
            HashSet<string> checkNames = new();
            Dictionary<string, string[]> zoneToChecks = getHintZoneToChecksMap();

            foreach (Zone zone in ProvinceUtils.ProvinceToZones(province))
            {
                string zoneName = ZoneUtils.IdToString(zone);
                string[] checks = zoneToChecks[zoneName];
                foreach (string check in checks)
                {
                    checkNames.Add(check);
                }
            }
            return checkNames;
        }

        public static HashSet<string> GetChecksForZone(Zone zone)
        {
            HashSet<string> checkNames = new();
            Dictionary<string, string[]> zoneToChecks = getHintZoneToChecksMap();

            string zoneName = ZoneUtils.IdToString(zone);
            string[] checks = zoneToChecks[zoneName];
            foreach (string check in checks)
            {
                checkNames.Add(check);
            }
            return checkNames;
        }

        public static SpotId TryGetSpotIdForBarrenZoneHint(Hint hint)
        {
            if (hint == null)
                throw new Exception("Called TryGetSpotIdForBarrenZoneHint with a null hint.");

            BarrenHint barrenHint = hint as BarrenHint;
            if (barrenHint != null)
            {
                AreaId areaId = barrenHint.areaId;
                if (areaId.type == AreaId.AreaType.Zone)
                {
                    Zone zone = ZoneUtils.StringToId(areaId.stringId);
                    if (zone == Zone.Invalid)
                        throw new Exception(
                            $"Expected to be able to parse stringId '{areaId.stringId}' to a valid zone."
                        );

                    return ZoneUtils.IdToSpotId(zone);
                }
            }
            return SpotId.Invalid;
        }

        public static List<string> GetTradeChainAllChecks(string srcCheckName)
        {
            if (!CheckIdClass.IsValidCheckName(srcCheckName))
                throw new Exception($"'{srcCheckName}' is not a valid checkName.");

            string currCheckName = srcCheckName;
            Item currItem = getCheckContents(srcCheckName);
            if (!tradeItemToRewardCheck.ContainsKey(currItem))
            {
                throw new Exception(
                    $"Cannot check trade chain on non-tradeItem '{currItem}' from checkName '{currCheckName}'."
                );
            }

            List<string> checks = new();
            HashSet<Item> seenItems = new();
            checks.Add(currCheckName);
            seenItems.Add(currItem);

            while (tradeItemToRewardCheck.ContainsKey(currItem))
            {
                currCheckName = tradeItemToRewardCheck[currItem];
                currItem = getCheckContents(currCheckName);
                // If there is a circular chain, we return null. For example,
                // MaleMantis => FemaleButterfly => MaleMantis.
                if (seenItems.Contains(currItem))
                    return null;
                seenItems.Add(currItem);
                checks.Add(currCheckName);
            }

            return checks;
        }

        public static string GetTradeChainFinalCheck(string srcCheckName)
        {
            List<string> chainChecks = GetTradeChainAllChecks(srcCheckName);
            if (!ListUtils.isEmpty(chainChecks))
                return chainChecks[chainChecks.Count - 1];
            return null;
        }

        public static string GetTradeChainFinalCheck(
            string srcCheckName,
            Dictionary<int, byte> itemPlacements
        )
        {
            if (!CheckIdClass.IsValidCheckName(srcCheckName))
                throw new Exception($"'{srcCheckName}' is not a valid checkName.");

            string currCheckName = srcCheckName;
            Item currItem = getCheckContents(srcCheckName, itemPlacements);
            while (tradeItemToRewardCheck.ContainsKey(currItem))
            {
                currCheckName = tradeItemToRewardCheck[currItem];
                currItem = getCheckContents(currCheckName, itemPlacements);
            }

            return currCheckName;
        }

        public static bool TradeChainContainsItem(string startCheckName, HashSet<Item> items)
        {
            if (ListUtils.isEmpty(items))
                return false;

            List<string> chainChecks = GetTradeChainAllChecks(startCheckName);
            if (!ListUtils.isEmpty(chainChecks))
            {
                foreach (string checkName in chainChecks)
                {
                    Item item = getCheckContents(checkName);
                    if (items.Contains(item))
                        return true;
                }
            }
            return false;
        }

        public static bool IsTradeItem(Item item)
        {
            return tradeItemToRewardCheck.ContainsKey(item);
        }

        public static bool CheckIsTradeItemReward(string checkName)
        {
            return tradeRewardCheckToSourceItem.ContainsKey(checkName);
        }

        public static bool IsTrapItem(Item item)
        {
            switch (item)
            {
                case Item.Foolish_Item:
                case Item.Foolish_Item_2:
                case Item.Foolish_Item_3:
                    return true;
                default:
                    return false;
            }
        }

        private static byte calcNumBitsForHintsAtSpot(List<HintSpot> hintSpots)
        {
            if (hintSpots != null && hintSpots.Count > 0)
            {
                int mostHintsPerSpot = 0;
                foreach (HintSpot spot in hintSpots)
                {
                    if (spot != null && spot.hints != null && spot.hints.Count > mostHintsPerSpot)
                        mostHintsPerSpot = spot.hints.Count;
                }

                if (mostHintsPerSpot > 0)
                    return GetBitsNeededForNum(mostHintsPerSpot);
            }

            return 1;
        }

        private static byte GetBitsNeededForNum(int num)
        {
            for (byte i = 1; i <= 16; i++)
            {
                int oneOverMax = 1 << i;
                if (num < oneOverMax)
                {
                    return i;
                }
            }
            return 32;
        }

        public static HintEncodingBitLengths GetHintEncodingBitLengths(List<HintSpot> hintSpots)
        {
            return new(
                HintTypeUtils.NumBitsToEncode,
                9,
                ZoneUtils.NumBitsToEncode,
                HintCategoryUtils.NumBitsToEncode,
                AreaId.NumBitsToEncode,
                ProvinceUtils.NumBitsToEncode,
                HintSpotLocationUtils.NumBitsToEncode,
                GoalConstants.NumBitsToEncode,
                TradeGroupUtils.NumBitsToEncode,
                calcNumBitsForHintsAtSpot(hintSpots)
            );
        }
    }
}
