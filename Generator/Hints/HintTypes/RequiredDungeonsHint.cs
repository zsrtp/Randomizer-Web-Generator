namespace TPRandomizer.Hints
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TPRandomizer.Assets;
    using TPRandomizer.SSettings.Enums;
    using TPRandomizer.Util;

    public class RequiredDungeonsHint : Hint
    {
        public override HintType type { get; } = HintType.RequiredDungeons;

        private byte requiredDungeons;
        private DungeonER shuffleDungeonEntrances;
        private bool barrenDungeons;
        private Dictionary<Zone, List<Zone>> dungeonEntrances;

        // always derived
        private List<Zone> requiredDungeonZones;

        // TODO: since we create this hint even when HintDist is set to None, need to make sure we
        // fully respect that regardless of what the Dungeon ER hint setting might happen to be set
        // to.

        public static RequiredDungeonsHint Create(HintGenData genData)
        {
            return new RequiredDungeonsHint(
                (byte)Randomizer.RequiredDungeons,
                genData.sSettings.shuffleDungeonEntrances,
                genData.sSettings.barrenDungeons,
                genData.dungeonEntrances
            );
        }

        private RequiredDungeonsHint(
            byte requiredDungeons,
            DungeonER shuffleDungeonEntrances,
            bool barrenDungeons,
            Dictionary<Zone, List<Zone>> dungeonEntrances
        )
        {
            this.requiredDungeons = requiredDungeons;
            this.shuffleDungeonEntrances = shuffleDungeonEntrances;
            this.barrenDungeons = barrenDungeons;
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

            // Midna's first text already indicates if there are no required dungeons, so only
            // include more text here if there are dungeons to list.
            if (requiredDungeonZones.Count > 0)
            {
                HintText reqDungeonsHintText = new();
                reqDungeonsHintText.text = GenLinkHouseSignText();
                hintTexts.Add(reqDungeonsHintText);
            }

            // Only hint dungeon entrances if dungeon ER is enabled.
            if (shuffleDungeonEntrances != DungeonER.Off)
            {
                HintText dungeonErHintText = new();
                dungeonErHintText.text = testGetDungeonEntranceHint();
                hintTexts.Add(dungeonErHintText);
            }

            // string normalizedText = Res.LangSpecificNormalize(text);

            // HintText hintText = new HintText();
            // hintText.text = normalizedText;
            // return new List<HintText> { hintText };
            return hintTexts;
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
            // TODO: leading Midna text "Here's what we know about dungeon entrances:\n(Entrance => Dungeon)

            List<KeyValuePair<Zone, List<Zone>>> filteredList = getDungeonEntrancesToHint();

            // TODO: abbrev should come from translation files.
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

            string rightArrow = CustomMessages.thinRightArrow;
            string divider = Res.IsCultureJa() ? "、" : ", ";
            string space = Res.IsCultureJa() ? "　" : " ";

            string result = "";
            foreach (KeyValuePair<Zone, List<Zone>> pair in filteredList)
            {
                if (result.Length > 0)
                    result += "\n";

                string val = "";
                foreach (Zone zone in pair.Value)
                {
                    if (val.Length > 0)
                        val += divider;
                    val += zoneToAbbrev[zone];
                }

                result += $"{zoneToAbbrev[pair.Key]}{space}{rightArrow}{space}{val}";
            }
            return result;
        }

        private List<KeyValuePair<Zone, List<Zone>>> getDungeonEntrancesToHint()
        {
            // Determine which dungeons we care about.
            HashSet<Zone> interestedDungeonsSet;
            if (barrenDungeons)
                interestedDungeonsSet = new(requiredDungeonZones);
            else
                interestedDungeonsSet = new()
                {
                    Zone.Forest_Temple,
                    Zone.Goron_Mines,
                    Zone.Lakebed_Temple,
                    Zone.Arbiters_Grounds,
                    Zone.Snowpeak_Ruins,
                    Zone.Temple_of_Time,
                    Zone.City_in_the_Sky,
                    Zone.Palace_of_Twilight,
                };

            if (shuffleDungeonEntrances == DungeonER.Dungeon_Hyrule)
                interestedDungeonsSet.Add(Zone.Hyrule_Castle);

            // Filter to all entrances which lead to a dungeon we care about.
            List<KeyValuePair<Zone, List<Zone>>> filteredList = new();
            foreach (KeyValuePair<Zone, List<Zone>> pair in dungeonEntrances)
            {
                foreach (Zone pointedToZone in pair.Value)
                {
                    if (interestedDungeonsSet.Contains(pointedToZone))
                    {
                        // If entrance leads to List of size "> 1" (i.e., SPR doors), then merge
                        // down to a single target zone to list if all zones in the list are the
                        // same zone (i.e., both doors lead to the same zone). This is so we don't
                        // get something like "SPR => GM, GM" and instead simply get "SPR => GM".
                        if (pair.Value.Count > 1)
                        {
                            HashSet<Zone> uniqueTargetZones = new(pair.Value);
                            if (uniqueTargetZones.Count == 1)
                                filteredList.Add(new(pair.Key, new(uniqueTargetZones)));
                            else
                                filteredList.Add(pair);
                        }
                        else
                            filteredList.Add(pair);
                        break;
                    }
                }
            }

            // Should be sorted, but go ahead and sort since we want to guarantee order.
            return filteredList.OrderBy((el) => el.Key).ToList();
        }

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);

            result += SettingsEncoder.EncodeNumAsBits(requiredDungeons, 8);
            result += SettingsEncoder.EncodeNumAsBits((byte)shuffleDungeonEntrances, 2);
            result += barrenDungeons ? "1" : "0";

            // Encode dungeonEntrances Dictionary
            result += SettingsEncoder.EncodeAsVlq16((ushort)dungeonEntrances.Count);
            foreach (KeyValuePair<Zone, List<Zone>> pair in dungeonEntrances)
            {
                Zone zoneKey = pair.Key;
                List<Zone> destinationZones = pair.Value;

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
            Dictionary<Zone, List<Zone>> dungeonEntrances = new();

            byte requiredDungeons = processor.NextByte();
            DungeonER shuffleDungeonEntrances = (DungeonER)processor.NextInt(2);
            bool barrenDungeons = processor.NextBool();

            int numDictEntries = processor.NextVlq16();
            for (int i = 0; i < numDictEntries; i++)
            {
                Zone zoneKey = (Zone)processor.NextInt(bitLengths.zoneId);

                List<Zone> value = new();
                dungeonEntrances[zoneKey] = value;

                int numValueEntries = processor.NextVlq16();
                for (int setEntryIdx = 0; setEntryIdx < numValueEntries; setEntryIdx++)
                {
                    Zone valueZone = (Zone)processor.NextInt(bitLengths.zoneId);
                    value.Add(valueZone);
                }
            }

            return new RequiredDungeonsHint(
                requiredDungeons,
                shuffleDungeonEntrances,
                barrenDungeons,
                dungeonEntrances
            );
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
        }
    }
}
