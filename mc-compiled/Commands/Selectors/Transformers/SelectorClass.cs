using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Transformers
{
    internal sealed class SelectorClass : SelectorTransformer
    {
        public string GetKeyword() => "CLASS";
        public bool CanBeInverted() => true;

        public void Transform(ref Selector rootSelector, ref Selector alignedSelector, bool inverted, Executor executor, Statement tokens, List<string> commands)
        {
            executor.RequireFeature(tokens, MCC.Feature.NULLS);

            string clazz = tokens.Next<TokenStringLiteral>();
            string family = MCC.CustomEntities.NullManager.FamilyName(clazz);

            // NullManager.FamilyName(...) always returns non-inverted name
            if (inverted)
                family = '!' + family;

            alignedSelector.entity.families.Add(family);
            return;
        }
    }
}
