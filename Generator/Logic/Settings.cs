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
        // WARNING: Certain properties of this class are referenced by name in
        // the logic json files. To rename a property, you must check with those
        // files and update both this file and any json files as needed.
        public LogicRules logicRules { get; set; }
        public CastleRequirements castleRequirements { get; set; }
        public PalaceRequirements palaceRequirements { get; set; }
        public FaronWoodsLogic faronWoodsLogic { get; set; }
        public bool shuffleGoldenBugs { get; set; }
        public bool shuffleSkyCharacters { get; set; }
        public bool shuffleNpcItems { get; set; }
        public bool shufflePoes { get; set; }
        public bool shuffleShopItems { get; set; }
        public bool shuffleHiddenSkills { get; set; }
        public SmallKeySettings smallKeySettings { get; set; }
        public BigKeySettings bigKeySettings { get; set; }
        public MapAndCompassSettings mapAndCompassSettings { get; set; }
        public bool skipPrologue { get; set; }
        public bool faronTwilightCleared { get; set; }
        public bool eldinTwilightCleared { get; set; }
        public bool lanayruTwilightCleared { get; set; }
        public bool skipMdh { get; set; }
        public bool skipMinorCutscenes { get; set; }
        public bool fastIronBoots { get; set; }
        public bool quickTransform { get; set; }
        public bool transformAnywhere { get; set; }
        public bool increaseWallet { get; set; }
        public bool modifyShopModels { get; set; }
        public TrapFrequency trapFrequency { get; set; }
        public bool barrenDungeons { get; set; }
        public bool skipMinesEntrance { get; set; }
        public bool skipLakebedEntrance { get; set; }
        public bool skipArbitersEntrance { get; set; }
        public bool skipSnowpeakEntrance { get; set; }
        public bool skipToTEntrance { get; set; }
        public bool skipCityEntrance { get; set; }
        public List<Item> startingItems { get; set; }
        public List<string> excludedChecks { get; set; }

        public RandomizerSetting() { }

        private RandomizerSetting(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            logicRules = (LogicRules)processor.NextInt(2);
            castleRequirements = (CastleRequirements)processor.NextInt(3);
            palaceRequirements = (PalaceRequirements)processor.NextInt(2);
            faronWoodsLogic = (FaronWoodsLogic)processor.NextInt(1);
            shuffleGoldenBugs = processor.NextBool();
            shuffleSkyCharacters = processor.NextBool();
            shuffleNpcItems = processor.NextBool();
            shufflePoes = processor.NextBool();
            shuffleShopItems = processor.NextBool();
            shuffleHiddenSkills = processor.NextBool();
            smallKeySettings = (SmallKeySettings)processor.NextInt(3);
            bigKeySettings = (BigKeySettings)processor.NextInt(3);
            mapAndCompassSettings = (MapAndCompassSettings)processor.NextInt(3);
            skipPrologue = processor.NextBool();
            faronTwilightCleared = processor.NextBool();
            eldinTwilightCleared = processor.NextBool();
            lanayruTwilightCleared = processor.NextBool();
            skipMdh = processor.NextBool();
            skipMinorCutscenes = processor.NextBool();
            fastIronBoots = processor.NextBool();
            quickTransform = processor.NextBool();
            transformAnywhere = processor.NextBool();
            increaseWallet = processor.NextBool();
            modifyShopModels = processor.NextBool();
            trapFrequency = (TrapFrequency)processor.NextInt(3);
            barrenDungeons = processor.NextBool();
            skipMinesEntrance = processor.NextBool();
            skipLakebedEntrance = processor.NextBool();
            skipArbitersEntrance = processor.NextBool();
            skipSnowpeakEntrance = processor.NextBool();
            skipToTEntrance = processor.NextBool();
            skipCityEntrance = processor.NextBool();
            startingItems = processor.NextItemList();
            excludedChecks = processor.NextExcludedChecksList();
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
