namespace TPRandomizer
{
    using TPRandomizer.Util;
    using TPRandomizer.FcSettings.Enums;

    public class FileCreationSettings
    {
        public GameRegion gameRegion { get; }
        public byte seedNumber { get; }
        public RandomizeBgm randomizeBgm { get; }
        public bool randomizeFanfares { get; }
        public bool disableEnemyBgm { get; }

        // public int tunicColor { get; }
        public int lanternGlowColor { get; }

        // public int midnaHairColor { get; }
        public int heartColor { get; }
        public int aBtnColor { get; }
        public int bBtnColor { get; }
        public int xBtnColor { get; }
        public int yBtnColor { get; }
        public int zBtnColor { get; }

        private FileCreationSettings(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            gameRegion = (GameRegion)processor.NextInt(3);
            seedNumber = (byte)processor.NextInt(4);

            randomizeBgm = (RandomizeBgm)processor.NextInt(2);
            randomizeFanfares = processor.NextBool();
            disableEnemyBgm = processor.NextBool();

            // tunicColor = processor.NextInt(4);
            lanternGlowColor = processor.NextInt(4);
            // midnaHairColor = processor.NextInt(1);
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
    }
}
