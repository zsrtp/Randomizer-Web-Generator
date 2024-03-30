// Note: we intentionally keep this namespace as "TPRandomizer" to make it as
// easy as possible to use.
namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Extensions.DependencyInjection;
    using TPRandomizer.Util;
    using System.Text.RegularExpressions;
    using System.Linq;

    public partial class Res
    {
        [GeneratedRegex(@"{([a-z0-9]+)(?:\(([a-z0-9:,]*)\))?}", RegexOptions.IgnoreCase)]
        private static partial Regex ResourceVal();

        private static Translations translations;

        static Res()
        {
            IServiceProvider provider = Global.GetServiceProvider();
            translations = provider.GetRequiredService<Translations>();
        }

        public static void UpdateCultureInfo(string name)
        {
            CultureInfo newCultureInfo = CultureInfo.GetCultureInfo(name);

            if (
                CultureInfo.CurrentCulture.Equals(newCultureInfo)
                && CultureInfo.CurrentUICulture.Equals(newCultureInfo)
            )
            {
                // Do nothing if new culture matches current culture.
                return;
            }

            // "fr-FR-DOG"
            CultureInfo.CurrentCulture = newCultureInfo;
            CultureInfo.CurrentUICulture = newCultureInfo;

            translations.OnCultureChange();
        }

        public static string Msg(string resKey)
        {
            return translations.GetMsg(resKey);
            // return translations.GetGreetingMessage();
        }

        public static ParsedRes ParseVal(string resVal)
        {
            Dictionary<string, Dictionary<string, string>> other = new();
            if (StringUtils.isEmpty(resVal))
                return null;

            HashSet<string> seenInterpolationKeys = new();

            string newVal = ResourceVal()
                .Replace(
                    resVal,
                    (match) =>
                    {
                        Group key = match.Groups[1];
                        if (!key.Success)
                            throw new Exception("Unexpected regex failure.");
                        string keyVal = key.Value;

                        if (seenInterpolationKeys.Contains(keyVal))
                            throw new Exception(
                                $"interpolation key '{keyVal}' is duplicated for \"{resVal}\""
                            );

                        Group otherGroup = match.Groups[2];
                        if (otherGroup.Success)
                        {
                            Dictionary<string, string> dict = ParseOtherGroup(otherGroup.Value);
                            other[keyVal] = dict;
                        }

                        seenInterpolationKeys.Add(keyVal);

                        return $"{{{keyVal}}}";
                    }
                );

            // match with regex, then for each one provide the values.
            ParsedRes parsedRes = new ParsedRes();
            parsedRes.value = newVal;
            parsedRes.other = other;

            return parsedRes;
        }

        private static Dictionary<string, string> ParseOtherGroup(string otherGroupStr)
        {
            Dictionary<string, string> result = new();
            if (StringUtils.isEmpty(otherGroupStr))
                return result;

            // else split and go over.
            string[] chunks = otherGroupStr.Split(",");
            foreach (string chunk in chunks)
            {
                string[] chunkHalves = chunk.Split(":");
                if (chunkHalves.Length != 2)
                    throw new Exception("Invalid chunkHalves for 'val'.");

                string key = chunkHalves[0];
                string value = chunkHalves[1];

                if (StringUtils.isEmpty(key) || StringUtils.isEmpty(value))
                    throw new Exception("chunkHalf was empty.");

                if (result.ContainsKey(key))
                    throw new Exception($"Duplicate chunkHalf key '{key}'.");

                result[key] = value;
            }

            return result;
        }

        public class ParsedRes
        {
            public string value;
            public Dictionary<string, Dictionary<string, string>> other = new();

            public string Substitute(Dictionary<string, string> interpolation)
            {
                if (ListUtils.isEmpty(interpolation))
                    return value;

                // Iterate over and replace
                string result = ResourceVal()
                    .Replace(
                        value,
                        (match) =>
                        {
                            Group key = match.Groups[1];
                            if (!key.Success)
                                throw new Exception("Unexpected regex failure.");

                            return interpolation[key.Value];
                        }
                    );

                return result;
            }
        }
    }
}
