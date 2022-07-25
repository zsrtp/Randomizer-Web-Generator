namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using TPRandomizer.FcSettings.Enums;

    /// <summary>
    /// summary text.
    /// </summary>
    public class SeedData
    {
        // See <add_documentation_reference_here> for the flowchart for
        // determining if you should increment the major or minor version.
        public static readonly UInt16 VersionMajor = 1;
        public static readonly UInt16 VersionMinor = 0;

        // For convenience. This does not include any sort of leading 'v', so
        // add that where you use this variable if you need it.
        public static readonly string VersionString = $"{VersionMajor}.{VersionMinor}";

        private static readonly List<byte> CheckDataRaw = new();
        private static readonly List<byte> BannerDataRaw = new();
        private static readonly SeedHeader SeedHeaderRaw = new();
        private static readonly short SeedHeaderSize = 0x160;
        private static readonly byte BgmHeaderSize = 0xC;

        private SeedGenResults seedGenResults;
        public FileCreationSettings fcSettings { get; }
        public BgmHeader BgmHeaderRaw = new();

        private SeedData(SeedGenResults seedGenResults, FileCreationSettings fcSettings)
        {
            this.seedGenResults = seedGenResults;
            this.fcSettings = fcSettings;
        }

        public static byte[] GenerateSeedDataBytes(
            SeedGenResults seedGenResults,
            FileCreationSettings fcSettings
        )
        {
            SeedData seedData = new SeedData(seedGenResults, fcSettings);
            return seedData.GenerateSeedDataBytesInternal();
        }

        private byte[] GenerateSeedDataBytesInternal()
        {
            /*
            * General Note: offset sizes are handled as two bytes. Because of this,
            * any seed bigger than 7 blocks will not work with this method. The seed structure is as follows:
            * Seed Header
            * Seed Data
            * Check Data
            * Bgm Header
            * Bgm Data
            */
            SharedSettings randomizerSettings = Randomizer.SSettings;
            List<byte> currentGCIData = new();
            List<byte> currentSeedHeader = new();
            List<byte> currentSeedData = new();
            List<byte> currentBgmData = new();
            char[] gameRegions = { 'J', 'P', 'E' };

            // Raw Check Data
            CheckDataRaw.AddRange(GeneratePatchSettings());
            CheckDataRaw.AddRange(GenerateEventFlags());
            CheckDataRaw.AddRange(GenerateRegionFlags());
            CheckDataRaw.AddRange(ParseDZXReplacements());
            CheckDataRaw.AddRange(ParseRELOverrides());
            CheckDataRaw.AddRange(ParsePOEReplacements());
            CheckDataRaw.AddRange(ParseARCReplacements());
            CheckDataRaw.AddRange(ParseObjectARCReplacements());
            CheckDataRaw.AddRange(ParseBossReplacements());
            CheckDataRaw.AddRange(ParseHiddenSkills());
            CheckDataRaw.AddRange(ParseBugRewards());
            CheckDataRaw.AddRange(ParseSkyCharacters());
            CheckDataRaw.AddRange(ParseShopItems());
            CheckDataRaw.AddRange(ParseStartingItems());
            while (CheckDataRaw.Count % 0x10 != 0)
            {
                CheckDataRaw.Add(Converter.GcByte(0x0));
            }
            SeedHeaderRaw.bgmHeaderOffset = (UInt16)CheckDataRaw.Count();

            // BGM Table info
            BgmHeaderRaw.bgmTableOffset = (UInt16)BgmHeaderSize;
            currentBgmData.AddRange(SoundAssets.GenerateBgmData(this));
            BgmHeaderRaw.fanfareTableOffset = (UInt16)(BgmHeaderSize + currentBgmData.Count);
            currentBgmData.AddRange(SoundAssets.GenerateFanfareData(this));

            SeedHeaderRaw.totalSize = (uint)(
                SeedHeaderSize + CheckDataRaw.Count + BgmHeaderSize + currentBgmData.Count
            );

            // Generate Seed Data
            currentSeedHeader.AddRange(GenerateSeedHeader(fcSettings.seedNumber));
            currentSeedData.AddRange(currentSeedHeader);
            currentSeedData.AddRange(CheckDataRaw);
            currentSeedData.AddRange(GenerateBgmHeader());
            currentSeedData.AddRange(currentBgmData);
            while (currentSeedData.Count % 0x20 != 0)
            {
                currentSeedData.Add(Converter.GcByte(0x0));
            }

            // Generate Data file
            // File.WriteAllBytes(
            //     "rando-data" + randomizerSettings.seedNumber,
            //     currentSeedData.ToArray()
            // );

            char region;
            switch (fcSettings.gameRegion)
            {
                case GameRegion.PAL:
                {
                    region = 'P';
                    break;
                }
                case GameRegion.JAP:
                {
                    region = 'J';
                    break;
                }
                default:
                {
                    region = 'E';
                    break;
                }
            }

            // Add seed banner
            BannerDataRaw.AddRange(Properties.Resources.seedGciImageData);
            BannerDataRaw.AddRange(
                Converter.StringBytes($"TPR SeedData v{VersionString}", 0x20, region)
            );
            BannerDataRaw.AddRange(
                Converter.StringBytes(seedGenResults.playthroughName, 0x20, region)
            );
            // Generate GCI Files
            currentGCIData.AddRange(BannerDataRaw);
            currentGCIData.AddRange(currentSeedData);
            var gci = new Gci(
                (byte)fcSettings.seedNumber,
                region,
                currentGCIData,
                seedGenResults.playthroughName
            );
            return gci.gciFile.ToArray();
            // File.WriteAllBytes(playthroughName, gci.gciFile.ToArray());
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="seedNumber">The number you want to convert.</param>
        /// <param name="seedHash">A randomized string that represents the current seed.</param>
        /// <returns> The inserted value as a byte. </returns>
        private List<byte> GenerateSeedHeader(int seedNumber)
        {
            List<byte> seedHeader = new();
            SharedSettings randomizerSettings = Randomizer.SSettings;
            SeedHeaderRaw.headerSize = (ushort)SeedHeaderSize;
            SeedHeaderRaw.dataSize = (ushort)CheckDataRaw.Count;
            SeedHeaderRaw.seed = BackendFunctions.GetChecksum(seedGenResults.playthroughName, 64);
            SeedHeaderRaw.versionMajor = VersionMajor;
            SeedHeaderRaw.versionMinor = VersionMinor;
            SeedHeaderRaw.requiredDungeons = (uint)seedGenResults.requiredDungeons;
            PropertyInfo[] seedHeaderProperties = SeedHeaderRaw.GetType().GetProperties();
            foreach (PropertyInfo headerObject in seedHeaderProperties)
            {
                if (headerObject.PropertyType == typeof(UInt32))
                {
                    seedHeader.AddRange(
                        Converter.GcBytes((UInt32)headerObject.GetValue(SeedHeaderRaw, null))
                    );
                }
                else if (headerObject.PropertyType == typeof(UInt64))
                {
                    seedHeader.AddRange(
                        Converter.GcBytes((UInt64)headerObject.GetValue(SeedHeaderRaw, null))
                    );
                }
                else if (headerObject.PropertyType == typeof(UInt16))
                {
                    seedHeader.AddRange(
                        Converter.GcBytes((UInt16)headerObject.GetValue(SeedHeaderRaw, null))
                    );
                }
            }

            seedHeader.Add(Converter.GcByte(fcSettings.heartColor));
            seedHeader.Add(Converter.GcByte(fcSettings.aBtnColor));
            seedHeader.Add(Converter.GcByte(fcSettings.bBtnColor));
            seedHeader.Add(Converter.GcByte(fcSettings.xBtnColor));
            seedHeader.Add(Converter.GcByte(fcSettings.yBtnColor));
            seedHeader.Add(Converter.GcByte(fcSettings.zBtnColor));
            seedHeader.Add(Converter.GcByte(fcSettings.lanternGlowColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.transformAnywhere ? 1 : 0));
            seedHeader.Add(Converter.GcByte(randomizerSettings.quickTransform ? 1 : 0));
            seedHeader.Add(Converter.GcByte((int)randomizerSettings.castleRequirements));
            seedHeader.Add(Converter.GcByte((int)randomizerSettings.palaceRequirements));
            while (seedHeader.Count < (SeedHeaderSize - 1))
            {
                seedHeader.Add((byte)0x0);
            }

            seedHeader.Add(Converter.GcByte(seedNumber));
            return seedHeader;
        }

        private List<byte> GenerateBgmHeader()
        {
            List<byte> bgmHeaderRaw = new();
            PropertyInfo[] bgmHeaderProperties = BgmHeaderRaw.GetType().GetProperties();
            foreach (PropertyInfo headerObject in bgmHeaderProperties)
            {
                if (headerObject.PropertyType == typeof(UInt32))
                {
                    bgmHeaderRaw.AddRange(
                        Converter.GcBytes((UInt32)headerObject.GetValue(BgmHeaderRaw, null))
                    );
                }
                else if (headerObject.PropertyType == typeof(UInt64))
                {
                    bgmHeaderRaw.AddRange(
                        Converter.GcBytes((UInt64)headerObject.GetValue(BgmHeaderRaw, null))
                    );
                }
                else if (headerObject.PropertyType == typeof(UInt16))
                {
                    bgmHeaderRaw.AddRange(
                        Converter.GcBytes((UInt16)headerObject.GetValue(BgmHeaderRaw, null))
                    );
                }
                else if (headerObject.PropertyType == typeof(byte))
                {
                    bgmHeaderRaw.Add(
                        Converter.GcByte((byte)headerObject.GetValue(BgmHeaderRaw, null))
                    );
                }
            }
            while (bgmHeaderRaw.Count % 0x4 != 0)
            {
                bgmHeaderRaw.Add(Converter.GcByte(0x0));
            }
            return bgmHeaderRaw;
        }

        private List<byte> GeneratePatchSettings()
        {
            SharedSettings randomizerSettings = Randomizer.SSettings;
            List<byte> listOfPatches = new();
            bool[] volatilePatchSettingsArray =
            {
                randomizerSettings.faronTwilightCleared,
                randomizerSettings.eldinTwilightCleared,
                randomizerSettings.lanayruTwilightCleared,
                randomizerSettings.skipMinorCutscenes,
                randomizerSettings.skipMdh,
            };
            bool[] oneTimePatchSettingsArray =
            {
                randomizerSettings.increaseWallet,
                randomizerSettings.fastIronBoots,
                randomizerSettings.modifyShopModels,
                fcSettings.disableEnemyBgm
            };
            int patchOptions = 0x0;
            int bitwiseOperator = 0;
            SeedHeaderRaw.volatilePatchInfoNumEntries = 1; // Start off at one to ensure alignment
            SeedHeaderRaw.oneTimePatchInfoNumEntries = 1; // Start off at one to ensure alignment
            for (int i = 0; i < volatilePatchSettingsArray.Length; i++)
            {
                if (((i % 8) == 0) && (i >= 8))
                {
                    SeedHeaderRaw.volatilePatchInfoNumEntries++;
                    listOfPatches.Add(Converter.GcByte(patchOptions));
                    patchOptions = 0;
                    bitwiseOperator = 0;
                }

                if (volatilePatchSettingsArray[i])
                {
                    patchOptions |= 0x80 >> bitwiseOperator;
                }

                bitwiseOperator++;
            }

            listOfPatches.Add(Converter.GcByte(patchOptions));
            SeedHeaderRaw.volatilePatchInfoDataOffset = (ushort)(CheckDataRaw.Count);
            SeedHeaderRaw.oneTimePatchInfoDataOffset = (ushort)(
                CheckDataRaw.Count + listOfPatches.Count
            );
            patchOptions = 0;
            bitwiseOperator = 0;
            for (int i = 0; i < oneTimePatchSettingsArray.Length; i++)
            {
                if (((i % 8) == 0) && (i >= 8))
                {
                    SeedHeaderRaw.oneTimePatchInfoNumEntries++;
                    listOfPatches.Add(Converter.GcByte(patchOptions));
                    patchOptions = 0;
                    bitwiseOperator = 0;
                }

                if (oneTimePatchSettingsArray[i])
                {
                    patchOptions |= 0x80 >> bitwiseOperator;
                }

                bitwiseOperator++;
            }
            listOfPatches.Add(Converter.GcByte(patchOptions));

            return listOfPatches;
        }

        private List<byte> ParseARCReplacements()
        {
            List<byte> listOfArcReplacements = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("ARC"))
                {
                    for (int i = 0; i < currentCheck.arcOffsets.Count; i++)
                    {
                        listOfArcReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)uint.Parse(
                                    currentCheck.arcOffsets[i],
                                    System.Globalization.NumberStyles.HexNumber
                                )
                            )
                        );
                        if (currentCheck.replacementType[i] != 3)
                        {
                            listOfArcReplacements.AddRange(
                                Converter.GcBytes((UInt32)currentCheck.itemId)
                            );
                        }
                        else
                        {
                            listOfArcReplacements.AddRange(
                                Converter.GcBytes(
                                    (UInt32)uint.Parse(
                                        currentCheck.flag,
                                        System.Globalization.NumberStyles.HexNumber
                                    )
                                )
                            );
                        }
                        listOfArcReplacements.Add(
                            Converter.GcByte(currentCheck.fileDirectoryType[i])
                        );
                        listOfArcReplacements.Add(
                            Converter.GcByte(currentCheck.replacementType[i])
                        );
                        listOfArcReplacements.Add(Converter.GcByte(currentCheck.stageIDX[i]));

                        if (currentCheck.fileDirectoryType[i] == 0)
                        {
                            listOfArcReplacements.Add(Converter.GcByte(currentCheck.roomIDX));
                        }
                        else
                        {
                            listOfArcReplacements.Add(Converter.GcByte(0x0));
                        }
                        count++;
                    }
                }
            }

            SeedHeaderRaw.arcCheckInfoNumEntries = count;
            SeedHeaderRaw.arcCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfArcReplacements;
        }

        private List<byte> ParseObjectARCReplacements()
        {
            List<byte> listOfArcReplacements = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("ObjectARC"))
                {
                    for (int i = 0; i < currentCheck.arcOffsets.Count; i++)
                    {
                        listOfArcReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)uint.Parse(
                                    currentCheck.arcOffsets[i],
                                    System.Globalization.NumberStyles.HexNumber
                                )
                            )
                        );
                        listOfArcReplacements.AddRange(
                            Converter.GcBytes((UInt32)currentCheck.itemId)
                        );
                        List<byte> fileNameBytes = new();
                        fileNameBytes.AddRange(Converter.StringBytes(currentCheck.fileName));
                        for (
                            int numberofFileNameBytes = fileNameBytes.Count;
                            numberofFileNameBytes < 15;
                            numberofFileNameBytes++
                        )
                        {
                            // Pad the length of the file name to 0x12 bytes.
                            fileNameBytes.Add(Converter.GcByte(0x00));
                        }
                        listOfArcReplacements.AddRange(fileNameBytes);
                        listOfArcReplacements.Add(Converter.GcByte(currentCheck.stageIDX[i]));
                        count++;
                    }
                }
            }

            SeedHeaderRaw.objectArcCheckInfoNumEntries = count;
            SeedHeaderRaw.objectArcCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfArcReplacements;
        }

        private List<byte> ParseDZXReplacements()
        {
            List<byte> listOfDZXReplacements = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("DZX"))
                {
                    // We will use the number of hashes to count DZX replacements per check for now.
                    for (int i = 0; i < currentCheck.hash.Count; i++)
                    {
                        byte[] dataArray = new byte[32];
                        for (int j = 0; j < currentCheck.actrData[i].Length; j++)
                        {
                            dataArray[j] = byte.Parse(
                                currentCheck.actrData[i][j],
                                System.Globalization.NumberStyles.HexNumber
                            );
                        }

                        if (currentCheck.dzxTag[i] == "TRES")
                        {
                            dataArray[28] = (byte)currentCheck.itemId;
                        }
                        else if (currentCheck.dzxTag[i] == "ACTR")
                        {
                            dataArray[11] = (byte)currentCheck.itemId;
                        }

                        listOfDZXReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt16)ushort.Parse(
                                    currentCheck.hash[i],
                                    System.Globalization.NumberStyles.HexNumber
                                )
                            )
                        );
                        listOfDZXReplacements.Add(Converter.GcByte(currentCheck.stageIDX[i]));
                        if (currentCheck.magicByte == null)
                        {
                            listOfDZXReplacements.Add(Converter.GcByte(0xFF)); // If a magic byte is not set, use 0xFF as a default.
                        }
                        else
                        {
                            listOfDZXReplacements.Add(
                                Converter.GcByte(
                                    byte.Parse(
                                        currentCheck.magicByte[i],
                                        System.Globalization.NumberStyles.HexNumber
                                    )
                                )
                            );
                        }

                        listOfDZXReplacements.AddRange(dataArray);
                        count++;
                    }
                }
            }

            SeedHeaderRaw.dzxCheckInfoNumEntries = count;
            SeedHeaderRaw.dzxCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfDZXReplacements;
        }

        private List<byte> ParsePOEReplacements()
        {
            List<byte> listOfPOEReplacements = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("Poe"))
                {
                    listOfPOEReplacements.Add(Converter.GcByte(currentCheck.stageIDX[0]));
                    listOfPOEReplacements.Add(
                        Converter.GcByte(
                            byte.Parse(
                                currentCheck.flag,
                                System.Globalization.NumberStyles.HexNumber
                            )
                        )
                    );
                    listOfPOEReplacements.AddRange(Converter.GcBytes((UInt16)currentCheck.itemId));
                    count++;
                }
            }

            SeedHeaderRaw.poeCheckInfoNumEntries = count;
            SeedHeaderRaw.poeCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfPOEReplacements;
        }

        private List<byte> ParseRELOverrides()
        {
            List<byte> listOfRELReplacements = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("REL"))
                {
                    for (int i = 0; i < currentCheck.moduleID.Count; i++)
                    {
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes((UInt32)currentCheck.stageIDX[i])
                        );
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)uint.Parse(
                                    currentCheck.moduleID[i],
                                    System.Globalization.NumberStyles.HexNumber
                                )
                            )
                        );
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)uint.Parse(
                                    currentCheck.relOffsets[i],
                                    System.Globalization.NumberStyles.HexNumber
                                )
                            )
                        );
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)(
                                    uint.Parse(
                                        currentCheck.relOverride[i],
                                        System.Globalization.NumberStyles.HexNumber
                                    ) + (byte)currentCheck.itemId
                                )
                            )
                        );
                        count++;
                    }
                }
            }

            SeedHeaderRaw.relCheckInfoNumEntries = count;
            SeedHeaderRaw.relCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfRELReplacements;
        }

        private List<byte> ParseBossReplacements()
        {
            List<byte> listOfBossReplacements = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("Boss"))
                {
                    listOfBossReplacements.AddRange(
                        Converter.GcBytes((UInt16)currentCheck.stageIDX[0])
                    );
                    listOfBossReplacements.AddRange(Converter.GcBytes((UInt16)currentCheck.itemId));
                    count++;
                }
            }

            SeedHeaderRaw.bossCheckInfoNumEntries = count;
            SeedHeaderRaw.bossCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfBossReplacements;
        }

        private List<byte> ParseBugRewards()
        {
            List<byte> listOfBugRewards = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("Bug Reward"))
                {
                    listOfBugRewards.AddRange(
                        Converter.GcBytes(
                            (UInt16)byte.Parse(
                                currentCheck.flag,
                                System.Globalization.NumberStyles.HexNumber
                            )
                        )
                    );
                    listOfBugRewards.AddRange(Converter.GcBytes((UInt16)(byte)currentCheck.itemId));
                    count++;
                }
            }

            SeedHeaderRaw.bugRewardCheckInfoNumEntries = count;
            SeedHeaderRaw.bugRewardCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfBugRewards;
        }

        private List<byte> ParseSkyCharacters()
        {
            List<byte> listOfSkyCharacters = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("Sky Book"))
                {
                    listOfSkyCharacters.Add(Converter.GcByte((byte)currentCheck.itemId));
                    listOfSkyCharacters.AddRange(
                        Converter.GcBytes((UInt16)currentCheck.stageIDX[0])
                    );
                    listOfSkyCharacters.Add(Converter.GcByte(currentCheck.roomIDX));
                    count++;
                }
            }

            SeedHeaderRaw.skyCharacterCheckInfoNumEntries = count;
            SeedHeaderRaw.skyCharacterCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfSkyCharacters;
        }

        private List<byte> ParseHiddenSkills()
        {
            List<byte> listOfHiddenSkills = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("Hidden Skill"))
                {
                    listOfHiddenSkills.AddRange(
                        Converter.GcBytes(
                            (UInt16)short.Parse(
                                currentCheck.flag,
                                System.Globalization.NumberStyles.HexNumber
                            )
                        )
                    );
                    listOfHiddenSkills.AddRange(Converter.GcBytes((UInt16)currentCheck.itemId));
                    listOfHiddenSkills.AddRange(
                        Converter.GcBytes((UInt16)currentCheck.lastStageIDX[0])
                    );
                    listOfHiddenSkills.AddRange(Converter.GcBytes((UInt16)currentCheck.roomIDX));
                    count++;
                }
            }

            SeedHeaderRaw.hiddenSkillCheckInfoNumEntries = count;
            SeedHeaderRaw.hiddenSkillCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfHiddenSkills;
        }

        private List<byte> ParseShopItems()
        {
            List<byte> listOfShopItems = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.category.Contains("Shop"))
                {
                    listOfShopItems.AddRange(
                        Converter.GcBytes(
                            (UInt16)short.Parse(
                                currentCheck.flag,
                                System.Globalization.NumberStyles.HexNumber
                            )
                        )
                    );
                    listOfShopItems.AddRange(Converter.GcBytes((UInt16)currentCheck.itemId));
                    count++;
                }
            }

            SeedHeaderRaw.shopCheckInfoNumEntries = count;
            SeedHeaderRaw.shopCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfShopItems;
        }

        private List<byte> ParseStartingItems()
        {
            SharedSettings randomizerSettings = Randomizer.SSettings;
            List<byte> listOfStartingItems = new();
            ushort count = 0;
            foreach (Item startingItem in randomizerSettings.startingItems)
            {
                listOfStartingItems.Add(Converter.GcByte((int)startingItem));
                count++;
            }

            SeedHeaderRaw.startingItemInfoNumEntries = count;
            SeedHeaderRaw.startingItemInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfStartingItems;
        }

        private List<byte> GenerateEventFlags()
        {
            List<byte> listOfEventFlags = new();
            ushort count = 0;
            byte[,] arrayOfEventFlags = { };

            arrayOfEventFlags = BackendFunctions.ConcatFlagArrays(
                arrayOfEventFlags,
                Assets.Flags.BaseRandomizerEventFlags
            );

            foreach (KeyValuePair<int, byte[,]> flagSettingsPair in Assets.Flags.EventFlags)
            {
                if (Flags.FlagSettings[flagSettingsPair.Key])
                {
                    arrayOfEventFlags = BackendFunctions.ConcatFlagArrays(
                        arrayOfEventFlags,
                        flagSettingsPair.Value
                    );
                }
            }

            for (int i = 0; i < arrayOfEventFlags.GetLength(0); i++)
            {
                listOfEventFlags.Add(Converter.GcByte(arrayOfEventFlags[i, 0]));
                listOfEventFlags.Add(Converter.GcByte(arrayOfEventFlags[i, 1]));
                count++;
            }

            SeedHeaderRaw.eventFlagsInfoNumEntries = count;
            SeedHeaderRaw.eventFlagsInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfEventFlags;
        }

        private List<byte> GenerateRegionFlags()
        {
            List<byte> listOfRegionFlags = new();
            ushort count = 0;
            byte[,] arrayOfRegionFlags = { };

            arrayOfRegionFlags = BackendFunctions.ConcatFlagArrays(
                arrayOfRegionFlags,
                Assets.Flags.BaseRandomizerRegionFlags
            );

            foreach (KeyValuePair<int, byte[,]> flagSettingsPair in Assets.Flags.RegionFlags)
            {
                if (Flags.FlagSettings[flagSettingsPair.Key])
                {
                    arrayOfRegionFlags = BackendFunctions.ConcatFlagArrays(
                        arrayOfRegionFlags,
                        flagSettingsPair.Value
                    );
                }
            }

            for (int i = 0; i < arrayOfRegionFlags.GetLength(0); i++)
            {
                listOfRegionFlags.Add(Converter.GcByte(arrayOfRegionFlags[i, 0]));
                listOfRegionFlags.Add(Converter.GcByte(arrayOfRegionFlags[i, 1]));
                count++;
            }

            SeedHeaderRaw.regionFlagsInfoNumEntries = count;
            SeedHeaderRaw.regionFlagsInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfRegionFlags;
        }

        private class SeedHeader
        {
            public UInt16 versionMajor { get; set; } // SeedData version major
            public UInt16 versionMinor { get; set; } // SeedData version minor
            public UInt16 headerSize { get; set; } // Total size of the header in bytes
            public UInt16 dataSize { get; set; } // Total number of bytes in the check data
            public UInt64 seed { get; set; } // Current seed
            public UInt32 totalSize { get; set; } // Total number of bytes in the gci after the comments
            public UInt32 requiredDungeons { get; set; } // Bitfield containing which dungeons are required to beat the seed. Only 8 bits are used, while the rest are reserved for future updates
            public UInt16 volatilePatchInfoNumEntries { get; set; } // bitArray where each bit represents a patch/modification to be applied for this playthrough
            public UInt16 volatilePatchInfoDataOffset { get; set; }
            public UInt16 oneTimePatchInfoNumEntries { get; set; } // bitArray where each bit represents a patch/modification to be applied for this playthrough
            public UInt16 oneTimePatchInfoDataOffset { get; set; }
            public UInt16 eventFlagsInfoNumEntries { get; set; } // eventFlags that need to be set for this seed
            public UInt16 eventFlagsInfoDataOffset { get; set; }
            public UInt16 regionFlagsInfoNumEntries { get; set; } // regionFlags that need to be set, alternating
            public UInt16 regionFlagsInfoDataOffset { get; set; }
            public UInt16 dzxCheckInfoNumEntries { get; set; }
            public UInt16 dzxCheckInfoDataOffset { get; set; }
            public UInt16 relCheckInfoNumEntries { get; set; }
            public UInt16 relCheckInfoDataOffset { get; set; }
            public UInt16 poeCheckInfoNumEntries { get; set; }
            public UInt16 poeCheckInfoDataOffset { get; set; }
            public UInt16 arcCheckInfoNumEntries { get; set; }
            public UInt16 arcCheckInfoDataOffset { get; set; }
            public UInt16 objectArcCheckInfoNumEntries { get; set; }
            public UInt16 objectArcCheckInfoDataOffset { get; set; }
            public UInt16 bossCheckInfoNumEntries { get; set; }
            public UInt16 bossCheckInfoDataOffset { get; set; }
            public UInt16 hiddenSkillCheckInfoNumEntries { get; set; }
            public UInt16 hiddenSkillCheckInfoDataOffset { get; set; }
            public UInt16 bugRewardCheckInfoNumEntries { get; set; }
            public UInt16 bugRewardCheckInfoDataOffset { get; set; }
            public UInt16 skyCharacterCheckInfoNumEntries { get; set; }
            public UInt16 skyCharacterCheckInfoDataOffset { get; set; }
            public UInt16 shopCheckInfoNumEntries { get; set; }
            public UInt16 shopCheckInfoDataOffset { get; set; }
            public UInt16 startingItemInfoNumEntries { get; set; }
            public UInt16 startingItemInfoDataOffset { get; set; }
            public UInt16 bgmHeaderOffset { get; set; }
        }
    }

    public class BgmHeader
    {
        public UInt16 bgmTableSize { get; set; }
        public UInt16 fanfareTableSize { get; set; }
        public UInt16 bgmTableOffset { get; set; }
        public UInt16 fanfareTableOffset { get; set; }
        public byte bgmTableNumEntries { get; set; }
        public byte fanfareTableNumEntries { get; set; }
    }
}
