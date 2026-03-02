namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.Marshalling;
    using TPRandomizer.Assets;
    using TPRandomizer.Util;

    public class EntranceHint : Hint
    {
        public override HintType type { get; } = HintType.Entrance;

        Zone sourceZone;

        // derived but encoded
        List<Zone> destinationZones;

        public static EntranceHint CreateFromDungeonEntrance(
            HintGenData genData,
            Zone dungeonEntranceZone
        )
        {
            // Then derive the destinationZones from the entrance.
            if (
                !genData.dungeonEntrances.TryGetValue(
                    dungeonEntranceZone,
                    out HashSet<Zone> destZones
                )
            )
                throw new Exception(
                    $"Could not find destinationZones for dungeonEntranceZone '{dungeonEntranceZone}'."
                );

            return new EntranceHint(dungeonEntranceZone, new(destZones));
        }

        private EntranceHint(Zone sourceZone, List<Zone> destinationZones)
        {
            this.sourceZone = sourceZone;
            this.destinationZones = destinationZones;

            // CalcDerived(genData, itemPlacements);
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
            string destZones = "";
            foreach (Zone zone in destinationZones)
            {
                if (destZones.Length > 0)
                    destZones += " and ";
                destZones += zone;
            }

            string text = $"They say that the {sourceZone} entrance leads to {destZones}.";

            string normalizedText = Res.LangSpecificNormalize(text);

            HintText hintText = new HintText();
            hintText.text = normalizedText;
            return new List<HintText> { hintText };

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

        public override string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            string result = base.encodeAsBits(bitLengths);

            result += SettingsEncoder.EncodeNumAsBits((byte)sourceZone, bitLengths.zoneId);

            result += SettingsEncoder.EncodeAsVlq16((ushort)destinationZones.Count);
            for (int i = 0; i < destinationZones.Count; i++)
            {
                Zone destZone = destinationZones[i];
                result += SettingsEncoder.EncodeNumAsBits((byte)destZone, bitLengths.zoneId);
            }

            return result;
        }

        public static EntranceHint decode(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, int> itemPlacements
        )
        {
            List<Zone> destinationZones = new();

            Zone sourceZone = (Zone)processor.NextInt(bitLengths.zoneId);

            int numDestZones = processor.NextVlq16();
            for (int i = 0; i < numDestZones; i++)
            {
                Zone zone = (Zone)processor.NextInt(bitLengths.zoneId);
                destinationZones.Add(zone);
            }

            return new EntranceHint(sourceZone, destinationZones);
        }

        public override HintInfo GetHintInfo(CustomMsgData customMsgData)
        {
            string hintText = toHintTextList(customMsgData)[0].text;

            HintInfo hintInfo = new(hintText);

            // if (!ListUtils.isEmpty(goodChecks))
            // {
            //     foreach (string checkName in goodChecks)
            //     {
            //         hintInfo.referencedChecks.Add(checkName);
            //         hintInfo.referencedItems.Add(checkNameToContents[checkName]);
            //     }
            // }

            // if (!ListUtils.isEmpty(bigKeyChecks))
            // {
            //     foreach (string checkName in bigKeyChecks)
            //     {
            //         hintInfo.referencedChecks.Add(checkName);
            //         hintInfo.referencedItems.Add(checkNameToContents[checkName]);
            //     }
            // }

            return hintInfo;
        }
    }
}
