namespace TPRandomizer
{
    using System.Collections.Generic;

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
        /// <summary>
        /// A dictionary of all randomizer locations.
        /// </summary>
        public Dictionary<string, Check> CheckDict = new();

        /// <summary>
        /// summary text.
        /// </summary>
        public static void GenerateCheckList()
        {
            RandomizerSetting parseSetting = Randomizer.RandoSetting;
            foreach (KeyValuePair<string, Check> check in Randomizer.Checks.CheckDict)
            {
                Check currentCheck = check.Value;
                if (currentCheck.checkStatus == "Ready")
                {
                    if (
                        (parseSetting.smallKeySettings == "Vanilla")
                        && currentCheck.category.Contains("Small Key")
                    )
                    {
                        currentCheck.checkStatus = "Vanilla";
                    }

                    if (
                        (parseSetting.bossKeySettings == "Vanilla")
                        && currentCheck.category.Contains("Big Key")
                    )
                    {
                        currentCheck.checkStatus = "Vanilla";
                    }

                    if (
                        (parseSetting.mapAndCompassSettings == "Vanilla")
                        && (
                            currentCheck.category.Contains("Dungeon Map")
                            || currentCheck.category.Contains("Compass")
                        )
                    )
                    {
                        currentCheck.checkStatus = "Vanilla";
                    }

                    if (!parseSetting.npcItemsShuffled)
                    {
                        if (currentCheck.category.Contains("Npc"))
                        {
                            if (
                                (
                                    (parseSetting.smallKeySettings == "Keysey")
                                    && currentCheck.category.Contains("Small Key")
                                )
                                || (
                                    (parseSetting.bossKeySettings == "Keysey")
                                    && currentCheck.category.Contains("Big Key")
                                )
                                || (
                                    (parseSetting.mapAndCompassSettings == "Start_With")
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

                    if (!parseSetting.poesShuffled)
                    {
                        if (currentCheck.category.Contains("Poe"))
                        {
                            currentCheck.checkStatus = "Vanilla";
                        }
                    }

                    if (!parseSetting.goldenBugsShuffled)
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

                    if (!parseSetting.shopItemsShuffled)
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

            if (!parseSetting.introSkipped)
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

            if (parseSetting.mdhSkipped)
            {
                Randomizer.Checks.CheckDict["Jovani House Poe"].checkStatus = "Excluded";
            }

            // Excluded until we figure out how to fix this check.
            Randomizer.Checks.CheckDict["Jovani 60 Poe Soul Reward"].checkStatus = "Vanilla";

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
