using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.Implementations;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes.Implementations
{
    public static class AttributeFunctions
    {
        internal static readonly IFunctionProvider PROVIDER = new AttributeProvider();

        private class AttributeProvider : IFunctionProvider
        {
            IEnumerable<Function> IFunctionProvider.ProvideFunctions(ScoreboardManager manager)
            {
                return ALL_ATTRIBUTES;
            }
        }

        public static readonly AttributeFunction AUTO = new AttributeFunction("auto", "auto",
            "Makes a function run every tick (via tick.json), or if specified, some other interval.")
            .AddParameter(new CompiletimeFunctionParameter<TokenIntegerLiteral>("interval", new TokenIntegerLiteral(0, IntMultiplier.none, 0)))
            .WithCallAction((parameters, executor, statement) =>
            {
                int interval = parameters[0].CurrentValue as TokenIntegerLiteral;
                return new AttributeAuto(interval);
            });
        
        /// <summary>
        /// Makes the attached value global.
        /// </summary>
        public static readonly AttributeFunction GLOBAL = new AttributeFunction("global", "global",
"Makes a value global, meaning it will only be accessed in the context of the global fakeplayer, `" + Executor.FAKEPLAYER_NAME + "`")
           .WithCallAction((parameters, executor, statement) => new AttributeGlobal());

        /// <summary>
        /// Makes the attached function extern.
        /// </summary>
        public static readonly AttributeFunction EXTERN = new AttributeFunction("extern", "extern",
"Makes a function extern, meaning it was written outside of MCCompiled and can now be called as any other function.")
           .WithCallAction((parameters, executor, statement) => new AttributeExtern());

        /// <summary>
        /// Makes the attached function partial.
        /// </summary>
        public static readonly AttributeFunction PARTIAL = new AttributeFunction("partial", "partial",
                "Makes a function partial, allowing it to be re-defined , appending to any previous code in it. When re-declaring a function, the partial attribute must be used in both.")
            .WithCallAction((parameters, executor, statement) => new AttributePartial());
        
        /// <summary>
        /// Makes the attached function export always, even if unused.
        /// </summary>
        public static readonly AttributeFunction EXPORT = new AttributeFunction("export", "export",
                "Marks a function for export, meaning it will be outputted regardless of if it is used or not.")
            .WithCallAction((parameters, executor, statement) => new AttributeExport());
        
        /// <summary>
        /// Binds the attached value to a MoLang query using animation controllers.
        /// </summary>
        public static readonly AttributeFunction BIND = new AttributeFunction("bind", "bind",
"Binds a value to a pre-defined MoLang query. See bindings.json.")
            .AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("query"))
            .WithCallAction((parameters, executor, statement) =>
            {
                string queryString = parameters[0].CurrentValue as TokenStringLiteral;
                var givenTargets = new List<string>();

                _ = statement.Next(); // skip 'bind' identifier
                _ = statement.Next(); // skip (
                _ = statement.Next(); // skip query string

                while (statement.NextIs<TokenStringLiteral>())
                {
                    string target = statement.Next<TokenStringLiteral>("target");

                    int colon = target.IndexOf(':');

                    if (colon != -1) // get rid of namespace
                        target = target.Substring(colon + 1);

                    if (!target.EndsWith(".json")) // force .json at the end
                        target += ".json";

                    givenTargets.Add(target);
                }

                MolangBindings.EnsureLoaded(Program.DEBUG);

                if (!MolangBindings.BINDINGS.TryGetValue(queryString, out MolangBinding binding))
                    throw new StatementException(statement, $"No binding could be found under the name '{queryString}'.");

                if(givenTargets.Count > 0)
                    return new AttributeBind(binding, givenTargets.ToArray());
                return new AttributeBind(binding, null);
            });

        /// <summary>
        /// Keep at the bottom of the class because of static ordering.
        /// </summary>
        internal static readonly AttributeFunction[] ALL_ATTRIBUTES = {
            GLOBAL,
            EXTERN,
            EXPORT,
            BIND,
            AUTO,
            PARTIAL
        };
    }
}
