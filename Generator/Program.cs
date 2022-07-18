using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TPRandomizer
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Global.Init();

            string command = args[0];

            switch (command)
            {
                case "generate_legacy":
                    // Randomizer.Start(args[1], args[2]);
                    Randomizer.Start(args[1], args[2], args[3]);
                    break;
                case "generate2":
                {
                    // id, settingsString, isRaceSeed, seed
                    string seed = "";
                    if (args.Length > 3)
                    {
                        seed = args[4];
                    }
                    Randomizer.CreateInputJson(args[1], args[2], args[3], seed);
                    break;
                }
                case "generate_final_output2":
                    // id, fileCreationSettingsString
                    Randomizer.GenerateFinalOutput2(args[1], args[2]);
                    break;
                case "print_check_ids":
                    Console.WriteLine(
                        JsonConvert.SerializeObject(CheckIdClass.GetNameToIdNumDictionary())
                    );
                    break;
                default:
                    throw new Exception("Unrecognized command.");
            }
        }
    }
}
