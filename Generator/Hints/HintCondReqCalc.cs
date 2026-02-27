namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TPRandomizer.Util;

    public class HintCondReqCalc
    {
        // Note: the main algorithm in this file is based on OoTMM analysis-foolish.ts

        // Note: checks can only be done against gaining/losing access to beating the game. Losing
        // access to a condReq check can be inaccurate. Example we ran into: fishRod on Barnes was
        // condReq. By losing access to both wallets (2nd one in CitS), we lost access to this
        // condReq fishRod, so we marked that wallet in CitS as important. However, wallet's only
        // purpose was wallet => fishRod => BB/B&C, but the skybook was behind BB/B&C, so the only
        // way to access the CitS wallet was to already have items which make the wallet have no use
        // (the BB/B&C distinction was not relevant for this seed), meaning finding the CitS wallet
        // could never make a difference, so it should not have been condReq.

        // If a single zigZag takes more time than this, something is probably wrong. The only way
        // this should be possible is with a really extreme plando which adds a massive number of
        // items to the locsSet. We have this here so the generator does not get stuck on a request
        // like this, but hopefully it is never relevant.
        private const int MaxSingleZigZagDurationMs = 90_000;

        private HintGenData genData;
        private HashSet<Item> startingItemsSet = new();
        private Dictionary<Item, List<string>> itemToSphere0Checks;
        private HashSet<string> baseForbiddenChecks = new();
        private HashSet<string> condRequiredChecks = new();
        private HashSet<string> prevCondRequiredChecks = new();

        public HintCondReqCalc(HintGenData genData)
        {
            this.genData = genData;

            startingItemsSet = new(genData.sSettings.startingItems);

            itemToSphere0Checks = new();
            if (!ListUtils.isEmpty(genData.playthroughSpheres.sphere0Checks))
            {
                foreach (string checkName in genData.playthroughSpheres.sphere0Checks)
                {
                    Item item = HintUtils.getCheckContents(checkName);
                    if (!itemToSphere0Checks.TryGetValue(item, out List<string> checksList))
                    {
                        checksList = new();
                        itemToSphere0Checks[item] = checksList;
                    }
                    checksList.Add(checkName);
                }
            }
        }

        // Returns true if was newly added to set, else false if was already in
        // the set.
        private bool markAsSometimesRequired(string checkName)
        {
            if (!condRequiredChecks.Contains(checkName))
            {
                Item contents = HintUtils.getCheckContents(checkName);
                Console.WriteLine($"Sometimes Required: {checkName} ({contents})");
                condRequiredChecks.Add(checkName);
                return true;
            }
            return false;
        }

        private void markClearlyDeadChecksGivingTradeItems()
        {
            foreach (KeyValuePair<Item, string> pair in HintUtils.tradeItemToRewardCheck)
            {
                Item tradeItem = pair.Key;
                if (
                    genData.tradeItemToChainEndCheck.TryGetValue(
                        tradeItem,
                        out string chainEndCheckName
                    )
                )
                {
                    Item chainEndItem = HintUtils.getCheckContents(chainEndCheckName);
                    if (
                        !genData.logicalItems.Contains(chainEndItem)
                        || genData.allowBarrenChecks.Contains(chainEndCheckName)
                        || genData.notReqChecks.Contains(chainEndCheckName)
                    )
                    {
                        // Mark all checks rewarding the tradeItem as "not required".
                        if (
                            genData.itemToChecksList.TryGetValue(
                                tradeItem,
                                out List<string> checksForItem
                            )
                        )
                        {
                            foreach (string checkName in checksForItem)
                            {
                                string reason = !genData.logicalItems.Contains(chainEndItem)
                                    ? "non-logical"
                                    : "known bad";
                                Console.WriteLine(
                                    $"- marked tradeItem notReq: {checkName} ({tradeItem}); chain end {reason}: {chainEndCheckName} ({chainEndItem})"
                                );
                                genData.notReqChecks.Add(checkName);
                            }
                        }
                    }
                }
            }
        }

        private bool checkMarkSphere0Checks()
        {
            HashSet<Item> itemsToCheck = new();
            foreach (string checkName in condRequiredChecks)
            {
                if (!prevCondRequiredChecks.Contains(checkName))
                    itemsToCheck.Add(HintUtils.getCheckContents(checkName));
            }

            bool didMarkSome = false;
            foreach (Item item in itemsToCheck)
            {
                if (itemToSphere0Checks.TryGetValue(item, out List<string> checksList))
                {
                    foreach (string checkName in checksList)
                    {
                        if (
                            !genData.requiredChecks.Contains(checkName)
                            && !condRequiredChecks.Contains(checkName)
                        )
                        {
                            Console.WriteLine(
                                $"Sometimes Required implied for sphere0 check: {checkName} ({item})"
                            );
                            condRequiredChecks.Add(checkName);
                            didMarkSome = true;
                        }
                    }
                }
            }
            return didMarkSome;
        }

        private bool checkMarkTradeItemSources()
        {
            // First, get a HashSet of all new condReq checks which are tradeItem rewards.
            HashSet<string> newRewardCheckNames = new();
            foreach (string checkName in condRequiredChecks)
            {
                if (!prevCondRequiredChecks.Contains(checkName))
                {
                    if (HintUtils.tradeRewardCheckToSourceItem.ContainsKey(checkName))
                    {
                        newRewardCheckNames.Add(checkName);
                    }
                }
            }

            // Then we iterate over all tradeItemToChainEndChecks. If a tradeItem ends in a newly
            // marked check, then we make sure it was not a startingItem and we also make sure there
            // is only 1 findable copy before marking it as condReq.
            bool didMarkSome = false;
            foreach (KeyValuePair<Item, string> pair in genData.tradeItemToChainEndCheck)
            {
                Item item = pair.Key;
                string chainEndCheckName = pair.Value;
                if (!newRewardCheckNames.Contains(chainEndCheckName))
                    continue;

                if (
                    !startingItemsSet.Contains(item)
                    && genData.itemToChecksList.TryGetValue(item, out List<string> checksGivingItem)
                )
                {
                    if (checksGivingItem != null && checksGivingItem.Count == 1)
                    {
                        string checkName = checksGivingItem[0];
                        if (
                            !genData.requiredChecks.Contains(checkName)
                            && !condRequiredChecks.Contains(checkName)
                        )
                        {
                            Console.WriteLine(
                                $"Sometimes Required implied for tradeChain check: {checkName} ({item})"
                            );
                            condRequiredChecks.Add(checkName);
                            didMarkSome = true;
                        }
                    }
                }
            }
            return didMarkSome;
        }

        private void checkZigZagDurationCausesError(Stopwatch stopwatch)
        {
            long elapsedMs = stopwatch.ElapsedMilliseconds;
            if (elapsedMs > MaxSingleZigZagDurationMs)
            {
                Console.WriteLine(
                    $"ZigZag iteration taking more than allowed time '{MaxSingleZigZagDurationMs} ms', so exiting program with error code 1."
                );
                Environment.Exit(1);
            }
        }

        private void calcBaseForbiddenChecks()
        {
            if (
                genData.majorItems.Contains(Item.Poe_Soul)
                || ListUtils.isEmpty(genData.playthroughSpheres.spheresVerbose)
            )
                return;

            int numFlexibleThatCouldMatter = genData.checkMaybeRelevantFlexiblePoeSoulsToFind();
            if (numFlexibleThatCouldMatter < 1)
                return;

            HashSet<string> unreqBarrenDungeonNames = new();
            if (genData.sSettings.barrenDungeons)
            {
                foreach (
                    KeyValuePair<string, Goal> pair in GoalConstants.requiredDungeonHintZoneToGoal
                )
                {
                    string dungeonName = pair.Key;
                    if (!HintUtils.DungeonIsRequired(dungeonName))
                    {
                        unreqBarrenDungeonNames.Add(dungeonName);
                    }
                }
            }

            bool hasUnreqBarrenDungeons = unreqBarrenDungeonNames.Count > 0;
            HashSet<string> relevantFlexibleCheckNames = new();
            bool brokeMeetingThreshold = false;

            for (int i = 0; i < genData.playthroughSpheres.spheresVerbose.Count; i++)
            {
                List<KeyValuePair<int, Item>> spherePairs = genData
                    .playthroughSpheres
                    .spheresVerbose[i];
                foreach (KeyValuePair<int, Item> checkAndItem in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(checkAndItem.Key);
                    Item contents = HintUtils.getCheckContents(checkName);
                    if (contents == Item.Poe_Soul && !genData.requiredChecks.Contains(checkName))
                    {
                        // Check that poe soul is not in an unreq barren dungeon.
                        if (hasUnreqBarrenDungeons)
                        {
                            string zoneName = genData.GetZoneNameForCheck(checkName);
                            if (!unreqBarrenDungeonNames.Contains(zoneName))
                                relevantFlexibleCheckNames.Add(checkName);
                        }
                        else
                            relevantFlexibleCheckNames.Add(checkName);
                    }
                }

                if (relevantFlexibleCheckNames.Count >= numFlexibleThatCouldMatter)
                {
                    brokeMeetingThreshold = true;
                    break;
                }
            }

            if (!brokeMeetingThreshold)
                return;

            // Include any checks rewarding poeSouls which are (1) not Required, and (2) not found
            // in a sphere early enough to be considered a reasonable check to do to get a poe soul,
            // to the baseForbiddenChecks which will be skipped over during condReq calcs. Note that
            // all checks rewarding poeSouls will stay as "skippable". This is to avoid saying a
            // fishingRod or skyBook is important for example when its only purpose is to go get a
            // poeSoul which would be super unreasonable to get compared to easier to get ones.
            int numAddedToBaseForbiddenChecks = 0;
            if (genData.itemToChecksList.TryGetValue(Item.Poe_Soul, out List<string> checksList))
            {
                foreach (string checkName in checksList)
                {
                    if (
                        !genData.requiredChecks.Contains(checkName)
                        && !relevantFlexibleCheckNames.Contains(checkName)
                    )
                    {
                        baseForbiddenChecks.Add(checkName);
                        numAddedToBaseForbiddenChecks += 1;
                    }
                }
            }
            Console.WriteLine(
                $"Added {numAddedToBaseForbiddenChecks} checks rewarding poeSouls to baseForbiddenChecks."
            );
        }

        private ZigZagState monteCarloZigZagDown(ZigZagState zz, Stopwatch stopwatch)
        {
            HashSet<string> checkNames = new(zz.allowedCheckNames);
            HashSet<string> allowedCheckNames = new(zz.allowedCheckNames);
            HashSet<string> forbiddenCheckNames = new(zz.forbiddenCheckNames);
            string lastBanishedCheckName = null;

            while (true)
            {
                if (checkNames.Count < 1)
                    break;

                checkZigZagDurationCausesError(stopwatch);

                string checkName = HintUtils.RemoveRandomHashSetItem(genData.rnd, checkNames);
                allowedCheckNames.Remove(checkName);
                forbiddenCheckNames.Add(checkName);

                HashSet<string> combinedForbiddenChecks = new(baseForbiddenChecks);
                combinedForbiddenChecks.UnionWith(forbiddenCheckNames);

                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    combinedForbiddenChecks
                );
                if (!wasSuccess)
                {
                    if (markAsSometimesRequired(checkName))
                    {
                        lastBanishedCheckName = checkName;
                    }
                    allowedCheckNames.Add(checkName);
                    forbiddenCheckNames.Remove(checkName);
                }
            }

            if (lastBanishedCheckName == null)
                return null;

            // Re-forbid the last banished checkName. In the code above, we always revert the
            // allowed/forbidden changes if the seed becomes unbeatable (meaning it is always
            // beatable at the start of the while loop). Here we reverse this for the last check we
            // found which had an outcome on the beatability of the seed when we forbid it. Even if
            // we added forbidden checks after this one (meaning they did not prevent us from
            // beating the seed), it is still guaranteed that re-forbidding the lastBanishedCheck
            // will make the seed unbeatable which is what we want when the result is next processed
            // by the zigZagUp.
            forbiddenCheckNames.Add(lastBanishedCheckName);
            allowedCheckNames.Remove(lastBanishedCheckName);

            return new(allowedCheckNames, forbiddenCheckNames);
        }

        private ZigZagState monteCarloZigZagUp(ZigZagState zz, Stopwatch stopwatch)
        {
            HashSet<string> checkNames = new(zz.forbiddenCheckNames);
            HashSet<string> forbiddenCheckNames = new(zz.forbiddenCheckNames);
            HashSet<string> allowedCheckNames = new(zz.allowedCheckNames);
            string lastAddedCheckName = null;

            while (true)
            {
                if (checkNames.Count < 1)
                    break;

                checkZigZagDurationCausesError(stopwatch);

                string checkName = HintUtils.RemoveRandomHashSetItem(genData.rnd, checkNames);
                forbiddenCheckNames.Remove(checkName);
                allowedCheckNames.Add(checkName);

                HashSet<string> combinedForbiddenChecks = new(baseForbiddenChecks);
                combinedForbiddenChecks.UnionWith(forbiddenCheckNames);

                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    combinedForbiddenChecks
                );
                if (wasSuccess)
                {
                    if (markAsSometimesRequired(checkName))
                    {
                        lastAddedCheckName = checkName;
                    }
                    allowedCheckNames.Remove(checkName);
                    forbiddenCheckNames.Add(checkName);
                }
            }

            if (lastAddedCheckName == null)
                return null;

            // Re-allow the last added checkName and merge forbidden
            allowedCheckNames.Add(lastAddedCheckName);
            forbiddenCheckNames.Remove(lastAddedCheckName);

            return new(allowedCheckNames, forbiddenCheckNames);
        }

        private bool monteCarloZigZag(HashSet<string> checkNames)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            bool result = false;
            List<ZigZagState> zzStack = new() { new(new(checkNames), new()) };

            while (true)
            {
                List<ZigZagState> downStates = zzStack;
                zzStack = new();
                foreach (ZigZagState downState in downStates)
                {
                    while (true)
                    {
                        long elapsedMs = stopwatch.ElapsedMilliseconds;
                        Console.WriteLine(
                            $"--Starting zigZagDown; elapsedMs for this zigZag is: {elapsedMs} ms."
                        );
                        ZigZagState step = monteCarloZigZagDown(downState, stopwatch);
                        if (step != null)
                        {
                            result = true;
                            zzStack.Add(step);
                        }
                        else
                            break;
                    }
                }

                if (zzStack.Count < 1)
                    break;

                List<ZigZagState> upStates = zzStack;
                zzStack = new();
                foreach (ZigZagState upState in upStates)
                {
                    while (true)
                    {
                        long elapsedMs = stopwatch.ElapsedMilliseconds;
                        Console.WriteLine(
                            $"--Starting zigZagUp; elapsedMs for this zigZag is: {elapsedMs} ms."
                        );
                        ZigZagState step = monteCarloZigZagUp(upState, stopwatch);
                        if (step != null)
                            zzStack.Add(step);
                        else
                            break;
                    }
                }

                if (zzStack.Count < 1)
                    break;
            }

            return result;
        }

        public HashSet<string> run()
        {
            HashSet<string> locsSet = new();

            bool isPoeSoulMajor = genData.majorItems.Contains(Item.Poe_Soul);

            markClearlyDeadChecksGivingTradeItems();
            calcBaseForbiddenChecks();

            // Add non-Required checks from the playthrough spheres which are guaranteed to be
            // conditionallyRequired (playthrough failed when removing them conditionally).
            foreach (
                List<KeyValuePair<int, Item>> spherePairs in genData.playthroughSpheres.spheres
            )
            {
                foreach (KeyValuePair<int, Item> checkAndItem in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(checkAndItem.Key);
                    Item contents = HintUtils.getCheckContents(checkName);

                    // Note: skip over non-major Poe Souls to match general behavior.
                    if (
                        !genData.requiredChecks.Contains(checkName)
                        && (contents != Item.Poe_Soul || isPoeSoulMajor)
                    )
                    {
                        condRequiredChecks.Add(checkName);
                        Console.WriteLine(
                            $"Sometimes Required (spheres): {checkName} ({checkAndItem.Value})"
                        );
                    }
                }
            }

            // Build `locsSet`
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;
                Item item = check.itemId;

                // We skip over non-logicalItems and checks which are already know to be "required"
                // or "not required". Already known sometimesRequired checks are still added since
                // they take part in the algorithm for finding other sometimesRequired checks. Also
                // note that we skip over non-major Poe Souls since those get special handling.
                if (
                    (item == Item.Poe_Soul && !isPoeSoulMajor)
                    || !genData.logicalItems.Contains(item)
                    || genData.requiredChecks.Contains(checkName)
                    || genData.allowBarrenChecks.Contains(checkName)
                    || genData.notReqChecks.Contains(checkName)
                )
                    continue;
                locsSet.Add(checkName);
            }

            prevCondRequiredChecks = new(condRequiredChecks);

            Stopwatch stopwatch = new();
            stopwatch.Start();

            int zigZagNumber = 0;
            int consecutiveFailures = 0;
            long prevElapsedMs = 0;
            while (true)
            {
                zigZagNumber += 1;

                if (monteCarloZigZag(locsSet))
                    consecutiveFailures = 0;
                else
                    consecutiveFailures += 1;

                if (prevCondRequiredChecks.Count != condRequiredChecks.Count)
                {
                    if (checkMarkSphere0Checks())
                        consecutiveFailures = 0;
                    if (checkMarkTradeItemSources())
                        consecutiveFailures = 0;
                }
                prevCondRequiredChecks = new(condRequiredChecks);

                long elapsedMs = stopwatch.ElapsedMilliseconds;
                Console.WriteLine(
                    $"--Finished zigZag #{zigZagNumber}; elapsedMs is: {elapsedMs} ms; consecutiveFailures is {consecutiveFailures}"
                );

                long breakThreshold = 100_000;

                // Break out if taking too long. Should capture either everything or almost
                // everything the first time usually.
                if (elapsedMs >= breakThreshold)
                {
                    Console.WriteLine(
                        $"--Breaking since next elapsedMs is {elapsedMs}ms (threshold {breakThreshold})"
                    );
                    break;
                }
                if (prevElapsedMs > 0)
                {
                    long expectedElapsedMs = 2 * elapsedMs - prevElapsedMs;
                    if (expectedElapsedMs >= breakThreshold)
                    {
                        Console.WriteLine(
                            $"--Breaking since next expectedElapsedMs is {expectedElapsedMs}ms (threshold {breakThreshold})"
                        );
                        break;
                    }
                }
                prevElapsedMs = elapsedMs;

                // Keep going for at least 20s to reduce chances we miss anything. For race seeds,
                // run until 5 consecutive failures instead of the normal 3.
                int consecutiveFailureThreshold = genData.isRaceSeed ? 5 : 3;
                // if (consecutiveFailures >= consecutiveFailureThreshold && elapsedMs >= 20_000)
                // TODO: revert temp duration reduction for testing
                if (consecutiveFailures >= consecutiveFailureThreshold && elapsedMs >= 2_000)
                {
                    Console.WriteLine(
                        $"Has at least {consecutiveFailureThreshold} consecutive failures (at {consecutiveFailures}) and at least 20s. Will break."
                    );
                    break;
                }
            }

            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            Console.WriteLine(
                $"ConditionallyRequired Elapsed Time: {elapsed.TotalMilliseconds} ms"
            );

            int numPoeSoulMarkedNotReq = 0;
            foreach (string checkName in locsSet)
            {
                if (!condRequiredChecks.Contains(checkName))
                {
                    Item contents = HintUtils.getCheckContents(checkName);
                    if (contents == Item.Poe_Soul)
                        numPoeSoulMarkedNotReq += 1;
                    else
                        Console.WriteLine($"- marked locsSet notReq: {checkName} ({contents})");
                    genData.notReqChecks.Add(checkName);
                }
            }

            if (numPoeSoulMarkedNotReq > 0)
                Console.WriteLine(
                    $"- condReq also marked {numPoeSoulMarkedNotReq} Poe Soul checks as notReq."
                );

            return condRequiredChecks;
        }

        private class ZigZagState
        {
            public HashSet<string> allowedCheckNames { get; set; }
            public HashSet<string> forbiddenCheckNames { get; set; }

            public ZigZagState(
                HashSet<string> allowedCheckNames,
                HashSet<string> forbiddenCheckNames
            )
            {
                this.allowedCheckNames = allowedCheckNames;
                this.forbiddenCheckNames = forbiddenCheckNames;
            }
        }
    }
}
