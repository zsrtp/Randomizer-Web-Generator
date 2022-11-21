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
        public static readonly string[][] MidnaBaseHairColors = new string[][]
        {
            // put the actual RGB values here. Note that there is no value for the default color as we don't want to make the change if the player doesn't want to change the color.
            // Colors are lightworld inactive, darkworld inactive, bothworld active
            new[] { "F5CFF300", "AD7F7F00", "1B002000" }, // Pink
            new[] { "E4654100", "A13E2200", "21000000" }, // Red
            new[] { "91830E00", "66500700", "0E0B0000" }, // Yellow
            new[] { "35795300", "254A2B00", "000E0500" }, // Green
            new[] { "0072FF00", "00468500", "00082800" }, // Blue
            new[] { "6F34FF00", "4E208500", "0D003400" }, // Purple
            new[] { "00000000", "00000000", "1A050000" }, // Brown
            new[] { "F0F1F100", "A9947E00", "090B0C00" }, // White
            new[] { "00000000", "00000000", "0B0B0B00" }, // Black
        };

        public static readonly string[][] MidnaTipsHairColors = new string[][]
        {
            // put the actual RGB values here. Note that there is no value for the default color as we don't want to make the change if the player doesn't want to change the color.
            // Colors are lightworld inactive, darkworld anyactive, lightworld active
            new[] { "DD00EB00", "DD00C300", "F64CFF00" }, // Pink
            new[] { "EB000000", "EB000000", "FF4F3A00" }, // Red
            new[] { "EBDE0000", "EBDE0000", "FFF8BF00" }, // Yellow
            new[] { "1FEB0000", "1FEB0000", "9AFF8100" }, // Green
            new[] { "0048EB00", "0048C300", "3A66FF00" }, // Blue
            new[] { "7B00EB00", "7B00C300", "943EFF00" }, // Purple
            new[] { "3F1D0B00", "3F1D0900", "59321E00" }, // Brown
            new[] { "EAEAEA00", "EAEAC200", "F3F3F300" }, // White
            new[] { "00000000", "00000000", "00000000", }, // Black
        };

        public static readonly string[][] MidnaGlowHairColors = new string[][]
        {
            // put the actual RGB values here. Note that there is no value for the default color as we don't want to make the change if the player doesn't want to change the color.
            // Colors are bothworld inactive, lightworld active, darkworld active
            new[] { "003C0002", "00580000", "00E30072", "00F20000", "00E3005F", "00F80000" }, // Pink
            new[] { "004F0002", "00010000", "00F0003A", "00250000", "00F00030", "008C0000" }, // Red
            new[] { "00240025", "00020000", "00CB00B7", "00000000", "00CB0099", "00780000" }, // Yellow
            new[] { "000A0029", "00100000", "000000B6", "006F0000", "00000098", "00B30000" }, // Green
            new[] { "0016001B", "005D0000", "00000060", "00FF0000", "00000050", "00FF0000" }, // Blue
            new[] { "00150008", "00790000", "00620000", "00FF0000", "00620000", "00FF0000" }, // Purple
            new[] { "003C0019", "000E0000", "003F001D", "000B0000", "003F0018", "007E0000" }, // Brown
            new[] { "00220024", "00240000", "00FF00FF", "00FF0000", "00FF00D5", "00FF0000" }, // White
            new[] { "00230023", "00230000", "00000000", "00000000", "00000000", "00780000" }, // Black
        };
    }
}
