namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using TPRandomizer.FcSettings.Enums;
    using Newtonsoft.Json;
    using TPRandomizer.Assets.CLR0;
    using System.ComponentModel.DataAnnotations.Schema;

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
            Assets.CustomMessages.MessageLanguage hintLanguage = Assets.CustomMessages.MessageLanguage.English;
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

            
            List<CustomMessages.MessageEntry> seedMessages = seedGenResults.customMsgData.GenMessageEntries();

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
            if (dataBytes !=null)
            {
                SeedHeaderRaw.shuffledEntranceInfoDataOffset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = GenerateBgmData();
            if (dataBytes !=null)
            {
                SeedHeaderRaw.bgmInfoDataOffset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = GenerateFanfareData();
            if (dataBytes !=null)
            {
                SeedHeaderRaw.fanfareInfoDataOffset = (UInt16)GCIDataRaw.Count();
                GCIDataRaw.AddRange(dataBytes);
            }

            dataBytes = ParseClr0Bytes();
            if (dataBytes !=null)
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
                    seedHeader.AddRange(
                        (List<byte>)headerObject.GetValue(SeedHeaderRaw, null)
                    );
                }
            }

            seedHeader.Add(Converter.GcByte((int)randomizerSettings.castleRequirements));
            seedHeader.Add(Converter.GcByte((int)randomizerSettings.palaceRequirements));
            int mapBits = 0;
            bool[] mapFlags = new bool[]
            {
                false,
                randomizerSettings.skipSnowpeakEntrance,
                false,
                randomizerSettings.lanayruTwilightCleared,
                randomizerSettings.eldinTwilightCleared,
                randomizerSettings.faronTwilightCleared,
                false,
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
            seedHeader.Add(Converter.GcByte((int)randomizerSettings.startingToD));

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
                randomizerSettings.openMap //map bits
            };
            bool[] oneTimePatchSettingsArray =
            {
                randomizerSettings.increaseWallet,
                randomizerSettings.fastIronBoots,
                fcSettings.disableEnemyBgm,
                randomizerSettings.instantText,
                randomizerSettings.skipMajorCutscenes,
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

            List<bool[]> flagArrayList = new() { volatilePatchSettingsArray, oneTimePatchSettingsArray, flagsBitfieldArray};
            SeedHeaderRaw.volatilePatchInfoNumEntries = (ushort)volatilePatchSettingsArray.Length; 
            SeedHeaderRaw.oneTimePatchInfoNumEntries = (ushort)oneTimePatchSettingsArray.Length; 
            SeedHeaderRaw.flagBitfieldInfoNumEntries = (ushort)flagsBitfieldArray.Length;
            ushort dataOffset = (ushort)CheckDataRaw.Count;
            SeedHeaderRaw.volatilePatchInfoDataOffset = dataOffset;
            SeedHeaderRaw.oneTimePatchInfoDataOffset = (ushort)(dataOffset + 0x10);
            SeedHeaderRaw.flagBitfieldInfoDataOffset = (ushort)(dataOffset + 0x20);

            foreach(bool[] flagArr in flagArrayList)
            {
                List<byte> listOfFlags = new() { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}; // Align to 16 bytes
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

            List<KeyValuePair<int, int>> midnaHairBytes = BuildMidnaHairBytes();
            foreach (KeyValuePair<int, int> pair in midnaHairBytes)
            {
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt16)0x3)); // replacement type
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt16)0xFF)); // stageIDX
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt32)0x33)); // moduleID
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt32)pair.Key)); // offset
                listOfRELReplacements.AddRange(Converter.GcBytes((UInt32)pair.Value));
                count++;
            }

            SeedHeaderRaw.relCheckInfoNumEntries = count;
            SeedHeaderRaw.relCheckInfoDataOffset = (ushort)(CheckDataRaw.Count);
            return listOfRELReplacements;
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
                new(0xA448, fcSettings.midnaHairTipsLightWorldActive << 8)
            };
        }

        private int[] MidnaGlowToInts(int glowRgb)
        {
            // returns 00RR00GG 00BB0000
            int[] ret = { (glowRgb & 0xFF0000) | (glowRgb & 0xFF00) >> 8, (glowRgb & 0xFF) << 16 };
            return ret;
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
                if (!randomizerSettings.startingItems.Contains(Item.Gerudo_Desert_Bulblin_Camp_Key))
                {
                    randomizerSettings.startingItems.Add(Item.Gerudo_Desert_Bulblin_Camp_Key);
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
                MessageHeaderSize
                + currentMessageData.Count
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
            CustomMessageHeaderRaw.msgTableSize = (ushort)(
                listOfCustomMessages.Count
            );

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
                Converter.GcBytes(
                    (UInt16)CustomMessageHeaderRaw.totalEntries
                )
            );
            messageTableInfo.AddRange(
                Converter.GcBytes(
                    (UInt32)CustomMessageHeaderRaw.msgTableSize
                )
            );
            messageTableInfo.AddRange(
                Converter.GcBytes(
                    (UInt32)CustomMessageHeaderRaw.msgIdTableOffset
                )
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
            List<byte> gciBytes = File.ReadAllBytes("/app/generator/Assets/gci/Randomizer." + gciRegion + ".gci").ToList(); // read in the file as an array of bytes

            for (int i = 0; i < maxRelEntries; i++)
            {
                int offset = 0x2084 + (i * 0xC);
                int currentId = (int)(gciBytes[offset] << 32 | gciBytes[offset + 1] << 16 | gciBytes[offset + 2] << 8 | gciBytes[offset + 3]);
                int relSize = (int)(gciBytes[offset+4] << 32 | gciBytes[offset + 5] << 16 | gciBytes[offset + 6] << 8 | gciBytes[offset + 7]);
                int relOffset = (int)(gciBytes[offset+8] << 32 | gciBytes[offset + 9] << 16 | gciBytes[offset + 0xA] << 8 | gciBytes[offset + 0xB]);

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
                    for( int index = 0; index < seed.Count; index++)
                    {
                        int byteIndex = index + relOffset + 0x40;
                        gciBytes[byteIndex] = seed[index];
                    }

                    // update the block count
                    int blocks = gciBytes.Count / 0x2000;
                    gciBytes[0x38] = (byte)((blocks & 0xFF00) >> 8);
                    gciBytes[0x39] = (byte)(blocks & 0xFF);

                    // Update modified time

                    int totalSeconds = BitConverter.ToInt32(Converter.GcBytes((UInt32)(DateTime.UtcNow - new DateTime(2000, 1, 1)).TotalSeconds));

                    gciBytes[0x28] = (byte)((totalSeconds & 0xFF000000) >> 24);
                    gciBytes[0x29] = (byte)((totalSeconds & 0xFF0000) >> 16);
                    gciBytes[0x2A] = (byte)((totalSeconds & 0xFF00) >> 8);
                    gciBytes[0x2B] = (byte)(totalSeconds & 0xFF);

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
                        foreach (SoundAssets.bgmReplacement currentReplacement in bgmReplacementArray)
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
                        foreach (SoundAssets.bgmReplacement currentReplacement in fanfareReplacementArray)
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
}
