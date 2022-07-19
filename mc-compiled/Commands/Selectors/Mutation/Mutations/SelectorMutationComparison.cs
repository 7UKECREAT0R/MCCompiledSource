using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Mutation.Mutations
{
    public class SelectorMutationComparison : SelectorMutation
    {
        ScoreboardValue a;
        TokenCompare.Type compare;
        Token b;

        public SelectorMutationComparison(bool invert, ScoreboardValue a, TokenCompare.Type compare, Token b) : base(invert, MutationTarget.PostSelector)
        {
            this.a = a;
            this.compare = compare;
            this.b = b;
        }

        public override SelectorMutation Clone()
        {
            return new SelectorMutationComparison(Invert, a, compare, b);
        }
        public override void Mutate(Executor executor, List<string> commands, Selector selector)
        {
            TokenCompare.Type compare = this.compare;

            if (Invert)
                compare = SelectorUtils.InvertComparison(compare);

            // if <boolean> {}
            if (b == null)
            {
                if (a is ScoreboardValueBoolean)
                {
                    selector.scores.checks.Add(new ScoresEntry(a, new Range(1, Invert)));
                    return;
                }

                throw new StatementException(executor.PeekLast(), "Value " + a.AliasName + " was used like a boolean.");
            }


            string entity = executor.ActiveSelectorStr;

            // if <value> <comp> <othervalue>
            if (b is TokenIdentifierValue)
            {
                TokenIdentifierValue b = this.b as TokenIdentifierValue;
                ScoreboardValue temp = executor.scoreboard.RequestTemp(a);
                commands.AddRange(temp.CommandsSet(entity, a, a.AliasName, b.word));
                commands.AddRange(temp.CommandsSub(entity, b.value, a.AliasName, b.word));
                Range check;

                switch (compare)
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

                selector.scores.checks.Add(new ScoresEntry(temp, check));
                return;
            }

            if (b is TokenNumberLiteral)
            {
                TokenNumberLiteral number = b as TokenNumberLiteral;
                var output = a.CompareToLiteral(a.AliasName, entity, compare, number);
                selector.scores.checks.AddRange(output.Item1);
                commands.AddRange(output.Item2);
                return;
            }

            throw new StatementException(executor.PeekLast(), "Attempted to compare value " + a.AliasName + " with invalid type: " + b.DebugString());
        }
    }
}
