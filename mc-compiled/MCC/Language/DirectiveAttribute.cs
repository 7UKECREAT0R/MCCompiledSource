using System;
using JetBrains.Annotations;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Attributes used to modify how directive statements behave.
/// </summary>
[Flags]
[UsedImplicitly]
public enum DirectiveAttribute
{
    /// <summary>
    ///     Won't expand any explicit PPV identifiers. Used in $macro to allow passing in parameters.
    /// </summary>
    DONT_DEREFERENCE = 1 << 0,
    /// <summary>
    ///     Won't attempt to flatten JSON arrays to their root values.
    /// </summary>
    DONT_FLATTEN_ARRAYS = 1 << 1,
    /// <summary>
    ///     Won't resolve PPV dereferences in string parameters.
    /// </summary>
    DONT_RESOLVE_STRINGS = 1 << 2,
    /// <summary>
    ///     One or more of this directive's arguments support FString formatting (see <see cref="Executor.FString" />)
    /// </summary>
    USES_FSTRING = 1 << 3,
    /// <summary>
    ///     Inverts a comparison previously run on this scope. Used by ELSE and ELIF.
    /// </summary>
    INVERTS_COMPARISON = 1 << 4,
    /// <summary>
    ///     Won't decorate this directive in the compiled file when decoration is enabled.
    /// </summary>
    DONT_DECORATE = 1 << 5,
    /// <summary>
    ///     This directive is documentable by placing a comment before it.
    /// </summary>
    DOCUMENTABLE = 1 << 6,
    /// <summary>
    ///     This directive will cause async code to split stages (await)
    /// </summary>
    CAUSES_ASYNC_SPLIT = 1 << 7,
    /// <summary>
    ///     This directive may expand to the functional equivalent of multiple lines.
    /// </summary>
    MAY_EXPAND = 1 << 8
}