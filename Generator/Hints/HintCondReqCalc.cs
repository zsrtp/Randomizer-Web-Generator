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

    public class HintCondReqCalc
    {
        private HintGenData genData;
        private HashSet<string> condRequiredChecks = new();
        private int zigZagCount = 0;

        public HintCondReqCalc(HintGenData genData)
        {
            this.genData = genData;
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

        private ZigZagState monteCarloZigZagDown(ZigZagState zz)
        {
            HashSet<string> checkNames = new(zz.allowedCheckNames);
            HashSet<string> allowedCheckNames = new(zz.allowedCheckNames);
            HashSet<string> forbiddenCheckNames = new(zz.forbiddenCheckNames);
            string lastBanishedCheckName = null;

            while (true)
            {
                if (checkNames.Count < 1)
                    break;

                string checkName = HintUtils.RemoveRandomHashSetItem(genData.rnd, checkNames);
                allowedCheckNames.Remove(checkName);
                forbiddenCheckNames.Add(checkName);

                bool wasSuccess = HintUtils.CalcBeatableWithForbiddenChecks(
                    genData.startingRoom,
                    forbiddenCheckNames
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

            while (true)
            {
                if (checkNames.Count < 1)
                    break;

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
            //
            //asdhfioasf
            // genData.lo
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

            // TODO: "genData.logicalItems" needs to be calculated differently.
            // It should use all major items as a base, and then it should
            // remove ones that are only there conditionally. And maybe we could
            // do the "allowBarrenChecks" stuff up front as well in order to
            // find the checks which can be filtered out of being included in
            // `locsSet` since they would be "not required".

            // When getting the status of a check later on, any check which is
            // not "required" or "sometimesRequired" would be "not required".

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;
                Item item = check.itemId;

                // Skip over Poe Souls since they make the calculation an order
                // of magnitude slower (for example, 13m29s vs 1m6s). They will
                // be handled later and only if Jovani is required or
                // conditionallyRequired.
                if (
                    item == Item.Poe_Soul
                    || genData.requiredChecks.Contains(checkName)
                    || !genData.condReqLogicalItems.Contains(check.itemId)
                    || genData.allowBarrenChecks.Contains(checkName)
                    || HintUtils.IsTradeItem(item)
                )
                    continue;

                locsSet.Add(checkName);
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();

            int consecutiveFailures = 0;
            while (true)
            {
                if (monteCarloZigZag(locsSet))
                    consecutiveFailures = 0;
                else
                    consecutiveFailures += 1;

                if (consecutiveFailures >= 3)
                    break;
            }

            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            // Console.WriteLine(
            //     $"ConditionallyRequired Elapsed Time: {elapsed.TotalMilliseconds} ms"
            // );

            // return results


            Console.WriteLine("---Logical items which are not useful:");

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict)
            {
                Check check = checkList.Value;
                string checkName = check.checkName;
                Item item = check.itemId;

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
