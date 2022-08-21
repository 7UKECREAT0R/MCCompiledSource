using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorTag : MutationProvider
    {
        public string GetKeyword() => "TAG";
        public bool CanBeInverted() => true;

        public Mutation.SelectorMutation[] GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            //while(tokens.NextIs<TokenStringLiteral>())
            //{
            //    string tag = tokens.Next<TokenStringLiteral>();
            //    tag = Command.UTIL.MakeInvertedString(tag, false);
            //    alignedSelector.tags.Add(new Tag(tag, inverted));
            //}
            return null;
        }
    }
}
