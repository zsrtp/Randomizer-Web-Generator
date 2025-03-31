namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Microsoft.CodeAnalysis.Operations;
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

            List<List<byte>> branchData =
                new()
                {
                    new() { 2, 3, 0, 0x23, 0, 0, 1, 0x2e }
                };

            return result;
        }
    }

    public class BranchTableEntryInfo
    {
        public ushort flwIndex = 0xFFFF;
        public ushort context = 1;
        public List<byte> sevenBytes = new();
        public List<ushort> resultMap = new();

        public BranchTableEntryInfo(
            ushort flwIndex,
            ushort context,
            List<byte> sevenBytes,
            List<ushort> resultMap
        )
        {
            this.flwIndex = flwIndex;
            this.context = context;
            this.sevenBytes = sevenBytes;
            this.resultMap = resultMap;
        }
    }

    public class BranchTable
    {
        public ushort numLookupEntries = 0;
        public List<byte> lookupTable = new();
        public List<byte> branchNodeData = new();
        public List<ushort> resultMapData = new();

        private ushort numBranchNodes = 0;
        private ushort numResultMapEntries = 0;

        private BranchTable() { }

        public static BranchTable GenBranchTable(List<BranchTableEntryInfo> entryList)
        {
            BranchTable inst = new BranchTable();
            if (ListUtils.isEmpty(entryList))
                return inst;

            List<BranchTableEntryInfo> a =
                new()
                {
                    new(0x1a3, 3, null, new() { 0x123, 0x456, 0xFFFF }),
                    new(0x1a3, 3, null, new() { 0x123, 0x456, 0xFFFF }),
                    new(0x1a3, 3, null, new() { 0x123, 0x456, 0xFFFF }),
                };

            foreach (BranchTableEntryInfo entry in entryList)
            {
                ushort branchNodeIndex = 0xFFFF;
                ushort resultMapIndex = 0xFFFF;

                if (entry.sevenBytes != null)
                {
                    if (entry.sevenBytes.Count != 7)
                        throw new Exception(
                            $"entry.sevenBytes.Count, expected 7, but was '{entry.sevenBytes.Count}'."
                        );

                    inst.branchNodeData.Add(2);
                    inst.branchNodeData.AddRange(entry.sevenBytes);
                    branchNodeIndex = inst.numBranchNodes;
                    inst.numBranchNodes += 1;
                }

                if (entry.resultMap != null)
                {
                    if (entry.resultMap.Count == 0)
                        throw new Exception("entry.resultMap.Count, expected non-empty List");

                    foreach (ushort remap in entry.resultMap)
                    {
                        inst.resultMapData.Add(remap);
                    }

                    resultMapIndex = inst.numResultMapEntries;
                    inst.numResultMapEntries += (ushort)entry.resultMap.Count;
                }

                // First need to add the branch overwrite in order to determine an index.
                // Then need to add the resultMapEntries in order to determine an index.

                if (branchNodeIndex != 0xFFFF || resultMapIndex != 0xFFFF)
                {
                    List<byte> lookupEntry = new();
                    lookupEntry.AddRange(Converter.GcBytes(entry.flwIndex)); // 0x00
                    lookupEntry.AddRange(Converter.GcBytes(entry.context)); // 0x02
                    lookupEntry.AddRange(Converter.GcBytes(branchNodeIndex)); // 0x04
                    lookupEntry.AddRange(Converter.GcBytes(resultMapIndex)); // 0x06
                    inst.lookupTable.AddRange(lookupEntry);

                    inst.numLookupEntries += 1;
                }
            }

            return inst;
        }
    }
}
