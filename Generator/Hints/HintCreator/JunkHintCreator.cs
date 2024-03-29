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
        private JunkHintCreator()
        {
            this.type = HintCreatorType.Junk;
        }

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
            return null;
        }
    }
}
