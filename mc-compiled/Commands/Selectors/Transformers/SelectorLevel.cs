using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorLevel : SelectorTransformer
    {
        public string GetKeyword() => "LEVEL";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            int levelMin = tokens.Next<TokenIntegerLiteral>();
            int? levelMax;

            if (tokens.NextIs<TokenIntegerLiteral>())
                levelMax = tokens.Next<TokenIntegerLiteral>();
            else
                levelMax = null;

            if (inverted && levelMax == null)
            {
                selector.player.levelMin = 0;
                selector.player.levelMax = levelMin;
            }
            else if (inverted)
            {
                Player invertCondition = new Player(null, levelMin, levelMax);
                string entity = executor.ActiveSelectorCore;
                ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            invertCondition.AsStoreIn(entity, inverter)
                        });
                selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
            }
            else
            {
                selector.player.levelMin = levelMin;
                selector.player.levelMax = levelMax;
            }
        }
    }
}
