using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorItem : SelectorTransformer
    {
        public string GetKeyword() => "ITEM";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            int? slot = null;
            int? data = null;
            ItemSlot? location = null;
            Range? quantity = null;

            if (tokens.NextIs<TokenRangeLiteral>())
                quantity = tokens.Next<TokenRangeLiteral>().range;
            else if (tokens.NextIs<TokenIntegerLiteral>())
                quantity = new Range(tokens.Next<TokenIntegerLiteral>().number, false);

            string item = tokens.Next<TokenStringLiteral>();

            if (tokens.NextIs<TokenIdentifierEnum>())
            {
                ParsedEnumValue enumValue = tokens.Next<TokenIdentifierEnum>().value;
                if (!enumValue.IsType<ItemSlot>())
                    throw new StatementException(tokens, $"Excpected an ItemSlot, got {enumValue.enumName}.");
                location = (ItemSlot)enumValue.value;
            }

            if (tokens.NextIs<TokenIntegerLiteral>())
                slot = tokens.Next<TokenIntegerLiteral>().number;
            if (tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>().number;

            HasItemEntry entry = new HasItemEntry()
            {
                item = item,
                slot = slot,
                data = data,
                location = location,
                quantity = quantity
            };

            if (inverted)
            {
                HasItems invertCondition = new HasItems(entry);
                string entity = executor.ActiveSelectorCore;
                ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                commands.AddRange(new[] {
                    Command.ScoreboardSet(entity, inverter, 0),
                    invertCondition.AsStoreIn(entity, inverter)
                });
                selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
            } else
                selector.hasItem.entries.Add(entry);
        }
    }
}
