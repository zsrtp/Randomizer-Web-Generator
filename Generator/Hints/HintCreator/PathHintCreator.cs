namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class PathHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Path;

        private PathHintCreator() { }

        new public static PathHintCreator fromJObject(JObject obj)
        {
            return new PathHintCreator();
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            List<PathHint> pathHints = genPathHints(genData, numHints);
            if (pathHints != null)
            {
                List<Hint> a = pathHints.ConvertAll(x => (Hint)x);
                return a;
            }
            return null;
        }

        private List<PathHint> genPathHints(HintGenData genData, int numHintsDesired)
        {
            Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap();

            List<List<KeyValuePair<Goal, List<string>>>> lists = splitPathGoalsToLists(genData);
            List<KeyValuePair<Goal, List<string>>> primaryList = lists[0];
            List<KeyValuePair<Goal, List<string>>> secondaryList = lists[1];

            // Then we pick from the primary list. Up to the size of the list, max is desired count.
            // Need primary list and desired hints (can be over max; will just return a certain count).
            // Also will resort the list in place based on which ones were picked (not picked moved to end).
            List<PathHint> pathHints = pickPrimaryPathHints(genData, primaryList, numHintsDesired);

            List<KeyValuePair<Goal, List<string>>> combinedList = new List<
                KeyValuePair<Goal, List<string>>
            >()
                .Concat(primaryList)
                .Concat(secondaryList)
                .ToList();

            Dictionary<Goal, HashSet<string>> goalToHintedZones = new();
            Dictionary<GoalEnum, Goal> goalEnumToGoal = new();
            foreach (KeyValuePair<Goal, List<string>> pair in combinedList)
            {
                goalToHintedZones[pair.Key] = new();
                goalEnumToGoal[pair.Key.goalEnum] = pair.Key;
            }

            // Mark hinted checks and for each goal which zones have been hinted.
            HashSet<string> hintedChecks = new();
            foreach (PathHint hint in pathHints)
            {
                hintedChecks.Add(hint.checkName);
                string zoneName = checkToHintZoneMap[hint.checkName];
                Goal goal = goalEnumToGoal[hint.goalEnum];
                goalToHintedZones[goal].Add(zoneName);
            }

            // For each goal, keep track of which zones have been hinted.
            // When iterating on a goal, filter out checks which have been hinted.
            // Reconstruct zone lists, giving priority to zones for that goal which have not been hinted.
            // Pick random zone for that goal.
            // Mark zone as hinted for that goal.
            // Pick random check for that zone which has not been hinted.
            // Continue.

            // foreach (KeyValuePair<Goal, List<string>> pair in combinedList)
            // {
            //     List<string> filteredChecks = pair.Value.Where(checkName => !hintedChecks.Contains(checkName)).ToList();
            //     pair.Value.Clear();
            //     pair.Value.AddRange(filteredChecks);
            // }

            int currentIndex = pathHints.Count;
            if (currentIndex >= combinedList.Count)
                currentIndex = 0;

            // TODO: Need to rewrite at the zone level.

            while (pathHints.Count < numHintsDesired && combinedList.Count > 0)
            {
                KeyValuePair<Goal, List<string>> pair = combinedList[currentIndex];
                // Filter already hinted checks from list
                pair.Value.RemoveAll(checkName => hintedChecks.Contains(checkName));

                if (pair.Value.Count < 1)
                {
                    // If no available checks for goal, filter out goal from options.
                    combinedList.RemoveAt(currentIndex);
                    if (currentIndex >= combinedList.Count)
                        currentIndex = 0;
                    continue;
                }

                // Condense remaining checks for checkList into priorityZones and not priority zones.
                HashSet<string> priorityZones = new();
                HashSet<string> secondaryZones = new();
                foreach (string checkName in pair.Value)
                {
                    string hintZone = checkToHintZoneMap[checkName];
                    if (goalToHintedZones[pair.Key].Contains(hintZone))
                        secondaryZones.Add(hintZone);
                    else
                        priorityZones.Add(hintZone);
                }

                string selectedZone;
                if (priorityZones.Count > 0)
                    selectedZone = HintUtils.RemoveRandomHashSetItem(genData.rnd, priorityZones);
                else
                    selectedZone = HintUtils.RemoveRandomHashSetItem(genData.rnd, secondaryZones);

                // Pick random check for that zone
                List<string> checksForZone = pair.Value.FindAll(
                    checkName => checkToHintZoneMap[checkName] == selectedZone
                );
                string selectedCheckName = HintUtils.RemoveRandomListItem(
                    genData.rnd,
                    checksForZone
                );
                pair.Value.Remove(selectedCheckName);

                // Mark zone as hinted for that goal
                goalToHintedZones[pair.Key].Add(selectedZone);

                // Create path hint for this goal and check
                Item contents = HintUtils.getCheckContents(selectedCheckName);
                // Mark checkName as directed toward
                genData.hinted.alreadyCheckDirectedToward.Add(selectedCheckName);
                hintedChecks.Add(selectedCheckName);

                PathHint hint = new PathHint(
                    AreaId.ZoneStr(selectedZone),
                    selectedCheckName,
                    pair.Key.goalEnum
                );
                pathHints.Add(hint);

                currentIndex = (currentIndex + 1) % combinedList.Count;
            }

            // // TODO: pick up with picking more hints. Not needed for playtesting
            // // current settings. (This error should be pushed down to the bottom
            // // when we truly fail; not expected to ever really happen).
            // if (primaryPathHints.Count < numHintsDesired)
            //     throw new Exception(
            //         $"Wanted {numHintsDesired} path hints, but only generated {primaryPathHints.Count}."
            //     );

            return pathHints;

            // // calculate from goalToRequiredChecks
            // Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
            //     sSettings
            // );

            // // Ideal, pick 1 hint from the required dungeons, up to the desired
            // // count or the numDungeons. These should be different zones for
            // // each. Then continue from the secondary list and loop back around
            // // everything until there are no more hints to give or no more
            // // desired.

            // // If unable to pick unique zones from required dungeons, then skip
            // // this step and begin looping through (might be partway through
            // // primary list).

            // // First we build the primary (dungeons with path checks) and
            // // secondary lists (hyrule castle and ganondorf; filtered based on
            // // previous checks).

            // Dictionary<Goal, List<string>> goalToZones = new();

            // foreach (KeyValuePair<Goal, List<string>> pair in goalToRequiredChecks)
            // {
            //     // if (!GoalConstants.IsDungeonGoal(pair.Key))
            //     //     continue;

            //     HashSet<string> zonesForGoal = new();

            //     List<string> checkNames = pair.Value;
            //     if (checkNames == null)
            //         continue;

            //     foreach (string checkName in checkNames)
            //     {
            //         if (!checkCanBeHintedSpol(checkName))
            //             continue;

            //         string zoneName = checkToHintZoneMap[checkName];
            //         zonesForGoal.Add(zoneName);
            //     }

            //     if (zonesForGoal.Count > 0)
            //     {
            //         List<string> zonesForGoalList = new List<string>()
            //             .Concat(zonesForGoal)
            //             .ToList();
            //         HintUtils.ShuffleListInPlace(rnd, zonesForGoalList);
            //         goalToZones[pair.Key] = zonesForGoalList;
            //     }
            // }

            // // if (goalToZones.Count != 3)
            // if (goalToZones.Count < 3)
            // {
            //     throw new Exception($"Expected 3 goals but only found {goalToZones.Count}.");
            // }

            // List<KeyValuePair<Goal, List<string>>> goalToZonesList = new();
            // foreach (KeyValuePair<Goal, List<string>> pair in goalToZones)
            // {
            //     goalToZonesList.Add(pair);
            // }

            // HashSet<string> results = new();

            // Dictionary<string, double> zoneWeightings = getZoneWeightings();

            // // commented out so can change function signature
            // // recurPickUniqueZonesCombo(results, goalToZonesList, new(), new());

            // if (results.Count == 0)
            //     throw new Exception($"Expected some results, but there were none.");

            // // From valid checks to hint, get a list of zones for each goal.

            // // Then we need to find distributions for the first 3 dungeons which
            // // are a different zone for each if possible.

            // List<KeyValuePair<double, string>> weightedList = new();
            // foreach (string comboId in results)
            // {
            //     string[] zones = comboId.Split("###");
            //     List<double> weights = new();
            //     double lowestWeight = 1000.0;

            //     double avgWeight = 0.0;

            //     for (int i = 0; i < zones.Length; i++)
            //     {
            //         string zone = zones[i];
            //         double weight = zoneWeightings[zone];
            //         if (weight < lowestWeight)
            //         {
            //             lowestWeight = weight;
            //         }
            //         avgWeight += weight;
            //     }

            //     avgWeight += lowestWeight * 7;
            //     avgWeight /= zones.Length + 7;

            //     weightedList.Add(new(avgWeight, comboId));
            // }

            // VoseInstance<string> voseInst = VoseAlgorithm.createInstance(weightedList);
            // string selectedComboId = voseInst.NextAndKeep(rnd);

            // string[] selectedZones = selectedComboId.Split("###");
            // List<PathHint> pathHints = new();

            // for (int i = 0; i < selectedZones.Length; i++)
            // {
            //     string desiredZoneName = selectedZones[i];
            //     Goal goal = goalToZonesList[i].Key;
            //     List<string> requiredChecksOfGoal = goalToRequiredChecks[goal];

            //     List<string> requiredChecksInZone = new();
            //     foreach (string checkName in requiredChecksOfGoal)
            //     {
            //         if (checkToHintZoneMap[checkName] == desiredZoneName)
            //         {
            //             requiredChecksInZone.Add(checkName);
            //         }
            //     }

            //     HintUtils.ShuffleListInPlace(rnd, requiredChecksInZone);
            //     string selectedCheckName = requiredChecksInZone[0];
            //     Item contents = getCheckContents(selectedCheckName);
            //     // Mark checkName as directed toward
            //     alreadyCheckDirectedToward.Add(selectedCheckName);

            //     PathHint hint = new PathHint(
            //         TypedId.Zone(desiredZoneName),
            //         selectedCheckName,
            //         goal.goalEnum
            //     );
            //     pathHints.Add(hint);

            //     // Create a hint from the selectedCheckName and the goal.
            // }

            // // Get every possible combination of zone selections for each which has no repeats.

            // // This can probably be done recursively.
            // return pathHints;
        }

        private List<List<KeyValuePair<Goal, List<string>>>> splitPathGoalsToLists(
            HintGenData genData
        )
        {
            List<KeyValuePair<Goal, List<string>>> primaryList = new();
            List<KeyValuePair<Goal, List<string>>> secondaryList = new();

            // Ideal, pick 1 hint from the required dungeons, up to the desired
            // count or the numDungeons. These should be different zones for
            // each. Then continue from the secondary list and loop back around
            // everything until there are no more hints to give or no more
            // desired.

            // If unable to pick unique zones from required dungeons, then skip
            // this step and begin looping through (might be partway through
            // primary list).

            // First we build the primary (dungeons with path checks) and
            // secondary lists (hyrule castle and ganondorf; filtered based on
            // previous checks).

            HashSet<string> checkNamesForPrimaryGoals = new();

            Dictionary<Goal, List<string>> goalToZones = new();

            foreach (KeyValuePair<Goal, List<string>> pair in genData.goalToRequiredChecks)
            {
                List<string> checkNames = pair.Value;
                if (ListUtils.isEmpty(checkNames))
                    continue;

                bool isPrimaryGoal = GoalConstants.IsDungeonGoal(pair.Key);
                List<string> canBeHintedCheckNames = new();

                foreach (string checkName in checkNames)
                {
                    if (!genData.checkCanBeHintedSpol(checkName))
                        continue;

                    canBeHintedCheckNames.Add(checkName);
                    if (isPrimaryGoal)
                    {
                        checkNamesForPrimaryGoals.Add(checkName);
                    }
                }

                if (canBeHintedCheckNames.Count > 0)
                {
                    if (isPrimaryGoal)
                        primaryList.Add(new(pair.Key, canBeHintedCheckNames));
                    else
                        secondaryList.Add(new(pair.Key, canBeHintedCheckNames));
                }
            }
            HintUtils.ShuffleListInPlace(genData.rnd, primaryList);

            // filter secondary list items to not include ones in primary
            secondaryList = filterPathHintSecondaryList(secondaryList, checkNamesForPrimaryGoals);

            return new() { primaryList, secondaryList };
        }

        private List<PathHint> pickPrimaryPathHints(
            HintGenData genData,
            List<KeyValuePair<Goal, List<string>>> primaryList,
            int numHintsDesired
        )
        {
            if (numHintsDesired < 1 || ListUtils.isEmpty(primaryList))
                return new();

            int numHintsToPick =
                numHintsDesired > primaryList.Count ? primaryList.Count : numHintsDesired;

            List<List<int>> combinations = getCombinationIndexes(primaryList.Count, numHintsToPick);

            reorderPathPrimaryList(genData, primaryList);

            return pickPrimaryPathHintsFromCombinations(
                genData,
                primaryList,
                numHintsToPick,
                combinations
            );
        }

        private List<List<int>> getCombinationIndexes(int totalLength, int numToPick)
        {
            // This returns every unique combination of indexes from doing nCr.
            // For example, if we had 4 choose 2, then we would get:
            // [[0,1],[0,2],[0,3],[1,2],[1,3],[2,3]]

            if (totalLength < 1)
                throw new Exception($"totalLength must be at least 1. Received '{totalLength}'.");
            else if (numToPick > totalLength)
                throw new Exception(
                    $"numToPick must not exceed totalLength. Received totalLength '{totalLength}' and numToPick '{numToPick}'."
                );

            List<int> sourceArr = new(totalLength);
            for (int i = 0; i < totalLength; i++)
            {
                sourceArr.Add(i);
            }

            List<List<int>> results = new();
            recurGetCombinationIndexes(numToPick, sourceArr, results);
            return results;
        }

        private void recurGetCombinationIndexes(
            int numToPick,
            List<int> selectionSource,
            List<List<int>> results,
            int startIndex = 0,
            List<int> pushedItems = null
        )
        {
            if (pushedItems == null)
                pushedItems = new(numToPick);

            for (int i = startIndex; i < selectionSource.Count; i++)
            {
                if (pushedItems.Count + 1 < numToPick)
                {
                    pushedItems.Add(selectionSource[i]);
                    recurGetCombinationIndexes(
                        numToPick,
                        selectionSource,
                        results,
                        i + 1,
                        pushedItems
                    );
                    pushedItems.RemoveAt(pushedItems.Count - 1);
                }
                else
                {
                    List<int> b = new(pushedItems);
                    b.Add(selectionSource[i]);
                    results.Add(b);
                }
            }
        }

        private void reorderPathPrimaryList(
            HintGenData genData,
            List<KeyValuePair<Goal, List<string>>> primaryList
        )
        {
            // We want to reorder the primary list so that if we are generating
            // 3 path hints with 5 required dungeons, we are likely to provide
            // hints for any combination of dungeons rather than always picking
            // from the front.

            List<KeyValuePair<double, int>> weightedIndexes = new();
            for (int i = 0; i < primaryList.Count; i++)
            {
                KeyValuePair<Goal, List<string>> pair = primaryList[i];
                double weight = pair.Value.Count;
                if (weight > 7)
                    weight = 3;
                weightedIndexes.Add(new(Math.Sqrt(weight), i));
            }

            List<int> newOrder = new();

            // Reorder based on weights. Slightly discourages goals which only
            // require 1 or 2 things as well as goals which require a huge
            // number of things.
            VoseInstance<int> voseInst = VoseAlgorithm.createInstance(weightedIndexes);
            while (voseInst.HasMore())
            {
                int index = voseInst.NextAndRemove(genData.rnd);
                newOrder.Add(index);
            }

            List<KeyValuePair<Goal, List<string>>> newList = new();
            foreach (int index in newOrder)
            {
                newList.Add(primaryList[index]);
            }

            primaryList.Clear();
            primaryList.AddRange(newList);
        }

        private List<PathHint> pickPrimaryPathHintsFromCombinations(
            HintGenData genData,
            List<KeyValuePair<Goal, List<string>>> primaryList,
            int numHintsDesired,
            List<List<int>> combinations
        )
        {
            Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap();

            List<KeyValuePair<Goal, List<string>>> goalAndZonesList = new();

            foreach (KeyValuePair<Goal, List<string>> pair in primaryList)
            {
                HashSet<string> zonesForGoal = new();

                List<string> checkNames = pair.Value;
                if (checkNames == null)
                    continue;

                foreach (string checkName in checkNames)
                {
                    string zoneName = checkToHintZoneMap[checkName];
                    zonesForGoal.Add(zoneName);
                }

                if (zonesForGoal.Count > 0)
                {
                    List<string> zonesForGoalList = new List<string>()
                        .Concat(zonesForGoal)
                        .ToList();
                    HintUtils.ShuffleListInPlace(genData.rnd, zonesForGoalList);
                    goalAndZonesList.Add(new(pair.Key, zonesForGoalList));
                }
            }

            Dictionary<string, int> results = new();
            List<int> failedAttempts = new() { 0 };

            for (int i = 0; i < combinations.Count; i++)
            {
                List<int> combination = combinations[i];
                List<KeyValuePair<Goal, List<string>>> partialList = new(combination.Count);
                foreach (int index in combination)
                {
                    partialList.Add(goalAndZonesList[index]);
                }

                recurPickUniqueZonesCombo(results, failedAttempts, i, partialList, new(), new());
                if (results.Count >= 100 || (results.Count > 0 && failedAttempts[0] >= 500))
                    break;
            }

            return pickPathHintsFromResults(
                genData,
                primaryList,
                results,
                combinations,
                goalAndZonesList
            );
        }

        private void recurPickUniqueZonesCombo(
            Dictionary<string, int> results,
            List<int> failedAttempts,
            int combinationIndex,
            List<KeyValuePair<Goal, List<string>>> goalsAndZones,
            HashSet<string> currentZones,
            List<string> currentZonesList
        )
        {
            List<string> currentColValues = goalsAndZones[currentZonesList.Count].Value;
            for (int i = 0; i < currentColValues.Count; i++)
            {
                string currZone = currentColValues[i];
                if (currentZones.Contains(currZone))
                {
                    failedAttempts[0] = failedAttempts[0] + 1;
                    continue;
                }

                if (currentZonesList.Count < goalsAndZones.Count - 1)
                {
                    // Not on the leaf list
                    currentZones.Add(currZone);
                    currentZonesList.Add(currZone);
                    recurPickUniqueZonesCombo(
                        results,
                        failedAttempts,
                        combinationIndex,
                        goalsAndZones,
                        currentZones,
                        currentZonesList
                    );
                    currentZonesList.RemoveAt(currentZonesList.Count - 1);
                    currentZones.Remove(currZone);
                }
                else
                {
                    // are on the leaf and this is a valid combination
                    currentZonesList.Add(currZone);
                    string key = string.Join("###", currentZonesList);
                    if (!results.ContainsKey(key))
                        results[key] = combinationIndex;
                    else
                        failedAttempts[0] = failedAttempts[0] + 1;
                    currentZonesList.Remove(currZone);
                }
                // need to call recu
            }

            // Iterate over each row in this column since we are the last one.
            // List<string> currentColValues2 = goalsAndZones[positions.Count].Value;
            // // for (int i = 0; i < currentColValues2.Count; i++)
            // // {
            // //     positions.Add(i);
            // //     recurPickUniqueZonesCombo(results, goalsAndZones, positions);
            // //     positions.RemoveAt(positions.Count - 1);
            // // }

            // do nothing
        }

        private List<PathHint> pickPathHintsFromResults(
            HintGenData genData,
            List<KeyValuePair<Goal, List<string>>> primaryList,
            Dictionary<string, int> results,
            List<List<int>> combinations,
            List<KeyValuePair<Goal, List<string>>> goalAndZonesList
        )
        {
            if (results.Count < 1)
                return new();

            Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap();
            Dictionary<string, double> zoneWeightings = getZoneWeightings(
                genData,
                goalAndZonesList
            );

            List<KeyValuePair<double, string>> weightedList = new();
            foreach (KeyValuePair<string, int> pair in results)
            {
                string[] zones = pair.Key.Split("###");
                List<double> weights = new();
                double lowestWeight = 1000.0;

                double avgWeight = 0.0;

                for (int i = 0; i < zones.Length; i++)
                {
                    string zone = zones[i];
                    double weight = zoneWeightings[zone];
                    if (weight < lowestWeight)
                    {
                        lowestWeight = weight;
                    }
                    avgWeight += weight;
                }

                // Lowest weight gets applied to the average 4 times

                // Apply 3 times in addition to the one previous
                avgWeight += lowestWeight * 3;
                // Include the 3 additional in the divisor
                avgWeight /= zones.Length + 3;

                weightedList.Add(new(avgWeight, pair.Key));
            }

            VoseInstance<string> voseInst = VoseAlgorithm.createInstance(weightedList);
            string selectedComboId = voseInst.NextAndKeep(genData.rnd);

            string[] selectedZones = selectedComboId.Split("###");
            List<PathHint> pathHints = new();

            int combinationIndex = results[selectedComboId];
            List<int> combination = combinations[combinationIndex];

            for (int i = 0; i < selectedZones.Length; i++)
            {
                string desiredZoneName = selectedZones[i];
                int goalAndZonesListIndex = combination[i];
                Goal goal = goalAndZonesList[goalAndZonesListIndex].Key;
                List<string> requiredChecksOfGoal = genData.goalToRequiredChecks[goal];

                List<string> requiredChecksInZone = new();
                foreach (string checkName in requiredChecksOfGoal)
                {
                    if (checkToHintZoneMap[checkName] == desiredZoneName)
                    {
                        requiredChecksInZone.Add(checkName);
                    }
                }

                HintUtils.ShuffleListInPlace(genData.rnd, requiredChecksInZone);
                string selectedCheckName = requiredChecksInZone[0];
                Item contents = HintUtils.getCheckContents(selectedCheckName);
                // Mark checkName as directed toward
                genData.hinted.alreadyCheckDirectedToward.Add(selectedCheckName);

                PathHint hint = new PathHint(
                    AreaId.ZoneStr(desiredZoneName),
                    selectedCheckName,
                    goal.goalEnum
                );
                pathHints.Add(hint);
            }

            // Reorder primaryList so that the goals we selected are at the
            // start of the list so the caller knows from which goal index it
            // should resume selecting more path hints.
            List<KeyValuePair<Goal, List<string>>> newPrimaryList = new();
            List<KeyValuePair<Goal, List<string>>> newPrimaryListEnd = new();
            HashSet<int> indexesToFront = new();
            foreach (int index in combination)
            {
                indexesToFront.Add(index);
            }

            for (int i = 0; i < primaryList.Count; i++)
            {
                if (indexesToFront.Contains(i))
                    newPrimaryList.Add(primaryList[i]);
                else
                    newPrimaryListEnd.Add(primaryList[i]);
            }

            newPrimaryList.AddRange(newPrimaryListEnd);
            primaryList.Clear();
            primaryList.AddRange(newPrimaryList);

            return pathHints;
        }

        private Dictionary<string, double> getZoneWeightings(
            HintGenData genData,
            List<KeyValuePair<Goal, List<string>>> goalAndZonesList
        )
        {
            Dictionary<string, double> zoneNameToWeight = new();

            foreach (KeyValuePair<Goal, List<string>> pair in goalAndZonesList)
            {
                List<string> zoneNames = pair.Value;
                foreach (string zoneName in zoneNames)
                {
                    if (!zoneNameToWeight.ContainsKey(zoneName))
                    {
                        zoneNameToWeight[zoneName] = genData.GetAreaWothWeight(
                            AreaId.ZoneStr(zoneName)
                        );
                    }
                }
            }

            return zoneNameToWeight;
        }

        private List<KeyValuePair<Goal, List<string>>> filterPathHintSecondaryList(
            List<KeyValuePair<Goal, List<string>>> origList,
            HashSet<string> checkNamesForPrimaryGoals
        )
        {
            List<KeyValuePair<Goal, List<string>>> filteredSecondaryList = new();
            HashSet<string> checkNamesForSecondaryGoals = new();

            foreach (KeyValuePair<Goal, List<string>> pair in origList)
            {
                List<string> filteredChecks = new();
                foreach (string checkName in pair.Value)
                {
                    if (
                        !checkNamesForPrimaryGoals.Contains(checkName)
                        && !checkNamesForSecondaryGoals.Contains(checkName)
                    )
                    {
                        filteredChecks.Add(checkName);
                    }
                }

                if (filteredChecks.Count > 0)
                {
                    foreach (string checkName in filteredChecks)
                    {
                        checkNamesForSecondaryGoals.Add(checkName);
                    }

                    filteredSecondaryList.Add(new(pair.Key, filteredChecks));
                }
            }

            return filteredSecondaryList;
        }
    }
}
