namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;

    public class SometimesHintCreator : LocationHintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Sometimes;

        private SometimesHintCreator()
        {
            this.markAsSometimes = true;
            this.actingAsSometimes = true;
        }

        new public static SometimesHintCreator fromJObject(JObject obj)
        {
            SometimesHintCreator inst = new SometimesHintCreator();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                // Note: name has no "sometimes" in it, but we include it in the code to be
                // explicit.
                inst.prioritizeNewSometimesZones = HintSettingUtils.getOptionalBool(
                    options,
                    "prioritizeNewZones",
                    inst.prioritizeNewSometimesZones
                );
            }

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache,
            BarrenPenalizer barrenPenalizer
        )
        {
            // Need to update validChecks at this point since it is the first
            // time we get access to hintSettings.
            this.validChecks = new(hintSettings.sometimesChecks);

            return base.tryCreateHint(genData, hintSettings, numHints, cache, barrenPenalizer);
        }
    }
}
