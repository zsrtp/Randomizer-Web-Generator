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
    using System.Globalization;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Win32.SafeHandles;

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
            // TODO: fill out all of the itemIds for English. The name should
            // match exactly (after lowercase change) with the keys which are in
            // the Item enum. The text on the other hand should match what shows
            // in the game. We should replace the contents of the wooden chest
            // with RAM modifications for fast testing and to see if there are
            // any color overrides for that item (for example, rupee text
            // normally matches the color of the rupee; also see what happens
            // for a silver rupee). Then we need to figure out an @ function to
            // put at the end of the base resource to handle generating the
            // variations (such as 'a {cs}Clawshot{ce}', 'the {cs}Clawshot{ce}',
            // '{cs}Clawshots{ce}', 'the {cs}Clawshots{ce}').

            // TODO: need to adjust the regex for the keys to be like:
            // item.progressive_clawshot--context#count@func

            // We can define a single function at the end of a thing. If we need
            // another function, then we should either manually define the
            // resources if it is a fairly unique exception (mouse => mice; or
            // maybe some things don't get a/an or a plural, such as "Milk"
            // maybe? The function should be able to handle different params
            // within reason), or we should create different functions to handle
            // different use-cases. For example, generating the plurals/articles
            // for an English item (or really just a noun) is one func. If we
            // are doing something completely different, then we should use a
            // different function rather than trying to handle combining an
            // arbitrary amount of funcs.

            // Also should make a yarn command you can run from the root which
            // will sort the Translations resx files alphabetically for the
            // 'data' elements based on the value of the 'name' attribute.
            // Probably do this first since will need to use it. Should go under
            // packages, but the yarn command should be at the top level so it
            // is easy to use.


            // string abc = Res.Msg("shop.basic-slot");

            // Res.ParsedRes abcd = Res.ParseVal(abc);

            Res.ParsedRes abcd = Res.ParseVal("shop.basic-slot");
            // TODO: add param so can pass in "sera: true" for example. The
            // basic-slot does not default to having this be true.

            // When we get item text, it should return back the meta in addition
            // to the value with the color inserted. This is so we can tell if
            // it is masc or fem for example, so we can use that context to pick
            // the sentence. This should be general based on the language, so we
            // use all of the meta which is passed back as the context for the
            // sentence.

            ItemHint itemHint = new ItemHint(
                AreaId.Zone(Zone.Lake_Hylia),
                "Lake Hylia Dock Poe",
                // Item.Progressive_Bow,
                // Item.Lantern,
                Item.Fused_Shadow_2
            );

            itemHint.toHintTextList();

            string bbb = GenItemText2(Item.Progressive_Bow, "def", isShop: true, isSeraShop: true);

            Item item = Randomizer.Checks.CheckDict["Sera Shop Slingshot"].itemId;

            string itemText = GenItemText(item, abcd.slotMeta["item"]);
            string priceText = GenShopPriceText(30);

            string aaaaa = abcd.Substitute(new() { { "item", itemText }, { "price", priceText } });
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

        public static string BuildContextFromMeta(Dictionary<string, string> meta)
        {
            if (ListUtils.isEmpty(meta))
                return null;

            List<string> chunks = new(meta.Count);
            foreach (KeyValuePair<string, string> pair in meta)
            {
                chunks.Add(pair.Key + "-" + pair.Value);
            }
            chunks.Sort(StringComparer.Ordinal);
            return string.Join(',', chunks);
        }

        public static string GenItemText2(
            Item item,
            string contextIn,
            bool isShop = false,
            bool isSeraShop = false
        )
        {
            return GenItemText2(out _, item, contextIn, isShop, isSeraShop);
        }

        public static string GenItemText2(
            out Dictionary<string, string> meta,
            Item item,
            string contextIn,
            bool isShop = false,
            bool isSeraShop = false
        )
        {
            // Needs a way to pass the meta out.

            string context = isShop ? "" : contextIn;
            // string resKey = GetItemResKey(item, context: context);
            string resKey = GetItemResKey(item);

            Res.ParsedRes abc = Res.ParseVal(
                resKey,
                new Dictionary<string, string>() { { "context", context }, }
            );

            meta = abc.meta;

            if (isShop)
                abc.CapitalizeFirstValidChar();

            // Pick the color
            string startColor;
            if (isShop)
                startColor = CustomMessages.messageColorOrange;
            else
            {
                // TODO: shop gets the highest priority, but the preferred color
                // can be passed in which is used ahead of the default fallback
                // color.

                // TODO: should have a getDefaultColor of item func which
                // returns Red from its default case.
                startColor = CustomMessages.messageColorRed;
            }

            string itemSuffix = "";
            if (isShop)
            {
                if (isSeraShop)
                    itemSuffix += " ";
                else
                    itemSuffix += ":";
            }
            itemSuffix += CustomMessages.messageColorWhite;

            string coloredItem;

            if (isShop)
                coloredItem =
                    startColor + abc.Substitute(new() { { "cs", "" }, { "ce", "" } }) + itemSuffix;
            else if (abc.value.Contains("{cs}"))
                coloredItem = abc.Substitute(new() { { "cs", startColor }, { "ce", itemSuffix }, });
            else
                coloredItem = startColor + abc.Substitute(null) + itemSuffix;

            return coloredItem;
        }

        private string GenItemText(Item item, Dictionary<string, string> other)
        {
            bool isShopItem = GetOtherBool(other, "shop");
            bool isSeraShop = GetOtherBool(other, "sera");

            string result = "";

            // TODO: this should use the shop color as a fallback for when there
            // is no provided "prioritize this specific color".

            if (isShopItem)
                result += CustomMessages.messageColorOrange;
            else
            {
                // TODO: should have a getDefaultColor of item func which
                // returns Red from its default case.
                result += CustomMessages.messageColorRed;
            }

            // result += item;
            result += Res.Msg(GetItemResKey(item));

            if (isShopItem)
            {
                if (isSeraShop)
                    result += " ";
                else
                    result += ":";
            }
            result += CustomMessages.messageColorWhite;

            return result;
        }

        private static string GetItemResKey(
            Item item,
            string context = null,
            bool ordinal = false,
            int? count = null
        )
        {
            string ret =
                "item." + ((byte)item).ToString("x2") + "_" + item.ToString().ToLowerInvariant();
            if (!StringUtils.isEmpty(context))
                ret += "$" + context;
            if (ordinal)
                ret += "#ordinal";
            if (count != null)
                ret += "#" + count;
            return ret;
        }

        private string GenShopPriceText(uint amount)
        {
            string result = CustomMessages.messageColorPurple;

            string shopText = Res.Msg(
                "shop.price",
                new() { { "count", amount.ToString(CultureInfo.InvariantCulture) } }
            );
            result += shopText;

            result += CustomMessages.messageColorWhite;
            return result;
        }

        private bool GetOtherBool(Dictionary<string, string> other, string key)
        {
            if (ListUtils.isEmpty(other))
                return false;
            return other.TryGetValue(key, out string value) && value == "true";
        }
    }
}
