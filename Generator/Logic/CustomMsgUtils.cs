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
        Sera_Slingshot_Slot,
        Sera_Slingshot_Cant_Afford,
        Sera_Slingshot_Confirm_Buy,
        Jovani_House_Sign,

        //TODO: Agitha's castle sign
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
    }

    public class CustomMsgUtils
    {
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
                {
                    MsgEntryId.Sera_Slingshot_Slot,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5AE)
                },
                {
                    MsgEntryId.Sera_Slingshot_Cant_Afford,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B3)
                },
                {
                    MsgEntryId.Sera_Slingshot_Confirm_Buy,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B4)
                },
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
            };

        public static MessageEntry GetEntry(MsgEntryId messageId, string message)
        {
            if (!idToEntry.TryGetValue(messageId, out MessageEntry entry))
                throw new Exception($"Failed to find MessageEntry for '{messageId}'.");

            entry.message = message;
            return entry;
        }
    }
}
