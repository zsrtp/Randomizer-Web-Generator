namespace TPRandomizer.Assets.CLR0
{
    using System;
    using System.Collections.Generic;

    // WARNING: The values of the RecolorIds cannot be changed once they are set
    // because this would break backwards compatibility!
    public enum RecolorId : UInt16
    {
        None = 0xFFFF,

        HerosClothes = 0, // Cap and body
        ABtn = 1,
        BBtn = 19,
    }

    public enum RecolorType : byte
    {
        Unknown = 0xFF,
        Rgb = 0,
        RgbArray = 1,
    }

    public class Clr0Result
    {
        public UInt32 basicDataEntry { get; }
        public List<byte> complexBytes { get; }

        public Clr0Result(UInt32 basicDataEntry, List<byte> complexBytes)
        {
            this.basicDataEntry = basicDataEntry;
            this.complexBytes = complexBytes;
        }
    }

    public abstract class Clr0Entry
    {
        public RecolorId recolorId { get; protected set; } = RecolorId.None;
        public RecolorType recolorType { get; protected set; } = RecolorType.Unknown;

        public abstract Clr0Result getResult();
    }

    public class Rgb
    {
        public byte r { get; }
        public byte g { get; }
        public byte b { get; }

        public Rgb(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    public class RgbEntry : Clr0Entry
    {
        private Rgb rgb { get; }

        public RgbEntry(RecolorId recolorId, byte r, byte g, byte b)
        {
            this.recolorType = RecolorType.Rgb;
            this.recolorId = recolorId;
            this.rgb = new Rgb(r, g, b);
        }

        override public Clr0Result getResult()
        {
            UInt32 result = rgb.b;
            result |= (UInt32)(rgb.g << 8);
            result |= (UInt32)(rgb.r << 16);

            return new Clr0Result(result, null);
        }
    }

    public class RgbArrayEntry : Clr0Entry
    {
        private List<Rgb> rgbList { get; }

        public RgbArrayEntry(RecolorId recolorId, List<Rgb> rgbList)
        {
            this.recolorType = RecolorType.RgbArray;
            this.recolorId = recolorId;
            this.rgbList = rgbList;
        }

        override public Clr0Result getResult()
        {
            return null;
        }
    }
}
