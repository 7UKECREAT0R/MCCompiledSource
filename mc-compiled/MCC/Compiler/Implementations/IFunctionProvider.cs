using System.Collections.Generic;
using mc_compiled.MCC.Functions;

namespace mc_compiled.MCC.Compiler.Implementations
{
    /// <summary>
    /// Interface for allowing a class to "provide" a set of functions to the compiler.
    /// </summary>
    internal interface IFunctionProvider
    {
        /// <summary>
        /// Provide the functions to be added to the function pool by the compiler.
        /// It's important to note that any functions that store state should have a new instance created every time this function is called.
        ///     (don't store any functions statically unless they are 100% implementation, and no state)
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        IEnumerable<Function> ProvideFunctions(ScoreboardManager manager);
    }
}
