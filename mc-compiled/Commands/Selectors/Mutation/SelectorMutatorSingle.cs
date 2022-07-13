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
        public readonly Selector selector;

        public SelectorMutatorSingle(Selector selector)
        {
            this.selector = selector;
        }

        public Selector[] GetMutations()
        {
            return new[] { selector };
        }
    }
}
