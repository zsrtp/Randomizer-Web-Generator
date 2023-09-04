namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using System.IO;
    using TPRandomizer.FcSettings.Enums;

    public enum EntranceType
    {
        Dungeon = 0
    }

    public class SpawnTableEntry
    {
        public string Type { get; set; }
        public EntranceInfo SourceRoomSpawn { get; set; }
        public EntranceInfo TargetRoomSpawn { get; set; }
    }

    public class EntranceInfo
    {
        public string SourceRoom { get; set; }
        public string TargetRoom { get; set; }
        public int Stage { get; set; }
        public int Room { get; set; }
        public string Spawn { get; set; }
        public string Type { get; set; }
        public string Parameters { get; set; }
    }

    public class Entrance
    {
        public string ParentArea { get; set; }
        public string ConnectedArea { get; set; }
        public string OriginalConnectedArea { get; set; }
        public bool IsPrimary { get; set; }
        public EntranceType Type { get; set; }
        public string ReverseEntrance { get; set; } // either this needs to be a pointer to a global table, or we still reference a global table and just do EntranceTable[EntranceName] when referencing it.

        public string getReverseEntrance()
        {
            return ReverseEntrance;
        }
    }

    public class EntrancePool
    {
        public EntranceType Type { get; set; }
        public List<string> EntranceList { get; set; }

        // constructor
        public EntrancePool(EntranceType type, List<string> entranceList)
        {
            Type = type;
            EntranceList = entranceList;
        }
    }

    /// <summary>
    /// summary text.
    /// </summary>
    public class EntranceRando
    {
        public List<SpawnTableEntry> SpawnTable = new();
        public Dictionary<string, Entrance> EntranceTable = new();

        public Dictionary<string, Room> RandomizeEntrances(
            Dictionary<string, Room> currentGraph,
            Random rnd
        )
        {
            // Read in the spawn table data from the .jsonc file.
            DeserializeSpawnTable();

            // Once we have the spawn table created, we want to verify that the data in the table is valid and generate a list of entrances from it.
            GetEntranceList(currentGraph);

            // Now that we have a list of all entrances verified, we want to generate the Entrance pools for each entrance type
            List<EntrancePool> shufflableEntrancePools = CreateEntrancePools(currentGraph);

            return currentGraph;
        }

        private static void DeserializeSpawnTable()
        {
            Console.WriteLine("Loading Entrance Table from file.");

            List<SpawnTableEntry> SpawnTable = JsonConvert.DeserializeObject<List<SpawnTableEntry>>(
                File.ReadAllText(Global.CombineRootPath("./Assets/Entrances/EntranceTable.jsonc"))
            );
            foreach (SpawnTableEntry Spawn in SpawnTable)
            {
                Randomizer.EntranceRandomizer.SpawnTable.Add(Spawn);
            }
        }

        List<EntrancePool> CreateEntrancePools(Dictionary<string, Room> worldGraph)
        {
            List<EntrancePool> newEntrancePools = new();

            // Keep track of the vanilla entrances as we could potentially rely on them later during the shuffling process.
            List<EntranceType> vanillaEntranceTypes = new();

            // Placeholder until I get the settings actually created
            bool isDungeonEREnabled = true;
            if (isDungeonEREnabled)
            {
                // If we are shuffling dungeon entrances, loop through the entrance table and make note of all of the dungeon entrances and add them to the pool.
                List<string> dungeonEntranceList = new();
                foreach (
                    KeyValuePair<
                        string,
                        Entrance
                    > entranceList in Randomizer.EntranceRandomizer.EntranceTable.ToList()
                )
                {
                    Entrance entrance = entranceList.Value;
                    if ((entrance.Type == EntranceType.Dungeon) && entrance.IsPrimary)
                    {
                        dungeonEntranceList.Add(entranceList.Key);

                        // DEBUG
                        Console.WriteLine(
                            "Entrance: " + entranceList.Key + " is able to be randomized"
                        );
                    }
                }

                bool decoupleEntrances = true;
                if (decoupleEntrances)
                {
                    dungeonEntranceList.AddRange(GetReverseEntrances(dungeonEntranceList));
                }

                EntrancePool dungeonEntrancePool = new EntrancePool(
                    EntranceType.Dungeon,
                    dungeonEntranceList
                );
                newEntrancePools.Add(dungeonEntrancePool);
            }

            return newEntrancePools;
        }

        /*
        List<Entrance> GetShufflableEntrances(Dictionary<string>, Room WorldGraph)
        {
            List<Entrance> shufflableEntrances = new();

        }
        */

        void GetEntranceList(Dictionary<string, Room> WorldGraph)
        {
            foreach (SpawnTableEntry tableEntry in Randomizer.EntranceRandomizer.SpawnTable)
            {
                // First we need to get the forward entry info and validate that it is valid.
                Entrance forwardEntrance = GetEntranceFromTableEntry(
                    tableEntry.SourceRoomSpawn.SourceRoom,
                    tableEntry.SourceRoomSpawn.TargetRoom
                );
                if (forwardEntrance == null)
                {
                    throw new Exception(
                        "Invalid entrance entry for: "
                            + tableEntry.SourceRoomSpawn.SourceRoom
                            + " -> "
                            + tableEntry.SourceRoomSpawn.TargetRoom
                    );
                }

                forwardEntrance.ParentArea = tableEntry.SourceRoomSpawn.SourceRoom;
                forwardEntrance.OriginalConnectedArea = tableEntry.SourceRoomSpawn.TargetRoom;
                forwardEntrance.ConnectedArea = tableEntry.SourceRoomSpawn.TargetRoom;
                forwardEntrance.Type = EntranceType.Dungeon;
                forwardEntrance.IsPrimary = true;

                //Check to see if the entrance is a two-way, if it is, create an entrace entry for the other side.
                if (tableEntry.TargetRoomSpawn != null)
                {
                    Entrance returnEntrance = GetEntranceFromTableEntry(
                        tableEntry.TargetRoomSpawn.SourceRoom,
                        tableEntry.TargetRoomSpawn.TargetRoom
                    );
                    if (returnEntrance == null)
                    {
                        throw new Exception(
                            "Invalid entrance entry for: "
                                + tableEntry.TargetRoomSpawn.SourceRoom
                                + " -> "
                                + tableEntry.TargetRoomSpawn.TargetRoom
                        );
                    }
                    returnEntrance.ParentArea = tableEntry.TargetRoomSpawn.SourceRoom;
                    returnEntrance.OriginalConnectedArea = tableEntry.TargetRoomSpawn.TargetRoom;
                    returnEntrance.ConnectedArea = tableEntry.TargetRoomSpawn.TargetRoom;
                    returnEntrance.Type = EntranceType.Dungeon;
                    Randomizer.EntranceRandomizer.EntranceTable.Add(
                        returnEntrance.ParentArea + " -> " + returnEntrance.OriginalConnectedArea,
                        returnEntrance
                    );
                    forwardEntrance.ReverseEntrance =
                        returnEntrance.ParentArea + " -> " + returnEntrance.OriginalConnectedArea;
                }

                Randomizer.EntranceRandomizer.EntranceTable.Add(
                    forwardEntrance.ParentArea + " -> " + forwardEntrance.OriginalConnectedArea,
                    forwardEntrance
                );
            }

            // Debug Statement
            Console.WriteLine("== DEBUG. Base Entrance Table: ==");
            foreach (
                KeyValuePair<
                    string,
                    Entrance
                > entranceList in Randomizer.EntranceRandomizer.EntranceTable.ToList()
            )
            {
                Entrance currentEntrance = entranceList.Value;
                Console.WriteLine(
                    "== "
                        + currentEntrance.ParentArea
                        + " -> "
                        + currentEntrance.ConnectedArea
                        + " =="
                );
            }
        }

        /// <summary>
        /// summary text
        /// </summary>
        Entrance GetEntranceFromTableEntry(string SourceRoom, string TargetRoom)
        {
            Entrance retrievedEntrance = null;
            Dictionary<string, Room> WorldGraph = Randomizer.Rooms.RoomDict;
            foreach (KeyValuePair<string, Room> roomEntry in WorldGraph)
            {
                Room currentRoom = roomEntry.Value;
                // We want to loop through every room until we find a match for the entrance information provided.
                if (currentRoom.RoomName == SourceRoom)
                {
                    foreach (RoomExit exit in currentRoom.Exits)
                    {
                        if (exit.ConnectedArea == TargetRoom)
                        {
                            retrievedEntrance = new();
                            break;
                        }
                    }
                }
            }
            return retrievedEntrance;
        }

        List<string> GetReverseEntrances(List<string> entranceList)
        {
            List<string> reverseEntrances = new();

            foreach (string entrance in entranceList)
            {
                reverseEntrances.Add(
                    Randomizer.EntranceRandomizer.EntranceTable[entrance].ReverseEntrance
                );
            }

            return reverseEntrances;
        }
    }
}
