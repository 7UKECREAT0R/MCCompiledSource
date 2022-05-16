using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorInside : SelectorTransformer
    {
        public string GetKeyword() => "INSIDE";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();
            int sizeX = tokens.Next<TokenIntegerLiteral>();
            int sizeY = tokens.Next<TokenIntegerLiteral>();
            int sizeZ = tokens.Next<TokenIntegerLiteral>();

            Area area = new Area(x, y, z, null, null, sizeX, sizeY, sizeZ);

            if (inverted)
            {
                ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                string entity = executor.ActiveSelectorCore;
                commands.AddRange(new[] {
                    Command.ScoreboardSet(entity, inverter, 0),
                    area.AsStoreIn(entity, inverter)
                });
                selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
            }
            else
                selector.area = area;
        }
    }
}
