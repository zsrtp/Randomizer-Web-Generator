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

            return new(pbaDict.Values);
        }

        private PotentialBarrenArea tryGenPba(HintGenData genData, AreaId areaId)
        {
            HashSet<string> checkNames = recursiveGetAreaAndDepsChecks(genData, areaId);

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
