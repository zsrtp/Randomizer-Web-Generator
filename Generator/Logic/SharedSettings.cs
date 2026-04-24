using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TPRandomizer.SSettings.Enums;
using TPRandomizer.Util;

namespace TPRandomizer
{
    /// <summary>
    /// These are "sSettings".
    /// </summary>
    public class SharedSettings
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
        public PoeSettings shufflePoes { get; set; }
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
        public WalletSize walletSize { get; set; }
        public bool autoFillWallet { get; set; }
        public bool modifyShopModels { get; set; }
        public TrapFrequency trapFrequency { get; set; }
        public bool barrenDungeons { get; set; }
        public GoronMinesEntrance goronMinesEntrance { get; set; }
        public bool skipLakebedEntrance { get; set; }
        public bool skipArbitersEntrance { get; set; }
        public bool skipSnowpeakEntrance { get; set; }
        public bool skipGroveEntrance { get; set; }
        public TotEntrance totEntrance { get; set; }
        public bool skipCityEntrance { get; set; }
        public bool instantText { get; set; }
        public bool openMap { get; set; }
        public ItemScarcity itemScarcity { get; set; }
        public DamageMagnification damageMagnification { get; set; }
        public bool bonksDoDamage { get; set; }
        public bool shuffleRewards { get; set; }
        public bool skipMajorCutscenes { get; set; }
        public bool increaseSpinnerSpeed { get; set; }
        public bool openDot { get; set; }
        public bool noSmallKeysOnBosses { get; set; }
        public bool gmShortcut { get; set; }
        public bool hcShortcut { get; set; }
        public StartingToD startingToD { get; set; }
        public HintDistribution hintDistribution { get; set; }
        public bool randomizeStartingPoint { get; set; }
        public bool shuffleHiddenRupees { get; set; }
        public IliaQuest iliaQuest { get; set; }
        public MirrorChamberEntrance mirrorChamberEntrance { get; set; }
        public DungeonER shuffleDungeonEntrances { get; set; }
        public bool unpairEntrances { get; set; }
        public bool decoupleEntrances { get; set; }
        public bool shuffleFreestandingRupees { get; set; }
        public int castleRequirementCount { get; set; }
        public CastleBKRequirements castleBKRequirements { get; set; }
        public int castleBKRequirementCount { get; set; }
        public bool skipBridgeDonation { get; set; }
        public int maloShopDonation { get; set; }
        public HintImportance hintImportance { get; set; }
        public bool noPlandoHints { get; set; }
        public bool adjustHintsForCompletionists { get; set; }
        public bool hintDungeonEntrances { get; set; }
        public List<Item> startingItems { get; set; }
        public List<string> excludedChecks { get; set; }
        public List<(string, Item)> plandoChecks { get; set; }

        public SharedSettings() { }

        private SharedSettings(UInt32 version, string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            logicRules = (LogicRules)processor.NextInt(2);
            castleRequirements = (CastleRequirements)processor.NextInt(3);
            palaceRequirements = (PalaceRequirements)processor.NextInt(2);
            faronWoodsLogic = (FaronWoodsLogic)processor.NextInt(1);
            shuffleGoldenBugs = processor.NextBool();
            shuffleSkyCharacters = processor.NextBool();
            shuffleNpcItems = processor.NextBool();
            shufflePoes = (PoeSettings)processor.NextInt(2);
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
            walletSize = (WalletSize)processor.NextInt(2);
            modifyShopModels = processor.NextBool();
            trapFrequency = (TrapFrequency)processor.NextInt(3);
            barrenDungeons = processor.NextBool();
            goronMinesEntrance = (GoronMinesEntrance)processor.NextInt(2);
            skipLakebedEntrance = processor.NextBool();
            skipArbitersEntrance = processor.NextBool();
            skipSnowpeakEntrance = processor.NextBool();
            skipGroveEntrance = processor.NextBool();
            totEntrance = (TotEntrance)processor.NextInt(3);
            skipCityEntrance = processor.NextBool();
            instantText = processor.NextBool();
            openMap = processor.NextBool();
            increaseSpinnerSpeed = processor.NextBool();
            openDot = processor.NextBool();
            itemScarcity = (ItemScarcity)processor.NextInt(2);
            damageMagnification = (DamageMagnification)processor.NextInt(3);
            bonksDoDamage = processor.NextBool();
            shuffleRewards = processor.NextBool();
            skipMajorCutscenes = processor.NextBool();
            noSmallKeysOnBosses = processor.NextBool();
            startingToD = (StartingToD)processor.NextInt(3);
            hintDistribution = (HintDistribution)processor.NextInt(5);
            randomizeStartingPoint = processor.NextBool();
            shuffleHiddenRupees = processor.NextBool();
            gmShortcut = processor.NextBool();
            hcShortcut = processor.NextBool();
            iliaQuest = (IliaQuest)processor.NextInt(3);
            mirrorChamberEntrance = (MirrorChamberEntrance)processor.NextInt(2);
            shuffleDungeonEntrances = (DungeonER)processor.NextInt(2);
            unpairEntrances = processor.NextBool();
            decoupleEntrances = processor.NextBool();
            shuffleFreestandingRupees = processor.NextBool();
            castleRequirementCount = processor.NextInt(6);
            castleBKRequirements = (CastleBKRequirements)processor.NextInt(3);
            castleBKRequirementCount = processor.NextInt(6);
            autoFillWallet = processor.NextBool();
            skipBridgeDonation = processor.NextBool();
            maloShopDonation = processor.NextInt(11);
            hintImportance = (HintImportance)processor.NextInt(2);
            noPlandoHints = processor.NextBool();
            adjustHintsForCompletionists = processor.NextBool();
            hintDungeonEntrances = processor.NextBool();
            // We sort these lists so that the order which the UI happens to
            // pass the data up does not affect anything.
            startingItems = processor.NextItemList();
            startingItems.Sort();
            excludedChecks = processor.NextExcludedChecksList();
            // StringComparer is needed because the default sort order is
            // different on Linux and Windows
            excludedChecks.Sort(StringComparer.Ordinal);

            bool hasPlandoList = processor.NextBool();
            if (hasPlandoList)
            {
                plandoChecks = processor.NextPlandoChecksList();
                // Sort by check name, using the same StringComparer as excludedChecks
                plandoChecks = plandoChecks.OrderBy(i => i.Item1, StringComparer.Ordinal).ToList();
            }
            else
                plandoChecks = new();
        }

        // Note: this function MUST be able to parse old versions of sSettings
        // strings which are read from the `input.json` files.
        public static SharedSettings FromString(string settingsString)
        {
            if (settingsString == null)
            {
                throw new Exception("sSettings string is null.");
            }

            Regex regex = new Regex(@"^([0-9a-fA-F]+)s[0-9a-zA-Z-_]+");
            Match match = regex.Match(settingsString);

            if (!match.Success || match.Groups.Count < 2)
            {
                throw new Exception("Unable to decode sSettings string.");
            }

            string versionHexStr = match.Groups[1].Value;
            UInt32 version = Convert.ToUInt32(versionHexStr, 16);

            // This is actually only 6 bits.
            int lengthVal = SettingsEncoder.DecodeToInt(
                settingsString.Substring(versionHexStr.Length + 1, 1)
            );

            int lengthDefCharCount = lengthVal & 0b111;
            int numExtraBits = (lengthVal >> 3) & 0b111;

            int numChars = SettingsEncoder.DecodeToInt(
                settingsString.Substring(versionHexStr.Length + 2, lengthDefCharCount)
            );

            string bits = SettingsEncoder.DecodeToBitString(
                settingsString.Substring(versionHexStr.Length + 2 + lengthDefCharCount, numChars)
            );

            if (numExtraBits > 0)
            {
                bits = bits.Substring(0, bits.Length - (6 - numExtraBits));
            }

            return new SharedSettings(version, bits);
        }
    }
}
