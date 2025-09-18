namespace TPRandomizer
{
    using System;
    using TPRandomizer.Assets.CLR0;
    using TPRandomizer.FcSettings.Enums;
    using TPRandomizer.Util;

    public class FileCreationSettings
    {
        public GameRegion gameRegion { get; }
        public EurLanguageTag eurLangTag { get; }
        public bool patchFileOnly { get; }
        public bool includeSpoilerLog { get; }
        public RandomizeBgm randomizeBgm { get; }

        public bool randomizeSfx { get; }
        public bool randomizeFanfares { get; }
        public bool disableEnemyBgm { get; }
        public bool invertCameraAxis { get; }

        public Clr0Entry hTunicHatColor { get; }
        public Clr0Entry hTunicBodyColor { get; }
        public Clr0Entry hTunicSkirtColor { get; }
        public Clr0Entry zTunicHatColor { get; }

        public Clr0Entry zTunicHelmetColor { get; }
        public Clr0Entry zTunicBodyColor { get; }
        public Clr0Entry zTunicScalesColor { get; }
        public Clr0Entry zTunicBootsColor { get; }

        public Clr0Entry lanternGlowColor { get; }
        public Clr0Entry msBladeColor { get; }
        public Clr0Entry msHandleColor { get; }
        public Clr0Entry boomerangColor { get; }
        public Clr0Entry ironsColor { get; }
        public Clr0Entry spinnerColor { get; }
        public Clr0Entry woodSwordColor { get; }
        public Clr0Entry eponaColor { get; }

        public Clr0Entry wolfColor { get; }

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

            gameRegion = (GameRegion)processor.NextInt(3);
            eurLangTag = (EurLanguageTag)processor.NextInt(3);
            patchFileOnly = processor.NextBool();
            includeSpoilerLog = processor.NextBool();

            randomizeBgm = (RandomizeBgm)processor.NextInt(2);
            randomizeFanfares = processor.NextBool();
            randomizeSfx = processor.NextBool();
            disableEnemyBgm = processor.NextBool();
            invertCameraAxis = processor.NextBool();

            hTunicHatColor = processor.NextClr0Entry(RecolorId.CMPR);
            hTunicBodyColor = processor.NextClr0Entry(RecolorId.CMPR);
            hTunicSkirtColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicHatColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicHelmetColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicBodyColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicScalesColor = processor.NextClr0Entry(RecolorId.CMPR);
            zTunicBootsColor = processor.NextClr0Entry(RecolorId.CMPR);
            msBladeColor = processor.NextClr0Entry(RecolorId.CMPR);
            msHandleColor = processor.NextClr0Entry(RecolorId.CMPR);
            boomerangColor = processor.NextClr0Entry(RecolorId.CMPR);
            ironsColor = processor.NextClr0Entry(RecolorId.CMPR);
            spinnerColor = processor.NextClr0Entry(RecolorId.CMPR);
            woodSwordColor = processor.NextClr0Entry(RecolorId.CMPR);
            eponaColor = processor.NextClr0Entry(RecolorId.CMPR);
            wolfColor = processor.NextClr0Entry(RecolorId.CMPR);
            lanternGlowColor = processor.NextClr0Entry(RecolorId.None);
            // midnaHairColor = processor.NextInt(1);
            heartColor = processor.NextClr0Entry(RecolorId.None);
            aBtnColor = processor.NextClr0Entry(RecolorId.None);
            bBtnColor = processor.NextClr0Entry(RecolorId.None);
            xBtnColor = processor.NextClr0Entry(RecolorId.None);
            yBtnColor = processor.NextClr0Entry(RecolorId.None);
            zBtnColor = processor.NextClr0Entry(RecolorId.None);

            bool isCustomMidnaHairBaseColor = processor.NextBool();
            if (isCustomMidnaHairBaseColor)
            {
                midnaHairBaseLightWorldInactive = processor.NextInt(24);
                midnaHairBaseDarkWorldInactive = processor.NextInt(24);
                midnaHairBaseAnyWorldActive = processor.NextInt(24);
                midnaHairGlowAnyWorldInactive = processor.NextInt(24);
                midnaHairGlowLightWorldActive = processor.NextInt(24);
                midnaHairGlowDarkWorldActive = processor.NextInt(24);
            }
            else
            {
                int midnaHairBaseColor = processor.NextInt(4);

                int[] baseAndGlowArr = ColorArrays.MidnaHairBaseAndGlowColors[midnaHairBaseColor];
                midnaHairBaseLightWorldInactive = baseAndGlowArr[0];
                midnaHairBaseDarkWorldInactive = baseAndGlowArr[1];
                midnaHairBaseAnyWorldActive = baseAndGlowArr[2];
                midnaHairGlowAnyWorldInactive = baseAndGlowArr[3];
                midnaHairGlowLightWorldActive = baseAndGlowArr[4];
                midnaHairGlowDarkWorldActive = baseAndGlowArr[5];
            }

            bool isCustomMidnaHairTipsColor = processor.NextBool();
            if (isCustomMidnaHairTipsColor)
            {
                midnaHairTipsLightWorldInactive = processor.NextInt(24);
                midnaHairTipsDarkWorldAnyActive = processor.NextInt(24);
                midnaHairTipsLightWorldActive = processor.NextInt(24);
            }
            else
            {
                int midnaHairTipsColor = processor.NextInt(4);

                int[] tipsArr = ColorArrays.MidnaHairTipsColors[midnaHairTipsColor];
                midnaHairTipsLightWorldInactive = tipsArr[0];
                midnaHairTipsDarkWorldAnyActive = tipsArr[1];
                midnaHairTipsLightWorldActive = tipsArr[2];
            }

            midnaDomeRingColor = processor.NextClr0Entry(RecolorId.None);
            linkHairColor = processor.NextClr0Entry(RecolorId.CMPR);
        }

        public static FileCreationSettings FromString(string fcSettingsString)
        {
            string bits = SettingsEncoder.DecodeToBitString(fcSettingsString);
            return new FileCreationSettings(bits);
        }

        public string GetLanguageTagString(GameRegion? exactGameRegion = null)
        {
            if (exactGameRegion == null)
                exactGameRegion = gameRegion;

            switch (exactGameRegion)
            {
                case GameRegion.All:
                case GameRegion.GC_USA:
                case GameRegion.WII_10_USA:
                case GameRegion.WII_12_USA:
                    return "en";
                case GameRegion.GC_JAP:
                case GameRegion.WII_10_JP:
                    return "ja";
                case GameRegion.GC_EUR:
                case GameRegion.WII_10_EU:
                    return ResolveEurLang(eurLangTag);
                default:
                    throw new Exception($"Unrecognized GameRegion '{exactGameRegion}'.");
            }
        }

        private static string ResolveEurLang(EurLanguageTag langTag)
        {
            switch (langTag)
            {
                case EurLanguageTag.English:
                    return "en-GB";
                case EurLanguageTag.French:
                    return "fr";
                case EurLanguageTag.German:
                    return "de";
                case EurLanguageTag.Italian:
                    return "it";
                case EurLanguageTag.Spanish:
                    return "es";
                default:
                    throw new Exception($"Unrecognized EurLanguageTag '{langTag}'.");
            }
        }
    }
}
