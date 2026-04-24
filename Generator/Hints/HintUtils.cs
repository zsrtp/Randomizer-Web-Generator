namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Util;
    using SSettings.Enums;

    public enum GoalEnum
    {
        Invalid = 0,
        Diababa = 1,
        Fyrus = 2,
        Morpheel = 3,
        Stallord = 4,
        Blizzeta = 5,
        Armogohma = 6,
        Argorok = 7,
        Zant = 8,
        Hyrule_Castle = 9,
        Ganondorf = 10,
    }

    public class Goal
    {
        public enum Type
        {
            Room,
            Check,
            Logic,
        }

        public GoalEnum goalEnum { get; }
        public Type type { get; }
        public string id { get; }
        private LogicAST reqsCache;

        public Goal(GoalEnum goalEnum, Type type, string id)
        {
            this.goalEnum = goalEnum;
            this.type = type;
            this.id = id;
        }

        public static Goal Check(string checkName)
        {
            return new Goal(GoalEnum.Invalid, Type.Check, checkName);
        }

        public static Goal Room(string roomName)
        {
            return new Goal(GoalEnum.Invalid, Type.Room, roomName);
        }

        public static Goal Logic(string logic)
        {
            return new Goal(GoalEnum.Invalid, Type.Logic, logic);
        }

        public LogicAST CachedRequirements()
        {
            if (type != Type.Logic)
                throw new Exception(
                    $"Tried to call CachedRequirements on a Goal with type '{type}'."
                );

            if (reqsCache != null)
                return reqsCache;

            return reqsCache = Parser.Parse(id);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                Goal g = (Goal)obj;
                return this.goalEnum == g.goalEnum && this.type == g.type && this.id == g.id;
            }
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (goalEnum, type, id).GetHashCode();
        }
    }

    public class GoalConstants
    {
        public static readonly byte NumBitsToEncode = 6;

        public static readonly Goal Diababa = new Goal(
            GoalEnum.Diababa,
            Goal.Type.Check,
            "Forest Temple Dungeon Reward"
        );
        public static readonly Goal Fyrus = new Goal(
            GoalEnum.Fyrus,
            Goal.Type.Check,
            "Goron Mines Dungeon Reward"
        );
        public static readonly Goal Morpheel = new Goal(
            GoalEnum.Morpheel,
            Goal.Type.Check,
            "Lakebed Temple Dungeon Reward"
        );
        public static readonly Goal Stallord = new Goal(
            GoalEnum.Stallord,
            Goal.Type.Check,
            "Arbiters Grounds Stallord Heart Container"
        );
        public static readonly Goal Blizzeta = new Goal(
            GoalEnum.Blizzeta,
            Goal.Type.Check,
            "Snowpeak Ruins Dungeon Reward"
        );
        public static readonly Goal Armogohma = new Goal(
            GoalEnum.Armogohma,
            Goal.Type.Check,
            "Temple of Time Dungeon Reward"
        );
        public static readonly Goal Argorok = new Goal(
            GoalEnum.Argorok,
            Goal.Type.Check,
            "City in The Sky Dungeon Reward"
        );

        public static readonly Goal Zant = new Goal(
            GoalEnum.Zant,
            Goal.Type.Check,
            "Palace of Twilight Zant Heart Container"
        );
        public static readonly Goal Hyrule_Castle = new Goal(
            GoalEnum.Hyrule_Castle,
            Goal.Type.Room,
            "Hyrule Castle Entrance"
        );
        public static readonly Goal Ganondorf = new Goal(
            GoalEnum.Ganondorf,
            Goal.Type.Check,
            "Hyrule Castle Ganondorf"
        );

        private static readonly Dictionary<GoalEnum, Goal> goalEnumToGoal =
            new()
            {
                { GoalEnum.Diababa, Diababa },
                { GoalEnum.Fyrus, Fyrus },
                { GoalEnum.Morpheel, Morpheel },
                { GoalEnum.Stallord, Stallord },
                { GoalEnum.Blizzeta, Blizzeta },
                { GoalEnum.Armogohma, Armogohma },
                { GoalEnum.Argorok, Argorok },
                { GoalEnum.Zant, Zant },
                { GoalEnum.Hyrule_Castle, Hyrule_Castle },
                { GoalEnum.Ganondorf, Ganondorf },
            };

        public static readonly Dictionary<string, Goal> requiredDungeonHintZoneToGoal =
            new()
            {
                { "Forest Temple", Diababa },
                { "Goron Mines", Fyrus },
                { "Lakebed Temple", Morpheel },
                { "Arbiter's Grounds", Stallord },
                { "Snowpeak Ruins", Blizzeta },
                { "Temple of Time", Armogohma },
                { "City in the Sky", Argorok },
                { "Palace of Twilight", Zant },
            };

        private static readonly Dictionary<SpotId, List<Goal>> glitchlessSpotToGoals =
            new()
            {
                {
                    SpotId.Agithas_Castle_Sign,
                    new() { Goal.Room("Castle Town South") }
                },
                {
                    SpotId.Ordon_Sign,
                    new() { Goal.Room("Outside Links House") }
                },
                {
                    SpotId.Sacred_Grove_Sign,
                    new() { Goal.Room("Sacred Grove Upper") }
                },
                {
                    SpotId.Faron_Field_Sign,
                    new() { Goal.Room("Faron Field") }
                },
                {
                    SpotId.Faron_Woods_Sign,
                    new() { Goal.Room("South Faron Woods") }
                },
                {
                    SpotId.Kakariko_Gorge_Sign,
                    new() { Goal.Room("Kakariko Gorge") }
                },
                {
                    SpotId.Kakariko_Village_Sign,
                    new() { Goal.Room("Lower Kakariko Village") }
                },
                {
                    SpotId.Kakariko_Graveyard_Sign,
                    new()
                    {
                        Goal.Room("Kakariko Graveyard"),
                        Goal.Logic("Gate_Keys and CanCompleteEldinTwilight")
                    }
                },
                {
                    SpotId.Eldin_Field_Sign,
                    new() { Goal.Room("Eldin Field") }
                },
                {
                    SpotId.North_Eldin_Sign,
                    new() { Goal.Room("North Eldin Field") }
                },
                {
                    SpotId.Death_Mountain_Sign,
                    new() { Goal.Room("Death Mountain Volcano") }
                },
                {
                    SpotId.Hidden_Village_Sign,
                    new() { Goal.Room("Hidden Village") }
                },
                {
                    SpotId.Lanayru_Field_Sign,
                    new() { Goal.Room("Lanayru Field") }
                },
                {
                    SpotId.Beside_Castle_Town_Sign,
                    new() { Goal.Room("Outside Castle Town West Grotto Ledge") }
                },
                {
                    SpotId.South_of_Castle_Town_Sign,
                    new() { Goal.Room("Outside Castle Town South") }
                },
                {
                    SpotId.Castle_Town_Sign,
                    new() { Goal.Room("Castle Town Center") }
                },
                {
                    SpotId.Great_Bridge_of_Hylia_Sign,
                    new()
                    {
                        Goal.Room("Lake Hylia Bridge"),
                        Goal.Logic("(Progressive_Clawshot, 1)")
                    }
                },
                {
                    SpotId.Lake_Hylia_Sign,
                    new() { Goal.Room("Lake Hylia Flight By Fowl") }
                },
                {
                    SpotId.Lake_Lantern_Cave_Sign,
                    new() { Goal.Room("Lake Hylia Long Cave"), Goal.Logic("CanSmash and Lantern") }
                },
                {
                    SpotId.Lanayru_Spring_Sign,
                    new()
                    {
                        Goal.Room("Lake Hylia Lanayru Spring"),
                        Goal.Logic("Zora_Armor or Iron_Boots")
                    }
                },
                {
                    SpotId.Zoras_Domain_Sign,
                    new() { Goal.Room("Zoras Domain West Ledge"), }
                },
                {
                    SpotId.Upper_Zoras_River_Sign,
                    new() { Goal.Room("Fishing Hole"), }
                },
                {
                    SpotId.Gerudo_Desert_Sign,
                    new() { Goal.Room("Gerudo Desert"), }
                },
                {
                    SpotId.Bulblin_Camp_Sign,
                    new() { Goal.Room("Bulblin Camp"), }
                },
                {
                    SpotId.Snowpeak_Mountain_Sign,
                    new() { Goal.Room("Snowpeak Climb Lower"), }
                },
                {
                    SpotId.Cave_of_Ordeals_Sign,
                    new() { Goal.Room("Gerudo Desert Cave of Ordeals Floors 01-11"), }
                },
                {
                    SpotId.Forest_Temple_Sign,
                    new()
                    {
                        Goal.Room("Forest Temple Lobby"),
                        // Lobby to west wing logic, minus needing to destroy the web.
                        Goal.Logic(
                            "((Forest_Temple_Small_Key, 2) and CanDefeatBokoblin) or (Progressive_Clawshot, 1)"
                        )
                    }
                },
                {
                    SpotId.Goron_Mines_Sign,
                    new() { Goal.Room("Goron Mines Upper East Wing"), }
                },
                {
                    SpotId.Lakebed_Temple_Sign,
                    // Note: can pull the immediate switch and easily walk to sign
                    new() { Goal.Room("Lakebed Temple Central Room"), }
                },
                {
                    SpotId.Arbiters_Grounds_Sign,
                    new() { Goal.Room("Arbiters Grounds Lobby"), }
                },
                {
                    SpotId.Snowpeak_Ruins_Sign,
                    new() { Goal.Room("Snowpeak Ruins Yeto and Yeta"), }
                },
                {
                    SpotId.Temple_of_Time_Sign,
                    new() { Goal.Room("Temple of Time Entrance"), }
                },
                {
                    SpotId.City_in_the_Sky_Sign,
                    new() { Goal.Room("City in The Sky Lobby"), }
                },
                {
                    SpotId.Palace_of_Twilight_Sign,
                    new() { Goal.Room("Palace of Twilight Entrance"), }
                },
                {
                    SpotId.Hyrule_Castle_Sign,
                    new() { Goal.Room("Hyrule Castle Entrance"), }
                },
                {
                    SpotId.Temple_of_Time_Beyond_Point_Sign,
                    // Note: matching main logic which expects Bow specifically even for glitched.
                    new()
                    {
                        Goal.Room("Temple of Time Moving Wall Hallways"),
                        Goal.Logic("(Progressive_Bow, 1)")
                    }
                },
                {
                    SpotId.Jovani_House_Sign,
                    new() { Goal.Room("Castle Town South") }
                },
                {
                    SpotId.Midna,
                    // Always available
                    new() { Goal.Logic("true") }
                },
            };

        // If non-glitchless logic and an entry is present here, it takes priority over the
        // glitchless one. Otherwise we fall back to the glitchless one.
        private static readonly Dictionary<SpotId, List<Goal>> glitchedSpotToGoals =
            new()
            {
                {
                    SpotId.Kakariko_Graveyard_Sign,
                    new()
                    {
                        Goal.Room("Kakariko Graveyard"),
                        // Note: based on logic for connection to Lake Hylia.
                        Goal.Logic(
                            "(HasBombs and (HasSword or Spinner)) or CanDoLJA or CanDoMoonBoots"
                        )
                    }
                },
                {
                    SpotId.Lake_Lantern_Cave_Sign,
                    new() { Goal.Room("Lake Hylia Long Cave"), Goal.Logic("CanSmash") }
                },
                {
                    SpotId.Lanayru_Spring_Sign,
                    new()
                    {
                        Goal.Room("Lake Hylia Lanayru Spring"),
                        Goal.Logic("Zora_Armor or HasHeavyMod")
                    }
                },
                {
                    SpotId.Forest_Temple_Sign,
                    new()
                    {
                        Goal.Room("Forest Temple Lobby"),
                        Goal.Logic(
                            "((Forest_Temple_Small_Key, 2) and CanDefeatBokoblin) or (Progressive_Clawshot, 1) or CanDoLJA"
                        )
                    }
                },
            };

        public static List<Goal> getGoalsForSpot(SpotId spotId, LogicRules logicRules)
        {
            List<Goal> list;
            if (logicRules != LogicRules.Glitchless)
            {
                if (glitchedSpotToGoals.TryGetValue(spotId, out list))
                    return list;
            }
            if (!glitchlessSpotToGoals.TryGetValue(spotId, out list))
                throw new Exception($"Could not find goals for spotId '{spotId}'.");
            return list;
        }

        public static bool IsDungeonGoal(Goal goal)
        {
            if (goal == null)
                return false;

            return requiredDungeonHintZoneToGoal.ContainsValue(goal);
        }

        public static Goal getGoalFromEnumThrows(GoalEnum goalEnum)
        {
            if (!goalEnumToGoal.TryGetValue(goalEnum, out Goal goal))
                throw new Exception($"Failed to find Goal for GoalEnum '{goalEnum}'.");
            return goal;
        }
    }

    public class HintUtils
    {
        private static readonly HashSet<string> dungeonZones =
            new()
            {
                "Forest Temple",
                "Goron Mines",
                "Lakebed Temple",
                "Arbiter's Grounds",
                "Snowpeak Ruins",
                "Temple of Time",
                "City in the Sky",
                "Palace of Twilight",
                "Hyrule Castle",
            };

        public static readonly Dictionary<Item, string> tradeItemToRewardCheck =
            new()
            {
                { Item.Female_Ant, "Agitha Female Ant Reward" },
                { Item.Female_Beetle, "Agitha Female Beetle Reward" },
                { Item.Female_Butterfly, "Agitha Female Butterfly Reward" },
                { Item.Female_Dayfly, "Agitha Female Dayfly Reward" },
                { Item.Female_Dragonfly, "Agitha Female Dragonfly Reward" },
                { Item.Female_Grasshopper, "Agitha Female Grasshopper Reward" },
                { Item.Female_Ladybug, "Agitha Female Ladybug Reward" },
                { Item.Female_Mantis, "Agitha Female Mantis Reward" },
                { Item.Female_Phasmid, "Agitha Female Phasmid Reward" },
                { Item.Female_Pill_Bug, "Agitha Female Pill Bug Reward" },
                { Item.Female_Snail, "Agitha Female Snail Reward" },
                { Item.Female_Stag_Beetle, "Agitha Female Stag Beetle Reward" },
                { Item.Male_Ant, "Agitha Male Ant Reward" },
                { Item.Male_Beetle, "Agitha Male Beetle Reward" },
                { Item.Male_Butterfly, "Agitha Male Butterfly Reward" },
                { Item.Male_Dayfly, "Agitha Male Dayfly Reward" },
                { Item.Male_Dragonfly, "Agitha Male Dragonfly Reward" },
                { Item.Male_Grasshopper, "Agitha Male Grasshopper Reward" },
                { Item.Male_Ladybug, "Agitha Male Ladybug Reward" },
                { Item.Male_Mantis, "Agitha Male Mantis Reward" },
                { Item.Male_Phasmid, "Agitha Male Phasmid Reward" },
                { Item.Male_Pill_Bug, "Agitha Male Pill Bug Reward" },
                { Item.Male_Snail, "Agitha Male Snail Reward" },
                { Item.Male_Stag_Beetle, "Agitha Male Stag Beetle Reward" },
                { Item.Asheis_Sketch, "Gift From Ralis" },
                { Item.Renados_Letter, "Telma Invoice" },
                { Item.Ilias_Charm, "Ilia Memory Reward" },
            };

        public static readonly Dictionary<string, Item> tradeRewardCheckToSourceItem;

        static HintUtils()
        {
            tradeRewardCheckToSourceItem = new();
            foreach (KeyValuePair<Item, string> pair in tradeItemToRewardCheck)
            {
                tradeRewardCheckToSourceItem[pair.Value] = pair.Key;
            }

            //         { Item.Female_Ant, "Agitha Female Ant Reward" },
            //         { Item.Female_Beetle, "Agitha Female Beetle Reward" },
            //         { Item.Female_Butterfly, "Agitha Female Butterfly Reward" },
            //         { Item.Female_Dayfly, "Agitha Female Dayfly Reward" },
            //         { Item.Female_Dragonfly, "Agitha Female Dragonfly Reward" },
            //         { Item.Female_Grasshopper, "Agitha Female Grasshopper Reward" },
            //         { Item.Female_Ladybug, "Agitha Female Ladybug Reward" },
            //         { Item.Female_Mantis, "Agitha Female Mantis Reward" },
            //         { Item.Female_Phasmid, "Agitha Female Phasmid Reward" },
            //         { Item.Female_Pill_Bug, "Agitha Female Pill Bug Reward" },
            //         { Item.Female_Snail, "Agitha Female Snail Reward" },
            //         { Item.Female_Stag_Beetle, "Agitha Female Stag Beetle Reward" },
            //         { Item.Male_Ant, "Agitha Male Ant Reward" },
            //         { Item.Male_Beetle, "Agitha Male Beetle Reward" },
            //         { Item.Male_Butterfly, "Agitha Male Butterfly Reward" },
            //         { Item.Male_Dayfly, "Agitha Male Dayfly Reward" },
            //         { Item.Male_Dragonfly, "Agitha Male Dragonfly Reward" },
            //         { Item.Male_Grasshopper, "Agitha Male Grasshopper Reward" },
            //         { Item.Male_Ladybug, "Agitha Male Ladybug Reward" },
            //         { Item.Male_Mantis, "Agitha Male Mantis Reward" },
            //         { Item.Male_Phasmid, "Agitha Male Phasmid Reward" },
            //         { Item.Male_Pill_Bug, "Agitha Male Pill Bug Reward" },
            //         { Item.Male_Snail, "Agitha Male Snail Reward" },
            //         { Item.Male_Stag_Beetle, "Agitha Male Stag Beetle Reward" },
            //         { Item.Asheis_Sketch, "Gift From Ralis" },
        }

        public static bool DungeonIsRequired(string dungeonHintZoneName)
        {
            return (
                    Randomizer.RequiredDungeons
                    & HintConstants.dungeonZonesToRequiredMaskMap[dungeonHintZoneName]
                ) != 0;
        }

        public static HashSet<string> getRequiredDungeonZones()
        {
            HashSet<string> result = new();

            foreach (KeyValuePair<string, byte> pair in HintConstants.dungeonZonesToRequiredMaskMap)
            {
                if ((Randomizer.RequiredDungeons & pair.Value) != 0)
                {
                    result.Add(pair.Key);
                }
            }

            return result;
        }

        public static HashSet<string> calculateRequiredChecks(
            Room startingRoom,
            List<List<KeyValuePair<int, Item>>> spheres
        )
        {
            HashSet<string> maybeRequiredCheckNames = new();

            foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
            {
                foreach (KeyValuePair<int, Item> pair in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(pair.Key);
                    maybeRequiredCheckNames.Add(checkName);
                }
            }

            // HashSet<string> requiredChecks = filterToRequiredChecksByPlaythroughs(
            //     startingRoom,
            //     maybeRequiredCheckNames
            // );
            // return requiredChecks;

            HashSet<Goal> goals = new() { GoalConstants.Ganondorf };

            Dictionary<Goal, List<string>> goalToRequiredChecks = filterToRequiredChecksOfGoals(
                startingRoom,
                maybeRequiredCheckNames,
                goals,
                false
            );

            HashSet<string> requiredChecks = new();
            foreach (KeyValuePair<Goal, List<string>> pair in goalToRequiredChecks)
            {
                List<string> checkNames = pair.Value;
                if (checkNames != null)
                {
                    foreach (string checkName in checkNames)
                    {
                        requiredChecks.Add(checkName);
                    }
                }
            }

            return requiredChecks;
        }

        public static bool? CalcAgithaRequired(Room startingRoom, SharedSettings sSettings)
        {
            if (sSettings.logicRules == LogicRules.No_Logic)
                return null;

            List<(string, Item)> checkAndOriginalContents = new();

            foreach (KeyValuePair<Item, string> pair in HintConstants.bugsToRewardChecksMap)
            {
                string agithaRewardCheck = pair.Value;

                // Replace check contents with a green rupee. If the playthrough
                // is still beatable, then that item cannot be considered for
                // SpoL hints.
                Item originalContents = Randomizer.Checks.CheckDict[agithaRewardCheck].itemId;
                Randomizer.Checks.CheckDict[agithaRewardCheck].itemId = Item.Green_Rupee;

                checkAndOriginalContents.Add(new(agithaRewardCheck, originalContents));
            }

            Dictionary<Goal, List<Goal>> goals = new();
            goals.Add(GoalConstants.Ganondorf, new() { GoalConstants.Ganondorf });

            // Result is true if beatable without Agitha
            Dictionary<Goal, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                startingRoom,
                goals,
                false
            );

            bool agithaRequired = !goalResults[GoalConstants.Ganondorf];

            // Put the original items back.
            foreach ((string, Item) pair in checkAndOriginalContents)
            {
                Randomizer.Checks.CheckDict[pair.Item1].itemId = pair.Item2;
            }

            return agithaRequired;
        }

        public static Dictionary<Goal, List<string>> calculateGoalsRequiredChecks(
            Room startingRoom,
            List<List<KeyValuePair<int, Item>>> spheres,
            SharedSettings sSettings,
            HashSet<Goal> goalsFromDungeons = null
        )
        {
            HashSet<string> maybeRequiredCheckNames = new();

            foreach (List<KeyValuePair<int, Item>> spherePairs in spheres)
            {
                foreach (KeyValuePair<int, Item> pair in spherePairs)
                {
                    string checkName = CheckIdClass.GetCheckName(pair.Key);
                    maybeRequiredCheckNames.Add(checkName);
                }
            }

            if (goalsFromDungeons == null)
                goalsFromDungeons = getGoalsBasedOnDungeons(sSettings);

            bool startWithBigKeys =
                sSettings.bigKeySettings == BigKeySettings.Anywhere
                || sSettings.bigKeySettings == BigKeySettings.Any_Dungeon;

            return filterToRequiredChecksOfGoals(
                startingRoom,
                maybeRequiredCheckNames,
                goalsFromDungeons,
                startWithBigKeys
            );
        }

        public static HashSet<Goal> getGoalsBasedOnDungeons(SharedSettings sSettings)
        {
            HashSet<Goal> result = new();

            // Goals to bosses are only valid if it is common knowledge based on
            // the settings that the bosses themselves are required.

            if (!sSettings.shuffleRewards)
            {
                // If not shuffling rewards, can simply add a goal for the boss
                // of each required dungeon.
                HashSet<string> requiredDungeons = getRequiredDungeonZones();
                foreach (string dungeonZone in requiredDungeons)
                {
                    if (GoalConstants.requiredDungeonHintZoneToGoal.ContainsKey(dungeonZone))
                    {
                        Goal goal = GoalConstants.requiredDungeonHintZoneToGoal[dungeonZone];
                        result.Add(goal);
                    }
                }
            }
            else
            {
                // If dungeonRewards are shuffled then only hint toward bosses
                // that we 100% know must be defeated purely based on settings.

                if (sSettings.castleRequirements == CastleRequirements.Vanilla)
                {
                    result.Add(GoalConstants.Stallord);
                    result.Add(GoalConstants.Zant);
                }
                else if (sSettings.castleRequirements == CastleRequirements.Dungeons)
                {
                    result.Add(GoalConstants.Diababa);
                    result.Add(GoalConstants.Fyrus);
                    result.Add(GoalConstants.Morpheel);
                    result.Add(GoalConstants.Stallord);
                    result.Add(GoalConstants.Blizzeta);
                    result.Add(GoalConstants.Armogohma);
                    result.Add(GoalConstants.Argorok);
                    result.Add(GoalConstants.Zant);
                }

                if (
                    result.Contains(GoalConstants.Zant)
                    && sSettings.palaceRequirements == PalaceRequirements.Vanilla
                )
                {
                    result.Add(GoalConstants.Argorok);
                }

                if (!sSettings.skipMdh)
                    result.Add(GoalConstants.Morpheel);

                if (sSettings.logicRules == LogicRules.Glitchless)
                {
                    // If we are playing glitchless and Skybooks are vanilla and
                    // are needed for City, we conclude that ToT is required as
                    // Impaz will have a book in village. This will change with
                    // ER.

                    // This seems like it fails to take starting items into
                    // consideration. Can worry about it later since as already
                    // noted, we will need to revisit for ER -isaac
                    if (
                        result.Contains(GoalConstants.Argorok)
                        && !sSettings.shuffleNpcItems
                        && !sSettings.skipCityEntrance
                    )
                    {
                        result.Add(GoalConstants.Armogohma);
                    }

                    // If Faron Woods is closed then we need to beat Forest
                    // Temple to leave.
                    if (sSettings.faronWoodsLogic == FaronWoodsLogic.Closed)
                        result.Add(GoalConstants.Diababa);
                }
            }

            result.Add(GoalConstants.Hyrule_Castle);
            result.Add(GoalConstants.Ganondorf);

            return result;
        }

        // private static HashSet<string> filterToRequiredChecksByPlaythroughs(
        //     Room startingRoom,
        //     HashSet<string> maybeRequiredCheckNames
        // )
        // {
        //     HashSet<string> requiredChecks = new();

        //     Dictionary<Goal, List<string>> goalsToRequiredChecks = filterToRequiredChecksOfGoals(
        //         startingRoom,
        //         maybeRequiredCheckNames
        //     );
        //     foreach (KeyValuePair<Goal, List<string>> pair in goalsToRequiredChecks)
        //     {
        //         List<string> checkNames = pair.Value;
        //         if (checkNames != null)
        //         {
        //             foreach (string checkName in checkNames)
        //             {
        //                 requiredChecks.Add(checkName);
        //             }
        //         }
        //     }

        //     return requiredChecks;
        // }

        private static Dictionary<Goal, List<string>> filterToRequiredChecksOfGoals(
            Room startingRoom,
            HashSet<string> maybeRequiredCheckNames,
            HashSet<Goal> goals,
            bool startWithBigKeys
        )
        {
            Dictionary<Goal, List<string>> goalsToRequiredChecks = new();
            foreach (Goal goal in goals)
            {
                goalsToRequiredChecks[goal] = new();
            }

            Dictionary<Goal, List<Goal>> goalDict = new();
            foreach (Goal goal in goals)
            {
                goalDict[goal] = new() { goal };
            }

            // I think it is safe to only generate the item pool once up front.
            Randomizer.Items.GenerateItemPool();

            // After we get potential checks, filter out any which can be removed in isolation and
            // the playthrough is still valid.
            foreach (string checkName in maybeRequiredCheckNames)
            {
                Dictionary<Goal, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                    startingRoom,
                    goalDict,
                    startWithBigKeys,
                    forbiddenCheckNames: new() { checkName }
                );

                foreach (KeyValuePair<Goal, bool> pair in goalResults)
                {
                    if (!pair.Value)
                        goalsToRequiredChecks[pair.Key].Add(checkName);
                }
            }

            return goalsToRequiredChecks;
        }

        public static HashSet<string> calcFindingItemBlocksItself(
            Room startingRoom,
            SharedSettings sSettings,
            List<string> checkNames
        )
        {
            HashSet<string> results = new();
            if (sSettings.logicRules == LogicRules.No_Logic || ListUtils.isEmpty(checkNames))
                return results;

            HashSet<Item> itemsForChecks = new();
            foreach (string checkName in checkNames)
            {
                Item item = HintUtils.getCheckContents(checkName);
                itemsForChecks.Add(item);
            }
            if (itemsForChecks.Count != 1)
                throw new Exception(
                    $"Expected itemsForChecks to be Count 1, but was '{itemsForChecks.Count}'."
                );

            Dictionary<string, List<Goal>> goalsForChecks = new();
            foreach (string checkName in checkNames)
            {
                goalsForChecks[checkName] = new() { Goal.Check(checkName) };
            }

            Dictionary<string, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                startingRoom,
                goalsForChecks,
                false,
                forbiddenCheckNames: new(checkNames)
            );

            foreach (KeyValuePair<string, bool> pair in goalResults)
            {
                if (!pair.Value)
                {
                    // This check which gives the item is locked behind first doing a different
                    // check which already gives the item.
                    results.Add(pair.Key);
                }
            }
            return results;
        }

        private static Dictionary<Item, List<string>> calcItemToChecksList()
        {
            Dictionary<Item, List<string>> itemToChecks = new();

            foreach (KeyValuePair<string, Check> pair in Randomizer.Checks.CheckDict)
            {
                Check check = pair.Value;
                Item contents = check.itemId;
                if (!itemToChecks.ContainsKey(contents))
                {
                    itemToChecks[contents] = new();
                }
                List<string> checkNameList = itemToChecks[contents];
                checkNameList.Add(pair.Value.checkName);
            }

            return itemToChecks;
        }

        public static Dictionary<Item, Dictionary<Goal, bool>> checkGoalsWithoutItems(
            Room startingRoom,
            List<Item> items,
            Dictionary<Goal, List<Goal>> goals
        )
        {
            Dictionary<Item, List<string>> itemToChecks = calcItemToChecksList();

            // I think it is safe to only generate the item pool once up front.
            Randomizer.Items.GenerateItemPool();

            Dictionary<Item, Dictionary<Goal, bool>> results = new();

            // After we get potential checks, filter out any which can be
            // removed in isolation and the playthrough is still valid.
            foreach (Item item in items)
            {
                // TODO: this needs to handle when the item does not show up.
                // For example, when the player starts with the only Boomerang.
                itemToChecks.TryGetValue(item, out List<string> checkNames);
                if (ListUtils.isEmpty(checkNames))
                {
                    results[item] = new();
                    checkNames = new();
                }

                Dictionary<Goal, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                    startingRoom,
                    goals,
                    true,
                    forbiddenCheckNames: new(checkNames)
                );

                results[item] = goalResults;
            }

            return results;
        }

        public static void ShuffleListInPlace<T>(Random rnd, IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<T> MergeListsRandomly<T>(Random rnd, params List<T>[] lists)
        {
            List<List<T>> newLists = new();
            foreach (List<T> list in lists)
            {
                List<T> listCopy = new(list);
                newLists.Add(listCopy);
            }

            List<T> outputList = new();

            while (true)
            {
                if (newLists.Count < 1)
                    break;

                int listIndex = rnd.Next(newLists.Count);
                List<T> list = newLists[listIndex];
                if (list.Count < 1)
                {
                    // remove this list and continue
                    newLists.RemoveAt(listIndex);
                    continue;
                }

                outputList.Add(list[0]);
                list.RemoveAt(0);
            }

            return outputList;
        }

        public static T RemoveRandomListItem<T>(Random rnd, List<T> list)
        {
            int index = rnd.Next(list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static T RemoveRandomHashSetItem<T>(Random rnd, HashSet<T> hashSet)
        {
            List<T> list = new(hashSet);
            int index = rnd.Next(list.Count);
            T item = list[index];
            hashSet.Remove(item);
            return item;
        }

        public static KeyValuePair<A, B> RemoveRandomDictionaryItem<A, B>(
            Random rnd,
            Dictionary<A, B> dict
        )
        {
            List<KeyValuePair<A, B>> list = new(dict);
            int index = rnd.Next(list.Count);
            KeyValuePair<A, B> item = list[index];
            dict.Remove(item.Key);
            return item;
        }

        public static KeyValuePair<A, B> PickRandomDictionaryPair<A, B>(
            Random rnd,
            Dictionary<A, B> dict
        )
        {
            int desiredIndex = rnd.Next(dict.Count);
            int i = 0;
            foreach (KeyValuePair<A, B> pair in dict)
            {
                if (i == desiredIndex)
                    return pair;
                i += 1;
            }
            throw new Exception("Failed to pick random dictionary key.");
        }

        public static T PickRandomListItem<T>(Random rnd, IList<T> list)
        {
            if (ListUtils.isEmpty(list))
                throw new Exception("Cannot pick a random item from null or empty list.");

            int randomIndex = rnd.Next(list.Count);
            return list[randomIndex];
        }

        public static T PickRandomHashSetItem<T>(Random rnd, HashSet<T> hashSet)
        {
            List<T> list = new(hashSet);
            int index = rnd.Next(list.Count);
            T item = list[index];
            return item;
        }

        public static bool checkIsExcludedOrVanilla(string checkName)
        {
            string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
            return HintConstants.excludedOrVanillaCheckStatuses.Contains(checkStatus);
        }

        public static bool checkIsExcluded(string checkName)
        {
            string checkStatus = Randomizer.Checks.CheckDict[checkName].checkStatus;
            return HintConstants.excludedCheckStatuses.Contains(checkStatus);
        }

        public static bool checkIsVanilla(string checkName)
        {
            return Randomizer.Checks.CheckDict[checkName].checkStatus == "Vanilla";
        }

        public static bool isItemGoldenBug(Item item)
        {
            return HintConstants.bugsToRewardChecksMap.ContainsKey(item);
        }

        public static bool isItemMaleBug(Item item)
        {
            return isItemGoldenBug(item) && item.ToString().StartsWith("Male");
        }

        public static bool isItemFemaleBug(Item item)
        {
            return isItemGoldenBug(item) && item.ToString().StartsWith("Female");
        }

        public static bool hintZoneIsDungeon(string hintZone)
        {
            return HintConstants.dungeonZones.Contains(hintZone);
        }

        public static Item getCheckContents(string checkName)
        {
            return Randomizer.Checks.CheckDict[checkName].itemId;
        }

        public static Item getCheckContents(string checkName, Dictionary<int, int> itemPlacements)
        {
            int srcCheckId = CheckIdClass.GetCheckIdNum(checkName);
            return (Item)itemPlacements[srcCheckId];
        }

        public static SpotId TryGetSpotIdForBarrenZoneHint(Hint hint)
        {
            if (hint == null)
                throw new Exception("Called TryGetSpotIdForBarrenZoneHint with a null hint.");

            BarrenHint barrenHint = hint as BarrenHint;
            if (barrenHint != null)
            {
                AreaId areaId = barrenHint.areaId;
                if (areaId.type == AreaId.AreaType.Zone)
                {
                    Zone zone = ZoneUtils.StringToId(areaId.stringId);
                    if (zone == Zone.Invalid)
                        throw new Exception(
                            $"Expected to be able to parse stringId '{areaId.stringId}' to a valid zone."
                        );

                    return ZoneUtils.IdToSpotId(zone);
                }
            }
            return SpotId.Invalid;
        }

        public static List<string> GetTradeChainAllChecks(string srcCheckName)
        {
            if (!CheckIdClass.IsValidCheckName(srcCheckName))
                throw new Exception($"'{srcCheckName}' is not a valid checkName.");

            string currCheckName = srcCheckName;
            Item currItem = getCheckContents(srcCheckName);
            if (!tradeItemToRewardCheck.ContainsKey(currItem))
            {
                throw new Exception(
                    $"Cannot check trade chain on non-tradeItem '{currItem}' from checkName '{currCheckName}'."
                );
            }

            List<string> checks = new();
            HashSet<Item> seenItems = new();
            checks.Add(currCheckName);
            seenItems.Add(currItem);

            while (tradeItemToRewardCheck.ContainsKey(currItem))
            {
                currCheckName = tradeItemToRewardCheck[currItem];
                currItem = getCheckContents(currCheckName);
                // If there is a circular chain, we return null. For example,
                // MaleMantis => FemaleButterfly => MaleMantis.
                if (seenItems.Contains(currItem))
                    return null;
                seenItems.Add(currItem);
                checks.Add(currCheckName);
            }

            return checks;
        }

        public static string GetTradeChainFinalCheck(string srcCheckName)
        {
            List<string> chainChecks = GetTradeChainAllChecks(srcCheckName);
            if (!ListUtils.isEmpty(chainChecks))
                return chainChecks[chainChecks.Count - 1];
            return null;
        }

        public static string GetTradeChainFinalCheck(
            string srcCheckName,
            Dictionary<int, int> itemPlacements
        )
        {
            if (!CheckIdClass.IsValidCheckName(srcCheckName))
                throw new Exception($"'{srcCheckName}' is not a valid checkName.");

            string currCheckName = srcCheckName;
            Item currItem = getCheckContents(srcCheckName, itemPlacements);
            while (tradeItemToRewardCheck.ContainsKey(currItem))
            {
                currCheckName = tradeItemToRewardCheck[currItem];
                currItem = getCheckContents(currCheckName, itemPlacements);
            }

            return currCheckName;
        }

        public static bool TradeChainContainsItem(string startCheckName, HashSet<Item> items)
        {
            if (ListUtils.isEmpty(items))
                return false;

            List<string> chainChecks = GetTradeChainAllChecks(startCheckName);
            if (!ListUtils.isEmpty(chainChecks))
            {
                foreach (string checkName in chainChecks)
                {
                    Item item = getCheckContents(checkName);
                    if (items.Contains(item))
                        return true;
                }
            }
            return false;
        }

        public static bool IsTradeItem(Item item)
        {
            return tradeItemToRewardCheck.ContainsKey(item);
        }

        public static bool CheckIsTradeItemReward(string checkName)
        {
            return tradeRewardCheckToSourceItem.ContainsKey(checkName);
        }

        public static bool IsTrapItem(Item item)
        {
            switch (item)
            {
                case Item.Foolish_Item:
                    return true;
                default:
                    return false;
            }
        }

        private static byte calcNumBitsForHintsAtSpot(List<HintSpot> hintSpots)
        {
            if (hintSpots != null && hintSpots.Count > 0)
            {
                int mostHintsPerSpot = 0;
                foreach (HintSpot spot in hintSpots)
                {
                    if (spot != null && spot.hints != null && spot.hints.Count > mostHintsPerSpot)
                        mostHintsPerSpot = spot.hints.Count;
                }

                if (mostHintsPerSpot > 0)
                    return GetBitsNeededForNum(mostHintsPerSpot);
            }

            return 1;
        }

        private static byte GetBitsNeededForNum(int num)
        {
            for (byte i = 1; i <= 16; i++)
            {
                int oneOverMax = 1 << i;
                if (num < oneOverMax)
                {
                    return i;
                }
            }
            return 32;
        }

        public static HintEncodingBitLengths GetHintEncodingBitLengths(List<HintSpot> hintSpots)
        {
            return new(
                HintTypeUtils.NumBitsToEncode,
                SeedGenResults.checkIDBitLength,
                ZoneUtils.NumBitsToEncode,
                HintCategoryUtils.NumBitsToEncode,
                AreaId.NumBitsToEncode,
                ProvinceUtils.NumBitsToEncode,
                HintSpotLocationUtils.NumBitsToEncode,
                GoalConstants.NumBitsToEncode,
                TradeGroupUtils.NumBitsToEncode,
                calcNumBitsForHintsAtSpot(hintSpots)
            );
        }

        public static bool CalcBeatableWithForbiddenChecks(
            Room startingRoom,
            HashSet<string> forbiddenCheckNames,
            HashSet<string> reachedChecks = null
        )
        {
            // I think it is safe to only generate the item pool once up front.
            Randomizer.Items.GenerateItemPool();

            Dictionary<string, Item> originalContentsMap = new();

            if (!ListUtils.isEmpty(forbiddenCheckNames))
            {
                foreach (string checkName in forbiddenCheckNames)
                {
                    // Replace check contents with a green rupee. We are checking if the playthrough
                    // is still beatable without doing any of the forbidden checks essentially (we
                    // do them in the playthrough, but they give junk).
                    Item originalContents = Randomizer.Checks.CheckDict[checkName].itemId;
                    Randomizer.Checks.CheckDict[checkName].itemId = Item.Green_Rupee;

                    originalContentsMap[checkName] = originalContents;
                }
            }

            Dictionary<Goal, List<Goal>> goals =
                new()
                {
                    {
                        GoalConstants.Ganondorf,
                        new() { GoalConstants.Ganondorf }
                    }
                };

            Dictionary<Goal, bool> goalResults = BackendFunctions.emulatePlaythrough2(
                startingRoom,
                goals,
                false,
                reachedChecks: reachedChecks
            );

            foreach (KeyValuePair<string, Item> pair in originalContentsMap)
            {
                // Put the original item back.
                Randomizer.Checks.CheckDict[pair.Key].itemId = pair.Value;
            }

            bool couldBeatGanondorf = goalResults[GoalConstants.Ganondorf];
            return couldBeatGanondorf;
        }
    }
}
