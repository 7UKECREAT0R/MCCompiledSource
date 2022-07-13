using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorType : SelectorTransformer
    {
        public string GetKeyword() => "TYPE";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            string type = tokens.Next<TokenStringLiteral>();

            if (inverted)
                type = Command.UTIL.ToggleInversion(type);

            alignedSelector.entity.type = type;
        }
    }
}
