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

        // Midna Hair (24-bit RGB vals)
        public int midnaHairBaseLightWorldInactive { get; }
        public int midnaHairBaseDarkWorldInactive { get; }
        public int midnaHairBaseAnyWorldActive { get; }
        public int midnaHairGlowAnyWorldInactive { get; }
        public int midnaHairGlowLightWorldActive { get; }
        public int midnaHairGlowDarkWorldActive { get; }
        public int midnaHairTipsLightWorldInactive { get; }
        public int midnaHairTipsDarkWorldAnyActive { get; }
        public int midnaHairTipsLightWorldActive { get; }

        // End Midna Hair
        public Clr0Entry midnaDomeRingColor { get; }

        public Clr0Entry linkHairColor { get; }

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

            int midnaHairBaseColor = processor.NextInt(4);

            int[] baseAndGlowArr = ColorArrays.MidnaHairBaseAndGlowColors[midnaHairBaseColor];
            midnaHairBaseLightWorldInactive = baseAndGlowArr[0];
            midnaHairBaseDarkWorldInactive = baseAndGlowArr[1];
            midnaHairBaseAnyWorldActive = baseAndGlowArr[2];
            midnaHairGlowAnyWorldInactive = baseAndGlowArr[3];
            midnaHairGlowLightWorldActive = baseAndGlowArr[4];
            midnaHairGlowDarkWorldActive = baseAndGlowArr[5];

            int midnaHairTipsColor = processor.NextInt(4);

            int[] tipsArr = ColorArrays.MidnaHairTipsColors[midnaHairTipsColor];
            midnaHairTipsLightWorldInactive = tipsArr[0];
            midnaHairTipsDarkWorldAnyActive = tipsArr[1];
            midnaHairTipsLightWorldActive = tipsArr[2];

            midnaDomeRingColor = processor.NextClr0Entry(RecolorId.None);
            linkHairColor = processor.NextClr0Entry(RecolorId.CMPR);
        }

        public static FileCreationSettings FromString(string fcSettingsString)
        {
            string bits = SettingsEncoder.DecodeToBitString(fcSettingsString);
            return new FileCreationSettings(bits);
        }
    }
}
