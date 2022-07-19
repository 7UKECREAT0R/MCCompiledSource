using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    class SelectorMutationMode : SelectorMutation
    {
        readonly GameMode gameMode;

        public SelectorMutationMode(bool invert, GameMode gameMode) : base(invert, MutationTarget.PostSelector)
        {
            this.gameMode = gameMode;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationMode(Invert, gameMode);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            selector.player.gamemode = gameMode;
            selector.player.gamemodeNot = Invert;
        }
    }
}
