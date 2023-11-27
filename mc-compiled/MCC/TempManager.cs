using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Compiler.TypeSystem.Implementations;

// ReSharper disable MemberCanBePrivate.Global

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages temp scoreboards. Used internally through <see cref="Executor.scoreboard"/> field in <see cref="Executor"/>
    /// </summary>
    public class TempManager : IDisposable
    {
        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
                return;

            foreach (TempStateContract contract in Contracts.Where(contract => contract != null))
                contract.Dispose();
            
            Contracts.Clear();
            _isDisposed = true;
        }

        // _tmp[L|G][SHORTCODE][INDEX]
        // Example: '_tmpGBLN1' is an index=1 Global Boolean
        // Example: '_tmpLDEC0' is an index=0 Local Decimal
        private const string PREFIX_LOCAL = "_tmpL";
        private const string PREFIX_GLOBAL = "_tmpG";

        private static string BuildLocalTempName(Typedef type, int index) =>
            $"{PREFIX_LOCAL}{type.TypeShortcode}{index}";
        private static string BuildGlobalTempName(Typedef type, int index) =>
            $"{PREFIX_GLOBAL}{type.TypeShortcode}{index}";

        private readonly ScoreboardManager manager;
        private readonly Executor executor;
        internal TempManager(ScoreboardManager manager, Executor executor)
        {
            this.manager = manager;
            this.executor = executor;

            this.localTemps = new Dictionary<Typedef, int>();
            this.globalTemps = new Dictionary<Typedef, int>();
            this.DefinedTemps = new HashSet<string>();
            this.Contracts = new List<TempStateContract>();

            Array valueTypes = Enum.GetValues(typeof(ScoreboardManager.ValueType));

            foreach (Typedef type in Typedef.ALL_TYPES)
            {
                this.localTemps[type] = 0;
                this.globalTemps[type] = 0;
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
        private Dictionary<Typedef, int> localTemps;
        /// <summary>
        /// The current number of global temps allocated.
        /// </summary>
        private Dictionary<Typedef, int> globalTemps;

        /// <summary>
        /// Requests a temp variable that copies the properties of the given <see cref="ScoreboardValue"/>.
        /// If overrideGlobal != null, then the clone's global settings will be overridden.
        /// </summary>
        /// <param name="toCopy">The <see cref="ScoreboardValue"/> to copy.</param>
        /// <param name="overrideGlobal">Set to non-null to override the global setting on the clone.</param>
        /// <returns></returns>
        public ScoreboardValue RequestCopy(ScoreboardValue toCopy, bool? overrideGlobal = null)
        {
            bool isNowGlobal = toCopy.clarifier.IsGlobal;
            Typedef type = toCopy.type;
            string shortcode = toCopy.type.TypeShortcode;

            string newName;
            Clarifier newClarifier = overrideGlobal == null?
                toCopy.clarifier.Clone() : overrideGlobal.Value?
                    Clarifier.Global():
                    Clarifier.Local();

            if (isNowGlobal)
            {
                this.globalTemps.TryGetValue(type, out int d);
                this.globalTemps[type] = d + 1;
                newName = PREFIX_GLOBAL + shortcode + d;
            }
            else
            {
                this.localTemps.TryGetValue(type, out int d);
                this.localTemps[type] = d + 1;
                newName = PREFIX_LOCAL + shortcode + d;
            }

            ScoreboardValue clone = toCopy.Clone(null,
                newName: null,
                newInternalName: newName,
                newClarifier: newClarifier);

            // define the temp value if it hasn't yet.
            if (!DefinedTemps.Add(clone.InternalName))
                return clone;
            
            executor.AddCommandsInit(clone.CommandsDefine());
            executor.AddCommandsInit(clone.CommandsInit(toCopy.clarifier.CurrentString));

            return clone;
        }
        public ScoreboardValue Request(bool global)
        {
            if (global)
                return RequestGlobal();
            else
                return RequestLocal();
        }
        public ScoreboardValue Request(Typedef type, bool global)
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
        public void Release(Typedef type, bool global)
        {
            if(global)
                ReleaseGlobal(type);
            else
                ReleaseLocal(type);
        }

        public ScoreboardValue RequestLocal() => RequestLocal(Typedef.INTEGER);
        public ScoreboardValue RequestLocal(Typedef type, object data = null)
        {
            this.localTemps.TryGetValue(type, out int currentDepth); // assignment

            // create the temp value.
            string name = BuildLocalTempName(type, currentDepth);
            var created = new ScoreboardValue(name, false, type, manager);
            
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
            Typedef type = literal.GetTypedef();

            if(type == null)
                throw new StatementException(forExceptions, $"Unexpected literal of type '{literal.GetType().Name}' in TempManager#RequestLocal");

            this.localTemps.TryGetValue(type, out int currentDepth); // assignment
            string name = BuildLocalTempName(type, currentDepth);

            ScoreboardValue created = literal.CreateValue(name, false, forExceptions);

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
        public void ReleaseLocal() => ReleaseLocal(Typedef.INTEGER);
        public void ReleaseLocal(Typedef type)
        {
            this.localTemps.TryGetValue(type, out int currentDepth); // assignment

            if (currentDepth == 0)
                throw new Exception($"Called ReleaseLocal with no local temps of type {type}.");

            this.localTemps[type] = currentDepth - 1;
        }

        public ScoreboardValue RequestGlobal() => RequestGlobal(Typedef.INTEGER);
        public ScoreboardValue RequestGlobal(Typedef type)
        {
            this.globalTemps.TryGetValue(type, out int currentDepth); // assignment

            // create the temp value.
            string name = BuildGlobalTempName(type, currentDepth);
            var created = new ScoreboardValue(name, true, type, manager);

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
            Typedef type = literal.GetTypedef();

            if (type == null)
                throw new StatementException(forExceptions, $"Unexpected literal of type '{literal.GetType().Name}' in TempManager#RequestGlobal");

            this.globalTemps.TryGetValue(type, out int currentDepth); // assignment
            string name = BuildGlobalTempName(type, currentDepth);

            ScoreboardValue created = literal.CreateValue(name, true, forExceptions);

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
        public void ReleaseGlobal() => ReleaseGlobal(Typedef.INTEGER);
        public void ReleaseGlobal(Typedef type)
        {
            this.globalTemps.TryGetValue(type, out int currentDepth); // assignment

            if (currentDepth == 0)
                throw new Exception($"Called ReleaseGlobal with no global temps of type {type}.");

            this.globalTemps[type] = currentDepth - 1;
        }

        /// <summary>
        /// Creates a contextual copy of the TempManager's state and pushes it, like a stack. To pop, just dispose the contract.
        /// </summary>
        /// <returns></returns>
        public TempStateContract PushTempState()
        {
            var contract = new TempStateContract(this);
            this.Contracts.Add(contract);
            return contract;
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
            
            private readonly TempManager parent;
            private readonly HashSet<string> definedTempsState;
            private readonly Dictionary<Typedef, int> localTempState;
            private readonly Dictionary<Typedef, int> globalTempState;

            internal TempStateContract(TempManager parent)
            {
                this.parent = parent;

                this.definedTempsState = new HashSet<string>(parent.DefinedTemps);
                this.localTempState = new Dictionary<Typedef, int>(parent.localTemps);
                this.globalTempState = new Dictionary<Typedef, int>(parent.globalTemps);
            }
            /// <summary>
            /// Restores this contract to its <see cref="TempManager"/> and gets rid of it.
            /// </summary>
            public void Dispose()
            {
                if (_isDisposed)
                    throw new Exception("Temp contract was already disposed.");

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
                hashCode = hashCode * -1521134295 + EqualityComparer<TempManager>.Default.GetHashCode(parent);
                hashCode = hashCode * -1521134295 + EqualityComparer<HashSet<string>>.Default.GetHashCode(definedTempsState);
                hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<Typedef, int>>.Default.GetHashCode(localTempState);
                hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<Typedef, int>>.Default.GetHashCode(globalTempState);
                return hashCode;
            }
        }
    }
}
