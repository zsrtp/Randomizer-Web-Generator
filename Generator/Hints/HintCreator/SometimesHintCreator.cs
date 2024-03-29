namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;

    public class SometimesHintCreator : LocationHintCreator
    {
        private SometimesHintCreator()
        {
            this.type = HintCreatorType.Sometimes;
            this.markAsSometimes = true;
        }

        new public static SometimesHintCreator fromJObject(JObject obj)
        {
            SometimesHintCreator inst = new SometimesHintCreator();

            if (obj.ContainsKey("options"))
                throw new Exception("'options' not supported on SometimesHintCreator for now.");

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            // Need to update validChecks at this point since it is the first
            // time we get access to hintSettings.
            this.validChecks = new(hintSettings.sometimesChecks);

            return base.tryCreateHint(genData, hintSettings, numHints, cache);
        }
    }
}
