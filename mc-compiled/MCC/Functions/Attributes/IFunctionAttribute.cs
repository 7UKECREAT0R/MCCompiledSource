using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions.Attributes
{
    /// <summary>
    /// Represents an attribute that can be placed on a UserFunction.
    /// </summary>
    public interface IFunctionAttribute
    {
        /// <summary>
        /// Called when this attribute is first added to a function.
        /// </summary>
        void OnCreated(RuntimeFunction function);

        /// <summary>
        /// Called when the function this attribute is attached to is called.
        /// </summary>
        /// <param name="function">UserFunction</param>
        /// <param name="statement"></param>
        /// <param name="executor"></param>
        void OnCalled(RuntimeFunction function, Statement statement, Executor executor);
    }
}
