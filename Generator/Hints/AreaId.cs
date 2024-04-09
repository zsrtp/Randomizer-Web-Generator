namespace TPRandomizer.Hints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using TPRandomizer.Util;

    public class AreaId : IEquatable<AreaId>
    {
        public static readonly byte NumBitsToEncode = 2;

        public enum AreaType : byte
        {
            Zone = 0,
            Category = 1,
            Province = 2,
            Check = 3,
        }

        public AreaType type { get; private set; }
        public string stringId { get; private set; }

        private AreaId() { }

        private AreaId(AreaType type, string stringId)
        {
            this.type = type;
            this.stringId = stringId;
        }

        public static AreaId Zone(Zone zone)
        {
            AreaId areaId = new();
            areaId.type = AreaType.Zone;
            areaId.stringId = ZoneUtils.IdToString(zone);
            return areaId;
        }

        public static AreaId ZoneStr(string zone)
        {
            AreaId areaId = new();
            areaId.type = AreaType.Zone;
            areaId.stringId = zone;
            return areaId;
        }

        public static AreaId Category(HintCategory category)
        {
            AreaId areaId = new();
            areaId.type = AreaType.Category;
            areaId.stringId = category.ToString();
            return areaId;
        }

        public static AreaId Province(Province province)
        {
            AreaId areaId = new();
            areaId.type = AreaType.Province;
            areaId.stringId = HintConstants.provinceToString[province];
            return areaId;
        }

        public static AreaId ProvinceStr(string provinceStr)
        {
            AreaId areaId = new();
            areaId.type = AreaType.Province;
            areaId.stringId = provinceStr;
            return areaId;
        }

        public string tempToString()
        {
            return $"{type.ToString()}:{stringId}";
        }

        public string GenResKey()
        {
            switch (type)
            {
                case AreaType.Zone:
                    Zone zone = ZoneUtils.StringToId(stringId);
                    return $"zone.{zone}".ToLowerInvariant();
                case AreaType.Category:
                    HintCategory category = HintCategoryUtils.StringToId(stringId);
                    return $"category.{category}".ToLowerInvariant();
                case AreaType.Province:
                    Province province = ProvinceUtils.StringToId(stringId);
                    return $"province.{province}".ToLowerInvariant();
                case AreaType.Check:
                    return $"check.{stringId}".ToLowerInvariant();
                default:
                    throw new Exception(
                        $"Failed to convert stringId to number for \"{stringId}\"."
                    );
            }
        }

        public string encodeAsBits(HintEncodingBitLengths bitLengths)
        {
            int id;
            byte idBitLength;

            switch (type)
            {
                case AreaType.Zone:
                    id = (int)ZoneUtils.StringToId(stringId);
                    idBitLength = bitLengths.zoneId;
                    break;
                case AreaType.Category:
                    id = (int)HintCategoryUtils.StringToId(stringId);
                    idBitLength = bitLengths.categoryId;
                    break;
                case AreaType.Province:
                    id = (int)ProvinceUtils.StringToId(stringId);
                    idBitLength = bitLengths.provinceId;
                    break;
                case AreaType.Check:
                    id = CheckIdClass.GetCheckIdNum(stringId);
                    idBitLength = bitLengths.checkId;
                    break;
                default:
                    throw new Exception($"Failed to encode areaId with \"{stringId}\".");
            }

            // Encode the type
            string result = SettingsEncoder.EncodeNumAsBits((int)type, bitLengths.areaId);
            result += SettingsEncoder.EncodeNumAsBits((int)id, idBitLength);
            return result;
        }

        public static AreaId decode(HintEncodingBitLengths bitLengths, BitsProcessor processor)
        {
            AreaType type = (AreaType)processor.NextInt(bitLengths.areaId);

            string stringId = null;

            switch (type)
            {
                case AreaType.Zone:
                    Zone zoneId = (Zone)processor.NextInt(bitLengths.zoneId);
                    stringId = ZoneUtils.IdToString(zoneId);
                    break;
                case AreaType.Category:
                    HintCategory category = (HintCategory)processor.NextInt(bitLengths.categoryId);
                    stringId = HintCategoryUtils.IdToString(category);
                    break;
                case AreaType.Province:
                    TPRandomizer.Hints.Province province =
                        (TPRandomizer.Hints.Province)processor.NextInt(bitLengths.provinceId);
                    stringId = ProvinceUtils.IdToString(province);
                    break;
                case AreaType.Check:
                    int checkId = processor.NextInt(bitLengths.checkId);
                    stringId = CheckIdClass.GetCheckName(checkId);
                    break;
                default:
                    throw new Exception($"Failed to encode areaId with \"{stringId}\".");
            }

            return new AreaId(type, stringId);
        }

        public static AreaType ParseAreaTypeStr(string str)
        {
            if (StringUtils.isEmpty(str))
                throw new Exception("Cannot parse an empty string.");

            AreaType areaType;
            bool success = Enum.TryParse(str, true, out areaType);
            if (success)
                return areaType;
            else
                throw new Exception($"Failed to parse '{areaType}' to AreaType enum.");
        }

        public static AreaId ParseString(string str)
        {
            if (StringUtils.isEmpty(str))
                throw new Exception("Cannot parse an empty string.");

            Regex r = new(@"^(\w+)\.(\w+)$");
            Match match = r.Match(str);
            if (!match.Success)
                throw new Exception($"Failed to parse areaId from '{str}'.");

            string areaTypeStr = match.Groups[1].Value;
            string areaVal = match.Groups[2].Value;

            AreaType areaType = ParseAreaTypeStr(areaTypeStr);
            return ParseString(areaType, areaVal);
        }

        public static AreaId ParseString(AreaType areaType, string areaVal)
        {
            switch (areaType)
            {
                case AreaType.Province:
                {
                    Province province = ProvinceUtils.StringToId(areaVal);
                    if (province == Hints.Province.Invalid)
                        throw new Exception($"areaVal '{areaVal}' resolved to Province.Invalid.");
                    return Province(province);
                }
                case AreaType.Zone:
                {
                    Zone zone = ZoneUtils.StringToId(areaVal);
                    if (zone == Hints.Zone.Invalid)
                        throw new Exception($"areaVal '{areaVal}' resolved to HintZone.Invalid.");
                    return Zone(zone);
                }
                default:
                    throw new Exception(
                        $"Cannot parse string with unsupported areaType '{areaType}'."
                    );
            }
        }

        public string ToLongStringId()
        {
            return type.ToString() + stringId;
        }

        public HashSet<string> ResolveToChecks()
        {
            switch (type)
            {
                case AreaType.Province:
                {
                    Province province = ProvinceUtils.StringToId(stringId);
                    return HintUtils.GetChecksForProvince(province);
                }
                case AreaType.Zone:
                {
                    Zone zone = ZoneUtils.StringToId(stringId);
                    return HintUtils.GetChecksForZone(zone);
                }
                case AreaType.Check:
                {
                    return new() { stringId };
                }
                case AreaType.Category:
                {
                    return new(
                        HintCategoryUtils.categoryToChecksMap[
                            HintCategoryUtils.StringToId(stringId)
                        ]
                    );
                }
                default:
                    throw new Exception($"Failed to resolve checks for '{stringId}'.");
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, stringId);
        }

        public bool Equals(AreaId other)
        {
            return type == other.type && stringId == other.stringId;
        }
    }
}
