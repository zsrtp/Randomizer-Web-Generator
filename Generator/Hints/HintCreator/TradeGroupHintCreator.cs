namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class TradeGroupHintCreator : HintCreator
    {
        private static readonly HashSet<TradeGroup> bugTradeGroups =
            new()
            {
                TradeGroup.Male_Bugs,
                TradeGroup.Female_Bugs,
                TradeGroup.Ants,
                TradeGroup.Mantises,
                TradeGroup.Butterflies,
                TradeGroup.Phasmids,
                TradeGroup.Dayflies,
                TradeGroup.Stag_Beetles,
                TradeGroup.Ladybugs,
                TradeGroup.Grasshoppers,
                TradeGroup.Beetles,
                TradeGroup.Pill_Bugs,
                TradeGroup.Snails,
                TradeGroup.Dragonflies,
            };

        private HashSet<TradeGroup> validGroups;
        private TradeGroupHint.Vagueness vagueness = TradeGroupHint.Vagueness.Vague;
        private HashSet<TradeGroupHint.Status> validStatuses;

        private TradeGroupHintCreator()
        {
            this.type = HintCreatorType.TradeGroup;
        }

        new public static TradeGroupHintCreator fromJObject(JObject obj)
        {
            TradeGroupHintCreator inst = new();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                List<string> validGroups = HintSettingUtils.getOptionalStringList(
                    options,
                    "validGroups",
                    null
                );
                if (validGroups != null)
                {
                    inst.validGroups = new();
                    foreach (string str in validGroups)
                    {
                        TradeGroup tradeGroup;
                        bool success = Enum.TryParse(str, true, out tradeGroup);
                        if (success)
                            inst.validGroups.Add(tradeGroup);
                        else
                            throw new Exception($"Failed to parse '{str}' to TradeGroup enum.");
                    }
                }

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
                        TradeGroupHint.Status status;
                        bool success = Enum.TryParse(str, true, out status);
                        if (success)
                            inst.validStatuses.Add(status);
                        else
                            throw new Exception(
                                $"Failed to parse '{str}' to TradeGroupHint.Status enum."
                            );
                    }
                }

                string vaguenessStr = HintSettingUtils.getOptionalString(
                    options,
                    "vagueness",
                    null
                );
                if (vaguenessStr != null)
                {
                    TradeGroupHint.Vagueness vagueness;
                    bool success = Enum.TryParse(vaguenessStr, true, out vagueness);
                    if (success)
                        inst.vagueness = vagueness;
                    else
                        throw new Exception(
                            $"Failed to parse '{vaguenessStr}' to TradeGroupHint.Vagueness enum."
                        );
                }
            }

            if (inst.validStatuses == null)
                inst.validGroups = new() { TradeGroup.Male_Bugs, TradeGroup.Female_Bugs, };

            if (inst.validStatuses == null)
                inst.validStatuses = new()
                {
                    TradeGroupHint.Status.Bad,
                    TradeGroupHint.Status.Important,
                };

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            if (numHints < 1 || validGroups.Count < 1 || validStatuses.Count < 1)
                return null;

            // Pick by group, then pick a validStatus from within the group. If
            // a group is there, it must have at least one validStatus which
            // matches up with the validStatuses.

            // The things we can pick from are groups.
            // Each group will have a list of supported checks.

            // PickableGroupThing
            // - requiredPickableThings => each has (srcChecks and tgtCheck)
            // - importantPickableThings => each has (srcChecks and tgtCheck)
            // - barrenPickableThings => has list of checks which should be marked known barren?

            HashSet<Item> groupNotBarrenIfHasItems = new();

            List<PickableTradeGroup> ptgs = new();

            HashSet<TradeGroup> forcedTradeGroups = new();

            HashSet<TradeGroup> groupsToDo = validGroups;
            if (hintSettings.agitha)
            {
                // If agitha hints are turned on and exactly one bugGender is
                // valid, make sure the other gender is also calculated since
                // they can depend on each other.
                bool hasMaleBugsGroup = false;
                bool hasFemaleBugsGroup = false;
                foreach (TradeGroup tradeGroup in validGroups)
                {
                    switch (tradeGroup)
                    {
                        case TradeGroup.Male_Bugs:
                            hasMaleBugsGroup = true;
                            break;
                        case TradeGroup.Female_Bugs:
                            hasFemaleBugsGroup = true;
                            break;
                    }
                }

                if (hasMaleBugsGroup && !hasFemaleBugsGroup)
                {
                    groupsToDo.Add(TradeGroup.Female_Bugs);
                    forcedTradeGroups.Add(TradeGroup.Female_Bugs);
                }
                else if (!hasMaleBugsGroup && hasFemaleBugsGroup)
                {
                    groupsToDo.Add(TradeGroup.Male_Bugs);
                    forcedTradeGroups.Add(TradeGroup.Male_Bugs);
                }
            }

            foreach (TradeGroup tradeGroup in groupsToDo)
            {
                // Skip tradeGroups which are already hinted and are not forced.
                if (
                    !forcedTradeGroups.Contains(tradeGroup)
                    && genData.hinted.hintedTradeGroups.Contains(tradeGroup)
                )
                    continue;

                PickableTradeGroup ptg = new(tradeGroup);
                ptgs.Add(ptg);

                // Get all checks for group.
                // List<string> unknownChecks = new();
                bool groupCanBeHintedBarren = true;

                HashSet<Item> items = TradeGroupUtils.ResolveToItems(tradeGroup);

                // For each item in the tradeGroup, see which check the chain
                // resolves to.
                foreach (Item item in items)
                {
                    // Skip if item is part of a circular loop or missing.
                    if (!genData.tradeItemToChainEndCheck.ContainsKey(item))
                        continue;

                    // If end check is good, then groups with this item cannot
                    // be hintedBarren regardless of anything else.
                    string endCheck = genData.tradeItemToChainEndCheck[item];
                    if (genData.CheckIsGood(endCheck, true))
                    {
                        groupCanBeHintedBarren = false;
                        groupNotBarrenIfHasItems.Add(item);
                    }

                    List<string> srcChecks = genData.itemToChecksList[item];
                    if (ListUtils.isEmpty(srcChecks))
                        continue;

                    HashSet<string> hintableSrcChecks = new();
                    foreach (string checkName in srcChecks)
                    {
                        if (SrcCheckIsHintable(genData, checkName))
                            hintableSrcChecks.Add(checkName);
                    }

                    if (ListUtils.isEmpty(hintableSrcChecks))
                        continue;

                    // Need to find at least one srcCheck which is directable.
                    // HashSet<string> directableSrcChecks

                    // At the end, first pick a status, then pick a
                    // tradeGroup which can be hinted that status.

                    if (EndCheckIsHintable(genData, endCheck))
                    {
                        // unknownChecks.Add(endCheck);

                        // Bypass since endCheck is mostly agithaReward
                        // checks.
                        if (genData.CheckIsGood(endCheck, true))
                        {
                            SrcAndTgtChecks sAndTChecks = new(hintableSrcChecks, endCheck);

                            if (EndCheckIsHintable(genData, endCheck))
                            {
                                if (genData.requiredChecks.Contains(endCheck))
                                    ptg.requiredOptions.Add(sAndTChecks);
                                ptg.importantOptions.Add(sAndTChecks);
                            }
                        }
                        else
                        {
                            ptg.barrenChecks.AddRange(srcChecks);
                            ptg.barrenChecks.Add(endCheck);
                        }
                    }
                }

                // Should not hint a group barren if it does not have a chain
                // starter which is not in the middle of a chain.

                // Should also not hint a group good or required if it ...

                // For each thing, we should keep track of which checks would be directedToward.

                // After generating a hint in an iteration, we go through and
                // any which have lost all of their valid chainStart
                // directedToward targets are no longer valid.

                // We also keep track of the valid chainEnd. This is also
                // necessary to get marked. For example, we don't hint femBugs
                // are barren if we already have a hint "Sketch leads to
                // Spinner" if the only Agitha reward is Spinner (Sketch =>
                // maleBugs => Spinner).


                //"MaleMantis path to
                // Lantern" and

                // For directing, we need the endCheck to not be diretedToward.
                // We also need at least one startCheck to be not a tradeCheck and

                // If cannot be barren, then clear barren on ptg
                if (!groupCanBeHintedBarren)
                    ptg.barrenChecks = new();
            }

            // Remove any groups that have no hintable.

            // Keep a clean copy of the ptgs before we remove elements.
            List<PickableTradeGroup> nonMutatedPtgs = new(ptgs);

            List<Hint> hints = new();

            while (hints.Count < numHints && ptgs.Count > 0)
            {
                // Pick a ptg, then pick a status.
                PickableTradeGroup ptg = HintUtils.RemoveRandomListItem(genData.rnd, ptgs);

                // Skip over bugGender if it was only added so its data is
                // available for the opposite gender (not actually a valid
                // selection).
                if (forcedTradeGroups.Contains(ptg.tradeGroup))
                    continue;

                List<TradeGroupHint.Status> possibleStatuses = ptg.GetPossibleStatuses(
                    validStatuses
                );

                // ptg was removed, so go to next iteration
                if (possibleStatuses.Count < 1)
                    continue;

                TradeGroupHint.Status status = HintUtils.PickRandomListItem(
                    genData.rnd,
                    possibleStatuses
                );

                SrcAndTgtChecks pickedSAndT = null;

                if (
                    genData.hinted.agithaHintedDead
                    && status == TradeGroupHint.Status.Bad
                    && bugTradeGroups.Contains(ptg.tradeGroup)
                )
                {
                    // If we are trying to hint a bug tradeGroup dead and we
                    // already know that Agitha is dead, then skip.
                    continue;
                }
                else if (
                    hintSettings.agitha
                    && status == TradeGroupHint.Status.Bad
                    && (
                        ptg.tradeGroup == TradeGroup.Male_Bugs
                        || ptg.tradeGroup == TradeGroup.Female_Bugs
                    )
                )
                {
                    // Special handling for barren gender when Agitha hints are
                    // on.
                    TradeGroup otherGenderTradeGroup =
                        ptg.tradeGroup == TradeGroup.Male_Bugs
                            ? TradeGroup.Female_Bugs
                            : TradeGroup.Male_Bugs;

                    PickableTradeGroup otherPtg = nonMutatedPtgs.Find(
                        (ptg) => ptg.tradeGroup == otherGenderTradeGroup
                    );

                    // Skip if can't find or can't hint important (we don't hint
                    // a gender as barren if Agitha hints are on and they
                    // already tell us all bugs are bad).
                    if (otherPtg == null || !otherPtg.CanHintImportant())
                        continue;

                    pickedSAndT = HintUtils.RemoveRandomListItem(
                        genData.rnd,
                        otherPtg.importantOptions
                    );
                    pickedSAndT.UpdateHinted(genData);

                    genData.hinted.hintedTradeGroups.Add(ptg.tradeGroup);

                    TradeGroupHint hint =
                        new(ptg.tradeGroup, vagueness, status, pickedSAndT.endCheck);
                    hints.Add(hint);
                }
                else
                {
                    // Default handling
                    switch (status)
                    {
                        case TradeGroupHint.Status.Bad:
                        {
                            // Mark all checks in the thing as knownBarren.
                            // genData.hinted.alreadyCheckKnownBarren.UnionWith(ptg.barrenChecks);
                            genData.hinted.AddHintedBarrenChecks(ptg.barrenChecks);
                            ptg.barrenChecks = new();
                            break;
                        }
                        case TradeGroupHint.Status.Important:
                        {
                            pickedSAndT = HintUtils.RemoveRandomListItem(
                                genData.rnd,
                                ptg.importantOptions
                            );
                            pickedSAndT.UpdateHinted(genData);
                            break;
                        }
                        case TradeGroupHint.Status.Required:
                        {
                            pickedSAndT = HintUtils.RemoveRandomListItem(
                                genData.rnd,
                                ptg.requiredOptions
                            );
                            pickedSAndT.UpdateHinted(genData);
                            break;
                        }
                        default:
                            throw new Exception($"Unable to handle unsupported status '{status}'.");
                    }

                    genData.hinted.hintedTradeGroups.Add(ptg.tradeGroup);

                    TradeGroupHint hint =
                        new(ptg.tradeGroup, vagueness, status, pickedSAndT?.endCheck);
                    hints.Add(hint);
                }

                // Do cleanup
                for (int i = ptgs.Count - 1; i >= 0; i--)
                {
                    PickableTradeGroup pickable = ptgs[i];
                    if (pickedSAndT != null)
                        pickable.UpdateSrcAndTargetLists(genData);
                    else
                        pickable.UpdateBarren(genData);

                    if (!pickable.CanHintSomething())
                        ptgs.RemoveAt(i);
                }
            }

            return hints;
        }

        private static bool SrcCheckIsHintable(HintGenData genData, string checkName)
        {
            return (
                !HintUtils.checkIsPlayerKnownStatus(checkName)
                && !HintUtils.CheckIsTradeItemReward(checkName)
                && !genData.hinted.hintsShouldIgnoreChecks.Contains(checkName)
                && !genData.hinted.alreadyCheckKnownBarren.Contains(checkName)
                && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
            );
        }

        private static bool EndCheckIsHintable(HintGenData genData, string checkName)
        {
            return (
                !HintUtils.checkIsPlayerKnownStatus(checkName)
                && !genData.hinted.alreadyCheckKnownBarren.Contains(checkName)
                && !genData.hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
            );
        }

        private class PickableTradeGroup
        {
            public TradeGroup tradeGroup;
            public List<SrcAndTgtChecks> requiredOptions = new();
            public List<SrcAndTgtChecks> importantOptions = new();
            public List<string> barrenChecks = new();

            public PickableTradeGroup(TradeGroup tradeGroup)
            {
                this.tradeGroup = tradeGroup;
            }

            public bool CanHintBarren()
            {
                return !ListUtils.isEmpty(barrenChecks);
            }

            public bool CanHintImportant()
            {
                return !ListUtils.isEmpty(importantOptions);
            }

            public bool CanHintRequired()
            {
                return !ListUtils.isEmpty(requiredOptions);
            }

            public bool CanHintSomething()
            {
                return CanHintBarren() || CanHintImportant() || CanHintRequired();
            }

            public List<TradeGroupHint.Status> GetPossibleStatuses(
                HashSet<TradeGroupHint.Status> validStatuses
            )
            {
                List<TradeGroupHint.Status> hintableStatuses = new();

                if (validStatuses.Contains(TradeGroupHint.Status.Bad) && CanHintBarren())
                    hintableStatuses.Add(TradeGroupHint.Status.Bad);

                if (validStatuses.Contains(TradeGroupHint.Status.Important) && CanHintImportant())
                    hintableStatuses.Add(TradeGroupHint.Status.Important);

                if (validStatuses.Contains(TradeGroupHint.Status.Required) && CanHintRequired())
                    hintableStatuses.Add(TradeGroupHint.Status.Required);

                return hintableStatuses;
            }

            public void UpdateSrcAndTargetLists(HintGenData genData)
            {
                UpdateImportantOrRequired(genData, importantOptions);
                UpdateImportantOrRequired(genData, requiredOptions);
            }

            public void UpdateBarren(HintGenData genData)
            {
                List<string> newBarren = new();
                foreach (string checkName in barrenChecks)
                {
                    if (!genData.hinted.alreadyCheckKnownBarren.Contains(checkName))
                        newBarren.Add(checkName);
                }
                barrenChecks = newBarren;
            }

            private void UpdateImportantOrRequired(HintGenData genData, List<SrcAndTgtChecks> list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    SrcAndTgtChecks el = list[i];
                    el.RemoveDirectedChecks(genData);
                    if (!el.IsValid())
                        list.RemoveAt(i);
                }
            }
        }

        private class SrcAndTgtChecks
        {
            public HashSet<string> srcChecks;
            public string endCheck;

            public SrcAndTgtChecks(HashSet<string> srcChecks, string endCheck)
            {
                this.srcChecks = srcChecks;
                this.endCheck = endCheck;
            }

            public void UpdateHinted(HintGenData genData)
            {
                foreach (string srcCheck in srcChecks)
                {
                    genData.hinted.alreadyCheckDirectedToward.Add(srcCheck);
                }
                genData.hinted.alreadyCheckDirectedToward.Add(endCheck);
            }

            public void RemoveDirectedChecks(HintGenData genData)
            {
                srcChecks.RemoveWhere(
                    (checkName) => genData.hinted.alreadyCheckDirectedToward.Contains(checkName)
                );
                if (genData.hinted.alreadyCheckDirectedToward.Contains(endCheck))
                    endCheck = null;
            }

            public bool IsValid()
            {
                return srcChecks.Count > 0 && !StringUtils.isEmpty(endCheck);
            }
        }
    }
}
