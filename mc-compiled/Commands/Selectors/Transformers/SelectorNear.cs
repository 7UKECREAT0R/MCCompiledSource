using mc_compiled.MCC;
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

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();

            float radius = tokens.Next<TokenNumberLiteral>().GetNumber();

            float? minRadius;

            if (tokens.NextIs<TokenNumberLiteral>())
                minRadius = tokens.Next<TokenNumberLiteral>().GetNumber();
            else
                minRadius = null;

            Area area = new Area(x, y, z, minRadius, radius);

            if (inverted && minRadius != null)
            {
                SelectorUtils.InvertSelector(ref rootSelector,
                    commands, executor, (sel) =>
                    {
                        sel.area = area;
                    });
            }
            else if (inverted)
            {
                byte[] byteArray = BitConverter.GetBytes(area.radiusMax.Value);
                int integralRepresentation = BitConverter.ToInt32(byteArray, 0);
                integralRepresentation += 1 << 3; // increment mantissa (probably)
                byteArray = BitConverter.GetBytes(integralRepresentation);
                area.radiusMin = BitConverter.ToSingle(byteArray, 0);
                area.radiusMax = null;
                rootSelector.area = area;
            }
            else
                rootSelector.area = area;
        }
    }
}
