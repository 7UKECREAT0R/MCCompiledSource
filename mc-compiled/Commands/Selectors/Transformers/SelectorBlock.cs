using mc_compiled.Commands.Selectors.Mutation;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorBlock : MutationProvider
    {
        public string GetKeyword() => "BLOCK";
        public bool CanBeInverted() => true;
        
        public SelectorMutation[] GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            /*
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();
            string block = tokens.Next<TokenStringLiteral>();
            int? data = null;

            if (tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();

            BlockCheck blockCheck = new BlockCheck(x, y, z, block, data);

            if (inverted)
            {
                SelectorUtils.InvertSelector(ref alignedSelector,
                    commands, executor, (sel) =>
                    {
                        sel.blockCheck = blockCheck;
                    });
            }
            else
                alignedSelector.blockCheck = blockCheck;
            */

            return null; // unfinished
            // it is just that shrimple
        }
    }
}
