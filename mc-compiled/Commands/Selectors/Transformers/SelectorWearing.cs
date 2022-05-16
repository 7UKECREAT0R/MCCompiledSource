using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorWearing : SelectorTransformer
    {
        public string GetKeyword() => "WEARING";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            string item = tokens.Next<TokenStringLiteral>();

            int? data = null;
            if (tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>().number;

            HasItemEntry entry = new HasItemEntry()
            {
                item = item,
                slot = null,
                data = data,
                location = ItemSlot.slot_armor,
                quantity = null
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
            }
            else
                selector.hasItem.entries.Add(entry);
        }
    }
}
