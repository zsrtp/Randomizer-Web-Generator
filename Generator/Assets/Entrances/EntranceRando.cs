namespace TPRandomizer.EntranceRando
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using System.IO;
    using TPRandomizer.FcSettings.Enums;

    /// <summary>
    /// summary text.
    /// </summary>
    public class EntranceRando
    {
        public Dictionary<string, Room> RandomizeEntrances(
            Dictionary<string, Room> currentGraph,
            Random rnd
        )
        {
            bool erEnabled = false;
            if (erEnabled)
            {
                // This code may have to run multiple times before it generates a successful graph, so we only want it to stop if the graph generation is successful.
                bool areEntrancesShuffled = false;
                while (!areEntrancesShuffled)
                {
                    // Next we want to loop through the room list. For every entrance, we want to check if there is an entry in the spawn table for it, and if so, can it be randomized.
                    foreach (KeyValuePair<string, Room> roomList in currentGraph.ToList())
                    {
                        Room currentRoom = roomList.Value;
                        for (int i = 0; i < currentRoom.Exits.Count; i++)
                        {
                            string entranceName =
                                currentRoom.RoomName + " -> " + currentRoom.Exits[i].RoomName;
                            for (int j = 0; j < Randomizer.Rooms.RandoEntranceTable.Count; j++)
                            {
                                if (
                                    entranceName == Randomizer.Rooms.RandoEntranceTable[j].SpawnName
                                )
                                {
                                    if (Randomizer.Rooms.RandoEntranceTable[j].Type == "taco") //placeholder for checking if the current spawn matches the requirements for ER
                                    {
                                        List<RoomSpawn> listOfSpawnsNeedingReplaced = new();
                                        listOfSpawnsNeedingReplaced.Add(
                                            Randomizer.Rooms.RandoEntranceTable[j]
                                        );
                                        int currSpawnIdx = j;
                                        while (listOfSpawnsNeedingReplaced.Count > 0)
                                        {
                                            // Once we get to this point, we assume that the current spawn is valid and able to be randomized.
                                            // Get a random entrance to replace this one with. Like the first one, we want to verify that it has not been shuffled and that it meets the criterea to be shuffled. If no replacements can be found, keep the spawn vanilla.

                                            List<int> replacementSpawnIndexes = new();
                                            RoomSpawn replacementSpawn =
                                                listOfSpawnsNeedingReplaced[0];
                                            for (
                                                int k = 0;
                                                k < Randomizer.Rooms.RandoEntranceTable.Count;
                                                k++
                                            )
                                            {
                                                if (
                                                    !Randomizer.Rooms.RandoEntranceTable[
                                                        k
                                                    ].HasBeenShuffled
                                                    && (
                                                        Randomizer.Rooms.RandoEntranceTable[k].Type
                                                        == "taco"
                                                    )
                                                )
                                                {
                                                    replacementSpawnIndexes.Add(k);
                                                }
                                            }

                                            if (replacementSpawnIndexes.Count > 0)
                                            {
                                                // if we have a match, we want to do the following:
                                                // 1. replace the exit name in the room so the graph knows where we can go later on
                                                // 2. Modify the current spawn info to show that it has updated (name and all data) because later, when we check for which entrances to put in the seed, we only want to replace entrances that have actually changed. This should make for less seed data and less work on the gci during run-time.

                                                int replacementIndex = rnd.Next(
                                                    replacementSpawnIndexes.Count
                                                );
                                                replacementSpawn = Randomizer
                                                    .Rooms
                                                    .RandoEntranceTable[replacementIndex];

                                                string srcRoomName =
                                                    replacementSpawn.SpawnName.Substring(
                                                        0,
                                                        replacementSpawn.SpawnName.IndexOf("-")
                                                    );
                                                string destRoomName =
                                                    replacementSpawn.SpawnName.Substring(
                                                        replacementSpawn.SpawnName.IndexOf(">") + 2
                                                    );
                                                currentRoom.Exits[i].RoomName = destRoomName;
                                                Randomizer.Rooms.RandoEntranceTable[
                                                    currSpawnIdx
                                                ].SpawnName =
                                                    currentRoom.RoomName + " -> " + destRoomName;
                                                Randomizer.Rooms.RandoEntranceTable[
                                                    currSpawnIdx
                                                ].SpawnInfo = replacementSpawn.SpawnInfo;

                                                // Once a replacement has been found, we want to set up the next loop so that we can find a replacement for this entrance because we don't want other entrances trying to reference it later.
                                                listOfSpawnsNeedingReplaced.Add(
                                                    Randomizer.Rooms.RandoEntranceTable[
                                                        replacementIndex
                                                    ]
                                                );
                                                Randomizer.Rooms.RandoEntranceTable[
                                                    currSpawnIdx
                                                ].HasBeenShuffled = true;
                                                Randomizer.Rooms.RandoEntranceTable[
                                                    replacementIndex
                                                ].HasBeenReferenced = true;

                                                // TODO HERE: Add code outline for handling coupled entrance if it exists
                                                if (erEnabled) //placeholder for checking for coupled entrance setting.
                                                {
                                                    // We first need to find the coupled entrances
                                                    int destCoupledSpawnIdx = 0;
                                                    int sourceCoupledSpawnIdx = 0;
                                                    for (
                                                        int k = 0;
                                                        k
                                                            < Randomizer
                                                                .Rooms
                                                                .VanillaEntranceTable
                                                                .Count;
                                                        k++
                                                    )
                                                    {
                                                        if (
                                                            (
                                                                Randomizer
                                                                    .Rooms
                                                                    .VanillaEntranceTable[
                                                                    k
                                                                ].SpawnName
                                                                == replacementSpawn.CoupledName
                                                            )
                                                            && !Randomizer
                                                                .Rooms
                                                                .VanillaEntranceTable[
                                                                k
                                                            ].HasBeenShuffled
                                                        )
                                                        {
                                                            destCoupledSpawnIdx = k;
                                                        }
                                                        else if (
                                                            (
                                                                Randomizer
                                                                    .Rooms
                                                                    .VanillaEntranceTable[
                                                                    k
                                                                ].SpawnName
                                                                == Randomizer
                                                                    .Rooms
                                                                    .RandoEntranceTable[
                                                                    currSpawnIdx
                                                                ].CoupledName
                                                            )
                                                            && !Randomizer
                                                                .Rooms
                                                                .VanillaEntranceTable[
                                                                k
                                                            ].HasBeenShuffled
                                                        )
                                                        {
                                                            sourceCoupledSpawnIdx = k;
                                                        }
                                                    }
                                                    // Now that we have the indexes for the coupled entrances, we want to set the destination's coupled spawn info to that of the source's coupled spawn info.

                                                    foreach (
                                                        KeyValuePair<
                                                            string,
                                                            Room
                                                        > secondRoomList in currentGraph.ToList()
                                                    )
                                                    {
                                                        Room destRoom = secondRoomList.Value;
                                                        if (destRoom.RoomName == destRoomName)
                                                        {
                                                            for (
                                                                int k = 0;
                                                                k < destRoom.Exits.Count;
                                                                k++
                                                            )
                                                            {
                                                                if (
                                                                    destRoom.Exits[i].RoomName
                                                                    == srcRoomName
                                                                )
                                                                {
                                                                    destRoom.Exits[i].RoomName =
                                                                        currentRoom.RoomName;
                                                                    Randomizer
                                                                        .Rooms
                                                                        .RandoEntranceTable[
                                                                        destCoupledSpawnIdx
                                                                    ].SpawnName =
                                                                        destRoom.RoomName
                                                                        + " -> "
                                                                        + currentRoom.RoomName;
                                                                    Randomizer
                                                                        .Rooms
                                                                        .RandoEntranceTable[
                                                                        destCoupledSpawnIdx
                                                                    ].SpawnInfo = Randomizer
                                                                        .Rooms
                                                                        .RandoEntranceTable[
                                                                        sourceCoupledSpawnIdx
                                                                    ].SpawnInfo;

                                                                    listOfSpawnsNeedingReplaced.Add(
                                                                        Randomizer
                                                                            .Rooms
                                                                            .RandoEntranceTable[
                                                                            sourceCoupledSpawnIdx
                                                                        ]
                                                                    );
                                                                    Randomizer
                                                                        .Rooms
                                                                        .RandoEntranceTable[
                                                                        destCoupledSpawnIdx
                                                                    ].HasBeenShuffled = true;
                                                                    Randomizer
                                                                        .Rooms
                                                                        .RandoEntranceTable[
                                                                        sourceCoupledSpawnIdx
                                                                    ].HasBeenReferenced = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            listOfSpawnsNeedingReplaced.Remove(
                                                listOfSpawnsNeedingReplaced[0]
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return currentGraph;
        }
    }
}
