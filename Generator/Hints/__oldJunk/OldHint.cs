// public enum HintType : byte
// {
//     SpiritOfLight = 0,
//     Barren = 1,
//     Always = 2,
//     Sometimes = 3,
//     EndOfGame = 4,
//     AgithaRewards = 5,
//     Random = 6,
//     Junk = 7,
// }

// public class Hint
// {
//     public string checkName { get; }
//     public Item contents { get; }
//     public HintType hintType { get; }
//     public byte count { get; }

//     public Hint(string checkName, Item contents, HintType hintType)
//     {
//         this.checkName = checkName;
//         this.contents = contents;
//         this.hintType = hintType;
//         this.count = 0;
//     }

//     public Hint(string checkName, Item contents, HintType hintType, byte count)
//     {
//         this.checkName = checkName;
//         this.contents = contents;
//         this.hintType = hintType;
//         this.count = count;
//     }
// }

// public class HintSpot
// {
//     public string name { get; }
//     public byte hintCount { get; set; }
//     public List<int> hintIndexArr { get; }

//     public HintSpot(string name)
//     {
//         this.name = name;
//         this.hintCount = 0;
//         this.hintIndexArr = new();
//     }
// }
