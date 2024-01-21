using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC
{
    internal static class FeatureManager
    {
        /// <summary>
        /// All features available for enabling.
        /// </summary>
        internal static readonly IEnumerable<Feature> FEATURE_LIST = ((Feature[])Enum.GetValues(typeof(Feature))).Where(f => f != 0);

        /// <summary>
        /// Dictionary of features and their actions to perform on an executor when enabled.
        /// </summary>
        private static readonly Dictionary<Feature, Action<Executor>> ENABLE_ACTIONS = new Dictionary<Feature, Action<Executor>>()
        {
            {
                Feature.DUMMIES, (executor) =>
                {
                    executor.entities.dummies.AddEntityToProject();
                    executor.SetPPV("null", executor.entities.dummies.dummyType);
                }
            },
            {
                Feature.EXPLODERS, (executor) =>
                {
                    executor.entities.exploders.AddEntityToProject();
                }
            },
            {
                Feature.TESTS, (executor) => executor.CreateTestsFile()
            }
        };
        
        /// <summary>
        /// Called when a feature is enabled so that its enable action can run.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="feature"></param>
        internal static void OnFeatureEnabled(Executor caller, Feature feature)
        {
            if (ENABLE_ACTIONS.TryGetValue(feature, out Action<Executor> run))
                run(caller);
        }
    }
}
