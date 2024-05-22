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

        [GeneratedRegex(@"^\$\(([a-z0-9-:,]*)\)")]
        private static partial Regex MetaVal();

        [GeneratedRegex(@"^(?:\{[^}]*\})+(.)")]
        private static partial Regex UppercaseVal();

        [GeneratedRegex(@"^\s")]
        private static partial Regex WhiteSpaceChar();

        [GeneratedRegex(@"\u2642|\u2640")]
        private static partial Regex MaleOrFemSign();

        private static Translations translations;

        static Res()
        {
            IServiceProvider provider = Global.GetServiceProvider();
            translations = provider.GetRequiredService<Translations>();
        }

        public static bool IsCultureJa()
        {
            return CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja";
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

            CultureInfo.CurrentCulture = newCultureInfo;
            CultureInfo.CurrentUICulture = newCultureInfo;

            translations.OnCultureChange();
        }

        public static string SimpleMsg(
            string resKey,
            Dictionary<string, string> interpolation = null,
            Dictionary<string, string> optionalContextMeta = null
        )
        {
            Result result = Msg(resKey, interpolation, optionalContextMeta);
            return result.Substitute(interpolation);
        }

        public static Result Msg(
            string resKey,
            Dictionary<string, string> interpolation = null,
            Dictionary<string, string> optionalContextMeta = null
        )
        {
            MsgResult msgResult = translations.GetMsg(resKey, interpolation, optionalContextMeta);
            Result parsedRes = new(msgResult.cultureInfo, msgResult.langCode);
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

        public static string GetEscRenderedChar(string escapeSequence)
        {
            if (StringUtils.isEmpty(escapeSequence))
                throw new Exception($"Did not expect an empty escSequence.");

            switch (escapeSequence)
            {
                case CustomMessages.maleSign:
                    return "\u2642";
                case CustomMessages.femaleSign:
                    return "\u2640";
                case CustomMessages.referenceMark:
                    return "\u203B";
                case CustomMessages.playerName:
                    return "Link";
                case CustomMessages.horseName:
                    return "Epona";
                case CustomMessages.bBtn:
                    return "B";
                case CustomMessages.heart:
                    return "\u2665";
                default:
                    return null;
            }
        }

        public static int GetEscRenderedCharLength(string escapeSequence)
        {
            if (
                escapeSequence == CustomMessages.playerName
                || escapeSequence == CustomMessages.horseName
            )
                return 8;

            string renderedStr = GetEscRenderedChar(escapeSequence);
            if (!StringUtils.isEmpty(renderedStr))
                return renderedStr.Length;

            return 0;
        }

        public static string GetColorFromEscSequence(string escapeSequence)
        {
            switch (escapeSequence)
            {
                case CustomMessages.messageColorWhite:
                    return "white";
                case CustomMessages.messageColorRed:
                    return "red";
                case CustomMessages.messageColorGreen:
                    return "green";
                case CustomMessages.messageColorLightBlue:
                    return "light blue";
                case CustomMessages.messageColorYellow:
                    return "yellow";
                case CustomMessages.messageColorPurple:
                    return "purple";
                case CustomMessages.messageColorOrange:
                    return "orange";
                case CustomMessages.messageColorDarkGreen:
                    return "dark green";
                case CustomMessages.messageColorBlue:
                    return "blue";
                case CustomMessages.messageColorSilver:
                    return "silver";
                default:
                    return null;
            }
        }

        public static string GetJunkHintText(uint number)
        {
            string resKey = translations.GetJunkHintResKey(number);

            if (StringUtils.isEmpty(resKey))
            {
                // Failed to find any junkHints to use.
                return "Junk hint!";
            }

            return SimpleMsg(resKey);
        }

        public static string LangSpecificNormalize(
            string valIn,
            int? maxLength = null,
            bool addLineBreaks = true
        )
        {
            string input = Regex.Unescape(valIn);
            input = MaleOrFemSign()
                .Replace(
                    input,
                    (match) =>
                    {
                        if (match.Success)
                        {
                            string character = match.Value;
                            if (character == "♂")
                                return CustomMessages.maleSign;
                            else if (character == "♀")
                                return CustomMessages.femaleSign;
                        }
                        return "";
                    }
                );

            // List<string> escapedList = new();
            List<TextChunk> chunks = new();

            int index = 0;
            // StringBuilder sb = new();
            TextChunk currentChunk = new TextChunk();
            while (index < input.Length)
            {
                string currentChar = input.Substring(index, 1);
                byte byteVal = (byte)currentChar[0];

                string renderedEsc = null;
                bool hasRenderedEsc = false;
                byte escLength = 0;

                if (byteVal == 0x1A)
                {
                    // determine how many chars to pull out.
                    escLength = (byte)input[index + 1];
                    // For Japanese only (since non-ja is always one byte per
                    // char), we may need to convert the string to bytes and
                    // process that way since an escape sequence (with furigana
                    // for example) will have fewer chars in it that the actual
                    // byte length of the sequence.
                    string escapeSequence = input.Substring(index, escLength);
                    index += escLength;

                    if (GetEscRenderedCharLength(escapeSequence) > 0)
                    {
                        // currentChunk.AddRenderedEscapeSequence(escapeSequence);
                        renderedEsc = escapeSequence;
                        hasRenderedEsc = true;
                    }
                    else
                    {
                        // If non-rendered escSequence (like text color change),
                        // add normal way.
                        currentChunk.AddEscapeSequence(escapeSequence);
                        continue;
                    }
                }

                if (!hasRenderedEsc && WhiteSpaceChar().IsMatch(currentChar))
                {
                    if (currentChunk.textType == TextChunk.Type.Text)
                    {
                        currentChunk.BuildVal();
                        chunks.Add(currentChunk);

                        currentChunk = new();
                    }

                    currentChunk.textType = TextChunk.Type.Whitespace;
                    currentChunk.AddChar(currentChar);
                }
                else
                {
                    if (currentChunk.textType == TextChunk.Type.Whitespace)
                    {
                        currentChunk.BuildVal();
                        chunks.Add(currentChunk);

                        currentChunk = new();
                    }

                    currentChunk.textType = TextChunk.Type.Text;
                    if (hasRenderedEsc)
                        currentChunk.AddRenderedEscapeSequence(renderedEsc);
                    else
                        currentChunk.AddChar(currentChar);
                }

                if (!hasRenderedEsc)
                    index += 1;
            }

            if (currentChunk.textType != TextChunk.Type.Unknown)
            {
                // Add the leftover chunk which was not added when context
                // swapped to a different type. The only time we would not add
                // this is when we did no work (the string was empty).
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
                                    else if (secondVal == "aucun")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 2);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 3, 8);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "qu'aucun";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal == "aucune")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 2);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 3, 9);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "qu'aucune";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal.StartsWith("{que-transform}"))
                                    {
                                        TextChunk nounChunk = chunks[i + 2];
                                        nounChunk.RemoveRange(0, "{que-transform}".Length);

                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 2);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 3, 3);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "qu'";
                                        newChunk.AppendChunk(nounChunk);
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                }
                                break;
                            }
                            case "de":
                            {
                                if (chunks[i + 1].textType == TextChunk.Type.Whitespace)
                                {
                                    string secondVal = chunks[i + 2].val;
                                    if (secondVal == "un")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 1);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 2, 2);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 2, 4);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "d'un";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal == "une")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 1);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 2, 2);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 2, 5);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "d'une";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal == "le")
                                    {
                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 0);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 2, 2);
                                        TransformEscSeqList(chunks[i + 2], newChunk, 2, 2);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "du";
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
                                        newChunk.val = "des";
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                    else if (secondVal.StartsWith("{que-transform}"))
                                    {
                                        TextChunk nounChunk = chunks[i + 2];
                                        nounChunk.RemoveRange(0, "{que-transform}".Length);

                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 1);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 2, 2);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "d'";
                                        newChunk.AppendChunk(nounChunk);
                                        chunks.RemoveRange(i, 3);
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
                            case "le":
                            case "la":
                            {
                                if (chunks[i + 1].textType == TextChunk.Type.Whitespace)
                                {
                                    string secondVal = chunks[i + 2].val;
                                    if (secondVal.StartsWith("{que-transform}"))
                                    {
                                        TextChunk nounChunk = chunks[i + 2];
                                        nounChunk.RemoveRange(0, "{que-transform}".Length);

                                        TextChunk newChunk = new();
                                        TransformEscSeqList(chunk, newChunk, 0, 1);
                                        TransformEscSeqList(chunks[i + 1], newChunk, 2, 2);
                                        newChunk.textType = TextChunk.Type.Text;
                                        newChunk.val = "l'";
                                        newChunk.AppendChunk(nounChunk);
                                        chunks.RemoveRange(i, 3);
                                        chunks.Insert(i, newChunk);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                // We still need this for cases where the que-transform does not
                // get removed (such as à {que-transform}Ordinn Nord).
                if (chunk.textType == TextChunk.Type.Text)
                {
                    if (chunk.val.StartsWith("{que-transform}"))
                        chunk.RemoveRange(0, "{que-transform}".Length);
                }
            }

            if (addLineBreaks)
            {
                int maxLengthVal = IsCultureJa() ? 30 : 36;

                if (maxLength != null)
                    maxLengthVal = (int)maxLength;
                AddLineBreaksToChunks(chunks, maxLengthVal);
            }

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
                    {
                        char character = chunk.val[i];
                        if (character == '\x1A')
                        {
                            if (!chunk.indexToRenderedEsc.TryGetValue(i, out string escSeq))
                                throw new Exception(
                                    $"Failed to get rendered escSeq for '{chunk.val}' at index {i}."
                                );
                            result += escSeq;
                        }
                        else
                            result += chunk.val[i];
                    }
                }
            }

            return result;
        }

        public static string NormalizeForMergingOnSign(string input)
        {
            if (StringUtils.isEmpty(input))
                return input;

            string output = input;
            int numNewLines = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '\x1A')
                {
                    byte escLength = (byte)input[i + 1];
                    i += escLength - 1;
                    continue;
                }

                if (c == '\n')
                    numNewLines += 1;
            }

            int linesPerTextbox = IsCultureJa() ? 3 : 4;

            int numToAdd;
            if (numNewLines == 0)
                numToAdd = linesPerTextbox;
            else
                numToAdd = linesPerTextbox - (numNewLines % linesPerTextbox);

            for (int i = 0; i < numToAdd; i++)
            {
                output += '\n';
            }

            return output;
        }

        public static string CreateAndList(string langCode, List<string> strings)
        {
            if (ListUtils.isEmpty(strings))
                return "";

            switch (langCode)
            {
                case "en":
                    return CreateAndListEn(strings);
                case "fr":
                    return CreateAndListFr(strings);
                default:
                    throw new Exception($"Failed to createAndList for langCode '{langCode}'.");
            }
        }

        private static string CreateAndListEn(List<string> strings)
        {
            StringBuilder sb = new();

            for (int i = 0; i < strings.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(strings[i]);
            }

            // Might theoretically use this more gramattically correct one below
            // somewhere. Right now we only use this for the Agitha sign which
            // wants the above implementation, so using that one for now.

            // for (int i = 0; i < strings.Count; i++)
            // {
            //     if (i > 0)
            //     {
            //         if (i == strings.Count - 1)
            //         {
            //             if (strings.Count >= 3)
            //                 sb.Append(", and ");
            //             else
            //                 sb.Append(" and ");
            //         }
            //         else
            //             sb.Append(", ");
            //     }
            //     sb.Append(strings[i]);
            // }

            return sb.ToString();
        }

        private static string CreateAndListFr(List<string> strings)
        {
            StringBuilder sb = new();

            for (int i = 0; i < strings.Count; i++)
            {
                if (i > 0)
                {
                    if (i == strings.Count - 1)
                        sb.Append(" et ");
                    else
                        sb.Append(", ");
                }
                sb.Append(strings[i]);
            }

            return sb.ToString();
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

        private static void AddLineBreaksToChunks(List<TextChunk> chunks, int maxLength)
        {
            // If any '\n' show up in the chunks, then we need to not add any
            // linebreaks before them no matter what.

            // Start from the back. Iterate until we find a Whitespace one which
            // contains a linebreak. Starting from after the last line break in
            // this one, we go forward in order to determine where to place
            // linebreaks.

            // int firstAllowedIndex = 0;
            // // int breakIndex = 0;
            // for (int i = chunks.Count - 1; i >= 0; i--)
            // {
            //     TextChunk chunk = chunks[i];
            //     if (chunk.textType == TextChunk.Type.Whitespace)
            //     {
            //         int lastBreakIndex = chunk.val.LastIndexOf('\n');
            //         if (lastBreakIndex >= 0)
            //         {
            //             // breakIndex = lastBreakIndex;
            //             firstAllowedIndex = i;
            //             break;
            //         }
            //     }
            // }
            int firstAllowedIndex = 0;
            // Trying with allowing wrap at any point. Needed for French shop
            // text line when it is super long (half milk bottle)

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

                        if (wouldBeLength > maxLength)
                        {
                            // Need to break;
                            chunk.val = "\n" + chunk.val[1..];
                            currChars = chunk.val.Length - 1;
                        }
                    }
                }
            }
        }

        private static int GetTextTypeChunkLength(TextChunk textChunk)
        {
            if (textChunk.textType == TextChunk.Type.Text)
            {
                int length = textChunk.val.Length;

                // Add lengths for any rendered escSequences.
                foreach (KeyValuePair<int, string> pair in textChunk.indexToRenderedEsc)
                {
                    string renderedEscSeq = pair.Value;
                    int renderedLength = GetEscRenderedCharLength(renderedEscSeq);
                    // Subtract 1 in order to offset the \x1A that we keep in the val.
                    length += renderedLength - 1;
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
            public Dictionary<int, string> indexToRenderedEsc = new();

            private readonly StringBuilder builder = new();

            public void AddChar(string character)
            {
                builder.Append(character);
            }

            public void AddRenderedEscapeSequence(string sequence)
            {
                builder.Append('\x1A');
                indexToRenderedEsc[builder.Length - 1] = sequence;
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
                        if (newKey < startIndex)
                            newKey = startIndex;

                        newDict[newKey] = pair.Value;
                    }
                    escapesAtIndexes = newDict;
                }

                if (!ListUtils.isEmpty(indexToRenderedEsc))
                {
                    Dictionary<int, string> newDict = new();
                    foreach (KeyValuePair<int, string> pair in indexToRenderedEsc)
                    {
                        int oldIdx = pair.Key;

                        if (oldIdx >= startIndex + count)
                        {
                            // If index is after the range, decrease the key by
                            // the number of removed chars.
                            newDict[pair.Key - count] = pair.Value;
                        }
                        else if (oldIdx >= startIndex)
                        {
                            // If index pointed to a char which was in the
                            // range, then we just filter it out since the char
                            // is no longer there.
                            continue;
                        }
                        else
                        {
                            // If index is before the range, then do nothing.
                            newDict[pair.Key] = pair.Value;
                        }
                    }
                    indexToRenderedEsc = newDict;
                }
            }

            public void AppendChunk(TextChunk chunk)
            {
                if (chunk == null)
                    throw new Exception("Expected chunk to not be null.");

                int oldValLength = val.Length;

                val += chunk.val;

                foreach (KeyValuePair<int, List<string>> pair in chunk.escapesAtIndexes)
                {
                    int newIndex = pair.Key + oldValLength;
                    if (!escapesAtIndexes.TryGetValue(newIndex, out List<string> list))
                    {
                        list = new();
                        escapesAtIndexes[newIndex] = list;
                    }
                    list.AddRange(pair.Value);
                }

                foreach (KeyValuePair<int, string> pair in chunk.indexToRenderedEsc)
                {
                    int newIndex = pair.Key + oldValLength;
                    indexToRenderedEsc[newIndex] = pair.Value;
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
                    case "it":
                        result += GetSuffixIt(val, ord);
                        break;
                    case "de":
                        result += GetSuffixDe(val, ord);
                        break;
                    case "ja":
                        result += "other";
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

            private static string GetSuffixIt(string val, bool ord)
            {
                string[] s = val.Split('.');

                uint wholeNumVal = StringUtils.isEmpty(s[0]) ? 0 : Convert.ToUInt32(s[0]);

                bool hasDecimals =
                    s.Length > 1 && !StringUtils.isEmpty(s[1]) && Convert.ToUInt32(s[1]) != 0;

                if (ord)
                {
                    if (
                        !hasDecimals
                        && (
                            wholeNumVal == 11
                            || wholeNumVal == 8
                            || wholeNumVal == 80
                            || wholeNumVal == 800
                        )
                    )
                        return "many";
                    return "other";
                }

                if (!hasDecimals)
                {
                    if (wholeNumVal == 1)
                        return "one";
                    else if (wholeNumVal != 0 && ((wholeNumVal % 1000000) == 0))
                        return "many";
                }

                return "other";
            }

            private static string GetSuffixDe(string val, bool ord)
            {
                if (ord)
                    return "other";

                string[] s = val.Split('.');

                uint wholeNumVal = StringUtils.isEmpty(s[0]) ? 0 : Convert.ToUInt32(s[0]);

                bool hasDecimals =
                    s.Length > 1 && !StringUtils.isEmpty(s[1]) && Convert.ToUInt32(s[1]) != 0;

                if (!hasDecimals && wholeNumVal == 1)
                    return "one";

                return "other";
            }

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

        public class Result
        {
            public CultureInfo cultureInfo { get; private set; }
            public string langCode { get; private set; }
            public bool foundValue;
            public string value;
            public Dictionary<string, string> meta = new();
            public Dictionary<string, Dictionary<string, string>> slotMeta = new();

            public Result(CultureInfo cultureInfo, string langCode)
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

                            if (interpolation.TryGetValue(key.Value, out string valToPut))
                            {
                                return valToPut;
                            }
                            // If not in interpolation, return the value back.
                            return match.Value;
                        }
                    );

                return result;
            }

            public string ResolveWithColor(string startColor, string endColor = null)
            {
                if (StringUtils.isEmpty(endColor))
                    endColor = CustomMessages.messageColorWhite;

                if (value.Contains("{cs}"))
                    return Substitute(new() { { "cs", startColor }, { "ce", endColor }, });

                return startColor + Substitute(null) + endColor;
            }

            public bool SlotMetaHasVal(string slot, string key, string val)
            {
                if (
                    StringUtils.isEmpty(slot)
                    || StringUtils.isEmpty(key)
                    || StringUtils.isEmpty(val)
                )
                    throw new Exception("Invalid params for SlotMetaHasVal.");

                if (slotMeta.TryGetValue(slot, out Dictionary<string, string> dictForSlot))
                {
                    if (dictForSlot.TryGetValue(key, out string valForKey))
                    {
                        return valForKey == val;
                    }
                }

                return false;
            }

            public bool MetaHasVal(string key, string val)
            {
                if (StringUtils.isEmpty(key) || StringUtils.isEmpty(val))
                    throw new Exception("Invalid params for MetaHasVal.");

                if (meta.TryGetValue(key, out string valForMeta))
                    return valForMeta == val;
                return false;
            }
        }
    }
}
