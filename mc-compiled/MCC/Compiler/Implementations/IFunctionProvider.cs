using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.Implementations
{
    /// <summary>
    /// Interface for allowing a class to "provide" a set of functions to the compiler.
    /// </summary>
    internal interface IFunctionProvider
    {
        /// <summary>
        /// Provide the functions to be added to the "backup" function pool to be added when used.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        IEnumerable<GenerativeFunction> ProvideFunctions(ScoreboardManager manager);
    }
}
