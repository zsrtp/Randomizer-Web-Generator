namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TPRandomizer.Util;
    using TPRandomizer.SSettings.Enums;

    public enum TradeGroup
    {
        Invalid = 0,
        Male_Bugs = 1,
        Female_Bugs = 2,
        Ants = 3,
        Mantises = 4,
        Butterflies = 5,
        Phasmids = 6,
        Dayflies = 7,
        Stag_Beetles = 8,
        Ladybugs = 9,
        Grasshoppers = 10,
        Beetles = 11,
        Pill_Bugs = 12,
        Snails = 13,
        Dragonflies = 14,
    }

    public class TradeGroupUtils
    {
        public static readonly byte NumBitsToEncode = 4;

        private static Dictionary<TradeGroup, string> enumToStr;
        private static readonly Dictionary<TradeGroup, HashSet<Item>> tradeGroupToItems =
            new()
            {
                {
                    TradeGroup.Male_Bugs,
                    new()
                    {
                        Item.Male_Ant,
                        Item.Male_Mantis,
                        Item.Male_Butterfly,
                        Item.Male_Phasmid,
                        Item.Male_Dayfly,
                        Item.Male_Stag_Beetle,
                        Item.Male_Ladybug,
                        Item.Male_Grasshopper,
                        Item.Male_Beetle,
                        Item.Male_Pill_Bug,
                        Item.Male_Snail,
                        Item.Male_Dragonfly,
                    }
                },
                {
                    TradeGroup.Female_Bugs,
                    new()
                    {
                        Item.Female_Ant,
                        Item.Female_Mantis,
                        Item.Female_Butterfly,
                        Item.Female_Phasmid,
                        Item.Female_Dayfly,
                        Item.Female_Stag_Beetle,
                        Item.Female_Ladybug,
                        Item.Female_Grasshopper,
                        Item.Female_Beetle,
                        Item.Female_Pill_Bug,
                        Item.Female_Snail,
                        Item.Female_Dragonfly,
                    }
                },
                {
                    TradeGroup.Ants,
                    new() { Item.Male_Ant, Item.Female_Ant, }
                },
                {
                    TradeGroup.Mantises,
                    new() { Item.Male_Mantis, Item.Female_Mantis, }
                },
                {
                    TradeGroup.Butterflies,
                    new() { Item.Male_Butterfly, Item.Female_Butterfly, }
                },
                {
                    TradeGroup.Phasmids,
                    new() { Item.Male_Phasmid, Item.Female_Phasmid, }
                },
                {
                    TradeGroup.Dayflies,
                    new() { Item.Male_Dayfly, Item.Female_Dayfly, }
                },
                {
                    TradeGroup.Stag_Beetles,
                    new() { Item.Male_Stag_Beetle, Item.Female_Stag_Beetle, }
                },
                {
                    TradeGroup.Ladybugs,
                    new() { Item.Male_Ladybug, Item.Female_Ladybug, }
                },
                {
                    TradeGroup.Grasshoppers,
                    new() { Item.Male_Grasshopper, Item.Female_Grasshopper, }
                },
                {
                    TradeGroup.Beetles,
                    new() { Item.Male_Beetle, Item.Female_Beetle, }
                },
                {
                    TradeGroup.Pill_Bugs,
                    new() { Item.Male_Pill_Bug, Item.Female_Pill_Bug, }
                },
                {
                    TradeGroup.Snails,
                    new() { Item.Male_Snail, Item.Female_Snail, }
                },
                {
                    TradeGroup.Dragonflies,
                    new() { Item.Male_Dragonfly, Item.Female_Dragonfly, }
                },
            };

        static TradeGroupUtils()
        {
            enumToStr = new();
            foreach (TradeGroup tradeGroup in Enum.GetValues(typeof(TradeGroup)))
            {
                enumToStr[tradeGroup] = tradeGroup.ToString();
            }
        }

        public static TradeGroup StringToId(string str)
        {
            TradeGroup tradeGroup;
            bool success = Enum.TryParse(str, true, out tradeGroup);
            if (success)
                return tradeGroup;
            else
                return TradeGroup.Invalid;
        }

        public static string IdToString(TradeGroup tradeGroup)
        {
            if (enumToStr.ContainsKey(tradeGroup))
                return enumToStr[tradeGroup];
            return null;
        }

        public static HashSet<Item> ResolveToItems(TradeGroup tradeGroup)
        {
            if (!tradeGroupToItems.ContainsKey(tradeGroup))
                throw new Exception($"Failed to resolve TradeGroup '{tradeGroup}' to items.");
            return tradeGroupToItems[tradeGroup];
        }

        public static string GenResKey(TradeGroup tradeGroup)
        {
            return $"trade-group.{tradeGroup.ToString().ToLowerInvariant()}";
        }
    }

    public class BeyondPointObj
    {
        public enum Validity
        {
            AlwaysPass,
            Snowpeak,
            Dungeon,
        }

        private static readonly Func<HintGenData, Zone, bool> alwaysPassFn = (genData, zone) =>
            true;
        private static readonly Func<HintGenData, Zone, bool> snowpeakFn = (genData, zone) =>
            genData.sSettings.shufflePoes != PoeSettings.All
            && genData.sSettings.shufflePoes != PoeSettings.Overworld
            && genData.sSettings.barrenDungeons
            && !HintUtils.DungeonIsRequired(ZoneUtils.IdToString(Zone.Snowpeak_Ruins));
        private static readonly Func<HintGenData, Zone, bool> dungeonFn = (genData, zone) =>
            ZoneUtils.IsDungeonZone(zone)
            && (
                !genData.sSettings.barrenDungeons
                || HintUtils.DungeonIsRequired(ZoneUtils.IdToString(zone))
            );

        public Zone zone { get; private set; }
        public SpotId spotId { get; private set; }
        public HintCategory category { get; private set; }
        private Func<HintGenData, Zone, bool> canHintFunc;

        public BeyondPointObj(Zone zone, SpotId spotId, HintCategory category, Validity validity)
        {
            this.zone = zone;
            this.spotId = spotId;
            this.category = category;

            switch (validity)
            {
                case Validity.AlwaysPass:
                    this.canHintFunc = alwaysPassFn;
                    break;
                case Validity.Snowpeak:
                    this.canHintFunc = snowpeakFn;
                    break;
                case Validity.Dungeon:
                    this.canHintFunc = dungeonFn;
                    break;
                default:
                    throw new Exception(
                        $"Cannot create HintBeyondPointObj with Validity '{validity}'."
                    );
            }
        }

        public bool CanBeHinted(HintGenData genData)
        {
            return !genData.hinted.hintedBarrenZones.Contains(zone) && canHintFunc(genData, zone);
        }
    }

    public class HintConstants
    {
        public static readonly Dictionary<string, Province> zoneToProvince =
            new()
            {
                { "Agitha's Castle", Province.Lanayru },
                { "Golden Wolf", Province.Split },
                { "Ordon", Province.Ordona },
                { "Sacred Grove", Province.Faron },
                { "Faron Field", Province.Faron },
                { "Faron Woods", Province.Faron },
                { "Kakariko Gorge", Province.Eldin },
                { "Kakariko Village", Province.Eldin },
                { "Kakariko Graveyard", Province.Eldin },
                { "Eldin Field", Province.Eldin },
                { "North Eldin", Province.Eldin },
                { "Death Mountain", Province.Eldin },
                { "Hidden Village", Province.Eldin },
                { "Lanayru Field", Province.Lanayru },
                { "Beside Castle Town", Province.Lanayru },
                { "South of Castle Town", Province.Lanayru },
                { "Castle Town", Province.Lanayru },
                { "Great Bridge of Hylia", Province.Lanayru },
                { "Lake Hylia", Province.Lanayru },
                { "Lake Lantern Cave", Province.Lanayru },
                { "Lanayru Spring", Province.Lanayru },
                { "Zora's Domain", Province.Lanayru },
                { "Upper Zora's River", Province.Lanayru },
                { "Gerudo Desert", Province.Desert },
                { "Bulblin Camp", Province.Desert },
                { "Snowpeak", Province.Peak },
                { "Cave of Ordeals", Province.Desert },
                { "Long Minigames", Province.Split },
                { "Forest Temple", Province.Dungeon },
                { "Goron Mines", Province.Dungeon },
                { "Lakebed Temple", Province.Dungeon },
                { "Arbiter's Grounds", Province.Dungeon },
                { "Snowpeak Ruins", Province.Dungeon },
                { "Temple of Time", Province.Dungeon },
                { "City in the Sky", Province.Dungeon },
                { "Palace of Twilight", Province.Dungeon },
                { "Hyrule Castle", Province.Dungeon },
            };

        public static readonly Dictionary<string, SpotId> hintZoneToHintSpotLocation =
            new()
            {
                { "Ordon", SpotId.Ordon_Sign },
                { "Sacred Grove", SpotId.Sacred_Grove_Sign },
                { "Faron Field", SpotId.Faron_Field_Sign },
                { "Faron Woods", SpotId.Faron_Woods_Sign },
                { "Kakariko Gorge", SpotId.Kakariko_Gorge_Sign },
                { "Kakariko Village", SpotId.Kakariko_Village_Sign },
                { "Kakariko Graveyard", SpotId.Kakariko_Graveyard_Sign },
                { "Eldin Field", SpotId.Eldin_Field_Sign },
                { "North Eldin", SpotId.North_Eldin_Sign },
                { "Death Mountain", SpotId.Death_Mountain_Sign },
                { "Hidden Village", SpotId.Hidden_Village_Sign },
                { "Lanayru Field", SpotId.Lanayru_Field_Sign },
                { "Beside Castle Town", SpotId.Beside_Castle_Town_Sign },
                { "South of Castle Town", SpotId.South_of_Castle_Town_Sign },
                { "Castle Town", SpotId.Castle_Town_Sign },
                { "Great Bridge of Hylia", SpotId.Great_Bridge_of_Hylia_Sign },
                { "Lake Hylia", SpotId.Lake_Hylia_Sign },
                { "Lake Lantern Cave", SpotId.Lake_Lantern_Cave_Sign },
                { "Lanayru Spring", SpotId.Lanayru_Spring_Sign },
                { "Zora's Domain", SpotId.Zoras_Domain_Sign },
                { "Upper Zora's River", SpotId.Upper_Zoras_River_Sign },
                { "Gerudo Desert", SpotId.Gerudo_Desert_Sign },
                { "Bulblin Camp", SpotId.Bulblin_Camp_Sign },
                { "Snowpeak", SpotId.Snowpeak_Sign },
                { "Cave of Ordeals", SpotId.Cave_of_Ordeals_Sign },
            };

        public static readonly Dictionary<string, SpotId> dungeonZoneToSpotLocation =
            new()
            {
                { "Forest Temple", SpotId.Forest_Temple_Sign },
                { "Goron Mines", SpotId.Goron_Mines_Sign },
                { "Lakebed Temple", SpotId.Lakebed_Temple_Sign },
                { "Arbiter's Grounds", SpotId.Arbiters_Grounds_Sign },
                { "Snowpeak Ruins", SpotId.Snowpeak_Ruins_Sign },
                { "Temple of Time", SpotId.Temple_of_Time_Sign },
                { "City in the Sky", SpotId.City_in_the_Sky_Sign },
                { "Palace of Twilight", SpotId.Palace_of_Twilight_Sign },
                { "Hyrule Castle", SpotId.Hyrule_Castle_Sign },
            };

        public static readonly Dictionary<string, byte> dungeonZonesToRequiredMaskMap =
            new()
            {
                { "Forest Temple", 0x01 },
                { "Goron Mines", 0x02 },
                { "Lakebed Temple", 0x04 },
                { "Arbiter's Grounds", 0x08 },
                { "Snowpeak Ruins", 0x10 },
                { "Temple of Time", 0x20 },
                { "City in the Sky", 0x40 },
                { "Palace of Twilight", 0x80 },
            };

        public static readonly Dictionary<string, string> jsonCategoryToDungeonZoneName =
            new()
            {
                { "Forest Temple", "Forest Temple" },
                { "Goron Mines", "Goron Mines" },
                { "Lakebed Temple", "Lakebed Temple" },
                { "Arbiters Grounds", "Arbiter's Grounds" },
                { "Snowpeak Ruins", "Snowpeak Ruins" },
                { "Temple of Time", "Temple of Time" },
                { "City in the Sky", "City in The Sky" },
                { "Palace of Twilight", "Palace of Twilight" },
                // Note: Hidden Village maps to Temple of Time
                { "Hidden Village", "Temple of Time" },
            };

        // Note: this list might need to be determined using settings. MDH
        // stuff, etc. Might be good enough though.
        public static readonly Dictionary<string, string> postDungeonChecksToDungeonZone =
            new()
            {
                { "Talo Sharpshooting", "Goron Mines" },
                { "Kakariko Village Malo Mart Hawkeye", "Goron Mines" },
                { "Death Mountain Trail Poe", "Goron Mines" },
                { "Snowboard Racing Prize", "Snowpeak Ruins" },
                { "Doctors Office Balcony Chest", "Temple of Time" },
                { "Renados Letter", "Temple of Time" },
                { "Telma Invoice", "Temple of Time" },
                { "Wooden Statue", "Temple of Time" },
                { "Ilia Memory Reward", "Temple of Time" },
            };

        public static readonly HashSet<string> preventBarrenHintIfAllCheckStatusesAre =
            new() { "Excluded", "Excluded-Unrequired", "Vanilla" };

        public static readonly HashSet<string> excludedCheckStatuses =
            new() { "Excluded", "Excluded-Unrequired", };

        public static readonly HashSet<string> dungeonZones =
            new()
            {
                "Forest Temple",
                "Goron Mines",
                "Lakebed Temple",
                "Arbiter's Grounds",
                "Snowpeak Ruins",
                "Temple of Time",
                "City in the Sky",
                "Palace of Twilight",
                "Hyrule Castle",
            };

        public static readonly Dictionary<Item, string> bigKeyToDungeonZone =
            new()
            {
                { Item.Forest_Temple_Big_Key, "Forest Temple" },
                { Item.Goron_Mines_Key_Shard, "Goron Mines" },
                { Item.Lakebed_Temple_Big_Key, "Lakebed Temple" },
                { Item.Arbiters_Grounds_Big_Key, "Arbiter's Grounds" },
                { Item.Snowpeak_Ruins_Bedroom_Key, "Snowpeak Ruins" },
                { Item.Temple_of_Time_Big_Key, "Temple of Time" },
                { Item.City_in_The_Sky_Big_Key, "City in the Sky" },
                { Item.Palace_of_Twilight_Big_Key, "Palace of Twilight" },
                { Item.Hyrule_Castle_Big_Key, "Hyrule Castle" },
            };

        public static readonly Dictionary<Item, string> bugsToRewardChecksMap =
            new()
            {
                { Item.Female_Ant, "Agitha Female Ant Reward" },
                { Item.Female_Beetle, "Agitha Female Beetle Reward" },
                { Item.Female_Butterfly, "Agitha Female Butterfly Reward" },
                { Item.Female_Dayfly, "Agitha Female Dayfly Reward" },
                { Item.Female_Dragonfly, "Agitha Female Dragonfly Reward" },
                { Item.Female_Grasshopper, "Agitha Female Grasshopper Reward" },
                { Item.Female_Ladybug, "Agitha Female Ladybug Reward" },
                { Item.Female_Mantis, "Agitha Female Mantis Reward" },
                { Item.Female_Phasmid, "Agitha Female Phasmid Reward" },
                { Item.Female_Pill_Bug, "Agitha Female Pill Bug Reward" },
                { Item.Female_Snail, "Agitha Female Snail Reward" },
                { Item.Female_Stag_Beetle, "Agitha Female Stag Beetle Reward" },
                { Item.Male_Ant, "Agitha Male Ant Reward" },
                { Item.Male_Beetle, "Agitha Male Beetle Reward" },
                { Item.Male_Butterfly, "Agitha Male Butterfly Reward" },
                { Item.Male_Dayfly, "Agitha Male Dayfly Reward" },
                { Item.Male_Dragonfly, "Agitha Male Dragonfly Reward" },
                { Item.Male_Grasshopper, "Agitha Male Grasshopper Reward" },
                { Item.Male_Ladybug, "Agitha Male Ladybug Reward" },
                { Item.Male_Mantis, "Agitha Male Mantis Reward" },
                { Item.Male_Phasmid, "Agitha Male Phasmid Reward" },
                { Item.Male_Pill_Bug, "Agitha Male Pill Bug Reward" },
                { Item.Male_Snail, "Agitha Male Snail Reward" },
                { Item.Male_Stag_Beetle, "Agitha Male Stag Beetle Reward" }
            };

        // Items which are guaranteed to only unlock a single check and serve no
        // other purpose. Currently this is the bugs and Ashei's Sketch.
        public static readonly Dictionary<Item, string> singleCheckItems;
        public static readonly Dictionary<Province, string> provinceToString =
            new()
            {
                { Province.Invalid, "Invalid" },
                { Province.Ordona, "Ordona" },
                { Province.Faron, "Faron" },
                { Province.Eldin, "Eldin" },
                { Province.Lanayru, "Lanayru" },
                { Province.Desert, "Desert" },
                { Province.Peak, "Peak" },
                { Province.Split, "Split" },
                { Province.Dungeon, "Dungeon" },
            };

        // Gets inited using `provinceToString`.
        public static readonly Dictionary<string, Province> stringToProvince;

        // This Set is intentionally overly protective by including a large
        // number of items, some of which could probably technically be left
        // out. But it doesn't hurt.
        public static readonly HashSet<Item> invalidSpolItems =
            new()
            {
                // Exclude items which are not helpful
                Item.Poe_Soul,
                Item.Progressive_Hidden_Skill,
                // Slingshot has about a 0.02% chance of being a SpoL item
                // (results were 12 out of 5858 seeds when tested). The chance
                // that is gets selected as a SpoL hint is even lower (less than
                // 1/1000). We had this show up in a seed once, and everyone had
                // no idea what the SpoL hint was talking about. Didn't figure
                // it out until checking the spoiler log after the race.
                // Needless to say, everyone kept going back to the same area
                // the entire time since no one knew what the hint meant. Given
                // that boss keys are not hinted since they are not as useful as
                // something like a Clawshot, going ahead and adding Slingshot
                // to invalidSpolItems since it is even less helpful than boss
                // keys (and is actually kind of harmful). Keep in mind that the
                // chance Slingshot was a SpoL hint was less than 1/1000, and
                // this change has no impact on whether or not Slingshot is
                // required. We just no longer give SpoL hints for Slingshot
                // since it is not helpful in the slightest.
                // Item.Slingshot,
                Item.Wooden_Shield,
                Item.Ordon_Shield,
                Item.Hylian_Shield,
                // Exclude FusedShadows and MirrorShards
                Item.Progressive_Mirror_Shard,
                Item.Mirror_Piece_3,
                Item.Mirror_Piece_4,
                Item.Progressive_Fused_Shadow,
                Item.Fused_Shadow_2,
                Item.Fused_Shadow_3,
                // Exclude dungeon items. (You could maybe have keys in when
                // Keysanity is on for example, but I think people would prefer
                // to have SpoL hints about things like the Boomerang, etc., so
                // the implementation is the same regardless of the settings for
                // now)
                Item.Big_Key,
                Item.Small_Key,
                Item.Compass,
                Item.Dungeon_Map,
                Item.Forest_Temple_Big_Key,
                Item.Forest_Temple_Small_Key,
                Item.Forest_Temple_Compass,
                Item.Forest_Temple_Dungeon_Map,
                Item.Goron_Mines_Big_Key,
                Item.Goron_Mines_Small_Key,
                Item.Goron_Mines_Compass,
                Item.Goron_Mines_Dungeon_Map,
                Item.Goron_Mines_Key_Shard,
                Item.Goron_Mines_Key_Shard_Second,
                Item.Goron_Mines_Key_Shard_3,
                Item.Lakebed_Temple_Big_Key,
                Item.Lakebed_Temple_Small_Key,
                Item.Lakebed_Temple_Compass,
                Item.Lakebed_Temple_Dungeon_Map,
                Item.Arbiters_Grounds_Big_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Arbiters_Grounds_Compass,
                Item.Arbiters_Grounds_Dungeon_Map,
                Item.Poe_Scent,
                Item.Snowpeak_Ruins_Bedroom_Key,
                // I think SPR compass actually unlocks a check, so should be valid
                // to add to the list. The other maps and compasses are just to be
                // safe. Could potentially base around a whitelist instead of a
                // blacklist.
                Item.Snowpeak_Ruins_Compass,
                Item.Snowpeak_Ruins_Ordon_Goat_Cheese,
                Item.Snowpeak_Ruins_Ordon_Pumpkin,
                Item.Snowpeak_Ruins_Small_Key,
                Item.Snowpeak_Ruins_Dungeon_Map,
                Item.Reekfish_Scent,
                Item.Temple_of_Time_Big_Key,
                Item.Temple_of_Time_Small_Key,
                Item.Temple_of_Time_Compass,
                Item.Temple_of_Time_Dungeon_Map,
                Item.City_in_The_Sky_Big_Key,
                Item.City_in_The_Sky_Small_Key,
                Item.City_in_The_Sky_Compass,
                Item.City_in_The_Sky_Dungeon_Map,
                Item.Palace_of_Twilight_Big_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Compass,
                Item.Palace_of_Twilight_Dungeon_Map,
                Item.Hyrule_Castle_Big_Key,
                Item.Hyrule_Castle_Small_Key,
                Item.Hyrule_Castle_Compass,
                Item.Hyrule_Castle_Dungeon_Map,
            };

        static HintConstants()
        {
            singleCheckItems = HintConstants.bugsToRewardChecksMap.ToDictionary(
                entry => entry.Key,
                entry => entry.Value
            );
            singleCheckItems.Add(Item.Asheis_Sketch, "Gift From Ralis");

            stringToProvince = new();
            foreach (KeyValuePair<Province, string> pair in provinceToString)
            {
                stringToProvince[pair.Value] = pair.Key;
            }
        }
    }
}
