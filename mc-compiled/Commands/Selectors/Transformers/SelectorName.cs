using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorName : SelectorTransformer
    {
        public string GetKeyword() => "NAME";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            string name = tokens.Next<TokenStringLiteral>();

            if(name.StartsWith("!"))
            {
                inverted = !inverted;
                name = name.Substring(1);
            }

            selector.entity.nameNot = inverted;
            selector.entity.name = name;
        }
    }
}
