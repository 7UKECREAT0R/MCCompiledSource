using mc_compiled.Commands.Selectors.Mutation;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorScoreOperation : MutationProvider
    {
        public string GetKeyword() => null; // hardcoded
        public bool CanBeInverted() => true;

        public SelectorMutation[] GetMutations(bool inverted, Executor executor, Statement tokens, TokenIdentifierValue a)
        {
            /*string entity = executor.ActiveSelectorStr;

            // if <boolean> {}
            if (a.value is ScoreboardValueBoolean && !tokens.NextIs<TokenCompare>())
            {
                alignedSelector.scores.checks.Add(new ScoresEntry(a.value, new Range(1, inverted)));
            }

            // if <value> <comp> <other>
            else if (tokens.NextIs<TokenCompare>())
            {
                TokenCompare compare = tokens.Next<TokenCompare>();
                TokenCompare.Type ctype = compare.GetCompareType();

                // invert the type (bad code on their part tbh)
                if (inverted)
                    ctype = SelectorUtils.InvertComparison(ctype);

                // if <value> <comp> identifier
                if (tokens.NextIs<TokenIdentifierValue>())
                {
                    TokenIdentifierValue b = tokens.Next<TokenIdentifierValue>();
                    ScoreboardValue temp = executor.scoreboard.RequestTemp(a.value);
                    commands.AddRange(temp.CommandsSet(entity, a.value, a.word, b.word));
                    commands.AddRange(temp.CommandsSub(entity, b.value, a.word, b.word));
                    Range check;

                    switch (ctype)
                    {
                        case TokenCompare.Type.EQUAL:
                            check = new Range(0, false);
                            break;
                        case TokenCompare.Type.NOT_EQUAL:
                            check = new Range(0, true);
                            break;
                        case TokenCompare.Type.LESS_THAN:
                            check = new Range(null, -1);
                            break;
                        case TokenCompare.Type.LESS_OR_EQUAL:
                            check = new Range(null, 0);
                            break;
                        case TokenCompare.Type.GREATER_THAN:
                            check = new Range(1, null);
                            break;
                        case TokenCompare.Type.GREATER_OR_EQUAL:
                            check = new Range(0, null);
                            break;
                        default:
                            check = new Range();
                            break;
                    }
                    alignedSelector.scores.checks.Add(new ScoresEntry(temp, check));
                    // if <value> <comp> 1234.5
                }
                else if (tokens.NextIs<TokenNumberLiteral>())
                {
                    TokenNumberLiteral number = tokens.Next<TokenNumberLiteral>();
                    var output = a.value.CompareToLiteral(a.word, entity, ctype, number);
                    alignedSelector.scores.checks.AddRange(output.Item1);
                    commands.AddRange(output.Item2);
                }
                else
                {
                    executor.PopSelector();
                    throw new StatementException(tokens, "Attempted to compare value with invalid token.");
                }
            }*/
            return null; // unfinished to this day
        }

        // this shouldnt ever get called if everything is working okay

        public SelectorMutation[] GetMutations(bool inverted, Executor executor, Statement tokens)
        {
            throw new NotImplementedException();
        }
    }
}
