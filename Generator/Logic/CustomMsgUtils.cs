namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Assets;
    using TPRandomizer.Hints;

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

    public class InfInst
    {
        public StgBmg stgBmg { get; private set; }
        public ushort infIdx { get; private set; }

        public InfInst(StgBmg stgBmg, ushort infIdx)
        {
            this.stgBmg = stgBmg;
            this.infIdx = infIdx;
        }
    }

    public class Node
    {
        /* zel_00 (global) */
        public static NodeInst br_TalkToMidnaRootNode = new(StgBmg.zel_00, 0x8f);
        public static NodeInst br_Z0GeneriCtxBranch = new(StgBmg.zel_00, 0x199);
        public static NodeInst ev_Z0GenericCtxEvent = new(StgBmg.zel_00, 0x1a4);
        public static NodeInst ev_MidnaTwoOptsInitEv = new(StgBmg.zel_00, 0x1a0);
        public static MsgNodeInst msg_MidnaTwoOptsBody = new(StgBmg.zel_00, 0x19f, 0x5de);
        public static MsgNodeInst msg_MidnaTwoOptsOptions = new(StgBmg.zel_00, 0x1a1, 0x5df);
        public static NodeInst br_MidnaTwoOptsResultBranch = new(StgBmg.zel_00, 0x1a3);
        public static MsgNodeInst msg_Z0_0x28 = new(StgBmg.zel_00, 0x28, 0xa11);
        public static MsgNodeInst msg_Z0_0x4d = new(StgBmg.zel_00, 0x4d, 0xa29);
        public static NodeInst zel00_FFFF = new(StgBmg.zel_00, 0xFFFF);

        /* zel_01 // Ordon */
        public static MsgNodeInst msg_LinksHouseSign = new(StgBmg.Ordon_Village, 0x467, 0x658);
        public static MsgNodeInst msg_SeraSlingshotSlot =
            new(StgBmg.Ordon_Village_Interiors, 0x4d7, 0x5AE);
        public static MsgNodeInst msg_SeraSlingshotCantAfford =
            new(StgBmg.Ordon_Village_Interiors, 0x4c9, 0x5B3);
        public static NodeInst br_SeraSlingshotCheckCanAfford =
            new(StgBmg.Ordon_Village_Interiors, 0x4ca);
        public static MsgNodeInst msg_SeraSlingshotConfirmation =
            new(StgBmg.Ordon_Village_Interiors, 0x4cb, 0x5B4);
        public static NodeInst ev_SeraSlingshotPayPrice =
            new(StgBmg.Ordon_Village_Interiors, 0x4cf);
        public static MsgNodeInst msg_SeraSlingshotBought =
            new(StgBmg.Ordon_Village_Interiors, 0x4d0, 0x5B5);
        public static MsgNodeInst msg_SeraSlingshotBought2 =
            new(StgBmg.Ordon_Village_Interiors, 0x4d5, 0x5B6);

        /* zel_02 // KV (interiors), KGY (interiors) */
        public static NodeInst br_KakMaloMartHylianShieldCanAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x421);
        public static NodeInst br_BarnesWaterBombSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x62c);
        public static NodeInst br_BarnesBomblingsSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x775);
        public static NodeInst br_BarnesBombsSlot = new(StgBmg.Kakariko_Village_Interiors, 0x47c);
        public static MsgNodeInst msg_BarnesNoBombBag =
            new(StgBmg.Kakariko_Village_Interiors, 0x47d, 0x9a);
        public static NodeInst ev_BarnesNoBombBagMenu =
            new(StgBmg.Kakariko_Village_Interiors, 0x47e);
        public static NodeInst ev_KakMaloMartHylianShieldPay =
            new(StgBmg.Kakariko_Village_Interiors, 0x424);
        public static NodeInst ev_KakMaloMartHylianShieldBeforePay =
            new(StgBmg.Kakariko_Village_Interiors, 0x429);
        public static MsgNodeInst msg_KakMaloMartHawkeyeSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x43b, 0x307);
        public static MsgNodeInst msg_KakMaloMartHawkeyeCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x434, 0x2D3);
        public static MsgNodeInst msg_KakMaloMartHawkeyeConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x430, 0x2D2);
        public static MsgNodeInst msg_KakMaloMartHawkeyeSoldOut =
            new(StgBmg.Kakariko_Village_Interiors, 0x43d, 0x306);
        public static MsgNodeInst msg_KakMaloMartHawkeyeSoldOutRead =
            new(StgBmg.Kakariko_Village_Interiors, 0x43c, 0x2D4);
        public static MsgNodeInst msg_KakMaloMartWoodenShieldSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x41d, 0x30D);
        public static MsgNodeInst msg_KakMaloMartWoodenShieldCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x415, 0x2C8);
        public static MsgNodeInst msg_KakMaloMartWoodenShieldConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x40f, 0x2C7);
        public static MsgNodeInst msg_KakMaloMartHylianShieldSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x42a, 0x30E);
        public static MsgNodeInst msg_KakMaloMartHylianShieldCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x422, 0x2CC);
        public static MsgNodeInst msg_KakMaloMartHylianShieldConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x41e, 0x2CB);
        public static MsgNodeInst msg_KakMaloMartHylianShieldSoldOut =
            new(StgBmg.Kakariko_Village_Interiors, 0x42f, 0x30B);
        public static MsgNodeInst msg_KakMaloMartHylianShieldSoldOutRead =
            new(StgBmg.Kakariko_Village_Interiors, 0x42c, 0x2D0);

        // If you buy the wooden shield slot before anything else, you will see
        // this one instead for that slot. For Vanilla, I think this might be so
        // the message changes depending on if you bought your Hylian Shield
        // here or somewhere else (since once you buy one you can never buy a
        // 2nd one; "you bought my last one" vs "there are no more"). Wooden
        // shield is only relevant due to custom rando shop slot stuff I think.
        // - isaac
        public static MsgNodeInst msg_KakMaloMartHylianShieldSoldOutRead2 =
            new(StgBmg.Kakariko_Village_Interiors, 0x42e, 0x2E0);
        public static MsgNodeInst msg_KakMaloMartRedPotionSlot =
            new(StgBmg.Kakariko_Village_Interiors, 0x44a, 0x305);
        public static MsgNodeInst msg_KakMaloMartRedPotionCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x43e, 0x2D6);
        public static MsgNodeInst msg_KakMaloMartRedPotionConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x441, 0x2D7);
        public static MsgNodeInst msg_KakMaloMartRedPotionBought =
            new(StgBmg.Kakariko_Village_Interiors, 0x446, 0x2D8);
        public static MsgNodeInst msg_BarnesBombBagConfirmation =
            new(StgBmg.Kakariko_Village_Interiors, 0x47f, 0x9B);
        public static MsgNodeInst msg_BarnesBombBagCantAfford =
            new(StgBmg.Kakariko_Village_Interiors, 0x484, 0xA0); // Also used by FLW index 0x523

        /* zel_03 // Death Mountain (interiors) */
        /* zel_04 // CT (interiors), sewers, HC in credits */
        public static MsgNodeInst msg_AgithaSign = new(StgBmg.Castle_Town, 0xa2d, 0x456);
        public static MsgNodeInst msg_JovaniSign = new(StgBmg.Castle_Town, 0xa2e, 0x457);
        public static MsgNodeInst msg_ChudleysFineGoodsMagicArmorSlot =
            new(StgBmg.Castle_Town_Shops, 0x3aa, 0x10A);
        public static MsgNodeInst msg_CtMaloMartMagicArmorSlot =
            new(StgBmg.Castle_Town_Shops, 0x612, 0x125);
        public static MsgNodeInst msg_CtMaloMartMagicArmorBought =
            new(StgBmg.Castle_Town_Shops, 0x60e, 0x11E);
        public static MsgNodeInst msg_CtMaloMartMagicArmorSoldOut =
            new(StgBmg.Castle_Town_Shops, 0x614, 0x130);
        public static MsgNodeInst msg_CtGoronRedPotionConfirmationInitial =
            new(StgBmg.Castle_Town_Shops, 0x9ae, 0x3BF);
        public static MsgNodeInst msg_CtGoronRedPotionConfirmationSecond =
            new(StgBmg.Castle_Town_Shops, 0x9b1, 0x3C1);
        public static MsgNodeInst msg_CtGoronRedPotionCantAfford =
            new(StgBmg.Castle_Town_Shops, 0x9b4, 0x3C2);
        public static MsgNodeInst msg_CtGoronLanternOilConfirmationInitial =
            new(StgBmg.Castle_Town_Shops, 0x99d, 0x3B3);
        public static MsgNodeInst msg_CtGoronLanternOilConfirmationSecond =
            new(StgBmg.Castle_Town_Shops, 0x99f, 0x3B5);
        public static MsgNodeInst msg_CtGoronLanternOilCantAfford =
            new(StgBmg.Castle_Town_Shops, 0x97e, 0x3AC); // Also used by FLW index 0x993
        public static MsgNodeInst msg_CtGoronArrowsConfirmationInitial =
            new(StgBmg.Castle_Town, 0x9eb, 0x3D8);
        public static MsgNodeInst msg_CtGoronArrowsConfirmationSecond =
            new(StgBmg.Castle_Town, 0x9ed, 0x3DB);
        public static MsgNodeInst msg_CtGoronShieldConfirmationIntitial =
            new(StgBmg.Castle_Town_Shops, 0xa06, 0x3E3);
        public static MsgNodeInst msg_CtGoronShieldConfirmationSecond =
            new(StgBmg.Castle_Town_Shops, 0xa09, 0x3E5);
        public static MsgNodeInst msg_CharloOptsBody = new(StgBmg.Castle_Town, 0x346, 0x355);
        public static MsgNodeInst msg_CharloOptsOptions = new(StgBmg.Castle_Town, 0x347, 0x356);

        /* zel_05 // All dungeons, (mini)bosses, grottos, caves, LA cutscene */
        /* zel_06 // FW (interiors), SP, SG, BC, GD, Mirror Chamber, HV (interiors), Hidden Skill */
        public static MsgNodeInst msg_CoroBuyOptionsConfirmation =
            new(StgBmg.Faron_Woods, 0x6a, 0xDD);

        /* zel_07 // ZD, Fishing Hole, Hena's house */
        public static MsgNodeInst msg_FishingHoleBottleSign =
            new(StgBmg.Fishing_Pond, 0x2d5, 0x47A);

        /* zel_08 // HF, Outside CT, LH, UZR, Zora's River, KB2, Title Screen */
    }

    // We can update strings by INF rather than FLW for ones which either do not
    // show up in msgFlow conversations or which show up in several.
    public class Inf
    {
        public static InfInst zel00_ChooseAQuestLog = new(StgBmg.zel_00, 0x42);
        public static InfInst zel00_RatioCheckSample = new(StgBmg.zel_00, 0x556);

        // Would have to shrink the font size on this one
        // public static InfInst zel00_TvSettings = new(StgBmg.zel_00, 0x55b);

        // Note: we use INF indexes for the Midna menus since they are used by many nodes.
        // This is only for the sake of clarity; strReplacements are always done by INF.
        public static InfInst zel00_MidnaOpts_WarpTalk = new(StgBmg.zel_00, 0xa2d);
        public static InfInst zel00_MidnaOpts_TransToWolfTalk = new(StgBmg.zel_00, 0x5df);
        public static InfInst zel00_MidnaOpts_TransToHumanTalk = new(StgBmg.zel_00, 0x5e0);
        public static InfInst zel00_MidnaOpts_TransToWolfWarpTalk = new(StgBmg.zel_00, 0xa2e);
        public static InfInst zel00_MidnaOpts_TransToHumanWarpTalk = new(StgBmg.zel_00, 0xbba);
    }

    public class CustomMsgUtils
    {
        private static readonly Dictionary<SpotId, MsgNodeInst> spotIdToVanillaNode =
            new()
            {
                { SpotId.Agithas_Castle_Sign, Node.msg_AgithaSign },
                { SpotId.Jovani_House_Sign, Node.msg_JovaniSign },
            };

        public static bool TryGetSpotIdVanillaNode(SpotId spotId, out MsgNodeInst node)
        {
            return spotIdToVanillaNode.TryGetValue(spotId, out node);
        }

        private static readonly Dictionary<SpotId, ushort> spotIdToFlowId =
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

        public static bool TryGetCustomSignFlowId(SpotId spotId, out ushort flowId)
        {
            return spotIdToFlowId.TryGetValue(spotId, out flowId);
        }

        public static ushort GetCustomSignFlowId(SpotId spotId)
        {
            if (!spotIdToFlowId.TryGetValue(spotId, out ushort flowId))
                throw new Exception($"Failed to find flowId for spotId '{spotId}'.");

            return flowId;
        }
    }

    public enum QueryIdx : ushort
    {
        query005 = 0,
        query001 = 1,
        query002 = 2,
        query003 = 3,
        query006 = 4,
        query007 = 5,
        query004 = 6,
        query008 = 7,
        query009 = 8,
        query010 = 9,
        query011 = 10,
        query012 = 11,
        query013 = 12,
        query014 = 13,
        query015 = 14,
        query016 = 15,
        query017 = 16,
        query018 = 17,
        query019 = 18,
        query020 = 19,
        query021 = 20,
        query022 = 21,
        query023 = 22,
        query024 = 23,
        query025 = 24,
        query026 = 25,
        query027 = 26,
        query028 = 27,
        query029 = 28,
        query030 = 29,
        query031 = 30,
        query032 = 31,
        query033 = 32,
        query034 = 33,
        query035 = 34,
        query036 = 35,
        query037 = 36,
        query038 = 37,
        query039 = 38,
        query040 = 39,
        query041 = 40,
        query042 = 41,
        query043 = 42,
        query044 = 43,
        query045 = 44,
        query046 = 45,
        query047 = 46,
        query048 = 47,
        query049 = 48,
        query050 = 49,
        query051 = 50,
        query052 = 51,
        query053 = 52,
        customQuery053_returnParams = 53,
        customQuery054_canChangeTod = 54,
    }

    public enum EventIdx : byte
    {
        event000 = 0,
        event001 = 1,
        event002 = 2,
        event003 = 3,
        event004 = 4,
        event005 = 5,
        event006 = 6,
        event007 = 7,
        event008 = 8,
        event009 = 9,
        event010 = 10,
        event011 = 11,
        event012 = 12,
        event013 = 13,
        event014 = 14,
        event015 = 15,
        event016 = 16,
        event017 = 17,
        event018 = 18,
        event019 = 19,
        event020 = 20,
        event021 = 21,
        event022 = 22,
        event023 = 23,
        event024 = 24,
        event025 = 25,
        event026 = 26,
        event027 = 27,
        event028 = 28,
        event029 = 29,
        event030 = 30,
        event031 = 31,
        event032 = 32,
        event033 = 33,
        event034 = 34,
        event035 = 35,
        event036 = 36,
        event037 = 37,
        event038 = 38,
        event039 = 39,
        event040 = 40,
        event041 = 41,
        event042 = 42,
        customEvent043_nop = 43,
        customEvent044_changeTimeOfDay = 44,
    }
}
