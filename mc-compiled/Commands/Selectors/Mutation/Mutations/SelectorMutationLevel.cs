using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationLevel : SelectorMutation
    {
        readonly int? levelMin, levelMax;

        public SelectorMutationLevel(bool invert, int? levelMin, int? levelMax) : base(invert, MutationTarget.PostSelector)
        {
            this.levelMin = levelMin;
            this.levelMax = levelMax;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationLevel(Invert, levelMin, levelMax);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            if (Invert)
            {
                SelectorUtils.InvertSelector(ref selector,
                    commands, executor, (sel) =>
                    {
                        if (levelMin.HasValue)
                            sel.player.levelMin = levelMin;
                        if (levelMax.HasValue)
                            sel.player.levelMax = levelMax;
                    });
            }
            else
            {
                if (levelMin.HasValue)
                    selector.player.levelMin = levelMin;
                if (levelMax.HasValue)
                    selector.player.levelMax = levelMax;
            }
        }
    }
}
