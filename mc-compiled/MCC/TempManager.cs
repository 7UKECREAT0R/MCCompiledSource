using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using System.Xml.Linq;

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
            this.Contracts = new List<TempStateContract>();

            Array valueTypes = Enum.GetValues(typeof(ScoreboardManager.ValueType));

            foreach (ScoreboardManager.ValueType valueType in valueTypes)
            {
                this.localTemps[valueType] = 0;
                this.globalTemps[valueType] = 0;
            }
        }

        /// <summary>
        /// The stack of <see cref="TempStateContract"/>s that belong to this instance.
        /// </summary>
        public List<TempStateContract> Contracts { get; private set; }
        /// <summary>
        /// The internal names of all the currently defined temps.
        /// </summary>
        public HashSet<string> DefinedTemps { get; private set; }

        /// <summary>
        /// The current number of local temps allocated.
        /// </summary>
        private Dictionary<ScoreboardManager.ValueType, int> localTemps;
        /// <summary>
        /// The current number of global temps allocated.
        /// </summary>
        private Dictionary<ScoreboardManager.ValueType, int> globalTemps;

        /// <summary>
        /// Requests a temp variable that copies the properties of the given <see cref="ScoreboardValue"/>.
        /// If overrideGlobal != null, then the clone's global settings will be overridden.
        /// </summary>
        /// <param name="toCopy">The <see cref="ScoreboardValue"/> to copy.</param>
        /// <param name="overrideGlobal">Set to non-null to override the global setting on the clone.</param>
        /// <returns></returns>
        public ScoreboardValue RequestCopy(ScoreboardValue toCopy, bool? overrideGlobal = null)
        {
            ScoreboardValue created = toCopy.Clone() as ScoreboardValue;
            bool isNowGlobal = created.clarifier.IsGlobal;

            // get depth depending on local/global.
            int currentDepth;
            if(isNowGlobal)
                this.globalTemps.TryGetValue(created.valueType, out currentDepth);
            else
                this.localTemps.TryGetValue(created.valueType, out currentDepth);

            // construct name
            string shortcode = ScoreboardManager.GetShortcodeFor(created.valueType);

            if(created.clarifier.IsGlobal)
                created.Name = PREFIX_GLOBAL + shortcode + currentDepth;
            else
                created.Name = PREFIX_LOCAL + shortcode + currentDepth;

            // override global setting if specified
            if (overrideGlobal.HasValue && toCopy.clarifier.IsGlobal != overrideGlobal)
            {
                isNowGlobal = overrideGlobal.Value;
                toCopy.clarifier.SetGlobal(isNowGlobal);
            }

            // define the temp value if it hasn't yet.
            if (DefinedTemps.Add(created.Name))
            {
                executor.AddCommandsInit(created.CommandsDefine());
                executor.AddCommandsInit(created.CommandsInit(toCopy.clarifier.CurrentString));
            }

            // increment the currentDepth.
            if (isNowGlobal)
                this.globalTemps[created.valueType] = currentDepth + 1;
            else
                this.localTemps[created.valueType] = currentDepth + 1;

            return created;
        }
        public ScoreboardValueInteger Request(bool global)
        {
            if (global)
                return RequestGlobal();
            else
                return RequestLocal();
        }
        public ScoreboardValue Request(ScoreboardManager.ValueType type, bool global)
        {
            if(global)
                return RequestGlobal(type);
            else
                return RequestLocal(type);
        }
        public ScoreboardValue Request(TokenLiteral literal, Statement forExceptions, bool global)
        {
            if(global)
                return RequestGlobal(literal, forExceptions);
            else
                return RequestLocal(literal, forExceptions);
        }
        public void Release(bool global)
        {
            if(global)
                ReleaseGlobal();
            else
                ReleaseLocal();
        }
        public void Release(ScoreboardManager.ValueType valueType, bool global)
        {
            if(global)
                ReleaseGlobal(valueType);
            else
                ReleaseLocal(valueType);
        }

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
                executor.AddCommandsInit(created.CommandsDefine());
                executor.AddCommandsInit(created.CommandsInit(created.clarifier.CurrentString));
            }

            // increment the currentDepth.
            this.localTemps[type] = currentDepth + 1;

            return created;
        }
        public ScoreboardValue RequestLocal(TokenLiteral literal, Statement forExceptions)
        {
            var type = literal.GetScoreboardValueType();

            if(type == ScoreboardManager.ValueType.INVALID)
                throw new StatementException(forExceptions, $"Unexpected literal of type '{literal.GetType().Name}' in TempManager::RequestLocal");

            this.localTemps.TryGetValue(type, out int currentDepth); // assignment
            string shortCode = ScoreboardManager.GetShortcodeFor(type);
            string name = PREFIX_LOCAL + shortCode + currentDepth;

            ScoreboardValue created = manager.CreateFromLiteral(name, false, literal, forExceptions);

            // define the temp value if it hasn't yet.
            if (DefinedTemps.Add(name))
            {
                executor.AddCommandsInit(created.CommandsDefine());
                executor.AddCommandsInit(created.CommandsInit(created.clarifier.CurrentString));
            }

            // increment the currentDepth.
            this.localTemps[type] = currentDepth + 1;

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
            string name = PREFIX_GLOBAL + shortCode + currentDepth;
            ScoreboardValue created = ScoreboardValue.CreateByType(type, name, true, manager);

            // define the temp value if it hasn't yet.
            if (DefinedTemps.Add(name))
            {
                executor.AddCommandsInit(created.CommandsDefine());
                executor.AddCommandsInit(created.CommandsInit(created.clarifier.CurrentString));
            }

            // increment the currentDepth.
            this.globalTemps[type] = currentDepth + 1;

            return created;
        }
        public ScoreboardValue RequestGlobal(TokenLiteral literal, Statement forExceptions)
        {
            var type = literal.GetScoreboardValueType();

            if (type == ScoreboardManager.ValueType.INVALID)
                throw new StatementException(forExceptions, $"Unexpected literal of type '{literal.GetType().Name}' in TempManager::RequestGlobal");

            this.globalTemps.TryGetValue(type, out int currentDepth); // assignment
            string shortCode = ScoreboardManager.GetShortcodeFor(type);
            string name = PREFIX_GLOBAL + shortCode + currentDepth;

            ScoreboardValue created = manager.CreateFromLiteral(name, true, literal, forExceptions);

            // define the temp value if it hasn't yet.
            if (DefinedTemps.Add(name))
            {
                executor.AddCommandsInit(created.CommandsDefine());
                executor.AddCommandsInit(created.CommandsInit(created.clarifier.CurrentString));
            }

            // increment the currentDepth.
            this.globalTemps[type] = currentDepth + 1;

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

        /// <summary>
        /// Creates a contextual copy of the TempManager's state and pushes it, like a stack.
        /// </summary>
        /// <returns></returns>
        public TempStateContract PushTempState()
        {
            TempStateContract contract = new TempStateContract(this);
            this.Contracts.Add(contract);
            return contract;
        }
        /// <summary>
        /// Restores the last popped version of the TempManager's state. Disposes the contract associated with it.
        /// To 
        /// </summary>
        public void PopTempState()
        {
            int lastIndex = this.Contracts.Count - 1;
            TempStateContract contract = this.Contracts[lastIndex];
            contract.Dispose();

            this.Contracts.RemoveAt(lastIndex);
        }

        /// <summary>
        /// Clear the resources used by this TempManager.
        /// </summary>
        internal void Clear()
        {
            this.Contracts.Clear();
            this.DefinedTemps.Clear();
            this.localTemps.Clear();
            this.globalTemps.Clear();
        }

        /// <summary>
        /// A contract given by <see cref="TempManager.PushTempState"/> used to release the state through disposal.
        /// This does not have to be disposed if the called does not want to use its features.
        /// </summary>
        public class TempStateContract : IDisposable
        {
            private bool _isDisposed;
            private TempManager parent;

            private readonly HashSet<string> definedTempsState;
            private readonly Dictionary<ScoreboardManager.ValueType, int> localTempState;
            private readonly Dictionary<ScoreboardManager.ValueType, int> globalTempState;

            internal TempStateContract(TempManager parent)
            {
                this.parent = parent;

                this.definedTempsState = new HashSet<string>(parent.DefinedTemps);
                this.localTempState = new Dictionary<ScoreboardManager.ValueType, int>(parent.localTemps);
                this.globalTempState = new Dictionary<ScoreboardManager.ValueType, int>(parent.globalTemps);
            }
            /// <summary>
            /// Restores this contract to its <see cref="TempManager"/> and gets rid of it.
            /// </summary>
            public void Dispose()
            {
                if (_isDisposed)
                    return;

                // copy all attributes to the TempManager
                parent.DefinedTemps = this.definedTempsState;
                parent.localTemps = this.localTempState;
                parent.globalTemps = this.globalTempState;

                // attempt to remove from the contract pool
                parent.Contracts.Remove(this);

                _isDisposed = true;
            }

            /// <summary>
            /// Returns if the given object is also the exact same TempStateContract.
            /// We want the equality check to fail when they are different instances, not just if their contents are equal.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return obj is TempStateContract && ReferenceEquals(obj, this);
            }
            /// <summary>
            /// Default HashCode generation.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                int hashCode = -1224315232;
                hashCode = hashCode * -1521134295 + _isDisposed.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<TempManager>.Default.GetHashCode(parent);
                hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(definedTempsState);
                hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<ScoreboardManager.ValueType, int>>.Default.GetHashCode(localTempState);
                hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<ScoreboardManager.ValueType, int>>.Default.GetHashCode(globalTempState);
                return hashCode;
            }
        }
    }
}
