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
    using SSettings.Enums;

    public class CustomMsgData
    {
        // Increment this when we need to change something about encoding and
        // decoding the data.
        private static readonly ushort latestEncodingVersion = 0;

        private byte requiredDungeons;
        private bool updateShopText;

        // checkName to useDefArticle
        private Dictionary<string, bool> selfHinterChecks;
        private List<HintSpot> hintSpots;

        // private Dictionary<string, Status> checkToStatus;
        private List<MessageEntry> results = new();
        private SharedSettings sSettings;

        private CustomMsgData(SharedSettings sSettings)
        {
            this.sSettings = sSettings;
        }

        private CustomMsgData(Builder builder, SharedSettings sSettings)
        {
            requiredDungeons = builder.requiredDungeons;
            updateShopText = builder.updateShopText;
            selfHinterChecks = builder.GetSelfHinterChecks();
            hintSpots = builder.hintSpots;

            this.sSettings = sSettings;
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
                foreach (KeyValuePair<string, bool> pair in selfHinterChecks)
                {
                    string checkName = pair.Key;
                    bool useDefArticle = pair.Value;

                    result += SettingsEncoder.EncodeNumAsBits(
                        CheckIdClass.GetCheckIdNum(checkName),
                        bitLengths.checkId
                    );
                    result += useDefArticle ? "1" : "0";
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
            SharedSettings sSettings,
            Dictionary<int, byte> itemPlacements,
            string sixCharString
        )
        {
            if (sixCharString == null)
                return null;

            CustomMsgData inst = new CustomMsgData(sSettings);

            BitsProcessor processor = new BitsProcessor(
                SettingsEncoder.DecodeToBitString(sixCharString)
            );

            // Once we actually need to start using the version, we can start
            // passing it to the Hint types, etc. as needed.
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

                bool useDefArticle = processor.NextBool();

                inst.selfHinterChecks[checkName] = useDefArticle;
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
            private HintGenData genData;
            public byte requiredDungeons { get; private set; }
            public bool updateShopText { get; private set; } = true;
            private bool forceNotUpdateShopText = false;
            private HashSet<string> selfHinterChecks =
                new() { "Barnes Bomb Bag", "Charlo Donation Blessing", "Fishing Hole Bottle" };
            public List<HintSpot> hintSpots { get; private set; } = new();

            public Builder(HintGenData genData, byte requiredDungeons)
            {
                this.genData = genData;
                this.requiredDungeons = requiredDungeons;
                if (!genData.sSettings.modifyShopModels)
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

            public Dictionary<string, bool> GetSelfHinterChecks()
            {
                Dictionary<string, bool> ret = new();
                foreach (string checkName in selfHinterChecks)
                {
                    Item item = HintUtils.getCheckContents(checkName);
                    bool useDefArticle = genData.ItemUsesDefArticle(item);

                    ret[checkName] = useDefArticle;
                }
                return ret;
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

            public CustomMsgData Build(SharedSettings sSettings)
            {
                return new(this, sSettings);
            }
        }

        public List<MessageEntry> GenMessageEntries()
        {
            // We store results as a property so we do not need to pass it around.
            results = new();

            // There are some static things that should always be applied which
            // do not depend on the item.
            GenStaticEntries(results);
            GenLinkHouseSignText(results);

            // handle shop text first
            if (updateShopText)
            {
                GenShopEntries();
            }

            GenSelfHinterEntries();

            // handle self-hinters next
            // results.Add(
            //     new MessageEntry
            //     {
            //         stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //         roomIDX = 1,
            //         messageID = 0x9B, // Barnes Bomb Bag Text.
            //         message =
            //             "I've got a special offer goin' right\nnow: my "
            //             // + getShortenedItemName(Randomizer.Checks.CheckDict["Barnes Bomb Bag"].itemId)
            //             + Randomizer.Checks.CheckDict["Barnes Bomb Bag"].itemId
            //             + CustomMessages.messageColorWhite
            //             + ", just\n"
            //             + CustomMessages.messageColorPurple
            //             + "120 Rupees"
            //             + CustomMessages.messageColorWhite
            //             + "! How 'bout that?"
            //             + CustomMessages.shopOption
            //     }
            // );

            // Handle custom hint signs
            GenHintSignEntries(results);

            List<MessageEntry> ret = results;
            results = null;
            return ret;
        }

        private void GenStaticEntries(List<MessageEntry> results)
        {
            // ----- Sera Shop -----

            Item seraSlingshotItem = updateShopText
                ? HintUtils.getCheckContents("Sera Shop Slingshot")
                : Item.Slingshot;
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Sera_Slingshot_Bought,
                    GenShopBoughtText(seraSlingshotItem, "sera")
                )
            );
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Sera_Slingshot_Bought_2,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.bought-sera2", null))
                )
            );

            // ----- Kakariko Malo Mart -----

            // Note that the Hawkeye soldOut sign is used as a comingSoon sign.
            // We only show the itemName on the sign if updateShopText is true.
            Res.Result hawkeyeSoldOutRes = Res.Msg(
                "shop.coming-soon",
                new() { { "context", updateShopText ? "item" : "" } }
            );
            Item hawkeyeItem = updateShopText
                ? HintUtils.getCheckContents("Kakariko Village Malo Mart Hawkeye")
                : Item.Hawkeye;
            if (HintUtils.IsTrapItem(hawkeyeItem))
                hawkeyeItem = Item.Hawkeye;
            string hawkeyeItemText = GenItemText3(
                out _,
                hawkeyeItem,
                CheckStatus.Unknown,
                prefStartColor: "",
                prefEndColor: "",
                capitalize: true
            );
            string hawkeyeSoldOutMsg = Res.LangSpecificNormalize(
                CustomMessages.messageColorOrange
                    + hawkeyeSoldOutRes.Substitute(new() { { "item", hawkeyeItemText } })
            );
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Sold_Out,
                    hawkeyeSoldOutMsg
                )
            );
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Sold_Out_Read,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.coming-soon-read", null))
                )
            );

            // This is used for the sold out sign for all slots in this shop.
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Sold_Out,
                    Res.LangSpecificNormalize(
                        CustomMessages.messageColorOrange + Res.SimpleMsg("shop.sold-out", null)
                    )
                )
            );
            // When you read the sold out sign
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Sold_Out_Read,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.sold-out-read", null))
                )
            );
            // If you buy the wooden shield slot before anything else, you will
            // see this one instead for that slot.
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Sold_Out_Read_2,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.sold-out-read", null))
                )
            );

            // Need to replace this one so it does not reference your bottle.
            // Replacing with the same text used for the Hylian shield.
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Bought,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.bought", null))
                )
            );

            // ----- Castle Town Malo Mart -----

            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Castle_Town_Malo_Mart_Magic_Armor_Bought,
                    Res.LangSpecificNormalize(
                        Res.SimpleMsg("shop.bought", new() { { "context", "magic-armor" } })
                    )
                )
            );

            // ----- Barnes -----

            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Barnes_Bomb_Bag_Cant_Afford,
                    Res.LangSpecificNormalize(
                        Res.SimpleMsg(
                            "shop.cant-afford",
                            new() { { "context", "barnes-bomb-bag" } }
                        )
                    )
                )
            );
        }

        private void AddShopConfirmationMsg(
            MsgEntryId msgEntryId,
            string checkName,
            Item defaultItem,
            uint price,
            string context = null
        )
        {
            Res.Result result = Res.Msg("shop.confirmation", new() { { "context", context } });

            Item item = HintUtils.getCheckContents(checkName);
            if (HintUtils.IsTrapItem(item))
                item = defaultItem;

            string itemText = GenItemText3(
                out Dictionary<string, string> itemMeta,
                item,
                CheckStatus.Unknown,
                contextIn: "def",
                capitalize: true,
                prefStartColor: CustomMessages.messageColorOrange
            );

            string verb = GenVerb(result, itemMeta);
            string theArticle = Res.Msg("noun.the-article", null, itemMeta).Substitute(null);
            string priceText = GenShopPriceText(price);

            string text = result.Substitute(
                new()
                {
                    { "item", itemText },
                    { "verb", verb },
                    { "price", priceText },
                    { "the-article", theArticle },
                    { "the-article2", theArticle }
                }
            );
            string normalizedText = Res.LangSpecificNormalize(text) + CustomMessages.shopOption;

            results.Add(CustomMsgUtils.GetEntry(msgEntryId, normalizedText));
        }

        private void AddShopCantAffordMsg(
            MsgEntryId msgEntryId,
            string checkName,
            Item defaultItem,
            uint price,
            string context = null
        )
        {
            Res.Result result = Res.Msg("shop.cant-afford", new() { { "context", context } });

            Item item = HintUtils.getCheckContents(checkName);
            if (HintUtils.IsTrapItem(item))
                item = defaultItem;

            string itemText = GenItemText3(
                out Dictionary<string, string> itemMeta,
                item,
                CheckStatus.Unknown,
                contextIn: "def",
                capitalize: true,
                prefStartColor: CustomMessages.messageColorOrange
            );

            string verb = GenVerb(result, itemMeta);
            string theArticle = Res.Msg("noun.the-article", null, itemMeta).Substitute(null);
            string priceText = GenShopPriceText(price);
            string price2Text = GenShopPriceText(price, false);

            string text = result.Substitute(
                new()
                {
                    { "item", itemText },
                    { "verb", verb },
                    { "price", priceText },
                    { "price2", price2Text },
                    { "the-article", theArticle },
                    { "the-article2", theArticle }
                }
            );
            string normalizedText = Res.LangSpecificNormalize(text);

            results.Add(CustomMsgUtils.GetEntry(msgEntryId, normalizedText));
        }

        private string GenShopBoughtText(Item item, string context)
        {
            Res.Result result = Res.Msg("shop.bought", new() { { "context", context } });

            string itemText = GenItemText3(
                out Dictionary<string, string> itemMeta,
                item,
                CheckStatus.Unknown,
                contextIn: "def"
            );

            string theArticle = Res.Msg("noun.the-article", null, itemMeta).Substitute(null);

            string text = result.Substitute(
                new() { { "item", itemText }, { "the-article", theArticle }, }
            );

            return Res.LangSpecificNormalize(text);
        }

        private string GenShopSoldOutText(Item item, string context)
        {
            Res.Result result = Res.Msg("shop.sold-out", new() { { "context", context } });

            string itemText = GenItemText3(
                out Dictionary<string, string> meta,
                item,
                CheckStatus.Unknown,
                isShop: true,
                includeShopSuffix: false
            );

            string text = result.Substitute(new() { { "item", itemText } });
            return Res.LangSpecificNormalize(text);
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

        private void GenSelfHinterEntries()
        {
            // For Charlo, we still need to use custom text even if disabled in
            // order to update the "Donate 100", "Donate 50" text.
            string charloText;
            if (
                selfHinterChecks.TryGetValue(
                    "Charlo Donation Blessing",
                    out bool charloUseDefArticle
                )
            )
            {
                Item item = HintUtils.getCheckContents("Charlo Donation Blessing");
                if (HintUtils.IsTrapItem(item))
                    item = Item.Piece_of_Heart;

                string itemText = GenItemText3(
                    out _,
                    item,
                    CheckStatus.Unknown,
                    contextIn: charloUseDefArticle ? "def" : "indef"
                );

                charloText = Res.LangSpecificNormalize(
                    Res.SimpleMsg("self-hinter.charlo", new() { { "item", itemText } })
                );
            }
            else
            {
                charloText = Res.LangSpecificNormalize(
                    Res.SimpleMsg("self-hinter.charlo", new() { { "context", "default" } }),
                    addLineBreaks: false
                );
            }
            // Specifically do not want to normalize this part.
            charloText += Res.SimpleMsg("self-hinter.charlo-options", null);
            results.Add(
                CustomMsgUtils.GetEntry(MsgEntryId.Charlo_Donation_Confirmation, charloText)
            );

            if (selfHinterChecks.ContainsKey("Fishing Hole Bottle"))
            {
                Item fishingBottleItem = HintUtils.getCheckContents("Fishing Hole Bottle");
                if (HintUtils.IsTrapItem(fishingBottleItem))
                    fishingBottleItem = Item.Empty_Bottle;

                string fishingBottleItemText = GenItemText3(
                    out _,
                    fishingBottleItem,
                    CheckStatus.Unknown,
                    contextIn: "fishing-bottle"
                );
                Res.Result fishingBottleRes = Res.Msg("self-hinter.fishing-bottle", null);
                string fishingBottleText = fishingBottleRes.Substitute(
                    new() { { "item", fishingBottleItemText } }
                );

                results.Add(
                    CustomMsgUtils.GetEntry(
                        MsgEntryId.Fishing_Hole_Bottle_Sign,
                        Res.LangSpecificNormalize(fishingBottleText, 30)
                    )
                );
            }
        }

        private void GenShopEntries()
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

            // AreaId areaId = AreaId.Category(HintCategory.Golden_Wolf);
            // AreaId areaId = AreaId.Category(HintCategory.Grotto);

            // // TradeChainHint tcHint = TradeChainHint.Create(
            // //     null,
            // //     "Bridge of Eldin Owl Statue Sky Character",
            // //     false,
            // //     true,
            // //     // areaId.type == AreaId.AreaType.Province
            // //     //   ? TradeChainHint.AreaType.Province :
            // //     TradeChainHint.AreaType.Zone,
            // //     TradeChainHint.RewardVagueness.Named,
            // //     TradeChainHint.RewardStatus.Good // This can be auto-calculated? Just specify display type?
            // // );
            // // string tcHintText = tcHint.toHintTextList()[0].text;


            // List<Hint> hints = new();
            // foreach (HintSpot hintSpot in hintSpots)
            // {
            //     if (hintSpot.location == SpotId.Ordon_Sign)
            //     {
            //         hints = hintSpot.hints;
            //     }
            // }

            // // List<Hint> hints =
            // //     new()
            // //     {
            // //         itipHint,
            // //         pathHint,
            // //         // itemHint,
            // //         niiaHint,
            // //         wothHint,
            // //         barrenHint,
            // //     };

            // StringBuilder sb = new();

            // for (int i = 0; i < hints.Count; i++)
            // {
            //     Hint hint = hints[i];

            //     List<HintText> hintTextList = hint.toHintTextList();
            //     for (int j = 0; j < hintTextList.Count; j++)
            //     {
            //         HintText hintText = hintTextList[j];
            //         string text = hintText.text;
            //         if (i < hints.Count - 1 || j < hintTextList.Count - 1)
            //             text = Res.NormalizeForMergingOnSign(text);

            //         sb.Append(text);
            //     }
            // }

            // string textForSign = sb.ToString();

            // results.Add(
            //     CustomMsgUtils.GetEntry(
            //         // MsgEntryId.Sera_Slingshot_Slot,
            //         MsgEntryId.Custom_Sign_Ordon,
            //         // itemHintText
            //         // GenBasicShopMsg("Sera Shop Slingshot", 30, true)
            //         textForSign
            //     // GenBasicShopMsg("Lake Lantern Cave Twelfth Chest", 30, true)
            //     )
            // );

            // Actual function content:

            // ----- Sera Shop -----

            uint seraSlingshotPrice = 30;
            AddShopSlotMsg(
                MsgEntryId.Sera_Slingshot_Slot,
                "Sera Shop Slingshot",
                Item.Slingshot,
                seraSlingshotPrice
            );
            AddShopCantAffordMsg(
                MsgEntryId.Sera_Slingshot_Cant_Afford,
                "Sera Shop Slingshot",
                Item.Slingshot,
                seraSlingshotPrice
            );
            AddShopConfirmationMsg(
                MsgEntryId.Sera_Slingshot_Confirmation,
                "Sera Shop Slingshot",
                Item.Slingshot,
                30,
                "sera"
            );

            // ----- Kakariko Malo Mart -----

            uint kakMaloHawkeyePrice = 100;
            AddShopSlotMsg(
                MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Slot,
                "Kakariko Village Malo Mart Hawkeye",
                Item.Hawkeye,
                kakMaloHawkeyePrice
            );
            AddShopCantAffordMsg(
                MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Cant_Afford,
                "Kakariko Village Malo Mart Hawkeye",
                Item.Hawkeye,
                kakMaloHawkeyePrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Confirmation,
                "Kakariko Village Malo Mart Hawkeye",
                Item.Hawkeye,
                kakMaloHawkeyePrice,
                "kak-malo"
            );

            uint kakMaloWoodenShieldPrice = 50;
            AddShopSlotMsg(
                MsgEntryId.Kakariko_Malo_Mart_Wooden_Shield_Slot,
                "Kakariko Village Malo Mart Wooden Shield",
                Item.Wooden_Shield,
                kakMaloWoodenShieldPrice
            );
            AddShopCantAffordMsg(
                MsgEntryId.Kakariko_Malo_Mart_Wooden_Shield_Cant_Afford,
                "Kakariko Village Malo Mart Wooden Shield",
                Item.Wooden_Shield,
                kakMaloWoodenShieldPrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                MsgEntryId.Kakariko_Malo_Mart_Wooden_Shield_Confirmation,
                "Kakariko Village Malo Mart Wooden Shield",
                Item.Wooden_Shield,
                kakMaloWoodenShieldPrice,
                "kak-malo"
            );

            uint kakMaloHylianShieldPrice = 200;
            AddShopSlotMsg(
                MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Slot,
                "Kakariko Village Malo Mart Hylian Shield",
                Item.Hylian_Shield,
                kakMaloHylianShieldPrice
            );
            AddShopCantAffordMsg(
                MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Cant_Afford,
                "Kakariko Village Malo Mart Hylian Shield",
                Item.Hylian_Shield,
                kakMaloHylianShieldPrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Confirmation,
                "Kakariko Village Malo Mart Hylian Shield",
                Item.Hylian_Shield,
                kakMaloHylianShieldPrice,
                "kak-malo"
            );

            uint kakMaloRedPotionPrice = 30;
            AddShopSlotMsg(
                MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Slot,
                "Kakariko Village Malo Mart Red Potion",
                Item.Red_Potion_Shop,
                kakMaloRedPotionPrice
            );
            AddShopCantAffordMsg(
                MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Cant_Afford,
                "Kakariko Village Malo Mart Red Potion",
                Item.Red_Potion_Shop,
                kakMaloRedPotionPrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Confirmation,
                "Kakariko Village Malo Mart Red Potion",
                Item.Red_Potion_Shop,
                kakMaloRedPotionPrice,
                "kak-malo"
            );

            // ----- Castle Town Malo Mart -----

            AddShopSlotMsg(
                MsgEntryId.Chudleys_Fine_Goods_Magic_Armor_Slot,
                "Castle Town Malo Mart Magic Armor",
                Item.Magic_Armor,
                598,
                "chudley"
            );

            AddShopSlotMsg(
                MsgEntryId.Castle_Town_Malo_Mart_Magic_Armor_Slot,
                "Castle Town Malo Mart Magic Armor",
                Item.Magic_Armor,
                598
            );
            results.Add(
                CustomMsgUtils.GetEntry(
                    MsgEntryId.Castle_Town_Malo_Mart_Magic_Armor_Sold_Out,
                    GenShopSoldOutText(
                        HintUtils.getCheckContents("Castle Town Malo Mart Magic Armor"),
                        "magic-armor"
                    )
                )
            );

            // ----- Barnes -----

            if (selfHinterChecks.TryGetValue("Barnes Bomb Bag", out bool barnesUseDefArticle))
            {
                Item item = HintUtils.getCheckContents("Barnes Bomb Bag");
                if (HintUtils.IsTrapItem(item))
                    item = Item.Filled_Bomb_Bag;

                string itemText = GenItemText3(
                    out _,
                    item,
                    CheckStatus.Unknown,
                    barnesUseDefArticle ? "def" : "indef",
                    prefStartColor: CustomMessages.messageColorOrange
                );

                string priceText = GenShopPriceText(120);

                Res.Result res = Res.Msg("self-hinter.barnes-bomb-bag", null);
                string text = res.Substitute(
                    new() { { "item", itemText }, { "price", priceText } }
                );

                results.Add(
                    CustomMsgUtils.GetEntry(
                        MsgEntryId.Barnes_Bomb_Bag_Confirmation,
                        Res.LangSpecificNormalize(text) + CustomMessages.shopOption
                    )
                );
            }
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

                    string text = hint.toHintTextList(this)[0].text;
                    if (i < hints.Count - 1)
                        text = Res.NormalizeForMergingOnSign(text);

                    sb.Append(text);
                }

                string textForSign = sb.ToString();
                messageEntry.message = textForSign;

                results.Add(messageEntry);
            }

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

        private void AddShopSlotMsg(
            MsgEntryId msgEntryId,
            string checkName,
            Item defaultItem,
            uint price,
            string context = null,
            bool shopSuffixIsColon = false
        )
        {
            Res.Result res = Res.Msg("shop.slot", new() { { "context", context } });

            Item item = HintUtils.getCheckContents(checkName);
            if (HintUtils.IsTrapItem(item))
                item = defaultItem;

            string itemText = GenItemText3(
                out _,
                item,
                CheckStatus.Unknown,
                isShop: true,
                includeShopSuffix: true,
                shopSuffixIsColon: shopSuffixIsColon
            );

            string priceText = GenShopPriceText(price);

            string text = res.Substitute(new() { { "item", itemText }, { "price", priceText } });
            string normalizedText = Res.LangSpecificNormalize(text);

            results.Add(CustomMsgUtils.GetEntry(msgEntryId, normalizedText));
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

        public string GenItemText3(
            out Dictionary<string, string> meta,
            Item item,
            CheckStatus checkStatus,
            string contextIn = null,
            int? count = null,
            bool isShop = false,
            bool shopSuffixIsColon = false,
            bool includeShopSuffix = false,
            string prefStartColor = null,
            string prefEndColor = null,
            bool? capitalize = null,
            CheckStatusDisplay checkStatusDisplay = CheckStatusDisplay.None
        )
        {
            string context = isShop ? "" : contextIn;
            string countStr = count?.ToString();

            // For no-logic, any that say requiredOrNot are downgraded to
            // Automatic (not sure if goodOrNot CheckStatusDisplay is even
            // necessary). Otherwise 100% of them will say "unrequired" since
            // there is no concept of "logically required" when there is no
            // logic.
            if (
                sSettings.logicRules == LogicRules.No_Logic
                && checkStatusDisplay == CheckStatusDisplay.Required_Or_Not
            )
            {
                checkStatusDisplay = CheckStatusDisplay.Automatic;
            }

            Res.Result abc = Res.ParseVal(
                GetItemResKey(item),
                new() { { "context", context }, { "count", countStr } }
            );
            meta = abc.meta;

            if (isShop || capitalize == true)
                abc.CapitalizeFirstValidChar();

            // Pick the color
            string startColor;
            string postItemText = "";
            if (isShop)
                startColor = CustomMessages.messageColorOrange;
            else if (prefStartColor != null)
            {
                // Allow passing an empty string in.
                startColor = prefStartColor;
            }
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
            if (includeShopSuffix && isShop)
            {
                if (shopSuffixIsColon)
                    itemSuffix = ":";
                else
                    itemSuffix = " ";
            }
            if (prefEndColor != null)
                itemSuffix += prefEndColor;
            else
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

        private string GenShopPriceText(uint amount, bool includeColor = true)
        {
            string result = "";
            if (includeColor)
                result += CustomMessages.messageColorPurple;

            string shopText = Res.SimpleMsgOld(
                "shop.price",
                new() { { "count", amount.ToString(CultureInfo.InvariantCulture) } }
            );
            result += shopText;

            if (includeColor)
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

        public SortedDictionary<string, object> GetDictForSpoiler()
        {
            SortedDictionary<string, object> keyToHintInfos = new();

            foreach (HintSpot hintSpot in hintSpots)
            {
                string key = hintSpot.location.ToString();

                List<Dictionary<string, object>> hintInfos = new();
                foreach (Hint hint in hintSpot.hints)
                {
                    HintInfo hintInfo = hint.GetHintInfo();
                    if (hintInfo != null)
                        hintInfos.Add(hint.GetHintInfo().GetSpoilerDict());
                    else
                        hintInfos.Add(null);
                }
                keyToHintInfos[key] = hintInfos;
            }

            return keyToHintInfos;
        }
    }
}
