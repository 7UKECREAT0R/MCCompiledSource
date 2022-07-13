using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation
{
    /// <summary>
    /// Describes a mutation/set of mutations of a selector.
    /// </summary>
    public interface SelectorMutation
    {
        /// <summary>
        /// Get the possible selector mutations.
        /// </summary>
        /// <returns></returns>
        Selector[] GetMutations();
    }
}
