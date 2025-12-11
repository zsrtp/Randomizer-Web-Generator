namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class BarrenHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Barren;

        private static readonly HashSet<HintCategory> defaultHintCategories =
            new()
            {
                HintCategory.Grotto,
                // Maybe cannot expect anyone other than racers to know what
                // exactly post-dungeon refers to?
                // HintCategoryEnum.Post_dungeon,
                HintCategory.Mist,
                HintCategory.Owl_Statue,
                HintCategory.Llc_Lantern_Chests,
                HintCategory.Underwater,
                HintCategory.Southern_Desert,
                HintCategory.Northern_Desert,
                HintCategory.Golden_Wolf,
            };

        private static readonly Dictionary<Zone, string> dungeonZoneToRegionName =
            new()
            {
                { Zone.Forest_Temple, "Forest Temple" },
                { Zone.Goron_Mines, "Goron Mines" },
                { Zone.Lakebed_Temple, "Lakebed Temple" },
                { Zone.Arbiters_Grounds, "Arbiters Grounds" },
                { Zone.Snowpeak_Ruins, "Snowpeak Ruins" },
                { Zone.Temple_of_Time, "Temple of Time" },
                { Zone.City_in_the_Sky, "City in The Sky" },
                { Zone.Palace_of_Twilight, "Palace of Twilight" },
            };

        private static readonly List<string> bossRooms =
            new()
            {
                "Forest Temple Boss Room",
                "Goron Mines Boss Room",
                "Lakebed Temple Boss Room",
                "Arbiters Grounds Boss Room",
                "Snowpeak Ruins Boss Room",
                "Temple of Time Boss Room",
                "City in The Sky Boss Room",
                "Palace of Twilight Boss Room",
            };

        // TODO: this should be calculated at the start of the genData. If an area is completely
        // excluded, then it does not matter as a dependency? Dependencies are all based on
        // interiors/caves, so nested stuff is not needed? Also all howling stones are found in
        // exteriors. Post-dungeon stuff is easily calculated as well. For now, don't worry about
        // more than one level deep since that is all that matters.

        // continued: if an interior is fully excluded, then no reason to hint anything about it.
        // Ex: if CoO is excluded, then no reason to say "0 items in {GD itself}" since there are no
        // internal zones which matter. But if was included, then would be important to indicate we
        // are not necessarily saying you can skip going to the GD interiors/caves.

        // Dependencies assuming no ER stuff:
        // DM => Ordon gold wolf (note: only 6 gold wolf howling stones)
        // UZR => BCT gold wolf
        // LH => LLC, LS, GD gold wolf
        // FW => SoCT gold wolf
        // SP => KGY gold wolf
        // HV => CT gold wolf
        // GD => CoO
        // CT => Agitha's Castle
        // dungeons:
        // GM => post-Fyrus
        // ToT => post-Armogohma, but only if vanilla Ilia quest setting.
        // SPR => post-SPR
        // Note: no post-LBT for Midna stuff or anything.

        // Also, northern desert needs to be updated to say 'itself' as well for CoO? Anything else?

        // Maybe make AreaDepHelper class in HintGenData.
        // It calcs an Area's dependent Areas. Will be more involved once more ER.
        // API:
        // public bool AreaHasDependentAreas(AreaId areaId)
        // ^ used for NumMajorItems hint to know if should say "itself".

        // For Barren Hints, need to know any dependent Zones (which are not entirely Vanilla or
        // Excluded or Excluded-Unrequired) which would be known barren if we hint the parent
        // barren.

        // Also need to be able to map an area back to its parent. This way if we are trying to hint
        // the child barren, we first must check if the parent can be Barren-hinted. This is not
        // related to northern/southern desert since those are not dependent offshoots of GD but GD
        // itself. We would have special handling in the BarrenHintCreator to handle those so that
        // we don't generate one unless the other side has something such that GD could not be
        // hinted barren.

        // We only need to jump up to the parent if it is allowed in the valid areas. For
        // northern/southern desert, those should have a built-in handling that they are impossible
        // to generate unless the other one is either not barren or completely
        // Vanilla/Excluded(Unreq). I think we would just have to confirm that a "barren southern
        // desert" hint is not possible to create (ignoring valid/invalid stuff) for example. But
        // this is not related to high-level dependency stuff.

        private static readonly HashSet<AreaId.AreaType> validAreaTypes =
            new() { AreaId.AreaType.Zone, AreaId.AreaType.Category, };

        // Needs to know areaType (defaults to zone)
        // Needs to know validAreas (defaults to all)

        private AreaId.AreaType areaType = AreaId.AreaType.Zone;
        private HashSet<AreaId> validAreas = null;

        private BarrenHintCreator() { }

        new public static BarrenHintCreator fromJObject(JObject obj)
        {
            BarrenHintCreator inst = new BarrenHintCreator();

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

                // If defines validAreas, parse according to the inst.areaType.
                List<string> validAreaStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validAreas",
                    null
                );
                if (validAreaStrList != null)
                {
                    inst.validAreas = new();
                    foreach (string validAreaStr in validAreaStrList)
                    {
                        if (validAreaStr.StartsWith("alias:"))
                        {
                            string alias = validAreaStr.Substring(6);
                            HashSet<AreaId> resolved = resolveAreaAlias(alias);
                            inst.validAreas.UnionWith(resolved);
                        }
                        else
                        {
                            if (validAreaStr.Contains('.'))
                            {
                                // Handle string specifying AreaType explicity.
                                AreaId areaId = AreaId.ParseString(validAreaStr);
                                inst.validAreas.Add(areaId);
                            }
                            else
                            {
                                // Use default areaType for string which is not explicit.
                                AreaId areaId = AreaId.ParseString(inst.areaType, validAreaStr);
                                inst.validAreas.Add(areaId);
                            }
                        }
                    }
                }
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
            if (numHints < 1)
                return null;

            // So now it should be safe to map a child dep to a single parent.

            // Or maybe we just create the entire tree here? For barren hints, we don't need to care
            // about saying "itself" or not. And for NumMajor hints, we only need to care about
            // literal child dependencies. Don't need to worry about parent-child stuff really.

            // We can build every dependency tree up front, even if we might not need them. Then
            // when we need to check if an AreaId is possible, we check it and keep track of the
            // result in a dictionary. Then we go up the tree and calculate until we reach one
            // which is not possible.

            // We should also have a dictionary of AreaId to TreeNode.

            // We should also keep track on the TreeNode if it is even a validArea. We do not need
            // to keep going up the tree if the next parent node is not a validArea. Well, that's
            // not true since the top parent might be valid. Really once we have the trees defined,
            // we need to simply do a full recursive calc from the root node. Then on each tree
            // node, we can keep track of "is it possible to hint barren" and "is it in validAreas".
            // It's only possible if all of its own deps are satisfied. It isn't exactly related to
            // the tree since from a dep perspective GD => CoO even though this isn't on the tree.
            // However, it is true that CoO => SD => GD from a tree perspective for Barren hint
            // priority.

            // Note: we cannot build the tree until we have genData.

            VoseInstance<PotentialBarrenArea> inst = cache.GetFromLatestNodeCache<
                VoseInstance<PotentialBarrenArea>
            >();

            if (inst == null)
            {
                HashSet<AreaId> baseAreaIds = GetBaseAreaIds(genData, hintSettings);
                Dictionary<AreaId, TreeNode> areaToNode = buildTree(genData, baseAreaIds);
                List<PotentialBarrenArea> potentialBarrenAreas = GetPotentialBarrenAreas(
                    genData,
                    baseAreaIds,
                    areaToNode
                );

                if (potentialBarrenAreas.Count > 0)
                {
                    bool useCategoryWeighting = true;
                    foreach (PotentialBarrenArea pba in potentialBarrenAreas)
                    {
                        if (pba.areaId.type != AreaId.AreaType.Category)
                        {
                            useCategoryWeighting = false;
                            break;
                        }
                    }

                    List<KeyValuePair<double, PotentialBarrenArea>> weightedList = new();
                    foreach (PotentialBarrenArea pba in potentialBarrenAreas)
                    {
                        double weight = pba.GetWeight(useCategoryWeighting);
                        // When hinting zones, if an area has only a single check then significantly
                        // reduce the likelihood that it gets hinted (ex: Snowpeak Mountain with a
                        // BeyondThisPoint hint or Death Mountain).
                        if (!useCategoryWeighting && pba.effectiveUnknownChecksCount < 2)
                            weight *= 0.2;

                        weightedList.Add(new(weight, pba));
                    }

                    inst = VoseAlgorithm.createInstance(weightedList);
                }
            }

            List<Hint> hints = new();

            while (inst != null && inst.HasMore() && hints.Count < numHints)
            {
                PotentialBarrenArea pba = inst.NextAndRemove(genData.rnd);
                AreaId areaId = pba.areaId;

                if (
                    areaId.type == AreaId.AreaType.Zone
                    && hintSettings.dungeons.maxBarrenHints != null
                )
                {
                    int maxBarrenDungeons = (int)hintSettings.dungeons.maxBarrenHints;
                    if (
                        ZoneUtils.IsDungeonZone(areaId.stringId)
                        && genData.hinted.GetNumHintedBarrenDungeons() >= maxBarrenDungeons
                    )
                    {
                        // Skip over selecting the zone if it is a dungeon and
                        // we have already used up our max barrenDungeons hints.
                        continue;
                    }
                }

                // Confirm can afford barren penalties. If unable to, then skips over. Note: root
                // area does not have to be a zone to have zone deps (ex: SouthernDesert => CoO)
                if (barrenPenalizer != null)
                {
                    HashSet<Zone> childZones = genData.GetZoneDeps(areaId);
                    if (!barrenPenalizer(areaId, childZones))
                        continue;
                }

                hints.Add(new BarrenHint(areaId));

                // If zone, mark zone as hinted barren.
                if (areaId.type == AreaId.AreaType.Zone)
                {
                    genData.hinted.hintedBarrenZones.Add(
                        ZoneUtils.StringToIdThrows(areaId.stringId)
                    );
                }

                // Mark barrenable checks as hinted barren.
                foreach (string check in pba.barrenableChecks)
                {
                    // genData.hinted.alreadyCheckKnownBarren.Add(check);
                    genData.hinted.AddHintedBarrenCheck(check);
                }
            }

            cache.PutLatestNodeCache(inst);

            return hints;
        }

        // TODO: can create dependencies at genData level so only have to do it once (will be more
        // involved with ER), since we need it for both Barren hints and "(to name) NumMajorItems
        // hint". And we would have to do it for all zones for ER.

        // continued: for Barren hints, we can only hint a zone as barren if all dependent checks
        // allow us to hint it as barren. If there are dependent zones, then we need to check those
        // and gather the relevant data from them (such as the checks which will be marked "known
        // barren").

        // For NumMajorItems hints, we need to determine the dependent checks (if any), and then we
        // need to determine if they can be hinted barren. This matches the BarrenHintCreator
        // exactly, except we don't care about our own zone technically. Note that if we go through
        // with this hint, it does mark those ones as "known barren" if we say they are barren.

        // For NumMajorItems hints, we only need to include "but it may lead to {something good}" if
        // we were to include "{zone name itself}" since it has inner zones (LLC, CoO, LS, or
        // Agitha). We would also say "itself" if it had post-dungeon checks or dependent golden
        // wolves. Maybe we only include the "but it leads to nothing / may lead to something good"
        // for when Hint Importance is enabled. The "itself" on its own would indicate stuff.

        private List<PotentialBarrenArea> GetPotentialBarrenAreas(
            HintGenData genData,
            HashSet<AreaId> baseAreaIds,
            Dictionary<AreaId, TreeNode> areaToNode
        )
        {
            Dictionary<AreaId, PotentialBarrenArea> pbaDict = new();

            // For each baseAreaId, if already in the tree, then go up tree until find result and
            // add its pba to the resultDict (ignoring if already there; ex: checks for ND and SD
            // will both say "no, do GD instead" which is fine). If result is null for a node, then
            // it is not valid. We should keep the highest up one which has a non-null pba.

            // If not in the tree, then simply do the calc. If non-null result, then can add pba.

            foreach (AreaId areaId in baseAreaIds)
            {
                // Skip since already present. Can happen when a lower tree node (such as CoO)
                // resolves to a higher node (such as GD), and then we later check for GD.
                if (pbaDict.ContainsKey(areaId))
                    continue;

                // If in tree, resolve that way.
                if (areaToNode.TryGetValue(areaId, out TreeNode currNode))
                {
                    TreeNode resultNode = null;
                    while (currNode != null)
                    {
                        if (currNode.pba != null)
                            resultNode = currNode;

                        currNode = currNode.parent;
                    }

                    if (resultNode != null)
                        pbaDict[resultNode.areaId] = resultNode.pba;
                    continue;
                }

                // For not in tree:
                PotentialBarrenArea pba = tryGenPba(genData, areaId);
                if (pba != null)
                    pbaDict[areaId] = pba;
            }

            int matches = 7;

            return new(pbaDict.Values);
        }

        private PotentialBarrenArea tryGenPba(HintGenData genData, AreaId areaId)
        {
            HashSet<string> checkNames = recursiveGetAreaAndDepsChecks(genData, areaId);

            // TODO: Other info to get:

            // - does it have any relevant (not vanilla or excluded) dependent checks. For example,
            //   we do not want to say "{Goron Mines itself}" if post-dungeon checks are excluded.

            // - also we want to know the majorItems and importantItems (if possible) counts.

            // TODO: since we want Vanilla checks to preventBarren now, we should include "itself"
            // if there are Vanilla checks in dependent areas as well (however, we can ignore ones
            // which are vanilla and not Major such as post-GM Poes). But if post-ToT ones are all
            // excluded or Vanilla, we still need to say "itself" if the vanilla ones include major
            // items such as Ilia quest items in HV. This "itself" stuff is for the IC hint.

            List<string> barrenableChecks = new();
            bool areaCanBeHintedBarren = true;
            int numUnknownChecks = 0;
            int numUnknownAllowBarrenChecks = 0;
            int extraWeighting = 0;

            foreach (string checkName in checkNames)
            {
                if (HintUtils.checkIsExcluded(checkName))
                    continue;

                // Note: we intentionally do not skip over Vanilla checks since it led to confusion,
                // especially for the Ilia memory quest. Since Ilia's Charm is perfectly a tradeItem
                // (purely a proxy like Ashei's sketch / golden bugs), then it would not prevent
                // barren when the "Ilia Memory Reward" is not major which is a nice bonus (even
                // without importance calcs since tradeItems which end in nothing are marked
                // notRequired). Even if it always prevented HV from being hinted barren (without
                // importance calcs), we would still want to do this to avoid "lying" to the player.

                // Note: Unreachable and hidden are already skipped over by the AreaCheckInfos at a
                // high level, but we are no longer skipping over Vanilla checks for the reasons
                // above. Since these checks are considered "checkIsPlayerKnownStatus" though, they
                // do not count toward the barren weighting of an area, and if the entire area was
                // only vanilla/excluded/knownPlando, it would not be a candidate for a barren hint
                // since the numUnknownChecks would be 0.

                Item contents = HintUtils.getCheckContents(checkName);
                bool itemAllowsBarrenForArea = genData.ItemAllowsBarrenForArea(contents, areaId);

                // Important: if an already hinted check (such as self-hinted Charlo) is GOOD, then
                // we cannot hint the area as barren, even if that check is not an unknown check.
                // Therefore we need to do this check before worrying about unknown vs not checks.
                if (!itemAllowsBarrenForArea && genData.CheckWouldPreventBarren(checkName))
                {
                    // Area can still be hinted barren for certain checks which are technically
                    // important/good but which should not actually prevent barren. For example, LBT
                    // small keys when the area is LBT and small keys are set to ownDungeon or when
                    // FusedShadows do not appear anywhere.
                    areaCanBeHintedBarren = false;
                    break;
                }

                // If the area is hinted barren, non-skipped checks should still be added to
                // `alreadyCheckKnownBarren` even if they are not considered 'unknown' for
                // determining if it is useful to hint the area as barren.
                barrenableChecks.Add(checkName);

                if (CheckIsUnknownStatus(genData, checkName))
                {
                    numUnknownChecks += 1;
                    if (itemAllowsBarrenForArea)
                    {
                        // We want to count these up front since they might not be hard-required
                        // (therefore not important/good), but we still want them to factor in to
                        // whether or not an area can be hinted barren. (HC with 4 keys where some
                        // of the key checks are either-or, meaning not all technically required).
                        // Also used for area weighting.
                        numUnknownAllowBarrenChecks += 1;
                    }
                }
                else if (genData.hinted.IsIgnoreCheckForBarrenWeighting(checkName))
                {
                    // Even though NothingBeyond hints are calculated ahead of time, we still
                    // include checks which were hinted barren this way into consideration when
                    // handling weighting. This is so we do not significantly reduce the effective
                    // size of LLC and some dungeons for barren hint calculation.
                    // extraWeighting += 1;

                    // TODO: temp setting this to do nothing since we really don't want this for
                    // Snowpeak Mountain when all it does it tell us Ashei is dead. Arguably the
                    // weight changes to the other BTP zones is how it should work anyway. Haven't
                    // removed the code yet so don't have to rewrite it in case we want to enable
                    // again.
                    extraWeighting += 0;
                }
            }

            // `numUnknownAllowBarrenChecks` is to avoid things like hinting Hyrule Castle barren
            // when you have HC with only 4 checks not excluded which are always small keys and the
            // big key. For most cases, `numUnknownAllowBarrenChecks` will be 0 meaning we want to
            // make sure there is at least 1 unknown check.
            if (areaCanBeHintedBarren && numUnknownChecks > numUnknownAllowBarrenChecks)
            {
                int effectiveUnknownChecksCount =
                    numUnknownChecks - numUnknownAllowBarrenChecks + extraWeighting;

                return new(areaId, barrenableChecks, effectiveUnknownChecksCount);
            }
            return null;
        }

        private List<string> ResolveBossAndPostDungeonChecks(Zone zone)
        {
            if (!dungeonZoneToRegionName.TryGetValue(zone, out string dungeonRegionName))
                throw new Exception($"Failed to find dungeonRegionName for Zone '{zone}'.");

            // Given a dungeon, we need to scan through the boss rooms until we
            // find the one that is hooked up to the end of the dungeon.
            foreach (string bossRoom in bossRooms)
            {
                Room bRoom = Randomizer.Rooms.RoomDict[bossRoom];
                if (bRoom.Region == dungeonRegionName)
                {
                    // Add bossRoom checks to result as well as any post-dungeon
                    // checks based on the boss (ex: post-Fyrus checks block
                    // barren for FT if Fyrus is randomized to be the boss at
                    // the end of FT).
                    List<string> result = new(bRoom.Checks);
                    switch (bossRoom)
                    {
                        case "Goron Mines Boss Room":
                            result.AddRange(CheckFunctions.postFyrusChecks);
                            break;
                        case "Snowpeak Ruins Boss Room":
                            result.AddRange(CheckFunctions.postBlizettaChecks);
                            break;
                        case "Temple of Time Boss Room":
                            result.AddRange(CheckFunctions.postArmogohmaChecks);
                            break;
                    }
                    return result;
                }
            }

            throw new Exception($"Failed to find bossRoom for Zone '${zone}'.");
        }

        private HashSet<AreaId> GetBaseAreaIds(HintGenData genData, HintSettings hintSettings)
        {
            HashSet<AreaId> result;

            if (validAreas != null)
            {
                // Allow user to specify empty set.
                result = validAreas;
            }
            else
            {
                // Pick default if not specified
                result = new();

                switch (areaType)
                {
                    case AreaId.AreaType.Zone:
                    {
                        // Pick all valid zones
                        foreach (KeyValuePair<string, string[]> pair in ZoneUtils.zoneNameToChecks)
                        {
                            result.Add(AreaId.ZoneStr(pair.Key));
                        }
                        break;
                    }
                    case AreaId.AreaType.Category:
                    {
                        foreach (HintCategory category in defaultHintCategories)
                        {
                            result.Add(AreaId.Category(category));
                        }
                        break;
                    }
                    default:
                        throw new Exception(
                            $"Failed to provide default baseAreaIds for areaType '{areaType}'."
                        );
                }
            }

            if (result.Count > 0)
            {
                // Always remove Agitha if Agitha hints are on.
                if (hintSettings.agitha)
                    result.Remove(AreaId.Zone(Zone.Agithas_Castle));

                // Remove CoO if CoO hints are on.
                if (hintSettings.caveOfOrdeals)
                    result.Remove(AreaId.Zone(Zone.Cave_of_Ordeals));

                // Remove any unrequiredBarren dungeons
                if (genData.sSettings.barrenDungeons)
                {
                    foreach (
                        KeyValuePair<string, byte> kv in HintConstants.dungeonZonesToRequiredMaskMap
                    )
                    {
                        string zoneName = kv.Key;
                        if (!HintUtils.DungeonIsRequired(zoneName))
                            result.Remove(AreaId.ZoneStr(zoneName));
                    }
                }

                // Filter out any areas which have either been IC-hinted or zones which have been
                // hinted barren.
                List<AreaId> areaIdsList = result.ToList();
                foreach (AreaId areaId in areaIdsList)
                {
                    if (genData.hinted.hintedImportanceCountAreas.Contains(areaId))
                    {
                        result.Remove(areaId);
                    }
                    else if (areaId.type == AreaId.AreaType.Zone)
                    {
                        Zone zone = ZoneUtils.StringToIdThrows(areaId.stringId);
                        if (genData.hinted.hintedBarrenZones.Contains(zone))
                            result.Remove(areaId);
                    }
                }
            }

            return result;
        }

        private static bool CheckIsUnknownStatus(HintGenData genData, string checkName)
        {
            return (
                !genData.checkIsPlayerKnownStatus(checkName)
                && !genData.hinted.alwaysHintedChecks.Contains(checkName)
                && !genData.hinted.alreadyCheckAgithaHintClaimed.Contains(checkName)
                && !genData.hinted.alreadyCheckKnownBarren.Contains(checkName)
                && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
            );
        }

        private class PotentialBarrenArea
        {
            public AreaId areaId;
            public List<string> barrenableChecks;
            public int effectiveUnknownChecksCount;

            public PotentialBarrenArea(
                AreaId areaId,
                List<string> barrenableChecks,
                int effectiveUnknownChecksCount
            )
            {
                this.areaId = areaId;
                this.barrenableChecks = barrenableChecks;
                // Not expected to ever throw here since should not be
                // attempting to create this object under these conditions.
                if (effectiveUnknownChecksCount <= 0)
                    throw new Exception(
                        $"Expected effectiveUnknownChecksCount to be more than 0, but area '{areaId.stringId}' had a value of '{effectiveUnknownChecksCount}'."
                    );
                this.effectiveUnknownChecksCount = effectiveUnknownChecksCount;
            }

            public double GetWeight(bool useCategoryWeighting)
            {
                // Use Sqrt for Category since we only have a slight preference for larger
                // categories. If zones are involved, we have a huge preference for larger zones, so
                // we use the raw value in that case.
                if (useCategoryWeighting)
                    return Math.Sqrt(effectiveUnknownChecksCount);
                return effectiveUnknownChecksCount;
            }
        }

        private static HashSet<AreaId> resolveAreaAlias(string alias)
        {
            HashSet<AreaId> result = new();

            switch (alias.ToLowerInvariant())
            {
                case "overworldzones":
                {
                    HashSet<Zone> overworldZones =
                        new()
                        {
                            Zone.Ordon,
                            Zone.Sacred_Grove,
                            Zone.Faron_Field,
                            Zone.Faron_Woods,
                            Zone.Kakariko_Gorge,
                            Zone.Kakariko_Village,
                            Zone.Kakariko_Graveyard,
                            Zone.Eldin_Field,
                            Zone.North_Eldin,
                            Zone.Death_Mountain,
                            Zone.Hidden_Village,
                            Zone.Lanayru_Field,
                            Zone.Beside_Castle_Town,
                            Zone.South_of_Castle_Town,
                            Zone.Castle_Town,
                            Zone.Agithas_Castle,
                            Zone.Great_Bridge_of_Hylia,
                            Zone.Lake_Hylia,
                            Zone.Lake_Lantern_Cave,
                            Zone.Lanayru_Spring,
                            Zone.Zoras_Domain,
                            Zone.Upper_Zoras_River,
                            Zone.Gerudo_Desert,
                            Zone.Bulblin_Camp,
                            Zone.Snowpeak_Mountain,
                            Zone.Cave_of_Ordeals,
                        };

                    foreach (Zone zone in overworldZones)
                    {
                        result.Add(AreaId.Zone(zone));
                    }
                    break;
                }
                default:
                    throw new Exception($"Failed to resolve alias '{alias}'.");
            }
            return result;
        }

        private Dictionary<AreaId, TreeNode> buildTree(
            HintGenData genData,
            HashSet<AreaId> baseAreaIds
        )
        {
            Dictionary<AreaId, TreeNode> areaToNode = new();

            List<(AreaId, AreaId)> list = new();

            AreaId gdAreaId = AreaId.Zone(Zone.Gerudo_Desert);
            list.Add((gdAreaId, AreaId.Category(HintCategory.Northern_Desert)));
            list.Add((gdAreaId, AreaId.Category(HintCategory.Southern_Desert)));

            foreach (KeyValuePair<AreaId, AreaCheckInfo> pair in genData.areaToCheckInfo)
            {
                AreaId areaId = pair.Key;
                AreaCheckInfo areaCheckInfo = pair.Value;

                // Skip over GD since we handle manually above
                if (areaId.Equals(gdAreaId))
                    continue;

                foreach (AreaId depAreaId in areaCheckInfo.dependentAreaIds)
                {
                    list.Add((areaId, depAreaId));
                }
            }

            foreach ((AreaId, AreaId) pair in list)
            {
                AreaId parent = pair.Item1;
                AreaId child = pair.Item2;

                if (!areaToNode.TryGetValue(parent, out TreeNode parentNode))
                {
                    parentNode = new TreeNode(parent);
                    areaToNode[parent] = parentNode;
                }
                if (!areaToNode.TryGetValue(child, out TreeNode childNode))
                {
                    childNode = new TreeNode(child);
                    areaToNode[child] = childNode;
                }

                if (childNode.parent != null)
                    throw new Exception($"Expected null parentNode.");

                childNode.parent = parentNode;
                parentNode.children.Add(childNode);
            }

            // Once the tree is built, any nodes which do not have a parent are root nodes. Does
            // this matter though?

            // Next what we need to do is start at the root node and drill down and d

            // We can iterate through the baseAreaIds. For each one, if we don't already have a PBA
            // for it (or null if not possible), then we calculate for it and store the result. If
            // one has a TreeNode, then we iterate up the tree and create a PBA for each node which
            // shows up in the baseAreaIds (until we run into no more praent nodes)?

            // Actually, let's just make it simple. We can gather the full checks for the AreaId as
            // a HashSet. It does not matter if the check is in the area or not? Well it does for
            // dungeon items. But dungeons are never children, so we can ignore that.

            // Get checks for zone, then get checks for any checkDeps and add.
            // Then iterate to recursively get checks for areaDeps and add.

            foreach (TreeNode node in areaToNode.Values)
            {
                if (baseAreaIds.Contains(node.areaId))
                {
                    PotentialBarrenArea pba = tryGenPba(genData, node.areaId);
                    node.pba = pba;
                }
            }
            return areaToNode;
        }

        private HashSet<string> recursiveGetAreaAndDepsChecks(HintGenData genData, AreaId areaId)
        {
            HashSet<string> result = new();

            AreaCheckInfo areaCheckInfo = genData.GetAreaCheckInfoThrows(areaId);

            result.UnionWith(areaCheckInfo.fullCheckNames);
            result.UnionWith(areaCheckInfo.dependentCheckNames);
            foreach (AreaId depAreaId in areaCheckInfo.dependentAreaIds)
            {
                result.UnionWith(recursiveGetAreaAndDepsChecks(genData, depAreaId));
            }
            return result;
        }

        private class TreeNode
        {
            public AreaId areaId;
            public TreeNode parent;
            public List<TreeNode> children = new();
            public PotentialBarrenArea pba;

            public TreeNode(AreaId areaId)
            {
                this.areaId = areaId;
            }
        }
    }
}
