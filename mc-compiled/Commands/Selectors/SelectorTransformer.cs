using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Transforms a selector based off of a set of tokens.
    /// </summary>
    public interface SelectorTransformer
    {
        /// <summary>
        /// Get the keyword which is used to invoke this transformer.
        /// </summary>
        /// <returns></returns>
        string GetKeyword();
        /// <summary>
        /// Returns if this selector can be inverted.
        /// </summary>
        /// <returns></returns>
        bool CanBeInverted();

        /// <summary>
        /// Take in a set of tokens to transform the selector given, potentially adding commands or changing the project as well.
        /// </summary>
        /// <param name="selector">The selector to transform.</param>
        /// <param name="inverted">Whether this statement is inverted or not.</param>
        /// <param name="executor">The executor running this transformation.</param>
        /// <param name="tokens">The fed-in tokens which specify how the transformation should occur.</param>
        /// <param name="commands">The list of commands to add to.</param>
        void Transform(ref Selector selector, bool inverted, Executor executor, Statement tokens, List<string> commands);
    }
}
