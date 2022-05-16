using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorLimit : SelectorTransformer
    {
        public string GetKeyword() => "LIMIT";
        public bool CanBeInverted() => false;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            int count = tokens.Next<TokenIntegerLiteral>();
            selector.count = new Count(count);
        }
    }
}
