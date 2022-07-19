using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    class SelectorMutationArea : SelectorMutation
    {
        readonly Area area;

        public SelectorMutationArea(bool invert, Area area) : base(invert, MutationTarget.PreSelector)
        {
            this.area = area;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationArea(Invert, area);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if (Invert)
            {
                SelectorUtils.InvertSelector(ref selector,
                    commands, executor, (sel) =>
                    {
                        sel.area = area;
                    });
            }
            else
                selector.area = area;
        }
    }
}
