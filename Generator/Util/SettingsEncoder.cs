namespace TPRandomizer.Util
{
    using System;
    using System.Collections.Generic;

    public class SettingsEncoder
    {
        private static string charMap =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        public static string EncodeAs6BitString(string bitString)
        {
            if (bitString == null || bitString.Length == 0)
            {
                return "";
            }

            string result = "";

            int remainder = bitString.Length % 6;
            if (remainder > 0)
            {
                int loopCount = 6 - remainder;
                for (int i = 0; i < loopCount; i++)
                {
                    bitString += "0";
                }
            }

            int iterations = bitString.Length / 6;
            for (int i = 0; i < iterations; i++)
            {
                result += charMap[Convert.ToByte(bitString.Substring(i * 6, 6), 2)];
            }

            return result;
        }

        public static string DecodeToBitString(string encodedStr)
        {
            string bitStr = "";

            for (int i = 0; i < encodedStr.Length; i++)
            {
                bitStr += Convert.ToString(charMap.IndexOf(encodedStr[i]), 2).PadLeft(6, '0');
            }

            return bitStr;
        }

        public static int DecodeToInt(string encodedStr)
        {
            return Convert.ToInt32(DecodeToBitString(encodedStr), 2);
        }

        public static string EncodeAsVlq16(UInt16 num)
        {
            if (num < 2)
            {
                return "0000" + num;
            }

            int bitsNeeded = GetVlq16BitLength(num) - 4;

            return (
                Convert.ToString(bitsNeeded, 2).PadLeft(4, '0')
                + Convert.ToString(num, 2).Substring(1)
            );
        }

        public static UInt16 DecodeVlq16(string bits)
        {
            byte bitsToRead = Convert.ToByte(bits.Substring(0, 4), 2);

            if (bitsToRead == 0)
            {
                return Convert.ToUInt16(bits.Substring(4, 1), 2);
            }

            if (bits.Length < 4 + bitsToRead)
            {
                throw new Exception("Not enough bits to decode vlq16.");
            }

            return (UInt16)((1 << bitsToRead) + Convert.ToUInt16(bits.Substring(4, bitsToRead), 2));
        }

        public static byte GetVlq16BitLength(UInt16 num)
        {
            if (num < 2)
            {
                return 5;
            }

            int bitsNeeded = 1;
            for (int i = 2; i <= 16; i++)
            {
                int oneOverMax = 1 << i;
                if (num < oneOverMax)
                {
                    bitsNeeded = i - 1;
                    break;
                }
            }

            return (byte)(4 + bitsNeeded);
        }

        public static string EncodeNumAsBits(int number, byte bitLength)
        {
            return Convert.ToString(number, 2).PadLeft(bitLength, '0');
        }
    }

    public class BitsProcessor
    {
        private string bits = "";
        private int currentIndex = 0;
        private bool done = false;

        public BitsProcessor(string bitString)
        {
            if (bitString != null)
                bits = bitString;
            if (bits.Length < 0)
                done = true;
        }

        public string NextString(string[] arr)
        {
            int len = 4;
            if (done || bits.Length < currentIndex + len)
            {
                throw new Exception("Not enough bits remaining");
            }

            string result = arr[Convert.ToInt32(bits.Substring(currentIndex, len), 2)];

            currentIndex += len;
            if (currentIndex >= bits.Length)
                done = true;

            return result;
        }

        public string NextString(string[] arr, byte bitLength)
        {
            if (done || bits.Length < currentIndex + bitLength)
            {
                throw new Exception("Not enough bits remaining");
            }

            string result = arr[Convert.ToInt32(bits.Substring(currentIndex, bitLength), 2)];

            currentIndex += bitLength;
            if (currentIndex >= bits.Length)
                done = true;

            return result;
        }

        public int NextInt(int numBits)
        {
            if (done || bits.Length < currentIndex + numBits)
            {
                throw new Exception("Not enough bits remaining");
            }

            int result = Convert.ToInt32(bits.Substring(currentIndex, numBits), 2);

            currentIndex += numBits;
            if (currentIndex >= bits.Length)
                done = true;

            return result;
        }

        public UInt16 NextUInt16()
        {
            int len = 16;
            if (done || bits.Length < currentIndex + len)
            {
                throw new Exception("Not enough bits remaining");
            }

            UInt16 result = Convert.ToUInt16(bits.Substring(currentIndex, len), 2);

            currentIndex += len;
            if (currentIndex >= bits.Length)
                done = true;

            return result;
        }

        public bool NextBool()
        {
            int len = 1;
            if (done || bits.Length < currentIndex + len)
            {
                throw new Exception("Not enough bits remaining");
            }

            bool result = bits.Substring(currentIndex, len) == "1";

            currentIndex += len;
            if (currentIndex >= bits.Length)
                done = true;

            return result;
        }

        public byte NextByte()
        {
            int len = 8;
            if (done || bits.Length < currentIndex + len)
            {
                throw new Exception("Not enough bits remaining");
            }

            byte result = Convert.ToByte(bits.Substring(currentIndex, len), 2);

            currentIndex += len;
            if (currentIndex >= bits.Length)
                done = true;

            return result;
        }

        public List<RecolorDefinition> NextRecolorDefinitions()
        {
            UInt16 numRecolorIds = NextUInt16();

            List<RecolorDefinition> recolorDefs = new();

            for (int j = 0; j < numRecolorIds; j++)
            {
                if (NextBool())
                {
                    recolorDefs.Add(new RecolorDefinition((RecolorId)j));
                }
            }

            foreach (RecolorDefinition recolorDef in recolorDefs)
            {
                byte red = NextByte();
                byte green = NextByte();
                byte blue = NextByte();

                recolorDef.rgb = new List<byte> { red, green, blue };
            }

            return recolorDefs;
        }

        public List<Item> NextItemList()
        {
            List<Item> list = new();

            while (true)
            {
                int itemId = NextInt(9);
                if (itemId >= 0 && itemId < 0x1FF)
                {
                    list.Add((Item)itemId);
                }
                else
                {
                    break;
                }
            }

            return list;
        }

        public List<string> NextExcludedChecksList()
        {
            List<string> list = new();

            while (true)
            {
                int checkIdNum = NextInt(9);
                if (checkIdNum >= 0 && checkIdNum < 0x1FF)
                {
                    list.Add(CheckIdClass.GetCheckName(checkIdNum));
                }
                else
                {
                    break;
                }
            }

            return list;
        }

        public UInt16 NextVlq16()
        {
            if (done || bits.Length < currentIndex + 4)
            {
                throw new Exception("Not enough bits remaining");
            }

            UInt16 val = SettingsEncoder.DecodeVlq16(bits);

            currentIndex += SettingsEncoder.GetVlq16BitLength(val);
            if (currentIndex >= bits.Length)
                done = true;

            return val;
        }
    }
}
