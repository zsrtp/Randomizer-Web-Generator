namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using TPRandomizer.SSettings.Enums;

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
        private const int RaceSeedMinCalcDurationMs = 25_000;

        private HintGenData genData;
        private HashSet<string> condRequiredChecks = new();
        private bool markedCondReqChecks = false;

        public HintCondReqCalc(HintGenData genData)
        {
            this.genData = genData;
        }

        private static readonly HashSet<Item> smallKeyItems =
            new()
            {
                Item.Forest_Temple_Small_Key,
                Item.Goron_Mines_Small_Key,
                Item.Lakebed_Temple_Small_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Snowpeak_Ruins_Small_Key,
                Item.Temple_of_Time_Small_Key,
                Item.City_in_The_Sky_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Hyrule_Castle_Small_Key,
                Item.Snowpeak_Ruins_Ordon_Pumpkin,
                Item.Snowpeak_Ruins_Ordon_Goat_Cheese,
            };

        private static readonly HashSet<Item> bigKeyItems =
            new()
            {
                Item.Forest_Temple_Big_Key,
                Item.Goron_Mines_Key_Shard,
                Item.Lakebed_Temple_Big_Key,
                Item.Arbiters_Grounds_Big_Key,
                Item.Temple_of_Time_Big_Key,
                Item.Snowpeak_Ruins_Bedroom_Key,
                Item.City_in_The_Sky_Big_Key,
                Item.Palace_of_Twilight_Big_Key,
                Item.Hyrule_Castle_Big_Key,
            };

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

                long before = stopwatch.ElapsedMilliseconds;
                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    forbiddenCheckNames
                );
                long after = stopwatch.ElapsedMilliseconds;
                long diff = after - before;
                // Console.WriteLine($"zzd diff: {diff}ms");
                // if (diff > 500)
                // {
                //     Console.WriteLine($"Duration was {diff}ms, so doing garbage collection.");
                //     GC.Collect(); // Forces garbage collection of all generations
                //     GC.WaitForPendingFinalizers(); // Blocks until all finalizers have run (optional)
                // }

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

                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    forbiddenCheckNames
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

            // Add non-Required checks from the playthrough spheres which are guaranteed to be
            // conditionallyRequired (playthrough failed when removing them conditionally).
            foreach (
                List<KeyValuePair<int, Item>> spherePairs in genData.playthroughSpheres.spheres
            )
            {
                foreach (KeyValuePair<int, Item> checkAndItem in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(checkAndItem.Key);
                    if (!genData.requiredChecks.Contains(checkName))
                    {
                        condRequiredChecks.Add(checkName);
                        Console.WriteLine(
                            $"Sometimes Required (spheres): {checkName} ({checkAndItem.Value})"
                        );
                    }
                }
            }

            markClearlyDeadChecksGivingTradeItems();

            // Build `locsSet`
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;
                Item item = check.itemId;

                // We skip over non-logicalItems and checks which are already know to be "required"
                // or "not required". Already known sometimesRequired checks are still added since
                // they take part in the algorithm for finding other sometimesRequired checks.
                if (
                    !genData.logicalItems.Contains(item)
                    || genData.requiredChecks.Contains(checkName)
                    || genData.allowBarrenChecks.Contains(checkName)
                    || genData.notReqChecks.Contains(checkName)
                )
                    continue;

                locsSet.Add(checkName);
            }

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

                if (markedCondReqChecks)
                {
                    markedCondReqChecks = false;
                    consecutiveFailures = 0;
                }

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

                // For race seeds, keep going until we break based on time. This is to obfuscate
                // info about the seed based on generation time, and we also want to be as sure as
                // possible that we got all of the sometimes required checks. It will either break
                // on time (handled above), or if it would quickly finish, we randomly wait between
                // 25 and 35 seconds.
                if (genData.isRaceSeed)
                {
                    if (consecutiveFailures >= 5 && elapsedMs >= 15_000)
                    {
                        Console.WriteLine(
                            $"Race seed with at least 5 consecutive failures (at {consecutiveFailures}) and at least 15s. Will break."
                        );

                        int sleepDuration = (int)(RaceSeedMinCalcDurationMs - elapsedMs);
                        sleepDuration += (int)new Random().NextInt64(10_000);
                        if (sleepDuration > 0)
                        {
                            Console.WriteLine(
                                $"Race seed, sleeping for {sleepDuration} ms with expected time {elapsedMs + sleepDuration} ms."
                            );
                            Thread.Sleep(sleepDuration);
                            break;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Race seed, breaking immediately since elapsedMs is {elapsedMs} ms."
                            );
                            break;
                        }
                    }
                }
                else if (consecutiveFailures >= 3)
                    break;
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
