namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.SSettings.Enums;
    using TPRandomizer.FcSettings.Enums;
    using System.Reflection;
    using Assets;

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
            int remainingGenerationAttempts = 30;

            Console.WriteLine("SeedData Version: " + SeedData.VersionString);

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks();
            DeserializeRooms();

            // Read in the settings string and set the settings values accordingly
            // BackendFunctions.InterpretSettingsString(settingsString);
            SSettings = SharedSettings.FromString(settingsString);
            PropertyInfo[] randoSettingProperties = SSettings.GetType().GetProperties();

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

            // Generate the world based on the room class values and their neighbour values. If we want to randomize entrances, we would do it before this step.
            Room startingRoom = SetupGraph();
            while (remainingGenerationAttempts > 0)
            {
                foreach (Item startingItem in Randomizer.SSettings.startingItems)
                {
                    Randomizer.Items.heldItems.Add(startingItem);
                }
                Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);
                remainingGenerationAttempts--;
                try
                {
                    // Place the items in the world based on the starting room.
                    PlaceItemsInWorld(startingRoom, rnd);
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
                string jsonContent = GenerateInputJsonContent(
                    settingsString,
                    seed,
                    seedHash,
                    isRaceSeed
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

        private static string GenerateInputJsonContent(
            string settingsString,
            string seed,
            int seedHash,
            bool isRaceSeed
        )
        {
            Dictionary<string, Item> checkIdToItemId = new();
            List<string> placementStrParts = new();
            SortedDictionary<int, byte> checkNumIdToItemId = new();

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

                    byte itemId = (byte)checkList.Value.itemId;

                    // For generating consistent filenames.
                    placementStrParts.Add(checkId + "_" + itemId);
                    checkIdToItemId[checkId] = checkList.Value.itemId;

                    // For storing placements in json file.
                    checkNumIdToItemId.Add(checkIdNum, itemId);
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
            string playthroughName = Util.PlaythroughName.GenName(filenameBits);

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
            builder.playthroughName = playthroughName;
            builder.requiredDungeons = (byte)Randomizer.RequiredDungeons;
            builder.SetItemPlacements(checkNumIdToItemId);

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
            if (SSettings.smallKeySettings == SmallKeySettings.Keysey)
                part2Settings.Add("smallKeySettings", SSettings.smallKeySettings);
            if (SSettings.bigKeySettings == BigKeySettings.Keysey)
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

            if (SSettings.skipMinesEntrance)
                part2Settings.Add("skipMinesEntrance", SSettings.skipMinesEntrance);
            if (SSettings.skipLakebedEntrance)
                part2Settings.Add("skipLakebedEntrance", SSettings.skipLakebedEntrance);
            if (SSettings.skipArbitersEntrance)
                part2Settings.Add("skipArbitersEntrance", SSettings.skipArbitersEntrance);
            if (SSettings.skipSnowpeakEntrance)
                part2Settings.Add("skipSnowpeakEntrance", SSettings.skipSnowpeakEntrance);
            if (SSettings.skipToTEntrance)
                part2Settings.Add("skipToTEntrance", SSettings.skipToTEntrance);
            if (SSettings.skipCityEntrance)
                part2Settings.Add("skipCityEntrance", SSettings.skipCityEntrance);

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

            SeedGenResults seedGenResults = new SeedGenResults(json);

            FileCreationSettings fcSettings = FileCreationSettings.FromString(fcSettingsString);
            SSettings = SharedSettings.FromString(seedGenResults.settingsString);

            // Generate the dictionary values that are needed and initialize the data for the selected logic type.
            DeserializeChecks();
            DeserializeRooms();

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

            byte[] bytes = SeedData.GenerateSeedDataBytes(seedGenResults, fcSettings);

            List<Dictionary<string, object>> jsonRoot = new();
            Dictionary<string, object> dict = new();

            string gameVer = "ge";
            switch (fcSettings.gameRegion)
            {
                case GameRegion.PAL:
                    gameVer = "gp";
                    break;
                case GameRegion.JAP:
                    gameVer = "gj";
                    break;
            }

            // dict.Add("name", "exampleNameFile.gci");
            dict.Add(
                "name",
                "TprSeed-v"
                    + SeedData.VersionString
                    + gameVer
                    + "-"
                    + fcSettings.seedNumber
                    + "--"
                    + seedGenResults.playthroughName
                    + ".gci"
            );
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
            // StartOver();
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

            // We determine which dungeons are required after the dungeon rewards are placed but before the other checks
            // are placed because if a certain dungeon's checks need to be excluded, we want to exclude the check before
            // any items are placed in it.
            CheckUnrequiredDungeons();

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
            PlaceNonImpactItems(Items.alwaysItems, rnd);

            // Any extra checks that have not been filled at this point are filled with "junk" items such as ammunition, foolish items, etc.
            Console.WriteLine("Placing Junk Items.");
            PlaceJunkItems(Items.JunkItems, rnd);
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
                                            if (currentCheck.checkStatus == "Ready")
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
                                                        currentCheck.category.Contains(
                                                            "Dungeon Reward"
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
            Randomizer.Items.heldItems.Clear();
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

            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            Randomizer.RequiredDungeons = 0;

            Randomizer.Rooms.RoomDict["Ordon Province"].IsStartingRoom = true;
        }

        private static void CheckUnrequiredDungeons()
        {
            Check dungeonCheck = new();
            int palace = 0;
            int city = 1;
            //int tot = 2;
            //int snowpeak = 3;
            int arbiters = 4;
            int lakebed = 5;
            //int mines = 6;
            int forest = 7;
            List<string>[] listOfAffectedChecks = new List<string>[]
            {
                CheckFunctions.palaceRequirementChecksGlitchless,
                CheckFunctions.cityRequirementChecksGlitchless,
                CheckFunctions.totRequirementChecksGlitchless,
                CheckFunctions.snowpeakRequirementChecksGlitchless,
                CheckFunctions.arbitersRequirementChecksGlitchless,
                CheckFunctions.lakebedRequirementChecksGlitchless,
                CheckFunctions.minesRequirementChecksGlitchless,
                CheckFunctions.forestRequirementChecksGlitchless
            };

            // Create the dungeon entries
            requiredDungeons forestTemple = new("Forest Temple Dungeon Reward", false, null);
            requiredDungeons goronMines = new("Goron Mines Dungeon Reward", false, null);
            requiredDungeons lakebedTemple = new("Lakebed Temple Dungeon Reward", false, null);
            requiredDungeons arbitersGrounds =
                new("Arbiters Grounds Stallord Heart Container", false, null);
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
                if (Randomizer.SSettings.logicRules == LogicRules.Glitchless)
                {
                    listOfRequiredDungeons[i].requirementChecks = listOfAffectedChecks[i];
                }
            }

            // First we want to check the Hyrule Castle access requirements to get the base required dungeons to access Hyrule.
            if (Randomizer.SSettings.castleRequirements == CastleRequirements.Fused_Shadows)
            {
                for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                {
                    if (
                        Checks.CheckDict[listOfRequiredDungeons[i].dungeonReward].itemId
                        == Item.Progressive_Fused_Shadow
                    )
                    {
                        listOfRequiredDungeons[i].isRequired = true;
                    }
                }
            }
            else if (Randomizer.SSettings.castleRequirements == CastleRequirements.Mirror_Shards)
            {
                for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                {
                    if (
                        Checks.CheckDict[listOfRequiredDungeons[i].dungeonReward].itemId
                        == Item.Progressive_Mirror_Shard
                    )
                    {
                        listOfRequiredDungeons[i].isRequired = true;
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
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        if (
                            Checks.CheckDict[listOfRequiredDungeons[i].dungeonReward].itemId
                            == Item.Progressive_Fused_Shadow
                        )
                        {
                            listOfRequiredDungeons[i].isRequired = true;
                        }
                    }
                }
                else if (
                    Randomizer.SSettings.palaceRequirements == PalaceRequirements.Mirror_Shards
                )
                {
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        if (
                            Checks.CheckDict[listOfRequiredDungeons[i].dungeonReward].itemId
                            == Item.Progressive_Mirror_Shard
                        )
                        {
                            listOfRequiredDungeons[i].isRequired = true;
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
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        if (
                            Checks.CheckDict[listOfRequiredDungeons[i].dungeonReward].itemId
                            == Item.Progressive_Fused_Shadow
                        )
                        {
                            listOfRequiredDungeons[i].isRequired = true;
                        }
                    }
                }
                else if (
                    Randomizer.SSettings.palaceRequirements == PalaceRequirements.Mirror_Shards
                )
                {
                    for (int i = 0; i < listOfRequiredDungeons.GetLength(0); i++)
                    {
                        if (
                            Checks.CheckDict[listOfRequiredDungeons[i].dungeonReward].itemId
                            == Item.Progressive_Mirror_Shard
                        )
                        {
                            listOfRequiredDungeons[i].isRequired = true;
                        }
                    }
                }
                else if (Randomizer.SSettings.palaceRequirements == PalaceRequirements.Vanilla)
                {
                    listOfRequiredDungeons[city].isRequired = true;
                }
            }

            // If Faron Woods is closed then we need to beat Forest Temple to leave.
            if (Randomizer.SSettings.faronWoodsLogic == FaronWoodsLogic.Closed)
            {
                listOfRequiredDungeons[forest].isRequired = true;
            }

            // If MDH is not skipped then we need to complete Lakebed to enter Hyrule
            if (!Randomizer.SSettings.skipMdh)
            {
                listOfRequiredDungeons[lakebed].isRequired = true;
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
                                (Checks.CheckDict[check].checkStatus != "Vanilla")
                                && !Checks.CheckDict[check].itemWasPlaced
                            )
                            {
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
            string[] files = System.IO.Directory.GetFiles(
                Global.CombineRootPath("./World/Checks/"),
                "*",
                SearchOption.AllDirectories
            );

            // Sort so that the item placement algorithm produces the exact same
            // result in production and development.
            Array.Sort(files, new FilenameComparer());

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
                if (currentCheck.category.Contains("Dungeon Reward"))
                {
                    currentCheck.itemId = Item.Recovery_Heart;
                }
                Checks.CheckDict[fileName] = currentCheck;

                // Console.WriteLine(fileName);
            }
        }

        private static void DeserializeRooms()
        {
            string[] files = System.IO.Directory.GetFiles(
                Global.CombineRootPath("./World/Rooms/"),
                "*",
                SearchOption.AllDirectories
            );

            // Sort so that the item placement algorithm produces the exact same
            // result in production and development.
            Array.Sort(files, new FilenameComparer());

            foreach (string file in files)
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
