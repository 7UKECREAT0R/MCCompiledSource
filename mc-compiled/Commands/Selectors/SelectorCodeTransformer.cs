using mc_compiled.Commands.Selectors.Transformers;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Provides a way of using keywords to transform a selector.
    /// It is stateless and only serves to act as a medium between runtime and implementation.
    /// <br />
    /// <br />
    /// The goal of the IfStatementProcessor is to standardize the processing of selector transformations
    /// across the language. It handles inversion and combination of selectors, as well as the directing
    /// of tokens to the right transformer. This was previously achieved by hardcoding into the if-statement
    /// implementation, but in light of upcoming changes and code housekeeping, this system will be more
    /// sustainable for the future.
    /// </summary>
    public static class SelectorCodeTransformer
    {
        internal static readonly SelectorScoreOperation SCORE_OPERATION = new SelectorScoreOperation();
        /// <summary>
        /// Dictionary of implemented selector transformers.
        /// </summary>
        public static readonly Dictionary<string, SelectorTransformer> TRANSFORMERS = new Dictionary<string, SelectorTransformer>()
        {
            { "ANY", new SelectorAny() },
            { "BLOCK", new SelectorBlock() },
            { "COUNT", new SelectorCount() },
            { "FAMILY", new SelectorFamily() },
            { "HOLDING", new SelectorHolding() },
            { "INSIDE", new SelectorInside() },
            { "ITEM", new SelectorItem() },
            { "LEVEL", new SelectorLevel() },
            { "LIMIT", new SelectorLimit() },
            { "MODE", new SelectorMode() },
            { "NAME", new SelectorName() },
            { "NEAR", new SelectorNear() },
            { "OFFSET", new SelectorOffset() },
            { "ROTATION", new SelectorRotation() },
            { "TAG", new SelectorTag() },
            { "TYPE", new SelectorType() },
            { "WEARING", new SelectorWearing() }
        };
        /// <summary>
        /// Read the next selector transformer(s) out of this set of tokens.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="executor"></param>
        /// <param name="tokens"></param>
        /// <param name="forceInvert"></param>
        /// <returns></returns>
        public static void TransformSelector(ref Selector input, Executor executor, Statement tokens, bool forceInvert)
        {
            executor.scoreboard.PushTempState();
            List<string> commands = new List<string>();

            do
            {
                if (tokens.NextIs<TokenAnd>())
                    tokens.Next();

                bool invert = forceInvert;
                bool isScore = tokens.NextIs<TokenIdentifierValue>();
                TokenIdentifier currentToken = tokens.Next<TokenIdentifier>();
                string word = currentToken.word.ToUpper();

                if(word.Equals("NOT"))
                {
                    invert = !forceInvert;
                    isScore = tokens.NextIs<TokenIdentifierValue>();
                    currentToken = tokens.Next<TokenIdentifier>();
                    word = currentToken.word.ToUpper();
                }

                // Scoreboard operation.
                if (isScore)
                {
                    TokenIdentifierValue score = currentToken as TokenIdentifierValue;
                    SCORE_OPERATION.Transform(ref input, invert, executor, tokens, commands, score);
                    continue;
                }

                // Other kind of operation.
                if(TRANSFORMERS.TryGetValue(word, out SelectorTransformer transformer))
                {
                    if (invert && !transformer.CanBeInverted())
                        throw new StatementException(tokens, $"Operation {transformer.GetKeyword()} cannot be inverted.");

                    transformer.Transform(ref input, invert, executor, tokens, commands);
                    continue;
                }

            } while (tokens.NextIs<TokenAnd>());

            if (commands.Count > 0)
                executor.AddCommandsClean(commands, "selector_setup", true);

            // done with temporary variables
            executor.scoreboard.PopTempState();
            return;
        }

        /// <summary>
        /// Get the complete list of implemented selector keywords.
        /// </summary>
        public static string[] KeywordList
        {
            get => TRANSFORMERS.Keys.ToArray();
        }

        /// <summary>
        /// Check if a keyword has an implementation.
        /// </summary>
        /// <param name="keyword">The keyword to search for.</param>
        /// <returns></returns>
        public static bool CheckKeyword(string keyword)
        {
            return TRANSFORMERS.ContainsKey(keyword.ToUpper());
        }
        /// <summary>
        /// Tries to fetch a <see cref="SelectorTransformer"/> from a specific keyword.
        /// </summary>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="transformer">The transformer to feed tokens into.</param>
        /// <returns></returns>
        public static bool TryGetTransformer(string keyword,
            out SelectorTransformer transformer)
        {
            return TRANSFORMERS.TryGetValue(keyword.ToUpper(), out transformer);
        }
        /// <summary>
        /// Get a <see cref="SelectorTransformer"/> instance from its keyword.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static SelectorTransformer GetTransformer(string keyword)
        {
            if (TRANSFORMERS.TryGetValue(keyword.ToUpper(), out SelectorTransformer result))
                return result;

            return null;
        }
    }
}
