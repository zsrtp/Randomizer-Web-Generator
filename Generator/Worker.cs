using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;

namespace TPRandomizer
{
    public class Worker
    {
        private IServiceProvider provider;

        // This constructor is called automatically by dependency injection
        // stuff when we create the instance in Program.cs
        public Worker(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public void Run(string[] args)
        {
            Global.Init(provider);

            string abc = Res.Msg();

            // string def = messageService.GetMsg(
            //     "item.progressive-clawshot",
            //     new() { { "context", "dog,cat" }, { "count", 12 } }
            // );

            Res.UpdateCultureInfo("fr-FR-DOG");

            string abc2 = Res.Msg();

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
