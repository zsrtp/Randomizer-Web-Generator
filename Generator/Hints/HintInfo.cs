namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TPRandomizer.Util;

    public class HintInfo
    {
        public string text;
        private List<string> colors = new();
        public string sourceCheck;
        public Item? sourceItem;
        public string targetCheck;
        public Item? targetItem;
        public string hintedCheck;
        public List<string> hintedChecks = new();
        public List<Item> hintedItems { get; private set; } = new();

        public HintInfo(string text)
        {
            ParseText(text);
            // can automatically format and pull colors from text

            // The thing that creates it can also specify the things
        }

        private void ParseText(string input)
        {
            bool hasOpenBracket = false;
            int index = 0;
            StringBuilder sb = new();
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
                    index += escLength;

                    string renderedStr = Res.GetEscRenderedChar(escapeSequence);
                    if (!StringUtils.isEmpty(renderedStr))
                    {
                        sb.Append(renderedStr);
                    }
                    else
                    {
                        string colorText = Res.GetColorFromEscSequence(escapeSequence);
                        if (!StringUtils.isEmpty(colorText))
                        {
                            if (colorText.ToLowerInvariant() == "white")
                            {
                                sb.Append('}');
                                hasOpenBracket = false;
                            }
                            else
                            {
                                colors.Add(colorText);
                                sb.Append('{');
                                hasOpenBracket = true;
                            }
                        }
                    }
                }
                else
                {
                    if (currentChar == "\n")
                        sb.Append(' ');
                    else
                        sb.Append(currentChar);

                    index += 1;
                }
            }

            // For some text, we do not set the text back to white at the end
            // since it would use up bytes for now reason, so we add a bracket
            // for the missing white color at the end if we have an open
            // bracket.
            if (hasOpenBracket)
                sb.Append('}');

            this.text = sb.ToString();
        }

        public Dictionary<string, object> GetSpoilerDict()
        {
            Dictionary<string, object> ret = new();

            ret["text"] = text;
            if (!ListUtils.isEmpty(colors))
                ret["colors"] = colors;
            if (!StringUtils.isEmpty(sourceCheck))
                ret["sourceCheck"] = sourceCheck;
            if (sourceItem != null)
                ret["sourceItem"] = sourceItem.ToString();
            if (!StringUtils.isEmpty(targetCheck))
                ret["targetCheck"] = targetCheck;
            if (targetItem != null)
                ret["targetItem"] = targetItem.ToString();
            if (!StringUtils.isEmpty(hintedCheck))
                ret["hintedCheck"] = hintedCheck;
            if (!ListUtils.isEmpty(hintedChecks))
                ret["hintedChecks"] = hintedChecks;
            if (!ListUtils.isEmpty(hintedItems))
            {
                List<string> hintedItemsText = new();

                foreach (Item item in hintedItems)
                {
                    hintedItemsText.Add(item.ToString());
                }

                ret["hintedItems"] = hintedItemsText;
            }

            return ret;
        }
    }
}
