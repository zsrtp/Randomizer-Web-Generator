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
        public byte archiveIndex;

        public CMPRTextureFileSettings(
            Clr0Entry colorEntry,
            string bmdFile,
            string texture,
            byte archiveIndex
        )
        {
            this.colorEntry = colorEntry;
            this.bmdFile = bmdFile;
            this.texture = texture;
            this.archiveIndex = archiveIndex;
        }
    };

    public struct BmdTextureAssociation
    {
        public byte recolorType;
        public string bmdFile;
        public List<string> textures;
        public byte archiveIndex;

        public BmdTextureAssociation(
            byte recolorType,
            string bmdFile,
            List<string> textures,
            byte archiveIndex
        )
        {
            this.recolorType = recolorType;
            this.bmdFile = bmdFile;
            this.textures = textures;
            this.archiveIndex = archiveIndex;
        }
    };

    public enum ArchiveIndex : byte
    {
        Link = 0,
        ZoraArmor = 1,
        ZoraArmorField = 2,
    }

    public class ColorArrays
    {
        public static readonly int[][] MidnaHairBaseAndGlowColors = new int[][]
        {
            // Array color order is:
            // BaseLightWorldInactive, BaseDarkWorldInactive, BaseAnyWorldActive,
            // GlowAnyWorldInactive, GlowLightWorldActive, GlowDarkWorldActive
            new[] { 0xFFDC00, 0xB48700, 0x500000, 0x500000, 0xFF7800, 0xFF6478 }, // Default
            new[] { 0xF5CFF3, 0xAD7F7F, 0x1B0020, 0x3C0258, 0xE372F2, 0xE35FF8 }, // Pink
            new[] { 0xE46541, 0xA13E22, 0x210000, 0x4F0201, 0xF03A25, 0xF0308C }, // Red
            new[] { 0x91830E, 0x665007, 0x0E0B00, 0x242502, 0xCBB700, 0xCB9978 }, // Yellow
            new[] { 0x357953, 0x254A2B, 0x000E05, 0x0A2910, 0x00B66F, 0x0098B3 }, // Green
            new[] { 0x0072FF, 0x004685, 0x000828, 0x161B5D, 0x0060FF, 0x0050FF }, // Blue
            new[] { 0x6F34FF, 0x4E2085, 0x0D0034, 0x150879, 0x6200FF, 0x6200FF }, // Purple
            new[] { 0x000000, 0x000000, 0x1A0500, 0x3C190E, 0x3F1D0B, 0x3F187E }, // Brown
            new[] { 0xF0F1F1, 0xA9947E, 0x090B0C, 0x222424, 0xFFFFFF, 0xFFD5FF }, // White
            new[] { 0x000000, 0x000000, 0x0B0B0B, 0x232323, 0x000000, 0x000078 }, // Black
        };

        public static readonly int[][] MidnaHairTipsColors = new int[][]
        {
            // Array color order is:
            // TipsLightWorldInactive, TipsDarkWorldAnyActive, TipsLightWorldActive
            new[] { 0x00C3EB, 0xC3C300, 0xAAFFC3 }, // Default
            new[] { 0xDD00EB, 0xDD00C3, 0xF64CFF }, // Pink
            new[] { 0xEB0000, 0xEB0000, 0xFF4F3A }, // Red
            new[] { 0xEBDE00, 0xEBDE00, 0xFFF8BF }, // Yellow
            new[] { 0x1FEB00, 0x1FEB00, 0x9AFF81 }, // Green
            new[] { 0x0048EB, 0x0048C3, 0x3A66FF }, // Blue
            new[] { 0x7B00EB, 0x7B00C3, 0x943EFF }, // Purple
            new[] { 0x3F1D0B, 0x3F1D09, 0x59321E }, // Brown
            new[] { 0xEAEAEA, 0xEAEAC2, 0xF3F3F3 }, // White
            new[] { 0x000000, 0x000000, 0x000000, }, // Black
        };
    }
}
