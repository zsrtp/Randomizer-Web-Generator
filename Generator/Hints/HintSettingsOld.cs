// namespace TPRandomizer.Hints
// {
//     using System.Collections.Generic;
//     using TPRandomizer.SSettings.Enums;

//     public enum HintZoneDefinition
//     {
//         Isaac = 0,
//         FrenchCommunity = 1,
//     }

//     public class HintSettingsOld
//     {
//         public HintDistribution hintDistribution { get; private set; } = HintDistribution.Isaac;
//         public bool enabled { get; private set; }
//         public bool showInWebsite { get; private set; }
//         public bool showInGame { get; private set; }
//         public HintZoneDefinition hintZoneDefinition { get; private set; }
//         public int spolHintCount { get; private set; }
//         public int barrenHintCount { get; private set; }
//         public List<string> alwaysHints { get; private set; } = new();
//         public HashSet<string> sometimesHintPool { get; private set; } = new();
//         public int sometimesHintCount { get; private set; }
//         public bool agithaRewardsHintsEnabled { get; private set; }
//         public bool simulateInGameHints { get; private set; }

//         public static HintSettingsOld NewIsaacHintSettings(bool simulateInGameHints)
//         {
//             if (simulateInGameHints)
//             {
//                 return NewIsaacSimulatedHintSettings();
//             }

//             HintSettingsOld hintSettings = new HintSettingsOld();

//             hintSettings.hintDistribution = HintDistribution.Isaac;
//             hintSettings.enabled = true;
//             hintSettings.showInWebsite = true;
//             hintSettings.showInGame = false;
//             hintSettings.hintZoneDefinition = HintZoneDefinition.Isaac;

//             hintSettings.spolHintCount = 2;
//             hintSettings.barrenHintCount = 2;

//             // Always hints
//             hintSettings.alwaysHints.Add("Goron Springwater Rush");
//             hintSettings.alwaysHints.Add("Iza Helping Hand");
//             hintSettings.alwaysHints.Add("Lanayru Ice Block Puzzle Cave Chest");
//             hintSettings.alwaysHints.Add("Snowpeak Icy Summit Poe");

//             // Sometimes hints
//             hintSettings.sometimesHintCount = 4;
//             hintSettings.sometimesHintPool.Add("Herding Goats Reward");
//             hintSettings.sometimesHintPool.Add("Wrestling With Bo");
//             hintSettings.sometimesHintPool.Add("Eldin Field Bomb Rock Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Spring Underwater Chest");
//             hintSettings.sometimesHintPool.Add("Death Mountain Alcove Chest");
//             hintSettings.sometimesHintPool.Add("Jovani 20 Poe Soul Reward");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Shell Blade Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Behind Gate Underwater Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Spinner Track Chest");
//             hintSettings.sometimesHintPool.Add("Plumm Fruit Balloon Minigame");
//             hintSettings.sometimesHintPool.Add("Zoras Domain Underwater Goron");
//             hintSettings.sometimesHintPool.Add("Fishing Hole Bottle");
//             hintSettings.sometimesHintPool.Add("Forest Temple Gale Boomerang");
//             hintSettings.sometimesHintPool.Add("Arbiters Grounds Death Sword Chest");
//             hintSettings.sometimesHintPool.Add("Snowboard Racing Prize");
//             hintSettings.sometimesHintPool.Add("Snowpeak Ruins Chapel Chest");
//             hintSettings.sometimesHintPool.Add("City in The Sky Aeralfos Chest");

//             hintSettings.agithaRewardsHintsEnabled = true;

//             hintSettings.simulateInGameHints = false;

//             return hintSettings;
//         }

//         private static HintSettingsOld NewIsaacSimulatedHintSettings()
//         {
//             HintSettingsOld hintSettings = new HintSettingsOld();

//             hintSettings.hintDistribution = HintDistribution.Isaac;
//             hintSettings.enabled = true;
//             hintSettings.showInWebsite = true;
//             hintSettings.showInGame = false;
//             hintSettings.hintZoneDefinition = HintZoneDefinition.Isaac;

//             hintSettings.spolHintCount = 2;
//             hintSettings.barrenHintCount = 2;

//             // Always hints
//             hintSettings.alwaysHints.Add("Goron Springwater Rush");
//             hintSettings.alwaysHints.Add("Iza Helping Hand");
//             hintSettings.alwaysHints.Add("Lanayru Ice Block Puzzle Cave Chest");
//             hintSettings.alwaysHints.Add("Snowpeak Icy Summit Poe");

//             // Sometimes hints
//             hintSettings.sometimesHintCount = 4;

//             hintSettings.sometimesHintPool.Add("Herding Goats Reward");
//             hintSettings.sometimesHintPool.Add("Links Basement Chest");
//             hintSettings.sometimesHintPool.Add("Ordon Ranch Grotto Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Wrestling With Bo");
//             hintSettings.sometimesHintPool.Add("Lost Woods Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Sacred Grove Baba Serpent Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Sacred Grove Spinner Chest");
//             hintSettings.sometimesHintPool.Add("Faron Field Bridge Chest");
//             hintSettings.sometimesHintPool.Add("Faron Woods Owl Statue Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Lantern Cave Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Kakariko Gorge Double Clawshot Chest");
//             hintSettings.sometimesHintPool.Add("Kakariko Gorge Owl Statue Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Spring Underwater Chest");
//             hintSettings.sometimesHintPool.Add("Kakariko Village Bomb Rock Spire Heart Piece");
//             hintSettings.sometimesHintPool.Add("Kakariko Village Malo Mart Hawkeye");
//             hintSettings.sometimesHintPool.Add("Kakariko Watchtower Alcove Chest");
//             hintSettings.sometimesHintPool.Add("Talo Sharpshooting");
//             hintSettings.sometimesHintPool.Add("Gift From Ralis");
//             hintSettings.sometimesHintPool.Add("Kakariko Graveyard Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Field Bomb Rock Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Field Bomskit Grotto Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Stockcave Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Death Mountain Alcove Chest");
//             hintSettings.sometimesHintPool.Add("Death Mountain Trail Poe");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Behind Gate Underwater Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Skulltula Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Spinner Track Chest");
//             hintSettings.sometimesHintPool.Add("Hyrule Field Amphitheater Owl Statue Chest");
//             hintSettings.sometimesHintPool.Add(
//                 "Outside South Castle Town Double Clawshot Chasm Chest"
//             );
//             hintSettings.sometimesHintPool.Add("Outside South Castle Town Fountain Chest");
//             hintSettings.sometimesHintPool.Add("Outside South Castle Town Tightrope Chest");
//             hintSettings.sometimesHintPool.Add("West Hyrule Field Helmasaur Grotto Chest");
//             hintSettings.sometimesHintPool.Add("STAR Prize 2");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Bridge Owl Statue Chest");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Bridge Vines Chest");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Shell Blade Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Underwater Chest");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Water Toadpoli Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Plumm Fruit Balloon Minigame");
//             hintSettings.sometimesHintPool.Add("Lanayru Spring Back Room Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Zoras Domain Extinguish All Torches Chest");
//             hintSettings.sometimesHintPool.Add("Zoras Domain Light All Torches Chest");
//             hintSettings.sometimesHintPool.Add("Zoras Domain Underwater Goron");
//             hintSettings.sometimesHintPool.Add("Fishing Hole Bottle");
//             hintSettings.sometimesHintPool.Add("Gerudo Desert Owl Statue Chest");
//             hintSettings.sometimesHintPool.Add("Gerudo Desert Rock Grotto Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Outside Arbiters Grounds Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Snowboard Racing Prize");
//             hintSettings.sometimesHintPool.Add("Snowpeak Cave Ice Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Forest Temple Diababa Heart Container");
//             hintSettings.sometimesHintPool.Add("Forest Temple Gale Boomerang");
//             hintSettings.sometimesHintPool.Add("Goron Mines Fyrus Heart Container");
//             hintSettings.sometimesHintPool.Add("Lakebed Temple Morpheel Heart Container");
//             hintSettings.sometimesHintPool.Add("Arbiters Grounds Death Sword Chest");
//             hintSettings.sometimesHintPool.Add("Snowpeak Ruins Blizzeta Heart Container");
//             hintSettings.sometimesHintPool.Add("Snowpeak Ruins Chapel Chest");
//             hintSettings.sometimesHintPool.Add("Temple of Time Armogohma Heart Container");
//             hintSettings.sometimesHintPool.Add("Temple of Time Lobby Lantern Chest");
//             hintSettings.sometimesHintPool.Add("City in The Sky Aeralfos Chest");
//             hintSettings.sometimesHintPool.Add("City in The Sky Argorok Heart Container");
//             hintSettings.sometimesHintPool.Add("City in The Sky West Wing First Chest");

//             hintSettings.agithaRewardsHintsEnabled = true;

//             hintSettings.simulateInGameHints = true;

//             return hintSettings;
//         }

//         public static HintSettingsOld NewFrenchCommunityHintSettings(bool simulateInGameHints)
//         {
//             HintSettingsOld hintSettings = new HintSettingsOld();

//             hintSettings.hintDistribution = HintDistribution.FrenchCommunity;
//             hintSettings.enabled = true;
//             hintSettings.showInWebsite = true;
//             hintSettings.showInGame = false;
//             hintSettings.hintZoneDefinition = HintZoneDefinition.FrenchCommunity;

//             hintSettings.spolHintCount = 3;
//             hintSettings.barrenHintCount = 3;

//             // Always hints
//             hintSettings.alwaysHints.Add("Goron Springwater Rush");
//             hintSettings.alwaysHints.Add("Iza Helping Hand");
//             hintSettings.alwaysHints.Add("Jovani 20 Poe Soul Reward");
//             hintSettings.alwaysHints.Add("Lanayru Ice Block Puzzle Cave Chest");

//             // Sometimes hints
//             hintSettings.sometimesHintCount = 5;
//             hintSettings.sometimesHintPool.Add("Outside Arbiters Grounds Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Charlo Donation Blessing");
//             hintSettings.sometimesHintPool.Add("STAR Prize 2");
//             hintSettings.sometimesHintPool.Add("Death Mountain Alcove Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Field Bomb Rock Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Field Bomskit Grotto Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Faron Field Bridge Chest");
//             hintSettings.sometimesHintPool.Add("Gerudo Desert Rock Grotto Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Gerudo Desert West Canyon Chest");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Bridge Vines Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Lantern Cave Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Gift From Ralis");
//             hintSettings.sometimesHintPool.Add("Kakariko Graveyard Lantern Chest");
//             // These 2 are written as a double hint. Not dealing with that right now.
//             // hintSettings.sometimesHintPool.Add("Talo Sharpshooting");
//             // hintSettings.sometimesHintPool.Add("Kakariko Village Malo Mart Hawkeye");
//             hintSettings.sometimesHintPool.Add("Kakariko Watchtower Alcove Chest");
//             hintSettings.sometimesHintPool.Add("Auru Gift To Fyer");
//             hintSettings.sometimesHintPool.Add("Lake Hylia Shell Blade Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Behind Gate Underwater Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Skulltula Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Spring Back Room Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Eldin Stockcave Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Herding Goats Reward");
//             hintSettings.sometimesHintPool.Add("Links Basement Chest");
//             hintSettings.sometimesHintPool.Add("Ordon Ranch Grotto Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Wrestling With Bo");
//             hintSettings.sometimesHintPool.Add("Outside South Castle Town Fountain Chest");
//             hintSettings.sometimesHintPool.Add("Outside South Castle Town Tightrope Chest");
//             hintSettings.sometimesHintPool.Add("Lanayru Field Spinner Track Chest");
//             hintSettings.sometimesHintPool.Add("Lost Woods Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Sacred Grove Past Owl Statue Chest");
//             hintSettings.sometimesHintPool.Add("Snowpeak Cave Ice Lantern Chest");
//             hintSettings.sometimesHintPool.Add("Snowpeak Freezard Grotto Chest");
//             hintSettings.sometimesHintPool.Add("Snowboard Racing Prize");
//             hintSettings.sometimesHintPool.Add("Fishing Hole Bottle");
//             hintSettings.sometimesHintPool.Add("Plumm Fruit Balloon Minigame");
//             hintSettings.sometimesHintPool.Add("Zoras Domain Underwater Goron");
//             // Dungeon Sometimes
//             hintSettings.sometimesHintPool.Add("Forest Temple West Tile Worm Chest Behind Stairs");
//             hintSettings.sometimesHintPool.Add("Forest Temple Gale Boomerang");
//             hintSettings.sometimesHintPool.Add("Goron Mines Outside Clawshot Chest");
//             // Lakebed Temple, BigKey Room and Before (double hint)
//             hintSettings.sometimesHintPool.Add("Arbiters Grounds North Turning Room Chest");
//             hintSettings.sometimesHintPool.Add("Arbiters Grounds Death Sword Chest");
//             // Snowpeak Ruins, Ball and Chain & After Darkhammer Chest (double hint)
//             hintSettings.sometimesHintPool.Add("Snowpeak Ruins Chapel Chest");
//             hintSettings.sometimesHintPool.Add("Temple of Time Darknut Chest");
//             // Temple Of Time, Big Key Chest Room (double hint)
//             hintSettings.sometimesHintPool.Add("City in The Sky Aeralfos Chest");
//             // City In The Sky, East Wing Ooccoo & Wind Room (double hint)
//             // Only when Poes are in pool
//             hintSettings.sometimesHintPool.Add("Snowpeak Icy Summit Poe");
//             hintSettings.sometimesHintPool.Add("Death Mountain Trail Poe");

//             hintSettings.agithaRewardsHintsEnabled = false;

//             hintSettings.simulateInGameHints = simulateInGameHints;

//             return hintSettings;
//         }
//     }
// }
