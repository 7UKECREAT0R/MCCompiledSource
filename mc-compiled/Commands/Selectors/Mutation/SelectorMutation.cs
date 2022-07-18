using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation
{
    /// <summary>
    /// Changes a selector in some way.
    /// </summary>
    public abstract class SelectorMutation: ISelectorMutator
    {
        /// <summary>
        /// Logically invert this mutation so that it tests for the opposite.
        /// </summary>
        public bool Invert { get; set; }
        /// <summary>
        /// The target this mutation wants to modify.
        /// </summary>
        public MutationTarget Target { get; set; }


        public SelectorMutation(bool invert, MutationTarget target)
        {
            this.Invert = invert;
            this.Target = target;
        }
        /// <summary>
        /// Make a deep clone of this instance, optionally inversing its invert value.
        /// </summary>
        /// <returns></returns>
        public SelectorMutation Clone(bool inverse)
        {
            SelectorMutation copy = Clone();
            if (inverse)
                copy.Invert = !copy.Invert;

            return copy;
        }
        /// <summary>
        /// Make a deep clone of this instance.
        /// </summary>
        /// <returns></returns>
        public abstract SelectorMutation Clone();

        /// <summary>
        /// Mutate a selector based off this mutation implementation. The selector passed in will be based on the <see cref="Target"/> given from this instance.
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="executor"></param>
        /// <param name="invert"></param>
        public abstract void Mutate(Executor executor, Selector selector);

        public void SetInverted(bool inverted) => this.Invert = inverted;
        public void Condense(List<MutationSet> mutations)
        {
            mutations.Add(new MutationSet(this));
        }
    }
    public enum MutationTarget
    {
        /// <summary>
        /// Apply this mutation to the selector before it's been positionally aligned.
        /// </summary>
        PreSelector,
        /// <summary>
        /// Apply this mutation to the selector after it's been positionally aligned to its target(s).
        /// </summary>
        PostSelector
    }
}
