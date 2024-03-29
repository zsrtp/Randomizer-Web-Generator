namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;
    using System.Linq;

    public class LocationHintCreator : HintCreator
    {
        protected enum NamedOrder
        {
            Basic,
            Random,
        }

        protected HashSet<string> validChecks = new();
        protected HashSet<string> invalidChecks = new();
        protected HashSet<Item> validItems = new();
        protected HashSet<Item> invalidItems = new();
        protected HashSet<LocationHint.Status> validStatuses;
        protected bool vague = false;
        protected bool markAsSometimes = false;
        protected List<string> namedChecks = null;
        protected NamedOrder namedOrder = NamedOrder.Basic;

        // Probability of less than 0 means to ignore it.
        protected double namedProbability = -1;

        protected LocationHintCreator()
        {
            this.type = HintCreatorType.Location;
        }

        new public static LocationHintCreator fromJObject(JObject obj)
        {
            LocationHintCreator inst = new LocationHintCreator();
            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];
                List<string> validChecks = HintSettingUtils.getOptionalStringList(
                    options,
                    "validChecks",
                    new()
                );
                foreach (string name in validChecks)
                {
                    if (!HintSettingUtils.IsValidCheckResolutionFormat(name))
                        throw new Exception(
                            $"'{name}' is not a valid format to resolve to checks."
                        );
                }
                inst.validChecks = new(validChecks);

                inst.validItems = HintSettingUtils.getOptionalItemSet(
                    options,
                    "validItems",
                    inst.validItems
                );

                List<string> invalidChecks = HintSettingUtils.getOptionalStringList(
                    options,
                    "invalidChecks",
                    new()
                );
                foreach (string name in invalidChecks)
                {
                    if (!HintSettingUtils.IsValidCheckResolutionFormat(name))
                        throw new Exception(
                            $"'{name}' is not a valid format to resolve to checks."
                        );
                }
                inst.invalidChecks = new(invalidChecks);

                inst.invalidItems = HintSettingUtils.getOptionalItemSet(
                    options,
                    "invalidItems",
                    inst.invalidItems
                );

                List<string> validStatusesList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validStatuses",
                    null
                );
                if (validStatusesList != null)
                {
                    inst.validStatuses = new();
                    foreach (string str in validStatusesList)
                    {
                        LocationHint.Status status;
                        bool success = Enum.TryParse(str, true, out status);
                        if (success)
                            inst.validStatuses.Add(status);
                        else
                            throw new Exception(
                                $"Failed to parse '{str}' to CheckContentsHint.Status enum."
                            );
                    }
                }

                inst.vague = HintSettingUtils.getOptionalBool(options, "vague", inst.vague);
                inst.markAsSometimes = HintSettingUtils.getOptionalBool(
                    options,
                    "markAsSometimes",
                    inst.markAsSometimes
                );

                List<string> namedChecksStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "namedChecks",
                    null
                );
                if (namedChecksStrList != null)
                {
                    inst.namedChecks = new();
                    foreach (string namedCheckStr in namedChecksStrList)
                    {
                        if (!CheckIdClass.IsValidCheckName(namedCheckStr))
                            throw new Exception($"'{namedCheckStr}' is not a valid checkName.");
                        inst.namedChecks.Add(namedCheckStr);
                    }
                }

                string namedOrderStr = HintSettingUtils.getOptionalString(
                    options,
                    "namedOrder",
                    null
                );
                if (!StringUtils.isEmpty(namedOrderStr))
                {
                    if (Enum.TryParse(namedOrderStr, true, out NamedOrder namedOrder))
                        inst.namedOrder = namedOrder;
                    else
                        throw new Exception(
                            $"Failed to parse '{namedOrderStr}' to NamedOrder enum."
                        );
                }

                inst.namedProbability = HintSettingUtils.getOptionalDouble(
                    options,
                    "namedProbability",
                    inst.namedProbability
                );
                if (inst.namedProbability > 1)
                    throw new Exception(
                        $"'namedProbability' must not be greater than 1, but was '{inst.namedProbability}'."
                    );
            }

            // if (inst.validChecks.Count == 0 && inst.validItems.Count == 0 && inst.namedOrder)
            //     throw new Exception("Must have at least one validCheck or validItem.");

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            bool isNamedProcessing = namedChecks != null;
            if (isNamedProcessing && namedProbability == 0)
                return null;

            List<string> possibleCheckNames = GetPossibleCheckNames(genData);
            if (ListUtils.isEmpty(possibleCheckNames))
                return null;

            if (!isNamedProcessing || namedOrder == NamedOrder.Random)
                HintUtils.ShuffleListInPlace(genData.rnd, possibleCheckNames);

            // Pick hints
            List<Hint> hints = new();

            bool useProbability = isNamedProcessing && namedProbability > 0;
            HashSet<string> hintedChecks = new();

            for (int i = 0; hints.Count < numHints && i < possibleCheckNames.Count; i++)
            {
                string checkName = possibleCheckNames[i];

                // Make sure we do not hint the same check multiple times.
                if (hintedChecks.Contains(checkName))
                    continue;

                if (useProbability)
                {
                    // If generated number is over probability, then we fail and
                    // should skip current index.
                    if (genData.rnd.NextDouble() >= namedProbability)
                        continue;
                }

                Hint hint = LocationHint.Create(genData, checkName, vague, markAsSometimes);
                hints.Add(hint);
                hintedChecks.Add(checkName);

                // Update 'hinted'.
                genData.hinted.alreadyCheckContentsHinted.Add(checkName);
            }

            return hints;
        }

        private List<string> GetPossibleCheckNames(HintGenData genData)
        {
            List<string> possibleCheckNames = new();

            HashSet<string> validCheckNames = new();
            foreach (string name in validChecks)
            {
                HashSet<string> res = genData.ResolveToChecks(name);
                validCheckNames.UnionWith(res);
            }

            HashSet<string> invalidCheckNames = new();
            foreach (string name in invalidChecks)
            {
                HashSet<string> res = genData.ResolveToChecks(name);
                invalidCheckNames.UnionWith(res);
            }

            if (namedChecks != null)
            {
                // Iterate over namedChecks
                foreach (string checkName in namedChecks)
                {
                    if (
                        CheckIsPossibleToHint(
                            genData,
                            validCheckNames,
                            invalidCheckNames,
                            checkName
                        )
                    )
                    {
                        possibleCheckNames.Add(checkName);
                    }
                }
            }
            else
            {
                // Iterate over all checks.
                foreach (
                    KeyValuePair<string, Check> checkList in Randomizer.Checks.CheckDict.ToList()
                )
                {
                    string checkName = checkList.Value.checkName;
                    if (
                        CheckIsPossibleToHint(
                            genData,
                            validCheckNames,
                            invalidCheckNames,
                            checkName
                        )
                    )
                    {
                        possibleCheckNames.Add(checkName);
                    }
                }
            }

            return possibleCheckNames;
        }

        private bool CheckIsPossibleToHint(
            HintGenData genData,
            HashSet<string> validCheckNames,
            HashSet<string> invalidCheckNames,
            string checkName
        )
        {
            Item item = HintUtils.getCheckContents(checkName);
            LocationHint.Status status = LocationHint.CalcStatus(genData, checkName);

            return (
                genData.checkCanBeLocationHinted(checkName)
                && (validCheckNames.Count == 0 || validCheckNames.Contains(checkName))
                && !invalidCheckNames.Contains(checkName)
                && (validItems.Count == 0 || validItems.Contains(item))
                && !invalidItems.Contains(item)
                && (validStatuses == null || validStatuses.Contains(status))
            );
        }
    }
}
