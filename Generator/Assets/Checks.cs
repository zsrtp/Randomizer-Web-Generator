namespace TPRandomizer
{
    using System.Collections.Generic;
    using TPRandomizer.SSettings.Enums;

    /// <summary>
    /// Identifies the basic structure containing multiple fields used to identify a check in the randomizer..
    /// </summary>
    public class Check
    {
        public string checkName { get; set; } // The common name for the check this can be used in the randomizer to identify the check."

        public string requirements { get; set; } // List of requirements to obtain this check while inside the room (so does not include the items needed to enter the room)

        public string checkStatus { get; set; } // Identifies if the check is excluded or not. We can write the randomizer to not place important items in excluded checks

        public List<string> category { get; set; } // Allows grouping of checks to make it easier to randomize them based on their type, region, exclusion status, etc.

        public bool itemWasPlaced { get; set; } // Identifies if we already placed an item on this check.

        public bool hasBeenReached { get; set; } // indicates that we can get the current check. Prevents unneccesary repetitive parsing.

        // Data that will be stored in the rando-data .gci file.
        public Item itemId { get; set; } // The original item id of the check. This allows us to make an array of all items in the item pool for randomization purposes. Also is useful for documentation purposes.

        public List<byte> stageIDX { get; set; } // Used by DZX, SHOP, POE, and BOSS checks. The index of the stage where the check is located.

        public List<byte> lastStageIDX { get; set; } // Used by SKILL checks. The index of the previous stage where the player encountered the wolf.

        public byte roomIDX { get; set; } // Used by SKILL checks to determine which wolf is being learned from.

        public List<string> hash { get; set; } // Used by DZX checks. The hash of the actor that will be modified by a DZX-based check replacement.

        public List<string> dzxTag { get; set; } // Used by DZX checks. The type of actor that will be modified.

        public List<string[]> actrData { get; set; } // Used by DZX checks. The data structure that will replace the current loaded ACTR.

        public string flag { get; set; } // Used by POE and SKILL checks. The flag to check to determine which check to replace.

        public List<byte> fileDirectoryType { get; set; } // Used by ARC checks. The type of file directory where the item is stored.

        public List<byte> replacementType { get; set; } // Used by ARC checks. The type of replacement taking place.

        public List<string> moduleID { get; set; } // Used by REL checks. The module ID for the rel file being loaded.

        public List<string> relOffsets { get; set; } // Used by REL checks.

        public List<string> arcOffsets { get; set; } // Used by ARC checks.

        public List<string> magicByte { get; set; }

        public string fileName { get; set; }

        public List<string> relOverride { get; set; } // Used by REL checks. The override instruction to be used when replacing the item in the rel.
    }

    /// <summary>
    /// Contains function and structure definitions for all usages related to the Check class.
    /// </summary>
    public class CheckFunctions
    {
        public static List<string> forestRequirementChecksGlitchless =
            new()
            {
                "Forest Temple Big Baba Key",
                "Forest Temple Big Key Chest",
                "Forest Temple Central Chest Behind Stairs",
                "Forest Temple Central Chest Hanging From Web",
                "Forest Temple Central North Chest",
                "Forest Temple Diababa Heart Container",
                "Forest Temple Dungeon Reward",
                "Forest Temple East Tile Worm Chest",
                "Forest Temple East Water Cave Chest",
                "Forest Temple Entrance Vines Chest",
                "Forest Temple Gale Boomerang",
                "Forest Temple North Deku Like Chest",
                "Forest Temple Second Monkey Under Bridge Chest",
                "Forest Temple Totem Pole Chest",
                "Forest Temple West Deku Like Chest",
                "Forest Temple West Tile Worm Chest Behind Stairs",
                "Forest Temple West Tile Worm Room Vines Chest",
                "Forest Temple Windless Bridge Chest",
            };
        public static List<string> minesRequirementChecksGlitchless =
            new()
            {
                "Goron Mines After Crystal Switch Room Magnet Wall Chest",
                "Goron Mines Beamos Room Chest",
                "Goron Mines Chest Before Dangoro",
                "Goron Mines Crystal Switch Room Small Chest",
                "Goron Mines Crystal Switch Room Underwater Chest",
                "Goron Mines Dangoro Chest",
                "Goron Mines Dungeon Reward",
                "Goron Mines Entrance Chest",
                "Goron Mines Fyrus Heart Container",
                "Goron Mines Gor Amato Chest",
                "Goron Mines Gor Amato Key Shard",
                "Goron Mines Gor Amato Small Chest",
                "Goron Mines Gor Ebizo Chest",
                "Goron Mines Gor Ebizo Key Shard",
                "Goron Mines Gor Liggs Chest",
                "Goron Mines Gor Liggs Key Shard",
                "Goron Mines Magnet Maze Chest",
                "Goron Mines Main Magnet Room Bottom Chest",
                "Goron Mines Main Magnet Room Top Chest",
                "Goron Mines Outside Beamos Chest",
                "Goron Mines Outside Clawshot Chest",
                "Goron Mines Outside Underwater Chest",
                "Kakariko Village Malo Mart Hawkeye",
                "Talo Sharpshooting",
            };

        public static List<string> lakebedRequirementChecksGlitchless =
            new()
            {
                "Lakebed Temple Before Deku Toad Alcove Chest",
                "Lakebed Temple Before Deku Toad Underwater Left Chest",
                "Lakebed Temple Before Deku Toad Underwater Right Chest",
                "Lakebed Temple Big Key Chest",
                "Lakebed Temple Central Room Chest",
                "Lakebed Temple Central Room Small Chest",
                "Lakebed Temple Central Room Spire Chest",
                "Lakebed Temple Chandelier Chest",
                "Lakebed Temple Deku Toad Chest",
                "Lakebed Temple Dungeon Reward",
                "Lakebed Temple East Lower Waterwheel Bridge Chest",
                "Lakebed Temple East Lower Waterwheel Stalactite Chest",
                "Lakebed Temple East Second Floor Southeast Chest",
                "Lakebed Temple East Second Floor Southwest Chest",
                "Lakebed Temple East Water Supply Clawshot Chest",
                "Lakebed Temple East Water Supply Small Chest",
                "Lakebed Temple Lobby Left Chest",
                "Lakebed Temple Lobby Rear Chest",
                "Lakebed Temple Morpheel Heart Container",
                "Lakebed Temple Stalactite Room Chest",
                "Lakebed Temple Underwater Maze Small Chest",
                "Lakebed Temple West Lower Small Chest",
                "Lakebed Temple West Second Floor Central Small Chest",
                "Lakebed Temple West Second Floor Northeast Chest",
                "Lakebed Temple West Second Floor Southeast Chest",
                "Lakebed Temple West Second Floor Southwest Underwater Chest",
                "Lakebed Temple West Water Supply Chest",
                "Lakebed Temple West Water Supply Small Chest",
            };

        public static List<string> arbitersRequirementChecksGlitchless =
            new()
            {
                "Arbiters Grounds Big Key Chest",
                "Arbiters Grounds Death Sword Chest",
                "Arbiters Grounds East Lower Turnable Redead Chest",
                "Arbiters Grounds East Turning Room Poe",
                "Arbiters Grounds East Upper Turnable Chest",
                "Arbiters Grounds East Upper Turnable Redead Chest",
                "Arbiters Grounds Entrance Chest",
                "Arbiters Grounds Ghoul Rat Room Chest",
                "Arbiters Grounds Hidden Wall Poe",
                "Arbiters Grounds North Turning Room Chest",
                "Arbiters Grounds Spinner Room First Small Chest",
                "Arbiters Grounds Spinner Room Lower Central Small Chest",
                "Arbiters Grounds Spinner Room Lower North Chest",
                "Arbiters Grounds Spinner Room Second Small Chest",
                "Arbiters Grounds Spinner Room Stalfos Alcove Chest",
                "Arbiters Grounds Stallord Heart Container",
                "Arbiters Grounds Torch Room East Chest",
                "Arbiters Grounds Torch Room Poe",
                "Arbiters Grounds Torch Room West Chest",
                "Arbiters Grounds West Chandelier Chest",
                "Arbiters Grounds West Poe",
                "Arbiters Grounds West Small Chest Behind Block",
                "Arbiters Grounds West Stalfos Northeast Chest",
                "Arbiters Grounds West Stalfos West Chest",
            };

        public static List<string> snowpeakRequirementChecksGlitchless =
            new()
            {
                "Snowpeak Ruins Ball and Chain",
                "Snowpeak Ruins Blizzeta Heart Container",
                "Snowpeak Ruins Broken Floor Chest",
                "Snowpeak Ruins Chapel Chest",
                "Snowpeak Ruins Chest After Darkhammer",
                "Snowpeak Ruins Courtyard Central Chest",
                "Snowpeak Ruins Dungeon Reward",
                "Snowpeak Ruins East Courtyard Buried Chest",
                "Snowpeak Ruins East Courtyard Chest",
                "Snowpeak Ruins Ice Room Poe",
                "Snowpeak Ruins Lobby Armor Poe",
                "Snowpeak Ruins Lobby Chandelier Chest",
                "Snowpeak Ruins Lobby East Armor Chest",
                "Snowpeak Ruins Lobby Poe",
                "Snowpeak Ruins Lobby West Armor Chest",
                "Snowpeak Ruins Mansion Map",
                "Snowpeak Ruins Northeast Chandelier Chest",
                "Snowpeak Ruins Ordon Pumpkin Chest",
                "Snowpeak Ruins West Cannon Room Central Chest",
                "Snowpeak Ruins West Cannon Room Corner Chest",
                "Snowpeak Ruins West Courtyard Buried Chest",
                "Snowpeak Ruins Wooden Beam Central Chest",
                "Snowpeak Ruins Wooden Beam Chandelier Chest",
                "Snowpeak Ruins Wooden Beam Northwest Chest",
                "Snowboard Racing Prize",
            };

        public static List<string> totRequirementChecksGlitchless =
            new()
            {
                "Temple of Time Armogohma Heart Container",
                "Temple of Time Armos Antechamber East Chest",
                "Temple of Time Armos Antechamber North Chest",
                "Temple of Time Armos Antechamber Statue Chest",
                "Temple of Time Big Key Chest",
                "Temple of Time Chest Before Darknut",
                "Temple of Time Darknut Chest",
                "Temple of Time Dungeon Reward",
                "Temple of Time First Staircase Armos Chest",
                "Temple of Time First Staircase Gohma Gate Chest",
                "Temple of Time First Staircase Window Chest",
                "Temple of Time Floor Switch Puzzle Room Upper Chest",
                "Temple of Time Gilloutine Chest",
                "Temple of Time Lobby Lantern Chest",
                "Temple of Time Moving Wall Beamos Room Chest",
                "Temple of Time Moving Wall Dinalfos Room Chest",
                "Temple of Time Poe Above Scales",
                "Temple of Time Poe Behind Gate",
                "Temple of Time Scales Gohma Chest",
                "Temple of Time Scales Upper Chest",
                "Renados Letter",
                "Telma Invoice",
                "Wooden Statue",
                "Ilia Charm",
                "Ilia Memory Reward",
                "Hidden Village Poe",
                "Skybook From Impaz",
                "Doctors Office Balcony Chest",
                "North Castle Town Golden Wolf",
                "Cats Hide and Seek Minigame",
            };

        public static List<string> cityRequirementChecksGlitchless =
            new()
            {
                "City in The Sky Aeralfos Chest",
                "City in The Sky Argorok Heart Container",
                "City in The Sky Baba Tower Alcove Chest",
                "City in The Sky Baba Tower Narrow Ledge Chest",
                "City in The Sky Baba Tower Top Small Chest",
                "City in The Sky Big Key Chest",
                "City in The Sky Central Outside Ledge Chest",
                "City in The Sky Central Outside Poe Island Chest",
                "City in The Sky Chest Behind North Fan",
                "City in The Sky Chest Below Big Key Chest",
                "City in The Sky Dungeon Reward",
                "City in The Sky East First Wing Chest After Fans",
                "City in The Sky East Tile Worm Small Chest",
                "City in The Sky East Wing After Dinalfos Alcove Chest",
                "City in The Sky East Wing After Dinalfos Ledge Chest",
                "City in The Sky East Wing Lower Level Chest",
                "City in The Sky Garden Island Poe",
                "City in The Sky Poe Above Central Fan",
                "City in The Sky Underwater East Chest",
                "City in The Sky Underwater West Chest",
                "City in The Sky West Garden Corner Chest",
                "City in The Sky West Garden Ledge Chest",
                "City in The Sky West Garden Lone Island Chest",
                "City in The Sky West Garden Lower Chest",
                "City in The Sky West Wing Baba Balcony Chest",
                "City in The Sky West Wing First Chest",
                "City in The Sky West Wing Narrow Ledge Chest",
                "City in The Sky West Wing Tile Worm Chest",
            };

        public static List<string> palaceRequirementChecksGlitchless =
            new()
            {
                "Palace of Twilight Big Key Chest",
                "Palace of Twilight Central First Room Chest",
                "Palace of Twilight Central Outdoor Chest",
                "Palace of Twilight Central Tower Chest",
                "Palace of Twilight Collect Both Sols",
                "Palace of Twilight East Wing First Room East Alcove",
                "Palace of Twilight East Wing First Room North Small Chest",
                "Palace of Twilight East Wing First Room West Alcove",
                "Palace of Twilight East Wing First Room Zant Head Chest",
                "Palace of Twilight East Wing Second Room Northeast Chest",
                "Palace of Twilight East Wing Second Room Northwest Chest",
                "Palace of Twilight East Wing Second Room Southeast Chest",
                "Palace of Twilight East Wing Second Room Southwest Chest",
                "Palace of Twilight West Wing Chest Behind Wall of Darkness",
                "Palace of Twilight West Wing First Room Central Chest",
                "Palace of Twilight West Wing Second Room Central Chest",
                "Palace of Twilight West Wing Second Room Lower South Chest",
                "Palace of Twilight West Wing Second Room Southeast Chest",
                "Palace of Twilight Zant Heart Container",
            };

        /// <summary>
        /// A dictionary of all randomizer locations.
        /// </summary>
        public Dictionary<string, Check> CheckDict = new();

        /// <summary>
        /// summary text.
        /// </summary>
        public static void GenerateCheckList()
        {
            SharedSettings parseSetting = Randomizer.SSettings;
            foreach (KeyValuePair<string, Check> check in Randomizer.Checks.CheckDict)
            {
                Check currentCheck = check.Value;
                if (currentCheck.checkStatus == "Ready")
                {
                    if (
                        (parseSetting.smallKeySettings == SmallKeySettings.Vanilla)
                        && currentCheck.category.Contains("Small Key")
                    )
                    {
                        currentCheck.checkStatus = "Vanilla";
                    }

                    if (
                        (parseSetting.bigKeySettings == BigKeySettings.Vanilla)
                        && currentCheck.category.Contains("Big Key")
                    )
                    {
                        currentCheck.checkStatus = "Vanilla";
                    }

                    if (
                        (parseSetting.mapAndCompassSettings == MapAndCompassSettings.Vanilla)
                        && (
                            currentCheck.category.Contains("Dungeon Map")
                            || currentCheck.category.Contains("Compass")
                        )
                    )
                    {
                        currentCheck.checkStatus = "Vanilla";
                    }

                    if (!parseSetting.shuffleNpcItems)
                    {
                        if (currentCheck.category.Contains("Npc"))
                        {
                            if (
                                (
                                    (parseSetting.smallKeySettings == SmallKeySettings.Keysey)
                                    && currentCheck.category.Contains("Small Key")
                                )
                                || (
                                    (parseSetting.bigKeySettings == BigKeySettings.Keysey)
                                    && currentCheck.category.Contains("Big Key")
                                )
                                || (
                                    (
                                        parseSetting.mapAndCompassSettings
                                        == MapAndCompassSettings.Start_With
                                    )
                                    && (
                                        currentCheck.category.Contains("Dungeon Map")
                                        || currentCheck.category.Contains("Compass")
                                    )
                                )
                            )
                            {
                                currentCheck.checkStatus = "Excluded";
                            }
                            else
                            {
                                currentCheck.checkStatus = "Vanilla";
                                Randomizer.Items.RandomizedImportantItems.Remove(
                                    currentCheck.itemId
                                );
                                Randomizer.Items.alwaysItems.Remove(currentCheck.itemId);
                            }
                        }
                    }

                    if (!parseSetting.shufflePoes)
                    {
                        if (currentCheck.category.Contains("Poe"))
                        {
                            currentCheck.checkStatus = "Vanilla";
                        }
                    }

                    if (!parseSetting.shuffleGoldenBugs)
                    {
                        if (currentCheck.category.Contains("Golden Bug"))
                        {
                            currentCheck.checkStatus = "Vanilla";
                        }
                    }

                    if (!parseSetting.shuffleHiddenSkills)
                    {
                        if (currentCheck.category.Contains("Hidden Skill"))
                        {
                            currentCheck.checkStatus = "Vanilla";
                            Randomizer.Items.RandomizedImportantItems.Remove(currentCheck.itemId);
                        }
                    }

                    if (!parseSetting.shuffleSkyCharacters)
                    {
                        if (currentCheck.category.Contains("Sky Book"))
                        {
                            currentCheck.checkStatus = "Vanilla";
                            Randomizer.Items.RandomizedImportantItems.Remove(currentCheck.itemId);
                        }
                    }

                    if (!parseSetting.shuffleShopItems)
                    {
                        if (currentCheck.category.Contains("Shop"))
                        {
                            currentCheck.checkStatus = "Vanilla";
                            Randomizer.Items.RandomizedImportantItems.Remove(currentCheck.itemId);
                            Randomizer.Items.alwaysItems.Remove(currentCheck.itemId);
                        }
                    }
                }
            }

            if (!parseSetting.skipPrologue)
            {
                // We want to set Uli Cradle Delivery vanilla if intro is not skipped since a Fishing Rod has to be there in order to progress the seed.
                // We also place the Lantern vanilla because it is a big logic hole and since we don't know how to make coro give both items in one state yet, it's safer to do this.
                Randomizer.Checks.CheckDict["Uli Cradle Delivery"].checkStatus = "Vanilla";
                Randomizer.Items.RandomizedImportantItems.Remove(
                    Randomizer.Checks.CheckDict["Uli Cradle Delivery"].itemId
                );
            }
            else
            {
                Randomizer.Checks.CheckDict["Uli Cradle Delivery"].checkStatus = "Excluded";
                Randomizer.Checks.CheckDict["Ordon Cat Rescue"].checkStatus = "Excluded";
                Randomizer.Items.RandomizedImportantItems.Remove(Item.North_Faron_Woods_Gate_Key);
                Randomizer.Items.RandomizedDungeonRegionItems.Remove(
                    Item.North_Faron_Woods_Gate_Key
                );
            }

            if (parseSetting.faronTwilightCleared)
            {
                Randomizer.Checks.CheckDict["Ordon Sword"].checkStatus = "Excluded";
                Randomizer.Checks.CheckDict["Ordon Shield"].checkStatus = "Excluded";
            }
            else
            {
                Randomizer.Items.RandomizedImportantItems.Remove(
                    Randomizer.Checks.CheckDict["Ordon Sword"].itemId
                );
                Randomizer.Checks.CheckDict["Ordon Sword"].checkStatus = "Vanilla";
                Randomizer.Items.RandomizedImportantItems.Remove(
                    Randomizer.Checks.CheckDict["Ordon Shield"].itemId
                );
                Randomizer.Checks.CheckDict["Ordon Shield"].checkStatus = "Vanilla";
            }

            if (parseSetting.skipMdh)
            {
                Randomizer.Checks.CheckDict["Jovani House Poe"].checkStatus = "Excluded";
            }

            // Vanilla until all of the flag issues are figured out.
            Randomizer.Checks.CheckDict["Renados Letter"].checkStatus = "Vanilla";
            Randomizer.Checks.CheckDict["Telma Invoice"].checkStatus = "Vanilla";
            Randomizer.Checks.CheckDict["Wooden Statue"].checkStatus = "Vanilla";
            Randomizer.Checks.CheckDict["Ilia Charm"].checkStatus = "Vanilla";
            Randomizer.Checks.CheckDict["Ilia Memory Reward"].checkStatus = "Vanilla";
            Randomizer.Items.RandomizedImportantItems.Remove(
                Randomizer.Checks.CheckDict["Renados Letter"].itemId
            );
            Randomizer.Items.RandomizedImportantItems.Remove(
                Randomizer.Checks.CheckDict["Telma Invoice"].itemId
            );
            Randomizer.Items.RandomizedImportantItems.Remove(
                Randomizer.Checks.CheckDict["Wooden Statue"].itemId
            );
            Randomizer.Items.RandomizedImportantItems.Remove(
                Randomizer.Checks.CheckDict["Ilia Charm"].itemId
            );
            Randomizer.Items.RandomizedImportantItems.Remove(
                Randomizer.Checks.CheckDict["Ilia Memory Reward"].itemId
            );
        }
    }
}
