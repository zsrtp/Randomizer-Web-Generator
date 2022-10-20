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

        public Clr0Entry hTunicHatColor { get; }
        public Clr0Entry hTunicBodyColor { get; }
        public Clr0Entry hTunicSkirtColor { get; }
        public Clr0Entry zTunicHatColor { get; }

        public Clr0Entry zTunicHelmetColor { get; }
        public Clr0Entry zTunicBodyColor { get; }
        public Clr0Entry zTunicScalesColor { get; }
        public Clr0Entry zTunicBootsColor { get; }

        public Clr0Entry lanternGlowColor { get; }

        // public int midnaHairColor { get; }
        public Clr0Entry heartColor { get; }

        public Clr0Entry aBtnColor { get; }

        public Clr0Entry bBtnColor { get; }
        public Clr0Entry xBtnColor { get; }
        public Clr0Entry yBtnColor { get; }
        public Clr0Entry zBtnColor { get; }

        private FileCreationSettings(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            gameRegion = (GameRegion)processor.NextInt(2);
            includeSpoilerLog = processor.NextBool();

            randomizeBgm = (RandomizeBgm)processor.NextInt(2);
            randomizeFanfares = processor.NextBool();
            disableEnemyBgm = processor.NextBool();

            hTunicHatColor = processor.NextClr0Entry(RecolorId.CMPR);
            hTunicBodyColor = processor.NextClr0Entry(RecolorId.CMPR);
            hTunicSkirtColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicHatColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicHelmetColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicBodyColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicScalesColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicBootsColor = processor.NextClr0Entry(RecolorId.CMPR);
            lanternGlowColor = processor.NextClr0Entry(RecolorId.None);
            // midnaHairColor = processor.NextInt(1);
            heartColor = processor.NextClr0Entry(RecolorId.None);
            aBtnColor = processor.NextClr0Entry(RecolorId.None);
            bBtnColor = processor.NextClr0Entry(RecolorId.None);
            xBtnColor = processor.NextClr0Entry(RecolorId.None);
            yBtnColor = processor.NextClr0Entry(RecolorId.None);
            zBtnColor = processor.NextClr0Entry(RecolorId.None);
        }

        public static FileCreationSettings FromString(string fcSettingsString)
        {
            string bits = SettingsEncoder.DecodeToBitString(fcSettingsString);
            return new FileCreationSettings(bits);
        }
    }
}
