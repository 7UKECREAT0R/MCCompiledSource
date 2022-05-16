﻿using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorNear : SelectorTransformer
    {
        public string GetKeyword() => "NEAR";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();
            int radius = tokens.Next<TokenIntegerLiteral>();

            int? minRadius = null;
            if (tokens.NextIs<TokenIntegerLiteral>())
                minRadius = tokens.Next<TokenIntegerLiteral>();

            Area area = new Area(x, y, z, minRadius, radius);

            if (inverted && minRadius != null)
            {
                ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                string entity = executor.ActiveSelectorCore;
                commands.AddRange(new[] {
                    Command.ScoreboardSet(entity, inverter, 0),
                    area.AsStoreIn(entity, inverter)
                });
                selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
            }
            else if (inverted)
            {
                area.radiusMin = area.radiusMax;
                area.radiusMax = 99999999f;
                selector.area = area;
            }
            else
                selector.area = area;
        }
    }
}