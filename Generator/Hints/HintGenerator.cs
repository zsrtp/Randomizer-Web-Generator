namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SSettings.Enums;
    using TPRandomizer.Util;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Hints.HintCreator;
    using System.Threading;

    class HintGenerator
    {
        private HintGenData genData;
        private HashSet<string> deadNodeIds = new();
        private HintGenCache cache = new();
        private HintSettings hintSettings;
        private Dictionary<string, HintGroup> mutableGroups;

        public HintGenerator(
            Random rnd,
            SharedSettings sSettings,
            PlaythroughSpheres playthroughSpheres,
            Room startingRoom
        )
        {
            this.genData = new HintGenData(rnd, sSettings, playthroughSpheres, startingRoom);
        }

        public CustomMsgData Generate()
        {
            CustomMsgData.Builder customMsgDataBuilder =
                new(genData, (byte)Randomizer.RequiredDungeons);

            // If user specified that there are no hintSettings, then we should
            // return the default customMsgData settings.
            if (genData.sSettings.hintDistribution == HintDistribution.None)
                return customMsgDataBuilder.Build(genData.sSettings);

            hintSettings = HintSettings.fromPath(genData);

            mutableGroups = hintSettings.createMutableGroups();

            // Adjust selfHinters if able
            customMsgDataBuilder.ApplyInvalidSelfHinters(hintSettings.invalidSelfHinters);
            // Note: selfHinters do not count as having their checkContents
            // already hinted. You can think of them like shop items or golden
            // bugs where you can walk up and see exactly what the item is.

            if (hintSettings.starting.excludeFromGroups)
                removeSpotFromMutableGroups(hintSettings.starting.spot);

            SpotToHints specialSpotToHints = new SpotToHints();
            SpotToHints normalSpotToHints = new SpotToHints();

            // Agitha
            if (hintSettings.agitha)
            {
                specialSpotToHints.addHintsToSpot(SpotId.Agithas_Castle_Sign, getAgithaHint());
            }

            PrepareJovaniHints(specialSpotToHints);
            PrepareCaveOfOrdealsHints(specialSpotToHints);

            // Always
            List<List<Hint>> alwaysHintsForSpots = PrepareAlwaysHints(specialSpotToHints);

            int alwaysSpotCount = 0;
            if (!ListUtils.isEmpty(alwaysHintsForSpots))
                alwaysSpotCount = alwaysHintsForSpots.Count;

            // Create user-defined special hints after this point.
            PrepareUserSpecialHints(specialSpotToHints);

            // Run through the process of creating BeyondPoint hints, but update
            // 'hinted' without placing any hints that we might end up having to
            // remove later. Even though this is done up front, we have it such
            // that zones are not less likely to be hinted barren even if they
            // could potentially get a NothingBeyondThisPoint hint.
            CreateBeyondPointHints(specialSpotToHints, false);

            // Potentially create bigKey anti-casino hints. We do this before
            // hint groups so we do not get something like a TradeChain hint for
            // a big key when a more precise version of that same hint is
            // already in a guaranteed location (the BK anti-casino hint). This
            // also leaves room for the same potential TradeChain to hint
            // something more useful such as a different item on Agitha.
            CreateBigKeyHints(specialSpotToHints);

            // Store list of Always hints to apply to the group.
            // The size of the list is the penalty to apply.
            // We can know the groupId to apply it to based on the Always object
            // in the hintSettings.

            // For hintedBarren area gets barren hint:

            // - the number of copies are still applied to the pool. If the zone
            //   is not in the pool (such as a dungeon with overworld pool), the
            //   zone still gets a hint for itself and it is removed from all
            //   pools. If the zone already has non-special hints assigned,
            //   throw an error.

            // - When resolving the "ownBarren" status of a zone at the end, if
            //   the zone has no other hints on it, replace with a junk hint.
            //   Else it gets a Barren hint pointing at itself.

            // Can also have options for this: "hintedBarrenZonesSelfHint":
            // "junkHint|barrenHint|none" "none" means no special behavior.
            // "barrenHint" means expected, except never uses a Junk hint.
            // "junkHint" means the same as "barrenHint", but will use a junk
            // hint when possible.

            // Each iteration applies one layer of hints to spots.
            // foreach (HintDefGrouping grouping in hintSettings.hintDefGroupings)
            for (int hgIdx = 0; hgIdx < hintSettings.hintDefGroupings.Count; hgIdx++)
            {
                HintDefGrouping grouping = hintSettings.hintDefGroupings[hgIdx];
                string groupId = grouping.groupId;
                // HintGroup group = hintSettings.groups[groupId];
                HintGroup group = (HintGroup)mutableGroups[groupId].Clone();
                HashSet<SpotId> spots = group.spots;

                bool matchesAlwaysGroupId = hintSettings.always.groupId == groupId;

                int layerSize = spots.Count;
                // Note: alwaysSpotCount gets set back to 0 after we place the
                // Always hints.
                if (matchesAlwaysGroupId)
                    layerSize -= alwaysSpotCount;

                if (layerSize == 0)
                    continue;
                else if (layerSize < 0)
                    throw new Exception(
                        $"layer size reduced to '{layerSize}' when making room for Always hints."
                    );

                HintLayerData layerData = new HintLayerData(
                    genData,
                    hintSettings,
                    group,
                    layerSize
                );

                // Generate hints according to a tree defined in JSON.
                RecHintResults recHintResults = recursiveHandleHintDef(
                    layerData,
                    grouping.hintDef,
                    null,
                    hgIdx.ToString()
                );

                specialSpotToHints.addHintsToSpot(
                    hintSettings.starting.spot,
                    layerData.startingHints
                );

                if (hintSettings.barren.ownZoneBehavior != Barren.OwnZoneBehavior.Off)
                {
                    // Handle starting hints.
                    foreach (Hint hint in layerData.startingHints)
                    {
                        SpotId spotId = HintUtils.TryGetSpotIdForBarrenZoneHint(hint);
                        if (spotId != SpotId.Invalid)
                        {
                            if (spots.Contains(spotId))
                            {
                                // Need to place a copy of the hint at this
                                // spot and remove the spot from this group
                                // for the layer.
                                if (
                                    hintSettings.barren.isMonopolize()
                                    && normalSpotToHints.spotHasHints(spotId)
                                )
                                {
                                    throw new Exception(
                                        $"Expected spot '{spotId}' to have no normal hints with barren.ownZoneBehavior set to 'monopolize', but it was not empty."
                                    );
                                }

                                spots.Remove(spotId);
                                normalSpotToHints.addHintToSpot(spotId, hint);
                            }
                            else
                            {
                                specialSpotToHints.addHintToSpot(spotId, hint);
                            }

                            // Additionally, if monopolize and not just
                            // prioritize, need to remove the spot from
                            // ALL groups.
                            if (hintSettings.barren.isMonopolize())
                                removeSpotFromMutableGroups(spotId);
                        }
                    }

                    // Handle normal hints.
                    for (int i = recHintResults.HintDefResults.Count - 1; i >= 0; i--)
                    {
                        HintDefResult hintDefResult = recHintResults.HintDefResults[i];
                        Hint hint = hintDefResult.hint;
                        SpotId spotId = HintUtils.TryGetSpotIdForBarrenZoneHint(hint);
                        if (spotId != SpotId.Invalid)
                        {
                            if (spots.Contains(spotId))
                            {
                                // Need to place a copy of the hint at this
                                // spot and remove the spot from this group
                                // for the layer.
                                if (
                                    hintSettings.barren.isMonopolize()
                                    && normalSpotToHints.spotHasHints(spotId)
                                )
                                {
                                    throw new Exception(
                                        $"Expected spot '{spotId}' to have no normal hints with barren.ownZoneBehavior set to 'monopolize', but it was not empty."
                                    );
                                }

                                spots.Remove(spotId);
                                normalSpotToHints.addHintToSpot(spotId, hint);

                                hintDefResult.OnPlacedCopy();
                                if (!hintDefResult.CanPlaceMoreCopies())
                                    recHintResults.RemoveHintDefResultAt(i);
                            }
                            else
                            {
                                specialSpotToHints.addHintToSpot(spotId, hint);
                            }

                            // Additionally, if monopolize and not just
                            // prioritize, need to remove the spot from
                            // ALL groups.
                            if (hintSettings.barren.isMonopolize())
                                removeSpotFromMutableGroups(spotId);
                        }
                    }
                }

                // Iterate through all generated hints (including starting
                // hints). The starting hint one takes care of all copies. The
                // placement of a barren hint for that zone is done as a special
                // hint, so this is handled separately from the normal stuff for
                // the recursive return for this layer.

                // For the return from the recursive work, iterate through and
                // find any BarrenZone hints.

                // For any of these which point to a zone/spot belonging to the
                // group, reduce the copies by 1, remove that spot from the
                // mutable group for this layer, and place the hint at that spot
                // for this layer.

                // If the spot does not exist at the layer, it should get a
                // special copy of the hint assigned to it.

                // We should probably group the spotToHints by iteration.

                if (matchesAlwaysGroupId && alwaysSpotCount > 0)
                {
                    alwaysSpotCount = 0;

                    if (!ListUtils.isEmpty(alwaysHintsForSpots))
                    {
                        List<SpotId> spotList = new(spots);

                        foreach (List<Hint> alwaysHintsForSpot in alwaysHintsForSpots)
                        {
                            // In the future, we would have special logic to try
                            // to place the Always hints so that you can access
                            // them before doing the checks that they hint. For
                            // now, we just pick a spot randomly.
                            SpotId spotId = HintUtils.RemoveRandomListItem(genData.rnd, spotList);

                            if (
                                hintSettings.always.monopolizeSpots
                                && normalSpotToHints.spotHasHints(spotId)
                            )
                            {
                                throw new Exception(
                                    $"Expected spot '{spotId}' to have no normal hints with always.monopolizeSpots set to true, but it was not empty."
                                );
                            }

                            spots.Remove(spotId);
                            normalSpotToHints.addHintsToSpot(spotId, alwaysHintsForSpot);

                            if (hintSettings.always.monopolizeSpots)
                                removeSpotFromMutableGroups(spotId);
                        }
                    }
                }

                // Place the generated normal hints in the remaining spots.
                List<SpotId> spotsToFill = new(spots);
                while (recHintResults.HintDefResults.Count > 0 && spotsToFill.Count > 0)
                {
                    HintDefResult result = recHintResults.HintDefResults[0];
                    if (!result.CanPlaceMoreCopies())
                    {
                        recHintResults.RemoveHintDefResultAt(0);
                        continue;
                    }

                    Hint hintToPlace = result.hint;

                    SpotId spotId = HintUtils.RemoveRandomListItem(genData.rnd, spotsToFill);
                    normalSpotToHints.addHintToSpot(spotId, hintToPlace);

                    result.OnPlacedCopy();
                    if (!result.CanPlaceMoreCopies())
                        recHintResults.RemoveHintDefResultAt(0);
                }

                // Remove any items from recHintResults which met its
                // requirements (such as minCopies) but is still in the list.
                for (int i = recHintResults.HintDefResults.Count - 1; i >= 0; i--)
                {
                    HintDefResult result = recHintResults.HintDefResults[i];
                    if (result.PlacedEnoughCopies())
                        recHintResults.RemoveHintDefResultAt(i);
                }

                // If we used up our hints, fill in the remaining spots.
                if (
                    grouping.useFillerHints
                    && recHintResults.HintDefResults.Count < 1
                    && spotsToFill.Count > 0
                )
                {
                    AddFillerHintsToSpots(
                        spotsToFill,
                        (spotId, fillerHint) =>
                        {
                            normalSpotToHints.addHintToSpot(spotId, fillerHint);
                        }
                    );
                }

                // If we have leftover hints we did not place, then throw.
                if (recHintResults.HintDefResults.Count > 0)
                {
                    int remainingCopies = 0;
                    if (!ListUtils.isEmpty(recHintResults.HintDefResults))
                    {
                        foreach (HintDefResult result in recHintResults.HintDefResults)
                        {
                            remainingCopies += result.GetRemainingCopies();
                        }
                    }
                    throw new Exception(
                        $"Expected hints to be empty, but had {recHintResults.HintDefResults.Count} hints and {remainingCopies} copies left."
                    );
                }

                // If we were supposed to fill in spots but there were unfilled
                // spots, then throw.
                if (grouping.useFillerHints && spotsToFill.Count > 0)
                {
                    throw new Exception(
                        $"Expected spotsToFill to be empty, but had {spotsToFill.Count} spotsToFill left."
                    );
                }
            }
            // Done with building hint layers at this point.

            // Go through BeyondPoint spots again and place any signs we are
            // able to.
            CreateBeyondPointHints(specialSpotToHints, true);

            List<HintSpot> ret = CreateHintSpotList(specialSpotToHints, normalSpotToHints);
            customMsgDataBuilder.SetHintSpots(ret);
            return customMsgDataBuilder.Build(genData.sSettings);
        }

        private List<Hint> getAgithaHint()
        {
            int numBugsInPool = 0;
            List<string> interestingAgithaChecks = new();
            // List<Item> items = new();

            foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
            {
                string agithaRewardCheckName = pair.Value;
                genData.hinted.hintsShouldIgnoreChecks.Add(agithaRewardCheckName);

                // If not included, skip over it
                if (HintUtils.checkIsPlayerKnownStatus(agithaRewardCheckName))
                    continue;

                numBugsInPool += 1;

                // Item contents = HintUtils.getCheckContents(agithaRewardCheckName);
                // if (
                //     genData.preventBarrenItems.Contains(contents)
                //     && !HintConstants.bugsToRewardChecksMap.ContainsKey(contents)
                // )
                // {

                Item contents = HintUtils.getCheckContents(agithaRewardCheckName);
                if (
                    genData.CheckIsGood(agithaRewardCheckName, true)
                    && !HintConstants.bugsToRewardChecksMap.ContainsKey(contents)
                )
                {
                    // Interesting contents which are not a bug.
                    interestingAgithaChecks.Add(agithaRewardCheckName);
                    // items.Add(HintUtils.getCheckContents(agithaRewardCheckName));
                }

                // if item is preventBarren and not a bug, then add to the list

                // Determine if there is anything interesting on Agitha. If
                // there isn't, then she is considered dead and bugs should not
                // prevent barren.

                // IMPORTANT: if Agitha has nothing, then bugs should not
                // prevent barren.
            }

            if (interestingAgithaChecks.Count < 1)
            {
                // Bugs should no longer prevent barren.
                foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
                {
                    genData.preventBarrenItems.Remove(pair.Key);
                }
            }

            if (numBugsInPool < 1)
                return null;

            // Shuffle list so no info is given away by the order the items are
            // listed on the sign.
            HintUtils.ShuffleListInPlace(genData.rnd, interestingAgithaChecks);

            AgithaRewardsHint hint = new AgithaRewardsHint(
                genData,
                numBugsInPool,
                interestingAgithaChecks
            );

            if (interestingAgithaChecks.Count < 1)
                genData.hinted.agithaHintedDead = true;

            return new List<Hint>() { hint };
        }

        private RecHintResults recursiveHandleHintDef(
            HintLayerData layerData,
            HintDef currHintDef,
            HintDefProps prevDefProps,
            string nodeId
        )
        {
            bool pushedStarting = false;
            RecHintResults results = new();
            HintDefProps currDefProps = HintDefProps.mergeCreate(prevDefProps, currHintDef);
            cache.PushNodeId(nodeId);

            if (currDefProps.starting > 0)
            {
                pushedStarting = true;
                layerData.pushStarting(currDefProps.starting);
            }

            bool pushedMaxPicks = layerData.CheckPushMaxPicks(currDefProps);

            if (currHintDef.hintCreator != null)
            {
                // Calc how much space left, then figure out how many hints to ask for.

                // Max is remaining / copies per. Ex: 19 / 4 copies => 4 rem 3.
                // Remainder divided by minCopies. Ex: min 2; 3 / 2 => 1 extra.
                // Total is 4 at 4 copies and 1 at 2 copies.

                // These are the results of the function (List<Pairs>) with the
                // number of copies set correctly.

                // Then we calc how many spots are used by iterating over the
                // list of what was passed back from the tryCreateHint (which
                // might be less than we asked for, or null).

                // This is also where we would handle the special junk hints for
                // barren zones stuff (can worry about later).

                bool doBarrenZoneHandling = false;
                if (
                    currHintDef.hintCreator.type == HintCreatorType.Barren
                    && hintSettings.barren.ownZoneBehavior != Barren.OwnZoneBehavior.Off
                )
                {
                    BarrenHintCreator bhCreator = currHintDef.hintCreator as BarrenHintCreator;
                    if (bhCreator != null)
                        doBarrenZoneHandling = bhCreator.HintsZone();
                }

                if (doBarrenZoneHandling)
                {
                    // How do we know that we have space to generate the barrenZone hint?

                    for (int i = 0; i < currDefProps.iterations; i++)
                    {
                        if (layerData.remainingSpots < 1)
                        {
                            // Need at least 1 spot even with unusedStarting so we
                            // can pay any potential -1 penalties.
                            break;
                        }

                        int numUnusedStaring = layerData.getCummUnusedStarting();

                        if (
                            numUnusedStaring < 1
                            && layerData.remainingSpots < currDefProps.copies
                            && (
                                currDefProps.minCopies < 1
                                || layerData.remainingSpots < currDefProps.minCopies
                            )
                        )
                        {
                            // Break if no unused starting and not enough spots
                            // left in the layer for either copies or minCopies
                            // (if minCopies is defined)
                            break;
                        }

                        int? currPicksLeft = layerData.GetCurrPicksLeft();
                        if (currPicksLeft != null)
                        {
                            int currPicksLeftInt = (int)currPicksLeft;
                            if (currPicksLeftInt < 1)
                                break;
                        }

                        // Need to make sure don't generate the hint if we are
                        // not sure we can handle it.

                        // If there are not any unusedStaring left, then we
                        // simply need to check that copies fits in the
                        // remainingSpots of layerData (can worry about
                        // minCopies in a minute).

                        // If there are unusedStarting left and none are
                        // guaranteed, we still must generate with the
                        // assumption that none of the generated hints are
                        // selected as starting.

                        // However, if one was theoretically to be selected as a
                        // starting hint, we would potentially need to pay its
                        // penalty.

                        List<Hint> hints = currHintDef.hintCreator.tryCreateHint(
                            genData,
                            hintSettings,
                            1,
                            cache
                        );

                        if (ListUtils.isEmpty(hints))
                            break;
                        else if (hints.Count > 1)
                            throw new Exception(
                                $"Was expecting BarrenZoneHintCreator to produce 1 hint, but produced '{hints.Count}'."
                            );

                        BarrenHint hint = hints[0] as BarrenHint;
                        if (hint == null)
                            throw new Exception("Unable to cast hint to BarrenHint.");

                        Zone zone = ZoneUtils.StringToId(hint.areaId.stringId);

                        if (zone == Zone.Invalid)
                            throw new Exception(
                                $"Failed to parse '{hint.areaId.stringId}' to valid HintZoneId."
                            );

                        HintDefResult hintDefResult = HintDefResult.FromHintDefProps(
                            hint,
                            currDefProps
                        );
                        results.AddHintDefResult(hintDefResult);

                        // If there are unusedStarting left, we

                        // Map zone to spot and check if spot is in current
                        // layer's group. It is possible that the result is
                        // Invalid (if it was Golden Wolf for example, which is
                        // a valid zone to hint barren).
                        SpotId spotId = ZoneUtils.IdToSpotId(zone);
                        layerData.updateUsingStartingBarrenHint(hintDefResult, spotId);

                        // // If there are no unusedStarting, then we do not care if the zone of the hint is in the group.
                        // // We just add it to the result and update the number of spots.
                        // if (numUnusedStaring < 1)
                        // {
                        //     layerData.updateSpotsLeftSingle(hintDefResult);
                        //     continue;
                        // }

                        // For each one that is generated that hits a spot in
                        // the current group, we add a -1 penalty. The total number of penalties that
                        // we can create is
                    }
                }
                else
                {
                    int startingAllowed = layerData.getCummUnusedStarting();

                    int fullAllowed = layerData.remainingSpots / currDefProps.copies;

                    int partialAllowed = 0;
                    if (currDefProps.minCopies > 0)
                    {
                        int remainder = layerData.remainingSpots % currDefProps.copies;
                        if (currDefProps.minCopies <= remainder)
                            partialAllowed = 1;
                    }

                    int maxAllowed = startingAllowed + fullAllowed + partialAllowed;

                    int numHints = currDefProps.iterations;
                    if (numHints > maxAllowed)
                        numHints = maxAllowed;

                    int? currPicksLeft = layerData.GetCurrPicksLeft();
                    if (currPicksLeft != null)
                    {
                        int currPicksLeftInt = (int)currPicksLeft;
                        if (numHints > currPicksLeftInt)
                            numHints = currPicksLeftInt;
                    }

                    // Do not call tryCreateHint if we want to generate 0 hints.
                    if (numHints > 0)
                    {
                        List<Hint> hints = currHintDef.hintCreator.tryCreateHint(
                            genData,
                            hintSettings,
                            numHints,
                            cache
                        );

                        if (!ListUtils.isEmpty(hints))
                        {
                            foreach (Hint hint in hints)
                            {
                                results.AddHintDefResult(
                                    HintDefResult.FromHintDefProps(hint, currDefProps)
                                );
                            }

                            layerData.updateSpotsLeft(results.HintDefResults);
                        }
                    }
                    else
                    {
                        int abc = 7;
                    }
                }
            }
            else
            {
                HintDefArrayProcessor arrProcessor = HintDefArrayProcessor.Create(
                    genData,
                    currDefProps,
                    currHintDef,
                    nodeId,
                    deadNodeIds
                );

                int idx;
                while ((idx = arrProcessor.NextIndex()) >= 0)
                {
                    if (layerData.killSwitched || layerData.IsMaxPicksSwitched())
                        break;

                    string childNodeId = $"{nodeId}.{idx}";

                    RecHintResults recHintResults = recursiveHandleHintDef(
                        layerData,
                        currHintDef.hintDefs[idx],
                        currDefProps,
                        childNodeId
                    );

                    results.MergeChildRecHintResults(recHintResults);

                    if (!recHintResults.didProduceHints)
                    {
                        // Note: we only want to mark a node as dead if it did
                        // not product any results. We have to be careful to not
                        // mark a node as dead if it produced hints, but all
                        // hints were picked as starting hints. In this case, it
                        // appears to have produced no hints, but in reality it
                        // did, so we should not mark it as dead.
                        deadNodeIds.Add(childNodeId);
                    }
                }

                int abc = 7;

                // For maxPicks, there is an array of active ones. We need to
                // take them into account when calculating how many hints we can
                // ask a creator for.

                // Also, it is possible for a parent one to cap before a child
                // one. For example, if there is one high up that allows 2, and
                // we have one lower than allows 4, then we will always resolve
                // the 2 one first. Therefore, we do not need to append one to
                // the array if it is the same or larger than the current value.

                // For example, if we had one which was 3 and it reduced to 1
                // (since 2 hints were generated), then we do not need to later
                // worry about appending one which has maxPicks of 1 since we
                // will already cut off from 1.

                // Imagine how we would handle it if the array was 1 million long.

                // We want to avoid having to decrement 1 from every element in
                // the array every time. Not feasible at large scale, but mainly
                // just not the optimal way of handling it.

                // We should keep track of the starting value as an array.

                // We can keep track of remainingPicks as a single int?

                // Imagine we append to array [7, 5, 2] before generating hints
                // (as we go down a tree), then we reach an array.

                // The current int would be 2. When we generate a hint, we
                // subtract from this. When we go back up the tree, we take the
                // difference that was there (started at 2, now 1, so diff of 2
                // - 1 => 1) and update the int from that. For example, the diff
                //   was 1, so when we pop back to the 5 one, we need to update
                //   the current int to be 5 - 1 => 4, meaning we can generate
                //   at most 4 more picks under this part of the tree.
            }

            // If should save to var, then save to var. This can happen on a
            // node or leaf.
            if (!StringUtils.isEmpty(currDefProps.saveToVar))
            {
                List<Hint> hints = new();
                foreach (HintDefResult hintDefResult in results.HintDefResults)
                {
                    hints.Add(hintDefResult.hint);
                }
                genData.vars.SaveToVar(currDefProps.saveToVar, hints);
            }

            if (pushedMaxPicks)
                layerData.PopMaxPicks();

            if (pushedStarting)
            {
                layerData.popAndUpdateStarting(currDefProps, results);
            }

            cache.PopNodeId();

            return results;
        }

        private class SpotToHints
        {
            private Dictionary<SpotId, List<Hint>> spotToHints = new();

            public void addHintToSpot(SpotId spot, Hint hint, bool atFront = false)
            {
                if (spot == SpotId.Invalid || hint == null)
                    return;

                if (!spotToHints.ContainsKey(spot))
                    spotToHints[spot] = new();

                List<Hint> hintsForSpot = spotToHints[spot];
                if (atFront)
                    hintsForSpot.Insert(0, hint);
                else
                    hintsForSpot.Add(hint);
            }

            public void addHintsToSpot(SpotId spot, List<Hint> hints, bool atFront = false)
            {
                if (spot == SpotId.Invalid || ListUtils.isEmpty(hints))
                    return;

                if (!spotToHints.ContainsKey(spot))
                    spotToHints[spot] = new();

                List<Hint> hintsForSpot = spotToHints[spot];
                foreach (Hint hint in hints)
                {
                    if (hint != null)
                    {
                        if (atFront)
                            hintsForSpot.Insert(0, hint);
                        else
                            hintsForSpot.Add(hint);
                    }
                }
            }

            public bool spotHasHints(SpotId spot)
            {
                return spotToHints.ContainsKey(spot) && spotToHints[spot].Count > 0;
            }

            public Dictionary<SpotId, List<Hint>> GetFinalResult()
            {
                return spotToHints;
            }
        }

        private void PrepareJovaniHints(SpotToHints spotToHints)
        {
            List<(int, string)> valToCheck =
                new() { (20, "Jovani 20 Poe Soul Reward"), (60, "Jovani 60 Poe Soul Reward") };

            Jovani jovani = hintSettings.jovani;

            int startingSouls = 0;
            foreach (Item item in genData.sSettings.startingItems)
            {
                if (item == Item.Poe_Soul)
                    startingSouls += 1;
            }

            List<JovaniRewardsHint.JovaniCheckInfo> checkInfoList = new();

            foreach ((int, string) pair in valToCheck)
            {
                int soulsForCheck = pair.Item1;
                string checkName = pair.Item2;

                // Skip over excluded ones entirely
                if (HintUtils.checkIsPlayerKnownStatus(checkName))
                    continue;

                bool failedMinSouls =
                    jovani.minSoulsForHint != null && jovani.minSoulsForHint > soulsForCheck;

                bool failedMinFoundSouls =
                    jovani.minFoundSoulsForHint != null
                    && soulsForCheck - startingSouls < jovani.minFoundSoulsForHint;

                bool unhinted = failedMinSouls || failedMinFoundSouls;

                CheckStatus checkStatus = genData.CalcCheckStatus(checkName);
                // Use this CheckStatusDisplay for everything for now.
                CheckStatusDisplay checkStatusDisplay = CheckStatusDisplay.Required_Info;

                JovaniRewardsHint.JovaniCheckInfo checkInfo =
                    new(
                        genData,
                        checkName,
                        (byte)soulsForCheck,
                        unhinted,
                        checkStatus,
                        checkStatusDisplay
                    );
                checkInfoList.Add(checkInfo);

                // Mark check as hinted
                genData.hinted.alreadyCheckContentsHinted.Add(checkName);
            }

            if (!ListUtils.isEmpty(checkInfoList))
            {
                JovaniRewardsHint hint = new JovaniRewardsHint(checkInfoList);
                spotToHints.addHintToSpot(SpotId.Jovani_House_Sign, hint);
            }
        }

        private void PrepareCaveOfOrdealsHints(SpotToHints spotToHints)
        {
            if (!hintSettings.caveOfOrdeals)
                return;

            List<string> checkNames =
                new()
                {
                    "Cave of Ordeals Floor 17 Poe",
                    "Cave of Ordeals Floor 33 Poe",
                    "Cave of Ordeals Floor 44 Poe",
                    "Cave of Ordeals Great Fairy Reward",
                };

            foreach (string checkName in checkNames)
            {
                if (HintUtils.checkIsPlayerKnownStatus(checkName))
                    continue;

                // Can add user configuration later. For now, indicate
                // requiredOrNot for all of thse checks.
                LocationHint hint = LocationHint.Create(
                    genData,
                    checkName,
                    display: CheckStatusDisplay.Required_Info
                );

                genData.hinted.alreadyCheckContentsHinted.Add(checkName);
                spotToHints.addHintToSpot(SpotId.Cave_of_Ordeals_Sign, hint);
            }
        }

        private void CreateBeyondPointHints(SpotToHints spotToHints, bool placeHintsOnSpots)
        {
            if (ListUtils.isEmpty(hintSettings.beyondPointZones))
                return;

            foreach (Zone zone in hintSettings.beyondPointZones)
            {
                BeyondPointObj beyondPointObj = ZoneUtils.idToBeyondPointData[zone];

                if (!beyondPointObj.CanBeHinted(genData))
                    continue;

                SpotId spotId = beyondPointObj.spotId;
                HashSet<string> checkNames = AreaId
                    .Category(beyondPointObj.category)
                    .ResolveToChecks();

                AreaId areaId = AreaId.Zone(zone);
                List<string> checksToHint = new();
                bool hasImportantCheck = false;
                foreach (string checkName in checkNames)
                {
                    if (
                        !HintUtils.checkIsPlayerKnownStatus(checkName)
                        && !genData.hinted.hintsShouldIgnoreChecks.Contains(checkName)
                    )
                    {
                        checksToHint.Add(checkName);

                        Item contents = HintUtils.getCheckContents(checkName);
                        bool itemAllowsBarrenForArea = genData.ItemAllowsBarrenForArea(
                            contents,
                            areaId
                        );

                        if (!itemAllowsBarrenForArea && genData.CheckIsGood(checkName))
                        {
                            hasImportantCheck = true;
                            break;
                        }
                    }
                }

                if (checksToHint.Count > 0)
                {
                    // If big keys are OwnDungeon, then we need to calculate if
                    // the big key is included. This only happens for dungeons,
                    // and the item we are looking for.

                    bool includeBigKeyInfo = false;
                    bool hasBigKeys = false;
                    if (genData.sSettings.bigKeySettings == BigKeySettings.Own_Dungeon)
                    {
                        includeBigKeyInfo = CheckForBeyondPointBigKeys(
                            zone,
                            checkNames,
                            out hasBigKeys
                        );
                    }

                    if (hasImportantCheck)
                    {
                        if (placeHintsOnSpots)
                        {
                            BeyondPointHint.BeyondPointType beyondPointType = BeyondPointHint
                                .BeyondPointType
                                .Good;
                            if (includeBigKeyInfo)
                            {
                                if (hasBigKeys)
                                    beyondPointType = BeyondPointHint
                                        .BeyondPointType
                                        .Good_And_Big_Keys;
                                else
                                    beyondPointType = BeyondPointHint
                                        .BeyondPointType
                                        .Good_No_Big_Keys;
                            }

                            spotToHints.addHintToSpot(
                                spotId,
                                new BeyondPointHint(beyondPointType),
                                true
                            );
                        }
                    }
                    else
                    {
                        if (placeHintsOnSpots)
                        {
                            BeyondPointHint.BeyondPointType beyondPointType = BeyondPointHint
                                .BeyondPointType
                                .Nothing;
                            if (includeBigKeyInfo && hasBigKeys)
                                beyondPointType = BeyondPointHint.BeyondPointType.Only_Big_Keys;

                            spotToHints.addHintToSpot(
                                spotId,
                                new BeyondPointHint(beyondPointType),
                                true
                            );
                        }

                        foreach (string checkName in checksToHint)
                        {
                            genData.hinted.AddNonWeightedBarrenCheck(checkName);
                        }
                    }
                }
            }
        }

        private bool CheckForBeyondPointBigKeys(
            Zone zone,
            HashSet<string> categoryCheckNames,
            out bool hasBigKeys
        )
        {
            if (ListUtils.isEmpty(categoryCheckNames))
                throw new Exception("Expected 'categoryCheckNames' to not be empty.");

            // set 'out' to false
            hasBigKeys = false;

            Dictionary<Zone, Item> zoneToBigKey =
                new()
                {
                    { Zone.Goron_Mines, Item.Goron_Mines_Key_Shard },
                    { Zone.Lakebed_Temple, Item.Lakebed_Temple_Big_Key },
                    { Zone.Arbiters_Grounds, Item.Arbiters_Grounds_Big_Key },
                    { Zone.Temple_of_Time, Item.Temple_of_Time_Big_Key },
                    { Zone.City_in_the_Sky, Item.City_in_The_Sky_Big_Key },
                };

            if (!zoneToBigKey.TryGetValue(zone, out Item bigKeyItem))
            {
                return false;
            }

            foreach (string checkName in categoryCheckNames)
            {
                if (
                    !HintUtils.checkIsPlayerKnownStatus(checkName)
                    && !genData.hinted.hintsShouldIgnoreChecks.Contains(checkName)
                )
                {
                    Item contents = HintUtils.getCheckContents(checkName);
                    if (contents == bigKeyItem)
                    {
                        hasBigKeys = true;
                        break;
                    }
                }
            }

            return true;
        }

        private void CreateBigKeyHints(SpotToHints spotToHints)
        {
            if (
                !hintSettings.dungeons.bigKeyHints
                || (
                    genData.sSettings.bigKeySettings != BigKeySettings.Any_Dungeon
                    && genData.sSettings.bigKeySettings != BigKeySettings.Anywhere
                )
            )
                return;

            Dictionary<Item, int> startingBigKeys = new();
            foreach (Item item in genData.sSettings.startingItems)
            {
                if (HintConstants.bigKeyToDungeonZone.ContainsKey(item))
                {
                    startingBigKeys.TryGetValue(item, out int count);
                    startingBigKeys[item] = count + 1;
                }
            }

            foreach (KeyValuePair<Item, string> pair in HintConstants.bigKeyToDungeonZone)
            {
                Item bigKeyItem = pair.Key;
                string zoneName = pair.Value;
                Zone zone = ZoneUtils.StringToId(zoneName);

                if (
                    zone != Zone.Hyrule_Castle
                    && genData.sSettings.barrenDungeons
                    && !HintUtils.DungeonIsRequired(zoneName)
                )
                    continue;

                int totalNeeded = ZoneUtils.StringToId(zoneName) == Zone.Goron_Mines ? 3 : 1;

                startingBigKeys.TryGetValue(bigKeyItem, out int startingCount);
                totalNeeded -= startingCount;

                if (totalNeeded < 1)
                    continue;

                List<Hint> selectedHints = new();

                // Find all possible hints and pick from them randomly up to
                // total needed.
                if (genData.itemToChecksList.TryGetValue(bigKeyItem, out List<string> checkNames))
                {
                    if (ListUtils.isEmpty(checkNames))
                        continue;

                    List<Hint> potentialHints = new();

                    foreach (string checkName in checkNames)
                    {
                        // If big key is on tradeReward (such as Agitha or
                        // Ralis), use a tradeChain hint instead or providing a
                        // not super helpful ItemHint.
                        if (HintUtils.CheckIsTradeItemReward(checkName))
                        {
                            foreach (
                                KeyValuePair<
                                    string,
                                    Item
                                > chainStartAndReward in genData.tradeChainStartToReward
                            )
                            {
                                if (chainStartAndReward.Value == bigKeyItem)
                                {
                                    string srcCheckName = chainStartAndReward.Key;
                                    AreaId areaId = GetAreaIdForBigKeyHint(
                                        zone,
                                        srcCheckName,
                                        totalNeeded
                                    );
                                    TradeChainHint hint = TradeChainHint.Create(
                                        genData,
                                        srcCheckName,
                                        false,
                                        true,
                                        areaId.type == AreaId.AreaType.Province
                                          ? TradeChainHint.AreaType.Province
                                          : TradeChainHint.AreaType.Zone,
                                        CheckStatus.Good,
                                        CheckStatusDisplay.None
                                    );
                                    potentialHints.Add(hint);

                                    // Update hinted
                                    genData.hinted.alreadyCheckDirectedToward.Add(srcCheckName);
                                    genData.hinted.alreadyCheckDirectedToward.Add(checkName);
                                }
                            }
                        }
                        else
                        {
                            AreaId areaId = GetAreaIdForBigKeyHint(zone, checkName, totalNeeded);
                            ItemHint hint = ItemHint.Create(genData, areaId, checkName);
                            potentialHints.Add(hint);

                            // Update hinted
                            genData.hinted.alreadyCheckDirectedToward.Add(checkName);
                        }
                    }

                    for (int i = 0; i < totalNeeded && potentialHints.Count > 0; i++)
                    {
                        Hint hint = HintUtils.RemoveRandomListItem(genData.rnd, potentialHints);
                        selectedHints.Add(hint);
                    }
                }

                SpotId spotId = ZoneUtils.IdToSpotId(zone);
                spotToHints.addHintsToSpot(spotId, selectedHints);
            }
        }

        private AreaId GetAreaIdForBigKeyHint(
            Zone dungeonZoneInQuestion,
            string checkForBigKey,
            int numToFind
        )
        {
            // TODO: testing with it always showing the zone
            string checkZoneName = HintUtils.checkNameToHintZone(checkForBigKey);
            return AreaId.ZoneStr(checkZoneName);

            // string checkZoneName = HintUtils.checkNameToHintZone(checkForBigKey);
            // Province checkProvince = HintUtils.checkNameToHintProvince(checkForBigKey);

            // return
            //     dungeonZoneInQuestion == Zone.Hyrule_Castle
            //     || numToFind > 1
            //     || checkProvince == Province.Dungeon
            //   ? AreaId.ZoneStr(checkZoneName)
            //   : AreaId.Province(checkProvince);
        }

        private List<List<Hint>> PrepareAlwaysHints(SpotToHints spotToHints)
        {
            Always always = hintSettings.always;
            if (always == null || ListUtils.isEmpty(always.checks))
                return null;

            List<string> checksToHint = CalcAlwaysChecksToHint();

            UpdateHintedForAlwaysHints(checksToHint);

            if (ListUtils.isEmpty(checksToHint))
                return null;

            List<Hint> locationHints = new();
            foreach (string checkName in checksToHint)
            {
                locationHints.Add(
                    LocationHint.Create(genData, checkName, display: always.checkStatusDisplay)
                );
            }

            if (always.starting)
            {
                // If starting, assign to the starting spot. Monopolize behavior
                // does nothing when we put the hints on the starting spot.
                spotToHints.addHintsToSpot(hintSettings.starting.spot, locationHints);
                return null;
            }

            HintGroup group = mutableGroups[always.groupId];
            if (group == null)
                throw new Exception($"Failed to resolve groupId '{always.groupId}'.");

            if (group.spots.Count < 1)
                throw new Exception(
                    $"Group '{always.groupId}' has no spots to place Always hints in."
                );

            int totalHints = checksToHint.Count * always.copies;

            int numSpotsToFill;
            if (always.idealNumSpots != null)
            {
                numSpotsToFill = (int)always.idealNumSpots;
            }
            else
            {
                // Try to use as few spots as reasonably possible (max 6).
                numSpotsToFill = 6;
                for (int i = 5; i >= 1; i--)
                {
                    double copiesPerSpot = (double)totalHints / i;
                    if (copiesPerSpot <= 2)
                        numSpotsToFill = i;
                    else
                        break;
                }
            }

            // Make sure we do not hint more spots than we have total hints.
            // This could happen if the user specifies a large idealNumSpots to
            // get one hint per spot or if totalHints is very small.
            if (numSpotsToFill > totalHints)
                numSpotsToFill = totalHints;

            // Cannot fill more spots than are in the group
            if (numSpotsToFill > group.spots.Count)
                numSpotsToFill = group.spots.Count;

            List<List<Hint>> hintsForSpots = new();

            // More copies than spots, so just give each spot the full list.
            if (numSpotsToFill <= always.copies)
            {
                // Each spot gets the full list.
                for (int i = 0; i < numSpotsToFill; i++)
                {
                    hintsForSpots.Add(locationHints);
                }
            }
            else
            {
                // Split hints across spots.
                int baseHintsPerSpot = totalHints / numSpotsToFill;
                int maxRemainderIndex = (totalHints % numSpotsToFill) - 1;

                // Used to avoid things like AB CD AB CD and instead get AB CD
                // BC DA. This could get really complicated if solving a general
                // case, but there is really no need.
                bool useOffsetOnLetterLoop = checksToHint.Count % baseHintsPerSpot == 0;

                int properAbsoluteHintIndex = 0;
                int effectiveHintIndex = 0;

                for (int i = 0; i < numSpotsToFill; i++)
                {
                    int numHintsForSpot = baseHintsPerSpot;
                    if (i <= maxRemainderIndex)
                        numHintsForSpot += 1;

                    List<Hint> hintsForSpot = new();
                    for (int j = 0; j < numHintsForSpot; j++)
                    {
                        int ii = effectiveHintIndex % locationHints.Count;
                        hintsForSpot.Add(locationHints[ii]);

                        properAbsoluteHintIndex += 1;

                        if (
                            useOffsetOnLetterLoop
                            && properAbsoluteHintIndex % locationHints.Count == 0
                        )
                            effectiveHintIndex += 2;
                        else
                            effectiveHintIndex += 1;
                    }
                    hintsForSpots.Add(hintsForSpot);
                }
            }

            // This list should be stored somewhere to be assigned later. We
            // should also apply a penalty to the group that the Always hints
            // belong to. In the case that it does not monopolize, the penalty
            // will be removed after the first layer using this group finishes.

            return hintsForSpots;

            // List<HintSpotLocation> spots = new(group.spots);
            // HintUtils.ShuffleListInPlace(genData.rnd, spots);

            // // Fill from the start
            // for (int i = 0; i < hintsForSpots.Count; i++)
            // {
            //     HintSpotLocation spotId = spots[i];
            //     spotToHints.addHintsToSpot(spotId, hintsForSpots[i]);
            //     removeSpotFromMutableGroups(spotId);
            // }

            // int adsfdasf = 7;
        }

        private List<string> CalcAlwaysChecksToHint()
        {
            // Note: we intentionally do not filter out Always checks which
            // happened to have already been hinted such as potentially Jovani
            // or CoO, since a user would have to deliberately make a custom
            // distribution to hint these checks twice, and we do not want to
            // confuse the user by not Always-hinting a check they specifically
            // asked to be Always hinted.

            Always always = hintSettings.always;
            List<string> checksToHint;

            if (always.idealNumExplicitlyHinted != null)
            {
                // Adjust based on idealNumHinted
                List<string> requiredAlways = new();
                List<string> goodAlways = new();
                List<string> badAlways = new();
                foreach (string checkName in always.checks)
                {
                    if (genData.requiredChecks.Contains(checkName))
                        requiredAlways.Add(checkName);
                    else
                    {
                        Item item = HintUtils.getCheckContents(checkName);
                        if (genData.preventBarrenItems.Contains(item))
                            goodAlways.Add(checkName);
                        else
                            badAlways.Add(checkName);
                    }
                }
                HintUtils.ShuffleListInPlace(genData.rnd, requiredAlways);
                HintUtils.ShuffleListInPlace(genData.rnd, goodAlways);
                HintUtils.ShuffleListInPlace(genData.rnd, badAlways);
                checksToHint = new List<string>()
                    .Concat(requiredAlways)
                    .Concat(goodAlways)
                    .Concat(badAlways)
                    .ToList();

                // Always hint any Required or Good checks. If we were to only
                // hint Required, then you could potentially not hint 2 of the 4
                // swords if they were on Always checks and there were enough
                // Required Always checks to where they did not get hinted. Bugs
                // would make this even more complicated, so best to hint all
                // checks which are not Bad no matter what. We also need to
                // include Good for no-logic since "Required" as a concept does
                // not exist.
                int numGoodOrRequired = requiredAlways.Count + goodAlways.Count;

                int idealNumExplicitlyHinted = (int)always.idealNumExplicitlyHinted;
                if (idealNumExplicitlyHinted == 0 && numGoodOrRequired < 1)
                    return null;

                int newLen = numGoodOrRequired;
                if (idealNumExplicitlyHinted > newLen)
                    newLen = idealNumExplicitlyHinted;

                // newLen will always be at least 1 here
                if (newLen < checksToHint.Count)
                    checksToHint = checksToHint.GetRange(0, newLen);
            }
            else
            {
                checksToHint = new(always.checks);
            }

            // Shuffle so all of the good checks aren't bunched up (don't all
            // get placed on the same spot).
            HintUtils.ShuffleListInPlace(genData.rnd, checksToHint);

            return checksToHint;
        }

        private void UpdateHintedForAlwaysHints(List<string> checksToHint)
        {
            HashSet<string> checksToHintSet = ListUtils.isEmpty(checksToHint)
              ? new()
              : new(checksToHint);

            foreach (string checkName in hintSettings.always.checks)
            {
                if (checksToHintSet.Contains(checkName))
                {
                    genData.hinted.alreadyCheckContentsHinted.Add(checkName);
                    genData.hinted.alwaysHintedChecks.Add(checkName);
                }
                else
                    genData.hinted.hintsShouldIgnoreChecks.Add(checkName);
            }
        }

        private void PrepareUserSpecialHints(SpotToHints specialSpotToHints)
        {
            for (int idx = 0; idx < hintSettings.specialHintDefs.Count; idx++)
            {
                UserSpecialHintDef specialHintDef = hintSettings.specialHintDefs[idx];

                // Not sure that the string is actually used anywhere. Value
                // seems unimportant in any case.
                HintGroup group = HintGroup.fromSpotId(
                    "__specialHintDefs__",
                    specialHintDef.spotId
                );

                // Currently only meant to generate a single hint on a single
                // spot with this method.
                int layerSize = 1;

                HintLayerData layerData = new HintLayerData(
                    genData,
                    hintSettings,
                    group,
                    layerSize
                );

                // Generate hints according to a tree defined in JSON.
                RecHintResults recHintResults = recursiveHandleHintDef(
                    layerData,
                    specialHintDef.hintDef,
                    null,
                    $"specialHintDef.{idx}"
                );

                if (recHintResults.didProduceHints)
                {
                    foreach (HintDefResult result in recHintResults.HintDefResults)
                    {
                        specialSpotToHints.addHintToSpot(specialHintDef.spotId, result.hint);
                    }
                }
            }
        }

        private void removeSpotFromMutableGroups(SpotId spotId)
        {
            foreach (KeyValuePair<string, HintGroup> pair in mutableGroups)
            {
                pair.Value.spots.Remove(spotId);
            }
        }

        private List<HintSpot> CreateHintSpotList(
            SpotToHints specialSpotToHints,
            SpotToHints normalSpotToHints
        )
        {
            // TODO: remove duplicates for spots and improve code. Removing
            // duplicates already done?

            Dictionary<SpotId, List<Hint>> resultDict = new();

            foreach (KeyValuePair<SpotId, List<Hint>> pair in specialSpotToHints.GetFinalResult())
            {
                if (!resultDict.ContainsKey(pair.Key))
                    resultDict[pair.Key] = new();
                resultDict[pair.Key].AddRange(pair.Value);
            }

            foreach (KeyValuePair<SpotId, List<Hint>> pair in normalSpotToHints.GetFinalResult())
            {
                if (!resultDict.ContainsKey(pair.Key))
                    resultDict[pair.Key] = new();
                resultDict[pair.Key].AddRange(pair.Value);
            }

            List<HintSpot> hintSpots = new();
            foreach (KeyValuePair<SpotId, List<Hint>> pair in resultDict)
            {
                HintSpot spot = new HintSpot(pair.Key);
                spot.hints.AddRange(pair.Value);
                hintSpots.Add(spot);
            }

            HintEncodingBitLengths bitLengths = HintUtils.GetHintEncodingBitLengths(hintSpots);

            // Iterate over each hint spot and remove duplicates. Two hints must
            // encode to the same string if and only if they represent the exact
            // same hint.
            foreach (HintSpot spot in hintSpots)
            {
                HashSet<string> encodedHints = new();

                List<Hint> newList = new();
                foreach (Hint hint in spot.hints)
                {
                    string encoded = hint.encodeAsBits(bitLengths);
                    if (!encodedHints.Contains(encoded))
                    {
                        encodedHints.Add(encoded);
                        newList.Add(hint);
                    }
                }

                // Try swap single barrenZone hint which points to itself for a
                // junk hint if enabled in settings.
                if (hintSettings.barren.ownZoneShowsAsJunkHint && newList.Count == 1)
                {
                    SpotId spotId = HintUtils.TryGetSpotIdForBarrenZoneHint(newList[0]);
                    if (spot.location == spotId && spotId != SpotId.Invalid)
                    {
                        Hint barrenAsJunkHint = new JunkHint(genData.rnd, true);
                        newList.Clear();
                        newList.Add(barrenAsJunkHint);
                    }
                }

                spot.hints.Clear();
                spot.hints.AddRange(newList);
            }

            FillUnfilledCustomSigns(hintSpots);

            return hintSpots;
        }

        private void FillUnfilledCustomSigns(List<HintSpot> hintSpots)
        {
            HashSet<SpotId> possibleSpotsToFill =
                new()
                {
                    SpotId.Ordon_Sign,
                    SpotId.Sacred_Grove_Sign,
                    SpotId.Faron_Field_Sign,
                    SpotId.Faron_Woods_Sign,
                    SpotId.Kakariko_Gorge_Sign,
                    SpotId.Kakariko_Village_Sign,
                    SpotId.Kakariko_Graveyard_Sign,
                    SpotId.Eldin_Field_Sign,
                    SpotId.North_Eldin_Sign,
                    SpotId.Death_Mountain_Sign,
                    SpotId.Hidden_Village_Sign,
                    SpotId.Lanayru_Field_Sign,
                    SpotId.Beside_Castle_Town_Sign,
                    SpotId.South_of_Castle_Town_Sign,
                    SpotId.Castle_Town_Sign,
                    SpotId.Great_Bridge_of_Hylia_Sign,
                    SpotId.Lake_Hylia_Sign,
                    SpotId.Lake_Lantern_Cave_Sign,
                    SpotId.Lanayru_Spring_Sign,
                    SpotId.Zoras_Domain_Sign,
                    SpotId.Upper_Zoras_River_Sign,
                    SpotId.Gerudo_Desert_Sign,
                    SpotId.Bulblin_Camp_Sign,
                    SpotId.Snowpeak_Sign,
                    SpotId.Cave_of_Ordeals_Sign,
                    SpotId.Forest_Temple_Sign,
                    SpotId.Goron_Mines_Sign,
                    SpotId.Lakebed_Temple_Sign,
                    SpotId.Arbiters_Grounds_Sign,
                    SpotId.Snowpeak_Ruins_Sign,
                    SpotId.Temple_of_Time_Sign,
                    SpotId.Temple_of_Time_Beyond_Point_Sign,
                    SpotId.City_in_the_Sky_Sign,
                    SpotId.Palace_of_Twilight_Sign,
                    SpotId.Hyrule_Castle_Sign,
                };

            // Do not bother adding hint to unrequired barren dungeon signs.
            if (genData.sSettings.barrenDungeons)
            {
                if (!HintUtils.DungeonIsRequired("Forest Temple"))
                    possibleSpotsToFill.Remove(SpotId.Forest_Temple_Sign);
                if (!HintUtils.DungeonIsRequired("Goron Mines"))
                    possibleSpotsToFill.Remove(SpotId.Goron_Mines_Sign);
                if (!HintUtils.DungeonIsRequired("Lakebed Temple"))
                    possibleSpotsToFill.Remove(SpotId.Lakebed_Temple_Sign);
                if (!HintUtils.DungeonIsRequired("Arbiter's Grounds"))
                    possibleSpotsToFill.Remove(SpotId.Arbiters_Grounds_Sign);
                if (!HintUtils.DungeonIsRequired("Snowpeak Ruins"))
                    possibleSpotsToFill.Remove(SpotId.Snowpeak_Ruins_Sign);
                if (!HintUtils.DungeonIsRequired("Temple of Time"))
                {
                    possibleSpotsToFill.Remove(SpotId.Temple_of_Time_Sign);
                    possibleSpotsToFill.Remove(SpotId.Temple_of_Time_Beyond_Point_Sign);
                }
                if (!HintUtils.DungeonIsRequired("City in the Sky"))
                    possibleSpotsToFill.Remove(SpotId.City_in_the_Sky_Sign);
                if (!HintUtils.DungeonIsRequired("Palace of Twilight"))
                    possibleSpotsToFill.Remove(SpotId.Palace_of_Twilight_Sign);
            }

            Dictionary<SpotId, HintSpot> spotIdToHintSpot = new();
            foreach (HintSpot hintSpot in hintSpots)
            {
                spotIdToHintSpot[hintSpot.location] = hintSpot;
            }

            List<SpotId> spotsToFill = new();
            foreach (SpotId spotId in possibleSpotsToFill)
            {
                if (
                    !spotIdToHintSpot.TryGetValue(spotId, out HintSpot hintSpot)
                    || hintSpot.hints.Count == 0
                )
                    spotsToFill.Add(spotId);
            }

            AddFillerHintsToSpots(
                spotsToFill,
                (spotId, fillerHint) =>
                {
                    if (!spotIdToHintSpot.TryGetValue(spotId, out HintSpot hintSpot))
                    {
                        hintSpot = new HintSpot(spotId);
                        hintSpots.Add(hintSpot);
                        spotIdToHintSpot[spotId] = hintSpot;
                    }
                    hintSpot.hints.Add(fillerHint);
                }
            );
        }

        private void AddFillerHintsToSpots(List<SpotId> spotsToFill, Action<SpotId, Hint> action)
        {
            if (ListUtils.isEmpty(spotsToFill))
                return;

            // NOTE: this mutates the spotsToFill list.

            List<Hint> fillerHints;

            if (hintSettings.barren.ownZoneShowsAsJunkHint)
            {
                JObject obj = JObject.Parse(
                    "{'hintType':'item',options:{validItems:['alias:junk','alias:rupees']}}"
                );
                ItemHintCreator fillerHintCreator = ItemHintCreator.fromJObject(obj);
                List<Hint> itemHints = fillerHintCreator.tryCreateHint(
                    genData,
                    hintSettings,
                    spotsToFill.Count,
                    cache
                );

                if (!ListUtils.isEmpty(itemHints))
                    fillerHints = itemHints;
                else
                {
                    // As an absolute last resort, create a hardcoded hint.
                    // We don't care if it was excluded as this is a final
                    // resort and there is no reason to do a huge number of
                    // if-statements. We will likely never make it to this
                    // point anyway (except for really crazy plandos).
                    string checkName = "Wooden Sword Chest";
                    Hint fillerHint = LocationHint.Create(genData, checkName);
                    fillerHints = new() { fillerHint };
                }
            }
            else
            {
                JObject obj = JObject.Parse("{}");
                JunkHintCreator junkHintCreator = JunkHintCreator.fromJObject(obj);
                fillerHints = junkHintCreator.tryCreateHint(
                    genData,
                    hintSettings,
                    spotsToFill.Count,
                    cache
                );
            }

            if (ListUtils.isEmpty(fillerHints))
                throw new Exception("Did not expect fillerHints to be empty.");

            int itemHintIndex = 0;
            for (int i = spotsToFill.Count - 1; i >= 0; i--)
            {
                Hint fillerHint = fillerHints[itemHintIndex];
                SpotId spotId = spotsToFill[i];

                // The caller handles adding the hint to the HintSpot.
                action.Invoke(spotId, fillerHint);

                spotsToFill.RemoveAt(i);
                itemHintIndex = (itemHintIndex + 1) % fillerHints.Count;
            }
        }

        private abstract class HintDefArrayProcessor
        {
            protected HintDefProps currDefProps;
            protected HintDef hintDef;
            protected string nodeId;
            protected HashSet<string> deadNodeIds;

            protected HintDefArrayProcessor(
                HintDefProps currDefProps,
                HintDef hintDef,
                string nodeId,
                HashSet<string> deadNodeIds
            )
            {
                this.currDefProps = currDefProps;
                this.hintDef = hintDef;
                this.nodeId = nodeId;
                this.deadNodeIds = deadNodeIds;
            }

            public static HintDefArrayProcessor Create(
                HintGenData genData,
                HintDefProps currDefProps,
                HintDef hintDef,
                string nodeId,
                HashSet<string> deadNodeIds
            )
            {
                switch (hintDef.selectionType)
                {
                    case HintDef.SelectionType.Basic:
                        return new BasicHdaProcessor(currDefProps, hintDef, nodeId, deadNodeIds);
                    case HintDef.SelectionType.RandomOrder:
                        return new RandomOrderHdaProcessor(
                            currDefProps,
                            hintDef,
                            nodeId,
                            deadNodeIds,
                            genData.rnd
                        );
                    case HintDef.SelectionType.RandomWeighted:
                        return new RandomWeightedHdaProcessor(
                            currDefProps,
                            hintDef,
                            nodeId,
                            deadNodeIds,
                            genData.rnd
                        );
                    default:
                        throw new Exception($"Unexpected selectionType '{hintDef.selectionType}'.");
                }
            }

            public abstract int NextIndex();
        }

        private class BasicHdaProcessor : HintDefArrayProcessor
        {
            private int currIteration = 0;
            private int idxInIteration = 0;
            private List<int> currIdxList;
            private bool done = false;

            public BasicHdaProcessor(
                HintDefProps currDefProps,
                HintDef hintDef,
                string nodeId,
                HashSet<string> deadNodeIds
            ) : base(currDefProps, hintDef, nodeId, deadNodeIds)
            {
                // At the end of an iteration, rebuild the list.
                BuildIdxList();
            }

            private void BuildIdxList()
            {
                currIdxList = new();
                for (int i = 0; i < hintDef.hintDefs.Count; i++)
                {
                    string idxNodeId = $"{nodeId}.{i}";
                    if (!deadNodeIds.Contains(idxNodeId))
                        currIdxList.Add(i);
                }

                if (currIdxList.Count < 1)
                {
                    deadNodeIds.Add(nodeId);
                    done = true;
                }
            }

            public override int NextIndex()
            {
                if (done)
                    return -1;

                while (idxInIteration < currIdxList.Count)
                {
                    int idx = currIdxList[idxInIteration];

                    string childNodeId = $"{nodeId}.{idx}";
                    idxInIteration += 1;

                    if (!deadNodeIds.Contains(childNodeId))
                        return idx;
                }

                currIteration += 1;
                idxInIteration = 0;

                if (currIteration < currDefProps.iterations)
                {
                    BuildIdxList();
                    return NextIndex();
                }
                else
                    done = true;

                return -1;
            }
        }

        private class RandomOrderHdaProcessor : HintDefArrayProcessor
        {
            private int currIteration = 0;
            private int idxInIteration = 0;
            private List<int> currIdxList;
            private bool done = false;
            private Random rnd;

            public RandomOrderHdaProcessor(
                HintDefProps currDefProps,
                HintDef hintDef,
                string nodeId,
                HashSet<string> deadNodeIds,
                Random rnd
            ) : base(currDefProps, hintDef, nodeId, deadNodeIds)
            {
                this.rnd = rnd;
                // At the end of an iteration, rebuild the list.
                BuildIdxList();
            }

            private void BuildIdxList()
            {
                List<KeyValuePair<double, int>> weightedList = new();
                HashSet<double> seenWeights = new();

                for (int i = 0; i < hintDef.hintDefs.Count; i++)
                {
                    string idxNodeId = $"{nodeId}.{i}";
                    if (!deadNodeIds.Contains(idxNodeId))
                    {
                        double weight = hintDef.hintDefs[i].weight;
                        seenWeights.Add(weight);
                        weightedList.Add(new(weight, i));
                    }
                }

                if (weightedList.Count < 1)
                {
                    deadNodeIds.Add(nodeId);
                    done = true;
                }
                else
                {
                    if (seenWeights.Count == 1)
                    {
                        // If all weights are the same, just randomize the order
                        // and use that.
                        HintUtils.ShuffleListInPlace(rnd, weightedList);

                        currIdxList = new();
                        foreach (KeyValuePair<double, int> pair in weightedList)
                        {
                            currIdxList.Add(pair.Value);
                        }
                    }
                    else
                    {
                        // Use weights to order the list.
                        VoseInstance<int> inst = VoseAlgorithm.createInstance(weightedList);
                        currIdxList = new();
                        while (inst.HasMore())
                        {
                            int idx = inst.NextAndRemove(rnd);
                            currIdxList.Add(idx);
                        }
                    }
                }
            }

            public override int NextIndex()
            {
                if (done)
                    return -1;

                while (idxInIteration < currIdxList.Count)
                {
                    int idx = currIdxList[idxInIteration];

                    string childNodeId = $"{nodeId}.{idx}";
                    idxInIteration += 1;

                    if (!deadNodeIds.Contains(childNodeId))
                        return idx;
                }

                currIteration += 1;
                idxInIteration = 0;

                if (currIteration < currDefProps.iterations)
                {
                    BuildIdxList();
                    return NextIndex();
                }
                else
                    done = true;

                return -1;
            }
        }

        private class RandomWeightedHdaProcessor : HintDefArrayProcessor
        {
            private bool done = false;
            private Random rnd;
            private HashSet<string> cachedDeadNodes;
            private VoseInstance<int> voseInst;

            public RandomWeightedHdaProcessor(
                HintDefProps currDefProps,
                HintDef hintDef,
                string nodeId,
                HashSet<string> deadNodeIds,
                Random rnd
            ) : base(currDefProps, hintDef, nodeId, deadNodeIds)
            {
                this.rnd = rnd;
            }

            private void BuildVoseInst()
            {
                if (cachedDeadNodes != null && cachedDeadNodes.SetEquals(deadNodeIds))
                    return;
                else
                    cachedDeadNodes = new(deadNodeIds);

                List<KeyValuePair<double, int>> weightedList = new();

                for (int i = 0; i < hintDef.hintDefs.Count; i++)
                {
                    string idxNodeId = $"{nodeId}.{i}";
                    if (!deadNodeIds.Contains(idxNodeId))
                    {
                        double weight = hintDef.hintDefs[i].weight;
                        weightedList.Add(new(weight, i));
                    }
                }

                voseInst = VoseAlgorithm.createInstance(weightedList);
            }

            public override int NextIndex()
            {
                if (done)
                    return -1;

                // Construct voseInst of non-dead nodes.
                BuildVoseInst();

                if (voseInst.HasMore())
                {
                    return voseInst.NextAndKeep(rnd);
                }

                done = true;
                return -1;
            }
        }
    }

    public class HintLayerData
    {
        public bool killSwitched { get; private set; }
        public int layerLength { get; private set; }
        public int remainingSpots { get; private set; }
        public List<int> totalStarting { get; private set; } = new();
        public List<int> unusedStarting { get; private set; } = new();
        public List<int> barrenZonePenalties { get; private set; } = new();

        // Note: cannot combine both picks lists because we need to know the
        // diff to apply to the previous index when we pop back to it.
        private List<int> picksRemainingList = new();
        private List<int> picksDiffList = new();
        public List<Hint> startingHints { get; private set; } = new();
        private HintGenData genData;
        private HintSettings hintSettings;
        private HintGroup group;

        public HintLayerData(
            HintGenData genData,
            HintSettings hintSettings,
            HintGroup group,
            int layerLength
        )
        {
            this.genData = genData;
            this.hintSettings = hintSettings;
            this.group = group;
            this.layerLength = layerLength;
            this.remainingSpots = layerLength;
        }

        public void pushStarting(int starting)
        {
            totalStarting.Add(starting);
            unusedStarting.Add(starting);
            barrenZonePenalties.Add(0);
        }

        public void popStarting()
        {
            if (totalStarting.Count > 0)
                totalStarting.RemoveAt(totalStarting.Count - 1);
            if (unusedStarting.Count > 0)
                unusedStarting.RemoveAt(unusedStarting.Count - 1);
            if (barrenZonePenalties.Count > 0)
                barrenZonePenalties.RemoveAt(barrenZonePenalties.Count - 1);
        }

        public void popAndUpdateStarting(HintDefProps currDefProps, RecHintResults results)
        {
            // Pick the starting hints from result if possible and remove from list.
            if (results.HintDefResults.Count > 0)
            {
                List<HintDefResult> pickedForStarting = RemoveRandomStartingHints(
                    results,
                    currDefProps.starting
                );

                foreach (HintDefResult result in pickedForStarting)
                {
                    startingHints.Add(result.hint);
                    genData.vars.OnPickedStartingHint(result.hint);
                }

                int numPenaltiesShouldHavePaid = 0;

                // Scan through ones that were picked for starting. For each one
                // that should have paid a penalty, keep its penalty.
                foreach (HintDefResult hintDefResult in pickedForStarting)
                {
                    if (hintShouldPayBarrenPenalty(hintDefResult.hint))
                        numPenaltiesShouldHavePaid += 1;
                }

                int numPenaltiesDidPay = barrenZonePenalties[barrenZonePenalties.Count - 1];

                int numSpotsToRefund = numPenaltiesDidPay - numPenaltiesShouldHavePaid;
                if (numSpotsToRefund > 0)
                    remainingSpots += numSpotsToRefund;
            }

            popStarting();
        }

        private List<HintDefResult> RemoveRandomStartingHints(
            // List<HintDefResult> results,
            RecHintResults results,
            int numToPick
        )
        {
            List<HintDefResult> selected = new();

            if (ListUtils.isEmpty(results.HintDefResults) || numToPick < 1)
                return selected;

            List<int> priorityPicks = new();
            List<int> secondaryPicks = new();

            for (int i = 0; i < results.HintDefResults.Count; i++)
            {
                if (hintSettings.barren.ownZoneBehavior != Barren.OwnZoneBehavior.Off)
                {
                    HintDefResult result = results.HintDefResults[i];
                    SpotId spot = HintUtils.TryGetSpotIdForBarrenZoneHint(result.hint);
                    if (spot != SpotId.Invalid && spot == hintSettings.starting.spot)
                    {
                        priorityPicks.Add(i);
                        continue;
                    }
                }
                secondaryPicks.Add(i);
            }

            HintUtils.ShuffleListInPlace(genData.rnd, priorityPicks);
            HintUtils.ShuffleListInPlace(genData.rnd, secondaryPicks);

            List<int> combined = new List<int>()
                .Concat(priorityPicks)
                .Concat(secondaryPicks)
                .ToList();

            HashSet<int> selectedResultsIndexes = new();

            for (int i = 0; i < numToPick && i < combined.Count; i++)
            {
                int randomIndex = combined[i];
                selectedResultsIndexes.Add(randomIndex);
            }

            for (int i = results.HintDefResults.Count - 1; i >= 0; i--)
            {
                if (selectedResultsIndexes.Count < 1)
                    break;
                else if (selectedResultsIndexes.Contains(i))
                {
                    HintDefResult hintDefResult = results.HintDefResults[i];
                    results.RemoveHintDefResultAt(i);
                    selectedResultsIndexes.Remove(i);
                    selected.Add(hintDefResult);
                }
            }

            return selected;
        }

        public int getCummTotalStarting()
        {
            int total = 0;
            foreach (int val in totalStarting)
            {
                total += val;
            }
            return total;
        }

        public int getCummUnusedStarting()
        {
            int total = 0;
            foreach (int val in unusedStarting)
            {
                total += val;
            }
            return total;
        }

        private bool tryRemoveUnusedStarting()
        {
            for (int i = unusedStarting.Count - 1; i >= 0; i--)
            {
                int val = unusedStarting[i];
                if (val > 0)
                {
                    unusedStarting[i] = val - 1;
                    return true;
                }
            }
            return false;
        }

        public bool hasSpotInGroup(SpotId spotId)
        {
            return spotId != SpotId.Invalid && group.spots.Contains(spotId);
        }

        public void updateSpotsLeft(ReadOnlyCollection<HintDefResult> hintDefResults)
        {
            if (ListUtils.isEmpty(hintDefResults))
                return;

            // Note for maxPicks: we do not care about copies, starting, etc. We
            // just see "this many hints were generated", so we decrement by
            // that many.
            TryApplyPicksDiff(hintDefResults.Count);

            foreach (HintDefResult hintDefResult in hintDefResults)
            {
                updateSpotsLeftPair(hintDefResult);
            }

            if (remainingSpots < 0)
                throw new Exception("Reduced layer spots to a negative value.");
            else if (remainingSpots == 0)
                killSwitched = true;
        }

        private void updateSpotsLeftSingle(HintDefResult hintDefResult)
        {
            updateSpotsLeftPair(hintDefResult);

            if (remainingSpots < 0)
                throw new Exception("Reduced layer spots to a negative value.");
            else if (remainingSpots == 0)
                killSwitched = true;
        }

        private void updateSpotsLeftPair(HintDefResult hintDefResult)
        {
            if (tryRemoveUnusedStarting())
                return;

            // If can remove full copies, do that.
            int fullCopiesSize = hintDefResult.copies;
            if (remainingSpots >= fullCopiesSize)
            {
                remainingSpots -= fullCopiesSize;
                return;
            }

            int minCopies = hintDefResult.minCopies;
            if (minCopies > 0)
            {
                if (remainingSpots >= minCopies)
                {
                    remainingSpots = 0;
                    return;
                }
            }

            throw new Exception("Failed to reduce layer spots.");
        }

        public void updateUsingStartingBarrenHint(HintDefResult hintDefResult, SpotId spotId)
        {
            if (hintDefResult == null)
                throw new Exception("Expected non-null hintDefResult.");

            if (hasSpotInGroup(spotId))
                tryPayBarrenZonePenalty();

            TryApplyPicksDiff(1);

            updateSpotsLeftSingle(hintDefResult);
        }

        private void tryPayBarrenZonePenalty()
        {
            // Start at the latest one, iterate through until find one where
            // penalty value is less than its max. This reduces the number of
            // spots in the layer by 1 also.
            for (int i = barrenZonePenalties.Count - 1; i >= 0; i--)
            {
                int currentPaid = barrenZonePenalties[i];
                int maxForIndex = totalStarting[i];
                if (currentPaid >= maxForIndex)
                    continue;

                barrenZonePenalties[i] = currentPaid + 1;
                remainingSpots -= 1;
                break;
            }
        }

        private bool hintShouldPayBarrenPenalty(Hint hint)
        {
            SpotId spotId = HintUtils.TryGetSpotIdForBarrenZoneHint(hint);
            if (spotId == SpotId.Invalid || !group.spots.Contains(spotId))
                return false;

            return true;
        }

        public bool CheckPushMaxPicks(HintDefProps hintDefProps)
        {
            int newMaxPicks = hintDefProps.maxPicks;
            if (newMaxPicks < 1)
                return false;

            bool shouldPush = false;
            if (picksRemainingList.Count < 1)
                shouldPush = true;
            else
            {
                int currRemainingPicks = picksRemainingList[^1] + picksDiffList[^1];
                if (currRemainingPicks > newMaxPicks)
                    shouldPush = true;
            }

            if (shouldPush)
            {
                picksRemainingList.Add(newMaxPicks);
                picksDiffList.Add(0);
                return true;
            }

            // Ignore if new maxPicks is not defined or if it would be less
            // restrictive than what we are currently on.
            return false;
        }

        public void PopMaxPicks()
        {
            if (picksRemainingList.Count < 1 || picksDiffList.Count < 1)
                throw new Exception("Attempted to pop maxPicks, but a list was already empty.");

            int currDiff = picksDiffList[^1];
            picksRemainingList.RemoveAt(picksRemainingList.Count - 1);
            picksDiffList.RemoveAt(picksDiffList.Count - 1);

            if (picksDiffList.Count > 0)
            {
                picksDiffList[^1] += currDiff;
                if (GetCurrPicksLeft() < 0)
                    throw new Exception(
                        $"Did not expect currPicksLeft to have a negative value, but was '{picksDiffList[^1]}'."
                    );
            }
        }

        public int? GetCurrPicksLeft()
        {
            if (picksRemainingList.Count < 1)
                return null;
            return picksRemainingList[^1] + picksDiffList[^1];
        }

        public bool IsMaxPicksSwitched()
        {
            int? currLeft = GetCurrPicksLeft();
            if (currLeft == null)
                return false;
            return ((int)currLeft) < 1;
        }

        private void TryApplyPicksDiff(int numHintsCreated)
        {
            int? currRemaining = GetCurrPicksLeft();

            // If we do not have pending maxPicks, then ignore.
            if (currRemaining == null)
                return;

            int currRemainingInt = (int)currRemaining;

            if (currRemainingInt < numHintsCreated)
                throw new Exception(
                    $"Had {currRemainingInt} picks left, but created {numHintsCreated} hints."
                );

            picksDiffList[^1] -= numHintsCreated;
        }
    }

    public class HintDefProps : ICloneable
    {
        public int iterations { get; private set; } = 1;
        public int maxPicks { get; private set; } = -1;
        public int copies { get; private set; } = 1;
        public int minCopies { get; private set; } = 0;
        public int starting { get; private set; } = 0;
        public string saveToVar { get; private set; }

        private HintDefProps() { }

        public static HintDefProps mergeCreate(HintDefProps props, HintDef hintDef)
        {
            HintDefProps ret;
            if (props != null)
                ret = (HintDefProps)props.Clone();
            else
                ret = new HintDefProps();
            if (hintDef != null)
            {
                ret.iterations = hintDef.iterations;
                ret.maxPicks = hintDef.maxPicks;
                // Inherit copies unless specified on the hintDef.
                if (hintDef.copies > 0)
                    ret.copies = hintDef.copies;
                // Inherit minCopies unless specified on the hintDef.
                if (hintDef.minCopies > 0)
                    ret.minCopies = hintDef.minCopies;
                ret.starting = hintDef.starting;
                ret.saveToVar = hintDef.saveToVar;
            }
            return ret;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class HintDefResult
    {
        public Hint hint { get; private set; }
        public int copies { get; private set; } = 1;
        public int minCopies { get; private set; } = 0;
        private int numCopiesPlaced = 0;

        private HintDefResult() { }

        public static HintDefResult FromHintDefProps(Hint hint, HintDefProps hintDefProps)
        {
            if (hintDefProps == null)
                throw new Exception("Expected hintDefProps to not be null.");
            HintDefResult inst = new HintDefResult();
            inst.hint = hint;
            inst.copies = hintDefProps.copies;
            inst.minCopies = hintDefProps.minCopies;

            return inst;
        }

        public void OnPlacedCopy()
        {
            numCopiesPlaced += 1;
            if (numCopiesPlaced > copies)
                throw new Exception(
                    $"Placed a copy of a hint which had no more copies to place ('copies' was '{copies}')."
                );
        }

        public bool CanPlaceMoreCopies()
        {
            return copies > numCopiesPlaced;
        }

        public int GetRemainingCopies()
        {
            int remainingCopies = copies - numCopiesPlaced;
            if (remainingCopies < 0)
                throw new Exception(
                    $"Expected remainingCopies to be non-negative, but was '{remainingCopies}'."
                );
            return remainingCopies;
        }

        public bool PlacedEnoughCopies()
        {
            bool copiesMet = numCopiesPlaced == copies;
            bool minCopiesDefinedAndMet = minCopies > 0 && numCopiesPlaced >= minCopies;

            if (numCopiesPlaced > copies)
                throw new Exception($"'copies' was {copies}, but placed {numCopiesPlaced} copies.");

            return copiesMet || minCopiesDefinedAndMet;
        }
    }

    // Results of recursive hint generation
    public class RecHintResults
    {
        public bool didProduceHints { get; private set; } = false;
        private readonly List<HintDefResult> _hintDefResults = new();
        public ReadOnlyCollection<HintDefResult> HintDefResults { get; private set; }

        public RecHintResults()
        {
            HintDefResults = _hintDefResults.AsReadOnly();
        }

        public void AddHintDefResult(HintDefResult result)
        {
            if (result == null)
                return;

            didProduceHints = true;
            _hintDefResults.Add(result);
        }

        public void MergeChildRecHintResults(RecHintResults childResults)
        {
            // If child produced hints, then we produced hints (even if all
            // selected as starting hints).
            if (childResults.didProduceHints)
                didProduceHints = true;

            AddHintDefResultRange(childResults.HintDefResults);
        }

        private void AddHintDefResultRange(ICollection<HintDefResult> results)
        {
            if (ListUtils.isEmpty(results))
                return;

            foreach (HintDefResult result in results)
            {
                if (result != null)
                {
                    didProduceHints = true;
                    _hintDefResults.Add(result);
                }
            }
        }

        public void RemoveHintDefResultAt(int index)
        {
            _hintDefResults.RemoveAt(index);
        }
    }

    public class HintGenCache
    {
        // For now, we clear the latest node cache quite often. Right now its
        // main purpose is for more efficiently generating barrenZone hints
        // back-to-back (since we have to do them one at a time). Using this
        // specific cache in other areas would have questionable returns, so not
        // worrying about them for now. If we did want ot use this in other
        // areas, we would need to clear the cache when we save vars and create
        // hints.
        private Dictionary<string, object> latestNodeCache = new();
        private Stack<string> nodeIdStack = new();

        public void PushNodeId(string nodeId)
        {
            nodeIdStack.Push(nodeId);
            latestNodeCache.Clear();
        }

        public void PopNodeId()
        {
            nodeIdStack.Pop();
            latestNodeCache.Clear();
        }

        public T GetFromLatestNodeCache<T>() where T : class
        {
            if (
                nodeIdStack.Count > 0
                && latestNodeCache.TryGetValue(nodeIdStack.Peek(), out object value)
            )
                return value as T;
            return null;
        }

        public void PutLatestNodeCache(object obj)
        {
            latestNodeCache.Clear();
            if (nodeIdStack.Count < 1)
                throw new Exception(
                    "Attempted to put to the latestNodeCache when 'nodeIdStack' is empty."
                );
            latestNodeCache[nodeIdStack.Peek()] = obj;
        }
    }
}
