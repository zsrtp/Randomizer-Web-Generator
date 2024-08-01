namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
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

        // Includes post-dungeon checks, etc.
        private static readonly Dictionary<Zone, List<string>> dungeonZoneToReqChecks =
            new()
            {
                { Zone.Forest_Temple, CheckFunctions.forestRequirementChecks },
                { Zone.Goron_Mines, CheckFunctions.minesRequirementChecks },
                { Zone.Lakebed_Temple, CheckFunctions.lakebedRequirementChecks },
                { Zone.Arbiters_Grounds, CheckFunctions.arbitersRequirementChecks },
                { Zone.Snowpeak_Ruins, CheckFunctions.snowpeakRequirementChecks },
                { Zone.Temple_of_Time, CheckFunctions.totRequirementChecks },
                { Zone.City_in_the_Sky, CheckFunctions.cityRequirementChecks },
                { Zone.Palace_of_Twilight, CheckFunctions.palaceRequirementChecks },
            };

        private static readonly HashSet<AreaId.AreaType> validAreaTypes =
            new() { AreaId.AreaType.Zone, AreaId.AreaType.Province, AreaId.AreaType.Category, };

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
                        AreaId areaId = AreaId.ParseString(inst.areaType, validAreaStr);
                        inst.validAreas.Add(areaId);
                    }
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

            VoseInstance<PotentialBarrenArea> inst = cache.GetFromLatestNodeCache<
                VoseInstance<PotentialBarrenArea>
            >();

            if (inst == null)
            {
                HashSet<AreaId> baseAreaIds = GetBaseAreaIds(genData, hintSettings);
                List<PotentialBarrenArea> potentialBarrenAreas = GetPotentialBarrenAreas(
                    genData,
                    baseAreaIds
                );

                if (potentialBarrenAreas.Count > 0)
                {
                    List<KeyValuePair<double, PotentialBarrenArea>> weightedList = new();
                    foreach (PotentialBarrenArea pba in potentialBarrenAreas)
                    {
                        weightedList.Add(new(pba.GetWeight(), pba));
                    }

                    inst = VoseAlgorithm.createInstance(weightedList);
                }
            }

            // List<PotentialBarrenArea> selectedAreas = new();
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
            HashSet<AreaId> baseAreaIds
        )
        {
            List<PotentialBarrenArea> potentialBarrenAreas = new();

            foreach (AreaId areaId in baseAreaIds)
            {
                HashSet<string> checkNames = ResolveAreaIdToChecks(areaId);

                List<string> barrenableChecks = new();
                bool areaCanBeHintedBarren = true;
                int numUnknownChecks = 0;
                int numUnknownAllowBarrenChecks = 0;
                int extraWeighting = 0;

                foreach (string checkName in checkNames)
                {
                    if (IsSkipOverCheck(genData, checkName))
                        continue;

                    Item contents = HintUtils.getCheckContents(checkName);
                    bool itemAllowsBarrenForArea = genData.ItemAllowsBarrenForArea(
                        contents,
                        areaId
                    );

                    // Important: if an already hinted check (such as
                    // self-hinted Charlo) is GOOD, then we cannot hint the area
                    // as barren, even if that check is not an unknown check.
                    // Therefore we need to do this check before worrying about
                    // unknown vs not checks.
                    if (genData.CheckIsGood(checkName) && !itemAllowsBarrenForArea)
                    {
                        // Area can still be hinted barren for certain checks
                        // which are technically important/good but which should
                        // not actually prevent barren. For example, LBT small
                        // keys when the area is LBT and small keys are set to
                        // ownDungeon or when FusedShadows do not appear
                        // anywhere.
                        areaCanBeHintedBarren = false;
                        break;
                    }

                    // If the area is hinted barren, non-skipped checks should
                    // still be added to `alreadyCheckKnownBarren` even if they
                    // are not considered 'unknown' for determining if it is
                    // useful to hint the area as barren.
                    barrenableChecks.Add(checkName);

                    if (CheckIsUnknownStatus(genData, checkName))
                    {
                        numUnknownChecks += 1;
                        if (itemAllowsBarrenForArea)
                        {
                            // We want to count these up front since they might
                            // not be hard-required (therefore not
                            // important/good), but we still want them to factor
                            // in to whether or not an area can be hinted
                            // barren. (HC with 4 keys where some of the key
                            // checks are either-or, meaning not all technically
                            // required). Also used for area weighting.
                            numUnknownAllowBarrenChecks += 1;
                        }
                    }
                    else if (genData.hinted.IsIgnoreCheckForBarrenWeighting(checkName))
                    {
                        // Even though NothingBeyond hints are calculated ahead
                        // of time, we still include checks which were hinted
                        // barren this way into consideration when handling
                        // weighting. This is so we do not significantly reduce
                        // the effective size of LLC and some dungeons for
                        // barren hint calculation.
                        extraWeighting += 1;
                    }
                }

                // `numUnknownAllowBarrenChecks` is to avoid things like hinting
                // Hyrule Castle barren when you have HC with only 4 checks not
                // excluded which are always small keys and the big key. For
                // most cases, `numUnknownAllowBarrenChecks` will be 0 meaning
                // we want to make sure there is at least 1 unknown check.
                if (areaCanBeHintedBarren && numUnknownChecks > numUnknownAllowBarrenChecks)
                {
                    int effectiveUnknownChecksCount =
                        numUnknownChecks - numUnknownAllowBarrenChecks + extraWeighting;

                    potentialBarrenAreas.Add(
                        new(areaId, barrenableChecks, effectiveUnknownChecksCount)
                    );
                }
            }

            return potentialBarrenAreas;
        }

        private HashSet<string> ResolveAreaIdToChecks(AreaId areaId)
        {
            if (areaId.type == AreaId.AreaType.Zone)
            {
                Zone zone = ZoneUtils.StringToId(areaId.stringId);
                if (dungeonZoneToReqChecks.TryGetValue(zone, out List<string> checksList))
                {
                    return new(checksList);
                }
            }

            return areaId.ResolveToChecks();
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
                        Dictionary<string, string[]> zoneToChecks =
                            HintUtils.getHintZoneToChecksMap();
                        foreach (KeyValuePair<string, string[]> pair in zoneToChecks)
                        {
                            result.Add(AreaId.ZoneStr(pair.Key));
                        }
                        break;
                    }
                    case AreaId.AreaType.Province:
                    {
                        // Pick all valid provinces
                        HashSet<string> provinces = ProvinceUtils.GetProvinceNames();
                        foreach (string provinceStr in provinces)
                        {
                            result.Add(AreaId.ProvinceStr(provinceStr));
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

            if (areaType == AreaId.AreaType.Zone && result.Count > 0)
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
            }

            // Validate all of the areaIds line up with the areaType.
            foreach (AreaId areaId in result)
            {
                if (areaId.type != areaType)
                    throw new Exception(
                        $"When getting baseAreaIds, at least one areaId had areaType '{areaId.type}', but expected '{areaType}'."
                    );
            }

            return result;
        }

        public bool HintsZone()
        {
            return areaType == AreaId.AreaType.Zone;
        }

        private static bool CheckIsUnknownStatus(HintGenData genData, string checkName)
        {
            return (
                !HintUtils.checkIsPlayerKnownStatus(checkName)
                && !genData.hinted.alwaysHintedChecks.Contains(checkName)
                && !genData.hinted.hintsShouldIgnoreChecks.Contains(checkName)
                && !genData.hinted.alreadyCheckKnownBarren.Contains(checkName)
                && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
            );
        }

        private static bool IsSkipOverCheck(HintGenData genData, string checkName)
        {
            // We intentionally do NOT skip over Always checks. Consider this
            // case: your average joe finds a hint that Eldin Field is Barren,
            // so they mark off 100% of the Eldin Field checks. Now assume that
            // the Always-hinted Goron Springwater Rush check rewards a required
            // Clawshot. If this player does not find the Always hint for this
            // check, then they may run out of checks to do and get confused.
            // The trade-off is that finding a hint about Goron Springwater Rush
            // when you already know it is unimportant is kind of a waste, but
            // it is better than people getting confused.
            return (
                HintUtils.checkIsPlayerKnownStatus(checkName)
                || genData.hinted.hintsShouldIgnoreChecks.Contains(checkName)
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

            public double GetWeight()
            {
                // Use Sqrt for Category since we only have a slight preference
                // for larger categories. For zones, we have a huge preference
                // for larger zones, so we use the raw value in that case.
                if (areaId.type == AreaId.AreaType.Category)
                    return Math.Sqrt(effectiveUnknownChecksCount);
                return effectiveUnknownChecksCount;
            }
        }
    }
}
