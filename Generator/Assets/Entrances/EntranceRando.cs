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
        public string SpawnType { get; set; }
        public string Parameters { get; set; }
    }

    public class Entrance
    {
        public string Requirements { get; set; }
        public string ParentArea { get; set; }
        public string ConnectedArea { get; set; }
        public string OriginalConnectedArea { get; set; }
        public bool IsPrimary { get; set; }
        public EntranceType Type { get; set; }
        public Entrance ReverseEntrance { get; set; } = null;
        public Entrance ReplacedEntrance { get; set; } = null;
        public Entrance AssumedEntrance { get; set; } = null;
        public bool Decoupled { get; set; }
        public int Stage { get; set; }
        public int Room { get; set; }
        public string Spawn { get; set; }
        public string SpawnType { get; set; }
        public string Parameters { get; set; }

        public Entrance getReverseEntrance()
        {
            return ReverseEntrance;
        }

        public void SetEntranceType(string entranceType)
        {
            if (entranceType == "Dungeon")
            {
                Type = EntranceType.Dungeon;
            }
        }

        Entrance GetNewTarget()
        {
            Entrance newExit = new();
            newExit.ConnectedArea = ConnectedArea;
            newExit.Requirements = "true";
            Randomizer.Rooms.RoomDict["Root"].Exits.Add(newExit);
            return newExit;
        }

        public void AssumeReachable()
        {
            if (AssumedEntrance == null)
            {
                AssumedEntrance = GetNewTarget();
            }
        }

        public void Connect(string newConnectedArea)
        {
            ConnectedArea = newConnectedArea;
        }

        public string Disconnect()
        {
            string previouslyConnected = ConnectedArea;
            ConnectedArea = "";
            return previouslyConnected;
        }

        public void BindTwoWay(Entrance otherEntrance)
        {
            ReverseEntrance = otherEntrance;
            otherEntrance.SetReverse(this);
        }

        Entrance GetReverse()
        {
            return ReverseEntrance;
        }

        void SetReverse(Entrance reverseEntrance)
        {
            ReverseEntrance = reverseEntrance;
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
            //List<EntrancePool> shufflableEntrancePools = CreateEntrancePools(currentGraph);

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

        /*
        List<EntrancePool> CreateEntrancePools(Dictionary<string, Room> worldGraph)
        {
            List<EntrancePool> newEntrancePools = new();

            // Keep track of the vanilla entrances as we could potentially rely on them later during the shuffling process.
            List<EntranceType> vanillaEntranceTypes = new();

            // Keep track of the types we need to decouple to make things cleaner
            List<EntranceType> typesToDecouple = new();

            // Placeholder until I get the settings actually created
            bool isDungeonEREnabled = true;
            if (isDungeonEREnabled)
            {
                // If we are shuffling dungeon entrances, loop through the entrance table and make note of all of the dungeon entrances and add them to the pool.
                List<string> dungeonEntranceList = new();
                dungeonEntranceList.AddRange(GetShufflableEntrances(EntranceType.Dungeon));

                bool decoupleEntrances = true;
                if (decoupleEntrances)
                {
                    dungeonEntranceList.AddRange(GetReverseEntrances(dungeonEntranceList));
                    typesToDecouple.Add(EntranceType.Dungeon);
                }

                EntrancePool dungeonEntrancePool = new EntrancePool(
                    EntranceType.Dungeon,
                    dungeonEntranceList
                );
                newEntrancePools.Add(dungeonEntrancePool);
            }
            else
            {
                vanillaEntranceTypes.Add(EntranceType.Dungeon)
            }

            // Set marked entrance types as decoupled
            foreach (EntranceType type in typesToDecouple)
            {
                foreach (
                    KeyValuePair<
                        string,
                        Entrance
                    > entranceList in Randomizer.EntranceRandomizer.EntranceTable.ToList()
                )
                {
                    Entrance entrance = entranceList.Value;
                    if (entrance.Type == type)
                    {
                        entrance.Decoupled = true;
                    }
                }
            }

            // Assign vanilla entrances
            foreach (EntranceType entranceType in vanillaEntranceTypes)
            {
                List<string> vanillaEntrances = GetShufflableEntrances(entranceType);
                foreach (string vanillaEntrance in vanillaEntrances)
                {
                    //TODO
                    // auto assumedForward = entrance->assumeReachable();
                    Entrance entrance = Randomizer.EntranceRandomizer.EntranceTable[vanillaEntrance];
                    if ((entrance.ReverseEntrance != "") && !entrance.Decoupled)
                    {
                        //auto assumedReturn = entrance->getReverse()->assumeReachable();
                        //
                    }
                }
            }

            return newEntrancePools;
        } */

        /*
        List<Entrance> GetShufflableEntrances(Dictionary<string>, Room WorldGraph)
        {
            List<Entrance> shufflableEntrances = new();

        }
        */

        void GetEntranceList(Dictionary<string, Room> WorldGraph)
        {
            // DEBUG
            List<string> entranceList = new();
            foreach (SpawnTableEntry tableEntry in Randomizer.EntranceRandomizer.SpawnTable)
            {
                // First we need to get the forward entry info and validate that it is valid.
                Entrance forwardEntrance = GetEntranceFromTableEntry(tableEntry.SourceRoomSpawn);

                if (forwardEntrance == null)
                {
                    throw new Exception(
                        "Invalid entrance entry for: "
                            + tableEntry.SourceRoomSpawn.SourceRoom
                            + " -> "
                            + tableEntry.SourceRoomSpawn.TargetRoom
                    );
                }

                forwardEntrance.SetEntranceType(tableEntry.Type);
                forwardEntrance.Stage = tableEntry.SourceRoomSpawn.Stage;
                forwardEntrance.Room = tableEntry.SourceRoomSpawn.Room;
                forwardEntrance.Spawn = tableEntry.SourceRoomSpawn.Spawn;
                forwardEntrance.SpawnType = tableEntry.SourceRoomSpawn.SpawnType;
                forwardEntrance.Parameters = tableEntry.SourceRoomSpawn.Parameters;
                forwardEntrance.IsPrimary = true;
                entranceList.Add(
                    tableEntry.SourceRoomSpawn.SourceRoom
                        + " -> "
                        + tableEntry.SourceRoomSpawn.TargetRoom
                );

                //Check to see if the entrance is a two-way, if it is, create an entrace entry for the other side.
                if (tableEntry.TargetRoomSpawn != null)
                {
                    Entrance returnEntrance = GetEntranceFromTableEntry(tableEntry.TargetRoomSpawn);
                    if (returnEntrance == null)
                    {
                        throw new Exception(
                            "Invalid entrance entry for: "
                                + tableEntry.TargetRoomSpawn.SourceRoom
                                + " -> "
                                + tableEntry.TargetRoomSpawn.TargetRoom
                        );
                    }
                    returnEntrance.SetEntranceType(tableEntry.Type);
                    returnEntrance.Stage = tableEntry.TargetRoomSpawn.Stage;
                    returnEntrance.Room = tableEntry.TargetRoomSpawn.Room;
                    returnEntrance.Spawn = tableEntry.TargetRoomSpawn.Spawn;
                    returnEntrance.SpawnType = tableEntry.TargetRoomSpawn.SpawnType;
                    returnEntrance.Parameters = tableEntry.TargetRoomSpawn.Parameters;
                    returnEntrance.IsPrimary = false;
                    forwardEntrance.BindTwoWay(returnEntrance);
                    entranceList.Add(
                        tableEntry.TargetRoomSpawn.SourceRoom
                            + " -> "
                            + tableEntry.TargetRoomSpawn.TargetRoom
                    );
                }
            }

            // Debug Statement
            Console.WriteLine("== DEBUG. Base Entrance Table: ==");
            foreach (string entrance in entranceList)
            {
                Console.WriteLine("== " + entrance + " ==");
            }
        }

        /// <summary>
        /// summary text
        /// </summary>
        Entrance GetEntranceFromTableEntry(EntranceInfo entranceInfo)
        {
            Dictionary<string, Room> WorldGraph = Randomizer.Rooms.RoomDict;
            foreach (KeyValuePair<string, Room> roomEntry in WorldGraph)
            {
                Room currentRoom = roomEntry.Value;
                // We want to loop through every room until we find a match for the entrance information provided.
                if (currentRoom.RoomName == entranceInfo.SourceRoom)
                {
                    foreach (Entrance entrance in currentRoom.Exits)
                    {
                        if (entrance.OriginalConnectedArea == entranceInfo.TargetRoom)
                        {
                            return entrance;
                        }
                    }
                }
            }
            return null;
        }

        /*
        List<string> GetShufflableEntrances(EntranceType entranceType)
        {
            List<string> shufflableEntrances = new();
            foreach (
                    KeyValuePair<
                        string,
                        Entrance
                    > entranceList in Randomizer.EntranceRandomizer.EntranceTable.ToList()
                )
                {
                    Entrance entrance = entranceList.Value;
                    if ((entrance.Type == entranceType) && entrance.IsPrimary)
                    {
                        shufflableEntrances.Add(entranceList.Key);

                        // DEBUG
                        Console.WriteLine(
                            "Entrance: " + entranceList.Key + " is able to be randomized"
                        );
                    }
                }
            return shufflableEntrances;
        }

        List<string> GetReverseEntrances(List<string> entranceList)
        {
            List<string> reverseEntrances = new();

            foreach (string entrance in entranceList)
            {
                reverseEntrances.Add(
                    Randomizer.EntranceRandomizer.EntranceTable[entrance].ReverseEntrance
                );
                // DEBUG
                Console.WriteLine(
                    "Entrance: "
                        + Randomizer.EntranceRandomizer.EntranceTable[entrance].ReverseEntrance
                        + " is a reverse entrance!"
                );
            }

            return reverseEntrances;
        } */

        /*
        void ChangeConnections()
        {
            Entrance areaToConnect = new();
            //Get the entrance we need
            foreach (
                    KeyValuePair<
                        string,
                        Entrance
                    > entranceList in Randomizer.EntranceRandomizer.EntranceTable.ToList()
                )
                {
                    if (entranceList.Key == newConnectedArea)
                    {
                        areaToConnect = entranceList.Value;
                        break;
                    }
                }

            foreach (KeyValuePair<string, Room> roomEntry in WorldGraph)
            {
                Room currentRoom = roomEntry.Value;
                if (currentRoom.RoomName == ParentArea)
                {
                    foreach(RoomExit exit in currentRoom.Exits)
                    {
                        if (exit.ConnectedArea == areaToConnect.OriginalConnectedArea)
                        {
                            ReplacedEntrance = ConnectedArea;
                            exit.ConnectedArea = ConnectedArea;
                        }
                    }
                }
            }
        }*/
    }
}
