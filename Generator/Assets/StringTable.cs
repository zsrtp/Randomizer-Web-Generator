namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using TPRandomizer.Assets.CLR0;
    using TPRandomizer.FcSettings.Enums;
    using TPRandomizer.SSettings.Enums;
    using TPRandomizer.Util;

    public class StringTableEntryInfo
    {
        public ushort context = 1;
        public ushort infIndex = 0xFFFF;
        public string str = "";

        public StringTableEntryInfo(ushort context, ushort infIndex, string str)
        {
            this.context = context;
            this.infIndex = infIndex;
            this.str = str;
        }
    }

    public class StringTableResult
    {
        public List<byte> contextInfLookupTable = new();
        public List<byte> strOffsetConversionTable = new();
        public ushort numLookupEntries = 0;
        public List<byte> strTable = new();
        public ushort numStringTableEntries = 0;
    }

    public class StringTable
    {
        public static StringTableResult GenStringTableInfo(List<StringTableEntryInfo> inputList)
        {
            StringTableResult result = new();
            result.numLookupEntries = (ushort)inputList.Count;

            Dictionary<string, int> stringToStrTableOffset = new();

            foreach (StringTableEntryInfo entry in inputList)
            {
                string str = entry.str;
                int strOffset;

                if (!stringToStrTableOffset.TryGetValue(str, out strOffset))
                {
                    // Is a new string
                    strOffset = result.strTable.Count;
                    result.strTable.AddRange(Converter.MessageStringBytes(str));
                    result.strTable.Add(Converter.GcByte(0x0));
                    stringToStrTableOffset[str] = strOffset;

                    result.numStringTableEntries += 1;
                }

                // Add u32 check for context and INF index
                result.contextInfLookupTable.AddRange(Converter.GcBytes(entry.context));
                result.contextInfLookupTable.AddRange(Converter.GcBytes(entry.infIndex));

                // Add u16 conversion to strOffset
                result.strOffsetConversionTable.AddRange(Converter.GcBytes((ushort)strOffset));
            }

            List<StringTableEntryInfo> strEntries =
                new()
                {
                    new(3, 0x5de, "What abc?" + CustomMessages.shopOption),
                    new(
                        3,
                        0x5df,
                        $"Hints{CustomMessages.messageOption1}Change time of day{CustomMessages.messageOption2}"
                    ),
                };

            return result;
        }
    }
}
