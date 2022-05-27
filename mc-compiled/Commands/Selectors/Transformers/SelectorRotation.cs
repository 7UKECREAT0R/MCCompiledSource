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
            TokenCompare comparison = tokens.Next<TokenCompare>();
            int number = tokens.Next<TokenIntegerLiteral>();
            int? min, max;

            TokenCompare.Type compareType = comparison.GetCompareType();
            if(inverted)
            {
                inverted = false;
                compareType = SelectorUtils.InvertComparison(compareType);
            }

            switch (compareType)
            {
                case TokenCompare.Type.EQUAL:
                    min = number;
                    max = number;
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    min = number;
                    max = number;
                    inverted = true;
                    break;
                case TokenCompare.Type.LESS_THAN:
                    min = null;
                    max = number;  // max is exclusive, maybe.
                    break;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    min = null;
                    max = number + 1; // max is exclusive, maybe.
                    break;
                case TokenCompare.Type.GREATER_THAN:
                    min = number + 1;
                    max = null;
                    break;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    min = number;
                    max = null;
                    break;
                default:
                    min = null;
                    max = null;
                    break;
            }

            if (axis == 'X')
            {
                if (inverted)
                {
                    SelectorUtils.InvertSelector(ref selector,
                        commands, executor, (sel) =>
                        {
                            sel.entity.rotXMin = min;
                            sel.entity.rotXMax = max;
                        });
                }
                else
                {
                    selector.entity.rotXMin = min;
                    selector.entity.rotXMax = max;
                }
            }
            else if (axis == 'Y')
            {
                if (inverted)
                {
                    SelectorUtils.InvertSelector(ref selector,
                        commands, executor, (sel) =>
                        {
                            sel.entity.rotYMin = min;
                            sel.entity.rotYMax = max;
                        });
                }
                else
                {
                    selector.entity.rotYMin = min;
                    selector.entity.rotYMax = max;
                }
            }
            else
                throw new StatementException(tokens, "Invalid rotation axis. Valid options are X and Y.");
        }
    }
}
