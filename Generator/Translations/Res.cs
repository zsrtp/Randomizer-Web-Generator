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
    using Microsoft.CodeAnalysis;

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

        public static string Msg(string resKey, Dictionary<string, string> interpolation = null)
        {
            // Pass the interpolation to the translationsService so that it can
            // build the stack of keys based on the current language it is
            // trying to check as it goes through its list.


            // Need to handle interpolation "count" when doing the initial
            // grabbing of the resource.

            // ParsedRes parsedRes = ParseVal(resKey);
            // parsedRes.Substitute();

            // MsgResult msgResult = translations.GetMsg(resKey, interpolation);

            ParsedRes parsedRes = ParseVal(resKey, interpolation);

            return parsedRes.Substitute(interpolation);

            // if (msgResult == null)
            //     return null;

            // return null;

            // return translations.GetMsg(resKey);
            // return translations.GetGreetingMessage();
        }

        public static ParsedRes ParseVal(
            string resKey,
            Dictionary<string, string> interpolation = null
        )
        {
            ParsedRes parsedRes = new();
            MsgResult msgResult = translations.GetMsg(resKey, interpolation);
            if (msgResult == null || StringUtils.isEmpty(msgResult.msg))
            {
                if (msgResult == null)
                    parsedRes.langCode = "en";
                else
                    parsedRes.langCode = msgResult.langCode;
                parsedRes.foundValue = false;
                parsedRes.value = resKey;
                parsedRes.other = new();
                return parsedRes;
            }

            string resVal = msgResult.msg;

            Dictionary<string, Dictionary<string, string>> other = new();

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
            parsedRes.langCode = msgResult.langCode;
            parsedRes.foundValue = true;
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

        public class Rule
        {
            public int[] numbers;
            public Func<int, int> plurals;

            public Rule(int[] numbers, int fc)
            {
                this.numbers = numbers;
                this.plurals = PluralResolver._rulesPluralsTypes[fc];
            }
        }

        public class PluralResolver
        {
            // 'en', 'de', 'es', 'it': 2
            // 'ja': 3
            // 'fr': 9

            public static string GetSuffix(string langCode, int number, bool ord = false)
            {
                return GetSuffix(
                    langCode,
                    Math.Abs(number).ToString(CultureInfo.InvariantCulture),
                    ord
                );
            }

            public static string GetSuffix(string langCode, double number, bool ord = false)
            {
                return GetSuffix(
                    langCode,
                    Math.Abs(number).ToString(CultureInfo.InvariantCulture),
                    ord
                );
            }

            public static string GetSuffix(string langCode, string val, bool ord = false)
            {
                if (StringUtils.isEmpty(val))
                    return "other";

                switch (langCode)
                {
                    case "en":
                        return GetSuffixEn(val, ord);
                    case "es":
                        return GetSuffixEs(val, ord);
                    case "fr":
                        return GetSuffixFr(val, ord);
                    default:
                        throw new Exception($"'{langCode}' is not a supported langCode.");
                }

                // We need to know which langauge we are looking at. This
                // depends on the resource sinc it is possible we are resolving
                // an English resource for French if it has not yet been defined
                // for French yet (or never will be).
            }

            private static string GetSuffixEn(string val, bool ord)
            {
                string[] s = val.Split('.');

                uint wholeNumVal = StringUtils.isEmpty(s[0]) ? 0 : Convert.ToUInt32(s[0]);

                bool hasDecimals =
                    s.Length > 1 && !StringUtils.isEmpty(s[1]) && Convert.ToUInt32(s[1]) != 0;

                if (ord)
                {
                    uint n10 = wholeNumVal % 10;
                    uint n100 = wholeNumVal % 100;

                    if (n10 == 1 && n100 != 11)
                        return "one";
                    else if (n10 == 2 && n100 != 12)
                        return "two";
                    else if (n10 == 3 && n100 != 13)
                        return "few";
                    else
                        return "other";
                }

                return wholeNumVal == 1 && !hasDecimals ? "one" : "other";
            }

            private static string GetSuffixEs(string val, bool ord)
            {
                if (ord)
                    return "other";

                string[] s = val.Split('.');

                uint wholeNumVal = StringUtils.isEmpty(s[0]) ? 0 : Convert.ToUInt32(s[0]);

                bool hasDecimals =
                    s.Length > 1 && !StringUtils.isEmpty(s[1]) && Convert.ToUInt32(s[1]) != 0;

                if (!hasDecimals)
                {
                    if (wholeNumVal == 1)
                        return "one";
                    else if (wholeNumVal != 0 && ((wholeNumVal % 1000000) == 0))
                        return "many";
                }

                return "other";
            }

            private static string GetSuffixFr(string val, bool ord)
            {
                string[] s = val.Split('.');

                uint wholeNumVal = StringUtils.isEmpty(s[0]) ? 0 : Convert.ToUInt32(s[0]);

                bool hasDecimals =
                    s.Length > 1 && !StringUtils.isEmpty(s[1]) && Convert.ToUInt32(s[1]) != 0;

                if (ord)
                {
                    if (wholeNumVal == 1 && !hasDecimals)
                        return "one";
                    return "other";
                }

                // 1.5 intentionally converts to "one".
                if (wholeNumVal == 0 || wholeNumVal == 1)
                    return "one";

                if (!hasDecimals && wholeNumVal != 0 && ((wholeNumVal % 1000000) == 0))
                    return "many";

                return "other";
            }

            // private static string SliceFromEnd(string str, int numChars)
            // {
            //     if (StringUtils.isEmpty(str))
            //         return str;

            //     int startIdx = Math.Max(str.Length - numChars, 0);
            //     return str.Substring(startIdx);
            // }

            public static readonly Dictionary<int, Func<int, int>> _rulesPluralsTypes =
                new()
                {
                    { 2, (n) => n != 1 ? 1 : 0 },
                    { 3, (n) => 0 },
                    { 9, (n) => n >= 2 ? 1 : 0 },
                };

            public static readonly Dictionary<string, Rule> rules =
                new()
                {
                    { "en", new(new[] { 1, 2 }, 2) },
                    { "ja", new(new[] { 1 }, 3) },
                    { "de", new(new[] { 1, 2 }, 2) },
                    { "es", new(new[] { 1, 2 }, 2) },
                    { "fr", new(new[] { 1, 2 }, 9) },
                    { "it", new(new[] { 1, 2 }, 2) },
                };

            public static Rule GetRule(string code)
            {
                if (rules.TryGetValue(code, out Rule rule))
                    return rule;
                return null;
            }

            public static bool NeedsPlural(string code)
            {
                Rule rule = GetRule(code);

                return rule != null && rule.numbers.Length > 1;
            }

            // public static List<string> GetPluralFormsOfKey(string code, string key)
            // {
            //     return GetSuffixes(code).Select((suffix) => $"{key}{suffix}").ToList();
            // }

            // public static List<string> GetSuffixes(string code)
            // {
            //     List<string> result = new();
            //     Rule rule = GetRule(code);

            //     if (rule == null)
            //         return result;

            //     foreach (int number in rule.numbers)
            //     {
            //         result.Add(GetSuffix(code, number));
            //     }

            //     return result;
            // }

            // public static string GetSuffix(string code, int count)
            // {
            //     Rule rule = GetRule(code);

            //     if (rule != null)
            //     {
            //         return GetSuffixRetroCompatible(rule, count);
            //     }

            //     // this.logger.warn(`no plural rule found for: ${code}`);
            //     return "";
            // }

            public static string GetSuffixRetroCompatible(Rule rule, int count)
            {
                // int idx = rule.noAbs ? rule.plurals(count) : rule.plurals(Math.abs(count));
                int idx = rule.plurals(Math.Abs(count));
                // int suffix = rule.numbers[idx];

                // const returnSuffix = () => (
                //   this.options.prepend && suffix.toString() ? this.options.prepend + suffix.toString() : suffix.toString()
                // );

                // // COMPATIBILITY JSON
                // // v1
                // if (this.options.compatibilityJSON === 'v1') {
                //   if (suffix === 1) return '';
                //   if (typeof suffix === 'number') return `_plural_${suffix.toString()}`;
                //   return returnSuffix();
                //   // eslint-disable-next-line no-else-return
                // } else if (/* v2 */ this.options.compatibilityJSON === 'v2') {
                //   return returnSuffix();
                // } else if (/* v3 - gettext index */ this.options.simplifyPluralSuffix && rule.numbers.length === 2 && rule.numbers[0] === 1) {
                //   return returnSuffix();
                // }

                // return this.options.prepend && idx.toString() ? this.options.prepend + idx.toString() : idx.toString();

                return idx.ToString();
            }
        }

        public class ParsedRes
        {
            public bool foundValue;
            public string langCode;
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
