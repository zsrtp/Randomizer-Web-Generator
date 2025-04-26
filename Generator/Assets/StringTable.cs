namespace TPRandomizer.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public static BmgNumber StgBmgToBmgNumber(StgBmg stgBmg)
        {
            if ((ushort)stgBmg >= 0x0100)
            {
                // BmgNumber
                BmgNumber val = (BmgNumber)((ushort)stgBmg & 0xFF);
                if (!Enum.IsDefined(val))
                    throw new Exception($"BmgNumber not found for StgBmg '{stgBmg}'.");
                return val;
            }
            // Stage
            return StageIdToBmgNum((StageIDs)stgBmg);
        }
    }

    public enum StgBmg : ushort
    {
        Lakebed_Temple = 0x0,
        Morpheel = 0x1,
        Deku_Toad = 0x2,
        Goron_Mines = 0x3,
        Fyrus = 0x4,
        Dangoro = 0x5,
        Forest_Temple = 0x6,
        Diababa = 0x7,
        Ook = 0x8,
        Temple_of_Time = 0x9,
        Armogohma = 0xa,
        Darknut = 0xb,
        City_in_the_Sky = 0xc,
        Argorok = 0xd,
        Aeralfos = 0xe,
        Palace_of_Twilight = 0xf,
        Zant_Main_Room = 0x10,
        Phantom_Zant_1 = 0x11,
        Phantom_Zant_2 = 0x12,
        Zant_Fight = 0x13,
        Hyrule_Castle = 0x14,
        Ganondorf_Castle = 0x15,
        Ganondorf_Field = 0x16,
        Ganondorf_Defeated = 0x17,
        Arbiters_Grounds = 0x18,
        Stallord = 0x19,
        Death_Sword = 0x1a,
        Snowpeak_Ruins = 0x1b,
        Blizzeta = 0x1c,
        Darkhammer = 0x1d,
        Lanayru_Ice_Puzzle_Cave = 0x1e,
        Cave_of_Ordeals = 0x1f,
        Eldin_Long_Cave = 0x20,
        Lake_Hylia_Long_Cave = 0x21,
        Eldin_Goron_Stockcave = 0x22,
        Grotto_1 = 0x23,
        Grotto_2 = 0x24,
        Grotto_3 = 0x25,
        Grotto_4 = 0x26,
        Grotto_5 = 0x27,
        Faron_Woods_Cave = 0x28,
        Ordon_Ranch = 0x29,
        Title_Screen = 0x2a,
        Ordon_Village = 0x2b,
        Ordon_Spring = 0x2c,
        Faron_Woods = 0x2d,
        Kakariko_Village = 0x2e,
        Death_Mountain = 0x2f,
        Kakariko_Graveyard = 0x30,
        Zoras_River = 0x31,
        Zoras_Domain = 0x32,
        Snowpeak = 0x33,
        Lake_Hylia = 0x34,
        Castle_Town = 0x35,
        Sacred_Grove = 0x36,
        Bulblin_Camp = 0x37,
        Hyrule_Field = 0x38,
        Outside_Castle_Town = 0x39,
        Bulblin_2 = 0x3a,
        Gerudo_Desert = 0x3b,
        Mirror_Chamber = 0x3c,
        Upper_Zoras_River = 0x3d,
        Fishing_Pond = 0x3e,
        Hidden_Village = 0x3f,
        Hidden_Skill = 0x40,
        Ordon_Village_Interiors = 0x41,
        Hyrule_Castle_Sewers = 0x42,
        Faron_Woods_Interiors = 0x43,
        Kakariko_Village_Interiors = 0x44,
        Death_Mountain_Interiors = 0x45,
        Castle_Town_Interiors = 0x46,
        Fishing_Pond_Interiors = 0x47,
        Hidden_Village_Interiors = 0x48,
        Castle_Town_Shops = 0x49,
        Star_Game = 0x4a,
        Kakariko_Graveyard_Interiors = 0x4b,
        Light_Arrows_Cutscene = 0x4c,
        Hyrule_Castle_Cutscenes = 0x4d,
        zel_00 = 0x0100,
        zel_01 = 0x0101,
        zel_02 = 0x0102,
        zel_03 = 0x0103,
        zel_04 = 0x0104,
        zel_05 = 0x0105,
        zel_06 = 0x0106,
        zel_07 = 0x0107,
        zel_08 = 0x0108,
    };

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

        protected static private void AddMaybeU16ToList(List<byte?> bytes, ushort? value)
        {
            if (value != null)
                bytes.AddRange(Converter.GcBytes((ushort)value).Cast<byte?>().ToArray());
            else
            {
                for (int i = 0; i < 2; i++)
                    bytes.Add(null);
            }
        }

        protected static private void AddMaybeIntToList(List<byte?> bytes, int? value)
        {
            if (value != null)
                bytes.AddRange(Converter.GcBytes((int)value).Cast<byte?>().ToArray());
            else
            {
                for (int i = 0; i < 4; i++)
                    bytes.Add(null);
            }
        }

        protected static List<byte> MaybeBytesToMagicByteList(List<byte?> maybeBytes)
        {
            HashSet<byte> seenBytes = new();

            for (int i = 0; i < maybeBytes.Count; i++)
            {
                byte? maybeByte = maybeBytes[i];
                if (maybeByte != null)
                    seenBytes.Add((byte)maybeByte);
            }

            byte magicByte = 0xFE;
            while (true)
            {
                if (!seenBytes.Contains(magicByte))
                    break;

                if (magicByte == 0)
                    throw new Exception($"Failed to find magic byte.");

                magicByte -= 1;
            }

            List<byte> result = new(maybeBytes.Count + 1) { magicByte };
            for (int i = 0; i < maybeBytes.Count; i++)
            {
                byte? maybeByte = maybeBytes[i];
                if (maybeByte != null)
                    result.Add((byte)maybeByte);
                else
                    result.Add(magicByte);
            }
            return result;
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

    public class BranchPatchEntity : Entity
    {
        public ushort? context;
        public ushort flwIndex;
        public byte? field_0x1; // 0x1 u8
        public ushort? queryIndex; // 0x2 u16
        public ushort? parameters; // 0x04 u16
        public ushort? nextNodeTableBaseIdx; // 0x06 u16

        public BranchPatchEntity(
            ushort flwIndex,
            ushort? context,
            StageIDs stageId = (StageIDs)5000,
            byte? field_0x1 = null,
            ushort? queryIndex = null,
            ushort? parameters = null,
            ushort? nextNodeTableBaseIdx = null
        )
        {
            // If not provided and defaults to 5000, treat as bmg00.
            if (stageId == (StageIDs)5000)
                bmgNumber = BmgNumber.zel_00;
            else
            {
                BmgNumber bmgNumber = BmgNumUtils.StageIdToBmgNum(stageId);
                this.bmgNumber = bmgNumber;
            }
            this.flwIndex = flwIndex;
            this.context = context;
            this.field_0x1 = field_0x1;
            this.queryIndex = queryIndex;
            this.parameters = parameters;
            this.nextNodeTableBaseIdx = nextNodeTableBaseIdx;

            if (context != null)
            {
                uint contextVal = (uint)context;
                if (context == 0)
                    throw new Exception($"context of 0 is not valid.");
                this.sortValue = (contextVal << 0x10) + flwIndex;
            }
            else
            {
                this.sortValue = flwIndex;
            }
        }

        public override bool getIsContextCompare()
        {
            return context != null;
        }

        public List<byte> getTableBytes()
        {
            List<byte?> maybeBytes = new(7) { field_0x1 };
            AddMaybeU16ToList(maybeBytes, queryIndex);
            AddMaybeU16ToList(maybeBytes, parameters);
            AddMaybeU16ToList(maybeBytes, nextNodeTableBaseIdx);

            return MaybeBytesToMagicByteList(maybeBytes);
        }
    }

    public class EventPatchEntity : Entity
    {
        private static Dictionary<BmgNumber, ushort> bmgToFfffNextNodeIdx =
            new() { { BmgNumber.zel_00, 0 } };

        public ushort? context { get; private set; }
        public ushort flwIndex { get; private set; }
        public byte? eventIndex { get; private set; } // 0x1 u8

        // Note: `vanillaNextNodeTableIdx` has a long name since this is
        // probably not what you want to use since it points to an existing
        // entry in the vanilla table rather than our own table.
        public ushort? vanillaNextNodeTableIdx { get; private set; } // 0x2 u16
        public ushort? nextNodeIdx { get; private set; }

        // Params are at offset 0x4. They are either u32, u16[2], or u8[4]
        private List<byte?> paramMaybeBytes = new();

        public EventPatchEntity(
            StgBmg stgBmg,
            ushort flwIndex,
            ushort? context,
            byte? eventIndex = null,
            ushort? vanillaNextNodeTableIdx = null,
            List<byte?> byteParams = null,
            List<ushort?> ushortParams = null,
            int? intParam = null,
            ushort? nextNodeIdx = null
        )
        {
            this.bmgNumber = BmgNumUtils.StgBmgToBmgNumber(stgBmg);
            this.flwIndex = flwIndex;
            this.context = context;
            this.eventIndex = eventIndex;
            this.vanillaNextNodeTableIdx = vanillaNextNodeTableIdx;
            this.nextNodeIdx = nextNodeIdx;

            // If the vanillaNextNodeTableIdx is not set and the nextNodeIdx is
            // 0xFFFF, we can do a minor optimization where we change
            // nextNodeIdx to null and vanillaNextNodeTableIdx to an idx which
            // stores 0xFFFF in vanilla based on the bmgNumber. For example, if
            // on zel_00, we can change vanillaNextNodeTableIdx to 0 since the
            // value at the start of the vanilla table is 0xFFFF.
            if (this.nextNodeIdx == 0xFFFF && this.vanillaNextNodeTableIdx == null)
            {
                if (bmgToFfffNextNodeIdx.TryGetValue(bmgNumber, out ushort vanillaIdx))
                {
                    this.vanillaNextNodeTableIdx = vanillaIdx;
                    this.nextNodeIdx = null;
                }
            }

            // Init paramMaybeBytes
            int numDefined = 0;
            bool byteParamsDefined = false;
            bool ushortParamsDefined = false;
            bool intParamDefined = false;

            if (!ListUtils.isEmpty(byteParams))
            {
                numDefined += 1;
                byteParamsDefined = true;
            }
            if (!ListUtils.isEmpty(ushortParams))
            {
                numDefined += 1;
                ushortParamsDefined = true;
            }
            if (intParam != null)
            {
                numDefined += 1;
                intParamDefined = true;
            }
            if (numDefined > 1)
                throw new Exception($"Expected 0 or 1 defined, but had '{numDefined}'.");

            if (byteParamsDefined)
            {
                paramMaybeBytes.AddRange(byteParams);
            }
            else if (ushortParamsDefined)
            {
                for (int i = 0; i < ushortParams.Count; i++)
                {
                    AddMaybeU16ToList(paramMaybeBytes, ushortParams[i]);
                }
            }
            else if (intParamDefined)
            {
                AddMaybeIntToList(paramMaybeBytes, intParam);
            }

            while (paramMaybeBytes.Count < 4)
                paramMaybeBytes.Add(null);

            if (paramMaybeBytes.Count != 4)
                throw new Exception(
                    $"paramMaybeBytes.Count must be 4, but was '{paramMaybeBytes.Count}'."
                );

            // Init sortValue
            if (context != null)
            {
                uint contextVal = (uint)context;
                if (context == 0)
                    throw new Exception($"context of 0 is not valid.");
                this.sortValue = (contextVal << 0x10) + flwIndex;
            }
            else
            {
                this.sortValue = flwIndex;
            }
        }

        public override bool getIsContextCompare()
        {
            return context != null;
        }

        public List<byte> getPatchBytes()
        {
            List<byte?> maybeBytes = new(7) { eventIndex };
            AddMaybeU16ToList(maybeBytes, vanillaNextNodeTableIdx);
            maybeBytes.AddRange(paramMaybeBytes);

            return MaybeBytesToMagicByteList(maybeBytes);
        }

        public bool hasPatchBytes()
        {
            List<byte> bytes = getPatchBytes();
            if (bytes.Count < 2)
                return false;
            byte magicValue = bytes[0];
            for (int i = 1; i < bytes.Count; i++)
            {
                if (bytes[i] != magicValue)
                    return true;
            }
            return false;
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
        private static EntityComparer entityComparer = new();
        private List<Entity> storedNodeRemaps = new();
        private List<Entity> storedStrRepl = new();
        private List<Entity> storedBranchPatches = new();
        private List<Entity> storedEventPatches = new();
        private List<Entity> storedEventNextNodes = new();

        //
        public List<ushort> tableSliceInfoTable = new();
        public List<uint> wordCompVals = new();
        public List<ushort> shortCompVals = new();
        public List<uint> nodeRemapTable = new();
        public List<byte> branchPatchTableData = new();
        public List<byte> eventPatchTableData = new();
        public List<ushort> eventNextNodeTable = new();
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
            public ushort branchPatchTableOffset;
            public ushort eventPatchTableOffset;
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
                // Need to calculate this before adding to the table
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

        public void AddNodeRemaps(List<NodeRemapEntity> nodeRemaps)
        {
            storedNodeRemaps.AddRange(nodeRemaps);
        }

        public void AddBranchPatches(List<BranchPatchEntity> branchPatches)
        {
            storedBranchPatches.AddRange(branchPatches);
        }

        public void AddEventEntities(List<EventPatchEntity> eventPatches)
        {
            foreach (EventPatchEntity entity in eventPatches)
            {
                if (entity.hasPatchBytes())
                    storedEventPatches.Add(entity);

                if (entity.nextNodeIdx != null)
                    storedEventNextNodes.Add(entity);
            }
        }

        public void AddStrReplacements(List<StrReplEntity> strReplacements)
        {
            storedStrRepl.AddRange(strReplacements);
        }

        private EntityLookupInfo BuildDataForEntityType(
            List<Entity> entities,
            bool basicUsesWordComp = false
        )
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
            }

            return result;
        }

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

        private void UpdateBranchPatchTableData(EntityLookupInfo branchPatchInfo)
        {
            List<BranchPatchEntity> entities = branchPatchInfo.entityList
                .Cast<BranchPatchEntity>()
                .ToList();
            foreach (BranchPatchEntity entity in entities)
            {
                branchPatchTableData.AddRange(entity.getTableBytes());
            }
        }

        private void UpdateEventPatchTableData(EntityLookupInfo eventPatchInfo)
        {
            List<EventPatchEntity> entities = eventPatchInfo.entityList
                .Cast<EventPatchEntity>()
                .ToList();
            foreach (EventPatchEntity entity in entities)
            {
                eventPatchTableData.AddRange(entity.getPatchBytes());
            }
        }

        private void UpdateEventNextNodeTable(EntityLookupInfo eventNextNodeInfo)
        {
            List<EventPatchEntity> entities = eventNextNodeInfo.entityList
                .Cast<EventPatchEntity>()
                .ToList();
            foreach (EventPatchEntity entity in entities)
            {
                if (entity.nextNodeIdx == null)
                    throw new Exception("nextNodeIdx was null, but expected to not be null.");

                eventNextNodeTable.Add((ushort)entity.nextNodeIdx);
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

            EntityLookupInfo nodeRemapInfo = BuildDataForEntityType(storedNodeRemaps, true);
            orderedEntityInfos.Add(nodeRemapInfo);
            UpdateNodeRemapTable(nodeRemapInfo);

            EntityLookupInfo branchPatchInfo = BuildDataForEntityType(storedBranchPatches);
            orderedEntityInfos.Add(branchPatchInfo);
            UpdateBranchPatchTableData(branchPatchInfo);

            EntityLookupInfo eventPatchInfo = BuildDataForEntityType(storedEventPatches);
            orderedEntityInfos.Add(eventPatchInfo);
            UpdateEventPatchTableData(eventPatchInfo);

            EntityLookupInfo eventNextNodeInfo = BuildDataForEntityType(storedEventNextNodes);
            orderedEntityInfos.Add(eventNextNodeInfo);
            UpdateEventNextNodeTable(eventNextNodeInfo);

            EntityLookupInfo strReplInfo = BuildDataForEntityType(storedStrRepl);
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

            // Align data to 8 bytes for branchPatchTable
            while (bodyData.Count % 8 != 0)
                bodyData.Add(0);
            header.branchPatchTableOffset = (ushort)(headerSize + bodyData.Count);
            bodyData.AddRange(branchPatchTableData);

            header.eventPatchTableOffset = (ushort)(headerSize + bodyData.Count);
            bodyData.AddRange(eventPatchTableData);

            header.strOffsetTableOffset = (ushort)(headerSize + bodyData.Count);
            foreach (ushort entry in strOffsetTable)
            {
                bodyData.AddRange(Converter.GcBytes(entry));
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
