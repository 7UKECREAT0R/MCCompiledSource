using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorItem : MutationProvider
    {
        public string GetKeyword() => "ITEM";
        public bool CanBeInverted() => true;

        public void GetMutations(bool inverted, Executor executor, Statement tokens)
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
                if(!location.HasValue && !slot.HasValue && !data.HasValue && !quantity.HasValue)
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
            } else
                alignedSelector.hasItem.entries.Add(entry);
        }
    }
}
