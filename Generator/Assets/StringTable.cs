using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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

    public enum BmgNumber
    {
        // Available everywhere: Midna, pause menu texts, Ooccoo Jr., caught
        // something while fishing, Great Fairy at springs, learning scent,
        // Epona, you got X item texts, some cutscene texts, credits, etc.
        zel_00 = 0,
        zel_01 = 1, // Ordon
        zel_02 = 2, // KV (interiors), KGY (interiors)
        zel_03 = 3, // Death Mountain (interiors)
        zel_04 = 4, // CT (interiors), sewers, HC in credits
        zel_05 = 5, // All dungeons, (mini)bosses, grottos, caves, LA cutscene
        zel_06 = 6, // FW (interiors), SP, SG, BC, GD, Mirror Chamber, HV (interiors), Hidden Skill
        zel_07 = 7, // ZD, Fishing Hole, Hena's house
        zel_08 = 8, // HF, Outside CT, LH, UZR, Zora's River, KB2, Title Screen

        // zel_99 not included since it has no content and none of our listed
        // stages map to it
    }

    class BmgNumUtils
    {
        private static Dictionary<StageIDs, BmgNumber> stageToBmg =
            new()
            {
                { StageIDs.Lakebed_Temple, BmgNumber.zel_05 },
                { StageIDs.Morpheel, BmgNumber.zel_05 },
                { StageIDs.Deku_Toad, BmgNumber.zel_05 },
                { StageIDs.Goron_Mines, BmgNumber.zel_05 },
                { StageIDs.Fyrus, BmgNumber.zel_05 },
                { StageIDs.Dangoro, BmgNumber.zel_05 },
                { StageIDs.Forest_Temple, BmgNumber.zel_05 },
                { StageIDs.Diababa, BmgNumber.zel_05 },
                { StageIDs.Ook, BmgNumber.zel_05 },
                { StageIDs.Temple_of_Time, BmgNumber.zel_05 },
                { StageIDs.Armogohma, BmgNumber.zel_05 },
                { StageIDs.Darknut, BmgNumber.zel_05 },
                { StageIDs.City_in_the_Sky, BmgNumber.zel_05 },
                { StageIDs.Argorok, BmgNumber.zel_05 },
                { StageIDs.Aeralfos, BmgNumber.zel_05 },
                { StageIDs.Palace_of_Twilight, BmgNumber.zel_05 },
                { StageIDs.Zant_Main_Room, BmgNumber.zel_05 },
                { StageIDs.Phantom_Zant_1, BmgNumber.zel_05 },
                { StageIDs.Phantom_Zant_2, BmgNumber.zel_05 },
                { StageIDs.Zant_Fight, BmgNumber.zel_05 },
                { StageIDs.Hyrule_Castle, BmgNumber.zel_05 },
                { StageIDs.Ganondorf_Castle, BmgNumber.zel_05 },
                { StageIDs.Ganondorf_Field, BmgNumber.zel_05 },
                { StageIDs.Ganondorf_Defeated, BmgNumber.zel_05 },
                { StageIDs.Arbiters_Grounds, BmgNumber.zel_05 },
                { StageIDs.Stallord, BmgNumber.zel_05 },
                { StageIDs.Death_Sword, BmgNumber.zel_05 },
                { StageIDs.Snowpeak_Ruins, BmgNumber.zel_05 },
                { StageIDs.Blizzeta, BmgNumber.zel_05 },
                { StageIDs.Darkhammer, BmgNumber.zel_05 },
                { StageIDs.Lanayru_Ice_Puzzle_Cave, BmgNumber.zel_05 },
                { StageIDs.Cave_of_Ordeals, BmgNumber.zel_05 },
                { StageIDs.Eldin_Long_Cave, BmgNumber.zel_05 },
                { StageIDs.Lake_Hylia_Long_Cave, BmgNumber.zel_05 },
                { StageIDs.Eldin_Goron_Stockcave, BmgNumber.zel_05 },
                { StageIDs.Grotto_1, BmgNumber.zel_05 },
                { StageIDs.Grotto_2, BmgNumber.zel_05 },
                { StageIDs.Grotto_3, BmgNumber.zel_05 },
                { StageIDs.Grotto_4, BmgNumber.zel_05 },
                { StageIDs.Grotto_5, BmgNumber.zel_05 },
                { StageIDs.Faron_Woods_Cave, BmgNumber.zel_05 },
                { StageIDs.Ordon_Ranch, BmgNumber.zel_01 },
                { StageIDs.Title_Screen, BmgNumber.zel_08 },
                { StageIDs.Ordon_Village, BmgNumber.zel_01 },
                { StageIDs.Ordon_Spring, BmgNumber.zel_01 },
                { StageIDs.Faron_Woods, BmgNumber.zel_06 },
                { StageIDs.Kakariko_Village, BmgNumber.zel_02 },
                { StageIDs.Death_Mountain, BmgNumber.zel_03 },
                { StageIDs.Kakariko_Graveyard, BmgNumber.zel_02 },
                { StageIDs.Zoras_River, BmgNumber.zel_08 },
                { StageIDs.Zoras_Domain, BmgNumber.zel_07 },
                { StageIDs.Snowpeak, BmgNumber.zel_06 },
                { StageIDs.Lake_Hylia, BmgNumber.zel_08 },
                { StageIDs.Castle_Town, BmgNumber.zel_04 },
                { StageIDs.Sacred_Grove, BmgNumber.zel_06 },
                { StageIDs.Bulblin_Camp, BmgNumber.zel_06 },
                { StageIDs.Hyrule_Field, BmgNumber.zel_08 },
                { StageIDs.Outside_Castle_Town, BmgNumber.zel_08 },
                { StageIDs.Bulblin_2, BmgNumber.zel_08 },
                { StageIDs.Gerudo_Desert, BmgNumber.zel_06 },
                { StageIDs.Mirror_Chamber, BmgNumber.zel_06 },
                { StageIDs.Upper_Zoras_River, BmgNumber.zel_08 },
                { StageIDs.Fishing_Pond, BmgNumber.zel_07 },
                { StageIDs.Hidden_Village, BmgNumber.zel_06 },
                { StageIDs.Hidden_Skill, BmgNumber.zel_06 },
                { StageIDs.Ordon_Village_Interiors, BmgNumber.zel_01 },
                { StageIDs.Hyrule_Castle_Sewers, BmgNumber.zel_04 },
                { StageIDs.Faron_Woods_Interiors, BmgNumber.zel_06 },
                { StageIDs.Kakariko_Village_Interiors, BmgNumber.zel_02 },
                { StageIDs.Death_Mountain_Interiors, BmgNumber.zel_03 },
                { StageIDs.Castle_Town_Interiors, BmgNumber.zel_04 },
                { StageIDs.Fishing_Pond_Interiors, BmgNumber.zel_07 },
                { StageIDs.Hidden_Village_Interiors, BmgNumber.zel_06 },
                { StageIDs.Castle_Town_Shops, BmgNumber.zel_04 },
                { StageIDs.Star_Game, BmgNumber.zel_04 },
                { StageIDs.Kakariko_Graveyard_Interiors, BmgNumber.zel_02 },
                { StageIDs.Light_Arrows_Cutscene, BmgNumber.zel_05 },
                { StageIDs.Hyrule_Castle_Cutscenes, BmgNumber.zel_04 },
            };

        public static BmgNumber StageIdToBmgNum(StageIDs stageId)
        {
            if (!stageToBmg.TryGetValue(stageId, out BmgNumber bmgNumber))
                throw new Exception($"Failed to find bmg for stageId '{stageId}'.");
            return bmgNumber;
        }
    }

    public class StringTableEntryInfo2
    {
        public BmgNumber bmgNumber { get; private set; }
        public ushort? context { get; private set; }
        public ushort infIndex { get; private set; }
        public string str { get; private set; }
        public int sortValue { get; private set; }

        public StringTableEntryInfo2(
            BmgNumber bmgNumber,
            ushort? context,
            ushort infIndex,
            string str
        )
        {
            Init(bmgNumber, context, infIndex, str);
        }

        public StringTableEntryInfo2(StageIDs stageId, ushort? context, ushort infIndex, string str)
        {
            BmgNumber bmgNumber = BmgNumUtils.StageIdToBmgNum(stageId);

            Init(bmgNumber, context, infIndex, str);
        }

        private void Init(BmgNumber bmgNumber, ushort? context, ushort infIndex, string str)
        {
            this.bmgNumber = bmgNumber;
            this.context = context;
            this.infIndex = infIndex;
            this.str = str;
            if (context != null)
            {
                uint contextVal = (uint)context;
                if (context == 0)
                    throw new Exception($"context of 0 is not valid.");
                this.sortValue = (int)((contextVal << 0x10) + infIndex);
            }
            else
            {
                this.sortValue = infIndex;
            }
        }
    }

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

    class EntryComparer : IComparer<StringTableEntryInfo2>
    {
        int IComparer<StringTableEntryInfo2>.Compare(
            StringTableEntryInfo2 a,
            StringTableEntryInfo2 b
        )
        {
            return (int)(a.sortValue - b.sortValue);
        }
    }

    class NodeRemapComparer : IComparer<BmgNodeRemap>
    {
        int IComparer<BmgNodeRemap>.Compare(BmgNodeRemap a, BmgNodeRemap b)
        {
            return (int)(a.sortValue - b.sortValue);
        }
    }

    public class BmgStrComps
    {
        public ushort nodeRemapContextCompStartIndex;
        public ushort nodeRemapContextCompLength = 0;
        public ushort contextCompsStartIndex;
        public ushort contextCompsLength = 0;
        public ushort basicCompsStartIndex;
        public ushort basicCompsLength = 0;
    }

    public class BmgNodeRemap
    {
        // public BmgNumber bmgNumber { get; private set; }
        // public ushort context { get; private set; }
        // public ushort infIndex { get; private set; }
        // public bool hasFliValue { get; private set; }
        // public int sortValue { get; private set; }

        public bool hasFliValue { get; private set; }
        public BmgNumber bmgNumber { get; private set; }
        public ushort fliValue { get; private set; }
        public ushort context { get; private set; }
        public ushort flwIndex { get; private set; }
        public ushort newFlwIndex { get; private set; }
        public ushort newContext { get; private set; }
        public int sortValue { get; private set; }

        public BmgNodeRemap(
            BmgNumber bmgNumber,
            ushort fliValue,
            ushort flwIndex,
            ushort newFlwIndex,
            ushort newContext
        )
        {
            Init(true, bmgNumber, fliValue, 0, flwIndex, newFlwIndex, newContext);
        }

        public BmgNodeRemap(
            StageIDs stageId,
            ushort fliValue,
            ushort flwIndex,
            ushort newFlwIndex,
            ushort newContext
        )
        {
            BmgNumber bmgNumber = BmgNumUtils.StageIdToBmgNum(stageId);

            Init(true, bmgNumber, fliValue, 0, flwIndex, newFlwIndex, newContext);
        }

        public BmgNodeRemap(ushort context, ushort flwIndex, ushort newFlwIndex, ushort newContext)
        {
            Init(false, BmgNumber.zel_00, 0, context, flwIndex, newFlwIndex, newContext);
        }

        private void Init(
            bool hasFliValue,
            BmgNumber bmgNumber,
            ushort fliValue,
            ushort context,
            ushort flwIndex,
            ushort newFlwIndex,
            ushort newContext
        )
        {
            this.hasFliValue = hasFliValue;
            this.bmgNumber = bmgNumber;
            this.fliValue = fliValue;
            this.context = context;
            this.flwIndex = flwIndex;
            this.newFlwIndex = newFlwIndex;
            this.newContext = newContext;
            if (hasFliValue)
                this.sortValue = (fliValue << 0x10) + flwIndex;
            else
                this.sortValue = (context << 0x10) + flwIndex;
        }
    }

    public class StringTable2
    {
        private static EntryComparer Comparer = new EntryComparer();
        private static EntryComparer NodeRemapComparer = new EntryComparer();

        public static StringTableResult2 GenStringTableInfo(List<StringTableEntryInfo2> inputList)
        {
            StringTableResult2 result = new();

            // We need to map from bmgNum => List<StringTableEntryInfo2>
            // Then we need to process them in groups going from 0 through 8 hardcoded.

            Dictionary<BmgNumber, List<StringTableEntryInfo2>> dict = new();

            foreach (StringTableEntryInfo2 entry in inputList)
            {
                BmgNumber bmgNumber = entry.bmgNumber;
                List<StringTableEntryInfo2> listForBmg;
                if (!dict.TryGetValue(bmgNumber, out listForBmg))
                {
                    listForBmg = new();
                    dict[bmgNumber] = listForBmg;
                }
                listForBmg.Add(entry);
            }

            Dictionary<string, ushort> stringToStrTableOffset = new();

            BmgNumber[] bmgNumbers = (BmgNumber[])Enum.GetValues(typeof(BmgNumber));
            for (int i = 0; i < bmgNumbers.Length; i++)
            {
                BmgNumber bmgNumber = bmgNumbers[i];

                BmgStrComps bmgStrComps = result.bmgStrCompsTable[i];
                bmgStrComps.contextCompsStartIndex = (ushort)result.contextCompVals.Count;
                bmgStrComps.basicCompsStartIndex = (ushort)result.basicCompVals.Count;

                if (!dict.TryGetValue(bmgNumber, out List<StringTableEntryInfo2> listForBmg))
                    continue;

                List<StringTableEntryInfo2> contextList = new();
                List<StringTableEntryInfo2> basicList = new();

                foreach (StringTableEntryInfo2 entry in listForBmg)
                {
                    if (entry.context == null)
                        basicList.Add(entry);
                    else
                        contextList.Add(entry);
                }

                contextList.Sort(Comparer);
                basicList.Sort(Comparer);

                foreach (StringTableEntryInfo2 entry in contextList)
                {
                    int lookupVal = entry.sortValue;
                    result.contextCompVals.Add((uint)lookupVal);
                    bmgStrComps.contextCompsLength += 1;

                    // Also need to add string to table.
                    string str = entry.str;
                    ushort strOffset;

                    if (!stringToStrTableOffset.TryGetValue(str, out strOffset))
                    {
                        // Is a new string
                        strOffset = (ushort)result.strTable.Count;
                        result.strTable.AddRange(Converter.MessageStringBytes(str));
                        result.strTable.Add(Converter.GcByte(0x0));
                        stringToStrTableOffset[str] = strOffset;
                    }
                    result.contextStrOffsets.Add(strOffset);
                }

                foreach (StringTableEntryInfo2 entry in basicList)
                {
                    ushort lookupVal = (ushort)entry.sortValue;
                    result.basicCompVals.Add(lookupVal);
                    bmgStrComps.basicCompsLength += 1;

                    // Also need to add string to table.
                    string str = entry.str;
                    ushort strOffset;

                    if (!stringToStrTableOffset.TryGetValue(str, out strOffset))
                    {
                        // Is a new string
                        strOffset = (ushort)result.strTable.Count;
                        result.strTable.AddRange(Converter.MessageStringBytes(str));
                        result.strTable.Add(Converter.GcByte(0x0));
                        stringToStrTableOffset[str] = strOffset;
                    }
                    result.basicStrOffsets.Add(strOffset);
                }
            }

            return result;
        }
    }

    public class StringTableResult2
    {
        private static NodeRemapComparer nodeRemapComparer = new();

        public List<BmgStrComps> bmgStrCompsTable = new();

        // node remaps
        private List<BmgNodeRemap> storedNodeRemaps = new();
        public List<uint> nodeRemapComps = new();
        public ushort numNodeRemapContextComps = 0;
        public List<uint> nodeRemapResults = new();

        // str replacements
        public List<uint> contextCompVals = new();
        public List<ushort> basicCompVals = new();
        public List<ushort> contextStrOffsets = new();
        public List<ushort> basicStrOffsets = new();
        public List<byte> strTable = new();

        public StringTableResult2()
        {
            BmgNumber[] bmgNumbers = (BmgNumber[])Enum.GetValues(typeof(BmgNumber));
            for (int i = 0; i < bmgNumbers.Length; i++)
            {
                bmgStrCompsTable.Add(new BmgStrComps());
            }
        }

        public class Header
        {
            public ushort bmgStrCompsTableOffset;
            public ushort bmgStrCompsTableNumEntries;
            public ushort nodeRemapCompsOffset;
            public ushort nodeRemapContextCompsLength;
            public ushort nodeRemapResultsOffset;
            public ushort contextCompValsOffset;
            public ushort basicCompValsOffset;
            public ushort strOffsetsTableOffset;
            public ushort numContextCompStrOffsets;
            public ushort strTableOffset;
        }

        public void AddNodeRemaps(List<BmgNodeRemap> nodeRemaps)
        {
            storedNodeRemaps.AddRange(nodeRemaps);
            // bmg => offset/length for FLI comps

            // offset/length of context comps

            // numContextComps

            // Table of u32 comp values.

            // Can use same offset/length for all, but have a special one for
            // context comps. So the comp values all sit in the same table. For
            // example, if the context comps were first, we would say something
            // like [offset 0, length 25] for those, then the offset for
            // bmg0FliComps would be [offset 25, length 1] for example.

        }

        private void CalcNodeRemapData()
        {
            List<BmgNodeRemap> contextCompRemaps = new();
            Dictionary<BmgNumber, List<BmgNodeRemap>> dict = new();

            foreach (BmgNodeRemap entry in storedNodeRemaps)
            {
                if (entry.hasFliValue)
                {
                    BmgNumber bmgNumber = entry.bmgNumber;
                    List<BmgNodeRemap> listForBmg;
                    if (!dict.TryGetValue(bmgNumber, out listForBmg))
                    {
                        listForBmg = new();
                        dict[bmgNumber] = listForBmg;
                    }
                    listForBmg.Add(entry);
                }
                else
                {
                    contextCompRemaps.Add(entry);
                }
            }

            // Handle context compares
            contextCompRemaps.Sort(nodeRemapComparer);

            foreach (BmgNodeRemap remap in contextCompRemaps)
            {
                nodeRemapComps.Add((uint)remap.sortValue);

                uint newShorts = (uint)(remap.newFlwIndex << 0x10) + remap.newContext;
                nodeRemapResults.Add(newShorts);
            }

            numNodeRemapContextComps = (ushort)contextCompRemaps.Count;

            // Handle FLI compares
            BmgNumber[] bmgNumbers = (BmgNumber[])Enum.GetValues(typeof(BmgNumber));
            for (int i = 0; i < bmgNumbers.Length; i++)
            {
                BmgNumber bmgNumber = bmgNumbers[i];

                BmgStrComps bmgStrComps = bmgStrCompsTable[i];
                bmgStrComps.nodeRemapContextCompStartIndex = (ushort)nodeRemapComps.Count;

                if (!dict.TryGetValue(bmgNumber, out List<BmgNodeRemap> listForBmg))
                    continue;

                listForBmg.Sort(nodeRemapComparer);

                bmgStrComps.nodeRemapContextCompLength = (ushort)listForBmg.Count;

                foreach (BmgNodeRemap entry in listForBmg)
                {
                    int lookupVal = entry.sortValue;
                    nodeRemapComps.Add((uint)lookupVal);

                    uint newShorts = (uint)(entry.newFlwIndex << 0x10) + entry.newContext;
                    nodeRemapResults.Add(newShorts);
                }
            }

            int abc = 7;
        }

        public Header AddBytesGenHeader(ushort headerSize, List<byte> bodyData)
        {
            CalcNodeRemapData();

            Header header = new();

            header.bmgStrCompsTableOffset = (ushort)(headerSize + bodyData.Count);
            foreach (BmgStrComps bmgStrComps in bmgStrCompsTable)
            {
                bodyData.AddRange(Converter.GcBytes(bmgStrComps.nodeRemapContextCompStartIndex)); // 0x00
                bodyData.AddRange(Converter.GcBytes(bmgStrComps.nodeRemapContextCompLength)); // 0x02
                bodyData.AddRange(Converter.GcBytes(bmgStrComps.contextCompsStartIndex)); // 0x04
                bodyData.AddRange(Converter.GcBytes(bmgStrComps.contextCompsLength)); // 0x06
                bodyData.AddRange(Converter.GcBytes(bmgStrComps.basicCompsStartIndex)); // 0x08
                bodyData.AddRange(Converter.GcBytes(bmgStrComps.basicCompsLength)); // 0x0a
            }
            header.bmgStrCompsTableNumEntries = (ushort)bmgStrCompsTable.Count;

            // Add nodeRemap stuff
            header.nodeRemapCompsOffset = (ushort)(headerSize + bodyData.Count);
            foreach (uint compVal in nodeRemapComps)
            {
                bodyData.AddRange(Converter.GcBytes(compVal)); // u32 comp val (context/fliVal , flwIndex)
            }
            header.nodeRemapContextCompsLength = numNodeRemapContextComps;

            header.nodeRemapResultsOffset = (ushort)(headerSize + bodyData.Count);
            foreach (uint remapVal in nodeRemapResults)
            {
                bodyData.AddRange(Converter.GcBytes(remapVal)); // u32 new vals (context, flwIndex)
            }

            // Add string comp stuff
            header.contextCompValsOffset = (ushort)(headerSize + bodyData.Count);
            foreach (uint contextCompVal in contextCompVals)
            {
                bodyData.AddRange(Converter.GcBytes(contextCompVal)); // entries are u32: [u16 context, u16 infIndex]
            }

            header.basicCompValsOffset = (ushort)(headerSize + bodyData.Count);
            foreach (ushort basicCompVal in basicCompVals)
            {
                bodyData.AddRange(Converter.GcBytes(basicCompVal)); // entries are u16 infIndexes
            }

            header.strOffsetsTableOffset = (ushort)(headerSize + bodyData.Count);
            header.numContextCompStrOffsets = (ushort)contextCompVals.Count;

            foreach (ushort strOffset in contextStrOffsets)
            {
                bodyData.AddRange(Converter.GcBytes(strOffset)); // entries are u16 offset in strTable
            }
            foreach (ushort strOffset in basicStrOffsets)
            {
                bodyData.AddRange(Converter.GcBytes(strOffset)); // entries are u16 offset in strTable
            }

            header.strTableOffset = (ushort)(headerSize + bodyData.Count);
            bodyData.AddRange(strTable);

            return header;
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

    public class EventTableEntryInfo
    {
        public ushort flwIndex = 0xFFFF;
        public ushort context = 1;
        public List<byte> sevenBytes = new();
        public ushort? nextFlwIndex = null;

        public EventTableEntryInfo(
            ushort flwIndex,
            ushort context,
            List<byte> sevenBytes,
            ushort? nextFlwIndex
        )
        {
            this.flwIndex = flwIndex;
            this.context = context;
            this.sevenBytes = sevenBytes;
            this.nextFlwIndex = nextFlwIndex;
        }
    }

    public class EventTable
    {
        public ushort numLookupEntries = 0;
        public List<byte> lookupTable = new();
        public List<byte> eventNodeData = new();
        public List<ushort> resultMapData = new();

        private ushort numEventNodes = 0;

        private EventTable() { }

        public static EventTable GenEventTable(
            List<EventTableEntryInfo> entryList,
            List<ushort> resultMapData
        )
        {
            if (resultMapData == null)
                throw new Exception("resultMapData must not be null.");

            EventTable inst = new EventTable();
            if (ListUtils.isEmpty(entryList))
                return inst;

            List<EventTableEntryInfo> aa =
                new()
                {
                    new(0x1a4, 3, new() { 9, 1, 0x31, 0, 0, 0, 0 }, 0xFFFF),
                    // new(0x1a3, 3, null, new() { 0x123, 0x456, 0xFFFF }),
                    // new(0x1a3, 3, null, new() { 0x123, 0x456, 0xFFFF }),
                };

            foreach (EventTableEntryInfo entry in entryList)
            {
                ushort eventNodeIndex = 0xFFFF;
                ushort resultMapIndex = 0xFFFF;

                if (entry.sevenBytes != null)
                {
                    if (entry.sevenBytes.Count != 7)
                        throw new Exception(
                            $"entry.sevenBytes.Count, expected 7, but was '{entry.sevenBytes.Count}'."
                        );

                    inst.eventNodeData.Add(3);
                    inst.eventNodeData.AddRange(entry.sevenBytes);
                    eventNodeIndex = inst.numEventNodes;
                    inst.numEventNodes += 1;
                }

                if (entry.nextFlwIndex != null)
                {
                    ushort nextFlwIndex = (ushort)entry.nextFlwIndex;

                    int index = resultMapData.FindIndex(flwIndex => nextFlwIndex == flwIndex);

                    if (index >= 0)
                    {
                        // FLW index is already present in list, so point at it
                        // rather than adding to the list.
                        resultMapIndex = (ushort)index;
                    }
                    else
                    {
                        // FLW index not in list, so add to end and point at it.
                        resultMapData.Add(nextFlwIndex);
                        resultMapIndex = (ushort)(resultMapData.Count - 1);
                    }
                }

                // First need to add the branch overwrite in order to determine an index.
                // Then need to add the resultMapEntries in order to determine an index.

                if (eventNodeIndex != 0xFFFF || resultMapIndex != 0xFFFF)
                {
                    List<byte> lookupEntry = new();
                    lookupEntry.AddRange(Converter.GcBytes(entry.flwIndex)); // 0x00
                    lookupEntry.AddRange(Converter.GcBytes(entry.context)); // 0x02
                    lookupEntry.AddRange(Converter.GcBytes(eventNodeIndex)); // 0x04
                    lookupEntry.AddRange(Converter.GcBytes(resultMapIndex)); // 0x06
                    inst.lookupTable.AddRange(lookupEntry);

                    inst.numLookupEntries += 1;
                }
            }

            return inst;
        }
    }
}
