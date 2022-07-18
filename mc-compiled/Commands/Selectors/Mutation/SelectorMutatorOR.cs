using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation
{
    /// <summary>
    /// A set of mutations OR'd together.
    /// </summary>
    public class SelectorMutatorOR: ISelectorMutator
    {
        public readonly ISelectorMutator[] set;
        public bool inverted;

        public SelectorMutatorOR(params SelectorMutation[] mutations)
        {
            this.set = mutations;
        }
        public void SetInverted(bool inverted) => this.inverted = inverted;
        public void Condense(List<MutationSet> mutations)
        {
            // ensure [count * length] capacity to hold the new items.
            int len = set.Length;
            mutations.Capacity = mutations.Count * len;

            // clone sets [len - 1] times (for loop starts at 1 to perform this)
            for(int i = 1; i < len; i++)
            {
                IEnumerable<MutationSet> allClone = mutations.Select(m => m.Clone());
                mutations.AddRange(allClone);
            }

            // loop of possible mutations
            for(int root = 0; root < len; root++)
            {
                ISelectorMutator mutator = this.set[root];

                // loop of resulting MutationSet list
                for (int resulting = 0; resulting < mutations.Count; resulting++)
                {
                    MutationSet list = mutations[resulting];
                    bool invertThis = ((resulting - root) % len) != 0;

                    if(mutator is SelectorMutation)
                    {
                        SelectorMutation mutation = (mutator as SelectorMutation).Clone(invertThis);
                        list.Add(mutation);
                    }
                    else if(mutator is SelectorMutatorAND)
                    {
                        SelectorMutatorAND and = mutator as SelectorMutatorAND;
                        bool oldInvertValue = and.inverted;
                        if(invertThis)
                            and.inverted = !and.inverted;
                        and.Condense(mutations);
                        and.inverted = oldInvertValue;
                    }
                    else if (mutator is SelectorMutatorOR)
                    {
                        SelectorMutatorOR and = mutator as SelectorMutatorOR;
                        bool oldInvertValue = and.inverted;
                        if (invertThis)
                            and.inverted = !and.inverted;
                        and.Condense(mutations);
                        and.inverted = oldInvertValue;
                    }
                }
            }
        }
    }
}
