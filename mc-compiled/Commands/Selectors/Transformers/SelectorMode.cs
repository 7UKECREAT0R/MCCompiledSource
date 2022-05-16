using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorMode : SelectorTransformer
    {
        public string GetKeyword() => "MODE";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            GameMode gameMode;

            if (tokens.NextIs<TokenIdentifierEnum>())
            {
                ParsedEnumValue enumValue = tokens.Next<TokenIdentifierEnum>().value;
                if (!enumValue.IsType<GameMode>())
                    throw new StatementException(tokens, $"Must specify GameMode; Given {enumValue.enumName}.");
                gameMode = (GameMode)enumValue.value;
            }
            else
                gameMode = (GameMode)tokens.Next<TokenIntegerLiteral>().number;

            selector.player.gamemode = gameMode;
            selector.player.gamemodeNot = inverted;
        }
    }
}
