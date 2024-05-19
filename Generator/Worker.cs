using System;
using System.Globalization;
using System.IO;
using System.Text;
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

            Res.UpdateCultureInfo("en");

            string str;
            byte[] bytes;

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
                {
                    // seedId

                    // Note: we need to use fancier printing rather than just
                    // Console.WriteLine in order for advanced unicode
                    // characters such as '♂' to be passed correctly.
                    str = Randomizer.GetSeedGenResultsJson(args[1]);
                    bytes = Encoding.UTF8.GetBytes(str);
                    using (Stream myOutStream = Console.OpenStandardOutput())
                    {
                        myOutStream.Write(bytes, 0, bytes.Length);
                    }
                    break;
                }
                // "dangerously_print_full_race_spoiler" should only ever be
                // called by a human manually from the command line. The website
                // must never call this code.
                case "dangerously_print_full_race_spoiler":
                    // seedId
                    str = Randomizer.GetSeedGenResultsJson(args[1], true);
                    bytes = Encoding.UTF8.GetBytes(str);
                    using (Stream myOutStream = Console.OpenStandardOutput())
                    {
                        myOutStream.Write(bytes, 0, bytes.Length);
                    }
                    break;
                default:
                    throw new Exception("Unrecognized command.");
            }
        }
    }
}
