namespace TPRandomizer.Util
{
    using System;
    using System.Collections.Generic;

    public class SettingsEncoder
    {
        private static string charMap =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

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

        public byte NextNibble()
        {
            int len = 4;
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
    }
}
