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

                resources.OnFinishedSetup();
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
                                        case "option1":
                                            return CustomMessages.messageOption1;
                                        case "option2":
                                            return CustomMessages.messageOption2;
                                        case "option3":
                                            return CustomMessages.messageOption3;
                                        case "player-name":
                                            return CustomMessages.playerName;
                                        case "horse-name":
                                            return CustomMessages.horseName;
                                        case "b-btn":
                                            return CustomMessages.bBtn;
                                        case "heart":
                                            return CustomMessages.heart;
                                        case "reference-mark":
                                            return CustomMessages.referenceMark;
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
                        case "en":
                            EnGenFunction(result);
                            break;
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

            private void EnGenFunction(List<KeyValuePair<string, string>> result)
            {
                switch (genFnName)
                {
                    case "ac":
                        EnGenFunctionAc(result);
                        break;
                    case "unc":
                        EnGenFunctionUnc(result);
                        break;
                    default:
                        throw new Exception($"Unrecognized genFnName '{genFnName}'.");
                }

                int abc = 7;
            }

            private void EnGenFunctionAc(List<KeyValuePair<string, string>> result)
            {
                if (genFnArgs == null || (genFnArgs.Length != 3 && genFnArgs.Length != 4))
                    throw new Exception(
                        $"Expected en ac genFnArgs to be non-null and length 3 or 4."
                    );

                string pluralHandling = genFnArgs[2];
                if (pluralHandling != "b" && pluralHandling != "s" && pluralHandling != "p")
                    throw new Exception(
                        $"Expected pluralMetaRaw to be 's' or 'p', but was '{pluralHandling}'."
                    );

                bool handleSingular = pluralHandling == "b" || pluralHandling == "s";
                bool handlePluralSpecial = pluralHandling == "p";
                bool handlePluralDefault = pluralHandling == "b";

                if (handlePluralDefault && genFnArgs.Length != 4)
                    throw new Exception(
                        $"When handlePluralDefault is true, expected genFnArgs to be length 4."
                    );

                string defArticle = genFnArgs[0] == "t" ? "the" : "";
                string indefArticle = genFnArgs[1];
                string pluralSuffix = genFnArgs.Length > 3 ? genFnArgs[3] : "";

                // plural handling 'd' for default, or 't' or 'f'

                // 'b' for both singular and plural, 's' for singular-only, 'p' for plural-only


                string pluralMeta = "$(plural)";
                bool isBasePlural = false;
                string baseMeta = isBasePlural ? pluralMeta : "";

                string wrappedBase = $"{{cs}}{baseValue}{{ce}}";

                if (handleSingular)
                {
                    // Base
                    result.Add(new(GenCurrentResKey(), baseMeta + wrappedBase));

                    if (!StringUtils.isEmpty(defArticle))
                    {
                        ChangeContext("def");
                        string defVal = baseMeta + defArticle + " " + wrappedBase;
                        result.Add(new(GenCurrentResKey(), defVal));
                    }

                    if (!StringUtils.isEmpty(indefArticle))
                    {
                        ChangeContext("indef");
                        string indefVal = indefArticle + " " + wrappedBase;
                        result.Add(new(GenCurrentResKey(), indefVal));
                    }

                    ChangeContext("count");
                    string countSingular = $"{{cs}}{{count}} {baseValue}{{ce}}";
                    result.Add(new(GenCurrentResKey(), countSingular));
                }

                if (handlePluralSpecial)
                {
                    ChangeContext("fishing-bottle");
                    string fishBottle = $"{pluralMeta}{{cs}}{baseValue}{{ce}}";
                    result.Add(new(GenCurrentResKey(), fishBottle));

                    ChangeContext("count");
                    count = "other";
                    string countPlural = $"{pluralMeta}{{cs}}{{count}} {baseValue}{{ce}}";
                    result.Add(new(GenCurrentResKey(), countPlural));
                }
                else if (handlePluralDefault && !StringUtils.isEmpty(pluralSuffix))
                {
                    ChangeContext("fishing-bottle");
                    string fishBottle = $"{pluralMeta}{{cs}}{baseValue}{pluralSuffix}{{ce}}";
                    result.Add(new(GenCurrentResKey(), fishBottle));

                    ChangeContext("count");
                    count = "other";
                    string countPlural =
                        $"{pluralMeta}{{cs}}{{count}} {baseValue}{pluralSuffix}{{ce}}";
                    result.Add(new(GenCurrentResKey(), countPlural));
                }
            }

            private void EnGenFunctionUnc(List<KeyValuePair<string, string>> result)
            {
                if (genFnArgs == null || genFnArgs.Length != 4)
                    throw new Exception($"Expected en unc genFnArgs to be non-null and length 4.");

                string pluralRaw = genFnArgs[3];
                if (pluralRaw != "s" && pluralRaw != "p")
                    throw new Exception(
                        $"Expected pluralRaw to be 's' or 'p', but was '{pluralRaw}'."
                    );

                string counter = genFnArgs[0];
                string defArticle = genFnArgs[1] == "t" ? "the" : null;
                string indefArticle = genFnArgs[2];
                bool isBasePlural = genFnArgs[3] == "p";

                string pluralMeta = "$(plural)";
                string baseMeta = isBasePlural ? pluralMeta : "";

                string wrappedBase = $"{{cs}}{baseValue}{{ce}}";

                // Base is always created
                result.Add(new(GenCurrentResKey(), baseMeta + wrappedBase));

                if (!StringUtils.isEmpty(defArticle))
                {
                    ChangeContext("def");
                    string defVal = baseMeta + defArticle + " " + wrappedBase;
                    result.Add(new(GenCurrentResKey(), defVal));
                }

                if (!StringUtils.isEmpty(indefArticle))
                {
                    ChangeContext("indef");
                    string indefVal = $"{indefArticle} {{cs}}{counter} of {baseValue}{{ce}}";
                    result.Add(new(GenCurrentResKey(), indefVal));
                }

                ChangeContext("count");
                string withCounterSingular = $"{{cs}}{{count}} {counter} of {baseValue}{{ce}}";
                result.Add(new(GenCurrentResKey(), withCounterSingular));

                count = "other";
                string withCounterPlural =
                    $"{pluralMeta}{{cs}}{{count}} {counter}s of {baseValue}{{ce}}";
                result.Add(new(GenCurrentResKey(), withCounterPlural));
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
                    result.Add(
                        new(
                            GenCurrentResKey(),
                            metaPrefix + (usesLApostrophe ? "{que-transform}" : "") + wrappedBase
                        )
                    );

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

        public MsgResult GetMsg(
            string key,
            Dictionary<string, string> interpolation = null,
            Dictionary<string, string> optionalContextMeta = null
        )
        {
            EnsureDictionary();

            for (int i = 0; i < resourcesList.Count; i++)
            {
                LocaleResources resources = resourcesList[i];

                // Handle resolving optional context which may vary between top
                // language and fallback language.
                Dictionary<string, string> resolvedInterpolation =
                    MergeInterpolationAndOptionalContext(
                        resources,
                        key,
                        interpolation,
                        optionalContextMeta
                    );

                string langCode = resources.GetLangCode();
                List<string> keysToTry = GenKeysToCheck(langCode, key, resolvedInterpolation);

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

        private Dictionary<string, string> MergeInterpolationAndOptionalContext(
            LocaleResources resources,
            string key,
            Dictionary<string, string> interpolationIn,
            Dictionary<string, string> optionalContextMeta = null
        )
        {
            Dictionary<string, string> interpolation;
            if (!ListUtils.isEmpty(interpolationIn))
                interpolation = new(interpolationIn);
            else
                interpolation = new();

            // Build context by combining static and optional for resKey
            HashSet<string> contextParts = null;
            if (
                interpolation.TryGetValue("context", out string staticContext)
                && !StringUtils.isEmpty(staticContext)
            )
            {
                contextParts = new(staticContext.Split(","));
            }

            Dictionary<string, string> contextDict = null;
            if (!ListUtils.isEmpty(optionalContextMeta))
                contextDict = FilterToRelevantContext(resources, key, optionalContextMeta);

            string context = CustomMsgData.BuildContextWithMeta(contextParts, contextDict);
            interpolation["context"] = context;

            return interpolation;
        }

        private Dictionary<string, string> FilterToRelevantContext(
            LocaleResources resources,
            string baseResKey,
            Dictionary<string, string> context
        )
        {
            Dictionary<string, string> filteredContext = new();
            if (StringUtils.isEmpty(baseResKey) || ListUtils.isEmpty(context))
                return filteredContext;

            HashSet<string> relevantContextVals = GetRelevantContextForBaseKey(
                resources,
                baseResKey
            );
            foreach (KeyValuePair<string, string> pair in context)
            {
                string key;
                if (pair.Value == "true")
                    key = pair.Key;
                else
                    key = pair.Key + "-" + pair.Value;

                if (relevantContextVals.Contains(key))
                    filteredContext[pair.Key] = pair.Value;
            }

            return filteredContext;
        }

        public string GetJunkHintResKey(uint number)
        {
            EnsureDictionary();

            for (int i = 0; i < resourcesList.Count; i++)
            {
                LocaleResources resources = resourcesList[i];

                string key = resources.GetJunkHintResKey(number);
                if (!StringUtils.isEmpty(key))
                    return key;
            }

            return null;
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

        private HashSet<string> GetRelevantContextForBaseKey(
            LocaleResources resources,
            string start
        )
        {
            EnsureDictionary();

            return resources.GetRelevantContextForBaseKey(start);
        }
    }

    public class LocaleResources
    {
        public CultureInfo cultureInfo { get; private set; }
        private Dictionary<string, string> dict = new();
        private Dictionary<string, HashSet<string>> baseResKeyToContextParts = new();
        private List<string> junkHintResKeys = new();

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
            {
                dict[resourceKey] = locStr;

                if (resourceKey.StartsWith("junk-hint."))
                {
                    junkHintResKeys.Add(resourceKey);
                }
            }
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

        public string GetJunkHintResKey(uint number)
        {
            if (ListUtils.isEmpty(junkHintResKeys))
                return null;

            int index = (int)number % junkHintResKeys.Count;

            return junkHintResKeys[index];
        }

        public void OnFinishedSetup()
        {
            // Sort junk hints since the order they are added to the list is not
            // guaranteed to be what you might expect.
            junkHintResKeys.Sort(StringComparer.Ordinal);
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
