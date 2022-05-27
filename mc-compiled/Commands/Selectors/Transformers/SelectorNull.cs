using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorNull : SelectorTransformer
    {
        public string GetKeyword() => "NULL";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            if(inverted)
                alignedSelector.entity.type = '!' + executor.entities.nulls.nullType;
            else
                alignedSelector.entity.type = executor.entities.nulls.nullType;

            if(tokens.NextIs<TokenStringLiteral>())
            {
                alignedSelector.entity.name = tokens.Next<TokenStringLiteral>();
                alignedSelector.entity.nameNot = inverted;
            }
        }
    }
}
