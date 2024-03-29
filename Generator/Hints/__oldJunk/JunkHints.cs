// namespace TPRandomizer.Hints
// {
//     using System;
//     using System.Collections.Generic;

//     public class JunkHints
//     {
//         private static Dictionary<string, int> nameToId;
//         private static Dictionary<int, string> idNumToName;
//         private static readonly List<string> hintList =
//             new()
//             {
//                 // Lore
//                 "They say that Coro, Hena, and Iza are siblings.",
//                 "They say that Hena does not like rowdy customers.",
//                 "They say that Agitha doesn't speak puppy.",
//                 // Tips
//                 "They say that Bomb Fish can be hooked with the Fishing Rod.",
//                 "They say that releasing the control stick when rolling over an edge prevents jumping.",
//                 "They say that the Ball and Chain can grab distant Pieces of Heart.",
//                 "They say that hawks will grab onto Cuccos.",
//                 "They say that hawks can attack distant enemies.",
//                 "They say that Trill will defeat enemies if he has to.",
//                 "That say that the fire breath of Dodongos can light torches.",
//                 "They say that rolling with the Iron Boots can damage enemies.",
//                 "They say that the Ball and Chain can destroy giant webs.",
//                 "They say that Ganondorf's biggest weakness may actually be a Fishing Rod!",
//                 "They say that the drained Magic Armor can sometimes be used as Iron Boots.",
//                 "They say that rupees may actually grow on some trees.",
//                 // Random facts
//                 "They say that the Mortal Draw has two animations.",
//                 "They say that you can pick up cats and dogs.",
//                 "They say that you can play fetch with dogs.",
//                 "They say that you can find bags of rupees when fishing.",
//                 "They say that fairies will rest on healthy individuals.",
//                 "They say that there is a rat in a suit of armor in Hyrule Castle.",
//                 "They say that it's possible to find a total of 60 Poes throughout Hyrule.",
//                 // Quotes
//                 "DON'T LITTER! The fish are CRYING!",
//                 "Go on, lad. Open the chest.",
//                 "Look into eyes of Yeto...",
//                 "Who need mirror?",
//                 "Li'l dragonfly, li'l dragonfly, when you look at me with those great big eyes, I...",
//                 "A sword wields no strength unless the hand that holds it has courage.",
//                 "What to order, what to order...I do believe I will start with the meat.",
//                 "Now THAT is a joke!",
//                 "Is perpetual twilight really all that bad?",
//                 // Joke
//                 "Stay hydrated.",
//                 "Bonk.",
//             };

//         static JunkHints()
//         {
//             nameToId = new(hintList.Count);
//             idNumToName = new(hintList.Count);

//             for (int i = 0; i < hintList.Count; i++)
//             {
//                 nameToId.Add(hintList[i], i);
//                 idNumToName.Add(i, hintList[i]);
//             }
//         }

//         public static int NameToId(string name)
//         {
//             if (nameToId.ContainsKey(name))
//             {
//                 return nameToId[name];
//             }
//             return -1;
//         }

//         public static string IdToName(int id)
//         {
//             if (idNumToName.ContainsKey(id))
//             {
//                 return idNumToName[id];
//             }
//             return null;
//         }

//         public static List<string> GetListCopy()
//         {
//             return new List<string>(hintList);
//         }

//         public class Generator
//         {
//             private List<int> shuffledIndexes;
//             private int nextIndex = 0;

//             public Generator(Random rnd)
//             {
//                 shuffledIndexes = new();
//                 for (int i = 0; i < JunkHints.hintList.Count; i++)
//                 {
//                     shuffledIndexes.Add(i);
//                 }
//                 ShuffleListInPlace(rnd, shuffledIndexes);
//             }

//             public Result GetNextHint()
//             {
//                 int index = nextIndex % shuffledIndexes.Count;
//                 bool isNewHint = nextIndex < shuffledIndexes.Count;
//                 int hintListIndex = shuffledIndexes[index];

//                 Result result = new Result(index, isNewHint, hintListIndex);

//                 nextIndex += 1;

//                 return result;
//             }

//             private static void ShuffleListInPlace<T>(Random rnd, IList<T> list)
//             {
//                 int n = list.Count;
//                 while (n > 1)
//                 {
//                     n--;
//                     int k = rnd.Next(n + 1);
//                     T value = list[k];
//                     list[k] = list[n];
//                     list[n] = value;
//                 }
//             }
//         }

//         public class Result
//         {
//             public int index;
//             public bool isNewHint;
//             public Hint hint;

//             public Result(int index, bool isNewHint, int hintListIndex)
//             {
//                 this.index = index;
//                 this.isNewHint = isNewHint;
//                 this.hint = new Hint(JunkHints.hintList[hintListIndex], 0, HintType.Junk);
//             }
//         }
//     }
// }
