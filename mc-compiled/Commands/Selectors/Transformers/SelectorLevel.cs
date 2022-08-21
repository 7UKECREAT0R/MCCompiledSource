using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorLevel : MutationProvider
    {
        public string GetKeyword() => "LEVEL";
        public bool CanBeInverted() => true;

        public Mutation.SelectorMutation[] GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            //int? levelMin;
            //int? levelMax;

            //TokenCompare comparison = tokens.Next<TokenCompare>();
            //int number = tokens.Next<TokenIntegerLiteral>();

            //TokenCompare.Type compareType = comparison.GetCompareType();
            //if (inverted)
            //{
            //    inverted = false;
            //    compareType = SelectorUtils.InvertComparison(compareType);
            //}

            //switch (compareType)
            //{
            //    case TokenCompare.Type.EQUAL:
            //        levelMin = number;
            //        levelMax = number;
            //        break;
            //    case TokenCompare.Type.NOT_EQUAL:
            //        levelMin = number;
            //        levelMax = number;
            //        inverted = true;
            //        break;
            //    case TokenCompare.Type.LESS_THAN:
            //        levelMin = null;
            //        levelMax = number - 1;
            //        break;
            //    case TokenCompare.Type.LESS_OR_EQUAL:
            //        levelMin = null;
            //        levelMax = number;
            //        break;
            //    case TokenCompare.Type.GREATER_THAN:
            //        levelMin = number + 1;
            //        levelMax = null;
            //        break;
            //    case TokenCompare.Type.GREATER_OR_EQUAL:
            //        levelMin = number;
            //        levelMax = null;
            //        break;
            //    default:
            //        levelMin = number;
            //        levelMax = number;
            //        break;
            //}

            //if(inverted)
            //{
            //    SelectorUtils.InvertSelector(ref alignedSelector,
            //        commands, executor, (sel) =>
            //        {
            //            if(levelMin.HasValue)
            //                sel.player.levelMin = levelMin;
            //            if(levelMax.HasValue)
            //                sel.player.levelMax = levelMax;
            //        });
            //}
            //else
            //{
            //    if (levelMin.HasValue)
            //        alignedSelector.player.levelMin = levelMin;
            //    if (levelMax.HasValue)
            //        alignedSelector.player.levelMax = levelMax;
            //}
            return null;
        }
    }
}
