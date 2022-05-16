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

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            Selector testFor = tokens.Next<TokenSelectorLiteral>();

            Range range;
            int min = tokens.Next<TokenIntegerLiteral>();

            if (tokens.NextIs<TokenIntegerLiteral>())
            {
                int max = tokens.Next<TokenIntegerLiteral>();
                if (min == max)
                    range = new Range(min, inverted);
                else
                    range = new Range(min, max, inverted);
            }
            else
                range = new Range(min, null, inverted);

            const string counter = "_mcc_counter";
            string activeSelector = executor.ActiveSelectorStr;

            ScoreboardValue temp = executor.scoreboard.RequestTemp();

            commands.Add(Command.ScoreboardSet(activeSelector, temp, 0));
            commands.Add(Command.Tag(activeSelector, counter));
            commands.Add(Command.Execute(testFor.ToString(), Coord.here, Coord.here, Coord.here,
                Command.ScoreboardAdd($"@e[tag={counter}]", temp, 1)));
            commands.Add(Command.TagRemove(activeSelector, counter));

            selector.scores.checks.Add(new ScoresEntry(temp, range));
        }
    }
}
