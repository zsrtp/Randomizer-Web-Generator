namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class EntranceHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Entrance;

        private static readonly HashSet<Zone> dungeonZones =
            new()
            {
                Zone.Forest_Temple,
                Zone.Goron_Mines,
                Zone.Lakebed_Temple,
                Zone.Arbiters_Grounds,
                Zone.Snowpeak_Ruins,
                Zone.Temple_of_Time,
                Zone.City_in_the_Sky,
                Zone.Palace_of_Twilight,
                Zone.Hyrule_Castle,
            };

        private bool ignoreAlreadyHinted = false;
        private HashSet<Zone> validSourceZones = null;
        private HashSet<Zone> validDestinationZones = null;

        private EntranceHintCreator() { }

        new public static EntranceHintCreator fromJObject(JObject obj)
        {
            EntranceHintCreator inst = new EntranceHintCreator();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                inst.ignoreAlreadyHinted = HintSettingUtils.getOptionalBool(
                    options,
                    "ignoreAlreadyHinted",
                    inst.ignoreAlreadyHinted
                );

                List<string> validSrcZonesStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validSourceZones",
                    null
                );
                if (!ListUtils.isEmpty(validSrcZonesStrList))
                {
                    inst.validSourceZones = new();

                    foreach (string zoneStr in validSrcZonesStrList)
                    {
                        Zone zone = ZoneUtils.StringToIdThrows(zoneStr);
                        inst.validSourceZones.Add(zone);
                    }
                }

                List<string> validDestZonesStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validDestinationZones",
                    null
                );
                if (!ListUtils.isEmpty(validDestZonesStrList))
                {
                    inst.validDestinationZones = new();

                    foreach (string zoneStr in validDestZonesStrList)
                    {
                        Zone zone = ZoneUtils.StringToIdThrows(zoneStr);
                        inst.validDestinationZones.Add(zone);
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
            HashSet<Zone> potentialZonesToHint = new();

            HashSet<Zone> allowedSourceZones = getAllowedSourceZones();
            HashSet<Zone> destZonesOfInterest = getDestZonesOfInterest(genData);
            foreach (KeyValuePair<Zone, HashSet<Zone>> pair in genData.dungeonEntrances)
            {
                Zone sourceZone = pair.Key;
                HashSet<Zone> destZones = pair.Value;
                if (!allowedSourceZones.Contains(sourceZone))
                    continue;

                bool hasDestOfInterest = false;
                foreach (Zone destZoneOfInterest in destZonesOfInterest)
                {
                    if (destZones.Contains(destZoneOfInterest))
                    {
                        hasDestOfInterest = true;
                        break;
                    }
                }

                if (hasDestOfInterest && EntranceZoneIsPossibleToHint(genData, sourceZone))
                    potentialZonesToHint.Add(sourceZone);
            }

            List<Hint> results = new();

            // TODO: Create weights based on how difficult each dungeon is to enter.

            for (int i = 0; i < numHints && potentialZonesToHint.Count > 0; i++)
            {
                Zone entranceZoneToHint = HintUtils.RemoveRandomHashSetItem(
                    genData.rnd,
                    potentialZonesToHint
                );

                EntranceHint hint = EntranceHint.CreateFromDungeonEntrance(
                    genData,
                    entranceZoneToHint
                );
                results.Add(hint);

                // Update hinted
                genData.hinted.hintedDungeonEntranceSources.Add(entranceZoneToHint);
            }

            return results;
        }

        private HashSet<Zone> getAllowedSourceZones()
        {
            if (validSourceZones != null)
                return new(validSourceZones);
            return dungeonZones;
        }

        private HashSet<Zone> getDestZonesOfInterest(HintGenData genData)
        {
            // HashSet<Zone> result = new();

            HashSet<Zone> interestedZones;

            if (validDestinationZones != null)
            {
                interestedZones = new(validDestinationZones);
            }
            else
            {
                // If specified source zones and not destination zones, then allow all dungeon
                // zones as destinations.
                if (validSourceZones != null && validDestinationZones == null)
                    return dungeonZones;

                interestedZones = new() { Zone.Hyrule_Castle };

                if (genData.sSettings.barrenDungeons)
                {
                    HashSet<string> requiredDungeonZones = HintUtils.getRequiredDungeonZones();
                    foreach (string zoneName in requiredDungeonZones)
                    {
                        interestedZones.Add(ZoneUtils.StringToIdThrows(zoneName));
                    }
                }
                else
                {
                    interestedZones.UnionWith(dungeonZones);
                }
            }

            // TODO: maybe should split up options so can provide list of ones interested in, but it
            // still pays attention to required dungeons. We also need to support where we specify a
            // destination, but we don't care if it is required or not.

            // foreach (Zone destZone in validDestinationZones)
            // {
            //     if (interestedZones.Contains(destZone))
            //         result.Add(destZone);
            // }
            // return result;

            return interestedZones;
        }

        private bool EntranceZoneIsPossibleToHint(HintGenData genData, Zone entranceZone)
        {
            return ignoreAlreadyHinted
                || !genData.hinted.hintedDungeonEntranceSources.Contains(entranceZone);
        }
    }
}
