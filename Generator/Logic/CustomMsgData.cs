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

        private byte requiredDungeons;
        private bool updateShopText;
        private HashSet<string> selfHinterChecks;
        private List<HintSpot> hintSpots;

        // private Dictionary<string, Status> checkToStatus;

        private CustomMsgData() { }

        private CustomMsgData(Builder builder)
        {
            requiredDungeons = builder.requiredDungeons;
            updateShopText = builder.updateShopText;
            selfHinterChecks = builder.GetSelfHinterChecks();
            hintSpots = builder.hintSpots;
        }

        public string Encode()
        {
            string result = SettingsEncoder.EncodeAsVlq16(latestEncodingVersion);

            HintEncodingBitLengths bitLengths = HintUtils.GetHintEncodingBitLengths(hintSpots);
            result += bitLengths.encodeAsBits();

            // Encode required dungeons
            result += SettingsEncoder.EncodeNumAsBits(requiredDungeons, 8);

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

            // Decode requiredDungeons
            inst.requiredDungeons = processor.NextByte();

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
            public byte requiredDungeons { get; private set; }
            public bool updateShopText { get; private set; } = true;
            private bool forceNotUpdateShopText = false;
            private HashSet<string> selfHinterChecks =
                new() { "Barnes Bomb Bag", "Charlo Donation Blessing", "Fishing Hole Bottle" };
            public List<HintSpot> hintSpots { get; private set; } = new();

            public Builder(byte requiredDungeons, SharedSettings sSettings)
            {
                this.requiredDungeons = requiredDungeons;
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

            // There are some static things that should always be applied which
            // do not depend on the item.
            GenStaticEntries(results);
            GenLinkHouseSignText(results);

            // handle shop text first
            if (updateShopText)
            {
                GenShopItemEntries(results);
            }

            // handle self-hinters next
            results.Add(
                new MessageEntry
                {
                    stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
                    roomIDX = 1,
                    messageID = 0x9B, // Barnes Bomb Bag Text.
                    message =
                        "I've got a special offer goin' right\nnow: my "
                        // + getShortenedItemName(Randomizer.Checks.CheckDict["Barnes Bomb Bag"].itemId)
                        + Randomizer.Checks.CheckDict["Barnes Bomb Bag"].itemId
                        + CustomMessages.messageColorWhite
                        + ", just\n"
                        + CustomMessages.messageColorPurple
                        + "120 Rupees"
                        + CustomMessages.messageColorWhite
                        + "! How 'bout that?"
                        + CustomMessages.shopOption
                }
            );
            results.Add(
                new MessageEntry
                {
                    stageIDX = (byte)StageIDs.Castle_Town,
                    roomIDX = 2,
                    messageID = 0x355, // Charlo Donation Text.
                    message =
                        "For a "
                        // + getShortenedItemName(Randomizer.Checks.CheckDict["Charlo Donation Blessing"].itemId)
                        + Randomizer.Checks.CheckDict["Charlo Donation Blessing"].itemId
                        + CustomMessages.messageColorWhite
                        + "...\nWould you please make a donation?"
                        + CustomMessages.messageOption1
                        + "100 Rupees\n"
                        + CustomMessages.messageOption2
                        + "50 Rupees\n"
                        + CustomMessages.messageOption3
                        + "Sorry..."
                }
            );
            results.Add(
                new MessageEntry
                {
                    stageIDX = (byte)StageIDs.Fishing_Pond,
                    roomIDX = 0,
                    messageID = 0x47A, // Fishing Hole Bottle Sign
                    message =
                        "           "
                        + CustomMessages.messageColorRed
                        + "DON'T LITTER!\n"
                        + CustomMessages.messageColorWhite
                        + "Do NOT toss a "
                        // + getShortenedItemName(Randomizer.Checks.CheckDict["Fishing Hole Bottle"].itemId)
                        + Randomizer.Checks.CheckDict["Fishing Hole Bottle"].itemId
                        + CustomMessages.messageColorWhite
                        + " or\ncans here! The fish are CRYING!\n\nKeep the fishing hole clean!"
                }
            );

            // then handle custom hint signs
            GenHintSignEntries(results);

            return results;
        }

        private void GenStaticEntries(List<MessageEntry> results)
        {
            // Note: we always update these since the "can't afford" message
            // references the vanilla item.
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Sera_Slingshot_Cant_Afford,
                    // TODO: use resource
                    "You don't have enough money!"
                )
            );

            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Sera_Slingshot_Confirm_Buy,
                    // TODO: use resource (add CustomMessages.shopOption) here
                    // since always needed at end regardless of language.
                    "Are you sure?" + CustomMessages.shopOption
                )
            );
        }

        private void GenLinkHouseSignText(List<MessageEntry> results)
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
                    ("required-dungeon.palace-of-twilight", 0x80, CustomMessages.messageColorPurple)
                };

            StringBuilder sb = new();
            foreach (var tuple in dungeonData)
            {
                if ((requiredDungeons & tuple.Item2) != 0)
                {
                    if (sb.Length > 0)
                        sb.Append('\n');
                    sb.Append(Res.Msg(tuple.Item1, null).ResolveWithColor(tuple.Item3));
                }
            }

            string text;
            if (sb.Length > 0)
                text = sb.ToString();
            else
                text = Res.SimpleMsg("required-dungeon.none", null);

            string normalized = Res.LangSpecificNormalize(text);
            results.Add(CustomMsgUtils.GetEntry(MsgEntryId.Link_House_Sign, normalized));
        }

        private void GenShopItemEntries(List<MessageEntry> results)
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

            // TODO: add param so can pass in "sera: true" for example. The
            // basic-slot does not default to having this be true.

            // When we get item text, it should return back the meta in addition
            // to the value with the color inserted. This is so we can tell if
            // it is masc or fem for example, so we can use that context to pick
            // the sentence. This should be general based on the language, so we
            // use all of the meta which is passed back as the context for the
            // sentence.

            // TODO: fix the rendering of the oe char (and I'm assuming capital
            // OE) in the game (currently renders as "S").


            // We know that the resource requires 'item' and 'price' because
            // those are the known params for that resource.

            // Our job is to put an item and price into each slot.

            // We need to extract any additional context from "item" and provide
            // it to the item resolution.

            // MessageEntry entry = CustomMsgUtils.GetEntry(MessageId.SeraSlingshotSlot);
            // entry.message = GenBasicShopMsg("Sera Shop Slingshot", 30, true);
            // results.Add(entry);

            AreaId areaId = AreaId.Category(HintCategory.Underwater);

            ItemHint itemHint = ItemHint.Create(
                null,
                // "Outside Lanayru Spring Left Statue Chest",
                areaId,
                "Wooden Sword Chest"
            // "Plumm Fruit Balloon Minigame",
            // display: CheckStatusDisplay.Required_Or_Not
            );
            string itemHintText = itemHint.toHintTextList()[0].text;

            NumItemInAreaHint niiaHint = new NumItemInAreaHint(2, Item.Progressive_Sword, areaId);
            string niiaText = niiaHint.toHintTextList()[0].text;

            WothHint wothHint = new WothHint(areaId, "Wooden Sword Chest");
            string wothHintText = wothHint.toHintTextList()[0].text;

            BarrenHint barrenHint = new BarrenHint(areaId);
            string barrenHintText = barrenHint.toHintTextList()[0].text;

            TradeGroupHint tradeGroupHint = new TradeGroupHint(
                TradeGroup.Mantises,
                TradeGroupHint.Vagueness.Named,
                TradeGroupHint.Status.Bad,
                "Wooden Sword Chest"
            );
            string tradeGroupHintText = tradeGroupHint.toHintTextList()[0].text;

            List<Hint> hints = new() { itemHint, niiaHint, wothHint, barrenHint, };

            StringBuilder sb = new();

            for (int i = 0; i < hints.Count; i++)
            {
                Hint hint = hints[i];

                string text = hint.toHintTextList()[0].text;
                if (i < hints.Count - 1)
                    text = Res.NormalizeForMergingOnSign(text);

                sb.Append(text);
            }

            string textForSign = sb.ToString();

            results.Add(
                CustomMsgUtils.GetEntry(
                    // MsgEntryId.Sera_Slingshot_Slot,
                    MsgEntryId.Custom_Sign_Ordon,
                    // itemHintText
                    // GenBasicShopMsg("Sera Shop Slingshot", 30, true)
                    textForSign
                // GenBasicShopMsg("Lake Lantern Cave Twelfth Chest", 30, true)
                )
            );
        }

        private void GenHintSignEntries(List<MessageEntry> results)
        {
            if (ListUtils.isEmpty(hintSpots))
                return;

            foreach (HintSpot hintSpot in hintSpots)
            {
                List<Hint> hints = hintSpot.hints;
                MessageEntry messageEntry = CustomMsgUtils.GetEntryForSpotId(hintSpot.location);
                StringBuilder sb = new();

                for (int i = 0; i < hints.Count; i++)
                {
                    Hint hint = hints[i];

                    string text = hint.toHintTextList()[0].text;
                    if (i < hints.Count - 1)
                        text = Res.NormalizeForMergingOnSign(text);

                    sb.Append(text);
                }

                string textForSign = sb.ToString();
                messageEntry.message = textForSign;

                results.Add(messageEntry);
            }

            // ItemHint itemHint = ItemHint.Create(
            //     null,
            //     // AreaId.Zone(Zone.Kakariko_Gorge),
            //     AreaId.Category(HintCategory.Grotto),
            //     // AreaId.Zone(Zone.North_Eldin),
            //     // AreaId.Zone(Zone.Lake_Hylia),
            //     "Lake Hylia Dock Poe"
            // );
            // string itemHintText = itemHint.toHintTextList()[0].text;

            BarrenHint barrenHint = new BarrenHint(AreaId.Category(HintCategory.Grotto));
            string bht = barrenHint.toHintTextList()[0].text;

            NumItemInAreaHint hhint = new NumItemInAreaHint(
                0,
                Item.Foolish_Item_3,
                // Item.Hyrule_Castle_Small_Key,
                // Item.Progressive_Bow,
                // AreaId.Province(Province.Dungeon)
                AreaId.Category(HintCategory.Grotto)
            );
            string niiaHintText = hhint.toHintTextList()[0].text;

            WothHint wothHint = new WothHint(
                // AreaId.Zone(Zone.Kakariko_Gorge),
                // AreaId.Category(HintCategory.Grotto),
                // AreaId.Province(Province.Dungeon),
                AreaId.Category(HintCategory.Grotto),
                "Lake Hylia Dock Poe"
            );
            string wothHintText = wothHint.toHintTextList()[0].text;

            results.Add(
                CustomMsgUtils.GetEntry(MsgEntryId.Custom_Sign_Fallback, "...")
            // new MessageEntry
            // {
            //     stageIDX = 0xFF,
            //     roomIDX = 0xFF,
            //     messageID = 0x1369, // Hint Message
            //     // message = itemHintText
            //     // message = wothHintText
            //     // message = tradeGroupHintText
            //     message = "abc"
            //     // message = bht
            // }
            );
        }

        private string GenBasicShopMsg(string checkName, uint price, bool isSeraShop = false)
        {
            Res.Result abcd = Res.ParseVal("shop.basic-slot");

            string itemText = GenItemText(
                HintUtils.getCheckContents(checkName),
                isShop: true,
                isSeraShop: isSeraShop
            );

            string priceText = GenShopPriceText(price);

            string aaaaa = abcd.Substitute(new() { { "item", itemText }, { "price", priceText } });
            string cc = Regex.Unescape(aaaaa);
            string dd = Res.LangSpecificNormalize(cc);
            return dd;
        }

        public static string BuildContextFromMeta(Dictionary<string, string> meta)
        {
            if (ListUtils.isEmpty(meta))
                return null;

            List<string> chunks = new(meta.Count);
            foreach (KeyValuePair<string, string> pair in meta)
            {
                if (pair.Value == "true")
                    chunks.Add(pair.Key);
                else
                    chunks.Add(pair.Key + "-" + pair.Value);
            }
            chunks.Sort(StringComparer.Ordinal);
            return string.Join(',', chunks);
        }

        public static string BuildContextWithMeta(
            HashSet<string> chunksIn,
            Dictionary<string, string> meta
        )
        {
            HashSet<string> chunks;
            if (!ListUtils.isEmpty(chunksIn))
                chunks = new(chunksIn);
            else
                chunks = new();

            if (!ListUtils.isEmpty(meta))
            {
                foreach (KeyValuePair<string, string> pair in meta)
                {
                    if (pair.Value == "true")
                        chunks.Add(pair.Key);
                    else
                        chunks.Add(pair.Key + "-" + pair.Value);
                }
            }

            chunks.ToList().Sort(StringComparer.Ordinal);
            return string.Join(',', chunks);
        }

        public static string GenItemText(
            Item item,
            string contextIn = null,
            int? count = null,
            bool isShop = false,
            bool isSeraShop = false,
            string prefStartColor = null
        )
        {
            return GenItemText(out _, item, contextIn, count, isShop, isSeraShop, prefStartColor);
        }

        public static string GenItemText(
            out Dictionary<string, string> meta,
            Item item,
            string contextIn = null,
            int? count = null,
            bool isShop = false,
            bool isSeraShop = false,
            string prefStartColor = null
        )
        {
            string context = isShop ? "" : contextIn;
            string countStr = count?.ToString();

            Res.Result abc = Res.ParseVal(
                GetItemResKey(item),
                new() { { "context", context }, { "count", countStr } }
            );
            meta = abc.meta;

            if (isShop)
                abc.CapitalizeFirstValidChar();

            // Pick the color
            string startColor;
            if (isShop)
                startColor = CustomMessages.messageColorOrange;
            else if (!StringUtils.isEmpty(prefStartColor))
                startColor = prefStartColor;
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
                    itemSuffix = " ";
                else
                    itemSuffix = ":";
            }
            itemSuffix += CustomMessages.messageColorWhite;

            string coloredItem;
            Dictionary<string, string> interpolation = new();
            if (count != null)
                interpolation.Add("count", countStr);

            if (isShop)
            {
                interpolation["cs"] = "";
                interpolation["ce"] = "";
                coloredItem = startColor + abc.Substitute(interpolation) + itemSuffix;
            }
            else if (abc.value.Contains("{cs}"))
            {
                interpolation["cs"] = startColor;
                interpolation["ce"] = itemSuffix;
                coloredItem = abc.Substitute(interpolation);
            }
            else
            {
                coloredItem = startColor + abc.Substitute(interpolation) + itemSuffix;
            }

            return coloredItem;
        }

        public static string GenItemText3(
            out Dictionary<string, string> meta,
            Item item,
            CheckStatus checkStatus,
            string contextIn = null,
            int? count = null,
            bool isShop = false,
            bool isSeraShop = false,
            string prefStartColor = null,
            CheckStatusDisplay checkStatusDisplay = CheckStatusDisplay.None
        )
        {
            string context = isShop ? "" : contextIn;
            string countStr = count?.ToString();

            Res.Result abc = Res.ParseVal(
                GetItemResKey(item),
                new() { { "context", context }, { "count", countStr } }
            );
            meta = abc.meta;

            if (isShop)
                abc.CapitalizeFirstValidChar();

            // Pick the color
            string startColor;
            string postItemText = "";
            if (isShop)
                startColor = CustomMessages.messageColorOrange;
            else if (!StringUtils.isEmpty(prefStartColor))
                startColor = prefStartColor;
            else
            {
                // Pick the default color here based on checkStatus and display.
                if (checkStatus == CheckStatus.Unknown)
                {
                    // If we do not know the status of the check, then display
                    // the default green.
                    startColor = CustomMessages.messageColorGreen;
                }
                else if (checkStatusDisplay == CheckStatusDisplay.Required_Or_Not)
                {
                    if (checkStatus == CheckStatus.Required)
                    {
                        startColor = CustomMessages.messageColorBlue;
                        postItemText = " " + Res.SimpleMsg("description.required-check", null);
                    }
                    else
                    {
                        postItemText = " " + Res.SimpleMsg("description.unrequired-check", null);
                        if (checkStatus == CheckStatus.Bad)
                            startColor = CustomMessages.messageColorPurple;
                        else
                            startColor = CustomMessages.messageColorGreen;
                    }
                }
                else if (checkStatusDisplay == CheckStatusDisplay.Good_Or_Not)
                {
                    if (checkStatus == CheckStatus.Bad)
                        startColor = CustomMessages.messageColorPurple;
                    else
                        startColor = CustomMessages.messageColorGreen;
                }
                else if (checkStatusDisplay == CheckStatusDisplay.Automatic)
                {
                    if (HintUtils.IsTradeItem(item))
                    {
                        if (checkStatus == CheckStatus.Bad)
                            postItemText = " " + Res.SimpleMsg("description.bad-check", null);
                        else
                            postItemText = " " + Res.SimpleMsg("description.good-check", null);
                    }

                    if (checkStatus == CheckStatus.Bad)
                        startColor = CustomMessages.messageColorPurple;
                    else
                        startColor = CustomMessages.messageColorGreen;
                }
                else
                {
                    // Display the default green.
                    startColor = CustomMessages.messageColorGreen;
                }
            }

            string itemSuffix = "";
            if (isShop)
            {
                if (isSeraShop)
                    itemSuffix = " ";
                else
                    itemSuffix = ":";
            }
            itemSuffix += CustomMessages.messageColorWhite;
            if (!StringUtils.isEmpty(postItemText))
                itemSuffix += postItemText;

            string coloredItem;
            Dictionary<string, string> interpolation = new();
            if (count != null)
                interpolation.Add("count", countStr);

            if (isShop)
            {
                interpolation["cs"] = "";
                interpolation["ce"] = "";
                coloredItem = startColor + abc.Substitute(interpolation) + itemSuffix;
            }
            else if (abc.value.Contains("{cs}"))
            {
                interpolation["cs"] = startColor;
                interpolation["ce"] = itemSuffix;
                coloredItem = abc.Substitute(interpolation);
            }
            else
            {
                coloredItem = startColor + abc.Substitute(interpolation) + itemSuffix;
            }

            return coloredItem;
        }

        private static string GetItemResKey(Item item)
        {
            return "item." + ((byte)item).ToString("x2") + "-" + item.ToString().ToLowerInvariant();
        }

        private string GenShopPriceText(uint amount)
        {
            string result = CustomMessages.messageColorPurple;

            string shopText = Res.SimpleMsgOld(
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

        public static string GenAreaPhrase(
            AreaId areaId,
            Dictionary<string, string> subjectMeta = null,
            string color = null
        )
        {
            Res.Result areaRes = Res.Msg(areaId.GenResKey(), null, subjectMeta);
            string areaString = areaRes.ResolveWithColor(color);

            if (!areaRes.meta.TryGetValue("ap", out string areaPhraseKey))
                areaPhraseKey = "default";

            Res.Result areaPhraseRes = Res.ParseVal($"area-phrase.{areaPhraseKey}");
            string areaPhrase = areaPhraseRes.Substitute(new() { { "area", areaString } });

            return areaPhrase;
        }

        public static string GenResWithSlotName(
            Res.Result hintResResult,
            string resKeyStart,
            Dictionary<string, string> subjectMeta = null,
            string startColor = ""
        )
        {
            if (StringUtils.isEmpty(startColor))
                startColor = "";

            string result = "";
            if (
                hintResResult.slotMeta.TryGetValue(resKeyStart, out Dictionary<string, string> meta)
            )
            {
                if (meta.TryGetValue("name", out string verbName))
                {
                    result = Res.Msg(resKeyStart + "." + verbName, null, subjectMeta)
                        .ResolveWithColor(startColor);
                }
            }

            return result;
        }

        public static string GenVerb(
            Res.Result hintResResult,
            Dictionary<string, string> subjectMeta = null
        )
        {
            string verb = "";
            if (hintResResult.slotMeta.TryGetValue("verb", out Dictionary<string, string> verbMeta))
            {
                if (verbMeta.TryGetValue("name", out string verbName))
                {
                    verb = Res.Msg("verb." + verbName, null, subjectMeta).Substitute(null);
                }
            }

            return verb;
        }
    }
}
