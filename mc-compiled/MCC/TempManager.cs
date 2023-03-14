using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages temp scoreboards. Used internally through Executor.scoreboard
    /// </summary>
    public class TempManager
    {
        // _tmp[L|G][SHORTCODE][INDEX]
        // Example: '_tmpGBLN1' is an index=1 Global Boolean
        // Example: '_tmpLDEC0' is an index=0 Local Decimal
        public const string PREFIX_LOCAL = "_tmpL";
        public const string PREFIX_GLOBAL = "_tmpG";

        private ScoreboardManager manager;
        private Executor executor;
        internal TempManager(ScoreboardManager manager, Executor executor)
        {
            this.manager = manager;
            this.executor = executor;

            this.localTemps = new Dictionary<ScoreboardManager.ValueType, int>();
            this.globalTemps = new Dictionary<ScoreboardManager.ValueType, int>();
            this.DefinedTemps = new HashSet<string>();
        }

        public HashSet<string> DefinedTemps { get; private set; }
        /// <summary>
        /// The current number of local temps allocated.
        /// </summary>
        private Dictionary<ScoreboardManager.ValueType, int> localTemps;
        /// <summary>
        /// The current number of global temps allocated.
        /// </summary>
        private Dictionary<ScoreboardManager.ValueType, int> globalTemps;

        public ScoreboardValueInteger RequestLocal() => RequestLocal(ScoreboardManager.ValueType.INT) as ScoreboardValueInteger;
        public ScoreboardValue RequestLocal(ScoreboardManager.ValueType type)
        {
            this.localTemps.TryGetValue(type, out int currentDepth); // assignment
            string shortCode = ScoreboardManager.GetShortcodeFor(type);

            // create the temp value.
            string name = PREFIX_LOCAL + shortCode + currentDepth;
            ScoreboardValue created = ScoreboardValue.CreateByType(type, name, false, manager);

            // define the temp value if it hasn't yet.
            if(DefinedTemps.Add(name))
            {
                executor.AddCommandsHead(created.CommandsInit());
                executor.AddCommandsHead(created.CommandsDefine());
            }

            // increment the currentDepth.
            localTemps[type] = currentDepth + 1;

            return created;
        }
        public void ReleaseLocal() => ReleaseLocal(ScoreboardManager.ValueType.INT);
        public void ReleaseLocal(ScoreboardManager.ValueType type)
        {
            this.localTemps.TryGetValue(type, out int currentDepth); // assignment

            if (currentDepth == 0)
                throw new Exception($"Called ReleaseLocal with no local temps of type {type}.");

            this.localTemps[type] = currentDepth - 1;
        }

        public ScoreboardValueInteger RequestGlobal() => RequestGlobal(ScoreboardManager.ValueType.INT) as ScoreboardValueInteger;
        public ScoreboardValue RequestGlobal(ScoreboardManager.ValueType type)
        {
            this.globalTemps.TryGetValue(type, out int currentDepth); // assignment
            string shortCode = ScoreboardManager.GetShortcodeFor(type);

            // create the temp value.
            string name = PREFIX_LOCAL + shortCode + currentDepth;
            ScoreboardValue created = ScoreboardValue.CreateByType(type, name, false, manager);

            // define the temp value if it hasn't yet.
            if (DefinedTemps.Add(name))
            {
                executor.AddCommandsHead(created.CommandsInit());
                executor.AddCommandsHead(created.CommandsDefine());
            }

            // increment the currentDepth.
            globalTemps[type] = currentDepth + 1;

            return created;
        }
        public void ReleaseGlobal() => ReleaseGlobal(ScoreboardManager.ValueType.INT);
        public void ReleaseGlobal(ScoreboardManager.ValueType type)
        {
            this.globalTemps.TryGetValue(type, out int currentDepth); // assignment

            if (currentDepth == 0)
                throw new Exception($"Called ReleaseGlobal with no global temps of type {type}.");

            this.globalTemps[type] = currentDepth - 1;
        }
    }
}
