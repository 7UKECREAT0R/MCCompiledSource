using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorTag : SelectorTransformer
    {
        public string GetKeyword() => "TAG";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            while(tokens.NextIs<TokenStringLiteral>())
            {
                string tag = tokens.Next<TokenStringLiteral>();
                tag = Command.UTIL.MakeInvertedString(tag, false);
                alignedSelector.tags.Add(new Tag(tag, inverted));
            }
        }
    }
}
