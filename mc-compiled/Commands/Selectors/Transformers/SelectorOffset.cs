using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorOffset : SelectorTransformer
    {
        public string GetKeyword() => "OFFSET";
        public bool CanBeInverted() => false;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            selector.offsetX = tokens.Next<TokenCoordinateLiteral>();
            selector.offsetY = tokens.Next<TokenCoordinateLiteral>();
            selector.offsetZ = tokens.Next<TokenCoordinateLiteral>();
        }
    }
}
