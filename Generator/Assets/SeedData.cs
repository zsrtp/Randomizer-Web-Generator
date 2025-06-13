namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using TPRandomizer.Assets.CLR0;
    using TPRandomizer.FcSettings.Enums;
    using TPRandomizer.SSettings.Enums;

    /// <summary>
    /// summary text.
    /// </summary>
    public class SeedData
    {
        // See <add_documentation_reference_here> for the flowchart for
        // determining if you should increment the major or minor version.
        public static readonly UInt16 VersionMajor = 1;
        public static readonly UInt16 VersionMinor = 3;
        public static readonly UInt16 VersionPatch = 0;

        // For convenience. This does not include any sort of leading 'v', so
        // add that where you use this variable if you need it.
        public static readonly string VersionString = $"{VersionMajor}.{VersionMinor}";

        private static List<byte> CheckDataRaw = new();
        private static List<byte> BannerDataRaw = new();

        private static List<byte> GCIDataRaw = new();
        private static SeedHeader SeedHeaderRaw = new();
        private static CustomTextHeader CustomMessageHeaderRaw = new();
        public static readonly int DebugInfoSize = 0x20;
        public static readonly int ImageDataSize = 0x1400;
        private static readonly short SeedHeaderSize = 0x160;
        private static short MessageHeaderSize = 0xC;
        private SeedGenResults seedGenResults;
        public FileCreationSettings fcSettings { get; }

        private SeedData(SeedGenResults seedGenResults, FileCreationSettings fcSettings)
        {
            this.seedGenResults = seedGenResults;
            this.fcSettings = fcSettings;
        }

        public static byte[] GenerateSeedDataBytes(
            SeedGenResults seedGenResults,
            FileCreationSettings fcSettings,
            GameRegion regionOverride,
            bool isGci
        )
        {
            SeedData seedData = new SeedData(seedGenResults, fcSettings);
            return seedData.GenerateSeedDataBytesInternal(regionOverride, isGci);
        }

        public byte[] GenerateSeedDataBytesInternal(GameRegion regionOverride, bool isGci)
        {
            Assets.CustomMessages.MessageLanguage hintLanguage = Assets
                .CustomMessages
                .MessageLanguage
                .English;
            /*
            * General Note: offset sizes are handled as two bytes. Because of this,
            * any seed bigger than 7 blocks will not work with this method. The seed structure is as follows:
            * Seed Header
            * Seed Data
            * Check Data
            * Bgm Header
            * Bgm Data
            * Message Header
            * Message Data
            */

            // Reset buffers (needed for when generating multiple files in a
            // single request)
            CheckDataRaw = new();
            BannerDataRaw = new();
            SeedHeaderRaw = new();
            GCIDataRaw = new();

            // First we need to generate the buffers for the various byte lists that will be used to populate the seed data.
            SharedSettings randomizerSettings = Randomizer.SSettings;
            Randomizer.Items.GenerateItemPool();
            List<byte> currentSeedHeader = new();
            List<byte> currentSeedData = new();
            List<byte> currentMessageHeader = new();
            List<byte> currentMessageData = new();
            List<byte> currentMessageEntryInfo = new();
            Dictionary<byte, List<CustomMessages.MessageEntry>> seedDictionary = new();

            List<CustomMessages.MessageEntry> seedMessages =
                seedGenResults.customMsgData.GenMessageEntries(seedGenResults);

            seedDictionary.Add((byte)hintLanguage, seedMessages);

            // If generating for all regions, we use the region passed in as an
            // argument rather than reading from fcSettings.
            GameRegion gameRegion =
                fcSettings.gameRegion == GameRegion.All ? regionOverride : fcSettings.gameRegion;

            char region;
            switch (gameRegion)
            {
                case GameRegion.GC_USA:
                case GameRegion.WII_10_USA:
                case GameRegion.WII_12_USA:
                    region = 'E';
                    break;
                case GameRegion.GC_EUR:
                case GameRegion.WII_10_EU:
                {
                    region = 'P';
                    break;
                }
                case GameRegion.GC_JAP:
                case GameRegion.WII_10_JP:
                {
                    region = 'J';
                    break;
                }
                default:
                {
                    throw new Exception("Did not specify which region the output should be for.");
                }
            }

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
            CheckDataRaw.AddRange(ParseEventItems());
            CheckDataRaw.AddRange(ParseStartingItems());
            while (CheckDataRaw.Count % 0x10 != 0)
            {
                CheckDataRaw.Add(Converter.GcByte(0x0));
            }

            GCIDataRaw.AddRange(CheckDataRaw);

            // Next we want to generate the non-check/item data
            List<byte> dataBytes = GenerateEntranceTable();
            if (dataBytes != null)
            {
                SeedHeaderRaw.shuffledEntranceInfoDataOffset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = GenerateBgmData();
            if (dataBytes != null)
            {
                SeedHeaderRaw.bgmInfoDataOffset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = GenerateFanfareData();
            if (dataBytes != null)
            {
                SeedHeaderRaw.fanfareInfoDataOffset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = GenerateBmg0SectionData(seedGenResults.customMsgData);
            if (dataBytes != null)
            {
                SeedHeaderRaw.bmg0Offset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = ParseClr0Bytes();
            if (dataBytes != null)
            {
                SeedHeaderRaw.clr0Offset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            // Custom Message Info
            currentMessageData.AddRange(
                ParseCustomMessageData((int)hintLanguage, currentMessageData, seedDictionary)
            );
            while (currentMessageData.Count % 0x4 != 0)
            {
                currentMessageData.Add(Converter.GcByte(0x0));
            }
            currentMessageEntryInfo.AddRange(GenerateMessageTableInfo((int)hintLanguage));
            currentMessageHeader.AddRange(GenerateMessageHeader(currentMessageEntryInfo));

            SeedHeaderRaw.totalSize = (uint)(
                SeedHeaderSize
                + GCIDataRaw.Count
                + currentMessageHeader.Count
                + currentMessageData.Count
            );

            // Generate Seed Data
            currentSeedHeader.AddRange(GenerateSeedHeader());
            currentSeedData.AddRange(currentSeedHeader);
            currentSeedData.AddRange(GCIDataRaw);
            currentSeedData.AddRange(currentMessageHeader);
            currentSeedData.AddRange(currentMessageData);

            if (isGci)
            {
                return patchGCIWithSeed(region, currentSeedData);
            }
            else
            {
                return currentSeedData.ToArray();
            }

            // File.WriteAllBytes(playthroughName, gci.gciFile.ToArray());
        }

        private List<byte> GenerateSeedHeader()
        {
            List<byte> seedHeader = new();
            SharedSettings randomizerSettings = Randomizer.SSettings;
            seedHeader.AddRange(Converter.StringBytes("TPR")); // magic
            seedHeader.AddRange(Converter.StringBytes(seedGenResults.playthroughName, 33)); // seed name
            SeedHeaderRaw.headerSize = (ushort)SeedHeaderSize;
            SeedHeaderRaw.dataSize = (ushort)GCIDataRaw.Count;
            SeedHeaderRaw.versionMajor = VersionMajor;
            SeedHeaderRaw.versionMinor = VersionMinor;
            SeedHeaderRaw.customTextHeaderSize = (ushort)MessageHeaderSize;
            SeedHeaderRaw.customTextHeaderOffset = (ushort)(GCIDataRaw.Count);
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
                else if (headerObject.PropertyType == typeof(List<byte>))
                {
                    seedHeader.AddRange((List<byte>)headerObject.GetValue(SeedHeaderRaw, null));
                }
            }

            seedHeader.Add(Converter.GcByte((int)randomizerSettings.castleRequirements));
            seedHeader.Add(Converter.GcByte((int)randomizerSettings.palaceRequirements));
            int mapBits = 0;
            bool[] mapFlags = new bool[]
            {
                false,
                randomizerSettings.skipSnowpeakEntrance,
                randomizerSettings.openMap,
                randomizerSettings.lanayruTwilightCleared,
                randomizerSettings.eldinTwilightCleared,
                randomizerSettings.faronTwilightCleared,
                randomizerSettings.skipPrologue,
                false,
            };
            for (int i = 0; i < mapFlags.GetLength(0); i++)
            {
                if (mapFlags[i])
                {
                    mapBits |= (0x80 >> i);
                }
            }

            seedHeader.Add(Converter.GcByte(mapBits));

            switch (randomizerSettings.damageMagnification)
            {
                case SSettings.Enums.DamageMagnification.OHKO:
                {
                    seedHeader.Add(Converter.GcByte(80)); // In a OHKO situation, we will need to deal a maximum magnification of 80 to ensure Link dies if the original damage was 1/4 heart.
                    break;
                }
                default:
                {
                    seedHeader.Add(Converter.GcByte((int)randomizerSettings.damageMagnification));
                    break;
                }
            }
            seedHeader.Add(
                Converter.GcByte(
                    (int)ItemFunctions.ToTSwordRequirements[(int)randomizerSettings.totEntrance]
                )
            );

            while (seedHeader.Count < SeedHeaderSize)
            {
                seedHeader.Add((byte)0x0);
            }

            return seedHeader;
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
                randomizerSettings.openMap, //map bits
            };
            bool[] oneTimePatchSettingsArray =
            {
                randomizerSettings.increaseWallet,
                randomizerSettings.fastIronBoots,
                fcSettings.disableEnemyBgm,
                randomizerSettings.instantText,
                randomizerSettings.skipMajorCutscenes,
                fcSettings.invertCameraAxis,
            };
            bool[] flagsBitfieldArray =
            {
                randomizerSettings.transformAnywhere,
                randomizerSettings.quickTransform,
                randomizerSettings.increaseSpinnerSpeed,
                randomizerSettings.bonksDoDamage,
                randomizerSettings.increaseWallet,
                randomizerSettings.modifyShopModels,
            };

            List<bool[]> flagArrayList = new()
            {
                volatilePatchSettingsArray,
                oneTimePatchSettingsArray,
                flagsBitfieldArray,
            };
            SeedHeaderRaw.volatilePatchInfoNumEntries = (ushort)volatilePatchSettingsArray.Length;
            SeedHeaderRaw.oneTimePatchInfoNumEntries = (ushort)oneTimePatchSettingsArray.Length;
            SeedHeaderRaw.flagBitfieldInfoNumEntries = (ushort)flagsBitfieldArray.Length;
            ushort dataOffset = (ushort)CheckDataRaw.Count;
            SeedHeaderRaw.volatilePatchInfoDataOffset = dataOffset;
            SeedHeaderRaw.oneTimePatchInfoDataOffset = (ushort)(dataOffset + 0x10);
            SeedHeaderRaw.flagBitfieldInfoDataOffset = (ushort)(dataOffset + 0x20);

            foreach (bool[] flagArr in flagArrayList)
            {
                List<byte> listOfFlags = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // Align to 16 bytes
                int bitwiseOperator = 7;
                for (int i = 0, j = 0; i < flagArr.Length; i++)
                {
                    if (((i % 8) == 0) && (i >= 8))
                    {
                        bitwiseOperator = 7;
                        j++;
                    }

                    if (flagArr[i])
                    {
                        listOfFlags[j] |= Converter.GcByte(0x80 >> bitwiseOperator);
                    }

                    bitwiseOperator--;
                }
                // Next we reverse the list to account for the enum structure on the rando side
                listOfFlags = BackendFunctions.ReverseBytes(listOfFlags, 0x4);
                listOfPatches.AddRange(listOfFlags);
            }

            return listOfPatches;
        }

        private List<byte> ParseARCReplacements()
        {
            List<byte> listOfArcReplacements = new();
            ushort count = 0;
            List<ARCReplacement> staticArcReplacements = generateStaticArcReplacements();
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.dataCategory.Contains("ARC"))
                {
                    for (int i = 0; i < currentCheck.arcOffsets.Count; i++)
                    {
                        listOfArcReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)
                                    uint.Parse(
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
                                    (UInt32)
                                        uint.Parse(
                                            currentCheck.overrideInstruction[i],
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

            foreach (ARCReplacement arcReplacement in staticArcReplacements)
            {
                listOfArcReplacements.AddRange(
                    Converter.GcBytes(
                        (UInt32)
                            uint.Parse(
                                arcReplacement.Offset,
                                System.Globalization.NumberStyles.HexNumber
                            )
                    )
                );
                listOfArcReplacements.AddRange(
                    Converter.GcBytes(
                        (UInt32)
                            uint.Parse(
                                arcReplacement.ReplacementValue,
                                System.Globalization.NumberStyles.HexNumber
                            )
                    )
                );
                listOfArcReplacements.Add(Converter.GcByte(arcReplacement.Directory));
                listOfArcReplacements.Add(Converter.GcByte(arcReplacement.ReplacementType));
                listOfArcReplacements.Add(Converter.GcByte(arcReplacement.StageIDX));
                listOfArcReplacements.Add(Converter.GcByte(arcReplacement.RoomID));
                count++;
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
                if (currentCheck.dataCategory.Contains("ObjectARC"))
                {
                    for (int i = 0; i < currentCheck.arcOffsets.Count; i++)
                    {
                        listOfArcReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)
                                    uint.Parse(
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
                if (currentCheck.dataCategory.Contains("DZX"))
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
                            /* This is still in development.
                            bool chestAppearanceMatchesContent = false;
                            if (chestAppearanceMatchesContent)
                            {
                                if (Randomizer.Items.RandomizedImportantItems.Contains(currentCheck.itemId))
                                {
                                    dataArray[4] = byte.Parse("41",System.Globalization.NumberStyles.HexNumber); // Hex for 'B'
                                    dataArray[5] = byte.Parse("30",System.Globalization.NumberStyles.HexNumber);  // Hex for '0'
                                    Console.WriteLine("doing the thing for " + currentCheck.checkName);
                                }
                                else
                                {
                                    dataArray[4] = byte.Parse("41",System.Globalization.NumberStyles.HexNumber); // Hex for 'A'
                                    dataArray[5] = byte.Parse("30",System.Globalization.NumberStyles.HexNumber); // Hex for '0'
                                    Console.WriteLine("doing the not thing for " + currentCheck.checkName);
                                }
                            }*/
                        }
                        else if (currentCheck.dzxTag[i] == "ACTR")
                        {
                            dataArray[11] = (byte)currentCheck.itemId;
                        }

                        listOfDZXReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt16)
                                    ushort.Parse(
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
                if (currentCheck.dataCategory.Contains("Poe"))
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
                    listOfPOEReplacements.Add(Converter.GcByte((int)currentCheck.itemId));
                    listOfPOEReplacements.Add(Converter.GcByte(0x0)); // padding
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
                if (currentCheck.dataCategory.Contains("REL"))
                {
                    for (int i = 0; i < currentCheck.moduleID.Count; i++)
                    {
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes((UInt16)currentCheck.relReplacementType[i])
                        );
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes((UInt16)currentCheck.stageIDX[i])
                        );
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)
                                    uint.Parse(
                                        currentCheck.moduleID[i],
                                        System.Globalization.NumberStyles.HexNumber
                                    )
                            )
                        );
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)
                                    uint.Parse(
                                        currentCheck.relOffsets[i],
                                        System.Globalization.NumberStyles.HexNumber
                                    )
                            )
                        );
                        Console.WriteLine(currentCheck.checkName);
                        listOfRELReplacements.AddRange(
                            Converter.GcBytes(
                                (UInt32)(
                                    uint.Parse(
                                        currentCheck.overrideInstruction[i],
                                        System.Globalization.NumberStyles.HexNumber
                                    ) + (byte)currentCheck.itemId
                                )
                            )
                        );
                        count++;
                    }
                }
            }

            List<RELReplacement> staticRelReplacements = GenerateStaticRelReplacements();

            foreach(RELReplacement replacement in staticRelReplacements)
            {
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt16)replacement.ReplacementType));
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt16)replacement.StageIDX));
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt32)replacement.ModuleID));
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt32)replacement.Offset));
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt32)replacement.ReplacementValue));
                count++;
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
                if (currentCheck.dataCategory.Contains("Boss"))
                {
                    listOfBossReplacements.AddRange(
                        Converter.GcBytes((UInt16)currentCheck.stageIDX[0])
                    );
                    listOfBossReplacements.Add(Converter.GcByte((int)currentCheck.itemId));
                    listOfBossReplacements.Add(Converter.GcByte(0x0)); // padding
                    count++;
                }
            }

            SeedHeaderRaw.bossCheckInfoNumEntries = count;
            SeedHeaderRaw.bossCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfBossReplacements;
        }

        private List<RELReplacement> GenerateStaticRelReplacements()
        {
            List<RELReplacement> listOfStaticReplacements =
            [
                // Shad rel patches
                new RELReplacement(
                    (int)ReplacementType.Instruction,
                    (int)0xFF,
                    (int)GCRelIDs.D_A_NPC_SHAD,
                    0x6E0,
                    DataFunctions.ASM_LOAD_IMMEDIATE(3, 0x333)
                ), // Patch flag checking to allow Shad to appear in the Kak basement.
                new RELReplacement(
                    (int)ReplacementType.Instruction,
                    (int)0xFF,
                    (int)GCRelIDs.D_A_NPC_SHAD,
                    0x6EC,
                    0x4182000c // beq
                ), // Patch flag checking to allow Shad to appear in the Kak basement.
                 new RELReplacement(
                    (int)ReplacementType.Instruction,
                    (int)0xFF,
                    (int)GCRelIDs.D_A_NPC_SHAD,
                    0x6F8,
                    0x48000100
                ), // Patch flag checking to allow Shad to appear in the Kak basement.
                 new RELReplacement(
                    (int)ReplacementType.Instruction,
                    (int)0xFF,
                    (int)GCRelIDs.D_A_NPC_SHAD,
                    0x30B0,
                    0x418000c4
                ), // Patch item checking so that showing the completed skybook doesn't bork the check
            ];

            // Parse Midna hair color replacement
            List<KeyValuePair<int, int>> midnaHairBytes = BuildMidnaHairBytes();
            foreach (KeyValuePair<int, int> pair in midnaHairBytes)
            {
                listOfStaticReplacements.Add( new RELReplacement((int)ReplacementType.Instruction, (int)0xFF, (int)GCRelIDs.D_A_MIDNA, pair.Key, pair.Value));
            }

            return listOfStaticReplacements;
        }

        private List<KeyValuePair<int, int>> BuildMidnaHairBytes()
        {
            int[] glowAnyWorldInactive = MidnaGlowToInts(fcSettings.midnaHairGlowAnyWorldInactive);
            int[] glowLightWorldActive = MidnaGlowToInts(fcSettings.midnaHairGlowLightWorldActive);
            int[] glowDarkWorldActive = MidnaGlowToInts(fcSettings.midnaHairGlowDarkWorldActive);

            return new()
            {
                new(0xA438, fcSettings.midnaHairBaseLightWorldInactive << 8),
                new(0xA424, fcSettings.midnaHairBaseDarkWorldInactive << 8),
                new(0xA434, fcSettings.midnaHairBaseAnyWorldActive << 8),
                new(0xA41C, glowAnyWorldInactive[0]),
                new(0xA420, glowAnyWorldInactive[1]),
                new(0xA440, glowLightWorldActive[0]),
                new(0xA444, glowLightWorldActive[1]),
                new(0xA42C, glowDarkWorldActive[0]),
                new(0xA430, glowDarkWorldActive[1]),
                new(0xA43C, fcSettings.midnaHairTipsLightWorldInactive << 8),
                new(0xA428, fcSettings.midnaHairTipsDarkWorldAnyActive << 8),
                new(0xA448, fcSettings.midnaHairTipsLightWorldActive << 8),
            };
        }

        private int[] MidnaGlowToInts(int glowRgb)
        {
            // returns 00RR00GG 00BB0000
            int[] ret = { (glowRgb & 0xFF0000) | (glowRgb & 0xFF00) >> 8, (glowRgb & 0xFF) << 16 };
            return ret;
        }

        private List<byte> ParseBugRewards()
        {
            List<byte> listOfBugRewards = new();
            ushort count = 0;
            SharedSettings randomizerSettings = Randomizer.SSettings;
            if (randomizerSettings.shuffleNpcItems)
            {
                foreach (
                    KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList()
                )
                {
                    Check currentCheck = checkList.Value;
                    if (currentCheck.dataCategory.Contains("Bug Reward"))
                    {
                        listOfBugRewards.AddRange(
                            Converter.GcBytes(
                                (UInt16)
                                    byte.Parse(
                                        currentCheck.flag,
                                        System.Globalization.NumberStyles.HexNumber
                                    )
                            )
                        );
                        listOfBugRewards.Add(Converter.GcByte((int)currentCheck.itemId));
                        listOfBugRewards.Add(Converter.GcByte(0x0)); // padding
                        count++;
                    }
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
                if (currentCheck.dataCategory.Contains("Sky Book"))
                {
                    listOfSkyCharacters.AddRange(
                        Converter.GcBytes((UInt16)currentCheck.stageIDX[0])
                    );
                    listOfSkyCharacters.Add(Converter.GcByte((byte)currentCheck.itemId));
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
                if (currentCheck.dataCategory.Contains("Hidden Skill"))
                {
                    listOfHiddenSkills.Add(Converter.GcByte(currentCheck.stageIDX[0]));

                    listOfHiddenSkills.Add(Converter.GcByte(currentCheck.roomIDX));
                    listOfHiddenSkills.Add(Converter.GcByte((byte)currentCheck.itemId));
                    listOfHiddenSkills.Add(Converter.GcByte(0x0)); // padding

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
                if (currentCheck.dataCategory.Contains("Shop"))
                {
                    listOfShopItems.Add(
                        Converter.GcByte(
                            int.Parse(
                                currentCheck.flag,
                                System.Globalization.NumberStyles.HexNumber
                            )
                        )
                    );
                    listOfShopItems.Add(Converter.GcByte((int)currentCheck.itemId));
                    listOfShopItems.Add(Converter.GcByte(0x0)); // padding
                    listOfShopItems.Add(Converter.GcByte(0x0)); // padding
                    count++;
                }
            }

            SeedHeaderRaw.shopCheckInfoNumEntries = count;
            SeedHeaderRaw.shopCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfShopItems;
        }

        private List<byte> ParseEventItems()
        {
            List<byte> listOfEventItems = new();
            ushort count = 0;
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.dataCategory.Contains("Event"))
                {
                    listOfEventItems.Add(Converter.GcByte((byte)currentCheck.itemId));

                    listOfEventItems.Add(Converter.GcByte((byte)currentCheck.stageIDX[0]));

                    listOfEventItems.Add(Converter.GcByte((byte)currentCheck.roomIDX));
                    listOfEventItems.Add(
                        Converter.GcByte(
                            byte.Parse(
                                currentCheck.flag,
                                System.Globalization.NumberStyles.HexNumber
                            )
                        )
                    );
                    count++;
                }
            }

            SeedHeaderRaw.eventCheckInfoNumEntries = count;
            SeedHeaderRaw.eventCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfEventItems;
        }

        private List<byte> ParseStartingItems()
        {
            SharedSettings randomizerSettings = Randomizer.SSettings;
            List<byte> listOfStartingItems = new();
            ushort count = 0;

            if (randomizerSettings.smallKeySettings == SSettings.Enums.SmallKeySettings.Keysy)
            {
                // We want to remove all small keys since they dont actually need to be given to the player
                foreach (Item sk in Randomizer.Items.RegionSmallKeys)
                {
                    randomizerSettings.startingItems.Remove(sk);
                }
            }

            foreach (Item startingItem in randomizerSettings.startingItems)
            {
                listOfStartingItems.Add(Converter.GcByte((int)startingItem));
                count++;
            }

            SeedHeaderRaw.startingItemInfoNumEntries = count;
            SeedHeaderRaw.startingItemInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfStartingItems;
        }

        private List<byte> ParseClr0Bytes()
        {
            List<byte> bytes = CLR0.CLR0.BuildClr0(fcSettings);
            return bytes;
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

        private List<byte> GenerateDebugInfoChunk(string seedId)
        {
            List<byte> debugInfoBytes = new();

            // 'w' for website. Can update this code to put 's' for standalone
            // in the future whenever that is needed.
            debugInfoBytes.AddRange(Converter.StringBytes("w")); // 0x00
            debugInfoBytes.Add(0);
            debugInfoBytes.AddRange(Converter.StringBytes("id:")); // 0x02
            debugInfoBytes.AddRange(Converter.StringBytes(seedId));
            debugInfoBytes.AddRange(Converter.StringBytes("cmt:")); // 0x10
            debugInfoBytes.AddRange(Converter.StringBytes(Global.gitCommit)); // 0x14
            // Ideally we would include the version, but this would require us
            // to go over 0x20 bytes. I think we might need to go to 0x40 at
            // that point, which would not be great. We should be able to find
            // the exact version using the git commit, so this is probably good
            // enough for now.

            while (debugInfoBytes.Count % 0x20 != 0)
            {
                debugInfoBytes.Add(0);
            }

            if (debugInfoBytes.Count > DebugInfoSize)
            {
                debugInfoBytes = debugInfoBytes.GetRange(0, DebugInfoSize);
            }

            return debugInfoBytes;
        }

        private List<byte> GenerateEntranceTable()
        {
            Console.WriteLine(seedGenResults.entrances);
            List<byte> entranceTable = new();
            string[] entranceBytes = seedGenResults.entrances.Split(",");
            for (int i = 0; i < entranceBytes.Count() - 1; i++)
            {
                Console.WriteLine("Start: " + entranceBytes[i]);
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                i++;
                entranceTable.Add(
                    Converter.GcByte(
                        byte.Parse(entranceBytes[i], System.Globalization.NumberStyles.HexNumber)
                    )
                );
                SeedHeaderRaw.shuffledEntranceInfoNumEntries++;
            }
            return entranceTable;
        }

        private List<ARCReplacement> generateStaticArcReplacements()
        {
            List<ARCReplacement> listOfStaticReplacements =
            [
                new ARCReplacement(
                    "1A62",
                    "00060064",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    0
                ), // Set Charlo Donation to check Link's wallet for 100 rupees.
                new ARCReplacement(
                    "1A84",
                    "00000064",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    0
                ), // Set Charlo Donation to increase donated amount by 100 rupees.
                new ARCReplacement(
                    "1ACC",
                    "00000064",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    0
                ), // Set Charlo Donation to remove 100 rupees from Link's wallet.
                new ARCReplacement(
                    "1ACC",
                    "00000064",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    0
                ), // Set Charlo Donation to remove 100 rupees from Link's wallet.
                new ARCReplacement(
                    "1324",
                    "00000181",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Palace_of_Twilight,
                    0
                ), // Remove the invisible wall from Palace
                new ARCReplacement(
                    "608",
                    "FF05FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Add a flag to the kak wooden shield shop item.
                /*listOfStaticReplacements.Add(
                new ARCReplacement(
                    "688",
                    "3D33FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                )
            ), // Add a flag to the kak arrows shop item.*/
                new ARCReplacement(
                    "6C8",
                    "3E3DFFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Change the flag of the Hawkeye item
                new ARCReplacement(
                    "708",
                    "3904FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Add a flag to the kak red potion shop item.
                new ARCReplacement(
                    "648",
                    "04FFFFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Change the flag of the Kak Hylian Shield sold out sign.
                new ARCReplacement(
                    "624",
                    "01478000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Change the kak Hawkeye sold out to a Hylian Shield sold out.
                new ARCReplacement(
                    "628",
                    "33FFFFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Change the flag of the new Hylian shield sold out.
                new ARCReplacement(
                    "694",
                    "01FFFFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ),
                new ARCReplacement(
                    "6A4",
                    "014B8000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ),
                new ARCReplacement(
                    "6A8",
                    "0BFFFFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Village_Interiors,
                    3
                ), // Replace kak left side red potion with a copy of the hawkeye sign.
                new ARCReplacement(
                    "11FC",
                    "3000EE63",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Sacred_Grove,
                    1
                ), // Change the statue to the past to use a custom flag so it's no longer tied to the portal
                new ARCReplacement(
                    "910",
                    "063010FF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Sacred_Grove,
                    1
                ), // Change the door to the past to use a custom flag so it's no longer tied to the portal
                new ARCReplacement(
                    "1574",
                    "80FF0000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Sacred_Grove,
                    1
                ), // Change the ms pedestal to use a custom flag so it's no longer tied to the portal
                new ARCReplacement(
                    "1094",
                    "00000000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Sacred_Grove,
                    1
                ), // Remove the fences from the grove portal fight. Area is constricted as is. no need to make things weird.
                new ARCReplacement(
                    "1BE0",
                    "00000000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Ordon_Village,
                    1
                ), // Remove Beth double actor from outside link's house. It just looks weird
                new ARCReplacement(
                    "1C00",
                    "00000000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Ordon_Village,
                    1
                ), // Remove Malo double actor from outside link's house. It just looks weird
                new ARCReplacement(
                    "5AC",
                    "00000000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.City_in_the_Sky,
                    6
                ), // Remove Argorok actor in west city, which breaks the bridge
                // Castle Town Hylian Shield Goron FLW patches
                new ARCReplacement(
                    "50C2",
                    "0001032F",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Check for custom flag before allowing player to buy hylian shield
                new ARCReplacement(
                    "50FA",
                    "0001032F",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Check to see if the flag for buying the shield has been set before continuing the conversation after buying the shield
                new ARCReplacement(
                    "50F0",
                    "0300089D",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Set the new flag for buying the shield
                new ARCReplacement(
                    "50F4",
                    "032F0000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Set the new flag for buying the shield
                new ARCReplacement(
                    "5418",
                    "C493D583",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Bulblin_Camp,
                    1
                ), // Move the spawn point from Outside AG -> camp to not be between gates.
                new ARCReplacement(
                    "1B4",
                    getStartingTime() + "0044",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    1
                ), // Set the time of day that is to be set
                new ARCReplacement(
                    "C60",
                    "0FF0FF0C",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    3
                ), // Add a flag to the Coro gate in the prologue layer
                new ARCReplacement(
                    "1B30",
                    "0FF0FF0C",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    3
                ), // Add a flag to the Coro gate in the Post FT layer
                new ARCReplacement(
                    "1004",
                    "00000000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Palace_of_Twilight,
                    0
                ), // Remove the TagYami SCOB that prevents the player from going north in PoT before collecting both sols
                // Coro FLW patches to allow him to give all items all the time
                new ARCReplacement(
                    "E24",
                    "0080013b",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from prologue state
                new ARCReplacement(
                    "288",
                    "02020001",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from post twilight state
                new ARCReplacement(
                    "28c",
                    "0080013b",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from post twilight state
                new ARCReplacement(
                    "780",
                    "02020001",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from twilight state
                new ARCReplacement(
                    "784",
                    "0080013b",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from twilight state
                new ARCReplacement(
                    "52c",
                    "00000000",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods_Interiors,
                    0
                ), // Remove original Twilight coro actr
                new ARCReplacement(
                    "808",
                    "02020001",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from post FT state
                new ARCReplacement(
                    "80c",
                    "0080013b",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Initialize new flow from post FT state
                new ARCReplacement(
                    "d60",
                    "01000125",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Allow flow state for lantern
                new ARCReplacement(
                    "d64",
                    "01a60000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Allow flow state for lantern
                new ARCReplacement(
                    "d58",
                    "03000138",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Set lantern flag
                new ARCReplacement(
                    "d5c",
                    "00800000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Set lantern flag
                new ARCReplacement(
                    "dA8",
                    "0202000c",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Check coro key flag
                new ARCReplacement(
                    "dAc",
                    "00320139",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Check coro key flag
                new ARCReplacement(
                    "d32",
                    "00c200a1",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Adjust message flow for coro key
                new ARCReplacement(
                    "53C",
                    "00b40000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Adjust message to jump past unnecessary flag check
                new ARCReplacement(
                    "E30",
                    "02020001",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Check coro bottle flag
                new ARCReplacement(
                    "E34",
                    "00da00d8",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Faron_Woods,
                    4
                ), // Check coro key flag
                new ARCReplacement(
                    "4F4",
                    "00000000",
                    (byte)FileDirectory.Stage,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.City_in_the_Sky,
                    0
                ), // Delete Savemem actr that causes the player to spawn in west wing

                // Shad FLW Patches
                new ARCReplacement(
                    "4A44",
                    "03330000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Graveyard_Interiors,
                    7
                ), // Set custom check flag
                new ARCReplacement(
                    "49C8",
                    "031108CC",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Graveyard_Interiors,
                    7
                ), // Call event017 to give player item
                new ARCReplacement(
                    "49CE",
                    "00000202",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Kakariko_Graveyard_Interiors,
                    7
                ), // Call event017 to give player item

                // // Temp change event for Transform (Midna)
                // new ARCReplacement(
                //     "4ACD8",
                //     // "032b0132",
                //     "032b0000",
                //     (byte)FileDirectory.Message,
                //     (byte)ReplacementType.Instruction,
                //     0xFE,
                //     0xFF
                // ), // for FLW index 421 (0x1a5)
                // new ARCReplacement(
                //     "4ACE0",
                //     // "032b0133",
                //     "032b0000",
                //     (byte)FileDirectory.Message,
                //     (byte)ReplacementType.Instruction,
                //     0xFE,
                //     0xFF
                // ), // for FLW index 422 (0x1a6)

                // Patch INF indexes for custom Talk to Midna hints to use the
                // blue text with Midna talking sounds.
                new ARCReplacement(
                    "18534",
                    "150d0000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    0xFE,
                    0xFF
                ), // For index 0x1373 in INF1 section (offset 0x8 in this)
                new ARCReplacement(
                    "18548",
                    "150d0000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    0xFE,
                    0xFF
                ), // For index 0x1374 in INF1 section (offset 0x8 in this)
                new ARCReplacement(
                    "1855C",
                    "150d0000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    0xFE,
                    0xFF
                ), // For index 0x1375 in INF1 section (offset 0x8 in this)
                new ARCReplacement(
                    "18570",
                    "150d0000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    0xFE,
                    0xFF
                ), // For index 0x1376 in INF1 section (offset 0x8 in this)
                new ARCReplacement(
                    "18584",
                    "150d0000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    0xFE,
                    0xFF
                ), // For index 0x1377 in INF1 section (offset 0x8 in this)

                /*
                // Note: I don't know how to modify the event system to get these items to work properly, but I already did the work on finding the replacement values, so just keeping them here.
                new ARCReplacement(
                    "3014",
                    "FF05FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Death_Mountain,
                    3
                ),
                new ARCReplacement(
                    "3950",
                    "FF05FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Death_Mountain,
                    3
                ), // Add flag to DM milk shop item

                new ARCReplacement(
                    "3034",
                    "FF28FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Death_Mountain,
                    3
                ),
                new ARCReplacement(
                    "3970",
                    "FF28FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Death_Mountain,
                    3
                ), // Add flag to DM wooden shield shop item

                new ARCReplacement(
                    "3054",
                    "FF04FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Death_Mountain,
                    3
                ),
                new ARCReplacement(
                    "3990",
                    "FF04FFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Death_Mountain,
                    3
                ), // Add flag to DM oil shop item

                new ARCReplacement(
                    "49C",
                    "FF3CFFFF",
                    (byte)FileDirectory.Room,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    0
                ), // Add flag to CT Red Potion */

                //.. ModifyChestAppearanceARC(), This is still in development
            ];

            List<ARCReplacement> listOfShopReplacements =
            [
                // Castle Town Red Potion Goron FLW patches
                new ARCReplacement(
                    "4E0A",
                    "00010330",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Check for custom flag before allowing player to buy CT red potion
                new ARCReplacement(
                    "4D52",
                    "00060028",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Instead of checking for an empty bottle, only check for rupees
                new ARCReplacement(
                    "4DF8",
                    "03000851",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Set custom event flag before proceeding in conversation
                new ARCReplacement(
                    "4DFC",
                    "03300000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Set custom event flag before proceeding in conversation
                // Castle Town Goron Shop Lantern Oil FLW patches
                new ARCReplacement(
                    "4BF2",
                    "00010331",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Check for custom flag before allowing player to buy CT lantern oil
                new ARCReplacement(
                    "4C0A",
                    "0006001E",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Instead of checking for an empty bottle, only check for rupees
                new ARCReplacement(
                    "4BF8",
                    "03000826",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Set custom event flag before proceeding in conversation
                new ARCReplacement(
                    "4BFC",
                    "03310000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town_Shops,
                    4
                ), // Set custom event flag before proceeding in conversation
                // Castle Town Goron Shop Arrow Refill FLW patches
                new ARCReplacement(
                    "4E1A",
                    "00010332",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    4
                ), // Check for custom flag before allowing player to buy CT arrows
                new ARCReplacement(
                    "4E4A",
                    "00060028",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    4
                ), // Instead of checking for the bow, only check for rupees
                new ARCReplacement(
                    "4E5A",
                    "00010332",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    4
                ), // Instead of checking for ammo, just re-check the flag
                new ARCReplacement(
                    "4FF0",
                    "03000887",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    4
                ), // Check for custom event flag before proceeding in conversation
                new ARCReplacement(
                    "4FF4",
                    "03320000",
                    (byte)FileDirectory.Message,
                    (byte)ReplacementType.Instruction,
                    (int)StageIDs.Castle_Town,
                    4
                ), // Check for custom event flag before proceeding in conversation
            ];
            if (Randomizer.SSettings.shuffleShopItems)
            {
                listOfStaticReplacements.AddRange(listOfShopReplacements);
            }
            return listOfStaticReplacements;
        }

        private static List<byte> ParseMessageIDTables(
            int currentLanguage,
            List<byte> currentMessageData,
            Dictionary<byte, List<CustomMessages.MessageEntry>> seedDictionary
        )
        {
            List<byte> listOfCustomMsgIDs = new();
            ushort count = 0;
            CustomMessageHeaderRaw.msgIdTableOffset = (ushort)(
                MessageHeaderSize + currentMessageData.Count
            );
            foreach (
                CustomMessages.MessageEntry messageEntry in seedDictionary
                    .ElementAt(currentLanguage)
                    .Value
            )
            {
                listOfCustomMsgIDs.Add(Converter.GcByte(messageEntry.stageIDX));
                listOfCustomMsgIDs.Add(Converter.GcByte(messageEntry.roomIDX));
                listOfCustomMsgIDs.AddRange(Converter.GcBytes((UInt16)messageEntry.messageID));
                count++;
            }
            CustomMessageHeaderRaw.totalEntries = count;
            return listOfCustomMsgIDs;
        }

        private static List<byte> ParseCustomMessageData(
            int currentLanguage,
            List<byte> currentMessageData,
            Dictionary<byte, List<CustomMessages.MessageEntry>> seedDictionary
        )
        {
            List<byte> listOfMsgOffsets = new();
            List<byte> listOfCustomMessages = new();

            foreach (
                CustomMessages.MessageEntry messageEntry in seedDictionary
                    .ElementAt(currentLanguage)
                    .Value
            )
            {
                listOfMsgOffsets.AddRange(Converter.GcBytes((UInt32)listOfCustomMessages.Count));
                listOfCustomMessages.AddRange(Converter.MessageStringBytes(messageEntry.message));
                listOfCustomMessages.Add(Converter.GcByte(0x0));
            }
            CustomMessageHeaderRaw.msgTableSize = (ushort)(listOfCustomMessages.Count);

            for (int i = 0; i < listOfCustomMessages.Count; i++)
            {
                listOfCustomMessages[i] ^= 0xFF;
            }

            List<byte> customMsgIDTables = new();
            customMsgIDTables.AddRange(
                ParseMessageIDTables(currentLanguage, currentMessageData, seedDictionary)
            );

            List<byte> customMessageData = new();
            customMessageData.AddRange(customMsgIDTables);
            customMessageData.AddRange(listOfMsgOffsets);
            customMessageData.AddRange(listOfCustomMessages);
            return customMessageData;
        }

        private static List<byte> GenerateMessageHeader(List<byte> messageTableInfo)
        {
            List<byte> messageHeader = new();
            messageHeader.AddRange(Converter.GcBytes((UInt16)(messageTableInfo.Count)));
            messageHeader.AddRange(messageTableInfo);

            return messageHeader;
        }

        private static List<byte> GenerateMessageTableInfo(int currentLanguage)
        {
            List<byte> messageTableInfo = new();
            messageTableInfo.AddRange(
                Converter.GcBytes((UInt16)CustomMessageHeaderRaw.totalEntries)
            );
            messageTableInfo.AddRange(
                Converter.GcBytes((UInt32)CustomMessageHeaderRaw.msgTableSize)
            );
            messageTableInfo.AddRange(
                Converter.GcBytes((UInt32)CustomMessageHeaderRaw.msgIdTableOffset)
            );

            return messageTableInfo;
        }

        private static List<ARCReplacement> ModifyChestAppearanceARC()
        {
            List<ARCReplacement> listOfArcReplacements = new();
            // Loop through all checks.
            foreach (KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList())
            {
                Check currentCheck = checkList.Value;
                if (currentCheck.dataCategory.Contains("Chest"))
                {
                    if (currentCheck.dataCategory.Contains("ARC")) // If the chest is an ARC check, so we need to add a new ARC replacement entry.
                    {
                        string offset = (
                            (UInt32)
                                uint.Parse(
                                    currentCheck.arcOffsets[0],
                                    System.Globalization.NumberStyles.HexNumber
                                ) - 0x18
                        ).ToString("X");
                        string value = "";

                        if (Randomizer.Items.RandomizedImportantItems.Contains(currentCheck.itemId))
                        {
                            value = "42300000"; // Big Blue Chest. Value is padded to a u32
                        }
                        else
                        {
                            value = "41300000"; // Small Brown Chest. Value is padded to a u32
                        }

                        listOfArcReplacements.Add(
                            new ARCReplacement(
                                offset,
                                value,
                                (byte)FileDirectory.Room,
                                (byte)ReplacementType.Instruction,
                                currentCheck.stageIDX[0],
                                currentCheck.roomIDX
                            )
                        );
                    }
                }
            }
            return listOfArcReplacements;
        }

        private static byte[] patchGCIWithSeed(char region, List<byte> seed)
        {
            List<byte> outputFile = new();
            string gciRegion = "";
            int maxRelEntries = 37;
            int id = 0x53454544;
            int previousSize = 0;
            int previousOffset = 0;
            switch (region)
            {
                case 'E':
                {
                    gciRegion = "us";
                    break;
                }
                case 'J':
                {
                    gciRegion = "jp";
                    break;
                }
                case 'P':
                {
                    gciRegion = "eu";
                    break;
                }
                default:
                {
                    gciRegion = "us";
                    break;
                }
            }
            List<byte> gciBytes = File.ReadAllBytes(
                    Global.CombineRootPath("./Assets/gci/Randomizer." + gciRegion + ".gci")
                )
                .ToList(); // read in the file as an array of bytes

            for (int i = 0; i < maxRelEntries; i++)
            {
                int offset = 0x2084 + (i * 0xC);
                int currentId = (int)(
                    gciBytes[offset] << 32
                    | gciBytes[offset + 1] << 16
                    | gciBytes[offset + 2] << 8
                    | gciBytes[offset + 3]
                );
                int relSize = (int)(
                    gciBytes[offset + 4] << 32
                    | gciBytes[offset + 5] << 16
                    | gciBytes[offset + 6] << 8
                    | gciBytes[offset + 7]
                );
                int relOffset = (int)(
                    gciBytes[offset + 8] << 32
                    | gciBytes[offset + 9] << 16
                    | gciBytes[offset + 0xA] << 8
                    | gciBytes[offset + 0xB]
                );

                if ((currentId == 0) || (relSize == 0) || (relOffset == 0))
                {
                    // Assume an empty section has been found

                    // write ID
                    gciBytes[offset] = (byte)((id & 0xFF000000) >> 24);
                    gciBytes[offset + 1] = (byte)((id & 0xFF0000) >> 16);
                    gciBytes[offset + 2] = (byte)((id & 0xFF00) >> 8);
                    gciBytes[offset + 3] = (byte)(id & 0xFF);

                    // write size as big endian
                    int bigESize = (int)SeedHeaderRaw.totalSize;
                    gciBytes[offset + 4] = (byte)((bigESize & 0xFF000000) >> 24);
                    gciBytes[offset + 5] = (byte)((bigESize & 0xFF0000) >> 16);
                    gciBytes[offset + 6] = (byte)((bigESize & 0xFF00) >> 8);
                    gciBytes[offset + 7] = (byte)(bigESize & 0xFF);

                    // create size and offset and rounded to multiple of 0x4 if needed
                    relOffset = previousSize + previousOffset;
                    while (relOffset % 0x4 != 0)
                    {
                        relOffset++;
                    }

                    gciBytes[offset + 8] = (byte)((relOffset & 0xFF000000) >> 24);
                    gciBytes[offset + 9] = (byte)((relOffset & 0xFF0000) >> 16);
                    gciBytes[offset + 10] = (byte)((relOffset & 0xFF00) >> 8);
                    gciBytes[offset + 11] = (byte)(relOffset & 0xFF);

                    // Calculate new size of gci
                    int newSize = (int)(relOffset + SeedHeaderRaw.totalSize);
                    while ((newSize - 0x40) % 0x2000 != 0)
                    {
                        newSize++;
                    }

                    while (gciBytes.Count < newSize)
                    {
                        gciBytes.Add(0);
                    }

                    // Write the file's data
                    for (int index = 0; index < seed.Count; index++)
                    {
                        int byteIndex = index + relOffset + 0x40;
                        gciBytes[byteIndex] = seed[index];
                    }

                    // update the block count
                    int blocks = gciBytes.Count / 0x2000;
                    gciBytes[0x38] = (byte)((blocks & 0xFF00) >> 8);
                    gciBytes[0x39] = (byte)(blocks & 0xFF);

                    // Update modified time

                    byte[] totalSeconds = Converter.GcBytes(
                        (UInt32)(DateTime.UtcNow - new DateTime(2000, 1, 1)).TotalSeconds
                    );

                    gciBytes[0x28] = totalSeconds[0];
                    gciBytes[0x29] = totalSeconds[1];
                    gciBytes[0x2A] = totalSeconds[2];
                    gciBytes[0x2B] = totalSeconds[3];

                    outputFile.AddRange(gciBytes);

                    break;
                }

                if (currentId == id)
                {
                    // The file being injected is already in the gci
                    Console.WriteLine("ERROR GCI Has already been patched with a seed!");
                    outputFile.AddRange(gciBytes);
                    break;
                }

                previousSize = relSize;
                previousOffset = relOffset;
            }
            return outputFile.ToArray();
        }

        //The Bgm Section will be laid out as follows: (BgmReplacementCount,
        //{bgmId,replacementId,replacementWave},{...},fanfareReplacementCount,fanfareId,replacementId,{...})
        private List<byte> GenerateBgmData()
        {
            List<byte> data = new();
            List<SoundAssets.bgmData> replacementPool = new();
            List<SoundAssets.bgmReplacement> bgmReplacementArray = new();
            if (fcSettings.randomizeBgm == RandomizeBgm.Off)
            {
                return data;
            }
            Dictionary<string, SoundAssets.bgmData> dataList = JsonConvert.DeserializeObject<
                Dictionary<string, SoundAssets.bgmData>
            >(File.ReadAllText(Global.CombineRootPath("./Assets/Sound/BackgroundMusic.jsonc")));
            if (fcSettings.randomizeBgm != RandomizeBgm.Off)
            {
                foreach (KeyValuePair<string, SoundAssets.bgmData> currentData in dataList)
                {
                    if (
                        fcSettings.randomizeBgm == RandomizeBgm.Overworld
                        && currentData.Value.sceneBgm == true
                    )
                    {
                        replacementPool.Add(currentData.Value);
                    }
                    if (
                        fcSettings.randomizeBgm == RandomizeBgm.Dungeon
                        && currentData.Value.dungeonBgm == true
                    )
                    {
                        replacementPool.Add(currentData.Value);
                    }
                    if (
                        fcSettings.randomizeBgm == RandomizeBgm.All
                        && (
                            currentData.Value.sceneBgm == true
                            || currentData.Value.bossBgm == true
                            || currentData.Value.minibossBgm == true
                            || currentData.Value.minigameBgm == true
                            || currentData.Value.eventBgm == true
                        )
                    )
                    {
                        replacementPool.Add(currentData.Value);
                    }
                }
                foreach (SoundAssets.bgmData currentData in replacementPool)
                {
                    SoundAssets.bgmReplacement replacement = new();
                    replacement.replacementBgmTrack = currentData.bgmID;
                    replacement.replacementBgmWave = currentData.bgmWave;
                    Random rnd = new();
                    while (true)
                    {
                        replacement.originalBgmTrack = replacementPool[
                            rnd.Next(replacementPool.Count)
                        ].bgmID;
                        bool foundSame = false;
                        foreach (
                            SoundAssets.bgmReplacement currentReplacement in bgmReplacementArray
                        )
                        {
                            if (currentReplacement.originalBgmTrack == replacement.originalBgmTrack)
                            {
                                foundSame = true;
                                break;
                            }
                        }
                        if (foundSame == false)
                        {
                            bool incompatible = false;
                            for (int i = 0; i < IncompatibleReplacements.GetLength(0); i++)
                            {
                                int original = IncompatibleReplacements[i, 0];
                                int replacementBgm = IncompatibleReplacements[i, 1];
                                if (original == replacement.originalBgmTrack)
                                {
                                    if (replacementBgm == replacement.replacementBgmTrack)
                                    {
                                        incompatible = true;
                                    }
                                }
                            }
                            if (!incompatible)
                            {
                                break;
                            }
                        }
                    }
                    bgmReplacementArray.Add(replacement);
                }
                if (replacementPool.Count != bgmReplacementArray.Count)
                {
                    Console.WriteLine(
                        "BGM Pool ("
                            + replacementPool.Count
                            + ") and Replacement ("
                            + bgmReplacementArray.Count
                            + ") have different lengths!"
                    );
                }
                foreach (SoundAssets.bgmReplacement currentReplacement in bgmReplacementArray)
                {
                    data.Add((byte)currentReplacement.originalBgmTrack);
                    data.Add((byte)currentReplacement.replacementBgmTrack);
                    data.Add((byte)currentReplacement.replacementBgmWave);
                    data.Add((byte)0x0); // Padding
                }
            }
            SeedHeaderRaw.bgmInfoNumEntries = (byte)bgmReplacementArray.Count;
            return data;
        }

        private List<byte> GenerateFanfareData()
        {
            List<byte> data = new();
            List<SoundAssets.bgmData> replacementPool = new();
            List<SoundAssets.bgmReplacement> fanfareReplacementArray = new();
            if (fcSettings.randomizeFanfares)
            {
                Dictionary<string, SoundAssets.bgmData> dataList = JsonConvert.DeserializeObject<
                    Dictionary<string, SoundAssets.bgmData>
                >(File.ReadAllText(Global.CombineRootPath("./Assets/Sound/BackgroundMusic.jsonc")));
                foreach (KeyValuePair<string, SoundAssets.bgmData> currentData in dataList)
                {
                    if (currentData.Value.isFanfare == true && currentData.Value.bgmWave == 0)
                    {
                        replacementPool.Add(currentData.Value);
                    }
                }
                foreach (SoundAssets.bgmData currentData in replacementPool)
                {
                    SoundAssets.bgmReplacement replacement = new();
                    replacement.replacementBgmTrack = currentData.bgmID;
                    replacement.replacementBgmWave = 0;
                    Random rnd = new();
                    while (true)
                    {
                        replacement.originalBgmTrack = replacementPool[
                            rnd.Next(replacementPool.Count)
                        ].bgmID;
                        bool foundSame = false;
                        foreach (
                            SoundAssets.bgmReplacement currentReplacement in fanfareReplacementArray
                        )
                        {
                            if (currentReplacement.originalBgmTrack == replacement.originalBgmTrack)
                            {
                                foundSame = true;
                                break;
                            }
                        }
                        if (foundSame == false)
                        {
                            break;
                        }
                    }
                    fanfareReplacementArray.Add(replacement);
                }
                if (replacementPool.Count != fanfareReplacementArray.Count)
                {
                    Console.WriteLine(
                        "Fanfare Pool ("
                            + replacementPool.Count
                            + ") and Replacement ("
                            + fanfareReplacementArray.Count
                            + ") have different lengths!"
                    );
                }
                foreach (SoundAssets.bgmReplacement currentReplacement in fanfareReplacementArray)
                {
                    data.Add((byte)currentReplacement.originalBgmTrack);
                    data.Add((byte)currentReplacement.replacementBgmTrack);
                    data.Add((byte)0x0); // Padding
                    data.Add((byte)0x0); // Padding
                }
            }
            SeedHeaderRaw.fanfareInfoNumEntries = (byte)fanfareReplacementArray.Count;

            return data;
        }

        private List<byte> GenerateBmg0SectionData(CustomMsgData customMsgData)
        {
            List<byte> allData = new();
            List<byte> bodyData = new();

            // Maybe should combine everything into one table.
            // - map signId to FLI value (what the sign starts on).

            // DONE - map FLI value to initial FLW index
            // - signs flows are static. Midna flow is dynamic depending on how
            // many text boxes she should have.

            // - map FLI value + FLW index to INF index (also happens for Midna)
            // These mappings would be entirely static. We control what node a
            // custom sign starts on using its flow, and we control what node
            // Midna starts on using the FLI to initial FLW mapping.

            // !!!!!!!!! NOTE: this will use the mask, so:
            // [0xFFF0, 0x7700, 0x24, 0x1369]
            // [0xFFF0, 0x7700, 0x25, 0x136A]
            // ...
            // [0xFFF0, 0x7710, 0x24, 0x136E]
            // [0xFFF0, 0x7710, 0x25, 0x136F]
            // [0xFFF0, 0x7710, 0x26, 0x1370]
            // ...
            // [0xFFFF, 0x0BB8, 0x24, 0x1372]
            // [0xFFFF, 0x0BB8, 0x25, 0x1373]
            // etc.
            // Mask is good because it should speed up execution time and it
            // also reduces the data size. And we would have to use 8 bytes per
            // entry anyway, so doesn't cost any space.

            // TODO: figure out the range of INF1 indexes we can work with for
            // this. Start lower than 0x1369 + how high can we go?

            // 0x1369 might be the lower bound? Not sure how Lunar found this
            // value, but we can only go maybe 2 earlier at most? Index 4999 is
            // 0x1387, so we have 31 values we can use right now.

            // 0x1369 => 182842 offset in DAT1 data
            // 0x1387 => 182872
            // Each is 1 greater than the last, and each points to a dead byte.
            // Maybe there is a way to use fake values here since we are
            // translating to a custom string anyway, but don't super need to
            // worry about that right now.
            // So we can have 31 different values right now, and we are going to
            // use 15 of them.
            // I would put Talk To Midna at 0x1369 through 0x136d inclusive.

            // Note: we use the mask because even if we start on flow 0x7703
            // (for 3 groups of text boxes: 0x26,0x27,0x28), once we change to
            // FLW node 0x27, we need to still map this to a custom INF index
            // instead of the one defined on the FLW entry.

            // 0x7700
            // 0x7701
            // 0x7702
            // 0x7703
            // 0x7704
            // 0x7710 2nd sign - first of 5 nodes
            // 0x7711 2nd sign - 2nd of 5 nodes
            // 0x7712 2nd sign - 3rd of 5 nodes
            // 0x7713 2nd sign - 4th of 5 nodes
            // 0x7714 2nd sign - 5th of 5 nodes

            // ^ these are values which need to map to initial FLW indexes.

            // The game will already fall back to doing nothing if there is no
            // mapping for the FLI?

            // data.AddRange(Converter.GcBytes((UInt16)0xbb8)); // FLI
            // data.AddRange(Converter.GcBytes((UInt16)0x25)); // FLW
            // data.AddRange(Converter.GcBytes((UInt16)0x136d)); // INF
            // data.Add(Converter.GcByte(0)); // bool
            // data.Add(Converter.GcByte(0)); // padding

            // List<(UInt16, UInt16, UInt16)> aa = new() {
            //     (0x7700, 0x24, 0x1369),
            //     (0x7701, 0x25, 0x136A),
            //     (0x7702, 0x26, 0x136B),
            //     (0x7703, 0x27, 0x136C),
            //     (0x7704, 0x28, 0x136D),
            //     (0x7710, 0x24, 0x1369),
            //     (0x7711, 0x25, 0x136A),
            //     (0x7712, 0x26, 0x136B),
            //     (0x7713, 0x27, 0x136C),
            //     (0x7714, 0x28, 0x136D),
            // };

            // u16 - signToFliOffset
            // u16 - numSignToFliEntries
            // u16 - flwIdxRemapOffset
            // u16 - numFlwIdxRemapEntries
            // u16 - infRemapOffset
            // u16 - numInfRemapEntries

            ushort headerSize = 0x38;

            StringTableResult2.Header header = customMsgData.AddBytesGenHeader(headerSize, bodyData);

            // Build header
            allData.AddRange(Converter.MessageStringBytes("BMG0")); // 0x00

            // TODO: there is probably an issue with converting the foundIndex
            // in the table with the value. We might need to add all ctx and
            // then all basic? We get an absolute index when searching in a
            // wordComp table for example, and we need to convert this into an
            // effectiveIndex in the entities table.

            // We might need a diff to apply to the absolute index stored
            // somewhere.

            // Maybe when we do the binarySearch, we pass a pointer to our first
            // element in the table slice as if it was the table, set the
            // startIdx to 0, and pass the same length. Then when we get a result, we add it to the 

            // Note, our foundIndex will never be earlier than the finalIndex.
            // It can easily be way later. It cannot be earlier though because
            // at best we start our slice at the start of the table. What we can do it store 

            // To convert a ctx foundIndex to its final index, we always
            // subtract the startIdx of the ctx data for that comp in the table.
            // So if we find a value at index 25 and our data starts at 10, our
            // final index is 25 - 10 => 15.

            // To convert a basic foundIndex to its final index, we need to do
            // this: foundIndex - basicStartIdx + ctxCompsLength. foundIndex -
            // basicStartIdx will be at least 0. ctxCompsLength will be at least
            // zero. One could be greater than the other? That is true. So our
            // diff could be positive or negative. The result is guaranteed to
            // be at least 0 though once we add everything together.
            allData.AddRange(Converter.GcBytes(header.entityInfoTableOffset)); // 0x04
            allData.AddRange(Converter.GcBytes(header.tableSliceInfosOffset)); // 0x06
            allData.AddRange(Converter.GcBytes(header.wordCompValsOffset)); // 0x08
            allData.AddRange(Converter.GcBytes(header.shortCompValsOffset)); // 0x0a
            allData.AddRange(Converter.GcBytes(header.nodeRemapTableOffset)); // 0x0c
            allData.AddRange(Converter.GcBytes(header.branchPatchTableOffset)); // 0x0e
            allData.AddRange(Converter.GcBytes(header.branchNextNodeBaseIdxTableOffset)); // 0x10
            allData.AddRange(Converter.GcBytes(header.branchNextNodeTableOffset)); // 0x12
            allData.AddRange(Converter.GcBytes(header.eventNextNodeTableOffset)); // 0x14
            allData.AddRange(Converter.GcBytes(header.eventPatchTableOffset)); // 0x16
            allData.AddRange(Converter.GcBytes(header.strOffsetTableOffset)); // 0x18
            allData.AddRange(Converter.GcBytes(header.strTableOffset)); // 0x1a
            allData.AddRange(Converter.GcBytes(header.strTableEncodedStart)); // 0x1c
            allData.AddRange(Converter.GcBytes(header.encodedStrTableNumBlocks)); // 0x1e
            for (int i = 0; i < 4; i++) // 0x20
            {
                allData.AddRange(Converter.GcBytes(header.encryptionKey[i]));
            }
            allData.Add(0); // 0x30

            while (allData.Count < headerSize)
                allData.Add(0);

            if (allData.Count != headerSize)
                throw new Exception($"Invalid headerSize for bmg0 block: '{allData.Count}'");

            // Add bodyData
            allData.AddRange(bodyData);

            // Align to 8 bytes
            while (allData.Count % 8 != 0)
            {
                allData.Add(0);
            }

            return allData;
        }


        private static string getStartingTime()
        {
            string time = "203F"; // By default, starting time is in the evening
            switch (Randomizer.SSettings.startingToD)
            {
                case StartingToD.Morning:
                {
                    time = "700F"; // Set time to 105
                    break;
                }
                case StartingToD.Noon:
                {
                    time = "C00F"; // Set time to 180
                    break;
                }
                case StartingToD.Night:
                {
                    time = "000F"; // Set time to 0
                    break;
                }
            }
            return time;
        }

        private static readonly int[,] IncompatibleReplacements = new int[,]
        {
            //Original, Replacement
            { 62, 148 }, // Armogohma Phase 1 overwriting Palace Theme
            { 62, 98 }, // Zant Boss Theme overwriting Palace Theme
            { 44, 8 }, // Ook Battle Music overwriting House Interiors
            { 55, 8 }, // Ook Battle Music overwriting Snowpeak Ruins
        };

        private class SeedHeader
        {
            public UInt16 versionMajor { get; set; } // SeedData version major
            public UInt16 versionMinor { get; set; } // SeedData version minor
            public UInt16 headerSize { get; set; } // Total size of the header in bytes
            public UInt16 dataSize { get; set; } // Total number of bytes in the check data
            public UInt32 totalSize { get; set; } // Total number of bytes in the gci after the comments
            public UInt16 volatilePatchInfoNumEntries { get; set; } // bitArray where each bit represents a patch/modification to be applied for this playthrough
            public UInt16 volatilePatchInfoDataOffset { get; set; }
            public UInt16 oneTimePatchInfoNumEntries { get; set; } // bitArray where each bit represents a patch/modification to be applied for this playthrough
            public UInt16 oneTimePatchInfoDataOffset { get; set; }
            public UInt16 flagBitfieldInfoNumEntries { get; set; } // bitArray where each bit represents a patch/modification to be applied for this playthrough
            public UInt16 flagBitfieldInfoDataOffset { get; set; }
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
            public UInt16 eventCheckInfoNumEntries { get; set; }
            public UInt16 eventCheckInfoDataOffset { get; set; }
            public UInt16 startingItemInfoNumEntries { get; set; }
            public UInt16 startingItemInfoDataOffset { get; set; }
            public UInt16 shuffledEntranceInfoNumEntries { get; set; }
            public UInt16 shuffledEntranceInfoDataOffset { get; set; }
            public UInt16 bgmInfoNumEntries { get; set; }
            public UInt16 bgmInfoDataOffset { get; set; }
            public UInt16 fanfareInfoNumEntries { get; set; }
            public UInt16 fanfareInfoDataOffset { get; set; }
            public UInt16 sfxInfoNumEntries { get; set; }
            public UInt16 sfxInfoDataOffset { get; set; }
            public UInt16 clr0Offset { get; set; }
            public UInt16 bmg0Offset { get; set; }
            public UInt16 customTextHeaderSize { get; set; }
            public UInt16 customTextHeaderOffset { get; set; }
        }

        internal class CustomTextHeader
        {
            public UInt16 headerSize { get; set; }
            public UInt16 totalEntries { get; set; }
            public UInt32 msgTableSize { get; set; }
            public UInt32 msgIdTableOffset { get; set; }
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

    public class ARCReplacement
    {
        private string offset;
        private string replacementValue;
        private byte directory;
        private int replacementType;
        private int stageIDX;
        private int roomID;

        public ARCReplacement(
            string offset,
            string replacementValue,
            byte directory,
            int replacementType,
            int stageIDX,
            int roomID
        )
        {
            this.offset = offset;
            this.replacementValue = replacementValue;
            this.directory = directory;
            this.replacementType = replacementType;
            this.stageIDX = stageIDX;
            this.roomID = roomID;
        }

        public string Offset
        {
            get { return offset; }
            set { offset = value; }
        } // The offset where the item is stored from the message flow header.
        public string ReplacementValue
        {
            get { return replacementValue; }
            set { replacementValue = value; }
        } // Used to be item, but can be more now.
        public byte Directory
        {
            get { return directory; }
            set { directory = value; }
        } // The type of directory where the check is stored.
        public int ReplacementType
        {
            get { return replacementType; }
            set { replacementType = value; }
        } // The type of replacement that is taking place.
        public int StageIDX
        {
            get { return stageIDX; }
            set { stageIDX = value; }
        } // The name of the file where the check is stored
        public int RoomID
        {
            get { return roomID; }
            set { roomID = value; }
        } // The room number for chests/room based dzr checks.
    }

    public class RELReplacement
    {
        private int offset;
        private int replacementValue;
        private int replacementType;
        private int stageIDX;
        private int moduleID;

        public RELReplacement(
            int replacementType,
            int stageIDX,
            int moduleID,
            int offset,
            int replacementValue
        )
        {
            this.ReplacementType = replacementType;
            this.StageIDX = stageIDX;
            this.ModuleID = moduleID;
            this.Offset = offset;
            this.ReplacementValue = replacementValue;
        }

        public int ReplacementType
        {
            get { return replacementType; }
            set { replacementType = value; }
        } // The type of replacement that is taking place.
        public int StageIDX
        {
            get { return stageIDX; }
            set { stageIDX = value; }
        } // The name of the file where the check is stored
        public int ModuleID
        {
            get { return moduleID; }
            set { moduleID = value; }
        } // The ID for the rel to be patched

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        } // The offset where the item is stored from the message flow header.
        public int ReplacementValue
        {
            get { return replacementValue; }
            set { replacementValue = value; }
        } // Used to be item, but can be more now.
        
        
    }

    enum FileDirectory : byte
    {
        Room = 0x0,
        Message = 0x1,
        Object = 0x2,
        Stage = 0x3,
    };

    enum ReplacementType : byte
    {
        Item = 0x0, // Standard item replacement
        HiddenSkill = 0x1, // Hidden Skill checks check for the room last loaded into.
        ItemMessage = 0x2, // Replaces messages for item IDs
        Instruction = 0x3, // Replaces a u32 instruction
        AlwaysLoaded = 0x4, // Replaces values specifically in the bmgres archive which is always loaded.
        MessageResource = 0x5, // Replaces values in the MESG section of a bmgres archive file.
    };

    enum GCRelIDs
    {
        F_PC_PROFILE_LST = 0x001,
        D_A_ANDSW,         // 0x002
        D_A_BG,            // 0x003
        D_A_BG_OBJ,        // 0x004
        D_A_DMIDNA,        // 0x005
        D_A_DOOR_DBDOOR00, // 0x006
        D_A_DOOR_KNOB00,   // 0x007
        D_A_DOOR_SHUTTER,  // 0x008
        D_A_DOOR_SPIRAL,   // 0x009
        D_A_DSHUTTER,      // 0x00A
        D_A_EP,            // 0x00B
        D_A_HITOBJ,        // 0x00C
        D_A_KYTAG00,       // 0x00D
        D_A_KYTAG04,       // 0x00E
        D_A_KYTAG17,       // 0x00F
        D_A_OBJ_BRAKEEFF,       // 0x010
        D_A_OBJ_BURNBOX,        // 0x011
        D_A_OBJ_CARRY,          // 0x012
        D_A_OBJ_ITO,            // 0x013
        D_A_OBJ_MOVEBOX,        // 0x014
        D_A_OBJ_SWPUSH,         // 0x015
        D_A_OBJ_TIMER,          // 0x016
        D_A_PATH_LINE,          // 0x017
        D_A_SCENE_EXIT,         // 0x018
        D_A_SET_BGOBJ,          // 0x019
        D_A_SWHIT0,             // 0x01A
        D_A_TAG_ALLMATO,        // 0x01B
        D_A_TAG_CAMERA,         // 0x01C
        D_A_TAG_CHKPOINT,       // 0x01D
        D_A_TAG_EVENT,          // 0x01E
        D_A_TAG_EVT,            // 0x01F
        D_A_TAG_EVTAREA,        // 0x020
        D_A_TAG_EVTMSG,         // 0x021
        D_A_TAG_HOWL,           // 0x022
        D_A_TAG_KMSG,           // 0x023
        D_A_TAG_LANTERN,        // 0x024
        D_A_TAG_MIST,           // 0x025
        D_A_TAG_MSG,            // 0x026
        D_A_TAG_PUSH,           // 0x027
        D_A_TAG_TELOP,          // 0x028
        D_A_TBOX,               // 0x029
        D_A_TBOX2,              // 0x02A
        D_A_VRBOX,              // 0x02B
        D_A_VRBOX2,             // 0x02C
        D_A_ARROW,              // 0x02D
        D_A_BOOMERANG,          // 0x02E
        D_A_CROD,               // 0x02F
        D_A_DEMO00,             // 0x030
        D_A_DISAPPEAR,          // 0x031
        D_A_MG_ROD,             // 0x032
        D_A_MIDNA,              // 0x033
        D_A_NBOMB,              // 0x034
        D_A_OBJ_LIFE_CONTAINER, // 0x035
        D_A_OBJ_YOUSEI,         // 0x036
        D_A_SPINNER,            // 0x037
        D_A_SUSPEND,            // 0x038
        D_A_TAG_ATTENTION,      // 0x039
        D_A_ALLDIE,             // 0x03A
        D_A_ANDSW2,             // 0x03B
        D_A_BD,                 // 0x03C
        D_A_CANOE,              // 0x03D
        D_A_CSTAF,              // 0x03E
        D_A_DEMO_ITEM,          // 0x03F
        D_A_DOOR_BOSSL1,        // 0x040
        D_A_E_DN,               // 0x041
        D_A_E_FM,               // 0x042
        D_A_E_GA,               // 0x043
        D_A_E_HB,               // 0x044
        D_A_E_NEST,             // 0x045
        D_A_E_RD,               // 0x046
        D_A_ECONT,              // 0x047
        D_A_FR,                 // 0x048
        D_A_GRASS,              // 0x049
        D_A_KYTAG05,            // 0x04A
        D_A_KYTAG10,            // 0x04B
        D_A_KYTAG11,            // 0x04C
        D_A_KYTAG14,            // 0x04D
        D_A_MG_FISH,            // 0x04E
        D_A_NPC_BESU,           // 0x04F
        D_A_NPC_FAIRY_SEIREI,   // 0x050
        D_A_NPC_FISH,           // 0x051
        D_A_NPC_HENNA,          // 0x052
        D_A_NPC_KAKASHI,        // 0x053
        D_A_NPC_KKRI,           // 0x054
        D_A_NPC_KOLIN,          // 0x055
        D_A_NPC_MARO,           // 0x056
        D_A_NPC_TARO,           // 0x057
        D_A_NPC_TKJ,            // 0x058
        D_A_OBJ_BHASHI,         // 0x059
        D_A_OBJ_BKDOOR,         // 0x05A
        D_A_OBJ_BOSSWARP,       // 0x05B
        D_A_OBJ_CBOARD,         // 0x05C
        D_A_OBJ_DIGPLACE,       // 0x05D
        D_A_OBJ_EFF,            // 0x05E
        D_A_OBJ_FMOBJ,          // 0x05F
        D_A_OBJ_GPTARU,         // 0x060
        D_A_OBJ_HHASHI,         // 0x061
        D_A_OBJ_KANBAN2,        // 0x062
        D_A_OBJ_KBACKET, // 0x063
        D_A_OBJ_KGATE,          // 0x064
        D_A_OBJ_KLIFT00,        // 0x065
        D_A_OBJ_KTONFIRE,       // 0x066
        D_A_OBJ_LADDER,         // 0x067
        D_A_OBJ_LV2CANDLE,      // 0x068
        D_A_OBJ_MAGNE_ARM,      // 0x069
        D_A_OBJ_METALBOX,       // 0x06A
        D_A_OBJ_MGATE,          // 0x06B
        D_A_OBJ_NAMEPLATE,      // 0x06C
        D_A_OBJ_ORNAMENT_CLOTH, // 0x06D
        D_A_OBJ_ROPE_BRIDGE,    // 0x06E
        D_A_OBJ_SWALLSHUTTER,   // 0x06F
        D_A_OBJ_STICK,          // 0x070
        D_A_OBJ_STONEMARK,      // 0x071
        D_A_OBJ_SWPROPELLER,    // 0x072
        D_A_OBJ_SWPUSH5,        // 0x073
        D_A_OBJ_YOBIKUSA,       // 0x074
        D_A_SCENE_EXIT2,        // 0x075
        D_A_SHOP_ITEM,          // 0x076
        D_A_SQ,                 // 0x077
        D_A_SWC00,              // 0x078
        D_A_TAG_CSTASW,         // 0x079
        D_A_TAG_AJNOT,          // 0x07A
        D_A_TAG_ATTACK_ITEM,    // 0x07B
        D_A_TAG_GSTART,         // 0x07C
        D_A_TAG_HINIT,          // 0x07D
        D_A_TAG_HJUMP,          // 0x07E
        D_A_TAG_HSTOP,          // 0x07F
        D_A_TAG_LV2PRCHK,       // 0x080
        D_A_TAG_MAGNE,          // 0x081
        D_A_TAG_MHINT,          // 0x082
        D_A_TAG_MSTOP,          // 0x083
        D_A_TAG_SPRING,         // 0x084
        D_A_TAG_STATUE_EVT,     // 0x085
        D_A_YKGR,               // 0x086
        D_A_L7DEMO_DR,          // 0x087
        D_A_L7LOW_DR,           // 0x088
        D_A_L7OP_DEMO_DR,       // 0x089
        D_A_B_BH,               // 0x08A
        D_A_B_BQ,               // 0x08B
        D_A_B_DR,               // 0x08C
        D_A_B_DRE,              // 0x08D
        D_A_B_DS,               // 0x08E
        D_A_B_GG,               // 0x08F
        D_A_B_GM,               // 0x090
        D_A_B_GND,              // 0x091
        D_A_B_GO,               // 0x092
        D_A_B_GOS,              // 0x093
        D_A_B_MGN,              // 0x094
        D_A_B_OB,               // 0x095
        D_A_B_OH,               // 0x096
        D_A_B_OH2,              // 0x097
        D_A_B_TN,               // 0x098
        D_A_B_YO,               // 0x099
        D_A_B_YO_ICE,           // 0x09A
        D_A_B_ZANT,             // 0x09B
        D_A_B_ZANT_MAGIC,       // 0x09C
        D_A_B_ZANT_MOBILE,      // 0x09D
        D_A_B_ZANT_SIMA,        // 0x09E
        D_A_BALLOON_2D,         // 0x09F
        D_A_BULLET,             // 0x0A0
        D_A_COACH_2D,           // 0x0A1
        D_A_COACH_FIRE,         // 0x0A2
        D_A_COW,                // 0x0A3
        D_A_CSTATUE,            // 0x0A4
        D_A_DO,                 // 0x0A5
        D_A_DOOR_BOSS,          // 0x0A6
        D_A_DOOR_BOSSL5,        // 0x0A7
        D_A_DOOR_MBOSSL1,       // 0x0A8
        D_A_DOOR_PUSH,          // 0x0A9
        D_A_E_AI,               // 0x0AA
        D_A_E_ARROW,            // 0x0AB
        D_A_E_BA,               // 0x0AC
        D_A_E_BEE,              // 0x0AD
        D_A_E_BG,               // 0x0AE
        D_A_E_BI,               // 0x0AF
        D_A_E_BI_LEAF,          // 0x0B0
        D_A_E_BS,               // 0x0B1
        D_A_E_BU,               // 0x0B2
        D_A_E_BUG,              // 0x0B3
        D_A_E_CR,               // 0x0B4
        D_A_E_CR_EGG,           // 0x0B5
        D_A_E_DB,               // 0x0B6
        D_A_E_DB_LEAF,          // 0x0B7
        D_A_E_DD,               // 0x0B8
        D_A_E_DF,               // 0x0B9
        D_A_E_DK,               // 0x0BA
        D_A_E_DT,               // 0x0BB
        D_A_E_FB,               // 0x0BC
        D_A_E_FK,               // 0x0BD
        D_A_E_FS,               // 0x0BE
        D_A_E_FZ,               // 0x0BF
        D_A_E_GB,               // 0x0C0
        D_A_E_GE,               // 0x0C1
        D_A_E_GI,               // 0x0C2
        D_A_E_GM,               // 0x0C3
        D_A_E_GOB,              // 0x0C4
        D_A_E_GS,               // 0x0C5
        D_A_E_HB_LEAF,          // 0x0C6
        D_A_E_HM,               // 0x0C7
        D_A_E_HP,               // 0x0C8
        D_A_E_HZ,               // 0x0C9
        D_A_E_HZELDA,           // 0x0CA
        D_A_E_IS,               // 0x0CB
        D_A_E_KG,               // 0x0CC
        D_A_E_KK,               // 0x0CD
        D_A_E_KR,               // 0x0CE
        D_A_E_MB,               // 0x0CF
        D_A_E_MD,               // 0x0D0
        D_A_E_MF,               // 0x0D1
        D_A_E_MK,               // 0x0D2
        D_A_E_MK_BO,            // 0x0D3
        D_A_E_MM,               // 0x0D4
        D_A_E_MM_MT,            // 0x0D5
        D_A_E_MS,               // 0x0D6
        D_A_E_NZ,               // 0x0D7
        D_A_E_OC,               // 0x0D8
        D_A_E_OCT_BG,           // 0x0D9
        D_A_E_OT,               // 0x0DA
        D_A_E_PH,               // 0x0DB
        D_A_E_PM,               // 0x0DC
        D_A_E_PO,               // 0x0DD
        D_A_E_PZ,               // 0x0DE
        D_A_E_RB,               // 0x0DF
        D_A_E_RDB,              // 0x0E0
        D_A_E_RDY,              // 0x0E1
        D_A_E_S1,               // 0x0E2
        D_A_E_SB,               // 0x0E3
        D_A_E_SF,               // 0x0E4
        D_A_E_SG,               // 0x0E5
        D_A_E_SH,               // 0x0E6
        D_A_E_SM,               // 0x0E7
        D_A_E_SM2,              // 0x0E8
        D_A_E_ST,               // 0x0E9
        D_A_E_ST_LINE,          // 0x0EA
        D_A_E_SW,               // 0x0EB
        D_A_E_TH,               // 0x0EC
        D_A_E_TH_BALL,          // 0x0ED
        D_A_E_TK,               // 0x0EE
        D_A_E_TK2,              // 0x0EF
        D_A_E_TK_BALL,          // 0x0F0
        D_A_E_TT,               // 0x0F1
        D_A_E_VT,               // 0x0F2
        D_A_E_WARPAPPEAR,       // 0x0F3
        D_A_E_WB,               // 0x0F4
        D_A_E_WS,               // 0x0F5
        D_A_E_WW,               // 0x0F6
        D_A_E_YC,               // 0x0F7
        D_A_E_YD,               // 0x0F8
        D_A_E_YD_LEAF,          // 0x0F9
        D_A_E_YG,               // 0x0FA
        D_A_E_YH,               // 0x0FB
        D_A_E_YK,               // 0x0FC
        D_A_E_YM,               // 0x0FD
        D_A_E_YM_TAG,           // 0x0FE
        D_A_E_YMB,              // 0x0FF
        D_A_E_YR,               // 0x100
        D_A_E_ZH,               // 0x101
        D_A_E_ZM,               // 0x102
        D_A_E_ZS,               // 0x103
        D_A_FORMATION_MNG,      // 0x104
        D_A_GUARD_MNG,          // 0x105
        D_A_HORSE,              // 0x106
        D_A_HOZELDA,            // 0x107
        D_A_IZUMI_GATE,         // 0x108
        D_A_KAGO,               // 0x109
        D_A_KYTAG01,            // 0x10A
        D_A_KYTAG02,            // 0x10B
        D_A_KYTAG03,            // 0x10C
        D_A_KYTAG06,            // 0x10D
        D_A_KYTAG07,            // 0x10E
        D_A_KYTAG08,            // 0x10F
        D_A_KYTAG09,            // 0x110
        D_A_KYTAG12,            // 0x111
        D_A_KYTAG13,            // 0x112
        D_A_KYTAG15,            // 0x113
        D_A_KYTAG16,            // 0x114
        D_A_MANT,               // 0x115
        D_A_MG_FSHOP,           // 0x116
        D_A_MIRROR,             // 0x117
        D_A_MOVIE_PLAYER,       // 0x118
        D_A_MYNA,               // 0x119
        D_A_NI,                 // 0x11A
        D_A_NPC_ARU,            // 0x11B
        D_A_NPC_ASH,            // 0x11C
        D_A_NPC_ASHB,           // 0x11D
        D_A_NPC_BANS,           // 0x11E
        D_A_NPC_BLUE_NS,        // 0x11F
        D_A_NPC_BOU,            // 0x120
        D_A_NPC_BOUS,           // 0x121
        D_A_NPC_CDN3,           // 0x122
        D_A_NPC_CHAT,           // 0x123
        D_A_NPC_CHIN,           // 0x124
        D_A_NPC_CLERKA,         // 0x125
        D_A_NPC_CLERKB,         // 0x126
        D_A_NPC_CLERKT,         // 0x127
        D_A_NPC_COACH,          // 0x128
        D_A_NPC_DF,             // 0x129
        D_A_NPC_DOC,            // 0x12A
        D_A_NPC_DOORBOY,        // 0x12B
        D_A_NPC_DRAINSOL,       // 0x12C
        D_A_NPC_DU,             // 0x12D
        D_A_NPC_FAIRY,          // 0x12E
        D_A_NPC_FGUARD,         // 0x12F
        D_A_NPC_GND,            // 0x130
        D_A_NPC_GRA,            // 0x131
        D_A_NPC_GRC,            // 0x132
        D_A_NPC_GRD,            // 0x133
        D_A_NPC_GRM,            // 0x134
        D_A_NPC_GRMC,           // 0x135
        D_A_NPC_GRO,            // 0x136
        D_A_NPC_GRR,            // 0x137
        D_A_NPC_GRS,            // 0x138
        D_A_NPC_GRZ,            // 0x139
        D_A_NPC_GUARD,          // 0x13A
        D_A_NPC_GWOLF,          // 0x13B
        D_A_NPC_HANJO,          // 0x13C
        D_A_NPC_HENNA0,         // 0x13D
        D_A_NPC_HOZ,            // 0x13E
        D_A_NPC_IMPAL,          // 0x13F
        D_A_NPC_INKO,           // 0x140
        D_A_NPC_INS,            // 0x141
        D_A_NPC_JAGAR,          // 0x142
        D_A_NPC_KASI_HANA,      // 0x143
        D_A_NPC_KASI_KYU,       // 0x144
        D_A_NPC_KASI_MICH,      // 0x145
        D_A_NPC_KDK,            // 0x146
        D_A_NPC_KN,             // 0x147
        D_A_NPC_KNJ,            // 0x148
        D_A_NPC_KOLINB,         // 0x149
        D_A_NPC_KS,             // 0x14A
        D_A_NPC_KYURY,          // 0x14B
        D_A_NPC_LEN,            // 0x14C
        D_A_NPC_LF,             // 0x14D
        D_A_NPC_LUD,            // 0x14E
        D_A_NPC_MIDP,           // 0x14F
        D_A_NPC_MK,             // 0x150
        D_A_NPC_MOI,            // 0x151
        D_A_NPC_MOIR,           // 0x152
        D_A_NPC_MYNA2,          // 0x153
        D_A_NPC_NE,             // 0x154
        D_A_NPC_P2,             // 0x155
        D_A_NPC_PACHI_BESU,     // 0x156
        D_A_NPC_PACHI_MARO,     // 0x157
        D_A_NPC_PACHI_TARO,     // 0x158
        D_A_NPC_PASSER,         // 0x159
        D_A_NPC_PASSER2,        // 0x15A
        D_A_NPC_POST,           // 0x15B
        D_A_NPC_POUYA,          // 0x15C
        D_A_NPC_PRAYER,         // 0x15D
        D_A_NPC_RACA,           // 0x15E
        D_A_NPC_RAFREL,         // 0x15F
        D_A_NPC_SARU,           // 0x160
        D_A_NPC_SEIB,           // 0x161
        D_A_NPC_SEIC,           // 0x162
        D_A_NPC_SEID,           // 0x163
        D_A_NPC_SEIRA,          // 0x164
        D_A_NPC_SEIRA2,         // 0x165
        D_A_NPC_SEIREI,         // 0x166
        D_A_NPC_SHAD,           // 0x167
        D_A_NPC_SHAMAN,         // 0x168
        D_A_NPC_SHOE,           // 0x169
        D_A_NPC_SHOP0,          // 0x16A
        D_A_NPC_SHOP_MARO,      // 0x16B
        D_A_NPC_SOLA,           // 0x16C
        D_A_NPC_SOLDIERA,       // 0x16D
        D_A_NPC_SOLDIERB,       // 0x16E
        D_A_NPC_SQ,             // 0x16F
        D_A_NPC_THE,            // 0x170
        D_A_NPC_THEB,           // 0x171
        D_A_NPC_TK,             // 0x172
        D_A_NPC_TKC,            // 0x173
        D_A_NPC_TKJ2,           // 0x174
        D_A_NPC_TKS,            // 0x175
        D_A_NPC_TOBY,           // 0x176
        D_A_NPC_TR,             // 0x177
        D_A_NPC_URI,            // 0x178
        D_A_NPC_WORM,           // 0x179
        D_A_NPC_WRESTLER,       // 0x17A
        D_A_NPC_YAMID,          // 0x17B
        D_A_NPC_YAMIS,          // 0x17C
        D_A_NPC_YAMIT,          // 0x17D
        D_A_NPC_YELIA,          // 0x17E
        D_A_NPC_YKM,            // 0x17F
        D_A_NPC_YKW,            // 0x180
        D_A_NPC_ZANB,           // 0x181
        D_A_NPC_ZANT,           // 0x182
        D_A_NPC_ZELR,           // 0x183
        D_A_NPC_ZELRO,          // 0x184
        D_A_NPC_ZELDA,          // 0x185
        D_A_NPC_ZRA,            // 0x186
        D_A_NPC_ZRC,            // 0x187
        D_A_NPC_ZRZ,            // 0x188
        D_A_OBJ_LV5KEY,         // 0x189
        D_A_OBJ_TURARA,         // 0x18A
        D_A_OBJ_TVCDLST,        // 0x18B
        D_A_OBJ_Y_TAIHOU,       // 0x18C
        D_A_OBJ_AMISHUTTER,     // 0x18D
        D_A_OBJ_ARI,            // 0x18E
        D_A_OBJ_AUTOMATA,       // 0x18F
        D_A_OBJ_AVALANCHE,      // 0x190
        D_A_OBJ_BALLOON,        // 0x191
        D_A_OBJ_BARDESK,        // 0x192
        D_A_OBJ_BATTA,          // 0x193
        D_A_OBJ_BBOX,           // 0x194
        D_A_OBJ_BED,            // 0x195
        D_A_OBJ_BEMOS,          // 0x196
        D_A_OBJ_BHBRIDGE,       // 0x197
        D_A_OBJ_BK_LEAF,        // 0x198
        D_A_OBJ_BKY_ROCK,       // 0x199
        D_A_OBJ_BMWINDOW,       // 0x19A
        D_A_OBJ_BMSHUTTER,      // 0x19B
        D_A_OBJ_BOMBF,          // 0x19C
        D_A_OBJ_BOUMATO,        // 0x19D
        D_A_OBJ_BRG,            // 0x19E
        D_A_OBJ_BSGATE,         // 0x19F
        D_A_OBJ_BUBBLEPILAR,    // 0x1A0
        D_A_OBJ_CATDOOR,        // 0x1A1
        D_A_OBJ_CB,             // 0x1A2
        D_A_OBJ_CBLOCK,         // 0x1A3
        D_A_OBJ_CDOOR,          // 0x1A4
        D_A_OBJ_CHANDELIER,     // 0x1A5
        D_A_OBJ_CHEST,          // 0x1A6
        D_A_OBJ_CHO,            // 0x1A7
        D_A_OBJ_COWDOOR,        // 0x1A8
        D_A_OBJ_CROPE,          // 0x1A9
        D_A_OBJ_CRVFENCE,       // 0x1AA
        D_A_OBJ_CRVGATE,        // 0x1AB
        D_A_OBJ_CRVHAHEN,       // 0x1AC
        D_A_OBJ_CRVLH_DOWN,     // 0x1AD
        D_A_OBJ_CRVLH_UP,       // 0x1AE
        D_A_OBJ_CRVSTEEL,       // 0x1AF
        D_A_OBJ_CRYSTAL,        // 0x1B0
        D_A_OBJ_CWALL,          // 0x1B1
        D_A_OBJ_DAMCPS,         // 0x1B2
        D_A_OBJ_DAN,            // 0x1B3
        D_A_OBJ_DIGHOLL,        // 0x1B4
        D_A_OBJ_DIGSNOW,        // 0x1B5
        D_A_OBJ_DMELEVATOR,     // 0x1B6
        D_A_OBJ_DROP,           // 0x1B7
        D_A_OBJ_DUST,           // 0x1B8
        D_A_OBJ_ENEMY_CREATE,   // 0x1B9
        D_A_OBJ_FALLOBJ,        // 0x1BA
        D_A_OBJ_FAN,            // 0x1BB
        D_A_OBJ_FCHAIN,         // 0x1BC
        D_A_OBJ_FIREWOOD,       // 0x1BD
        D_A_OBJ_FIREWOOD2,      // 0x1BE
        D_A_OBJ_FIREPILLAR,     // 0x1BF
        D_A_OBJ_FIREPILLAR2,    // 0x1C0
        D_A_OBJ_FLAG,           // 0x1C1
        D_A_OBJ_FLAG2,          // 0x1C2
        D_A_OBJ_FLAG3,          // 0x1C3
        D_A_OBJ_FOOD,           // 0x1C4
        D_A_OBJ_FW,             // 0x1C5
        D_A_OBJ_GADGET,         // 0x1C6
        D_A_OBJ_GANONWALL,      // 0x1C7
        D_A_OBJ_GANONWALL2,     // 0x1C8
        D_A_OBJ_GB,             // 0x1C9
        D_A_OBJ_GEYSER,         // 0x1CA
        D_A_OBJ_GLOWSPHERE,     // 0x1CB
        D_A_OBJ_GM,             // 0x1CC
        D_A_OBJ_GOGATE,         // 0x1CD
        D_A_OBJ_GOMIKABE,       // 0x1CE
        D_A_OBJ_GRA2,           // 0x1CF
        D_A_OBJ_GRAWALL,        // 0x1D0
        D_A_OBJ_GRA_ROCK,       // 0x1D1
        D_A_OBJ_GRAVE_STONE,    // 0x1D2
        D_A_OBJ_GROUNDWATER,    // 0x1D3
        D_A_OBJ_GRZ_ROCK,       // 0x1D4
        D_A_OBJ_H_SAKU,         // 0x1D5
        D_A_OBJ_HAKAI_BRL,      // 0x1D6
        D_A_OBJ_HAKAI_FTR,      // 0x1D7
        D_A_OBJ_HASU2,          // 0x1D8
        D_A_OBJ_HATA,           // 0x1D9
        D_A_OBJ_HB,             // 0x1DA
        D_A_OBJ_HBOMBKOYA,      // 0x1DB
        D_A_OBJ_HEAVYSW,        // 0x1DC
        D_A_OBJ_HFUTA,          // 0x1DD
        D_A_OBJ_HSTARGET,       // 0x1DE
        D_A_OBJ_ICE_L,          // 0x1DF
        D_A_OBJ_ICE_S,          // 0x1E0
        D_A_OBJ_ICEBLOCK,       // 0x1E1
        D_A_OBJ_ICELEAF,        // 0x1E2
        D_A_OBJ_IHASI,          // 0x1E3
        D_A_OBJ_IKADA,          // 0x1E4
        D_A_OBJ_INOBONE,        // 0x1E5
        D_A_OBJ_ITA,            // 0x1E6
        D_A_OBJ_ITAMATO,        // 0x1E7
        D_A_OBJ_KABUTO,         // 0x1E8
        D_A_OBJ_KAG,            // 0x1E9
        D_A_OBJ_KAGE,           // 0x1EA
        D_A_OBJ_KAGO,           // 0x1EB
        D_A_OBJ_KAISOU,         // 0x1EC
        D_A_OBJ_KAMAKIRI,       // 0x1ED
        D_A_OBJ_KANTERA,        // 0x1EE
        D_A_OBJ_KATATSUMURI,    // 0x1EF
        D_A_OBJ_KAZENEKO,       // 0x1F0
        D_A_OBJ_KBOX,             // 0x1F1
        D_A_OBJ_KEY,              // 0x1F2
        D_A_OBJ_KEYHOLE,          // 0x1F3
        D_A_OBJ_KI,               // 0x1F4
        D_A_OBJ_KIPOT,            // 0x1F5
        D_A_OBJ_KITA,             // 0x1F6
        D_A_OBJ_KJGJS,            // 0x1F7
        D_A_OBJ_KKANBAN,          // 0x1F8
        D_A_OBJ_KNBULLET,         // 0x1F9
        D_A_OBJ_KSHUTTER,         // 0x1FA
        D_A_OBJ_KUWAGATA,         // 0x1FB
        D_A_OBJ_KWHEEL00,         // 0x1FC
        D_A_OBJ_KWHEEL01,         // 0x1FD
        D_A_OBJ_KZNKARM,          // 0x1FE
        D_A_OBJ_LAUNDRY,          // 0x1FF
        D_A_OBJ_LAUNDRY_ROPE,     // 0x200
        D_A_OBJ_LBOX,             // 0x201
        D_A_OBJ_LP,               // 0x202
        D_A_OBJ_LV1CANDLE00,      // 0x203
        D_A_OBJ_LV1CANDLE01,      // 0x204
        D_A_OBJ_LV3CANDLE,        // 0x205
        D_A_OBJ_LV3WATER,         // 0x206
        D_A_OBJ_LV3WATER2,        // 0x207
        D_A_OBJ_LV3WATERB,        // 0x208
        D_A_OBJ_LV3SAKA00,        // 0x209
        D_A_OBJ_LV3WATEREFF,      // 0x20A
        D_A_OBJ_LV4CANDLEDEMOTAG, // 0x20B
        D_A_OBJ_LV4CANDLETAG,     // 0x20C
        D_A_OBJ_LV4EDSHUTTER,     // 0x20D
        D_A_OBJ_LV4GATE,          // 0x20E
        D_A_OBJ_LV4HSTARGET,      // 0x20F
        D_A_OBJ_LV4POGATE,        // 0x210
        D_A_OBJ_LV4RAILWALL,      // 0x211
        D_A_OBJ_LV4SLIDEWALL,     // 0x212
        D_A_OBJ_LV4BRIDGE,        // 0x213
        D_A_OBJ_LV4CHANDELIER,    // 0x214
        D_A_OBJ_LV4DIGSAND,       // 0x215
        D_A_OBJ_LV4FLOOR,         // 0x216
        D_A_OBJ_LV4GEAR,          // 0x217
        D_A_OBJ_LV4PRELVTR,       // 0x218
        D_A_OBJ_LV4PRWALL,        // 0x219
        D_A_OBJ_LV4SAND,          // 0x21A
        D_A_OBJ_LV5FLOORBOARD,    // 0x21B
        D_A_OBJ_LV5ICEWALL,       // 0x21C
        D_A_OBJ_LV5SWICE,         // 0x21D
        D_A_OBJ_LV5YCHNDLR,       // 0x21E
        D_A_OBJ_LV5YIBLLTRAY,     // 0x21F
        D_A_OBJ_LV6CHANGEGATE,    // 0x220
        D_A_OBJ_LV6FURIKOTRAP,    // 0x221
        D_A_OBJ_LV6LBLOCK,        // 0x222
        D_A_OBJ_LV6SWGATE,        // 0x223
        D_A_OBJ_LV6SZGATE,        // 0x224
        D_A_OBJ_LV6TENBIN,        // 0x225
        D_A_OBJ_LV6TOGEROLL,      // 0x226
        D_A_OBJ_LV6TOGETRAP,      // 0x227
        D_A_OBJ_LV6BEMOS,         // 0x228
        D_A_OBJ_LV6BEMOS2,        // 0x229
        D_A_OBJ_LV6EGATE,         // 0x22A
        D_A_OBJ_LV6ELEVTA,        // 0x22B
        D_A_OBJ_LV6SWTURN,        // 0x22C
        D_A_OBJ_LV7BSGATE,        // 0x22D
        D_A_OBJ_LV7PROPELLERY,    // 0x22E
        D_A_OBJ_LV7BRIDGE,        // 0x22F
        D_A_OBJ_LV8KEKKAITRAP,    // 0x230
        D_A_OBJ_LV8LIFT,          // 0x231
        D_A_OBJ_LV8OPTILIFT,      // 0x232
        D_A_OBJ_LV8UDFLOOR,       // 0x233
        D_A_OBJ_LV9SWSHUTTER,     // 0x234
        D_A_OBJ_MAGLIFT,          // 0x235
        D_A_OBJ_MAGLIFTROT,       // 0x236
        D_A_OBJ_MAKI,             // 0x237
        D_A_OBJ_MASTER_SWORD,     // 0x238
        D_A_OBJ_MATO,             // 0x239
        D_A_OBJ_MHOLE,            // 0x23A
        D_A_OBJ_MIE,              // 0x23B
        D_A_OBJ_MIRROR_6POLE,     // 0x23C
        D_A_OBJ_MIRROR_CHAIN,     // 0x23D
        D_A_OBJ_MIRROR_SAND,      // 0x23E
        D_A_OBJ_MIRROR_SCREW,     // 0x23F
        D_A_OBJ_MIRROR_TABLE,     // 0x240
        D_A_OBJ_MSIMA,            // 0x241
        D_A_OBJ_MVSTAIR,          // 0x242
        D_A_OBJ_MYOGAN,           // 0x243
        D_A_OBJ_NAGAISU,          // 0x244
        D_A_OBJ_NAN,              // 0x245
        D_A_OBJ_NDOOR,            // 0x246
        D_A_OBJ_NOUGU,            // 0x247
        D_A_OBJ_OCTHASHI,         // 0x248
        D_A_OBJ_OILTUBO,          // 0x249
        D_A_OBJ_ONSEN,            // 0x24A
        D_A_OBJ_ONSENFIRE,        // 0x24B
        D_A_OBJ_ONSENTARU,        // 0x24C
        D_A_OBJ_PDOOR,            // 0x24D
        D_A_OBJ_PDTILE,           // 0x24E
        D_A_OBJ_PDWALL,           // 0x24F
        D_A_OBJ_PICTURE,          // 0x250
        D_A_OBJ_PILLAR,           // 0x251
        D_A_OBJ_PLEAF,            // 0x252
        D_A_OBJ_POCANDLE,         // 0x253
        D_A_OBJ_POFIRE,           // 0x254
        D_A_OBJ_POTBOX,           // 0x255
        D_A_OBJ_PROP,             // 0x256
        D_A_OBJ_PUMPKIN,          // 0x257
        D_A_OBJ_RCIRCLE,          // 0x258
        D_A_OBJ_RFHOLE,           // 0x259
        D_A_OBJ_RGATE,            // 0x25A
        D_A_OBJ_RIVERROCK,        // 0x25B
        D_A_OBJ_ROCK,             // 0x25C
        D_A_OBJ_ROTBRIDGE,        // 0x25D
        D_A_OBJ_ROTTRAP,          // 0x25E
        D_A_OBJ_ROTEN,            // 0x25F
        D_A_OBJ_RSTAIR,           // 0x260
        D_A_OBJ_RW,               // 0x261
        D_A_OBJ_SAIDAN,           // 0x262
        D_A_OBJ_SAKUITA,          // 0x263
        D_A_OBJ_SAKUITA_ROPE,     // 0x264
        D_A_OBJ_SCANNON,          // 0x265
        D_A_OBJ_SCANNON_CRS,      // 0x266
        D_A_OBJ_SCANNON_TEN,      // 0x267
        D_A_OBJ_SEKIDOOR,         // 0x268
        D_A_OBJ_SEKIZO,           // 0x269
        D_A_OBJ_SEKIZOA,          // 0x26A
        D_A_OBJ_SHIELD,           // 0x26B
        D_A_OBJ_SM_DOOR,          // 0x26C
        D_A_OBJ_SMALLKEY,         // 0x26D
        D_A_OBJ_SMGDOOR,          // 0x26E
        D_A_OBJ_SMOKE,            // 0x26F
        D_A_OBJ_SMTILE,           // 0x270
        D_A_OBJ_SMW_STONE,        // 0x271
        D_A_OBJ_SNOWEFFTAG,       // 0x272
        D_A_OBJ_SNOW_SOUP,        // 0x273
        D_A_OBJ_SO,               // 0x274
        D_A_OBJ_SPINLIFT,         // 0x275
        D_A_OBJ_SS_DRINK,         // 0x276
        D_A_OBJ_SS_ITEM,          // 0x277
        D_A_OBJ_STAIRBLOCK,       // 0x278
        D_A_OBJ_STONE,            // 0x279
        D_A_OBJ_STOPPER,          // 0x27A
        D_A_OBJ_STOPPER2,         // 0x27B
        D_A_OBJ_SUISYA,           // 0x27C
        D_A_OBJ_SW,               // 0x27D
        D_A_OBJ_SWBALLA,          // 0x27E
        D_A_OBJ_SWBALLB,          // 0x27F
        D_A_OBJ_SWBALLC,          // 0x280
        D_A_OBJ_SWLIGHT,          // 0x281
        D_A_OBJ_SWCHAIN,          // 0x282
        D_A_OBJ_SWHANG,           // 0x283
        D_A_OBJ_SWORD,            // 0x284
        D_A_OBJ_SWPUSH2,          // 0x285
        D_A_OBJ_SWSPINNER,        // 0x286
        D_A_OBJ_SWTURN,           // 0x287
        D_A_OBJ_SYROCK,           // 0x288
        D_A_OBJ_SZBRIDGE,         // 0x289
        D_A_OBJ_TAFENCE,          // 0x28A
        D_A_OBJ_TABLE,            // 0x28B
        D_A_OBJ_TAKARADAI,        // 0x28C
        D_A_OBJ_TATIGI,           // 0x28D
        D_A_OBJ_TEN,              // 0x28E
        D_A_OBJ_TESTCUBE,         // 0x28F
        D_A_OBJ_TGAKE,            // 0x290
        D_A_OBJ_THASHI,           // 0x291
        D_A_OBJ_THDOOR,           // 0x292
        D_A_OBJ_TIMEFIRE,         // 0x293
        D_A_OBJ_TKS,              // 0x294
        D_A_OBJ_TMOON,            // 0x295
        D_A_OBJ_TOARU_MAKI,       // 0x296
        D_A_OBJ_TOBY,             // 0x297
        D_A_OBJ_TOBYHOUSE,        // 0x298
        D_A_OBJ_TOGETRAP,         // 0x299
        D_A_OBJ_TOMBO,            // 0x29A
        D_A_OBJ_TORNADO,          // 0x29B
        D_A_OBJ_TORNADO2,         // 0x29C
        D_A_OBJ_TP,               // 0x29D
        D_A_OBJ_TREESH,           // 0x29E
        D_A_OBJ_TWGATE,           // 0x29F
        D_A_OBJ_UDOOR,            // 0x2A0
        D_A_OBJ_USAKU,            // 0x2A1
        D_A_OBJ_VGROUND,          // 0x2A2
        D_A_OBJ_VOLCBALL,         // 0x2A3
        D_A_OBJ_VOLCBOM,          // 0x2A4
        D_A_OBJ_WARP_KBRG,        // 0x2A5
        D_A_OBJ_WARP_OBRG,        // 0x2A6
        D_A_OBJ_WATERGATE,        // 0x2A7
        D_A_OBJ_WATERPILLAR,      // 0x2A8
        D_A_OBJ_WATERFALL,        // 0x2A9
        D_A_OBJ_WCHAIN,           // 0x2AA
        D_A_OBJ_WDSTICK,          // 0x2AB
        D_A_OBJ_WEB0,             // 0x2AC
        D_A_OBJ_WEB1,             // 0x2AD
        D_A_OBJ_WELL_COVER,       // 0x2AE
        D_A_OBJ_WFLAG,            // 0x2AF
        D_A_OBJ_WIND_STONE,       // 0x2B0
        D_A_OBJ_WINDOW,           // 0x2B1
        D_A_OBJ_WOOD_PENDULUM,    // 0x2B2
        D_A_OBJ_WOOD_STATUE,      // 0x2B3
        D_A_OBJ_WSWORD,           // 0x2B4
        D_A_OBJ_YEL_BAG,          // 0x2B5
        D_A_OBJ_YSTONE,           // 0x2B6
        D_A_OBJ_ZCLOTH,           // 0x2B7
        D_A_OBJ_ZDOOR,            // 0x2B8
        D_A_OBJ_ZRTURARA,         // 0x2B9
        D_A_OBJ_ZRTURARAROCK,     // 0x2BA
        D_A_OBJ_ZRAMARK,          // 0x2BB
        D_A_OBJ_ZRA_FREEZE,       // 0x2BC
        D_A_OBJ_ZRA_ROCK,         // 0x2BD
        D_A_PASSER_MNG,           // 0x2BE
        D_A_PERU,                 // 0x2BF
        D_A_PPOLAMP,              // 0x2C0
        D_A_SKIP_2D,              // 0x2C1
        D_A_STARTANDGOAL,         // 0x2C2
        D_A_SWBALL,               // 0x2C3
        D_A_SWLBALL,              // 0x2C4
        D_A_SWTIME,               // 0x2C5
        D_A_TAG_LV6GATE,          // 0x2C6
        D_A_TAG_LV7GATE,          // 0x2C7
        D_A_TAG_LV8GATE,          // 0x2C8
        D_A_TAG_TWGATE,           // 0x2C9
        D_A_TAG_ARENA,            // 0x2CA
        D_A_TAG_ASSISTANCE,       // 0x2CB
        D_A_TAG_BOTTLE_ITEM,      // 0x2CC
        D_A_TAG_CHGRESTART,       // 0x2CD
        D_A_TAG_CSW,              // 0x2CE
        D_A_TAG_ESCAPE,           // 0x2CF
        D_A_TAG_FIREWALL,         // 0x2D0
        D_A_TAG_GRA,              // 0x2D1
        D_A_TAG_GUARD,            // 0x2D2
        D_A_TAG_INSTRUCTION,      // 0x2D3
        D_A_TAG_KAGO_FALL,        // 0x2D4
        D_A_TAG_LIGHTBALL,        // 0x2D5
        D_A_TAG_LV5SOUP,          // 0x2D6
        D_A_TAG_LV6CSTASW,        // 0x2D7
        D_A_TAG_MMSG,             // 0x2D8
        D_A_TAG_MWAIT,            // 0x2D9
        D_A_TAG_MYNA2,            // 0x2DA
        D_A_TAG_MYNA_LIGHT,       // 0x2DB
        D_A_TAG_PACHI,            // 0x2DC
        D_A_TAG_POFIRE,           // 0x2DD
        D_A_TAG_QS,               // 0x2DE
        D_A_TAG_RET_ROOM,         // 0x2DF
        D_A_TAG_RIVER_BACK,       // 0x2E0
        D_A_TAG_RMBIT_SW,         // 0x2E1
        D_A_TAG_SCHEDULE,         // 0x2E2
        D_A_TAG_SETBALL,          // 0x2E3
        D_A_TAG_SETRESTART,       // 0x2E4
        D_A_TAG_SHOP_CAMERA,      // 0x2E5
        D_A_TAG_SHOP_ITEM,        // 0x2E6
        D_A_TAG_SMK_EMT,          // 0x2E7
        D_A_TAG_SPINNER,          // 0x2E8
        D_A_TAG_SPPATH,           // 0x2E9
        D_A_TAG_SS_DRINK,         // 0x2EA
        D_A_TAG_STREAM,           // 0x2EB
        D_A_TAG_THEB_HINT,        // 0x2EC
        D_A_TAG_WARA_HOWL,        // 0x2ED
        D_A_TAG_WATCHGE,          // 0x2EE
        D_A_TAG_WATERFALL,        // 0x2EF
        D_A_TAG_WLJUMP,           // 0x2F0
        D_A_TAG_YAMI,             // 0x2F1
        D_A_TALK,                 // 0x2F2
        D_A_TBOXSW,               // 0x2F3
        D_A_TITLE,                // 0x2F4
        D_A_WARP_BUG,             // 0x2F5
    };
}
