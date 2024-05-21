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

            for (int i = 0; i < numHints; i++)
            {
                results.Add(new JunkHint(genData.rnd));
            }

            return results;
        }
    }
}
