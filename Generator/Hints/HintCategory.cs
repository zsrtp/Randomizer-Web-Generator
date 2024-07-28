namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SSettings.Enums;
    using TPRandomizer.Util;

    public enum HintCategory
    {
        Invalid = 0,
        Grotto = 1,
        Post_dungeon = 2,
        Mist = 3,
        Owl_Statue = 4,
        Llc_Lantern_Chests = 5,
        Underwater = 6,
        Southern_Desert = 7,
        Northern_Desert = 8,
        Goron_Mines_2nd_Part = 9,
        Temple_of_Time_2nd_Half = 10,
        City_in_the_Sky_East_Wing = 11,
        Dungeon = 12,
        Lake_Lantern_Cave_2nd_Half = 13,
        Arbiters_Grounds_2nd_Half = 14,
        Lakebed_Temple_2nd_Wing = 15,
        Forest_Temple_West_Wing = 16,
        Snowpeak_Ruins_2nd_Floor = 17,
        Snowpeak_Beyond_This_Point = 18,
        Golden_Wolf = 19,
    }

    public class HintCategoryUtils
    {
        public static readonly byte NumBitsToEncode = 5;
        private static Dictionary<HintCategory, string> enumToStr;
        private static Dictionary<string, HintCategory> strToEnum;

        private static Dictionary<string, HashSet<HintCategory>> checkToCategories;

        public static readonly Dictionary<HintCategory, string[]> categoryToChecksMap =
            new()
            {
                {
                    HintCategory.Grotto,
                    new[]
                    {
                        "Ordon Ranch Grotto Lantern Chest",
                        "Sacred Grove Baba Serpent Grotto Chest",
                        "Faron Field Corner Grotto Left Chest",
                        "Faron Field Corner Grotto Rear Chest",
                        "Faron Field Corner Grotto Right Chest",
                        "Eldin Field Bomskit Grotto Lantern Chest",
                        "Eldin Field Bomskit Grotto Left Chest",
                        "Eldin Field Water Bomb Fish Grotto Chest",
                        "Eldin Field Stalfos Grotto Left Small Chest",
                        "Eldin Field Stalfos Grotto Right Small Chest",
                        "Eldin Field Stalfos Grotto Stalfos Chest",
                        "Lanayru Field Poe Grotto Left Poe",
                        "Lanayru Field Poe Grotto Right Poe",
                        "Lanayru Field Skulltula Grotto Chest",
                        "West Hyrule Field Helmasaur Grotto Chest",
                        "Outside South Castle Town Tektite Grotto Chest",
                        "Lake Hylia Bridge Bubble Grotto Chest",
                        "Lake Hylia Shell Blade Grotto Chest",
                        "Lake Hylia Water Toadpoli Grotto Chest",
                        "Gerudo Desert Rock Grotto First Poe",
                        "Gerudo Desert Rock Grotto Lantern Chest",
                        "Gerudo Desert Rock Grotto Second Poe",
                        "Gerudo Desert Skulltula Grotto Chest",
                        "Snowpeak Freezard Grotto Chest",
                    }
                },
                {
                    HintCategory.Post_dungeon,
                    new[]
                    {
                        "Talo Sharpshooting",
                        "Kakariko Village Malo Mart Hawkeye",
                        "Death Mountain Trail Poe",
                        "Snowboard Racing Prize",
                        "Doctors Office Balcony Chest",
                        "Renados Letter",
                        "Telma Invoice",
                        "Wooden Statue",
                        "Ilia Memory Reward",
                    }
                },
                {
                    HintCategory.Mist,
                    new[]
                    {
                        "Faron Mist Cave Lantern Chest",
                        "Faron Mist Cave Open Chest",
                        "Faron Mist North Chest",
                        "Faron Mist South Chest",
                        "Faron Mist Stump Chest",
                    }
                },
                {
                    HintCategory.Owl_Statue,
                    new[]
                    {
                        "Sacred Grove Past Owl Statue Chest",
                        "Sacred Grove Temple of Time Owl Statue Poe",
                        "Faron Woods Owl Statue Chest",
                        "Faron Woods Owl Statue Sky Character",
                        "Kakariko Gorge Owl Statue Chest",
                        "Kakariko Gorge Owl Statue Sky Character",
                        "Bridge of Eldin Owl Statue Chest",
                        "Bridge of Eldin Owl Statue Sky Character",
                        "Hyrule Field Amphitheater Owl Statue Chest",
                        "Hyrule Field Amphitheater Owl Statue Sky Character",
                        "Lake Hylia Bridge Owl Statue Chest",
                        "Lake Hylia Bridge Owl Statue Sky Character",
                        "Gerudo Desert Owl Statue Chest",
                        "Gerudo Desert Owl Statue Sky Character",
                        "Hyrule Castle Graveyard Owl Statue Chest",
                    }
                },
                {
                    HintCategory.Llc_Lantern_Chests,
                    new[]
                    {
                        "Lake Lantern Cave Sixth Chest",
                        "Lake Lantern Cave End Lantern Chest",
                    }
                },
                {
                    HintCategory.Underwater,
                    new[]
                    {
                        "Eldin Spring Underwater Chest",
                        "Lanayru Field Behind Gate Underwater Chest",
                        "Lake Hylia Underwater Chest",
                        "Lanayru Spring Underwater Left Chest",
                        "Lanayru Spring Underwater Right Chest",
                        "Zoras Domain Extinguish All Torches Chest",
                        "Zoras Domain Light All Torches Chest",
                        "Zoras Domain Underwater Goron",
                        "Goron Mines Crystal Switch Room Underwater Chest",
                        "Goron Mines Outside Underwater Chest",
                        "Lakebed Temple Before Deku Toad Underwater Left Chest",
                        "Lakebed Temple Before Deku Toad Underwater Right Chest",
                        "Lakebed Temple Central Room Spire Chest",
                        "Lakebed Temple West Second Floor Southwest Underwater Chest",
                        "City in The Sky Underwater East Chest",
                        "City in The Sky Underwater West Chest",
                    }
                },
                {
                    HintCategory.Southern_Desert,
                    new[]
                    {
                        "Gerudo Desert East Canyon Chest",
                        "Gerudo Desert East Poe",
                        "Gerudo Desert Female Dayfly",
                        "Gerudo Desert Lone Small Chest",
                        "Gerudo Desert Male Dayfly",
                        "Gerudo Desert Owl Statue Chest",
                        "Gerudo Desert Owl Statue Sky Character",
                        "Gerudo Desert Peahat Ledge Chest",
                        "Gerudo Desert Poe Above Cave of Ordeals",
                        "Gerudo Desert Skulltula Grotto Chest",
                        "Gerudo Desert South Chest Behind Wooden Gates",
                        "Gerudo Desert West Canyon Chest"
                    }
                },
                {
                    HintCategory.Northern_Desert,
                    new[]
                    {
                        "Gerudo Desert Campfire East Chest",
                        "Gerudo Desert Campfire North Chest",
                        "Gerudo Desert Campfire West Chest",
                        "Gerudo Desert North Peahat Poe",
                        "Gerudo Desert North Small Chest Before Bulblin Camp",
                        "Gerudo Desert Northeast Chest Behind Gates",
                        "Gerudo Desert Northwest Chest Behind Gates",
                        "Gerudo Desert Rock Grotto First Poe",
                        "Gerudo Desert Rock Grotto Lantern Chest",
                        "Gerudo Desert Rock Grotto Second Poe",
                    }
                },
                {
                    // The entire side path from the open room with water at the
                    // bottom.
                    HintCategory.Goron_Mines_2nd_Part,
                    new[]
                    {
                        "Goron Mines Gor Ebizo Chest",
                        "Goron Mines Gor Ebizo Key Shard",
                        "Goron Mines Chest Before Dangoro",
                        "Goron Mines Dangoro Chest",
                        "Goron Mines Beamos Room Chest",
                        "Goron Mines Gor Liggs Chest",
                        "Goron Mines Gor Liggs Key Shard",
                        "Goron Mines Main Magnet Room Top Chest",
                    }
                },
                {
                    HintCategory.Temple_of_Time_2nd_Half,
                    new[]
                    {
                        "Temple of Time Moving Wall Dinalfos Room Chest",
                        "Temple of Time Scales Gohma Chest",
                        "Temple of Time Poe Above Scales",
                        "Temple of Time Scales Upper Chest",
                        "Temple of Time Floor Switch Puzzle Room Upper Chest",
                        "Temple of Time Big Key Chest",
                        "Temple of Time Gilloutine Chest",
                        "Temple of Time Chest Before Darknut",
                        "Temple of Time Darknut Chest",
                    }
                },
                {
                    HintCategory.City_in_the_Sky_East_Wing,
                    new[]
                    {
                        "City in The Sky East First Wing Chest After Fans",
                        "City in The Sky East Tile Worm Small Chest",
                        "City in The Sky East Wing After Dinalfos Alcove Chest",
                        "City in The Sky East Wing After Dinalfos Ledge Chest",
                        "City in The Sky East Wing Lower Level Chest",
                        "City in The Sky Aeralfos Chest",
                    }
                },
                // We don't put `Dungeon` here since it isn't used and it would
                // be massive.
                {
                    HintCategory.Lake_Lantern_Cave_2nd_Half,
                    new[]
                    {
                        "Lake Lantern Cave Second Poe",
                        "Lake Lantern Cave Final Poe",
                        "Lake Lantern Cave Ninth Chest",
                        "Lake Lantern Cave Tenth Chest",
                        "Lake Lantern Cave Eleventh Chest",
                        "Lake Lantern Cave Twelfth Chest",
                        "Lake Lantern Cave Thirteenth Chest",
                        "Lake Lantern Cave Fourteenth Chest",
                        "Lake Lantern Cave End Lantern Chest",
                    }
                },
                {
                    HintCategory.Snowpeak_Beyond_This_Point,
                    new[]
                    {
                        // All but "Ashei Sketch" are technically beyond the
                        // sign. However, we only end up creating this hint when
                        // it would hint about "Snowpeak Cave Ice Lantern Chest"
                        // and "Snowpeak Freezard Grotto Chest" and SPR is not
                        // required.
                        "Snowboard Racing Prize",
                        "Snowpeak Above Freezard Grotto Poe",
                        "Snowpeak Blizzard Poe",
                        "Snowpeak Cave Ice Lantern Chest",
                        "Snowpeak Cave Ice Poe",
                        "Snowpeak Freezard Grotto Chest",
                        "Snowpeak Icy Summit Poe",
                        "Snowpeak Poe Among Trees"
                    }
                },
                {
                    HintCategory.Arbiters_Grounds_2nd_Half,
                    new[]
                    {
                        "Arbiters Grounds North Turning Room Chest",
                        "Arbiters Grounds Big Key Chest",
                        "Arbiters Grounds Spinner Room First Small Chest",
                        "Arbiters Grounds Spinner Room Lower Central Small Chest",
                        "Arbiters Grounds Spinner Room Lower North Chest",
                        "Arbiters Grounds Spinner Room Second Small Chest",
                        "Arbiters Grounds Spinner Room Stalfos Alcove Chest",
                        "Arbiters Grounds Death Sword Chest",
                        "Arbiters Grounds Stallord Heart Container",
                        "Arbiters Grounds Dungeon Reward",
                    }
                },
                {
                    HintCategory.Lakebed_Temple_2nd_Wing,
                    new[]
                    {
                        "Lakebed Temple West Lower Small Chest",
                        "Lakebed Temple West Second Floor Central Small Chest",
                        "Lakebed Temple West Second Floor Northeast Chest",
                        "Lakebed Temple West Second Floor Southeast Chest",
                        "Lakebed Temple West Second Floor Southwest Underwater Chest",
                        "Lakebed Temple West Water Supply Chest",
                        "Lakebed Temple West Water Supply Small Chest",
                        "Lakebed Temple Underwater Maze Small Chest",
                        "Lakebed Temple Big Key Chest",
                    }
                },
                {
                    HintCategory.Forest_Temple_West_Wing,
                    new[]
                    {
                        "Forest Temple Big Baba Key",
                        "Forest Temple Totem Pole Chest",
                        "Forest Temple West Deku Like Chest",
                        "Forest Temple West Tile Worm Chest Behind Stairs",
                        "Forest Temple West Tile Worm Room Vines Chest",
                        "Forest Temple Gale Boomerang",
                    }
                },
                {
                    HintCategory.Snowpeak_Ruins_2nd_Floor,
                    new[]
                    {
                        "Snowpeak Ruins Chapel Chest",
                        "Snowpeak Ruins Ice Room Poe",
                        "Snowpeak Ruins Lobby Chandelier Chest",
                        "Snowpeak Ruins Northeast Chandelier Chest",
                        "Snowpeak Ruins Wooden Beam Chandelier Chest",
                    }
                },
                {
                    HintCategory.Golden_Wolf,
                    new[]
                    {
                        "Faron Woods Golden Wolf",
                        "Gerudo Desert Golden Wolf",
                        "Kakariko Graveyard Golden Wolf",
                        "North Castle Town Golden Wolf",
                        "Ordon Spring Golden Wolf",
                        "Outside South Castle Town Golden Wolf",
                        "West Hyrule Field Golden Wolf"
                    }
                }
            };

        static HintCategoryUtils()
        {
            enumToStr = new()
            {
                { HintCategory.Grotto, "Grotto" },
                { HintCategory.Post_dungeon, "Post_dungeon" },
                { HintCategory.Mist, "Mist" },
                { HintCategory.Owl_Statue, "Owl_Statue" },
                { HintCategory.Llc_Lantern_Chests, "Llc_Lantern_Chests" },
                { HintCategory.Underwater, "Underwater" },
                { HintCategory.Southern_Desert, "Southern_Desert" },
                { HintCategory.Northern_Desert, "Northern_Desert" },
                { HintCategory.Goron_Mines_2nd_Part, "Goron_Mines_2nd_Part" },
                { HintCategory.Temple_of_Time_2nd_Half, "Temple_of_Time_2nd_Half" },
                { HintCategory.City_in_the_Sky_East_Wing, "City_in_the_Sky_East_Wing" },
                { HintCategory.Dungeon, "Dungeon" },
                { HintCategory.Lake_Lantern_Cave_2nd_Half, "Lake_Lantern_Cave_2nd_Half" },
                { HintCategory.Arbiters_Grounds_2nd_Half, "Arbiters_Grounds_2nd_Half" },
                { HintCategory.Lakebed_Temple_2nd_Wing, "Lakebed_Temple_2nd_Wing" },
                { HintCategory.Snowpeak_Ruins_2nd_Floor, "Snowpeak_Ruins_2nd_Floor" },
                { HintCategory.Snowpeak_Beyond_This_Point, "Snowpeak_Beyond_This_Point" },
                { HintCategory.Golden_Wolf, "Golden_Wolf" },
            };

            strToEnum = new();
            foreach (KeyValuePair<HintCategory, string> pair in enumToStr)
            {
                strToEnum[pair.Value] = pair.Key;
            }

            checkToCategories = new();
            foreach (KeyValuePair<HintCategory, string[]> pair in categoryToChecksMap)
            {
                foreach (string checkName in pair.Value)
                {
                    if (
                        !checkToCategories.TryGetValue(
                            checkName,
                            out HashSet<HintCategory> categories
                        )
                    )
                    {
                        categories = new();
                        checkToCategories[checkName] = categories;
                    }
                    categories.Add(pair.Key);
                }
            }
        }

        public static HintCategory StringToId(string category)
        {
            if (strToEnum.ContainsKey(category))
                return strToEnum[category];
            return HintCategory.Invalid;
        }

        public static string IdToString(HintCategory category)
        {
            if (enumToStr.ContainsKey(category))
                return enumToStr[category];
            return null;
        }

        public static HashSet<HintCategory> checkNameToCategories(string checkName)
        {
            if (checkToCategories.TryGetValue(checkName, out HashSet<HintCategory> categories))
            {
                return categories;
            }
            return null;
        }
    }
}
