namespace TPRandomizer.Hints.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints.HintCreator;
    using TPRandomizer.SSettings.Enums;
    using TPRandomizer.Util;

    class HintSettingUtils
    {
        private static Dictionary<string, Func<HintGenData, bool>> checkToConditions;
        private static HashSet<string> defaultAlwaysChecks;
        private static HashSet<string> defaultSometimesChecks;

        static HintSettingUtils()
        {
            checkToConditions = new();
            defaultAlwaysChecks = new()
            {
                "Cats Hide and Seek Minigame",
                "Iza Helping Hand",
                "Iza Raging Rapids Minigame",
                "Lanayru Ice Block Puzzle Cave Chest",
                "Goron Springwater Rush",
                "Plumm Fruit Balloon Minigame",
                "Palace of Twilight Collect Both Sols"
            };

            Dictionary<string, Func<HintGenData, bool>> conditionalAlways =
                new()
                {
                    // {
                    //     "Lake Hylia Shell Blade Grotto Chest",
                    //     // Only hint when the poe next to the grotto is excluded
                    //     // or vanilla
                    //     (genData) => HintUtils.checkIsPlayerKnownStatus("Flight By Fowl Ledge Poe")
                    // },
                    {
                        "Snowpeak Icy Summit Poe",
                        // Only hint when the poe is shuffled and there is no
                        // reason to go to SPR (unrequired is barren and SPR is
                        // unrequired).
                        (genData) =>
                            !HintUtils.checkIsPlayerKnownStatus("Snowpeak Icy Summit Poe")
                            && genData.sSettings.barrenDungeons
                            && !HintUtils.getRequiredDungeonZones().Contains("Snowpeak Ruins")
                    },
                };
            foreach (KeyValuePair<string, Func<HintGenData, bool>> pair in conditionalAlways)
            {
                defaultAlwaysChecks.Add(pair.Key);
                checkToConditions.Add(pair.Key, pair.Value);
            }

            defaultSometimesChecks = new()
            {
                // "Outside South Castle Town Double Clawshot Chasm Chest", // no point
                // "West Hyrule Field Helmasaur Grotto Chest", // takes away from better ones
                // "Lake Hylia Bridge Vines Chest", // not great
                // "Lake Hylia Water Toadpoli Grotto Chest", // not great
                // "Temple of Time Lobby Lantern Chest", // never great when it shows up
                // "Kakariko Graveyard Lantern Chest", // already have Ralis; lantern is near warp

                // "Links Basement Chest", // This is just a worse ranch grotto hint
                // Since both Ordon lantern checks are together, hinting one
                // dead just makes you less likely to check the other one
                // which could actually be unhelpful if it mattered.
                // "Ordon Ranch Grotto Lantern Chest",

                // Ordon - cat
                // SG - spinner and lantern
                // FF - under bridge
                // FW - ?? not really anything good; mist category kind of helps
                // KG - cave lantern and DCS
                // KV - underwater chest
                // KGY - Ralis (removed Lantern since near warp)
                // EF - bomb and lantern
                // NE - lava cave lantern
                // DM - chest
                // HV - --covered by Always; no singular
                // LF - underwater, lantern, spinner
                // BCT - --not needed
                // CT - Star 2
                // SoCT - Tightrope and fountain
                // GBoH - ??
                // LH - underwater
                // LLC - --not needed at all
                // LS - lantern
                // ZD - goron, lantern, rang
                // UZR - --not needed
                // GD - lantern
                // BC - lantern
                // SP - --covered by beyondPoint
                // GW - --not needed
                // CoO - --covered
                // LMG - --always
                // dungeons

                "Wrestling With Bo",
                "Herding Goats Reward",
                "Lost Woods Lantern Chest",
                // "Sacred Grove Baba Serpent Grotto Chest", // takes slot for better hints
                "Sacred Grove Spinner Chest",
                "Faron Field Bridge Chest",
                "Eldin Lantern Cave Lantern Chest",
                "Kakariko Gorge Double Clawshot Chest",
                "Eldin Spring Underwater Chest",
                // "Kakariko Village Bomb Rock Spire Heart Piece", // takes slot for better hints
                // "Kakariko Village Malo Mart Hawkeye",
                // "Kakariko Watchtower Alcove Chest", // not needed
                // "Talo Sharpshooting",
                "Gift From Ralis",
                "Eldin Field Bomb Rock Chest",
                "Eldin Field Bomskit Grotto Lantern Chest",
                "Eldin Stockcave Lantern Chest",
                "Death Mountain Alcove Chest",
                // "Death Mountain Trail Poe",
                "Lanayru Field Behind Gate Underwater Chest",
                "Lanayru Field Skulltula Grotto Chest",
                "Lanayru Field Spinner Track Chest",
                "Outside South Castle Town Fountain Chest",
                "Outside South Castle Town Tightrope Chest",
                "STAR Prize 2",
                "Lake Hylia Underwater Chest",
                "Lake Hylia Shell Blade Grotto Chest",
                "Lanayru Spring Back Room Lantern Chest",
                "Zoras Domain Extinguish All Torches Chest",
                "Zoras Domain Light All Torches Chest",
                "Zoras Domain Underwater Goron",
                "Gerudo Desert Rock Grotto Lantern Chest",
                "Outside Arbiters Grounds Lantern Chest",
                "Snowboard Racing Prize",
                // "Snowpeak Cave Ice Lantern Chest", // Seemed kind of worthless when it showed up
                "Forest Temple Gale Boomerang",
                "Lakebed Temple Deku Toad Chest",
                "Arbiters Grounds Death Sword Chest",
                // This saves you a little time, but it is nowhere near as
                // helpful as an easily isolated check like Bomskit grotto.
                // "Snowpeak Ruins Chapel Chest",
                "City in The Sky Aeralfos Chest",
            };

            // TODO: Filter out certain sometimes hints if they are sphere0
            // (probably stick to ones which require only a single item?).
            // Need to adjust params to pass this info in.

            Dictionary<string, Func<HintGenData, bool>> conditionalSometimes =
                new()
                {
                    { "Ordon Cat Rescue", genNotSphere0Lambda("Ordon Cat Rescue") },
                    // Fishing Hole Bottle is hinted by checking the sign next to it now
                    // { "Fishing Hole Bottle", genNotSphere0Lambda("Fishing Hole Bottle") },
                };
            foreach (KeyValuePair<string, Func<HintGenData, bool>> pair in conditionalSometimes)
            {
                defaultSometimesChecks.Add(pair.Key);
                checkToConditions.Add(pair.Key, pair.Value);
            }
        }

        private static Func<HintGenData, bool> genNotSphere0Lambda(string checkName)
        {
            return (genData) => !genData.isCheckSphere0(checkName);
        }

        public static T fromJObject<T>(JObject obj)
        {
            foreach (KeyValuePair<string, JToken> pair in obj)
            {
                if (typeof(T).GetProperty(pair.Key) == null)
                {
                    throw new Exception(
                        $"Property '{pair.Key}' does not exist on class {typeof(T)}"
                    );
                }
            }

            return obj.ToObject<T>();
        }

        public static bool getBool(JToken obj, string propertyName)
        {
            JToken token = obj[propertyName];
            if (token.Type == JTokenType.Boolean)
                return (bool)token;
            throw new Exception($"Property '{propertyName}' on JObject was not a bool.");
        }

        public static bool getOptionalBool(JObject obj, string propertyName, bool defaultVal)
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getBool(obj, propertyName);
        }

        public static int getInt(JToken obj, string propertyName)
        {
            JToken token = obj[propertyName];
            if (token.Type == JTokenType.Integer)
                return (int)token;
            throw new Exception($"Property '{propertyName}' on JObject was not an int.");
        }

        public static int getOptionalInt(JObject obj, string propertyName, int defaultVal)
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getInt(obj, propertyName);
        }

        public static int? getOptionalNullableInt(JObject obj, string propertyName, int? defaultVal)
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            JToken token = obj[propertyName];
            if (token.Type == JTokenType.Null)
                return null;
            return getInt(obj, propertyName);
        }

        public static double getDouble(JToken obj, string propertyName)
        {
            JToken token = obj[propertyName];
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                return (double)token;
            throw new Exception(
                $"Property '{propertyName}' on JObject was not an integer or float."
            );
        }

        public static double getOptionalDouble(JObject obj, string propertyName, double defaultVal)
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getDouble(obj, propertyName);
        }

        public static string getString(JObject obj, string propertyName)
        {
            JToken token = obj[propertyName];
            if (token.Type == JTokenType.String)
                return (string)token;
            throw new Exception($"Property '{propertyName}' on JObject was not a string.");
        }

        public static string getOptionalString(JObject obj, string propertyName, string defaultVal)
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getString(obj, propertyName);
        }

        public static List<string> getOptionalStringList(
            JObject obj,
            string propertyName,
            List<string> defaultVal
        )
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getStringList(obj, propertyName);
        }

        public static List<string> getStringList(JObject obj, string propertyName)
        {
            JToken token = obj[propertyName];
            if (token.Type != JTokenType.Array)
                throw new Exception($"Property '{propertyName}' on JObject was not an array.");

            JArray arr = (JArray)token;
            List<string> ret = new();

            foreach (JToken entry in arr)
            {
                if (entry.Type != JTokenType.String)
                    throw new Exception(
                        $"Entry in array was expected to be a string, but was '{entry.Type}'."
                    );

                ret.Add((string)entry);
            }
            return ret;
        }

        public static Item parseItem(string itemName)
        {
            if (Enum.TryParse(itemName, true, out Item item))
                return item;
            else
                throw new Exception($"Failed to parse itemName '{itemName}' to Item enum.");
        }

        public static CheckStatus parseCheckStatus(string statusStr)
        {
            if (Enum.TryParse(statusStr, true, out CheckStatus status))
                return status;
            else
                throw new Exception(
                    $"Failed to parse statusStr '{statusStr}' to CheckStatus enum."
                );
        }

        public static CheckStatusDisplay getOptionalCheckStatusDisplay(
            JObject obj,
            string propertyName,
            CheckStatusDisplay defaultVal
        )
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;

            JToken token = obj[propertyName];
            if (token.Type != JTokenType.String)
                throw new Exception(
                    $"Expected checkStatusDisplay token to be a string, but was '{token.Type}'."
                );

            if (Enum.TryParse((string)token, true, out CheckStatusDisplay checkStatusDisplay))
                return checkStatusDisplay;
            else
                throw new Exception(
                    $"Failed to parse CheckStatusDisplay '{propertyName}' to enum."
                );
        }

        public static HashSet<Item> getItemSet(JObject obj, string propertyName)
        {
            HashSet<Item> items = new();
            List<String> contents = HintSettingUtils.getStringList(obj, propertyName);
            foreach (string itemName in contents)
            {
                Item item = parseItem(itemName);
                items.Add(item);
            }
            return items;
        }

        public static HashSet<Item> getOptionalItemSet(
            JObject obj,
            string propertyName,
            HashSet<Item> defaultVal
        )
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getItemSet(obj, propertyName);
        }

        public static List<Item> getItemList(JObject obj, string propertyName)
        {
            List<Item> items = new();
            List<string> contents = getStringList(obj, propertyName);
            foreach (string itemName in contents)
            {
                Item item = parseItem(itemName);
                items.Add(item);
            }
            return items;
        }

        public static List<Item> getOptionalItemList(
            JObject obj,
            string propertyName,
            List<Item> defaultVal
        )
        {
            if (!obj.ContainsKey(propertyName))
                return defaultVal;
            return getItemList(obj, propertyName);
        }

        public enum CheckListType
        {
            AlwaysChecks,
            SometimesChecks
        }

        private static HashSet<string> loadChecksList(JToken token, HintGenData genData)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            HashSet<string> result = new();

            JArray arr = (JArray)token;
            foreach (JToken el in arr)
            {
                if (el.Type != JTokenType.String)
                    throw new Exception("All elements in the checksList must be strings.");

                string checkName = (string)el;
                if (!CheckIdClass.IsValidCheckName(checkName))
                    throw new Exception($"'{checkName}' is not a valid checkName.");

                if (checkToConditions.ContainsKey(checkName))
                {
                    if (checkToConditions[checkName](genData))
                        result.Add(checkName);
                }
                else
                {
                    result.Add(checkName);
                }
            }

            return result;
        }

        public static HashSet<string> loadBaseAndAddChecksList(
            CheckListType checkListType,
            JToken token,
            HintSettings hintSettings,
            HintGenData genData
        )
        {
            HashSet<string> currentSet = null;
            HashSet<string> defaultSet = null;
            JObject obj = null;

            if (token != null && token.Type == JTokenType.Object)
                obj = (JObject)token;

            if (obj != null)
                defaultSet = loadChecksList(obj["baseChecks"], genData);

            if (defaultSet == null)
            {
                switch (checkListType)
                {
                    case CheckListType.AlwaysChecks:
                        defaultSet = new(defaultAlwaysChecks);
                        break;
                    case CheckListType.SometimesChecks:
                        defaultSet = new(defaultSometimesChecks);
                        break;
                    default:
                        throw new Exception($"Unrecognized CheckListType '{checkListType}'.");
                }
            }

            currentSet = defaultSet;

            string addChecksKey =
                checkListType == CheckListType.AlwaysChecks ? "always" : "sometimes";

            currentSet.UnionWith(hintSettings.addChecks[addChecksKey]);

            // Filter based on conditions
            HashSet<string> result = new();
            foreach (string checkName in currentSet)
            {
                if (
                    !HintUtils.checkIsPlayerKnownStatus(checkName)
                    && (
                        !checkToConditions.ContainsKey(checkName)
                        || checkToConditions[checkName](genData)
                    )
                )
                    result.Add(checkName);
            }
            return result;
        }

        public static bool IsVarDefinition(string str)
        {
            if (StringUtils.isEmpty(str))
                return false;
            Regex r = new Regex(@"^var:(\w+)(?:\.(\w+))?$");
            Match match = r.Match(str);
            return match.Success;
        }

        public static KeyValuePair<string, string> ParseVarDefinition(string str)
        {
            Regex r = new Regex(@"^var:(\w+)(?:\.(\w+))?$");
            Match match = r.Match(str);
            if (!match.Success)
                throw new Exception($"Failed to parse varDef '{str}'.");

            string varName = match.Groups[1].Value;
            string property = null;
            if (match.Groups[2].Success)
                property = match.Groups[2].Value;

            return new(varName, property);
        }

        public static bool IsAreaDefinition(string str)
        {
            if (StringUtils.isEmpty(str))
                return false;

            Regex r = new Regex(@"^(province|zone|category)\.\w+$");
            return r.Match(str).Success;
        }

        public static bool IsValidCheckResolutionFormat(string name)
        {
            if (IsVarDefinition(name))
                return true;

            if (IsAreaDefinition(name))
            {
                // Resolve as an area.
                AreaId areaId = AreaId.ParseString(name);
                // Throws is fails to resolve, so never expecting it to be null here.
                return areaId != null;
            }

            // Resolve as checkName.
            if (!CheckIdClass.IsValidCheckName(name))
                throw new Exception($"Failed to resolve '{name}' as a checkName.");
            return true;
        }
    }

    public class Always
    {
        public bool starting { get; private set; } = false;
        public string groupId { get; private set; }
        public bool monopolizeSpots { get; private set; } = true;
        public CheckStatusDisplay checkStatusDisplay { get; private set; } =
            CheckStatusDisplay.Required_Info;
        public int? idealNumSpots { get; private set; }
        public int? idealNumExplicitlyHinted { get; private set; }
        public int copies { get; private set; } = 1;
        public HashSet<string> checks { get; private set; }

        private Always() { }

        public static Always fromJToken(
            JToken token,
            HintSettings hintSettings,
            HintGenData genData
        )
        {
            // If no object provided, there will be no Always hints since the
            // groupId must be specified in order to know where to place the
            // hints.
            if (token == null || token.Type != JTokenType.Object)
                return null;

            JObject obj = (JObject)token;
            Always inst = new Always();

            inst.starting = HintSettingUtils.getOptionalBool(obj, "starting", inst.starting);

            if (!inst.starting)
            {
                inst.groupId = HintSettingUtils.getString(obj, "groupId");
                if (!hintSettings.groups.ContainsKey(inst.groupId))
                    throw new Exception($"No group found for always.groupId '{inst.groupId}'.");
            }

            inst.monopolizeSpots = HintSettingUtils.getOptionalBool(
                obj,
                "monopolizeSpots",
                inst.monopolizeSpots
            );

            inst.checkStatusDisplay = HintSettingUtils.getOptionalCheckStatusDisplay(
                obj,
                "checkStatusDisplay",
                inst.checkStatusDisplay
            );

            inst.idealNumSpots = HintSettingUtils.getOptionalNullableInt(
                obj,
                "idealNumSpots",
                inst.idealNumSpots
            );
            if (inst.idealNumSpots < 1)
                throw new Exception(
                    $"'Always' optional property `idealNumSpots` must be greater than 0, but received '{inst.idealNumSpots}'."
                );

            inst.idealNumExplicitlyHinted = HintSettingUtils.getOptionalNullableInt(
                obj,
                "idealNumExplicitlyHinted",
                inst.idealNumExplicitlyHinted
            );
            if (inst.idealNumExplicitlyHinted < 0)
                throw new Exception(
                    $"'Always' optional property `idealNumExplicitlyHinted` must be greater than or equal to 0, but received '{inst.idealNumExplicitlyHinted}'."
                );

            inst.copies = HintSettingUtils.getOptionalInt(obj, "copies", inst.copies);
            if (inst.copies < 1)
                throw new Exception("always.copies must be greater than 0.");

            HashSet<string> checksSet = HintSettingUtils.loadBaseAndAddChecksList(
                HintSettingUtils.CheckListType.AlwaysChecks,
                obj,
                hintSettings,
                genData
            );

            // Filter based on hintSettings.
            HashSet<string> removeChecks = hintSettings.removeChecks["always"];

            if (removeChecks.Count > 0)
            {
                HashSet<string> filtered = new();
                foreach (string checkName in checksSet)
                {
                    if (!removeChecks.Contains(checkName))
                        filtered.Add(checkName);
                }
                inst.checks = filtered;
            }
            else
                inst.checks = checksSet;

            return inst;
        }
    }

    public class Barren
    {
        public enum OwnZoneBehavior
        {
            Off = 0,
            Prioritize = 1,
            Monopolize = 2,
        }

        public OwnZoneBehavior ownZoneBehavior { get; private set; } = OwnZoneBehavior.Off;
        public bool ownZoneShowsAsJunkHint { get; private set; } = false;

        private Barren() { }

        public static Barren fromJToken(JToken token)
        {
            Barren inst = new Barren();

            if (token != null && token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;

                string ownZoneBehaviorStr = HintSettingUtils.getOptionalString(
                    obj,
                    "ownZoneBehavior",
                    null
                );

                if (!StringUtils.isEmpty(ownZoneBehaviorStr))
                {
                    OwnZoneBehavior ownZoneBehavior;
                    bool success = Enum.TryParse(ownZoneBehaviorStr, true, out ownZoneBehavior);
                    if (success)
                        inst.ownZoneBehavior = ownZoneBehavior;
                    else
                        throw new Exception(
                            $"Failed to parse ownZoneBehavior '{ownZoneBehaviorStr}' to OwnZoneBehavior enum."
                        );
                }

                inst.ownZoneShowsAsJunkHint = HintSettingUtils.getOptionalBool(
                    obj,
                    "ownZoneShowsAsJunkHint",
                    inst.ownZoneShowsAsJunkHint
                );
            }

            return inst;
        }

        public bool isMonopolize()
        {
            return ownZoneBehavior == OwnZoneBehavior.Monopolize;
        }
    }

    public class Starting
    {
        public SpotId spot { get; private set; } = SpotId.Ordon_Sign;
        public bool excludeFromGroups { get; private set; } = false;

        private Starting() { }

        public static Starting fromJToken(JToken token)
        {
            Starting inst = new Starting();

            if (token != null)
            {
                if (token.Type != JTokenType.Object)
                    throw new Exception(
                        $"Expected 'starting' to be an object, but received '{token.Type}'."
                    );

                JObject obj = (JObject)token;

                inst.spot = parseSpot(obj, inst.spot);

                inst.excludeFromGroups = HintSettingUtils.getOptionalBool(
                    obj,
                    "excludeFromGroups",
                    inst.excludeFromGroups
                );
            }

            return inst;
        }

        private static SpotId parseSpot(JObject obj, SpotId defaultSpotId)
        {
            string spotStr = HintSettingUtils.getOptionalString(obj, "spot", null);
            if (StringUtils.isEmpty(spotStr))
                return defaultSpotId;

            SpotId spotId;
            bool success = Enum.TryParse(spotStr, true, out spotId);
            if (!success)
                throw new Exception($"Failed to parse starting.spot '{spotStr}' to SpotId enum.");

            HashSet<SpotId> validStartingSpots =
                new()
                {
                    SpotId.Ordon_Sign,
                    SpotId.Sacred_Grove_Sign,
                    SpotId.Faron_Field_Sign,
                    SpotId.Faron_Woods_Sign,
                    SpotId.Kakariko_Gorge_Sign,
                    SpotId.Kakariko_Village_Sign,
                    SpotId.Kakariko_Graveyard_Sign,
                    SpotId.Eldin_Field_Sign,
                    SpotId.North_Eldin_Sign,
                    SpotId.Death_Mountain_Sign,
                    SpotId.Hidden_Village_Sign,
                    SpotId.Lanayru_Field_Sign,
                    SpotId.Beside_Castle_Town_Sign,
                    SpotId.South_of_Castle_Town_Sign,
                    SpotId.Castle_Town_Sign,
                    SpotId.Great_Bridge_of_Hylia_Sign,
                    SpotId.Lake_Hylia_Sign,
                    SpotId.Lake_Lantern_Cave_Sign,
                    SpotId.Lanayru_Spring_Sign,
                    SpotId.Zoras_Domain_Sign,
                    SpotId.Upper_Zoras_River_Sign,
                    SpotId.Gerudo_Desert_Sign,
                    SpotId.Bulblin_Camp_Sign,
                    SpotId.Snowpeak_Sign,
                    SpotId.Cave_of_Ordeals_Sign,
                    SpotId.Forest_Temple_Sign,
                    SpotId.Goron_Mines_Sign,
                    SpotId.Lakebed_Temple_Sign,
                    SpotId.Arbiters_Grounds_Sign,
                    SpotId.Snowpeak_Ruins_Sign,
                    SpotId.Temple_of_Time_Sign,
                    SpotId.City_in_the_Sky_Sign,
                    SpotId.Palace_of_Twilight_Sign,
                    SpotId.Hyrule_Castle_Sign,
                };

            if (!validStartingSpots.Contains(spotId))
                throw new Exception($"Spot '{spotStr}' is not a valid starting.spot.");

            return spotId;
        }
    }

    public class Jovani
    {
        public int? minSoulsForHint { get; private set; } = null;
        public int? minFoundSoulsForHint { get; private set; } = null;

        private Jovani() { }

        public static Jovani fromJToken(JToken token)
        {
            Jovani inst = new Jovani();

            if (token != null)
            {
                if (token.Type != JTokenType.Object)
                    throw new Exception(
                        $"'jovani' must be null or an object, but type was '{token.Type}'."
                    );

                JObject obj = (JObject)token;

                inst.minSoulsForHint = HintSettingUtils.getOptionalNullableInt(
                    obj,
                    "minSoulsForHint",
                    inst.minSoulsForHint
                );
                if (inst.minSoulsForHint < 0)
                    throw new Exception(
                        $"jovani.minSoulsForHint must be null or non-negative, but received '{inst.minSoulsForHint}'."
                    );

                inst.minFoundSoulsForHint = HintSettingUtils.getOptionalNullableInt(
                    obj,
                    "minFoundSoulsForHint",
                    inst.minFoundSoulsForHint
                );
                if (inst.minFoundSoulsForHint < 0)
                    throw new Exception(
                        $"jovani.minFoundSoulsForHint must be null or non-negative, but received '{inst.minFoundSoulsForHint}'."
                    );
            }

            return inst;
        }
    }

    public class Dungeons
    {
        public bool bigKeyHints { get; private set; } = true;
        public int? maxBarrenHints { get; private set; } = 1;
        public int? maxWothHints { get; private set; } = 2;

        private Dungeons() { }

        public static Dungeons fromJToken(JToken token)
        {
            Dungeons inst = new Dungeons();

            if (token != null && token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;

                inst.bigKeyHints = HintSettingUtils.getOptionalBool(
                    obj,
                    "bigKeyHints",
                    inst.bigKeyHints
                );

                inst.maxBarrenHints = HintSettingUtils.getOptionalNullableInt(
                    obj,
                    "maxBarrenHints",
                    inst.maxBarrenHints
                );
                if (inst.maxBarrenHints != null && ((int)inst.maxBarrenHints < 0))
                    throw new Exception(
                        $"dungeons.maxBarrenHints must be null or non-negative, but received '{inst.maxBarrenHints}'."
                    );

                inst.maxWothHints = HintSettingUtils.getOptionalNullableInt(
                    obj,
                    "maxWothHints",
                    inst.maxWothHints
                );
                if (inst.maxWothHints != null && ((int)inst.maxWothHints < 0))
                    throw new Exception(
                        $"dungeons.maxWothHints must be null or non-negative, but received '{inst.maxWothHints}'."
                    );
            }

            return inst;
        }
    }

    public class HintDef
    {
        public enum SelectionType
        {
            Basic,
            RandomOrder,
            RandomWeighted,
        }

        public int iterations { get; private set; } = 1;
        public int maxPicks { get; private set; } = -1;
        public int copies { get; private set; } = -1;
        public int minCopies { get; private set; } = 0;
        public int starting { get; private set; } = 0;
        public double weight { get; private set; } = 1.0;
        public string saveToVar { get; private set; }

        public SelectionType selectionType { get; private set; } = SelectionType.Basic;
        public HintCreator hintCreator { get; private set; }
        public List<HintDef> hintDefs { get; private set; } = new();

        public static HintDef fromJToken(JToken token)
        {
            HintDef inst = new HintDef();

            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;

                inst.iterations = HintSettingUtils.getOptionalInt(
                    obj,
                    "iterations",
                    inst.iterations
                );
                int? maxPicksVal = HintSettingUtils.getOptionalNullableInt(obj, "maxPicks", null);
                if (maxPicksVal != null)
                {
                    int maxPicksInt = (int)maxPicksVal;
                    if (maxPicksInt <= 0)
                        throw new Exception($"'maxPicks' must be greater than 0 if specified.");
                    inst.maxPicks = maxPicksInt;
                }
                inst.copies = HintSettingUtils.getOptionalInt(obj, "copies", inst.copies);
                inst.minCopies = HintSettingUtils.getOptionalInt(obj, "minCopies", inst.minCopies);
                inst.starting = HintSettingUtils.getOptionalInt(obj, "starting", inst.starting);
                inst.weight = HintSettingUtils.getOptionalDouble(obj, "weight", inst.weight);
                if (inst.weight < 0.01 || inst.weight > 1000)
                    throw new Exception(
                        $"Expected weight to be at most 1000 and at least 0.01, but was '{inst.weight}'."
                    );
                inst.saveToVar = HintSettingUtils.getOptionalString(
                    obj,
                    "saveToVar",
                    inst.saveToVar
                );

                bool definesHintType = obj.ContainsKey("hintType");
                bool definesHintDef = obj.ContainsKey("hintDef");

                if (definesHintType && definesHintDef)
                    throw new Exception(
                        "Cannot define both 'hintDef' and 'hintType' on a single hintDef."
                    );

                if (obj.ContainsKey("selectionType"))
                {
                    if (definesHintType)
                        throw new Exception(
                            "It is not valid to define 'selectionType' on an object which defines 'hintType'."
                        );

                    string selectionTypeStr = HintSettingUtils.getOptionalString(
                        obj,
                        "selectionType",
                        null
                    );
                    if (!StringUtils.isEmpty(selectionTypeStr))
                    {
                        SelectionType selectionType;
                        bool success = Enum.TryParse(selectionTypeStr, true, out selectionType);
                        if (success)
                            inst.selectionType = selectionType;
                        else
                            throw new Exception(
                                $"Failed to parse selectionType '{selectionTypeStr}' to SelectionType enum."
                            );
                    }
                }

                if (definesHintType)
                {
                    inst.hintCreator = HintCreator.fromJObject(obj);
                }
                else if (definesHintDef)
                {
                    JToken hintDef = obj["hintDef"];
                    if (hintDef.Type == JTokenType.Array)
                    {
                        JArray arr = (JArray)hintDef;
                        foreach (JToken tkn in arr)
                        {
                            inst.hintDefs.Add(fromJToken(tkn));
                        }
                    }
                    else
                    {
                        inst.hintDefs.Add(fromJToken(hintDef));
                    }
                }
                else
                {
                    throw new Exception(
                        "Expected object to have property 'hintDef' or 'hintType', but found neither."
                    );
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray arr = (JArray)token;
                foreach (JToken obj in arr)
                {
                    inst.hintDefs.Add(fromJToken(obj));
                }
            }
            else
            {
                throw new Exception("'hintDef' must be an array or an object.");
            }

            return inst;
        }
    }

    public class HintGroup : ICloneable
    {
        public string id { get; private set; }
        public HashSet<SpotId> spots = new();

        private static Dictionary<string, SpotId> overworldZoneToSpot =
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

        private static Dictionary<string, SpotId> dungeonZoneToSpot =
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

        private static Dictionary<string, SpotId> zoneToSpot;
        private static Dictionary<SpotId, string> spotToZone;

        // Note: important that we only include zones that users are actually
        // allowed to define. For example, they are not allowed to put hints on
        // the Agitha sign or ToT middle sign.
        private static Dictionary<string, string> entryNameToZone =
            new()
            {
                { "ordon", "Ordon" },
                { "sacredgrove", "SacredGrove" },
                { "faronfield", "Faron Field" },
                { "faronwoods", "Faron Woods" },
                { "kakarikogorge", "Kakariko Gorge" },
                { "kakarikovillage", "Kakariko Village" },
                { "kakarikograveyard", "Kakariko Graveyard" },
                { "eldinfield", "Eldin Field" },
                { "northeldin", "North Eldin" },
                { "deathmountain", "Death Mountain" },
                { "hiddenvillage", "Hidden Village" },
                { "lanayrufield", "Lanayru Field" },
                { "besidecastletown", "Beside Castle Town" },
                { "southofcastletown", "South of Castle Town" },
                { "castletown", "Castle Town" },
                { "greatbridgeofhylia", "Great Bridge of Hylia" },
                { "lakehylia", "Lake Hylia" },
                { "lakelanterncave", "Lake Lantern Cave" },
                { "lanayruspring", "Lanayru Spring" },
                { "zorasdomain", "Zora's Domain" },
                { "upperzorasriver", "Upper Zora's River" },
                { "gerudodesert", "Gerudo Desert" },
                { "bulblincamp", "Bulblin Camp" },
                { "snowpeak", "Snowpeak" },
                { "caveofordeals", "Cave of Ordeals" },
                { "foresttemple", "Forest Temple" },
                { "goronmines", "Goron Mines" },
                { "lakebedtemple", "Lakebed Temple" },
                { "arbitersgrounds", "Arbiter's Grounds" },
                { "snowpeakruins", "Snowpeak Ruins" },
                { "templeoftime", "Temple of Time" },
                { "cityinthesky", "City in the Sky" },
                { "palaceoftwilight", "Palace of Twilight" },
                { "hyrulecastle", "Hyrule Castle" },
            };

        static HintGroup()
        {
            zoneToSpot = new(overworldZoneToSpot.Count + dungeonZoneToSpot.Count);
            foreach (KeyValuePair<string, SpotId> pair in overworldZoneToSpot)
            {
                zoneToSpot[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<string, SpotId> pair in dungeonZoneToSpot)
            {
                zoneToSpot[pair.Key] = pair.Value;
            }

            spotToZone = new(zoneToSpot.Count);
            foreach (KeyValuePair<string, SpotId> pair in zoneToSpot)
            {
                spotToZone[pair.Value] = pair.Key;
            }
        }

        public static HintGroup fromSpotId(string id, SpotId spotId)
        {
            HintGroup ret = new HintGroup();
            ret.id = id;
            ret.spots = new HashSet<SpotId> { spotId };

            return ret;
        }

        public static HintGroup fromJObject(string id, JToken arrToken)
        {
            HintGroup ret = new HintGroup();
            ret.id = id;

            JArray arr = (JArray)arrToken;
            foreach (JToken token in arr)
            {
                if (token.Type != JTokenType.String)
                    throw new Exception("All values in 'groups' lists must be strings.");

                HashSet<SpotId> spots = resolveGroupEntry(token.ToString());
                ret.spots.UnionWith(spots);
            }

            // Remove spots which are completely player-known if hints are set
            // up to work this way (which probably doesn't even need a
            // setting?).

            // TODO: this removes HC from the list when checks are all excluded.
            // Need a way to specify spots for certain groups even when they are
            // excluded. For example, we may want to specify which groups get a
            // bigKey hint which can include HC even when it would normally be
            // excluded.

            // There may be a case where we want to specify all dungeons when
            // unrequiredDungeonsAreBarren are off as well. Imagine that PoT is
            // required and FT is not. It might be the case that PoT BK is on
            // Diababa HC, so the FT BK becomes required but there is no hint
            // for it. We should only not hint the BK of a dungeon when it is
            // guaranteed to be completely unrequired and not HC. This can use
            // the exact same logic for filtering (contains all
            // knownPlayerStatus checks and is not HC).

            // In this case, can have this all handled by a simple
            // "includeBigKeyHints" which only gets included when BK is
            // AnyDungeon or Keysanity. Can adjust the setting in the future if
            // there is a request, but this should cover everything. If you
            // wanted the BK hinted for OwnDungeon, you should realistically be
            // playing on Keysy. Don't expect this feature request to ever
            // happen (Sign says specific check of own dungeon which has BK).
            HashSet<SpotId> filteredSpots = new();

            Dictionary<string, string[]> zoneToChecks = HintUtils.getHintZoneToChecksMap();

            foreach (SpotId spot in ret.spots)
            {
                string zoneName = spotToZone[spot];
                string[] checkNames = zoneToChecks[zoneName];
                // Do not filter out Hyrule Castle even if all HC checks are a
                // known status since it is expected that players will always
                // have to pass that hint sign.
                if (zoneName == "Hyrule Castle")
                    filteredSpots.Add(spot);
                else
                {
                    foreach (string checkName in checkNames)
                    {
                        if (!HintUtils.checkIsPlayerKnownStatus(checkName))
                        {
                            // Spot passes filter if contains check with a
                            // non-playerKnown status.
                            filteredSpots.Add(spot);
                            break;
                        }
                    }
                }
            }

            ret.spots = filteredSpots;
            return ret;
        }

        private static HashSet<SpotId> resolveGroupEntry(string valIn)
        {
            if (StringUtils.isEmpty(valIn))
                throw new Exception("Hint group entry must be a non-empty string.");

            string val = valIn.ToLower();
            HashSet<SpotId> ret = new();

            if (val.StartsWith("alias:"))
            {
                string alias = val.Substring(6);
                switch (alias)
                {
                    case "overworldzones":
                        foreach (KeyValuePair<string, SpotId> pair in overworldZoneToSpot)
                        {
                            ret.Add(pair.Value);
                        }
                        break;
                    case "dungeonzones":
                        foreach (KeyValuePair<string, SpotId> pair in dungeonZoneToSpot)
                        {
                            ret.Add(pair.Value);
                        }
                        break;
                    case "requireddungeons":
                    {
                        HashSet<string> requiredDungeonZones = HintUtils.getRequiredDungeonZones();
                        foreach (string zoneName in requiredDungeonZones)
                        {
                            ret.Add(zoneToSpot[zoneName]);
                        }
                        break;
                    }
                    default:
                        throw new Exception($"Failed to resolve group entry alias '{alias}'.");
                }
            }
            else
            {
                string zoneName = entryNameToZone[val];
                if (!zoneToSpot.ContainsKey(zoneName))
                    throw new Exception($"Failed to resolve group entry '{val}'.");
                ret.Add(zoneToSpot[zoneName]);
            }

            return ret;
        }

        public object Clone()
        {
            HintGroup clone = (HintGroup)this.MemberwiseClone();
            clone.spots = new(this.spots);
            return clone;
        }
    }

    public class UserSpecialHintDef
    {
        public SpotId spotId { get; private set; }
        public HintDef hintDef { get; private set; } = new();

        public static UserSpecialHintDef fromJObject(JObject obj)
        {
            UserSpecialHintDef ret = new();

            string spotStr = HintSettingUtils.getOptionalString(obj, "spot", null);
            if (StringUtils.isEmpty(spotStr))
                throw new Exception("'spot' for specialHintDefs must be a non-empty string.");

            if (!Enum.TryParse(spotStr, true, out SpotId spotId))
                throw new Exception($"Failed to parse starting.spot '{spotStr}' to SpotId enum.");

            if (spotId == SpotId.Invalid)
                throw new Exception("Parsed 'spot' for specialHintDefs to 'Invalid'.");

            ret.spotId = spotId;
            ret.hintDef = HintDef.fromJToken(obj["hintDef"]);

            return ret;
        }
    }

    public class HintDefGrouping
    {
        public string groupId { get; private set; }
        public bool useFillerHints { get; private set; } = true;
        public HintDef hintDef { get; private set; } = new();

        public static HintDefGrouping fromJObject(Dictionary<string, HintGroup> groups, JObject obj)
        {
            HintDefGrouping ret = new();

            string groupId = HintSettingUtils.getString(obj, "groupId");
            if (!groups.ContainsKey(groupId))
                throw new Exception($"Group id '{groupId}' was not defined in 'groups'.");

            bool useFillerHints = HintSettingUtils.getOptionalBool(
                obj,
                "useFillerHints",
                ret.useFillerHints
            );

            ret.groupId = groupId;
            ret.useFillerHints = useFillerHints;
            ret.hintDef = HintDef.fromJToken(obj["hintDef"]);

            return ret;
        }
    }

    public class HintSettings
    {
        public Starting starting { get; private set; }
        public bool agitha { get; private set; }
        public Jovani jovani { get; private set; }
        public bool caveOfOrdeals { get; private set; }
        public HashSet<string> invalidSelfHinters { get; private set; }
        public Dungeons dungeons { get; private set; }
        public HashSet<Zone> beyondPointZones { get; private set; }
        public Always always { get; private set; }
        public Barren barren { get; private set; }
        public HashSet<string> sometimesChecks { get; private set; }
        public Dictionary<string, HashSet<string>> addChecks { get; private set; }
        public Dictionary<string, HashSet<string>> removeChecks { get; private set; }
        public Dictionary<string, HashSet<Item>> addItems { get; private set; }
        public Dictionary<string, HashSet<Item>> removeItems { get; private set; }
        public Dictionary<string, HintGroup> groups { get; private set; } = new();
        public List<UserSpecialHintDef> specialHintDefs { get; private set; } = new();
        public List<HintDefGrouping> hintDefGroupings { get; private set; } = new();

        public static HintSettings fromPath(HintGenData genData)
        {
            if (genData.sSettings.hintDistribution == HintDistribution.None)
                return null;

            string jsonPath = ResolveJsonPath(genData);
            string contents = File.ReadAllText(jsonPath);

            JObject root = JObject.Parse(contents);

            HintSettings ret = new HintSettings();
            ret.addChecks = loadAddChecks(root);
            ret.removeChecks = loadRemoveChecks(root);
            ret.addItems = loadAddItems(root);
            ret.removeItems = loadRemoveItems(root);
            ret.groups = loadGroups(root["groups"]);

            ret.starting = Starting.fromJToken(root["starting"]);

            ret.agitha = HintSettingUtils.getOptionalBool(root, "agitha", true);
            ret.jovani = Jovani.fromJToken(root["jovani"]);
            ret.caveOfOrdeals = HintSettingUtils.getOptionalBool(root, "caveOfOrdeals", true);
            ret.invalidSelfHinters = loadInvalidSelfHinters(root);
            ret.dungeons = Dungeons.fromJToken(root["dungeons"]);
            ret.beyondPointZones = loadBeyondPointZones(root);
            ret.always = Always.fromJToken(root["always"], ret, genData);
            ret.barren = Barren.fromJToken(root["barren"]);
            ret.sometimesChecks = ret.loadSometimesChecks(root["sometimes"], genData);

            ret.specialHintDefs = loadSpecialHintDefs(root["specialHintDefs"]);
            ret.hintDefGroupings = loadHintDefGroupings(ret.groups, root["hints"]);

            ret.validate();

            genData.updateFromHintSettings(ret);

            return ret;
        }

        private static string ResolveJsonPath(HintGenData genData)
        {
            string basePath = Global.CombineRootPath("./Assets/HintDistributions");

            switch (genData.sSettings.hintDistribution)
            {
                case HintDistribution.Balanced:
                    return Path.Combine(basePath, "balanced.jsonc");
                case HintDistribution.Blossom:
                    return Path.Combine(basePath, "blossom.jsonc");
                case HintDistribution.Strong:
                    return Path.Combine(basePath, "strong.jsonc");
                case HintDistribution.Very_Strong:
                    return Path.Combine(basePath, "very-strong.jsonc");
                case HintDistribution.Weak:
                    return Path.Combine(basePath, "weak.jsonc");
                default:
                    throw new Exception(
                        $"Unrecognized HintDistribution '{genData.sSettings.hintDistribution}'."
                    );
            }
        }

        private static Dictionary<string, HashSet<T>> loadKeyToTypeList<T>(
            JToken root,
            string name,
            HashSet<string> validKeys,
            Func<string, T> converter,
            Action<string> handleInvalidType
        )
        {
            JToken token = root[name];

            Dictionary<string, HashSet<T>> ret = new();
            foreach (string key in validKeys)
            {
                ret.Add(key, new());
            }

            if (token == null || token.Type == JTokenType.Null)
                return ret;
            else if (token.Type != JTokenType.Object)
                throw new Exception($"Expected {name} to be an object, but was '{token.Type}'.");

            JObject obj = (JObject)token;
            foreach (KeyValuePair<string, JToken> pair in obj)
            {
                T objKey = converter(pair.Key);

                if (pair.Value.Type != JTokenType.Array)
                    throw new Exception($"{name} value must be a string array.");

                JArray arr = (JArray)pair.Value;
                if (arr.Count < 1)
                    throw new Exception($"{name} arrays must not be empty.");

                foreach (JToken typeToken in arr)
                {
                    if (typeToken.Type != JTokenType.String)
                        throw new Exception(
                            $"{name} array entries must be strings, but received '{typeToken.Type}'."
                        );
                    string type = (string)typeToken;

                    if (!validKeys.Contains(type))
                    {
                        if (handleInvalidType != null)
                            handleInvalidType(type);
                        // Must always throw even if the handler does not.
                        throw new Exception($"'{type}' is not a valid type for {name}.");
                    }
                    ret[type].Add(objKey);
                }
            }

            return ret;
        }

        private static Dictionary<string, HashSet<string>> loadAddChecks(JObject root)
        {
            HashSet<string> validKeys = new() { "always", "sometimes", };

            return loadKeyToTypeList<string>(
                root,
                "addChecks",
                validKeys,
                (checkName) =>
                {
                    if (!CheckIdClass.IsValidCheckName(checkName))
                        throw new Exception($"'{checkName}' is not a valid checkName.");
                    return checkName;
                },
                null
            );
        }

        private static Dictionary<string, HashSet<string>> loadRemoveChecks(JObject root)
        {
            HashSet<string> validKeys = new() { "always", "sometimes", };

            return loadKeyToTypeList<string>(
                root,
                "removeChecks",
                validKeys,
                (checkName) =>
                {
                    if (!CheckIdClass.IsValidCheckName(checkName))
                        throw new Exception($"'{checkName}' is not a valid checkName.");
                    return checkName;
                },
                null
            );
        }

        private static Dictionary<string, HashSet<Item>> loadAddItems(JObject root)
        {
            HashSet<string> validKeys = new() { "majorItems" };

            return loadKeyToTypeList<Item>(
                root,
                "addItems",
                validKeys,
                HintSettingUtils.parseItem,
                (type) =>
                {
                    if (type == "sometimes")
                        throw new Exception(
                            "Cannot add valid items to sometimes hints as all items are valid by default. To make certain items invalid, use 'removeItems'. To define exactly which items are valid, use 'sometimes.validItems'."
                        );
                }
            );
        }

        private static Dictionary<string, HashSet<Item>> loadRemoveItems(JObject root)
        {
            HashSet<string> validKeys = new() { "majorItems", "sometimes", };

            return loadKeyToTypeList<Item>(
                root,
                "removeItems",
                validKeys,
                HintSettingUtils.parseItem,
                null
            );
        }

        private static HashSet<string> loadInvalidSelfHinters(JObject root)
        {
            HashSet<string> result = new();

            List<string> strList = HintSettingUtils.getOptionalStringList(
                root,
                "invalidSelfHinters",
                null
            );

            if (!ListUtils.isEmpty(strList))
            {
                foreach (string str in strList)
                {
                    if (StringUtils.isEmpty(str))
                        throw new Exception("Expected selfHinter string to not be empty.");

                    if (str.StartsWith("alias:"))
                    {
                        if (str != "alias:all")
                            throw new Exception($"Invalid selfHinter alias '{str}'.");

                        result.Add(str);
                    }
                    else
                    {
                        if (!CheckIdClass.IsValidCheckName(str))
                            throw new Exception($"checkname '{str}' is invalid.");
                        result.Add(str);
                    }
                }
            }

            return result;
        }

        private static HashSet<Zone> loadBeyondPointZones(JObject root)
        {
            HashSet<Zone> result = null;

            List<string> strList = HintSettingUtils.getOptionalStringList(
                root,
                "beyondPointZones",
                null
            );
            if (strList != null)
            {
                result = new();
                foreach (string str in strList)
                {
                    if (str.StartsWith("alias:"))
                    {
                        string aliasStr = str.Substring(6).ToLowerInvariant();
                        switch (aliasStr)
                        {
                            case "all":
                                result.UnionWith(
                                    new HashSet<Zone>()
                                    {
                                        Zone.Lake_Lantern_Cave,
                                        Zone.Snowpeak,
                                        Zone.Goron_Mines,
                                        Zone.Lakebed_Temple,
                                        Zone.Arbiters_Grounds,
                                        Zone.Temple_of_Time,
                                        Zone.City_in_the_Sky
                                    }
                                );
                                break;
                            case "overworld":
                                result.UnionWith(
                                    new HashSet<Zone>() { Zone.Lake_Lantern_Cave, Zone.Snowpeak, }
                                );
                                break;
                            case "dungeons":
                                result.UnionWith(
                                    new HashSet<Zone>()
                                    {
                                        Zone.Goron_Mines,
                                        Zone.Lakebed_Temple,
                                        Zone.Arbiters_Grounds,
                                        Zone.Temple_of_Time,
                                        Zone.City_in_the_Sky
                                    }
                                );
                                break;
                            default:
                                throw new Exception(
                                    $"Cannot resolve alias '{aliasStr}' for beyondPointZones."
                                );
                        }
                    }
                    else
                    {
                        bool success = Enum.TryParse(str, true, out Zone zone);
                        if (!success)
                            throw new Exception(
                                $"Failed to parse string '{str}' to HintZone enum."
                            );
                        else if (!ZoneUtils.idToBeyondPointData.ContainsKey(zone))
                            throw new Exception(
                                $"HintZone '{zone}' is not a valid zone for beyondPoint hints."
                            );

                        result.Add(zone);
                    }
                }
            }

            if (result == null)
                result = new();

            return result;
        }

        private HashSet<string> loadSometimesChecks(JToken token, HintGenData genData)
        {
            HashSet<string> set = HintSettingUtils.loadBaseAndAddChecksList(
                HintSettingUtils.CheckListType.SometimesChecks,
                token,
                this,
                genData
            );

            HashSet<Item> validItems = null;
            if (token != null && token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;
                validItems = HintSettingUtils.getOptionalItemSet(obj, "validItems", null);

                if (validItems != null && validItems.Count == 0)
                    return new();
            }

            HashSet<string> removeChecks = this.removeChecks["sometimes"];
            HashSet<Item> removeItems = this.removeItems["sometimes"];

            HashSet<string> filtered = new();
            foreach (string checkName in set)
            {
                Item item = HintUtils.getCheckContents(checkName);
                if (
                    !removeChecks.Contains(checkName)
                    && !removeItems.Contains(item)
                    && (validItems == null || validItems.Contains(item))
                )
                    filtered.Add(checkName);
            }

            return filtered;
        }

        private static Dictionary<string, HintGroup> loadGroups(JToken token)
        {
            if (token == null)
                throw new Exception(
                    "Cannot create groups from null token. Please define 'groups' in the hints configuration."
                );

            Dictionary<string, HintGroup> ret = new();
            // token is object
            JObject obj = (JObject)token;

            foreach (KeyValuePair<string, JToken> pair in obj)
            {
                string id = pair.Key;
                if (StringUtils.isEmpty(id))
                    throw new Exception("Group id must be a non-empty string.");

                ret[id] = HintGroup.fromJObject(id, pair.Value);
            }

            return ret;
        }

        private static List<UserSpecialHintDef> loadSpecialHintDefs(JToken token)
        {
            if (token == null)
                return new();

            List<UserSpecialHintDef> ret = new();
            JArray rootArr = (JArray)token;

            foreach (JToken objToken in rootArr)
            {
                JObject obj = (JObject)objToken;
                UserSpecialHintDef specialHintDef = UserSpecialHintDef.fromJObject(obj);
                ret.Add(specialHintDef);
            }

            return ret;
        }

        private static List<HintDefGrouping> loadHintDefGroupings(
            Dictionary<string, HintGroup> groups,
            JToken token
        )
        {
            if (token == null)
                return new();

            List<HintDefGrouping> ret = new();
            JArray rootArr = (JArray)token;

            foreach (JToken objToken in rootArr)
            {
                JObject obj = (JObject)objToken;
                HintDefGrouping grouping = HintDefGrouping.fromJObject(groups, obj);
                ret.Add(grouping);
            }

            return ret;
        }

        public Dictionary<string, HintGroup> createMutableGroups()
        {
            Dictionary<string, HintGroup> ret = new();
            foreach (KeyValuePair<string, HintGroup> pair in groups)
            {
                ret[pair.Key] = (HintGroup)pair.Value.Clone();
            }
            return ret;
        }

        private void validate()
        {
            if (barren.ownZoneShowsAsJunkHint && !ListUtils.isEmpty(hintDefGroupings))
            {
                // No JunkHintCreators can be specified in hintDefGroupings when
                // this setting is on.
                for (int i = 0; i < hintDefGroupings.Count; i++)
                {
                    HintDef hintDef = hintDefGroupings[i].hintDef;
                    if (hasJunkHintCreators(hintDef))
                    {
                        throw new Exception(
                            "When barren.ownZoneShowsAsJunkHint is true, JunkHintCreators cannot be defined in hintDefGroupings."
                        );
                    }
                }
            }

            // When barren is monopolize, we need to ensure that only the first
            // layer defines Barren Zone hints. This is because a later layer's
            // barren hint can need to claim a spot which already has hints on
            // it.
            if (barren.isMonopolize() && !ListUtils.isEmpty(hintDefGroupings))
            {
                // Iterate through all but the first hintDefGrouping and make
                // sure that they do not have any 'zone' BarrenHintCreators.
                for (int i = 1; i < hintDefGroupings.Count; i++)
                {
                    HintDef hintDef = hintDefGroupings[i].hintDef;
                    if (hasBarrenZoneHintCreators(hintDef))
                    {
                        throw new Exception(
                            "When barren ownZoneBehavior is 'monopolize', only the first hintDefGrouping can define BarrenHintCreators which hint zones."
                        );
                    }
                }
            }

            if (always != null && !always.starting)
            {
                if (ListUtils.isEmpty(hintDefGroupings))
                    throw new Exception(
                        "When 'always' hints are not starting hints, there must be at least one hintDefGrouping defined."
                    );

                // Always hints are always non-special, and since we cannot have
                // non-special hints on hintedBarren zones when barren zones are
                // 'monopolize', we must ensure that Always and Barren zones do
                // not overlap. This means that when barrenZones are
                // 'monopolize', we must define the Always hints as part of the
                // first layer even if always.monopolizeSpots is false.
                if (always.monopolizeSpots || barren.isMonopolize())
                {
                    // When Always is set to monopolize, it means that spots
                    // which contain Always hints cannot contain any other
                    // non-special hints. Therefore its groupId MUST ALWAYS be
                    // the first layer's groupId.

                    // This allows us to make the needed modifications to all
                    // groups before any other layer tries to make use of spots
                    // that it might not have access to.

                    // It is not possible to know how other groups would be
                    // impacted without actually picking the spots to use,
                    // meaning it is not possible to know the size of a group.
                    // It is mandatory that we know the size so we know how many
                    // hints to generate, so this is a hard requirement.
                    if (hintDefGroupings[0].groupId != always.groupId)
                    {
                        throw new Exception(
                            "When barren.ownZoneBehavior is 'monopolize' or always.monopolizeSpots is true, the first hintDefGrouping must have the same groupId as 'always'."
                        );
                    }
                }
                else
                {
                    // We must have a group to use for placing the Always hints
                    // when they are not starting hints. Validation to make sure
                    // the group is not empty is handled when we need to place
                    // the Always hints since we might not actually need to
                    // place any hints.
                    bool foundMatchingGroupId = false;
                    for (int i = 0; i < hintDefGroupings.Count; i++)
                    {
                        if (hintDefGroupings[i].groupId == always.groupId)
                        {
                            foundMatchingGroupId = true;
                            break;
                        }
                    }
                    if (!foundMatchingGroupId)
                        throw new Exception(
                            "When always hints are not starting, at least one hintDefGrouping must have the same groupId as 'always'."
                        );
                }
            }
        }

        private bool hasBarrenZoneHintCreators(HintDef hintDef)
        {
            if (hintDef.hintCreator != null)
            {
                if (hintDef.hintCreator.type == HintCreatorType.Barren)
                {
                    BarrenHintCreator bhCreator = hintDef.hintCreator as BarrenHintCreator;
                    if (bhCreator != null)
                        return bhCreator.HintsZone();
                }
            }
            else
            {
                List<HintDef> hintDefs = hintDef.hintDefs;
                for (int i = 0; i < hintDefs.Count; i++)
                {
                    if (hasBarrenZoneHintCreators(hintDefs[i]))
                        return true;
                }
            }

            return false;
        }

        private bool hasJunkHintCreators(HintDef hintDef)
        {
            if (hintDef.hintCreator != null)
            {
                if (hintDef.hintCreator.type == HintCreatorType.Junk)
                    return true;
            }
            else
            {
                List<HintDef> hintDefs = hintDef.hintDefs;
                for (int i = 0; i < hintDefs.Count; i++)
                {
                    if (hasJunkHintCreators(hintDefs[i]))
                        return true;
                }
            }

            return false;
        }
    }
}
