namespace TPRandomizer.Hints.Settings
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.HintCreator;

    public enum HintCreatorType
    {
        Null = 0,

        // The below values are intentionally not assigned specific numbers to
        // indicate that they are not encoded anywhere and their order is not
        // important. Note that every HintType does not necessarily have a
        // HintCreator (since we do not want people to be able to manually
        // create hints of some types, such as AgithaRewards and BeyondPoint).
        // Additionally, some HintCreators do not actually have a corresponding
        // HintType (such as the SometimesHintCreator creating Location hints).
        Junk,
        Location,
        Woth,
        Barren,
        Item,
        NumItemInArea,
        Path,
        ItemToItemPath,
        TradeChain,
        TradeGroup,
        Sometimes,
    }

    public abstract class HintCreator
    {
        public HintCreatorType type { get; protected set; } = HintCreatorType.Null;

        public abstract List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        );

        private static HintCreatorType typeFromStr(string strType)
        {
            if (!Enum.TryParse(strType, true, out HintCreatorType type))
                throw new Exception($"Failed to parse HintCreatorType '{strType}'.");
            return type;
        }

        public static HintCreator fromJObject(JObject obj)
        {
            string strType = HintSettingUtils.getString(obj, "hintType");
            HintCreatorType type = typeFromStr(strType);

            switch (type)
            {
                case HintCreatorType.Junk:
                    return JunkHintCreator.fromJObject(obj);
                case HintCreatorType.Sometimes:
                    return SometimesHintCreator.fromJObject(obj);
                case HintCreatorType.Location:
                    return LocationHintCreator.fromJObject(obj);
                case HintCreatorType.Barren:
                    return BarrenHintCreator.fromJObject(obj);
                case HintCreatorType.Item:
                    return ItemHintCreator.fromJObject(obj);
                case HintCreatorType.NumItemInArea:
                    return NumItemInAreaHintCreator.fromJObject(obj);
                case HintCreatorType.Woth:
                    return WothHintCreator.fromJObject(obj);
                case HintCreatorType.Path:
                    return PathHintCreator.fromJObject(obj);
                case HintCreatorType.ItemToItemPath:
                    return ItemToItemPathHintCreator.fromJObject(obj);
                case HintCreatorType.TradeChain:
                    return TradeChainHintCreator.fromJObject(obj);
                case HintCreatorType.TradeGroup:
                    return TradeGroupHintCreator.fromJObject(obj);
                default:
                    throw new Exception(
                        $"Tried to create HintCreator, but found unexpected HintCreatorType '{type}'."
                    );
            }
        }
    }
}
