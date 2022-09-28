namespace TPRandomizer.Assets.CLR0
{
    using System;
    using System.Collections.Generic;

    class Clr0EntryComparer : IComparer<Clr0Entry>
    {
        int IComparer<Clr0Entry>.Compare(Clr0Entry a, Clr0Entry b)
        {
            // Cast to `int` so we can get negative results.
            return (int)a.recolorId - (int)b.recolorId;
        }
    }

    public class CLR0
    {
        // Maps nibble value to how many bits it has set.
        private static readonly byte[] NIBBLE_LOOKUP =
        {
            0,
            1,
            1,
            2,
            1,
            2,
            2,
            3,
            1,
            2,
            2,
            3,
            2,
            3,
            3,
            4
        };

        public static List<byte> BuildClr0(FileCreationSettings fcSettings)
        {
            List<Clr0Entry> entries = new();

            entries.Add(fcSettings.aBtnColor);
            entries.Add(fcSettings.bBtnColor);
            // entries.Add(new RgbEntry(Assets.CLR0.RecolorId.HerosClothes, 0x99, 0x44, 0x88));
            entries.Add(fcSettings.tunicColor);

            entries.RemoveAll(entry => entry == null);
            entries.Sort(new Clr0EntryComparer());

            return GenBytes(entries);
        }

        private static List<byte> GenBytes(List<Clr0Entry> entries)
        {
            int bitTableOffset = 0;
            int bitTableLen = 0;
            UInt16 minEnum = 0;
            UInt16 maxEnum = 0;
            int cummCountTableOffset = 0;
            int cummCountTableLen = 0;
            int basicDataOffset = 0;
            int complexDataOffset = 0;
            byte[] bitTable = new byte[1];
            UInt16[] cummCountTable = new UInt16[1];
            List<UInt32> basicData = new();
            List<byte> complexData = new();
            int totalByteLen = 0x16;

            if (entries.Count > 0)
            {
                bitTableOffset = 0x16;
                minEnum = (UInt16)entries[0].recolorId;
                maxEnum = (UInt16)entries[entries.Count - 1].recolorId;

                cummCountTableLen = (maxEnum - minEnum) / 8;
                bitTableLen = cummCountTableLen + 1;

                bitTable = new byte[bitTableLen];
                if (cummCountTableLen > 0)
                {
                    cummCountTable = new UInt16[cummCountTableLen];
                }

                foreach (Clr0Entry clr0Entry in entries)
                {
                    UInt16 diff = (UInt16)(clr0Entry.recolorId - minEnum);

                    int bitTableIndex = diff / 8;
                    byte bitNum = (byte)(diff % 8);
                    bitTable[bitTableIndex] |= (byte)(1 << bitNum);

                    Clr0Result result = clr0Entry.getResult();

                    UInt32 basicDataEntry;

                    if (result.complexBytes != null)
                    {
                        // Has complex data
                        UInt32 currentComplexDataLen = (UInt32)(complexData.Count);
                        if (currentComplexDataLen > 0x00FFFFFF)
                        {
                            throw new Exception(
                                "Trying to create a CLR0 basicDataEntry which is not able to store the offset to the complexData."
                            );
                        }

                        basicDataEntry = currentComplexDataLen;

                        complexData.AddRange(result.complexBytes);
                    }
                    else
                    {
                        basicDataEntry = result.basicDataEntry;
                    }

                    basicDataEntry = basicDataEntry & 0x00FFFFFF;
                    basicDataEntry |= (UInt32)((byte)clr0Entry.recolorType << 24);

                    basicData.Add(basicDataEntry);
                }

                // Determine offsets
                cummCountTableOffset = bitTableOffset + bitTableLen;
                basicDataOffset = cummCountTableOffset + cummCountTableLen * 2;
                complexDataOffset = basicDataOffset + basicData.Count * 4;

                totalByteLen = complexDataOffset + complexData.Count;

                if (cummCountTableLen < 1)
                {
                    cummCountTableOffset = 0;
                }
                else
                {
                    UInt16 cummulativeCount = 0;

                    for (int i = 0; i < cummCountTableLen; i++)
                    {
                        cummulativeCount += GetNumBitsSet(bitTable[i]);
                        cummCountTable[i] = cummulativeCount;
                    }
                }

                if (basicData.Count < 1)
                    basicDataOffset = 0;

                if (complexData.Count < 1)
                    complexDataOffset = 0;
            }

            List<byte> finalBytes = new();
            finalBytes.AddRange(Converter.StringBytes("CLR0", 4)); // 0x00, "CLR0"
            finalBytes.AddRange(Converter.GcBytes((UInt32)totalByteLen)); // 0x04, u32, totalBytes
            finalBytes.Add(Converter.GcByte(0x00)); // 0x08, u8, reserved; always 0 for now
            // 0x09, u8, bitTableOffset; Normally 0x16, but 0 if no entries.
            finalBytes.Add(Converter.GcByte(bitTableOffset));
            finalBytes.AddRange(Converter.GcBytes((UInt16)minEnum)); // 0x0A, u16, minEnum
            finalBytes.AddRange(Converter.GcBytes((UInt16)maxEnum)); // 0x0C, u16, maxEnum
            finalBytes.AddRange(Converter.GcBytes((UInt16)cummCountTableOffset)); // 0x0E, u16, cummCountTableOffset
            finalBytes.AddRange(Converter.GcBytes((UInt32)complexDataOffset)); // 0x10, u32, complexDataOffset
            finalBytes.AddRange(Converter.GcBytes((UInt16)basicDataOffset)); // 0x14, u16, basicDataOffset

            if (bitTableLen > 0)
            {
                finalBytes.AddRange(bitTable);
            }

            if (cummCountTableLen > 0)
            {
                for (int i = 0; i < cummCountTableLen; i++)
                {
                    finalBytes.AddRange(Converter.GcBytes((UInt16)cummCountTable[i]));
                }
            }

            if (basicData.Count > 0)
            {
                for (int i = 0; i < basicData.Count; i++)
                {
                    finalBytes.AddRange(Converter.GcBytes((UInt32)basicData[i]));
                }
            }

            if (complexData.Count > 0)
            {
                finalBytes.AddRange(complexData);
            }

            return finalBytes;
        }

        private static byte GetNumBitsSet(byte byteVal)
        {
            return (byte)(NIBBLE_LOOKUP[byteVal & 0x0f] + NIBBLE_LOOKUP[byteVal >> 4]);
        }
    }
}
