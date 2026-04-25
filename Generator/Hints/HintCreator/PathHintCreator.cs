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

        private HashSet<Item> validItems = null;
        private HashSet<Item> invalidItems = new();
        private HashSet<Goal> validGoals = null;
        private bool unhintedGoalsOnly = false;

        private PathHintCreator() { }

        new public static PathHintCreator fromJObject(JObject obj)
        {
            PathHintCreator inst = new();
            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                inst.validItems = HintSettingUtils.getOptionalItemSet(
                    options,
                    "validItems",
                    inst.validItems
                );

                inst.invalidItems = HintSettingUtils.getOptionalItemSet(
                    options,
                    "invalidItems",
                    inst.invalidItems
                );

                List<string> validGoalsStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validGoals",
                    null
                );
                if (validGoalsStrList != null)
                {
                    inst.validGoals = new();
                    foreach (string goalEnumStr in validGoalsStrList)
                    {
                        if (!Enum.TryParse(goalEnumStr, true, out GoalEnum goalEnum))
                            throw new Exception($"Failed to parse '{goalEnum}' to GoalEnum.");
                        Goal goal = GoalConstants.getGoalFromEnumThrows(goalEnum);
                        inst.validGoals.Add(goal);
                    }
                }

                inst.unhintedGoalsOnly = HintSettingUtils.getOptionalBool(
                    options,
                    "unhintedGoalsOnly",
                    inst.unhintedGoalsOnly
                );
            }

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache,
            BarrenPenalizer barrenPenalizer
        )
        {
            if (genData.sSettings.logicRules == SSettings.Enums.LogicRules.No_Logic)
                return null;

            List<PathHint> pathHints = genPathHints2(genData, numHints);
            if (pathHints != null)
            {
                List<Hint> a = pathHints.ConvertAll(x => (Hint)x);
                return a;
            }
            return null;
        }

        private List<PathHint> genPathHints2(HintGenData genData, int numHintsDesired)
        {
            List<PathHint> createdHints = new();

            if (!genData.goalManager.attemptedPriorityPicks)
            {
                genData.goalManager.notifyAttemptedPriorityPicks();
                List<PathHint> priorityHints = doPrimaryAttemptStuff(genData, numHintsDesired);
                if (!ListUtils.isEmpty(priorityHints))
                    createdHints.AddRange(priorityHints);
            }

            // If still need more hints, gather any valid goals (ordered by tier; randomized
            // internally) which have not already been hinted. Try to create hints one at a time for
            // each of those, prioritizing zones which have not yet been hinted if possible.
            createPathHintsSimple(genData, numHintsDesired, createdHints, true);

            if (!unhintedGoalsOnly)
            {
                // If still need more hints, then do the same as above but can create hints for already
                // hinted goals.
                createPathHintsSimple(genData, numHintsDesired, createdHints, false);
            }

            return createdHints;
        }

        private List<PathHint> doPrimaryAttemptStuff(HintGenData genData, int numHintsDesired)
        {
            List<GoalInfo> priorityGoals = getPriorityGoals(genData, numHintsDesired);

            List<PathHint> createdHints = pickPrimaryPathHints(
                genData,
                priorityGoals,
                numHintsDesired
            );

            if (createdHints.Count < numHintsDesired)
            {
                for (int i = createdHints.Count; i < priorityGoals.Count; i++)
                {
                    Goal goal = priorityGoals[i].goal;
                    List<GoalInfo> yetHintedGoalsToChecks = getGoalsToHintableChecks(
                        genData,
                        new() { goal }
                    );
                    if (yetHintedGoalsToChecks.Count > 0)
                    {
                        PathHint hint = createHintForGoalAndChecks(
                            genData,
                            yetHintedGoalsToChecks[0]
                        );
                        if (hint != null)
                            createdHints.Add(hint);
                    }
                }
            }

            return createdHints;
        }

        private void createPathHintsSimple(
            HintGenData genData,
            int numHintsDesired,
            List<PathHint> createdHints,
            bool includeUnhintedGoalsOnly
        )
        {
            List<Goal> availableGoals = getRandomWithinTierGoals(genData);

            int i = 0;
            while (createdHints.Count < numHintsDesired && availableGoals.Count > 0)
            {
                if (i >= availableGoals.Count)
                    i = 0;

                Goal goal = availableGoals[i];
                if (includeUnhintedGoalsOnly && genData.goalManager.hintedGoals.Contains(goal))
                {
                    availableGoals.RemoveAt(i);
                    continue;
                }

                List<GoalInfo> goalAndChecks = getGoalsToHintableChecks(genData, new() { goal });
                if (ListUtils.isEmpty(goalAndChecks))
                {
                    availableGoals.RemoveAt(i);
                    continue;
                }

                PathHint hint = createHintForGoalAndChecks(genData, goalAndChecks[0]);
                if (hint != null)
                    createdHints.Add(hint);
                else
                {
                    availableGoals.RemoveAt(i);
                    continue;
                }

                i += 1;
            }
        }

        private List<Goal> getRandomWithinTierGoals(HintGenData genData)
        {
            Dictionary<Goal, int> goalTiers =
                new()
                {
                    { GoalConstants.Diababa, 0 },
                    { GoalConstants.Fyrus, 0 },
                    { GoalConstants.Morpheel, 0 },
                    { GoalConstants.Stallord, 0 },
                    { GoalConstants.Blizzeta, 0 },
                    { GoalConstants.Armogohma, 0 },
                    { GoalConstants.Argorok, 0 },
                    { GoalConstants.Zant, 0 },
                    { GoalConstants.Hyrule_Castle, 1 },
                    { GoalConstants.Ganondorf, 2 },
                };

            HashSet<Goal> allowedGoals = null;

            // From the list of allowed goals (or everything if no goals specified), order the goals
            // into a list.
            Dictionary<int, List<Goal>> allowedGoalsByTier = new();
            foreach (
                KeyValuePair<Goal, List<List<string>>> pair in genData.goalManager.goalToCheckLists
            )
            {
                Goal goal = pair.Key;
                if (validGoals != null && !validGoals.Contains(goal))
                    continue;

                if (allowedGoals == null || allowedGoals.Contains(goal))
                {
                    int tier = goalTiers[goal];
                    if (!allowedGoalsByTier.TryGetValue(tier, out List<Goal> goalsForTier))
                    {
                        goalsForTier = new();
                        allowedGoalsByTier[tier] = goalsForTier;
                    }
                    goalsForTier.Add(goal);
                }
            }

            // The list will be ordered by tier and then each randomized internally before grouping.
            // Ex: [LBT, AG, PoT],[HC],[Ganondorf] => [LBT, PoT, AG],[HC],[Ganondorf] =>
            // [LBT, PoT, AG, HC, Ganondorf]
            List<int> tierValues = new();
            foreach (KeyValuePair<int, List<Goal>> pair in allowedGoalsByTier)
            {
                tierValues.Add(pair.Key);
                List<Goal> goalsForTier = pair.Value;
                if (goalsForTier.Count > 1)
                    HintUtils.ShuffleListInPlace(genData.rnd, goalsForTier);
            }
            tierValues.Sort();

            List<Goal> finalGoals = new();
            foreach (int tier in tierValues)
            {
                finalGoals.AddRange(allowedGoalsByTier[tier]);
            }

            return finalGoals;
        }

        private PathHint createHintForGoalAndChecks(HintGenData genData, GoalInfo goalInfo)
        {
            Goal goal = goalInfo.goal;
            List<string> checkNames = goalInfo.highestPriorityCheckNames;

            if (ListUtils.isEmpty(checkNames))
                return null;

            HashSet<string> priorityZones = new();
            HashSet<string> secondaryZones = new();
            foreach (string checkName in checkNames)
            {
                string zoneName = genData.GetZoneNameForCheck(checkName);
                if (genData.goalManager.hintedZoneNames.Contains(zoneName))
                    secondaryZones.Add(zoneName);
                else
                    priorityZones.Add(zoneName);
            }

            string selectedZone;
            if (priorityZones.Count > 0)
                selectedZone = HintUtils.RemoveRandomHashSetItem(genData.rnd, priorityZones);
            else
                selectedZone = HintUtils.RemoveRandomHashSetItem(genData.rnd, secondaryZones);

            // Pick random check for that zone
            List<string> checksForZone = checkNames.FindAll(
                checkName => genData.GetZoneNameForCheck(checkName) == selectedZone
            );
            string selectedCheckName = HintUtils.PickRandomListItem(genData.rnd, checksForZone);
            Item contents = HintUtils.getCheckContents(selectedCheckName);
            // Mark checkName as directed toward
            genData.hinted.alreadyCheckDirectedToward.Add(selectedCheckName);
            // Mark goal and zoneName as hinted before
            genData.goalManager.hintedGoals.Add(goal);
            genData.goalManager.hintedZoneNames.Add(selectedZone);

            PathHint hint = new PathHint(
                AreaId.ZoneStr(selectedZone),
                selectedCheckName,
                goal.goalEnum
            );
            return hint;
        }

        private List<GoalInfo> getPriorityGoals(HintGenData genData, int numHintsDesired)
        {
            if (numHintsDesired < 1)
                return new();

            List<List<Goal>> goalTiers =
                new()
                {
                    new()
                    {
                        GoalConstants.Diababa,
                        GoalConstants.Fyrus,
                        GoalConstants.Morpheel,
                        GoalConstants.Stallord,
                        GoalConstants.Blizzeta,
                        GoalConstants.Armogohma,
                        GoalConstants.Argorok,
                    },
                    new() { GoalConstants.Zant },
                    new() { GoalConstants.Hyrule_Castle },
                    new() { GoalConstants.Ganondorf },
                };

            List<GoalInfo> gatheredLists = new();

            for (int i = 0; i < goalTiers.Count; i++)
            {
                List<Goal> filteredGoals = filterFromValidGoals(goalTiers[i]);

                List<GoalInfo> hintableGoalsForTier = getGoalsToHintableChecks(
                    genData,
                    filteredGoals
                );
                if (!ListUtils.isEmpty(hintableGoalsForTier))
                {
                    HintUtils.ShuffleListInPlace(genData.rnd, hintableGoalsForTier);

                    GoalInfo selectedGoalInfo = null;
                    foreach (GoalInfo goalInfo in hintableGoalsForTier)
                    {
                        if (goalInfo.totalChecksRemaining == 1)
                        {
                            selectedGoalInfo = goalInfo;
                            break;
                        }
                    }
                    if (selectedGoalInfo == null)
                    {
                        selectedGoalInfo = HintUtils.PickRandomListItem(
                            genData.rnd,
                            hintableGoalsForTier
                        );
                    }
                    gatheredLists.Add(selectedGoalInfo);
                }
                if (gatheredLists.Count >= numHintsDesired)
                    break;
            }

            return gatheredLists;
        }

        private List<Goal> filterFromValidGoals(List<Goal> goalsIn)
        {
            if (validGoals == null)
                return goalsIn;

            List<Goal> result = new();
            foreach (Goal goal in goalsIn)
            {
                if (validGoals.Contains(goal))
                    result.Add(goal);
            }
            return result;
        }

        private List<GoalInfo> getGoalsToHintableChecks(HintGenData genData, List<Goal> goals)
        {
            List<GoalInfo> result = new();

            Dictionary<Goal, List<List<string>>> goalToCheckLists = genData
                .goalManager
                .goalToCheckLists;

            if (goals.Count < 1)
                return new();

            foreach (Goal goal in goals)
            {
                if (unhintedGoalsOnly && genData.goalManager.hintedGoals.Contains(goal))
                    continue;

                if (goalToCheckLists.TryGetValue(goal, out List<List<string>> listOfLists))
                {
                    int numTotalHintable = 0;
                    List<string> hintableCheckNames = null;

                    foreach (List<string> list in listOfLists)
                    {
                        List<string> possibleCheckNamesForGoal = new();

                        // Calculate if we have anything valid in the list.
                        if (!ListUtils.isEmpty(list))
                        {
                            foreach (string checkName in list)
                            {
                                if (CheckIsPossibleToHint(genData, checkName))
                                {
                                    possibleCheckNamesForGoal.Add(checkName);
                                    numTotalHintable += 1;
                                }
                            }
                        }

                        if (hintableCheckNames == null && possibleCheckNamesForGoal.Count > 0)
                            hintableCheckNames = possibleCheckNamesForGoal;
                    }

                    if (!ListUtils.isEmpty(hintableCheckNames))
                        result.Add(new(goal, hintableCheckNames, numTotalHintable));
                }
            }

            return result;
        }

        private List<PathHint> pickPrimaryPathHints(
            HintGenData genData,
            List<GoalInfo> primaryList,
            int numHintsDesired
        )
        {
            if (numHintsDesired < 1 || ListUtils.isEmpty(primaryList))
                return new();

            int numHintsToPick =
                numHintsDesired > primaryList.Count ? primaryList.Count : numHintsDesired;

            List<List<int>> combinations = getCombinationIndexes(primaryList.Count, numHintsToPick);

            return pickPrimaryPathHintsFromCombinations(genData, primaryList, combinations);
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

        private List<PathHint> pickPrimaryPathHintsFromCombinations(
            HintGenData genData,
            List<GoalInfo> primaryList,
            List<List<int>> combinations
        )
        {
            // Group by zone for each goal, only including ones where there are actually checks.
            List<KeyValuePair<Goal, List<string>>> goalAndZonesList = new();
            foreach (GoalInfo goalInfo in primaryList)
            {
                HashSet<string> zonesForGoal = new();

                List<string> checkNames = goalInfo.highestPriorityCheckNames;
                if (checkNames == null)
                    continue;

                foreach (string checkName in checkNames)
                {
                    string zoneName = genData.GetZoneNameForCheck(checkName);
                    zonesForGoal.Add(zoneName);
                }

                if (zonesForGoal.Count > 0)
                {
                    List<string> zonesForGoalList = new List<string>()
                        .Concat(zonesForGoal)
                        .ToList();
                    HintUtils.ShuffleListInPlace(genData.rnd, zonesForGoalList);
                    goalAndZonesList.Add(new(goalInfo.goal, zonesForGoalList));
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
            List<GoalInfo> primaryList,
            Dictionary<string, int> results,
            List<List<int>> combinations,
            List<KeyValuePair<Goal, List<string>>> goalAndZonesList
        )
        {
            if (results.Count < 1)
                return new();

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

            // Might not need, but just in case we try to pick the same check
            // back-to-back for example.
            HashSet<string> justPickedCheckNames = new();

            for (int i = 0; i < selectedZones.Length; i++)
            {
                string desiredZoneName = selectedZones[i];
                int goalAndZonesListIndex = combination[i];
                Goal goal = goalAndZonesList[goalAndZonesListIndex].Key;

                GoalInfo goalAndChecks = primaryList.Find((el) => el.goal == goal);
                List<string> possibleChecksForGoal = goalAndChecks.highestPriorityCheckNames;

                List<string> possibleChecksInZone = new();
                foreach (string checkName in possibleChecksForGoal)
                {
                    if (
                        !justPickedCheckNames.Contains(checkName)
                        && genData.GetZoneNameForCheck(checkName) == desiredZoneName
                    )
                    {
                        possibleChecksInZone.Add(checkName);
                    }
                }

                HintUtils.ShuffleListInPlace(genData.rnd, possibleChecksInZone);
                string selectedCheckName = possibleChecksInZone[0];
                justPickedCheckNames.Add(selectedCheckName);
                Item contents = HintUtils.getCheckContents(selectedCheckName);
                // Mark checkName as directed toward
                genData.hinted.alreadyCheckDirectedToward.Add(selectedCheckName);
                // Mark goal and zoneName as hinted before
                genData.goalManager.hintedGoals.Add(goal);
                genData.goalManager.hintedZoneNames.Add(desiredZoneName);

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
            List<GoalInfo> newPrimaryList = new();
            List<GoalInfo> newPrimaryListEnd = new();
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

        private bool CheckIsPossibleToHint(HintGenData genData, string checkName)
        {
            Item item = HintUtils.getCheckContents(checkName);

            return (
                genData.CheckCanBeWothPathHinted(checkName)
                && (validItems == null || validItems.Contains(item))
                && (invalidItems == null || !invalidItems.Contains(item))
            );
        }

        private class GoalInfo
        {
            public Goal goal { get; }
            public List<string> highestPriorityCheckNames { get; }
            public int totalChecksRemaining { get; }

            public GoalInfo(
                Goal goal,
                List<string> highestPriorityCheckNames,
                int totalChecksRemaining
            )
            {
                this.goal = goal;
                this.highestPriorityCheckNames = highestPriorityCheckNames;
                this.totalChecksRemaining = totalChecksRemaining;
            }
        }
    }
}
