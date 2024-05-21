namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;
    using System.Linq;

    public class JunkHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Junk;

        private JunkHintCreator() { }

        new public static JunkHintCreator fromJObject(JObject obj)
        {
            JunkHintCreator inst = new JunkHintCreator();
            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            List<Hint> results = new();

            for (int i = 0; i < 130; i++)
            {
                // results.Add(new JunkHint(genData.rnd));
                JunkHint junkHint = new JunkHint((ushort)i);
                string aa = junkHint.toHintTextList(null)[0].text;
                Console.WriteLine("");
                Console.WriteLine(aa);
                Console.WriteLine("");
                int abc = 7;
            }

            for (int i = 0; i < numHints; i++)
            {
                results.Add(new JunkHint(genData.rnd));
            }

            return results;
        }
    }
}
