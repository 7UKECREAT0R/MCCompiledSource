using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationBlock : SelectorMutation
    {
        readonly BlockCheck blockCheck;
        public SelectorMutationBlock(bool invert, BlockCheck blockCheck) : base(invert, MutationTarget.PreSelector)
        {
            this.blockCheck = blockCheck;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationBlock(Invert, blockCheck);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if(Invert)
            {
                SelectorUtils.InvertSelector(ref selector,
                    commands, executor, (sel) =>
                    {
                        sel.blockCheck = blockCheck;
                    });
            } else
            {
                selector.blockCheck = blockCheck;
            }
            
        }
    }
}
