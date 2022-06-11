namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        /// A reference to the settings structures that need to be used by the randomizer.
        /// </summary>
        public static RandomizerSetting RandoSetting = new();

        /// <summary>
        /// A reference to the settings that the settings string use that need to be used by the randomizer.
        /// </summary>
        public static readonly SettingData RandoSettingData = new();

        /// <summary>
        /// The most recent version of the randomizer that will support the seed generated.
        /// </summary>
        public static readonly byte RandomizerVersionMajor = 1;

        /// <summary>
        /// The oldest version of the randomizer that will support the seed generated..
        /// </summary>
        public static readonly byte RandomizerVersionMinor = 0;

        /// <summary>
        /// Generates a randomizer seed given a settings string.
        /// </summary>
        /// <param name="settingsString"> The Settings String to be read in. </param>
        public static bool Start(string settingsString, string generatorSeedHash, string seed)
        {
            bool generationStatus = false;
            int remainingGenerationAttempts = 30;
            RandomizerSetting parseSetting = Randomizer.RandoSetting;
            Console.WriteLine(
                "Twilight Princess Randomizer Version "
                    + RandomizerVersionMajor
                    + "."
                    + RandomizerVersionMinor
            );
            // Random rnd = new();
            Random rnd = new(Util.Hash.CalculateMD5(seed));
            string seedHash = generatorSeedHash;
            ;

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks();
            DeserializeRooms();

            // Read in the settings string and set the settings values accordingly
            BackendFunctions.InterpretSettingsString(settingsString);

            // Generate the item pool based on user settings/input.
            Randomizer.Items.GenerateItemPool();
            CheckFunctions.GenerateCheckList();

            // Generate the world based on the room class values and their neighbour values. If we want to randomize entrances, we would do it before this step.
            Room startingRoom = SetupGraph();
            while (remainingGenerationAttempts > 0)
            {
                foreach (Item startingItem in parseSetting.StartingItems)
                {
                    Randomizer.Items.heldItems.Add(startingItem);
                }
                Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);
                remainingGenerationAttempts--;
                try
                {
                    // Place the items in the world based on the starting room.
                    PlaceItemsInWorld(startingRoom, rnd);
                    Console.WriteLine("Generating Seed Data.");
                    Assets.SeedData.GenerateSeedData(seedHash);
                    Console.WriteLine("Generating Spoiler Log.");
                    BackendFunctions.GenerateSpoilerLog(startingRoom, seedHash);
                    IEnumerable<string> fileList = new string[]
                    {
                        "TPR-v1.0-" + seedHash + ".txt",
                        "TPR-v1.0-" + seedHash + "-Seed-Data.gci"
                    };
                    BackendFunctions.CreateZipFile("Seed/TPR-v1.0-" + seedHash + ".zip", fileList);
                    Console.WriteLine("Generation Complete!");
                    generationStatus = true;
                    break;
                }
                // If for some reason the assumed fill fails, we want to dump everything and start over.
                catch (ArgumentOutOfRangeException a)
                {
                    Console.WriteLine(a + " No checks remaining, starting over..");
                    StartOver();
                    continue;
                }
            }

            CleanUp();
            return generationStatus;
        }

        public static bool CreateInputJson(string settingsString, string seed)
        {
            // Generate seedHash from seed
            if (seed == null || seed.Length < 1)
            {
                // 132 bits of data as 22 characters. Wanted at least 128 bits,
                // and the 6bit encoding only needs 22 chars instead of the 32
                // hex characters you would normally use for 128 bits. Use 2
                // characters because we don't want it to be easily confused
                // with the seed's id in the URL.
                seed = Util.Hash.GenId() + Util.Hash.GenId();
            }

            int seedHash = Util.Hash.CalculateMD5(seed);

            bool generationStatus = false;
            int remainingGenerationAttempts = 30;
            Console.WriteLine(
                "Twilight Princess Randomizer Version "
                    + RandomizerVersionMajor
                    + "."
                    + RandomizerVersionMinor
            );
            Random rnd = new Random(seedHash);
            // string seedHash = generatorSeedHash;

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks();
            DeserializeRooms();

            // Read in the settings string and set the settings values accordingly
            // BackendFunctions.InterpretSettingsString(settingsString);
            RandoSetting = RandomizerSetting.FromString(settingsString);

            foreach (string checkName in RandoSetting.ExcludedChecks)
            {
                Randomizer.Checks.CheckDict[checkName].checkStatus = "Excluded";
            }

            // Generate the item pool based on user settings/input.
            Randomizer.Items.GenerateItemPool();
            CheckFunctions.GenerateCheckList();

            // Generate the world based on the room class values and their neighbour values. If we want to randomize entrances, we would do it before this step.
            Room startingRoom = SetupGraph();
            while (remainingGenerationAttempts > 0)
            {
                foreach (Item startingItem in Randomizer.RandoSetting.StartingItems)
                {
                    Randomizer.Items.heldItems.Add(startingItem);
                }
                Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);
                remainingGenerationAttempts--;
                try
                {
                    // Place the items in the world based on the starting room.
                    PlaceItemsInWorld(startingRoom, rnd);
                    // Console.WriteLine("Generating Seed Data.");
                    // Assets.SeedData.GenerateSeedData(seedHash);
                    // Console.WriteLine("Generating Spoiler Log.");
                    // BackendFunctions.GenerateSpoilerLog(startingRoom, seedHash);
                    // IEnumerable<string> fileList = new string[]
                    // {
                    //     "TPR-v1.0-" + seedHash + ".txt",
                    //     "TPR-v1.0-" + seedHash + "-Seed-Data.gci"
                    // };
                    // BackendFunctions.CreateZipFile("Seed/TPR-v1.0-" + seedHash + ".zip", fileList);
                    // Console.WriteLine("Generation Complete!");
                    generationStatus = true;
                    break;
                }
                // If for some reason the assumed fill fails, we want to dump everything and start over.
                catch (ArgumentOutOfRangeException a)
                {
                    Console.WriteLine(a + " No checks remaining, starting over..");
                    StartOver();
                    continue;
                }
            }

            string jsonContent = GenerateInputJsonContent(settingsString, seed, seedHash);

            string id = "";
            string outputPath = "";
            bool idConfirmedUnique = false;

            while (!idConfirmedUnique)
            {
                id = Util.Hash.GenId();
                // outputPath = Path.Combine("seeds", id, "input.json");
                outputPath = Global.CombineOutputPath("seeds", id, "input.json");

                if (!File.Exists(outputPath))
                    idConfirmedUnique = true;
            }

            try
            {
                // Write json file to id dir.
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, jsonContent);

                Console.WriteLine("SUCCESS:" + id);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem writing input.json file for id: " + id);
                System.Environment.Exit(1);
            }

            CleanUp();
            return generationStatus;
        }

        private static string GenerateInputJsonContent(
            string settingsString,
            string seed,
            int seedHash
        )
        {
            Dictionary<string, Item> checkIdToItemId = new();
            List<string> placementStrParts = new();

            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                if (checkList.Value.checkStatus != "Vanilla")
                {
                    string checkId = CheckIdClass.FromString(checkList.Key);
                    if (checkId == null)
                    {
                        throw new Exception(
                            "Need to update CheckId to support check named \""
                                + checkList.Key
                                + "\"."
                        );
                    }
                    placementStrParts.Add(checkId + "_" + (byte)checkList.Value.itemId);
                    checkIdToItemId[checkId] = checkList.Value.itemId;
                }
            }

            placementStrParts.Sort();
            string itemPlacementPart = String.Join("-", placementStrParts);

            //

            // Only need to take settings into account which are important to part2.

            // Dictionary<string, Object> part2Settings = GenPart2Settings(false);

            SortedDictionary<string, object> part2SettingsForString = GenPart2Settings(false);

            string part2SettingsPart = JsonConvert.SerializeObject(part2SettingsForString);

            // string aaa = JsonConvert.SerializeObject(part2Settings);

            //

            string filenameInput = String.Join(
                "%%%%",
                new List<string> { itemPlacementPart, part2SettingsPart, seed }
            );

            int filenameBits = Util.Hash.CalculateMD5(filenameInput);
            string filename = Util.Filename.GenFilename(filenameBits);
            // convert bits to name


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

            Dictionary<string, object> inputJsonRoot = new();
            inputJsonRoot.Add("version", "1");
            inputJsonRoot.Add("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            inputJsonRoot.Add("settingsString", settingsString);
            inputJsonRoot.Add("seed", seed);
            inputJsonRoot.Add("seedHash", seedHash.ToString("x8"));
            inputJsonRoot.Add("filename", filename);
            inputJsonRoot.Add("settings", GenPart2Settings(true));
            inputJsonRoot.Add("itemPlacement", checkIdToItemId);

            return JsonConvert.SerializeObject(inputJsonRoot);
        }

        private static SortedDictionary<string, object> GenPart2Settings(bool noExclusions)
        {
            SortedDictionary<string, object> part2Settings = new();

            // If a setting matches what the game behavior would have been
            // before that setting existed, we leave it off when noExclusions is
            // false.

            // Multi-option fields which are only included for certain values
            if (noExclusions || RandoSetting.castleRequirements != "Vanilla")
                part2Settings.Add("castleRequirements", RandoSetting.castleRequirements);
            if (noExclusions || RandoSetting.palaceRequirements != "Vanilla")
                part2Settings.Add("palaceRequirements", RandoSetting.palaceRequirements);
            // TODO: Change this one to a boolean called "faronWoodsOpen"
            if (noExclusions || RandoSetting.faronWoodsLogic == "Open")
                part2Settings.Add("faronWoodsLogic", RandoSetting.faronWoodsLogic);
            if (noExclusions || RandoSetting.smallKeySettings == "Keysey")
                part2Settings.Add("smallKeySettings", RandoSetting.smallKeySettings);
            if (noExclusions || RandoSetting.bossKeySettings == "Keysey")
                part2Settings.Add("bossKeySettings", RandoSetting.bossKeySettings);
            if (noExclusions || RandoSetting.mapAndCompassSettings == "Start_With")
                part2Settings.Add("mapAndCompassSettings", RandoSetting.mapAndCompassSettings);

            // Boolean fields included when true
            if (noExclusions || RandoSetting.mdhSkipped)
                part2Settings.Add("mdhSkipped", RandoSetting.mdhSkipped);
            if (noExclusions || RandoSetting.introSkipped)
                part2Settings.Add("introSkipped", RandoSetting.introSkipped);
            if (noExclusions || RandoSetting.faronTwilightCleared)
                part2Settings.Add("faronTwilightCleared", RandoSetting.faronTwilightCleared);
            if (noExclusions || RandoSetting.eldinTwilightCleared)
                part2Settings.Add("eldinTwilightCleared", RandoSetting.eldinTwilightCleared);
            if (noExclusions || RandoSetting.lanayruTwilightCleared)
                part2Settings.Add("lanayruTwilightCleared", RandoSetting.lanayruTwilightCleared);
            if (noExclusions || RandoSetting.skipMinorCutscenes)
                part2Settings.Add("skipMinorCutscenes", RandoSetting.skipMinorCutscenes);
            if (noExclusions || RandoSetting.fastIronBoots)
                part2Settings.Add("fastIronBoots", RandoSetting.fastIronBoots);
            if (noExclusions || RandoSetting.quickTransform)
                part2Settings.Add("quickTransform", RandoSetting.quickTransform);
            if (noExclusions || RandoSetting.transformAnywhere)
                part2Settings.Add("transformAnywhere", RandoSetting.transformAnywhere);
            if (noExclusions || RandoSetting.increaseWallet)
                part2Settings.Add("increaseWallet", RandoSetting.increaseWallet);
            if (noExclusions || RandoSetting.modifyShopModels)
                part2Settings.Add("modifyShopModels", RandoSetting.modifyShopModels);

            // Complex fields
            if (noExclusions || RandoSetting.StartingItems?.Count > 0)
                part2Settings.Add("StartingItems", RandoSetting.StartingItems);

            if (noExclusions)
            {
                // Any settings which are not factored into determining if two
                // outputs should have the same filename should go in here. We
                // only include these in the generated `input.json` so we can
                // show the user the values in the generator UI.

                // Value unimportant once items are placed
                part2Settings.Add("logicRules", RandoSetting.logicRules);
                part2Settings.Add("goldenBugsShuffled", RandoSetting.goldenBugsShuffled);
                part2Settings.Add("poesShuffled", RandoSetting.poesShuffled);
                part2Settings.Add("npcItemsShuffled", RandoSetting.npcItemsShuffled);
                part2Settings.Add("shopItemsShuffled", RandoSetting.shopItemsShuffled);
                // Store as number because less space, and also doesn't prevent
                // us from adjusting the check names in the future.
                part2Settings.Add(
                    "ExcludedChecks",
                    RandoSetting.ExcludedChecks.Select(CheckIdClass.GetCheckIdNum).ToList()
                );
                part2Settings.Add("shuffleHiddenSkills", RandoSetting.shuffleHiddenSkills);
                part2Settings.Add("shuffleSkyCharacters", RandoSetting.shuffleSkyCharacters);
                part2Settings.Add("iceTrapSettings", RandoSetting.iceTrapSettings);

                // // Input to part2 of the generation process (varies by player)
                // int TunicColor;
                // int MidnaHairColor;
                // int lanternColor;
                // int heartColor;
                // int aButtonColor;
                // int bButtonColor;
                // int xButtonColor;
                // int yButtonColor;
                // int zButtonColor;
                // bool shuffleBackgroundMusic;
                // bool shuffleItemFanfares;
                // bool disableEnemyBackgoundMusic;
                // string gameRegion;

                // Not stored on the server
                // int seedNumber;
            }

            return part2Settings;
        }

        public static bool GenerateFinalOutput(string id, string tempArg, string pSettingsString)
        {
            string inputJsonPath = Global.CombineOutputPath("seeds", id, "input.json");

            if (!File.Exists(inputJsonPath))
            {
                throw new Exception("input.json not found for (path: " + inputJsonPath + ") id: " + id);
            }

            string fileContents = File.ReadAllText(inputJsonPath);
            JObject json = JsonConvert.DeserializeObject<JObject>(fileContents);

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks();
            DeserializeRooms();

            // Read in the settings string and set the settings values accordingly
            // BackendFunctions.InterpretSettingsString(settingsString);
            RandoSetting.PopulateFromInputJson(json);
            PSettings pSettings = PSettings.FromString(pSettingsString);

            Dictionary<string, Item> itemPlacement = json["itemPlacement"].ToObject<
                Dictionary<string, Item>
            >();

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check check = checkList.Value;
                string checkId = CheckIdClass.FromString(check.checkName);
                if (itemPlacement.ContainsKey(checkId))
                {
                    check.itemId = itemPlacement[checkId];
                }
            }

            // TODO: ^ fill in settings from the JObject json

            // Generate the item pool based on user settings/input.
            // Randomizer.Items.GenerateItemPool();
            // CheckFunctions.GenerateCheckList();

            // Generate the world based on the room class values and their neighbour values. If we want to randomize entrances, we would do it before this step.
            // Room startingRoom = SetupGraph();
            // while (remainingGenerationAttempts > 0)
            // {
            //     foreach (Item startingItem in parseSetting.StartingItems)
            //     {
            //         Randomizer.Items.heldItems.Add(startingItem);
            //     }
            //     Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);
            //     remainingGenerationAttempts--;
            // try
            // {
            //         // Place the items in the world based on the starting room.
            //         PlaceItemsInWorld(startingRoom, rnd);
            Console.WriteLine("\nGenerating Seed Data.");
            // Assets.SeedData.GenerateSeedData("aBc"); // just making up a seed hash right now
            Assets.SeedData.GenerateSeedDataNew(tempArg, pSettings); // just making up a seed hash right now
            Console.WriteLine("Done!");
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
            //         StartOver();
            //         continue;
            //     }
            // }

            CleanUp();
            // return generationStatus;

            return true;
        }

        public static bool GenerateFinalOutput2(string id, string tempArg, string pSettingsString)
        {
            string inputJsonPath = Global.CombineOutputPath("seeds", id, "input.json");

            if (!File.Exists(inputJsonPath))
            {
                throw new Exception("input.json not found for (path: " + inputJsonPath + ") id: " + id);
            }

            string fileContents = File.ReadAllText(inputJsonPath);
            JObject json = JsonConvert.DeserializeObject<JObject>(fileContents);

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks();
            DeserializeRooms();

            // Read in the settings string and set the settings values accordingly
            // BackendFunctions.InterpretSettingsString(settingsString);
            RandoSetting.PopulateFromInputJson(json);
            PSettings pSettings = PSettings.FromString(pSettingsString);

            Dictionary<string, Item> itemPlacement = json["itemPlacement"].ToObject<
                Dictionary<string, Item>
            >();

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check check = checkList.Value;
                string checkId = CheckIdClass.FromString(check.checkName);
                if (itemPlacement.ContainsKey(checkId))
                {
                    check.itemId = itemPlacement[checkId];
                }
            }

            // TODO: ^ fill in settings from the JObject json

            // Generate the item pool based on user settings/input.
            // Randomizer.Items.GenerateItemPool();
            // CheckFunctions.GenerateCheckList();

            // Generate the world based on the room class values and their neighbour values. If we want to randomize entrances, we would do it before this step.
            // Room startingRoom = SetupGraph();
            // while (remainingGenerationAttempts > 0)
            // {
            //     foreach (Item startingItem in parseSetting.StartingItems)
            //     {
            //         Randomizer.Items.heldItems.Add(startingItem);
            //     }
            //     Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);
            //     remainingGenerationAttempts--;
            // try
            // {
            //         // Place the items in the world based on the starting room.
            //         PlaceItemsInWorld(startingRoom, rnd);
            Console.WriteLine("\nGenerating Seed Data.");
            // Assets.SeedData.GenerateSeedData("aBc"); // just making up a seed hash right now
            byte[] bytes = Assets.SeedData.GenerateSeedDataNewByteArray(tempArg, pSettings); // just making up a seed hash right now


            List<Dictionary<string, object>> jsonRoot = new();
            Dictionary<string, object> dict = new();
            dict.Add("name", "exampleNameFile.gci");
            dict.Add("length", bytes.Length);
            jsonRoot.Add(dict);

            // inputJsonRoot.Add("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            // inputJsonRoot.Add("settingsString", settingsString);
            // inputJsonRoot.Add("seed", seed);
            // inputJsonRoot.Add("seedHash", seedHash.ToString("x8"));
            // inputJsonRoot.Add("filename", filename);
            // inputJsonRoot.Add("settings", GenPart2Settings(true));
            // inputJsonRoot.Add("itemPlacement", checkIdToItemId);
            string fileDefs = JsonConvert.SerializeObject(jsonRoot);
            string hexValue = fileDefs.Length.ToString("x8");

            Console.Write("BYTES:");
            Console.Write(hexValue);
            Console.Write(fileDefs);
            // Write 8 chars of hex for length of json
            // json looks like: [{name: '', length: number}, {}]

            using (Stream myOutStream = Console.OpenStandardOutput())
            {
                myOutStream.Write(bytes, 0, bytes.Length);
            }

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
            //         StartOver();
            //         continue;
            //     }
            // }

            CleanUp();
            // return generationStatus;

            return true;
        }

        /// <summary>
        /// Places a given item into a given check.
        /// </summary>
        /// <param name="startingRoom"> The room that the player will start the game from. </param>
        /// <returns> A complete playthrough graph for the player to traverse. </returns>
        public static List<Room> GeneratePlaythroughGraph(Room startingRoom)
        {
            List<Room> playthroughGraph = new();

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
                    for (int i = 0; i < roomsToExplore[0].Neighbours.Count; i++)
                    {
                        // If you can access the neighbour and it hasnt been visited yet.
                        if (
                            Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]].Visited
                            == false
                        )
                        {
                            // Parse the neighbour's requirements to find out if we can access it
                            var areNeighbourRequirementsMet = Logic.EvaluateRequirements(
                                roomsToExplore[0].NeighbourRequirements[i]
                            );
                            if ((bool)areNeighbourRequirementsMet == true)
                            {
                                if (
                                    !Randomizer.Rooms.RoomDict[
                                        roomsToExplore[0].Neighbours[i]
                                    ].ReachedByPlaythrough
                                )
                                {
                                    availableRooms++;
                                    Randomizer.Rooms.RoomDict[
                                        roomsToExplore[0].Neighbours[i]
                                    ].ReachedByPlaythrough = true;
                                    playthroughGraph.Add(
                                        Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]]
                                    );
                                }
                                roomsToExplore.Add(
                                    Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]]
                                );
                                Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]].Visited =
                                    true;

                                /*Console.WriteLine(
                                    "Neighbour: "
                                        + Randomizer.Rooms.RoomDict[
                                            roomsToExplore[0].Neighbours[i]
                                        ].RoomName
                                        + " added to room list."
                                );*/
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
            // Any vanilla checks will be placed first for the sake of logic. Even if they aren't available to be randomized in the game yet,
            // we may need to logically account for their placement.
            Console.WriteLine("Placing Vanilla Checks.");
            PlaceVanillaChecks();

            // Excluded checks are next and will just be filled with "junk" items (i.e. ammo refills, etc.). This is to
            // prevent important items from being placed in checks that the player or randomizer has requested to be not
            // considered in logic.
            Console.WriteLine("Placing Excluded Checks.");
            PlaceExcludedChecks(rnd);

            // Dungeon rewards have a very limited item pool, so we want to place them first to prevent the generator from putting
            // an unnecessary item in one of the checks.
            // starting room, list of checks to be randomized, items to be randomized, item pool, restriction
            Console.WriteLine("Placing Dungeon Rewards.");
            PlaceItemsRestricted(
                startingRoom,
                Items.ShuffledDungeonRewards,
                Randomizer.Items.heldItems,
                "Dungeon Rewards",
                rnd
            );

            // Next we want to place items that are locked to a specific region such as keys, maps, compasses, etc.
            Console.WriteLine("Placing Region-Restricted Checks.");
            PlaceItemsRestricted(
                startingRoom,
                Items.RandomizedDungeonRegionItems,
                Randomizer.Items.heldItems,
                "Region",
                rnd
            );

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
            PlaceNonImpactItems(Items.alwaysItems, rnd);

            // Any extra checks that have not been filled at this point are filled with "junk" items such as ammunition, foolish items, etc.
            Console.WriteLine("Placing Junk Items.");
            PlaceJunkItems(Items.JunkItems, rnd);

            return;
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

            return;
        }

        /// <summary>
        /// Places junk items in checks that have been labeled as excluded.
        /// </summary>
        private static void PlaceExcludedChecks(Random rnd)
        {
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (!currentCheck.itemWasPlaced && (currentCheck.checkStatus == "Excluded"))
                {
                    PlaceItemInCheck(
                        Items.JunkItems[rnd.Next(Items.JunkItems.Count)],
                        currentCheck
                    );
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
                            // Console.WriteLine("Currently Exploring: " + graphRoom.RoomName);
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
                                    var areCheckRequirementsMet = Logic.EvaluateRequirements(
                                        currentCheck.requirements
                                    );
                                    if ((bool)areCheckRequirementsMet == true)
                                    {
                                        if (currentCheck.itemWasPlaced)
                                        {
                                            playthroughItems.Add(currentCheck.itemId);

                                            // Console.WriteLine("Added " + currentCheck.itemId + " to item list.");
                                        }
                                        else
                                        {
                                            if (restriction == "Region")
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
                                            else if (restriction == "Dungeon Rewards")
                                            {
                                                if (
                                                    currentCheck.category.Contains("Dungeon Reward")
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

            return;
        }

        /// <summary>
        /// Places all items in a list into the world with no restrictions.
        /// </summary>
        /// <param name="itemsToBeRandomized"> The group of items that are to be randomized. </param>
        private static void PlaceNonImpactItems(List<Item> itemsToBeRandomized, Random rnd)
        {
            List<string> availableChecks = new();
            Item itemToPlace;
            Check checkToReciveItem;

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

            return;
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

            return;
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

            // Console.WriteLine("Placed " + check.itemId + " in check " + check.checkName);
            return;
        }

        private static void StartOver()
        {
            Randomizer.Items.heldItems.Clear();
            Console.WriteLine("Logical error. Starting Over.");
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                currentCheck.hasBeenReached = false;
                currentCheck.itemWasPlaced = false;
                Checks.CheckDict[currentCheck.checkName] = currentCheck;
            }

            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            Randomizer.Rooms.RoomDict["Ordon Province"].IsStartingRoom = true;
        }

        private static Room SetupGraph()
        {
            // We want to be safe and make sure that the room classes are prepped and ready to be linked together. Then we define our starting room.
            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            Room startingRoom = Randomizer.Rooms.RoomDict["Ordon Province"];
            startingRoom.IsStartingRoom = true;
            Randomizer.Rooms.RoomDict["Ordon Province"] = startingRoom;
            return startingRoom;
        }

        private static void DeserializeChecks()
        {
            foreach (
                string file in System.IO.Directory.GetFiles(
                    // "./Generator/World/Checks/",
                    Global.CombineRootPath("./World/Checks/"),
                    "*",
                    SearchOption.AllDirectories
                )
            )
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
                Checks.CheckDict[fileName] = currentCheck;

                //Console.WriteLine("Check File Loaded " + fileName);
            }

            return;
        }

        private static void DeserializeRooms()
        {
            foreach (
                string file in System.IO.Directory.GetFiles(
                    // "./Generator/World/Rooms/",
                    Global.CombineRootPath("./World/Rooms/"),
                    "*",
                    SearchOption.AllDirectories
                )
            )
            {
                string contents = File.ReadAllText(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                Randomizer.Rooms.RoomDict.Add(fileName, new Room());
                Randomizer.Rooms.RoomDict[fileName] = JsonConvert.DeserializeObject<Room>(contents);
                Room currentRoom = Randomizer.Rooms.RoomDict[fileName];
                currentRoom.RoomName = fileName;
                currentRoom.Visited = false;
                currentRoom.IsStartingRoom = false;
                for (int i = 0; i < currentRoom.NeighbourRequirements.Count; i++)
                {
                    currentRoom.NeighbourRequirements[i] =
                        "(" + currentRoom.NeighbourRequirements[i] + ")";
                }

                Randomizer.Rooms.RoomDict[fileName] = currentRoom;

                // Console.WriteLine("Room File Loaded " + fileName);
            }

            return;
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
    }
}
