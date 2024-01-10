namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TPRandomizer.SSettings.Enums;

    /// <summary>
    /// summary text.
    /// </summary>
    public class CustomMessages
    {
        static string messageSpeedFast = "\x1A\x05\x00\x00\x01";
        static string messageSpeedSlow = "\x1A\x05\x00\x00\x02";
        static string messageColorWhite = "\x1A\x06\xFF\x00\x00\x00";
        static string messageColorRed = "\x1A\x06\xFF\x00\x00\x01";
        static string messageColorGreen = "\x1A\x06\xFF\x00\x00\x02";
        static string messageColorLightBlue = "\x1A\x06\xFF\x00\x00\x03";
        static string messageColorYellow = "\x1A\x06\xFF\x00\x00\x04";
        static string messageColorPurple = "\x1A\x06\xFF\x00\x00\x06";
        static string messageColorOrange = "\x1A\x06\xFF\x00\x00\x08";
        static string messageColorDarkGreen = "\x1A\x06\xFF\x00\x00\x09";
        static string messageColorBlue = "\x1A\x06\xFF\x00\x00\x0A";
        static string messageColorSilver = "\x1A\x06\xFF\x00\x00\x0B";
        static string playerName = "\x1A\x05\x00\x00\x00";
        static string messageOption1 = "\x1A\x06\x00\x00\x09\x01";
        static string messageOption2 = "\x1A\x06\x00\x00\x09\x02";
        static string messageOption3 = "\x1A\x06\x00\x00\x09\x03";

        public static string hylianShieldShopItemText = Item.Hylian_Shield.ToString();
        public static string barnesBombShopItemText = Item.Filled_Bomb_Bag.ToString();
        public static string charloDonationItemText = Item.Piece_of_Heart.ToString();

        //static string messageIconR = "\x1A\x05\x00\x00\x0E";
        //static string messageIconA = "\x1A\x05\x00\x00\x0A";
        //static string messageIconX = "\x1A\x05\x00\x00\x0F";
        //static string messageIconY = "\x1A\x05\x00\x00\x10";

        public struct MessageEntry
        {
            public short messageID;
            public byte stageIDX;
            public byte roomIDX;
            public string message;
        }

        public Dictionary<byte, MessageEntry[]> CustomPALMessageDictionary =
            new()
            {
                { 0, englishMessages },
                { 1, germanMessages },
                { 2, frenchMessages },
                { 3, spanishMessages },
                { 4, italianMessages }
            };

        List<MessageEntry[]> listOfLanguageEntries =
            new()
            {
                englishMessages,
                germanMessages,
                frenchMessages,
                spanishMessages,
                italianMessages,
                japaneseMessages
            };

        public Dictionary<byte, MessageEntry[]> CustomUSMessageDictionary =
            new() { { 0, englishMessages }, };
        public Dictionary<byte, MessageEntry[]> CustomJPMessageDictionary =
            new() { { 0, japaneseMessages } };

        public static MessageEntry[] englishMessages =
        {
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x66,
                message =
                    messageSpeedFast
                    + "This is a "
                    + messageColorRed
                    + "test "
                    + messageColorWhite
                    + "hint!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xED,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF0,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF1,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF2,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorOrange
                    + "Bulblin Camp"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x96,
                message =
                    messageSpeedSlow
                    + "You got the "
                    + messageColorRed
                    + "Shadow Crystal"
                    + messageColorWhite
                    + "!\nThis is a dark manifestation\nof "
                    + messageColorRed
                    + "Zant's "
                    + messageColorWhite
                    + "power that allows\nyou to transform at will!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11A,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11B,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x120,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x121,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x122,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFD,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x110,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x111,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF6,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF7,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF8,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF9,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x145,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Ending Blow"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x146,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Shield Attack"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x147,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Back Slice"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x148,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Helm Splitter"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x149,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Mortal Draw"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Jump Strike"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14B,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Great Spin"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xB0,
                message =
                    messageSpeedFast
                    + "Power has been restored to\nthe "
                    + messageColorRed
                    + "Dominion Rod"
                    + messageColorWhite
                    + "! Now it can\nbe used to imbude statues\nwith life in the present!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13F,
                message =
                    messageSpeedFast
                    + "You found the first "
                    + messageColorRed
                    + "Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x140,
                message =
                    messageSpeedFast
                    + "You found the second "
                    + messageColorRed
                    + "Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x141,
                message =
                    messageSpeedFast
                    + "You found the third "
                    + messageColorRed
                    + "Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x142,
                message =
                    messageSpeedFast
                    + "You found the fourth "
                    + messageColorRed
                    + "Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x143,
                message =
                    messageSpeedFast
                    + "You found the fifth "
                    + messageColorRed
                    + "Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "Fused Shadow!\n"
                    + messageColorWhite
                    + "It seems to have some "
                    + messageColorGreen
                    + "moss"
                    + messageColorWhite
                    + "\ngrowing on it.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13D,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "second Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorRed
                    + "warm"
                    + messageColorWhite
                    + " to\nthe touch.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13E,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "final Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorBlue
                    + "wet"
                    + messageColorWhite
                    + " and\nsmells like fish.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x109,
                message =
                    messageSpeedFast
                    + "You got the second shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nhas a beautiful shine to it\nand feels slightly "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10A,
                message =
                    messageSpeedFast
                    + "You got the third shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nis covered in dirt and\n"
                    + messageColorDarkGreen
                    + "webs"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10B,
                message =
                    messageSpeedFast
                    + "You got the final shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nfeels lighter than "
                    + messageColorYellow
                    + "air"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF3,
                message =
                    messageSpeedFast
                    + "A "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + " wind blows.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xBBB, // Talk to Midna
                message =
                    "What is it, "
                    + playerName
                    + "?"
                    + messageOption1
                    + "Transform\n"
                    + messageOption2
                    + "Warp\n"
                    + messageOption3
                    + "Change time of day"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x30E, // Hylian Shield Check
                message =
                    messageColorOrange
                    + hylianShieldShopItemText
                    + ": "
                    + messageColorPurple
                    + "200 Rupees\n"
                    + messageColorWhite
                    + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x30E, // Barnes Bomb Bag Text. Need to update messageID
                message =
                    "I've got a special offer goin' right\nnow: my "
                    + messageColorRed
                    + barnesBombShopItemText
                    + ",\njust "
                    + messageColorPurple
                    + "120 Rupees"
                    + messageColorWhite
                    + "! How 'bout that?"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x30E, // Charlo Donation Text. Need to update messageID
                message =
                    "For a "
                    + messageColorRed
                    + charloDonationItemText
                    + "...\nWould you please make a donation?"
                    + messageOption1
                    + "100 Rupees\n"
                    + messageOption2
                    + "50 Rupees\n"
                    + messageOption3
                    + "Sorry..."
            },
        };
        public static MessageEntry[] germanMessages =
        {
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xE9, // Forest Temple Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir Türen\nim "
                    + messageColorGreen
                    + "Waldschrein"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEA, // Goron Mines Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir Türen\nin den "
                    + messageColorRed
                    + "Minen der Goronen"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEB, // Lakebed Tample Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir Türen\nim "
                    + messageColorBlue
                    + "Seeschrein"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEC, // Arbiter's Grounds Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir Türen\nin der "
                    + messageColorOrange
                    + "Wüstenburg"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xED, // Snowpeak Ruins Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir Türen\nin der "
                    + messageColorLightBlue
                    + "Bergruine"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEE, // Temple of Time Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir\nTüren im "
                    + messageColorDarkGreen
                    + "Zeitschrein"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEF, // City in The Sky Small Ley
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir\nTüren in "
                    + messageColorYellow
                    + "Kumula"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF0, // Palace of Twilight Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir\nTüren im "
                    + messageColorPurple
                    + "Schattenpalast"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF1, // Hyrule Castle Small Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir\nTüren auf "
                    + messageColorSilver
                    + "Schloss Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF2, // Bulblin Camp Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "kleinen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet das\nTor im "
                    + messageColorOrange
                    + "Camp der Bulblins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x96, // Shadow Crystal
                message =
                    messageSpeedSlow
                    + "Du erhältst den "
                    + messageColorRed
                    + "Nachtkristall"
                    + messageColorWhite
                    + "!\nDiese dunkle Manifestation der\nMacht "
                    + messageColorRed
                    + "Zantos "
                    + messageColorWhite
                    + "erlaubt es dir, dich\nnach Belieben zu verwandeln!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11A, // Forest Temple Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie im "
                    + messageColorGreen
                    + "Waldschrein"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11B, // Goron Mines Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie in den "
                    + messageColorRed
                    + "Minen der\nGoronen"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11C, // Lakebed Temple Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie im "
                    + messageColorBlue
                    + "Seeschrein"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11D, // Arbiters Grounds Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie in der "
                    + messageColorOrange
                    + "Wüstenburg"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11E, // Snowpeak Ruins Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie in der "
                    + messageColorLightBlue
                    + "Bergruine"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11F, // Temple of Time Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie im "
                    + messageColorDarkGreen
                    + "Zeitschrein"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x120, // City in the Sky Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie in "
                    + messageColorYellow
                    + "Kumula"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x121, // Palace of Twilight Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie im "
                    + messageColorPurple
                    + "Schattenpalast"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x122, // Hyrule Castle Dungeon Map
                message =
                    messageSpeedFast
                    + "Du erhältst eine "
                    + messageColorRed
                    + "Dungeon-Karte"
                    + messageColorWhite
                    + "!\nVerwende sie auf "
                    + messageColorSilver
                    + "Schloss Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFD, // Forest Temple Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nim "
                    + messageColorGreen
                    + "Waldschrein "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFE, // Goron Mines Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nin den "
                    + messageColorRed
                    + "Minen der Goronen "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFF, // Lakebed Temple Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nim "
                    + messageColorBlue
                    + "Seeschrein "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10C, // Arbiters Grounds Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nin der "
                    + messageColorOrange
                    + "Wüstenburg "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10D, // Snowpeak Ruins Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nin der "
                    + messageColorLightBlue
                    + "Bergruine "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10E, // Temple of Time Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nim "
                    + messageColorDarkGreen
                    + "Zeitschrein "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10F, // City in The Sky Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nin "
                    + messageColorYellow
                    + "Kumula "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x110, // Palace of Twilight Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nim "
                    + messageColorPurple
                    + "Schattenpalast "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x111, // Hyrule Castle Compass
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "Kompass"
                    + messageColorWhite
                    + "!\nEr zeigt dir, wo du die Schätze\nauf "
                    + messageColorSilver
                    + "Schloss Hyrule "
                    + messageColorWhite
                    + "findest."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF6, // Forest Temple Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir den\nWeg zum Boss des "
                    + messageColorGreen
                    + "Waldschreins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF7, // Lakebed Temple Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir den\nWeg zum Boss der "
                    + messageColorBlue
                    + "Seeschreins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF8, // Arbiters Grounds Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir den\nWeg zum Boss der "
                    + messageColorOrange
                    + "Wüstenburg"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF9, // Temple of Time Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir den\nWeg zum Boss des "
                    + messageColorDarkGreen
                    + "Zeitschreins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFA, // City in The Sky Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir den\nWeg zum Boss von "
                    + messageColorYellow
                    + "Kumula"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFB, // Palace of Twilight Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir\nden Weg zum Boss\ndes "
                    + messageColorPurple
                    + "Schattenpalasts"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFC, // Hyrule Castle Big Key
                message =
                    messageSpeedFast
                    + "Du erhältst einen "
                    + messageColorRed
                    + "großen\nSchlüssel"
                    + messageColorWhite
                    + "! Er öffnet dir\nden Weg zum Boss\nvon "
                    + messageColorSilver
                    + "Schloss Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x145, // Ending Blow
                message =
                    messageSpeedFast
                    + "Du erlernst den "
                    + messageColorRed
                    + "Fangstoß"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x146, // Shield Attack
                message =
                    messageSpeedFast
                    + "Du erlernst die "
                    + messageColorRed
                    + "Schildattacke"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x147, // Back Slice
                message =
                    messageSpeedFast
                    + "Du erlernst den "
                    + messageColorRed
                    + "Rundumhieb"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x148, // Helm Splitter
                message =
                    messageSpeedFast
                    + "Du erlernst den "
                    + messageColorRed
                    + "Helmspalter"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x149, // Mortal Draw
                message =
                    messageSpeedFast
                    + "Du erlernst das "
                    + messageColorRed
                    + "Blankziehen"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14A, // Jump Strike
                message =
                    messageSpeedFast
                    + "Du erlernst die\n"
                    + messageColorRed
                    + "Riesensprungattacke"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14B, // Great Spin
                message =
                    messageSpeedFast
                    + "Du erlernst die\n"
                    + messageColorRed
                    + "Riesenwirbelattacke"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xB0, // Dominion Rod
                message =
                    messageSpeedFast
                    + "Der "
                    + messageColorRed
                    + "Kopierstab "
                    + messageColorWhite
                    + "hat wieder\nmagische Kraft! Verwende ihn,\num den antiken Statuen wieder\nLeben einzuhauchen."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13F, // First Skybook Character
                message =
                    messageSpeedFast
                    + "Du erhältst das erste\n"
                    + messageColorRed
                    + "kumulanische Zeichen"
                    + messageColorWhite
                    + "! Damit\nkannst du einen Teil des\nKumulaner-Dokuments lesen!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x140, // Second Skybook Character
                message =
                    messageSpeedFast
                    + "Du erhältst das zweite\n"
                    + messageColorRed
                    + "kumulanische Zeichen"
                    + messageColorWhite
                    + "! Damit\nkannst du einen Teil des\nKumulaner-Dokuments lesen!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x141, // Third Skybook Character
                message =
                    messageSpeedFast
                    + "Du erhältst das dritte\n"
                    + messageColorRed
                    + "kumulanische Zeichen"
                    + messageColorWhite
                    + "! Damit\nkannst du einen Teil des\nKumulaner-Dokuments lesen!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x142, // Fourth Skybook Character
                message =
                    messageSpeedFast
                    + "Du erhältst das vierte\n"
                    + messageColorRed
                    + "kumulanische Zeichen"
                    + messageColorWhite
                    + "! Damit\nkannst du einen Teil des\nKumulaner-Dokuments lesen!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x143, // Fifth Skybook Character
                message =
                    messageSpeedFast
                    + "Du erhältst das fünfte\n"
                    + messageColorRed
                    + "kumulanische Zeichen"
                    + messageColorWhite
                    + "! Damit\nkannst du einen Teil des\nKumulaner-Dokuments lesen!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13C, // First Fused Shadow
                message =
                    messageSpeedFast
                    + "Du erhältst einen\n"
                    + messageColorRed
                    + "Schattenkristall"
                    + messageColorWhite
                    + "! Es scheint,\nals würde etwas "
                    + messageColorGreen
                    + "Moos\n"
                    + messageColorWhite
                    + "darauf wachsen.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13D, // Second Fused Shadow
                message =
                    messageSpeedFast
                    + "Du erhältst den zweiten\n"
                    + messageColorRed
                    + "Schattenkristall"
                    + messageColorWhite
                    + "! Er fühlt sich\n"
                    + messageColorRed
                    + "warm "
                    + messageColorWhite
                    + "an.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13E, // Third Fused Shadow
                message =
                    messageSpeedFast
                    + "Du erhältst den letzten\n"
                    + messageColorRed
                    + "Schattenkristall"
                    + messageColorWhite
                    + "! Ein Geruch\nvon "
                    + messageColorBlue
                    + "Wasser und Fisch "
                    + messageColorWhite
                    + "steigt\ndir in die Nase.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x109, // Second Mirror Shard
                message =
                    messageSpeedFast
                    + "Du erhältst die zweite\nScherbe des "
                    + messageColorRed
                    + "Schattenspiegels"
                    + messageColorWhite
                    + "!\nDie "
                    + messageColorLightBlue
                    + "vereiste Oberfläche\n"
                    + messageColorWhite
                    + "glitzert im Licht um dich herum.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10A, // Third Mirror Shard
                message =
                    messageSpeedFast
                    + "Du erhältst die dritte\nScherbe des "
                    + messageColorRed
                    + "Schattenspiegels"
                    + messageColorWhite
                    + "!\nSie ist übersäht mit Staub und\n"
                    + messageColorDarkGreen
                    + "Spinnweben"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10B, // Final Mirror Shard
                message =
                    messageSpeedFast
                    + "Du erhältst die letzte\nScherbe des "
                    + messageColorRed
                    + "Schattenspiegels"
                    + messageColorWhite
                    + "!\nSie fühlt sich leicht wie eine\n"
                    + messageColorYellow
                    + "Feder "
                    + messageColorWhite
                    + "an.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF3,
                message =
                    messageSpeedFast
                    + "A "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + " wind blows.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xBBB, // Talk to Midna
                message =
                    "Was ist los, "
                    + playerName
                    + "?"
                    + messageOption1
                    + "Verwandeln\n"
                    + messageOption2
                    + "Teleportieren\n"
                    + messageOption3
                    + "Tageszeit ändern"
            },
        };

        public static MessageEntry[] frenchMessages =
        {
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xE9, // Forest Temple Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au \n"
                    + messageColorGreen
                    + "Temple Sylvestre"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEA, // Goron Miens Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée aux\n"
                    + messageColorRed
                    + "Mines Goron"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEB, // Lakebed Temple Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au \n"
                    + messageColorBlue
                    + "Temple Abyssal"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEC, // Arbiter's Grounds
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée dans la\n"
                    + messageColorOrange
                    + "Tour du Jugement"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xED, // Snowpeak Ruins Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée aux\n"
                    + messageColorLightBlue
                    + "Ruines des Pics Blancs"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEE, // Temple of Time Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au \n"
                    + messageColorDarkGreen
                    + "Temple du Temps"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEF, // City in The Sky Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée à\n"
                    + messageColorYellow
                    + "Célestia"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF0, // Palace of Twilight Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorPurple
                    + "Palais du Crépuscule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF1, // Hyrule Castle Small Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorSilver
                    + "Château d'Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF2, // Bulblin Camp Key
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorOrange
                    + "Camp Bulblin"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x96, // Shadow Crystal
                message =
                    messageSpeedFast
                    + "Vous obtenez le "
                    + messageColorRed
                    + "Crystal Maudit"
                    + messageColorWhite
                    + "!\nLa sombre manifestation des\npouvoirs de Xanto qui permet\nde se transformer à volonté!"
            },
            // Dungeon Maps
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11a, // Forest Temple Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient au\n"
                    + messageColorGreen
                    + "Temple Sylvestre"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11b, // Goron Mines Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient aux\n"
                    + messageColorRed
                    + "Mines Goron"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11c, // Lakebed Temple Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient au\n"
                    + messageColorBlue
                    + "Temple Abyssal"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11d, // Arbiters Grounds Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient à la\n"
                    + messageColorOrange
                    + "Tour du Jugement"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11e, // Snowpeak Ruins Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient aux\n"
                    + messageColorLightBlue
                    + "Ruines des Pics Blancs"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11f, // Temple of Time Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient au\n"
                    + messageColorDarkGreen
                    + "Temple du Temps"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x120, // City in The Sky Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient à\n"
                    + messageColorYellow
                    + "Célestia"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x121, // Palace of Twilight Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient au\n"
                    + messageColorPurple
                    + "Palais du Crépuscule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x122, // Hyrule Castle Dungeon Map
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "! Elle appartient au\n"
                    + messageColorSilver
                    + "Château d'Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFD, // Forest Temple Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient au \n"
                    + messageColorGreen
                    + "Temple Sylvestre"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFE, // Goron Mines Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient aux \n"
                    + messageColorRed
                    + "Mines Goron"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFF, // Lakebed temple Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient au \n"
                    + messageColorBlue
                    + "Temple Abyssal"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10C, // Arbiters Grounds Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient à la \n"
                    + messageColorOrange
                    + "Tour du Jugement"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10D, // Snowpeak Ruins Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient aux \n"
                    + messageColorLightBlue
                    + "Ruines des Pics Blancs"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10E, // Temple of Time Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient au \n"
                    + messageColorDarkGreen
                    + "Temple du Temps"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10F, // City in The Sky Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient à "
                    + messageColorYellow
                    + "Célestia"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x110, // Palace of Twilight Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient au \n"
                    + messageColorPurple
                    + "Palais du Crépuscule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x111, // Hyrule Castle Compass
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "boussole"
                    + messageColorWhite
                    + "!\nElle appartient au \n"
                    + messageColorSilver
                    + "Château d'Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF6, // Forest Temple Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorGreen
                    + "Temple Sylvestre"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF7, // Lakebed Temple Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorBlue
                    + "Temple Abyssal"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF8, // Arbiters Grounds Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée dans la\n"
                    + messageColorOrange
                    + "Tour du Jugement"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF9, // Temple of Time Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorDarkGreen
                    + "Temple du Temps"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFA, // City in The Sky Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée à\n"
                    + messageColorYellow
                    + "Célestia"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFB, // Palace of Twilight Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorPurple
                    + "Palais du Crépuscule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFC, // Hyrule Castle Big Key
                message =
                    messageSpeedFast
                    + "Vous obtenez "
                    + messageColorRed
                    + "la grande clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorSilver
                    + "Château d'Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x145, // Ending Blow
                message =
                    messageSpeedFast
                    + "Vous avez appris le "
                    + messageColorRed
                    + "Coup de\nGrâce"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x146, // Shield Attack
                message =
                    messageSpeedFast
                    + "Vous avez appris la "
                    + messageColorRed
                    + "Charge\nBouclier"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x147, // Back Slice
                message =
                    messageSpeedFast
                    + "Vous avez appris le "
                    + messageColorRed
                    + "Coup à\nRevers"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x148, // Helm Splitter
                message =
                    messageSpeedFast
                    + "Vous avez appris le "
                    + messageColorRed
                    + "\nBrise-Casque"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x149, // Mortal Draw
                message =
                    messageSpeedFast
                    + "Vous avez appris le "
                    + messageColorRed
                    + "Coup\nÉclair"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14A, // Jump Strike
                message =
                    messageSpeedFast
                    + "Vous avez appris le "
                    + messageColorRed
                    + "Coup\nPlongé"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14B, // Great Spin
                message =
                    messageSpeedFast
                    + "Vous avez appris "
                    + messageColorRed
                    + "l'Attaque\nTourbillon"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xB0, // Dominion Rod
                message =
                    messageSpeedFast
                    + "Le "
                    + messageColorRed
                    + "bâton Anima "
                    + messageColorWhite
                    + "a recouvré ses\npouvoirs magiques! Il peut\nmaintenant être utilisé pour\ninsuffler la vie aux statues\ndans le présent!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13F, // First Sky Character
                message =
                    messageSpeedFast
                    + "Vous avez trouvé le premier\n"
                    + messageColorRed
                    + "glyphe célestien"
                    + messageColorWhite
                    + "! Vous avez\ndéchiffré un des mots manquants\ndes anciens écrits célestiens!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x140, // Second Sky Character
                message =
                    messageSpeedFast
                    + "Vous avez trouvé le second\n"
                    + messageColorRed
                    + "glyphe célestien"
                    + messageColorWhite
                    + "! Vous avez\ndéchiffré un des mots manquants\ndes anciens écrits célestiens!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x141, // Third Sky Character
                message =
                    messageSpeedFast
                    + "Vous avez trouvé le troisième\n"
                    + messageColorRed
                    + "glyphe célestien"
                    + messageColorWhite
                    + "! Vous avez\ndéchiffré un des mots manquants\ndes anciens écrits célestiens!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x142, // Fourth Sky Character
                message =
                    messageSpeedFast
                    + "Vous avez trouvé le quatrième\n"
                    + messageColorRed
                    + "glyphe célestien"
                    + messageColorWhite
                    + "! Vous avez\ndéchiffré un des mots manquants\ndes anciens écrits célestiens!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x143,
                message =
                    messageSpeedFast
                    + "Vous avez trouvé le cinquième\n"
                    + messageColorRed
                    + "glyphe célestien"
                    + messageColorWhite
                    + "! Vous avez\ndéchiffré un des mots manquants\ndes anciens écrits célestiens!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13C, // First Fused Shadow
                message =
                    messageSpeedFast
                    + "Vous obtenez un "
                    + messageColorRed
                    + "Cristal d'Ombre"
                    + messageColorWhite
                    + "!\nSa surface semble être\nrecouverte par "
                    + messageColorGreen
                    + "de la mousse"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13D, // Second Fused Shadow
                message =
                    messageSpeedFast
                    + "Vous obtenez un second\n"
                    + messageColorRed
                    + "Cristal d'Ombre"
                    + messageColorWhite
                    + "! Il émane "
                    + messageColorRed
                    + "une\ndouce chaleur "
                    + messageColorWhite
                    + "sur vos mains.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13E, // Third Fused Shadow
                message =
                    messageSpeedFast
                    + "Vous obtenez le dernier\n"
                    + messageColorRed
                    + "Cristal d'Ombre"
                    + messageColorWhite
                    + "! Une forte\n"
                    + messageColorBlue
                    + "odeur de poisson "
                    + messageColorWhite
                    + "s'en dégage.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x109, // Second Mirror Piece
                message =
                    messageSpeedFast
                    + "Vous obtenez le second fragment\ndu "
                    + messageColorRed
                    + "Miroir des Ombres"
                    + messageColorWhite
                    + "! Il est\n"
                    + messageColorLightBlue
                    + "un peu froid "
                    + messageColorWhite
                    + "et brille de\nmille feux.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10A, // Third Mirror Shard
                message =
                    messageSpeedFast
                    + "Vous obtenez le troisième\nfragment du "
                    + messageColorRed
                    + "Miroir des Ombres"
                    + messageColorWhite
                    + "!\nIl est très sale et "
                    + messageColorDarkGreen
                    + "recouvert\nde toiles"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10B, // Final Mirror Shard
                message =
                    messageSpeedFast
                    + "Vous obtenez le dernier\nfragment du "
                    + messageColorRed
                    + "Miroir des Ombres"
                    + messageColorWhite
                    + "!\nIl est encore plus\n"
                    + messageColorYellow
                    + "léger que l'air"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF3, // Foolish Item
                message =
                    messageSpeedFast
                    + "Le "
                    + messageColorLightBlue
                    + "blizzard "
                    + messageColorWhite
                    + "souffle.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xBBB, // Talk to Midna
                message =
                    "Qu'est-ce qu'il y a, "
                    + playerName
                    + "?"
                    + messageOption1
                    + "Je veux me transformer\n"
                    + messageOption2
                    + "Je veux me téléporter\n"
                    + messageOption3
                    + "Changer l'heure de la journée"
            },
        };

        public static MessageEntry[] spanishMessages =
        {
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xE9, // Forest Temple Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorGreen
                    + "Templo del Bosque"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEA, // Goron Mines Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en las\n"
                    + messageColorRed
                    + "Minas de los Goron"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEB, // Lakebed Temple Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorBlue
                    + "Santuario del Lago"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEC, //Arbiter's Grounds Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorOrange
                    + "Patíbulo del Desierto"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xED, // Snowpeak Ruins Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en las\n"
                    + messageColorLightBlue
                    + "Ruinas del Pico Nevado"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEE, // Temple of Time Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorDarkGreen
                    + "Templo del Tiempo"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEF, // City in The Sky Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en\n"
                    + messageColorYellow
                    + "Celestia"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF0, // Palace of Twilight Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorPurple
                    + "Palacio del Crepúsculo"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF1, // Hyrule Castle Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorSilver
                    + "Castillo de Hyrule"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF2, // Bulblin Camp Small Key
                message =
                    messageSpeedFast
                    + "¡Has obtenido una "
                    + messageColorRed
                    + "llave pequeña"
                    + messageColorWhite
                    + "!\nPuede ser utilizada en el\n"
                    + messageColorOrange
                    + "Campo Bulbin"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x96, // Shadow Crystal
                message =
                    messageSpeedSlow
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "Cristal Oscuro"
                    + messageColorWhite
                    + "!\nEsa manifestación maléfica del\npoder de "
                    + messageColorRed
                    + "Zant "
                    + messageColorWhite
                    + "te permite\ntransformarte cuando quieras!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11A, // Forest Temple Dungeon Map
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorGreen
                    + "Templo del Bosque"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11B, // Goron Mines Dungeon Map
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nlas "
                    + messageColorRed
                    + "Minas de los Goron"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11C, // Lakebed Temple Dungeon Map
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorBlue
                    + "Santuario del Lago"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11D, // Arbiter's Grounds Dungeon Map
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorOrange
                    + "Patíbulo del Desierto"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11E, // Snowpeak Ruins Dungeon Map
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nlas "
                    + messageColorLightBlue
                    + "Ruinas del Pico Nevado"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11F, // Temple of Time Dungeon Map
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorDarkGreen
                    + "Templo del Tiempo"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x120,
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x121,
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x122,
                message =
                    messageSpeedFast
                    + "¡Has obtenido el "
                    + messageColorRed
                    + "mapa de una\nmazmorra"
                    + messageColorWhite
                    + "! Indica el camino en\nel "
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFD,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x110,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x111,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF6,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF7,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF8,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF9,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x145,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Ending Blow"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x146,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Shield Attack"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x147,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Back Slice"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x148,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Helm Splitter"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x149,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Mortal Draw"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Jump Strike"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14B,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Great Spin"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xB0,
                message =
                    messageSpeedFast
                    + "Power has been restored to\nthe "
                    + messageColorRed
                    + "Dominion Rod"
                    + messageColorWhite
                    + "! Now it can\nbe used to imbude statues\nwith life in the present!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13F,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "first Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x140,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "second Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x141,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "third Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x142,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "fourth Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x143,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "fifth Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "Fused Shadow!\n"
                    + messageColorWhite
                    + "It seems to have some "
                    + messageColorGreen
                    + "moss"
                    + messageColorWhite
                    + "\ngrowing on it.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13D,
                message =
                    messageSpeedFast
                    + "You got the second "
                    + messageColorRed
                    + "Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorRed
                    + "warm"
                    + messageColorWhite
                    + " to\nthe touch.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13E,
                message =
                    messageSpeedFast
                    + "You got the final "
                    + messageColorRed
                    + "Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorBlue
                    + "wet"
                    + messageColorWhite
                    + " and\nsmells like fish.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x109,
                message =
                    messageSpeedFast
                    + "You got the second shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nhas a beautiful shine to it\nand feels slightly "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10A,
                message =
                    messageSpeedFast
                    + "You got the third shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nis covered in dirt and\n"
                    + messageColorDarkGreen
                    + "webs"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10B,
                message =
                    messageSpeedFast
                    + "You got the final shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nfeels lighter than "
                    + messageColorYellow
                    + "air"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF3,
                message =
                    messageSpeedFast
                    + "A "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + " wind blows.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x99,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Big Wallet"
                    + messageColorWhite
                    + "! You can now hold "
                    + messageColorRed
                    + "5,000 Rupees"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x9A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Giant Wallet"
                    + messageColorWhite
                    + "! You can now hold "
                    + messageColorPurple
                    + "9,999 Rupees"
                    + messageColorWhite
                    + "!"
            },
        };

        public static MessageEntry[] italianMessages =
        {
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xE9,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the \n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xED,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF0,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF1,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF2,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorOrange
                    + "Bulblin Camp"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x96,
                message =
                    messageSpeedSlow
                    + "You got the "
                    + messageColorRed
                    + "Shadow Crystal"
                    + messageColorWhite
                    + "!\nThis is a dark manifestation\nof "
                    + messageColorRed
                    + "Zant's "
                    + messageColorWhite
                    + "power that allows\nyou to transform at will!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11A,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11B,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x120,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x121,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x122,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFD,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x110,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x111,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF6,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF7,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF8,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF9,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x145,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Ending Blow"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x146,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Shield Attack"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x147,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Back Slice"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x148,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Helm Splitter"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x149,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Mortal Draw"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Jump Strike"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14B,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Great Spin"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xB0,
                message =
                    messageSpeedFast
                    + "Power has been restored to\nthe "
                    + messageColorRed
                    + "Dominion Rod"
                    + messageColorWhite
                    + "! Now it can\nbe used to imbude statues\nwith life in the present!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13F,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "first Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x140,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "second Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x141,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "third Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x142,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "fourth Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x143,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "fifth Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "Fused Shadow!\n"
                    + messageColorWhite
                    + "It seems to have some "
                    + messageColorGreen
                    + "moss"
                    + messageColorWhite
                    + "\ngrowing on it.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13D,
                message =
                    messageSpeedFast
                    + "You got the second "
                    + messageColorRed
                    + "Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorRed
                    + "warm"
                    + messageColorWhite
                    + " to\nthe touch.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13E,
                message =
                    messageSpeedFast
                    + "You got the final "
                    + messageColorRed
                    + "Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorBlue
                    + "wet"
                    + messageColorWhite
                    + " and\nsmells like fish.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x109,
                message =
                    messageSpeedFast
                    + "You got the second shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nhas a beautiful shine to it\nand feels slightly "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10A,
                message =
                    messageSpeedFast
                    + "You got the third shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nis covered in dirt and\n"
                    + messageColorDarkGreen
                    + "webs"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10B,
                message =
                    messageSpeedFast
                    + "You got the final shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nfeels lighter than "
                    + messageColorYellow
                    + "air"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF3,
                message =
                    messageSpeedFast
                    + "A "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + " wind blows.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x99,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Big Wallet"
                    + messageColorWhite
                    + "! You can now hold "
                    + messageColorRed
                    + "5,000 Rupees"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x9A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Giant Wallet"
                    + messageColorWhite
                    + "! You can now hold "
                    + messageColorPurple
                    + "9,999 Rupees"
                    + messageColorWhite
                    + "!"
            },
        };

        public static MessageEntry[] japaneseMessages =
        {
            new MessageEntry // Forest Temple Small Key
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xE9,
                message =
                    messageSpeedFast
                    + messageColorRed
                    + "\x8F\xAC\x82\xB3\x82\xC8\x83\x4A\x83\x4D "
                    + messageColorWhite
                    + "\x82\xF0\x8E\xE8\x82\xC9\x93\xFC\x82\xEA\x82\xBD\x81\x49\n"
                    + messageColorGreen
                    + "\x90\x58\x82\xCC\x90\x5F\x93\x61 "
                    + messageColorWhite
                    + "\x82\xC5\x8E\x67\x97\x70\x82\xC5\x82\xAB\x82\xE9"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xED,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xEF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF0,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF1,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF2,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "small key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorOrange
                    + "Bulblin Camp"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x96,
                message =
                    messageSpeedSlow
                    + "You got the "
                    + messageColorRed
                    + "Shadow Crystal"
                    + messageColorWhite
                    + "!\nThis is a dark manifestation\nof "
                    + messageColorRed
                    + "Zant's "
                    + messageColorWhite
                    + "power that allows\nyou to transform at will!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11A,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11B,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x11F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x120,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x121,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x122,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "dungeon map"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFD,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFE,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorRed
                    + "Goron Mines"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFF,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10D,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorLightBlue
                    + "Snowpeak Ruins"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10E,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10F,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x110,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x111,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "compass"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF6,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorGreen
                    + "Forest Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF7,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorBlue
                    + "Lakebed Temple"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF8,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorOrange
                    + "Arbiter's Grounds"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF9,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorDarkGreen
                    + "Temple of Time"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFA,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorYellow
                    + "City in The Sky"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFB,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in the\n"
                    + messageColorPurple
                    + "Palace of Twilight"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xFC,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "big key"
                    + messageColorWhite
                    + "!\nIt can be used in\n"
                    + messageColorSilver
                    + "Hyrule Castle"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x145,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Ending Blow"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x146,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Shield Attack"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x147,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Back Slice"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x148,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Helm Splitter"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x149,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Mortal Draw"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Jump Strike"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x14B,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Great Spin"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xB0,
                message =
                    messageSpeedFast
                    + "Power has been restored to\nthe "
                    + messageColorRed
                    + "Dominion Rod"
                    + messageColorWhite
                    + "! Now it can\nbe used to imbude statues\nwith life in the present!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13F,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "first Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x140,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "second Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x141,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "third Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x142,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "fourth Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x143,
                message =
                    messageSpeedFast
                    + "You found the "
                    + messageColorRed
                    + "fifth Sky\ncharacter"
                    + messageColorWhite
                    + "! A missing part\nof the word in the Ancient\nSky Book has been restored."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13C,
                message =
                    messageSpeedFast
                    + "You got a "
                    + messageColorRed
                    + "Fused Shadow!\n"
                    + messageColorWhite
                    + "It seems to have some "
                    + messageColorGreen
                    + "moss"
                    + messageColorWhite
                    + "\ngrowing on it.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13D,
                message =
                    messageSpeedFast
                    + "You got the second "
                    + messageColorRed
                    + "Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorRed
                    + "warm"
                    + messageColorWhite
                    + " to\nthe touch.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x13E,
                message =
                    messageSpeedFast
                    + "You got the final "
                    + messageColorRed
                    + "Fused\nShadow"
                    + messageColorWhite
                    + "! It feels "
                    + messageColorBlue
                    + "wet"
                    + messageColorWhite
                    + " and\nsmells like fish.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x109,
                message =
                    messageSpeedFast
                    + "You got the second shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nhas a beautiful shine to it\nand feels slightly "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10A,
                message =
                    messageSpeedFast
                    + "You got the third shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nis covered in dirt and\n"
                    + messageColorDarkGreen
                    + "webs"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x10B,
                message =
                    messageSpeedFast
                    + "You got the final shard of\nthe "
                    + messageColorRed
                    + "Mirror of Twilight"
                    + messageColorWhite
                    + "! It\nfeels lighter than "
                    + messageColorYellow
                    + "air"
                    + messageColorWhite
                    + ".."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0xF3,
                message =
                    messageSpeedFast
                    + "A "
                    + messageColorLightBlue
                    + "cold"
                    + messageColorWhite
                    + " wind blows.."
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x99,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Big Wallet"
                    + messageColorWhite
                    + "! You can now hold "
                    + messageColorRed
                    + "5,000 Rupees"
                    + messageColorWhite
                    + "!"
            },
            new MessageEntry
            {
                stageIDX = 0xFF,
                roomIDX = 0xFF,
                messageID = 0x9A,
                message =
                    messageSpeedFast
                    + "You got the "
                    + messageColorRed
                    + "Giant Wallet"
                    + messageColorWhite
                    + "! You can now hold "
                    + messageColorPurple
                    + "9,999 Rupees"
                    + messageColorWhite
                    + "!"
            },
        };

        /* length test:
        * string test = "Vous obtenez une petite clé! Elle peut être utilisée au Château d'Hyrule.";
        * test = Regex.Replace(test, @"(?<=\G.{30})", "\r\n");
        */

        enum StageIDs : byte
        {
            Lakebed_Temple = 0x0,
            Morpheel = 0x1,
            Deku_Toad,
            Goron_Mines,
            Fyrus,
            Dangoro,
            Forest_Temple,
            Diababa,
            Ook,
            Temple_of_Time,
            Armogohma,
            Darknut,
            City_in_the_Sky,
            Argorok,
            Aeralfos,
            Palace_of_Twilight,
            Zant_Main_Room,
            Phantom_Zant_1,
            Phantom_Zant_2,
            Zant_Fight,
            Hyrule_Castle,
            Ganondorf_Castle,
            Ganondorf_Field,
            Ganondorf_Defeated,
            Arbiters_Grounds,
            Stallord,
            Death_Sword,
            Snowpeak_Ruins,
            Blizzeta,
            Darkhammer,
            Lanayru_Ice_Puzzle_Cave,
            Cave_of_Ordeals,
            Eldin_Long_Cave,
            Lake_Hylia_Long_Cave,
            Eldin_Goron_Stockcave,
            Grotto_1,
            Grotto_2,
            Grotto_3,
            Grotto_4,
            Grotto_5,
            Faron_Woods_Cave,
            Ordon_Ranch,
            Title_Screen,
            Ordon_Village,
            Ordon_Spring,
            Faron_Woods,
            Kakariko_Village,
            Death_Mountain,
            Kakariko_Graveyard,
            Zoras_River,
            Zoras_Domain,
            Snowpeak,
            Lake_Hylia,
            Castle_Town,
            Sacred_Grove,
            Bulblin_Camp,
            Hyrule_Field,
            Outside_Castle_Town,
            Bulblin_2,
            Gerudo_Desert,
            Mirror_Chamber,
            Upper_Zoras_River,
            Fishing_Pond,
            Hidden_Village,
            Hidden_Skill,
            Ordon_Village_Interiors,
            Hyrule_Castle_Sewers,
            Faron_Woods_Interiors,
            Kakariko_Village_Interiors,
            Death_Mountain_Interiors,
            Castle_Town_Interiors,
            Fishing_Pond_Interiors,
            Hidden_Village_Interiors,
            Castle_Town_Shops,
            Star_Game,
            Kakariko_Graveyard_Interiors,
            Light_Arrows_Cutscene,
            Hyrule_Castle_Cutscenes
        };

        /* length test:
        * string test = "Vous obtenez une petite clé! Elle peut être utilisée au Château d'Hyrule.";
        * test = Regex.Replace(test, @"(?<=\G.{30})", "\r\n");
        */
    }
}
