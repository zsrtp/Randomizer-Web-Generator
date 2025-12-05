namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks.Dataflow;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class ImportanceCountHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.ImportanceCount;

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

        private static readonly HashSet<AreaId.AreaType> validAreaTypes =
            new() { AreaId.AreaType.Zone, AreaId.AreaType.Category, };

        private AreaId.AreaType areaType = AreaId.AreaType.Zone;
        private HashSet<AreaId> validAreas = null;

        private ImportanceCountHintCreator() { }

        new public static ImportanceCountHintCreator fromJObject(JObject obj)
        {
            ImportanceCountHintCreator inst = new ImportanceCountHintCreator();

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
            HintGenCache cache,
            BarrenPenalizer barrenPenalizer
        )
        {
            HashSet<AreaId> baseAreaIds = GetBaseAreaIds(genData, hintSettings);
            List<PotentialIcArea> potentialIcAreas = GetPotentialIcAreas(genData, baseAreaIds);

            if (potentialIcAreas.Count < 1)
                return null;

            List<KeyValuePair<double, PotentialIcArea>> weightedList = new();
            foreach (PotentialIcArea pba in potentialIcAreas)
            {
                double weight = Math.Log(pba.effectiveUnknownChecksCount);
                weightedList.Add(new(weight, pba));
            }

            VoseInstance<PotentialIcArea> voseInst = VoseAlgorithm.createInstance(weightedList);

            List<Hint> hints = new();

            while (voseInst.HasMore() && hints.Count < numHints)
            {
                PotentialIcArea pia = voseInst.NextAndRemove(genData.rnd);

                hints.Add(new ImportanceCountHint(pia));

                genData.hinted.hintedImportanceCountAreas.Add(pia.areaId);
            }

            return hints;
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

        private List<PotentialIcArea> GetPotentialIcAreas(
            HintGenData genData,
            HashSet<AreaId> baseAreaIds
        )
        {
            List<PotentialIcArea> ret = new();

            foreach (AreaId areaId in baseAreaIds)
            {
                // Skip over if zone already hinted barren.
                if (areaId.type == AreaId.AreaType.Zone)
                {
                    Zone zone = ZoneUtils.StringToIdThrows(areaId.stringId);
                    if (genData.hinted.hintedBarrenZones.Contains(zone))
                        continue;
                }

                PotentialIcArea pia = tryGenPia(genData, areaId);
                if (pia != null)
                    ret.Add(pia);
            }
            return ret;
        }

        private PotentialIcArea tryGenPia(HintGenData genData, AreaId areaId)
        {
            HashSet<string> checkNames = recursiveGetAreaAndDepsChecks(genData, areaId);

            AreaCheckInfo areaCheckInfo = genData.GetAreaCheckInfoThrows(areaId);
            HashSet<string> ownAreaCheckNames = areaCheckInfo.fullCheckNames;

            int numUnknownChecks = 0;
            int numUnknownAllowBarrenChecks = 0;
            PotentialIcArea pia = new(areaId, genData.didCondReqCalc);

            foreach (string checkName in checkNames)
            {
                // Skip over Vanilla, Excluded, and Excluded-Unrequired. Unreachable and hidden are
                // already skipped over by the AreaCheckInfos at a high level.
                if (HintUtils.checkIsPlayerKnownStatus(checkName))
                    continue;

                if (!ownAreaCheckNames.Contains(checkName))
                {
                    // We know we have dependent checks now, but these do not actually get included
                    // in any counts.
                    pia.hasRelevantDependentChecks = true;
                    continue;
                }

                Item contents = HintUtils.getCheckContents(checkName);

                if (genData.majorItems.Contains(contents))
                {
                    if (genData.didCondReqCalc)
                    {
                        DetailedCheckStatus status = genData.CalcDetailedCheckStatus(checkName);
                        if (status != DetailedCheckStatus.NotRequired)
                            pia.importantChecks.Add(checkName);
                        else
                            pia.majorChecks.Add(checkName);
                    }
                    else
                        pia.majorChecks.Add(checkName);
                }

                if (CheckIsUnknownStatus(genData, checkName))
                {
                    numUnknownChecks += 1;
                    bool itemAllowsBarrenForArea = genData.ItemAllowsBarrenForArea(
                        contents,
                        areaId
                    );
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
            int effectiveUnknownChecksCount = numUnknownChecks - numUnknownAllowBarrenChecks;
            if (effectiveUnknownChecksCount > 0)
            {
                pia.effectiveUnknownChecksCount = effectiveUnknownChecksCount;
                return pia;

                // return new(areaId, barrenableChecks, effectiveUnknownChecksCount);
            }
            return null;
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

        private static bool CheckIsUnknownStatus(HintGenData genData, string checkName)
        {
            return (
                !HintUtils.checkIsPlayerKnownStatus(checkName)
                && !genData.hinted.alwaysHintedChecks.Contains(checkName)
                && !genData.hinted.alreadyCheckAgithaHintClaimed.Contains(checkName)
                && !genData.hinted.alreadyCheckKnownBarren.Contains(checkName)
                && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
            );
        }
    }

    public class PotentialIcArea
    {
        public AreaId areaId { get; private set; }
        public bool hasRelevantDependentChecks;
        public HashSet<string> importantChecks = new();
        public HashSet<string> majorChecks = new();
        public bool indicatesImportant { get; private set; }
        public int effectiveUnknownChecksCount;

        public PotentialIcArea(AreaId areaId, bool indicatesImportant)
        {
            this.areaId = areaId;
            this.indicatesImportant = indicatesImportant;
        }
    }
}
