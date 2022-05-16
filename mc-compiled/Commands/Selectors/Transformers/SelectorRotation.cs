using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorRotation : SelectorTransformer
    {
        public string GetKeyword() => "ROTATION";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            char axis = char.ToUpper(tokens.Next<TokenIdentifier>().word[0]);
            int rotMin = tokens.Next<TokenIntegerLiteral>();
            int rotMax = tokens.Next<TokenIntegerLiteral>();
            if (rotMin > rotMax)
            {
                int temp = rotMin;
                rotMin = rotMax;
                rotMax = temp;
            }

            if (axis == 'X')
            {
                if (inverted)
                {
                    ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                    string entity = executor.ActiveSelectorCore;
                    commands.AddRange(new[] {
                        Command.ScoreboardSet(entity, inverter, 0),
                        $"execute {entity}[rxm={rotMin},rx={rotMax}] ~~~ scoreboard players set @s {inverter.baseName} 1"
                    });
                    selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                }
                else
                {
                    selector.entity.rotXMin = rotMin;
                    selector.entity.rotXMax = rotMax;
                }
            }
            else if (axis == 'Y')
            {
                if (inverted)
                {
                    ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                    string entity = executor.ActiveSelectorCore;
                    commands.AddRange(new[] {
                        Command.ScoreboardSet(entity, inverter, 0),
                        $"execute {entity}[rym={rotMin},ry={rotMax}] ~~~ scoreboard players set @s {inverter.baseName} 1"
                    });
                    selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                }
                else
                {
                    selector.entity.rotYMin = rotMin;
                    selector.entity.rotYMax = rotMax;
                }
            }
            else
                throw new StatementException(tokens, "Invalid rotation axis. Valid options can be X or Y.");
        }
    }
}
