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
                case "generate2": {
                    // settingsString, seed
                    string seed = "";
                    if (args.Length > 2)
                    {
                        seed = args[2];
                    }
                    Randomizer.CreateInputJson(args[1], seed);
                    break;
                }
                case "generate_final_output":
                    // id, tempArg, pSettingsString,
                    Randomizer.GenerateFinalOutput(args[1], args[2], args[3]);
                    break;
                case "generate_final_output2":
                    // id, tempArg, pSettingsString,
                    Randomizer.GenerateFinalOutput2(args[1], args[2], args[3]);
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
