using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Transforms a selector based off of a set of tokens.
    /// </summary>
    public interface SelectorTransformer
    {
        /// <summary>
        /// Get the keyword which is used to invoke this transformer.
        /// </summary>
        /// <returns></returns>
        string GetKeyword();
        /// <summary>
        /// Returns if this selector can be inverted.
        /// </summary>
        /// <returns></returns>
        bool CanBeInverted();

        /// <summary>
        /// Take in a set of tokens to transform the selector given, potentially adding commands or changing the project as well.
        /// </summary>
        /// <param name="rootSelector">The root selector; still aligned to the active entity.</param>
        /// <param name="alignedSelector">The selector after being aligned to each individual selected entity.</param>
        /// <param name="inverted">Whether this statement is inverted or not.</param>
        /// <param name="executor">The executor running this transformation.</param>
        /// <param name="tokens">The fed-in tokens which specify how the transformation should occur.</param>
        /// <param name="commands">The list of commands to add to.</param>
        /// 
        void Transform(ref Selector rootSelector, ref Selector alignedSelector,
            bool inverted, Executor executor, Statement tokens, List<string> commands);
    }

    /// <summary>
    /// i mostly just use this for inverting only once lol
    /// </summary>
    public static class SelectorUtils
    {
        /// <summary>
        /// Creates the commands needed to properly invert a selector if it doesn't natively support inversion.
        /// </summary>
        /// <param name="selector">The selector that will have a new score check added.</param>
        /// <param name="commands">The list of commands to append to.</param>
        /// <param name="executor">The parent executor running this.</param>
        /// <param name="transformer">The way to transform this selector after it's copied.</param>
        public static void InvertSelector(ref Selector selector, List<string> commands, Executor executor, Action<Selector> transformer)
        {
            MCC.ScoreboardValue inverter = executor.scoreboard.RequestTemp();
            Selector _entity = new Selector(executor.ActiveSelector);
            transformer(_entity);

            string previousEntity = executor.ActiveSelectorStr;

            commands.AddRange(new[] {
                Command.ScoreboardSet(previousEntity, inverter, 0),
                _entity.GetAsPrefix() + Command.ScoreboardSet("@s", inverter.Name, 1)
            });

            selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
        }

        /// <summary>
        /// Invert the functionality of a <see cref="TokenCompare.Type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TokenCompare.Type InvertComparison(TokenCompare.Type type)
        {
            switch (type)
            {
                case TokenCompare.Type.EQUAL:
                    return TokenCompare.Type.NOT_EQUAL;
                case TokenCompare.Type.NOT_EQUAL:
                    return TokenCompare.Type.EQUAL;
                case TokenCompare.Type.LESS_THAN:
                    return TokenCompare.Type.GREATER_OR_EQUAL;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return TokenCompare.Type.GREATER_THAN;
                case TokenCompare.Type.GREATER_THAN:
                    return TokenCompare.Type.LESS_OR_EQUAL;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return TokenCompare.Type.LESS_THAN;
                default:
                    return type;
            }
        }
    }
}
