using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorPosition : SelectorTransformer
    {
        public string GetKeyword() => "POSITION";
        public bool CanBeInverted() => true;

        public void Transform(ref LegacySelector rootSelector, ref LegacySelector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            if(tokens.NextIs<TokenIdentifier>())
            {
                char axis = char.ToUpper(tokens.Next<TokenIdentifier>().word[0]);
                TokenCompare comparison = tokens.Next<TokenCompare>();

                Coord pos = tokens.Next<TokenCoordinateLiteral>();
                if (pos.isFacingOffset)
                    throw new StatementException(tokens, "Cannot use facing offset in position check.");

                Coord start = pos;
                int planeThickness;

                TokenCompare.Type compareType = comparison.GetCompareType();
                if (inverted)
                {
                    inverted = false;
                    compareType = SelectorUtils.InvertComparison(compareType);
                }

                const int VOLUME_MIN = -100000000;
                const int VOLUME_MAX = 100000000;
                const int VOLUME_HALFMIN = VOLUME_MIN / 2;

                switch (compareType)
                {
                    case TokenCompare.Type.EQUAL:
                        planeThickness = 0;
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        planeThickness = 0;
                        inverted = true;
                        break;
                    case TokenCompare.Type.LESS_THAN:
                        start -= 1;
                        planeThickness = VOLUME_MIN;
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        planeThickness = VOLUME_MIN;
                        break;
                    case TokenCompare.Type.GREATER_THAN:
                        start += 1;
                        planeThickness = VOLUME_MAX;
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        planeThickness = VOLUME_MAX;
                        break;
                    default:
                        planeThickness = 0;
                        break;
                }

                Area area;
                if (axis == 'X')
                {
                    area = rootSelector.area.Clone();
                    area.x = start;
                    area.volumeX = planeThickness;

                    if (!area.y.HasValue)
                        area.y = VOLUME_HALFMIN;
                    if (!area.z.HasValue)
                        area.z = VOLUME_HALFMIN;
                    if (!area.volumeY.HasValue)
                        area.volumeY = VOLUME_MAX;
                    if (!area.volumeZ.HasValue)
                        area.volumeZ = VOLUME_MAX;
                }
                else if (axis == 'Y')
                {
                    area = rootSelector.area.Clone();
                    area.y = start;
                    area.volumeY = planeThickness;

                    if (!area.x.HasValue)
                        area.x = VOLUME_HALFMIN;
                    if (!area.z.HasValue)
                        area.z = VOLUME_HALFMIN;
                    if (!area.volumeX.HasValue)
                        area.volumeX = VOLUME_MAX;
                    if (!area.volumeZ.HasValue)
                        area.volumeZ = VOLUME_MAX;
                }
                else if (axis == 'Z')
                {
                    area = rootSelector.area.Clone();
                    area.z = start;
                    area.volumeZ = planeThickness;

                    if (!area.x.HasValue)
                        area.x = VOLUME_HALFMIN;
                    if (!area.y.HasValue)
                        area.y = VOLUME_HALFMIN;
                    if (!area.volumeX.HasValue)
                        area.volumeX = VOLUME_MAX;
                    if (!area.volumeY.HasValue)
                        area.volumeY = VOLUME_MAX;
                } else
                    throw new StatementException(tokens, "Invalid axis. Valid options are X, Y, and Z.");

                if (inverted)
                    SelectorUtils.InvertSelector(ref rootSelector,
                        commands, executor, (sel) =>
                        {
                            sel.area = area;
                        });
                else
                    rootSelector.area = area;
            } else
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                Area area = new Area(x, y, z, null, null, 0, 0, 0);

                if (inverted)
                {
                    SelectorUtils.InvertSelector(ref rootSelector,
                        commands, executor, (sel) =>
                        {
                            sel.area = area;
                        });
                } else
                    rootSelector.area = area;
            }
        }
    }
}
