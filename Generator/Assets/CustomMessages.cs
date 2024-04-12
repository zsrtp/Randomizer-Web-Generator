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
        public static string messageSpeedFast = "\x1A\x05\x00\x00\x01";
        public static string messageSpeedSlow = "\x1A\x05\x00\x00\x02";
        public static string messageColorWhite = "\x1A\x06\xFF\x00\x00\x00";
        public static string messageColorRed = "\x1A\x06\xFF\x00\x00\x01";
        public static string messageColorGreen = "\x1A\x06\xFF\x00\x00\x02";
        public static string messageColorLightBlue = "\x1A\x06\xFF\x00\x00\x03";
        public static string messageColorYellow = "\x1A\x06\xFF\x00\x00\x04";
        public static string messageColorPurple = "\x1A\x06\xFF\x00\x00\x06";
        public static string messageColorOrange = "\x1A\x06\xFF\x00\x00\x08";
        public static string messageColorDarkGreen = "\x1A\x06\xFF\x00\x00\x09";
        public static string messageColorBlue = "\x1A\x06\xFF\x00\x00\x0A";
        public static string messageColorSilver = "\x1A\x06\xFF\x00\x00\x0B";
        public static string playerName = "\x1A\x05\x00\x00\x00";
        public static string messageOption1 = "\x1A\x06\x00\x00\x09\x01";
        public static string messageOption2 = "\x1A\x06\x00\x00\x09\x02";
        public static string messageOption3 = "\x1A\x06\x00\x00\x09\x03";
        public static string shopOption = "\x1A\x05\x00\x00\x20";
        public static string maleSign = "\x1A\x05\x06\x00\x02";
        public static string femaleSign = "\x1A\x05\x06\x00\x03";

        // Unused for now but who knows.
        public static string[][] foolishShopItemNames =
        [
            [ Item.Hylian_Shield.ToString(), "Highlian Shield", "Hilan Shield", "Hylian Sheeld" ],
            [ Item.Slingshot.ToString(), "Slongshut" ]
        ];

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

            public MessageEntry(StageIDs stageIdx, byte roomIdx, short messageId)
            {
                messageID = messageId;
                stageIDX = (byte)stageIdx;
                roomIDX = roomIdx;
            }
        }


        public List<MessageEntry> englishMessages = new List<MessageEntry>
        {
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 1,
            //     messageID = 0x9B, // Barnes Bomb Bag Text.
            //     message =
            //         "I've got a special offer goin' right\nnow: my "
            //         + getShortenedItemName(Randomizer.Checks.CheckDict["Barnes Bomb Bag"].itemId)
            //         + messageColorWhite
            //         + ", just\n"
            //         + messageColorPurple
            //         + "120 Rupees"
            //         + messageColorWhite
            //         + "! How 'bout that?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town,
            //     roomIDX = 2,
            //     messageID = 0x355, // Charlo Donation Text.
            //     message =
            //         "For a "
            //         + getShortenedItemName(Randomizer.Checks.CheckDict["Charlo Donation Blessing"].itemId)
            //         + messageColorWhite
            //         + "...\nWould you please make a donation?"
            //         + messageOption1
            //         + "100 Rupees\n"
            //         + messageOption2
            //         + "50 Rupees\n"
            //         + messageOption3
            //         + "Sorry..."
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Fishing_Pond,
            //     roomIDX = 0,
            //     messageID = 0x47A, // Fishing Hole Bottle Sign
            //     message =
            //         "           "
            //         + messageColorRed
            //         + "DON'T LITTER!\n"
            //         + messageColorWhite
            //         + "Do NOT toss a "
            //         + getShortenedItemName(Randomizer.Checks.CheckDict["Fishing Hole Bottle"].itemId)
            //         + messageColorWhite
            //         + " or\ncans here! The fish are CRYING!\n\nKeep the fishing hole clean!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town,
            //     roomIDX = 3,
            //     messageID = 0x457, // Jovani House Sign
            //     message =
            //         "20 Soul Reward: "
            //         + getShortenedItemName(Randomizer.Checks.CheckDict["Jovani 20 Poe Soul Reward"].itemId)
            //         + messageColorWhite
            //         + "\n60 Soul Reward: "
            //         + getShortenedItemName(Randomizer.Checks.CheckDict["Jovani 60 Poe Soul Reward"].itemId)
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Ordon_Village_Interiors,
            //     roomIDX = 1,
            //     messageID = 0x5B4, // Slingshot Confirmation
            //     message =
            //         "Are you sure?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Ordon_Village_Interiors,
            //     roomIDX = 1,
            //     messageID = 0x5AE, // Slingshot Confirmation - Not enough money
            //     message =
            //         "You don't have enough money!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town_Shops,
            //     roomIDX = 0,
            //     messageID = 0x119, // Magic Armor Confirmation
            //     message =
            //         "Are you sure?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town_Shops,
            //     roomIDX = 0,
            //     messageID = 0x11E, // Magic Armor Confirmation after purchase
            //     message =
            //         "We have sold out!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town_Shops,
            //     roomIDX = 0,
            //     messageID = 0x130, // Magic Armor sold out
            //     message =
            //         "-SOLD OUT-\nThis item has been discontinued."
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town_Shops,
            //     roomIDX = 0,
            //     messageID = 0x11C, // Magic Armor Confirmation - not enough money
            //     message =
            //         "You don't have enough money!"
            // },

            // new MessageEntry
            // {
            //     stageIDX = 54,
            //     roomIDX = 1,
            //     messageID = 0x1369, // Hint Message
            //     message =
            //         "This is a test hint\nfor Sacred Grove\nonly\na b c d\ne f g h i j k l"
            // },
            // new MessageEntry
            // {
            //     stageIDX = 0xFF,
            //     roomIDX = 0xFF,
            //     messageID = 0x1369, // Hint Message
            //     message =
            //         $"They say that there is nothing to\nbe found in {messageColorPurple}Kakariko Village{messageColorWhite}.\n\n\n1 " + messageColorOrange + "2 3 4" + messageColorWhite + " 5 6 7 8 9\nThis is a test hint\n\n\n1 1 2 3 4 5 6 7 8 9\nThis is a\ntest hint\n\n2 1 2 3 4 5 6 7 8 9\nThis is a\ntest hint\n0 1 2 3 4 5 6 7 8 9\nThis is a test hint\n0 1 2 3 4 5 6 7 8\n9 This is a\ntest hint\n3 1 2 3 4 5 6\n7 8 9"
            // },

            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2CC, // Hylian Shield Confirmation - Not enough money
            //     message =
            //         "You don't have enough money!"
            // },
            //  new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2CB, // Hylian Shield Confirmation
            //     message =
            //         "Are you sure?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D2, // Hawkeye Confirmation
            //     message =
            //         "Are you sure?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x306, // coming soon sign
            //     message =
            //     messageColorOrange+
            //         "COMING SOON"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D4, // coming soon sign read
            //     message =
            //         "Can't you read?\nWe don't have that in stock yet."
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D3, // Hawkeye Confirmation - Not enough money
            //     message =
            //         "You don't have enough money!"
            // },
            //  new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2C8, // Wooden Shield confirmation - not enough money
            //     message =
            //         "You don't have enough money!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2C7, // Wooden Shield confirmation
            //     message =
            //         "Are you sure?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x30B, // Sold out sign
            //     message =
            //     messageColorOrange+
            //         "-SOLD OUT-\nThis item has been discontinued."
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2E0, // red potion sold out sign read
            //     message =
            //         "Can't you read?\nThat item is out of stock."
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D0, // wooden shield sold out sign read
            //     message =
            //         "Can't you read?\nThat item is out of stock."
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D7, // Red Potion confirmation
            //     message =
            //         "Are you sure?"
            //         + shopOption
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D6, // Red Potion confirmation - not enough money
            //     message =
            //         "You don't have enough money!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x2D8, // Red Potion after buying
            //     message =
            //         "Thank you for your purchase..."
            // },
        };

        public List<MessageEntry> englishShopMessages = new List<MessageEntry>
        {
            // This is the far right one which should be 0x5AE!
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
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Castle_Town_Shops,
            //     roomIDX = 0,
            //     messageID = 0x125, // Magic Armor Check
            //     message =
            //         getShortenedItemName(Randomizer.Checks.CheckDict["Castle Town Malo Mart Magic Armor"].itemId)
            //         + ": "
            //         + messageColorPurple
            //         + "598 Rupees\n"
            //         + messageColorWhite
            //         + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
            // },
            //  new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x30E, // Hylian Shield Check
            //     message =
            //         getShortenedItemName(Randomizer.Checks.CheckDict["Kakariko Village Malo Mart Hylian Shield"].itemId)
            //         + ": "
            //         + messageColorPurple
            //         + "200 Rupees\n"
            //         + messageColorWhite
            //         + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x307, // Hawkeye Display
            //     message =
            //         getShortenedItemName(Randomizer.Checks.CheckDict["Kakariko Village Malo Mart Hawkeye"].itemId)
            //         + ": "
            //         + messageColorPurple
            //         + "100 Rupees\n"
            //         + messageColorWhite
            //         + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x30D, // Wooden Shield text
            //     message =
            //     getShortenedItemName(Randomizer.Checks.CheckDict["Kakariko Village Malo Mart Wooden Shield"].itemId)
            //         + ": "
            //         + messageColorPurple
            //         + "50 Rupees\n"
            //         + messageColorWhite
            //         + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
            // },
            // new MessageEntry
            // {
            //     stageIDX = (byte)StageIDs.Kakariko_Village_Interiors,
            //     roomIDX = 3,
            //     messageID = 0x305, // Red Potion Text
            //     message =
            //     getShortenedItemName(Randomizer.Checks.CheckDict["Kakariko Village Malo Mart Red Potion"].itemId)
            //         + ": "
            //         + messageColorPurple
            //         + "30 Rupees\n"
            //         + messageColorWhite
            //         + "     LIMITED SUPPLY!\nDon't let them sell out before you\nbuy one!"
            // },
        };

        public static string getShortenedItemName(Item item)
        {
           string shortName = "";

           switch (item)
           {
                case Item.Progressive_Sky_Book:
                case Item.Progressive_Fused_Shadow:
                case Item.Progressive_Mirror_Shard:
                case Item.Progressive_Sword:
                case Item.Progressive_Bow:
                case Item.Progressive_Clawshot:
                case Item.Progressive_Fishing_Rod:
                case Item.Progressive_Dominion_Rod:
                case Item.Progressive_Hidden_Skill:
                case Item.Progressive_Wallet:
                {
                    shortName = item.ToString().Replace('_', ' ');
                    shortName = shortName.Replace("Progressive ", "");
                    shortName = messageColorRed + shortName;
                    break;
                }

                case Item.Forest_Temple_Big_Key:
                case Item.Forest_Temple_Compass:
                case Item.Forest_Temple_Dungeon_Map:
                case Item.Forest_Temple_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Forest Temple", "FT");
                    shortName = messageColorGreen + shortName;
                    break;
                }

                case Item.Goron_Mines_Key_Shard:
                case Item.Goron_Mines_Compass:
                case Item.Goron_Mines_Dungeon_Map:
                case Item.Goron_Mines_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Goron Mines", "GM");
                    shortName = messageColorRed + shortName;
                    break;
                }

                case Item.Lakebed_Temple_Big_Key:
                case Item.Lakebed_Temple_Compass:
                case Item.Lakebed_Temple_Dungeon_Map:
                case Item.Lakebed_Temple_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Lakebed Temple", "LBT");
                    shortName = messageColorBlue + shortName;
                    break;
                }

                case Item.Arbiters_Grounds_Big_Key:
                case Item.Arbiters_Grounds_Compass:
                case Item.Arbiters_Grounds_Dungeon_Map:
                case Item.Arbiters_Grounds_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Arbiters Grounds", "AG");
                    shortName = messageColorOrange + shortName;
                    break;
                }

                case Item.Snowpeak_Ruins_Bedroom_Key:
                case Item.Snowpeak_Ruins_Compass:
                case Item.Snowpeak_Ruins_Dungeon_Map:
                case Item.Snowpeak_Ruins_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Snowpeak Ruins", "SPR");
                    shortName = messageColorLightBlue + shortName;
                    break;
                }

                case Item.Snowpeak_Ruins_Ordon_Goat_Cheese:
                case Item.Snowpeak_Ruins_Ordon_Pumpkin:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Snowpeak Ruins", "SPR").Replace("Ordon ", "");
                    shortName = messageColorLightBlue + shortName;
                    break;
                }

                case Item.Temple_of_Time_Big_Key:
                case Item.Temple_of_Time_Compass:
                case Item.Temple_of_Time_Dungeon_Map:
                case Item.Temple_of_Time_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Temple of Time", "ToT");
                    shortName = messageColorDarkGreen + shortName;
                    break;
                }

                case Item.City_in_The_Sky_Big_Key:
                case Item.City_in_The_Sky_Compass:
                case Item.City_in_The_Sky_Dungeon_Map:
                case Item.City_in_The_Sky_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("City in The Sky", "CitS");
                    shortName = messageColorYellow + shortName;
                    break;
                }

                case Item.Palace_of_Twilight_Big_Key:
                case Item.Palace_of_Twilight_Compass:
                case Item.Palace_of_Twilight_Dungeon_Map:
                case Item.Palace_of_Twilight_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Palace of Twilight", "PoT");
                    shortName = messageColorPurple + shortName;
                    break;
                }

                case Item.Hyrule_Castle_Big_Key:
                case Item.Hyrule_Castle_Compass:
                case Item.Hyrule_Castle_Dungeon_Map:
                case Item.Hyrule_Castle_Small_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Hyrule Castle", "HC");
                    shortName = messageColorSilver + shortName;
                    break;
                }

                case Item.Gerudo_Desert_Bulblin_Camp_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("Gerudo Desert ", "");
                    shortName = messageColorOrange + shortName;
                    break;
                }

                case Item.North_Faron_Woods_Gate_Key:
                {
                   shortName = item.ToString().Replace('_', ' ').Replace("North Faron Woods", "Faron");
                    shortName = messageColorOrange + shortName;
                    break;
                }

                case Item.Purple_Rupee_Links_House:
                {
                    shortName = "Link's Rupee";
                    shortName = messageColorPurple + shortName;
                    break;
                }

                default:
                {
                    shortName = item.ToString().Replace('_', ' ');
                    shortName = messageColorRed + shortName;
                    break;
                }
           }
           return shortName;
        }

        /* length test:
        * string test = "Vous obtenez une petite clé! Elle peut être utilisée au Château d'Hyrule.";
        * test = Regex.Replace(test, @"(?<=\G.{30})", "\r\n");
        */

        public enum MessageLanguage
        {
            English = 0,
            French = 1,
            Spanish,
            Italian,
            German,
            Japanese
        };
    }
}
