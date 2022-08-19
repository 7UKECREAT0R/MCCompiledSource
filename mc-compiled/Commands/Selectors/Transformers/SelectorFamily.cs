using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorFamily : MutationProvider
    {
        public string GetKeyword() => "FAMILY";
        public bool CanBeInverted() => true;

        public void GetMutations(bool inverted, Executor executor, Statement tokens)
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
