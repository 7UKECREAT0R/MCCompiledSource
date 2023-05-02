using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Places a function in a specific folder.
        /// </summary>
        public static readonly AttributeFunction FOLDER = new AttributeFunction("folder", "folder",
"When attached to a function, the file will be placed in the given folder path rather than the behavior_pack/functions root.")
            .AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("path"))
            .WithCallAction((parameters, excecutor, statement) =>
            {
                string folderPath = parameters[0].CurrentValue as TokenStringLiteral;
                return new AttributeFolder(folderPath);
            });

        /// <summary>
        /// Binds the attached value to a MoLang query using animation controllers.
        /// </summary>
        public static readonly AttributeFunction BIND = new AttributeFunction("bind", "bind",
"Binds a value to a pre-defined MoLang query.")
            .AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("query"))
            .WithCallAction((parameters, executor, statement) =>
            {
                string query = parameters[0].CurrentValue as TokenStringLiteral;
                return new AttributeBind(query);
            });


        /// <summary>
        /// Keep at the bottom of the class because it's having weird order problems.
        /// </summary>
        internal static readonly AttributeFunction[] ALL_ATTRIBUTES = new AttributeFunction[]
        {
            GLOBAL,
            FOLDER,
            BIND
        };
    }
}
