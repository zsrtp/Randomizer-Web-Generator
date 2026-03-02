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

        private static readonly HashSet<Zone> defaultValidZones =
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

        private HashSet<Zone> validDestinationZones = null;

        private EntranceHintCreator() { }

        new public static EntranceHintCreator fromJObject(JObject obj)
        {
            EntranceHintCreator inst = new EntranceHintCreator();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

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

            if (inst.validDestinationZones == null)
                inst.validDestinationZones = defaultValidZones;

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

            HashSet<Zone> destZonesOfInterest = getDestZonesOfInterest(genData);
            foreach (KeyValuePair<Zone, HashSet<Zone>> pair in genData.dungeonEntrances)
            {
                Zone sourceZone = pair.Key;
                HashSet<Zone> destZones = pair.Value;

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

        private HashSet<Zone> getDestZonesOfInterest(HintGenData genData)
        {
            HashSet<Zone> result = new();

            HashSet<Zone> interestedZones = new() { Zone.Hyrule_Castle };

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
                interestedZones.UnionWith(defaultValidZones);
            }

            foreach (Zone destZone in validDestinationZones)
            {
                if (interestedZones.Contains(destZone))
                    result.Add(destZone);
            }

            return result;
        }

        private bool EntranceZoneIsPossibleToHint(HintGenData genData, Zone entranceZone)
        {
            return !genData.hinted.hintedDungeonEntranceSources.Contains(entranceZone);
        }
    }
}
