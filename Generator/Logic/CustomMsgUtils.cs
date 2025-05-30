namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Assets;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;
    using MessageEntry = Assets.CustomMessages.MessageEntry;
    using HintSpotBmgData = Assets.CustomMessages.HintSpotBmgData;
    using FLIGroup = Assets.CustomMessages.FLIGroup;
    using System.Threading.Tasks.Dataflow;

    public class NodeInst
    {
        public StgBmg stgBmg { get; private set; }
        public ushort flwIdx { get; private set; }

        public NodeInst(StgBmg stgBmg, ushort flwIdx)
        {
            this.stgBmg = stgBmg;
            this.flwIdx = flwIdx;
        }
    }

    public class MsgNodeInst : NodeInst
    {
        public ushort infIndex { get; private set; }

        public MsgNodeInst(StgBmg stgBmg, ushort flwIdx, ushort infIndex) : base(stgBmg, flwIdx)
        {
            this.infIndex = infIndex;
        }
    }

    public class Node
    {
        // Generic branch node in zel_00 we can patch under context.
        public static NodeInst brTalkToMidnaRootNode = new(StgBmg.zel_00, 0x8f);
        public static NodeInst brZel00ThreeOptsResultBranch = new(StgBmg.zel_00, 0x193);
        public static NodeInst brZ0_GeneriCtxBranch = new(StgBmg.zel_00, 0x199);
        public static NodeInst brKakMaloMartHylianShieldCanAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x421);

        // Generic event node in zel_00 we can patch under context.
        public static NodeInst evZ0_GenericCtxEvent = new(StgBmg.zel_00, 0x1a4);
        public static NodeInst evZel00Other = new(StgBmg.zel_00, 0x9);

        public static NodeInst evKakMaloMartHylianShieldPay =
            new(StgBmg.Kakariko_Village_Interiors, 0x424);
        public static NodeInst evKakMaloMartHylianShieldBeforePay =
            new(StgBmg.Kakariko_Village_Interiors, 0x429);

        // new(StgBmg.zel_00, 0x1a4, 3, eventIndex: 43, nextNodeIdx: 0xFFFF),

        public static NodeInst evZ0_MidnaTwoOptsInitEv = new(StgBmg.zel_00, 0x1a0);
        public static MsgNodeInst msgZ0_MidnaTwoOptsBody = new(StgBmg.zel_00, 0x19f, 0x5de);
        public static MsgNodeInst msgZ0_MidnaTwoOptsOptions = new(StgBmg.zel_00, 0x1a1, 0x5df);
        public static NodeInst brZ0_MidnaTwoOptsResultBranch = new(StgBmg.zel_00, 0x1a3);
        public static NodeInst msgZel00_0x9 = new(StgBmg.zel_00, 0x9);
        public static MsgNodeInst msgZ0_0x27 = new(StgBmg.zel_00, 0x27, 0x5e5);
        public static MsgNodeInst msgZ0_0x28 = new(StgBmg.zel_00, 0x28, 0xa11);
        public static NodeInst zel00_FFFF = new(StgBmg.zel_00, 0xFFFF);
        public static MsgNodeInst msgCT_StarSigns = new(StgBmg.Castle_Town, 0x883, 0x4ce);
        public static MsgNodeInst msgCT_StarGirlsNoAttemptFirst =
            new(StgBmg.Castle_Town, 0x73, 0x368);
        public static MsgNodeInst msgCT_StarGirlsNoAttemptSecond =
            new(StgBmg.Castle_Town, 0x75, 0x369);
        public static MsgNodeInst msgCT_StarGirlsNoAttemptThird =
            new(StgBmg.Castle_Town, 0x76, 0x36a);
    }

    public enum MsgEntryId
    {
        Link_House_Sign,
        Sera_Slingshot_Slot,
        Sera_Slingshot_Cant_Afford,
        Sera_Slingshot_Confirmation,
        Sera_Slingshot_Bought,
        Sera_Slingshot_Bought_2,
        Kakariko_Malo_Mart_Hawkeye_Slot,
        Kakariko_Malo_Mart_Hawkeye_Cant_Afford,
        Kakariko_Malo_Mart_Hawkeye_Confirmation,
        Kakariko_Malo_Mart_Hawkeye_Sold_Out,
        Kakariko_Malo_Mart_Hawkeye_Sold_Out_Read,
        Kakariko_Malo_Mart_Wooden_Shield_Slot,
        Kakariko_Malo_Mart_Wooden_Shield_Cant_Afford,
        Kakariko_Malo_Mart_Wooden_Shield_Confirmation,
        Kakariko_Malo_Mart_Hylian_Shield_Slot,
        Kakariko_Malo_Mart_Hylian_Shield_Cant_Afford,
        Kakariko_Malo_Mart_Hylian_Shield_Confirmation,
        Kakariko_Malo_Mart_Hylian_Shield_Sold_Out,
        Kakariko_Malo_Mart_Hylian_Shield_Sold_Out_Read,

        // If you buy the wooden shield slot before anything else, you will see
        // this one instead for that slot.
        Kakariko_Malo_Mart_Hylian_Shield_Sold_Out_Read_2,
        Kakariko_Malo_Mart_Red_Potion_Slot,
        Kakariko_Malo_Mart_Red_Potion_Cant_Afford,
        Kakariko_Malo_Mart_Red_Potion_Confirmation,
        Kakariko_Malo_Mart_Red_Potion_Bought,
        Chudleys_Fine_Goods_Magic_Armor_Slot,
        Castle_Town_Malo_Mart_Magic_Armor_Slot,
        Castle_Town_Malo_Mart_Magic_Armor_Bought,
        Castle_Town_Malo_Mart_Magic_Armor_Sold_Out,
        Castle_Town_Goron_Red_Potion_Confirmation_Initial,
        Castle_Town_Goron_Red_Potion_Confirmation_Second,
        Castle_Town_Goron_Red_Potion_Cant_Afford,
        Castle_Town_Goron_Lantern_Oil_Confirmation_Initial,
        Castle_Town_Goron_Lantern_Oil_Confirmation_Second,
        Castle_Town_Goron_Lantern_Oil_Cant_Afford,
        Castle_Town_Goron_Arrows_Confirmation_Initial,
        Castle_Town_Goron_Arrows_Confirmation_Second,
        Castle_Town_Goron_Shield_Confirmation_Intitial,
        Castle_Town_Goron_Shield_Confirmation_Second,
        Agithas_Castle_Sign,
        Jovani_House_Sign,
        Custom_Sign_Ordon,
        Custom_Sign_Sacred_Grove,
        Custom_Sign_Faron_Field,
        Custom_Sign_Faron_Woods,
        Custom_Sign_Kakariko_Gorge,
        Custom_Sign_Kakariko_Village,
        Custom_Sign_Kakariko_Graveyard,
        Custom_Sign_Eldin_Field,
        Custom_Sign_North_Eldin,
        Custom_Sign_Death_Mountain,
        Custom_Sign_Hidden_Village,
        Custom_Sign_Lanayru_Field,
        Custom_Sign_Beside_Castle_Town,
        Custom_Sign_South_of_Castle_Town,
        Custom_Sign_Castle_Town,
        Custom_Sign_Great_Bridge_of_Hylia,
        Custom_Sign_Lake_Hylia,
        Custom_Sign_Lake_Lantern_Cave,
        Custom_Sign_Lanayru_Spring,
        Custom_Sign_Zoras_Domain,
        Custom_Sign_Upper_Zoras_River,
        Custom_Sign_Gerudo_Desert,
        Custom_Sign_Bulblin_Camp,
        Custom_Sign_Snowpeak_Mountain,
        Custom_Sign_Golden_Wolf,
        Custom_Sign_Cave_of_Ordeals,
        Custom_Sign_LongMinigames,
        Custom_Sign_Forest_Temple,
        Custom_Sign_Goron_Mines,
        Custom_Sign_Lakebed_Temple,
        Custom_Sign_Arbiters_Grounds,
        Custom_Sign_Snowpeak_Ruins,
        Custom_Sign_Temple_of_Time,
        Custom_Sign_Temple_of_Time_Midpoint,
        Custom_Sign_City_in_the_Sky,
        Custom_Sign_Palace_of_Twilight,
        Custom_Sign_Hyrule_Castle,
        Custom_Sign_Fallback,
        Barnes_Bomb_Bag_Confirmation,
        Barnes_Bomb_Bag_Cant_Afford,
        Charlo_Donation_Confirmation,
        Fishing_Hole_Bottle_Sign,
        Coro_Buy_Options_Confirmation,
    }

    public class CustomMsgUtils
    {
        private static readonly Dictionary<SpotId, MsgEntryId> spotIdToEntry =
            new()
            {
                { SpotId.Agithas_Castle_Sign, MsgEntryId.Agithas_Castle_Sign },
                { SpotId.Jovani_House_Sign, MsgEntryId.Jovani_House_Sign },
                { SpotId.Ordon_Sign, MsgEntryId.Custom_Sign_Ordon },
                { SpotId.Sacred_Grove_Sign, MsgEntryId.Custom_Sign_Sacred_Grove },
                { SpotId.Faron_Field_Sign, MsgEntryId.Custom_Sign_Faron_Field },
                { SpotId.Faron_Woods_Sign, MsgEntryId.Custom_Sign_Faron_Woods },
                { SpotId.Kakariko_Gorge_Sign, MsgEntryId.Custom_Sign_Kakariko_Gorge },
                { SpotId.Kakariko_Village_Sign, MsgEntryId.Custom_Sign_Kakariko_Village },
                { SpotId.Kakariko_Graveyard_Sign, MsgEntryId.Custom_Sign_Kakariko_Graveyard },
                { SpotId.Eldin_Field_Sign, MsgEntryId.Custom_Sign_Eldin_Field },
                { SpotId.North_Eldin_Sign, MsgEntryId.Custom_Sign_North_Eldin },
                { SpotId.Death_Mountain_Sign, MsgEntryId.Custom_Sign_Death_Mountain },
                { SpotId.Hidden_Village_Sign, MsgEntryId.Custom_Sign_Hidden_Village },
                { SpotId.Lanayru_Field_Sign, MsgEntryId.Custom_Sign_Lanayru_Field },
                { SpotId.Beside_Castle_Town_Sign, MsgEntryId.Custom_Sign_Beside_Castle_Town },
                { SpotId.South_of_Castle_Town_Sign, MsgEntryId.Custom_Sign_South_of_Castle_Town },
                { SpotId.Castle_Town_Sign, MsgEntryId.Custom_Sign_Castle_Town },
                { SpotId.Great_Bridge_of_Hylia_Sign, MsgEntryId.Custom_Sign_Great_Bridge_of_Hylia },
                { SpotId.Lake_Hylia_Sign, MsgEntryId.Custom_Sign_Lake_Hylia },
                { SpotId.Lake_Lantern_Cave_Sign, MsgEntryId.Custom_Sign_Lake_Lantern_Cave },
                { SpotId.Lanayru_Spring_Sign, MsgEntryId.Custom_Sign_Lanayru_Spring },
                { SpotId.Zoras_Domain_Sign, MsgEntryId.Custom_Sign_Zoras_Domain },
                { SpotId.Upper_Zoras_River_Sign, MsgEntryId.Custom_Sign_Upper_Zoras_River },
                { SpotId.Gerudo_Desert_Sign, MsgEntryId.Custom_Sign_Gerudo_Desert },
                { SpotId.Bulblin_Camp_Sign, MsgEntryId.Custom_Sign_Bulblin_Camp },
                { SpotId.Snowpeak_Mountain_Sign, MsgEntryId.Custom_Sign_Snowpeak_Mountain },
                { SpotId.Cave_of_Ordeals_Sign, MsgEntryId.Custom_Sign_Cave_of_Ordeals },
                { SpotId.Forest_Temple_Sign, MsgEntryId.Custom_Sign_Forest_Temple },
                { SpotId.Goron_Mines_Sign, MsgEntryId.Custom_Sign_Goron_Mines },
                { SpotId.Lakebed_Temple_Sign, MsgEntryId.Custom_Sign_Lakebed_Temple },
                { SpotId.Arbiters_Grounds_Sign, MsgEntryId.Custom_Sign_Arbiters_Grounds },
                { SpotId.Snowpeak_Ruins_Sign, MsgEntryId.Custom_Sign_Snowpeak_Ruins },
                { SpotId.Temple_of_Time_Sign, MsgEntryId.Custom_Sign_Temple_of_Time },
                { SpotId.City_in_the_Sky_Sign, MsgEntryId.Custom_Sign_City_in_the_Sky },
                { SpotId.Palace_of_Twilight_Sign, MsgEntryId.Custom_Sign_Palace_of_Twilight },
                { SpotId.Hyrule_Castle_Sign, MsgEntryId.Custom_Sign_Hyrule_Castle },
                {
                    SpotId.Temple_of_Time_Beyond_Point_Sign,
                    MsgEntryId.Custom_Sign_Temple_of_Time_Midpoint
                },
            };

        private static readonly Dictionary<MsgEntryId, MessageEntry> idToEntry =
            new()
            {
                // new MessageEntry
                // {
                //     stageIDX = (byte)StageIDs.Ordon_Village_Interiors,
                //     roomIDX = 1,
                //     // messageID = 0x5AD, // Slingshot Check
                //     messageID = 0x5AE, // Slingshot Check
                //     message =
                //         getShortenedItemName(Randomizer.Checks.CheckDict["Sera Shop Slingshot"].itemId)
                //         + ": "
                //         + messageColorPurple
                //         + "30 Rupees\n"
                //         + messageColorWhite
                //         + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
                // },
                { MsgEntryId.Link_House_Sign, new(StageIDs.Ordon_Village, 1, 0x658) },
                { MsgEntryId.Sera_Slingshot_Slot, new(StageIDs.Ordon_Village_Interiors, 1, 0x5AE) },
                {
                    MsgEntryId.Sera_Slingshot_Cant_Afford,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B3)
                },
                {
                    MsgEntryId.Sera_Slingshot_Confirmation,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B4)
                },
                {
                    MsgEntryId.Sera_Slingshot_Bought,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B5)
                },
                {
                    MsgEntryId.Sera_Slingshot_Bought_2,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B6)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Slot,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x307)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Cant_Afford,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D3)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Confirmation,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D2)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Sold_Out,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x306)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hawkeye_Sold_Out_Read,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D4)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Wooden_Shield_Slot,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x30D)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Wooden_Shield_Cant_Afford,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2C8)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Wooden_Shield_Confirmation,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2C7)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Slot,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x30E)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Cant_Afford,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2CC)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Confirmation,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2CB)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Sold_Out,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x30B)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Sold_Out_Read,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D0)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Hylian_Shield_Sold_Out_Read_2,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2E0)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Slot,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x305)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Cant_Afford,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D6)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Confirmation,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D7)
                },
                {
                    MsgEntryId.Kakariko_Malo_Mart_Red_Potion_Bought,
                    new(StageIDs.Kakariko_Village_Interiors, 3, 0x2D8)
                },
                {
                    MsgEntryId.Chudleys_Fine_Goods_Magic_Armor_Slot,
                    new(StageIDs.Castle_Town_Shops, 0, 0x10A)
                },
                {
                    MsgEntryId.Castle_Town_Malo_Mart_Magic_Armor_Slot,
                    new(StageIDs.Castle_Town_Shops, 0, 0x125)
                },
                {
                    MsgEntryId.Castle_Town_Malo_Mart_Magic_Armor_Bought,
                    new(StageIDs.Castle_Town_Shops, 0, 0x11E)
                },
                {
                    MsgEntryId.Castle_Town_Malo_Mart_Magic_Armor_Sold_Out,
                    new(StageIDs.Castle_Town_Shops, 0, 0x130)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Red_Potion_Confirmation_Initial,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3BF)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Red_Potion_Confirmation_Second,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3C1)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Red_Potion_Cant_Afford,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3C2)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Lantern_Oil_Confirmation_Initial,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3B3)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Lantern_Oil_Confirmation_Second,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3B5)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Lantern_Oil_Cant_Afford,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3AC)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Arrows_Confirmation_Initial,
                    new(StageIDs.Castle_Town, 0, 0x3D8)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Arrows_Confirmation_Second,
                    new(StageIDs.Castle_Town, 0, 0x3DB)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Shield_Confirmation_Intitial,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3E3)
                },
                {
                    MsgEntryId.Castle_Town_Goron_Shield_Confirmation_Second,
                    new(StageIDs.Castle_Town_Shops, 4, 0x3E5)
                },
                { MsgEntryId.Agithas_Castle_Sign, new(StageIDs.Castle_Town, 3, 0x456) },
                { MsgEntryId.Jovani_House_Sign, new(StageIDs.Castle_Town, 3, 0x457) },
                { MsgEntryId.Custom_Sign_Ordon, new(StageIDs.Ordon_Village, 1, 0x1369) },
                { MsgEntryId.Custom_Sign_Sacred_Grove, new(StageIDs.Sacred_Grove, 1, 0x1369) },
                { MsgEntryId.Custom_Sign_Faron_Field, new(StageIDs.Hyrule_Field, 6, 0x1369) },
                { MsgEntryId.Custom_Sign_Faron_Woods, new(StageIDs.Faron_Woods, 4, 0x1369) },
                { MsgEntryId.Custom_Sign_Kakariko_Gorge, new(StageIDs.Hyrule_Field, 3, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Kakariko_Village,
                    new(StageIDs.Kakariko_Village, 0, 0x1369)
                },
                {
                    MsgEntryId.Custom_Sign_Kakariko_Graveyard,
                    new(StageIDs.Kakariko_Graveyard, 0, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Eldin_Field, new(StageIDs.Hyrule_Field, 0, 0x1369) },
                { MsgEntryId.Custom_Sign_North_Eldin, new(StageIDs.Hyrule_Field, 7, 0x1369) },
                { MsgEntryId.Custom_Sign_Death_Mountain, new(StageIDs.Death_Mountain, 3, 0x1369) },
                { MsgEntryId.Custom_Sign_Hidden_Village, new(StageIDs.Hidden_Village, 0, 0x1369) },
                { MsgEntryId.Custom_Sign_Lanayru_Field, new(StageIDs.Hyrule_Field, 10, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Beside_Castle_Town,
                    new(StageIDs.Outside_Castle_Town, 8, 0x1369)
                },
                {
                    MsgEntryId.Custom_Sign_South_of_Castle_Town,
                    new(StageIDs.Outside_Castle_Town, 16, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Castle_Town, new(StageIDs.Castle_Town, 0, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Great_Bridge_of_Hylia,
                    new(StageIDs.Hyrule_Field, 13, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Lake_Hylia, new(StageIDs.Lake_Hylia, 0, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Lake_Lantern_Cave,
                    new(StageIDs.Lake_Hylia_Long_Cave, 0, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Lanayru_Spring, new(StageIDs.Lake_Hylia, 1, 0x1369) },
                { MsgEntryId.Custom_Sign_Zoras_Domain, new(StageIDs.Zoras_Domain, 1, 0x1369) },
                { MsgEntryId.Custom_Sign_Upper_Zoras_River, new(StageIDs.Fishing_Pond, 0, 0x1369) },
                { MsgEntryId.Custom_Sign_Gerudo_Desert, new(StageIDs.Gerudo_Desert, 0, 0x1369) },
                { MsgEntryId.Custom_Sign_Bulblin_Camp, new(StageIDs.Bulblin_Camp, 1, 0x1369) },
                { MsgEntryId.Custom_Sign_Snowpeak_Mountain, new(StageIDs.Snowpeak, 0, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Cave_of_Ordeals,
                    new(StageIDs.Cave_of_Ordeals, 0, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Forest_Temple, new(StageIDs.Forest_Temple, 0, 0x1369) },
                { MsgEntryId.Custom_Sign_Goron_Mines, new(StageIDs.Goron_Mines, 17, 0x1369) },
                { MsgEntryId.Custom_Sign_Lakebed_Temple, new(StageIDs.Lakebed_Temple, 2, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Arbiters_Grounds,
                    new(StageIDs.Arbiters_Grounds, 2, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Snowpeak_Ruins, new(StageIDs.Snowpeak_Ruins, 1, 0x1369) },
                { MsgEntryId.Custom_Sign_Temple_of_Time, new(StageIDs.Temple_of_Time, 0, 0x1369) },
                {
                    MsgEntryId.Custom_Sign_Temple_of_Time_Midpoint,
                    new(StageIDs.Temple_of_Time, 4, 0x1369)
                },
                {
                    MsgEntryId.Custom_Sign_City_in_the_Sky,
                    new(StageIDs.City_in_the_Sky, 2, 0x1369)
                },
                {
                    MsgEntryId.Custom_Sign_Palace_of_Twilight,
                    new(StageIDs.Palace_of_Twilight, 0, 0x1369)
                },
                { MsgEntryId.Custom_Sign_Hyrule_Castle, new(StageIDs.Hyrule_Castle, 11, 0x1369) },
                { MsgEntryId.Custom_Sign_Fallback, new(0xFF, 0xFF, 0x1369) },
                {
                    MsgEntryId.Barnes_Bomb_Bag_Confirmation,
                    new(StageIDs.Kakariko_Village_Interiors, 1, 0x9B)
                },
                {
                    MsgEntryId.Barnes_Bomb_Bag_Cant_Afford,
                    new(StageIDs.Kakariko_Village_Interiors, 1, 0xA0)
                },
                { MsgEntryId.Charlo_Donation_Confirmation, new(StageIDs.Castle_Town, 2, 0x355) },
                { MsgEntryId.Fishing_Hole_Bottle_Sign, new(StageIDs.Fishing_Pond, 0, 0x47A) },
                { MsgEntryId.Coro_Buy_Options_Confirmation, new(StageIDs.Faron_Woods, 4, 0xDD) },
            };

        private static readonly Dictionary<SpotId, HintSpotBmgData> spotToBmgData =
            new()
            {
                { SpotId.Ordon_Sign, new(StageIDs.Ordon_Village, 1) },
                { SpotId.Sacred_Grove_Sign, new(StageIDs.Sacred_Grove, 1) },
                { SpotId.Faron_Field_Sign, new(StageIDs.Hyrule_Field, 6) },
                { SpotId.Faron_Woods_Sign, new(StageIDs.Faron_Woods, 4) },
                { SpotId.Kakariko_Gorge_Sign, new(StageIDs.Hyrule_Field, 3) },
                { SpotId.Kakariko_Village_Sign, new(StageIDs.Kakariko_Village, 0) },
                { SpotId.Kakariko_Graveyard_Sign, new(StageIDs.Kakariko_Graveyard, 0) },
                { SpotId.Eldin_Field_Sign, new(StageIDs.Hyrule_Field, 0) },
                { SpotId.North_Eldin_Sign, new(StageIDs.Hyrule_Field, 7) },
                { SpotId.Death_Mountain_Sign, new(StageIDs.Death_Mountain, 3) },
                { SpotId.Hidden_Village_Sign, new(StageIDs.Hidden_Village, 0) },
                { SpotId.Lanayru_Field_Sign, new(StageIDs.Hyrule_Field, 10) },
                { SpotId.Beside_Castle_Town_Sign, new(StageIDs.Outside_Castle_Town, 8) },
                { SpotId.South_of_Castle_Town_Sign, new(StageIDs.Outside_Castle_Town, 16) },
                { SpotId.Castle_Town_Sign, new(StageIDs.Castle_Town, 0) },
                { SpotId.Great_Bridge_of_Hylia_Sign, new(StageIDs.Hyrule_Field, 13) },
                { SpotId.Lake_Hylia_Sign, new(StageIDs.Lake_Hylia, 0) },
                { SpotId.Lake_Lantern_Cave_Sign, new(StageIDs.Lake_Hylia_Long_Cave, 0) },
                { SpotId.Lanayru_Spring_Sign, new(StageIDs.Lake_Hylia, 1) },
                { SpotId.Zoras_Domain_Sign, new(StageIDs.Zoras_Domain, 1) },
                { SpotId.Upper_Zoras_River_Sign, new(StageIDs.Fishing_Pond, 0) },
                { SpotId.Gerudo_Desert_Sign, new(StageIDs.Gerudo_Desert, 0) },
                { SpotId.Bulblin_Camp_Sign, new(StageIDs.Bulblin_Camp, 1) },
                { SpotId.Snowpeak_Mountain_Sign, new(StageIDs.Snowpeak, 0) },
                { SpotId.Cave_of_Ordeals_Sign, new(StageIDs.Cave_of_Ordeals, 0) },
                { SpotId.Forest_Temple_Sign, new(StageIDs.Forest_Temple, 0) },
                { SpotId.Goron_Mines_Sign, new(StageIDs.Goron_Mines, 17) },
                { SpotId.Lakebed_Temple_Sign, new(StageIDs.Lakebed_Temple, 2) },
                { SpotId.Arbiters_Grounds_Sign, new(StageIDs.Arbiters_Grounds, 2) },
                { SpotId.Snowpeak_Ruins_Sign, new(StageIDs.Snowpeak_Ruins, 1) },
                { SpotId.Temple_of_Time_Sign, new(StageIDs.Temple_of_Time, 0) },
                { SpotId.Temple_of_Time_Beyond_Point_Sign, new(StageIDs.Temple_of_Time, 4) },
                { SpotId.City_in_the_Sky_Sign, new(StageIDs.City_in_the_Sky, 2) },
                { SpotId.Palace_of_Twilight_Sign, new(StageIDs.Palace_of_Twilight, 0) },
                { SpotId.Hyrule_Castle_Sign, new(StageIDs.Hyrule_Castle, 11) },
            };

        public static bool SpotIdHasBmgData(SpotId spotId)
        {
            return spotToBmgData.ContainsKey(spotId);
        }

        private static readonly Dictionary<SpotId, ushort> spotToFliValue =
            new()
            {
                { SpotId.Ordon_Sign, 0x72b0 },
                { SpotId.Sacred_Grove_Sign, 0x7360 },
                { SpotId.Faron_Field_Sign, 0x7381 },
                { SpotId.Faron_Woods_Sign, 0x72d0 },
                { SpotId.Kakariko_Gorge_Sign, 0x7382 },
                { SpotId.Kakariko_Village_Sign, 0x72e0 },
                { SpotId.Kakariko_Graveyard_Sign, 0x7300 },
                { SpotId.Eldin_Field_Sign, 0x7380 },
                { SpotId.North_Eldin_Sign, 0x7383 },
                { SpotId.Death_Mountain_Sign, 0x72f0 },
                { SpotId.Hidden_Village_Sign, 0x73f0 },
                { SpotId.Lanayru_Field_Sign, 0x7384 },
                { SpotId.Beside_Castle_Town_Sign, 0x7390 },
                { SpotId.South_of_Castle_Town_Sign, 0x7391 },
                { SpotId.Castle_Town_Sign, 0x7350 },
                { SpotId.Great_Bridge_of_Hylia_Sign, 0x7385 },
                { SpotId.Lake_Hylia_Sign, 0x7340 },
                { SpotId.Lake_Lantern_Cave_Sign, 0x7210 },
                { SpotId.Lanayru_Spring_Sign, 0x7341 },
                { SpotId.Zoras_Domain_Sign, 0x7320 },
                { SpotId.Upper_Zoras_River_Sign, 0x73e0 },
                { SpotId.Gerudo_Desert_Sign, 0x73b0 },
                { SpotId.Bulblin_Camp_Sign, 0x7370 },
                { SpotId.Snowpeak_Mountain_Sign, 0x7330 },
                { SpotId.Cave_of_Ordeals_Sign, 0x71f0 },
                { SpotId.Forest_Temple_Sign, 0x7060 },
                { SpotId.Goron_Mines_Sign, 0x7030 },
                { SpotId.Lakebed_Temple_Sign, 0x7000 },
                { SpotId.Arbiters_Grounds_Sign, 0x7180 },
                { SpotId.Snowpeak_Ruins_Sign, 0x71b0 },
                { SpotId.Temple_of_Time_Sign, 0x7090 },
                { SpotId.Temple_of_Time_Beyond_Point_Sign, 0x7091 },
                { SpotId.City_in_the_Sky_Sign, 0x70c0 },
                { SpotId.Palace_of_Twilight_Sign, 0x70f0 },
                { SpotId.Hyrule_Castle_Sign, 0x7140 },
            };

        public static bool SpotHasCustomFliValue(SpotId spotId)
        {
            return spotToFliValue.ContainsKey(spotId);
        }

        public static ushort GetFliValueOfSpot(SpotId spotId)
        {
            if (!spotToFliValue.TryGetValue(spotId, out ushort fliValue))
                throw new Exception($"Failed to find fliValue for spotId '{spotId}'.");

            return fliValue;
        }

        public static MessageEntry GetEntryForSpotId(SpotId spotId)
        {
            if (!spotIdToEntry.TryGetValue(spotId, out MsgEntryId msgEntryId))
                throw new Exception($"Failed to find MsgEntryId for SpotId '{spotId}'.");
            return GetEntry(msgEntryId);
        }

        public static MessageEntry GetEntry(MsgEntryId messageId, string message = null)
        {
            if (!idToEntry.TryGetValue(messageId, out MessageEntry entry))
                throw new Exception($"Failed to find MessageEntry for '{messageId}'.");

            if (!StringUtils.isEmpty(message))
                entry.message = message;
            else
                entry.message = "";
            return entry;
        }
    }
}
