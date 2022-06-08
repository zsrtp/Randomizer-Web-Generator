namespace TPRandomizer.Util
{
    using System;
    using System.Text;
    using System.Security.Cryptography;

    public class Hash
    {
        private static string charMap =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        public static int CalculateMD5(string inputStr)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(inputStr));
                string rr = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                if (rr.Length > 8)
                {
                    rr = rr.Substring(0, 8);
                }
                else if (rr.Length < 1)
                {
                    return 0;
                }

                return int.Parse(rr, System.Globalization.NumberStyles.HexNumber);
            }
        }

        public static string GenId()
        {
            Random rnd = new Random();
            int num;
            int charIndex;
            string id = "";

            for (int i = 0; i < 2; i++)
            {
                num = rnd.Next();
                for (int j = 0; j < 5; j++)
                {
                    charIndex = num & (0x3f);
                    id += charMap[charIndex];
                    num = num >> 6;
                }
            }

            num = rnd.Next();
            charIndex = num & (0x3f);
            id += charMap[charIndex];

            return id;
        }
    }
}
