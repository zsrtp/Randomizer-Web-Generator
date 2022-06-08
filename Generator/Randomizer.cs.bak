namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Generates a randomizer seed given a settings string.
    /// </summary>
    public class Randomizer
    {
        /// <summary>
        /// A reference to all logic functions that need to be used by the randomizer.
        /// </summary>
        public static readonly LogicFunctions Logic = new ();

        /// <summary>
        /// A reference to all check-related functions that need to be used by the randomizer.
        /// </summary>
        public static readonly CheckFunctions Checks = new ();

        /// <summary>
        /// A reference to all room-related functions that need to be used by the randomizer.
        /// </summary>
        public static readonly RoomFunctions Rooms = new ();

        /// <summary>
        /// A reference to all item lists and functions that need to be used by the randomizer.
        /// </summary>
        public static readonly ItemFunctions Items = new ();

        /// <summary>
        /// A reference to the settings structures that need to be used by the randomizer.
        /// </summary>
        public static readonly RandomizerSetting RandoSetting = new ();

        /// <summary>
        /// A reference to the settings that the settings string use that need to be used by the randomizer.
        /// </summary>
        public static readonly SettingData RandoSettingData = new ();

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
        public static bool Start(string settingsString)
        {
            bool generationStatus = false;
            int remainingGenerationAttempts = 30;
            Console.WriteLine("Twilight Princess Randomizer Version " + RandomizerVersionMajor + "." + RandomizerVersionMinor);
            Random rnd = new ();
            string seedHash =
                HashAssets.hashAdjectives[rnd.Next(HashAssets.hashAdjectives.Count - 1)]
                + "-"
                + HashAssets.characterNames[rnd.Next(HashAssets.characterNames.Count - 1)];

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
                Randomizer.Items.heldItems.AddRange(Randomizer.Items.BaseItemPool);
                remainingGenerationAttempts--;
                try
                {
                    // Place the items in the world based on the starting room.
                    PlaceItemsInWorld(startingRoom);
                    Console.WriteLine("Generating Seed Data.");
                    Assets.SeedData.GenerateSeedData(seedHash);
                    Console.WriteLine("Generating Spoiler Log.");
                    BackendFunctions.GenerateSpoilerLog(startingRoom, seedHash);
                    IEnumerable<string> fileList = new string[] {"TPR-v1.0-" + seedHash + ".txt", "TPR-v1.0-" + seedHash + "-Seed-Data.gci"};
                    BackendFunctions.CreateZipFile("TPR-v1.0-" + seedHash + ".zip", fileList);
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

        /// <summary>
        /// Places a given item into a given check.
        /// </summary>
        /// <param name="startingRoom"> The room that the player will start the game from. </param>
        /// <returns> A complete playthrough graph for the player to traverse. </returns>
        public static List<Room> GeneratePlaythroughGraph(Room startingRoom)
        {
            List<Room> playthroughGraph = new ();
            
            int availableRooms = 1;
            List<Room> roomsToExplore = new ();

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
                        if (Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]].Visited == false)
                        {
                            // Parse the neighbour's requirements to find out if we can access it
                            var areNeighbourRequirementsMet = Logic.EvaluateRequirements(
                            roomsToExplore[0].NeighbourRequirements[i]);
                            if ((bool)areNeighbourRequirementsMet == true)
                            {
                                if (!Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]].ReachedByPlaythrough)
                                {
                                    availableRooms++;
                                    Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]].ReachedByPlaythrough = true;
                                    playthroughGraph.Add(Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]]);
                                }
                                roomsToExplore.Add(Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]]);
                                Randomizer.Rooms.RoomDict[roomsToExplore[0].Neighbours[i]].Visited = true;

                                // Console.WriteLine("Neighbour: " + currentNeighbour.name + " added to room list.");
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
        private static void PlaceItemsInWorld(Room startingRoom)
        {
            // Any vanilla checks will be placed first for the sake of logic. Even if they aren't available to be randomized in the game yet,
            // we may need to logically account for their placement.
            Console.WriteLine("Placing Vanilla Checks.");
            PlaceVanillaChecks();

            // Excluded checks are next and will just be filled with "junk" items (i.e. ammo refills, etc.). This is to
            // prevent important items from being placed in checks that the player or randomizer has requested to be not
            // considered in logic.
            Console.WriteLine("Placing Excluded Checks.");
            PlaceExcludedChecks();

            // Dungeon rewards have a very limited item pool, so we want to place them first to prevent the generator from putting
            // an unnecessary item in one of the checks.
            // starting room, list of checks to be randomized, items to be randomized, item pool, restriction
            Console.WriteLine("Placing Dungeon Rewards.");
            PlaceItemsRestricted(startingRoom, Items.ShuffledDungeonRewards, Randomizer.Items.heldItems, "Dungeon Rewards");

            // Next we want to place items that are locked to a specific region such as keys, maps, compasses, etc.
            Console.WriteLine("Placing Region-restriced Checks.");
            PlaceItemsRestricted(startingRoom, Items.RandomizedDungeonRegionItems, Randomizer.Items.heldItems, "Region");

            // Once all of the items that have some restriction on their placement are placed, we then place all of the items that can
            // be logically important (swords, clawshot, bow, etc.)
            Console.WriteLine("Placing Important Items.");
            PlaceItemsRestricted(startingRoom, Items.RandomizedImportantItems, Randomizer.Items.heldItems, string.Empty);

            // Next we will place the "always" items. Basically the constants in every seed, so Heart Pieces, Heart Containers, etc.
            // These items do not affect logic at all so there is very little constraint to this method.
            Console.WriteLine("Placing Non Impact Items.");
            PlaceNonImpactItems(Items.alwaysItems);

            // Any extra checks that have not been filled at this point are filled with "junk" items such as ammunition, foolish items, etc.
            Console.WriteLine("Placing Junk Items.");
            PlaceJunkItems(Items.JunkItems);

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
        private static void PlaceExcludedChecks()
        {
            Random rnd = new ();
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (!currentCheck.itemWasPlaced && (currentCheck.checkStatus == "Excluded"))
                {
                    PlaceItemInCheck(Items.JunkItems[rnd.Next(Items.JunkItems.Count - 1)], currentCheck);
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
        private static void PlaceItemsRestricted(Room startingRoom, List<Item> itemGroup, List<Item> itemPool, string restriction)
        {
            // Essentially we want to do the following: make a copy of our item pool for safe keeping so we can modify
            // the current item pool as the playthrough happens. We ONLY modify our copied item pool if we place an item.
            // Once all of the items in ItemGroup have been placed, we dump our item pool and restore it with the copy we have.
            if (itemGroup.Count > 0)
            {
                Random rnd = new ();
                List<string> availableChecks = new ();
                Item itemToPlace;
                Check checkToReciveItem;
                List<Item> itemsToBeRandomized = new ();
                List<Item> playthroughItems = new ();
                List<Item> currentItemPool = new ();
                currentItemPool.AddRange(itemPool);
                itemsToBeRandomized.AddRange(itemGroup);

                while (itemsToBeRandomized.Count > 0)
                {
                    // NEEDS WORK: currently we have to dump the item pool and then refill it with the copy because if not,
                    // the item pool will compound and be way too big affecting both memory and logic.
                    itemPool.Clear();
                    itemPool.AddRange(currentItemPool);
                    itemToPlace = itemsToBeRandomized[rnd.Next(itemsToBeRandomized.Count - 1)];

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
                            // Console.WriteLine("Currently Exploring: " + graphRoom.name);
                            for (int i = 0; i < graphRoom.Checks.Count; i++)
                            {
                                // Create reference to the dictionary entry of the check whose logic we are evaluating
                                if (!Checks.CheckDict.TryGetValue(graphRoom.Checks[i], out Check currentCheck))
                                {
                                    if (graphRoom.Checks[i].ToString() == string.Empty)
                                    {
                                        // Console.WriteLine("Room has no checks, continuing on....");
                                        break;
                                    }
                                }

                                if (!currentCheck.hasBeenReached)
                                {
                                    var areCheckRequirementsMet = Logic.EvaluateRequirements(currentCheck.requirements);
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
                                                if (RoomFunctions.IsRegionCheck(itemToPlace, currentCheck, graphRoom))
                                                {
                                                    // Console.WriteLine("Added " + currentCheck.checkName + " to check list.");
                                                    availableChecks.Add(currentCheck.checkName);
                                                }
                                            }
                                            else if (restriction == "Dungeon Rewards")
                                            {
                                                if (currentCheck.category.Contains("Dungeon Reward"))
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
                    }
                    while (playthroughItems.Count > 0);

                    checkToReciveItem = Checks.CheckDict[availableChecks[rnd.Next(availableChecks.Count - 1)].ToString()];
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
        private static void PlaceNonImpactItems(List<Item> itemsToBeRandomized)
        {
            Random rnd = new ();
            List<string> availableChecks = new ();
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

                checkToReciveItem = Checks.CheckDict[availableChecks[rnd.Next(availableChecks.Count - 1)].ToString()];
                PlaceItemInCheck(itemToPlace, checkToReciveItem);
                availableChecks.Clear();
            }

            return;
        }

        /// <summary>
        /// Places all items in a list into the world with no restrictions. Does not empty the list of items, however.
        /// </summary>
        /// <param name="itemsToBeRandomized"> The group of items that are to be randomized. </param>
        private static void PlaceJunkItems(List<Item> itemsToBeRandomized)
        {
            Random rnd = new ();
            foreach (KeyValuePair<string, Check> checkList in Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (!currentCheck.itemWasPlaced)
                {
                    PlaceItemInCheck(itemsToBeRandomized[rnd.Next(itemsToBeRandomized.Count - 1)], currentCheck);
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
            foreach (string file in System.IO.Directory.GetFiles("./Randomizer/World/Checks/", "*", SearchOption.AllDirectories))
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

                // Console.WriteLine("Check File Loaded " + fileName);
            }

            return;
        }

        private static void DeserializeRooms()
        {
            foreach (string file in System.IO.Directory.GetFiles("./Randomizer/World/Rooms/", "*", SearchOption.AllDirectories))
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
