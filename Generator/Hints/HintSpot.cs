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
        Midna_Talk_To = 1,
        Agithas_Castle_Sign = 2,
        Ordon_Sign = 3,
        Sacred_Grove_Sign = 4,
        Faron_Field_Sign = 5,
        Faron_Woods_Sign = 6,
        Kakariko_Gorge_Sign = 7,
        Kakariko_Village_Sign = 8,
        Kakariko_Graveyard_Sign = 9,
        Eldin_Field_Sign = 10,
        North_Eldin_Sign = 11,
        Death_Mountain_Sign = 12,
        Hidden_Village_Sign = 13,
        Lanayru_Field_Sign = 14,
        Beside_Castle_Town_Sign = 15,
        South_of_Castle_Town_Sign = 16,
        Castle_Town_Sign = 17,
        Great_Bridge_of_Hylia_Sign = 18,
        Lake_Hylia_Sign = 19,
        Lake_Lantern_Cave_Sign = 20,
        Lanayru_Spring_Sign = 21,
        Zoras_Domain_Sign = 22,
        Upper_Zoras_River_Sign = 23,
        Gerudo_Desert_Sign = 24,
        Bulblin_Camp_Sign = 25,
        Snowpeak_Sign = 26,
        Cave_of_Ordeals_Sign = 27,
        Forest_Temple_Sign = 28,
        Goron_Mines_Sign = 29,
        Lakebed_Temple_Sign = 30,
        Arbiters_Grounds_Sign = 31,
        Snowpeak_Ruins_Sign = 32,
        Temple_of_Time_Sign = 33,
        City_in_the_Sky_Sign = 34,
        Palace_of_Twilight_Sign = 35,
        Hyrule_Castle_Sign = 36,
        Temple_of_Time_Beyond_Point_Sign = 37,
        Jovani_House_Sign = 38,
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
