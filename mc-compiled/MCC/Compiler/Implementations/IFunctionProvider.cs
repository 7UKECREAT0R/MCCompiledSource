using mc_compiled.MCC.Functions;
using System.Collections.Generic;

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
        IEnumerable<Function> ProvideFunctions(ScoreboardManager manager);
    }
}
