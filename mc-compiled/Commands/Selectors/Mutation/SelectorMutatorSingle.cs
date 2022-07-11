using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation
{
    /// <summary>
    /// Single selector mutation.
    /// </summary>
    public class SelectorMutatorSingle : SelectorMutation
    {
        public readonly LegacySelector selector;

        public SelectorMutatorSingle(LegacySelector selector)
        {
            this.selector = selector;
        }

        public LegacySelector[] GetMutations()
        {
            return new[] { selector };
        }
    }
}
