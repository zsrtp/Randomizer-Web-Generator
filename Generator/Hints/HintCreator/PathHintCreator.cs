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
            // List<PathHint> pathHints = genPathHints(genData, numHints);
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
            // Priorities [reqDungeon, Zant, HC, Ganondorf]

            // We only attempt to do the priorities once! If we fail to generate any from a unique
            // combination, then we just try to generate normally from these.

            // In any case, once we finish messing we those, we permanently don't try again. After
            // that point, we just do them like this: [...reqDungeons, Zant, HC, Ganondorf], trying
            // to make sure each one is hinted a single time.

            // Then after each has been hinted at least once, we just randomly pick them to hint.

            //

            // If we have not generated any path hints which count toward one per goal yet, then we
            // should try to do the fancy picking to get different zones.

            // We want to avoid adding goals such that the number of goals is greater than the
            // number we are trying to pick. For example, if we are only picking 2 and adding the
            // first tier (req dungeons) gives us 3 goals, then we should not continue to add HC to
            // the list. Otherwise we get something like [FT, GM, LBT, HC] for example, where if we
            // are hinting 2, it could pick something like [GM, HC] and give us an HC hint even
            // though we have not already attempted to generate at least one hint per dungeon.

            // So we should first add required dungeons. That's always good. Then if the numDesired
            // is greater than what we have, we can try to add additional goals. However, if adding
            // the additional goal would cause the combined list to go higher than numDesired, we
            // don't add it and we instead proceed with what we have.

            // In the case that we have already done the fancy thing, or attempted it and it did or
            // did not work, we mark it as never doing that again. Then from that point forward, we
            // handle it like this:

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
            List<KeyValuePair<Goal, List<string>>> priorityGoals = getPriorityGoals(
                genData,
                numHintsDesired
            );

            List<PathHint> createdHints = pickPrimaryPathHints(
                genData,
                priorityGoals,
                numHintsDesired
            );

            if (createdHints.Count < numHintsDesired)
            {
                for (int i = createdHints.Count; i < priorityGoals.Count; i++)
                {
                    Goal goal = priorityGoals[i].Key;
                    List<KeyValuePair<Goal, List<string>>> yetHintedGoalsToChecks =
                        getGoalsToHintableChecks(genData, new() { goal });
                    if (yetHintedGoalsToChecks.Count > 0)
                    {
                        KeyValuePair<Goal, List<string>> goalAndChecks = yetHintedGoalsToChecks[0];
                        PathHint hint = createHintForGoalAndChecks(genData, goalAndChecks);
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

                List<KeyValuePair<Goal, List<string>>> goalAndChecks = getGoalsToHintableChecks(
                    genData,
                    new() { goal }
                );
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

        private PathHint createHintForGoalAndChecks(
            HintGenData genData,
            KeyValuePair<Goal, List<string>> goalAndChecks
        )
        {
            Goal goal = goalAndChecks.Key;
            List<string> checkNames = goalAndChecks.Value;

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

            // // Mark zone as hinted for that goal
            // goalToHintedZones[pair.Key].Add(selectedZone);

            // // Create path hint for this goal and check
            // Item contents = HintUtils.getCheckContents(selectedCheckName);
            // // Mark checkName as directed toward
            // genData.hinted.alreadyCheckDirectedToward.Add(selectedCheckName);
            // hintedChecks.Add(selectedCheckName);

            // PathHint hint = new PathHint(
            //     AreaId.ZoneStr(selectedZone),
            //     selectedCheckName,
            //     pair.Key.goalEnum
            // );
            // pathHints.Add(hint);

        }

        private List<KeyValuePair<Goal, List<string>>> getPriorityGoals(
            HintGenData genData,
            int numHintsDesired
        )
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

            List<KeyValuePair<Goal, List<string>>> gatheredLists = new();

            for (int i = 0; i < goalTiers.Count; i++)
            {
                List<Goal> filteredGoals = filterToValidGoals(goalTiers[i]);

                List<KeyValuePair<Goal, List<string>>> hintableGoalsForTier =
                    getGoalsToHintableChecks(genData, filteredGoals);
                if (!ListUtils.isEmpty(hintableGoalsForTier))
                {
                    KeyValuePair<Goal, List<string>> goal = HintUtils.PickRandomListItem(
                        genData.rnd,
                        hintableGoalsForTier
                    );
                    gatheredLists.Add(goal);
                }
                if (gatheredLists.Count >= numHintsDesired)
                    break;
            }

            return gatheredLists;
        }

        private List<Goal> filterToValidGoals(List<Goal> goalsIn)
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

        private List<KeyValuePair<Goal, List<string>>> getGoalsToHintableChecks(
            HintGenData genData,
            List<Goal> goals
        )
        {
            List<KeyValuePair<Goal, List<string>>> result = new();

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
                    foreach (List<string> list in listOfLists)
                    {
                        List<string> possibleCheckNamesForGoal = new();

                        // Calculate if we have anything valid in the list.
                        if (!ListUtils.isEmpty(list))
                        {
                            foreach (string checkName in list)
                            {
                                if (CheckIsPossibleToHint(genData, checkName))
                                    possibleCheckNamesForGoal.Add(checkName);
                            }
                        }

                        if (possibleCheckNamesForGoal.Count > 0)
                        {
                            result.Add(new(goal, possibleCheckNamesForGoal));
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private List<PathHint> genPathHints(HintGenData genData, int numHintsDesired)
        {
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
                string zoneName = genData.GetZoneNameForCheck(hint.checkName);
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
                    string hintZone = genData.GetZoneNameForCheck(checkName);
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
                    checkName => genData.GetZoneNameForCheck(checkName) == selectedZone
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

            foreach (KeyValuePair<Goal, List<string>> pair in genData.goalToRequiredChecks)
            {
                List<string> checkNames = pair.Value;
                if (ListUtils.isEmpty(checkNames))
                    continue;

                bool isPrimaryGoal = GoalConstants.IsDungeonGoal(pair.Key);
                List<string> canBeHintedCheckNames = new();

                foreach (string checkName in checkNames)
                {
                    if (!CheckIsPossibleToHint(genData, checkName))
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

            // Need to sort secondaryList to always have HC in front of
            // Ganondorf. There aren't any other goal hints in the secondary
            // list, but if there were they would be ahead of HC in an undefined
            // order.
            KeyValuePair<Goal, List<string>> hcGoalPair = new(GoalConstants.Diababa, null);
            KeyValuePair<Goal, List<string>> ganondorfGoalPair = new(GoalConstants.Diababa, null);
            for (int i = secondaryList.Count - 1; i >= 0; i--)
            {
                KeyValuePair<Goal, List<string>> pair = secondaryList[i];
                if (pair.Key.goalEnum == GoalEnum.Hyrule_Castle)
                {
                    hcGoalPair = pair;
                    secondaryList.RemoveAt(i);
                }
                else if (pair.Key.goalEnum == GoalEnum.Ganondorf)
                {
                    ganondorfGoalPair = pair;
                    secondaryList.RemoveAt(i);
                }
            }

            if (hcGoalPair.Key.goalEnum == GoalEnum.Hyrule_Castle)
                secondaryList.Add(hcGoalPair);
            if (ganondorfGoalPair.Key.goalEnum == GoalEnum.Ganondorf)
                secondaryList.Add(ganondorfGoalPair);

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

            // TODO: temp disabling this part
            // reorderPathPrimaryList(genData, primaryList);

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
            // Group by zone for each goal, only including ones where there are actually checks.
            List<KeyValuePair<Goal, List<string>>> goalAndZonesList = new();
            foreach (KeyValuePair<Goal, List<string>> pair in primaryList)
            {
                HashSet<string> zonesForGoal = new();

                List<string> checkNames = pair.Value;
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

                KeyValuePair<Goal, List<string>> goalAndChecks = primaryList.Find(
                    (el) => el.Key == goal
                );
                List<string> possibleChecksForGoal = goalAndChecks.Value;

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

        private bool CheckIsPossibleToHint(HintGenData genData, string checkName)
        {
            Item item = HintUtils.getCheckContents(checkName);

            return (
                genData.CheckCanBeWothPathHinted(checkName)
                && (invalidItems == null || !invalidItems.Contains(item))
            );
        }
    }
}
