using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TPRandomizer.Util;
using TPRandomizer.Assets;

namespace TPRandomizer
{
    public class SeedGenResults
    {
        private static readonly string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        // meta
        public string seedId { get; } // not stored in the file
        public string timestamp { get; }
        public string imageVersion { get; }
        public string gitCommit { get; }

        // input
        public string settingsString { get; set; }
        public string seed { get; set; }
        public bool isRaceSeed { get; set; }

        // output
        public string playthroughName { get; set; }
        public Dictionary<int, byte> itemPlacements { get; }
        public byte requiredDungeons { get; set; }
        public List<List<KeyValuePair<int, Item>>> spheres { get; }
        public string entrances { get; }

        public SeedGenResults(string seedId, JObject inputJsonContents)
        {
            this.seedId = seedId;

            // Can read `version` as well if format ever changes and we need to
            // support multiple formats.

            JObject meta = (JObject)inputJsonContents["meta"];
            // timestamp is automatically converted to DateTime then to a string
            // if we cast to a string, so we have to manually make sure it
            // maintains the same format
            DateTime timestampAsDateTime = (DateTime)meta["ts"];
            timestamp = timestampAsDateTime.ToString(TimestampFormat);
            imageVersion = (string)meta["imgVer"];
            gitCommit = (string)meta["gitCmt"];

            JObject input = (JObject)inputJsonContents["input"];
            settingsString = (string)input["settings"];
            seed = (string)input["seed"];
            isRaceSeed = (int)input["race"] == 1;

            JObject output = (JObject)inputJsonContents["output"];
            this.playthroughName = (string)output["name"];
            this.itemPlacements = DecodeItemPlacements((string)output["itemPlacement"]);
            this.requiredDungeons = (byte)output["reqDungeons"];
            this.spheres = DecodeSpheres((string)output["spheres"]);
            this.entrances = DecodeEntrances((string)output["entrances"]);
        }

        public static string EncodeEntrances()
        {
            string encodedString = "";
            foreach (KeyValuePair<string, Room> roomEntry in Randomizer.Rooms.RoomDict)
            {
                //Console.WriteLine("checking room: " + roomEntry.Value.RoomName);
                foreach (Entrance entrance in roomEntry.Value.Exits)
                {
                    if (entrance.IsShuffled())
                    {
                        Console.WriteLine(
                            entrance.GetOriginalName()
                                + " is shuffled with "
                                + entrance.GetReplacedEntrance().GetOriginalName()
                        );
                        // Get the original entrance that the entrance leads to in vanilla
                        encodedString =
                            encodedString + entrance.GetReplacedEntrance().GetStage().ToString("X");
                        encodedString = encodedString + ",";
                        encodedString =
                            encodedString + entrance.GetReplacedEntrance().GetRoom().ToString("X");
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetReplacedEntrance().GetSpawn();
                        encodedString = encodedString + ",";
                        encodedString =
                            encodedString + entrance.GetReplacedEntrance().GetSpawnType();
                        encodedString = encodedString + ",";
                        encodedString =
                            encodedString + entrance.GetReplacedEntrance().GetParameters();
                        encodedString = encodedString + ",";

                        // Add new connection info
                        encodedString = encodedString + entrance.GetStage().ToString("X");
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetRoom().ToString("X");
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetSpawn();
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetSpawnType();
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetParameters();
                        encodedString = encodedString + ",";
                        entrance.SetAsUnshuffled();
                    }
                }
            }
            return encodedString;
        }

        public static string DecodeEntrances(string encodeString)
        {
            return encodeString;
        }

        public static string EncodeItemPlacements(SortedDictionary<int, byte> checkNumIdToItemId)
        {
            UInt16 version = 0;
            string result = SettingsEncoder.EncodeAsVlq16(version);

            if (checkNumIdToItemId.Count() == 0)
            {
                result += "0";
                return SettingsEncoder.EncodeAs6BitString(result);
            }

            result += "1";

            int smallest = checkNumIdToItemId.First().Key;
            int largest = checkNumIdToItemId.Last().Key;

            result += SettingsEncoder.EncodeNumAsBits(smallest, 9);
            result += SettingsEncoder.EncodeNumAsBits(largest, 9);

            string itemBits = "";

            for (int i = smallest; i <= largest; i++)
            {
                if (checkNumIdToItemId.ContainsKey(i))
                {
                    result += "1";
                    itemBits += SettingsEncoder.EncodeNumAsBits(checkNumIdToItemId[i], 8);
                }
                else
                {
                    result += "0";
                }
            }

            result += itemBits;

            return SettingsEncoder.EncodeAs6BitString(result);
        }

        private Dictionary<int, byte> DecodeItemPlacements(string sixCharString)
        {
            BitsProcessor processor = new BitsProcessor(
                SettingsEncoder.DecodeToBitString(sixCharString)
            );

            Dictionary<int, byte> checkNumIdToItemId = new();

            UInt16 version = processor.NextVlq16();

            if (!processor.NextBool())
            {
                return checkNumIdToItemId;
            }

            int smallest = processor.NextInt(9);
            int largest = processor.NextInt(9);

            List<int> checkIdsWithItemIds = new();

            for (int i = smallest; i <= largest; i++)
            {
                if (processor.NextBool())
                {
                    checkIdsWithItemIds.Add(i);
                }
            }

            for (int i = 0; i < checkIdsWithItemIds.Count; i++)
            {
                int checkId = checkIdsWithItemIds[i];
                byte itemId = processor.NextByte();
                checkNumIdToItemId[checkId] = itemId;
            }

            return checkNumIdToItemId;
        }

        public static string EncodeSpheres(List<List<KeyValuePair<int, Item>>> spheres)
        {
            UInt16 version = 0;
            string result = SettingsEncoder.EncodeAsVlq16(version);

            if (spheres != null) // In no logic, there is a chance that no items are reachable and therefore, no spheres will be possible
            {
                foreach (List<KeyValuePair<int, Item>> spherePairsList in spheres)
                {
                    result += SettingsEncoder.EncodeAsVlq16((UInt16)spherePairsList.Count);

                    foreach (KeyValuePair<int, Item> pair in spherePairsList)
                    {
                        result += SettingsEncoder.EncodeNumAsBits(pair.Key, 9); // checkId
                        result += SettingsEncoder.EncodeNumAsBits((int)pair.Value, 8); // itemId
                    }
                }
            }

            result += SettingsEncoder.EncodeAsVlq16(0); // empty sphere marks the end

            return SettingsEncoder.EncodeAs6BitString(result);
        }

        private List<List<KeyValuePair<int, Item>>> DecodeSpheres(string sixCharString)
        {
            BitsProcessor processor = new BitsProcessor(
                SettingsEncoder.DecodeToBitString(sixCharString)
            );

            List<List<KeyValuePair<int, Item>>> result = new();

            UInt16 version = processor.NextVlq16();

            while (true)
            {
                int numPairsInSphere = processor.NextVlq16();

                if (numPairsInSphere < 1)
                {
                    break;
                }

                List<KeyValuePair<int, Item>> spherePairs = new();

                for (int i = 0; i < numPairsInSphere; i++)
                {
                    int checkId = processor.NextInt(9);
                    Item itemId = (Item)processor.NextByte();

                    spherePairs.Add(new KeyValuePair<int, Item>(checkId, itemId));
                }
                result.Add(spherePairs);
            }

            return result;
        }

        // forceOutputEverything only exists so that we can print
        public string ToSpoilerString(
            SortedDictionary<string, string> sortedCheckNameToItemNameDict,
            bool prettyPrint,
            bool dangerouslyPrintFullRaceSpoiler = false
        )
        {
            // This method is very similar to the Builder's ToString. The
            // difference is that this is what is sent to the UI for the spoiler
            // log, whereas the ToString method's return value is written to the
            // input.json file. So we decode some values which

            Dictionary<string, object> root = new();

            if (dangerouslyPrintFullRaceSpoiler)
            {
                root.Add(
                    "raceSeedFullSpoiler",
                    "FOR INTERNAL USE ONLY. If you are seeing this and you should not be, please report it to the development team!"
                );
            }

            root.Add("playthroughName", playthroughName);
            root.Add("isRaceSeed", isRaceSeed);
            root.Add("seedString", seed);
            root.Add("settingsString", settingsString);
            root.Add("settings", SSettingsToDetailedDict());

            if (!isRaceSeed || dangerouslyPrintFullRaceSpoiler)
            {
                root.Add("requiredDungeons", GetRequiredDungeonsStringList());
                root.Add("itemPlacements", sortedCheckNameToItemNameDict);
                root.Add("spheres", GetSpheresForSpoiler());
            }

            // Note this is the metaData from the file, not the current
            // imageVersion, etc.
            Dictionary<string, object> metaObj = new();
            root.Add("meta", metaObj);
            metaObj.Add("seedId", seedId);
            metaObj.Add("timestamp", timestamp);
            metaObj.Add("imageVersion", imageVersion);
            metaObj.Add("gitCommit", gitCommit);

            // Just increment "version" whenever you make a change to what this
            // method outputs
            root.Add(
                "version",
                "s" + Assets.SeedData.VersionMajor + "." + Assets.SeedData.VersionMinor
            );

            if (prettyPrint || dangerouslyPrintFullRaceSpoiler)
            {
                return JsonConvert.SerializeObject(root, Formatting.Indented);
            }
            return JsonConvert.SerializeObject(root);
        }

        private List<string> GetRequiredDungeonsStringList()
        {
            List<string> reqDungeonsList = new();

            foreach (RequiredDungeon reqDungeonEnum in Enum.GetValues(typeof(RequiredDungeon)))
            {
                if (((1 << (byte)reqDungeonEnum) & requiredDungeons) != 0)
                {
                    reqDungeonsList.Add(reqDungeonEnum.ToString());
                }
            }

            return reqDungeonsList;
        }

        private Dictionary<string, Dictionary<string, string>> GetSpheresForSpoiler()
        {
            Dictionary<string, Dictionary<string, string>> result = new();

            int sphereIndex = 0;
            foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
            {
                if (spherePairs.Count > 0)
                {
                    Dictionary<string, string> spherePairsMap = new();

                    foreach (KeyValuePair<int, Item> pair in spherePairs)
                    {
                        spherePairsMap.Add(
                            CheckIdClass.GetCheckName(pair.Key),
                            pair.Value.ToString()
                        );
                    }

                    result.Add("Sphere " + sphereIndex, spherePairsMap);
                }

                sphereIndex++;
            }

            return result;
        }

        private Dictionary<string, object> SSettingsToDetailedDict()
        {
            SharedSettings sSettings = SharedSettings.FromString(settingsString);

            Dictionary<string, object> result = new();

            result.Add("logicRules", sSettings.logicRules.ToString());
            result.Add("castleRequirements", sSettings.castleRequirements.ToString());
            result.Add("palaceRequirements", sSettings.palaceRequirements.ToString());
            result.Add("faronWoodsLogic", sSettings.faronWoodsLogic.ToString());
            result.Add("shuffleGoldenBugs", sSettings.shuffleGoldenBugs);
            result.Add("shuffleSkyCharacters", sSettings.shuffleSkyCharacters);
            result.Add("shuffleNpcItems", sSettings.shuffleNpcItems);
            result.Add("shufflePoes", sSettings.shufflePoes);
            result.Add("shuffleShopItems", sSettings.shuffleShopItems);
            result.Add("shuffleHiddenSkills", sSettings.shuffleHiddenSkills);
            result.Add("smallKeySettings", sSettings.smallKeySettings.ToString());
            result.Add("bigKeySettings", sSettings.bigKeySettings.ToString());
            result.Add("mapAndCompassSettings", sSettings.mapAndCompassSettings.ToString());
            result.Add("skipPrologue", sSettings.skipPrologue);
            result.Add("faronTwilightCleared", sSettings.faronTwilightCleared);
            result.Add("eldinTwilightCleared", sSettings.eldinTwilightCleared);
            result.Add("lanayruTwilightCleared", sSettings.lanayruTwilightCleared);
            result.Add("skipMdh", sSettings.skipMdh);
            result.Add("skipMinorCutscenes", sSettings.skipMinorCutscenes);
            result.Add("fastIronBoots", sSettings.fastIronBoots);
            result.Add("quickTransform", sSettings.quickTransform);
            result.Add("transformAnywhere", sSettings.transformAnywhere);
            result.Add("increaseWallet", sSettings.increaseWallet);
            result.Add("modifyShopModels", sSettings.modifyShopModels);
            result.Add("trapFrequency", sSettings.trapFrequency.ToString());
            result.Add("barrenDungeons", sSettings.barrenDungeons);
            result.Add("goronMinesEntrance", sSettings.goronMinesEntrance.ToString());
            result.Add("skipLakebedEntrance", sSettings.skipLakebedEntrance);
            result.Add("skipArbitersEntrance", sSettings.skipArbitersEntrance);
            result.Add("skipSnowpeakEntrance", sSettings.skipSnowpeakEntrance);
            result.Add("totEntrance", sSettings.totEntrance.ToString());
            result.Add("skipCityEntrance", sSettings.skipCityEntrance);
            result.Add("instantText", sSettings.instantText);
            result.Add("openMap", sSettings.openMap);
            result.Add("increaseSpinnerSpeed", sSettings.increaseSpinnerSpeed);
            result.Add("openDot", sSettings.openDot);

            result.Add("startingItems", sSettings.startingItems);
            result.Add("excludedChecks", sSettings.excludedChecks);

            return result;
        }

        public class Builder
        {
            public string settingsString { get; set; }
            public string seed { get; set; }
            public bool isRaceSeed { get; set; }
            public string seedHashString { get; set; }
            public string playthroughName { get; set; }
            public byte requiredDungeons { get; set; }
            private string itemPlacement;
            private string spheres;
            public string entrances;

            public Builder() { }

            public void SetItemPlacements(SortedDictionary<int, byte> checkNumIdToItemId)
            {
                itemPlacement = EncodeItemPlacements(checkNumIdToItemId);
            }

            public void SetSpheres(List<List<KeyValuePair<int, Item>>> spheresList)
            {
                spheres = EncodeSpheres(spheresList);
            }

            public void SetEntrances()
            {
                entrances = EncodeEntrances();
            }

            public string GetEntrances(string encodedString)
            {
                return DecodeEntrances(encodedString);
            }

            override public string ToString()
            {
                Dictionary<string, object> inputJsonRoot = new();
                // Need to update format for any changes.
                // For minor additions, can bump to 1.1, etc.
                // For major format changes, can change to 2, etc.
                inputJsonRoot.Add("version", "1");

                Dictionary<string, object> metaObj = new();
                inputJsonRoot.Add("meta", metaObj);
                metaObj.Add("ts", DateTime.UtcNow.ToString(TimestampFormat));
                metaObj.Add("imgVer", Global.imageVersion);
                metaObj.Add("gitCmt", Global.gitCommit);

                Dictionary<string, object> inputObj = new();
                inputJsonRoot.Add("input", inputObj);
                inputObj.Add("settings", settingsString);
                inputObj.Add("seed", seed);
                inputObj.Add("race", isRaceSeed ? 1 : 0);

                Dictionary<string, object> outputObj = new();
                inputJsonRoot.Add("output", outputObj);
                outputObj.Add("seedHash", seedHashString);
                outputObj.Add("name", playthroughName);
                outputObj.Add("itemPlacement", itemPlacement);
                outputObj.Add("reqDungeons", requiredDungeons);
                outputObj.Add("spheres", spheres);
                outputObj.Add("entrances", entrances);

                return JsonConvert.SerializeObject(inputJsonRoot);
            }
        }

        private enum RequiredDungeon : byte
        {
            ForestTemple = 0,
            GoronMines = 1,
            LakebedTemple = 2,
            ArbitersGrounds = 3,
            SnowpeakRuins = 4,
            TempleOfTime = 5,
            CityInTheSky = 6,
            PalaceOfTwilight = 7,
        }
    }
}
