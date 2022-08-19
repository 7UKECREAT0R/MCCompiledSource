using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorOffset : MutationProvider
    {
        public string GetKeyword() => "OFFSET";
        public bool CanBeInverted() => false;

        public void GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            rootSelector.offsetX = tokens.Next<TokenCoordinateLiteral>();
            rootSelector.offsetY = tokens.Next<TokenCoordinateLiteral>();
            rootSelector.offsetZ = tokens.Next<TokenCoordinateLiteral>();
        }
    }
}
