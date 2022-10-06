namespace TPRandomizer.Assets.CLR0
{
    using System;
    using System.Collections.Generic;

    // WARNING: The values of the RecolorIds cannot be changed once they are set
    // because this would break backwards compatibility! Note this is a u16 and
    // not a u8.
    public enum RecolorId : UInt16
    {
        None = 0xFFFF,

        CMPR = 0x00,
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

    public class CMPRTextureEntry
    {
        public uint rgb;
        public string textureName;

        public CMPRTextureEntry(uint rgb, string textureName)
        {
            this.rgb = rgb;
            this.textureName = textureName;
        }
    }

    public struct CMPRTextureFileSettings
    {
        public Clr0Entry colorEntry; // The setting that contains the data for this override
        public string bmdFile;
        public string texture;

        public CMPRTextureFileSettings(Clr0Entry colorEntry, string bmdFile, string texture)
        {
            this.colorEntry = colorEntry;
            this.bmdFile = bmdFile;
            this.texture = texture;
        }
    };

    public struct BmdTextureAssociation
    {
        public byte recolorType;
        public string bmdFile;
        public List<string> textures;

        public BmdTextureAssociation(byte recolorType, string bmdFile, List<string> textures)
        {
            this.recolorType = recolorType;
            this.bmdFile = bmdFile;
            this.textures = textures;
        }
    };
}
