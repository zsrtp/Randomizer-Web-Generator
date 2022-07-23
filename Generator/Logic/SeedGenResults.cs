using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TPRandomizer.Util;

namespace TPRandomizer
{
    public class SeedGenResults
    {
        // input
        public string settingsString { get; set; }
        public string seed { get; set; }
        public bool isRaceSeed { get; set; }

        // output
        public string playthroughName { get; set; }
        public Dictionary<int, byte> itemPlacements { get; }
        public byte requiredDungeons { get; set; }

        public SeedGenResults(JObject inputJsonContents)
        {
            // Can read `version` as well if format ever changes and we need to
            // support multiple formats.

            JObject input = (JObject)inputJsonContents["input"];
            settingsString = (string)input["settings"];
            seed = (string)input["seed"];
            isRaceSeed = (int)input["race"] == 1;

            JObject output = (JObject)inputJsonContents["output"];
            this.playthroughName = (string)output["name"];
            this.itemPlacements = DecodeItemPlacements((string)output["itemPlacement"]);
            this.requiredDungeons = (byte)output["reqDungeons"];
        }

        public static string EncodeItemPlacements(SortedDictionary<int, byte> checkNumIdToItemId)
        {
            UInt16 version = 0;
            string result = SettingsEncoder.EncodeAsVlq16(version);

            if (checkNumIdToItemId.Count() == 0)
            {
                result += "0";
                return SettingsEncoder.EncodeAs6BitString(result);
            }

            result += "1";

            int smallest = checkNumIdToItemId.First().Key;
            int largest = checkNumIdToItemId.Last().Key;

            result += SettingsEncoder.EncodeNumAsBits(smallest, 9);
            result += SettingsEncoder.EncodeNumAsBits(largest, 9);

            string itemBits = "";

            for (int i = smallest; i <= largest; i++)
            {
                if (checkNumIdToItemId.ContainsKey(i))
                {
                    result += "1";
                    itemBits += SettingsEncoder.EncodeNumAsBits(checkNumIdToItemId[i], 8);
                }
                else
                {
                    result += "0";
                }
            }

            result += itemBits;

            return SettingsEncoder.EncodeAs6BitString(result);
        }

        private Dictionary<int, byte> DecodeItemPlacements(string sixCharString)
        {
            BitsProcessor processor = new BitsProcessor(
                SettingsEncoder.DecodeToBitString(sixCharString)
            );

            Dictionary<int, byte> checkNumIdToItemId = new();

            UInt16 version = processor.NextVlq16();

            if (!processor.NextBool())
            {
                return checkNumIdToItemId;
            }

            int smallest = processor.NextInt(9);
            int largest = processor.NextInt(9);

            List<int> checkIdsWithItemIds = new();

            for (int i = smallest; i <= largest; i++)
            {
                if (processor.NextBool())
                {
                    checkIdsWithItemIds.Add(i);
                }
            }

            for (int i = 0; i < checkIdsWithItemIds.Count; i++)
            {
                byte itemId = processor.NextByte();
                checkNumIdToItemId[smallest + i] = itemId;
            }

            return checkNumIdToItemId;
        }

        public class Builder
        {
            public string settingsString { get; set; }
            public string seed { get; set; }
            public bool isRaceSeed { get; set; }
            public string seedHashString { get; set; }
            public string playthroughName { get; set; }
            public byte requiredDungeons { get; set; }
            private string itemPlacement;

            public Builder() { }

            public void SetItemPlacements(SortedDictionary<int, byte> checkNumIdToItemId)
            {
                itemPlacement = EncodeItemPlacements(checkNumIdToItemId);
            }

            override public string ToString()
            {
                Dictionary<string, object> inputJsonRoot = new();
                // Need to update format for any changes.
                // For minor additions, can bump to 1.1, etc.
                // For major format changes, can change to 2, etc.
                inputJsonRoot.Add("version", "1");

                Dictionary<string, object> metaObj = new();
                inputJsonRoot.Add("meta", metaObj);
                metaObj.Add("ts", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                metaObj.Add("imgVer", Global.imageVersion);
                metaObj.Add("gitCmt", Global.gitCommit);

                Dictionary<string, object> inputObj = new();
                inputJsonRoot.Add("input", inputObj);
                inputObj.Add("settings", settingsString);
                inputObj.Add("seed", seed);
                inputObj.Add("race", isRaceSeed ? 1 : 0);

                Dictionary<string, object> outputObj = new();
                inputJsonRoot.Add("output", outputObj);
                outputObj.Add("seedHash", seedHashString);
                outputObj.Add("name", playthroughName);
                outputObj.Add("itemPlacement", itemPlacement);
                outputObj.Add("reqDungeons", requiredDungeons);

                return JsonConvert.SerializeObject(inputJsonRoot);
            }
        }
    }
}
