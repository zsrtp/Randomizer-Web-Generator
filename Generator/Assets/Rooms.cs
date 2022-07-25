namespace TPRandomizer
{
    using System.Collections.Generic;
    using TPRandomizer.SSettings.Enums;

    /// <summary>
    /// summary text.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Gets or sets the name of the room. This is the name we give the room to identify it (it can be a series of rooms that don't have requirements between each other to make the algorithm go faster).
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// Gets or sets the room name of the rooms adjacent to the current room.
        /// </summary>
        public List<string> Neighbours { get; set; }

        /// <summary>
        /// Gets or sets a list of list of requirements to enter each neighbouring roo.
        /// </summary>
        public List<string> NeighbourRequirements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current room is the starting room. If true, this room will always be the starting point of the graph.
        /// </summary>
        public bool IsStartingRoom { get; set; }

        /// <summary>
        /// Gets or sets a list of checks contained inside the room.
        /// </summary>
        public List<string> Checks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current room has been visited in the current playthrough.
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current room has been visited at least once in the current world generation.
        /// </summary>
        public bool ReachedByPlaythrough { get; set; }

        /// <summary>
        /// Gets or sets the logical region that the room is contained in.
        /// </summary>
        public string Region { get; set; }
    }

    /// <summary>
    /// summary text.
    /// </summary>
    public class RoomFunctions
    {
        /// <summary>
        /// A dictionary of all of the rooms that will be used to generate a playthrough graph.
        /// </summary>
        public Dictionary<string, Room> RoomDict = new();

        /// <summary>
        /// summary text.
        /// </summary>
        /// <param name="itemToPlace">The item being checked.</param>
        /// <param name="currentCheck">The check being verified.</param>
        /// <param name="currentRoom">The room where the check is located.</param>
        /// <returns>A value that determines if the specified item and check meet the regional requirements set by the generation.</returns>
        public static bool IsRegionCheck(Item itemToPlace, Check currentCheck, Room currentRoom)
        {
            SharedSettings parseSetting = Randomizer.SSettings;
            string itemName = itemToPlace.ToString();
            itemName = itemName.Replace("_", " ");
            if (Randomizer.Items.RegionSmallKeys.Contains(itemToPlace))
            {
                if (
                    (parseSetting.smallKeySettings == SmallKeySettings.Own_Dungeon)
                    && itemName.Contains(currentRoom.Region)
                )
                {
                    return true;
                }
                else if (
                    (parseSetting.smallKeySettings == SmallKeySettings.Any_Dungeon)
                    && currentCheck.category.Contains("Dungeon")
                )
                {
                    return true;
                }
            }
            else if (Randomizer.Items.DungeonBigKeys.Contains(itemToPlace))
            {
                if (parseSetting.bigKeySettings == BigKeySettings.Own_Dungeon)
                {
                    if (itemName.Contains(currentRoom.Region))
                    {
                        return true;
                    }
                }
                else if (parseSetting.bigKeySettings == BigKeySettings.Any_Dungeon)
                {
                    if (currentCheck.category.Contains("Dungeon"))
                    {
                        return true;
                    }
                }
            }
            else if (Randomizer.Items.DungeonMapsAndCompasses.Contains(itemToPlace))
            {
                if (parseSetting.mapAndCompassSettings == MapAndCompassSettings.Own_Dungeon)
                {
                    if (itemName.Contains(currentRoom.Region))
                    {
                        return true;
                    }
                }
                else if (parseSetting.mapAndCompassSettings == MapAndCompassSettings.Any_Dungeon)
                {
                    if (currentCheck.category.Contains("Dungeon"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
