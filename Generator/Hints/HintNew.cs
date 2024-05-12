namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using TPRandomizer.Util;

    // HintSpot has an array of Hints.
    // Hints have an array of textboxes.

    // HintSpot points to an array of Hints

    // HintSpots are an array of things which can take an array of hints.
    // 0 => Midna hint. 1 => Ordon spot ... etc.

    // AgithaHints are still a hintSpot, but the content will be a single Agitha
    // hint which internally handles multiple text boxes.


    public enum HintType : byte
    {
        Junk = 0,
        Location = 1,
        Woth = 2,
        Barren = 3,
        Item = 4,
        AgithaRewards = 5,
        BeyondPoint = 6,
        NumItemInArea = 7,
        Path = 8,
        ItemToItemPath = 9,
        TradeChain = 10,
        TradeGroup = 11,
        JovaniRewards = 12,
    }

    public class HintTypeUtils
    {
        public static readonly byte NumBitsToEncode = 4;
    }

    public interface IAreaHinter
    {
        AreaId GetAreaId();
    }

    public abstract class Hint
    {
        public abstract HintType type { get; }

        public abstract List<HintText> toHintTextList(CustomMsgData customMsgData);

        public virtual HintInfo GetHintInfo()
        {
            return null;
        }

        public virtual string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            return SettingsEncoder.EncodeNumAsBits((int)type, bitLengths.hintType);
        }

        public static Hint decodeHint(
            HintEncodingBitLengths bitLengths,
            BitsProcessor processor,
            Dictionary<int, byte> itemPlacements
        )
        {
            HintType type = (HintType)processor.NextInt(bitLengths.hintType);
            switch (type)
            {
                case HintType.Junk:
                    return JunkHint.decode(bitLengths, processor, itemPlacements);
                case HintType.Location:
                    return LocationHint.decode(bitLengths, processor, itemPlacements);
                case HintType.Woth:
                    return WothHint.decode(bitLengths, processor, itemPlacements);
                case HintType.Barren:
                    return BarrenHint.decode(bitLengths, processor, itemPlacements);
                case HintType.Item:
                    return ItemHint.decode(bitLengths, processor, itemPlacements);
                case HintType.AgithaRewards:
                    return AgithaRewardsHint.decode(bitLengths, processor, itemPlacements);
                case HintType.BeyondPoint:
                    return BeyondPointHint.decode(bitLengths, processor, itemPlacements);
                case HintType.NumItemInArea:
                    return NumItemInAreaHint.decode(bitLengths, processor, itemPlacements);
                case HintType.Path:
                    return PathHint.decode(bitLengths, processor, itemPlacements);
                case HintType.ItemToItemPath:
                    return ItemToItemPathHint.decode(bitLengths, processor, itemPlacements);
                case HintType.TradeChain:
                    return TradeChainHint.decode(bitLengths, processor, itemPlacements);
                case HintType.TradeGroup:
                    return TradeGroupHint.decode(bitLengths, processor, itemPlacements);
                case HintType.JovaniRewards:
                    return JovaniRewardsHint.decode(bitLengths, processor, itemPlacements);
                default:
                    throw new Exception(
                        $"Tried to decode hintType, but found unexpected type `{type}`."
                    );
            }
        }
    }

    public class HintEncodingBitLengths
    {
        public byte hintType = HintTypeUtils.NumBitsToEncode;
        public byte checkId = 9;
        public byte zoneId = ZoneUtils.NumBitsToEncode;
        public byte categoryId = HintCategoryUtils.NumBitsToEncode;
        public byte areaId = AreaId.NumBitsToEncode;
        public byte provinceId = ProvinceUtils.NumBitsToEncode;
        public byte hintSpotLocation = HintSpotLocationUtils.NumBitsToEncode;
        public byte goalEnum = GoalConstants.NumBitsToEncode;
        public byte tradeGroupId = TradeGroupUtils.NumBitsToEncode;
        public byte hintsPerSpot;

        public HintEncodingBitLengths(
            byte hintType,
            byte checkId,
            byte zoneId,
            byte categoryId,
            byte areaId,
            byte provinceId,
            byte hintSpotLocation,
            byte goalEnum,
            byte tradeGroupId,
            byte hintsPerSpot
        )
        {
            this.hintType = hintType;
            this.checkId = checkId;
            this.zoneId = zoneId;
            this.categoryId = categoryId;
            this.areaId = areaId;
            this.provinceId = provinceId;
            this.hintSpotLocation = hintSpotLocation;
            this.goalEnum = goalEnum;
            this.tradeGroupId = tradeGroupId;
            this.hintsPerSpot = hintsPerSpot;
        }

        // 4 bits => 0b0000 is 1, 0b1111 is 16.
        private string numAsBits(byte num)
        {
            return SettingsEncoder.EncodeNumAsBits(num - 1, 4);
        }

        public string encodeAsBits()
        {
            string result = numAsBits(hintType);
            result += numAsBits(checkId);
            result += numAsBits(zoneId);
            result += numAsBits(categoryId);
            result += numAsBits(areaId);
            result += numAsBits(provinceId);
            result += numAsBits(hintSpotLocation);
            result += numAsBits(goalEnum);
            result += numAsBits(tradeGroupId);
            result += numAsBits(hintsPerSpot);
            return result;
        }

        public static HintEncodingBitLengths decode(BitsProcessor processor)
        {
            byte hintType = (byte)(processor.NextInt(4) + 1);
            byte checkId = (byte)(processor.NextInt(4) + 1);
            byte zoneId = (byte)(processor.NextInt(4) + 1);
            byte categoryId = (byte)(processor.NextInt(4) + 1);
            byte areaId = (byte)(processor.NextInt(4) + 1);
            byte provinceId = (byte)(processor.NextInt(4) + 1);
            byte hintSpotLocation = (byte)(processor.NextInt(4) + 1);
            byte goalEnum = (byte)(processor.NextInt(4) + 1);
            byte tradeGroupId = (byte)(processor.NextInt(4) + 1);
            byte hintsPerSpot = (byte)(processor.NextInt(4) + 1);

            return new HintEncodingBitLengths(
                hintType,
                checkId,
                zoneId,
                categoryId,
                areaId,
                provinceId,
                hintSpotLocation,
                goalEnum,
                tradeGroupId,
                hintsPerSpot
            );
        }
    }
}
