using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace mc_compiled.MCC;

/// <summary>
///     Static class for managing compiler options/context.
/// </summary>
public static class GlobalContext
{
    private static readonly Stack<Context> contexts = new();
    /// <summary>
    ///     Get the current context. Never null, but may be invalid.
    /// </summary>
    [NotNull]
    public static Context Current { get; private set; } = new();

    /// <summary>
    ///     Shorthand for <c>GlobalContext.Current.debug</c>
    /// </summary>
    public static bool Debug => Current.debug;
    /// <summary>
    ///     Shorthand for <c>GlobalContext.Current.decorate</c>
    /// </summary>
    public static bool Decorate => Current.decorate;

    internal static void Pop()
    {
        contexts.Pop();
        Current = contexts.Peek() ?? new Context();
    }

    /// <summary>
    ///     Creates a new <see cref="Context" /> and pushes it to the top of the stack. Has default values.
    ///     Access using <see cref="GlobalContext.Current" />
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public static ContextContract NewDefault()
    {
        Current = new Context();
        contexts.Push(Current);
        return new ContextContract(Current);
    }
    /// <summary>
    ///     Creates a new <see cref="Context" /> cloned from the existing one and pushes it to the top of the stack.
    ///     Access using <see cref="GlobalContext.Current" />
    /// </summary>
    /// <returns></returns>
    public static ContextContract NewInherit()
    {
        Current = (Context) Current.Clone();
        contexts.Push(Current);
        return new ContextContract(Current);
    }
}

/// <summary>
///     A contract for a context that agrees to release itself once it's done being used.
/// </summary>
public class ContextContract : IDisposable
{
    public readonly Context heldContext;

    internal ContextContract(Context context) { this.heldContext = context; }
    public void Dispose() { GlobalContext.Pop(); }
}

/// <summary>
///     Compilation context.
/// </summary>
public class Context : ICloneable
{
    /// <summary>
    ///     The compiler will stay open and continue to compile files as they change.
    /// </summary>
    internal bool daemon;
    /// <summary>
    ///     Emit extra debug information in the console.
    /// </summary>
    public bool debug;
    /// <summary>
    ///     Decorate the compiled files with comments for human readers.
    /// </summary>
    public bool decorate;
    /// <summary>
    ///     Export all functions regardless if they're used or not.
    /// </summary>
    public bool exportAll;
    /// <summary>
    ///     Ignore manifests and leave them as-is. Used in special cases where the compiler doesn't support manifests.
    /// </summary>
    public bool ignoreManifests;
    /// <summary>
    ///     Don't pause when done compiling.
    /// </summary>
    internal bool noPause;
    /// <summary>
    ///     Special Regolith compilation settings.
    /// </summary>
    internal bool regolith;
    /// <summary>
    ///     Compiler will search and compile multiple files.
    /// </summary>
    internal bool search;
    /// <summary>
    ///     Add debug print statements to trace the execution of the application. Only really used for debugging.
    /// </summary>
    public bool trace;

    public object Clone() { return MemberwiseClone(); }
}