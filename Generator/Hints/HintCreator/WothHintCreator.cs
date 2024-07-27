namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class WothHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Woth;

        private static readonly HashSet<HintCategory> maxValidCategories =
            new()
            {
                HintCategory.Grotto,
                HintCategory.Post_dungeon,
                HintCategory.Mist,
                HintCategory.Owl_Statue,
                HintCategory.Llc_Lantern_Chests,
                HintCategory.Underwater,
                HintCategory.Southern_Desert,
                HintCategory.Northern_Desert,
            };

        private static readonly HashSet<AreaId.AreaType> validAreaTypes =
            new() { AreaId.AreaType.Zone, AreaId.AreaType.Province, AreaId.AreaType.Category, };

        private AreaId.AreaType areaType = AreaId.AreaType.Zone;

        private WothHintCreator() { }

        new public static WothHintCreator fromJObject(JObject obj)
        {
            WothHintCreator inst = new WothHintCreator();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                string areaTypeStr = HintSettingUtils.getOptionalString(options, "areaType", null);
                if (areaTypeStr != null)
                {
                    AreaId.AreaType areaType = AreaId.ParseAreaTypeStr(areaTypeStr);
                    if (!validAreaTypes.Contains(areaType))
                        throw new Exception($"AreaType '{areaType}' is not a valid areaType.");

                    inst.areaType = areaType;
                }
            }

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            if (numHints < 1)
                return null;

            // Dictionary<string, HashSet<AreaId>> checkToPossibleAreas = new();
            Dictionary<AreaId, HashSet<string>> areaToPossibleChecks = new();

            // We can hint zone/province/category.

            // Iterate through required checks and group by validAreaId => HashSet<checkName>?

            // Get zones which can be hinted SpoL, and track which ones have a
            // check which can be hinted SpoL which is not in sphere 0.
            foreach (string checkName in genData.requiredChecks)
            {
                if (!genData.checkCanBeHintedSpol(checkName))
                    continue;

                HashSet<AreaId> newAreaIds = CheckToAreaIds(genData, hintSettings, checkName);

                if (ListUtils.isEmpty(newAreaIds))
                    continue;

                // if (!checkToPossibleAreas.TryGetValue(checkName, out HashSet<AreaId> areaIds))
                // {
                //     areaIds = new();
                //     checkToPossibleAreas[checkName] = areaIds;
                // }
                // areaIds.UnionWith(newAreaIds);

                foreach (AreaId newAreaId in newAreaIds)
                {
                    if (
                        !areaToPossibleChecks.TryGetValue(newAreaId, out HashSet<string> checkNames)
                    )
                    {
                        checkNames = new();
                        areaToPossibleChecks[newAreaId] = checkNames;
                    }
                    checkNames.Add(checkName);
                }
            }

            // For each area that has not been hinted before, create voseInst.

            // Once every area has been hinted, we move on to just randomly
            // picking an area.

            List<KeyValuePair<double, AreaId>> weightedList = new();
            foreach (KeyValuePair<AreaId, HashSet<string>> pair in areaToPossibleChecks)
            {
                AreaId areaId = pair.Key;
                if (genData.hinted.hintedWothAreas.Contains(areaId))
                {
                    // Only areas which we have not hinted woth already get
                    // added to the weighted picking. The rest will be handled
                    // after there are no options left for weighted picking.
                    continue;
                }

                double weight = genData.GetAreaWothWeight(areaId);
                // Never expecting weight to be 0, but it is a possible return.
                if (weight <= 0)
                    continue;
                weightedList.Add(new(weight, areaId));
            }

            List<Hint> hints = new();

            if (weightedList.Count > 0)
            {
                VoseInstance<AreaId> inst = VoseAlgorithm.createInstance(weightedList);
                while (inst.HasMore() && hints.Count < numHints)
                {
                    AreaId areaId = inst.NextAndRemove(genData.rnd);

                    if (AreaIdFailsDungeonMaxWothCheck(genData, hintSettings, areaId))
                    {
                        // Skip over if we selected a dungeon and we have
                        // already met the max wothHints for dungeons limit.
                        continue;
                    }

                    // Pick check for this areaId.
                    HashSet<string> possibleChecks = areaToPossibleChecks[areaId];
                    string checkName = HintUtils.RemoveRandomHashSetItem(
                        genData.rnd,
                        possibleChecks
                    );

                    if (possibleChecks.Count < 1)
                        areaToPossibleChecks.Remove(areaId);

                    hints.Add(new WothHint(areaId, checkName));

                    genData.hinted.alreadyCheckDirectedToward.Add(checkName);
                    genData.hinted.hintedWothAreas.Add(areaId);
                }
            }

            while (areaToPossibleChecks.Count > 0 && hints.Count < numHints)
            {
                KeyValuePair<AreaId, HashSet<string>> pair = HintUtils.PickRandomDictionaryPair(
                    genData.rnd,
                    areaToPossibleChecks
                );

                // Pick check for this areaId.
                AreaId areaId = pair.Key;
                HashSet<string> possibleChecks = pair.Value;
                string checkName = HintUtils.RemoveRandomHashSetItem(genData.rnd, possibleChecks);

                if (possibleChecks.Count < 1)
                    areaToPossibleChecks.Remove(areaId);

                if (AreaIdFailsDungeonMaxWothCheck(genData, hintSettings, areaId))
                {
                    // Skip over if we selected a dungeon and we have already
                    // met the max wothHints for dungeons limit. Also remove so
                    // we do not select again.
                    areaToPossibleChecks.Remove(areaId);
                    continue;
                }

                hints.Add(new WothHint(areaId, checkName));

                genData.hinted.alreadyCheckDirectedToward.Add(checkName);
                genData.hinted.hintedWothAreas.Add(areaId);
            }

            // string hintZone = checkToHintZone[checkName];

            // In this case, we can still pick the category to hint. It is
            // just that when we remove the check, it is possible that
            // multiple categories are no longer valid to hint (for example,
            // if that one check was the only valid target in either
            // category).

            // So we need the list of valid checks which point to the
            // areaIds, and we need the list of valid areaIds which point
            // back to checks.

            // After picking an areaId, we pick a check in that areaId.
            // Also mark that areaId as hinted.

            // Also from that check, Dictionary to get the HashSet of areaIds.
            // Then clean those areaIds in the reverse dictionary (areaId to valid checks).
            // If no valid checks left, remove that areaId from the list of possibilities.

            // bool hasSphereLater = false;
            // areaToHasSphereLater.TryGetValue(hintZone, out hasSphereLater);
            // if (!hasSphereLater)
            // {
            //     hasSphereLater = !genData.isCheckSphere0(checkName);
            // }
            // zoneToHasSphereLater[hintZone] = hasSphereLater;




            // condense into list of zones.

            // pick zone based on numberOfNonSphere0 checks (large ones are
            // bad). single check ones are also bad. optimal size would
            // probably be

            // Don't count checks known to player

            // Dungeons, Desert, and LLC are hardcoded weaker?

            // Interesting hint is non-sphere0 ZD.
            // Also Lanayru Field
            // Also Sacred Grove
            // Faron Woods would be fine also
            // WoCT and SoCt are fine
            // Kak Gorge is fine
            // Snowpeak is super good if non-s0 check.

            // Boring:
            // Dungeons
            // LLC
            // Desert
            // Bulblin Camp
            // Sphere0 Kak GY (non-s0 is fine)
            // s0 Ordon check

            // ^ these are all boring because "I was going to do that
            // anyway" or if you find the hint later, then "I already did
            // that".

            //   "Death Mountain" 1 (0, 1), lol

            // Special ones:
            //   "Agitha", don't hint if gets own hint
            //   "Hidden Village", good if not excluded
            //   "Hero's Spirit", great if not excluded
            //   "Cave of Ordeals", great if not excluded

            //   "Castle Town" 4 (2, 2), PLEASE
            //   "Snowpeak" 3 (1, 2), great if not s0, else meh

            //   "Lanayru Field" 6 (0, 6), good
            //   "Sacred Grove" 6 (0, 6), good
            //   "Great Bridge of Hylia" 7 (0, 7), good
            //   "Kakariko Gorge" 9 (2, 7), good (better if not s0)
            //   "N Eldin Field" 8 (0, 8), good (no s0 at all)
            //   "Lanayru Spring" 7 (0, 7), good (no s0)
            //   "Faron Woods" 10 (3, 7), probably good since encourages glitches (3 s0, 7 not)

            //   "West of Castle Town" 5 (1, 4), pretty good (1 s0)
            //   "South of Castle Town" 6 (3, 3), pretty good
            //   "Lake Hylia" 10 (8, 2), not bad

            //   "Zora's Domain" 6 (3, 3), good if not s0. Okay if s0
            //   "Faron Field" 7 (4, 3), okay if not s0, but kind of meh since usually do
            //   "Kakariko Village" 8 (5, 3, 2), fine
            //   "S Eldin Field" 8 (4, 4), fine (better if not s0)
            //   "Kakariko Graveyard" 4 (2, 2), way better if not s0, else meh
            //   "Upper Zora's River" 3 (2, 1), okay

            //   "Bulblin Camp" 6 (0, 6), not amazing; excluded anyway

            // Special bad ones:
            //   "Ordon" 10 (7, 3), never great
            //   "Lake Lantern Cave" 15 (0, 15), HORRIBLE
            //   "Gerudo Desert" 16 (0, 16), HORRIBLE

            //   "Forest Temple" (all have a lot), all dungeons are really boring
            //   "Goron Mines",
            //   "Lakebed Temple",
            //   "Arbiter's Grounds",
            //   "Snowpeak Ruins",
            //   "Temple of Time",
            //   "City in the Sky",
            //   "Palace of Twilight",
            //   "Hyrule Castle,

            // Build possible checks. Should hint by zone (fully prioritize not
            // hinting the same zone twice; can store hinted zones on hinted;
            // wanted to do this anyway for dungeon woth limit).

            return hints;
        }

        private HashSet<AreaId> CheckToAreaIds(
            HintGenData genData,
            HintSettings hintSettings,
            string checkName
        )
        {
            switch (areaType)
            {
                case AreaId.AreaType.Zone:
                {
                    string zoneName = HintUtils.checkNameToHintZone(checkName);
                    if (
                        AreaIdFailsDungeonMaxWothCheck(
                            genData,
                            hintSettings,
                            AreaId.ZoneStr(zoneName)
                        )
                    )
                    {
                        // Skip over if it is a dungeon and we have already met
                        // the max wothHints for dungeons limit.
                        return null;
                    }
                    return new() { AreaId.ZoneStr(zoneName) };
                }
                case AreaId.AreaType.Province:
                {
                    Province province = HintUtils.checkNameToHintProvince(checkName);
                    return new() { AreaId.Province(province) };
                }
                case AreaId.AreaType.Category:
                {
                    HashSet<HintCategory> categories = HintCategoryUtils.checkNameToCategories(
                        checkName
                    );
                    HashSet<AreaId> areaIds = new();
                    if (!ListUtils.isEmpty(categories))
                    {
                        foreach (HintCategory category in categories)
                        {
                            // Filter out any categories which are not valid,
                            // such as CitS east wing.
                            if (maxValidCategories.Contains(category))
                                areaIds.Add(AreaId.Category(category));
                        }
                    }
                    return areaIds;
                }
                default:
                    throw new Exception(
                        $"Cannot call CheckToAreaIds with invalid areaType '{areaType}'."
                    );
            }
        }

        private bool AreaIdFailsDungeonMaxWothCheck(
            HintGenData genData,
            HintSettings hintSettings,
            AreaId areaId
        )
        {
            // Note the comparison will return false when
            // hintSettings.dungeons.maxWothHints is null.
            return (
                areaId.type == AreaId.AreaType.Zone
                && HintUtils.hintZoneIsDungeon(areaId.stringId)
                && genData.hinted.GetNumHintedWothDungeons() >= hintSettings.dungeons.maxWothHints
            );
        }
    }
}
