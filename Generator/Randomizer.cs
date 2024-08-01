namespace TPRandomizer
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.SSettings.Enums;
    using TPRandomizer.FcSettings.Enums;
    using System.Reflection;
    using Assets;
    using System.ComponentModel;
    using Hints;

    /// <summary>
    /// Generates a randomizer seed given a settings string.
    /// </summary>
    public class Randomizer
    {
        /// <summary>
        /// A reference to all logic functions that need to be used by the randomizer.
        /// </summary>
        public static readonly LogicFunctions Logic = new();

        /// <summary>
        /// A reference to all check-related functions that need to be used by the randomizer.
        /// </summary>
        public static readonly CheckFunctions Checks = new();

        /// <summary>
        /// A reference to all room-related functions that need to be used by the randomizer.
        /// </summary>
        public static readonly RoomFunctions Rooms = new();

        /// <summary>
        /// A reference to all item lists and functions that need to be used by the randomizer.
        /// </summary>
        public static readonly ItemFunctions Items = new();

        /// <summary>
        /// A reference to all Entrance definitions and functions that need to be used by the randomizer.
        /// </summary>
        public static readonly EntranceRando EntranceRandomizer = new();

        /// <summary>
        /// A reference to the sSettings.
        /// </summary>
        public static SharedSettings SSettings = new();

        public static int RequiredDungeons = 0;

        public static bool CreateInputJson(
            string idParam,
            string settingsString,
            string raceSeedParam,
            string seed
        )
        {
            if (
                idParam == null
                || (idParam != "idnull" && !(new Regex("^id[0-9A-Za-z-_]{11}$").IsMatch(idParam)))
            )
            {
                throw new Exception("Invalid id param.");
            }

            string id = "";
            string outputPath = "";

            if (idParam == "idnull")
            {
                bool idConfirmedUnique = false;
                while (!idConfirmedUnique)
                {
                    id = Util.Hash.GenId();
                    outputPath = Global.CombineOutputPath("seeds", id, "input.json");

                    if (!File.Exists(outputPath))
                        idConfirmedUnique = true;
                }
            }
            else
            {
                id = idParam.Substring(2);
                outputPath = Global.CombineOutputPath("seeds", id, "input.json");
                if (File.Exists(outputPath))
                {
                    throw new Exception("input.json already exists for the id '" + id + "'.");
                }
            }

            // Generate seedHash from seed
            if (seed == null || seed.Length < 1)
            {
                // 132 bits of data as 22 characters. Wanted at least 128 bits,
                // and the 6bit encoding only needs 22 chars instead of the 32
                // hex characters you would normally use for 128 bits.
                // Concatenate 2 together because we don't want it to be easily
                // confused with the seed's id in the URL.
                seed = Util.Hash.GenId() + Util.Hash.GenId();
            }

            bool isRaceSeed = raceSeedParam.ToLowerInvariant() == "true";

            int seedHash = Util.Hash.HashSeed(seed, isRaceSeed);
            Random rnd = new Random(seedHash);

            bool generationStatus = false;
            int remainingGenerationAttempts = 10;

            Console.WriteLine("SeedData Version: " + SeedData.VersionString);

            // Read in the settings string and set the settings values accordingly
            // BackendFunctions.InterpretSettingsString(settingsString);
            SSettings = SharedSettings.FromString(settingsString);
            PropertyInfo[] randoSettingProperties = SSettings.GetType().GetProperties();

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks(SSettings);
            DeserializeRooms(SSettings);

            foreach (PropertyInfo settingProperty in randoSettingProperties)
            {
                Console.WriteLine(
                    settingProperty.Name + ": " + settingProperty.GetValue(SSettings, null)
                );
            }

            foreach (string checkName in SSettings.excludedChecks)
            {
                Randomizer.Checks.CheckDict[checkName].checkStatus = "Excluded";
            }

            // Generate the item pool based on user settings/input.
            Randomizer.Items.GenerateItemPool();
            CheckFunctions.GenerateCheckList();

            while (remainingGenerationAttempts > 0)
            {
                remainingGenerationAttempts--;
                foreach (Item startingItem in Randomizer.SSettings.startingItems)
                {
                    Randomizer.Items.heldItems.Add(startingItem);
                }
                Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);

                // Place plando checks first
                Console.WriteLine("Placing Plando Checks.");
                PlacePlandoChecks();

                Console.WriteLine("Placing Vanilla Checks.");
                PlaceVanillaChecks();

                // Once we have placed all vanilla checks, we want to give the player all of the items they should be searching for and then generate the world based on the room class values and their neighbour values.
                SetupGraph();
                try
                {
                    Randomizer.EntranceRandomizer.RandomizeEntrances(rnd);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    StartOver();
                    continue;
                }
                try
                {
                    // Place the items in the world based on the starting room.
                    PlaceItemsInWorld(Randomizer.Rooms.RoomDict["Root"], rnd);
                    generationStatus = true;
                    break;
                }
                // If for some reason the assumed fill fails, we want to dump everything and start over.
                catch (ArgumentOutOfRangeException a)
                {
                    a = null;
                    Console.WriteLine(
                        "/~~~~~~~~~~~~~~~~~~~~~ Generation Failure. No checks remaining, starting over..~~~~~~~~~~~~~~~~~~~~~~~~~~~~/"
                            + a
                    );
                    StartOver();
                    continue;
                }
            }

            if (generationStatus)
            {
                // Randomizer.Items.GenerateItemPool();

                // List<List<KeyValuePair<int, Item>>> spheres = GenerateSpoilerLog(
                //     Randomizer.Rooms.RoomDict["Root"]
                // );

                // if (spheres == null)
                // {
                //     throw new Exception("Error! Playthrough not valid.");
                // }

                PlaythroughSpheres playthroughSpheres = GenerateSpoilerLog(
                    Randomizer.Rooms.RoomDict["Root"]
                );

                if (
                    playthroughSpheres.spheres == null
                    && SSettings.logicRules != LogicRules.No_Logic
                )
                {
                    throw new Exception("Error! Playthrough not valid.");
                }

                CustomMsgData customMsgData;
                try
                {
                    HintGenerator gen = new HintGenerator(
                        rnd,
                        SSettings,
                        playthroughSpheres,
                        Randomizer.Rooms.RoomDict["Root"]
                    );

                    customMsgData = gen.Generate();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                string jsonContent = GenerateInputJsonContent(
                    settingsString,
                    seed,
                    seedHash,
                    isRaceSeed,
                    playthroughSpheres.spheres,
                    customMsgData
                );

                try
                {
                    // Write json file to id dir.
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    File.WriteAllText(outputPath, jsonContent);

                    Console.WriteLine("SUCCESS:" + id);
                }
                catch (Exception e)
                {
                    e = null;
                    Console.WriteLine("Problem writing input.json file for id: " + id + e);
                    System.Environment.Exit(1);
                }
            }

            CleanUp();
            return generationStatus;
        }

        public static PlaythroughSpheres GenerateSpoilerLog(Room startingRoom)
        {
            Randomizer.Items.GenerateItemPool();

            foreach (Item startingItem in Randomizer.SSettings.startingItems)
            {
                Randomizer.Items.heldItems.Add(startingItem);
            }

            bool isPlaythroughValid = BackendFunctions.ValidatePlaythrough(startingRoom, true);
            if (!isPlaythroughValid || SSettings.logicRules == LogicRules.No_Logic)
            {
                return new PlaythroughSpheres(null, null, null);
            }

            return BackendFunctions.CalculateOptimalPlaythrough2(startingRoom);
        }

        private static string GenerateInputJsonContent(
            string settingsString,
            string seed,
            int seedHash,
            bool isRaceSeed,
            List<List<KeyValuePair<int, Item>>> spheres,
            CustomMsgData customMsgData
        )
        {
            Dictionary<string, Item> checkIdToItemId = new();
            List<string> placementStrParts = new();
            SortedDictionary<int, byte> checkNumIdToItemId = new();

            List<KeyValuePair<int, Item>> dungeonRewards = new();

            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                // We don't store itemIds in the json for vanilla checks to save space.
                if (checkList.Value.checkStatus != "Vanilla")
                {
                    string checkId = CheckIdClass.FromString(checkList.Key);
                    int checkIdNum = CheckIdClass.GetCheckIdNum(checkList.Key);
                    if (checkId == null || checkIdNum < 0)
                    {
                        throw new Exception(
                            "Need to update CheckId to support check named \""
                                + checkList.Key
                                + "\"."
                        );
                    }

                    Check check = checkList.Value;

                    byte itemIdByte = (byte)check.itemId;

                    // For generating consistent filenames.
                    placementStrParts.Add(checkId + "_" + itemIdByte);
                    checkIdToItemId[checkId] = check.itemId;

                    // For storing placements in json file.
                    checkNumIdToItemId.Add(checkIdNum, itemIdByte);

                    if (check.checkCategory.Contains("Dungeon Reward"))
                    {
                        dungeonRewards.Add(
                            new KeyValuePair<int, Item>(
                                CheckIdClass.GetCheckIdNum(check.checkName),
                                check.itemId
                            )
                        );
                    }
                }
            }

            // StringComparer is needed because the default sort order is
            // different on Linux and Windows
            placementStrParts.Sort(StringComparer.Ordinal);
            string itemPlacementPart = String.Join("-", placementStrParts);

            // Only need to take settings into account which are important to part2.
            SortedDictionary<string, object> part2SettingsForString = GenPart2Settings();

            string part2SettingsPart = JsonConvert.SerializeObject(part2SettingsForString);

            string seedHashAsString = seedHash.ToString("x8");

            string filenameInput = String.Join(
                "%%%%",
                new List<string> { itemPlacementPart, part2SettingsPart, seedHashAsString }
            );

            int filenameBits = Util.Hash.CalculateMD5(filenameInput);
            List<string> playthroughNames = Util.PlaythroughName.GenNames(filenameBits);

            // When generating the filename, the following should be taken into account:

            // - item placements for non-vanilla placements (sorted so that
            //   always the same)
            // - settings which affect things other than item-placement (also
            //   sort). When new settings are added in the future, they should
            //   only be taken into account in the hash when their setting would
            //   cause the gameplay to differ from a seed that was generated
            //   before that setting existed. For example, let's say a seed was
            //   generated at point A. At point B, we now have a setting for
            //   super-clawshot (on or off, off by default). If we generate a
            //   seed at point B with super-clawshot turned off, the playthrough
            //   will be identical to the one generated at point A, so the
            //   filenames should remain the same. However, we still need to
            //   specify the super-clawshot setting in the json because if
            //   someone were to generate a GCI using the seed from point B as
            //   input and now super-clawshot is on by default (this is just an
            //   example), their playthrough experience would be different even
            //   though the filename is the same.

            // TODO: review if the above comment needs a little revision

            SeedGenResults.Builder builder = new();
            // inputs
            builder.settingsString = settingsString;
            builder.seed = seed;
            builder.isRaceSeed = isRaceSeed;
            // outputs
            builder.seedHashString = seedHashAsString;
            builder.playthroughName = playthroughNames[0];
            builder.wiiPlaythroughName = playthroughNames[1];
            builder.requiredDungeons = (byte)Randomizer.RequiredDungeons;
            builder.SetItemPlacements(checkNumIdToItemId);
            builder.SetSpheres(spheres);
            builder.SetEntrances();
            builder.SetCustomMsgData(customMsgData);
            Console.WriteLine(builder.GetEntrances(builder.entrances));
            return builder.ToString();
        }

        private static SortedDictionary<string, object> GenPart2Settings()
        {
            // Please read the comments below when updating the sSettings to
            // determine if/how this method needs to be updated.

            // Generally speaking, it should be added if it affects starting
            // state or the ability to traverse the game graph.

            // StringComparer is needed because the default sort order is
            // different on Linux and Windows
            SortedDictionary<string, object> part2Settings = new(StringComparer.Ordinal);

            // If a setting matches what the game behavior would have been
            // before that setting existed, we leave it off.

            // We don't add a setting when its only effect has to do with
            // itemPlacement. itemPlacement is handled on its own. If one seed
            // generation had smallKeys in OwnDungeon and another had them in
            // AnyDungeon and the placement ended up being the exact same, (all
            // other things being the same) the playthroughName should match
            // because the generated file would be the same, meaning the player
            // has already played this exact scenario.

            // A setting is only added if it is set to a value which has a
            // definite impact on the game either. For example, it affects the
            // starting state (such as you start with these flags already set
            // and these items in your inventory). Another example would be if
            // there was a setting that had an impact on your ability to
            // navigate an edge of the graph during gameplay (such as trying to
            // enter the Stallord boss fight from the back causes you to
            // teleport back to the mirror chamber).

            // Note that the string keys must never be changed after initial
            // release. It is okay if the name does not match exactly with the
            // sSettings property.

            // Multi-option fields which are only included for certain values
            if (SSettings.castleRequirements != CastleRequirements.Vanilla)
                part2Settings.Add("castleRequirements", SSettings.castleRequirements);
            if (SSettings.palaceRequirements != PalaceRequirements.Vanilla)
                part2Settings.Add("palaceRequirements", SSettings.palaceRequirements);
            // TODO: Change this one to a boolean called "faronWoodsOpen"
            if (SSettings.faronWoodsLogic == FaronWoodsLogic.Open)
                part2Settings.Add("faronWoodsLogic", SSettings.faronWoodsLogic);
            if (SSettings.smallKeySettings == SmallKeySettings.Keysy)
                part2Settings.Add("smallKeySettings", SSettings.smallKeySettings);
            if (SSettings.bigKeySettings == BigKeySettings.Keysy)
                part2Settings.Add("bigKeySettings", SSettings.bigKeySettings);
            if (SSettings.mapAndCompassSettings == MapAndCompassSettings.Start_With)
                part2Settings.Add("mapAndCompassSettings", SSettings.mapAndCompassSettings);

            // Boolean fields included when true
            if (SSettings.skipPrologue)
                part2Settings.Add("skipPrologue", SSettings.skipPrologue);
            if (SSettings.faronTwilightCleared)
                part2Settings.Add("faronTwilightCleared", SSettings.faronTwilightCleared);
            if (SSettings.eldinTwilightCleared)
                part2Settings.Add("eldinTwilightCleared", SSettings.eldinTwilightCleared);
            if (SSettings.lanayruTwilightCleared)
                part2Settings.Add("lanayruTwilightCleared", SSettings.lanayruTwilightCleared);
            if (SSettings.skipMdh)
                part2Settings.Add("skipMdh", SSettings.skipMdh);
            if (SSettings.skipMinorCutscenes)
                part2Settings.Add("skipMinorCutscenes", SSettings.skipMinorCutscenes);
            if (SSettings.fastIronBoots)
                part2Settings.Add("fastIronBoots", SSettings.fastIronBoots);
            if (SSettings.quickTransform)
                part2Settings.Add("quickTransform", SSettings.quickTransform);
            if (SSettings.transformAnywhere)
                part2Settings.Add("transformAnywhere", SSettings.transformAnywhere);
            if (SSettings.increaseWallet)
                part2Settings.Add("increaseWallet", SSettings.increaseWallet);
            if (SSettings.modifyShopModels)
                part2Settings.Add("modifyShopModels", SSettings.modifyShopModels);

            if (SSettings.goronMinesEntrance != GoronMinesEntrance.Closed)
                part2Settings.Add("goronMinesEntrance", SSettings.goronMinesEntrance);
            if (SSettings.skipLakebedEntrance)
                part2Settings.Add("skipLakebedEntrance", SSettings.skipLakebedEntrance);
            if (SSettings.skipArbitersEntrance)
                part2Settings.Add("skipArbitersEntrance", SSettings.skipArbitersEntrance);
            if (SSettings.skipSnowpeakEntrance)
                part2Settings.Add("skipSnowpeakEntrance", SSettings.skipSnowpeakEntrance);
            if (SSettings.totEntrance != TotEntrance.Closed)
                part2Settings.Add("totEntrance", SSettings.totEntrance);
            if (SSettings.skipCityEntrance)
                part2Settings.Add("skipCityEntrance", SSettings.skipCityEntrance);
            if (SSettings.instantText)
                part2Settings.Add("instantText", SSettings.instantText);
            if (SSettings.itemScarcity != ItemScarcity.Vanilla)
                part2Settings.Add("itemScarcity", SSettings.itemScarcity);
            if (SSettings.openMap)
                part2Settings.Add("openMap", SSettings.openMap);
            if (SSettings.increaseSpinnerSpeed)
                part2Settings.Add("increaseSpinnerSpeed", SSettings.increaseSpinnerSpeed);
            if (SSettings.openDot)
                part2Settings.Add("openDot", SSettings.openDot);

            // Complex fields
            if (SSettings.startingItems?.Count > 0)
            {
                List<Item> startingItems = new(SSettings.startingItems);
                startingItems.Sort();
                part2Settings.Add("startingItems", startingItems);
            }

            return part2Settings;
        }

        public static bool GenerateFinalOutput2(string id, string fcSettingsString)
        {
            string inputJsonPath = Global.CombineOutputPath("seeds", id, "input.json");

            if (!File.Exists(inputJsonPath))
            {
                throw new Exception(
                    "input.json not found for (path: " + inputJsonPath + ") id: " + id
                );
            }

            string fileContents = File.ReadAllText(inputJsonPath);
            JObject json = JsonConvert.DeserializeObject<JObject>(fileContents);

            FileCreationSettings fcSettings = FileCreationSettings.FromString(fcSettingsString);

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeCheckData(SSettings, fcSettings);
            DeserializeRooms(SSettings);

            SeedGenResults seedGenResults = new SeedGenResults(id, json);

            SSettings = SharedSettings.FromString(seedGenResults.settingsString);

            foreach (KeyValuePair<int, byte> kvp in seedGenResults.itemPlacements.ToList())
            {
                // key is checkId, value is itemId
                string checkName = CheckIdClass.GetCheckName(kvp.Key);
                if (Randomizer.Checks.CheckDict.ContainsKey(checkName))
                {
                    Randomizer.Checks.CheckDict[checkName].itemId = (Item)kvp.Value;
                }
            }

            Console.WriteLine("\nGenerating Seed Data.");

            // sSettings from input.json
            // seedGenResults from input.json, such as required dungeons
            // fcSettings

            List<Tuple<Dictionary<string, object>, byte[]>> fileDefs = new();

            if (fcSettings.gameRegion == GameRegion.All)
            {
                // For now, 'All' only generates for GameCube until we do more
                // work related to Wii code.
                List<GameRegion> gameRegionsForAll =
                    new() { GameRegion.GC_USA, GameRegion.GC_EUR, GameRegion.GC_JAP, };

                // Create files for all regions
                // foreach (GameRegion gameRegion in GameRegion.GetValues(typeof(GameRegion)))
                foreach (GameRegion gameRegion in gameRegionsForAll)
                {
                    if (gameRegion != GameRegion.All)
                    {
                        // Update language to be used with resource system.
                        string langTag = fcSettings.GetLanguageTagString(gameRegion);
                        Res.UpdateCultureInfo(langTag);

                        fileDefs.Add(GenGciFileDef(id, seedGenResults, fcSettings, gameRegion));
                    }
                }
            }
            else
            {
                // Update language to be used with resource system.
                string langTag = fcSettings.GetLanguageTagString();
                Res.UpdateCultureInfo(langTag);

                // Create file for one region
                fileDefs.Add(GenGciFileDef(id, seedGenResults, fcSettings, fcSettings.gameRegion));
            }

            if (!seedGenResults.isRaceSeed && fcSettings.includeSpoilerLog)
            {
                // Set back to default language ('en') before creating spoiler
                // log when gameRegion is 'All'.
                if (fcSettings.gameRegion == GameRegion.All)
                {
                    // Update language to be used with resource system.
                    string langTag = fcSettings.GetLanguageTagString();
                    Res.UpdateCultureInfo(langTag);
                }

                // Add fileDef for spoilerLog
                string spoilerLogText = GetSeedGenResultsJson(id);
                byte[] spoilerBytes = Encoding.UTF8.GetBytes(spoilerLogText);

                Dictionary<string, object> dict = new();
                dict.Add("name", $"Tpr--{seedGenResults.playthroughName}--SpoilerLog-{id}.json");
                dict.Add("length", spoilerBytes.Length);

                fileDefs.Add(new(dict, spoilerBytes));
            }

            PrintFileDefs(id, seedGenResults, fcSettings, fileDefs);

            // Console.WriteLine("Done!");
            // Console.WriteLine("Generating Spoiler Log.");
            // BackendFunctions.GenerateSpoilerLog(startingRoom, seedHash);
            //         IEnumerable<string> fileList = new string[]
            //         {
            //             "TPR-v1.0-" + seedHash + ".txt",
            //             "TPR-v1.0-" + seedHash + "-Seed-Data.gci"
            //         };
            //         BackendFunctions.CreateZipFile("Seed/TPR-v1.0-" + seedHash + ".zip", fileList);
            //         Console.WriteLine("Generation Complete!");
            //         generationStatus = true;
            //         break;
            //     }
            //     // If for some reason the assumed fill fails, we want to dump everything and start over.
            //     catch (ArgumentOutOfRangeException a)
            //     {
            //         Console.WriteLine(a + " No checks remaining, starting over..");
            // StartOver();
            //         continue;
            //     }
            // }

            CleanUp();
            // return generationStatus;

            return true;
        }

        private static Tuple<Dictionary<string, object>, byte[]> GenGciFileDef(
            string seedId,
            SeedGenResults seedGenResults,
            FileCreationSettings fcSettings,
            GameRegion gameRegionOverride
        )
        {
            byte[] bytes = SeedData.GenerateSeedDataBytes(
                seedGenResults,
                fcSettings,
                gameRegionOverride
            );

            Dictionary<string, object> dict = new();

            string gameVer;
            switch (gameRegionOverride)
            {
                case GameRegion.GC_USA:
                    gameVer = "E";
                    break;
                case GameRegion.GC_EUR:
                    gameVer = "P";
                    break;
                case GameRegion.GC_JAP:
                    gameVer = "J";
                    break;
                default:
                    throw new Exception("Did not specify output region");
            }

            string fileName =
                "Tpr-" + gameVer + "-" + seedGenResults.playthroughName + "-" + seedId;

            fileName += ".gci";

            dict.Add("name", fileName);
            dict.Add("length", bytes.Length);

            return new(dict, bytes);
        }

        private static void PrintFileDefs(
            string seedId,
            SeedGenResults seedGenResults,
            FileCreationSettings fcSettings,
            List<Tuple<Dictionary<string, object>, byte[]>> fileDefs
        )
        {
            if (fileDefs.Count > 1)
            {
                // Write ZIP file instead
                string zipFilename = $"TPR--{seedGenResults.playthroughName}--{seedId}.zip";
                fileDefs = MergeFileDefsToZip(zipFilename, fileDefs);
            }

            List<Dictionary<string, object>> jsonRoot = fileDefs
                .Select(tuple => tuple.Item1)
                .ToList();

            string fileDefMetaJson = JsonConvert.SerializeObject(jsonRoot);
            string hexValue = fileDefMetaJson.Length.ToString("x8");

            // Write 8 chars of hex for length of json
            // json looks like: [{name: '', length: number}, {}]
            Console.Write("BYTES:");
            Console.Write(hexValue);
            Console.Write(fileDefMetaJson);

            // Write file bytes
            for (int i = 0; i < fileDefs.Count; i++)
            {
                byte[] bytes = fileDefs[i].Item2;

                using (Stream myOutStream = Console.OpenStandardOutput())
                {
                    myOutStream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static List<Tuple<Dictionary<string, object>, byte[]>> MergeFileDefsToZip(
            string filename,
            List<Tuple<Dictionary<string, object>, byte[]>> fileDefs
        )
        {
            byte[] compressedBytes;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (
                    ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)
                )
                {
                    for (int i = 0; i < fileDefs.Count; i++)
                    {
                        string name = (string)fileDefs[i].Item1["name"];
                        byte[] bytes = fileDefs[i].Item2;

                        ZipArchiveEntry demoFile = archive.CreateEntry(name);

                        using (Stream entryStream = demoFile.Open())
                        {
                            using (BinaryWriter streamWriter = new BinaryWriter(entryStream))
                            {
                                streamWriter.Write(bytes, 0, bytes.Length);
                            }
                        }
                    }
                }

                compressedBytes = memoryStream.ToArray();
            }

            Dictionary<string, object> meta = new();
            meta.Add("name", filename);
            meta.Add("length", compressedBytes.Length);

            List<Tuple<Dictionary<string, object>, byte[]>> list = new();

            list.Add(new(meta, compressedBytes));

            return list;
        }

        /// <summary>
        /// Places a given item into a given check.
        /// </summary>
        /// <param name="startingRoom"> The room that the player will start the game from. </param>
        /// <returns> A complete playthrough graph for the player to traverse. </returns>
        public static List<Room> GeneratePlaythroughGraph(Room startingRoom)
        {
            List<Room> playthroughGraph = new();
            Room availableRoom;

            int availableRooms = 1;
            List<Room> roomsToExplore = new();

            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                currentRoom.ReachedByPlaythrough = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            startingRoom.Visited = true;
            playthroughGraph.Add(startingRoom);
            if (Randomizer.SSettings.openMap)
            {
                if (Randomizer.SSettings.faronTwilightCleared)
                {
                    if (LogicFunctions.CanUse(Item.Shadow_Crystal))
                    {
                        availableRoom = Randomizer.Rooms.RoomDict["South Faron Woods"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;

                        availableRoom = Randomizer.Rooms.RoomDict["North Faron Woods"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;
                    }
                }

                if (Randomizer.SSettings.eldinTwilightCleared)
                {
                    if (LogicFunctions.CanUse(Item.Shadow_Crystal))
                    {
                        availableRoom = Randomizer.Rooms.RoomDict["Lower Kakariko Village"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;

                        availableRoom = Randomizer.Rooms.RoomDict["Kakariko Gorge"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;

                        availableRoom = Randomizer.Rooms.RoomDict["Death Mountain Volcano"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;
                    }
                }

                if (Randomizer.SSettings.lanayruTwilightCleared)
                {
                    if (LogicFunctions.CanUse(Item.Shadow_Crystal))
                    {
                        availableRoom = Randomizer.Rooms.RoomDict["Lake Hylia"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;

                        availableRoom = Randomizer.Rooms.RoomDict["Outside Castle Town West"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;

                        availableRoom = Randomizer.Rooms.RoomDict["Zoras Throne Room"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;
                    }
                }

                if (Randomizer.SSettings.skipSnowpeakEntrance)
                {
                    if (LogicFunctions.CanUse(Item.Shadow_Crystal))
                    {
                        availableRoom = Randomizer.Rooms.RoomDict["Snowpeak Summit Upper"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;
                    }
                }

                if (Randomizer.SSettings.totEntrance != TotEntrance.Closed)
                {
                    if (LogicFunctions.CanUse(Item.Shadow_Crystal))
                    {
                        availableRoom = Randomizer.Rooms.RoomDict["Sacred Grove Lower"];
                        playthroughGraph.Add(availableRoom);
                        availableRoom.Visited = true;
                    }
                }
            }

            // Build the world by parsing through each room, linking their neighbours, and setting the logic for the checks in the room to reflect the world.
            while (availableRooms > 0)
            {
                availableRooms = 0;
                roomsToExplore.Add(startingRoom);
                foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
                {
                    Room currentRoom = roomList.Value;
                    currentRoom.Visited = false;
                    Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
                }
                while (roomsToExplore.Count > 0)
                {
                    //Console.WriteLine("Currently Exploring: " + roomsToExplore[0].RoomName);
                    for (int i = 0; i < roomsToExplore[0].Exits.Count; i++)
                    {
                        // If you can access the neighbour and it hasnt been visited yet.
                        //Console.WriteLine("Exit: " + roomsToExplore[0].Exits[i].GetOriginalName());
                        if (roomsToExplore[0].Exits[i].ConnectedArea != "")
                        {
                            if (
                                Randomizer.Rooms.RoomDict[
                                    roomsToExplore[0].Exits[i].ConnectedArea
                                ].Visited == false
                            )
                            {
                                // Parse the neighbour's requirements to find out if we can access it
                                var areNeighbourRequirementsMet = false;
                                /*Console.WriteLine(
                                    "Checking neighbor: "
                                        + Randomizer.Rooms.RoomDict[
                                            roomsToExplore[0].Exits[i].ConnectedArea
                                        ].RoomName
                                );*/
                                if (SSettings.logicRules == LogicRules.No_Logic)
                                {
                                    areNeighbourRequirementsMet = true;
                                }
                                else
                                {
                                    areNeighbourRequirementsMet = Logic.EvaluateRequirements(
                                        roomsToExplore[0].RoomName,
                                        roomsToExplore[0].Exits[i].Requirements
                                    );
                                }

                                if ((bool)areNeighbourRequirementsMet == true)
                                {
                                    if (
                                        !Randomizer.Rooms.RoomDict[
                                            roomsToExplore[0].Exits[i].ConnectedArea
                                        ].ReachedByPlaythrough
                                    )
                                    {
                                        availableRooms++;
                                        Randomizer.Rooms.RoomDict[
                                            roomsToExplore[0].Exits[i].ConnectedArea
                                        ].ReachedByPlaythrough = true;
                                        playthroughGraph.Add(
                                            Randomizer.Rooms.RoomDict[
                                                roomsToExplore[0].Exits[i].ConnectedArea
                                            ]
                                        );
                                    }
                                    roomsToExplore.Add(
                                        Randomizer.Rooms.RoomDict[
                                            roomsToExplore[0].Exits[i].ConnectedArea
                                        ]
                                    );
                                    Randomizer.Rooms.RoomDict[
                                        roomsToExplore[0].Exits[i].ConnectedArea
                                    ].Visited = true;

                                    /* Console.WriteLine(
                                         "Neighbour: "
                                             + Randomizer.Rooms.RoomDict[
                                                 roomsToExplore[0].Exits[i].ConnectedArea
                                             ].RoomName
                                             + " added to room list."
                                     );*/
                                }
                                /*else
                                {
                                    Console.WriteLine(
                                        "Neighbour: "
                                            + Randomizer.Rooms.RoomDict[
                                                roomsToExplore[0].Exits[i].ConnectedArea
                                            ].RoomName
                                            + " requirement not met"
                                    );
                                }*/
                            }
                        }
                    }

                    roomsToExplore.Remove(roomsToExplore[0]);
                }
            }

            return playthroughGraph;
        }

        /// <summary>
        /// Places the generated item pool's items into the world graph that has been created.
        /// </summary>
        /// <param name="startingRoom"> The room node that the generation algorithm will begin with. </param>
        private static void PlaceItemsInWorld(Room startingRoom, Random rnd)
        {
            // Dungeon rewards have a very limited item pool, so we want to place them first to prevent the generator from putting
            // an unnecessary item in one of the checks.
            if (SSettings.shuffleRewards)
            {
                PlaceItemsRestricted(
                    startingRoom,
                    Items.ShuffledDungeonRewards,
                    Randomizer.Items.heldItems,
                    string.Empty,
                    rnd
                );
            }
            else
            {
                placeDungeonRewards(Items.ShuffledDungeonRewards, rnd);
            }

            /*
            // This is the old dungeon item placing code
            // starting room, list of checks to be randomized, items to be randomized, item pool, restriction
            Console.WriteLine("Placing Dungeon Rewards.");
            PlaceItemsRestricted(
                startingRoom,
                Items.ShuffledDungeonRewards,
                Randomizer.Items.heldItems,
                "Dungeon Rewards",
                rnd
            );
            */

            // We determine which dungeons are required after the dungeon rewards are placed but before the other checks
            // are placed because if a certain dungeon's checks need to be excluded, we want to exclude the check before
            // any items are placed in it.
            CheckUnrequiredDungeons();

            // Next we want to place items that are locked to a specific region such as keys, maps, compasses, etc.
            Console.WriteLine("Placing Region-Restricted Checks.");
            PlaceItemsRestricted(
                startingRoom,
                Items.RandomizedDungeonRegionItems,
                Randomizer.Items.heldItems,
                "Region",
                rnd
            );

            // Excluded checks are next and will just be filled with "junk" items (i.e. ammo refills, etc.). This is to
            // prevent important items from being placed in checks that the player or randomizer has requested to be not
            // considered in logic.
            Console.WriteLine("Placing Excluded Checks.");
            PlaceExcludedChecks(rnd);

            // Once all of the items that have some restriction on their placement are placed, we then place all of the items that can
            // be logically important (swords, clawshot, bow, etc.)
            Console.WriteLine("Placing Important Items.");
            PlaceItemsRestricted(
                startingRoom,
                Items.RandomizedImportantItems,
                Randomizer.Items.heldItems,
                string.Empty,
                rnd
            );

            // Next we will place the "always" items. Basically the constants in every seed, so Heart Pieces, Heart Containers, etc.
            // These items do not affect logic at all so there is very little constraint to this method.
            Console.WriteLine("Placing Non Impact Items.");
            PlaceNonImpactItems(Randomizer.Items.alwaysItems, rnd);

            // Any extra checks that have not been filled at this point are filled with "junk" items such as ammunition, foolish items, etc.
            Console.WriteLine("Placing Junk Items.");
            PlaceJunkItems(Items.JunkItems, rnd);

            // Only validate if we are not no-logic
            if (SSettings.logicRules != LogicRules.No_Logic)
            {
                if (!BackendFunctions.ValidatePlaythrough(startingRoom))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Fills locations with their original items.
        /// </summary>
        /// <param name="vanillaChecks"> A list of checks that will have their original item placed in them. </param>
        private static void PlaceVanillaChecks()
        {
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.checkStatus == "Vanilla")
                {
                    Randomizer.Items.heldItems.Remove(currentCheck.itemId);
                    PlaceItemInCheck(currentCheck.itemId, currentCheck);
                }
            }
        }

        /// <summary>
        /// Places junk items in checks that have been labeled as excluded.
        /// </summary>
        private static void PlaceExcludedChecks(Random rnd)
        {
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (!currentCheck.itemWasPlaced && (currentCheck.checkStatus.Contains("Excluded")))
                {
                    PlaceItemInCheck(
                        Items.JunkItems[rnd.Next(Items.JunkItems.Count)],
                        currentCheck
                    );
                }
            }
        }

        /// <summary>
        /// Places manually placed items where the user specifies
        /// </summary>
        private static void PlacePlandoChecks()
        {
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.checkStatus.Contains("Plando"))
                {
                    PlaceItemInCheck(currentCheck.itemId, currentCheck);
                }
            }
        }

        /// <summary>
        /// Places all items in an Item Group into the world graph based on a listed restriction.
        /// </summary>
        /// <param name="startingRoom"> The room node that the randomizer begins its graph building from. </param>
        /// <param name="itemGroup"> The group of items that are to be randomized in with the current restriction. </param>
        /// <param name="itemPool"> The current item pool. </param>
        /// <param name="restriction"> The restriction the randomizer must follow when checking where to place items. </param>
        private static void PlaceItemsRestricted(
            Room startingRoom,
            List<Item> itemGroup,
            List<Item> itemPool,
            string restriction,
            Random rnd
        )
        {
            // Essentially we want to do the following: make a copy of our item pool for safe keeping so we can modify
            // the current item pool as the playthrough happens. We ONLY modify our copied item pool if we place an item.
            // Once all of the items in ItemGroup have been placed, we dump our item pool and restore it with the copy we have.
            if (itemGroup.Count > 0)
            {
                List<string> availableChecks = new();
                Item itemToPlace;
                Check checkToReciveItem;
                List<Item> itemsToBeRandomized = new();
                List<Item> playthroughItems = new();
                List<Item> currentItemPool = new();
                currentItemPool.AddRange(itemPool);

                // The itemGroup list is intended to be readonly so we want to make a copy of it and modify the copy.
                itemsToBeRandomized.AddRange(itemGroup);

                while (itemsToBeRandomized.Count > 0)
                {
                    // NEEDS WORK: currently we have to dump the item pool and then refill it with the copy because if not,
                    // the item pool will compound and be way too big affecting both memory and logic.
                    itemPool.Clear();
                    itemPool.AddRange(currentItemPool);
                    itemToPlace = itemsToBeRandomized[rnd.Next(itemsToBeRandomized.Count)];

                    // Console.WriteLine("Item to place: " + itemToPlace);
                    itemPool.Remove(itemToPlace);
                    itemsToBeRandomized.Remove(itemToPlace);
                    foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
                    {
                        Check currentCheck = checkList.Value;
                        currentCheck.hasBeenReached = false;
                        Checks.CheckDict[currentCheck.checkName] = currentCheck;
                    }

                    // Walk through the current graph and get a list of rooms that we can currently access
                    // If we collect any items during the playthrough, we add them to the player's inventory
                    // and try walking through the graph again until we have collected every item that we can.
                    do
                    {
                        playthroughItems.Clear();
                        List<Room> currentPlaythroughGraph = GeneratePlaythroughGraph(startingRoom);
                        foreach (Room graphRoom in currentPlaythroughGraph)
                        {
                            graphRoom.Visited = true;
                            //Console.WriteLine("Currently Exploring: " + graphRoom.RoomName);
                            for (int i = 0; i < graphRoom.Checks.Count; i++)
                            {
                                // Create reference to the dictionary entry of the check whose logic we are evaluating
                                if (
                                    !Checks.CheckDict.TryGetValue(
                                        graphRoom.Checks[i],
                                        out Check currentCheck
                                    )
                                )
                                {
                                    if (graphRoom.Checks[i].ToString() == string.Empty)
                                    {
                                        // Console.WriteLine("Room has no checks, continuing on....");
                                        break;
                                    }
                                }
                                if (!currentCheck.hasBeenReached)
                                {
                                    var areCheckRequirementsMet = false;
                                    if (SSettings.logicRules == LogicRules.No_Logic)
                                    {
                                        areCheckRequirementsMet = true;
                                    }
                                    else
                                    {
                                        areCheckRequirementsMet = Logic.EvaluateRequirements(
                                            currentCheck.checkName,
                                            currentCheck.requirements
                                        );
                                    }

                                    if ((bool)areCheckRequirementsMet == true)
                                    {
                                        if (currentCheck.itemWasPlaced)
                                        {
                                            playthroughItems.Add(currentCheck.itemId);

                                            /*Console.WriteLine(
                                                "Added " + currentCheck.itemId + " to item list."
                                            );*/
                                        }
                                        else
                                        {
                                            if (
                                                (restriction == "Region")
                                                && (currentCheck.checkStatus != "Excluded")
                                                && (currentCheck.checkStatus != "Plando")
                                            )
                                            {
                                                if (
                                                    RoomFunctions.IsRegionCheck(
                                                        itemToPlace,
                                                        currentCheck,
                                                        graphRoom
                                                    )
                                                )
                                                {
                                                    // Console.WriteLine("Added " + currentCheck.checkName + " to check list.");
                                                    availableChecks.Add(currentCheck.checkName);
                                                }
                                            }
                                            else if (currentCheck.checkStatus == "Ready")
                                            {
                                                if (restriction == "Dungeon Rewards")
                                                {
                                                    if (
                                                        currentCheck.checkCategory.Contains(
                                                            "Dungeon Reward"
                                                        )
                                                    )
                                                    {
                                                        // Console.WriteLine("Added " + currentCheck.checkName + " to check list.");
                                                        availableChecks.Add(currentCheck.checkName);
                                                    }
                                                }
                                                else if (Randomizer.SSettings.noSmallKeysOnBosses)
                                                {
                                                    if (
                                                        !ItemFunctions.IsSmallKeyOnBossCheck(
                                                            itemToPlace,
                                                            currentCheck
                                                        )
                                                    )
                                                    {
                                                        // Console.WriteLine("Added " + currentCheck.checkName + " to check list.");
                                                        availableChecks.Add(currentCheck.checkName);
                                                    }
                                                }
                                                else
                                                {
                                                    // Console.WriteLine("Added " + currentCheck.checkName + " to check list.");
                                                    availableChecks.Add(currentCheck.checkName);
                                                }
                                            }
                                        }

                                        currentCheck.hasBeenReached = true;
                                    }
                                }
                            }
                        }

                        itemPool.AddRange(playthroughItems);
                    } while (playthroughItems.Count > 0);
                    checkToReciveItem = Checks.CheckDict[
                        availableChecks[rnd.Next(availableChecks.Count)].ToString()
                    ];
                    currentItemPool.Remove(itemToPlace);
                    PlaceItemInCheck(itemToPlace, checkToReciveItem);
                    availableChecks.Clear();
                }

                itemPool.Clear();
                itemPool.AddRange(currentItemPool);
            }
        }

        /// <summary>
        /// Places all items in a list into the world with no restrictions.
        /// </summary>
        /// <param name="itemsToBeRandomized"> The group of items that are to be randomized. </param>
        private static void PlaceNonImpactItems(List<Item> itemGroup, Random rnd)
        {
            List<string> availableChecks = new();
            Item itemToPlace;
            Check checkToReciveItem;

            // The itemGroup list is intended to be readonly so we want to make a copy of it and modify the copy.
            List<Item> itemsToBeRandomized = new();
            itemsToBeRandomized.AddRange(itemGroup);

            while (itemsToBeRandomized.Count > 0)
            {
                itemToPlace = itemsToBeRandomized[rnd.Next(itemsToBeRandomized.Count - 1)];

                // Console.WriteLine("Item to place: " + itemToPlace);
                itemsToBeRandomized.Remove(itemToPlace);
                foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
                {
                    checkToReciveItem = checkList.Value;
                    if (!checkToReciveItem.itemWasPlaced)
                    {
                        availableChecks.Add(checkToReciveItem.checkName);
                    }
                }

                checkToReciveItem = Checks.CheckDict[
                    availableChecks[rnd.Next(availableChecks.Count - 1)].ToString()
                ];
                PlaceItemInCheck(itemToPlace, checkToReciveItem);
                availableChecks.Clear();
            }
        }

        /// <summary>
        /// Places all items in a list into the world with no restrictions. Does not empty the list of items, however.
        /// </summary>
        /// <param name="itemsToBeRandomized"> The group of items that are to be randomized. </param>
        private static void PlaceJunkItems(List<Item> itemsToBeRandomized, Random rnd)
        {
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (!currentCheck.itemWasPlaced)
                {
                    PlaceItemInCheck(
                        itemsToBeRandomized[rnd.Next(itemsToBeRandomized.Count - 1)],
                        currentCheck
                    );
                }
            }
        }

        /// <summary>
        /// Places a given item into a given check.
        /// </summary>
        /// <param name="item"> The item to be placed in the check. </param>
        /// <param name="check"> The check to recieve the item. </param>
        private static void PlaceItemInCheck(Item item, Check check)
        {
            // Console.WriteLine("Placing item in check.");
            check.itemWasPlaced = true;
            check.itemId = item;

            //Console.WriteLine("Placed " + check.itemId + " in check " + check.checkName);
        }

        private static void StartOver()
        {
            // If we are restarting we want to empty the player's inventory since we don't know what items we have and it won't matter if we are restarting.
            Randomizer.Items.heldItems.Clear();

            // Next we want to change any checks that were marked as unrequired since the generator could select different dungeons next time. We also want to make all checks available to be placed again.
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.checkStatus == "Excluded-Unrequired")
                {
                    currentCheck.checkStatus = "Ready";
                }
                currentCheck.hasBeenReached = false;
                currentCheck.itemWasPlaced = false;
                Checks.CheckDict[currentCheck.checkName] = currentCheck;
            }

            // Next for Entrance rando, we want to clear the current room and entrance tables since they will be re-generated as the generator will try to re-shuffle the entrances a different way to find a placement that is successful.
            Randomizer.Rooms.RoomDict.Clear();
            DeserializeRooms(SSettings);
            Randomizer.EntranceRandomizer.SpawnTable.Clear();

            // Finally set the required dungeons to 0 since the value may change during the next attempt.
            Randomizer.RequiredDungeons = 0;
        }

        private static void CheckUnrequiredDungeons()
        {
            int palace = 0;
            int city = 1;
            int tot = 2;
            //int snowpeak = 3;
            int arbiters = 4;
            int lakebed = 5;
            //int mines = 6;
            int forest = 7;
            List<string>[] listOfAffectedChecks = new List<string>[]
            {
                CheckFunctions.palaceRequirementChecks,
                CheckFunctions.cityRequirementChecks,
                CheckFunctions.totRequirementChecks,
                CheckFunctions.snowpeakRequirementChecks,
                CheckFunctions.arbitersRequirementChecks,
                CheckFunctions.lakebedRequirementChecks,
                CheckFunctions.minesRequirementChecks,
                CheckFunctions.forestRequirementChecks
            };

            // Create the dungeon entries
            requiredDungeons forestTemple = new("Forest Temple Dungeon Reward", false, null);
            requiredDungeons goronMines = new("Goron Mines Dungeon Reward", false, null);
            requiredDungeons lakebedTemple = new("Lakebed Temple Dungeon Reward", false, null);
            requiredDungeons arbitersGrounds = new("Arbiters Grounds Dungeon Reward", false, null);
            requiredDungeons snowpeakRuins = new("Snowpeak Ruins Dungeon Reward", false, null);
            requiredDungeons templeOfTime = new("Temple of Time Dungeon Reward", false, null);
            requiredDungeons cityInTheSky = new("City in The Sky Dungeon Reward", false, null);
            requiredDungeons palaceOfTwilight =
                new("Palace of Twilight Zant Heart Container", false, null);

            requiredDungeons[] listOfRequiredDungeons = new requiredDungeons[]
            {
                palaceOfTwilight,
                cityInTheSky,
                templeOfTime,
                snowpeakRuins,
                arbitersGrounds,
                lakebedTemple,
                goronMines,
                forestTemple,
            };

            for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
            {
                listOfRequiredDungeons[i].requirementChecks = listOfAffectedChecks[i];
            }

            // First we want to check the Hyrule Castle access requirements to get the base required dungeons to access Hyrule.
            if (Randomizer.SSettings.castleRequirements == CastleRequirements.Fused_Shadows)
            {
                // First we want to loop through all of our potentially required dungeons
                for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                {
                    // Next we want to loop through each required check for each dungeon and see if there is a dungeon reward that matches the requirement. Note: we check all requirement checks as they can still signify that a dungeon is required, even if the check isn't necessarily in a dungeon (i.e DMT Poe signifies that GM is required.)
                    foreach (string dungeonCheck in listOfRequiredDungeons[i].requirementChecks)
                    {
                        Check currentCheck = Checks.CheckDict[dungeonCheck];
                        if (
                            currentCheck.itemId == Item.Progressive_Fused_Shadow
                            && currentCheck.itemWasPlaced
                        )
                        {
                            listOfRequiredDungeons[i].isRequired = true;
                            break;
                        }
                    }
                }
            }
            else if (Randomizer.SSettings.castleRequirements == CastleRequirements.Mirror_Shards)
            {
                // First we want to loop through all of our potentially required dungeons
                for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                {
                    // Next we want to loop through each required check for each dungeon and see if there is a dungeon reward that matches the requirement. Note: we check all requirement checks as they can still signify that a dungeon is required, even if the check isn't necessarily in a dungeon (i.e DMT Poe signifies that GM is required.)
                    foreach (string dungeonCheck in listOfRequiredDungeons[i].requirementChecks)
                    {
                        Check currentCheck = Checks.CheckDict[dungeonCheck];
                        if (
                            currentCheck.itemId == Item.Progressive_Mirror_Shard
                            && currentCheck.itemWasPlaced
                        )
                        {
                            listOfRequiredDungeons[i].isRequired = true;
                            break;
                        }
                    }
                }
            }
            else if (Randomizer.SSettings.castleRequirements == CastleRequirements.Vanilla)
            {
                // If Palace is required then Arbiters is automatically required.
                listOfRequiredDungeons[arbiters].isRequired = true;
                listOfRequiredDungeons[palace].isRequired = true;
                if (Randomizer.SSettings.palaceRequirements == PalaceRequirements.Fused_Shadows)
                {
                    // First we want to loop through all of our potentially required dungeons
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        // Next we want to loop through each required check for each dungeon and see if there is a dungeon reward that matches the requirement. Note: we check all requirement checks as they can still signify that a dungeon is required, even if the check isn't necessarily in a dungeon (i.e DMT Poe signifies that GM is required.)
                        foreach (string dungeonCheck in listOfRequiredDungeons[i].requirementChecks)
                        {
                            Check currentCheck = Checks.CheckDict[dungeonCheck];
                            if (
                                currentCheck.itemId == Item.Progressive_Fused_Shadow
                                && currentCheck.itemWasPlaced
                            )
                            {
                                listOfRequiredDungeons[i].isRequired = true;
                                break;
                            }
                        }
                    }
                }
                else if (
                    Randomizer.SSettings.palaceRequirements == PalaceRequirements.Mirror_Shards
                )
                {
                    // First we want to loop through all of our potentially required dungeons
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        // Next we want to loop through each required check for each dungeon and see if there is a dungeon reward that matches the requirement. Note: we check all requirement checks as they can still signify that a dungeon is required, even if the check isn't necessarily in a dungeon (i.e DMT Poe signifies that GM is required.)
                        foreach (string dungeonCheck in listOfRequiredDungeons[i].requirementChecks)
                        {
                            Check currentCheck = Checks.CheckDict[dungeonCheck];
                            if (
                                currentCheck.itemId == Item.Progressive_Mirror_Shard
                                && currentCheck.itemWasPlaced
                            )
                            {
                                listOfRequiredDungeons[i].isRequired = true;
                                break;
                            }
                        }
                    }
                }
                else if (Randomizer.SSettings.palaceRequirements == PalaceRequirements.Vanilla)
                {
                    listOfRequiredDungeons[city].isRequired = true;
                }
            }
            else if (Randomizer.SSettings.castleRequirements == CastleRequirements.All_Dungeons)
            {
                for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                {
                    listOfRequiredDungeons[i].isRequired = true;
                }
            }

            if (listOfRequiredDungeons[palace].isRequired)
            {
                // If Palace is required then Arbiters is automatically required.
                listOfRequiredDungeons[arbiters].isRequired = true;
                listOfRequiredDungeons[palace].isRequired = true;
                if (Randomizer.SSettings.palaceRequirements == PalaceRequirements.Fused_Shadows)
                {
                    // First we want to loop through all of our potentially required dungeons
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        // Next we want to loop through each required check for each dungeon and see if there is a dungeon reward that matches the requirement. Note: we check all requirement checks as they can still signify that a dungeon is required, even if the check isn't necessarily in a dungeon (i.e DMT Poe signifies that GM is required.)
                        foreach (string dungeonCheck in listOfRequiredDungeons[i].requirementChecks)
                        {
                            Check currentCheck = Checks.CheckDict[dungeonCheck];
                            if (
                                currentCheck.itemId == Item.Progressive_Fused_Shadow
                                && currentCheck.itemWasPlaced
                            )
                            {
                                listOfRequiredDungeons[i].isRequired = true;
                                break;
                            }
                        }
                    }
                }
                else if (
                    Randomizer.SSettings.palaceRequirements == PalaceRequirements.Mirror_Shards
                )
                {
                    // First we want to loop through all of our potentially required dungeons
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        // Next we want to loop through each required check for each dungeon and see if there is a dungeon reward that matches the requirement. Note: we check all requirement checks as they can still signify that a dungeon is required, even if the check isn't necessarily in a dungeon (i.e DMT Poe signifies that GM is required.)
                        foreach (string dungeonCheck in listOfRequiredDungeons[i].requirementChecks)
                        {
                            Check currentCheck = Checks.CheckDict[dungeonCheck];
                            if (
                                currentCheck.itemId == Item.Progressive_Mirror_Shard
                                && currentCheck.itemWasPlaced
                            )
                            {
                                listOfRequiredDungeons[i].isRequired = true;
                                break;
                            }
                        }
                    }
                }
                else if (Randomizer.SSettings.palaceRequirements == PalaceRequirements.Vanilla)
                {
                    listOfRequiredDungeons[city].isRequired = true;
                }
            }

            // If MDH is not skipped then we need to complete Lakebed to enter Hyrule
            if (!Randomizer.SSettings.skipMdh)
            {
                listOfRequiredDungeons[lakebed].isRequired = true;
            }

            if (Randomizer.SSettings.logicRules == LogicRules.Glitchless)
            {
                // If we are playing glitchless and Skybooks are vanilla and are needed for City, we conclude that ToT is required as Impaz will have a book in village. This will change with ER.
                if (
                    listOfRequiredDungeons[city].isRequired
                    && !Randomizer.SSettings.shuffleNpcItems
                    && !Randomizer.SSettings.skipCityEntrance
                )
                {
                    listOfRequiredDungeons[tot].isRequired = true;
                }

                // If Faron Woods is closed then we need to beat Forest Temple to leave.
                if (Randomizer.SSettings.faronWoodsLogic == FaronWoodsLogic.Closed)
                {
                    listOfRequiredDungeons[forest].isRequired = true;
                }
            }

            for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
            {
                if (!listOfRequiredDungeons[i].isRequired)
                {
                    if (Randomizer.SSettings.barrenDungeons)
                    {
                        foreach (string check in listOfRequiredDungeons[i].requirementChecks)
                        {
                            if (
                                Checks.CheckDict[check].checkStatus != "Vanilla"
                                && Checks.CheckDict[check].checkStatus != "Excluded"
                            )
                            {
                                // Note: this used to check against
                                // itemWasPlaced, but this caused dungeonReward
                                // checks in unrequired barren dungeons to not
                                // be marked as "Excluded-Unrequired".

                                //Console.WriteLine(check + " is now excluded");
                                Checks.CheckDict[check].checkStatus = "Excluded-Unrequired";
                            }
                        }
                    }
                }
                else
                {
                    Randomizer.RequiredDungeons |= 0x80 >> i;
                    Console.WriteLine(
                        listOfRequiredDungeons[i].dungeonReward + " is a required Dungeon!"
                    );
                }
            }
        }

        private static void SetupGraph()
        {
            // We want to be safe and make sure that the room classes are prepped and ready to be linked together. Then we define our starting room.
            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            // This line is just filler until we have a random starting room
            Room startingRoom = Randomizer.Rooms.RoomDict["Outside Links House"];

            Entrance rootExit = new();
            rootExit.ConnectedArea = startingRoom.RoomName;
            rootExit.Requirements = "(true)";

            Randomizer.Rooms.RoomDict["Root"].Exits.Add(rootExit);
        }

        private static void DeserializeChecks(SharedSettings SSettings)
        {
            string[] files;

            // We keep the logic files seperate based on their logic. GC and Wii should use the same logic.
            if (SSettings.logicRules == LogicRules.Glitchless)
            {
                files = System.IO.Directory.GetFiles(
                    Global.CombineRootPath("./World/Checks/"),
                    "*",
                    SearchOption.AllDirectories
                );
            }
            else
            {
                files = System.IO.Directory.GetFiles(
                    Global.CombineRootPath("./Glitched-World/Checks/"),
                    "*",
                    SearchOption.AllDirectories
                );
            }

            // Sort so that the item placement algorithm produces the exact same
            // result in production and development.
            // If we have already generated a dictionary from DeserializeCheckMetadata, then we only need to apply the logic data from the files.
            Array.Sort(files, new FilenameComparer());
            if (Checks.CheckDict.Count == 0)
            {
                foreach (string file in files)
                {
                    string contents = File.ReadAllText(file);
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    Checks.CheckDict.Add(fileName, new Check());
                    Checks.CheckDict[fileName] = JsonConvert.DeserializeObject<Check>(contents);
                    Check currentCheck = Checks.CheckDict[fileName];
                    currentCheck.checkName = fileName;
                    currentCheck.requirements = "(" + currentCheck.requirements + ")";
                    currentCheck.checkStatus = "Ready";
                    currentCheck.itemWasPlaced = false;
                    currentCheck.isRequired = false;
                    Checks.CheckDict[fileName] = currentCheck;
                }
            }
            else
            {
                foreach (string file in files)
                {
                    string contents = File.ReadAllText(file);
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    Check currentCheck = JsonConvert.DeserializeObject<Check>(contents);
                    Checks.CheckDict[fileName].requirements = "(" + currentCheck.requirements + ")";
                    Checks.CheckDict[fileName].checkCategory = currentCheck.checkCategory;
                    Checks.CheckDict[fileName].checkName = fileName;
                    Checks.CheckDict[fileName].checkStatus = "Ready";
                    Checks.CheckDict[fileName].itemWasPlaced = false;
                    Checks.CheckDict[fileName].isRequired = false;
                    Checks.CheckDict[fileName].itemId = currentCheck.itemId;
                }
            }
        }

        private static void DeserializeCheckData(
            SharedSettings SSettings,
            FileCreationSettings FcSettings
        )
        {
            string[] files = null;

            // The GC/Wii files have different offsets for the data that is needed to replace certain checks.
            switch (FcSettings.gameRegion)
            {
                // For now, 'All' only generates for GameCube until we do more
                // work related to Wii code.
                case GameRegion.GC_USA:
                case GameRegion.GC_EUR:
                case GameRegion.GC_JAP:
                case GameRegion.All:
                {
                    files = System.IO.Directory.GetFiles(
                        Global.CombineRootPath("./Assets/CheckMetadata/Gamecube/"),
                        "*",
                        SearchOption.AllDirectories
                    );
                    break;
                }

                case GameRegion.WII_10_USA:
                case GameRegion.WII_10_EU:
                case GameRegion.WII_10_JP:
                {
                    files = System.IO.Directory.GetFiles(
                        Global.CombineRootPath("./Assets/CheckMetadata/Wii1.0/"),
                        "*",
                        SearchOption.AllDirectories
                    );
                    break;
                }
            }

            // Sort so that the item placement algorithm produces the exact same
            // result in production and development.
            Array.Sort(files, new FilenameComparer());

            foreach (string file in files)
            {
                string contents = File.ReadAllText(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                Checks.CheckDict.Add(fileName, new Check());
                Checks.CheckDict[fileName] = JsonConvert.DeserializeObject<Check>(contents);
                Checks.CheckDict[fileName].checkName = fileName;
            }

            DeserializeChecks(SSettings);
        }

        public static void DeserializeRooms(SharedSettings SSettings)
        {
            //Before anything, create an entry for the root of the world
            Randomizer.Rooms.RoomDict.Add("Root", new Room());
            Randomizer.Rooms.RoomDict["Root"].RoomName = "Root";
            Randomizer.Rooms.RoomDict["Root"].Exits = new();
            Randomizer.Rooms.RoomDict["Root"].Checks = new();
            Randomizer.Rooms.RoomDict["Root"].Visited = false;

            string[] files;
            if (SSettings.logicRules == LogicRules.Glitchless)
            {
                files = System.IO.Directory.GetFiles(
                    Global.CombineRootPath("./World/Rooms/"),
                    "*",
                    SearchOption.AllDirectories
                );
            }
            else
            {
                files = System.IO.Directory.GetFiles(
                    Global.CombineRootPath("./Glitched-World/Rooms/"),
                    "*",
                    SearchOption.AllDirectories
                );
            }

            // Sort so that the item placement algorithm produces the exact same
            // result in production and development.
            Array.Sort(files, new FilenameComparer());

            foreach (string file in files)
            {
                string contents = File.ReadAllText(file);
                string fileName = Path.GetFileNameWithoutExtension(file);

                //Console.WriteLine("Loading Room File: " + fileName);

                List<Room> fileRooms = JsonConvert.DeserializeObject<List<Room>>(contents);
                foreach (Room room in fileRooms)
                {
                    Randomizer.Rooms.RoomDict.Add(room.RoomName, new Room());
                    Randomizer.Rooms.RoomDict[room.RoomName] = room;
                    Room currentRoom = Randomizer.Rooms.RoomDict[room.RoomName];
                    currentRoom.Visited = false;
                    for (int i = 0; i < currentRoom.Exits.Count; i++)
                    {
                        currentRoom.Exits[i].Requirements =
                            "(" + currentRoom.Exits[i].Requirements + ")";

                        currentRoom.Exits[i].ParentArea = currentRoom.RoomName;
                        currentRoom.Exits[i].OriginalConnectedArea = currentRoom.Exits[
                            i
                        ].ConnectedArea;
                    }

                    Randomizer.Rooms.RoomDict[room.RoomName] = currentRoom;
                    //Console.WriteLine("Room created: " + room.RoomName);
                }

                //Console.WriteLine("Room File Loaded " + fileName);
            }
        }

        /// <summary>
        /// summary text.
        /// </summary>
        private static void CleanUp()
        {
            Checks.CheckDict.Clear();
            Rooms.RoomDict.Clear();
            Items.ShuffledDungeonRewards.Clear();
            Items.RandomizedDungeonRegionItems.Clear();
            Randomizer.Items.RandomizedImportantItems.Clear();
            Items.JunkItems.Clear();
            Randomizer.Items.heldItems.Clear();
            Randomizer.Items.BaseItemPool.Clear();
        }

        public struct requiredDungeons
        {
            public string dungeonReward;
            public bool isRequired;
            public List<String> requirementChecks;

            public requiredDungeons(
                string dungeonReward,
                bool isRequired,
                List<string> requirementChecks
            )
            {
                this.dungeonReward = dungeonReward;
                this.isRequired = isRequired;
                this.requirementChecks = requirementChecks;
            }
        };

        public static string GetSeedGenResultsJson(
            string seedId,
            bool dangerouslyPrintFullRaceSpoiler = false
        )
        {
            string inputPath = Global.CombineOutputPath("seeds", seedId, "input.json");
            if (!File.Exists(inputPath))
            {
                throw new Exception("input.json not found for id '" + seedId + "'.");
            }

            string fileContents = File.ReadAllText(inputPath);
            JObject json = JsonConvert.DeserializeObject<JObject>(fileContents);

            if (Checks.CheckDict.Count < 1)
            {
                DeserializeChecks(SSettings);
            }

            SeedGenResults seedGenResults = new SeedGenResults(seedId, json);

            return seedGenResults.ToSpoilerString(
                GetSortedCheckNameToItemNameDict(seedGenResults),
                dangerouslyPrintFullRaceSpoiler
            );
        }

        private static SortedDictionary<string, string> GetSortedCheckNameToItemNameDict(
            SeedGenResults seedGenResults
        )
        {
            if (Checks.CheckDict.Count < 1)
            {
                // Can't deserialize twice if generating the spoiler in the same
                // call as the GCI creation(s).
                DeserializeChecks(SSettings);
                DeserializeRooms(SSettings);
            }

            foreach (KeyValuePair<int, byte> kvp in seedGenResults.itemPlacements)
            {
                // key is checkId, value is itemId
                string checkName = CheckIdClass.GetCheckName(kvp.Key);
                if (Randomizer.Checks.CheckDict.ContainsKey(checkName))
                {
                    Randomizer.Checks.CheckDict[checkName].itemId = (Item)kvp.Value;
                }
            }

            SharedSettings sharedSettings = SharedSettings.FromString(
                seedGenResults.settingsString
            );

            SortedDictionary<string, string> checkNameToItemName = new(StringComparer.Ordinal);

            foreach (KeyValuePair<string, Check> kvp in Checks.CheckDict)
            {
                string checkId = CheckIdClass.FromString(kvp.Key);
                int checkIdNum = CheckIdClass.GetCheckIdNum(kvp.Key);
                if (checkId == null || checkIdNum < 0)
                {
                    throw new Exception(
                        "Need to update CheckId to support check named \"" + kvp.Key + "\"."
                    );
                }

                Check check = kvp.Value;

                if (seedGenResults.itemPlacements.ContainsKey(checkIdNum))
                {
                    check.itemId = (Item)seedGenResults.itemPlacements[checkIdNum];
                }

                if (!sharedSettings.shuffleNpcItems && check.checkCategory.Contains("Bug Reward"))
                {
                    checkNameToItemName[check.checkName] = "Vanilla";
                }
                else
                {
                    checkNameToItemName[check.checkName] = check.itemId.ToString();
                }
            }

            return checkNameToItemName;
        }

        private static void placeDungeonRewards(List<Item> ShuffledDungeonRewards, Random rnd)
        {
            List<Check> dungeonRewards = new();
            List<Item> itemsToBeRandomized = new();
            int numAttemptsRemaining = 30;
            itemsToBeRandomized.AddRange(ShuffledDungeonRewards);
            if (itemsToBeRandomized.Count > 0)
            {
                Check currentCheck;
                Item currentItem;
                foreach (KeyValuePair<string, Check> kvp in Checks.CheckDict)
                {
                    currentCheck = kvp.Value;
                    if (
                        currentCheck.checkCategory.Contains("Dungeon Reward")
                        || (
                            Randomizer.SSettings.shuffleRewards
                            && (currentCheck.checkStatus == "Ready")
                        )
                    )
                    {
                        dungeonRewards.Add(currentCheck);
                    }
                }

                while (itemsToBeRandomized.Count > 0)
                {
                    if (numAttemptsRemaining == 0)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    currentCheck = dungeonRewards[rnd.Next(dungeonRewards.Count)];
                    currentItem = itemsToBeRandomized[rnd.Next(itemsToBeRandomized.Count)];

                    // We don't want to lock ourselves out of Palace
                    if (currentCheck.checkCategory.Contains("Palace of Twilight"))
                    {
                        if (
                            Randomizer.SSettings.palaceRequirements
                                == PalaceRequirements.Fused_Shadows
                            && (currentItem == Item.Progressive_Fused_Shadow)
                        )
                        {
                            continue;
                        }

                        if (
                            Randomizer.SSettings.palaceRequirements
                                == PalaceRequirements.Mirror_Shards
                            && (currentItem == Item.Progressive_Mirror_Shard)
                        )
                        {
                            continue;
                        }
                    }
                    if (currentCheck.checkStatus == "Excluded")
                    {
                        // Don't place a required dungeon reward on a check that is excluded
                        if (
                            Randomizer.SSettings.castleRequirements
                                == CastleRequirements.Fused_Shadows
                            && (currentItem == Item.Progressive_Fused_Shadow)
                        )
                        {
                            numAttemptsRemaining--;
                            continue;
                        }

                        if (
                            Randomizer.SSettings.castleRequirements
                                == CastleRequirements.Mirror_Shards
                            && (currentItem == Item.Progressive_Mirror_Shard)
                        )
                        {
                            numAttemptsRemaining--;
                            continue;
                        }
                    }
                    PlaceItemInCheck(currentItem, currentCheck);
                    // for debugging
                    /*Console.WriteLine(
                        "Placed Reward: " + currentItem + " in: " + currentCheck.checkName
                    );*/
                    itemsToBeRandomized.Remove(currentItem);
                    dungeonRewards.Remove(currentCheck);
                    Randomizer.Items.heldItems.Remove(currentItem);
                }
            }
        }
    }

    internal class FilenameComparer : IComparer<string>
    {
        int IComparer<string>.Compare(string s1, string s2)
        {
            return String.CompareOrdinal(
                Path.GetFileNameWithoutExtension(s1),
                Path.GetFileNameWithoutExtension(s2)
            );
        }
    }
}
