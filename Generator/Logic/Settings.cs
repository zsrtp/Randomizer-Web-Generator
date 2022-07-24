using System;
using System.Collections.Generic;
using TPRandomizer.Util;
using TPRandomizer.SSettings.Enums;

namespace TPRandomizer
{
    /// <summary>
    /// summary text.
    /// </summary>
    public class RandomizerSetting
    {
        public LogicRules logicRules { get; set; }
        public CastleRequirements castleRequirements { get; set; }
        public PalaceRequirements palaceRequirements { get; set; }
        public FaronWoodsLogic faronWoodsLogic { get; set; }
        public bool mdhSkipped { get; set; }
        public bool prologueSkipped { get; set; }
        public SmallKeySettings smallKeySettings { get; set; }
        public string bossKeySettings { get; set; }
        public string mapAndCompassSettings { get; set; }
        public bool goldenBugsShuffled { get; set; }
        public bool poesShuffled { get; set; }
        public bool npcItemsShuffled { get; set; }
        public bool shopItemsShuffled { get; set; }
        public bool faronTwilightCleared { get; set; }
        public bool eldinTwilightCleared { get; set; }
        public bool lanayruTwilightCleared { get; set; }
        public bool skipMinorCutscenes { get; set; }
        public bool fastIronBoots { get; set; }
        public bool quickTransform { get; set; }
        public bool transformAnywhere { get; set; }
        public string iceTrapSettings { get; set; }
        public List<Item> StartingItems { get; set; }
        public List<string> ExcludedChecks { get; set; }
        public bool shuffleHiddenSkills { get; set; }
        public bool shuffleSkyCharacters { get; set; }
        public bool increaseWallet { get; set; }
        public bool modifyShopModels { get; set; }
        public bool barrenDungeons { get; set; }
        public bool skipMinesEntrance { get; set; }
        public bool skipLakebedEntrance { get; set; }
        public bool skipArbitersEntrance { get; set; }
        public bool skipSnowpeakEntrance { get; set; }
        public bool skipToTEntrance { get; set; }
        public bool skipCityEntrance { get; set; }

        public RandomizerSetting() { }

        private RandomizerSetting(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            logicRules = (LogicRules)processor.NextInt(2);
            castleRequirements = (CastleRequirements)processor.NextInt(3);
            palaceRequirements = (PalaceRequirements)processor.NextInt(2);
            faronWoodsLogic = (FaronWoodsLogic)processor.NextInt(1);
            goldenBugsShuffled = processor.NextBool();
            shuffleSkyCharacters = processor.NextBool();
            npcItemsShuffled = processor.NextBool();
            poesShuffled = processor.NextBool();
            shopItemsShuffled = processor.NextBool();
            shuffleHiddenSkills = processor.NextBool();
            smallKeySettings = (SmallKeySettings)processor.NextInt(3);
            bossKeySettings = processor.NextString(SettingData.bossKeySettings, 3);
            mapAndCompassSettings = processor.NextString(SettingData.mapAndCompassSettings, 3);
            prologueSkipped = processor.NextBool();
            faronTwilightCleared = processor.NextBool();
            eldinTwilightCleared = processor.NextBool();
            lanayruTwilightCleared = processor.NextBool();
            mdhSkipped = processor.NextBool();
            skipMinorCutscenes = processor.NextBool();
            fastIronBoots = processor.NextBool();
            quickTransform = processor.NextBool();
            transformAnywhere = processor.NextBool();
            increaseWallet = processor.NextBool();
            modifyShopModels = processor.NextBool();
            iceTrapSettings = processor.NextString(SettingData.iceTrapSettings, 3);
            barrenDungeons = processor.NextBool();
            skipMinesEntrance = processor.NextBool();
            skipLakebedEntrance = processor.NextBool();
            skipArbitersEntrance = processor.NextBool();
            skipSnowpeakEntrance = processor.NextBool();
            skipToTEntrance = processor.NextBool();
            skipCityEntrance = processor.NextBool();
            StartingItems = processor.NextItemList();
            ExcludedChecks = processor.NextExcludedChecksList();
        }

        public static RandomizerSetting FromString(string settingsString)
        {
            if (
                settingsString == null
                || !settingsString.StartsWith("0s")
                || settingsString.Length < 3
            )
            {
                throw new Exception("Unable to decode settingsString.");
            }

            // This is actually only 6 bits.
            int lengthVal = SettingsEncoder.DecodeToInt(settingsString.Substring(2, 1));

            int lengthDefCharCount = lengthVal & 0b111;
            int numExtraBits = (lengthVal >> 3) & 0b111;

            int numChars = SettingsEncoder.DecodeToInt(
                settingsString.Substring(3, lengthDefCharCount)
            );

            string bits = SettingsEncoder.DecodeToBitString(
                settingsString.Substring(3 + lengthDefCharCount, numChars)
            );

            if (numExtraBits > 0)
            {
                bits = bits.Substring(0, bits.Length - (6 - numExtraBits));
            }

            return new RandomizerSetting(bits);
        }
    }

    public class SettingData
    {
        public bool mdhSkipped { get; set; }

        public bool prologueSkipped { get; set; }

        public static string[] bossKeySettings { get; set; } =
            new string[] { "Vanilla", "Own_Dungeon", "Any_Dungeon", "Keysanity", "Keysey" };

        public static string[] mapAndCompassSettings { get; set; } =
            new string[] { "Vanilla", "Own_Dungeon", "Any_Dungeon", "Anywhere", "Start_With" };

        public bool goldenBugsShuffled { get; set; }

        public bool poesShuffled { get; set; }

        public bool npcItemsShuffled { get; set; }

        public bool shopItemsShuffled { get; set; }

        public bool faronTwilightCleared { get; set; }

        public bool eldinTwilightCleared { get; set; }

        public bool lanayruTwilightCleared { get; set; }

        public bool skipMinorCutscenes { get; set; }

        public bool fastIronBoots { get; set; }

        public bool quickTransform { get; set; }

        public bool transformAnywhere { get; set; }

        public static string[] iceTrapSettings { get; set; } =
            new string[] { "None", "Few", "Many", "Mayhem", "Nightmare" };

        public List<Item> StartingItems { get; set; }

        public List<string> ExcludedChecks { get; set; }

        public string[] TunicColor { get; set; } =
            new string[]
            {
                "Default",
                "Red",
                "Green",
                "Blue",
                "Yellow",
                "Purple",
                "Grey",
                "Black",
                "White",
                "Random",
            };

        public string[] MidnaHairColor { get; set; } =
            new string[] { "Default", "Red", "Blue", "Cyan" };

        public string[] lanternColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Orange",
                "Yellow",
                "Lime Green",
                "Dark Green",
                "Blue",
                "Purple",
                "Black",
                "White",
                "Cyan"
            };

        public string[] heartColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Rainbow",
                "Teal",
                "Pink",
                "Orange",
                "Blue",
                "Purple",
                "Green",
                "Black",
                "Mango",
                "Dragon Fruit"
            };

        public string[] aButtonColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Red",
                "Orange",
                "Yellow",
                "Dark Green",
                "Purple",
                "Black",
                "Grey",
                "Pink"
            };

        public string[] bButtonColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Orange",
                "Pink",
                "Green",
                "Blue",
                "Purple",
                "Black",
                "Teal"
            };

        public string[] xButtonColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Red",
                "Orange",
                "Yellow",
                "Lime Green",
                "Dark Green",
                "Blue",
                "Purple",
                "Black",
                "Pink",
                "Cyan"
            };

        public string[] yButtonColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Red",
                "Orange",
                "Yellow",
                "Lime Green",
                "Dark Green",
                "Blue",
                "Purple",
                "Black",
                "Pink",
                "Cyan"
            };

        public string[] zButtonColor { get; set; } =
            new string[]
            {
                "Default",
                "Random",
                "Red",
                "Orange",
                "Yellow",
                "Lime Green",
                "Dark Green",
                "Purple",
                "Black",
                "Light Blue"
            };

        public int backgroundMusicSetting { get; set; }

        public bool shuffleItemFanfares { get; set; }

        public bool disableEnemyBackgoundMusic { get; set; }

        public string[] gameRegion { get; set; } = new string[] { "NTSC", "PAL", "JAP" };

        public bool shuffleHiddenSkills { get; set; }

        public bool shuffleSkyCharacters { get; set; }

        public string[] seedNumber { get; set; } =
            new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public bool increaseWallet { get; set; }

        public bool modifyShopModels { get; set; }
        public bool barrenDungeons { get; set; }
        public bool skipMinesEntrance { get; set; }
        public bool skipLakebedEntrance { get; set; }
        public bool skipArbitersEntrance { get; set; }
        public bool skipSnowpeakEntrance { get; set; }
        public bool skipToTEntrance { get; set; }
        public bool skipCityEntrance { get; set; }
    }
}
