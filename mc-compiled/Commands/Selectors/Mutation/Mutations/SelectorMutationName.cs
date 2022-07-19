using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationName : SelectorMutation
    {
        readonly string name;
        public SelectorMutationName(bool invert, string name) : base(invert, MutationTarget.PostSelector)
        {
            this.name = name;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationName(Invert, name);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if (Invert)
            {
                if (name == null)
                {
                    selector.entity.name = null;
                    return;
                }

                string inverseName = Command.UTIL.ToggleInversion(name);
                selector.entity.name = inverseName;
            }
            else
            {
                selector.entity.name = name;
            }
        }
    }
}
