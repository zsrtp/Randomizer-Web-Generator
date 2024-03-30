namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MessageEntry = Assets.CustomMessages.MessageEntry;
    using TPRandomizer.Assets;
    using System.Linq.Expressions;
    using System.Text;
    using System.Text.RegularExpressions;

    public class CustomMsgData
    {
        // Increment this when we need to change something about encoding and
        // decoding the data.
        private static readonly ushort latestEncodingVersion = 0;

        private bool updateShopText;
        private HashSet<string> selfHinterChecks;
        private List<HintSpot> hintSpots;

        // private Dictionary<string, Status> checkToStatus;

        private CustomMsgData() { }

        private CustomMsgData(Builder builder)
        {
            updateShopText = builder.updateShopText;
            selfHinterChecks = builder.GetSelfHinterChecks();
            hintSpots = builder.hintSpots;
        }

        public string Encode()
        {
            string result = SettingsEncoder.EncodeAsVlq16(latestEncodingVersion);

            HintEncodingBitLengths bitLengths = HintUtils.GetHintEncodingBitLengths(hintSpots);
            result += bitLengths.encodeAsBits();

            // Encode updateShopText
            result += updateShopText ? "1" : "0";

            // Encode selfHinterChecks
            int numSelfHinters = selfHinterChecks != null ? selfHinterChecks.Count : 0;
            result += SettingsEncoder.EncodeAsVlq16((ushort)numSelfHinters);
            if (numSelfHinters > 0)
            {
                foreach (string checkName in selfHinterChecks)
                {
                    result += SettingsEncoder.EncodeNumAsBits(
                        CheckIdClass.GetCheckIdNum(checkName),
                        bitLengths.checkId
                    );
                }
            }

            // Encode hintSpots
            int numHintSpots = hintSpots != null ? hintSpots.Count : 0;
            result += SettingsEncoder.EncodeAsVlq16((ushort)numHintSpots);
            if (numHintSpots > 0)
            {
                foreach (HintSpot spot in hintSpots)
                {
                    if (spot == null || spot.hints.Count < 1)
                        throw new Exception("Tried to encode an invalid hint spot.");

                    result += SettingsEncoder.EncodeNumAsBits(
                        (int)spot.location,
                        bitLengths.hintSpotLocation
                    );

                    int numHints = spot.hints.Count;
                    result += SettingsEncoder.EncodeNumAsBits(numHints, bitLengths.hintsPerSpot);
                    foreach (Hint hint in spot.hints)
                    {
                        if (hint == null)
                            throw new Exception("Tried to encode null hint.");

                        result += hint.encodeAsBits(bitLengths);
                    }
                }
            }

            return SettingsEncoder.EncodeAs6BitString(result);
        }

        public static CustomMsgData Decode(
            Dictionary<int, byte> itemPlacements,
            string sixCharString
        )
        {
            if (sixCharString == null)
                return null;

            CustomMsgData inst = new CustomMsgData();

            BitsProcessor processor = new BitsProcessor(
                SettingsEncoder.DecodeToBitString(sixCharString)
            );

            ushort version = processor.NextVlq16();
            HintEncodingBitLengths bitLengths = HintEncodingBitLengths.decode(processor);

            // Decode updateShopText
            inst.updateShopText = processor.NextBool();

            // Decode selfHinterChecks
            ushort numSelfHinterChecks = processor.NextVlq16();
            inst.selfHinterChecks = new();
            for (int i = 0; i < numSelfHinterChecks; i++)
            {
                int checkId = processor.NextInt(bitLengths.checkId);
                string checkName = CheckIdClass.GetCheckName(checkId);
                inst.selfHinterChecks.Add(checkName);
            }

            // Decode hintSpots
            int numHintSpots = processor.NextVlq16();
            List<HintSpot> hintSpots = new();
            for (int hintSpotIdx = 0; hintSpotIdx < numHintSpots; hintSpotIdx++)
            {
                SpotId location = (SpotId)processor.NextInt(bitLengths.hintSpotLocation);
                HintSpot spot = new HintSpot(location);

                int numHints = processor.NextInt(bitLengths.hintsPerSpot);
                for (int hintIdx = 0; hintIdx < numHints; hintIdx++)
                {
                    Hint hint = Hint.decodeHint(bitLengths, processor, itemPlacements);
                    spot.hints.Add(hint);
                }

                hintSpots.Add(spot);
            }

            inst.hintSpots = hintSpots;

            return inst;

            // HintResults hintResults = new();

            // UInt16 version = processor.NextVlq16();

            // int hintCountBitLength = processor.NextInt(3) + 1;
            // int numHints = processor.NextVlq16();
            // int hintIndexBitLength = GetBitsNeededForNum(numHints - 1);

            // for (int i = 0; i < numHints; i++)
            // {
            //     Hint hint = processor.NextHint();
            //     hintResults.hints.Add(hint);
            // }

            // List<string> hintSpotNames = HintGenerator.GetBaseHintSpotNames(this.decodedSSettings);

            // int numHintSpots = processor.NextVlq16();
            // for (int i = 0; i < numHintSpots; i++)
            // {
            //     string name = "";
            //     if (i < hintSpotNames.Count)
            //     {
            //         name = hintSpotNames[i];
            //     }

            //     hintResults.hintSpots.Add(
            //         processor.NextHintSpot(name, hintCountBitLength, hintIndexBitLength)
            //     );
            // }

            // hintResults.midnaHintSpot = processor.NextHintSpot(
            //     "Midna",
            //     hintCountBitLength,
            //     hintIndexBitLength
            // );

            // return hintResults;

        }

        // function here for generating the MessageEntry stuff!!!!

        public class Builder
        {
            public bool updateShopText { get; private set; } = true;
            private bool forceNotUpdateShopText = false;
            private HashSet<string> selfHinterChecks =
                new() { "Barnes Bomb Bag", "Charlo Donation Blessing", "Fishing Hole Bottle" };
            public List<HintSpot> hintSpots { get; private set; } = new();

            public Builder(SharedSettings sSettings)
            {
                if (!sSettings.modifyShopModels)
                {
                    updateShopText = false;
                    forceNotUpdateShopText = true;
                }
            }

            public bool SetUpdateShopText(bool shouldUpdate)
            {
                if (forceNotUpdateShopText)
                    return false;

                updateShopText = shouldUpdate;
                return true;
            }

            public HashSet<string> GetSelfHinterChecks()
            {
                return new HashSet<string>(selfHinterChecks);
            }

            public void ApplyInvalidSelfHinters(HashSet<string> invalidSelfHinters)
            {
                if (ListUtils.isEmpty(invalidSelfHinters))
                    return;

                foreach (string str in invalidSelfHinters)
                {
                    if (str == "alias:all")
                    {
                        selfHinterChecks.Clear();
                        return;
                    }
                    else
                        selfHinterChecks.Remove(str);
                }
            }

            public void SetHintSpots(List<HintSpot> hintSpots)
            {
                this.hintSpots = hintSpots;
            }

            public CustomMsgData Build()
            {
                return new(this);
            }
        }

        public List<MessageEntry> GenMessageEntries()
        {
            List<MessageEntry> results = new();

            GenStaticEntries(results);

            // handle shop text first
            if (updateShopText)
            {
                GenShopItemText(results);
            }

            // There are some static things that should always be applied which
            // do not depend on the item.


            // handle self-hinters next

            // then handle custom hint signs

            return results;
        }

        private void GenStaticEntries(List<MessageEntry> results)
        {
            // Note: we always update these since the "can't afford" message
            // references the price.
            MessageEntry entry1 = CustomMsgUtils.GetEntry(MessageId.SeraSlingshotCantAfford);
            entry1.message = "You don't have enough money!";
            results.Add(entry1);

            MessageEntry entry = CustomMsgUtils.GetEntry(MessageId.SeraSlingshotConfirmBuy);
            entry.message = "Are you sure?" + CustomMessages.shopOption;
            results.Add(entry);
        }

        private void GenShopItemText(List<MessageEntry> results)
        {
            string abc = Res.Msg("shop.basic-slot");

            Res.ParsedRes abcd = Res.ParseVal(abc);

            Item item = Randomizer.Checks.CheckDict["Sera Shop Slingshot"].itemId;

            string itemText = GenItemText(item, abcd.other["item"]);

            string aaaaa = abcd.Substitute(new() { { "item", itemText }, { "price", "30" } });
            string bb = "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!";
            string cc = Regex.Unescape(aaaaa);

            // We know that the resource requires 'item' and 'price' because
            // those are the known params for that resource.

            // Our job is to put an item and price into each slot.

            // We need to extract any additional context from "item" and provide
            // it to the item resolution.

            MessageEntry entry = CustomMsgUtils.GetEntry(MessageId.SeraSlingshotSlot);
            // entry.message = aaaaa;
            entry.message = cc;
            // entry.message =
            //     CustomMessages.getShortenedItemName(
            //         Randomizer.Checks.CheckDict["Sera Shop Slingshot"].itemId
            //     )
            //     + ": "
            //     + CustomMessages.messageColorPurple
            //     + "30 Rupees\n"
            //     + CustomMessages.messageColorWhite
            //     + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!";
            results.Add(entry);
        }

        private string GenItemText(Item item, Dictionary<string, string> other)
        {
            bool isShopItem = false;
            if (!ListUtils.isEmpty(other))
            {
                if (other.TryGetValue("shop", out string shopVal) && shopVal == "true")
                    isShopItem = true;
            }

            string result = "";

            if (isShopItem)
                result += CustomMessages.messageColorOrange;
            else
                result += CustomMessages.messageColorRed;

            result += item;
            if (isShopItem)
                result += ":";
            result += CustomMessages.messageColorWhite;

            return result;
        }
    }
}
