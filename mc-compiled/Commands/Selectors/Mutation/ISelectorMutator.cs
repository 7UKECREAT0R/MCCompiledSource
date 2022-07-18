using mc_compiled.MCC.Compiler;
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
    public interface ISelectorMutator
    {
        /// <summary>
        /// Add the possible BASE selector mutators to this list. All must be SelectorMutation at the end.
        /// </summary>
        /// <param name="mutations">The list to modify.</param>
        void Condense(List<MutationSet> mutations);
        /// <summary>
        /// Set whether this mutator/mutation is inverted.
        /// </summary>
        /// <param name="invert"></param>
        void SetInverted(bool invert);
    }
}
