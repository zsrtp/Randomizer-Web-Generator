namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints.HintCreator;
    using TPRandomizer.Util;

    public enum Province
    {
        Invalid = 0,
        Ordona = 1,
        Faron = 2,
        Eldin = 3,
        Lanayru = 4,
        Desert = 5,
        Peak = 6,

        // Split Province is the zones that are in multiple provinces:
        // Golden_Wolf and Long_Minigames.
        Split = 7,
        Dungeon = 8,
    }

    public class ProvinceUtils
    {
        public static readonly byte NumBitsToEncode = 4;
        private static Dictionary<Province, string> enumToStr;
        private static readonly Dictionary<Province, HashSet<Zone>> provinceToZones =
            new()
            {
                {
                    Province.Ordona,
                    new() { Zone.Ordon }
                },
                {
                    Province.Faron,
                    new() { Zone.Faron_Field, Zone.Faron_Woods, Zone.Sacred_Grove }
                },
                {
                    Province.Eldin,
                    new()
                    {
                        Zone.Death_Mountain,
                        Zone.Eldin_Field,
                        Zone.Hidden_Village,
                        Zone.Kakariko_Gorge,
                        Zone.Kakariko_Graveyard,
                        Zone.Kakariko_Village,
                        Zone.North_Eldin,
                    }
                },
                {
                    Province.Lanayru,
                    new()
                    {
                        Zone.Agithas_Castle,
                        Zone.Castle_Town,
                        Zone.Great_Bridge_of_Hylia,
                        Zone.Lake_Hylia,
                        Zone.Lake_Lantern_Cave,
                        Zone.Lanayru_Field,
                        Zone.Lanayru_Spring,
                        Zone.South_of_Castle_Town,
                        Zone.Upper_Zoras_River,
                        Zone.Beside_Castle_Town,
                        Zone.Zoras_Domain,
                    }
                },
                {
                    Province.Desert,
                    new() { Zone.Bulblin_Camp, Zone.Cave_of_Ordeals, Zone.Gerudo_Desert, }
                },
                {
                    Province.Peak,
                    new() { Zone.Snowpeak, }
                },
                {
                    Province.Split,
                    new() { Zone.Golden_Wolf, Zone.LongMinigames, }
                },
                {
                    Province.Dungeon,
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
                    }
                }
            };
        private static Dictionary<Zone, Province> zoneToProvince;

        static ProvinceUtils()
        {
            enumToStr = new();

            foreach (Province province in Enum.GetValues(typeof(Province)))
            {
                enumToStr[province] = province.ToString();
            }

            zoneToProvince = new();
            foreach (KeyValuePair<Province, HashSet<Zone>> pair in provinceToZones)
            {
                HashSet<Zone> zones = pair.Value;
                foreach (Zone zone in zones)
                {
                    zoneToProvince[zone] = pair.Key;
                }
            }
        }

        public static Province StringToId(string str)
        {
            Province province;
            bool success = Enum.TryParse(str, true, out province);
            if (success)
                return province;
            else
                return Province.Invalid;
        }

        public static string IdToString(Province province)
        {
            if (enumToStr.ContainsKey(province))
                return enumToStr[province];
            return null;
        }

        public static Province ZoneToProvince(Zone zone)
        {
            Province province;
            bool success = zoneToProvince.TryGetValue(zone, out province);
            if (success)
                return province;
            return Province.Invalid;
        }

        public static HashSet<Zone> ProvinceToZones(Province province)
        {
            HashSet<Zone> zones;
            bool success = provinceToZones.TryGetValue(province, out zones);
            if (success)
                return zones;
            return new();
        }

        public static HashSet<string> GetProvinceNames()
        {
            HashSet<string> result = new();
            List<Province> validProvinces =
                new()
                {
                    Province.Ordona,
                    Province.Faron,
                    Province.Eldin,
                    Province.Lanayru,
                    Province.Desert,
                    Province.Peak,
                    Province.Split,
                    Province.Dungeon,
                };
            foreach (Province province in validProvinces)
            {
                result.Add(IdToString(province));
            }
            return result;
        }
    }
}
