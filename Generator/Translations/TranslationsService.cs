// Note: it is important that the namespace of this file be just "TPRandomizer"
// and that the class name matches the names of the resx files ("Translations").
// The way the resx files are found is extremely confusing, but you can look
// here for some amount of info:
// https://github.com/aspnet/Localization/issues/340 -isaac
namespace TPRandomizer
{
    using System.Resources;
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
    using System.Text;
    using TPRandomizer.Assets;
    using System.Linq.Expressions;

    // public sealed class Translations(IStringLocalizer<Translations> localizer)
    // public sealed class Translations(IStringLocalizer<Translations> localizer)
    public sealed partial class Translations
    {
        [GeneratedRegex(
            @"^([0-9a-z-_.]+)(?:\$([0-9a-z-,]+))?(#ordinal)?(?:#([a-z]+))?(?:@([0-9a-z-_]+)\(([a-z-_,]*)\))?$",
            RegexOptions.IgnoreCase
        )]
        public static partial Regex ResKeyRegex();

        [GeneratedRegex(@"(?:#\(([a-z0-9-]+)\))")]
        private static partial Regex EscSeqAlias();

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
                string langCode = resources.GetLangCode();

                using (var enumerator = localizer.GetAllStrings(false).GetEnumerator())
                {
                    while (true)
                    {
                        bool didMoveNext = false;
                        try
                        {
                            didMoveNext = enumerator.MoveNext();
                        }
                        catch (Exception)
                        { // do nothing
                        }
                        if (!didMoveNext)
                            break;

                        LocalizedString locStr = enumerator.Current;

                        List<KeyValuePair<string, string>> resKvPairs = ResolveResourceKey(
                            out ResKeyParts resKeyParts,
                            langCode,
                            locStr
                        );
                        foreach (KeyValuePair<string, string> pair in resKvPairs)
                        {
                            resources.TryAddResource(pair.Key, pair.Value);
                        }
                        if (resKeyParts != null)
                        {
                            resources.AddBaseKeyToContextParts(
                                resKeyParts.resKeyBase,
                                resKeyParts.relevantContexts
                            );
                        }
                    }
                }

                resourcesList.Add(resources);

                if (CultureInfo.CurrentCulture.ThreeLetterISOLanguageName == "ivl")
                    shouldBreak = true;
                else
                    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo
                        .CurrentCulture
                        .Parent;

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

        private static List<KeyValuePair<string, string>> ResolveResourceKey(
            out ResKeyParts resKeyParts,
            string langCode,
            LocalizedString locStr
        )
        {
            Match match = ResKeyRegex().Match(locStr.Name);

            if (!match.Success)
            {
                resKeyParts = null;
                return null;
            }

            resKeyParts = new(langCode, locStr.Value, match);
            List<KeyValuePair<string, string>> resKvPairs = resKeyParts.GenOutputKeyValPairs();
            return resKvPairs;

            // string resKey = match.Groups[1].Value;

            // Group contextGroup = match.Groups[2];
            // if (contextGroup.Success)
            // {
            //     string[] contextChunks = contextGroup.Value.Split(',');
            //     Array.Sort(contextChunks, StringComparer.Ordinal);
            //     resKey += "$" + string.Join(",", contextChunks);
            // }

            // Group ordinalGroup = match.Groups[3];
            // if (ordinalGroup.Success)
            //     resKey += ordinalGroup.Value; // already includes leading '#'

            // string countName = null;
            // Group countGroup = match.Groups[4];
            // if (countGroup.Success)
            // {
            //     countName = countGroup.Value;
            //     resKey += "#" + countGroup.Value;
            // }

            // string genFn = null;
            // Group genFnGroup = match.Groups[5];
            // if (genFnGroup.Success)
            //     genFn = genFnGroup.Value;

            // string[] genFnArgs = null;
            // Group genFnArgsGroup = match.Groups[6];
            // if (genFnArgsGroup.Success)
            //     genFnArgs = genFnArgsGroup.Value.Split(',');

            // List<KeyValuePair<string, string>> outputKeyAndVal = new();
            // outputKeyAndVal.Add(new(resKey, locStr.Value));

            // if (!StringUtils.isEmpty(genFn))
            // {
            //     switch (langCode)
            //     {
            //         case "fr":
            //             FrGenFunction(outputKeyAndVal, genFn, genFnArgs, resKey, countName);
            //             break;
            //         default:
            //             // do nothing
            //             break;
            //     }
            // }

            // return resKey;
            // return null;
        }

        private class ResKeyParts
        {
            private string langCode;
            private string baseValue;
            public string resKeyBase;
            private List<string> origContextList = new();
            private List<string> activeContextList = new();
            public HashSet<string> relevantContexts = new();
            private string ordinal;
            private string count;
            private string genFnName;
            private string[] genFnArgs;

            public ResKeyParts(string langCode, string baseValue, Match match)
            {
                // Transform baseValue if it contains escapeSequence aliases.
                if (!StringUtils.isEmpty(baseValue))
                {
                    baseValue = EscSeqAlias()
                        .Replace(
                            baseValue,
                            (match) =>
                            {
                                if (match.Success)
                                {
                                    string escSeqAlias = match.Groups[1].Value;
                                    switch (escSeqAlias)
                                    {
                                        case "player-name":
                                            return CustomMessages.playerName;
                                        case "white":
                                            return CustomMessages.messageColorWhite;
                                        case "red":
                                            return CustomMessages.messageColorRed;
                                        case "green":
                                            return CustomMessages.messageColorGreen;
                                        case "light-blue":
                                            return CustomMessages.messageColorLightBlue;
                                        case "yellow":
                                            return CustomMessages.messageColorYellow;
                                        case "purple":
                                            return CustomMessages.messageColorPurple;
                                        case "orange":
                                            return CustomMessages.messageColorOrange;
                                        case "dark-green":
                                            return CustomMessages.messageColorDarkGreen;
                                        case "dark-blue":
                                            return CustomMessages.messageColorBlue;
                                        case "silver":
                                            return CustomMessages.messageColorSilver;
                                        default:
                                            throw new Exception(
                                                $"Failed to understand escSeqAlias '{escSeqAlias}'."
                                            );
                                    }
                                }
                                throw new Exception("Unexpected error in EscSeqAlias Replace.");
                            }
                        );
                }

                this.langCode = langCode;
                this.baseValue = baseValue;
                resKeyBase = match.Groups[1].Value;

                Group contextGroup = match.Groups[2];
                if (contextGroup.Success)
                {
                    string[] contextChunks = contextGroup.Value.Split(',');
                    Array.Sort(contextChunks, StringComparer.Ordinal);
                    origContextList = new(contextChunks);
                }

                Group ordinalGroup = match.Groups[3];
                if (ordinalGroup.Success)
                    ordinal = ordinalGroup.Value;

                Group countGroup = match.Groups[4];
                if (countGroup.Success)
                    count = countGroup.Value;

                Group genFnGroup = match.Groups[5];
                if (genFnGroup.Success)
                    genFnName = genFnGroup.Value;

                Group genFnArgsGroup = match.Groups[6];
                if (genFnArgsGroup.Success)
                    genFnArgs = genFnArgsGroup.Value.Split(',');

                ChangeContext();
            }

            public List<KeyValuePair<string, string>> GenOutputKeyValPairs()
            {
                List<KeyValuePair<string, string>> result = new();

                if (!StringUtils.isEmpty(genFnName))
                {
                    switch (langCode)
                    {
                        case "fr":
                            FrGenFunction(result);
                            break;
                        default:
                            // do nothing
                            break;
                    }
                }

                // Add the base key if nothing was added by the generator fn.
                if (ListUtils.isEmpty(result))
                    result.Add(new(GenCurrentResKey(), baseValue));

                return result;
            }

            private void ChangeContext(string additionalContext = null)
            {
                activeContextList = new(origContextList);
                if (!StringUtils.isEmpty(additionalContext))
                    activeContextList.Add(additionalContext);

                relevantContexts.UnionWith(activeContextList);
            }

            private string GenCurrentResKey()
            {
                string ret = resKeyBase;

                if (!ListUtils.isEmpty(activeContextList))
                {
                    activeContextList.Sort(StringComparer.Ordinal);
                    ret += "$" + string.Join(",", activeContextList);
                }

                if (!StringUtils.isEmpty(ordinal))
                    ret += ordinal; // already starts with '#'

                if (!StringUtils.isEmpty(count))
                    ret += "#" + count;

                return ret;
            }

            private void FrGenFunction(List<KeyValuePair<string, string>> result)
            {
                if (genFnName != "ac")
                    throw new Exception($"Expected genFn to be 'ac', but was '{genFnName}'.");
                if (!StringUtils.isEmpty(count) && count != "other")
                    throw new Exception(
                        $"Expected countName to be 'other' or empty, but was '{count}'."
                    );
                if (genFnArgs == null || genFnArgs.Length < 1)
                    throw new Exception($"Expected genFnArgs to be non-empty.");

                if (genFnArgs[0] != "m" && genFnArgs[0] != "f")
                    throw new Exception(
                        $"Expected genFnArgs[0] to be 'm' or 'f', but was '{genFnArgs[0]}'."
                    );

                bool isMasculine = genFnArgs[0] == "m";
                string wrappedBase = $"{{cs}}{baseValue}{{ce}}";

                if (count == "other")
                {
                    string metaPrefix = $"$(gender:{genFnArgs[0]},plural)";

                    // Plural
                    if (genFnArgs.Length != 1)
                        throw new Exception(
                            $"Expected genFnArgs to be 1 element for plural, but was '{genFnArgs.Length}'."
                        );

                    // Base
                    result.Add(new(GenCurrentResKey(), metaPrefix + wrappedBase));

                    // Definite article
                    ChangeContext("def");
                    string defVal = metaPrefix + "les " + wrappedBase;
                    result.Add(new(GenCurrentResKey(), defVal));

                    // Indefinite article
                    ChangeContext("indef");
                    string indefVal = metaPrefix + "des " + wrappedBase;
                    result.Add(new(GenCurrentResKey(), indefVal));

                    // Count
                    ChangeContext("count");
                    string countVal = metaPrefix + "{cs}{count} " + baseValue + "{ce}";
                    result.Add(new(GenCurrentResKey(), countVal));
                }
                else
                {
                    string metaPrefix = $"$(gender:{genFnArgs[0]})";
                    string negativeMetaPrefix = $"$(gender:{genFnArgs[0]},negative)";

                    // Non-plural
                    if (genFnArgs.Length > 2)
                        throw new Exception(
                            $"Expected genFnArgs to be 1 or 2 elements, but was '{genFnArgs.Length}'."
                        );

                    bool usesLApostrophe = false;
                    if (genFnArgs.Length > 1)
                    {
                        if (genFnArgs[1] != "true" && genFnArgs[1] != "false")
                            throw new Exception(
                                $"Expected genFnArgs[1] to be missing, 'true', or 'false', but was '{genFnArgs[1]}'."
                            );
                        usesLApostrophe = genFnArgs[1] == "true";
                    }

                    // Base
                    result.Add(new(GenCurrentResKey(), metaPrefix + wrappedBase));

                    // Definite article
                    ChangeContext("def");
                    string defPrefix = "l'";
                    if (!usesLApostrophe)
                    {
                        if (isMasculine)
                            defPrefix = "le ";
                        else
                            defPrefix = "la ";
                    }
                    string defVal = metaPrefix + defPrefix + wrappedBase;
                    result.Add(new(GenCurrentResKey(), defVal));

                    // Indefinite article
                    ChangeContext("indef");
                    string indefPrefix = isMasculine ? "un " : "une ";
                    string indefVal = metaPrefix + indefPrefix + wrappedBase;
                    result.Add(new(GenCurrentResKey(), indefVal));

                    // Count
                    ChangeContext("count");
                    string countVal = metaPrefix + "{cs}{count} " + baseValue + "{ce}";
                    result.Add(new(GenCurrentResKey(), countVal));

                    // Handle 'count' context and 'zero' count
                    count = "zero";
                    string noneWord = isMasculine ? "aucun" : "aucune";
                    string noneCountVal = $"{negativeMetaPrefix}{{cs}}{noneWord} {baseValue}{{ce}}";
                    result.Add(new(GenCurrentResKey(), noneCountVal));
                }
            }
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

                string langCode = resources.GetLangCode();
                List<string> keysToTry = GenKeysToCheck(langCode, key, interpolation);

                for (int keyIdx = keysToTry.Count - 1; keyIdx >= 0; keyIdx--)
                {
                    string keyToTry = keysToTry[keyIdx];

                    string value = resources.TryGetValue(keyToTry);
                    if (value != null)
                        return new MsgResult(resources.cultureInfo, value, true);
                }
            }

            LocaleResources finalResources = resourcesList[^1];
            return new MsgResult(finalResources.cultureInfo, key, false);
        }

        private List<string> GenKeysToCheck(
            string langCode,
            string key,
            Dictionary<string, string> interpolation
        )
        {
            // Based on:
            // https://github.com/i18next/i18next/blob/002268b0249a3670367d8f2d65bd7e665712036b/src/Translator.js#L448
            // except we give ordinal plurals priority over non-ordinals. If you
            // specifically ask for an ordinal, then I am not sure why this
            // would not have higher priority over the less specific
            // non-ordinal. -isaac

            if (StringUtils.isEmpty(key))
                throw new Exception("key must not be empty.");

            List<string> keys = new() { key };

            if (ListUtils.isEmpty(interpolation))
                return keys;

            bool needsPluralHandling = false;
            double countAsDouble = -1;
            if (interpolation.TryGetValue("count", out string count) && !StringUtils.isEmpty(count))
            {
                needsPluralHandling = true;
                countAsDouble = double.Parse(count, CultureInfo.InvariantCulture);
            }

            bool isOrdinal = GetOtherBool(interpolation, "ordinal");

            bool needsZeroSuffixLookup = needsPluralHandling && !isOrdinal && countAsDouble == 0;

            bool needsContextHandling =
                interpolation.TryGetValue("context", out string context)
                && !StringUtils.isEmpty(context);

            string pluralSuffix = null;
            if (needsPluralHandling)
                pluralSuffix = "#" + Res.PluralResolver.GetSuffix(langCode, count, isOrdinal);
            string zeroSuffix = "#zero";
            string ordinalPrefix = "#ordinal#";

            if (needsPluralHandling)
            {
                // If ordinal is true, we append the non-ordinal first (lower
                // priority).
                if (isOrdinal && pluralSuffix.IndexOf(ordinalPrefix) == 0)
                    keys.Add(key + pluralSuffix.Replace(ordinalPrefix, "#"));
                keys.Add(key + pluralSuffix);
            }
            if (needsZeroSuffixLookup)
                keys.Add(key + zeroSuffix);

            // Repeat for with context
            if (needsContextHandling)
            {
                string contextKey = key + "$" + context;
                keys.Add(contextKey);

                if (needsPluralHandling)
                {
                    // If ordinal is true, we append the non-ordinal first
                    // (lower priority).
                    if (isOrdinal && pluralSuffix.IndexOf(ordinalPrefix) == 0)
                        keys.Add(contextKey + pluralSuffix.Replace(ordinalPrefix, "#"));
                    keys.Add(contextKey + pluralSuffix);
                }
                if (needsZeroSuffixLookup)
                    keys.Add(contextKey + zeroSuffix);
            }

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

        public HashSet<string> GetRelevantContextForBaseKey(string start)
        {
            // Only operate on ideal language. Ideally we should not be using
            // fallbacks anyway.
            return resourcesList[0].GetRelevantContextForBaseKey(start);
        }
    }

    public class LocaleResources
    {
        public CultureInfo cultureInfo { get; private set; }
        private Dictionary<string, string> dict = new();
        private Dictionary<string, HashSet<string>> baseResKeyToContextParts = new();

        public LocaleResources(CultureInfo cultureInfo)
        {
            this.cultureInfo = cultureInfo;
        }

        public string GetLangCode()
        {
            return MsgResult.NormalizeLangCode(cultureInfo.TwoLetterISOLanguageName);
        }

        public void TryAddResource(string resourceKey, string locStr)
        {
            if (!StringUtils.isEmpty(resourceKey) && !dict.ContainsKey(resourceKey))
                dict[resourceKey] = locStr;
        }

        public string TryGetValue(string resourceKey)
        {
            if (!StringUtils.isEmpty(resourceKey) && dict.ContainsKey(resourceKey))
                return dict[resourceKey];
            return null;
        }

        public bool IsEmpty()
        {
            return dict.Count < 1;
        }

        public void AddBaseKeyToContextParts(string baseResKey, HashSet<string> contextParts)
        {
            if (StringUtils.isEmpty(baseResKey) || ListUtils.isEmpty(contextParts))
                return;

            if (!baseResKeyToContextParts.TryGetValue(baseResKey, out HashSet<string> current))
            {
                current = new();
                baseResKeyToContextParts[baseResKey] = current;
            }
            current.UnionWith(contextParts);
        }

        public HashSet<string> GetRelevantContextForBaseKey(string baseKey)
        {
            if (
                !StringUtils.isEmpty(baseKey)
                && baseResKeyToContextParts.TryGetValue(baseKey, out HashSet<string> result)
            )
                return result;
            return new();
        }
    }

    public class MsgResult
    {
        public CultureInfo cultureInfo;
        public string langCode;
        public string msg;
        public bool foundValue;

        public MsgResult(CultureInfo cultureInfo, string msg, bool foundValue)
        {
            this.cultureInfo = cultureInfo;
            this.langCode = NormalizeLangCode(cultureInfo.TwoLetterISOLanguageName);
            this.msg = msg;
            this.foundValue = foundValue;
        }

        public static string NormalizeLangCode(string twoCharLangCode)
        {
            if (StringUtils.isEmpty(twoCharLangCode) || twoCharLangCode == "iv")
                return "en";
            return twoCharLangCode;
        }
    }
}
