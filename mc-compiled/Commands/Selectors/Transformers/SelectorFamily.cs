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

        public void Transform(ref LegacySelector rootSelector, ref LegacySelector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            while(tokens.NextIs<TokenStringLiteral>())
            {
                string family = tokens.Next<TokenStringLiteral>();

                if (inverted)
                    family = Command.UTIL.ToggleInversion(family);

                alignedSelector.entity.families.Add(family);
            }
        }
    }
}
