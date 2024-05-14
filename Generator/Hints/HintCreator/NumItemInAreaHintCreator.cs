namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class NumItemInAreaHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.NumItemInArea;

        private Item item;
        private AreaId areaType;
        private List<string> validAreas;
        private string order;

        private NumItemInAreaHintCreator() { }

        new public static NumItemInAreaHintCreator fromJObject(JObject obj)
        {
            NumItemInAreaHintCreator inst = new NumItemInAreaHintCreator();
            if (!obj.ContainsKey("options"))
                throw new Exception("Missing 'options' for NumItemInAreaHintCreator.");

            JObject options = (JObject)obj["options"];

            string itemStr = HintSettingUtils.getString(options, "item");
            inst.item = HintSettingUtils.parseItem(itemStr);

            string areaTypeStr = HintSettingUtils.getString(options, "areaType").ToLower();
            Func<string, bool> isValidAreaName;
            switch (areaTypeStr)
            {
                case "province":
                {
                    inst.areaType = AreaId.Province(Province.Invalid);
                    isValidAreaName = (name) => ProvinceUtils.StringToId(name) != Province.Invalid;
                    break;
                }
                case "zone":
                {
                    inst.areaType = AreaId.ZoneStr("Invalid");
                    isValidAreaName = (name) => ZoneUtils.StringToId(name) != Zone.Invalid;
                    break;
                }
                // Not supporting category right now since it does not have a
                // use case at the moment. Also categories will need to work
                // with Entrance Rando, so better to not do too much with them
                // for now.
                default:
                    throw new Exception(
                        $"NumItemInAreaHintCreator: '{areaTypeStr}' is not a valid areaType."
                    );
            }

            // If validAreas is defined, make sure each value in list is valid.
            List<string> validAreas = HintSettingUtils.getOptionalStringList(
                options,
                "validAreas",
                new()
            );
            if (!ListUtils.isEmpty(validAreas))
            {
                foreach (string areaName in validAreas)
                {
                    if (!isValidAreaName(areaName))
                        throw new Exception(
                            $"For areaType '{areaTypeStr}', '{areaName}' is not a valid areaName."
                        );
                }
            }
            inst.validAreas = validAreas;

            string orderStr = HintSettingUtils.getOptionalString(options, "order", null);
            if (orderStr != null)
            {
                orderStr = orderStr.ToLower();
                if (orderStr != "asc" && orderStr != "desc")
                    throw new Exception("Option 'order' must be either 'asc' or 'desc'.");
            }
            inst.order = orderStr;

            // Will need a way to say "this check belongs to this area".

            // zones already map to list of checkNames.
            // categories also map to a list of checkNames.
            // provinces map to zones which map to checkNames.
            // If no areaList is defined, iterate over all checkNames.

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            if (!genData.itemToChecksList.ContainsKey(item))
                return null;

            Func<string, string> checkToAreaId;
            Func<string, HashSet<string>> getAllNamesInArea;
            Func<string, AreaId> areaIdStrToAreaId;

            switch (areaType.type)
            {
                case AreaId.AreaType.Province:
                {
                    checkToAreaId = (checkName) =>
                        ProvinceUtils.IdToString(HintUtils.checkNameToHintProvince(checkName));
                    getAllNamesInArea = (str) => ProvinceUtils.GetProvinceNames();
                    areaIdStrToAreaId = (str) => AreaId.Province(ProvinceUtils.StringToId(str));
                    break;
                }
                case AreaId.AreaType.Category:
                {
                    checkToAreaId = (checkName) => HintUtils.checkNameToHintZone(checkName);
                    getAllNamesInArea = (str) =>
                    {
                        HashSet<string> result = new();
                        Dictionary<string, string[]> dict = HintUtils.getHintZoneToChecksMap();
                        foreach (KeyValuePair<string, string[]> pair in dict)
                        {
                            result.Add(pair.Key);
                        }
                        return result;
                    };
                    areaIdStrToAreaId = (str) => AreaId.ZoneStr(str);
                    break;
                }
                default:
                    throw new Exception(
                        $"Failed to generate numItemInArea hint for areaType '{areaType.type}'."
                    );
            }

            HashSet<string> validAreaIds;
            if (!ListUtils.isEmpty(validAreas))
                validAreaIds = new(validAreas);
            else
                validAreaIds = getAllNamesInArea(null);

            if (validAreaIds.Count < 1)
                return null;

            Dictionary<string, List<string>> areaToCheckNames = new();
            foreach (string areaId in validAreaIds)
            {
                areaToCheckNames[areaId] = new();
            }

            foreach (string checkName in genData.itemToChecksList[item])
            {
                string areaId = checkToAreaId(checkName);
                if (validAreaIds.Contains(areaId))
                    areaToCheckNames[areaId].Add(checkName);
            }

            List<string> areaIdsToPickFrom;

            if (StringUtils.isEmpty(order))
            {
                areaIdsToPickFrom = new(validAreaIds);
                HintUtils.ShuffleListInPlace(genData.rnd, areaIdsToPickFrom);
            }
            else
            {
                Dictionary<int, HashSet<string>> countToAreaIds = new();
                foreach (KeyValuePair<string, List<string>> pair in areaToCheckNames)
                {
                    int count = pair.Value.Count;
                    if (!countToAreaIds.ContainsKey(count))
                        countToAreaIds[count] = new();
                    HashSet<string> areaIds = countToAreaIds[count];
                    areaIds.Add(pair.Key);
                }

                List<int> counts = countToAreaIds.Keys.ToList();
                if (order == "asc")
                    counts.Sort((a, b) => a.CompareTo(b));
                else if (order == "desc")
                    counts.Sort((a, b) => b.CompareTo(a));

                areaIdsToPickFrom = new();
                foreach (int count in counts)
                {
                    List<string> partialAreaIdsToPickFrom = new(countToAreaIds[count]);
                    HintUtils.ShuffleListInPlace(genData.rnd, partialAreaIdsToPickFrom);
                    areaIdsToPickFrom.AddRange(partialAreaIdsToPickFrom);
                }
            }

            List<Hint> hintResults = new();

            for (int i = 0; i < numHints && areaIdsToPickFrom.Count > 0; i++)
            {
                string areaIdStr = areaIdsToPickFrom[0];
                areaIdsToPickFrom.RemoveAt(0);

                List<string> checkNames = areaToCheckNames[areaIdStr];
                int count = checkNames.Count;
                AreaId areaId = areaIdStrToAreaId(areaIdStr);
                // Create hint with this areaId.
                hintResults.Add(new NumItemInAreaHint(count, item, areaId, checkNames));
            }

            return hintResults;
        }
    }
}
