namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class FileCreationSettings
    {
        public string gameRegion { get; }
        public byte seedNumber { get; }
        public Options.RandomizeBgm randomizeBgm { get; }
        public bool randomizeFanfares { get; }
        public bool disableEnemyBgm { get; }
        public int tunicColor { get; }
        public int lanternColor { get; }
        public int midnaHairColor { get; }
        public int heartColor { get; }
        public int aBtnColor { get; }
        public int bBtnColor { get; }
        public int xBtnColor { get; }
        public int yBtnColor { get; }
        public int zBtnColor { get; }

        private FileCreationSettings(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            gameRegion = processor.NextString(Options.gameRegion, 3);
            seedNumber = processor.NextNibble();

            randomizeBgm = (Options.RandomizeBgm)processor.NextInt(2);
            randomizeFanfares = processor.NextBool();
            disableEnemyBgm = processor.NextBool();

            tunicColor = processor.NextInt(4);
            lanternColor = processor.NextInt(4);
            midnaHairColor = processor.NextInt(1);
            heartColor = processor.NextInt(4);
            aBtnColor = processor.NextInt(4);
            bBtnColor = processor.NextInt(3);
            xBtnColor = processor.NextInt(4);
            yBtnColor = processor.NextInt(4);
            zBtnColor = processor.NextInt(4);
        }

        public static FileCreationSettings FromString(string fcSettingsString)
        {
            string bits = SettingsEncoder.DecodeToBitString(fcSettingsString);
            return new FileCreationSettings(bits);
        }

        public class Options
        {
            public static readonly string[] gameRegion = new string[] { "NTSC", "PAL", "JAP" };

            public enum RandomizeBgm
            {
                Vanilla = 0,
                Overworld = 1,
                Dungeon = 2,
                All = 3,
            }
        }
    }
}
