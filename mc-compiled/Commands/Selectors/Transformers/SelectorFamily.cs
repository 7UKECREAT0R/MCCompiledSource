using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorFamily : SelectorTransformer
    {
        public string GetKeyword() => "FAMILY";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            string family = tokens.Next<TokenStringLiteral>();

            if (family.StartsWith("!"))
            {
                inverted = !inverted;
                family = family.Substring(1);
            }

            if (inverted)
                family = '!' + family;

            alignedSelector.entity.family = family;
        }
    }
}
