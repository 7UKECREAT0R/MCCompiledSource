using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorHolding : SelectorTransformer
    {
        public string GetKeyword() => "HOLDING";
        public bool CanBeInverted() => true;

        public void Transform(ref LegacySelector rootSelector, ref LegacySelector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            int? data = null;
            Range? quantity = null;

            if (tokens.NextIs<TokenRangeLiteral>())
                quantity = tokens.Next<TokenRangeLiteral>().range;
            else if (tokens.NextIs<TokenIntegerLiteral>())
                quantity = new Range(tokens.Next<TokenIntegerLiteral>().number, false);

            string item = tokens.Next<TokenStringLiteral>();

            if (tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>().number;

            HasItemEntry entry = new HasItemEntry()
            {
                item = item,
                slot = 0,
                data = data,
                location = ItemSlot.slot_weapon_mainhand,
                quantity = quantity
            };

            if (inverted)
            {
                if (!data.HasValue && !quantity.HasValue)
                {
                    // very basic clause so checking quantity=0 will work fine
                    entry.quantity = new Range(0, false);
                    alignedSelector.hasItem.entries.Add(entry);
                    return;
                }

                SelectorUtils.InvertSelector(ref alignedSelector,
                    commands, executor, (sel) =>
                    {
                        sel.hasItem.entries.Clear();
                        sel.hasItem.entries.Add(entry);
                    });
            }
            else
                alignedSelector.hasItem.entries.Add(entry);
        }
    }
}
