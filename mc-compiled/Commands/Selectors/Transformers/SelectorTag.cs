using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorTag : SelectorTransformer
    {
        public string GetKeyword() => "TAG";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            string tag = tokens.Next<TokenStringLiteral>();

            if (tag.StartsWith("!"))
            {
                inverted = !inverted;
                tag = tag.Substring(1);
            }

            selector.tags.Add(new Tag(tag, inverted));
        }
    }
}
