namespace TPRandomizer
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Util;

    public class PSettings
    {
        public string gameVersion { get; }
        public byte seedNumber { get; }
        public List<RecolorDefinition> recolorDefinitions { get; }

        private PSettings(string bits)
        {
            BitsProcessor processor = new BitsProcessor(bits);

            gameVersion = processor.NextString(PSettingsOptions.gameVersion);
            seedNumber = processor.NextNibble();
            recolorDefinitions = processor.NextRecolorDefinitions();
        }

        public static PSettings FromString(string pSettingsString)
        {
            if (pSettingsString == null || !pSettingsString.StartsWith("0p"))
            {
                throw new Exception("Unable to decode pSettingsString.");
            }

            string bits = SettingsEncoder.DecodeToBitString(pSettingsString.Substring(2));
            return new PSettings(bits);
        }

        private class PSettingsOptions
        {
            public static readonly string[] gameVersion = new string[] { "GZ2E", "GZ2P", "GZ2J" };
        }
    }
}
