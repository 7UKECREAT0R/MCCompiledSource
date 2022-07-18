using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation
{
    public class SelectorMutatorAND : ISelectorMutator
    {
        public readonly ISelectorMutator[] set;
        public bool inverted;

        public SelectorMutatorAND(params SelectorMutation[] mutations)
        {
            this.set = mutations;
        }
        public void SetInverted(bool inverted) => this.inverted = inverted;
        public void Condense(List<MutationSet> mutations)
        {
            foreach (ISelectorMutator mutator in set)
            {
                if(mutator is SelectorMutation)
                {
                    // copy the mutation instance and set its inversion
                    SelectorMutation mutation = mutator as SelectorMutation;
                    SelectorMutation copy = mutation.Clone(inverted);

                    // add to all variants of the set (AND)
                    foreach (MutationSet mutationSet in mutations)
                        mutationSet.Add(copy);

                    continue;
                }

                mutator.Condense(mutations);
            }
        }
    }
}
