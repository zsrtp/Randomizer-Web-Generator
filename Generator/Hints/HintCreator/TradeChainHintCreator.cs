namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class TradeChainHintCreator : HintCreator
    {
        public bool vagueSourceItem = false;
        public bool includeArea = true;
        public CheckStatusDisplay checkStatusDisplay = CheckStatusDisplay.None;
        public TradeChainHint.AreaType? areaType = null;

        // TODO: can add a way of displaying the reward as vague like "is on the
        // WotH", "something good", "nothing", unhelpful. Can worry about later
        // since would delay the feature and be fairly involved.
        private HashSet<CheckStatus> validCheckStatuses;

        // When `requiredChainItems` is not null, can only hint chains which
        // contain at least one item in this set (including final reward).
        private HashSet<Item> requiredChainItems = null;

        private TradeChainHintCreator()
        {
            this.type = HintCreatorType.TradeChain;
        }

        new public static TradeChainHintCreator fromJObject(JObject obj)
        {
            TradeChainHintCreator inst = new TradeChainHintCreator();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                inst.vagueSourceItem = HintSettingUtils.getOptionalBool(
                    options,
                    "vagueSourceItem",
                    inst.vagueSourceItem
                );

                inst.includeArea = HintSettingUtils.getOptionalBool(
                    options,
                    "includeArea",
                    inst.includeArea
                );

                inst.checkStatusDisplay = HintSettingUtils.getOptionalCheckStatusDisplay(
                    options,
                    "checkStatusDisplay",
                    inst.checkStatusDisplay
                );

                string areaTypeStr = HintSettingUtils.getOptionalString(options, "areaType", null);
                if (!StringUtils.isEmpty(areaTypeStr))
                {
                    TradeChainHint.AreaType areaType;
                    bool success = Enum.TryParse(areaTypeStr, true, out areaType);
                    if (success)
                        inst.areaType = areaType;
                    else
                        throw new Exception(
                            $"Failed to parse areaType '{areaTypeStr}' to AreaType enum."
                        );
                }

                List<string> validCheckStatusesStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validCheckStatuses",
                    new()
                );
                inst.validCheckStatuses = new();
                foreach (string statusStr in validCheckStatusesStrList)
                {
                    if (Enum.TryParse(statusStr, true, out CheckStatus checkStatus))
                        inst.validCheckStatuses.Add(checkStatus);
                    else
                        throw new Exception(
                            $"Failed to parse checkStatus '{statusStr}' to CheckStatus enum."
                        );
                }

                List<string> requiredChainItemsStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "requiredChainItems",
                    null
                );
                if (requiredChainItemsStrList != null)
                {
                    // Allow for resolving to an empty Set to mean unhintable
                    // rather than ignoring it.
                    inst.requiredChainItems = new();
                    foreach (string itemStr in requiredChainItemsStrList)
                    {
                        if (itemStr.StartsWith("alias:"))
                        {
                            string alias = itemStr.Substring(6);
                            HashSet<Item> resolved = ResolveItemsAlias(alias);
                            inst.requiredChainItems.UnionWith(resolved);
                        }
                        else
                        {
                            Item item = HintSettingUtils.parseItem(itemStr);
                            inst.requiredChainItems.Add(item);
                        }
                    }
                }
            }

            if (ListUtils.isEmpty(inst.validCheckStatuses))
            {
                inst.validCheckStatuses = new() { CheckStatus.Good };
            }

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            if (numHints < 1 || (requiredChainItems != null && requiredChainItems.Count < 1))
                return null;

            // Iterate over all validItems (defaults to all; need to add to options)
            // Also need to add validStatuses (bad,good,required). Default to any

            // Can calc all trade chains up front when initializing genData
            // since also need to be used by tradeItemGroup hints.

            // Iterate over each trade item.
            // For each check that rewards that item:
            // If check is on a trade item reward check (such as Ralis), we know it is not the start of a chain, so skip over it.
            // If check is not a trade item reward check (such as most things), we know it IS the start of a chain.
            // For these:
            //

            // IMPORTANT: need to handle recursive chains.
            // For example, MaleMantis => FemaleButterFly => FemalePhasmid => MaleMantis.
            // MaleMantis reward is one time and that item was already in the chain, so we stop there.
            // The 2nd MaleMantis does not unlock any checks, meaning it is bad.

            // Imagine the above, but there is a 3rd MaleMantis as well. This
            // one is doomed to be part of this chain since the MaleMantis
            // reward is always the same thing, so it is not possible for it to
            // lead to anything.

            // You could potentially see this with: Sketch => Sketch as well.

            // Only ones we can hint for the chain hints are chain starters.

            // Note that we need to pay attention to if the chain starter is required for the WotH hints.
            // If you had MaleMantis => Clawshot, but there was a Clawshot in sphere0 KakVil
            // and sphere0 Lake Hylia, then neither of those is hard required even though the end of the
            // chain is required.

            // It is valid to hint a chain starter even if it is also not a
            // chain starter since a chain starter is something that you can be
            // on the lookout for.

            // For hinting a chain to the HC BK for example, we get a list of
            // every checkName which is the start of a chain that ends in HC BK,
            // and pick one randomly. (Circular chains are not added to the list
            // of valid chains since they do not have a valid end)

            // For the group hints, given a groupEnum we can try to find the valid statuses for it.
            // For example, if something is required, then it can also be good.
            // Something that is bad cannot be required though.

            // So if given female bugs as the group, we iterate through all female bugs which actually
            // appear in the item pool.
            // For each

            // MaleMantis (Ordon) => FemLadyBug (MaleMantisReward) => MalePillBug => Clawshot
            // FemLadyBug (Eldin Field) => MalePillBug => Clawshot

            // So what do we want? This:
            // Dictionary<checkName, Item> chainStarterToReward.

            // For groups, we also want this structure:
            // Dictionary<tradeItem, chainResultCheckName>;

            // ^ We can iterate through this for each trade item to mark them as preventBarren or not.

            // This can be used to quickly look up the following:

            // validStatuses for femBugs?
            // Iterate through each femBug.
            // Can immediately get the check and determine if it is good or bad or required.
            // Then we can compile the results of each femBug.
            // If there is a good, then bad is invalid.
            // If there is a bad, that doesn't mean anything unless they are all bad.
            // If there are no good, then bad is valid.
            // If there is a good, then good is valid.
            // If there is a required, then required is valid. (Female bugs are on the way of the hero.)
            // Unhelpful is always valid (don't need to include in calc).

            // Can just keep track of counts for good and required.
            // If good > 0 => good is valid
            // If required > 0 => required is valid
            // If both are 0 and bad > 0 => bad is valid

            // When hinting required or good, this can always be done, and it
            // points at the destination check. This works with TradeChain hints
            // because only work if the source and target checks are both not
            // directedToward, so if the TradeGroup directsToward a the
            // destination, then they cannot both point to the same thing.

            // If Agitha hints are off, then hinting bad is valid as long as at
            // least one female bug is a chainStarter which is not
            // alreadyKnownBarren. No point in hinting that female bugs are dead
            // when they are not chain starters since (unless Sketch gets
            // involved), you basically save no time by knowing this since you
            // can immediately trade the bug you receive back in.

            // If Agitha hints are on, then hinting bad is valid only if we can
            // find a good destination item for a chainStarter that does not
            // belong to the group we are hinting as bad. For example, we can
            // only hint female bugs as bad if there is a maleChainStarter bug
            // which leads to something good.

            // For now, all TradeGroups are bugs. Need to make sure we are not
            // factoring Sketch into stuff. If Agitha hints are on and Agitha
            // has nothing, it is not valid to hint female bugs as dead even if
            // Sketch leads to something good.



            // For tradeChains, we may want to limit like this:
            // - only hint with this reward status: ['required', 'good', 'bad'] (can include multiple)
            // (defaults to 'good'). Need to store this on the hint.

            // "They say that the {Male Mantis} found at {Kakariko Gorge} leads to the
            // {Hyrule Castle Big Key}."

            // "They say that a {bug} found at {City in the Sky} leads to
            // {something good}."

            // "They say that {trading an item} found at {Lake Lantern Cave}
            // leads to {Clawshot}."

            // Iterate over all tradeChain starts which are not tradeItemReward checks

            List<
                KeyValuePair<string, CheckStatus>
            > validChainStarters = new();
            Dictionary<string, HashSet<string>> endCheckToStartChecks = new();

            foreach (KeyValuePair<string, Item> pair in genData.tradeChainStartToReward)
            {
                string startCheckName = pair.Key;
                Item startItem = HintUtils.getCheckContents(startCheckName);

                // Never hint a chainStarterCheck that rewards a tradeItem the
                // player already starts with.
                if (
                    genData.sSettings.startingItems.Contains(startItem)
                    || !IsChainStarterCheckHintable(genData, startCheckName)
                )
                    continue;

                if (!ListUtils.isEmpty(requiredChainItems))
                {
                    if (!HintUtils.TradeChainContainsItem(startCheckName, requiredChainItems))
                    {
                        // Make sure chain contains at least one item in
                        // requiredChainItems.
                        continue;
                    }
                }

                string endCheckName = genData.tradeItemToChainEndCheck[startItem];
                if (!IsChainEndCheckHintable(genData, endCheckName))
                    continue;

                if (IsRequiredValidStatus() && genData.CheckIsRequired(endCheckName))
                {
                    validChainStarters.Add(new(startCheckName, CheckStatus.Required));
                }
                else if (IsGoodValidStatus() && genData.CheckIsGood(endCheckName, true))
                {
                    validChainStarters.Add(new(startCheckName, CheckStatus.Good));
                }
                else if (IsBadValidStatus() && !genData.CheckIsGood(endCheckName, true))
                {
                    validChainStarters.Add(new(startCheckName, CheckStatus.Bad));
                }
                else
                {
                    // Continue if did not find validChainStarter
                    continue;
                }

                if (!endCheckToStartChecks.ContainsKey(endCheckName))
                    endCheckToStartChecks[endCheckName] = new();
                endCheckToStartChecks[endCheckName].Add(startCheckName);
            }

            List<Hint> result = new();

            for (int i = 0; i < numHints; i++)
            {
                if (validChainStarters.Count < 1)
                    break;

                int randomIndex = genData.rnd.Next(validChainStarters.Count);
                KeyValuePair<string, CheckStatus> selected = validChainStarters[randomIndex];

                string startCheckName = selected.Key;
                CheckStatus checkStatus = selected.Value;

                Item starterItem = HintUtils.getCheckContents(startCheckName);
                string endCheckName = genData.tradeItemToChainEndCheck[starterItem];

                TradeChainHint.AreaType finalAreaType;
                if (areaType != null)
                    finalAreaType = (TradeChainHint.AreaType)areaType;
                else
                {
                    Item endItem = HintUtils.getCheckContents(endCheckName);
                    AreaId.AreaType areaIdType = genData.GetRecommendedAreaIdType(
                        startCheckName,
                        endItem
                    );
                    if (areaIdType == AreaId.AreaType.Province)
                        finalAreaType = TradeChainHint.AreaType.Province;
                    else
                        finalAreaType = TradeChainHint.AreaType.Zone;
                }

                TradeChainHint hint = TradeChainHint.Create(
                    genData,
                    startCheckName,
                    vagueSourceItem,
                    includeArea,
                    finalAreaType,
                    checkStatus,
                    checkStatusDisplay
                );
                result.Add(hint);

                // Update hinted. Mark both the start and the end as
                // directedToward so that things are easy to understand. You
                // could technically skip marking the endCheckName as
                // directedToward, but the behavior would not be as predictable
                // and you might have hint overlap / unintentionally unhelpful
                // hints if someone provided a wonky hintDistribution as input.
                genData.hinted.alreadyCheckDirectedToward.Add(startCheckName);
                genData.hinted.alreadyCheckDirectedToward.Add(endCheckName);

                // Filter out all chainStarters which end on the endCheckName of
                // the selected startCheckName.
                HashSet<string> startChecksForEndCheck = endCheckToStartChecks[endCheckName];

                validChainStarters = validChainStarters
                    .Where(pair => !startChecksForEndCheck.Contains(pair.Key))
                    .ToList();
            }

            return result;
        }

        private bool IsRequiredValidStatus()
        {
            return validCheckStatuses.Contains(CheckStatus.Required);
        }

        private bool IsGoodValidStatus()
        {
            return validCheckStatuses.Contains(CheckStatus.Good);
        }

        private bool IsBadValidStatus()
        {
            return validCheckStatuses.Contains(CheckStatus.Bad);
        }

        private bool IsChainStarterCheckHintable(HintGenData genData, string checkName)
        {
            HintedThings3 hinted = genData.hinted;

            return !genData.CheckShouldBeIgnored(checkName)
                && !hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !hinted.alreadyCheckDirectedToward.Contains(checkName)
                && !hinted.alreadyCheckKnownBarren.Contains(checkName);
        }

        private bool IsChainEndCheckHintable(HintGenData genData, string checkName)
        {
            HintedThings3 hinted = genData.hinted;

            // Skip over ignored since the endCheck might be an Agitha reward
            // which would be ignored normally.
            return !hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !hinted.alreadyCheckDirectedToward.Contains(checkName)
                && !hinted.alreadyCheckKnownBarren.Contains(checkName);
        }

        private static HashSet<Item> ResolveItemsAlias(string alias)
        {
            switch (alias)
            {
                case "bugs":
                    return new(Randomizer.Items.goldenBugs);
                default:
                    throw new Exception($"Failed to resolve alias '{alias}'.");
            }
        }
    }
}
