using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TPRandomizer.Assets;
using TPRandomizer.Util;

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
        public string wiiPlaythroughName { get; set; }
        public Dictionary<int, int> itemPlacements { get; }
        public byte requiredDungeons { get; set; }
        public List<List<KeyValuePair<int, Item>>> spheres { get; }
        public string entrances { get; }
        public CustomMsgData customMsgData { get; }

        // other
        public SharedSettings decodedSSettings;

        public static byte checkIDBitLength = 10;

        public SeedGenResults(string seedId, JObject inputJsonContents)
        {
            if (Randomizer.Checks.CheckDict.Count < 1)
                throw new Exception(
                    "Tried to decode SeedGenResults, but CheckDict was not initialized."
                );

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
            decodedSSettings = SharedSettings.FromString(settingsString);
            seed = (string)input["seed"];
            isRaceSeed = (int)input["race"] == 1;

            JObject output = (JObject)inputJsonContents["output"];
            this.playthroughName = (string)output["name"];
            this.wiiPlaythroughName = (string)output["wiiName"];
            this.itemPlacements = DecodeItemPlacements((string)output["itemPlacement"]);
            this.requiredDungeons = (byte)output["reqDungeons"];
            this.spheres = DecodeSpheres((string)output["spheres"]);
            this.entrances = DecodeEntrances((string)output["entrances"]);
            this.customMsgData = CustomMsgData.Decode(
                decodedSSettings,
                itemPlacements,
                (string)output["customMsg"]
            );
        }

        public static string EncodeEntrances()
        {
            string spawnRoom = Randomizer.Rooms.RoomDict["Root"].Exits[0].ConnectedArea;
            EntranceInfo vanillaSpawn = Randomizer.EntranceRandomizer.vanillaSpawn;
            string encodedString = "";
            Console.WriteLine("Spawn point is " + spawnRoom);

            encodedString = encodedString + vanillaSpawn.Stage.ToString("X") + ",";
            encodedString = encodedString + vanillaSpawn.Room.ToString("X") + ",";
            encodedString = encodedString + vanillaSpawn.Spawn + ",";
            encodedString = encodedString + vanillaSpawn.State + ",";
            if (Randomizer.SSettings.randomizeStartingPoint)
            {
                Entrance randomSpawn = Randomizer.EntranceRandomizer.spawnList[
                    Randomizer.spawnIndex
                ];

                encodedString = encodedString + randomSpawn.Stage.ToString("X") + ",";
                encodedString = encodedString + randomSpawn.Room.ToString("X") + ",";
                encodedString = encodedString + randomSpawn.Spawn + ",";
                encodedString = encodedString + randomSpawn.State + ",";
            }
            else
            {
                encodedString = encodedString + vanillaSpawn.Stage.ToString("X") + ",";
                encodedString = encodedString + vanillaSpawn.Room.ToString("X") + ",";
                encodedString = encodedString + vanillaSpawn.Spawn + ",";
                encodedString = encodedString + vanillaSpawn.State + ",";
            }

            List<EntranceInfo> ooccooSpawns =
                new()
                {
                    new("Forest Temple Entrance", "", (int)StageIDs.Faron_Woods, 6, "96", "FF", "")
                };

            foreach (EntranceInfo ooccooSpawn in ooccooSpawns)
            {
                encodedString +=
                    ooccooSpawn.Stage.ToString("X")
                    + ","
                    + ooccooSpawn.Room.ToString("X")
                    + ","
                    + ooccooSpawn.Spawn
                    + ","
                    + ooccooSpawn.State
                    + ",";

                Entrance ftBossExit = Randomizer.Rooms.RoomDict["Forest Temple Boss Room"].Exits[
                    0
                ].GetReplacedEntrance();
                Console.WriteLine("Replaced thing is " + ftBossExit.OriginalName);
                encodedString +=
                    ftBossExit.GetStage().ToString("X")
                    + ","
                    + ftBossExit.GetRoom().ToString("X")
                    + ","
                    + ftBossExit.GetSpawn()
                    + ","
                    + ftBossExit.GetState()
                    + ",";
            }

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
                        encodedString = encodedString + entrance.GetStage().ToString("X") + ",";
                        encodedString = encodedString + entrance.GetRoom().ToString("X") + ",";
                        encodedString = encodedString + entrance.GetSpawn() + ",";
                        encodedString = encodedString + entrance.GetState() + ",";

                        // Add new connection info

                        encodedString =
                            encodedString + entrance.GetReplacedEntrance().GetStage().ToString("X");
                        encodedString = encodedString + ",";
                        encodedString =
                            encodedString + entrance.GetReplacedEntrance().GetRoom().ToString("X");
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetReplacedEntrance().GetSpawn();
                        encodedString = encodedString + ",";
                        encodedString = encodedString + entrance.GetReplacedEntrance().GetState();
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

            result += SettingsEncoder.EncodeNumAsBits(smallest, SeedGenResults.checkIDBitLength);
            result += SettingsEncoder.EncodeNumAsBits(largest, SeedGenResults.checkIDBitLength);

            string itemBits = "";

            for (int i = smallest; i <= largest; i++)
            {
                if (checkNumIdToItemId.ContainsKey(i))
                {
                    result += "1";
                    itemBits += SettingsEncoder.EncodeNumAsBits(checkNumIdToItemId[i], 9);
                }
                else
                {
                    result += "0";
                }
            }

            result += itemBits;

            return SettingsEncoder.EncodeAs6BitString(result);
        }

        private Dictionary<int, int> DecodeItemPlacements(string sixCharString)
        {
            BitsProcessor processor = new BitsProcessor(
                SettingsEncoder.DecodeToBitString(sixCharString)
            );

            Dictionary<int, int> checkNumIdToItemId = new();

            UInt16 version = processor.NextVlq16();

            if (!processor.NextBool())
            {
                return checkNumIdToItemId;
            }

            int smallest = processor.NextInt(SeedGenResults.checkIDBitLength);
            int largest = processor.NextInt(SeedGenResults.checkIDBitLength);

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
                int itemId = processor.NextInt(9);
                checkNumIdToItemId[checkId] = itemId;
            }

            // Randomizer.CheckDict - need to iterate through entire list. Any
            // that aren't in itemPlacements get inserted into dict with their
            // vanilla contents. We need this since we don't always encode 100%
            // of the checks, and an old seed might have been created before new
            // checks exist as well.
            foreach (KeyValuePair<string, Check> pair in Randomizer.Checks.CheckDict)
            {
                int checkId = CheckIdClass.GetCheckIdNum(pair.Key);
                if (!checkNumIdToItemId.ContainsKey(checkId))
                    checkNumIdToItemId[checkId] = (int)pair.Value.itemId;
            }
            // Ensure we have a mapping for all checkIds.
            int currCheckId = 0;
            while (CheckIdClass.IsValidCheckId(currCheckId))
            {
                if (!checkNumIdToItemId.ContainsKey(currCheckId))
                    throw new Exception(
                        $"Expected checkNumToItemId to contain key '{currCheckId}', but was missing."
                    );
                currCheckId++;
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
                        result += SettingsEncoder.EncodeNumAsBits(
                            pair.Key,
                            SeedGenResults.checkIDBitLength
                        ); // checkId
                        result += SettingsEncoder.EncodeNumAsBits((int)pair.Value, 9); // itemId
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
                    int checkId = processor.NextInt(SeedGenResults.checkIDBitLength);
                    Item itemId = (Item)processor.NextInt(9);

                    spherePairs.Add(new KeyValuePair<int, Item>(checkId, itemId));
                }
                result.Add(spherePairs);
            }

            return result;
        }

        // forceOutputEverything only exists so that we can print
        public string ToSpoilerString(
            SortedDictionary<string, string> sortedCheckNameToItemNameDict,
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
            root.Add("wiiPlaythroughName", wiiPlaythroughName);
            root.Add("isRaceSeed", isRaceSeed);
            root.Add("seedString", seed);
            root.Add("settingsString", settingsString);
            root.Add("settings", SSettingsToDetailedDict());

            if (!isRaceSeed || dangerouslyPrintFullRaceSpoiler)
            {
                root.Add("requiredDungeons", GetRequiredDungeonsStringList());
                root.Add("shuffledEntrances", GetShuffledEntrancesStringList());
                root.Add("itemPlacements", sortedCheckNameToItemNameDict);
                root.Add("hints", customMsgData.GetDictForSpoiler());
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

            root.Add(
                "version",
                "s"
                    + Assets.SeedData.VersionMajor
                    + "."
                    + Assets.SeedData.VersionMinor
                    + "."
                    + Assets.SeedData.VersionPatch
            );

            return SpoilerJsonWriterUtils.Serialize(root);
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

        private List<string> GetShuffledEntrancesStringList()
        {
            EntranceRando entranceRando = new();
            List<string> shuffledEntrances = new();

            EntranceRando.DeserializeSpawnTable();

            List<EntranceInfo> entranceInfo = new();
            foreach (SpawnTableEntry entry in Randomizer.EntranceRandomizer.SpawnTable)
            {
                entranceInfo.Add(entry.SourceRoomSpawn);
                if (entry.TargetRoomSpawn != null)
                {
                    entranceInfo.Add(entry.TargetRoomSpawn);
                }
            }
            string[] entranceBytes = entrances.Split(",");

            // Spawn location is always the first entry in the entrance table
            foreach (EntranceInfo entry in entranceInfo)
            {
                if (entry.Stage.ToString("X") == entranceBytes[4])
                {
                    if (entry.Room.ToString("X") == entranceBytes[5])
                    {
                        if (entry.Spawn == entranceBytes[6])
                        {
                            if (entry.State == entranceBytes[7])
                            {
                                shuffledEntrances.Add("Spawn Location -> " + entry.TargetRoom);
                            }
                        }
                    }
                }
            }
            entranceBytes = entranceBytes.Skip(8).ToArray();

            //Console.WriteLine(entrances);
            for (int i = 0; i < entranceBytes.Length - 1; i++)
            {
                //Console.WriteLine(i);
                /*Console.WriteLine(
                    "testing spoiler spawn: "
                        + entranceBytes[i]
                        + ","
                        + entranceBytes[i + 1]
                        + ","
                        + entranceBytes[i + 2]
                        + ","
                        + entranceBytes[i + 3]
                        + ","
                        + entranceBytes[i + 4]
                );*/
                foreach (EntranceInfo entry in entranceInfo)
                {
                    if (entry.Stage.ToString("X") == entranceBytes[i])
                    {
                        if (entry.Room.ToString("X") == entranceBytes[i + 1])
                        {
                            if (entry.Spawn == entranceBytes[i + 2])
                            {
                                if (entry.State == entranceBytes[i + 3])
                                {
                                    foreach (EntranceInfo entry2 in entranceInfo)
                                    {
                                        if (entry2.Stage.ToString("X") == entranceBytes[i + 4])
                                        {
                                            if (entry2.Room.ToString("X") == entranceBytes[i + 5])
                                            {
                                                if (entry2.Spawn == entranceBytes[i + 6])
                                                {
                                                    if (entry2.State == entranceBytes[i + 7])
                                                    {
                                                        shuffledEntrances.Add(
                                                            entry.SourceRoom
                                                                + " -> "
                                                                + entry2.TargetRoom
                                                        );
                                                        /*Console.WriteLine(
                                                            entry.SourceRoom
                                                                + " -> "
                                                                + entry2.TargetRoom
                                                        );*/
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                i = i + 7;
            }

            return shuffledEntrances;
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
            result.Add("castleRequirementCount", sSettings.castleRequirementCount);
            result.Add("palaceRequirements", sSettings.palaceRequirements.ToString());
            result.Add("faronWoodsLogic", sSettings.faronWoodsLogic.ToString());
            result.Add("shuffleGoldenBugs", sSettings.shuffleGoldenBugs);
            result.Add("shuffleSkyCharacters", sSettings.shuffleSkyCharacters);
            result.Add("shuffleNpcItems", sSettings.shuffleNpcItems);
            result.Add("shufflePoes", sSettings.shufflePoes.ToString());
            result.Add("shuffleShopItems", sSettings.shuffleShopItems);
            result.Add("shuffleHiddenSkills", sSettings.shuffleHiddenSkills);
            result.Add("itemScarcity", sSettings.itemScarcity.ToString());
            result.Add("damageMagnification", sSettings.damageMagnification.ToString());
            result.Add("bonksDoDamage", sSettings.bonksDoDamage);
            result.Add("shuffleRewards", sSettings.shuffleRewards);
            result.Add("smallKeySettings", sSettings.smallKeySettings.ToString());
            result.Add("bigKeySettings", sSettings.bigKeySettings.ToString());
            result.Add("mapAndCompassSettings", sSettings.mapAndCompassSettings.ToString());
            result.Add("skipPrologue", sSettings.skipPrologue);
            result.Add("faronTwilightCleared", sSettings.faronTwilightCleared);
            result.Add("eldinTwilightCleared", sSettings.eldinTwilightCleared);
            result.Add("lanayruTwilightCleared", sSettings.lanayruTwilightCleared);
            result.Add("skipMdh", sSettings.skipMdh);
            result.Add("skipMinorCutscenes", sSettings.skipMinorCutscenes);
            result.Add("skipMajorCutscenes", sSettings.skipMajorCutscenes);
            result.Add("fastIronBoots", sSettings.fastIronBoots);
            result.Add("quickTransform", sSettings.quickTransform);
            result.Add("transformAnywhere", sSettings.transformAnywhere);
            result.Add("walletSize", sSettings.walletSize.ToString());
            result.Add("autoFillWallet", sSettings.autoFillWallet);
            result.Add("modifyShopModels", sSettings.modifyShopModels);
            result.Add("trapFrequency", sSettings.trapFrequency.ToString());
            result.Add("barrenDungeons", sSettings.barrenDungeons);
            result.Add("goronMinesEntrance", sSettings.goronMinesEntrance.ToString());
            result.Add("skipLakebedEntrance", sSettings.skipLakebedEntrance);
            result.Add("skipArbitersEntrance", sSettings.skipArbitersEntrance);
            result.Add("skipSnowpeakEntrance", sSettings.skipSnowpeakEntrance);
            result.Add("skipGroveEntrance", sSettings.skipGroveEntrance);
            result.Add("totEntrance", sSettings.totEntrance.ToString());
            result.Add("skipCityEntrance", sSettings.skipCityEntrance);
            result.Add("instantText", sSettings.instantText);
            result.Add("openMap", sSettings.openMap);
            result.Add("increaseSpinnerSpeed", sSettings.increaseSpinnerSpeed);
            result.Add("openDot", sSettings.openDot);
            result.Add("noSmallKeysOnBosses", sSettings.noSmallKeysOnBosses);
            result.Add("startingToD", sSettings.startingToD.ToString());
            result.Add("hintDistribution", sSettings.hintDistribution.ToString());
            result.Add("randomizeStartingPoint", sSettings.randomizeStartingPoint);
            result.Add("shuffleHiddenRupees", sSettings.shuffleHiddenRupees);
            result.Add("hcShortcut", sSettings.hcShortcut);
            result.Add("iliaQuest", sSettings.iliaQuest.ToString());
            result.Add("mirrorChamberEntrance", sSettings.mirrorChamberEntrance.ToString());
            result.Add("shuffleDungeonEntrances", sSettings.shuffleDungeonEntrances.ToString());
            result.Add("shuffleFreestandingRupees", sSettings.shuffleFreestandingRupees);
            result.Add("decoupleEntrances", sSettings.decoupleEntrances);
            result.Add("unpairEntrances", sSettings.unpairEntrances);
            result.Add("castleBKRequirements", sSettings.castleBKRequirements.ToString());
            result.Add("castleBKRequirementCount", sSettings.castleBKRequirementCount);
            result.Add("skipBridgeDonation", sSettings.skipBridgeDonation);
            result.Add("maloShopDonation", sSettings.maloShopDonation);

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
            public string wiiPlaythroughName { get; set; }
            public byte requiredDungeons { get; set; }
            private string itemPlacement;
            private string spheres;
            public string entrances;
            public string customMsgData;

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

            public void SetCustomMsgData(CustomMsgData customMsgData)
            {
                this.customMsgData = customMsgData.Encode();
            }

            public override string ToString()
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
                outputObj.Add("wiiName", wiiPlaythroughName);
                outputObj.Add("itemPlacement", itemPlacement);
                outputObj.Add("reqDungeons", requiredDungeons);
                outputObj.Add("spheres", spheres);
                outputObj.Add("entrances", entrances);
                outputObj.Add("customMsg", customMsgData);

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
