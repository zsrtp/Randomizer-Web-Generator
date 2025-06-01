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
        public static MsgNodeInst msgCT_AgithaSign = new(StgBmg.Castle_Town, 0xa2d, 0x456);
        public static MsgNodeInst msgCT_JovaniSign = new(StgBmg.Castle_Town, 0xa2e, 0x457);
        public static MsgNodeInst msgOrdon_LinksHouseSign = new(StgBmg.Ordon_Village, 0x467, 0x658);

        //
        public static MsgNodeInst msg_SeraSlingshotSlot =
            new(StgBmg.Ordon_Village_Interiors, 0x4d7, 0x5AE);
        public static MsgNodeInst msg_SeraSlingshotCantAfford =
            new(StgBmg.Ordon_Village_Interiors, 0x4c9, 0x5B3);
        public static MsgNodeInst msg_SeraSlingshotConfirmation =
            new(StgBmg.Ordon_Village_Interiors, 0x4cb, 0x5B4);
        public static MsgNodeInst msg_SeraSlingshotBought =
            new(StgBmg.Ordon_Village_Interiors, 0x4d0, 0x5B5);
        public static MsgNodeInst msg_SeraSlingshotBought2 =
            new(StgBmg.Ordon_Village_Interiors, 0x4d5, 0x5B6);
        public static MsgNodeInst msgKV_MaloMartHawkeyeSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x43b, 0x307);
        public static MsgNodeInst msgKV_MaloMartHawkeyeCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x434, 0x2D3);
        public static MsgNodeInst msgKV_MaloMartHawkeyeConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x430, 0x2D2);
        public static MsgNodeInst msgKV_MaloMartHawkeyeSoldOut =
            new(StgBmg.Kakariko_Village_Interiors, 0x43d, 0x306);
        public static MsgNodeInst msgKV_MaloMartHawkeyeSoldOutRead =
            new(StgBmg.Kakariko_Village_Interiors, 0x43c, 0x2D4);
        public static MsgNodeInst msgKV_MaloMartWoodenShieldSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x41d, 0x30D);
        public static MsgNodeInst msgKV_MaloMartWoodenShieldCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x415, 0x2C8);
        public static MsgNodeInst msgKV_MaloMartWoodenShieldConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x40f, 0x2C7);
        public static MsgNodeInst msgKV_MaloMartHylianShieldSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x42a, 0x30E);
        public static MsgNodeInst msgKV_MaloMartHylianShieldCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x422, 0x2CC);
        public static MsgNodeInst msgKV_MaloMartHylianShieldConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x41e, 0x2CB);
        public static MsgNodeInst msgKV_MaloMartHylianShieldSoldOut =
            new(StgBmg.Kakariko_Village_Interiors, 0x42f, 0x30B);
        public static MsgNodeInst msgKV_MaloMartHylianShieldSoldOutRead =
            new(StgBmg.Kakariko_Village_Interiors, 0x42c, 0x2D0);

        // If you buy the wooden shield slot before anything else, you will see
        // this one instead for that slot. For Vanilla, I think this might be so
        // the message changes depending on if you bought your Hylian Shield
        // here or somewhere else (since once you buy one you can never buy a
        // 2nd one; "you bought my last one" vs "there are no more"). Wooden
        // shield is only relevant due to custom rando shop slot stuff I think.
        // - isaac
        public static MsgNodeInst msgKV_MaloMartHylianShieldSoldOutRead2 =
            new(StgBmg.Kakariko_Village_Interiors, 0x42e, 0x2E0);
        public static MsgNodeInst msgKV_MaloMartRedPotionSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x44a, 0x305);
        public static MsgNodeInst msgKV_MaloMartRedPotionCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x43e, 0x2D6);
        public static MsgNodeInst msgKV_MaloMartRedPotionConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x441, 0x2D7);
        public static MsgNodeInst msgKV_MaloMartRedPotionBought =
            new(StgBmg.Kakariko_Village_Interiors, 0x446, 0x2D8);
        public static MsgNodeInst msg_ChudleysFineGoodsMagicArmorSlot =
            new(StgBmg.Castle_Town_Shops, 0x3aa, 0x10A);
        public static MsgNodeInst msgCT_MaloMartMagicArmorSlot =
            new(StgBmg.Castle_Town_Shops, 0x612, 0x125);
        public static MsgNodeInst msgCT_MaloMartMagicArmorBought =
            new(StgBmg.Castle_Town_Shops, 0x60e, 0x11E);
        public static MsgNodeInst msgCT_MaloMartMagicArmorSoldOut =
            new(StgBmg.Castle_Town_Shops, 0x614, 0x130);
        public static MsgNodeInst msgCT_GoronRedPotionConfirmationInitial =
            new(StgBmg.Castle_Town_Shops, 0x9ae, 0x3BF);
        public static MsgNodeInst msgCT_GoronRedPotionConfirmationSecond =
            new(StgBmg.Castle_Town_Shops, 0x9b1, 0x3C1);
        public static MsgNodeInst msgCT_GoronRedPotionCantAfford =
            new(StgBmg.Castle_Town_Shops, 0x9b4, 0x3C2);
        public static MsgNodeInst msgCT_GoronLanternOilConfirmationInitial =
            new(StgBmg.Castle_Town_Shops, 0x99d, 0x3B3);
        public static MsgNodeInst msgCT_GoronLanternOilConfirmationSecond =
            new(StgBmg.Castle_Town_Shops, 0x99f, 0x3B5);

        // TODO: test if text is wrong if MDH is not cleared. This is probably
        // the issue the person reported in the discord. Also need to test with
        // the Arrows goron since seems like the same thing. Red potion and
        // Hylian shield ones do not appear to do this from looking at the
        // graph.
        public static MsgNodeInst msgCT_GoronLanternOilCantAfford =
            new(StgBmg.Castle_Town_Shops, 0x97e, 0x3AC); // Also used by FLW index 0x993
        public static MsgNodeInst msgCT_GoronArrowsConfirmationInitial =
            new(StgBmg.Castle_Town, 0x9eb, 0x3D8);
        public static MsgNodeInst msgCT_GoronArrowsConfirmationSecond =
            new(StgBmg.Castle_Town, 0x9ed, 0x3DB);
        public static MsgNodeInst msgCT_GoronShieldConfirmationIntitial =
            new(StgBmg.Castle_Town_Shops, 0xa06, 0x3E3);
        public static MsgNodeInst msgCT_GoronShieldConfirmationSecond =
            new(StgBmg.Castle_Town_Shops, 0xa09, 0x3E5);
        public static MsgNodeInst msg_BarnesBombBagConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x47f, 0x9B);
        public static MsgNodeInst msgKV_BarnesBombBagCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x484, 0xA0); // Also used by FLW index 0x523
        public static MsgNodeInst msgCT_CharloOptsBody = new(StgBmg.Castle_Town, 0x346, 0x355);
        public static MsgNodeInst msgCT_CharloOptsOptions = new(StgBmg.Castle_Town, 0x347, 0x356);
        public static MsgNodeInst msg_FishingHoleBottleSign =
            new(StgBmg.Fishing_Pond, 0x2d5, 0x47A);
        public static MsgNodeInst msg_CoroBuyOptionsConfirmation =
            new(StgBmg.Faron_Woods, 0x6a, 0xDD);
    }

    public class CustomMsgUtils
    {
        private static readonly Dictionary<SpotId, MsgNodeInst> spotIdToVanillaNode =
            new()
            {
                { SpotId.Agithas_Castle_Sign, Node.msgCT_AgithaSign },
                { SpotId.Jovani_House_Sign, Node.msgCT_JovaniSign },
            };

        public static bool TryGetSpotIdVanillaNode(SpotId spotId, out MsgNodeInst node)
        {
            return spotIdToVanillaNode.TryGetValue(spotId, out node);
        }

        private static readonly Dictionary<SpotId, ushort> spotIdToFliValue =
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

        public static bool TryGetCustomSignFliValue(SpotId spotId, out ushort fliValue)
        {
            return spotIdToFliValue.TryGetValue(spotId, out fliValue);
        }

        public static ushort GetFliValueOfSpot(SpotId spotId)
        {
            if (!spotIdToFliValue.TryGetValue(spotId, out ushort fliValue))
                throw new Exception($"Failed to find fliValue for spotId '{spotId}'.");

            return fliValue;
        }
    }
}
