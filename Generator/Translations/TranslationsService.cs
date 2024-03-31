// Note: it is important that the namespace of this file be just "TPRandomizer"
// and that the class name matches the names of the resx files ("Translations").
// The way the resx files are found is extremely confusing, but you can look
// here for some amount of info:
// https://github.com/aspnet/Localization/issues/340 -isaac
namespace TPRandomizer
{
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.Extensions.Localization;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks.Dataflow;
    using TPRandomizer.Util;
    using System;
    using TPRandomizer.Properties;
    using System.Linq;

    // public sealed class Translations(IStringLocalizer<Translations> localizer)
    // public sealed class Translations(IStringLocalizer<Translations> localizer)
    public sealed partial class Translations
    {
        [GeneratedRegex(
            "^([0-9a-z-.]+)(?:_([0-9a-z-.,]+))?(?:#([0-9a-z-.]+))?$",
            RegexOptions.IgnoreCase
        )]
        private static partial Regex ResKeyRegex();

        private IStringLocalizer<Translations> localizer;
        List<LocaleResources> resourcesList;

        // Called automatically by dependency injection stuff.
        public Translations(IStringLocalizer<Translations> localizer)
        {
            this.localizer = localizer;
        }

        static Translations() { }

        public void OnCultureChange()
        {
            resourcesList = null;
        }

        private void EnsureDictionary()
        {
            if (resourcesList != null)
                return;

            resourcesList = new();

            // Store current culture
            CultureInfo startingCultureInfo = CultureInfo.CurrentUICulture;

            while (true)
            {
                bool shouldBreak = false;

                LocaleResources resources = new(CultureInfo.CurrentCulture);

                try
                {
                    foreach (LocalizedString locStr in localizer.GetAllStrings(false))
                    {
                        string resKey = ResolveResourceKey(locStr);
                        resources.TryAddResource(resKey, locStr);
                    }
                }
                catch (Exception) { }
                finally
                {
                    resourcesList.Add(resources);

                    if (CultureInfo.CurrentCulture.ThreeLetterISOLanguageName == "ivl")
                        shouldBreak = true;
                    else
                        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo
                            .CurrentCulture
                            .Parent;
                }

                if (shouldBreak)
                    break;
            }

            // Filter out any empty list entries
            for (int i = resourcesList.Count - 1; i >= 0; i--)
            {
                if (resourcesList[i].IsEmpty())
                    resourcesList.RemoveAt(i);
            }

            // Restore current culture
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = startingCultureInfo;
        }

        private static string ResolveResourceKey(LocalizedString locStr)
        {
            Match match = ResKeyRegex().Match(locStr.Name);

            if (!match.Success)
                return null;

            string resKey = match.Groups[1].Value;

            Group contextGroup = match.Groups[2];
            if (contextGroup.Success)
            {
                string[] contextChunks = contextGroup.Value.Split(',');
                Array.Sort(contextChunks, StringComparer.Ordinal);
                resKey += "_" + string.Join(",", contextChunks);
            }

            Group countGroup = match.Groups[3];
            if (countGroup.Success)
            {
                resKey += "#" + countGroup.Value;
            }

            return resKey;
        }

        public string GetGreetingMessage()
        {
            EnsureDictionary();
            // LocalizedString localizedString = localizer["GreetingMessage"];
            LocalizedString localizedString = localizer["dog2"];
            if (localizedString.ResourceNotFound)
                return null;
            return localizedString;
        }

        public MsgResult GetMsg(string key, Dictionary<string, string> interpolation = null)
        {
            EnsureDictionary();

            for (int i = 0; i < resourcesList.Count; i++)
            {
                LocaleResources resources = resourcesList[i];

                string langCode = NormalizeLangCode(resources.GetLangCode());
                List<string> keysToTry = GenKeysToCheck(langCode, key, interpolation);

                for (int keyIdx = keysToTry.Count - 1; keyIdx >= 0; keyIdx--)
                {
                    string keyToTry = keysToTry[keyIdx];

                    LocalizedString localeString = resources.TryGetValue(keyToTry);
                    if (localeString != null && !localeString.ResourceNotFound)
                        return new MsgResult(langCode, localeString.Value);
                }
            }
            return null;
        }

        private string NormalizeLangCode(string langCode)
        {
            if (StringUtils.isEmpty(langCode) || langCode == "iv")
                return "en";
            return langCode;
        }

        private List<string> GenKeysToCheck(
            string langCode,
            string key,
            Dictionary<string, string> interpolation
        )
        {
            if (StringUtils.isEmpty(key))
                throw new Exception("key must not be empty.");

            List<string> keys = new() { key };

            if (ListUtils.isEmpty(interpolation))
                return keys;

            bool needsPluralHandling = false;
            double countAsDouble = -1;
            if (interpolation.TryGetValue("count", out string count))
            {
                needsPluralHandling = true;
                countAsDouble = double.Parse(count, CultureInfo.InvariantCulture);
            }

            bool isOrdinal = GetOtherBool(interpolation, "ordinal");

            bool needsZeroSuffixLookup = needsPluralHandling && !isOrdinal && countAsDouble == 0;

            string pluralSuffix = null;
            if (needsPluralHandling)
                pluralSuffix = "#" + Res.PluralResolver.GetSuffix(langCode, count, isOrdinal);
            string zeroSuffix = "#zero";
            string ordinalPrefix = "#ordinal#";
            if (needsPluralHandling)
            {
                keys.Add(key + pluralSuffix);
                if (isOrdinal && pluralSuffix.IndexOf(ordinalPrefix) == 0)
                    keys.Add(pluralSuffix.Replace(ordinalPrefix, "#"));
            }
            if (needsZeroSuffixLookup)
                keys.Add(key + zeroSuffix);

            // bool needsPluralHandling = options.count !== undefined && typeof options.count !== 'string';


            // do stuff here

            return keys;
        }

        private static bool GetOtherBool(Dictionary<string, string> other, string key)
        {
            if (ListUtils.isEmpty(other))
                return false;
            return other.TryGetValue(key, out string value) && value == "true";
        }

        public string GetMsg(string name, Dictionary<string, object> options)
        {
            EnsureDictionary();

            return null;

            // if (StringUtils.isEmpty(name))
            //     return null;

            // if (options != null && options.Count > 0)
            // {
            //     LocalizedString localizedString = ResolveComplex(name, options);
            //     if (localizedString != null && !localizedString.ResourceNotFound)
            //     {
            //         return localizedString;
            //     }
            // }

            // return GetMsg(name);
        }

        private LocalizedString ResolveComplex(string name, Dictionary<string, object> options)
        {
            EnsureDictionary();

            return null;

            // int? count = null;
            // if (options.ContainsKey("count"))
            //     count = (int)options["count"];

            // // TODO: resolve count based on language of resource

            // Dictionary<string, LocaleString> innerDict;
            // bool found = complexResDict.TryGetValue(name, out innerDict);
            // if (found) { }

            // return null;
        }
    }

    public class LocaleResources
    {
        private CultureInfo cultureInfo;
        private Dictionary<string, LocalizedString> dict = new();

        public LocaleResources(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
        }

        public string GetLangCode()
        {
            return cultureInfo.TwoLetterISOLanguageName;
        }

        public void TryAddResource(string resourceKey, LocalizedString locStr)
        {
            if (!StringUtils.isEmpty(resourceKey) && !dict.ContainsKey(resourceKey))
                dict[resourceKey] = locStr;
        }

        public LocalizedString TryGetValue(string resourceKey)
        {
            if (!StringUtils.isEmpty(resourceKey) && dict.ContainsKey(resourceKey))
                return dict[resourceKey];
            return null;
        }

        public bool IsEmpty()
        {
            return dict.Count < 1;
        }
    }

    public class MsgResult
    {
        public string langCode;
        public string msg;

        public MsgResult(string langCode, string msg)
        {
            if (StringUtils.isEmpty(langCode) || langCode == "iv")
                this.langCode = "en";
            else
                this.langCode = langCode;

            this.msg = msg;
        }
    }
}
