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
    using System.Reflection.Metadata;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.Marshalling;
    using System.Runtime.Serialization;
    using System.Security.Cryptography.X509Certificates;
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

    public class StrReplEntity : Entity
    {
        public ushort? context { get; private set; }
        public ushort infIndex { get; private set; }
        public string str { get; private set; }

        public StrReplEntity(BmgNumber bmgNumber, ushort? context, ushort infIndex, string str)
        {
            Init(bmgNumber, context, infIndex, str);
        }

        public StrReplEntity(StageIDs stageId, ushort? context, ushort infIndex, string str)
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
                this.sortValue = (contextVal << 0x10) + infIndex;
            }
            else
            {
                this.sortValue = infIndex;
            }
        }

        public override bool getIsContextCompare()
        {
            return context != null;
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
            {
                if (flwIndex == 0xFFFF && newContext == 0)
                {
                    // This is to make it more difficult to create infinite flow
                    // loops.
                    throw new Exception(
                        $"Not allowed to remap an 0xFFFF FlwIndex using an FLI value unless you set a nonzero new flowContext."
                    );
                }
                this.sortValue = (fliValue << 0x10) + flwIndex;
            }
            else
                this.sortValue = (context << 0x10) + flwIndex;
        }
    }

    public abstract class Entity
    {
        public BmgNumber bmgNumber { get; protected set; }
        public uint sortValue { get; protected set; }
        public abstract bool getIsContextCompare();

        public void AddToCorrectList(
            bool basicUsesWordComp,
            List<uint> wordList,
            List<ushort> shortList
        )
        {
            if (basicUsesWordComp)
                wordList.Add(sortValue);
            else
                shortList.Add((ushort)sortValue);
        }
    }

    class EntityComparer : IComparer<Entity>
    {
        int IComparer<Entity>.Compare(Entity a, Entity b)
        {
            return (int)a.sortValue - (int)b.sortValue;
        }
    }

    public class NodeRemapEntity : Entity
    {
        public bool hasFliValue { get; private set; }
        public ushort fliValue { get; private set; }
        public ushort context { get; private set; }
        public ushort flwIndex { get; private set; }
        public ushort newFlwIndex { get; private set; }
        public ushort newContext { get; private set; }

        public NodeRemapEntity(
            BmgNumber bmgNumber,
            ushort fliValue,
            ushort flwIndex,
            ushort newFlwIndex,
            ushort newContext
        )
        {
            Init(true, bmgNumber, fliValue, 0, flwIndex, newFlwIndex, newContext);
        }

        public NodeRemapEntity(
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

        public NodeRemapEntity(
            ushort context,
            ushort flwIndex,
            ushort newFlwIndex,
            ushort newContext
        )
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
            {
                if (flwIndex == 0xFFFF && newContext == 0)
                {
                    // This is to make it more difficult to create infinite flow
                    // loops.
                    throw new Exception(
                        $"Not allowed to remap an 0xFFFF FlwIndex using an FLI value unless you set a nonzero new flowContext."
                    );
                }
                this.sortValue = (uint)(fliValue << 0x10) + flwIndex;
            }
            else
                this.sortValue = (uint)(context << 0x10) + flwIndex;
        }

        public override bool getIsContextCompare()
        {
            return !hasFliValue;
        }

        public uint getEntityTableUint()
        {
            return (uint)(newFlwIndex << 0x10) + newContext;
        }
    }

    public abstract class BaseCl<E>
    {
        public List<E> entities = new();
    }

    public class EntityLookupInfo
    {
        public short ctxCompAdjustment;
        public short basicCompAdjustment;
        public byte tableSliceInfoStartIdx;
        public List<byte> bmgLookupBytes = new();

        //
        public List<Entity> entityList = new();

        public List<byte> toBytes()
        {
            // Result is 14 (0xE) bytes long which is fine since largest
            // alignment is 2 bytes.
            List<byte> bytes = new();
            bytes.AddRange(Converter.GcBytes(ctxCompAdjustment)); // s16
            bytes.AddRange(Converter.GcBytes(basicCompAdjustment)); // s16
            bytes.Add(tableSliceInfoStartIdx); // u8
            bytes.AddRange(bmgLookupBytes); // u8[9]

            if (bytes.Count != 14)
                throw new Exception($"Expected bytes.Count to be 14, but was '{bytes.Count}'.");
            return bytes;
        }
    }

    public class StringTableResult2
    {
        private static NodeRemapComparer nodeRemapComparer = new();
        private static EntryComparer StrReplComparer = new();
        private static EntityComparer entityComparer = new();

        // node remaps
        private List<BmgNodeRemap> storedNodeRemaps = new();
        private List<Entity> storedNodeRemapEntities = new();
        public List<uint> nodeRemapComps = new();
        public ushort numNodeRemapContextComps = 0;
        public List<uint> nodeRemapResults = new();

        // str replacements
        public List<Entity> storedStrRepl = new();

        // results
        public List<ushort> contextStrOffsets = new();
        public List<ushort> basicStrOffsets = new();

        //
        // public ushort nodeRemapCtxCompsCount;
        // public ushort strReplCtxCompsCount;
        // public List<short> compIndexAdjustments = new();
        public List<byte> tableSliceLookupsByCompType = new();
        public List<ushort> tableSliceInfoTable = new();
        public List<uint> wordCompVals = new();
        public List<ushort> shortCompVals = new();
        public List<uint> nodeRemapTable = new();
        public List<ushort> strOffsetTable = new();
        public List<byte> strTable = new();

        public StringTableResult2() { }

        public class Header
        {
            public ushort entityInfoTableOffset;
            public ushort tableSliceInfosOffset;
            public ushort wordCompValsOffset;
            public ushort shortCompValsOffset;
            public ushort nodeRemapTableOffset;
            public ushort strOffsetTableOffset;
            public ushort strTableOffset;
        }

        class CompLists<T>
        {
            public List<T> ctxList = new();
            public List<T> basicList = new();
        }

        class TableSlicesPair
        {
            public ushort ctxStartIdx;
            public ushort ctxLen;
            public ushort basicStartIdx;
            public ushort basicLen;

            public byte UpdateTableSliceInfoTable(
                List<ushort> tableSliceInfoTable,
                byte tableSliceInfoStartIdx
            )
            {
                // TODO: isn't there a problem here where we can only support 64 / 18 types?

                // We need a base index, and then we can add an offset on top of
                // that which means we can handle an infinite number basically.

                // We have 9 bytes

                // We also have these things per enum:

                // u16 offset to entityData
                // s16 ctxCompAdjustment
                // s16 basicCompAdjustment
                // ^ which is 6 bytes
                // If we had another byte, that would make each one 0x10 bytes long

                // So like:
                // u16 entityDataOffset
                // s16 ctxCompAdjustment
                // s16 basicCompAdjustment
                // u8 tableSliceInfoStartIdx
                // u8[9] bmgLookupBytes (these could even be changed to u4,u4) if it makes sense

                // This structure might honestly take up slightly more space
                // when you factor in the code, but with how clean it is it
                // might be worth it for simplifying. We can also effectively
                // hardcode in the 9 for the BMGs which we could have been doing
                // anyway. We do remove at least one u16 offset from the header
                // though (not counting the ones we just move into these
                // structures) since we only need an offset to this table.

                // Maybe we pass in a ptr to the structure to the function which
                // returns the foundIndex? Since we will need it anyway, we only
                // have to look it up one time that way.


                byte baseOffset = (byte)(tableSliceInfoTable.Count / 2 - tableSliceInfoStartIdx);

                byte byteVal = 0;
                if (ctxLen > 0)
                {
                    byteVal |= 0x80;
                    tableSliceInfoTable.Add(ctxStartIdx);
                    tableSliceInfoTable.Add(ctxLen);
                }
                if (basicLen > 0)
                {
                    byteVal |= 0x40;
                    tableSliceInfoTable.Add(basicStartIdx);
                    tableSliceInfoTable.Add(basicLen);
                }
                if (byteVal != 0)
                    byteVal += baseOffset;
                return byteVal;
            }
        }

        public void AddStrReplacements(List<StrReplEntity> strReplacements)
        {
            storedStrRepl.AddRange(strReplacements);
        }

        public void AddNodeRemaps(List<BmgNodeRemap> nodeRemaps)
        {
            storedNodeRemaps.AddRange(nodeRemaps);
        }

        public void AddNodeRemapEntities(List<NodeRemapEntity> nodeRemaps)
        {
            storedNodeRemapEntities.AddRange(nodeRemaps);
        }

        private EntityLookupInfo CalcThing(List<Entity> entities, bool basicUsesWordComp = false)
        {
            EntityLookupInfo result = new();
            List<CompLists<Entity>> compListsList = new();
            List<TableSlicesPair> pairsList = new();
            // Init lists
            BmgNumber[] bmgNumbers = (BmgNumber[])Enum.GetValues(typeof(BmgNumber));
            for (int i = 0; i < bmgNumbers.Length; i++)
            {
                compListsList.Add(new());
                pairsList.Add(new());
            }

            HashSet<ulong> seenBmgPlusCompVals = new();

            foreach (Entity entity in entities)
            {
                int bmgNumber = (byte)entity.bmgNumber;
                CompLists<Entity> compLists = compListsList[bmgNumber];

                // Validate bmgPlusCompVal is unique
                ulong bmgPlusCompVal = ((ulong)bmgNumber << 32) + entity.sortValue;
                if (seenBmgPlusCompVals.Contains(bmgPlusCompVal))
                    throw new Exception($"Duplicate bmgPlusCompVal '{bmgPlusCompVal}'.");
                else
                    seenBmgPlusCompVals.Add(bmgPlusCompVal);

                if (entity.getIsContextCompare())
                    compLists.ctxList.Add(entity);
                else
                    compLists.basicList.Add(entity);
            }

            ushort numCtxEntities = 0;

            // Handle context

            ushort firstCtxCompOffset = (ushort)wordCompVals.Count;

            for (int i = 0; i < bmgNumbers.Length; i++)
            {
                TableSlicesPair tableSlicesPair = pairsList[i];
                CompLists<Entity> compLists = compListsList[i];

                List<Entity> ctxList = compLists.ctxList;
                ctxList.Sort(entityComparer);

                tableSlicesPair.ctxStartIdx = (ushort)wordCompVals.Count;

                foreach (Entity entity in ctxList)
                {
                    wordCompVals.Add(entity.sortValue);
                    tableSlicesPair.ctxLen += 1;

                    result.entityList.Add(entity);
                    numCtxEntities += 1;
                }
            }

            // Handle basic

            ushort firstBasicCompOffset;
            if (basicUsesWordComp)
                firstBasicCompOffset = (ushort)wordCompVals.Count;
            else
                firstBasicCompOffset = (ushort)shortCompVals.Count;

            for (int i = 0; i < bmgNumbers.Length; i++)
            {
                TableSlicesPair tableSlicesPair = pairsList[i];
                CompLists<Entity> compLists = compListsList[i];

                List<Entity> basicList = compLists.basicList;
                basicList.Sort(entityComparer);

                if (basicUsesWordComp)
                    tableSlicesPair.basicStartIdx = (ushort)wordCompVals.Count;
                else
                    tableSlicesPair.basicStartIdx = (ushort)shortCompVals.Count;

                foreach (Entity entity in basicList)
                {
                    entity.AddToCorrectList(basicUsesWordComp, wordCompVals, shortCompVals);
                    tableSlicesPair.basicLen += 1;

                    result.entityList.Add(entity);
                }
            }

            // compIndexAdjustments.Add((short)(-1 * firstCtxCompOffset));
            // compIndexAdjustments.Add((short)(numCtxEntities - firstBasicCompOffset));
            result.ctxCompAdjustment = (short)(-1 * firstCtxCompOffset);
            result.basicCompAdjustment = (short)(numCtxEntities - firstBasicCompOffset);
            result.tableSliceInfoStartIdx = (byte)(tableSliceInfoTable.Count / 2);

            foreach (TableSlicesPair pair in pairsList)
            {
                byte lookupByte = pair.UpdateTableSliceInfoTable(
                    tableSliceInfoTable,
                    result.tableSliceInfoStartIdx
                );
                result.bmgLookupBytes.Add(lookupByte);
                // tableSliceLookupsByCompType.Add(lookupByte);
            }

            return result;
        }

        // private void CalcNodeRemapData()
        // {
        //     // TODO: try to make this function generic so can reuse for strings, etc.

        //     List<CompLists<BmgNodeRemap>> compListsList = new();
        //     List<TableSlicesPair> pairsList = new();
        //     // Init lists
        //     BmgNumber[] bmgNumbers = (BmgNumber[])Enum.GetValues(typeof(BmgNumber));
        //     for (int i = 0; i < bmgNumbers.Length; i++)
        //     {
        //         compListsList.Add(new());
        //         pairsList.Add(new());
        //     }

        //     HashSet<ulong> seenBmgPlusCompVals = new();

        //     foreach (BmgNodeRemap entry in storedNodeRemaps)
        //     {
        //         int bmgNumber = (int)entry.bmgNumber;
        //         CompLists<BmgNodeRemap> compLists = compListsList[bmgNumber];

        //         // Validate bmgPlusCompVal is unique
        //         ulong bmgPlusCompVal = ((ulong)bmgNumber << 32) + (uint)entry.sortValue;
        //         if (seenBmgPlusCompVals.Contains(bmgPlusCompVal))
        //             throw new Exception($"Duplicate bmgPlusCompVal '{bmgPlusCompVal}'.");
        //         else
        //             seenBmgPlusCompVals.Add(bmgPlusCompVal);

        //         if (entry.hasFliValue)
        //             compLists.basicList.Add(entry);
        //         else
        //             compLists.ctxList.Add(entry);
        //     }

        //     List<uint> entityTable = new();

        //     // Handle context

        //     ushort firstCtxCompOffset = (ushort)wordCompVals.Count;

        //     for (int i = 0; i < bmgNumbers.Length; i++)
        //     {
        //         TableSlicesPair tableSlicesPair = pairsList[i];
        //         CompLists<BmgNodeRemap> compLists = compListsList[i];

        //         List<BmgNodeRemap> ctxList = compLists.ctxList;
        //         ctxList.Sort(nodeRemapComparer);

        //         tableSlicesPair.ctxStartIdx = (ushort)wordCompVals.Count;

        //         foreach (BmgNodeRemap entry in ctxList)
        //         {
        //             int lookupVal = entry.sortValue;
        //             wordCompVals.Add((uint)lookupVal);
        //             tableSlicesPair.ctxLen += 1;

        //             uint newShorts = (uint)(entry.newFlwIndex << 0x10) + entry.newContext;
        //             entityTable.Add(newShorts);
        //         }
        //     }

        //     ushort numCtxEntities = (ushort)entityTable.Count;

        //     // Handle basic

        //     ushort firstBasicCompOffset = (ushort)wordCompVals.Count;

        //     for (int i = 0; i < bmgNumbers.Length; i++)
        //     {
        //         TableSlicesPair tableSlicesPair = pairsList[i];
        //         CompLists<BmgNodeRemap> compLists = compListsList[i];

        //         List<BmgNodeRemap> basicList = compLists.basicList;
        //         basicList.Sort(nodeRemapComparer);

        //         tableSlicesPair.basicStartIdx = (ushort)wordCompVals.Count;

        //         foreach (BmgNodeRemap entry in basicList)
        //         {
        //             int lookupVal = entry.sortValue;
        //             wordCompVals.Add((uint)lookupVal);
        //             tableSlicesPair.basicLen += 1;

        //             uint newShorts = (uint)(entry.newFlwIndex << 0x10) + entry.newContext;
        //             entityTable.Add(newShorts);
        //         }
        //     }

        //     compIndexAdjustments.Add((short)(-1 * firstCtxCompOffset));
        //     compIndexAdjustments.Add((short)(numCtxEntities - firstBasicCompOffset));

        //     foreach (TableSlicesPair pair in pairsList)
        //     {
        //         byte lookupByte = pair.UpdateTableSliceInfoTable(tableSliceInfoTable);
        //         tableSliceLookupsByCompType.Add(lookupByte);
        //     }

        //     int abc = 7;
        // }

        // private void CalcStrReplData()
        // {
        //     List<CompLists<StringTableEntryInfo2>> compListsList = new();
        //     // Init list
        //     BmgNumber[] bmgNumbers = (BmgNumber[])Enum.GetValues(typeof(BmgNumber));
        //     for (int i = 0; i < bmgNumbers.Length; i++)
        //     {
        //         compListsList.Add(new());
        //     }

        //     HashSet<ulong> seenBmgPlusCompVals = new();

        //     foreach (StringTableEntryInfo2 entry in storedStrRepl)
        //     {
        //         int bmgNumber = (int)entry.bmgNumber;
        //         CompLists<StringTableEntryInfo2> compLists = compListsList[bmgNumber];

        //         // Validate bmgPlusCompVal is unique
        //         ulong bmgPlusCompVal = ((ulong)bmgNumber << 32) + (uint)entry.sortValue;
        //         if (seenBmgPlusCompVals.Contains(bmgPlusCompVal))
        //             throw new Exception($"Duplicate bmgPlusCompVal '{bmgPlusCompVal}'.");
        //         else
        //             seenBmgPlusCompVals.Add(bmgPlusCompVal);

        //         if (entry.context == null)
        //             compLists.basicList.Add(entry);
        //         else
        //             compLists.ctxList.Add(entry);
        //     }

        //     Dictionary<string, ushort> stringToStrTableOffset = new();

        //     List<ushort> ctxStrOffsetTable = new();
        //     List<ushort> basicStrOffsetTable = new();

        //     int absCtxStartIdx = wordCompVals.Count;

        //     // List<uint> newCtxWordCompVals = new();

        //     List<(int, int, int, int)> aaa = new();

        //     for (int i = 0; i < bmgNumbers.Length; i++)
        //     {
        //         CompLists<StringTableEntryInfo2> compLists = compListsList[i];

        //         List<StringTableEntryInfo2> ctxList = compLists.ctxList;
        //         ctxList.Sort(StrReplComparer);

        //         int ctxStartIdx = wordCompVals.Count;
        //         int ctxLen = 0;
        //         int basicStartIdx = shortCompVals.Count;
        //         int basicLen = 0;

        //         foreach (StringTableEntryInfo2 entry in ctxList)
        //         {
        //             int lookupVal = entry.sortValue;
        //             // newCtxWordCompVals.Add((uint)lookupVal);
        //             wordCompVals.Add((uint)lookupVal);
        //             ctxLen += 1;

        //             // Also need to add string to table.
        //             string str = entry.str;
        //             ushort strOffset;

        //             if (!stringToStrTableOffset.TryGetValue(str, out strOffset))
        //             {
        //                 // Is a new string
        //                 strOffset = (ushort)strTable.Count;
        //                 strTable.AddRange(Converter.MessageStringBytes(str));
        //                 strTable.Add(Converter.GcByte(0x0));
        //                 stringToStrTableOffset[str] = strOffset;
        //             }
        //             ctxStrOffsetTable.Add(strOffset);
        //         }

        //         List<StringTableEntryInfo2> basicList = compLists.basicList;
        //         basicList.Sort(StrReplComparer);

        //         foreach (StringTableEntryInfo2 entry in basicList)
        //         {
        //             int lookupVal = entry.sortValue;
        //             shortCompVals.Add((ushort)lookupVal);
        //             basicLen += 1;

        //             // Also need to add string to table.
        //             string str = entry.str;
        //             ushort strOffset;

        //             if (!stringToStrTableOffset.TryGetValue(str, out strOffset))
        //             {
        //                 // Is a new string
        //                 strOffset = (ushort)strTable.Count;
        //                 strTable.AddRange(Converter.MessageStringBytes(str));
        //                 strTable.Add(Converter.GcByte(0x0));
        //                 stringToStrTableOffset[str] = strOffset;
        //             }
        //             basicStrOffsetTable.Add(strOffset);
        //         }

        //         byte byteVal = (byte)(tableSliceInfoTable.Count / 2);
        //         bool hasVal = false;
        //         if (ctxLen > 0)
        //         {
        //             hasVal = true;
        //             byteVal |= 0x80;
        //             tableSliceInfoTable.Add((ushort)ctxStartIdx);
        //             tableSliceInfoTable.Add((ushort)ctxLen);
        //         }
        //         if (basicLen > 0)
        //         {
        //             hasVal = true;
        //             byteVal |= 0x40;
        //             tableSliceInfoTable.Add((ushort)basicStartIdx);
        //             tableSliceInfoTable.Add((ushort)basicLen);
        //         }
        //         if (!hasVal)
        //             byteVal = 0;

        //         tableSliceLookupsByCompType.Add(byteVal);

        //         aaa.Add((ctxStartIdx, ctxLen, basicStartIdx, basicLen));
        //     }

        //     strOffsetTable.AddRange(ctxStrOffsetTable);
        //     strOffsetTable.AddRange(basicStrOffsetTable);

        //     // strReplCtxCompsCount = (ushort)ctxStrOffsetTable.Count;

        //     int abc = 7;
        // }

        private void UpdateNodeRemapTable(EntityLookupInfo nodeRemapInfo)
        {
            List<NodeRemapEntity> nodeRemapEntities = nodeRemapInfo.entityList
                .Cast<NodeRemapEntity>()
                .ToList();
            foreach (NodeRemapEntity entity in nodeRemapEntities)
            {
                nodeRemapTable.Add(entity.getEntityTableUint());
            }
        }

        private void UpdateStrTables(EntityLookupInfo strReplInfo)
        {
            Dictionary<string, ushort> stringToStrTableOffset = new();

            List<StrReplEntity> nodeRemapEntities = strReplInfo.entityList
                .Cast<StrReplEntity>()
                .ToList();
            foreach (StrReplEntity entity in nodeRemapEntities)
            {
                string str = entity.str;

                if (!stringToStrTableOffset.TryGetValue(str, out ushort strOffset))
                {
                    // Is a new string
                    strOffset = (ushort)strTable.Count;
                    strTable.AddRange(Converter.MessageStringBytes(str));
                    strTable.Add(Converter.GcByte(0x0));
                    stringToStrTableOffset[str] = strOffset;
                }
                strOffsetTable.Add(strOffset);
            }
        }

        public Header AddBytesGenHeader(ushort headerSize, List<byte> bodyData)
        {
            // wordCompVals.Add(0);
            // wordCompVals.Add(1);
            // wordCompVals.Add(17);

            List<EntityLookupInfo> orderedEntityInfos = new();

            EntityLookupInfo nodeRemapInfo = CalcThing(storedNodeRemapEntities, true);
            orderedEntityInfos.Add(nodeRemapInfo);
            UpdateNodeRemapTable(nodeRemapInfo);

            EntityLookupInfo strReplInfo = CalcThing(storedStrRepl);
            orderedEntityInfos.Add(strReplInfo);
            UpdateStrTables(strReplInfo);

            Header header = new();

            header.entityInfoTableOffset = (ushort)(headerSize + bodyData.Count);
            foreach (EntityLookupInfo entityLookupInfo in orderedEntityInfos)
            {
                bodyData.AddRange(entityLookupInfo.toBytes());
            }
            while (bodyData.Count % 4 != 0)
                bodyData.Add(0);

            // bodyData.AddRange(tableSliceLookupsByCompType);
            // while (bodyData.Count % 4 != 0)
            //     bodyData.Add(0);

            header.tableSliceInfosOffset = (ushort)(headerSize + bodyData.Count);
            foreach (ushort entry in tableSliceInfoTable)
            {
                bodyData.AddRange(Converter.GcBytes(entry));
            }

            header.wordCompValsOffset = (ushort)(headerSize + bodyData.Count);
            foreach (uint entry in wordCompVals)
            {
                bodyData.AddRange(Converter.GcBytes(entry));
            }

            header.shortCompValsOffset = (ushort)(headerSize + bodyData.Count);
            foreach (ushort entry in shortCompVals)
            {
                bodyData.AddRange(Converter.GcBytes(entry));
            }

            header.nodeRemapTableOffset = (ushort)(headerSize + bodyData.Count);
            foreach (uint entry in nodeRemapTable)
            {
                bodyData.AddRange(Converter.GcBytes(entry));
            }

            header.strOffsetTableOffset = (ushort)(headerSize + bodyData.Count);
            foreach (ushort entry in strOffsetTable)
            {
                bodyData.AddRange(Converter.GcBytes(entry));
            }

            header.strTableOffset = (ushort)(headerSize + bodyData.Count);
            bodyData.AddRange(strTable);

            // // Add nodeRemap stuff
            // header.nodeRemapCompsOffset = (ushort)(headerSize + bodyData.Count);
            // foreach (uint compVal in nodeRemapComps)
            // {
            //     bodyData.AddRange(Converter.GcBytes(compVal)); // u32 comp val (context/fliVal , flwIndex)
            // }
            // header.nodeRemapContextCompsLength = numNodeRemapContextComps;

            // header.nodeRemapResultsOffset = (ushort)(headerSize + bodyData.Count);
            // foreach (uint remapVal in nodeRemapResults)
            // {
            //     bodyData.AddRange(Converter.GcBytes(remapVal)); // u32 new vals (context, flwIndex)
            // }

            // // Add string comp stuff
            // header.contextCompValsOffset = (ushort)(headerSize + bodyData.Count);
            // foreach (uint contextCompVal in wordCompVals)
            // {
            //     bodyData.AddRange(Converter.GcBytes(contextCompVal)); // entries are u32: [u16 context, u16 infIndex]
            // }

            // header.basicCompValsOffset = (ushort)(headerSize + bodyData.Count);
            // foreach (ushort basicCompVal in shortCompVals)
            // {
            //     bodyData.AddRange(Converter.GcBytes(basicCompVal)); // entries are u16 infIndexes
            // }

            // header.strOffsetsTableOffset = (ushort)(headerSize + bodyData.Count);
            // header.numContextCompStrOffsets = (ushort)wordCompVals.Count;

            // foreach (ushort strOffset in contextStrOffsets)
            // {
            //     bodyData.AddRange(Converter.GcBytes(strOffset)); // entries are u16 offset in strTable
            // }
            // foreach (ushort strOffset in basicStrOffsets)
            // {
            //     bodyData.AddRange(Converter.GcBytes(strOffset)); // entries are u16 offset in strTable
            // }

            // header.strTableOffset = (ushort)(headerSize + bodyData.Count);
            // bodyData.AddRange(strTable);

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
