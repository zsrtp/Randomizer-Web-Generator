namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using System.IO;
    using TPRandomizer.FcSettings.Enums;

    public enum EntranceType : int
    {
        None = 0,
        Boss,
        Boss_Reverse,
        Dungeon,
        Dungeon_Reverse,
        Cave,
        Cave_Reverse,
        Door,
        Door_Reverse,
        Misc,
        Misc_Reverse,
        Mixed,
        All
    }

    public enum EntranceShuffleError
    {
        NONE = 0,
        BAD_LINKS_SPAWN,
        BAD_ENTRANCE_SHUFFLE_TABLE_ENTRY,
        RAN_OUT_OF_RETRIES,
        NO_MORE_VALID_ENTRANCES,
        ALL_LOCATIONS_NOT_REACHABLE,
        NOT_ENOUGH_SPHERE_ZERO_LOCATIONS,
        ATTEMPTED_SELF_CONNECTION,
        FAILED_TO_DISCONNECT_TARGET,
        DUNGEON_ENTRANCES_CONNECTED
    };

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
        public bool Primary { get; set; } = false;
        public EntranceType Type { get; set; } = EntranceType.None;
        public Entrance ReverseEntrance { get; set; } = null;
        public Entrance ReplacedEntrance { get; set; } = null;
        public Entrance AssumedEntrance { get; set; } = null;
        public bool Decoupled { get; set; }
        public int Stage { get; set; }
        public int Room { get; set; }
        public string Spawn { get; set; }
        public string SpawnType { get; set; }
        public string Parameters { get; set; }
        public bool Shuffled { get; set; }
        public string OriginalName { get; set; }
        public bool AlreadySetOriginalName { get; set; }

        public bool IsPrimary()
        {
            return Primary;
        }

        public void SetAsPrimary()
        {
            Primary = true;
        }

        public Entrance GetAssumedEntrance()
        {
            return AssumedEntrance;
        }

        public EntranceType GetEntranceType()
        {
            return Type;
        }

        public Entrance GetReplacedEntrance()
        {
            return ReplacedEntrance;
        }

        public string GetOriginalName()
        {
            return OriginalName;
        }

        public void SetOriginalName()
        {
            if (!AlreadySetOriginalName)
            {
                OriginalName = ParentArea + " -> " + ConnectedArea;
                AlreadySetOriginalName = true;
            }
        }

        public string GetCurrentName()
        {
            return ParentArea + " -> " + ConnectedArea;
        }

        public void SetReplacedEntrance(Entrance replacedEntrance)
        {
            ReplacedEntrance = replacedEntrance;
        }

        public string GetConnectedArea()
        {
            return ConnectedArea;
        }

        public string GetParentArea()
        {
            return ParentArea;
        }

        public void SetParameters(string newParams)
        {
            Parameters = newParams;
        }

        public void SetAsDecoupled()
        {
            Decoupled = true;
        }

        public bool IsDecoupled()
        {
            return Decoupled;
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
            newExit.ParentArea = "Root";
            newExit.Requirements = "(true)";
            newExit.Connect(ConnectedArea);
            newExit.SetReplacedEntrance(this);
            newExit.SetOriginalName();
            Randomizer.Rooms.RoomDict["Root"].Exits.Add(newExit);
            return newExit;
        }

        public Entrance AssumeReachable()
        {
            if (AssumedEntrance == null)
            {
                AssumedEntrance = GetNewTarget();
                Disconnect();
            }
            return AssumedEntrance;
        }

        public Entrance GetReverse()
        {
            return ReverseEntrance;
        }

        public void Connect(string newConnectedArea)
        {
            ConnectedArea = newConnectedArea;
            //Console.WriteLine("Connecting " + this.ParentArea + " to " + ConnectedArea);
            Randomizer.Rooms.RoomDict[ConnectedArea].Exits.Add(this);
            Randomizer.Rooms.RoomDict[this.ParentArea].Exits.Add(this);
        }

        public string Disconnect()
        {
            //Console.WriteLine("Disconnecting " + this.ParentArea + " from " + ConnectedArea);
            Randomizer.Rooms.RoomDict[ConnectedArea].Exits.Remove(this);
            string previouslyConnected = ConnectedArea;
            ConnectedArea = "";
            Randomizer.Rooms.RoomDict[this.ParentArea].Exits.Remove(this);
            return previouslyConnected;
        }

        public void BindTwoWay(Entrance otherEntrance)
        {
            ReverseEntrance = otherEntrance;
            otherEntrance.SetReverse(this);
        }

        public void SetReverse(Entrance reverseEntrance)
        {
            ReverseEntrance = reverseEntrance;
        }

        public void SetAsShuffled()
        {
            Shuffled = true;
        }

        public void SetAsUnshuffled()
        {
            Shuffled = false;
        }
    }

    public class EntrancePool
    {
        public List<Entrance> EntranceList { get; set; }

        public EntrancePool()
        {
            EntranceList = new List<Entrance>(); // Initialize EntranceList in the constructor
        }
    }

    public class EntrancePools
    {
        public EntranceType Type { get; set; }
        public EntrancePool EntrancePool { get; set; }
    }

    /// <summary>
    /// summary text.
    /// </summary>
    public class EntranceRando
    {
        public List<SpawnTableEntry> SpawnTable = new();
        public Dictionary<string, Entrance> EntranceTable = new();

        public void RandomizeEntrances(Random rnd)
        {
            EntranceShuffleError err = EntranceShuffleError.NONE;
            // Read in the spawn table data from the .jsonc file.
            DeserializeSpawnTable();

            // Once we have the spawn table created, we want to verify that the data in the table is valid and generate a list of entrances from it.
            GetEntranceList();

            // Now that we have a list of all entrances verified, we want to generate the Entrance pools for each entrance type
            Dictionary<EntranceType, EntrancePool> shufflableEntrancePools = CreateEntrancePools();
            Dictionary<EntranceType, EntrancePool> targetEntrancePools = CreateTargetEntrances(
                shufflableEntrancePools
            );

            // Shuffle the entrances
            foreach (
                KeyValuePair<EntranceType, EntrancePool> entrancePool in shufflableEntrancePools
            )
            {
                err = ShuffleEntrancePool(
                    entrancePool.Value,
                    targetEntrancePools[entrancePool.Key],
                    rnd
                );
                if (err != EntranceShuffleError.NONE)
                {
                    Console.WriteLine(
                        "Encountered an error when shuffling the following pool type: "
                            + entrancePool.Key
                    );
                    EntranceShuffleErrorCheck(err);
                }
            }

            // Validate the world one last time to ensure that everything went okay
            err = ValidateWorld();
            if (err == EntranceShuffleError.NONE)
            {
                Console.WriteLine("World validated");
            }
            EntranceShuffleErrorCheck(err);
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

        Dictionary<EntranceType, EntrancePool> CreateEntrancePools()
        {
            Dictionary<EntranceType, EntrancePool> newEntrancePools = new();

            // Keep track of the vanilla entrances as we could potentially rely on them later during the shuffling process.
            List<EntranceType> vanillaEntranceTypes = new();

            // Keep track of the types we need to decouple to make things cleaner
            List<EntranceType> typesToDecouple = new();

            // Placeholder until I get the settings actually created
            bool isDungeonEREnabled = true;
            if (isDungeonEREnabled)
            {
                // If we are shuffling dungeon entrances, loop through the entrance table and make note of all of the dungeon entrances and add them to the pool.
                newEntrancePools.Add(
                    EntranceType.Dungeon,
                    GetShufflableEntrances(EntranceType.Dungeon, true)
                );

                bool decoupleEntrances = true;
                if (decoupleEntrances)
                {
                    newEntrancePools.Add(
                        EntranceType.Dungeon_Reverse,
                        GetReverseEntrances(newEntrancePools, EntranceType.Dungeon)
                    );
                    typesToDecouple.Add(EntranceType.Dungeon);
                    typesToDecouple.Add(EntranceType.Dungeon_Reverse);
                }
            }
            else
            {
                vanillaEntranceTypes.Add(EntranceType.Dungeon);
            }

            // Set marked entrance types as decoupled
            foreach (EntranceType type in typesToDecouple)
            {
                foreach (Entrance entrance in newEntrancePools[type].EntranceList)
                {
                    entrance.SetAsDecoupled();
                }
            }

            // Assign vanilla entrances
            foreach (EntranceType entranceType in vanillaEntranceTypes)
            {
                EntrancePool vanillaEntrances = GetShufflableEntrances(entranceType, true);
                foreach (Entrance vanillaEntrance in vanillaEntrances.EntranceList)
                {
                    Entrance assumedForward = vanillaEntrance.AssumeReachable();
                    if ((vanillaEntrance.ReverseEntrance != null) && !vanillaEntrance.IsDecoupled())
                    {
                        Entrance assumedReturn = vanillaEntrance.GetReverse().AssumeReachable();
                        assumedForward.BindTwoWay(assumedReturn);
                    }

                    ChangeConnections(vanillaEntrance, assumedForward);
                    ConfirmReplacement(vanillaEntrance, assumedForward);
                }
            }

            SetShuffledEntrances(newEntrancePools);

            return newEntrancePools;
        }

        void GetEntranceList()
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
                forwardEntrance.SetOriginalName();
                forwardEntrance.OriginalConnectedArea = forwardEntrance.GetConnectedArea();
                forwardEntrance.Stage = tableEntry.SourceRoomSpawn.Stage;
                forwardEntrance.Room = tableEntry.SourceRoomSpawn.Room;
                forwardEntrance.Spawn = tableEntry.SourceRoomSpawn.Spawn;
                forwardEntrance.SpawnType = tableEntry.SourceRoomSpawn.SpawnType;
                forwardEntrance.Parameters = tableEntry.SourceRoomSpawn.Parameters;
                forwardEntrance.SetAsPrimary();
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
                    returnEntrance.SetOriginalName();
                    returnEntrance.OriginalConnectedArea = forwardEntrance.GetConnectedArea();
                    returnEntrance.Stage = tableEntry.TargetRoomSpawn.Stage;
                    returnEntrance.Room = tableEntry.TargetRoomSpawn.Room;
                    returnEntrance.Spawn = tableEntry.TargetRoomSpawn.Spawn;
                    returnEntrance.SpawnType = tableEntry.TargetRoomSpawn.SpawnType;
                    returnEntrance.Parameters = tableEntry.TargetRoomSpawn.Parameters;
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

        EntrancePool GetShufflableEntrances(EntranceType entranceType, bool onlyPrimary)
        {
            EntrancePool shufflableEntrances = new();

            Console.WriteLine("Verifying Entrance shufflable");

            foreach (KeyValuePair<string, Room> roomEntry in Randomizer.Rooms.RoomDict)
            {
                Room currentRoom = roomEntry.Value;
                // We want to loop through every room until we find a match for the entrance information provided.
                foreach (Entrance entrance in currentRoom.Exits)
                {
                    if (
                        (entrance.Type == entranceType)
                        && (!onlyPrimary || entrance.IsPrimary())
                        && (entrance.GetEntranceType() != EntranceType.None)
                    )
                    {
                        shufflableEntrances.EntranceList.Add(entrance);
                        // DEBUG
                        Console.WriteLine(
                            "Entrance: " + entrance.GetOriginalName() + " is able to be randomized"
                        );
                    }
                }
            }
            return shufflableEntrances;
        }

        EntrancePool GetReverseEntrances(
            Dictionary<EntranceType, EntrancePool> entrancePool,
            EntranceType type
        )
        {
            EntrancePool reversePool = new();

            foreach (Entrance poolEntrance in entrancePool[type].EntranceList)
            {
                if (poolEntrance.Type == type)
                {
                    Console.WriteLine(
                        "Adding " + poolEntrance.GetReverse().GetOriginalName() + " to reverse pool"
                    );
                    reversePool.EntranceList.Add(poolEntrance.GetReverse());
                }
            }

            return reversePool;
        }

        void ChangeConnections(Entrance entrance, Entrance targetEntrance)
        {
            /*Console.WriteLine(
                "Changing connections for "
                    + entrance.GetOriginalName()
                    + " and "
                    + targetEntrance.GetOriginalName()
            );*/
            entrance.Connect(targetEntrance.Disconnect());
            entrance.SetReplacedEntrance(targetEntrance.GetReplacedEntrance());
            if ((entrance.GetReplacedEntrance() != null) && !entrance.IsDecoupled())
            {
                targetEntrance
                    .GetReplacedEntrance()
                    .GetReverse()
                    .Connect(entrance.GetReverse().GetAssumedEntrance().Disconnect());
                targetEntrance
                    .GetReplacedEntrance()
                    .GetReverse()
                    .SetReplacedEntrance(entrance.GetReverse());
            }
        }

        void ConfirmReplacement(Entrance entrance, Entrance targetEntrance)
        {
            DeleteTargetEntrance(targetEntrance);
            if ((entrance.GetReverse() != null) && !entrance.IsDecoupled())
            {
                DeleteTargetEntrance(entrance.GetReverse().GetAssumedEntrance());
            }
        }

        void DeleteTargetEntrance(Entrance targetEntrance)
        {
            if (targetEntrance.GetConnectedArea() != "")
            {
                targetEntrance.Disconnect();
            }
            if (targetEntrance.GetParentArea() != "")
            {
                RemoveEntrance(targetEntrance);
            }
        }

        void RemoveEntrance(Entrance entranceToRemove)
        {
            Randomizer.Rooms.RoomDict[entranceToRemove.GetParentArea()].Exits.Remove(
                entranceToRemove
            );
        }

        void SetShuffledEntrances(Dictionary<EntranceType, EntrancePool> entrancePools)
        {
            foreach (KeyValuePair<EntranceType, EntrancePool> entrancePool in entrancePools)
            {
                EntrancePool currentPool = entrancePool.Value;
                foreach (Entrance entrance in currentPool.EntranceList)
                {
                    entrance.SetAsShuffled();
                    if (entrance.GetReverse() != null)
                    {
                        entrance.GetReverse().SetAsShuffled();
                    }
                }
            }
        }

        Dictionary<EntranceType, EntrancePool> CreateTargetEntrances(
            Dictionary<EntranceType, EntrancePool> entrancePools
        )
        {
            Dictionary<EntranceType, EntrancePool> targetEntrancePools = new();
            foreach (KeyValuePair<EntranceType, EntrancePool> entrancePool in entrancePools)
            {
                targetEntrancePools.Add(entrancePool.Key, AssumeEntrancePool(entrancePool.Value));

                //DEBUG
                Console.WriteLine("Targets for entrance type: " + entrancePool.Key);
                foreach (Entrance entrance in AssumeEntrancePool(entrancePool.Value).EntranceList)
                {
                    Console.WriteLine("Target: " + entrance.GetOriginalName());
                }

                // DEBUG END
            }
            return targetEntrancePools;
        }

        EntrancePool AssumeEntrancePool(EntrancePool entrancePool)
        {
            EntrancePool assumedPool = new();
            foreach (Entrance entrance in entrancePool.EntranceList)
            {
                Entrance assumedForward = entrance.AssumeReachable();
                if ((entrance.GetReverse() != null) && !entrance.IsDecoupled())
                {
                    Entrance assumedReturn = entrance.GetReverse().AssumeReachable();
                    assumedForward.BindTwoWay(assumedReturn);
                }
                assumedPool.EntranceList.Add(assumedForward);
            }
            return assumedPool;
        }

        EntranceShuffleError ShuffleEntrancePool(
            EntrancePool entrancePool,
            EntrancePool targetEntrances,
            Random rnd,
            int retryCount = 20
        )
        {
            EntranceShuffleError err = EntranceShuffleError.NONE;

            while (retryCount > 0)
            {
                retryCount--;

                // Keep a copy of placed entrance pairs as a reference in case we need to roll back.
                Dictionary<Entrance, Entrance> rollBacks = new();

                err = ShuffleEntrances(entrancePool, targetEntrances, rollBacks, rnd);
                if (err != EntranceShuffleError.NONE)
                {
                    Console.WriteLine(
                        "Failed to place all entrances in a pool for the world. Will retry "
                            + retryCount
                            + " more times"
                    );
                    Console.WriteLine("Last Error: " + err);
                    foreach (KeyValuePair<Entrance, Entrance> rollBack in rollBacks)
                    {
                        RestoreConnections(rollBack.Key, rollBack.Value);
                    }
                    continue;
                }
                foreach (KeyValuePair<Entrance, Entrance> rollBack in rollBacks)
                {
                    ConfirmReplacement(rollBack.Key, rollBack.Value);
                }
                return EntranceShuffleError.NONE;
            }
            Console.WriteLine("Entrance placement attempt count exceeded for world.");
            return EntranceShuffleError.RAN_OUT_OF_RETRIES;
        }

        EntranceShuffleError ShuffleEntrances(
            EntrancePool entrances,
            EntrancePool targetEntrances,
            Dictionary<Entrance, Entrance> rollBacks,
            Random rnd
        )
        {
            entrances.EntranceList.Shuffle(rnd);
            foreach (Entrance entrance in entrances.EntranceList)
            {
                Console.WriteLine("Attempting to shuffle: " + entrance.GetOriginalName());
                EntranceShuffleError err = EntranceShuffleError.NONE;
                if (entrance.GetConnectedArea() != "")
                {
                    continue;
                }
                targetEntrances.EntranceList.Shuffle(rnd);

                foreach (Entrance target in targetEntrances.EntranceList)
                {
                    // If the target has already been disconnected then don't use it again
                    if (target.GetConnectedArea() == "")
                    {
                        continue;
                    }
                    err = ReplaceEntrance(entrance, target, rollBacks, rnd);
                    if (err == EntranceShuffleError.NONE)
                    {
                        break;
                    }
                }

                if (entrance.GetConnectedArea() == "")
                {
                    Console.WriteLine(
                        "Could not connect " + entrance.GetOriginalName() + ". Error: " + err
                    );
                    return EntranceShuffleError.NO_MORE_VALID_ENTRANCES;
                }
            }

            //Verify that all targets were disconnected and that we didn't create any closed root loops
            foreach (Entrance target in targetEntrances.EntranceList)
            {
                if (target.GetConnectedArea() != "")
                {
                    Console.WriteLine(
                        "Error: Target entrance "
                            + target.GetCurrentName()
                            + " was never disconnected"
                    );
                    return EntranceShuffleError.FAILED_TO_DISCONNECT_TARGET;
                }
            }
            return EntranceShuffleError.NONE;
        }

        EntranceShuffleError ReplaceEntrance(
            Entrance entrance,
            Entrance target,
            Dictionary<Entrance, Entrance> rollBacks,
            Random rnd
        )
        {
            Console.WriteLine(
                "Attempting to connect: "
                    + entrance.GetOriginalName()
                    + " to "
                    + target.GetReplacedEntrance().GetOriginalName()
            );

            EntranceShuffleError err = EntranceShuffleError.NONE;
            err = CheckEntranceCompatibility(entrance, target, rollBacks);
            EntranceShuffleErrorCheck(err);
            ChangeConnections(entrance, target);
            err = ValidateWorld();

            // If the replacement produces an invalid world graph, then undo the connection and try again
            if (err != EntranceShuffleError.NONE)
            {
                if (entrance.GetConnectedArea() != "")
                {
                    RestoreConnections(entrance, target);
                }
                return err;
            }
            rollBacks.Add(entrance, target);
            return EntranceShuffleError.NONE;
        }

        EntranceShuffleError CheckEntranceCompatibility(
            Entrance entrance,
            Entrance target,
            Dictionary<Entrance, Entrance> rollBacks
        )
        {
            // Self-connections can cause issues and increase the failure rate of the placement algorith, so we want to prevent them.
            if (entrance.GetReverse() != null)
            {
                if (target.GetReplacedEntrance() == entrance.GetReverse())
                {
                    return EntranceShuffleError.ATTEMPTED_SELF_CONNECTION;
                }
            }
            return EntranceShuffleError.NONE;
        }

        EntranceShuffleError ValidateWorld()
        {
            if (!BackendFunctions.ValidatePlaythrough(Randomizer.Rooms.RoomDict["Root"]))
            {
                return EntranceShuffleError.ALL_LOCATIONS_NOT_REACHABLE;
            }
            return EntranceShuffleError.NONE;
        }

        void EntranceShuffleErrorCheck(EntranceShuffleError err)
        {
            switch (err)
            {
                case EntranceShuffleError.ALL_LOCATIONS_NOT_REACHABLE:
                {
                    Console.WriteLine("All locations are not reachable!");
                    break;
                }
                case EntranceShuffleError.ATTEMPTED_SELF_CONNECTION:
                {
                    Console.WriteLine("Attempted self connection!");
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        void RestoreConnections(Entrance entrance, Entrance targetEntrance)
        {
            Console.WriteLine("Restoring Connection for " + entrance.GetOriginalName());
            targetEntrance.Connect(entrance.Disconnect());
            entrance.SetReplacedEntrance(null);
            if ((entrance.GetReverse() != null) && !entrance.IsDecoupled())
            {
                entrance
                    .GetReverse()
                    .GetAssumedEntrance()
                    .Connect(targetEntrance.GetReplacedEntrance().GetReverse().Disconnect());
                targetEntrance.GetReplacedEntrance().GetReverse().SetReplacedEntrance(null);
            }
        }
    }
}
