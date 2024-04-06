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

    public enum MessageId
    {
        SeraSlingshotSlot,
        SeraSlingshotCantAfford,
        SeraSlingshotConfirmBuy,
    }

    public class CustomMsgUtils
    {
        private static readonly Dictionary<MessageId, MessageEntry> dog =
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
                    MessageId.SeraSlingshotSlot,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5AE)
                },
                {
                    MessageId.SeraSlingshotCantAfford,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B3)
                },
                {
                    MessageId.SeraSlingshotConfirmBuy,
                    new(StageIDs.Ordon_Village_Interiors, 1, 0x5B4)
                },
            };

        public static MessageEntry GetEntry(MessageId messageId, string message)
        {
            if (!dog.TryGetValue(messageId, out MessageEntry entry))
                throw new Exception($"Failed to find MessageEntry for '{messageId}'.");

            entry.message = message;
            return entry;
        }
    }
}
