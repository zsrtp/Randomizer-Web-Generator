using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

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
            // Prepare localization service before entering the main part of the
            // code.

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLocalization(
                options =>
                {
                    // This path is added to the root "Generator" dir to find
                    // the folder which contains the resx files.
                    options.ResourcesPath = "Translations";
                }
            );
            builder.Services.AddSingleton<Translations>();

            IHost host = builder.Build();

            Worker worker = ActivatorUtilities.CreateInstance<Worker>(host.Services);
            worker.Run(args);
        }
    }
}
