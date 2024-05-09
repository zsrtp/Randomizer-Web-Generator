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
        Custom_Sign_Snowpeak,
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
    }

    public class CustomMsgUtils
    {
        private static readonly Dictionary<SpotId, MsgEntryId> spotIdToEntry =
            new()
            {
                { SpotId.Agithas_Castle_Sign, MsgEntryId.Agithas_Castle_Sign },
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
                { SpotId.Snowpeak_Sign, MsgEntryId.Custom_Sign_Snowpeak },
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
                { MsgEntryId.Custom_Sign_Snowpeak, new(StageIDs.Snowpeak, 0, 0x1369) },
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
            };

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
