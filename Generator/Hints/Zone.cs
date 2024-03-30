namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;

    public enum Zone : int
    {
        Invalid = 0,
        Ordon = 1,
        Sacred_Grove = 2,
        Faron_Field = 3,
        Faron_Woods = 4,
        Kakariko_Gorge = 5,
        Kakariko_Village = 6,
        Kakariko_Graveyard = 7,
        Eldin_Field = 8,
        North_Eldin = 9,
        Death_Mountain = 10,
        Hidden_Village = 11,
        Lanayru_Field = 12,
        Beside_Castle_Town = 13,
        South_of_Castle_Town = 14,
        Castle_Town = 15,
        Agitha = 16,
        Great_Bridge_of_Hylia = 17,
        Lake_Hylia = 18,
        Lake_Lantern_Cave = 19,
        Lanayru_Spring = 20,
        Zoras_Domain = 21,
        Upper_Zoras_River = 22,
        Gerudo_Desert = 23,
        Bulblin_Camp = 24,
        Snowpeak = 25,
        Golden_Wolf = 26,
        Cave_of_Ordeals = 27,
        LongMinigames = 28,
        Forest_Temple = 29,
        Goron_Mines = 30,
        Lakebed_Temple = 31,
        Arbiters_Grounds = 32,
        Snowpeak_Ruins = 33,
        Temple_of_Time = 34,
        City_in_the_Sky = 35,
        Palace_of_Twilight = 36,
        Hyrule_Castle = 37,
    }

    public class ZoneUtils
    {
        public static readonly byte NumBitsToEncode = 6;
        private static Dictionary<Zone, string> enumToStr;
        private static Dictionary<string, Zone> strToEnum;
        private static Dictionary<Zone, SpotId> idToSpotId =
            new()
            {
                { Zone.Ordon, SpotId.Ordon_Sign },
                { Zone.Sacred_Grove, SpotId.Sacred_Grove_Sign },
                { Zone.Faron_Field, SpotId.Faron_Field_Sign },
                { Zone.Faron_Woods, SpotId.Faron_Woods_Sign },
                { Zone.Kakariko_Gorge, SpotId.Kakariko_Gorge_Sign },
                { Zone.Kakariko_Village, SpotId.Kakariko_Village_Sign },
                { Zone.Kakariko_Graveyard, SpotId.Kakariko_Graveyard_Sign },
                { Zone.Eldin_Field, SpotId.Eldin_Field_Sign },
                { Zone.North_Eldin, SpotId.North_Eldin_Sign },
                { Zone.Death_Mountain, SpotId.Death_Mountain_Sign },
                { Zone.Hidden_Village, SpotId.Hidden_Village_Sign },
                { Zone.Lanayru_Field, SpotId.Lanayru_Field_Sign },
                { Zone.Beside_Castle_Town, SpotId.Beside_Castle_Town_Sign },
                { Zone.South_of_Castle_Town, SpotId.South_of_Castle_Town_Sign },
                { Zone.Castle_Town, SpotId.Castle_Town_Sign },
                { Zone.Great_Bridge_of_Hylia, SpotId.Great_Bridge_of_Hylia_Sign },
                { Zone.Lake_Hylia, SpotId.Lake_Hylia_Sign },
                { Zone.Lake_Lantern_Cave, SpotId.Lake_Lantern_Cave_Sign },
                { Zone.Lanayru_Spring, SpotId.Lanayru_Spring_Sign },
                { Zone.Zoras_Domain, SpotId.Zoras_Domain_Sign },
                { Zone.Upper_Zoras_River, SpotId.Upper_Zoras_River_Sign },
                { Zone.Gerudo_Desert, SpotId.Gerudo_Desert_Sign },
                { Zone.Bulblin_Camp, SpotId.Bulblin_Camp_Sign },
                { Zone.Snowpeak, SpotId.Snowpeak_Sign },
                { Zone.Cave_of_Ordeals, SpotId.Cave_of_Ordeals_Sign },
                { Zone.Forest_Temple, SpotId.Forest_Temple_Sign },
                { Zone.Goron_Mines, SpotId.Goron_Mines_Sign },
                { Zone.Lakebed_Temple, SpotId.Lakebed_Temple_Sign },
                { Zone.Arbiters_Grounds, SpotId.Arbiters_Grounds_Sign },
                { Zone.Snowpeak_Ruins, SpotId.Snowpeak_Ruins_Sign },
                { Zone.Temple_of_Time, SpotId.Temple_of_Time_Sign },
                { Zone.City_in_the_Sky, SpotId.City_in_the_Sky_Sign },
                { Zone.Palace_of_Twilight, SpotId.Palace_of_Twilight_Sign },
                { Zone.Hyrule_Castle, SpotId.Hyrule_Castle_Sign },
            };

        public static readonly Dictionary<Zone, BeyondPointObj> idToBeyondPointData =
            new()
            {
                {
                    Zone.Lake_Lantern_Cave,
                    new BeyondPointObj(
                        Zone.Lake_Lantern_Cave,
                        SpotId.Lake_Lantern_Cave_Sign,
                        HintCategory.Lake_Lantern_Cave_2nd_Half,
                        BeyondPointObj.Validity.AlwaysPass
                    )
                },
                {
                    Zone.Snowpeak,
                    new BeyondPointObj(
                        Zone.Snowpeak,
                        SpotId.Snowpeak_Sign,
                        HintCategory.Snowpeak_Beyond_This_Point,
                        BeyondPointObj.Validity.Snowpeak
                    )
                },
                {
                    Zone.Goron_Mines,
                    new BeyondPointObj(
                        Zone.Goron_Mines,
                        SpotId.Goron_Mines_Sign,
                        HintCategory.Goron_Mines_2nd_Part,
                        BeyondPointObj.Validity.Dungeon
                    )
                },
                {
                    Zone.Lakebed_Temple,
                    new BeyondPointObj(
                        Zone.Lakebed_Temple,
                        SpotId.Lakebed_Temple_Sign,
                        HintCategory.Lakebed_Temple_2nd_Wing,
                        BeyondPointObj.Validity.Dungeon
                    )
                },
                {
                    Zone.Arbiters_Grounds,
                    new BeyondPointObj(
                        Zone.Arbiters_Grounds,
                        SpotId.Arbiters_Grounds_Sign,
                        HintCategory.Arbiters_Grounds_2nd_Half,
                        BeyondPointObj.Validity.Dungeon
                    )
                },
                {
                    Zone.Temple_of_Time,
                    new BeyondPointObj(
                        Zone.Temple_of_Time,
                        SpotId.Temple_of_Time_Beyond_Point_Sign,
                        HintCategory.Temple_of_Time_2nd_Half,
                        BeyondPointObj.Validity.Dungeon
                    )
                },
                {
                    Zone.City_in_the_Sky,
                    new BeyondPointObj(
                        Zone.City_in_the_Sky,
                        SpotId.City_in_the_Sky_Sign,
                        HintCategory.City_in_the_Sky_East_Wing,
                        BeyondPointObj.Validity.Dungeon
                    )
                },
            };

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

        public static readonly Dictionary<string, string[]> zoneNameToChecks =
            new()
            {
                {
                    "Ordon",
                    new[]
                    {
                        "Herding Goats Reward",
                        "Links Basement Chest",
                        "Ordon Cat Rescue",
                        "Ordon Ranch Grotto Lantern Chest",
                        "Ordon Shield",
                        "Ordon Sword",
                        "Sera Shop Slingshot",
                        "Uli Cradle Delivery",
                        "Wooden Sword Chest",
                        "Wrestling With Bo"
                    }
                },
                {
                    "Sacred Grove",
                    new[]
                    {
                        "Lost Woods Boulder Poe",
                        "Lost Woods Lantern Chest",
                        "Lost Woods Waterfall Poe",
                        "Sacred Grove Baba Serpent Grotto Chest",
                        "Sacred Grove Female Snail",
                        "Sacred Grove Male Snail",
                        "Sacred Grove Master Sword Poe",
                        "Sacred Grove Past Owl Statue Chest",
                        "Sacred Grove Pedestal Master Sword",
                        "Sacred Grove Pedestal Shadow Crystal",
                        "Sacred Grove Spinner Chest",
                        "Sacred Grove Temple of Time Owl Statue Poe"
                    }
                },
                {
                    "Faron Field",
                    new[]
                    {
                        "Faron Field Bridge Chest",
                        "Faron Field Corner Grotto Left Chest",
                        "Faron Field Corner Grotto Rear Chest",
                        "Faron Field Corner Grotto Right Chest",
                        "Faron Field Female Beetle",
                        "Faron Field Male Beetle",
                        "Faron Field Poe",
                        "Faron Field Tree Heart Piece"
                    }
                },
                {
                    "Faron Woods",
                    new[]
                    {
                        "Coro Bottle",
                        "Faron Mist Cave Lantern Chest",
                        "Faron Mist Cave Open Chest",
                        "Faron Mist North Chest",
                        "Faron Mist Poe",
                        "Faron Mist South Chest",
                        "Faron Mist Stump Chest",
                        "Faron Woods Owl Statue Chest",
                        "Faron Woods Owl Statue Sky Character",
                        "North Faron Woods Deku Baba Chest",
                        "South Faron Cave Chest"
                    }
                },
                {
                    "Kakariko Gorge",
                    new[]
                    {
                        "Eldin Lantern Cave First Chest",
                        "Eldin Lantern Cave Lantern Chest",
                        "Eldin Lantern Cave Poe",
                        "Eldin Lantern Cave Second Chest",
                        "Kakariko Gorge Double Clawshot Chest",
                        "Kakariko Gorge Female Pill Bug",
                        "Kakariko Gorge Male Pill Bug",
                        "Kakariko Gorge Owl Statue Chest",
                        "Kakariko Gorge Owl Statue Sky Character",
                        "Kakariko Gorge Poe",
                        "Kakariko Gorge Spire Heart Piece"
                    }
                },
                {
                    "Kakariko Village",
                    new[]
                    {
                        "Barnes Bomb Bag",
                        "Eldin Spring Underwater Chest",
                        "Ilia Memory Reward",
                        "Kakariko Inn Chest",
                        "Kakariko Village Bomb Rock Spire Heart Piece",
                        "Kakariko Village Bomb Shop Poe",
                        "Kakariko Village Female Ant",
                        "Kakariko Village Malo Mart Hawkeye",
                        "Kakariko Village Malo Mart Hylian Shield",
                        "Kakariko Village Malo Mart Red Potion",
                        "Kakariko Village Malo Mart Wooden Shield",
                        "Kakariko Village Watchtower Poe",
                        "Kakariko Watchtower Alcove Chest",
                        "Kakariko Watchtower Chest",
                        "Renados Letter",
                        "Talo Sharpshooting"
                    }
                },
                {
                    "Kakariko Graveyard",
                    new[]
                    {
                        "Gift From Ralis",
                        "Kakariko Graveyard Grave Poe",
                        "Kakariko Graveyard Lantern Chest",
                        "Kakariko Graveyard Male Ant",
                        "Kakariko Graveyard Open Poe",
                        "Rutelas Blessing"
                    }
                },
                {
                    "Eldin Field",
                    new[]
                    {
                        "Bridge of Eldin Male Phasmid",
                        "Bridge of Eldin Owl Statue Chest",
                        "Eldin Field Bomb Rock Chest",
                        "Eldin Field Bomskit Grotto Lantern Chest",
                        "Eldin Field Bomskit Grotto Left Chest",
                        "Eldin Field Female Grasshopper",
                        "Eldin Field Male Grasshopper",
                        "Eldin Field Water Bomb Fish Grotto Chest"
                    }
                },
                {
                    "North Eldin",
                    new[]
                    {
                        "Bridge of Eldin Female Phasmid",
                        "Bridge of Eldin Owl Statue Sky Character",
                        "Eldin Field Stalfos Grotto Left Small Chest",
                        "Eldin Field Stalfos Grotto Right Small Chest",
                        "Eldin Field Stalfos Grotto Stalfos Chest",
                        "Eldin Stockcave Lantern Chest",
                        "Eldin Stockcave Lowest Chest",
                        "Eldin Stockcave Upper Chest"
                    }
                },
                {
                    "Death Mountain",
                    new[] { "Death Mountain Alcove Chest", "Death Mountain Trail Poe" }
                },
                {
                    "Hidden Village",
                    new[]
                    {
                        "Cats Hide and Seek Minigame",
                        "Hidden Village Poe",
                        "Ilia Charm",
                        "Skybook From Impaz"
                    }
                },
                {
                    "Lanayru Field",
                    new[]
                    {
                        "Lanayru Field Behind Gate Underwater Chest",
                        "Lanayru Field Bridge Poe",
                        "Lanayru Field Female Stag Beetle",
                        "Lanayru Field Male Stag Beetle",
                        "Lanayru Field Poe Grotto Left Poe",
                        "Lanayru Field Poe Grotto Right Poe",
                        "Lanayru Field Skulltula Grotto Chest",
                        "Lanayru Field Spinner Track Chest",
                        "Lanayru Ice Block Puzzle Cave Chest"
                    }
                },
                {
                    "Beside Castle Town",
                    new[]
                    {
                        "Hyrule Field Amphitheater Owl Statue Chest",
                        "Hyrule Field Amphitheater Owl Statue Sky Character",
                        "Hyrule Field Amphitheater Poe",
                        "West Hyrule Field Female Butterfly",
                        "West Hyrule Field Helmasaur Grotto Chest",
                        "West Hyrule Field Male Butterfly"
                    }
                },
                {
                    "South of Castle Town",
                    new[]
                    {
                        "Outside South Castle Town Double Clawshot Chasm Chest",
                        "Outside South Castle Town Female Ladybug",
                        "Outside South Castle Town Fountain Chest",
                        "Outside South Castle Town Male Ladybug",
                        "Outside South Castle Town Poe",
                        "Outside South Castle Town Tektite Grotto Chest",
                        "Outside South Castle Town Tightrope Chest",
                        "Wooden Statue"
                    }
                },
                {
                    "Castle Town",
                    new[]
                    {
                        "Castle Town Malo Mart Magic Armor",
                        "Charlo Donation Blessing",
                        "Doctors Office Balcony Chest",
                        "East Castle Town Bridge Poe",
                        "Jovani 20 Poe Soul Reward",
                        "Jovani 60 Poe Soul Reward",
                        "Jovani House Poe",
                        "STAR Prize 1",
                        "STAR Prize 2",
                        "Telma Invoice"
                    }
                },
                {
                    "Agitha",
                    new[]
                    {
                        "Agitha Female Ant Reward",
                        "Agitha Female Beetle Reward",
                        "Agitha Female Butterfly Reward",
                        "Agitha Female Dayfly Reward",
                        "Agitha Female Dragonfly Reward",
                        "Agitha Female Grasshopper Reward",
                        "Agitha Female Ladybug Reward",
                        "Agitha Female Mantis Reward",
                        "Agitha Female Phasmid Reward",
                        "Agitha Female Pill Bug Reward",
                        "Agitha Female Snail Reward",
                        "Agitha Female Stag Beetle Reward",
                        "Agitha Male Ant Reward",
                        "Agitha Male Beetle Reward",
                        "Agitha Male Butterfly Reward",
                        "Agitha Male Dayfly Reward",
                        "Agitha Male Dragonfly Reward",
                        "Agitha Male Grasshopper Reward",
                        "Agitha Male Ladybug Reward",
                        "Agitha Male Mantis Reward",
                        "Agitha Male Phasmid Reward",
                        "Agitha Male Pill Bug Reward",
                        "Agitha Male Snail Reward",
                        "Agitha Male Stag Beetle Reward"
                    }
                },
                {
                    "Great Bridge of Hylia",
                    new[]
                    {
                        "Lake Hylia Bridge Bubble Grotto Chest",
                        "Lake Hylia Bridge Cliff Chest",
                        "Lake Hylia Bridge Cliff Poe",
                        "Lake Hylia Bridge Female Mantis",
                        "Lake Hylia Bridge Male Mantis",
                        "Lake Hylia Bridge Owl Statue Chest",
                        "Lake Hylia Bridge Owl Statue Sky Character",
                        "Lake Hylia Bridge Vines Chest"
                    }
                },
                {
                    "Lake Hylia",
                    new[]
                    {
                        "Auru Gift To Fyer",
                        "Flight By Fowl Fifth Platform Chest",
                        "Flight By Fowl Fourth Platform Chest",
                        "Flight By Fowl Ledge Poe",
                        "Flight By Fowl Second Platform Chest",
                        "Flight By Fowl Third Platform Chest",
                        "Flight By Fowl Top Platform Reward",
                        "Isle of Riches Poe",
                        "Lake Hylia Alcove Poe",
                        "Lake Hylia Dock Poe",
                        "Lake Hylia Shell Blade Grotto Chest",
                        "Lake Hylia Tower Poe",
                        "Lake Hylia Underwater Chest",
                        "Lake Hylia Water Toadpoli Grotto Chest",
                        "Outside Lanayru Spring Left Statue Chest",
                        "Outside Lanayru Spring Right Statue Chest"
                    }
                },
                {
                    "Lake Lantern Cave",
                    new[]
                    {
                        "Lake Lantern Cave Eighth Chest",
                        "Lake Lantern Cave Eleventh Chest",
                        "Lake Lantern Cave End Lantern Chest",
                        "Lake Lantern Cave Fifth Chest",
                        "Lake Lantern Cave Final Poe",
                        "Lake Lantern Cave First Chest",
                        "Lake Lantern Cave First Poe",
                        "Lake Lantern Cave Fourteenth Chest",
                        "Lake Lantern Cave Fourth Chest",
                        "Lake Lantern Cave Ninth Chest",
                        "Lake Lantern Cave Second Chest",
                        "Lake Lantern Cave Second Poe",
                        "Lake Lantern Cave Seventh Chest",
                        "Lake Lantern Cave Sixth Chest",
                        "Lake Lantern Cave Tenth Chest",
                        "Lake Lantern Cave Third Chest",
                        "Lake Lantern Cave Thirteenth Chest",
                        "Lake Lantern Cave Twelfth Chest"
                    }
                },
                {
                    "Lanayru Spring",
                    new[]
                    {
                        "Lanayru Spring Back Room Lantern Chest",
                        "Lanayru Spring Back Room Left Chest",
                        "Lanayru Spring Back Room Right Chest",
                        "Lanayru Spring East Double Clawshot Chest",
                        "Lanayru Spring Underwater Left Chest",
                        "Lanayru Spring Underwater Right Chest",
                        "Lanayru Spring West Double Clawshot Chest"
                    }
                },
                {
                    "Zora's Domain",
                    new[]
                    {
                        "Zoras Domain Chest Behind Waterfall",
                        "Zoras Domain Chest By Mother and Child Isles",
                        "Zoras Domain Extinguish All Torches Chest",
                        "Zoras Domain Light All Torches Chest",
                        "Zoras Domain Male Dragonfly",
                        "Zoras Domain Mother and Child Isle Poe",
                        "Zoras Domain Underwater Goron",
                        "Zoras Domain Waterfall Poe"
                    }
                },
                {
                    "Upper Zora's River",
                    new[]
                    {
                        "Fishing Hole Bottle",
                        "Fishing Hole Heart Piece",
                        "Upper Zoras River Female Dragonfly",
                        "Upper Zoras River Poe"
                    }
                },
                {
                    "Gerudo Desert",
                    new[]
                    {
                        "Gerudo Desert Campfire East Chest",
                        "Gerudo Desert Campfire North Chest",
                        "Gerudo Desert Campfire West Chest",
                        "Gerudo Desert East Canyon Chest",
                        "Gerudo Desert East Poe",
                        "Gerudo Desert Female Dayfly",
                        "Gerudo Desert Lone Small Chest",
                        "Gerudo Desert Male Dayfly",
                        "Gerudo Desert North Peahat Poe",
                        "Gerudo Desert North Small Chest Before Bulblin Camp",
                        "Gerudo Desert Northeast Chest Behind Gates",
                        "Gerudo Desert Northwest Chest Behind Gates",
                        "Gerudo Desert Owl Statue Chest",
                        "Gerudo Desert Owl Statue Sky Character",
                        "Gerudo Desert Peahat Ledge Chest",
                        "Gerudo Desert Poe Above Cave of Ordeals",
                        "Gerudo Desert Rock Grotto First Poe",
                        "Gerudo Desert Rock Grotto Lantern Chest",
                        "Gerudo Desert Rock Grotto Second Poe",
                        "Gerudo Desert Skulltula Grotto Chest",
                        "Gerudo Desert South Chest Behind Wooden Gates",
                        "Gerudo Desert West Canyon Chest",
                        "Outside Bulblin Camp Poe"
                    }
                },
                {
                    "Bulblin Camp",
                    new[]
                    {
                        "Bulblin Camp First Chest Under Tower At Entrance",
                        "Bulblin Camp Poe",
                        "Bulblin Camp Roasted Boar",
                        "Bulblin Camp Small Chest in Back of Camp",
                        "Bulblin Guard Key",
                        "Outside Arbiters Grounds Lantern Chest",
                        "Outside Arbiters Grounds Poe"
                    }
                },
                {
                    "Snowpeak",
                    new[]
                    {
                        "Ashei Sketch",
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
                    "Golden Wolf",
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
                },
                {
                    "Cave of Ordeals",
                    new[]
                    {
                        "Cave of Ordeals Floor 17 Poe",
                        "Cave of Ordeals Floor 33 Poe",
                        "Cave of Ordeals Floor 44 Poe",
                        "Cave of Ordeals Great Fairy Reward"
                    }
                },
                {
                    "Long Minigames",
                    new[]
                    {
                        "Goron Springwater Rush",
                        "Iza Helping Hand",
                        "Iza Raging Rapids Minigame",
                        "Plumm Fruit Balloon Minigame"
                    }
                },
                {
                    "Forest Temple",
                    new[]
                    {
                        "Forest Temple Big Baba Key",
                        "Forest Temple Big Key Chest",
                        "Forest Temple Central Chest Behind Stairs",
                        "Forest Temple Central Chest Hanging From Web",
                        "Forest Temple Central North Chest",
                        "Forest Temple Diababa Heart Container",
                        "Forest Temple Dungeon Reward",
                        "Forest Temple East Tile Worm Chest",
                        "Forest Temple East Water Cave Chest",
                        "Forest Temple Entrance Vines Chest",
                        "Forest Temple Gale Boomerang",
                        "Forest Temple North Deku Like Chest",
                        "Forest Temple Second Monkey Under Bridge Chest",
                        "Forest Temple Totem Pole Chest",
                        "Forest Temple West Deku Like Chest",
                        "Forest Temple West Tile Worm Chest Behind Stairs",
                        "Forest Temple West Tile Worm Room Vines Chest",
                        "Forest Temple Windless Bridge Chest"
                    }
                },
                {
                    "Goron Mines",
                    new[]
                    {
                        "Goron Mines After Crystal Switch Room Magnet Wall Chest",
                        "Goron Mines Beamos Room Chest",
                        "Goron Mines Chest Before Dangoro",
                        "Goron Mines Crystal Switch Room Small Chest",
                        "Goron Mines Crystal Switch Room Underwater Chest",
                        "Goron Mines Dangoro Chest",
                        "Goron Mines Dungeon Reward",
                        "Goron Mines Entrance Chest",
                        "Goron Mines Fyrus Heart Container",
                        "Goron Mines Gor Amato Chest",
                        "Goron Mines Gor Amato Key Shard",
                        "Goron Mines Gor Amato Small Chest",
                        "Goron Mines Gor Ebizo Chest",
                        "Goron Mines Gor Ebizo Key Shard",
                        "Goron Mines Gor Liggs Chest",
                        "Goron Mines Gor Liggs Key Shard",
                        "Goron Mines Magnet Maze Chest",
                        "Goron Mines Main Magnet Room Bottom Chest",
                        "Goron Mines Main Magnet Room Top Chest",
                        "Goron Mines Outside Beamos Chest",
                        "Goron Mines Outside Clawshot Chest",
                        "Goron Mines Outside Underwater Chest"
                    }
                },
                {
                    "Lakebed Temple",
                    new[]
                    {
                        "Lakebed Temple Before Deku Toad Alcove Chest",
                        "Lakebed Temple Before Deku Toad Underwater Left Chest",
                        "Lakebed Temple Before Deku Toad Underwater Right Chest",
                        "Lakebed Temple Big Key Chest",
                        "Lakebed Temple Central Room Chest",
                        "Lakebed Temple Central Room Small Chest",
                        "Lakebed Temple Central Room Spire Chest",
                        "Lakebed Temple Chandelier Chest",
                        "Lakebed Temple Deku Toad Chest",
                        "Lakebed Temple Dungeon Reward",
                        "Lakebed Temple East Lower Waterwheel Bridge Chest",
                        "Lakebed Temple East Lower Waterwheel Stalactite Chest",
                        "Lakebed Temple East Second Floor Southeast Chest",
                        "Lakebed Temple East Second Floor Southwest Chest",
                        "Lakebed Temple East Water Supply Clawshot Chest",
                        "Lakebed Temple East Water Supply Small Chest",
                        "Lakebed Temple Lobby Left Chest",
                        "Lakebed Temple Lobby Rear Chest",
                        "Lakebed Temple Morpheel Heart Container",
                        "Lakebed Temple Stalactite Room Chest",
                        "Lakebed Temple Underwater Maze Small Chest",
                        "Lakebed Temple West Lower Small Chest",
                        "Lakebed Temple West Second Floor Central Small Chest",
                        "Lakebed Temple West Second Floor Northeast Chest",
                        "Lakebed Temple West Second Floor Southeast Chest",
                        "Lakebed Temple West Second Floor Southwest Underwater Chest",
                        "Lakebed Temple West Water Supply Chest",
                        "Lakebed Temple West Water Supply Small Chest"
                    }
                },
                {
                    "Arbiter's Grounds",
                    new[]
                    {
                        "Arbiters Grounds Big Key Chest",
                        "Arbiters Grounds Death Sword Chest",
                        "Arbiters Grounds Dungeon Reward",
                        "Arbiters Grounds East Lower Turnable Redead Chest",
                        "Arbiters Grounds East Turning Room Poe",
                        "Arbiters Grounds East Upper Turnable Chest",
                        "Arbiters Grounds East Upper Turnable Redead Chest",
                        "Arbiters Grounds Entrance Chest",
                        "Arbiters Grounds Ghoul Rat Room Chest",
                        "Arbiters Grounds Hidden Wall Poe",
                        "Arbiters Grounds North Turning Room Chest",
                        "Arbiters Grounds Spinner Room First Small Chest",
                        "Arbiters Grounds Spinner Room Lower Central Small Chest",
                        "Arbiters Grounds Spinner Room Lower North Chest",
                        "Arbiters Grounds Spinner Room Second Small Chest",
                        "Arbiters Grounds Spinner Room Stalfos Alcove Chest",
                        "Arbiters Grounds Stallord Heart Container",
                        "Arbiters Grounds Torch Room East Chest",
                        "Arbiters Grounds Torch Room Poe",
                        "Arbiters Grounds Torch Room West Chest",
                        "Arbiters Grounds West Chandelier Chest",
                        "Arbiters Grounds West Poe",
                        "Arbiters Grounds West Small Chest Behind Block",
                        "Arbiters Grounds West Stalfos Northeast Chest",
                        "Arbiters Grounds West Stalfos West Chest"
                    }
                },
                {
                    "Snowpeak Ruins",
                    new[]
                    {
                        "Snowpeak Ruins Ball and Chain",
                        "Snowpeak Ruins Blizzeta Heart Container",
                        "Snowpeak Ruins Broken Floor Chest",
                        "Snowpeak Ruins Chapel Chest",
                        "Snowpeak Ruins Chest After Darkhammer",
                        "Snowpeak Ruins Courtyard Central Chest",
                        "Snowpeak Ruins Dungeon Reward",
                        "Snowpeak Ruins East Courtyard Buried Chest",
                        "Snowpeak Ruins East Courtyard Chest",
                        "Snowpeak Ruins Ice Room Poe",
                        "Snowpeak Ruins Lobby Armor Poe",
                        "Snowpeak Ruins Lobby Chandelier Chest",
                        "Snowpeak Ruins Lobby East Armor Chest",
                        "Snowpeak Ruins Lobby Poe",
                        "Snowpeak Ruins Lobby West Armor Chest",
                        "Snowpeak Ruins Mansion Map",
                        "Snowpeak Ruins Northeast Chandelier Chest",
                        "Snowpeak Ruins Ordon Pumpkin Chest",
                        "Snowpeak Ruins West Cannon Room Central Chest",
                        "Snowpeak Ruins West Cannon Room Corner Chest",
                        "Snowpeak Ruins West Courtyard Buried Chest",
                        "Snowpeak Ruins Wooden Beam Central Chest",
                        "Snowpeak Ruins Wooden Beam Chandelier Chest",
                        "Snowpeak Ruins Wooden Beam Northwest Chest"
                    }
                },
                {
                    "Temple of Time",
                    new[]
                    {
                        "Temple of Time Armogohma Heart Container",
                        "Temple of Time Armos Antechamber East Chest",
                        "Temple of Time Armos Antechamber North Chest",
                        "Temple of Time Armos Antechamber Statue Chest",
                        "Temple of Time Big Key Chest",
                        "Temple of Time Chest Before Darknut",
                        "Temple of Time Darknut Chest",
                        "Temple of Time Dungeon Reward",
                        "Temple of Time First Staircase Armos Chest",
                        "Temple of Time First Staircase Gohma Gate Chest",
                        "Temple of Time First Staircase Window Chest",
                        "Temple of Time Floor Switch Puzzle Room Upper Chest",
                        "Temple of Time Gilloutine Chest",
                        "Temple of Time Lobby Lantern Chest",
                        "Temple of Time Moving Wall Beamos Room Chest",
                        "Temple of Time Moving Wall Dinalfos Room Chest",
                        "Temple of Time Poe Above Scales",
                        "Temple of Time Poe Behind Gate",
                        "Temple of Time Scales Gohma Chest",
                        "Temple of Time Scales Upper Chest"
                    }
                },
                {
                    "City in the Sky",
                    new[]
                    {
                        "City in The Sky Aeralfos Chest",
                        "City in The Sky Argorok Heart Container",
                        "City in The Sky Baba Tower Alcove Chest",
                        "City in The Sky Baba Tower Narrow Ledge Chest",
                        "City in The Sky Baba Tower Top Small Chest",
                        "City in The Sky Big Key Chest",
                        "City in The Sky Central Outside Ledge Chest",
                        "City in The Sky Central Outside Poe Island Chest",
                        "City in The Sky Chest Behind North Fan",
                        "City in The Sky Chest Below Big Key Chest",
                        "City in The Sky Dungeon Reward",
                        "City in The Sky East First Wing Chest After Fans",
                        "City in The Sky East Tile Worm Small Chest",
                        "City in The Sky East Wing After Dinalfos Alcove Chest",
                        "City in The Sky East Wing After Dinalfos Ledge Chest",
                        "City in The Sky East Wing Lower Level Chest",
                        "City in The Sky Garden Island Poe",
                        "City in The Sky Poe Above Central Fan",
                        "City in The Sky Underwater East Chest",
                        "City in The Sky Underwater West Chest",
                        "City in The Sky West Garden Corner Chest",
                        "City in The Sky West Garden Ledge Chest",
                        "City in The Sky West Garden Lone Island Chest",
                        "City in The Sky West Garden Lower Chest",
                        "City in The Sky West Wing Baba Balcony Chest",
                        "City in The Sky West Wing First Chest",
                        "City in The Sky West Wing Narrow Ledge Chest",
                        "City in The Sky West Wing Tile Worm Chest"
                    }
                },
                {
                    "Palace of Twilight",
                    new[]
                    {
                        "Palace of Twilight Big Key Chest",
                        "Palace of Twilight Central First Room Chest",
                        "Palace of Twilight Central Outdoor Chest",
                        "Palace of Twilight Central Tower Chest",
                        "Palace of Twilight Collect Both Sols",
                        "Palace of Twilight East Wing First Room East Alcove",
                        "Palace of Twilight East Wing First Room North Small Chest",
                        "Palace of Twilight East Wing First Room West Alcove",
                        "Palace of Twilight East Wing First Room Zant Head Chest",
                        "Palace of Twilight East Wing Second Room Northeast Chest",
                        "Palace of Twilight East Wing Second Room Northwest Chest",
                        "Palace of Twilight East Wing Second Room Southeast Chest",
                        "Palace of Twilight East Wing Second Room Southwest Chest",
                        "Palace of Twilight West Wing Chest Behind Wall of Darkness",
                        "Palace of Twilight West Wing First Room Central Chest",
                        "Palace of Twilight West Wing Second Room Central Chest",
                        "Palace of Twilight West Wing Second Room Lower South Chest",
                        "Palace of Twilight West Wing Second Room Southeast Chest",
                        "Palace of Twilight Zant Heart Container"
                    }
                },
                {
                    "Hyrule Castle",
                    new[]
                    {
                        "Hyrule Castle Big Key Chest",
                        "Hyrule Castle East Wing Balcony Chest",
                        "Hyrule Castle East Wing Boomerang Puzzle Chest",
                        "Hyrule Castle Graveyard Grave Switch Room Back Left Chest",
                        "Hyrule Castle Graveyard Grave Switch Room Front Left Chest",
                        "Hyrule Castle Graveyard Grave Switch Room Right Chest",
                        "Hyrule Castle Graveyard Owl Statue Chest",
                        "Hyrule Castle King Bulblin Key",
                        "Hyrule Castle Lantern Staircase Chest",
                        "Hyrule Castle Main Hall Northeast Chest",
                        "Hyrule Castle Main Hall Northwest Chest",
                        "Hyrule Castle Main Hall Southwest Chest",
                        "Hyrule Castle Southeast Balcony Tower Chest",
                        "Hyrule Castle Treasure Room Eighth Small Chest",
                        "Hyrule Castle Treasure Room Fifth Chest",
                        "Hyrule Castle Treasure Room Fifth Small Chest",
                        "Hyrule Castle Treasure Room First Chest",
                        "Hyrule Castle Treasure Room First Small Chest",
                        "Hyrule Castle Treasure Room Fourth Chest",
                        "Hyrule Castle Treasure Room Fourth Small Chest",
                        "Hyrule Castle Treasure Room Second Chest",
                        "Hyrule Castle Treasure Room Second Small Chest",
                        "Hyrule Castle Treasure Room Seventh Small Chest",
                        "Hyrule Castle Treasure Room Sixth Small Chest",
                        "Hyrule Castle Treasure Room Third Chest",
                        "Hyrule Castle Treasure Room Third Small Chest",
                        "Hyrule Castle West Courtyard Central Small Chest",
                        "Hyrule Castle West Courtyard North Small Chest"
                    }
                },
            };

        static ZoneUtils()
        {
            enumToStr = new()
            {
                { Zone.Ordon, "Ordon" },
                { Zone.Sacred_Grove, "Sacred Grove" },
                { Zone.Faron_Field, "Faron Field" },
                { Zone.Faron_Woods, "Faron Woods" },
                { Zone.Kakariko_Gorge, "Kakariko Gorge" },
                { Zone.Kakariko_Village, "Kakariko Village" },
                { Zone.Kakariko_Graveyard, "Kakariko Graveyard" },
                { Zone.Eldin_Field, "Eldin Field" },
                { Zone.North_Eldin, "North Eldin" },
                { Zone.Death_Mountain, "Death Mountain" },
                { Zone.Hidden_Village, "Hidden Village" },
                { Zone.Lanayru_Field, "Lanayru Field" },
                { Zone.Beside_Castle_Town, "Beside Castle Town" },
                { Zone.South_of_Castle_Town, "South of Castle Town" },
                { Zone.Castle_Town, "Castle Town" },
                { Zone.Agitha, "Agitha" },
                { Zone.Great_Bridge_of_Hylia, "Great Bridge of Hylia" },
                { Zone.Lake_Hylia, "Lake Hylia" },
                { Zone.Lake_Lantern_Cave, "Lake Lantern Cave" },
                { Zone.Lanayru_Spring, "Lanayru Spring" },
                { Zone.Zoras_Domain, "Zora's Domain" },
                { Zone.Upper_Zoras_River, "Upper Zora's River" },
                { Zone.Gerudo_Desert, "Gerudo Desert" },
                { Zone.Bulblin_Camp, "Bulblin Camp" },
                { Zone.Snowpeak, "Snowpeak" },
                { Zone.Golden_Wolf, "Golden Wolf" },
                { Zone.Cave_of_Ordeals, "Cave of Ordeals" },
                { Zone.LongMinigames, "Long Minigames" },
                { Zone.Forest_Temple, "Forest Temple" },
                { Zone.Goron_Mines, "Goron Mines" },
                { Zone.Lakebed_Temple, "Lakebed Temple" },
                { Zone.Arbiters_Grounds, "Arbiter's Grounds" },
                { Zone.Snowpeak_Ruins, "Snowpeak Ruins" },
                { Zone.Temple_of_Time, "Temple of Time" },
                { Zone.City_in_the_Sky, "City in the Sky" },
                { Zone.Palace_of_Twilight, "Palace of Twilight" },
                { Zone.Hyrule_Castle, "Hyrule Castle" },
            };

            strToEnum = new();
            foreach (KeyValuePair<Zone, string> pair in enumToStr)
            {
                strToEnum[pair.Value] = pair.Key;
            }
        }

        public static Zone StringToId(string zoneName)
        {
            if (strToEnum.ContainsKey(zoneName))
                return strToEnum[zoneName];
            return Zone.Invalid;
        }

        public static Zone StringToIdThrows(string zoneName)
        {
            Zone zone = StringToId(zoneName);
            if (zone == Zone.Invalid)
                throw new Exception($"Expected zoneName '{zoneName}' to resolve to a valid zone.");
            return zone;
        }

        public static string IdToString(Zone zoneId)
        {
            if (enumToStr.ContainsKey(zoneId))
                return enumToStr[zoneId];
            return null;
        }

        public static SpotId IdToSpotId(Zone zoneId)
        {
            if (idToSpotId.ContainsKey(zoneId))
                return idToSpotId[zoneId];
            return SpotId.Invalid;
        }

        public static bool IsDungeonZone(Zone zoneId)
        {
            return dungeonZones.Contains(zoneId);
        }

        public static bool IsDungeonZone(string stringId)
        {
            Zone zoneId = StringToId(stringId);
            return dungeonZones.Contains(zoneId);
        }
    }
}
