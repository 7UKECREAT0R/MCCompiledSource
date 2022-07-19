using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationRotationY : SelectorMutation
    {
        readonly int? min, max;

        public SelectorMutationRotationY(bool invert, int? min, int? max) : base(invert, MutationTarget.PostSelector)
        {
            this.min = min;
            this.max = max;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationRotationY(Invert, min, max);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if (Invert)
            {
                SelectorUtils.InvertSelector(ref selector,
                    commands, executor, (sel) =>
                    {
                        if(min.HasValue)
                            sel.entity.rotYMin = min;
                        if(max.HasValue)
                            sel.entity.rotYMax = max;
                    });
            }
            else
            {
                if(min.HasValue)
                    selector.entity.rotYMin = min;
                if(max.HasValue)
                    selector.entity.rotYMax = max;
            }
        }
    }
}
