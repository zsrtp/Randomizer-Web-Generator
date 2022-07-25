namespace TPRandomizer
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using TPRandomizer.SSettings.Enums;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Item : byte
    {
        Recovery_Heart = 0x00,
        Green_Rupee = 0x01,
        Blue_Rupee = 0x02,
        Yellow_Rupee = 0x03,
        Red_Rupee = 0x04,
        Purple_Rupee = 0x05,
        Orange_Rupee = 0x06,
        Silver_Rupee = 0x07,

        /*Borrow_Bomb_Bag?	=	0x08,*/
        /*Progressive_Bomb_Bag?	=	0x09,*/
        Bombs_5 = 0x0A,
        Bombs_10 = 0x0B,
        Bombs_20 = 0x0C,
        Bombs_30 = 0x0D,
        Arrows_10 = 0x0E,
        Arrows_20 = 0x0F,
        Arrows_30 = 0x10,
        Arrows_1 = 0x11,
        Seeds_50 = 0x12,

        /*?	=	0x13,*/
        /*?	=	0x14,*/
        /*?	=	0x15,*/
        Water_Bombs_5 = 0x16,
        Water_Bombs_10 = 0x17,
        Water_Bombs_15 = 0x18,
        Water_Bombs_3 = 0x19,
        Bomblings_5 = 0x1A,
        Bomblings_10 = 0x1B,
        Bomblings_3 = 0x1C,
        Bombling_1 = 0x1D,
        Fairy = 0x1E,
        Recovery_Heart_x3 = 0x1F,
        Small_Key = 0x20,
        Piece_of_Heart = 0x21,
        Heart_Container = 0x22,
        Dungeon_Map = 0x23,
        Compass = 0x24,
        Ooccoo_FT = 0x25,
        Big_Key = 0x26,
        Ooccoo_Jr = 0x27,
        Ordon_Sword = 0x28,
        Master_Sword = 0x29,
        Ordon_Shield = 0x2A,
        Wooden_Shield = 0x2B,
        Hylian_Shield = 0x2C,
        Ooccoos_Note = 0x2D,
        Ordon_Clothing = 0x2E,
        Heros_Clothes = 0x2F,
        Magic_Armor = 0x30,
        Zora_Armor = 0x31,
        Shadow_Crystal = 0x32,
        Ooccoo_Dungeon = 0x33,

        /*unused*/
        Small_Wallet = 0x34,
        Progressive_Wallet = 0x35,
        Giant_Wallet = 0x36,

        /*Piece_of_Heart_2?	=	0x37,*/
        /*Piece_of_Heart_3?	=	0x38,*/
        /*Piece_of_Heart_4?	=	0x39,*/
        /*Piece_of_Heart_5?	=	0x3A,*/
        /*sword?	=	0x3B,*/
        /*?	=	0x3C,*/
        Coral_Earring = 0x3D,
        Hawkeye = 0x3E,
        Progressive_Sword = 0x3F,
        Boomerang = 0x40,
        Spinner = 0x41,
        Ball_and_Chain = 0x42,
        Progressive_Bow = 0x43,
        Progressive_Clawshot = 0x44,
        Iron_Boots = 0x45,
        Progressive_Dominion_Rod = 0x46,
        Double_Clawshot = 0x47,
        Lantern = 0x48,
        Master_Sword_Light = 0x49,
        Progressive_Fishing_Rod = 0x4A,
        Slingshot = 0x4B,
        Dominion_Rod_Uncharged = 0x4C,

        /*?	=	0x4D,*/
        /*?	=	0x4E,*/
        Giant_Bomb_Bag = 0x4F,
        Barnes_Bomb_Bag = 0x50,
        Filled_Bomb_Bag = 0x51,

        /*Giant_Bomb_Bag?	=	0x52,*/
        /*?	=	0x53,*/
        /*unused*/
        Small_Quiver = 0x54,
        Big_Quiver = 0x55,
        Giant_Quiver = 0x56,

        /*?	=	0x57,*/
        Fishing_Rod_Lure = 0x58,
        Bow_Bombs = 0x59,
        Bow_Hawkeye = 0x5A,
        Fishing_Rod_Bee_Larva = 0x5B,
        Fishing_Rod_Coral_Earring = 0x5C,
        Fishing_Rod_Worm = 0x5D,
        Fishing_Rod_Earring_Bee_Larva = 0x5E,
        Fishing_Rod_Earring_Worm = 0x5F,
        Empty_Bottle = 0x60,
        Red_Potion_Shop = 0x61,
        Green_Potion = 0x62,
        Blue_Potion = 0x63,
        Milk = 0x64,
        Sera_Bottle = 0x65,
        Lantern_Oil_Shop = 0x66,
        Water = 0x67,
        Lantern_Oil_Scooped = 0x68,
        Red_Potion_Scooped = 0x69,
        Nasty_soup = 0x6A,
        Hot_spring_water_Scooped = 0x6B,
        Fairy_Bottle = 0x6C,
        Hot_Spring_Water_Shop = 0x6D,
        Lantern_Refill_Scooped = 0x6E,
        Lantern_Refill_Shop = 0x6F,
        Bomb_Bag_Regular_Bombs = 0x70,
        Bomb_Bag_Water_Bombs = 0x71,
        Bomb_Bag_Bomblings = 0x72,
        Fairy_Tears = 0x73,
        Worm = 0x74,
        Jovani_Bottle = 0x75,
        Bee_Larva_Scooped = 0x76,
        Rare_Chu_Jelly = 0x77,
        Red_Chu_Jelly = 0x78,
        Blue_Chu_Jelly = 0x79,
        Green_Chu_Jelly = 0x7A,
        Yellow_Chu_Jelly = 0x7B,
        Purple_Chu_Jelly = 0x7C,
        Simple_Soup = 0x7D,
        Good_Soup = 0x7E,
        Superb_Soup = 0x7F,
        Renados_Letter = 0x80,
        Invoice = 0x81,
        Wooden_Statue = 0x82,
        Ilias_Charm = 0x83,
        Horse_Call = 0x84,
        Forest_Temple_Small_Key = 0x85, /*custom*/
        Goron_Mines_Small_Key = 0x86, /*custom*/
        Lakebed_Temple_Small_Key = 0x87, /*custom*/
        Arbiters_Grounds_Small_Key = 0x88, /*custom*/
        Snowpeak_Ruins_Small_Key = 0x89, /*custom*/
        Temple_of_Time_Small_Key = 0x8A, /*custom*/
        City_in_The_Sky_Small_Key = 0x8B, /*custom*/
        Palace_of_Twilight_Small_Key = 0x8C, /*custom*/
        Hyrule_Castle_Small_Key = 0x8D, /*custom*/
        Gerudo_Desert_Bulblin_Camp_Key = 0x8E, /*custom*/
        Foolish_Item = 0x8F, /*custom*/
        Aurus_Memo = 0x90,
        Asheis_Sketch = 0x91,
        Forest_Temple_Big_Key = 0x92, /*custom*/
        Lakebed_Temple_Big_Key = 0x93, /*custom*/
        Arbiters_Grounds_Big_Key = 0x94, /*custom*/
        Temple_of_Time_Big_Key = 0x95, /*custom*/
        City_in_The_Sky_Big_Key = 0x96, /*custom*/
        Palace_of_Twilight_Big_Key = 0x97, /*custom*/
        Hyrule_Castle_Big_Key = 0x98, /*custom*/
        Forest_Temple_Compass = 0x99, /*custom*/
        Goron_Mines_Compass = 0x9A, /*custom*/
        Lakebed_Temple_Compass = 0x9B, /*custom*/
        Lantern_Yellow_Chu_Chu = 0x9C,
        Coro_Bottle = 0x9D,
        Bee_Larva_Shop = 0x9E,
        Black_Chu_Jelly = 0x9F,

        /*unused*/
        Tear_Of_Light = 0xA0,
        Vessel_Of_Light_Faron = 0xA1,
        Vessel_Of_Light_Eldin = 0xA2,
        Vessel_Of_Light_Lanayru = 0xA3,

        /*unused*/
        Vessel_Of_Light_Full = 0xA4,
        Progressive_Mirror_Shard = 0xA5,
        Mirror_Piece_3 = 0xA6,
        Mirror_Piece_4 = 0xA7,
        Arbiters_Grounds_Compass = 0xA8, /*custom*/
        Snowpeak_Ruins_Compass = 0xA9, /*custom*/
        Temple_of_Time_Compass = 0xAA, /*custom*/
        City_in_The_Sky_Compass = 0xAB, /*custom*/
        Palace_of_Twilight_Compass = 0xAC, /*custom*/
        Hyrule_Castle_Compass = 0xAD, /*custom*/

        /*Unused	=	0xAE,*/
        /*Unused	=	0xAF,*/
        Ilias_Scent = 0xB0,

        /*Unused_Scent?	=	0xB1,*/
        Poe_Scent = 0xB2,
        Reekfish_Scent = 0xB3,
        Youths_Scent = 0xB4,
        Medicine_Scent = 0xB5,
        Forest_Temple_Dungeon_Map = 0xB6,
        Goron_Mines_Dungeon_Map = 0xB7,
        Lakebed_Temple_Dungeon_Map = 0xB8,
        Arbiters_Grounds_Dungeon_Map = 0xB9,
        Snowpeak_Ruins_Dungeon_Map = 0xBA,
        Temple_of_Time_Dungeon_Map = 0xBB,
        City_in_The_Sky_Dungeon_Map = 0xBC,
        Palace_of_Twilight_Dungeon_Map = 0xBD,
        Hyrule_Castle_Dungeon_Map = 0xBE,

        /*Bottle_Insides?	=	0xBF,*/
        Male_Beetle = 0xC0,
        Female_Beetle = 0xC1,
        Male_Butterfly = 0xC2,
        Female_Butterfly = 0xC3,
        Male_Stag_Beetle = 0xC4,
        Female_Stag_Beetle = 0xC5,
        Male_Grasshopper = 0xC6,
        Female_Grasshopper = 0xC7,
        Male_Phasmid = 0xC8,
        Female_Phasmid = 0xC9,
        Male_Pill_Bug = 0xCA,
        Female_Pill_Bug = 0xCB,
        Male_Mantis = 0xCC,
        Female_Mantis = 0xCD,
        Male_Ladybug = 0xCE,
        Female_Ladybug = 0xCF,
        Male_Snail = 0xD0,
        Female_Snail = 0xD1,
        Male_Dragonfly = 0xD2,
        Female_Dragonfly = 0xD3,
        Male_Ant = 0xD4,
        Female_Ant = 0xD5,
        Male_Dayfly = 0xD6,
        Female_Dayfly = 0xD7,
        Progressive_Fused_Shadow = 0xD8,
        Fused_Shadow_2 = 0xD9,
        Fused_Shadow_3 = 0xDA,
        Ancient_Sky_Book_First_Character = 0xDB, /*custom*/
        Ancient_Sky_Book_Second_Character = 0xDC, /*custom*/
        Ancient_Sky_Book_Third_Character = 0xDD, /*custom*/
        Ancient_Sky_Book_Fourth_Character = 0xDE, /*custom*/
        Ancient_Sky_Book_Fifth_Character = 0xDF, /*custom*/
        Poe_Soul = 0xE0,
        Progressive_Hidden_Skill = 0xE1,
        Shield_Attack = 0xE2,
        Back_Slice = 0xE3,
        Helm_Splitter = 0xE4,
        Mortal_Draw = 0xE5,
        Jump_Strike = 0xE6,
        Great_Spin = 0xE7,

        /*?	=	0xE8,*/
        Progressive_Sky_Book = 0xE9,
        Ancient_Sky_Book_Partly_Filled = 0xEA,
        Ancient_Sky_Book_Completed = 0xEB,
        Ooccoo_CitS = 0xEC,
        Purple_Rupee_Links_House = 0xED,
        North_Faron_Woods_Gate_Key = 0xEE,

        /*Blue_Fire?	=	0xEF,*/
        /*Blue_Fire?	=	0xF0,*/
        /*Blue_Fire?	=	0xF1,*/
        /*Blue_Fire?	=	0xF2,*/
        Gate_Keys = 0xF3,
        Snowpeak_Ruins_Ordon_Pumpkin = 0xF4,
        Snowpeak_Ruins_Ordon_Goat_Cheese = 0xF5,
        Snowpeak_Ruins_Bedroom_Key = 0xF6,

        /*Shield?	=	0xF7,*/
        Got_Lantern_Back = 0xF8,
        Goron_Mines_Key_Shard = 0xF9,
        Goron_Mines_Key_Shard_Second = 0xFA,
        Goron_Mines_Key_Shard_3 = 0xFB,

        /*Key?	=	0xFC,*/
        Goron_Mines_Big_Key = 0xFD,
        Coro_Key = 0xFE,
        Gives_Vanilla = 0xFF
    };

    public class ItemFunctions
    {
        public List<Item> RandomizedImportantItems = new();
        public List<Item> StartingItems = new(); // Any items that the player starts with as selected by the gui.
        public List<Item> RandomizedDungeonRegionItems = new(); // Items that are shuffled among dungeons.
        public List<Item> JunkItems = new(); // Extra junk items that are put in the pool if there are checks left and all items have been placed..
        public List<Item> BaseItemPool = new(); // The list of Items that have yet to be randomized..
        public List<Item> heldItems = new(); // The list of items that the player currently has. This is to be used when emulating the playthrough..

        public List<Item> ShuffledDungeonRewards = new();
        internal List<Item> VanillaDungeonRewards =
            new()
            {
                Item.Progressive_Fused_Shadow,
                Item.Progressive_Fused_Shadow,
                Item.Progressive_Fused_Shadow,
                Item.Progressive_Mirror_Shard,
                Item.Progressive_Mirror_Shard,
                Item.Progressive_Mirror_Shard,
            };

        internal List<Item> RegionSmallKeys =
            new()
            {
                Item.Gerudo_Desert_Bulblin_Camp_Key,
                Item.North_Faron_Woods_Gate_Key,
                Item.Forest_Temple_Small_Key,
                Item.Forest_Temple_Small_Key,
                Item.Forest_Temple_Small_Key,
                Item.Forest_Temple_Small_Key,
                Item.Goron_Mines_Small_Key,
                Item.Goron_Mines_Small_Key,
                Item.Goron_Mines_Small_Key,
                Item.Lakebed_Temple_Small_Key,
                Item.Lakebed_Temple_Small_Key,
                Item.Lakebed_Temple_Small_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Arbiters_Grounds_Small_Key,
                Item.Snowpeak_Ruins_Small_Key,
                Item.Snowpeak_Ruins_Small_Key,
                Item.Snowpeak_Ruins_Small_Key,
                Item.Snowpeak_Ruins_Small_Key,
                Item.Temple_of_Time_Small_Key,
                Item.Temple_of_Time_Small_Key,
                Item.Temple_of_Time_Small_Key,
                Item.City_in_The_Sky_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Palace_of_Twilight_Small_Key,
                Item.Hyrule_Castle_Small_Key,
                Item.Hyrule_Castle_Small_Key,
                Item.Hyrule_Castle_Small_Key,
                Item.Snowpeak_Ruins_Ordon_Pumpkin,
                Item.Snowpeak_Ruins_Ordon_Goat_Cheese,
            };

        internal List<Item> DungeonBigKeys =
            new()
            {
                Item.Forest_Temple_Big_Key,
                Item.Goron_Mines_Key_Shard,
                Item.Goron_Mines_Key_Shard,
                Item.Goron_Mines_Key_Shard,
                Item.Lakebed_Temple_Big_Key,
                Item.Arbiters_Grounds_Big_Key,
                Item.Temple_of_Time_Big_Key,
                Item.Snowpeak_Ruins_Bedroom_Key,
                Item.City_in_The_Sky_Big_Key,
                Item.Palace_of_Twilight_Big_Key,
                Item.Hyrule_Castle_Big_Key,
            };

        internal List<Item> DungeonMapsAndCompasses =
            new()
            {
                Item.Forest_Temple_Dungeon_Map,
                Item.Forest_Temple_Compass,
                Item.Goron_Mines_Dungeon_Map,
                Item.Goron_Mines_Compass,
                Item.Lakebed_Temple_Dungeon_Map,
                Item.Lakebed_Temple_Compass,
                Item.Arbiters_Grounds_Dungeon_Map,
                Item.Arbiters_Grounds_Compass,
                Item.Snowpeak_Ruins_Dungeon_Map,
                Item.Snowpeak_Ruins_Compass,
                Item.Temple_of_Time_Dungeon_Map,
                Item.Temple_of_Time_Compass,
                Item.City_in_The_Sky_Dungeon_Map,
                Item.City_in_The_Sky_Compass,
                Item.Palace_of_Twilight_Dungeon_Map,
                Item.Palace_of_Twilight_Compass,
                Item.Hyrule_Castle_Dungeon_Map,
                Item.Hyrule_Castle_Compass,
            };

        internal List<Item> ImportantItems =
            new()
            {
                Item.Progressive_Sword,
                Item.Progressive_Sword,
                Item.Progressive_Sword,
                Item.Progressive_Sword,
                Item.Progressive_Wallet,
                Item.Boomerang,
                Item.Lantern,
                Item.Slingshot,
                Item.Progressive_Fishing_Rod,
                Item.Progressive_Fishing_Rod,
                Item.Iron_Boots,
                Item.Progressive_Bow,
                Item.Filled_Bomb_Bag,
                Item.Zora_Armor,
                Item.Progressive_Clawshot,
                Item.Progressive_Clawshot,
                Item.Shadow_Crystal,
                Item.Aurus_Memo,
                Item.Asheis_Sketch,
                Item.Spinner,
                Item.Ball_and_Chain,
                Item.Progressive_Dominion_Rod,
                Item.Progressive_Dominion_Rod,
                Item.Progressive_Sky_Book,
                Item.Progressive_Sky_Book,
                Item.Progressive_Sky_Book,
                Item.Progressive_Sky_Book,
                Item.Progressive_Sky_Book,
                Item.Progressive_Sky_Book,
                Item.Progressive_Sky_Book,
                Item.Renados_Letter,
                Item.Invoice,
                Item.Wooden_Statue,
                Item.Ilias_Charm,
                Item.Horse_Call,
                Item.Gate_Keys,
                Item.Empty_Bottle,
                Item.Progressive_Hidden_Skill,
                Item.Progressive_Hidden_Skill,
                Item.Progressive_Hidden_Skill,
                Item.Progressive_Hidden_Skill,
                Item.Progressive_Hidden_Skill,
                Item.Progressive_Hidden_Skill,
                Item.Progressive_Hidden_Skill,
                Item.Magic_Armor,
                Item.Ordon_Shield,
            };

        public readonly List<Item> goldenBugs =
            new()
            {
                Item.Male_Ant,
                Item.Female_Ant,
                Item.Male_Beetle,
                Item.Female_Beetle,
                Item.Male_Pill_Bug,
                Item.Female_Pill_Bug,
                Item.Male_Phasmid,
                Item.Female_Phasmid,
                Item.Male_Grasshopper,
                Item.Female_Grasshopper,
                Item.Male_Stag_Beetle,
                Item.Female_Stag_Beetle,
                Item.Male_Butterfly,
                Item.Female_Butterfly,
                Item.Male_Ladybug,
                Item.Female_Ladybug,
                Item.Male_Mantis,
                Item.Female_Mantis,
                Item.Male_Dragonfly,
                Item.Female_Dragonfly,
                Item.Male_Dayfly,
                Item.Female_Dayfly,
                Item.Male_Snail,
                Item.Female_Snail,
            };

        /// <summary>
        /// summary text.
        /// </summary>
        public List<Item> alwaysItems =
            new() // Items from the vanilla pool that are guaranteed to be in every seed
            {
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Piece_of_Heart,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Heart_Container,
                Item.Purple_Rupee_Links_House,
                Item.Green_Rupee,
                Item.Green_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Orange_Rupee,
                Item.Silver_Rupee,
                Item.Silver_Rupee,
                Item.Progressive_Wallet,
                Item.Progressive_Bow,
                Item.Progressive_Bow,
                Item.Filled_Bomb_Bag,
                Item.Filled_Bomb_Bag,
                Item.Giant_Bomb_Bag,
                Item.Sera_Bottle,
                Item.Coro_Bottle,
                Item.Jovani_Bottle,
                Item.Hylian_Shield,
                Item.Hawkeye,
            };

        private readonly List<Item> vanillaJunkItems =
            new() // Junk items from the vanilla pool
            {
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_5,
                Item.Bombs_10,
                Item.Bombs_10,
                Item.Bombs_20,
                Item.Bombs_30,
                Item.Arrows_10,
                Item.Arrows_10,
                Item.Arrows_10,
                Item.Arrows_10,
                Item.Arrows_10,
                Item.Arrows_20,
                Item.Arrows_20,
                Item.Arrows_20,
                Item.Arrows_20,
                Item.Arrows_20,
                Item.Arrows_20,
                Item.Arrows_30,
                Item.Arrows_30,
                Item.Seeds_50,
                Item.Seeds_50,
                Item.Water_Bombs_5,
                Item.Water_Bombs_5,
                Item.Water_Bombs_5,
                Item.Water_Bombs_10,
                Item.Water_Bombs_10,
                Item.Water_Bombs_10,
                Item.Water_Bombs_10,
                Item.Water_Bombs_10,
                Item.Water_Bombs_15,
                Item.Water_Bombs_15,
                Item.Water_Bombs_15,
                Item.Bomblings_5,
                Item.Bomblings_5,
                Item.Bomblings_10,
                Item.Bomblings_10,
                Item.Blue_Rupee,
                Item.Yellow_Rupee,
                Item.Yellow_Rupee,
                Item.Yellow_Rupee,
                Item.Yellow_Rupee,
                Item.Yellow_Rupee,
                Item.Yellow_Rupee,
                Item.Red_Rupee,
                Item.Red_Rupee,
                Item.Red_Rupee,
                Item.Red_Rupee,
                Item.Red_Rupee,
                Item.Red_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee,
                Item.Purple_Rupee
            };

        /// <summary>
        /// summary text.
        /// </summary>
        public void GenerateItemPool()
        {
            SharedSettings parseSetting = Randomizer.SSettings;
            Randomizer.Items.RandomizedImportantItems.AddRange(this.ImportantItems);
            Randomizer.Items.BaseItemPool.AddRange(this.VanillaDungeonRewards);
            Randomizer.Items.ShuffledDungeonRewards.AddRange(this.VanillaDungeonRewards);

            if (parseSetting.shufflePoes)
            {
                this.RandomizedImportantItems.AddRange(Enumerable.Repeat(Item.Poe_Soul, 60));
            }

            if (parseSetting.shuffleGoldenBugs)
            {
                this.RandomizedImportantItems.AddRange(this.goldenBugs);
            }

            // Check Small Key settings before adding them to the rando pool
            if (
                (parseSetting.smallKeySettings == SmallKeySettings.Own_Dungeon)
                || (parseSetting.smallKeySettings == SmallKeySettings.Any_Dungeon)
            )
            {
                this.RandomizedDungeonRegionItems.AddRange(this.RegionSmallKeys);
                Randomizer.Items.BaseItemPool.AddRange(this.RegionSmallKeys);
            }
            else if (parseSetting.smallKeySettings == SmallKeySettings.Anywhere)
            {
                this.RandomizedImportantItems.AddRange(this.RegionSmallKeys);
            }
            else if (parseSetting.smallKeySettings == SmallKeySettings.Keysey)
            {
                this.RandomizedImportantItems.Remove(Item.Gate_Keys);
                if (!parseSetting.startingItems.Contains(Item.Gerudo_Desert_Bulblin_Camp_Key))
                {
                    parseSetting.startingItems.Add(Item.Gerudo_Desert_Bulblin_Camp_Key);
                }
            }

            // Check Big Key Settings before adding them to the pool
            if (
                (parseSetting.bigKeySettings == BigKeySettings.Own_Dungeon)
                || (parseSetting.bigKeySettings == BigKeySettings.Any_Dungeon)
            )
            {
                this.RandomizedDungeonRegionItems.AddRange(this.DungeonBigKeys);
                Randomizer.Items.BaseItemPool.AddRange(this.DungeonBigKeys);
            }
            else if (parseSetting.bigKeySettings == BigKeySettings.Anywhere)
            {
                this.RandomizedImportantItems.AddRange(this.DungeonBigKeys);
            }

            // Check Map and Compass settings before adding to pool
            if (
                (parseSetting.mapAndCompassSettings == MapAndCompassSettings.Own_Dungeon)
                || (parseSetting.mapAndCompassSettings == MapAndCompassSettings.Any_Dungeon)
            )
            {
                this.RandomizedDungeonRegionItems.AddRange(this.DungeonMapsAndCompasses);
                Randomizer.Items.BaseItemPool.AddRange(this.DungeonMapsAndCompasses);
            }
            else if (parseSetting.mapAndCompassSettings == MapAndCompassSettings.Anywhere)
            {
                this.RandomizedImportantItems.AddRange(this.DungeonMapsAndCompasses);
            }

            // Modifying Item Pool based on ice trap settings
            // If we have Ice Trap Mayhem or Nightmare, all extra junk items are replaced with Foolish Items
            switch (parseSetting.trapFrequency)
            {
                case TrapFrequency.Few: // There is a small chance that a Foolish Item could appear
                {
                    this.JunkItems.AddRange(this.vanillaJunkItems);
                    this.JunkItems.AddRange(Enumerable.Repeat(Item.Foolish_Item, 6));
                    break;
                }

                case TrapFrequency.Many: // There is an increased chance that a Foolish Item could appear
                {
                    this.JunkItems.AddRange(this.vanillaJunkItems);
                    this.JunkItems.AddRange(Enumerable.Repeat(Item.Foolish_Item, 27));
                    break;
                }

                case TrapFrequency.Mayhem: // All junk items outside of the item pool are Foolish Items
                {
                    this.JunkItems.AddRange(this.vanillaJunkItems);
                    this.JunkItems.AddRange(Enumerable.Repeat(Item.Foolish_Item, 64));
                    break;
                }

                case TrapFrequency.Nightmare: // All junk items are Foolish Items
                {
                    this.JunkItems.Add(Item.Foolish_Item);
                    break;
                }

                default:
                {
                    this.JunkItems.AddRange(this.vanillaJunkItems);
                    break;
                }
            }

            foreach (Item startingItem in parseSetting.startingItems)
            {
                RandomizedImportantItems.Remove(startingItem);
            }

            // If a poe is excluded, we still want to place the item that was in its location.
            foreach (string excludedCheck in parseSetting.excludedChecks)
            {
                if (Randomizer.Checks.CheckDict[excludedCheck].itemId == Item.Poe_Soul)
                {
                    if (!parseSetting.shufflePoes)
                    {
                        this.alwaysItems.Add(Randomizer.Checks.CheckDict[excludedCheck].itemId);
                    }
                }
            }

            // Remove the bulblin camp key from the item pool if we have the setting to skip Bulblin Camp enabled.
            if (parseSetting.skipArbitersEntrance)
            {
                if (parseSetting.smallKeySettings == SmallKeySettings.Anywhere)
                {
                    this.RandomizedImportantItems.Remove(Item.Gerudo_Desert_Bulblin_Camp_Key);
                }
                else
                {
                    this.alwaysItems.Remove(Item.Gerudo_Desert_Bulblin_Camp_Key);
                }
            }

            Randomizer.Items.BaseItemPool.AddRange(this.RandomizedImportantItems);
            return;
        }
    }
}
