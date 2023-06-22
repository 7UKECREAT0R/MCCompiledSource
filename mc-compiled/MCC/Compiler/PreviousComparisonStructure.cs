using mc_compiled.Commands.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Represents a comparison and how it was carried out. Used for if/else statements. Allocates one BOOL global temp variable, which is released on disposal.
    /// </summary>
    internal class PreviousComparisonStructure : IDisposable
    {
        internal bool cancel;

        private readonly TempManager tempManager;
        internal readonly Statement sourceStatement;
        internal readonly int scope;
        internal ConditionalSubcommand conditionalUsed;

        internal readonly ScoreboardValueBoolean resultStore;

        /// <summary>
        /// Allocates one BOOL global temp variable, which is released on disposal.
        /// </summary>
        /// <param name="tempManager"></param>
        /// <param name="caller"></param>
        /// <param name="set"></param>
        /// <param name="scope"></param>
        /// <param name="setupFile"></param>
        internal PreviousComparisonStructure(TempManager tempManager, Statement caller, int scope, ConditionalSubcommand conditionalUsed = null)
        {
            this.conditionalUsed = conditionalUsed;
            this.tempManager = tempManager;
            this.sourceStatement = caller;
            this.scope = scope;
            this.resultStore = tempManager.RequestGlobal(ScoreboardManager.ValueType.BOOL) as ScoreboardValueBoolean;
        }

        internal PreviousComparisonStructure(bool cancel)
        {
            this.cancel = cancel;
        }

        bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if(tempManager != null)
                tempManager.Release(resultStore.valueType, resultStore.clarifier.IsGlobal);
        }
    }
}
