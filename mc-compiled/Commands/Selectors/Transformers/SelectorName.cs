using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorName : MutationProvider
    {
        public string GetKeyword() => "NAME";
        public bool CanBeInverted() => true;

        public Mutation.SelectorMutation[] GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            //string name = tokens.Next<TokenStringLiteral>();

            //if (inverted)
            //    name = Command.UTIL.ToggleInversion(name);

            //alignedSelector.entity.name = name;
            return null;
        }
    }
}
