using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorNull : MutationProvider
    {
        public string GetKeyword() => "NULL";
        public bool CanBeInverted() => true;

        public void GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            executor.RequireFeature(tokens, MCC.Feature.NULLS);

            if(inverted)
                alignedSelector.entity.type = '!' + executor.entities.nulls.nullType;
            else
                alignedSelector.entity.type = executor.entities.nulls.nullType;

            if(tokens.NextIs<TokenStringLiteral>())
            {
                string name = tokens.Next<TokenStringLiteral>();

                if (inverted)
                    name = Command.UTIL.ToggleInversion(name);

                alignedSelector.entity.name = name;
            }
        }
    }
}
