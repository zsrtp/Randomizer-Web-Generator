namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;
    using System.IO.Compression;
    using Assets;

    /// <summary>
    /// summary text.
    /// </summary>
    public class BackendFunctions
    {
        /// <summary>
        /// summary text.
        /// </summary>
        private static bool ValidatePlaythrough(Room startingRoom)
        {
            bool areAllChecksReachable = true;
            bool areAllRoomsReachable = true;
            List<Item> playthroughItems = new();
            SharedSettings parseSetting = Randomizer.SSettings;

            // Console.WriteLine("Item to place: " + itemToPlace);
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                currentCheck.hasBeenReached = false;
                Randomizer.Checks.CheckDict[currentCheck.checkName] = currentCheck;
            }

            foreach (Item startingItem in parseSetting.startingItems)
            {
                Randomizer.Items.heldItems.Add(startingItem);
            }

            // Walk through the current graph and get a list of rooms that we can currently access
            // If we collect any items during the playthrough, we add them to the player's inventory
            // and try walking through the graph again until we have collected every item that we can.
            do
            {
                playthroughItems.Clear();
                List<Room> currentPlaythroughGraph = Randomizer.GeneratePlaythroughGraph(
                    startingRoom
                );
                foreach (Room graphRoom in currentPlaythroughGraph)
                {
                    // Console.WriteLine("Currently Exploring: " + graphRoom.name);
                    for (int i = 0; i < graphRoom.Checks.Count; i++)
                    {
                        // Create reference to the dictionary entry of the check whose logic we are evaluating
                        if (
                            !Randomizer.Checks.CheckDict.TryGetValue(
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
                            var areCheckRequirementsMet = Randomizer.Logic.EvaluateRequirements(
                                currentCheck.requirements
                            );
                            if ((bool)areCheckRequirementsMet == true)
                            {
                                if (currentCheck.itemWasPlaced)
                                {
                                    playthroughItems.Add(currentCheck.itemId);

                                    // Console.WriteLine("Added " + currentCheck.itemId + " to item list.");
                                }

                                currentCheck.hasBeenReached = true;
                            }
                        }
                    }
                }

                Randomizer.Items.heldItems.AddRange(playthroughItems);
            } while (playthroughItems.Count > 0);

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check listedCheck = checkList.Value;
                if (!listedCheck.hasBeenReached)
                {
                    areAllChecksReachable = false;
                    Console.WriteLine(listedCheck.checkName + " is not reachable!");
                }
            }

            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                if (!currentRoom.Visited)
                {
                    areAllRoomsReachable = false;
                    Console.WriteLine(currentRoom.RoomName + " is not reachable!");
                }
            }

            if (areAllChecksReachable && areAllRoomsReachable)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// summary text.
        /// </summary>
        private static List<string> CalculateOptimalPlaythrough(Room startingRoom)
        {
            SharedSettings parseSetting = Randomizer.SSettings;
            bool hasCompletedSphere;
            bool hasConcludedPlaythrough;
            List<List<string>> listofPlaythroughs = new();
            int sphereCount;
            List<Room> currentPlaythroughGraph;
            List<Item> playthroughItems = new();
            List<Item> sphereItems = new();
            Dictionary<string, Check> playthroughDictionary = new();
            sphereCount = 0;
            List<string> currentPlaythrough = new();
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check listedCheck = checkList.Value;
                listedCheck.hasBeenReached = false;
                Randomizer.Checks.CheckDict[listedCheck.checkName] = listedCheck;
            }

            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            Randomizer.Items.heldItems.Clear();
            foreach (Item startingItem in parseSetting.startingItems)
            {
                Randomizer.Items.heldItems.Add(startingItem);
                playthroughDictionary.Add("Starting Item: " + startingItem.ToString(), null);
            }

            while (!Randomizer.Rooms.RoomDict["Ganondorf Castle"].Visited)
            {
                hasCompletedSphere = false;
                hasConcludedPlaythrough = false;
                currentPlaythroughGraph = Randomizer.GeneratePlaythroughGraph(startingRoom);
                playthroughDictionary.Add("Sphere: " + sphereCount, null);

                // Walk through the current graph and get a list of rooms that we can currently access
                // If we collect any items during the playthrough, we add them to the player's inventory
                // and try walking through the graph again until we have collected every item that we can.

                sphereItems.Clear();
                foreach (Room graphRoom in currentPlaythroughGraph)
                {
                    // Console.WriteLine("Currently Exploring: " + graphRoom.name);
                    if (graphRoom.RoomName == "Ganondorf Castle")
                    {
                        graphRoom.Visited = true;
                        hasConcludedPlaythrough = true;
                        break;
                    }

                    for (int i = 0; i < graphRoom.Checks.Count; i++)
                    {
                        // Create reference to the dictionary entry of the check whose logic we are evaluating
                        if (
                            !Randomizer.Checks.CheckDict.TryGetValue(
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
                            var areCheckRequirementsMet = Randomizer.Logic.EvaluateRequirements(
                                currentCheck.requirements
                            );
                            if ((bool)areCheckRequirementsMet == true)
                            {
                                sphereItems.Add(currentCheck.itemId);
                                currentCheck.hasBeenReached = true;
                                if (
                                    Randomizer.Items.ImportantItems.Contains(currentCheck.itemId)
                                    || Randomizer.Items.RegionSmallKeys.Contains(
                                        currentCheck.itemId
                                    )
                                    || Randomizer.Items.DungeonBigKeys.Contains(currentCheck.itemId)
                                    || Randomizer.Items.VanillaDungeonRewards.Contains(
                                        currentCheck.itemId
                                    )
                                    || Randomizer.Items.goldenBugs.Contains(currentCheck.itemId)
                                    || (currentCheck.itemId == Item.Poe_Soul)
                                )
                                {
                                    playthroughDictionary.Add(
                                        "    "
                                            + currentCheck.checkName
                                            + ": "
                                            + currentCheck.itemId,
                                        currentCheck
                                    );
                                    hasCompletedSphere = true;
                                }
                            }
                        }
                    }
                }

                Randomizer.Items.heldItems.AddRange(sphereItems);

                sphereCount++;
                if ((hasCompletedSphere == false) && !hasConcludedPlaythrough)
                {
                    Console.WriteLine(
                        "Could not validate playthrough. There possibly is an error in logic or the specific playthrough has failed."
                    );
                    break;
                }
            }

            bool playthroughStatus;
            Item currentItem;
            foreach (KeyValuePair<string, Check> dictEntry in playthroughDictionary.Reverse())
            {
                if (dictEntry.Value != null)
                {
                    currentItem = dictEntry.Value.itemId;
                    Randomizer.Checks.CheckDict[dictEntry.Value.checkName].itemId = 0x0;
                    playthroughStatus = emulatePlaythrough(startingRoom);
                    if (playthroughStatus)
                    {
                        playthroughDictionary.Remove(dictEntry.Key);
                    }
                    else
                    {
                        Randomizer.Checks.CheckDict[dictEntry.Value.checkName].itemId = currentItem;
                    }
                }
            }
            int index = 1;
            currentPlaythrough.Add("Sphere: " + 0);
            for (int i = 0; i < playthroughDictionary.Count; i++)
            {
                KeyValuePair<string, Check> dictEntry = playthroughDictionary.ElementAt(i);
                if (dictEntry.Value == null)
                {
                    if ((i > 0) && playthroughDictionary.ElementAt(i - 1).Value != null)
                    {
                        currentPlaythrough.Add("Sphere: " + index);
                        index++;
                    }
                }
                else
                {
                    currentPlaythrough.Add(dictEntry.Key);
                }
            }

            currentPlaythrough.Add("    Ganondorf Castle: Ganondorf Defeated");

            return currentPlaythrough;
        }

        static bool emulatePlaythrough(Room startingRoom)
        {
            bool hasCompletedSphere;
            bool hasConcludedPlaythrough;
            List<Room> currentPlaythroughGraph;
            List<Item> sphereItems = new();
            SharedSettings parseSetting = Randomizer.SSettings;

            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check listedCheck = checkList.Value;
                listedCheck.hasBeenReached = false;
                Randomizer.Checks.CheckDict[listedCheck.checkName] = listedCheck;
            }

            foreach (KeyValuePair<string, Room> roomList in Randomizer.Rooms.RoomDict.ToList())
            {
                Room currentRoom = roomList.Value;
                currentRoom.Visited = false;
                Randomizer.Rooms.RoomDict[currentRoom.RoomName] = currentRoom;
            }

            Randomizer.Items.heldItems.Clear();
            foreach (Item startingItem in parseSetting.startingItems)
            {
                Randomizer.Items.heldItems.Add(startingItem);
            }

            while (!Randomizer.Rooms.RoomDict["Ganondorf Castle"].Visited)
            {
                hasCompletedSphere = false;
                hasConcludedPlaythrough = false;
                currentPlaythroughGraph = Randomizer.GeneratePlaythroughGraph(startingRoom);

                // Walk through the current graph and get a list of rooms that we can currently access
                // If we collect any items during the playthrough, we add them to the player's inventory
                // and try walking through the graph again until we have collected every item that we can.
                do
                {
                    sphereItems.Clear();
                    foreach (Room graphRoom in currentPlaythroughGraph)
                    {
                        // Console.WriteLine("Currently Exploring: " + graphRoom.name);
                        if (graphRoom.RoomName == "Ganondorf Castle")
                        {
                            graphRoom.Visited = true;
                            hasConcludedPlaythrough = true;
                            return true;
                        }

                        for (int i = 0; i < graphRoom.Checks.Count; i++)
                        {
                            // Create reference to the dictionary entry of the check whose logic we are evaluating
                            if (
                                !Randomizer.Checks.CheckDict.TryGetValue(
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
                                var areCheckRequirementsMet = Randomizer.Logic.EvaluateRequirements(
                                    currentCheck.requirements
                                );
                                if ((bool)areCheckRequirementsMet == true)
                                {
                                    currentCheck.hasBeenReached = true;
                                    if (
                                        Randomizer.Items.ImportantItems.Contains(
                                            currentCheck.itemId
                                        )
                                        || Randomizer.Items.RegionSmallKeys.Contains(
                                            currentCheck.itemId
                                        )
                                        || Randomizer.Items.DungeonBigKeys.Contains(
                                            currentCheck.itemId
                                        )
                                        || Randomizer.Items.VanillaDungeonRewards.Contains(
                                            currentCheck.itemId
                                        )
                                        || Randomizer.Items.goldenBugs.Contains(currentCheck.itemId)
                                        || (currentCheck.itemId == Item.Poe_Soul)
                                    )
                                    {
                                        sphereItems.Add(currentCheck.itemId);
                                        hasCompletedSphere = true;
                                    }
                                }
                            }
                        }
                    }

                    Randomizer.Items.heldItems.AddRange(sphereItems);
                } while (sphereItems.Count > 0);

                if ((hasCompletedSphere == false) && !hasConcludedPlaythrough)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static void GenerateSpoilerLog(Room startingRoom, string seedHash)
        {
            Check currentCheck;
            bool isPlaythroughValid;
            Randomizer.Items.GenerateItemPool();

            string fileHash = "TPR-v1.0-" + seedHash + ".txt";

            // Once everything is complete, we want to write the results to a spoiler log.
            using StreamWriter file = new(fileHash);

            file.WriteLine("SeedData Version: " + SeedData.VersionString);
            file.WriteLine("Settings: ");
            file.WriteLine(JsonConvert.SerializeObject(Randomizer.SSettings, Formatting.Indented));
            file.WriteLine(string.Empty);
            file.WriteLine("Dungeon Rewards: ");
            foreach (KeyValuePair<string, Check> check in Randomizer.Checks.CheckDict)
            {
                currentCheck = check.Value;
                if (currentCheck.itemWasPlaced && currentCheck.category.Contains("Dungeon Reward"))
                {
                    file.WriteLine(currentCheck.checkName + ": " + currentCheck.itemId);
                }
            }
            file.WriteLine(string.Empty);
            file.WriteLine("Item Locations: ");
            foreach (KeyValuePair<string, Check> check in Randomizer.Checks.CheckDict)
            {
                currentCheck = check.Value;
                if (currentCheck.itemWasPlaced)
                {
                    file.WriteLine(currentCheck.checkName + ": " + currentCheck.itemId);
                }
                else
                {
                    Console.WriteLine("Check: " + currentCheck.checkName + " has no item.");
                }
            }

            file.WriteLine(string.Empty);
            file.WriteLine(string.Empty);
            file.WriteLine(string.Empty);
            file.WriteLine("Playthrough: ");
            isPlaythroughValid = ValidatePlaythrough(startingRoom);
            if (isPlaythroughValid)
            {
                Console.WriteLine("Playthrough Validated");
            }
            else
            {
                Console.WriteLine("ERROR. Some checks/rooms may not be reachable.");
            }

            List<string> optimalPlaythrough = CalculateOptimalPlaythrough(startingRoom);
            optimalPlaythrough.ForEach(
                delegate(string playthroughItem)
                {
                    file.WriteLine(playthroughItem);
                }
            );
            file.Close();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static byte[,] ConcatFlagArrays(byte[,] destArray, byte[,] sourceArray)
        {
            byte[,] array3 = new byte[
                destArray.GetLength(0) + sourceArray.GetLength(0),
                destArray.GetLength(1) + sourceArray.GetLength(1)
            ];
            int j = 0;
            for (int i = 0; i < destArray.GetLength(0); i++)
            {
                array3[i, 0] = destArray[i, 0];
                array3[i, 1] = destArray[i, 1];
            }

            for (int i = destArray.GetLength(0); i < array3.GetLength(0); i++)
            {
                array3[i, 0] = sourceArray[j, 0];
                array3[i, 1] = sourceArray[j, 1];
                j++;
            }

            return array3;
        }

        private static IEnumerable<UInt64> Blockify(byte[] inputAsBytes, int blockSize)
        {
            int i = 0;

            // UInt64 used since that is the biggest possible value we can return.
            // Using an unsigned type is important - otherwise an arithmetic overflow will result
            UInt64 block = 0;

            // Run through all the bytes
            while (i < inputAsBytes.Length)
            {
                // Keep stacking them side by side by shifting left and OR-ing
                block = block << 8 | inputAsBytes[i];

                i++;

                // Return a block whenever we meet a boundary
                if (i % blockSize == 0 || i == inputAsBytes.Length)
                {
                    yield return block;

                    // Set to 0 for next iteration
                    block = 0;
                }
            }
        }

        // Generates a Fletcher 16,32,or 64 based on an input string
        // https://regularcoder.wordpress.com/2014/01/04/fletchers-checksum-in-c/

        /// <summary>
        /// summary text.
        /// </summary>
        public static UInt64 GetChecksum(String inputWord, int n)
        {
            // Fletcher 16: Read a single byte
            // Fletcher 32: Read a 16 bit block (two bytes)
            // Fletcher 64: Read a 32 bit block (four bytes)
            int bytesPerCycle = n / 16;

            // 2^x gives max value that can be stored in x bits
            // no of bits here is 8 * bytesPerCycle (8 bits to a byte)
            UInt64 modValue = (UInt64)(Math.Pow(2, 8 * bytesPerCycle) - 1);

            // ASCII encoding conveniently gives us 1 byte per character
            byte[] inputAsBytes = Encoding.ASCII.GetBytes(inputWord);

            UInt64 sum1 = 0;
            UInt64 sum2 = 0;
            foreach (UInt64 block in Blockify(inputAsBytes, bytesPerCycle))
            {
                sum1 = (sum1 + block) % modValue;
                sum2 = (sum2 + sum1) % modValue;
            }

            return sum1 + (sum2 * (modValue + 1));
        }

        public static void CreateZipFile(string fileName, IEnumerable<string> files)
        {
            // Create and open a new ZIP file
            var zip = ZipFile.Open(fileName, ZipArchiveMode.Create);
            foreach (var file in files)
            {
                // Add the entry for each file
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                File.Delete(file);
            }
            // Dispose of the object when we are done
            zip.Dispose();
        }
    }
}
