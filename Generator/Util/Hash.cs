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
            HashAlgorithm algorithm = MD5.Create();

            byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputStr));

            // We compute the result this way so that it is consistent
            // regardless of the endianess of the machine this runs on.
            int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result += bytes[i] << (8 * (3 - i));
            }

            return result;
        }

        private static int HashSeedWithSalt(string seedStr)
        {
            byte[] plainText = Encoding.UTF8.GetBytes(seedStr);
            byte[] salt = Global.seedHashSecret;

            // https://stackoverflow.com/a/2138588
            HashAlgorithm algorithm = SHA256.Create();

            byte[] plainTextWithSaltBytes = new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            byte[] bytes = algorithm.ComputeHash(plainTextWithSaltBytes);

            // We compute the result this way so that it is consistent
            // regardless of the endianess of the machine this runs on.
            int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result += bytes[i] << (8 * (3 - i));
            }

            return result;
        }

        public static int HashSeed(string seedStr, bool isRaceSeed)
        {
            if (isRaceSeed)
            {
                return HashSeedWithSalt(seedStr);
            }
            return CalculateMD5(seedStr);
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
