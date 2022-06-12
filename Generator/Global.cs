using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TPRandomizer
{
    public class Global
    {
        public static string outputPath { get; }
        public static string rootPath { get; }

        static Global()
        {
            string envFileDir = InitEnv();

            rootPath = ResolvePath(envFileDir, Environment.GetEnvironmentVariable("TPR_GENERATOR_PATH"));
            outputPath = ResolvePath(envFileDir, Environment.GetEnvironmentVariable("OUTPUT_VOLUME_PATH"));

            Directory.CreateDirectory(outputPath);
        }

        public static void Init()
        {
        }

        public static string CombineOutputPath(params string[] paths)
        {
            return paths.Aggregate(outputPath, (acc, p) => Path.Combine(acc, p));
        }

        public static string CombineRootPath(params string[] paths)
        {
            return paths.Aggregate(rootPath, (acc, p) => Path.Combine(acc, p));
        }

        private static string InitEnv()
        {
            string path = Assembly.GetEntryAssembly().Location;

            string desiredFilename = ".env.development";
            if (Environment.GetEnvironmentVariable("TPR_ENV") == "production")
            {
                desiredFilename = ".env";
            }

            while (true)
            {
                path = Path.GetDirectoryName(path);
                if (path == null)
                {
                    throw new Exception("Unable to find output.config.json");
                }

                string outputConfigPath = Path.Join(path, desiredFilename);
                if (File.Exists(outputConfigPath))
                {
                    Util.DotEnv.Load(outputConfigPath);
                    return path;
                }
            }
        }

        private static string ResolvePath(string path1, string path2)
        {
            if (Path.IsPathRooted(path2))
            {
                return Path.GetFullPath(path2);
            }
            return Path.Join(path1, path2);
        }
    }
}
