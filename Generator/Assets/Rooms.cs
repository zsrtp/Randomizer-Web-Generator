namespace TPRandomizer
{
    using System.Collections.Generic;
    using TPRandomizer.SSettings.Enums;
    using System;

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
        public List<Entrance> Exits { get; set; }

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

    enum StageIDs : int
    {
        Lakebed_Temple = 0x0,
        Morpheel = 0x1,
        Deku_Toad,
        Goron_Mines,
        Fyrus,
        Dangoro,
        Forest_Temple,
        Diababa,
        Ook,
        Temple_of_Time,
        Armogohma,
        Darknut,
        City_in_the_Sky,
        Argorok,
        Aeralfos,
        Palace_of_Twilight,
        Zant_Main_Room,
        Phantom_Zant_1,
        Phantom_Zant_2,
        Zant_Fight,
        Hyrule_Castle,
        Ganondorf_Castle,
        Ganondorf_Field,
        Ganondorf_Defeated,
        Arbiters_Grounds,
        Stallord,
        Death_Sword,
        Snowpeak_Ruins,
        Blizzeta,
        Darkhammer,
        Lanayru_Ice_Puzzle_Cave,
        Cave_of_Ordeals,
        Eldin_Long_Cave,
        Lake_Hylia_Long_Cave,
        Eldin_Goron_Stockcave,
        Grotto_1,
        Grotto_2,
        Grotto_3,
        Grotto_4,
        Grotto_5,
        Faron_Woods_Cave,
        Ordon_Ranch,
        Title_Screen,
        Ordon_Village,
        Ordon_Spring,
        Faron_Woods,
        Kakariko_Village,
        Death_Mountain,
        Kakariko_Graveyard,
        Zoras_River,
        Zoras_Domain,
        Snowpeak,
        Lake_Hylia,
        Castle_Town,
        Sacred_Grove,
        Bulblin_Camp,
        Hyrule_Field,
        Outside_Castle_Town,
        Bulblin_2,
        Gerudo_Desert,
        Mirror_Chamber,
        Upper_Zoras_River,
        Fishing_Pond,
        Hidden_Village,
        Hidden_Skill,
        Ordon_Village_Interiors,
        Hyrule_Castle_Sewers,
        Faron_Woods_Interiors,
        Kakariko_Village_Interiors,
        Death_Mountain_Interiors,
        Castle_Town_Interiors,
        Fishing_Pond_Interiors,
        Hidden_Village_Interiors,
        Castle_Town_Shops,
        Star_Game,
        Kakariko_Graveyard_Interiors,
        Light_Arrows_Cutscene,
        Hyrule_Castle_Cutscenes
    };

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
                    return checkBarrenRegionLocation(currentRoom, currentCheck, itemName);
                }
                else if (
                    (parseSetting.smallKeySettings == SmallKeySettings.Any_Dungeon)
                    && (
                        currentCheck.category.Contains("Dungeon")
                        || itemName.Contains(currentRoom.Region)
                    )
                )
                {
                    return checkBarrenRegionLocation(currentRoom, currentCheck, itemName);
                }
            }
            else if (Randomizer.Items.DungeonBigKeys.Contains(itemToPlace))
            {
                if (parseSetting.bigKeySettings == BigKeySettings.Own_Dungeon)
                {
                    if (itemName.Contains(currentRoom.Region))
                    {
                        return checkBarrenRegionLocation(currentRoom, currentCheck, itemName);
                    }
                }
                else if (parseSetting.bigKeySettings == BigKeySettings.Any_Dungeon)
                {
                    if (currentCheck.category.Contains("Dungeon"))
                    {
                        return checkBarrenRegionLocation(currentRoom, currentCheck, itemName);
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

        private static bool checkBarrenRegionLocation(
            Room currentRoom,
            Check currentCheck,
            string itemName
        )
        {
            SharedSettings parseSetting = Randomizer.SSettings;
            if (parseSetting.barrenDungeons)
            {
                if (
                    !itemName.Contains(currentRoom.Region)
                    && currentCheck.checkStatus.Contains("Excluded")
                )
                {
                    return false;
                }
                //Console.WriteLine("Can place " + itemName + " in " + currentCheck.checkName);
            }
            return true;
        }
    }
}
