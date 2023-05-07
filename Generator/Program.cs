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
                case "generate2":
                {
                    // seedId, settingsString, isRaceSeed, seed
                    string seed = "";
                    if (args.Length > 4)
                    {
                        seed = args[4];
                    }
                    Randomizer.CreateInputJson(args[1], args[2], args[3], seed);
                    break;
                }
                case "generate_final_output2":
                    // seedId, fileCreationSettingsString
                    Randomizer.GenerateFinalOutput2(args[1], args[2]);
                    break;
                case "print_check_ids":
                    Console.WriteLine(
                        JsonConvert.SerializeObject(CheckIdClass.GetNameToIdNumDictionary())
                    );
                    break;
                case "print_seed_gen_results":
                    // seedId
                    Console.WriteLine(Randomizer.GetSeedGenResultsJson(args[1], false));
                    break;
                // "dangerously_print_full_race_spoiler" should only ever be
                // called by a human manually from the command line. The website
                // must never call this code.
                case "dangerously_print_full_race_spoiler":
                    // seedId
                    Console.WriteLine(Randomizer.GetSeedGenResultsJson(args[1], true, true));
                    break;
                default:
                    throw new Exception("Unrecognized command.");
            }
        }
    }
}
