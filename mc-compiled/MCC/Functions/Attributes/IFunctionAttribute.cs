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
        /// Returns the string used to represent this attribute in debugging contexts.
        /// </summary>
        /// <returns></returns>
        string GetDebugString();

        /// <summary>
        /// Called when this attribute is first added to a function.
        /// </summary>
        void OnCreated(RuntimeFunction function);

        /// <summary>
        /// Called when the function this attribute is attached to is called.
        /// This is called after the function call has been completed, but before the return value has been set.
        /// </summary>
        /// <param name="function">The function being called.</param>
        /// <param name="commandBuffer">The commands that are going to be added when the procedure is done.</param>
        /// <param name="executor">The executor in this context.</param>
        /// <param name="statement">The calling statement.</param>
        void OnCalled(RuntimeFunction function, List<string> commandBuffer, Executor executor, Statement statement);
    }
}
