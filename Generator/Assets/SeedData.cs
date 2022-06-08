namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// summary text.
    /// </summary>
    public class SeedData
    {
        private static readonly List<byte> CheckDataRaw = new();
        private static readonly List<byte> BannerDataRaw = new();
        private static readonly SeedHeader SeedHeaderRaw = new();
        private static readonly CustomTextHeader CustomMessageHeaderRaw = new();
        private static readonly short SeedHeaderSize = 0x160;

        private static short MessageHeaderSize = 0x4;

        /// <summary>
        /// summary text.
        /// </summary>
        internal class SeedHeader
        {
            public UInt16 minVersion { get; set; } // minimal required REL version
            public UInt16 maxVersion { get; set; } // maximum supported REL version
            public UInt16 headerSize { get; set; } // Total size of the header in bytes
            public UInt16 dataSize { get; set; } // Total number of bytes in the check data
            public UInt64 seed { get; set; } // Current seed
            public UInt32 totalSize { get; set; } // Total number of bytes in the gci after the comments
            public UInt16 patchInfoNumEntries { get; set; } // bitArray where each bit represents a patch/modification to be applied for this playthrough
            public UInt16 patchInfoDataOffset { get; set; }
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
            public UInt16 customTextHeaderSize { get; set; }
            public UInt16 customTextHeaderOffset { get; set; }
        }

        internal class CustomTextHeader
        {
            public UInt16 headerSize { get; set; }
            public byte totalLanguages { get; set; }
            public byte padding { get; set; }
            public CustomMessageTableInfo[] entry { get; set; }
        }

        internal class CustomMessageTableInfo
        {
            public byte language { get; set; }
            public byte padding { get; set; }
            public UInt16 totalEntries { get; set; }
            public UInt32 msgTableSize { get; set; }
            public UInt32 msgIdTableOffset { get; set; }
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static void GenerateSeedData(string seedHash)
        {
            /*
            * General Note: offset sizes are handled as two bytes. Because of this,
            * any seed bigger than 7 blocks will not work with this method.
            */
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            List<byte> currentSeedHeader = new();
            List<byte> currentSeedData = new();
            List<byte> currentMessageHeader = new();
            List<byte> currentMessageData = new();
            List<byte> currentMessageEntryInfo = new();
            TPRandomizer.Assets.CustomMessages customMessage = new();
            Dictionary<byte, CustomMessages.MessageEntry[]> seedDictionary = new();

            char regionCode;
            switch (randomizerSettings.gameRegion)
            {
                case "JAP":
                    regionCode = 'J';
                    CustomMessageHeaderRaw.totalLanguages = 1;
                    seedDictionary = customMessage.CustomJPMessageDictionary;
                    break;
                case "PAL":
                    regionCode = 'P';
                    CustomMessageHeaderRaw.totalLanguages = 3;
                    seedDictionary = customMessage.CustomPALMessageDictionary;
                    break;
                default:
                    regionCode = 'E';
                    CustomMessageHeaderRaw.totalLanguages = 1;
                    seedDictionary = customMessage.CustomUSMessageDictionary;
                    break;
            }

            // Add seed banner
            BannerDataRaw.AddRange(Properties.Resources.seedGciImageData);
            BannerDataRaw.AddRange(Converter.StringBytes("TPR 1.0 Seed Data", 0x20, regionCode));
            BannerDataRaw.AddRange(Converter.StringBytes(seedHash, 0x20, regionCode));

            // Header Info
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

            // Custom Message Info
            CustomMessageHeaderRaw.entry = new CustomMessageTableInfo[
                CustomMessageHeaderRaw.totalLanguages
            ];
            for (int i = 0; i < CustomMessageHeaderRaw.totalLanguages; i++)
            {
                CustomMessageHeaderRaw.entry[i] = new CustomMessageTableInfo();
                currentMessageData.AddRange(
                    ParseCustomMessageData(i, currentMessageData, seedDictionary)
                );
                while (currentMessageData.Count % 0x4 != 0)
                {
                    currentMessageData.Add(Converter.GcByte(0x0));
                }
                currentMessageEntryInfo.AddRange(GenerateMessageTableInfo(i));
            }

            currentMessageHeader.AddRange(GenerateMessageHeader(currentMessageEntryInfo));
            SeedHeaderRaw.totalSize = (uint)(
                SeedHeaderSize
                + CheckDataRaw.Count
                + currentMessageHeader.Count
                + currentMessageData.Count
            );
            currentSeedHeader.AddRange(GenerateSeedHeader(randomizerSettings.seedNumber, seedHash));
            currentSeedData.AddRange(BannerDataRaw);
            currentSeedData.AddRange(currentSeedHeader);
            currentSeedData.AddRange(CheckDataRaw);
            currentSeedData.AddRange(currentMessageHeader);
            currentSeedData.AddRange(currentMessageData);

            var gci = new Gci(
                (byte)randomizerSettings.seedNumber,
                randomizerSettings.gameRegion,
                currentSeedData,
                seedHash
            );
            string fileHash = "TPR-v1.0-" + seedHash + "-Seed-Data.gci";
            File.WriteAllBytes(fileHash, gci.gciFile.ToArray());
        }

        public static void GenerateSeedDataNew(string seedHash, PSettings pSettings)
        {
            /*
            * General Note: offset sizes are handled as two bytes. Because of this,
            * any seed bigger than 7 blocks will not work with this method.
            */
            List<byte> currentSeedHeader = new();
            List<byte> currentSeedData = new();
            List<byte> currentMessageHeader = new();
            List<byte> currentMessageData = new();
            List<byte> currentMessageEntryInfo = new();
            TPRandomizer.Assets.CustomMessages customMessage = new();
            Dictionary<byte, CustomMessages.MessageEntry[]> seedDictionary = new();

            char regionCode;
            string regionName;

            switch (pSettings.gameVersion)
            {
                case "GZ2J":
                    regionName = "JAP";
                    regionCode = 'J';
                    CustomMessageHeaderRaw.totalLanguages = 1;
                    seedDictionary = customMessage.CustomJPMessageDictionary;
                    break;
                case "GZ2P":
                    regionName = "PAL";
                    regionCode = 'P';
                    CustomMessageHeaderRaw.totalLanguages = 3;
                    seedDictionary = customMessage.CustomPALMessageDictionary;
                    break;
                default:
                    regionName = "US";
                    regionCode = 'E';
                    CustomMessageHeaderRaw.totalLanguages = 1;
                    seedDictionary = customMessage.CustomUSMessageDictionary;
                    break;
            }

            // Add seed banner
            BannerDataRaw.AddRange(Properties.Resources.seedGciImageData);
            BannerDataRaw.AddRange(Converter.StringBytes("TPR 1.0 Seed Data", 0x20, regionCode));
            BannerDataRaw.AddRange(Converter.StringBytes(seedHash, 0x20, regionCode));

            // Header Info
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

            // Custom Message Info
            CustomMessageHeaderRaw.entry = new CustomMessageTableInfo[
                CustomMessageHeaderRaw.totalLanguages
            ];
            for (int i = 0; i < CustomMessageHeaderRaw.totalLanguages; i++)
            {
                CustomMessageHeaderRaw.entry[i] = new CustomMessageTableInfo();
                currentMessageData.AddRange(
                    ParseCustomMessageData(i, currentMessageData, seedDictionary)
                );
                while (currentMessageData.Count % 0x4 != 0)
                {
                    currentMessageData.Add(Converter.GcByte(0x0));
                }
                currentMessageEntryInfo.AddRange(GenerateMessageTableInfo(i));
            }

            currentMessageHeader.AddRange(GenerateMessageHeader(currentMessageEntryInfo));
            SeedHeaderRaw.totalSize = (uint)(
                SeedHeaderSize
                + CheckDataRaw.Count
                + currentMessageHeader.Count
                + currentMessageData.Count
            );
            currentSeedHeader.AddRange(GenerateSeedHeader(pSettings.seedNumber, seedHash));
            currentSeedData.AddRange(BannerDataRaw);
            currentSeedData.AddRange(currentSeedHeader);
            currentSeedData.AddRange(CheckDataRaw);
            currentSeedData.AddRange(currentMessageHeader);
            currentSeedData.AddRange(currentMessageData);

            var gci = new Gci(pSettings.seedNumber, regionName, currentSeedData, seedHash);
            string fileHash = "TPR-v1.0-" + seedHash + "-Seed-Data.gci";
            File.WriteAllBytes(fileHash, gci.gciFile.ToArray());
        }

        public static byte[] GenerateSeedDataNewByteArray(string seedHash, PSettings pSettings)
        {
            /*
            * General Note: offset sizes are handled as two bytes. Because of this,
            * any seed bigger than 7 blocks will not work with this method.
            */
            List<byte> currentSeedHeader = new();
            List<byte> currentSeedData = new();
            List<byte> currentMessageHeader = new();
            List<byte> currentMessageData = new();
            List<byte> currentMessageEntryInfo = new();
            TPRandomizer.Assets.CustomMessages customMessage = new();
            Dictionary<byte, CustomMessages.MessageEntry[]> seedDictionary = new();

            char regionCode;
            string regionName;

            switch (pSettings.gameVersion)
            {
                case "GZ2J":
                    regionName = "JAP";
                    regionCode = 'J';
                    CustomMessageHeaderRaw.totalLanguages = 1;
                    seedDictionary = customMessage.CustomJPMessageDictionary;
                    break;
                case "GZ2P":
                    regionName = "PAL";
                    regionCode = 'P';
                    CustomMessageHeaderRaw.totalLanguages = 3;
                    seedDictionary = customMessage.CustomPALMessageDictionary;
                    break;
                default:
                    regionName = "US";
                    regionCode = 'E';
                    CustomMessageHeaderRaw.totalLanguages = 1;
                    seedDictionary = customMessage.CustomUSMessageDictionary;
                    break;
            }

            // Add seed banner
            BannerDataRaw.AddRange(Properties.Resources.seedGciImageData);
            BannerDataRaw.AddRange(Converter.StringBytes("TPR 1.0 Seed Data", 0x20, regionCode));
            BannerDataRaw.AddRange(Converter.StringBytes(seedHash, 0x20, regionCode));

            // Header Info
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

            // Custom Message Info
            CustomMessageHeaderRaw.entry = new CustomMessageTableInfo[
                CustomMessageHeaderRaw.totalLanguages
            ];
            for (int i = 0; i < CustomMessageHeaderRaw.totalLanguages; i++)
            {
                CustomMessageHeaderRaw.entry[i] = new CustomMessageTableInfo();
                currentMessageData.AddRange(
                    ParseCustomMessageData(i, currentMessageData, seedDictionary)
                );
                while (currentMessageData.Count % 0x4 != 0)
                {
                    currentMessageData.Add(Converter.GcByte(0x0));
                }
                currentMessageEntryInfo.AddRange(GenerateMessageTableInfo(i));
            }

            currentMessageHeader.AddRange(GenerateMessageHeader(currentMessageEntryInfo));
            SeedHeaderRaw.totalSize = (uint)(
                SeedHeaderSize
                + CheckDataRaw.Count
                + currentMessageHeader.Count
                + currentMessageData.Count
            );
            currentSeedHeader.AddRange(GenerateSeedHeader(pSettings.seedNumber, seedHash));
            currentSeedData.AddRange(BannerDataRaw);
            currentSeedData.AddRange(currentSeedHeader);
            currentSeedData.AddRange(CheckDataRaw);
            currentSeedData.AddRange(currentMessageHeader);
            currentSeedData.AddRange(currentMessageData);

            var gci = new Gci(pSettings.seedNumber, regionName, currentSeedData, seedHash);
            string fileHash = "TPR-v1.0-" + seedHash + "-Seed-Data.gci";
            return gci.gciFile.ToArray();
            // File.WriteAllBytes(fileHash, gci.gciFile.ToArray());
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="seedNumber">The number you want to convert.</param>
        /// <param name="seedHash">A randomized string that represents the current seed.</param>
        /// <returns> The inserted value as a byte. </returns>
        internal static List<byte> GenerateSeedHeader(int seedNumber, string seedHash)
        {
            List<byte> seedHeader = new();
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            // SettingData settingData = Randomizer.RandoSettingData;
            SeedHeaderRaw.headerSize = (ushort)SeedHeaderSize;
            SeedHeaderRaw.dataSize = (ushort)CheckDataRaw.Count;
            SeedHeaderRaw.seed = BackendFunctions.GetChecksum(seedHash, 64);
            SeedHeaderRaw.minVersion = (ushort)(
                Randomizer.RandomizerVersionMajor << 8 | Randomizer.RandomizerVersionMinor
            );
            SeedHeaderRaw.maxVersion = (ushort)(
                Randomizer.RandomizerVersionMajor << 8 | Randomizer.RandomizerVersionMinor
            );

            SeedHeaderRaw.customTextHeaderSize = (ushort)MessageHeaderSize;
            SeedHeaderRaw.customTextHeaderOffset = (ushort)(SeedHeaderSize + CheckDataRaw.Count);
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

            seedHeader.Add(Converter.GcByte(randomizerSettings.heartColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.aButtonColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.bButtonColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.xButtonColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.yButtonColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.zButtonColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.lanternColor));
            seedHeader.Add(Converter.GcByte(randomizerSettings.transformAnywhere ? 1 : 0));
            seedHeader.Add(Converter.GcByte(randomizerSettings.quickTransform ? 1 : 0));
            seedHeader.Add(
                Converter.GcByte(
                    Array.IndexOf(
                        SettingData.castleRequirements,
                        randomizerSettings.castleRequirements
                    )
                )
            );
            seedHeader.Add(
                Converter.GcByte(
                    Array.IndexOf(
                        SettingData.palaceRequirements,
                        randomizerSettings.palaceRequirements
                    )
                )
            );
            while (seedHeader.Count < (SeedHeaderSize - 1))
            {
                seedHeader.Add((byte)0x0);
            }

            seedHeader.Add(Converter.GcByte(seedNumber));
            return seedHeader;
        }

        private static List<byte> GeneratePatchSettings()
        {
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            List<byte> listOfPatches = new();
            bool[] patchSettingsArray =
            {
                randomizerSettings.increaseWallet,
                randomizerSettings.shuffleBackgroundMusic,
                randomizerSettings.disableEnemyBackgoundMusic,
                randomizerSettings.fastIronBoots,
                randomizerSettings.faronTwilightCleared,
                randomizerSettings.eldinTwilightCleared,
                randomizerSettings.lanayruTwilightCleared,
                randomizerSettings.modifyShopModels,
                randomizerSettings.skipMinorCutscenes,
                randomizerSettings.mdhSkipped
            };
            int patchOptions = 0x0;
            int bitwiseOperator = 0;
            SeedHeaderRaw.patchInfoNumEntries = 1; // Start off at one to ensure alignment
            for (int i = 0; i < patchSettingsArray.Length; i++)
            {
                if (((i % 8) == 0) && (i >= 8))
                {
                    SeedHeaderRaw.patchInfoNumEntries++;
                    listOfPatches.Add(Converter.GcByte(patchOptions));
                    patchOptions = 0;
                    bitwiseOperator = 0;
                }

                if (patchSettingsArray[i])
                {
                    patchOptions |= 0x80 >> bitwiseOperator;
                }

                bitwiseOperator++;
            }

            listOfPatches.Add(Converter.GcByte(patchOptions));
            SeedHeaderRaw.patchInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfPatches;
        }

        private static List<byte> ParseARCReplacements()
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

        private static List<byte> ParseObjectARCReplacements()
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

        private static List<byte> ParseDZXReplacements()
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

        private static List<byte> ParsePOEReplacements()
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

        private static List<byte> ParseRELOverrides()
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

        private static List<byte> ParseBossReplacements()
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

        private static List<byte> ParseBugRewards()
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

        private static List<byte> ParseSkyCharacters()
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

        private static List<byte> ParseHiddenSkills()
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

        private static List<byte> ParseShopItems()
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

        private static List<byte> ParseStartingItems()
        {
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            List<byte> listOfStartingItems = new();
            ushort count = 0;
            foreach (Item startingItem in randomizerSettings.StartingItems)
            {
                listOfStartingItems.Add(Converter.GcByte((int)startingItem));
                count++;
            }

            SeedHeaderRaw.startingItemInfoNumEntries = count;
            SeedHeaderRaw.startingItemInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfStartingItems;
        }

        private static List<byte> ParseMessageIDTables(
            int currentLanguage,
            List<byte> currentMessageData,
            Dictionary<byte, CustomMessages.MessageEntry[]> seedDictionary
        )
        {
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            List<byte> listOfCustomMsgIDs = new();
            ushort count = 0;
            CustomMessageHeaderRaw.entry[currentLanguage].language = seedDictionary
                .ElementAt(currentLanguage)
                .Key;
            CustomMessageHeaderRaw.entry[currentLanguage].msgIdTableOffset = (ushort)(
                (0xC * (CustomMessageHeaderRaw.totalLanguages - currentLanguage))
                + currentMessageData.Count
            );
            foreach (
                CustomMessages.MessageEntry messageEntry in seedDictionary
                    .ElementAt(currentLanguage)
                    .Value
            )
            {
                listOfCustomMsgIDs.AddRange(Converter.GcBytes((UInt16)messageEntry.messageID));
                count++;
            }
            CustomMessageHeaderRaw.entry[currentLanguage].totalEntries = count;
            count = 0;

            while (listOfCustomMsgIDs.Count % 0x4 != 0)
            {
                listOfCustomMsgIDs.Add(Converter.GcByte(0xFF));
            }
            return listOfCustomMsgIDs;
        }

        private static List<byte> ParseCustomMessageData(
            int currentLanguage,
            List<byte> currentMessageData,
            Dictionary<byte, CustomMessages.MessageEntry[]> seedDictionary
        )
        {
            TPRandomizer.Assets.CustomMessages customMessage = new();
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            List<byte> customMessageData = new();
            List<byte> listOfCustomMessages = new();
            List<byte> listOfMsgOffsets = new();
            List<byte> customMsgIDTables = new();

            customMsgIDTables.AddRange(
                ParseMessageIDTables(currentLanguage, currentMessageData, seedDictionary)
            );

            foreach (
                CustomMessages.MessageEntry messageEntry in seedDictionary
                    .ElementAt(currentLanguage)
                    .Value
            )
            {
                listOfMsgOffsets.AddRange(Converter.GcBytes((UInt32)(listOfCustomMessages.Count)));
                listOfCustomMessages.AddRange(Converter.MessageStringBytes(messageEntry.message));
                listOfCustomMessages.Add(Converter.GcByte(0x0));
            }
            CustomMessageHeaderRaw.entry[currentLanguage].msgTableSize = (ushort)(
                listOfCustomMessages.Count
            );

            customMessageData.AddRange(customMsgIDTables);
            customMessageData.AddRange(listOfMsgOffsets);
            customMessageData.AddRange(listOfCustomMessages);
            return customMessageData;
        }

        private static List<byte> GenerateMessageHeader(List<byte> messageTableInfo)
        {
            TPRandomizer.Assets.CustomMessages customMessage = new();
            RandomizerSetting randomizerSettings = Randomizer.RandoSetting;
            List<byte> messageHeader = new();
            CustomMessageHeaderRaw.padding = 0x0;
            messageHeader.AddRange(Converter.GcBytes((UInt16)(0x4 + messageTableInfo.Count))); // header size
            messageHeader.Add(Converter.GcByte(CustomMessageHeaderRaw.totalLanguages));
            messageHeader.Add(Converter.GcByte(0x0)); // padding
            messageHeader.AddRange(messageTableInfo);

            MessageHeaderSize = (short)messageHeader.Count;

            return messageHeader;
        }

        private static List<byte> GenerateMessageTableInfo(int currentLanguage)
        {
            List<byte> messageTableInfo = new();
            messageTableInfo.Add(
                Converter.GcByte(CustomMessageHeaderRaw.entry[currentLanguage].language)
            );
            messageTableInfo.Add(Converter.GcByte(0x0)); // padding
            messageTableInfo.AddRange(
                Converter.GcBytes(
                    (UInt16)CustomMessageHeaderRaw.entry[currentLanguage].totalEntries
                )
            );
            messageTableInfo.AddRange(
                Converter.GcBytes(
                    (UInt32)CustomMessageHeaderRaw.entry[currentLanguage].msgTableSize
                )
            );
            messageTableInfo.AddRange(
                Converter.GcBytes(
                    (UInt32)CustomMessageHeaderRaw.entry[currentLanguage].msgIdTableOffset
                )
            );

            return messageTableInfo;
        }

        private static List<byte> GenerateEventFlags()
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

        private static List<byte> GenerateRegionFlags()
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
    }
}
