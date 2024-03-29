namespace TPRandomizer.Hints
{
    using System.Collections.Generic;

    public enum TextColor : byte
    {
        White = 0,
        Red = 1,
        Green = 2,
        LightBlue = 3,
        Yellow = 4,
        Unk_LightestBlue = 5,
        Purple = 6,

        // WhiteAgain = 7, // #ffffff, same as White
        Orange = 8,
        CustomDarkGreen = 9,
        CustomBlue = 0xA,
        CustomSilver = 0xB,
    };

    public class HintText
    {
        public static readonly Dictionary<TextColor, string> GcColors =
            new()
            {
                { TextColor.White, "#ffffff" },
                { TextColor.Red, "#f07878" },
                { TextColor.Green, "#aadc8c" },
                { TextColor.LightBlue, "#a0b4dc" },
                { TextColor.Yellow, "#dcdc82" },
                { TextColor.Unk_LightestBlue, "#b4c8e6" },
                { TextColor.Purple, "#c8a0dc" },
                { TextColor.Orange, "#dcaa78" },
                { TextColor.CustomDarkGreen, "#4bbe4b" },
                { TextColor.CustomBlue, "#4b96d7" },
                { TextColor.CustomSilver, "#bfbfbf" },
            };

        public string text { get; set; }
        public List<string> colors { get; set; } = new();
        public string checkName { get; set; }
        public string checkContents { get; set; }
    }
}
