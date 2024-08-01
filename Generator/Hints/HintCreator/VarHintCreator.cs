namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;
    using System.Linq;

    public class VarHintCreator : HintCreator
    {
        public override HintCreatorType type { get; } = HintCreatorType.Var;

        private string varName;
        private bool includeStartingHints = false;
        private bool randomOrder = false;

        private VarHintCreator() { }

        new public static VarHintCreator fromJObject(JObject obj)
        {
            VarHintCreator inst = new VarHintCreator();
            if (!obj.ContainsKey("options"))
                throw new Exception("Must define 'varName' option for VarHintCreator.");

            JObject options = (JObject)obj["options"];

            inst.varName = HintSettingUtils.getOptionalString(options, "varName", inst.varName);
            if (StringUtils.isEmpty(inst.varName))
                throw new Exception("'varName' must be a non-empty string.");

            inst.includeStartingHints = HintSettingUtils.getOptionalBool(
                options,
                "includeStartingHints",
                inst.includeStartingHints
            );

            inst.randomOrder = HintSettingUtils.getOptionalBool(
                options,
                "randomOrder",
                inst.randomOrder
            );

            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            // This hint creator returns additional copies of hints which were
            // already created.

            List<Hint> listFromVar = genData.vars.GetHintsForVarName(varName, includeStartingHints);
            int numInVarList = listFromVar.Count;

            if (randomOrder)
                HintUtils.ShuffleListInPlace(genData.rnd, listFromVar);

            List<Hint> results = new(numHints);
            if (!ListUtils.isEmpty(listFromVar))
            {
                for (int i = 0; i < numHints; i++)
                {
                    int effectiveIndex = i % numInVarList;
                    Hint hint = listFromVar[effectiveIndex];
                    results.Add(hint);
                }
            }

            return results;
        }
    }
}
