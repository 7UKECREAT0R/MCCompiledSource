using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Utility methods for selectors and the systems surrounding them.
    /// </summary>
    public static class SelectorUtils
    {
        /// <summary>
        /// Invert the functionality of a <see cref="TokenCompare.Type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TokenCompare.Type InvertComparison(this TokenCompare.Type type)
        {
            switch (type)
            {
                case TokenCompare.Type.EQUAL:
                    return TokenCompare.Type.NOT_EQUAL;
                case TokenCompare.Type.NOT_EQUAL:
                    return TokenCompare.Type.EQUAL;
                case TokenCompare.Type.LESS:
                    return TokenCompare.Type.GREATER_OR_EQUAL;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return TokenCompare.Type.GREATER;
                case TokenCompare.Type.GREATER:
                    return TokenCompare.Type.LESS_OR_EQUAL;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return TokenCompare.Type.LESS;
                default:
                    return type;
            }
        }

        /// <summary>
        /// Invert the direction of a <see cref="TokenCompare.Type"/>. Differs from <see cref="InvertComparison(TokenCompare.Type)"/> in that only the L/R direction is changed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TokenCompare.Type InvertDirection(this TokenCompare.Type type)
        {
            switch (type)
            {
                case TokenCompare.Type.EQUAL:
                    return TokenCompare.Type.EQUAL;
                case TokenCompare.Type.NOT_EQUAL:
                    return TokenCompare.Type.NOT_EQUAL;
                case TokenCompare.Type.LESS:
                    return TokenCompare.Type.GREATER;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return TokenCompare.Type.GREATER_OR_EQUAL;
                case TokenCompare.Type.GREATER:
                    return TokenCompare.Type.LESS;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return TokenCompare.Type.LESS_OR_EQUAL;
                default:
                    return type;
            }
        }

        /// <summary>
        /// Get this core as a command string. e.g., @s, @a, @initiator
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string AsCommandString(this Selector.Core core)
        {
            return '@' + core.ToString();
        }
    }
}
