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

        public void Transform(ref LegacySelector rootSelector, ref LegacySelector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            rootSelector.offsetX = tokens.Next<TokenCoordinateLiteral>();
            rootSelector.offsetY = tokens.Next<TokenCoordinateLiteral>();
            rootSelector.offsetZ = tokens.Next<TokenCoordinateLiteral>();
        }
    }
}
