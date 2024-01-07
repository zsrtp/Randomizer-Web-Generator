namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TPRandomizer.Util;

    /// <summary>
    /// summary text.
    /// </summary>
    public class Gci
    {
        private readonly List<byte> gciHeader;
        public List<byte> gciFile;
        private readonly List<byte> gciData;

        private byte[] Header
        {
            get { return gciHeader.ToArray<byte>(); }
        }

        private byte[] Data
        {
            get { return gciData.ToArray<byte>(); }
        }

        private byte[] GCIFile
        {
            get { return gciFile.ToArray<byte>(); }
        }

        private int Length
        {
            get { return gciHeader.Count + gciData.Count; }
        }

        /// <summary>
        /// text.
        /// </summary>
        /// <param name="seedRegion">The region of the game that the seed is being generated for.</param>
        /// <param name="seedData">Any data that needs to be read into the GCI file.</param>
        /// <returns> The inserted value as a byte. </returns>
        public Gci(char regionCode, List<byte> seedData, string playthroughName)
        {
            gciHeader = new List<byte>();
            gciData = new List<byte>();
            gciFile = new List<byte>();

            // Populate GCI Header
            /*x0*/
            gciHeader.AddRange(Converter.StringBytes("GZ2"));
            /*x3*/
            gciHeader.Add(Converter.StringBytes(regionCode));
            /*x4*/
            gciHeader.AddRange(Converter.StringBytes("01"));
            /*x6*/
            gciHeader.Add(Converter.GcByte(0xFF)); //unused
            /*x7*/
            gciHeader.Add(Converter.GcByte(1)); // banner flags (C8)
            /*x8*/
            gciHeader.AddRange(Converter.StringBytes(playthroughName, 0x20));
            /*x28*/
            gciHeader.AddRange(
                Converter.GcBytes((UInt32)(DateTime.UtcNow - new DateTime(2000, 1, 1)).TotalSeconds)
            );
            /*x2c*/
            gciHeader.AddRange(Converter.GcBytes(SeedData.DebugInfoSize)); // Image data offset
            /*x30*/
            gciHeader.AddRange(Converter.GcBytes((UInt16)0x0001)); // iconFormats
            /*x32*/
            gciHeader.AddRange(Converter.GcBytes((UInt16)0x0002)); // iconAnimationSpeeds
            /*x34*/
            gciHeader.Add(Converter.GcByte(0x04)); // permissions
            /*x35*/
            gciHeader.Add(Converter.GcByte(0x00)); // copy counter
            /*x36*/
            gciHeader.AddRange(Converter.GcBytes((UInt16)0x00)); // first block number
            /*x38*/
            gciHeader.AddRange(Converter.GcBytes((UInt16)0x04)); // Actual num of blocks.
            /*x3A*/
            gciHeader.AddRange(Converter.GcBytes((UInt16)0xFFFF)); // unused
            /*x3C*/
            gciHeader.AddRange(
                Converter.GcBytes((UInt32)(SeedData.DebugInfoSize + SeedData.ImageDataSize))
            ); // Comments Offset

            gciFile.AddRange(gciHeader);
            gciFile.AddRange(seedData);

            // Pad
            while (gciFile.Count < (4 * 0x2000) + 0x40) // Pad to 2 blocks.
                gciFile.Add((byte)0x0);
        }

        public static string playthroughNameToFilename(string playthroughName)
        {
            string verAsStr = VersionNumToChars(SeedData.VersionMajor, SeedData.VersionMinor);

            return "sd"
                + verAsStr
                + "000"
                + playthroughName.Substring(playthroughName.Length - 3)
                + playthroughName.Substring(0, playthroughName.Length - 4);

            // Explanation:

            // - "sd" is used when iterating through DirectoryEntries to
            //   determine that a TP file is a rando seed.

            // - The next 4 characters represent 24 bits. The first 4 bits (0
            //   through 15) plus 1 (1 through 16) indicates how many bits long
            //   the verMajor is (let's say this value is X). Of the 20 bits
            //   remaining, the upper X bits are the verMajor, and the remaining
            //   20 - X bits are the verMinor. This means we are not able to
            //   represent the maximum theoretical seed version of 65535.65535,
            //   but this is not a problem. Given that the minor version resets
            //   to 0 whenever the the major version increments, the minor
            //   version should usually be fairly low. The maximum version can
            //   still go up to 65535, but we would only have 4 bits left for
            //   the minor version at that point. In any case, we have plenty of
            //   versions to work with, and we can restructure in the distant
            //   future if we ever need to (not expected to ever be needed).

            // - The next 3 chars "000" represents 18 bits all set to 0. These
            //   are reserved in case we want to show additional data when the
            //   player is picking a seed on the title screen. Note that we have
            //   some other bits to work with in the DirectoryEntry, but it
            //   might be on the hackier side and there aren't a ton available.
            //   So for now, we can think that we have room for 18 more bits of
            //   data.

            // - The next 3 chars are the 3 chars that go after the underscore
            //   in the playthroughName. For example, these would be "123" if
            //   the playthroughName was "AngryMidna_123".

            // - Following these 3 chars is the first part of the
            //   playthroughName. For "AngryMidna_123", this would be
            //   "AngryMidna". This section can be up to 20 chars long (10 for
            //   adjective and 10 for noun).

            // The final result can be up to 32 characters. Note that the
            // `filename` in the DirectoryEntry (GCI header) does not need to be
            // null-terminated (it can use all 32 characters).
        }

        private static string VersionNumToChars(UInt16 verMajor, UInt16 verMinor)
        {
            byte bitsForVerMajor = checkBitsNeededForNum(verMajor);
            byte bitsForVerMinor = checkBitsNeededForNum(verMinor);

            byte bitsRequired = (byte)(bitsForVerMajor + bitsForVerMinor);

            if (bitsRequired > 20)
            {
                throw new Exception(
                    $"Verion {verMajor}.{verMinor} requires {bitsRequired} bits, but the max is 20."
                );
            }

            // We only use the lower 24 bits of this value. The 24 bits are
            // encoded as 4 characters (6 bits each).
            int resultAsNumber = (bitsForVerMajor - 1 << 20);

            // Add bits for verMajor
            resultAsNumber |= (verMajor << (20 - bitsForVerMajor));
            // Add bits for verMinor
            resultAsNumber |= verMinor;

            string result = "";
            for (int i = 3; i >= 0; i--)
            {
                int val = (resultAsNumber >> (6 * i)) & 0x3F;
                result += SettingsEncoder.EncodeByteAs6BitChar((byte)val);
            }

            return result;
        }

        private static byte checkBitsNeededForNum(UInt16 number)
        {
            for (byte i = 1; i <= 16; i++)
            {
                int oneOverMax = 1 << i;
                if (number < oneOverMax)
                {
                    return i;
                }
            }

            throw new Exception($"Value \"{number}\" could not be represented as a U16.");
        }
    }
}
