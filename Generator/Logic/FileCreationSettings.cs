namespace TPRandomizer
{
    using TPRandomizer.Util;
    using TPRandomizer.FcSettings.Enums;
    using TPRandomizer.Assets.CLR0;

    public class FileCreationSettings
    {
        public GameRegion gameRegion { get; }
        public bool includeSpoilerLog { get; }
        public RandomizeBgm randomizeBgm { get; }
        public bool randomizeFanfares { get; }
        public bool disableEnemyBgm { get; }

        public Clr0Entry tunicColor { get; }
        public int lanternGlowColor { get; }

        // public int midnaHairColor { get; }
        public int heartColor { get; }

        public Clr0Entry aBtnColor { get; }

        // public int bBtnColor { get; }
        public Clr0Entry bBtnColor { get; }
        public int xBtnColor { get; }
        public int yBtnColor { get; }
        public int zBtnColor { get; }

        private FileCreationSettings(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            gameRegion = (GameRegion)processor.NextInt(2);
            includeSpoilerLog = processor.NextBool();

            randomizeBgm = (RandomizeBgm)processor.NextInt(2);
            randomizeFanfares = processor.NextBool();
            disableEnemyBgm = processor.NextBool();

            tunicColor = processor.NextClr0Entry(Assets.CLR0.RecolorId.HerosClothes);
            lanternGlowColor = processor.NextInt(4);
            // midnaHairColor = processor.NextInt(1);
            heartColor = processor.NextInt(4);
            aBtnColor = processor.NextClr0Entry(Assets.CLR0.RecolorId.ABtn);
            // bBtnColor = processor.NextInt(3);
            bBtnColor = new RgbEntry(Assets.CLR0.RecolorId.BBtn, 0x88, 0x88, 0);
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
