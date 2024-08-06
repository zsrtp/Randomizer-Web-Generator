namespace TPRandomizer.Hints
{
    using System.Collections.Generic;

    public class HintSpotLocationUtils
    {
        public static readonly byte NumBitsToEncode = 6;
    }

    public enum SpotId : byte
    {
        Invalid = 0,
        Agithas_Castle_Sign = 1,
        Ordon_Sign = 2,
        Sacred_Grove_Sign = 3,
        Faron_Field_Sign = 4,
        Faron_Woods_Sign = 5,
        Kakariko_Gorge_Sign = 6,
        Kakariko_Village_Sign = 7,
        Kakariko_Graveyard_Sign = 8,
        Eldin_Field_Sign = 9,
        North_Eldin_Sign = 10,
        Death_Mountain_Sign = 11,
        Hidden_Village_Sign = 12,
        Lanayru_Field_Sign = 13,
        Beside_Castle_Town_Sign = 14,
        South_of_Castle_Town_Sign = 15,
        Castle_Town_Sign = 16,
        Great_Bridge_of_Hylia_Sign = 17,
        Lake_Hylia_Sign = 18,
        Lake_Lantern_Cave_Sign = 19,
        Lanayru_Spring_Sign = 20,
        Zoras_Domain_Sign = 21,
        Upper_Zoras_River_Sign = 22,
        Gerudo_Desert_Sign = 23,
        Bulblin_Camp_Sign = 24,
        Snowpeak_Mountain_Sign = 25,
        Cave_of_Ordeals_Sign = 26,
        Forest_Temple_Sign = 27,
        Goron_Mines_Sign = 28,
        Lakebed_Temple_Sign = 29,
        Arbiters_Grounds_Sign = 30,
        Snowpeak_Ruins_Sign = 31,
        Temple_of_Time_Sign = 32,
        City_in_the_Sky_Sign = 33,
        Palace_of_Twilight_Sign = 34,
        Hyrule_Castle_Sign = 35,
        Temple_of_Time_Beyond_Point_Sign = 36,
        Jovani_House_Sign = 37,
    }

    public class HintSpot
    {
        public SpotId location { get; }
        public List<Hint> hints { get; } = new();

        public HintSpot(SpotId location)
        {
            this.location = location;
        }
    }
}
