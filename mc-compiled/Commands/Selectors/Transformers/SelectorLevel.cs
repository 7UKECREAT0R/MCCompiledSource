using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorLevel : SelectorTransformer
    {
        public string GetKeyword() => "LEVEL";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            int levelMin = tokens.Next<TokenIntegerLiteral>();
            int? levelMax;

            if (tokens.NextIs<TokenIntegerLiteral>())
                levelMax = tokens.Next<TokenIntegerLiteral>();
            else
                levelMax = null;

            if (inverted && levelMax == null)
            {
                alignedSelector.player.levelMin = 0;
                alignedSelector.player.levelMax = levelMin;
            }
            else if (inverted)
            {
                SelectorUtils.InvertSelector(ref alignedSelector,
                    commands, executor, (sel) =>
                    {
                        sel.player.levelMin = 0;
                        sel.player.levelMax = levelMin;
                    });
            }
            else
            {
                alignedSelector.player.levelMin = levelMin;
                alignedSelector.player.levelMax = levelMax;
            }
        }
    }
}
