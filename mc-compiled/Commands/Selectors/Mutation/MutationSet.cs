using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation
{
    /// <summary>
    /// A list of mutations for one line.
    /// </summary>
    public class MutationSet : List<SelectorMutation>
    {
        public MutationSet() : base() { }
        public MutationSet(SelectorMutation single) : base(1)
        {
            Add(single);
        }
        public MutationSet(IEnumerable<SelectorMutation> objects) : base(objects) { }

        /// <summary>
        /// Make a deep copy of this mutation set.
        /// </summary>
        /// <returns></returns>
        public MutationSet Clone()
        {
            return new MutationSet(this.Select(mutation => mutation.Clone()));
        }
    }
}
