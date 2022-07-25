namespace TPRandomizer.SSettings.Enums
{
    // WARNING: Certain enums in this file are are referenced by name in the
    // logic json files. To rename an enum, you must check with those files and
    // update both this file and any json files as needed.

    public enum LogicRules
    {
        Glitchless = 0,
        Glitched = 1,
        NoLogic = 2,
    }

    public enum CastleRequirements
    {
        Open = 0,
        Fused_Shadows = 1,
        Mirror_Shards = 2,
        All_Dungeons = 3,
        Vanilla = 4,
    }

    public enum PalaceRequirements
    {
        Open = 0,
        Fused_Shadows = 1,
        Mirror_Shards = 2,
        Vanilla = 3
    }

    public enum FaronWoodsLogic
    {
        Open = 0,
        Closed = 1
    }

    public enum SmallKeySettings
    {
        Vanilla = 0,
        OwnDungeon = 1,
        AnyDungeon = 2,
        Anywhere = 3,
        Keysey = 4,
    }

    public enum BigKeySettings
    {
        Vanilla = 0,
        OwnDungeon = 1,
        AnyDungeon = 2,
        Anywhere = 3,
        Keysey = 4,
    }

    public enum MapAndCompassSettings
    {
        Vanilla = 0,
        OwnDungeon = 1,
        AnyDungeon = 2,
        Anywhere = 3,
        StartWith = 4,
    }

    public enum TrapFrequency
    {
        None = 0,
        Few = 1,
        Many = 2,
        Mayhem = 3,
        Nightmare = 4,
    }
}
