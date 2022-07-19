using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationOffset : SelectorMutation
    {
        readonly Coord x, y, z;

        public SelectorMutationOffset(bool invert, Coord x, Coord y, Coord z) : base(invert, MutationTarget.PreSelector)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationOffset(Invert, x, y, z);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            selector.offsetX = x;
            selector.offsetY = y;
            selector.offsetZ = z;
        }
    }
}
