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
        private const int RaceSeedMinCalcDurationMs = 30_000;

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

        private bool updateSometimesRequiredFromTradeItems()
        {
            bool addedCheck = false;

            // Checks rewarding a trade item for a chain ending in a sometimesRequired reward are
            // themselves sometimes required, as long as they are not already known to be required,
            // allowBarren, or notReq. allowBarren example: maleAnt => domRod. 2nd maleAnt behind
            // DDR.
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
                    // Need to check against required checks as well since you can have something
                    // like this: required domRod on Agitha reward with 2 copies of the bug in
                    // sphere0. Neither bug is required, but either could be your first and you
                    // definitely need at least one.
                    if (
                        genData.requiredChecks.Contains(chainEndCheckName)
                        || condRequiredChecks.Contains(chainEndCheckName)
                    )
                    {
                        if (
                            genData.itemToChecksList.TryGetValue(
                                tradeItem,
                                out List<string> checksList
                            )
                        )
                        {
                            foreach (string checkName in checksList)
                            {
                                if (
                                    !condRequiredChecks.Contains(checkName)
                                    && !genData.requiredChecks.Contains(checkName)
                                    && !genData.allowBarrenChecks.Contains(checkName)
                                    && !genData.notReqChecks.Contains(checkName)
                                )
                                {
                                    Item contents = HintUtils.getCheckContents(checkName);
                                    Console.WriteLine(
                                        $"Sometimes Required (TrdItm): {checkName} ({contents})"
                                    );
                                    condRequiredChecks.Add(checkName);
                                    addedCheck = true;
                                }
                            }
                        }
                    }
                }
            }
            return addedCheck;
        }

        private bool handleHiddenSkills(Dictionary<Item, int> itemToInflexibleCount)
        {
            // Returns true if did specialHiddenSkillHandling, else false.
            if (genData.sSettings.logicRules == LogicRules.Glitchless)
            {
                // If there is an inflexible Hidden Skill (start with one or a required one), then
                // we automatically consider the rest to be "not required". Otherwise we will simply
                // consider them all to be sometimesRequired. This is for performance reasons.
                if (
                    itemToInflexibleCount.TryGetValue(
                        Item.Progressive_Hidden_Skill,
                        out int numInflexibleHiddenSkills
                    )
                    && numInflexibleHiddenSkills > 0
                )
                {
                    List<string> checksForItem = genData.itemToChecksList[
                        Item.Progressive_Hidden_Skill
                    ];
                    foreach (string checkName in checksForItem)
                    {
                        if (!genData.requiredChecks.Contains(checkName))
                            genData.notReqChecks.Add(checkName);
                    }
                }
                else
                {
                    // No inflexible Hidden Skills, so consider them all "sometimes required".
                    List<string> checksForItem = genData.itemToChecksList[
                        Item.Progressive_Hidden_Skill
                    ];
                    foreach (string checkName in checksForItem)
                    {
                        if (
                            !condRequiredChecks.Contains(checkName)
                            && !genData.requiredChecks.Contains(checkName)
                            && !genData.allowBarrenChecks.Contains(checkName)
                            && !genData.notReqChecks.Contains(checkName)
                        )
                        {
                            Console.WriteLine(
                                $"Sometimes Required (HdnSkl): {checkName} ({Item.Progressive_Hidden_Skill})"
                            );
                            condRequiredChecks.Add(checkName);
                        }
                    }
                }
                // Return true since did special hidden skill handling.
                return true;
            }
            return false;
        }

        private void handlePoeSouls(Dictionary<Item, int> itemToInflexibleCount)
        {
            // If poe souls are needed for HC or HC BK thresholds and the inflexible count does not
            // already take care of the threshold, then mark all unknownStatus ones as
            // sometimesRequired. We do not ever mark them as "not required" here since even if they
            // aren't used for these thresholds, it's possible a Jovani reward is sometimesRequired,
            // etc. However, we do not know the full condReq status of the Jovani rewards until
            // after condReq calculations in the file finish, so leave any notRequired marking up to
            // hintGenData which handles poeSouls and tradeItems following the calcs in this file.
            List<int> thresholds = new();

            if (genData.sSettings.castleRequirements == CastleRequirements.Poe_Souls)
                thresholds.Add(genData.sSettings.castleRequirementCount);

            if (genData.sSettings.castleBKRequirements == CastleBKRequirements.Poe_Souls)
                thresholds.Add(genData.sSettings.castleBKRequirementCount);

            int largestThreshold = 0;
            foreach (int threshold in thresholds)
            {
                if (threshold > largestThreshold)
                    largestThreshold = threshold;
            }

            if (largestThreshold < 0)
                return;

            if (largestThreshold > 0)
            {
                if (!itemToInflexibleCount.TryGetValue(Item.Poe_Soul, out int numInflexible))
                    numInflexible = 0;
                if (numInflexible < largestThreshold)
                {
                    // Finding Poe souls is useful for meeting a threshold, so mark as
                    // sometimesRequired.
                    if (
                        genData.itemToChecksList.TryGetValue(
                            Item.Poe_Soul,
                            out List<string> checksForItem
                        )
                    )
                    {
                        foreach (string checkName in checksForItem)
                        {
                            if (
                                !genData.requiredChecks.Contains(checkName)
                                && !genData.allowBarrenChecks.Contains(checkName)
                                && !genData.notReqChecks.Contains(checkName)
                            )
                            {
                                Console.WriteLine(
                                    $"Sometimes Required (poeSoul): {checkName} ({Item.Poe_Soul})"
                                );
                                condRequiredChecks.Add(checkName);
                            }
                        }
                    }
                }
            }
        }

        private void handleHearts()
        {
            // If Hearts useful for HC access, mark all with an unknown status as sometimesRequired.
            // Basic implementation for now since not able to change starting hearts or set
            // requirement to be less than 4.
            if (
                genData.sSettings.castleRequirements == CastleRequirements.Hearts
                || genData.sSettings.castleBKRequirements == CastleBKRequirements.Hearts
            )
            {
                List<Item> items = new() { Item.Heart_Container, Item.Piece_of_Heart, };

                foreach (Item item in items)
                {
                    if (genData.itemToChecksList.TryGetValue(item, out List<string> checksForItem))
                    {
                        foreach (string checkName in checksForItem)
                        {
                            if (
                                !genData.requiredChecks.Contains(checkName)
                                && !genData.allowBarrenChecks.Contains(checkName)
                                && !genData.notReqChecks.Contains(checkName)
                            )
                            {
                                Console.WriteLine(
                                    $"Sometimes Required (heart): {checkName} ({item})"
                                );
                                condRequiredChecks.Add(checkName);
                            }
                        }
                    }
                }
            }
        }

        private void handleSmallKeys()
        {
            // If small keys are anywhere or anyDungeon, then mark any key with an unknown status as
            // sometimesRequired.
            if (
                genData.sSettings.smallKeySettings == SmallKeySettings.Any_Dungeon
                || genData.sSettings.smallKeySettings == SmallKeySettings.Anywhere
            )
            {
                foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
                {
                    Check check = checkList.Value;
                    string checkName = check.checkName;
                    Item item = check.itemId;

                    if (
                        smallKeyItems.Contains(item)
                        && !condRequiredChecks.Contains(checkName)
                        && !genData.requiredChecks.Contains(checkName)
                        && !genData.allowBarrenChecks.Contains(checkName)
                        && !genData.notReqChecks.Contains(checkName)
                    )
                    {
                        Console.WriteLine($"Sometimes Required (SmKey): {checkName} ({item})");
                        condRequiredChecks.Add(checkName);
                    }
                }
            }
        }

        private void handleBigKeys()
        {
            // If big keys are anywhere or anyDungeon, then mark any key with an unknown status as
            // sometimesRequired.
            if (
                genData.sSettings.bigKeySettings == BigKeySettings.Any_Dungeon
                || genData.sSettings.bigKeySettings == BigKeySettings.Anywhere
            )
            {
                foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
                {
                    Check check = checkList.Value;
                    string checkName = check.checkName;
                    Item item = check.itemId;

                    if (
                        bigKeyItems.Contains(item)
                        && !condRequiredChecks.Contains(checkName)
                        && !genData.requiredChecks.Contains(checkName)
                        && !genData.allowBarrenChecks.Contains(checkName)
                        && !genData.notReqChecks.Contains(checkName)
                    )
                    {
                        Console.WriteLine($"Sometimes Required (BigKey): {checkName} ({item})");
                        condRequiredChecks.Add(checkName);
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

                HashSet<string> nextReachedChecks = new();
                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    forbiddenCheckNames,
                    nextReachedChecks
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

                HashSet<string> nextReachedChecks = new();
                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    forbiddenCheckNames,
                    nextReachedChecks
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

        public HashSet<string> run(Dictionary<Item, int> itemToInflexibleCount)
        {
            HashSet<string> locsSet = new();

            // Add non-required checks from the playthrough spheres which are guaranteed to be
            // conditionallyRequired (playthrough failed when removing them conditionally).
            foreach (
                List<KeyValuePair<int, Item>> spherePairs in genData.playthroughSpheres.spheres
            )
            {
                foreach (KeyValuePair<int, Item> checkAndItem in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(checkAndItem.Key);
                    Item contents = checkAndItem.Value;

                    // Skip over certain items which have special handling for the sake of
                    // consistency. We do not skip over tradeItems however.
                    if (
                        !genData.requiredChecks.Contains(checkName)
                        && contents != Item.Poe_Soul
                        && contents != Item.Heart_Container
                        && contents != Item.Piece_of_Heart
                        && !smallKeyItems.Contains(contents)
                        && !bigKeyItems.Contains(contents)
                    )
                    {
                        condRequiredChecks.Add(checkName);
                        Console.WriteLine(
                            $"Sometimes Required (spheres): {checkName} ({checkAndItem.Value})"
                        );
                    }
                }
            }

            bool specialHiddenSkillHandling = handleHiddenSkills(itemToInflexibleCount);

            handlePoeSouls(itemToInflexibleCount);
            handleHearts();
            handleSmallKeys();
            handleBigKeys();

            // Build `locsSet`
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;
                Item item = check.itemId;

                // Certain items are never added to the `locsSet` for performance reasons. We also
                // skip over non-logicalItems and checks which are already know to be "required" or
                // "not required". It is critical we keep locsSet as small as possible. With enough
                // checks, calculation can take more than 10 minutes, 20 minutes, etc. We only
                // include checks which can immediately be marked as notRequired if they are not
                // caculated to be sometimesRequired.
                if (
                    !genData.logicalItems.Contains(item)
                    || genData.requiredChecks.Contains(checkName)
                    || genData.allowBarrenChecks.Contains(checkName)
                    || genData.notReqChecks.Contains(checkName)
                    || item == Item.Poe_Soul
                    || item == Item.Heart_Container
                    || item == Item.Piece_of_Heart
                    || smallKeyItems.Contains(item)
                    || bigKeyItems.Contains(item)
                    || HintUtils.IsTradeItem(item)
                    || (specialHiddenSkillHandling && item == Item.Progressive_Hidden_Skill)
                )
                    continue;

                locsSet.Add(checkName);
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();

            // Initially handle sometimesRequired tradeItems leading to a required check. We handle
            // tradeItems how we do since we really want to keep `locsSet` as small as possible for
            // performance reasons. An increase in locsSet of just 8 checks caused execution to take
            // nearly 20s more in a tradeItem test I did. -isaac
            updateSometimesRequiredFromTradeItems();

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

                // Run after normal calcs so we can mark tradeItems which lead to newly discovered
                // sometimesRequired checks. If we add more, then reset failures.
                if (updateSometimesRequiredFromTradeItems())
                    consecutiveFailures = 0;

                long elapsedMs = stopwatch.ElapsedMilliseconds;
                Console.WriteLine(
                    $"--Finished zigZag #{zigZagNumber}; elapsedMs is: {elapsedMs} ms; consecutiveFailures is {consecutiveFailures}"
                );

                long breakThreshold = 100_000;
                if (genData.isRaceSeed && consecutiveFailures >= 3)
                    breakThreshold = 60_000;

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
                // 30 and 40 seconds.
                if (genData.isRaceSeed)
                {
                    if (consecutiveFailures >= 5 && elapsedMs >= 10_000)
                    {
                        Console.WriteLine(
                            $"Race seed with {consecutiveFailures} consecutive failures and at least 10s. Will break."
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

            foreach (string checkName in locsSet)
            {
                if (!condRequiredChecks.Contains(checkName))
                {
                    Console.WriteLine(
                        $"- marked locsSet notReq: {checkName} ({HintUtils.getCheckContents(checkName)})"
                    );
                    genData.notReqChecks.Add(checkName);
                }
            }

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
