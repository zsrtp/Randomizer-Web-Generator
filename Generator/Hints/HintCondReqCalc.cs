namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TPRandomizer.Util;
    using TPRandomizer.SSettings.Enums;
    using System.Threading;

    public class HintCondReqCalc
    {
        private HintGenData genData;
        private HashSet<string> condRequiredChecks = new();
        private bool markedCondReqChecks = false;
        private int zigZagCount = 0;

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
            // themselves sometimes required, as long as they are not already known to be required
            // or allowBarren. allowBarren example: maleAnt => domRod. 2nd maleAnt behind DDR.
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

        private ZigZagState monteCarloZigZagDown(ZigZagState zz)
        {
            HashSet<string> checkNames = new(zz.allowedCheckNames);
            HashSet<string> allowedCheckNames = new(zz.allowedCheckNames);
            HashSet<string> forbiddenCheckNames = new(zz.forbiddenCheckNames);
            string lastBanishedCheckName = null;

            HashSet<string> prevReachedChecks = new();
            HintUtils.CalcBeatableWithForbiddenChecks(
                genData.startingRoom,
                forbiddenCheckNames,
                prevReachedChecks
            );

            while (true)
            {
                if (checkNames.Count < 1)
                    break;

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

                // If removing check prevented access to sometimesRequired checks and it has not yet
                // been marked sometimesRequired, then mark as sometimesRequired.
                if (!condRequiredChecks.Contains(checkName))
                {
                    foreach (string prevReachedCheck in prevReachedChecks)
                    {
                        if (
                            condRequiredChecks.Contains(prevReachedCheck)
                            && !nextReachedChecks.Contains(prevReachedCheck)
                        )
                        {
                            // Can no longer access previously accessible sometimesRequired check.
                            // Therefore the removed check must also be sometimesRequired.
                            markedCondReqChecks = true;
                            Item contents = HintUtils.getCheckContents(checkName);
                            Console.WriteLine(
                                $"Marked pending (zigZagDown): {checkName} ({contents}); lost access to '{prevReachedCheck}' ({HintUtils.getCheckContents(prevReachedCheck)})"
                            );
                            condRequiredChecks.Add(checkName);
                            break;
                        }
                    }
                }
                prevReachedChecks = nextReachedChecks;
            }

            if (lastBanishedCheckName == null)
                return null;

            // Re-forbid the last banished checkName. In the code above, we
            // always revert the allowed/forbidden changes if the seed becomes
            // unbeatable (meaning it is always beatable at the start of the
            // while loop). Here we reverse this for the last check we found
            // which had an outcome on the beatability of the seed when we
            // forbid it. Even if we added forbidden checks after this one
            // (meaning they did not prevent us from beating the seed), it is
            // still guaranteed that re-forbidding the lastBanishedCheck will
            // make the seed unbeatable which is what we want when the result is
            // next processed by the zigZagUp.
            forbiddenCheckNames.Add(lastBanishedCheckName);
            allowedCheckNames.Remove(lastBanishedCheckName);

            return new(allowedCheckNames, forbiddenCheckNames);
        }

        private ZigZagState monteCarloZigZagUp(ZigZagState zz)
        {
            HashSet<string> checkNames = new(zz.forbiddenCheckNames);
            HashSet<string> forbiddenCheckNames = new(zz.forbiddenCheckNames);
            HashSet<string> allowedCheckNames = new(zz.allowedCheckNames);
            string lastAddedCheckName = null;

            HashSet<string> prevReachedChecks = new();
            HintUtils.CalcBeatableWithForbiddenChecks(
                genData.startingRoom,
                forbiddenCheckNames,
                prevReachedChecks
            );

            while (true)
            {
                if (checkNames.Count < 1)
                    break;

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

                // If adding check allowed access to sometimesRequired checks and it has not yet
                // been marked sometimesRequired, then mark as sometimesRequired. This one adds
                // checks less frequently than zigZagDown, but I have seen it add one before. -isaac
                if (!condRequiredChecks.Contains(checkName))
                {
                    foreach (string nextReachedCheck in nextReachedChecks)
                    {
                        if (
                            condRequiredChecks.Contains(nextReachedCheck)
                            && !prevReachedChecks.Contains(nextReachedCheck)
                        )
                        {
                            // Can now access newly accessible sometimesRequired check. Therefore
                            // the added check must also be sometimesRequired.
                            markedCondReqChecks = true;
                            Item contents = HintUtils.getCheckContents(checkName);
                            Console.WriteLine(
                                $"Marked pending (zigZagUp): {checkName} ({contents}); gained access to '{nextReachedCheck}' ({HintUtils.getCheckContents(nextReachedCheck)})"
                            );
                            condRequiredChecks.Add(checkName);
                            break;
                        }
                    }
                }
                prevReachedChecks = nextReachedChecks;
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
                        ZigZagState step = monteCarloZigZagDown(downState);
                        // this.progress();
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
                        ZigZagState step = monteCarloZigZagUp(upState);
                        // this.progress();
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

            // TODO: don't add poeSouls or hearts to condReq from the spheres. They will be handled
            // in their own functions.

            // continued: also don't add small or big keys? They will be added based on key settings
            // below. And if they aren't added, then they will not be marked condReq (ex:
            // ownDungeon). However, if one is optional for accessing a sometimesRequired check,
            // then checks which are sometimesRequired to access it should also be sometimesRequired
            // for accessing the later check, so it should work out. Basically this means it is
            // assumed the player does not skip OwnDungeon small keys for no reason?

            // Add non-required checks from the playthrough spheres which are guaranteed to be
            // conditionallyRequired (playthrough failed when removing them conditionally).
            foreach (
                List<KeyValuePair<int, Item>> spherePairs in genData.playthroughSpheres.spheres
            )
            {
                foreach (KeyValuePair<int, Item> checkAndItem in spherePairs)
                {
                    // TODO: don't mark smallKeys and bigKeys automatically? Handle later?
                    string checkName = CheckIdClass.GetCheckName(checkAndItem.Key);
                    // Item contents = checkAndItem.Value;
                    if (!genData.requiredChecks.Contains(checkName))
                    {
                        condRequiredChecks.Add(checkName);
                        Console.WriteLine(
                            $"Sometimes Required (spheres): {checkName} ({checkAndItem.Value})"
                        );
                    }
                }
            }

            // TODO: run a function which sees if it should mark poeSouls as
            // sometimesRequired. Only marked if:
            // - find larger threshold of "HC requirement is poe souls" or "HC
            //   BK requirement is poe souls". Then the starting amount must not
            //   be greater than this. If those are true, then all Poe Souls are
            //   automatically marked as Sometimes Required.
            // - If neither of those settings are poe souls, then poe souls are
            //   not even considered major items, so they just show as purple
            //   text with no modifier in hints.
            // - REGARDLESS: poe souls are NEVER included in the locsSet since
            //   this would make generation take a crazy amount of time (13
            //   minutes plus).

            // TODO: same applies for heart containers and heart pieces. Though
            // the threshold checks are a little different. Lots of notes in
            // text file about this. Read through those (txt 152, line ~850ish).

            // TODO: can mark the Sometimes Required poe souls up front.
            // However, the zigZag still relies on the seed becoming unbeatable.
            // But we would like for a domRod that leads to a sometimesRequired
            // poe soul to be considered sometimesRequired. So what we should do
            // is keep track of the ones that remove access to at least one poe
            // soul on the zigZagDown. Then after we are done, we can go ahead
            // and merge those into the known sometimesRequired checks.

            // Used with glitchless logic to speed up execution time.
            if (genData.hiddenSkillsAutoSometimesRequired)
            {
                List<string> checksForItem = genData.itemToChecksList[
                    Item.Progressive_Hidden_Skill
                ];
                foreach (string checkName in checksForItem)
                {
                    if (
                        !condRequiredChecks.Contains(checkName)
                        && !genData.requiredChecks.Contains(checkName)
                        && !genData.allowBarrenChecks.Contains(checkName)
                    )
                    {
                        Console.WriteLine(
                            $"Sometimes Required (HdnSkl): {checkName} ({Item.Progressive_Hidden_Skill})"
                        );
                        condRequiredChecks.Add(checkName);
                    }
                }
            }

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
                    )
                    {
                        Console.WriteLine($"Sometimes Required (SmKey): {checkName} ({item})");
                        condRequiredChecks.Add(checkName);
                    }
                }
            }

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
                    )
                    {
                        Console.WriteLine($"Sometimes Required (BigKey): {checkName} ({item})");
                        condRequiredChecks.Add(checkName);
                    }
                }
            }

            // TODO: need to handle calculating skippable checks as well. Any logical checks which
            // are not required, allowBarren, or sometimesRequired.

            // continued: this can probably be done back in the HintGenData file after (potentially)
            // running this file based on settings. Any logical checks which are not marked as
            // "required", "sometimes required", or "not required" are therefore "skippable". What
            // exactly blocks barren (skippable vs sometimesRequired) is based on the distribution
            // (or maybe settings based on checkboxes we might give the user)?

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;
                Item item = check.itemId;

                // Certain items are never added to the `locsSet` for performance reasons. We also
                // skip over non-logicalItems and checks which are already know to be "required" or
                // "not required". It is critical we keep locsSet as small as possible. With enough
                // checks, calculation can take more than 10 minutes, 20 minutes, etc.
                if (
                    !genData.condReqLogicalItems.Contains(item)
                    || genData.requiredChecks.Contains(checkName)
                    || genData.allowBarrenChecks.Contains(checkName)
                    || item == Item.Poe_Soul
                    || item == Item.Heart_Container
                    || item == Item.Piece_of_Heart
                    || smallKeyItems.Contains(item)
                    || bigKeyItems.Contains(item)
                    || HintUtils.IsTradeItem(item)
                    || (
                        item == Item.Progressive_Hidden_Skill
                        && genData.hiddenSkillsAutoSometimesRequired
                    )
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

                // TODO: maybe the 60s threshold for races should come from the hint distribution
                // file. If it is more than 100k ms, then it stays at 100k ms. This way we can
                // adjust based on the relative complexity of the race settings since it isn't
                // really one size fits all.
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
                // possible that we got all of the sometimes required checks.
                if (genData.isRaceSeed)
                {
                    if (consecutiveFailures >= 5 && elapsedMs >= 10_000)
                    {
                        Console.WriteLine(
                            $"Race seed with {consecutiveFailures} consecutive failures and at least 10s. Will break."
                        );
                        // Wait then break
                        if (elapsedMs < 60_000)
                        {
                            int sleepDuration = (int)(60_000 - elapsedMs);
                            Console.WriteLine(
                                $"Race seed, sleeping for {sleepDuration} ms before breaking to reach 1 min."
                            );
                            // TODO: re-enable this
                            // Thread.Sleep(sleepDuration);
                            break;
                        }
                        else
                            break;
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

            Console.WriteLine("---Logical items which are not useful:");

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;

                // Skip over Poe Souls since they make the calculation an order
                // of magnitude slower (for example, 13m29s vs 1m6s). They will
                // be handled later and only if Jovani is required or
                // conditionallyRequired.
                if (
                    genData.logicalItems.Contains(check.itemId)
                    && !condRequiredChecks.Contains(checkName)
                    && !genData.requiredChecks.Contains(checkName)
                    && !HintUtils.checkIsPlayerKnownStatus(checkName)
                    && check.itemId != Item.Poe_Soul
                )
                    Console.WriteLine($"- {checkName} ({check.itemId})");
            }

            // Note: seed is guaranteed beatable using only required and sometimesRequired checks
            // since it is a superset of the spheres which are generated by keeping checks for which
            // removal causes an unbeatable seed. So no need to reconfirm that here.

            // Note: when getting the status of a check later on, any check which is not "required"
            // or "sometimesRequired" would be "not required".

            return this.condRequiredChecks;

            // We can't ignore vanilla checks since they might impact whether or
            // not randomized checks are conditionally required?

            // For example, imagine the following:
            // - vanilla sky characters
            // - 6 plandoed sky characters in sphere0
            // - start with 2 dominion rods
            // - only 1 bomb bag and it is in CitS
            // - only 1 B&C and it is sphere0
            // - only 1 rang and it is the first chest in KG cave

            // Actually it might be good in this case for the sphere0 sky
            // characters to not be barren blockers.

            // Maybe we should say "we assume that a player will always do
            // vanilla checks which reward logical items, so we do not consider
            // paths in which a player skips these".

            // We still would want to know if a vanilla check is useful for CAMC
            // though. Maybe we just always make them large if they are a
            // vanilla check which rewards a logical item.

            // We probably don't need to worry about "excluded" checks either,
            // since the contents of these should correctly filter them out, and
            // it might be possible to plando an item on an excluded check? Idk
            // what the status would be in that case.

            // If the Jovani rewards are all non-logical though, we can
            // definitely skip over poe souls though.

            // We can probably do this for AgithaRewards and Sketch as well. Any
            // ones where the item only trades for a check and it does not on
            // its own open up any paths, etc.

            // What if "if it is not possible to skip ALL checks rewarding poe
            // souls, then we will consider them all conditionally required.
            // However, checks rewarding poe souls can still show up in barren
            // areas."

            // EXCEPT: we should at least not consider a check rewarding a poe
            // soul conditionally required if it is logically locked behind
            // already doing the highest Jovani reward which gives a logical
            // item.
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
