using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationHasItem : SelectorMutation
    {
        readonly HasItemEntry entry;
        public SelectorMutationHasItem(bool invert, HasItemEntry entry) : base(invert, MutationTarget.PostSelector)
        {
            this.entry = entry;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationHasItem(Invert, entry);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if(Invert)
            {
                if (entry.IsBare)
                {
                    HasItemEntry temp = entry;
                    temp.quantity = new Range(0, false);
                    selector.hasItem.entries.Add(temp);
                    return;
                }

                SelectorUtils.InvertSelector(ref selector,
                    commands, executor, (sel) =>
                    {
                        sel.hasItem.entries.Add(entry);
                    });
            } else
            {
                selector.hasItem.entries.Add(entry);
            }
        }
    }
}
