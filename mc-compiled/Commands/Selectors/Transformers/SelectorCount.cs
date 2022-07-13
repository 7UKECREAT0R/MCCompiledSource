using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorCount : SelectorTransformer
    {
        public string GetKeyword() => "COUNT";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            Selector testFor = tokens.Next<TokenSelectorLiteral>();
            TokenCompare comparison = tokens.Next<TokenCompare>();
            int number = tokens.Next<TokenIntegerLiteral>();
            Range range;

            switch (comparison.GetCompareType())
            {
                case TokenCompare.Type.EQUAL:
                    range = new Range(number, inverted);
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    range = new Range(number, !inverted);
                    break;
                case TokenCompare.Type.LESS_THAN:
                    range = new Range(null, number - 1, inverted);
                    break;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    range = new Range(null, number, inverted);
                    break;
                case TokenCompare.Type.GREATER_THAN:
                    range = new Range(number + 1, null, inverted);
                    break;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    range = new Range(number, null, inverted);
                    break;
                default:
                    range = new Range(number, inverted);
                    break;
            }

            const string counter = "_mcc_counter";
            string activeSelector = executor.ActiveSelectorStr;

            ScoreboardValue temp = executor.scoreboard.RequestTemp();
            commands.Add(Command.ScoreboardSet(activeSelector, temp, 0));
            commands.Add(Command.Tag(activeSelector, counter));
            commands.Add(Command.Execute(testFor.ToString(), Coord.here, Coord.here, Coord.here,
                Command.ScoreboardAdd($"@e[tag={counter}]", temp, 1)));
            commands.Add(Command.TagRemove(activeSelector, counter));
            rootSelector.scores.checks.Add(new ScoresEntry(temp, range));
        }
    }
}
