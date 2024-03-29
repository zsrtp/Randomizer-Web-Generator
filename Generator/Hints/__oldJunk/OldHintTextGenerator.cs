// public class HintTextGenerator
// {
//     // public static HintText GenHintText(SharedSettings sharedSettings, Hint hint)
//     // {
//     //     if (hint == null)
//     //     {
//     //         return null;
//     //     }

//     //     switch (hint.hintType)
//     //     {
//     //         case HintType.Always:
//     //         case HintType.Sometimes:
//     //         case HintType.Random:
//     //             return GenCheckContentsHint(hint);
//     //         case HintType.SpiritOfLight:
//     //             return GenSpolHint(sharedSettings, hint);
//     //         case HintType.Barren:
//     //             return GenBarrenHint(sharedSettings, hint);
//     //         case HintType.Junk:
//     //             return GenJunkHint(hint);
//     //     }

//     //     return null;
//     // }

//     // private static HintText GenCheckContentsHint(Hint hint)
//     // {
//     //     HintText hintText = new HintText();
//     //     hintText.text = $"They say that {{{hint.checkName}}} has {{{hint.contents}}}.";
//     //     hintText.colors = new()
//     //     {
//     //         HintText.GcColors[TextColor.Red],
//     //         HintText.GcColors[TextColor.Green]
//     //     };
//     //     hintText.checkName = hint.checkName;
//     //     hintText.checkContents = hint.contents.ToString();
//     //     return hintText;
//     // }

//     // private static HintText GenSpolHint(SharedSettings sharedSettings, Hint hint)
//     // {
//     //     Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//     //         sharedSettings
//     //     );
//     //     string zoneName = checkToHintZoneMap[hint.checkName];

//     //     HintText hintText = new HintText();
//     //     hintText.text =
//     //         $"The {{Spirits of Light}} guide the hero chosen by the gods to {{{zoneName}}}.";
//     //     hintText.colors = new()
//     //     {
//     //         HintText.GcColors[TextColor.CustomBlue],
//     //         HintText.GcColors[TextColor.Red]
//     //     };
//     //     hintText.checkName = hint.checkName;
//     //     hintText.checkContents = hint.contents.ToString();
//     //     return hintText;
//     // }

//     // private static HintText GenBarrenHint(SharedSettings sharedSettings, Hint hint)
//     // {
//     //     Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//     //         sharedSettings
//     //     );
//     //     string zoneName = checkToHintZoneMap[hint.checkName];

//     //     HintText hintText = new HintText();
//     //     if (zoneName == "Agitha")
//     //     {
//     //         hintText.text =
//     //             $"They say that there is {{nothing}} to be gained from {{{zoneName}}}.";
//     //     }
//     //     else if (zoneName == "Hero's Spirit")
//     //     {
//     //         hintText.text =
//     //             $"They say that there is {{nothing}} to be gained from the {{{zoneName}}}.";
//     //     }
//     //     else
//     //     {
//     //         hintText.text =
//     //             $"They say that there is {{nothing}} to be found in {{{zoneName}}}.";
//     //     }
//     //     hintText.colors = new()
//     //     {
//     //         HintText.GcColors[TextColor.Purple],
//     //         HintText.GcColors[TextColor.Red]
//     //     };
//     //     return hintText;
//     // }

//     // private static HintText GenJunkHint(Hint hint)
//     // {
//     //     HintText hintText = new HintText();
//     //     hintText.text = hint.checkName;
//     //     return hintText;
//     // }

//     // public static List<HintText> GenAgithaHintTexts(HintResults hintResults, int numBugsInPool)
//     // {
//     //     HashSet<Item> bugs =
//     //         new()
//     //         {
//     //             Item.Male_Ant,
//     //             Item.Female_Ant,
//     //             Item.Male_Beetle,
//     //             Item.Female_Beetle,
//     //             Item.Male_Pill_Bug,
//     //             Item.Female_Pill_Bug,
//     //             Item.Male_Phasmid,
//     //             Item.Female_Phasmid,
//     //             Item.Male_Grasshopper,
//     //             Item.Female_Grasshopper,
//     //             Item.Male_Stag_Beetle,
//     //             Item.Female_Stag_Beetle,
//     //             Item.Male_Butterfly,
//     //             Item.Female_Butterfly,
//     //             Item.Male_Ladybug,
//     //             Item.Female_Ladybug,
//     //             Item.Male_Mantis,
//     //             Item.Female_Mantis,
//     //             Item.Male_Dragonfly,
//     //             Item.Female_Dragonfly,
//     //             Item.Male_Dayfly,
//     //             Item.Female_Dayfly,
//     //             Item.Male_Snail,
//     //             Item.Female_Snail,
//     //         };

//     //     int numBugsGivenByAgitha = 0;

//     //     List<Hint> nonBugAgithaHints = new();
//     //     if (hintResults != null && hintResults.hints != null)
//     //     {
//     //         foreach (Hint hint in hintResults.hints)
//     //         {
//     //             if (hint.hintType == HintType.AgithaRewards)
//     //             {
//     //                 if (bugs.Contains(hint.contents))
//     //                 {
//     //                     numBugsGivenByAgitha += 1;
//     //                 }
//     //                 else
//     //                 {
//     //                     nonBugAgithaHints.Add(hint);
//     //                 }
//     //             }
//     //         }
//     //     }

//     //     int numForText = numBugsInPool - numBugsGivenByAgitha;
//     //     if (numForText < 0)
//     //     {
//     //         numForText = 0;
//     //     }

//     //     List<HintText> hintTexts = new();

//     //     HintText firstText = new HintText();
//     //     firstText.text =
//     //         $"Please bring the {{{numForText}}} golden bugs to Princess Agitha's ball.";
//     //     firstText.colors = new() { HintText.GcColors[TextColor.Red] };
//     //     hintTexts.Add(firstText);

//     //     if (nonBugAgithaHints.Count < 1)
//     //     {
//     //         HintText secondText = new HintText();
//     //         secondText.text =
//     //             "I will share my {happiness} with benefactors of the insect kingdom.";
//     //         secondText.colors = new() { HintText.GcColors[TextColor.Red] };
//     //         hintTexts.Add(secondText);
//     //     }
//     //     else
//     //     {
//     //         HintText secondText = new HintText();
//     //         secondText.text =
//     //             "I have {GREAT happiness} to share with benefactors of the insect kingdom:";
//     //         secondText.colors = new() { HintText.GcColors[TextColor.Red] };
//     //         hintTexts.Add(secondText);

//     //         List<string> rewardsFromAgitha = new();
//     //         foreach (Hint hint in nonBugAgithaHints)
//     //         {
//     //             if (!bugs.Contains(hint.contents))
//     //             {
//     //                 rewardsFromAgitha.Add(hint.contents.ToString());
//     //             }
//     //         }
//     //         rewardsFromAgitha.Sort(StringComparer.Ordinal);

//     //         List<string> thirdTextColors = new();

//     //         StringBuilder sb = new StringBuilder();
//     //         for (int i = 0; i < rewardsFromAgitha.Count; i++)
//     //         {
//     //             thirdTextColors.Add(HintText.GcColors[TextColor.Red]);
//     //             string reward = rewardsFromAgitha[i];
//     //             sb.Append($"{{{reward}}}");
//     //             if (i != rewardsFromAgitha.Count - 1)
//     //             {
//     //                 sb.Append(", ");
//     //             }
//     //         }

//     //         HintText thirdText = new HintText();
//     //         thirdText.text = sb.ToString();
//     //         thirdText.colors = thirdTextColors;
//     //         hintTexts.Add(thirdText);
//     //     }

//     //     return hintTexts;
//     // }

//     // public static List<HintText> GenEndOfGameHintText(
//     //     HintResults hintResults,
//     //     SharedSettings sSettings
//     // )
//     // {
//     //     List<HintText> hcBigKeyText = GenEndOfGameHcBigKeyHintText(hintResults, sSettings);
//     //     List<HintText> swordHintText = GenEndOfGameSwordHintText(hintResults, sSettings);

//     //     return hcBigKeyText.Concat(swordHintText).ToList();
//     // }

//     // private static List<HintText> GenEndOfGameHcBigKeyHintText(
//     //     HintResults hintResults,
//     //     SharedSettings sSettings
//     // )
//     // {
//     //     List<HintText> hintTexts = new();

//     //     Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//     //         sSettings
//     //     );

//     //     string checkNameWithHcBigKey = null;

//     //     for (int i = 0; i < hintResults.hints.Count; i++)
//     //     {
//     //         Hint hint = hintResults.hints[i];
//     //         if (
//     //             hint.hintType == HintType.EndOfGame
//     //             && hint.contents == Item.Hyrule_Castle_Big_Key
//     //         )
//     //         {
//     //             checkNameWithHcBigKey = hint.checkName;
//     //             break;
//     //         }
//     //     }

//     //     if (checkNameWithHcBigKey != null)
//     //     {
//     //         string hintZone = checkToHintZoneMap[checkNameWithHcBigKey];
//     //         string foundInText = hintZone == "Agitha" ? "received from" : "found in";

//     //         HintText hintText = new();
//     //         hintText.colors = new()
//     //         {
//     //             HintText.GcColors[TextColor.Green],
//     //             HintText.GcColors[TextColor.Red]
//     //         };
//     //         hintText.text =
//     //             $"They say that the {{Hyrule Castle Big Key}} can be {foundInText} {{{hintZone}}}.";
//     //         hintText.checkName = checkNameWithHcBigKey;
//     //         hintText.checkContents = Item.Hyrule_Castle_Big_Key.ToString();

//     //         hintTexts.Add(hintText);
//     //     }

//     //     return hintTexts;
//     // }

//     // private static List<HintText> GenEndOfGameSwordHintText(
//     //     HintResults hintResults,
//     //     SharedSettings sSettings
//     // )
//     // {
//     //     List<HintText> hintTexts = new();

//     //     Dictionary<string, string> checkToHintZoneMap = HintUtils.getCheckToHintZoneMap(
//     //         sSettings
//     //     );

//     //     HashSet<string> swordZoneNames = new();
//     //     foreach (Hint hint in hintResults.hints)
//     //     {
//     //         if (hint.hintType == HintType.EndOfGame && hint.contents == Item.Progressive_Sword)
//     //         {
//     //             string zoneName = checkToHintZoneMap[hint.checkName];
//     //             swordZoneNames.Add(zoneName);
//     //         }
//     //     }

//     //     if (swordZoneNames.Count > 0)
//     //     {
//     //         HintText hintText = new();
//     //         hintText.colors = new() { HintText.GcColors[TextColor.Green] };

//     //         List<string> swordZoneList = swordZoneNames.ToList();
//     //         swordZoneList.Sort(StringComparer.Ordinal);

//     //         StringBuilder sb = new StringBuilder();
//     //         if (swordZoneList.Count == 1)
//     //         {
//     //             sb.Append(swordZoneList[0]);
//     //         }
//     //         else if (swordZoneList.Count == 2)
//     //         {
//     //             sb.Append(swordZoneList[0]);
//     //             sb.Append(" and ");
//     //             sb.Append(swordZoneList[1]);
//     //         }
//     //         else
//     //         {
//     //             for (int i = 0; i < swordZoneList.Count; i++)
//     //             {
//     //                 sb.Append($"{{{swordZoneList[i]}}}");
//     //                 if (i < swordZoneList.Count - 2)
//     //                 {
//     //                     sb.Append(", ");
//     //                 }
//     //                 else if (i == swordZoneList.Count - 2)
//     //                 {
//     //                     sb.Append(", and ");
//     //                 }
//     //             }
//     //         }

//     //         string inOrAt = swordZoneList[0] == "Agitha" ? "at" : "in";

//     //         if (swordZoneList.Count == 1)
//     //         {
//     //             hintText.text =
//     //                 $"They say that a {{sword}} can be found {inOrAt} {sb.ToString()}.";
//     //         }
//     //         else
//     //         {
//     //             hintText.text =
//     //                 $"They say that {{swords}} can be found {inOrAt} {sb.ToString()}.";
//     //         }
//     //         for (int i = 0; i < swordZoneList.Count; i++)
//     //         {
//     //             hintText.colors.Add(HintText.GcColors[TextColor.Red]);
//     //         }
//     //         hintTexts.Add(hintText);
//     //     }
//     //     else
//     //     {
//     //         // Not expected to ever run.
//     //         HintText hintText = new HintText();
//     //         hintText.text = "Not sure where the {swords} are.";
//     //         hintText.colors = new() { HintText.GcColors[TextColor.Green] };
//     //         hintTexts.Add(hintText);
//     //     }

//     //     return hintTexts;
//     // }
// }
