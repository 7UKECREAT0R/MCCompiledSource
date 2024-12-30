using System;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Attributes used to modify how directive statements behave.
/// </summary>
[Flags]
[UsedImplicitly]
public enum DirectiveAttribute
{
    DONT_DEREFERENCE =
        1 << 0, // Won't expand any explicit PPV identifiers. Used in $macro to allow passing in parameters.
    DONT_FLATTEN_ARRAYS = 1 << 1, // Won't attempt to flatten JSON arrays to their root values.
    DONT_RESOLVE_STRINGS = 1 << 2, // Won't resolve PPV entries in string parameters.
    USES_FSTRING = 1 << 3, // Indicates support for format-strings.
    INVERTS_COMPARISON = 1 << 4, // Inverts a comparison previously run on this scope. Used by ELSE and ELIF.
    DONT_DECORATE = 1 << 5, // Won't decorate this directive in the compiled file when decoration is enabled.
    DOCUMENTABLE = 1 << 6, // This directive is documentable by placing a comment before it.
    CAUSES_ASYNC_SPLIT = 1 << 7 // This directive will cause async code to split stages (await)
}