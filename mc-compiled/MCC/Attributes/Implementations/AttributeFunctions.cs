using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.Async;
using mc_compiled.MCC.Compiler.Implementations;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;

// Disabling this because I'd like to keep the names of the lambda parameters -lukecreator
// ReSharper disable UnusedParameter.Local

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

        private static readonly AttributeFunction AUTO = new AttributeFunction("auto", "auto",
            "Makes a function run every tick (via tick.json), or if specified, some other interval.")
            .AddParameter(new CompiletimeFunctionParameter<TokenIntegerLiteral>("interval", new TokenIntegerLiteral(0, IntMultiplier.none, 0)))
            .WithCallAction((parameters, tokens, executor, statement) =>
            {
                int interval = parameters[0].CurrentValue as TokenIntegerLiteral;
                return new AttributeAuto(interval);
            });
        
        /// <summary>
        /// Makes the attached value global. Dually used as a parameter in the async attribute.
        /// </summary>
        private static readonly AttributeFunction GLOBAL = new AttributeFunction("global", "global",
"Makes a value global, never being assigned on an entity. Alternately used as a parameter for the 'async' attribute.")
           .WithCallAction((parameters, tokens, executor, statement) => new AttributeGlobal());
        
        /// <summary>
        /// Makes the attached value global. Dually used as a parameter in the async attribute.
        /// </summary>
        private static readonly AttributeFunction LOCAL = new AttributeFunction("local", "local",
                "Makes a value local (default). Alternately used as a parameter for the 'async' attribute.")
            .WithCallAction((parameters, tokens, executor, statement) => new AttributeLocal());
        
        /// <summary>
        /// Makes the attached function extern.
        /// </summary>
        private static readonly AttributeFunction EXTERN = new AttributeFunction("extern", "extern",
"Makes a function extern, meaning it was written outside of MCCompiled and can now be called as any other function.")
           .WithCallAction((parameters, tokens, executor, statement) => new AttributeExtern());

        /// <summary>
        /// Makes the attached function partial.
        /// </summary>
        private static readonly AttributeFunction PARTIAL = new AttributeFunction("partial", "partial",
                "Makes a function partial, allowing it to be re-defined , appending to any previous code in it. When re-declaring a function, the partial attribute must be used in both.")
            .WithCallAction((parameters, tokens, executor, statement) => new AttributePartial());
        
        /// <summary>
        /// Makes the attached function export always, even if unused.
        /// </summary>
        private static readonly AttributeFunction EXPORT = new AttributeFunction("export", "export",
                "Marks a function for export, meaning it will be outputted regardless of if it is used or not.")
            .WithCallAction((parameters, tokens, executor, statement) => new AttributeExport());
        
        /// <summary>
        /// Binds the attached value to a MoLang query using animation controllers.
        /// </summary>
        private static readonly AttributeFunction BIND = new AttributeFunction("bind", "bind",
"Binds a value to a pre-defined MoLang query. See bindings.json.")
            .AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("query"))
            .WithCallAction((parameters, tokens, executor, statement) =>
            {
                string queryString = parameters[0].CurrentValue as TokenStringLiteral;
                var givenTargets = new List<string>();

                var feeder = new TokenFeeder(tokens);
                _ = feeder.Next(); // skip the query string
                
                while (feeder.NextIs<TokenStringLiteral>(true))
                {
                    string target = feeder.Next<TokenStringLiteral>("target");

                    int colon = target.IndexOf(':');

                    if (colon != -1) // get rid of namespace
                        target = target[(colon + 1)..];

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

        private static readonly AttributeFunction ASYNC = new AttributeFunction("async", "async",
                "Makes the given function asynchronous, either locally (state is attached to an entity) or globally (state is global only).")
            .AddParameter(new CompiletimeFunctionParameter<TokenAttribute>("local/global"))
            .WithCallAction((parameters, tokens, Executor, statement) =>
            {
                var passedInAttribute = (TokenAttribute)parameters[0].CurrentValue;
                bool isLocal = passedInAttribute.attribute is AttributeLocal;
                bool isGlobal = passedInAttribute.attribute is AttributeGlobal;

                return isLocal switch
                {
                    false when !isGlobal => throw new StatementException(statement,
                        "Parameter for 'async' must be either 'local' or 'global'."),
                    true => new AttributeAsync(AsyncTarget.Local),
                    _ => new AttributeAsync(AsyncTarget.Global)
                };
            });

        /// <summary>
        /// Keep at the bottom of the class because of static ordering.
        /// </summary>
        internal static readonly AttributeFunction[] ALL_ATTRIBUTES =
        [
            GLOBAL,
            LOCAL,
            EXTERN,
            EXPORT,
            BIND,
            AUTO,
            PARTIAL,
            ASYNC
        ];
    }
}
