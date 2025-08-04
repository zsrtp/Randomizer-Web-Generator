namespace TPRandomizer.Assets
{
    using System.Collections.Generic;
    using TPRandomizer.SSettings.Enums;

    // Note on how the region flags are defined: The first number in the pair is
    // the region node index (ex: Ordon is node 0, Hyrule Castle is node 0x18).
    // The second number defines the offset and mask. The lower 3 bits (val % 8)
    // says how much to right-shift a mask of 0x80 (shift 7 would give 0x01).
    // The upper 5 bits say the offset within the region node. For example, 0x5A
    // is 0b01011010 which is 01011 and 010. This means the offset is 0b01011
    // (or 0xB) and the mask is 0x80 >> 2 (or 0x20).

    /// <summary>
    /// summary text.
    /// </summary>
    public class Flags
    {
        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] FaronTwilightRegionFlags = new byte[,]
        {
            { 0x2, 0x46 }, // Midna jump 1 mist area.
            { 0x2, 0x47 }, // Midna jump 1 mist area.
            { 0x2, 0x98 }, // South Faron Portal.
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] EldinTwilightRegionFlags = new byte[,]
        {
            { 0x3, 0x14 }, // Collected Tear From Bomb Storage
            { 0x3, 0x1A }, // Collected Tear From Bomb Storage
            { 0x3, 0x1B }, // Collected Tear From Bomb Storage
            { 0x3, 0x67 }, // Ant house entered from top
            { 0x3, 0x64 }, // Ant house box pushed
            { 0x3, 0x5E }, // Defeated Ant house Tears of Light bug
            { 0x3, 0x1E }, // Collected Tear from Ant house
            { 0x3, 0xBD }, // Done Midna jumps in ant house.
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] BaseRandomizerRegionFlags = new byte[,]
        {
            { 0x0, 0x57 }, // Spider on Link's Ladder killed.
            { 0x2, 0x63 }, // Trill lets you shop at his store.
            { 0x2, 0x48 }, // Talked to Coro after bugs
            { 0x3, 0xB9 }, // Barnes sells water bombs.
            { 0x3, 0xB3 }, // Colin Rescued CS (Malo Mart is Open).
            { 0x2, 0x60 }, // Got Lantern Back from Monkey
            { 0x2, 0x61 }, // Saw bugs move in Coro's house
            { 0x2, 0x7D }, // Talked to Midna about Coro spirit
            { 0x2, 0x4E }, // Saved Monkey from Puppets.
            { 0x2, 0x62 }, // Midna text before jumping to lost woods
            { 0x6, 0x4C }, // Bridge of Eldin Warped back CS.
            { 0x2, 0x95 }, // Midna text after warping to North Faron for bridge.
            { 0x2, 0xBF }, // Burned First cobweb in faron cave
            { 0x2, 0xBE }, // Burned second cobweb in faron cave
            { 0xA, 0x99 }, // Desert Entrance CS.
            { 0xA, 0x20 }, // Set Freestanding key flag.
            { 0x3, 0xA4 }, // Barnes Sells Bombs.
            { 0x6, 0x7E }, // Kakariko Gorge placed CS
            { 0x10, 0x49 }, // FT Ook Bridge Destroyed
            { 0x17, 0x94 }, // Set flag to trigger the twilight fence before Zant
            { 0x3, 0x42 }, // Big Rock fell at DMT
            { 0x6, 0x83 }, // Set the flag for the Ganon Barriers in Hyrule Field during Eldin Twilight.
            { 0x9, 0x40 }, // Original Jovani Poe killed. It is replaced with a custom actor.
            { 0x9, 0x76 }, // Jovani Chest CS 2
            { 0x9, 0x7F }, // Open Chest to Jovani
            { 0x9, 0x7E }, // Jovani Chest CS
            { 0x9, 0x50 }, // Set flag for Midna breaking Barrier CS.
            { 0x3, 0xA7 }, // Unlock Jumps to top of Sanctuary
            { 0x3, 0x9A }, // Kakariko Village intro CS.
            { 0xA, 0x7F }, // Mirror Raised Cutscene Flag (Places Boar at desert entrance)
            { 0x0, 0x7E }, // Midna jumps to Shop unlocked
            { 0x17, 0x5E }, // Palace of Twilight intro CS and removes the invisible waterfall wall with a custom ARC patch.
            { 0x17, 0x64 }, // East platform to palace active.
            { 0x3, 0x54 }, // Custom flag. Sets the sign in Kak Malo mart slot 1 to appear.
            { 0x3, 0x99 }, // Remove wooden shield from Kak Malo Mart counter.
            { 0x0, 0x63 }, // Spawn the Chest in Link's House
            { 0x9, 0xBC }, // Spawn Gengle by default as his actor interferes with the poe soul
            { 0x7, 0x58 }, // Sacred Grove MS Pedestal Map
            { 0x0, 0x6B }, // Ordon Spring Portal.
            { 0x0, 0x44 }, // Midna Text after Ordon Shield (Spawns sword)
            { 0x0, 0x46 }, // Midna Text after Ordon Sword
            { 0x0, 0x68 }, // Approach faron wall with Midna
            { 0x0, 0xA0 }, // Midna allows player to approach Faron Twilight Wall
            { 0x0, 0xBA }, // Explored area outside Link's house as wolf
            { 0x0, 0x61 }, // Defeated first bulblin outside link's house
            { 0x0, 0x62 }, // Defeated second bulblin outside link's house
            { 0x0, 0x60 }, // Defeated Hugo
            { 0x18, 0x4B }, // Watched CS with Allies in HC.
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] LanayruTwilightRegionFlags = new byte[,]
        {
            { 0x6, 0x58 }, // Lake Hylia has water on Hyrule Field Map
            { 0x4, 0x7F }, // Lake Hylia has water on Lake Hylia Map.
        };

        public static readonly byte[,] MDHRegionFlags = new byte[,]
        {
            { 0x4, 0x51 }, // Set flag for MDH Cutscene in Lake Hylia
        };

        public static readonly byte[,] CutsceneRegionFlags = new byte[,]
        {
            { 0x0, 0x4A }, // Ordon Day 3 Intro CS.
            { 0x0, 0x4C }, // Knocked down Ordon bee nest CS.
            { 0x0, 0x4E }, // Ordon Ranch first time CS.
            { 0x0, 0x53 }, // Ilia spring CS watched.
            { 0x0, 0x54 }, // Ilia spring CS started.
            { 0x0, 0x55 }, // Ordon Village first time CS.
            { 0x0, 0x56 }, // Ilia spring CS trigger.
            { 0x0, 0x68 }, // Approach Faron Twilgiht with Midna CS.
            { 0x0, 0x6E }, // Enter shield house as wolf CS.
            { 0x0, 0x75 }, // Midna text after hearing Bo and Jaggle talk about the shield.
            { 0x0, 0x7C }, // Midna text before jumping to Ordon Shop roof.
            { 0x0, 0x7D }, // Rusl talking to Uli during wolf night CS.
            { 0x0, 0xB8 }, // Enter Ordon Village as wolf CS.
            { 0x1, 0x42 }, // Midna text after first gate in sewers.
            { 0x1, 0x43 }, // Midna text after exiting to rooftops.
            { 0x1, 0x51 }, // Zelda tower intro CS.
            { 0x1, 0x57 }, // Outside top door intro CS.
            { 0x1, 0x5A }, // Went to the otherside of the fence in sewers CS.
            { 0x1, 0x5B }, // Top of stairway intro CS.
            { 0x1, 0x5C }, // Stairway intro CS.
            { 0x1, 0x7B }, // Midna text when approaching the rooftop guard.
            { 0x2, 0x74 }, // Faron intro CS.
            { 0x2, 0x77 }, // See Faron Light Spirit from afar CS.
            { 0x2, 0x7C }, // Entered mist area as human.
            { 0x3, 0x49 }, // Death mountain intro CS.
            { 0x3, 0x83 }, // Kakariko Graveyard intro CS.
            { 0x3, 0x8C }, // Midna text after Meteor fell.
            { 0x4, 0x58 }, // Talked to Rutella in Lanayru Twilight.
            { 0x4, 0x5F }, // Zora's domain intro CS twilight.
            { 0x4, 0x67 }, // Midna text after jumping to Lake from burning bridge.
            { 0x4, 0x6B }, // Zora's Domain exit flood water cutscene.
            { 0x4, 0x72 }, // Midna text after arriving at frozen Upper Zora River.
            { 0x4, 0x91 }, // Midna text after frozen Zora Domain intro CS.
            { 0x4, 0xB0 }, // Watched CS of Ooccoo running to Sky Cannon.
            { 0x6, 0x68 }, // Midna text after warping Gorge bridge.
            { 0x6, 0x7C }, // Midna text after Lanayru Field twilight CS.
            { 0x6, 0x72 }, // Faron Field intro CS.
            { 0x6, 0x40 }, // Twilight Lanayru Field intro CS.
            { 0x6, 0x4F }, // Cutscene of gate outside Kakariko Village.
            { 0x6, 0xB3 }, // Midna text after entering Lanayru Twilight.
            { 0x6, 0xB4 }, // Midna text when seeing Lanayru Twilight from far away.
            { 0x6, 0xB6 }, // Midna text after entering Eldin Twilight.
            { 0x6, 0xB7 }, // Midna text when seeing Eldin Twilight from far away.
            { 0x7, 0x42 }, // Midna text after pushing block shortcut as human after Grove 2.
            { 0x7, 0x44 }, // Lost Woods intro CS.
            { 0x8, 0x45 }, // Snowpeak Summit intro CS.
            { 0x8, 0x5E }, // Midna text outside SPR.
            { 0x8, 0x5F }, // Snowpeak intro CS.
            { 0x9, 0x55 }, // STAR Tent intro CS.
            { 0x9, 0x7D }, // Jovani House intro CS.
            { 0xA, 0x53 }, // Mirror Chamber Intro CS.
            { 0x10, 0x41 }, // Midna text after getting Boomerang.
            { 0x10, 0x42 }, // Midna text after Ook breaks the bridge.
            { 0x10, 0x47 }, // Midna text after freeing first monkey.
            { 0x10, 0x49 }, // Bridge before Ook broken.
            { 0x10, 0x56 }, // Bokoblins spot Link in windless bridge room.
            { 0x10, 0x57 }, // Turned bridge in windless bridge room.
            { 0x10, 0x72 }, // West Tile Worm room intro CS.
            { 0x10, 0x76 }, // Second monkey room intro CS.
            { 0x10, 0x7C }, // Big Baba room intro CS.
            { 0x10, 0x7D }, // Midna text in room before boss room.
            { 0x10, 0x7E }, // Midna text after saving monkey after defeating Ook.
            { 0x10, 0x85 }, // Midna text after opening hanging chest.
            { 0x10, 0x83 }, // East outside room intro CS.
            { 0x10, 0xB6 }, // Forest Temple intro CS.
            { 0x11, 0x43 }, // Cut rope of door in outside room CS.
            { 0x11, 0x44 }, // Pressed second button of the main magnet room of the second floor CS trigger.
            { 0x11, 0x45 }, // Pressed third button in entrance room CS.
            { 0x11, 0x46 }, // Pressed second button in entrance room CS.
            { 0x11, 0x47 }, // Cut rope of door in Toadpoli room CS.
            { 0x11, 0x4A }, // Pressed first button of the main magnet room on the second floor CS.
            { 0x11, 0x68 }, // Pressed outside magnet switch for first time CS.
            { 0x11, 0x72 }, // Main magnet room intro CS.
            { 0x11, 0x73 }, // Main magnet room intro CS trigger.
            { 0x11, 0x7A }, // Outside room intro CS.
            { 0x11, 0x80 }, // Room after Bow chest intro CS.
            { 0x11, 0x81 }, // Pulled Beamos in outside room CS.
            { 0x11, 0x84 }, // Open gate in Toadpoli room CS.
            { 0x11, 0x85 }, // Pressed second button in Toadpoli room CS.
            { 0x11, 0x88 }, // Magnet maze room intro CS.
            { 0x11, 0x8A }, // Goron Mines intro CS.
            { 0x11, 0x8B }, // Pressed first button in entrance room CS.
            { 0x11, 0x8C }, // Open gate in entrance room CS.
            { 0x11, 0xBC }, // Main magnet room second floor intro CS.
            { 0x11, 0xBD }, // Main magnet room second floor intro CS trigger.
            { 0x11, 0xBE }, // Hit crystal switch in room after bow chest CS.
            { 0x11, 0xBF }, // Room after Bow chest intro CS trigger.
            { 0x12, 0x46 }, // Midna Stalactite text in second room.
            { 0x12, 0x7E }, // Horizontal wheel is turning in east room CS.
            { 0x12, 0x7F }, // Horizontal wheel is turning in east room CS trigger.
            { 0x12, 0xA5 }, // Central room intro CS.
            { 0x12, 0xA6 }, // South bridge to main room intro CS.
            { 0x12, 0xA7 }, // Lakebed Temple intro CS.
            { 0x12, 0xAA }, // Rotate staircase main room CS.
            { 0x12, 0xB4 }, // East water supply Chu Worm CS.
            { 0x13, 0x5A }, // Turn walls in third room Basement second floor CS.
            { 0x13, 0x73 }, // Arbiters Grounds intro CS.
            { 0x13, 0x94 }, // Risen tracks on pilar before boss CS.
            { 0x14, 0x83 }, // First Floor Northwest room intro CS.
            { 0x14, 0xA1 }, // Midna text after finding Bedroom Key.
            { 0x14, 0xA7 }, // Snowpeak Ruins intro CS.
            { 0x14, 0xAA }, // Freezard in cage CS.
            { 0x14, 0xAC }, // Courtyard intro CS.
            { 0x14, 0xAE }, // Pumpkin room intro CS.
            { 0x14, 0xB2 }, // Midna text after getting Cheese.
            { 0x14, 0xB4 }, // Midna text after getting Pumpkin.
            { 0x15, 0x40 }, // Midna text telling you to use your senses on the missing statue.
            { 0x15, 0x41 }, // Midna text after using senses on missing statue.
            { 0x15, 0x4A }, // Temple of Time intro CS.
            { 0x15, 0x4B }, // Scales of Time room intro CS.
            { 0x15, 0x4C }, // CS after changing the balance on the scales for the first time.
            { 0x15, 0x4D }, // CS trigger after changing the balance on the scales for the first time.
            { 0x15, 0x90 }, // Pressed button in room 1 for the first time CS.
            { 0x15, 0x91 }, // Pressed the button on the seventh floor for the first time CS.
            { 0x15, 0x94 }, // Pressed the button on the fifth floor for the first time CS.
            { 0x15, 0x95 }, // Pressed buttons on third floor for the first time CS.
            { 0x15, 0x96 }, // Pressed the button on the second floor for the first time CS.
            { 0x16, 0x64 }, // North wing main room intro CS.
            { 0x16, 0x65 }, // East wing fan room second floor intro CS.
            { 0x16, 0x66 }, // Went beyond first gate outside shop intro CS.
            { 0x16, 0x67 }, // City in The Sky intro CS.
            { 0x16, 0x6E }, // East bridge extended CS.
            { 0x17, 0x4D }, // Phantom Zant 1 CS.
            { 0x17, 0x66 }, // Midna text when west hand steals sol.
            { 0x17, 0x6F }, // Midna text about black fog in west room.
            { 0x17, 0x70 }, // Midna text after finding west sol.
            { 0x17, 0x72 }, // Midna text trigger when seeing a Twili for the first time.
            { 0x17, 0x78 }, // Midna text when seeing a Twili for the first time.
            { 0x17, 0x79 }, // Midna text after Light Sword cutscene.
            { 0x17, 0x95 }, // Midna text after re-entering west wing after sol was stolen.
            { 0x17, 0x9E }, // Midna text at dungeon entrance.
            { 0x17, 0xB3 }, // Watched east wing second room stairs CS.
            { 0x18, 0x4F }, // Hyrule Castle Graveyard intro CS.
            { 0x18, 0x8C }, // East garden intro CS.
            { 0x18, 0x8D }, // East garden intro CS trigger.
            { 0x18, 0x77 }, // Midna text at the east end of the east garden.
            { 0x18, 0x82 }, // South garden intro CS.
            { 0x18, 0x99 }, // Double Darknut room intro CS
            { 0x18, 0xA4 }, // Midna text after Owl Statue chest in graveyard.
            { 0x18, 0xB7 }, // Lit southeast torch in second floor north room for the first time CS.
            { 0x18, 0xB8 }, // Lit northeast torch in second floor north room for the first time CS.
            { 0x13, 0x9C }, // Main room poes taking flames cs.
            { 0x13, 0x9A }, // Close poe door
            { 0x15, 0x54 }, // statue getting possessed for the first time cs
            { 0x7, 0x43 }, // cs after pushing block human
        };

        public static readonly byte[,] SmallKeyRegionFlags = new byte[,]
        {
            { 0x2, 0x53 }, // Coro gate unlocked.
            { 0x2, 0x4B }, // North Faron Gate Unlocked.
            { 0x3, 0xBA }, // Followed Rutella to graveyard.
            { 0x3, 0xB6 }, // Started Rutella escort.
            { 0x10, 0x54 }, // Unlocked door to Second Monkey.
            { 0x10, 0x58 }, // Unlock windless bridge east door.
            { 0x10, 0x61 }, // Opened big baba monkey cage.
            { 0x10, 0x74 }, // Opened tile worm monkey cage.
            { 0x11, 0x60 }, // Unlock north door in toadpoli room.
            { 0x11, 0x62 }, // Unlock west locked door in main magnet room.
            { 0x11, 0x6C }, // Unlock east outside door.
            { 0x12, 0x6B }, // Unlock east door main room 2F.
            { 0x12, 0x7B }, // Unlocked door in second east room 2F.
            { 0x12, 0x7C }, // Unlocked door before Deku Toad.
            { 0x13, 0x78 }, // Unlocked door in second east room 2F.
            { 0x13, 0x84 }, // Unlocked door in elevator room 2B.
            { 0x13, 0x85 }, // Unlocked door in first room.
            { 0x13, 0x92 }, // Unlocked door in first east room 1F.
            { 0x13, 0x99 }, // Unlocked door in fourth east room.
            { 0x14, 0x4D }, // Unlock North lobby door.
            { 0x14, 0x4C }, // Unlock West lobby door.
            { 0x14, 0x6F }, // Unlock door in southeast room 2F.
            { 0x14, 0x73 }, // Unlock door in east outside hallway.
            { 0x14, 0x74 }, // Unlock west door in courtyard.
            { 0x14, 0x70 }, // Unlock door to lobby from Freezard room.
            { 0x15, 0x44 }, // Unlock door in room 1.
            { 0x15, 0x42 }, // Unlock door in room 6 on 8F.
            { 0x15, 0x43 }, // Unlock door in 5F.
            { 0x16, 0x59 }, // Unlock east bridge door.
            { 0x17, 0x57 }, // Unlock door in north room 3.
            { 0x17, 0x58 }, // Unlock door in east room 2.
            { 0x17, 0x59 }, // Unlock door in west room 2.
            { 0x17, 0x6C }, // Unlock door in north room 2.
            { 0x17, 0x7A }, // Unlock door in norht room 1.
            { 0x17, 0x7B }, // Unlock door in east room 1.
            { 0x17, 0x7C }, // Unlock door in west room 1.
            { 0x18, 0x93 }, // Unlock door outside 3F.
            { 0x18, 0xB0 }, // Unlock treasure room door.
            { 0x18, 0xA3 }, // Unlock door in south garden.
        };

        public static readonly byte[,] BigKeyRegionFlags = new byte[,]
        {
            { 0x10, 0x48 }, // Unlocked Forest Temple Boss Door.
            { 0x10, 0xED }, // Got Forest Temple Big Key.
            { 0x11, 0x48 }, // Unlocked Goron Mines Boss Door.
            { 0x11, 0xED }, // Got Goron Mines Big Key.
            { 0x12, 0x8A }, // Unlocked Lakebed Temple Boss Door.
            { 0x12, 0xED }, // Got Lakebed Temple Big Key.
            { 0x13, 0x47 }, // Unlocked Arbiter's Grounds Boss Door.
            { 0x13, 0xED }, // Got Arbiter's Grounds Big Key.
            { 0x14, 0x57 }, // Unlocked Snowpeak Ruins Boss Door.
            { 0x14, 0xED }, // Got Snowpeak Ruins Big Key.
            { 0x14, 0x56 }, // Watched CS of Yeta entering boss room.
            { 0x15, 0x7F }, // Unlocked Temple of Time Boss Door.
            { 0x15, 0xED }, // Got Temple of Time Big Key.
            { 0x16, 0x58 }, // Unlocked City in The Sky Boss Door.
            { 0x16, 0xED }, // Got City in The Sky Big Key.
            { 0x17, 0x56 }, // Unlocked Palace of Twilight Boss Door.
            { 0x17, 0xED }, // Got Palace of Twilight Big Key.
            { 0x18, 0xA1 }, // Unlocked Hyrule Castle Boss Door.
            { 0x18, 0xED }, // Got Hyrule Castle Big Key.
        };

        public static readonly byte[,] MapAndCompassRegionFlags = new byte[,]
        {
            { 0x10, 0xEE }, // Got Forest Temple Compass.
            { 0x11, 0xEE }, // Got Goron Mines Compass.
            { 0x12, 0xEE }, // Got Lakebed Temple Compass.
            { 0x13, 0xEE }, // Got Arbiter's Grounds Compass.
            { 0x14, 0xEE }, // Got Snowpeak Ruins Compass.
            { 0x15, 0xEE }, // Got Temple of Time Compass.
            { 0x16, 0xEE }, // Got City in The Sky Compass.
            { 0x17, 0xEE }, // Got Palace of Twilight Compass.
            { 0x18, 0xEE }, // Got Hyrule Castle Compass.
            { 0x10, 0xEF }, // Got Forest Temple Dungeon Map.
            { 0x11, 0xEF }, // Got Goron Mines Dungeon Map.
            { 0x12, 0xEF }, // Got Lakebed Temple Dungeon Map.
            { 0x13, 0xEF }, // Got Arbiter's Grounds Dungeon Map.
            { 0x14, 0xEF }, // Got Snowpeak Ruins Dungeon Map.
            { 0x15, 0xEF }, // Got Temple of Time Dungeon Map.
            { 0x16, 0xEF }, // Got City in The Sky Dungeon Map.
            { 0x17, 0xEF }, // Got Palace of Twilight Dungeon Map.
            { 0x18, 0xEF }, // Got Hyrule Castle Dungeon Map.
        };

        public static readonly byte[,] OpenLakebedRegionFlags = new byte[,]
        {
            { 0x4, 0x70 }, // Blew up rock in front of lakebed CS.
            { 0x4, 0x78 }, // Blew up rock in front of lakebed.
        };

        public static readonly byte[,] OpenArbitersRegionFlags = new byte[,]
        {
            { 0xA, 0x43 }, // Explored part 9 of the Bulblin camp area
            { 0xA, 0x44 }, // Explored part 8 of the Bulblin camp area
            { 0xA, 0x45 }, // Explored part 7 of the Bulblin camp area
            { 0xA, 0x46 }, // Explored part 6 of the Bulblin camp area
            { 0xA, 0x47 }, // Explored part 5 of the Bulblin camp area
            { 0xA, 0x4C }, // Explored part 2 of the Bulblin camp area
            { 0xA, 0x4D }, // Explored part 4 of the Bulblin camp area
            { 0xA, 0x4E }, // Explored part 3 of the Bulblin camp area
        };

        public static readonly byte[,] OpenToTRegionFlags = new byte[,]
        {
            { 0x7, 0x49 }, // Stairs to Temple of time created.
            { 0x7, 0x4A }, // Struck master sword pedestal with sword.
            { 0x7, 0x4B }, // Stairs and window appear and work properly (Past).
            { 0x7, 0xBC }, // Statue in present is gone.
        };
        public static readonly byte[,] OpenGroveRegionFlags = new byte[,]
        {
            { 0x7, 0xB6 }, // Skull Kid - Human defeated.
            { 0x7, 0xB7 }, // Lost Woods Turns to day after defeating Skull Kid - Human
            { 0x7, 0x5B }, // Block pushed down
            { 0x7, 0x42 }, // Midna text after block pushed down
            { 0x7, 0x43 }, // cs after pushing block human
        };
        public static readonly byte[,] OpenSnowpeakRegionFlags = new byte[,]
        {
            { 0x8, 0x49 }, // Snowpeak summit cs.
            { 0x8, 0x45 }, // Snowpeak Summit intro CS.
        };
        public static readonly byte[,] OpenDMTRegionFlags = new byte[,]
        {
            { 0x3, 0x79 }, // moved death mountain rock to exit
            { 0x3, 0x8F }, // moved death mountain rock to hot spring water
            { 0x3, 0xB0 }, // Goron lets you enter elevator in sumo hall
        };
        public static readonly byte[,] OpenDotRegionFlags = new byte[,]
        {
            { 0x15, 0x59 }, // deactivate statue slot in room 1 (opens door and deactivates statue)
            { 0x15, 0x80 }, // open big door in room 1 cs part 2
            { 0x15, 0x81 }, // open big door in room 1 cs part 1
            { 0x15, 0xBC }, // big door in room 1 opens
            { 0x15, 0xBE }, // open big door in room 1 cs part 1 trigger
            { 0x15, 0xBD }, // open big door in room 1 cs part 2 trigger
            { 0x15, 0xBF }, // statue placed in slot in room 1
        };

        public static readonly byte[,] HcShortcutFlags = new byte[,]
        {
            { 0x18, 0x6F }, // watched double Dinalfos cs 1
            { 0x18, 0x70 }, // watched double Dinalfos cs 2
            { 0x18, 0x85 }, // watched focus on lowered chandelier cs
            { 0x18, 0x9D }, // lower the main hall chandelier
            { 0x18, 0xAF }, // defeated double Dinalfos (opens gates both sides)
        };

        public static readonly byte[,] OpenMapRegionFlags = new byte[,]
        {
            // Lake Long Cave Flags
            { 0x1A, 0x40 },
            { 0x1A, 0x41 },
            { 0x1A, 0x42 },
            { 0x1A, 0x43 },
            { 0x1A, 0x44 },
            { 0x1A, 0x45 },
            { 0x1A, 0x46 },
            { 0x1A, 0x47 },
            { 0x1A, 0x48 },
            { 0x1A, 0x49 },
            { 0x1A, 0x4A },
            { 0x1A, 0x4B },
            { 0x1A, 0x4C },
            { 0x1A, 0x4D },
            { 0x1A, 0x4E },
            { 0x1A, 0x4F },
            { 0x1A, 0x50 },
            { 0x1A, 0x51 },
            { 0x1A, 0x52 },
            { 0x1A, 0x53 },
            { 0x1A, 0x54 },
            { 0x1A, 0x55 },
            { 0x1A, 0x56 },
            { 0x1A, 0x57 },
            { 0x1A, 0x58 },
            { 0x1A, 0x59 },
            { 0x1A, 0x5A },
            { 0x1A, 0x5B },
            { 0x1A, 0x5C },
            { 0x1A, 0x5D },
            { 0x1A, 0x5E },
            { 0x1A, 0x75 },
            { 0x1A, 0x76 },
            { 0x1A, 0x77 },
            { 0x1A, 0x78 },
            { 0x1A, 0x79 },
            { 0x1A, 0x7A },
            { 0x1A, 0x7B },
            { 0x1A, 0x7C },
            { 0x1A, 0x7D },
            { 0x1A, 0x7E },
            { 0x1A, 0x7F },
            // Eldin Cave Map Flags
            { 0x19, 0xA1 },
            { 0x19, 0xA2 },
            { 0x19, 0xA3 },
            { 0x19, 0xA4 },
            { 0x19, 0xA5 },
            { 0x19, 0xA6 },
            { 0x19, 0xA7 },
            { 0x19, 0xA8 },
            { 0x19, 0xA9 },
            { 0x19, 0xAA },
            { 0x19, 0xAB },
            { 0x19, 0xAC },
            { 0x19, 0xAD },
            { 0x19, 0xAE },
            { 0x19, 0xAF },
            { 0x19, 0xB0 },
            { 0x19, 0xB1 },
            { 0x19, 0xB2 },
            { 0x19, 0xB3 },
            { 0x19, 0xB4 },
            { 0x19, 0xB5 },
            { 0x19, 0xB6 },
            { 0x19, 0xB7 },
            { 0x19, 0xB8 },
            { 0x19, 0xB9 },
            { 0x19, 0xBA },
            { 0x19, 0xBB },
        };

        public static readonly byte[,] StatueRegionFlags = new byte[,]
        {
            { 0x9, 0x56 }, // Remove invisible wall from Doctor
        };

        public static readonly byte[,] CharmRegionFlags = new byte[,]
        {
            { 0x6, 0x43 }, // Remove HV rocks from Hyrule field
            { 0x3, 0x70 }, // Darbus destroyed HV rocks
        };

        public static readonly byte[,] DungeonERRegionFlags = new byte[,] { };

        public static readonly byte[,] HCBKRegionFlags = new byte[,]
        {
            { 0x18, 0x94 }, // Open HC BK gate
        };

        public static readonly byte[,] bridgeDonationRegionFlags = new byte[,]
        {
            { 0x6, 0x44 }, // East Castle Town Bridge
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly Dictionary<int, byte[,]> RegionFlags =
            new()
            {
                { 1, FaronTwilightRegionFlags },
                { 2, EldinTwilightRegionFlags },
                { 3, LanayruTwilightRegionFlags },
                { 4, CutsceneRegionFlags },
                { 6, MDHRegionFlags },
                { 7, SmallKeyRegionFlags },
                { 8, BigKeyRegionFlags },
                { 9, MapAndCompassRegionFlags },
                { 13, OpenLakebedRegionFlags },
                { 14, OpenArbitersRegionFlags },
                { 15, OpenSnowpeakRegionFlags },
                { 16, OpenToTRegionFlags },
                { 18, OpenGroveRegionFlags },
                { 19, OpenDMTRegionFlags },
                { 20, OpenDotRegionFlags },
                { 21, OpenMapRegionFlags },
                { 22, HcShortcutFlags },
                { 26, StatueRegionFlags },
                { 27, CharmRegionFlags },
                { 28, DungeonERRegionFlags },
                { 29, HCBKRegionFlags },
                { 30, bridgeDonationRegionFlags },
            };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] BaseRandomizerEventFlags = new byte[,]
        {
            { 0x3, 0x82 }, // Gave wooden sword to Talo. Talked to squirrel outside link's house
            { 0x6, 0x29 }, // Tame Epona, KB1 trigger activated, Warped Kakariko Bridge Back.
            { 0xF, 0x40 }, // Talked to Doctor for the first time.
            { 0x12, 0x8 }, // Can use Sera's Shop.
            { 0x14, 0x10 }, // Put Bo outside, ready to wrestle
            { 0xA, 0x2F }, // Bridge of Eldin Stolen, KB1 defeated, KB1 started
            { 0xF, 0x68 }, // Bridge of Eldin Warped Back, forced text when entering dr. clinic, talked to dr before giving invoice
            { 0x40, 0x88 }, // Saved monkey from puppets, Visited Gerudo Desert for the first time.
            { 0x41, 0x18 }, // Talked to Fado after Faron and Eldin Twilight
            { 0x7, 0xA0 }, // Watched Colin CS after KB1, talked to Bo before sumo
            { 0x20, 0x20 }, // Master Sword Story Progression
            { 0x20, 0x10 }, // Arbiters Grounds Story Progression
            { 0x2C, 0x10 }, // Raised the mirror in the Mirror Chamber
            { 0x1B, 0x38 }, // Skip Monkey Escort
            { 0x1C, 0x20 }, // Talked to Bo after opening boots chest.
            { 0x5F, 0x20 }, // Shad leaves sanctuary.
            { 0xF7, 0x1 }, // Add 256 Rupees to Charlo.
            { 0xF8, 0xF4 }, // Add 244 Rupees to Charlo.
            { 0x60, 0x1 }, // Talked to Fyer after Lanayru Twilight
            { 0x38, 0x80 }, // Talked to Jovani after defeating Poe.
            { 0x22, 0x8 }, // Talked to Yeto on top of the mountain after clearing SPR
            { 0x3B, 0x40 }, // Won Snowboard race against Yeto.
            { 0x2F, 0x80 }, // Talked to Goron outside East Castle Town
            { 0x1C, 0x10 }, //Win Sumo round 1 against Bo
            { 0x39, 0x2 }, // Released first caught fish in Ordon Day 2
            { 0x10, 0x2 }, // Talked to Jaggle after climbing vines.
            { 0xB, 0x20 }, // Talked to Yeta in Snowpeak for the first time
            { 0x43, 0x8 }, // Senses unlocked
            { 0x46, 0x10 }, // Rode Epona back to Link's House
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] FaronTwilightEventFlags = new byte[,]
        {
            { 0x5, 0x7F }, // Midna Charge Unlocked, Finished Sewers, Met Zelda in swers, Midna cut prison chain, watched sewers intro CS, Escaped Cell in Sewers.
            { 0x6, 0x10 }, // Cleared Faron Twilight
            { 0xC, 0x18 }, // Midna Accompanies Wolf, Sword and shield removed from wolf's back.
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] EldinTwilightEventFlags = new byte[,]
        {
            { 0x7, 0x8 }, // Cleared Eldin Twilight
            { 0x6, 0x4 }, // Map Warping unlocked.
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] LanayruTwilightEventFlags = new byte[,]
        {
            { 0x8, 0x80 }, // Zora's Domain Thawed.
            { 0xC, 0x2 }, // Lanayru Twilight Story Flag.
            { 0xA, 0x10 }, // Defeated Kargarok Rider at Lake (allows player to howl for Kargorok.);
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] IntroEventFlags = new byte[,]
        {
            { 0x4, 0x4 }, // Talked to Uli Day 1.
            { 0x45, 0x10 }, // Saved Talo
            { 0x4A, 0x60 }, // Completed Ordon Day 1 and Finished Sword Training.
            { 0x16, 0x1 }, // Completed Ordon Day 2.
            { 0x15, 0x80 }, // Watched CS for Goats 2 Done.
        };

        public static readonly byte[,] CutsceneEventFlags = new byte[,]
        {
            { 0x1, 0x40 }, // Talked to Yeto First Time.
            { 0x3, 0x90 }, // Jaggle Calls out to Link, talked to Squirrel as Wolf in Ordon.
            { 0x6, 0xC0 }, // CS After beating Ordon Shadow, CS after entering Faron Twilight.
            { 0x7, 0x2 }, // First Time Talking to Gor Coron in Sumo Hall
            { 0x15, 0x1 }, // Talked to Agitha for the first time.
            { 0x20, 0x1 }, // Talked to Telma for the first time.
            { 0x5E, 0x10 }, // Midna text after beating Forest Temple.
            { 0x1D, 0x40 }, // Listened to Fyer at drained lake.
            { 0x22, 0x1 }, // Plumm initial CS watched.
            { 0x23, 0x10 }, // STAR initial CS watched.
            { 0x26, 0x2 }, // Talked to Yeto on Snowpeak.
            { 0x28, 0x40 }, // Used Ooccoo for the first time.
            { 0x37, 0x4 }, // Postman twilight text.
            { 0x38, 0x6 }, // Hena cabin first time CS, talked to Hena first time.
            { 0x3A, 0x1 }, // Talked to Ralis in Graveyard for the first time.
            { 0x40, 0x2 }, // Agreed to help Rusl after Snowpeak Ruins.
            { 0x42, 0x5 }, // Watched post-ToT Ooccoo CS. Watched Cutscene with Rusl in North Faron Woods.
            { 0x45, 0x8 }, // Allows postman letters to show up in inventory.
            { 0x4A, 0x10 }, // Saw Talo in cage CS.
            { 0x3E, 0x2 }, // City Ooccoo CS watched.
            { 0x59, 0x40 }, // Met Postman for the first time.
            { 0x5D, 0x40 }, // Midna text after Kargarok flight.
            { 0x25, 0x2 }, // Watched cutscene with Yeto on top of mountain
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly byte[,] OpenForestEventFlags = new byte[,]
        {
            { 0x6, 0x2 }, // Forest Temple Story Flag
            { 0xC, 0x40 }, // Talked to Farone after clearing Forest Temple
            { 0x5E, 0x10 }, // Midna text after Forest Temple completed
        };

        public static readonly byte[,] MDHEventFlags = new byte[,]
        {
            { 0xC, 0x1 }, // Midna's Desperate Hour started.
            { 0x1E, 0x8 }, // Midna's Deseperate Hour Completed.
        };

        public static readonly byte[,] SmallKeyEventFlags = new byte[,]
        {
            { 0x8, 0x50 }, // Zora Escort started and completed.
            { 0x4, 0x80 }, // Told Yeta about pumpkin.
            { 0x0, 0x3 }, // Yeto put pumpkin and cheese in soup.
            { 0x14, 0x60 }, // Snowpeak Ruins North and West doors unlocked.
            { 0x1, 0x20 }, // Told Yeta about cheese
        };

        public static readonly byte[,] OpenCastleEventFlags = new byte[,]
        {
            { 0x42, 0x8 }, // Remove Castle Barrier
        };

        public static readonly byte[,] OpenPalaceEventFlags = new byte[,]
        {
            { 0x2B, 0x8 }, // Mirror of Twilight Repaired.
        };

        public static readonly byte[,] OpenMinesEventFlags = new byte[,]
        {
            { 0x7, 0x6 }, // Talked to Gor Coron, Won Sumo against Gor Coron.
        };
        public static readonly byte[,] OpenArbitersEventFlags = new byte[,]
        {
            { 0xB, 0x40 }, // Escaped Burning Tent in Bulblin Camp.
        };
        public static readonly byte[,] OpenSnowpeakEventFlags = new byte[,]
        {
            { 0x61, 0x20 }, // Got the reekfish and smelled it (removes void in Snowpeak).
        };
        public static readonly byte[,] OpenCityEventFlags = new byte[,]
        {
            { 0x3B, 0x8 }, // Sky Cannon Repaired.
        };

        public static readonly byte[,] OverworldEREventFlags = new byte[,]
        {
            { 0x5, 0x7A }, // Finished Sewers, Midna text after entering Faron Twilight, Met Zelda in sewers, Midna cut prison chain, Watched Sewers intro CS, Escaped cell in sewers.
            { 0xC, 0x10 }, // Midna accompanies Wolf
        };

        public static readonly byte[,] RenadoLetterEventFlags = new byte[,]
        {
            { 0x20, 0x4 }, // ToT Story Progression Flag
            { 0xF, 0x80 }, // Renados Letter Check
        };

        public static readonly byte[,] WoodStatueEventFlags = new byte[,]
        {
            { 0x27, 0x10 }, // Showed Invoice to Doctor
            { 0x2F, 0x4 }, // Got Medicine Scent
            { 0x21, 0x2 }, // Talked to Louise after Medicine Scent
            { 0x22, 0x4 }, // Got Wooden Statue
        };

        public static readonly byte[,] CharmEventFlags = new byte[,]
        {
            { 0x23, 0x40 }, // Gave statue to Ilia
            { 0x2E, 0x8 }, // HV barrier removed
            { 0x22, 0x80 }, // Got Ilia's Charm
        };

        public static readonly byte[,] bridgeDonationEventFlags = new byte[,]
        {
            { 0x2E, 0x20 }, // Bridge Fundraising completed.
        };

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly Dictionary<int, byte[,]> EventFlags =
            new()
            {
                { 0, IntroEventFlags },
                { 1, FaronTwilightEventFlags },
                { 2, EldinTwilightEventFlags },
                { 3, LanayruTwilightEventFlags },
                { 4, CutsceneEventFlags },
                { 5, OpenForestEventFlags },
                { 6, MDHEventFlags },
                { 7, SmallKeyEventFlags },
                { 10, OpenCastleEventFlags },
                { 11, OpenPalaceEventFlags },
                { 12, OpenMinesEventFlags },
                { 14, OpenArbitersEventFlags },
                { 15, OpenSnowpeakEventFlags },
                { 17, OpenCityEventFlags },
                { 23, OverworldEREventFlags },
                { 24, RenadoLetterEventFlags },
                { 26, WoodStatueEventFlags },
                { 27, CharmEventFlags },
                { 30, bridgeDonationEventFlags },
            };
        private static readonly SharedSettings RandomizerSettings = Randomizer.SSettings;

        /// <summary>
        /// summary text.
        /// </summary>
        public static readonly bool[] FlagSettings = new bool[]
        {
            /* 0 */RandomizerSettings.skipPrologue,
            /* 1 */RandomizerSettings.faronTwilightCleared,
            /* 2 */RandomizerSettings.eldinTwilightCleared,
            /* 3 */RandomizerSettings.lanayruTwilightCleared,
            /* 4 */RandomizerSettings.skipMinorCutscenes,
            /* 5 */RandomizerSettings.faronWoodsLogic == FaronWoodsLogic.Open,
            /* 6 */RandomizerSettings.skipMdh,
            /* 7 */RandomizerSettings.smallKeySettings == SmallKeySettings.Keysy,
            /* 8 */RandomizerSettings.bigKeySettings == BigKeySettings.Keysy,
            /* 9 */RandomizerSettings.mapAndCompassSettings == MapAndCompassSettings.Start_With,
            /* 10 */RandomizerSettings.castleRequirements == CastleRequirements.Open,
            /* 11 */RandomizerSettings.palaceRequirements == PalaceRequirements.Open,
            /* 12 */RandomizerSettings.goronMinesEntrance != GoronMinesEntrance.Closed,
            /* 13 */RandomizerSettings.skipLakebedEntrance,
            /* 14 */RandomizerSettings.skipArbitersEntrance,
            /* 15 */RandomizerSettings.skipSnowpeakEntrance,
            /* 16 */RandomizerSettings.totEntrance == TotEntrance.None,
            /* 17 */RandomizerSettings.skipCityEntrance,
            /* 18 */RandomizerSettings.skipGroveEntrance,
            /* 19 */RandomizerSettings.goronMinesEntrance == GoronMinesEntrance.Open,
            /* 20 */RandomizerSettings.openDot,
            /* 21 */RandomizerSettings.openMap,
            /* 22 */RandomizerSettings.hcShortcut,
            /* 23 */RandomizerSettings.randomizeStartingPoint,
            /* 24 */RandomizerSettings.iliaQuest >= IliaQuest.Letter,
            /* 25 */RandomizerSettings.iliaQuest >= IliaQuest.Invoice,
            /* 26 */RandomizerSettings.iliaQuest >= IliaQuest.Statue,
            /* 27 */RandomizerSettings.iliaQuest >= IliaQuest.Charm,
            /* 28 */RandomizerSettings.shuffleDungeonEntrances != DungeonER.Off,
            /* 29 */RandomizerSettings.castleBKRequirements == CastleBKRequirements.None,
            /* 30 */RandomizerSettings.skipBridgeDonation,
        };
    }
}
