using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationRotationX : SelectorMutation
    {
        readonly int? min, max;

        public SelectorMutationRotationX(bool invert, int? min, int? max) : base(invert, MutationTarget.PostSelector)
        {
            this.min = min;
            this.max = max;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationRotationX(Invert, min, max);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if (Invert)
            {
                SelectorUtils.InvertSelector(ref selector,
                    commands, executor, (sel) =>
                    {
                        if(min.HasValue)
                            sel.entity.rotXMin = min;
                        if(max.HasValue)
                            sel.entity.rotXMax = max;
                    });
            }
            else
            {
                if(min.HasValue)
                    selector.entity.rotXMin = min;
                if(max.HasValue)
                    selector.entity.rotXMax = max;
            }
        }
    }
}
