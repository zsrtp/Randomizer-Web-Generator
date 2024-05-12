namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class HintInfo
    {
        public string text;
        private List<string> colors = new();
        public string sourceCheck;
        public string targetCheck;
        public string hintedCheck;
        public List<Item> hintedItems { get; private set; } = new();

        public HintInfo(string text)
        {
            ParseText(text);
            // can automatically format and pull colors from text

            // The thing that creates it can also specify the things
        }

        private void ParseText(string input)
        {
            this.text = input;
        }

        public Dictionary<string, object> GetSpoilerDict()
        {
            Dictionary<string, object> ret = new();

            ret["text"] = text;
            if (!ListUtils.isEmpty(colors))
                ret["colors"] = colors;
            if (!StringUtils.isEmpty(sourceCheck))
                ret["sourceCheck"] = sourceCheck;
            if (!StringUtils.isEmpty(targetCheck))
                ret["targetCheck"] = targetCheck;
            if (!StringUtils.isEmpty(hintedCheck))
                ret["hintedCheck"] = hintedCheck;
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
