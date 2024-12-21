using System;
using mc_compiled.Commands.Execute;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Represents a comparison and how it was carried out. Used for if/else statements. Allocates one BOOL global temp
///     variable, which is released on disposal.
/// </summary>
internal class PreviousComparisonStructure : IDisposable
{
    internal readonly bool cancel;

    internal readonly string previousComparisonString;
    internal readonly ScoreboardValue resultStore;
    internal readonly int scope;
    internal readonly Statement sourceStatement;

    private readonly TempManager tempManager;

    private bool _disposed;
    internal ConditionalSubcommand conditionalUsed;

    /// <summary>
    ///     Allocates one BOOL global temp variable, which is released on disposal.
    /// </summary>
    /// <param name="tempManager"></param>
    /// <param name="caller"></param>
    /// <param name="scope"></param>
    /// <param name="previousComparisonString"></param>
    /// <param name="conditionalUsed"></param>
    internal PreviousComparisonStructure(TempManager tempManager, Statement caller, int scope,
        string previousComparisonString, ConditionalSubcommand conditionalUsed = null)
    {
        this.conditionalUsed = conditionalUsed;
        this.tempManager = tempManager;
        this.sourceStatement = caller;
        this.scope = scope;
        this.previousComparisonString = previousComparisonString;
        this.resultStore = tempManager.RequestGlobal(Typedef.BOOLEAN);
    }

    internal PreviousComparisonStructure(bool cancel)
    {
        this.cancel = cancel;
    }
    public void Dispose()
    {
        if (this._disposed)
            return;

        this._disposed = true;
        //tempManager?.Release(resultStore.type, resultStore.clarifier.IsGlobal);
    }
}