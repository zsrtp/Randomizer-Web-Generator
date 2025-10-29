namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using SSettings.Enums;
    using TPRandomizer.Assets;
    using TPRandomizer.Hints;
    using TPRandomizer.Util;
    using MessageEntry = Assets.CustomMessages.MessageEntry;

    public class CustomMsgData
    {
        // Increment this when we need to change something about encoding and
        // decoding the data.
        private static readonly ushort latestEncodingVersion = 0;
        private CtxGen ctxGen = new();

        private byte requiredDungeons;
        private bool updateShopText;

        // checkName to useDefArticle
        private Dictionary<string, SelfHinterData> selfHinterChecks;
        private List<HintSpot> hintSpots;

        // private Dictionary<string, Status> checkToStatus;
        private List<MessageEntry> results = new();
        private StringTableResult2 results2 = new();
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
                foreach (KeyValuePair<string, SelfHinterData> pair in selfHinterChecks)
                {
                    string checkName = pair.Key;
                    SelfHinterData selfHinterData = pair.Value;

                    result += SettingsEncoder.EncodeNumAsBits(
                        CheckIdClass.GetCheckIdNum(checkName),
                        bitLengths.checkId
                    );
                    result += selfHinterData.encode();
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
            Dictionary<int, int> itemPlacements,
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

                SelfHinterData selfHinterData = SelfHinterData.decode(processor);

                inst.selfHinterChecks[checkName] = selfHinterData;
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
            private List<Item> selfHinterTrapReplacements;
            public byte requiredDungeons { get; private set; }
            public bool updateShopText { get; private set; } = true;
            private bool forceNotUpdateShopText = false;
            private Dictionary<string, bool> selfHinterChecksToIsShop =
                new()
                {
                    { "Barnes Bomb Bag", true },
                    { "Charlo Donation Blessing", false },
                    { "Fishing Hole Bottle", false },
                    { "Coro Bottle", true },
                    { "Castle Town Goron Shop Red Potion", true },
                    { "Castle Town Goron Shop Lantern Oil", true },
                    { "Castle Town Goron Shop Arrow Refill", true },
                    { "Castle Town Goron Shop Hylian Shield", true },
                };
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

                prepareSelfHinterTrapReplacements();
            }

            private void prepareSelfHinterTrapReplacements()
            {
                // Based off of the models that are used in the cpp minus items
                // that people might skip such as slingshot or hawkeye.
                HashSet<Item> items =
                    new()
                    {
                        Item.Magic_Armor,
                        Item.Progressive_Sword,
                        Item.Shadow_Crystal,
                        Item.Boomerang,
                        Item.Spinner,
                        Item.Ball_and_Chain,
                        Item.Progressive_Bow,
                        Item.Progressive_Clawshot,
                        Item.Iron_Boots,
                        Item.Progressive_Fishing_Rod,
                        Item.Progressive_Dominion_Rod,
                        Item.Filled_Bomb_Bag,
                        Item.Progressive_Sky_Book,
                    };

                List<Item> itemsToPickFrom = new();
                foreach (Item item in items)
                {
                    // Only hint items that can actually be found by the player.
                    if (
                        genData.itemToChecksList.TryGetValue(item, out List<string> checkNames)
                        && !ListUtils.isEmpty(checkNames)
                    )
                        itemsToPickFrom.Add(item);
                }

                // Pick any item if none of them are better options than the others.
                if (itemsToPickFrom.Count < 1)
                    itemsToPickFrom = new(items);

                selfHinterTrapReplacements = itemsToPickFrom;
            }

            public bool SetUpdateShopText(bool shouldUpdate)
            {
                if (forceNotUpdateShopText)
                    return false;

                updateShopText = shouldUpdate;
                return true;
            }

            public Dictionary<string, SelfHinterData> GetSelfHinterChecks()
            {
                Dictionary<string, SelfHinterData> ret = new();
                foreach (KeyValuePair<string, bool> pair in selfHinterChecksToIsShop)
                {
                    string checkName = pair.Key;
                    bool isShopCheck = pair.Value;
                    if (!updateShopText && isShopCheck)
                        continue;

                    Item item = HintUtils.getCheckContents(checkName);
                    if (HintUtils.IsTrapItem(item))
                        item = HintUtils.PickRandomListItem(
                            genData.rnd,
                            selfHinterTrapReplacements
                        );

                    bool useDefArticle = genData.ItemUsesDefArticle(item);
                    SelfHinterData selfHinterData = new(useDefArticle, item);

                    ret[checkName] = selfHinterData;
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
                        selfHinterChecksToIsShop.Clear();
                        return;
                    }
                    else
                        selfHinterChecksToIsShop.Remove(str);
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

        public class SelfHinterData
        {
            public bool useDefArticle { get; }
            public Item itemToHint { get; } // might be different than actual for traps

            public SelfHinterData(bool useDefArticle, Item itemToHint)
            {
                this.useDefArticle = useDefArticle;
                this.itemToHint = itemToHint;
            }

            public string encode()
            {
                string result = useDefArticle ? "1" : "0";
                result += SettingsEncoder.EncodeNumAsBits((int)itemToHint, 8);
                return result;
            }

            public static SelfHinterData decode(BitsProcessor processor)
            {
                bool useDefArticle = processor.NextBool();
                Item itemToHint = (Item)processor.NextByte();
                return new SelfHinterData(useDefArticle, itemToHint);
            }
        }

        public List<MessageEntry> GenMessageEntries(SeedGenResults seedGenResults)
        {
            // TODO: this should return a structure.
            // It includes what it returns right now.
            // However, we need to properly handle:

            // - breaking Midna and custom sign text into their own structures.
            //   They should include "this hintSign id maps to this array of
            //   text boxes". We should handle splitting the text for a spot
            //   into the arrays here since the text manipulation is more of a
            //   concern of this file compared to the other one which is more
            //   about putting the data in the GCI.

            // We should go ahead and put the messageEntries into the same list
            // we return. However, we also need to return data which is used for
            // building the BMG section.

            // We store results as a property so we do not need to pass it around.
            results = new();

            // There are some static things that should always be applied which
            // do not depend on the item.
            GenStaticEntries(seedGenResults);
            string linkHouseSignText = GenLinkHouseSignText();
            results2.AddStrReplacement(StrRepl.Hidden(Node.msg_LinksHouseSign, linkHouseSignText));

            // handle shop text first
            if (updateShopText)
            {
                GenShopEntries();
            }

            GenSelfHinterEntries();

            // Handle custom hint signs, Agitha and Jovani signs
            GenHintSignEntries();

            List<MessageEntry> ret = results;
            results = null;

            return ret;
        }

        private void GenStaticEntries(SeedGenResults seedGenResults)
        {
            string seedName = seedGenResults.playthroughName;
            results2.AddStrReplacement(StrRepl.PublicInf(Inf.zel00_ChooseAQuestLog, seedName));
            results2.AddStrReplacement(StrRepl.PublicInf(Inf.zel00_RatioCheckSample, seedName));

            // ----- Sera Shop -----

            Item seraSlingshotItem = updateShopText
                ? HintUtils.getCheckContents("Sera Shop Slingshot")
                : Item.Slingshot;
            results2.AddStrReplacement(
                StrRepl.Hidden(
                    Node.msg_SeraSlingshotBought,
                    GenShopBoughtText(seraSlingshotItem, "sera")
                )
            );
            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_SeraSlingshotBought2,
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
            results2.AddStrReplacement(
                StrRepl.Hidden(Node.msg_KakMaloMartHawkeyeSoldOut, hawkeyeSoldOutMsg)
            );
            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_KakMaloMartHawkeyeSoldOutRead,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.coming-soon-read", null))
                )
            );

            // This is used for the sold out sign for all slots in this shop.
            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_KakMaloMartHylianShieldSoldOut,
                    Res.LangSpecificNormalize(
                        CustomMessages.messageColorOrange + Res.SimpleMsg("shop.sold-out", null)
                    )
                )
            );

            string textKvMaloMartHylianShieldSoldOutRead = Res.LangSpecificNormalize(
                Res.SimpleMsg("shop.sold-out-read", null)
            );
            // When you read the sold out sign
            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_KakMaloMartHylianShieldSoldOutRead,
                    textKvMaloMartHylianShieldSoldOutRead
                )
            );
            // If you buy the wooden shield slot before anything else, you will
            // see this one instead for that slot.
            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_KakMaloMartHylianShieldSoldOutRead2,
                    textKvMaloMartHylianShieldSoldOutRead
                )
            );

            // Need to replace this one so it does not reference your bottle.
            // Replacing with the same text used for the Hylian shield.
            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_KakMaloMartRedPotionBought,
                    Res.LangSpecificNormalize(Res.SimpleMsg("shop.bought", null))
                )
            );

            // ----- Castle Town Malo Mart -----

            results2.AddStrReplacement(
                StrRepl.Public(
                    Node.msg_CtMaloMartMagicArmorBought,
                    Res.LangSpecificNormalize(
                        Res.SimpleMsg("shop.bought", new() { { "context", "magic-armor" } })
                    )
                )
            );

            // ----- Barnes -----

            Res.Result barnesCantAffordRes = Res.Msg(
                "shop.cant-afford",
                new() { { "context", "barnes-bomb-bag" } }
            );

            // For some languages (like English), we use the default text.
            if (!barnesCantAffordRes.MetaHasVal("skip-msg", "true"))
            {
                results2.AddStrReplacement(
                    StrRepl.Public(
                        Node.msg_BarnesBombBagCantAfford,
                        Res.LangSpecificNormalize(barnesCantAffordRes.Substitute(null))
                    )
                );
            }

            AddBarnesShopAdjustments();
            // AddIzaMinigameSkipAbility();

            // ----- New Stuff -----

            // string table stuff
            string messageOption1_8not9 = "\x1A\x06\x00\x00\x08\x01";
            string messageOption2_8not9 = "\x1A\x06\x00\x00\x08\x02";
            // NOTE: ^ CRITICAL that the mesasge options match this which is
            // what is normally used for the nodes we are overwriting. The midna
            // menus have 1a 06 00 00 **09** xx when there are 3 options, but 1a
            // 06 00 00 **08** xx when there are 2 options.

            List<StrRepl> strEntries2 =
                new()
                {
                    // Fallback text
                    StrRepl.CustomSignText(
                        1, // TODO: create an enum of reserved context values probably.
                        Res.LangSpecificNormalize(Res.SimpleMsg("hint.none-placed-here"))
                    ),
                };

            results2.AddStrReplacements(strEntries2);

            List<NodeRemap> nodeRemaps =
                new()
                {
                    // Test for hylian shield price change
                    // new(StgBmg.Kakariko_Village_Interiors, 0x421, 0x421, 15, fliValue: 0x145),

                    // TODO: I think we do not need to set a context for this
                    // since we are always adjusting the existing thing that is
                    // there rather than reusing nodes in a different way that
                    // the game normally does.
                    NodeRemap.Fli(
                        0x145,
                        Node.br_KakMaloMartHylianShieldCanAfford,
                        Node.br_KakMaloMartHylianShieldCanAfford.flwIdx,
                        15
                    )
                    // Temp, test skipping over payment evNode of Hylian shield
                    // new(StgBmg.Kakariko_Village_Interiors, 0x424, 0x428, 15, context: 15),
                };

            results2.AddNodeRemaps(nodeRemaps);

            List<BranchPatchEntity> branchPatches =
                new()
                {
                    // Patch comparison to see if you can afford hylian shield
                    // Change price to 0x13b (315)
                    // new(StgBmg.Kakariko_Village_Interiors, 0x421, 15, parameters: 0x13b),

                    // Patches to use a custom queryFunction which always returns
                    // the parameters as the procResult. In this case, we are always
                    // returning 0 which means you are considered to always be able
                    // to afford the Hylian shield regardless of your current Rupee
                    // count.
                    new(
                        Node.br_KakMaloMartHylianShieldCanAfford,
                        // StgBmg.Kakariko_Village_Interiors,
                        // 0x421,
                        15,
                        queryIndex: 53,
                        parameters: 0
                    ),
                };

            results2.AddBranchPatches(branchPatches);

            results2.AddEventEntities(
                new()
                {
                    new(Node.ev_KakMaloMartHylianShieldPay, 15, intParam: 0x13b),
                    // Patch to subtract 0x13b (315) when buying Hylian Shield at Kak Malo Mart
                    // TODO: probably don't need a context for this?
                    // new(StgBmg.Kakariko_Village_Interiors, 0x424, 15, intParam: 0x13b),
                    // Here is a different version of skipping over the payment
                    // event node. This is preferable to using a nodeRemap since
                    // those take 4 bytes in their table and this only takes 2.
                    // new(StgBmg.Kakariko_Village_Interiors, 0x429, 15, nextNodeIdx: 0x428),
                    new(Node.ev_KakMaloMartHylianShieldBeforePay, 15, nextNodeIdx: 0x428),
                }
            );

            AddMidnaConversationStuff();

            // TODO: temp quiz sign for demo
            // AddTestQuizSign();
        }

        private void AddBarnesShopAdjustments()
        {
            ushort firstSlotBaseCtx = ctxGen.getNewContext();
            ushort checkBombSlotCtx = ctxGen.getNewContext();

            results2.AddNodeRemaps(
                new()
                {
                    // Set base context when selecting first slot.
                    NodeRemap.Fli(
                        0x169,
                        Node.br_BarnesBombsSlot,
                        Node.br_BarnesBombsSlot.flwIdx,
                        firstSlotBaseCtx
                    ),
                    // If remapping back to the initial branch node (because had
                    // already done the check), then set the context so we can
                    // do custom handling for branch result 0 (no bomb bags).
                    // This context is used for all 3 bomb slots when you have
                    // no bomb bags in order to show the "oh, you have no bomb
                    // bags" msg and then end the flow.
                    NodeRemap.Ctx(
                        firstSlotBaseCtx,
                        Node.br_BarnesBombsSlot,
                        Node.br_BarnesBombsSlot.flwIdx,
                        checkBombSlotCtx
                    ),
                    // Set context when entering water bomb slot selection flow.
                    NodeRemap.Fli(
                        0x178,
                        Node.br_BarnesWaterBombSlot,
                        Node.br_BarnesWaterBombSlot.flwIdx,
                        checkBombSlotCtx
                    ),
                    // Set context when entering bomblings slot selection flow.
                    NodeRemap.Fli(
                        0x185,
                        Node.br_BarnesBomblingsSlot,
                        Node.br_BarnesBomblingsSlot.flwIdx,
                        checkBombSlotCtx
                    ),
                    // Map to 0xFFFF after showing "no bomb bag" msg for the
                    // appropriate context.
                    NodeRemap.Ctx(checkBombSlotCtx, Node.ev_BarnesNoBombBagMenu, 0xFFFF, 0)
                }
            );

            List<BranchPatchEntity> branchPatches =
                new()
                {
                    new(
                        Node.br_BarnesBombsSlot,
                        firstSlotBaseCtx,
                        queryIndex: 0x1, // query001 (checks eventBit)
                        // M_044 = 0x0908, // Kakariko Village - [Barnes Bomb Shop] Bought premium pack,
                        // Found at index 0x4d (77) in `dSv_event_flag_c::saveBitLabels`
                        parameters: 0x4d,
                        nextNodeIndexes: new()
                        {
                            // Has done check, so return to same node to run
                            // vanilla query023 and proceed as normal. We change
                            // the context so that result 0 in this case says
                            // "oh, you have no bomb bag?" but does not ask you
                            // to buy the check.
                            Node.br_BarnesBombsSlot.flwIdx,
                            // Has not done check, so go directly to the menu
                            // event node. We skip over the "no bomb bag?" part
                            // since since we use this msg legitimately in other
                            // places, and it is more convenient and
                            // understandable to go directly to the menu.
                            Node.ev_BarnesNoBombBagMenu.flwIdx,
                        }
                    ),
                    // Patch water bombs and bombling slots to show "no bombs
                    // msg" when you have no bomb bags instead of resolving to
                    // 0xFFFF and showing nothing.
                    new(
                        Node.br_BarnesWaterBombSlot,
                        checkBombSlotCtx,
                        nextNodeIndexes: new()
                        {
                            Node.msg_BarnesNoBombBag.flwIdx,
                            // Vanilla results for 1 through 3:
                            0x62d,
                            0x62b,
                            0x62a
                        }
                    ),
                    new(
                        Node.br_BarnesBomblingsSlot,
                        checkBombSlotCtx,
                        nextNodeIndexes: new()
                        {
                            Node.msg_BarnesNoBombBag.flwIdx,
                            // Vanilla results for 1 through 3:
                            0x776,
                            0x774,
                            0x773
                        }
                    ),
                };

            results2.AddBranchPatches(branchPatches);
        }

        private void AddMidnaConversationStuff()
        {
            // Note: Midna voice is only guaranteed to work normally when the
            // instantText option is not enabled. Even in the vanilla game, if
            // Midna starts going through a line with text and you press A to
            // make the rest of the text instantly appear, she stops talking
            // right when you press A.

            ushort baseMidnaCtx = ctxGen.getNewContext();
            ushort hintsBaseCtx = ctxGen.getNewContext();

            string messageOption1_8not9 = "\x1A\x06\x00\x00\x08\x01";
            string messageOption2_8not9 = "\x1A\x06\x00\x00\x08\x02";

            List<string> hintMessages = new() { };

            int numReqDungeons = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((requiredDungeons & (1 << i)) != 0)
                    numReqDungeons += 1;
            }

            string midnaDungeonMsg = Res.SimpleMsg(
                "midna.required-dungeons",
                new Dictionary<string, string>() { { "count", numReqDungeons.ToString() } }
            );

            // Note: need to normalize so JP number converts to wide version.
            hintMessages.Add(Res.LangSpecificNormalize(midnaDungeonMsg));
            if (numReqDungeons > 0)
            {
                hintMessages.Add(GenLinkHouseSignText());
            }

            foreach (HintSpot hintSpot in hintSpots)
            {
                if (hintSpot.location == SpotId.Ordon_Sign)
                {
                    List<string> hintTexts = hintSpot.hints
                        .Select((hint) => hint.toHintTextList(this)[0].text)
                        .ToList();
                    hintMessages.AddRange(hintTexts);
                    break;
                }
            }

            // TODO: when calculate the correct hintMessages, use a single
            // fallback text if there is no text at all. Should never happen
            // since there will always be text about required dungeons.

            List<StrRepl> strEntries2 =
                new()
                {
                    StrRepl.Public(
                        Node.msg_MidnaTwoOptsBody,
                        Res.SimpleMsg("menu.midna-other.body") + CustomMessages.shopOption,
                        baseMidnaCtx
                    ),
                    StrRepl.Public(
                        Node.msg_MidnaTwoOptsOptions,
                        $"{messageOption1_8not9}{Res.SimpleMsg("menu.midna-other.option.change-time-of-day")}\n{messageOption2_8not9}{Res.SimpleMsg("menu.midna-other.option.hints")}",
                        baseMidnaCtx
                    ),
                };
            results2.AddStrReplacements(strEntries2);

            List<NodeRemap> nodeRemaps =
                new()
                {
                    // Start at custom branch node to decide if can change ToD or not
                    NodeRemap.Fli(
                        0xbb8,
                        Node.br_TalkToMidnaRootNode,
                        Node.br_Z0GeneriCtxBranch.flwIdx,
                        baseMidnaCtx
                    ),
                    // When we first enter the Hints text, update to a new
                    // context. The base Midna context needs 0xFFFF to not be
                    // remapped (so backing out of the menu works).
                    NodeRemap.Ctx(
                        baseMidnaCtx,
                        // Node.msgZ0_0x28,
                        // Node.msgZ0_0x28.flwIdx,
                        // Node.msgZ0_0x26,
                        // Node.msgZ0_0x26.flwIdx,
                        // Node.msgZ0_0x27,
                        // Node.msgZ0_0x27.flwIdx,
                        Node.msg_Z0_0x4d,
                        Node.msg_Z0_0x4d.flwIdx,
                        hintsBaseCtx
                    )
                };
            results2.AddNodeRemaps(nodeRemaps);

            List<BranchPatchEntity> branchPatches =
                new()
                {
                    // Check if can change ToD:
                    new(
                        Node.br_Z0GeneriCtxBranch,
                        baseMidnaCtx,
                        queryIndex: 54,
                        nextNodeIndexes: new()
                        {
                            Node.ev_MidnaTwoOptsInitEv.flwIdx,
                            // Node.msgZ0_0x28.flwIdx,
                            // Node.msgZ0_0x28.flwIdx
                            // Node.msgZ0_0x26.flwIdx,
                            // Node.msgZ0_0x26.flwIdx
                            Node.msg_Z0_0x4d.flwIdx
                            // Node.msgZ0_0x4a.flwIdx,
                            // Node.msgZ0_0x4a.flwIdx
                            // Node.msgZ0_0x27.flwIdx
                        }
                    ),
                    // Handle choice of "Change ToD / Hints" menu
                    new(
                        Node.br_MidnaTwoOptsResultBranch,
                        baseMidnaCtx,
                        nextNodeIndexes: new()
                        {
                            Node.ev_Z0GenericCtxEvent.flwIdx,
                            // Node.msgZ0_0x28.flwIdx,
                            // Node.msgZ0_0x26.flwIdx,
                            Node.msg_Z0_0x4d.flwIdx,
                            // Node.msgZ0_0x4a.flwIdx,
                            // Node.msgZ0_0x27.flwIdx,
                            0xFFFF
                        }
                    ),
                };
            results2.AddBranchPatches(branchPatches);

            // Make event change ToD
            results2.AddEventEntity(
                new(Node.ev_Z0GenericCtxEvent, baseMidnaCtx, eventIndex: 44, nextNodeIdx: 0xFFFF)
            );

            // Add Midna hint messages
            ushort latestContext = hintsBaseCtx;
            for (int i = 0; i < hintMessages.Count; i++)
            {
                string msg = hintMessages[i];

                // MsgNodeInst node;
                // if (i == 0)
                //     node = Node.msgZ0_0x26;
                // else if (i == 1)
                //     node = Node.msgZ0_0x27;
                // else
                //     node = Node.msgZ0_0x28;

                // results2.AddStrReplacement(StrRepl.Hidden(Node.msgZ0_0x28, msg, latestContext));
                results2.AddStrReplacement(StrRepl.Hidden(Node.msg_Z0_0x4d, msg, latestContext));
                // results2.AddStrReplacement(StrRepl.Hidden(node, msg, latestContext));
                // latestContext = GetNewContext();

                if (i < hintMessages.Count - 1)
                {
                    // If has more messages, map back to same node instead of
                    // continuing to 0xFFFF.
                    ushort prevCtx = latestContext;
                    latestContext = GetNewContext();

                    results2.AddNodeRemap(
                        NodeRemap.Ctx(
                            prevCtx,
                            Node.zel00_FFFF,
                            // Node.msgZ0_0x28.flwIdx,
                            Node.msg_Z0_0x4d.flwIdx,
                            latestContext
                        )
                    );
                }
            }

            string msgWarp = Res.SimpleMsg("menu.midna-base.option.warp");
            string msgTransformIntoWolf = Res.SimpleMsg(
                "menu.midna-base.option.transform-into-wolf"
            );
            string msgTransformIntoHuman = Res.SimpleMsg(
                "menu.midna-base.option.transform-into-human"
            );
            string msgSomethingElse = Res.SimpleMsg("menu.midna-base.option.something-else");

            results2.AddStrReplacements(
                new()
                {
                    StrRepl.PublicInf(
                        Inf.zel00_MidnaOpts_WarpTalk,
                        $"{CustomMessages.option1of2}{msgWarp}\n{CustomMessages.option2of2}{msgSomethingElse}"
                    ),
                    StrRepl.PublicInf(
                        Inf.zel00_MidnaOpts_TransToWolfTalk,
                        $"{CustomMessages.option1of2}{msgTransformIntoWolf}\n{CustomMessages.option2of2}{msgSomethingElse}"
                    ),
                    StrRepl.PublicInf(
                        Inf.zel00_MidnaOpts_TransToHumanTalk,
                        $"{CustomMessages.option1of2}{msgTransformIntoHuman}\n{CustomMessages.option2of2}{msgSomethingElse}"
                    ),
                    StrRepl.PublicInf(
                        Inf.zel00_MidnaOpts_TransToWolfWarpTalk,
                        $"{CustomMessages.messageOption1}{msgTransformIntoWolf}\n{CustomMessages.messageOption2}{msgWarp}\n{CustomMessages.messageOption3}{msgSomethingElse}"
                    ),
                    StrRepl.PublicInf(
                        Inf.zel00_MidnaOpts_TransToHumanWarpTalk,
                        $"{CustomMessages.messageOption1}{msgTransformIntoHuman}\n{CustomMessages.messageOption2}{msgWarp}\n{CustomMessages.messageOption3}{msgSomethingElse}"
                    )
                }
            );
        }

        // private void AddTestQuizSign()
        // {
        //     // Add Midna hint messages
        //     ushort ctx = GetNewContext();
        //     ushort endHintCtx = GetNewContext();

        //     // Start on node with ctx.
        //     results2.AddNodeRemap(
        //         NodeRemap.Fli(0x72b1, Node.zel00_FFFF, Node.evZ0_MidnaThreeOptsInitEv.flwIdx, ctx)
        //     );
        //     // Change context for end hint since needs to be unique since
        //     // sharing same INF index for all custom sign stuff.
        //     results2.AddNodeRemap(
        //         NodeRemap.Ctx(ctx, Node.msgZ0_0x4d, Node.msgZ0_0x4d.flwIdx, endHintCtx)
        //     );

        //     // Change text for body and options under this context:
        //     List<StrRepl> strEntries2 =
        //         new()
        //         {
        //             StrRepl.CustomSignText(
        //                 // Node.msgZ0_MidnaThreeOptsBody,
        //                 ctx,
        //                 Res.LangSpecificNormalize(
        //                     "How many cuccos are at Lake Hylia?" + CustomMessages.shopOption
        //                 )
        //             ),
        //             StrRepl.CustomSignOptions(
        //                 // Node.msgZ0_MidnaThreeOptsOptions,
        //                 ctx,
        //                 $"{CustomMessages.messageOption1}7\n{CustomMessages.messageOption2}8\n{CustomMessages.messageOption3}9"
        //             ),
        //             // Set custom text for correct selection.
        //             StrRepl.CustomSignText(
        //                 // Node.msgZ0_0x4d,
        //                 endHintCtx,
        //                 Res.LangSpecificNormalize("Correct!\n\n\n\nCustom hint text would go here.")
        //             ),
        //         };
        //     results2.AddStrReplacements(strEntries2);

        //     // Handle option choice
        //     List<BranchPatchEntity> branchPatches =
        //         new()
        //         {
        //             // Handle choice of "Change ToD / Hints" menu
        //             new(
        //                 Node.brZ0_MidnaThreeOptsResultBranch,
        //                 ctx,
        //                 nextNodeIndexes: new()
        //                 {
        //                     // Node.evZ0_GenericCtxEvent.flwIdx,
        //                     // Node.msgZ0_0x28.flwIdx,
        //                     // Node.msgZ0_0x26.flwIdx,
        //                     Node.msgZ0_0x4d.flwIdx,
        //                     Node.evZ0_GenericCtxEvent.flwIdx,
        //                     Node.evZ0_GenericCtxEvent.flwIdx,
        //                     0xFFFF
        //                 }
        //             ),
        //         };
        //     results2.AddBranchPatches(branchPatches);

        //     // Set custom event node to queue trap item
        //     results2.AddEventEntity(
        //         new(
        //             Node.evZ0_GenericCtxEvent,
        //             ctx,
        //             eventIndex: 44,
        //             intParam: (ushort)Item.Foolish_Item
        //         )
        //     );
        // }

        private void AddShopConfirmationMsg(
            MsgNodeInst msgNode,
            string checkName,
            Item defaultItem,
            uint price,
            string context = null,
            Dictionary<string, string> priceContextMeta = null
        )
        {
            Res.Result result = Res.Msg("shop.confirmation", new() { { "context", context } });

            Item item = HintUtils.getCheckContents(checkName);
            bool useDefArticle = true;

            // If we store info about the check in selfHinterChecks, use that.
            if (selfHinterChecks.TryGetValue(checkName, out SelfHinterData selfHinterData))
            {
                item = selfHinterData.itemToHint;
                useDefArticle = selfHinterData.useDefArticle;
            }
            else
            {
                // Not in selfHinterData (for example, a normal item behind a
                // shop counter)
                if (HintUtils.IsTrapItem(item))
                    item = defaultItem;
            }

            // Try to get "item" slotMeta. Results in null if not there.
            result.slotMeta.TryGetValue("item", out Dictionary<string, string> resultSlotMetaItem);

            string itemText = GenItemText3(
                out Dictionary<string, string> itemMeta,
                item,
                CheckStatus.Unknown,
                contextIn: useDefArticle ? "def" : "indef",
                prefStartColor: CustomMessages.messageColorOrange,
                optionalContextMetaIn: resultSlotMetaItem
            );

            string nounVal = GenNamedSlotVal(result, "noun", itemMeta);

            string verb = GenVerb(result, itemMeta);
            string priceText = GenShopPriceText(price, priceContextMeta: priceContextMeta);

            string text = result.Substitute(
                new()
                {
                    { "item", itemText },
                    { "verb", verb },
                    { "price", priceText },
                    { "noun", nounVal },
                }
            );
            string normalizedText = Res.LangSpecificNormalize(text) + CustomMessages.shopOption;

            results2.AddStrReplacement(StrRepl.Hidden(msgNode, normalizedText));
        }

        private void AddShopCantAffordMsg(
            MsgNodeInst msgNode,
            string checkName,
            Item defaultItem,
            uint price,
            string context = null
        )
        {
            Res.Result result = Res.Msg("shop.cant-afford", new() { { "context", context } });

            Item item = HintUtils.getCheckContents(checkName);
            bool useDefArticle = true;

            // If we store info about the check in selfHinterChecks, use that.
            if (selfHinterChecks.TryGetValue(checkName, out SelfHinterData selfHinterData))
            {
                item = selfHinterData.itemToHint;
                useDefArticle = selfHinterData.useDefArticle;
            }
            else
            {
                // Not in selfHinterData (for example, a normal item behind a
                // shop counter)
                if (HintUtils.IsTrapItem(item))
                    item = defaultItem;
            }

            // Try to get "item" slotMeta. Results in null if not there.
            result.slotMeta.TryGetValue("item", out Dictionary<string, string> resultSlotMetaItem);

            string itemText = GenItemText3(
                out Dictionary<string, string> itemMeta,
                item,
                CheckStatus.Unknown,
                contextIn: useDefArticle ? "def" : "indef",
                prefStartColor: CustomMessages.messageColorOrange,
                optionalContextMetaIn: resultSlotMetaItem
            );

            string verb = GenVerb(result, itemMeta);
            string nounVal = GenNamedSlotVal(result, "noun", itemMeta);
            string priceText = GenShopPriceText(price);
            string price2Text = GenShopPriceText(price, false);

            string text = result.Substitute(
                new()
                {
                    { "item", itemText },
                    { "verb", verb },
                    { "price", priceText },
                    { "price2", price2Text },
                    { "noun", nounVal },
                    { "noun2", nounVal },
                }
            );
            string normalizedText = Res.LangSpecificNormalize(text);

            results2.AddStrReplacement(StrRepl.Hidden(msgNode, normalizedText));
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

            string nounVal = GenNamedSlotVal(result, "noun", itemMeta);

            string text = result.Substitute(new() { { "item", itemText }, { "noun", nounVal } });

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

        private void GenSelfHinterEntries()
        {
            // Charlo donation
            if (
                selfHinterChecks.TryGetValue(
                    "Charlo Donation Blessing",
                    out SelfHinterData charloData
                )
            )
            {
                Res.Result result = Res.Msg("self-hinter.charlo");

                // Try to get "item" slotMeta. Results in null if not there.
                result.slotMeta.TryGetValue(
                    "item",
                    out Dictionary<string, string> resultSlotMetaItem
                );

                string itemText = GenItemText3(
                    out _,
                    charloData.itemToHint,
                    CheckStatus.Unknown,
                    contextIn: charloData.useDefArticle ? "def" : "indef",
                    optionalContextMetaIn: resultSlotMetaItem
                );

                string charloText = Res.LangSpecificNormalize(
                    result.Substitute(new() { { "item", itemText } })
                );
                results2.AddStrReplacement(
                    StrRepl.Hidden(Node.msg_CharloOptsBody, charloText + CustomMessages.shopOption)
                );
            }

            // Note we always need to update the options text to 100 Rupees, 50
            // Rupees, etc. even if the body text is vanilla based on settings.
            string charloOptionsText = Res.SimpleMsg("self-hinter.charlo-options", null);
            results2.AddStrReplacement(
                StrRepl.Public(Node.msg_CharloOptsOptions, charloOptionsText)
            );

            // Fishing Hole Bottle sign
            if (
                selfHinterChecks.TryGetValue(
                    "Fishing Hole Bottle",
                    out SelfHinterData fishingBottleData
                )
            )
            {
                string fishingBottleItemText = GenItemText3(
                    out _,
                    fishingBottleData.itemToHint,
                    CheckStatus.Unknown,
                    contextIn: "fishing-bottle"
                );
                Res.Result fishingBottleRes = Res.Msg("self-hinter.fishing-bottle", null);
                string fishingBottleText = Res.LangSpecificNormalize(
                    fishingBottleRes.Substitute(new() { { "item", fishingBottleItemText } }),
                    Res.IsCultureJa() ? 25 : 30
                );
                results2.AddStrReplacement(
                    StrRepl.Hidden(Node.msg_FishingHoleBottleSign, fishingBottleText)
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

            // uint seraSlingshotPrice = 30;
            // TODO: temp change for demo
            ushort seraSlingshotPrice = 55;

            results2.AddBranchPatches(
                new()
                {
                    new(Node.br_SeraSlingshotCheckCanAfford, null, parameters: seraSlingshotPrice),
                }
            );
            results2.AddEventEntity(
                new(Node.ev_SeraSlingshotPayPrice, null, intParam: seraSlingshotPrice)
            );

            AddShopSlotMsg(
                Node.msg_SeraSlingshotSlot,
                "Sera Shop Slingshot",
                Item.Slingshot,
                seraSlingshotPrice,
                "sera"
            );
            AddShopCantAffordMsg(
                Node.msg_SeraSlingshotCantAfford,
                "Sera Shop Slingshot",
                Item.Slingshot,
                seraSlingshotPrice
            );
            AddShopConfirmationMsg(
                Node.msg_SeraSlingshotConfirmation,
                "Sera Shop Slingshot",
                Item.Slingshot,
                seraSlingshotPrice,
                "sera"
            );

            // ----- Coro -----

            // Note that the item text is orange because of this function.
            // However having it be orange matches the other shop items and is
            // easier to read since there is the red "refills" in the text also,
            // so leaving it as orange intentionally.
            AddShopConfirmationMsg(
                Node.msg_CoroBuyOptionsConfirmation,
                "Coro Bottle",
                Item.Coro_Bottle,
                100,
                "coro"
            );

            // ----- Kakariko Malo Mart -----

            uint kakMaloHawkeyePrice = 100;
            AddShopSlotMsg(
                Node.msg_KakMaloMartHawkeyeSlot,
                "Kakariko Village Malo Mart Hawkeye",
                Item.Hawkeye,
                kakMaloHawkeyePrice
            );
            AddShopCantAffordMsg(
                Node.msg_KakMaloMartHawkeyeCantAfford,
                "Kakariko Village Malo Mart Hawkeye",
                Item.Hawkeye,
                kakMaloHawkeyePrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                Node.msg_KakMaloMartHawkeyeConfirmation,
                "Kakariko Village Malo Mart Hawkeye",
                Item.Hawkeye,
                kakMaloHawkeyePrice,
                "kak-malo"
            );

            uint kakMaloWoodenShieldPrice = 50;
            AddShopSlotMsg(
                Node.msg_KakMaloMartWoodenShieldSlot,
                "Kakariko Village Malo Mart Wooden Shield",
                Item.Wooden_Shield,
                kakMaloWoodenShieldPrice
            );
            AddShopCantAffordMsg(
                Node.msg_KakMaloMartWoodenShieldCantAfford,
                "Kakariko Village Malo Mart Wooden Shield",
                Item.Wooden_Shield,
                kakMaloWoodenShieldPrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                Node.msg_KakMaloMartWoodenShieldConfirmation,
                "Kakariko Village Malo Mart Wooden Shield",
                Item.Wooden_Shield,
                kakMaloWoodenShieldPrice,
                "kak-malo"
            );

            // uint kakMaloHylianShieldPrice = 200;
            // TODO: temp test code
            uint kakMaloHylianShieldPrice = 315;
            AddShopSlotMsg(
                Node.msg_KakMaloMartHylianShieldSlot,
                "Kakariko Village Malo Mart Hylian Shield",
                Item.Hylian_Shield,
                kakMaloHylianShieldPrice
            );
            AddShopCantAffordMsg(
                Node.msg_KakMaloMartHylianShieldCantAfford,
                "Kakariko Village Malo Mart Hylian Shield",
                Item.Hylian_Shield,
                kakMaloHylianShieldPrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                Node.msg_KakMaloMartHylianShieldConfirmation,
                "Kakariko Village Malo Mart Hylian Shield",
                Item.Hylian_Shield,
                kakMaloHylianShieldPrice,
                "kak-malo"
            );

            uint kakMaloRedPotionPrice = 30;
            AddShopSlotMsg(
                Node.msg_KakMaloMartRedPotionSlot,
                "Kakariko Village Malo Mart Red Potion",
                Item.Red_Potion_Shop,
                kakMaloRedPotionPrice
            );
            AddShopCantAffordMsg(
                Node.msg_KakMaloMartRedPotionCantAfford,
                "Kakariko Village Malo Mart Red Potion",
                Item.Red_Potion_Shop,
                kakMaloRedPotionPrice,
                "kak-malo"
            );
            AddShopConfirmationMsg(
                Node.msg_KakMaloMartRedPotionConfirmation,
                "Kakariko Village Malo Mart Red Potion",
                Item.Red_Potion_Shop,
                kakMaloRedPotionPrice,
                "kak-malo"
            );

            // ----- Castle Town Malo Mart -----

            AddShopSlotMsg(
                Node.msg_ChudleysFineGoodsMagicArmorSlot,
                "Castle Town Malo Mart Magic Armor",
                Item.Magic_Armor,
                598,
                "chudley"
            );

            AddShopSlotMsg(
                Node.msg_CtMaloMartMagicArmorSlot,
                "Castle Town Malo Mart Magic Armor",
                Item.Magic_Armor,
                598,
                "magic-armor"
            );
            results2.AddStrReplacement(
                StrRepl.Hidden(
                    Node.msg_CtMaloMartMagicArmorSoldOut,
                    GenShopSoldOutText(
                        HintUtils.getCheckContents("Castle Town Malo Mart Magic Armor"),
                        "magic-armor"
                    )
                )
            );

            // ----- Castle Town Gorons -----

            // Gorons use "rrubis" instead of "rubis" for French
            Dictionary<string, string> goronPriceContextMeta = new() { { "goron", "true" } };

            uint ctGoronRedPotionPrice = 40;
            AddShopConfirmationMsg(
                Node.msg_CtGoronRedPotionConfirmationInitial,
                "Castle Town Goron Shop Red Potion",
                Item.Red_Potion_Shop,
                ctGoronRedPotionPrice,
                "ct-goron-red-potion",
                priceContextMeta: goronPriceContextMeta
            );
            AddShopConfirmationMsg(
                Node.msg_CtGoronRedPotionConfirmationSecond,
                "Castle Town Goron Shop Red Potion",
                Item.Red_Potion_Shop,
                ctGoronRedPotionPrice,
                "ct-goron-red-potion",
                priceContextMeta: goronPriceContextMeta
            );
            AddShopCantAffordMsg(
                Node.msg_CtGoronRedPotionCantAfford,
                "Castle Town Goron Shop Red Potion",
                Item.Red_Potion_Shop,
                ctGoronRedPotionPrice,
                "ct-small-gorons"
            );

            uint ctGoronLanternOilPrice = 30;
            AddShopConfirmationMsg(
                Node.msg_CtGoronLanternOilConfirmationInitial,
                "Castle Town Goron Shop Lantern Oil",
                Item.Lantern_Oil_Shop,
                ctGoronLanternOilPrice,
                "ct-goron-oil-initial",
                priceContextMeta: goronPriceContextMeta
            );
            AddShopConfirmationMsg(
                Node.msg_CtGoronLanternOilConfirmationSecond,
                "Castle Town Goron Shop Lantern Oil",
                Item.Lantern_Oil_Shop,
                ctGoronLanternOilPrice,
                "ct-goron-oil-later",
                priceContextMeta: goronPriceContextMeta
            );
            AddShopCantAffordMsg(
                Node.msg_CtGoronLanternOilCantAfford,
                "Castle Town Goron Shop Lantern Oil",
                Item.Lantern_Oil_Shop,
                ctGoronLanternOilPrice,
                "ct-small-gorons"
            );

            uint ctGoronArrowsPrice = 40;
            AddShopConfirmationMsg(
                Node.msg_CtGoronArrowsConfirmationInitial,
                "Castle Town Goron Shop Arrow Refill",
                Item.Arrows_30,
                ctGoronArrowsPrice,
                "ct-goron-arrows",
                priceContextMeta: goronPriceContextMeta
            );
            AddShopConfirmationMsg(
                Node.msg_CtGoronArrowsConfirmationSecond,
                "Castle Town Goron Shop Arrow Refill",
                Item.Arrows_30,
                ctGoronArrowsPrice,
                "ct-goron-arrows",
                priceContextMeta: goronPriceContextMeta
            );

            uint ctGoronShieldPrice = 210;
            AddShopConfirmationMsg(
                Node.msg_CtGoronShieldConfirmationIntitial,
                "Castle Town Goron Shop Hylian Shield",
                Item.Hylian_Shield,
                ctGoronShieldPrice,
                "ct-goron-shield-initial",
                priceContextMeta: goronPriceContextMeta
            );
            AddShopConfirmationMsg(
                Node.msg_CtGoronShieldConfirmationSecond,
                "Castle Town Goron Shop Hylian Shield",
                Item.Hylian_Shield,
                ctGoronShieldPrice,
                "ct-goron-shield-later",
                priceContextMeta: goronPriceContextMeta
            );

            // ----- Barnes -----

            if (selfHinterChecks.TryGetValue("Barnes Bomb Bag", out SelfHinterData barnesData))
            {
                Res.Result result = Res.Msg("self-hinter.barnes-bomb-bag");

                // Try to get "item" slotMeta. Results in null if not there.
                result.slotMeta.TryGetValue(
                    "item",
                    out Dictionary<string, string> resultSlotMetaItem
                );

                string itemText = GenItemText3(
                    out _,
                    barnesData.itemToHint,
                    CheckStatus.Unknown,
                    barnesData.useDefArticle ? "def" : "indef",
                    prefStartColor: CustomMessages.messageColorOrange,
                    optionalContextMetaIn: resultSlotMetaItem
                );

                string priceText = GenShopPriceText(120);

                string text = result.Substitute(
                    new() { { "item", itemText }, { "price", priceText } }
                );

                results2.AddStrReplacement(
                    StrRepl.Hidden(
                        Node.msg_BarnesBombBagConfirmation,
                        Res.LangSpecificNormalize(text) + CustomMessages.shopOption
                    )
                );
            }
        }

        private void GenHintSignEntries()
        {
            if (!ListUtils.isEmpty(hintSpots))
            {
                foreach (HintSpot hintSpot in hintSpots)
                {
                    if (
                        CustomMsgUtils.TryGetCustomSignFlowId(
                            hintSpot.location,
                            out ushort customSignFlowId
                        )
                    )
                    {
                        // Is custom sign
                        List<string> hintTexts = hintSpot.hints
                            .Select((hint) => hint.toHintTextList(this)[0].text)
                            .ToList();

                        List<string> msgNodeTexts = Res.SplitOversizedTexts(hintTexts);

                        AddCustomSignEntityData(hintSpot.location, msgNodeTexts);
                    }
                    else if (
                        CustomMsgUtils.TryGetSpotIdVanillaNode(
                            hintSpot.location,
                            out MsgNodeInst node
                        )
                    )
                    {
                        // Is vanilla node (Agitha, Jovani)
                        List<Hint> hints = hintSpot.hints;
                        if (hints.Count > 1)
                            throw new Exception(
                                $"Expected only a single hint for SpotId '{hintSpot.location}'."
                            );

                        string text = hints[0].toHintTextList(this)[0].text;
                        results2.AddStrReplacement(StrRepl.Hidden(node, text));
                    }
                    else
                    {
                        throw new Exception(
                            $"Failed to find spot info for SpotId '{hintSpot.location}'."
                        );
                    }
                }
            }
        }

        private void AddCustomSignEntityData(SpotId spotId, List<string> messages)
        {
            if (ListUtils.isEmpty(messages))
                return;

            ushort flowId = CustomMsgUtils.GetCustomSignFlowId(spotId);

            ushort latestContext = GetNewContext();

            results2.AddNodeRemap(
                NodeRemap.Fli(flowId, Node.zel00_FFFF, Node.msg_Z0_0x28.flwIdx, latestContext)
            );

            for (int i = 0; i < messages.Count; i++)
            {
                string msg = messages[i];

                results2.AddStrReplacement(StrRepl.CustomSignText(latestContext, msg));

                if (i < messages.Count - 1)
                {
                    // If has more messages, map back to same node instead of
                    // continuing to 0xFFFF.
                    ushort prevCtx = latestContext;
                    latestContext = GetNewContext();

                    results2.AddNodeRemap(
                        NodeRemap.Ctx(
                            prevCtx,
                            Node.zel00_FFFF,
                            Node.msg_Z0_0x28.flwIdx,
                            latestContext
                        )
                    );
                }
            }
        }

        private void AddShopSlotMsg(
            MsgNodeInst msgNode,
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
                out Dictionary<string, string> itemMeta,
                item,
                CheckStatus.Unknown,
                isShop: true,
                includeShopSuffix: true,
                shopSuffixIsColon: shopSuffixIsColon
            );

            string nounVal = GenNamedSlotVal(res, "noun", itemMeta);
            string noun2Val = GenNamedSlotVal(res, "noun2", itemMeta, keyStart: "noun");

            string priceText = GenShopPriceText(price);

            string text = res.Substitute(
                new()
                {
                    { "item", itemText },
                    { "price", priceText },
                    { "noun", nounVal },
                    { "noun2", noun2Val },
                    { "noun2b", noun2Val },
                }
            );
            string normalizedText = Res.LangSpecificNormalize(text);

            results2.AddStrReplacement(StrRepl.Hidden(msgNode, normalizedText));
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

            List<string> chunksList = chunks.ToList();
            chunksList.Sort(StringComparer.Ordinal);
            return string.Join(',', chunksList);
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

            Res.Result abc = Res.Msg(
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
            CheckStatusDisplay checkStatusDisplay = CheckStatusDisplay.None,
            bool isLogicalItem = true,
            Dictionary<string, string> optionalContextMetaIn = null
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
                && checkStatusDisplay == CheckStatusDisplay.Required_Info
            )
            {
                checkStatusDisplay = CheckStatusDisplay.Automatic;
            }

            Dictionary<string, string> optionalContextMetaA;
            if (!ListUtils.isEmpty(optionalContextMetaIn))
                optionalContextMetaA = new(optionalContextMetaIn);
            else
                optionalContextMetaA = new();

            if (!StringUtils.isEmpty(context))
            {
                HashSet<string> contextParts = new(context.Split(","));
                foreach (string contextPart in contextParts)
                {
                    optionalContextMetaA[contextPart] = "true";
                }
            }

            // Swap to making all context optional so we only use def/indef if
            // they are defined on the item. Otherwise we have to define
            // "def,shop-group-of" and "indef,shop-group-of" instead of just
            // "shop-group-of" for an item which does not use "def" or "indef"
            // at all. If needed, we can probably make a paramter to this
            // function be "requiredContext".
            Res.Result abc = Res.Msg(
                GetItemResKey(item),
                new() { { "count", countStr } },
                optionalContextMetaA
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
                else if (checkStatusDisplay == CheckStatusDisplay.Required_Info)
                {
                    if (checkStatus == CheckStatus.Required)
                    {
                        startColor = CustomMessages.messageColorBlue;
                        if (isLogicalItem)
                            postItemText = " " + Res.SimpleMsg("description.required-check", null);
                    }
                    else if (checkStatus == CheckStatus.Bad)
                    {
                        startColor = CustomMessages.messageColorPurple;
                        if (isLogicalItem)
                            postItemText =
                                " " + Res.SimpleMsg("description.unrequired-check", null);
                    }
                    else
                    {
                        startColor = CustomMessages.messageColorGreen;
                        if (isLogicalItem)
                            postItemText = " " + Res.SimpleMsg("description.skippable-check", null);
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
                    else if (isLogicalItem && checkStatus == CheckStatus.Bad)
                    {
                        // If item is a logicalItem which is bad for some
                        // reason, then explicitly call it out. For example, if
                        // a bomb bag is considered bad because a different bomb
                        // bag is on a logically required check.
                        postItemText = " " + Res.SimpleMsg("description.bad-check", null);
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
            Dictionary<string, string> optionalContextMeta = new();
            if (count != null)
                optionalContextMeta.Add("count", countStr);

            if (isShop)
            {
                optionalContextMeta["cs"] = "";
                optionalContextMeta["ce"] = "";
                coloredItem = startColor + abc.Substitute(optionalContextMeta) + itemSuffix;
            }
            else if (abc.value.Contains("{cs}"))
            {
                optionalContextMeta["cs"] = startColor;
                optionalContextMeta["ce"] = itemSuffix;
                coloredItem = abc.Substitute(optionalContextMeta);
            }
            else
            {
                coloredItem = startColor + abc.Substitute(optionalContextMeta) + itemSuffix;
            }

            return coloredItem;
        }

        private static string GetItemResKey(Item item)
        {
            return "item." + ((byte)item).ToString("x2") + "-" + item.ToString().ToLowerInvariant();
        }

        private string GenShopPriceText(
            uint amount,
            bool includeColor = true,
            Dictionary<string, string> priceContextMeta = null
        )
        {
            string result = "";
            if (includeColor)
                result += CustomMessages.messageColorPurple;

            Dictionary<string, string> interpolation =
                new() { { "count", amount.ToString(CultureInfo.InvariantCulture) }, };

            string shopText = Res.Msg("shop.price", interpolation, priceContextMeta)
                .Substitute(interpolation);
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

            Res.Result areaPhraseRes = Res.Msg($"area-phrase.{areaPhraseKey}");
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

        public static string GenNamedSlotVal(
            Res.Result hintResResult,
            string slotName,
            Dictionary<string, string> subjectMeta = null,
            string keyStart = null
        )
        {
            if (StringUtils.isEmpty(keyStart))
                keyStart = slotName;

            string val = "";
            if (
                hintResResult.slotMeta.TryGetValue(slotName, out Dictionary<string, string> valMeta)
            )
            {
                if (valMeta.TryGetValue("name", out string valName))
                {
                    val = Res.Msg($"{keyStart}.{valName}", null, subjectMeta).Substitute(null);
                }
            }

            return val;
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
                    HintInfo hintInfo = hint.GetHintInfo(this);
                    if (hintInfo != null)
                        hintInfos.Add(hint.GetHintInfo(this).GetSpoilerDict());
                    else
                        hintInfos.Add(null);
                }
                keyToHintInfos[key] = hintInfos;
            }

            return keyToHintInfos;
        }

        public StringTableResult2.Header AddBytesGenHeader(ushort headerSize, List<byte> bodyData)
        {
            return results2.AddBytesGenHeader(headerSize, bodyData);
        }

        private ushort GetNewContext()
        {
            return ctxGen.getNewContext();
        }

        private class CtxGen
        {
            // Starting at 2 since context of 1 is reserved for custom sign
            // fallback msg.
            private ushort nextContext = 2;

            public ushort getNewContext()
            {
                ushort result = nextContext;
                nextContext += 1;
                if (result == 0)
                    throw new Exception("Was returning an invalid context value of '0'.");
                return result;
            }
        }
    }
}
