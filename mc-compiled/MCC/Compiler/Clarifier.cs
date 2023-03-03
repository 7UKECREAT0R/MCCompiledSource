using mc_compiled.Commands.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Serves as a clarification as to who is being referenced for a Value.
    /// 
    /// Example for all cows:
    ///     @e[type=cow]
    /// Example for a fakeplayer 'neanderthal'
    ///     neanderthal
    /// 
    /// </summary>
    public class Clarifier
    {
        public const string DEFAULT_CLARIFIER = "@s";

        private bool global;
        private string currentString;

        /// <summary>
        /// Creates a new Clarifier with the default string attached to the global state.
        /// </summary>
        /// <param name="global"></param>
        public Clarifier(bool global)
        {
            this.global = global;
            this.Reset();
        }
        /// <summary>
        /// Sets this clarifier's global state.
        /// </summary>
        /// <param name="global">Whether to have global set or not.</param>
        /// <returns>This object for chaining.</returns>
        public Clarifier SetGlobal(bool global)
        {
            this.global = global;
            this.Reset();
            return this;
        }

        /// <summary>
        /// The string representing the current clarifier.<br />
        /// <br />
        /// To set, use <see cref="SetSelector(Selector)"/> or <see cref="SetString(string)"/>
        /// </summary>
        public string CurrentString
        {
            get => currentString;
        }
        /// <summary>
        /// Returns if the clarifier was defined as global.
        /// </summary>
        public bool IsGlobal
        {
            get => global;
        }
        /// <summary>
        /// Reset this clarifier to its default value, changing depending on if it's <see cref="global"/> or not.
        /// </summary>
        public void Reset()
        {
            if (global)
                currentString = Executor.FAKEPLAYER_NAME;
            else
                currentString = DEFAULT_CLARIFIER;
        }

        /// <summary>
        /// Sets the clarifier to a specific selector.
        /// </summary>
        /// <param name="selector"></param>
        public void SetSelector(Selector selector, Statement callingStatement)
        {
            if (global)
                throw new StatementException(callingStatement, "Attempted to clarify a global value.");

            string str = selector.ToString();
            this.currentString = str;
        }
        /// <summary>
        /// Sets the clarifier to a specific string.
        /// </summary>
        /// <param name="selector"></param>
        public void SetString(string str, Statement callingStatement)
        {
            if (global)
                throw new StatementException(callingStatement, "Attempted to clarify a global value.");

            this.currentString = str;
        }
    }
}
