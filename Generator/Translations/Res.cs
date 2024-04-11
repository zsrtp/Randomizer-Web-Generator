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
    using System.Text;
    using TPRandomizer.Assets;
    using System.Linq;

    public partial class Res
    {
        [GeneratedRegex(@"{([a-z0-9-]+)(?:\(([a-z0-9-:,]*)\))?}")]
        private static partial Regex ResourceVal();

        [GeneratedRegex(@"^\$\(([a-z0-9:,]*)\)")]
        private static partial Regex MetaVal();

        [GeneratedRegex(@"^(?:\{[^}]*\})+(.)")]
        private static partial Regex UppercaseVal();

        [GeneratedRegex(@"^\s")]
        private static partial Regex WhiteSpaceChar();

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
            MsgResult msgResult = translations.GetMsg(resKey, interpolation);
            ParsedRes parsedRes = new(msgResult.cultureInfo, msgResult.langCode);
            string resVal = msgResult.msg;

            if (!msgResult.foundValue)
            {
                parsedRes.value = resVal;
                return parsedRes;
            }

            HashSet<string> seenInterpolationKeys = new();

            resVal = MetaVal()
                .Replace(
                    resVal,
                    (match) =>
                    {
                        if (match.Success)
                        {
                            string metaRaw = match.Groups[1].Value;
                            parsedRes.meta = ParseOtherGroup(metaRaw);
                        }
                        return "";
                    }
                );

            resVal = ResourceVal()
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
                            parsedRes.slotMeta[keyVal] = dict;
                        }

                        seenInterpolationKeys.Add(keyVal);

                        return $"{{{keyVal}}}";
                    }
                );

            parsedRes.foundValue = true;
            parsedRes.value = resVal;

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

                if (chunkHalves.Length < 1 || chunkHalves.Length > 2)
                    throw new Exception(
                        $"Invalid chunkHalves length '{chunkHalves.Length}' for 'val'."
                    );

                string key = chunkHalves[0];
                string value = chunkHalves.Length == 2 ? chunkHalves[1] : "true";

                if (StringUtils.isEmpty(key) || StringUtils.isEmpty(value))
                    throw new Exception("chunkHalf was empty.");

                if (result.ContainsKey(key))
                    throw new Exception($"Duplicate chunkHalf key '{key}'.");

                result[key] = value;
            }

            return result;
        }

        public static string LangSpecificNormalize(string valIn)
        {
            string input = Regex.Unescape(valIn);

            // List<string> escapedList = new();
            List<TextChunk> chunks = new();

            int index = 0;
            // StringBuilder sb = new();
            TextChunk currentChunk = new TextChunk();
            while (index < input.Length)
            {
                string currentChar = input.Substring(index, 1);
                byte byteVal = (byte)currentChar[0];

                if (byteVal == 0x1A)
                {
                    // determine how many chars to pull out.
                    byte escLength = (byte)input[index + 1];

                    // For Japanese only (since non-ja is always one byte per
                    // char), we may need to convert the string to bytes and
                    // process that way since an escape sequence (with furigana
                    // for example) will have fewer chars in it that the actual
                    // byte length of the sequence.
                    string escapeSequence = input.Substring(index, escLength);
                    currentChunk.AddEscapeSequence(escapeSequence);
                    // currentChunk.escapedList.Add(escapeSequence);
                    index += escLength;
                    continue;
                }

                if (WhiteSpaceChar().IsMatch(currentChar))
                {
                    if (currentChunk.textType == TextChunk.Type.Text)
                    {
                        currentChunk.BuildVal();
                        chunks.Add(currentChunk);

                        currentChunk = new();
                        currentChunk.textType = TextChunk.Type.Whitespace;
                        currentChunk.AddChar(currentChar);
                    }
                    else
                    {
                        currentChunk.textType = TextChunk.Type.Whitespace;
                        currentChunk.AddChar(currentChar);
                    }
                }
                else
                {
                    if (currentChunk.textType == TextChunk.Type.Whitespace)
                    {
                        currentChunk.BuildVal();
                        chunks.Add(currentChunk);

                        currentChunk = new();
                        currentChunk.textType = TextChunk.Type.Text;
                        currentChunk.AddChar(currentChar);
                    }
                    else
                    {
                        currentChunk.textType = TextChunk.Type.Text;
                        currentChunk.AddChar(currentChar);
                    }
                }

                index += 1;
            }

            // Expected to always add unless the string was just empty.
            if (currentChunk.textType != TextChunk.Type.Unknown)
            {
                currentChunk.BuildVal();
                chunks.Add(currentChunk);
            }

            // Let's assume french right now.

            // Need to break into chunks (and preserve whitespace). We also need
            // to maintain the position escaped sequences. We also need to
            // probably run the thing to convert '\\n' as 2 chars to '\n' first.

            // Now we should handle converting any "que" + whitespace + "une" to "qu'une"

            // To do this, we iterate over the chunks.
            for (int i = 0; i < chunks.Count; i++)
            {
                TextChunk chunk = chunks[i];
                if (i < chunks.Count - 2)
                {
                    if (chunk.textType == TextChunk.Type.Text)
                    {
                        switch (chunk.val)
                        {
                            case "que":
                            {
                                if (chunks[i + 1].textType == TextChunk.Type.Whitespace)
                                {
                                    string secondVal = chunks[i + 2].val;
                                    if (secondVal == "un")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 2);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 3, 5);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "qu'un";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal == "une")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 2);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 3, 6);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "qu'une";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal.StartsWith("{que-transform}"))
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 2);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "qu'";
                                        chunks.RemoveRange(i, 2);
                                        chunks.Insert(i, newChunk);
                                    }
                                }
                                break;
                            }
                            case "à":
                            {
                                if (chunks[i + 1].textType == TextChunk.Type.Whitespace)
                                {
                                    string secondVal = chunks[i + 2].val;
                                    if (secondVal == "le")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 0);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 2, 2);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 2, 2);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "au";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal == "les")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 0);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 3, 3);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "aux";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                if (chunk.textType == TextChunk.Type.Text)
                {
                    if (chunk.val.StartsWith("{que-transform}"))
                        chunk.RemoveRange(0, "{que-transform}".Length);
                }
            }

            AddLineBreaksToChunks(chunks);

            string result = "";
            foreach (TextChunk chunk in chunks)
            {
                for (int i = 0; i <= chunk.val.Length; i++)
                {
                    if (chunk.escapesAtIndexes.TryGetValue(i, out List<string> escapeSequences))
                    {
                        foreach (string seq in escapeSequences)
                        {
                            result += seq;
                        }
                    }

                    if (i < chunk.val.Length)
                        result += chunk.val[i];
                }
            }

            return result;
        }

        private static void TransformEscSeqList(
            TextChunk oldTextChunk,
            TextChunk newTextChunk,
            int newStart,
            int newCap
        )
        {
            Dictionary<int, List<string>> oldDict = oldTextChunk.escapesAtIndexes;
            if (ListUtils.isEmpty(oldDict))
                return;

            Dictionary<int, List<string>> newDict = newTextChunk.escapesAtIndexes;

            List<int> keyNumbers = oldDict.Keys.ToList();
            keyNumbers.Sort();

            foreach (int key in keyNumbers)
            {
                int newKey = key + newStart;
                if (newKey > newCap)
                    newKey = newCap;

                if (!newDict.TryGetValue(key, out List<string> values))
                {
                    values = new();
                    newDict[newKey] = values;
                }

                values.AddRange(oldDict[key]);
            }
        }

        private static void AddLineBreaksToChunks(List<TextChunk> chunks)
        {
            // If any '\n' show up in the chunks, then we need to not add any
            // linebreaks before them no matter what.

            // Start from the back. Iterate until we find a Whitespace one which
            // contains a linebreak. Starting from after the last line break in
            // this one, we go forward in order to determine where to place
            // linebreaks.

            int firstAllowedIndex = 0;
            int breakIndex = 0;
            for (int i = chunks.Count - 1; i >= 0; i--)
            {
                TextChunk chunk = chunks[i];
                if (chunk.textType == TextChunk.Type.Whitespace)
                {
                    int lastBreakIndex = chunk.val.LastIndexOf('\n');
                    if (lastBreakIndex >= 0)
                    {
                        breakIndex = lastBreakIndex;
                        firstAllowedIndex = i;
                        break;
                    }
                }
            }

            // Need to start at first chunk and count up characters.

            int currChars = 0;
            // Need to look at current length based on previous chunks.

            // Then we add the length for this chunk and up to the next text
            // chunk. If it would be more than the cutoff for breaking, then we
            // need to

            // For now, let's not worry about a case where someone has a
            // whitespace chunk that is 50 spaces in a row. This should never
            // occur and it would complicate the code a lot. -isaac

            // foreach (TextChunk chunk in chunks)
            for (int i = 0; i < chunks.Count; i++)
            {
                TextChunk chunk = chunks[i];
                if (chunk.textType == TextChunk.Type.Text)
                {
                    currChars += GetTextTypeChunkLength(chunk);
                }
                else if (chunk.textType == TextChunk.Type.Whitespace)
                {
                    int lastBreakIndex = chunk.val.LastIndexOf('\n');
                    if (lastBreakIndex >= 0)
                    {
                        currChars = chunk.val.Length - 1 - lastBreakIndex;
                        continue;
                    }
                    else
                    {
                        currChars += chunk.val.Length;
                    }

                    if (i >= firstAllowedIndex && i + 1 < chunks.Count)
                    {
                        int wouldBeLength =
                            currChars + chunk.val.Length + GetTextTypeChunkLength(chunks[i + 1]);

                        if (wouldBeLength > 35)
                        {
                            // Need to break;
                            chunk.val = "\n" + chunk.val[1..];
                            currChars = chunk.val.Length - 1;
                        }
                    }
                }
                // need to iterate through

            }

            int abc = 7;
        }

        private static int GetTextTypeChunkLength(TextChunk textChunk)
        {
            if (textChunk.textType == TextChunk.Type.Text)
            {
                int length = textChunk.val.Length;
                foreach (KeyValuePair<int, List<string>> pair in textChunk.escapesAtIndexes)
                {
                    foreach (string val in pair.Value)
                    {
                        if (val == CustomMessages.playerName)
                            length += 8;
                    }
                }
                return length;
            }
            return 0;
        }

        private class TextChunk
        {
            public enum Type
            {
                Unknown,
                Whitespace,
                Text,
            }

            public Type textType = Type.Unknown;
            public string val;
            public Dictionary<int, List<string>> escapesAtIndexes = new();

            private StringBuilder builder = new();

            public void AddChar(string character)
            {
                builder.Append(character);
            }

            public void AddEscapeSequence(string sequence)
            {
                int key = builder.Length;
                if (!escapesAtIndexes.TryGetValue(key, out List<string> values))
                {
                    values = new();
                    escapesAtIndexes[key] = values;
                }
                values.Add(sequence);
            }

            public void BuildVal()
            {
                val = builder.ToString();
                builder.Clear();
            }

            public void RemoveRange(int startIndex, int count)
            {
                if (startIndex < 0 || startIndex >= val.Length)
                    throw new Exception("TextChunk startIndex out of range");
                if (startIndex + count > val.Length)
                    throw new Exception("TextChunk count out of range");

                List<char> charList = val.ToList();
                charList.RemoveRange(startIndex, count);
                val = string.Join("", charList);

                if (!ListUtils.isEmpty(escapesAtIndexes))
                {
                    Dictionary<int, List<string>> newDict = new();
                    foreach (KeyValuePair<int, List<string>> pair in escapesAtIndexes)
                    {
                        int newKey = pair.Key;
                        if (newKey > startIndex)
                            newKey -= count;

                        newDict[newKey] = pair.Value;
                    }
                    escapesAtIndexes = newDict;
                }
            }
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

                string result = ord ? "ordinal#" : "";

                switch (langCode)
                {
                    case "en":
                        result += GetSuffixEn(val, ord);
                        break;
                    case "es":
                        result += GetSuffixEs(val, ord);
                        break;
                    case "fr":
                        result += GetSuffixFr(val, ord);
                        break;
                    default:
                        throw new Exception($"'{langCode}' is not a supported langCode.");
                }

                // We need to know which langauge we are looking at. This
                // depends on the resource sinc it is possible we are resolving
                // an English resource for French if it has not yet been defined
                // for French yet (or never will be).
                return result;
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
            public CultureInfo cultureInfo { get; private set; }
            public string langCode { get; private set; }
            public bool foundValue;
            public string value;
            public Dictionary<string, string> meta = new();
            public Dictionary<string, Dictionary<string, string>> slotMeta = new();

            public ParsedRes(CultureInfo cultureInfo, string langCode)
            {
                this.cultureInfo = cultureInfo;
                this.langCode = langCode;
            }

            public void CapitalizeFirstValidChar()
            {
                if (StringUtils.isEmpty(value))
                    return;

                if (value[0] == '{')
                {
                    // Search using regex
                    value = UppercaseVal()
                        .Replace(
                            value,
                            (match) =>
                            {
                                string fullVal = match.Groups[0].Value;
                                int index = match.Groups[1].Index;

                                return string.Concat(
                                    fullVal.AsSpan(0, index),
                                    fullVal.Substring(index, 1).ToUpper(cultureInfo),
                                    fullVal.AsSpan(index + 1)
                                );
                                // return match.Groups[1].Value.ToUpper(cultureInfo);
                            }
                        );
                }
                else
                {
                    value = string.Concat(
                        value.Substring(0, 1).ToUpper(cultureInfo),
                        value.AsSpan(1)
                    );
                }
                //
            }

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

            public string ResolveWithColors(string startColor, string endColor)
            {
                if (value.Contains("{cs}"))
                    return Substitute(new() { { "cs", startColor }, { "ce", endColor }, });

                return startColor + Substitute(null) + endColor;
            }
        }
    }
}
