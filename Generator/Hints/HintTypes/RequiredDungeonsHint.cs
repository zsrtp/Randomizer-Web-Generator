namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class RequiredDungeonsHint : Hint
    {
        public override HintType type { get; } = HintType.RequiredDungeons;

        private byte requiredDungeons;
        private Dictionary<Zone, HashSet<Zone>> dungeonEntrances;

        // always derived
        private List<Zone> requiredDungeonZones;

        // TODO: since we create this hint even when HintDist is set to None, need to make sure we
        // fully respect that regardless of what the Dungeon ER hint setting might happen to be set
        // to.

        public static RequiredDungeonsHint Create(HintGenData genData)
        {
            return new RequiredDungeonsHint(
                (byte)Randomizer.RequiredDungeons,
                genData.dungeonEntrances
            );
        }

        private RequiredDungeonsHint(
            byte requiredDungeons,
            Dictionary<Zone, HashSet<Zone>> dungeonEntrances
        )
        {
            this.requiredDungeons = requiredDungeons;
            this.dungeonEntrances = dungeonEntrances;

            CalcDerived();
        }

        private void CalcDerived()
        {
            requiredDungeonZones = new();
            foreach (KeyValuePair<string, byte> pair in HintConstants.dungeonZonesToRequiredMaskMap)
            {
                if ((requiredDungeons & pair.Value) != 0)
                    requiredDungeonZones.Add(ZoneUtils.StringToIdThrows(pair.Key));
            }
        }

        // private void CalcDerived(HintGenData genData, Dictionary<int, int> itemPlacements)
        // {
        //     buildCheckToItemMappings(goodChecks, itemPlacements);
        //     buildCheckToItemMappings(bigKeyChecks, itemPlacements);

        //     // When creating the hint during generation, we calculate rather
        //     // than use input value.
        //     if (genData != null)
        //     {
        //         // If we have bigKeyChecks, then do the calc.
        //         if (!ListUtils.isEmpty(bigKeyChecks))
        //         {
        //             Item bigKeyItem = checkNameToContents[bigKeyChecks[0]];

        //             if (
        //                 genData.itemToChecksList.TryGetValue(
        //                     bigKeyItem,
        //                     out List<string> checksGivingItem
        //                 )
        //             )
        //             {
        //                 bigKeyUseDefiniteArticle = checksGivingItem.Count == 1;
        //             }
        //         }
        //     }
        // }

        // private void buildCheckToItemMappings(
        //     List<string> checkNames,
        //     Dictionary<int, int> itemPlacements
        // )
        // {
        //     if (!ListUtils.isEmpty(checkNames))
        //     {
        //         foreach (string checkName in checkNames)
        //         {
        //             Item contents;
        //             if (itemPlacements != null)
        //             {
        //                 // When decoding hint from string
        //                 contents = HintUtils.getCheckContents(checkName, itemPlacements);
        //             }
        //             else
        //             {
        //                 // When creating hint during generation
        //                 contents = HintUtils.getCheckContents(checkName);
        //             }
        //             checkNameToContents[checkName] = contents;
        //         }
        //     }
        // }

        public override List<HintText> toHintTextList(CustomMsgData customMsgData)
        {
            List<HintText> hintTexts = new();

            string midnaDungeonMsg = Res.SimpleMsg(
                "midna.required-dungeons",
                new Dictionary<string, string>()
                {
                    { "count", requiredDungeonZones.Count.ToString() }
                }
            );

            // Note: need to normalize so JP number converts to wide version.
            HintText firstHintText = new();
            firstHintText.text = Res.LangSpecificNormalize(midnaDungeonMsg);
            hintTexts.Add(firstHintText);

            if (requiredDungeonZones.Count > 0)
            {
                HintText reqDungeonsHintText = new();
                reqDungeonsHintText.text = GenLinkHouseSignText();
                hintTexts.Add(reqDungeonsHintText);

                HintText dungeonErHintText = new();
                // dungeonErHintText.text = testGetReqDungeonProvincesMsg();
                dungeonErHintText.text = testGetDungeonEntranceHint();
                hintTexts.Add(dungeonErHintText);
            }

            // string normalizedText = Res.LangSpecificNormalize(text);

            // HintText hintText = new HintText();
            // hintText.text = normalizedText;
            // return new List<HintText> { hintText };
            return hintTexts;

            // string context;
            // if (!ListUtils.isEmpty(goodChecks))
            // {
            //     if (!includeBigKeyInfo)
            //         context = "good";
            //     else if (!ListUtils.isEmpty(bigKeyChecks))
            //         context = "good-and-big-keys";
            //     else
            //         context = "good-no-big-keys";
            // }
            // else
            // {
            //     if (includeBigKeyInfo && !ListUtils.isEmpty(bigKeyChecks))
            //         context = "only-big-keys";
            //     else
            //         context = "";
            // }

            // string bigKeysText = "";

            // if (includeBigKeyInfo)
            // {
            //     // In the future, we may need to check this differently
            //     // depending on how plurals work in different languages. We want
            //     // to prioritize the "count" one, but if we pass both "count"
            //     // and "context", then it will resolve to the "context" one
            //     // without the "count" first. For now, just comparing if there
            //     // are 2 or more since it works for both English and French.
            //     Dictionary<string, string> interpolation = new();
            //     if (bigKeyChecks.Count >= 2)
            //         interpolation["count"] = bigKeyChecks.Count.ToString();
            //     else
            //         interpolation["context"] = bigKeyUseDefiniteArticle ? "def" : "indef";

            //     bigKeysText = Res.Msg("hint-type.beyond-point.big-key", interpolation)
            //         .ResolveWithColor(CustomMessages.messageColorOrange);
            // }

            // string text = Res.LangSpecificNormalize(
            //     Res.Msg("hint-type.beyond-point", new() { { "context", context } })
            //         .Substitute(new() { { "big-key", bigKeysText } })
            // );

            // HintText hintText = new HintText();
            // hintText.text = text;
            // return new List<HintText> { hintText };
        }

        private string GenLinkHouseSignText()
        {
            List<(string, byte, string)> dungeonData =
                new()
                {
                    ("required-dungeon.forest-temple", 0x01, CustomMessages.messageColorGreen),
                    ("required-dungeon.goron-mines", 0x02, CustomMessages.messageColorRed),
                    ("required-dungeon.lakebed-temple", 0x04, CustomMessages.messageColorBlue),
                    ("required-dungeon.arbiters-grounds", 0x08, CustomMessages.messageColorOrange),
                    ("required-dungeon.snowpeak-ruins", 0x10, CustomMessages.messageColorLightBlue),
                    ("required-dungeon.temple-of-time", 0x20, CustomMessages.messageColorDarkGreen),
                    ("required-dungeon.city-in-the-sky", 0x40, CustomMessages.messageColorYellow),
                    (
                        "required-dungeon.palace-of-twilight",
                        0x80,
                        CustomMessages.messageColorPurple
                    ),
                };

            StringBuilder sb = new();
            foreach (var tuple in dungeonData)
            {
                if ((requiredDungeons & tuple.Item2) != 0)
                {
                    if (sb.Length > 0)
                        sb.Append('\n');
                    // Use an empty string for the end color so we do not run
                    // out of bytes and have the text get cut off.
                    sb.Append(Res.Msg(tuple.Item1, null).ResolveWithColor(tuple.Item3, ""));
                }
            }

            string text;
            if (sb.Length > 0)
                text = sb.ToString();
            else
                text = Res.SimpleMsg("required-dungeon.none", null);

            string normalized = Res.LangSpecificNormalize(text);
            return normalized;
        }

        private string testGetDungeonEntranceHint()
        {
            // Leaving out SPR and HC for now. These should really be built according to the
            // settings, then the order is encoded with each tier already randomized. In fact, we
            // only need to store any entrance which should actually be hinted.
            List<List<Zone>> hintTiers =
                new()
                {
                    new() { Zone.Lakebed_Temple, Zone.Arbiters_Grounds },
                    new() { Zone.Goron_Mines, Zone.Temple_of_Time },
                    new() { Zone.City_in_the_Sky, Zone.Palace_of_Twilight },
                    new() { Zone.Forest_Temple },
                    // Hardest: SPR, HC
                    // Hard: LBT, AG
                    // Medium: ToT, GM
                    // Easy: CitS, PoT
                    // Trivial: FT
                };

            // TODO: randomization within tiers must be done using genData.rnd. For now just doing
            // here for testing.

            Random replaceMeRandom = new Random();
            foreach (List<Zone> list in hintTiers)
            {
                HintUtils.ShuffleListInPlace(replaceMeRandom, list);
            }

            // But for this, we want to ignore SPR & HC

            List<KeyValuePair<Zone, HashSet<Zone>>> filteredList = new();
            HashSet<Zone> entranceZones = new();

            HashSet<Zone> interestedDungeonsSet = new(requiredDungeonZones);
            interestedDungeonsSet.Add(Zone.Hyrule_Castle);
            foreach (KeyValuePair<Zone, HashSet<Zone>> pair in dungeonEntrances)
            {
                foreach (Zone pointedToZone in pair.Value)
                {
                    if (interestedDungeonsSet.Contains(pointedToZone))
                    {
                        entranceZones.Add(pair.Key);
                        filteredList.Add(pair);
                        break;
                    }
                }
            }

            Zone zoneToHint = Zone.Invalid;

            foreach (List<Zone> list in hintTiers)
            {
                foreach (Zone zone in list)
                {
                    if (entranceZones.Contains(zone))
                    {
                        zoneToHint = zone;
                        break;
                    }
                }
                if (zoneToHint != Zone.Invalid)
                    break;
            }

            List<KeyValuePair<Zone, HashSet<Zone>>> sorted = filteredList
                .OrderBy((el) => el.Key)
                .ToList();

            Dictionary<Zone, string> zoneToAbbrev =
                new()
                {
                    { Zone.Forest_Temple, "FT" },
                    { Zone.Goron_Mines, "GM" },
                    { Zone.Lakebed_Temple, "LBT" },
                    { Zone.Arbiters_Grounds, "AG" },
                    { Zone.Snowpeak_Ruins, "SPR" },
                    { Zone.Temple_of_Time, "ToT" },
                    { Zone.City_in_the_Sky, "CitS" },
                    { Zone.Palace_of_Twilight, "PoT" },
                    { Zone.Hyrule_Castle, "HC" },
                };

            string result = "";
            foreach (KeyValuePair<Zone, HashSet<Zone>> pair in sorted)
            {
                if (result.Length > 0)
                    result += "\n";
                if (pair.Key == zoneToHint)
                {
                    string val = "";
                    foreach (Zone zone in pair.Value)
                    {
                        if (val.Length > 0)
                            val += ",";
                        val += zoneToAbbrev[zone];
                    }

                    result += $"{zoneToAbbrev[pair.Key]} => {val}";
                }
                else
                    result += $"{zoneToAbbrev[pair.Key]} => ??";
            }
            return result;
        }

        private string testGetReqDungeonProvincesMsg()
        {
            Dictionary<Zone, Province> entranceToProvince =
                new()
                {
                    { Zone.Forest_Temple, Province.Faron },
                    { Zone.Goron_Mines, Province.Eldin },
                    { Zone.Lakebed_Temple, Province.Lanayru },
                    { Zone.Arbiters_Grounds, Province.Desert },
                    { Zone.Snowpeak_Ruins, Province.Peak },
                    { Zone.Temple_of_Time, Province.Faron },
                    { Zone.City_in_the_Sky, Province.Lanayru },
                    { Zone.Palace_of_Twilight, Province.Desert },
                    { Zone.Hyrule_Castle, Province.Lanayru },
                };

            Dictionary<Province, string> provinceToColorList =
                new()
                {
                    { Province.Ordona, CustomMessages.messageColorYellow },
                    { Province.Faron, CustomMessages.messageColorGreen },
                    { Province.Eldin, CustomMessages.messageColorRed },
                    { Province.Lanayru, CustomMessages.messageColorBlue },
                    { Province.Desert, CustomMessages.messageColorOrange },
                    { Province.Peak, CustomMessages.messageColorLightBlue },
                };

            HashSet<Zone> requiredDungeonsSet = new(requiredDungeonZones);

            HashSet<Province> provinces = new();
            foreach (KeyValuePair<Zone, HashSet<Zone>> pair in dungeonEntrances)
            {
                foreach (Zone pointedToZone in pair.Value)
                {
                    if (requiredDungeonsSet.Contains(pointedToZone))
                    {
                        provinces.Add(entranceToProvince[pair.Key]);
                        break;
                    }
                }
            }

            if (provinces.Count < 1)
                return null;

            List<Province> provinceList = new(provinces);
            provinceList.Sort();

            StringBuilder sb =
                new("The required dungeons are found randomly in these provinces:\n");
            int index = 0;
            foreach (Province province in provinceList)
            {
                AreaId areaId = AreaId.Province(province);
                string color = provinceToColorList[province];
                string areaRes = Res.Msg(areaId.GenResKey(), null, null).ResolveWithColor(color);

                if (index > 0)
                    sb.Append(", ");
                sb.Append(areaRes);
                index += 1;
            }

            string text = sb.ToString();

            string normalized = Res.LangSpecificNormalize(text);
            return normalized;
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);

            result += SettingsEncoder.EncodeNumAsBits(requiredDungeons, 8);

            // Encode dungeonEntrances Dictionary
            result += SettingsEncoder.EncodeAsVlq16((ushort)dungeonEntrances.Count);
            foreach (KeyValuePair<Zone, HashSet<Zone>> pair in dungeonEntrances)
            {
                Zone zoneKey = pair.Key;
                HashSet<Zone> destinationZones = pair.Value;

                result += SettingsEncoder.EncodeNumAsBits((byte)zoneKey, bitLengths.zoneId);
                result += SettingsEncoder.EncodeAsVlq16((ushort)destinationZones.Count);

                foreach (Zone destZone in destinationZones)
                {
                    result += SettingsEncoder.EncodeNumAsBits((byte)destZone, bitLengths.zoneId);
                }
            }

            return result;
        }

        public static RequiredDungeonsHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, int> itemPlacements
        )
        {
            Dictionary<Zone, HashSet<Zone>> dungeonEntrances = new();

            byte requiredDungeons = processor.NextByte();

            int numDictEntries = processor.NextVlq16();
            for (int i = 0; i < numDictEntries; i++)
            {
                Zone zoneKey = (Zone)processor.NextInt(bitLengths.zoneId);

                HashSet<Zone> value = new();
                dungeonEntrances[zoneKey] = value;

                int numValueEntries = processor.NextVlq16();
                for (int setEntryIdx = 0; setEntryIdx < numValueEntries; setEntryIdx++)
                {
                    Zone valueZone = (Zone)processor.NextInt(bitLengths.zoneId);
                    value.Add(valueZone);
                }
            }

            return new RequiredDungeonsHint(requiredDungeons, dungeonEntrances);
        }

        public override List<HintInfo> GetHintInfos(CustomMsgData customMsgData)
        {
            var hintInfos = toHintTextList(customMsgData)
                .Select(
                    (hintText) =>
                    {
                        HintInfo hintInfo = new(hintText.text);
                        return hintInfo;
                    }
                );
            List<HintInfo> result = new(hintInfos);
            return result;

            // string hintText = toHintTextList(customMsgData)[0].text;

            // HintInfo hintInfo = new(hintText);

            // // if (!ListUtils.isEmpty(goodChecks))
            // // {
            // //     foreach (string checkName in goodChecks)
            // //     {
            // //         hintInfo.referencedChecks.Add(checkName);
            // //         hintInfo.referencedItems.Add(checkNameToContents[checkName]);
            // //     }
            // // }

            // // if (!ListUtils.isEmpty(bigKeyChecks))
            // // {
            // //     foreach (string checkName in bigKeyChecks)
            // //     {
            // //         hintInfo.referencedChecks.Add(checkName);
            // //         hintInfo.referencedItems.Add(checkNameToContents[checkName]);
            // //     }
            // // }

            // return hintInfo;
        }
    }
}
