using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions
{
    /// <summary>
    /// Manages the user, system, and attribute functions in the project, as well as the parsing of them.
    /// </summary>
    public class FunctionManager
    {
        private Dictionary<string, Function> functionRegistry = new Dictionary<string, Function>(StringComparer.OrdinalIgnoreCase);

        public void RegisterFunction(Function function)
        {
            string key = function.Keyword;
            string[] aliases = function.Aliases;

            this.functionRegistry[key] = function;
            
            if(aliases != null)
                foreach (string alias in aliases)
                    this.functionRegistry[key] = function;
        }
        /// <summary>
        /// Returns all registered functions that are under a specific type.
        /// To get all functions, use <see cref="FetchAll"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> FetchAll<T>() where T: Function
        {
            return (
                from func
                in functionRegistry.Values
                where func is T
                select func as T
            );
        }
        /// <summary>
        /// Returns all functions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Function> FetchAll()
        {
            return functionRegistry.Values;
        }
    }
}
