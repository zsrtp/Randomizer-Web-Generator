namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        static string messageIconR = "\x1A\x05\x00\x00\x0E";
        static string messageIconA = "\x1A\x05\x00\x00\x0A";
        static string messageIconX = "\x1A\x05\x00\x00\x0F";
        static string messageIconY = "\x1A\x05\x00\x00\x10";

        public struct MessageEntry
        {
            public short messageID;
            public string message;
        }

        public Dictionary<byte, MessageEntry[]> CustomPALMessageDictionary =
            new() { { 0, englishMessages }, { 1, germanMessages }, { 2, frenchMessages } };

        public Dictionary<byte, MessageEntry[]> CustomUSMessageDictionary =
            new() { { 0, englishMessages }, };
        public Dictionary<byte, MessageEntry[]> CustomJPMessageDictionary =
            new() { { 0, japaneseMessages } };

        public static MessageEntry[] englishMessages =
        {
            new MessageEntry
            {
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

        public static MessageEntry[] germanMessages =
        {
            new MessageEntry
            {
                messageID = 0xE9,
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
            }
        };

        public static MessageEntry[] frenchMessages =
        {
            new MessageEntry
            {
                messageID = 0xE9,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "petite clé"
                    + messageColorWhite
                    + "!\nElle peut être utilisée au\n"
                    + messageColorSilver
                    + "Camp Bulblin"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
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
                messageID = 0,
                message =
                    messageSpeedFast
                    + "Vous obtenez une "
                    + messageColorRed
                    + "carte de\ndonjon"
                    + messageColorWhite
                    + "!  Elle appartient à\n"
                    + messageColorYellow
                    + "Célestia"
                    + messageColorWhite
                    + "."
            },
            new MessageEntry
            {
                messageID = 0,
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
                messageID = 0,
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
        };

        public static MessageEntry[] japaneseMessages =
        {
            new MessageEntry
            {
                messageID = 0xEA,
                message =
                    messageSpeedFast
                    + messageColorRed
                    + "\x8F\xAC\x82\xB3\x82\xC8\x83\x4A\x83\x4D "
                    + messageColorWhite
                    + "\x82\xF0\x8E\xE8\x82\xC9\x93\xFC\x82\xEA\x82\xBD\x81\x49 "
                    + messageColorGreen
                    + "\x90\x58\x82\xCC\x90\x5F\x93\x61 "
                    + messageColorWhite
                    + "\x82\xC5\x8E\x67\x97\x70\x82\xC5\x82\xAB\x82\xE9 "
            }
        };
    }
}
