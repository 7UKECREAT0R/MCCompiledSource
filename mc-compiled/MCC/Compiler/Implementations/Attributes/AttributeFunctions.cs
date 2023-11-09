﻿using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.Implementations.Attributes
{
    public static class AttributeFunctions
    {
        internal static readonly IFunctionProvider PROVIDER = new AttributeProvider();

        internal class AttributeProvider : IFunctionProvider
        {
            IEnumerable<Function> IFunctionProvider.ProvideFunctions(ScoreboardManager manager)
            {
                return ALL_ATTRIBUTES;
            }
        }

        /// <summary>
        /// Makes the attached value global.
        /// </summary>
        public static readonly AttributeFunction GLOBAL = new AttributeFunction("global", "global",
"Makes a value global, meaning it will only be accessed in the context of the global fakeplayer, '" + Executor.FAKEPLAYER_NAME + "'.")
           .WithCallAction((parameters, excecutor, statement) =>
           {
               return new AttributeGlobal();
           });

        /// <summary>
        /// Makes the attached function extern.
        /// </summary>
        public static readonly AttributeFunction EXTERN = new AttributeFunction("extern", "extern",
"Makes a function extern, meaning it was written outside of MCCompiled and can now be called as any other function.")
           .WithCallAction((parameters, excecutor, statement) =>
           {
               return new AttributeExtern();
           });

        /// <summary>
        /// Binds the attached value to a MoLang query using animation controllers.
        /// </summary>
        public static readonly AttributeFunction BIND = new AttributeFunction("bind", "bind",
"Binds a value to a pre-defined MoLang query. See bindings.json.")
            .AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("query"))
            .WithCallAction((parameters, executor, statement) =>
            {
                string queryString = parameters[0].CurrentValue as TokenStringLiteral;

                List<string> givenTargets = new List<string>();

                _ = statement.Next(); // skip 'bind' identifier
                _ = statement.Next(); // skip (
                _ = statement.Next(); // skip query string

                while (statement.NextIs<TokenStringLiteral>())
                {
                    string target = statement.Next<TokenStringLiteral>();

                    int colon = target.IndexOf(':');

                    if (colon != -1) // get rid of namespace
                        target = target.Substring(colon + 1);

                    if (!target.EndsWith(".json")) // force .json at the end
                        target += ".json";

                    givenTargets.Add(target);
                }

                MolangBindings.EnsureLoaded(Program.DEBUG);

                if (!MolangBindings.BINDINGS.TryGetValue(queryString, out var binding))
                    throw new StatementException(statement, $"No binding could be found under the name '{queryString}'.");

                if(givenTargets.Count > 0)
                    return new AttributeBind(binding, givenTargets.ToArray());
                else
                    return new AttributeBind(binding, null);
            });

        /// <summary>
        /// Keep at the bottom of the class because it's having weird order problems.
        /// </summary>
        internal static readonly AttributeFunction[] ALL_ATTRIBUTES = new AttributeFunction[]
        {
            GLOBAL,
            EXTERN,
            BIND
        };
    }
}
