using mc_compiled.Commands.Selectors.Mutation;
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
        public static readonly Dictionary<string, MutationProvider> TRANSFORMERS = new Dictionary<string, MutationProvider>()
        {
            { "ANY", new SelectorAny() },
            { "BLOCK", new SelectorBlock() },
            { "CLASS", new SelectorClass() },
            { "COUNT", new SelectorCount() },
            { "FAMILY", new SelectorFamily() },
            { "HOLDING", new SelectorHolding() },
            { "INSIDE", new SelectorInside() },
            { "ITEM", new SelectorItem() },
            { "LEVEL", new SelectorLevel() },
            { "MODE", new SelectorMode() },
            { "NAME", new SelectorName() },
            { "NEAR", new SelectorNear() },
            { "NULL", new SelectorNull() },
            { "OFFSET", new SelectorOffset() },
            { "POSITION", new SelectorPosition() },
            { "ROTATION", new SelectorRotation() },
            { "TAG", new SelectorTag() },
            { "TYPE", new SelectorType() }
        };
        /// <summary>
        /// Resolve all of the necessary sets of mutations based off of a 
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="commands"></param>
        /// <param name="tokens"></param>
        /// <param name="elseStatement"></param>
        /// <returns></returns>
        public static List<MutationSet> ResolveAllMutations(Executor executor, List<String> commands, Statement tokens, bool elseStatement)
        {
            const int GROUP_SINGLE = 0;
            const int GROUP_AND = 1;
            const int GROUP_OR = 2;


            // buffer to hold groups of conditions
            Stack<ISelectorMutator> buffer = new Stack<ISelectorMutator>();
            List<MutationSet> sets = new List<MutationSet>();
            int group = GROUP_SINGLE;

            bool invertNext = false;

            if (!tokens.HasNext)
                throw new StatementException(tokens, "Got empty statement while trying to fetch mutations.");

            do
            {
                bool invert = elseStatement;

                if(invertNext)
                {
                    invert = !invert;
                    invertNext = false;
                }

                Token token = tokens.Next();

            }

            /*executor.scoreboard.PushTempState();

            do
            {
                if (tokens.NextIs<TokenContinue>())
                    tokens.Next();

                bool invert = elseStatement;
                bool isScore = tokens.NextIs<TokenIdentifierValue>();

                if (tokens.NextIs<TokenNot>())
                {
                    invert = !elseStatement;
                    isScore = tokens.NextIs<TokenIdentifierValue>();
                }

                TokenIdentifier currentToken = tokens.Next<TokenIdentifier>();
                string word = currentToken.word.ToUpper();

                // Scoreboard operation.
                if (isScore)
                {
                    TokenIdentifierValue score = currentToken as TokenIdentifierValue;
                    SelectorMutation[] mutations = SCORE_OPERATION.GetMutations(invert, executor, tokens, score);
                    continue;
                }

                // Other kind of operation.
                if(TRANSFORMERS.TryGetValue(word, out MutationProvider transformer))
                {
                    if (invert && !transformer.CanBeInverted())
                        throw new StatementException(tokens, $"Operation {transformer.GetKeyword()} cannot be inverted.");

                    SelectorMutation[] mutations = transformer.GetMutations(invert, executor, tokens);
                    continue;
                }

                throw new StatementException(tokens, $"Unknown selector transformer: {word}");

            } while (tokens.NextIs<TokenAnd>());

            // done with temporary variables
            executor.scoreboard.PopTempState();
            return;*/
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
        /// Tries to fetch a <see cref="MutationProvider"/> from a specific keyword.
        /// </summary>
        /// <param name="keyword">The keyword to search for.</param>
        /// <param name="transformer">The transformer to feed tokens into.</param>
        /// <returns></returns>
        public static bool TryGetTransformer(string keyword,
            out MutationProvider transformer)
        {
            return TRANSFORMERS.TryGetValue(keyword.ToUpper(), out transformer);
        }
        /// <summary>
        /// Get a <see cref="MutationProvider"/> instance from its keyword.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static MutationProvider GetTransformer(string keyword)
        {
            if (TRANSFORMERS.TryGetValue(keyword.ToUpper(), out MutationProvider result))
                return result;

            return null;
        }
    }
}
