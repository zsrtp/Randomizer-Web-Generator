using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TPRandomizer.Util;

namespace TPRandomizer
{
    public class SeedGenResults
    {
        public string settings { get; }
        public string playthroughName { get; }
        public Dictionary<int, byte> itemPlacements { get; }
        public byte requiredDungeons { get; }

        public SeedGenResults(JObject inputJsonContents)
        {
            JObject input = (JObject)inputJsonContents["input"];
            settings = (string)input["settings"];

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
    }
}
