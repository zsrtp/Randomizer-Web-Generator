namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using SSettings.Enums;
    using TPRandomizer.Util;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Hints.HintCreator;

    public delegate bool BarrenPenalizer(AreaId areaId, HashSet<Zone> childZones);

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
            Room startingRoom,
            bool isRaceSeed
        )
        {
            this.genData = new HintGenData(
                rnd,
                sSettings,
                playthroughSpheres,
                startingRoom,
                isRaceSeed
            );
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

                if (hintSettings.barren.monopolizeSpots)
                {
                    // For any of these which point to a zone/spot belonging to the group, reduce
                    // the copies by 1, remove that spot from the mutable group for this layer, and
                    // place the hint at that spot for this layer. If the spot does not exist at the
                    // layer, it should get a special copy of the hint assigned to it.

                    // Handle starting hints.
                    handleMonopolizeBarrenHints(
                        spots,
                        specialSpotToHints,
                        normalSpotToHints,
                        layerData.startingHints,
                        true
                    );

                    // Handle normal hints.
                    HashSet<uint> placedNormalHintUids = handleMonopolizeBarrenHints(
                        spots,
                        specialSpotToHints,
                        normalSpotToHints,
                        recHintResults.HintDefResults.Select((def) => def.hint).ToList(),
                        false
                    );
                    if (placedNormalHintUids.Count > 0)
                    {
                        for (int i = recHintResults.HintDefResults.Count - 1; i >= 0; i--)
                        {
                            HintDefResult hintDefResult = recHintResults.HintDefResults[i];

                            if (placedNormalHintUids.Contains(hintDefResult.hint.uniqueHintId))
                            {
                                hintDefResult.OnPlacedCopy();
                                if (!hintDefResult.CanPlaceMoreCopies())
                                    recHintResults.RemoveHintDefResultAt(i);
                            }
                        }
                    }
                }

                // Always hints
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

        private HashSet<uint> handleMonopolizeBarrenHints(
            HashSet<SpotId> groupSpots,
            SpotToHints specialSpotToHints,
            SpotToHints normalSpotToHints,
            List<Hint> hints,
            bool areStartingHints
        )
        {
            HashSet<uint> placedNormalHintUids = new();

            foreach (Hint hintOuter in hints)
            {
                Dictionary<Zone, Hint> zoneToBaseHint = new();
                AreaId areaId = null;

                BarrenHint barrenHint = hintOuter as BarrenHint;
                if (barrenHint != null)
                    areaId = barrenHint.areaId;
                else
                {
                    // Not a Barren hint
                    continue;
                }

                if (areaId.type == AreaId.AreaType.Zone)
                {
                    Zone parentZone = ZoneUtils.StringToIdThrows(areaId.stringId);
                    zoneToBaseHint[parentZone] = hintOuter;
                }
                // Note: areaId for a barren hint can still cause us to create/place hints here. For
                // example, a Southern Desert hint causing us to create a CoO hint.
                HashSet<Zone> childZones = genData.GetZoneDeps(areaId);
                foreach (Zone childZone in childZones)
                {
                    zoneToBaseHint[childZone] = null;
                }

                // For any relevant parentZone + childZones, either place a copy of a self-hinting
                // BarrenHint there (creating a new hint if necessary).
                foreach (KeyValuePair<Zone, Hint> pair in zoneToBaseHint)
                {
                    Zone zone = pair.Key;
                    Hint hint = pair.Value;

                    SpotId spotId = ZoneUtils.IdToSpotId(zone);
                    if (spotId != SpotId.Invalid)
                    {
                        // Need to place a copy of the hint at this spot and remove the spot from
                        // this group for the layer.
                        if (
                            hintSettings.barren.monopolizeSpots
                            && normalSpotToHints.spotHasHints(spotId)
                        )
                        {
                            throw new Exception(
                                $"Expected spot '{spotId}' to have no normal hints with barren.ownZoneBehavior set to 'monopolize', but it was not empty."
                            );
                        }

                        bool wasInGroup = groupSpots.Remove(spotId);
                        if (wasInGroup && !areStartingHints && hint != null)
                        {
                            normalSpotToHints.addHintToSpot(spotId, hint);
                            placedNormalHintUids.Add(hint.uniqueHintId);
                        }
                        else
                        {
                            // If we are handling a starting hint and the zone that it hints barren
                            // matches the startingHintsZone, then skip placing a duplicate copy at
                            // this same spot. Otherwise we can place.
                            if (!(areStartingHints && hintSettings.starting.spot == spotId))
                            {
                                // For childZones, we need to create a new hint for its own zone.
                                if (hint == null)
                                    hint = new BarrenHint(AreaId.Zone(zone));

                                specialSpotToHints.addHintToSpot(spotId, hint);
                            }
                        }

                        // Additionally, if monopolize and not just prioritize, need to remove the
                        // spot from ALL groups.
                        if (hintSettings.barren.monopolizeSpots)
                            removeSpotFromMutableGroups(spotId);
                    }
                }
            }

            return placedNormalHintUids;
        }

        private List<Hint> getAgithaHint()
        {
            int numBugsInPool = 0;
            Dictionary<Item, List<string>> interestingItemToCheckNames = new();

            foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
            {
                string agithaRewardCheckName = pair.Value;
                genData.hinted.alreadyCheckAgithaHintClaimed.Add(agithaRewardCheckName);

                // If unreachable or excluded, then skip. Vanilla still should be included.
                if (
                    genData.unreachableChecks.Contains(agithaRewardCheckName)
                    || HintUtils.checkIsExcluded(agithaRewardCheckName)
                )
                    continue;

                numBugsInPool += 1;

                Item contents = HintUtils.getCheckContents(agithaRewardCheckName);

                bool shouldHint;
                if (genData.sSettings.adjustHintsForCompletionists)
                {
                    // Note that the sign will indicate golden bugs on Agitha as well since they are
                    // needed for completion.
                    shouldHint = !HintConstants.junkItems.Contains(contents);
                }
                else
                {
                    // Normally we hint non-bugs which are Good. Note that we include purely based
                    // on status, so we list poeSouls even if they are not majorItems for example.
                    shouldHint =
                        !HintConstants.bugsToRewardChecksMap.ContainsKey(contents)
                        && genData.CheckIsGood(agithaRewardCheckName);
                }

                if (shouldHint)
                {
                    if (
                        !interestingItemToCheckNames.TryGetValue(
                            contents,
                            out List<string> checkNamesForItem
                        )
                    )
                    {
                        checkNamesForItem = new();
                        interestingItemToCheckNames[contents] = checkNamesForItem;
                    }
                    checkNamesForItem.Add(agithaRewardCheckName);
                }
            }

            if (numBugsInPool < 1)
                return null;

            List<string> interestingAgithaChecks;
            if (interestingItemToCheckNames.Count > 0)
            {
                List<KeyValuePair<Item, List<string>>> asList =
                    interestingItemToCheckNames.ToList();
                // Shuffle list before sorting so no info is given away by the order the items are
                // listed on the sign. Otherwise if both items had 1 copy for example, you could
                // narrow down which bugs led to Item2 after you trade a bug in for Item1.
                HintUtils.ShuffleListInPlace(genData.rnd, asList);

                interestingAgithaChecks = asList
                    .OrderByDescending((kvp) => kvp.Value.Count)
                    .SelectMany((kvp) => kvp.Value)
                    .ToList();
            }
            else
                interestingAgithaChecks = new();

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
                bool doBarrenHandling = false;
                if (
                    currHintDef.hintCreator.type == HintCreatorType.Barren
                    && hintSettings.barren.monopolizeSpots
                )
                {
                    BarrenHintCreator bhCreator = currHintDef.hintCreator as BarrenHintCreator;
                    if (bhCreator != null)
                        doBarrenHandling = true;
                }

                // If we would be creating Barren hints when barren ownZoneBehavior is set to
                // "monopolize", then we need to do special handling where we create hints 1 at a
                // time. Else we do more basic handling below. Note: we need to do this even for
                // categories since they can have dependent zones (ex: SouthernDesert => CoO, or LH
                // => LS under ER).
                if (doBarrenHandling)
                {
                    for (int i = 0; i < currDefProps.iterations; i++)
                    {
                        // If no picks left, then we cannot keep creating hints.
                        int? currPicksLeft = layerData.GetCurrPicksLeft();
                        if (currPicksLeft != null)
                        {
                            int currPicksLeftInt = (int)currPicksLeft;
                            if (currPicksLeftInt < 1)
                                break;
                        }

                        List<SpotPenalty> successfulNewList = null;
                        AreaId successfulAreaId = null;
                        // Note: `successfulSpotPenalty` can still be null if there was no penalty.
                        // Ex: the zone is not in the group. Imagine group is OW zones and we create
                        // a barren hint for a dungeon.
                        SpotPenalty successfulSpotPenalty = null;

                        BarrenPenalizer lambda = (AreaId areaId, HashSet<Zone> childZones) =>
                        {
                            Zone? zone = null;
                            if (areaId.type == AreaId.AreaType.Zone)
                                zone = ZoneUtils.StringToIdThrows(areaId.stringId);

                            List<SpotPenalty> newList = layerData.CreateNewPenaltiesList(
                                zone,
                                childZones,
                                out SpotPenalty createdSpotPenalty
                            );

                            int numHints = layerData.getNumCreatableHints(
                                1,
                                currDefProps.copies,
                                currDefProps.minCopies,
                                newList
                            );
                            if (numHints > 0)
                            {
                                successfulAreaId = areaId;
                                successfulNewList = newList;
                                successfulSpotPenalty = createdSpotPenalty;
                                return true;
                            }
                            else
                            {
                                // TODO: cache failed SpotPenalties in a HashSet so can immediately
                                // know if will fail. Not strictly necessary, but could be good for
                                // case where we have room for 0 penalties and everything would have
                                // 1 penalty. Then again, really doesn't seem super necessary, and
                                // in practice don't expect it to ever run in most cases.
                            }
                            return false;
                        };

                        List<Hint> hints = currHintDef.hintCreator.tryCreateHint(
                            genData,
                            hintSettings,
                            1,
                            cache,
                            lambda
                        );

                        if (ListUtils.isEmpty(hints))
                        {
                            if (successfulNewList != null)
                            {
                                int numHintsCreated = hints != null ? hints.Count : 0;
                                throw new Exception(
                                    $"Expected BarrenZoneHintCreator to produce exactly 1 hint, but produced '{numHintsCreated}'."
                                );
                            }
                            // If was expected to produce no hints, then simply break.
                            break;
                        }
                        else if (hints.Count > 1)
                            throw new Exception(
                                $"Expected BarrenZoneHintCreator to produce at most 1 hint, but produced '{hints.Count}'."
                            );

                        BarrenHint hint = hints[0] as BarrenHint;
                        if (hint == null)
                            throw new Exception("Unable to cast hint to BarrenHint.");

                        if (successfulAreaId == null)
                            throw new Exception("Expected successfulAreaId to be non-null.");

                        if (hint.areaId != successfulAreaId)
                            throw new Exception(
                                $"Created BarrenHint for areaId '{hint.areaId.stringId}' did not match expected zone it said it would create ({successfulAreaId.stringId})."
                            );

                        if (successfulSpotPenalty != null)
                        {
                            // Update uniqueHintId of spotPenalty for later use.
                            successfulSpotPenalty.uniqueHintId = hint.uniqueHintId;
                        }

                        HintDefResult hintDefResult = HintDefResult.FromHintDefProps(
                            hint,
                            currDefProps
                        );
                        results.AddHintDefResult(hintDefResult);

                        layerData.handleCreatedHint(hintDefResult, successfulNewList);
                    }
                }
                else
                {
                    int numHints = layerData.getNumCreatableHints(
                        currDefProps.iterations,
                        currDefProps.copies,
                        currDefProps.minCopies,
                        null
                    );

                    // Do not call tryCreateHint if we want to generate 0 hints.
                    if (numHints > 0)
                    {
                        List<Hint> hints = currHintDef.hintCreator.tryCreateHint(
                            genData,
                            hintSettings,
                            numHints,
                            cache,
                            null
                        );

                        if (!ListUtils.isEmpty(hints))
                        {
                            foreach (Hint hint in hints)
                            {
                                HintDefResult hintDefResult = HintDefResult.FromHintDefProps(
                                    hint,
                                    currDefProps
                                );
                                results.AddHintDefResult(hintDefResult);

                                layerData.handleCreatedHint(hintDefResult, null);
                            }
                        }
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

                // Skip over excluded ones entirely. Vanilla can still be listed.
                if (HintUtils.checkIsExcluded(checkName))
                    continue;

                int reqFoundSoulsToDoCheck = soulsForCheck - startingSouls;
                if (reqFoundSoulsToDoCheck < 0)
                    reqFoundSoulsToDoCheck = 0;

                bool failedMinSouls =
                    jovani.minSoulsForHint != null && jovani.minSoulsForHint > soulsForCheck;

                bool failedMinFoundSouls =
                    jovani.minFoundSoulsForHint != null
                    && reqFoundSoulsToDoCheck < jovani.minFoundSoulsForHint;

                bool unhinted = failedMinSouls || failedMinFoundSouls;

                bool vague = false;
                if (jovani.maxFoundSoulsForVagueItem != null)
                {
                    if (reqFoundSoulsToDoCheck <= jovani.maxFoundSoulsForVagueItem)
                        vague = true;
                }

                DetailedCheckStatus checkStatus = genData.CalcDetailedCheckStatus(checkName);
                // Use this CheckStatusDisplay for everything for now.
                CheckStatusDisplay checkStatusDisplay = CheckStatusDisplay.Required_Info;

                JovaniRewardsHint.JovaniCheckInfo checkInfo =
                    new(
                        genData,
                        checkName,
                        (byte)soulsForCheck,
                        unhinted,
                        vague,
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
                    .ResolveToChecks(genData);

                AreaId areaId = AreaId.Zone(zone);
                List<string> checksToHint = new();
                List<string> barrenBlockerChecks = new();
                foreach (string checkName in checkNames)
                {
                    // Note: we include Vanilla, and we hint any checks which would prevent barren.
                    // For completionist, this would include things like heart containers, etc.
                    if (!HintUtils.checkIsExcluded(checkName))
                    {
                        checksToHint.Add(checkName);

                        Item contents = HintUtils.getCheckContents(checkName);
                        bool itemAllowsBarrenForArea = genData.ItemAllowsBarrenForArea(
                            contents,
                            areaId
                        );

                        if (!itemAllowsBarrenForArea && genData.CheckWouldPreventBarren(checkName))
                        {
                            barrenBlockerChecks.Add(checkName);
                        }
                    }
                }

                if (checksToHint.Count > 0)
                {
                    // If big keys are OwnDungeon, then we need to calculate if
                    // the big key is included. This only happens for dungeons,
                    // and the item we are looking for.

                    bool includeBigKeyInfo = false;
                    List<string> bigKeyChecks = null;
                    if (genData.sSettings.bigKeySettings == BigKeySettings.Own_Dungeon)
                    {
                        includeBigKeyInfo = CheckForBeyondPointBigKeys(
                            zone,
                            checkNames,
                            out bigKeyChecks
                        );
                    }

                    if (placeHintsOnSpots)
                    {
                        spotToHints.addHintToSpot(
                            spotId,
                            BeyondPointHint.Create(
                                genData,
                                includeBigKeyInfo,
                                barrenBlockerChecks,
                                bigKeyChecks
                            ),
                            true
                        );
                    }

                    if (ListUtils.isEmpty(barrenBlockerChecks))
                    {
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
            out List<string> checksWithBigKey
        )
        {
            if (ListUtils.isEmpty(categoryCheckNames))
                throw new Exception("Expected 'categoryCheckNames' to not be empty.");

            // Init 'out'
            checksWithBigKey = new();

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
                    && !genData.hinted.alreadyCheckAgithaHintClaimed.Contains(checkName)
                )
                {
                    Item contents = HintUtils.getCheckContents(checkName);
                    if (contents == bigKeyItem)
                    {
                        checksWithBigKey.Add(checkName);
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
                                    AreaId areaId = GetAreaIdForBigKeyHint(genData, srcCheckName);
                                    TradeChainHint hint = TradeChainHint.Create(
                                        genData,
                                        srcCheckName,
                                        false,
                                        true,
                                        areaId.type == AreaId.AreaType.Province
                                          ? TradeChainHint.AreaType.Province
                                          : TradeChainHint.AreaType.Zone,
                                        DetailedCheckStatus.Unknown,
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
                            AreaId areaId = GetAreaIdForBigKeyHint(genData, checkName);
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

        private AreaId GetAreaIdForBigKeyHint(HintGenData genData, string checkForBigKey)
        {
            string checkZoneName = genData.GetZoneNameForCheck(checkForBigKey);
            return AreaId.ZoneStr(checkZoneName);
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
                    CheckStatus status = genData.CalcCheckStatus(checkName);

                    if (status == CheckStatus.Required)
                        requiredAlways.Add(checkName);
                    else if (status == CheckStatus.Good)
                        goodAlways.Add(checkName);
                    else
                        badAlways.Add(checkName);
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
                {
                    // For unhinted Always checks (ex: 9 in pool but we only hint 5), these are
                    // considered to be known to be dead (or else we would have to hint them).
                    genData.hinted.AddHintedBarrenCheck(checkName);
                }
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
                    SpotId.Snowpeak_Mountain_Sign,
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

            // Remove all signs in unrequiredBarren dungeons from potential
            // spots to fill.
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

            // Get a list of all hintSpots that we actually need to fill.
            List<SpotId> spotsToFill = new();
            foreach (SpotId spotId in possibleSpotsToFill)
            {
                if (
                    !spotIdToHintSpot.TryGetValue(spotId, out HintSpot hintSpot)
                    || hintSpot.hints.Count == 0
                )
                {
                    Zone zoneForSpot = ZoneUtils.SpotIdToZone(spotId);
                    if (zoneForSpot == Zone.Invalid)
                        throw new Exception($"SpotId '{spotId}' mapped to Zone.Invalid.");
                    else if (zoneForSpot == Zone.Hyrule_Castle)
                    {
                        // Always add filler hints if needed to the HC sign
                        // since players are always expected to pass it.
                        spotsToFill.Add(spotId);
                        break;
                    }

                    // For other zones, we only want to fill in signs for zones
                    // which are not all excluded.
                    foreach (string checkName in genData.GetChecksForZone(zoneForSpot))
                    {
                        if (!HintUtils.checkIsPlayerKnownStatus(checkName))
                        {
                            spotsToFill.Add(spotId);
                            break;
                        }
                    }
                }
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
                    cache,
                    null
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
                    cache,
                    null
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

    public class SpotPenalty : IEquatable<SpotPenalty>, IComparable<SpotPenalty>
    {
        // TODO: override GetHashCode
        public int spotsToTake { get; private set; }
        public bool isSelfInGroup { get; private set; }
        public uint uniqueHintId { get; set; }
        public HashSet<Zone> childZones { get; private set; }

        public SpotPenalty(
            int spotsToTake,
            bool isSelfInGroup,
            uint uniqueHintId,
            HashSet<Zone> childZones
        )
        {
            this.spotsToTake = spotsToTake;
            this.isSelfInGroup = isSelfInGroup;
            this.uniqueHintId = uniqueHintId;
            this.childZones = childZones ?? new();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            SpotPenalty objAsThisType = obj as SpotPenalty;
            if (objAsThisType == null)
                return false;
            return Equals(objAsThisType);
        }

        public bool Equals(SpotPenalty other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(SpotPenalty other)
        {
            // Note: null is always considered the small for C#. Default Comparer sorting fails if
            // we don't match this. Positive result means we are bigger; negative means other is
            // bigger.
            if (other == null)
                return 1;

            int spotsToTakeComp = spotsToTake - other.spotsToTake;
            if (spotsToTakeComp != 0)
                return spotsToTakeComp;

            if (isSelfInGroup == other.isSelfInGroup)
                return 0;
            else if (isSelfInGroup)
                return 1;
            else
                return -1;
        }
    }

    public class HintLayerData
    {
        public bool killSwitched { get; private set; }
        public int layerLength { get; private set; }
        public int remainingSpots { get; private set; }

        // Note: cannot combine both picks lists because we need to know the
        // diff to apply to the previous index when we pop back to it.
        private List<int> picksRemainingList = new();
        private List<int> picksDiffList = new();
        public List<Hint> startingHints { get; private set; } = new();
        private HintGenData genData;
        private HintSettings hintSettings;
        private HintGroup group;

        // Starting and Barren zone penalty stuff:
        public int currMaxStartingAllowed { get; private set; } = 0;
        private List<int> pendingCopiesAsc = new();
        private List<SpotPenalty> pendingSpotPenalties = new();

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

        // Handles maxPicks, remainingSpots adjustments (either immediate or delayed under
        // starting), and also BarrenZone spotPenalties for barren monopolize stuff.
        public void handleCreatedHint(
            HintDefResult hintDefResult,
            List<SpotPenalty> newSpotPenalties
        )
        {
            if (hintDefResult == null)
                throw new Exception(
                    "Received a null HintDefResult, but expected a non-null param."
                );

            // Note for maxPicks: we do not care about copies, starting, etc. We just see "this many
            // hints were generated", so we decrement by that many.
            TryApplyPicksDiff(1);

            // If we need to update spotPenalties for a new BarrenHint, the new list is provided as
            // input since all of the calculations are handled externally to this function.
            if (newSpotPenalties != null)
            {
                pendingSpotPenalties = newSpotPenalties;
            }

            // If we are not currently under a starting hints portion of the tree, then resolve the
            // remainingSpots changes immediately (paying penalties as needed). If under starting
            // spots, then make adjustments to the copies list so we can adjust correctly once
            // starting hints are selected. Ex: if we have a 5-copy hint and a 3-copy and only one
            // is selected as starting, the amount that remainingSpots is reduced by can differ.

            if (currMaxStartingAllowed > 0)
            {
                // Add copies for hint to pendingCopiesAsc.
                pendingCopiesAsc.Add(hintDefResult.copies);
                pendingCopiesAsc.Sort();

                if (newSpotPenalties == null)
                {
                    // If `newSpotPenalties` not provided, then we are adding a non-barren hint, so
                    // add an empty spotPenalty. We add empty ones since worst-case scenario for
                    // reducing as many spots as possible is to pick non-Barren hints as starting,
                    // so the fact that these have empty penalties is relevant and needs to be
                    // accounted for.
                    SpotPenalty spotPenalty = new(0, false, hintDefResult.hint.uniqueHintId, null);
                    pendingSpotPenalties.Add(spotPenalty);
                    pendingSpotPenalties.Sort();
                    pendingSpotPenalties.Reverse();
                }

                if (pendingCopiesAsc.Count != pendingSpotPenalties.Count)
                    throw new Exception(
                        $"Expected pendingCopiesAsc (Count {pendingCopiesAsc.Count}) and pendingSpotPenalties (Count {pendingSpotPenalties.Count}) to have equal Counts."
                    );
            }
            else
            {
                // No pending starting hints.
                if (pendingSpotPenalties.Count > 1)
                    throw new Exception(
                        $"Expected 0 or 1 pendingSpotPenalties, but was '{pendingSpotPenalties.Count}'."
                    );

                resolveRemainingFromNewHint(hintDefResult, false);
            }
        }

        private void resolveRemainingFromNewHint(HintDefResult hintDefResult, bool isStartingHint)
        {
            // If found matching hint in penalties, then pay the penalty.
            if (pendingSpotPenalties.Count > 0)
            {
                uint hintId = hintDefResult.hint.uniqueHintId;
                for (int i = 0; i < pendingSpotPenalties.Count; i++)
                {
                    SpotPenalty spotPenalty = pendingSpotPenalties[i];
                    if (spotPenalty.uniqueHintId == hintId)
                    {
                        pendingSpotPenalties.RemoveAt(i);

                        int spotsToTake = spotPenalty.spotsToTake;
                        if (!isStartingHint && spotPenalty.isSelfInGroup)
                            spotsToTake -= 1;

                        if (spotsToTake > 0)
                            remainingSpots -= spotsToTake;
                        break;
                    }
                }
            }

            // If starting hint, don't need to further reduce spots after paying any potential
            // penalty.
            if (isStartingHint)
                return;

            // Otherwise reduce spotsRemaining based on copies or minCopies.
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

            throw new Exception(
                $"Failed to reduce layer spots. remainingSpots is currently '{remainingSpots}'."
            );
        }

        public void pushStarting(int starting)
        {
            if (currMaxStartingAllowed > 0)
                throw new Exception($"Nested 'starting' in hintDefs in invalid.");
            if (pendingCopiesAsc.Count != 0)
                throw new Exception(
                    $"Expected pendingCopiesAsc to have Count of 0, but was '{pendingCopiesAsc.Count}'."
                );
            if (pendingSpotPenalties.Count != 0)
                throw new Exception(
                    $"Expected spotPenalties to have Count of 0, but was '{pendingSpotPenalties.Count}'."
                );

            currMaxStartingAllowed = starting;
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

                    // Adjust remainingSpots for each selected starting hint. Handles penalties
                    // internally.
                    resolveRemainingFromNewHint(result, true);
                }
            }

            // Then for any still in results, resolve as non-starting.
            foreach (HintDefResult result in results.HintDefResults)
            {
                // Adjust remainingSpots for the remaining hints not selected as starting.
                resolveRemainingFromNewHint(result, false);
            }

            // Other cleanup:
            pendingCopiesAsc = new();
            currMaxStartingAllowed = 0;

            if (pendingSpotPenalties.Count > 0)
                throw new Exception(
                    $"Expected pendingSpotPenalties to be Count 0, but had Count '{pendingSpotPenalties.Count}'."
                );
        }

        private List<HintDefResult> RemoveRandomStartingHints(RecHintResults results, int numToPick)
        {
            List<HintDefResult> selected = new();

            if (ListUtils.isEmpty(results.HintDefResults) || numToPick < 1)
                return selected;

            List<int> priorityPicks = new();
            List<int> secondaryPicks = new();

            for (int i = 0; i < results.HintDefResults.Count; i++)
            {
                if (hintSettings.barren.monopolizeSpots)
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

        public bool CheckPushMaxPicks(HintDefProps hintDefProps)
        {
            int newMaxPicks = hintDefProps.maxPicks;
            if (newMaxPicks < 1)
                return false;

            bool shouldPush = false;
            // If no active maxPicks, then it is always valid to push one.
            if (picksRemainingList.Count < 1)
                shouldPush = true;
            else
            {
                // Only push the new inner maxPicks if it is less than how many picks we currently
                // have remaining. If we were to have something like 2 picks left and an inner node
                // had a maxPicks of 4, then we are still only allowed to pick 2 more things since
                // we are still under the outer maxPicks node whose rules must apply correctly.
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

            // Ignore if new maxPicks is not defined or if it would be less restrictive than what we
            // are currently on.
            return false;
        }

        public void PopMaxPicks()
        {
            if (picksRemainingList.Count < 1 || picksDiffList.Count < 1)
                throw new Exception("Attempted to pop maxPicks, but a list was already empty.");

            int currDiff = picksDiffList[^1];
            picksRemainingList.RemoveAt(picksRemainingList.Count - 1);
            picksDiffList.RemoveAt(picksDiffList.Count - 1);

            // Note: we store maxPicks as a list and keep track of the diffs because we might have
            // nested maxPicks. Imagine you have a maxPicks of 3 at a node in the tree, and then
            // within that you have a node with maxPicks of 1. Later when we create the hint and
            // stop for the maxPicks of 1, we are still under the maxPicks of 3 and we picked
            // something, meaning there are only 2 of 3 remaining for the outer maxPicks. So we have
            // to apply the diff back to the outer maxPicks to indicate we have already used up one
            // of the picks once we pop the inner maxPicks.
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
            // If we have no maxPicks currently in effect, return null.
            if (picksRemainingList.Count < 1)
                return null;
            // If we have at least one maxPicks in effect, look at the current innermost active one
            // and see how many maxPicks we are allowed vs how many we have used.
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

        private int calcPenalties(List<SpotPenalty> spotPenalties)
        {
            if (ListUtils.isEmpty(spotPenalties))
                return 0;

            int result = 0;
            for (int i = 0; i < spotPenalties.Count; i++)
            {
                SpotPenalty spotPenalty = spotPenalties[i];

                if (i < currMaxStartingAllowed)
                {
                    // If would be taken as starting.
                    result += spotPenalty.spotsToTake;
                }
                else
                {
                    // If would not be taken as starting.
                    int val = spotPenalty.spotsToTake;
                    if (spotPenalty.isSelfInGroup && val > 0)
                        val -= 1;
                    result += val;
                }
            }

            return result;
        }

        public List<SpotPenalty> CreateNewPenaltiesList(
            Zone? zone,
            HashSet<Zone> childZones,
            out SpotPenalty createdSpotPenalty
        )
        {
            List<SpotPenalty> tempPenaltiesList = new(pendingSpotPenalties);

            int numMatchingZonesInGroup = 0;
            bool isSelfInGroup = false;

            if (zone != null)
            {
                Zone actualZone = (Zone)zone;
                SpotId spotIdForZone = ZoneUtils.IdToSpotId(actualZone);
                // Note: it is possible to have a valid zone which does not have a valid spotId it
                // can map to (ex: Agitha's Castle), so we do not throw for invalid here or below.
                if (spotIdForZone != SpotId.Invalid && group.spots.Contains(spotIdForZone))
                {
                    numMatchingZonesInGroup += 1;
                    isSelfInGroup = true;
                }
            }

            if (childZones != null && childZones.Count > 0)
            {
                if (zone != null)
                {
                    Zone actualZone = (Zone)zone;
                    // Don't double-count main zone. If also listed in childZones, throw since this is
                    // not expected.
                    if (childZones.Contains(actualZone))
                        throw new Exception(
                            $"Did not expect to find zone '{actualZone}' defined as its own child zone."
                        );
                }

                foreach (Zone childZone in childZones)
                {
                    SpotId spotId = ZoneUtils.IdToSpotId(childZone);
                    if (spotId != SpotId.Invalid && group.spots.Contains(spotId))
                        numMatchingZonesInGroup += 1;
                }
            }

            // Note: we go ahead and create even if 0 numMatching. Doesn't impact anything, but we
            // will expect the penalties and pendingCopiesAsc list counts to match.
            SpotPenalty spotPenalty = new(numMatchingZonesInGroup, isSelfInGroup, 0, childZones);
            createdSpotPenalty = spotPenalty;

            tempPenaltiesList.Add(spotPenalty);
            tempPenaltiesList.Sort();
            tempPenaltiesList.Reverse();

            return tempPenaltiesList;
        }

        public int getNumCreatableHints(
            int iterations,
            int copies,
            int minCopies,
            List<SpotPenalty> spotPenaltyList
        )
        {
            if (spotPenaltyList == null)
                spotPenaltyList = pendingSpotPenalties;

            if (iterations < 1)
                return 0;

            int currPicksLeftInt = int.MaxValue;
            int result;

            int? currPicksLeft = GetCurrPicksLeft();
            if (currPicksLeft != null)
            {
                currPicksLeftInt = (int)currPicksLeft;
                if (currPicksLeftInt < 1)
                    return 0;
            }

            // Calculate our base remainingSpots - penalty.
            int currentPenalties = calcPenalties(spotPenaltyList);
            int baseSpotsAvailable = remainingSpots - currentPenalties;

            if (currMaxStartingAllowed > 0)
            {
                // If has pending starting:

                // We also need to take off spots for any that are guaranteed already pushed out
                // from the starting range. Ex: 3 starting, and we have already generated hints with
                // copies [2,3,3],3,4 such that we are guaranteed to already be losing 7 spots (3+4
                // which don't fit in starting list).
                for (int i = currMaxStartingAllowed; i < pendingCopiesAsc.Count; i++)
                {
                    baseSpotsAvailable -= pendingCopiesAsc[i];
                }

                int fullCopySize = copies;

                int startingSpotsUnused = currMaxStartingAllowed - pendingCopiesAsc.Count;
                if (startingSpotsUnused < 0)
                    startingSpotsUnused = 0;

                int numCanAffordToPushOut = 0;
                int cummSpotsUsed = 0;

                int startIdx = pendingCopiesAsc.Count - 1;
                if (startIdx >= currMaxStartingAllowed)
                    startIdx = currMaxStartingAllowed - 1;

                bool failedAPushOut = false;
                for (int i = startIdx; i >= 0; i--)
                {
                    int fullCopySizeInList = pendingCopiesAsc[i];
                    if (fullCopySizeInList > fullCopySize)
                    {
                        int tempCummSpotsUsed = cummSpotsUsed + fullCopySizeInList;
                        if (tempCummSpotsUsed <= baseSpotsAvailable)
                        {
                            // Can afford to push out.
                            numCanAffordToPushOut += 1;
                            cummSpotsUsed = tempCummSpotsUsed;
                        }
                        else
                        {
                            failedAPushOut = true;
                            break;
                        }
                    }
                    else
                    {
                        // Pushed out all that have greater copies than our current fullCopySize.
                        break;
                    }
                }

                int finalAvailable = startingSpotsUnused + numCanAffordToPushOut;

                // If we failed to push one out, then we can only add up until that point. Imagine
                // the thing we fail to push out is 100 copies. We cannot assume it gets picked for
                // starting, so it is never safe to push out. Therefore it is not safe to do
                // anything other than use our starting spots in this case. We also handle the case
                // where we push out some, but we can't push out all.
                if (!failedAPushOut)
                {
                    int newSpotsAvailable = baseSpotsAvailable - cummSpotsUsed;
                    int fullCopiesFittingInRemaining = newSpotsAvailable / fullCopySize;

                    finalAvailable += fullCopiesFittingInRemaining;
                }

                result = finalAvailable;
            }
            else
            {
                // If no pending starting, then do simple calc.
                int fullAllowed = baseSpotsAvailable / copies;

                int partialAllowed = 0;
                if (minCopies > 0)
                {
                    int remainder = baseSpotsAvailable % copies;
                    if (minCopies <= remainder)
                        partialAllowed = 1;
                }

                int maxAllowed = fullAllowed + partialAllowed;

                int numHints = iterations;
                if (numHints > maxAllowed)
                    numHints = maxAllowed;

                result = numHints;
            }

            if (result > iterations)
                result = iterations;

            if (result > currPicksLeftInt)
                result = currPicksLeftInt;

            return result;
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
