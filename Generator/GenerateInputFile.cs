using System;
using System.IO;

namespace TPRandomizer
{
    public class GenerateInputFile
    {
        private static string charMap =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        private static string genId()
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

        public static bool Run(string settingsString)
        {
            bool success = false;
            // string id = "thisisanid";
            string id = genId();
            string outputPath = Path.Combine("seeds", id, "input.json");

            // byte[] bytes = { 0x4, 0x6, 0xab };

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                // File.WriteAllBytes(outputPath, bytes);
                File.WriteAllText(outputPath, settingsString);

                Console.WriteLine("SUCCESS:" + id);
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem writing input.json file for id: " + id);
                System.Environment.Exit(1);
            }

            return success;
        }
    }
}
