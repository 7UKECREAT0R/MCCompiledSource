using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Selectors;

/// <summary>
///     Utility methods for selectors and the systems surrounding them.
/// </summary>
public static class SelectorUtils
{
    /// <summary>
    ///     Invert the functionality of a <see cref="TokenCompare.Type" />
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static TokenCompare.Type InvertComparison(this TokenCompare.Type type)
    {
        return type switch
        {
            TokenCompare.Type.EQUAL => TokenCompare.Type.NOT_EQUAL,
            TokenCompare.Type.NOT_EQUAL => TokenCompare.Type.EQUAL,
            TokenCompare.Type.LESS => TokenCompare.Type.GREATER_OR_EQUAL,
            TokenCompare.Type.LESS_OR_EQUAL => TokenCompare.Type.GREATER,
            TokenCompare.Type.GREATER => TokenCompare.Type.LESS_OR_EQUAL,
            TokenCompare.Type.GREATER_OR_EQUAL => TokenCompare.Type.LESS,
            _ => type
        };
    }

    /// <summary>
    ///     Invert the direction of a <see cref="TokenCompare.Type" />. Differs from
    ///     <see cref="InvertComparison(TokenCompare.Type)" /> in that only the L/R direction is changed.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static TokenCompare.Type InvertDirection(this TokenCompare.Type type)
    {
        return type switch
        {
            TokenCompare.Type.EQUAL => TokenCompare.Type.EQUAL,
            TokenCompare.Type.NOT_EQUAL => TokenCompare.Type.NOT_EQUAL,
            TokenCompare.Type.LESS => TokenCompare.Type.GREATER,
            TokenCompare.Type.LESS_OR_EQUAL => TokenCompare.Type.GREATER_OR_EQUAL,
            TokenCompare.Type.GREATER => TokenCompare.Type.LESS,
            TokenCompare.Type.GREATER_OR_EQUAL => TokenCompare.Type.LESS_OR_EQUAL,
            _ => type
        };
    }

    /// <summary>
    ///     Get this core as a command string. e.g., @s, @a, @initiator
    /// </summary>
    /// <param name="core"></param>
    /// <returns></returns>
    public static string AsCommandString(this Selector.Core core)
    {
        return '@' + core.ToString();
    }
}