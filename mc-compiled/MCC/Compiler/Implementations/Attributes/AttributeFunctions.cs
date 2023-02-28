using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
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
                return new[]
                {
                    GLOBAL,
                    FOLDER
                };
            }
        }


        /// <summary>
        /// Makes the attached value global.
        /// </summary>
        public static readonly AttributeFunction GLOBAL = new AttributeFunction("global", "global")
           .WithCallAction((parameters, excecutor, statement) =>
           {
               return new AttributeGlobal();
           });

        /// <summary>
        /// Places a function in a specific folder.
        /// </summary>
        public static readonly AttributeFunction FOLDER = new AttributeFunction("folder", "folder")
            .AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("path"))
            .WithCallAction((parameters, excecutor, statement) =>
            {
                string folderPath = parameters[0].CurrentValue as TokenStringLiteral;
                return new AttributeFolder(folderPath);
            });

    }
}
