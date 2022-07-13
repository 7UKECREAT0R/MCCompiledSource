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

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();
            int sizeX = tokens.Next<TokenIntegerLiteral>();
            int sizeY = tokens.Next<TokenIntegerLiteral>();
            int sizeZ = tokens.Next<TokenIntegerLiteral>();

            if(sizeX < 0)
            {
                sizeX *= -1;
                x -= sizeX;
            }
            if (sizeY < 0)
            {
                sizeY *= -1;
                y -= sizeY;
            }
            if (sizeZ < 0)
            {
                sizeZ *= -1;
                z -= sizeZ;
            }

            Area area;

            if(sizeX == 0 && sizeY == 0 && sizeZ == 0)
            {
                Executor.Warn($"To compare exact position, you *should* use 'if...position <x> <y> <z>'.", tokens);
                area = new Area(x, y, z);
            } else
                area = new Area(x, y, z, null, null, sizeX, sizeY, sizeZ);

            if (inverted)
            {
                SelectorUtils.InvertSelector(ref rootSelector,
                    commands, executor, (sel) =>
                    {
                        sel.area = area;
                    });
            }
            else
                rootSelector.area = area;
        }
    }
}
