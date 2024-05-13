namespace TPRandomizer
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Util;
    using TPRandomizer.Assets;
    using System.Runtime.Serialization;
    using System.IO;
    using System.Text.RegularExpressions;

    // Handles special formatting for "hints" section of the spoilerLog so that
    // it does not take up a large number of lines.
    public partial class SpoilerJsonWriter : JsonTextWriter
    {
        [GeneratedRegex(@"^hints\.\w+")]
        private static partial Regex Reg();

        private Stack<string> stack = new();
        private bool isActive = false;

        public SpoilerJsonWriter(TextWriter textWriter) : base(textWriter)
        {
            this.Formatting = Formatting.Indented;
        }

        public override void WriteStartArray()
        {
            base.WriteStartArray();

            stack.Push(Path);
            isActive = Reg().IsMatch(stack.Peek());
        }

        public override void WriteEndArray()
        {
            stack.Pop();

            if (stack.Count > 0)
                isActive = Reg().IsMatch(stack.Peek());
            else
                isActive = false;

            base.WriteEndArray();
        }

        public override void WriteStartObject()
        {
            base.WriteStartObject();

            if (isActive)
                this.Formatting = Formatting.None;
        }

        public override void WriteEndObject()
        {
            base.WriteEndObject();

            this.Formatting = Formatting.Indented;
        }

        protected override void WriteValueDelimiter()
        {
            base.WriteValueDelimiter();

            // Adds space after the comma
            if (isActive)
                WriteWhitespace(" ");
        }

        public override void WritePropertyName(string name, bool escape)
        {
            base.WritePropertyName(name, escape);

            // Adds space after the colon
            if (isActive)
                WriteWhitespace(" ");
        }
    }

    public class SpoilerJsonWriterUtils
    {
        public static string Serialize(object value)
        {
            using (TextWriter sw = new StringWriter())
            using (JsonWriter writer = new SpoilerJsonWriter(sw))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(writer, value);
                return sw.ToString();
            }
        }
    }
}
