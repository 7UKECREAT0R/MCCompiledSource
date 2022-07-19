using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationFamily : SelectorMutation
    {
        readonly string[] families;
        public SelectorMutationFamily(bool invert, params string[] families) : base(invert, MutationTarget.PostSelector)
        {
            this.families = families;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationFamily(Invert, families.ToArray());
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if (Invert)
            {
                for (int i = 0; i < families.Length; i++)
                {
                    string family = Command.UTIL.ToggleInversion(families[i]);
                    selector.entity.families.Add(family);
                }
            }
            else
            {
                selector.entity.families.AddRange(families);
            }
        }
    }
}
