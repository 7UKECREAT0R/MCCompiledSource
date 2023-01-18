using mc_compiled.MCC.Compiler.Implementations;
using mc_compiled.MCC.Compiler.Implementations.Functions;
using mc_compiled.MCC.Functions.Types;
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
        private Dictionary<string, List<Function>> functionRegistry;    // functions actively in the project

        /// <summary>
        /// Contains all compiler-implemented providers that should be added on new instances of a FunctionManager.
        /// </summary>
        internal static IFunctionProvider[] DefaultCompilerProviders =
        {
            new Average()
        };

        /// <summary>
        /// Create a new FunctionManager with an empty registry.
        /// </summary>
        internal FunctionManager(ScoreboardManager manager)
        {
            this.functionRegistry = new Dictionary<string, List<Function>>(StringComparer.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Registers the function providers included with the compiler.
        /// </summary>
        internal void RegisterDefaultProviders(ScoreboardManager manager)
        {
            foreach (IFunctionProvider provider in FunctionManager.DefaultCompilerProviders)
            {
                var functions = provider.ProvideFunctions(manager);
                foreach (Function function in functions)
                    RegisterFunction(function);
            }
        }

        /// <summary>
        /// Register a function.
        /// </summary>
        /// <param name="function"></param>
        public void RegisterFunction(Function function)
        {
            string key = function.Keyword;
            string[] aliases = function.Aliases;

            RegisterUnderName(key, function);

            if (aliases != null)
                foreach (string alias in aliases)
                    RegisterUnderName(alias, function);
        }
        /// <summary>
        /// Registers a function under a specific name. Ignores keyword and aliases of the function itself.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="function"></param>
        private void RegisterUnderName(string name, Function function)
        {
            List<Function> functions;

            if (!functionRegistry.TryGetValue(name, out functions))
                functions = new List<Function>();

            functions.Add(function);
            functionRegistry[name] = functions;
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
                from funcList in functionRegistry.Values
                from func in funcList
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
            return functionRegistry.Values.SelectMany(item => item);
        }
        
        /// <summary>
        /// Tries to fetch all functions that matched a keyword. Returned array is sorted by highest importance first.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="functions"></param>
        /// <returns></returns>
        public bool TryGetFunctions(string name, out Function[] functions)
        {
            if(functionRegistry.TryGetValue(name, out List<Function> results))
            {
                results.Sort(FunctionComparator.Instance);
                functions = results.ToArray();
                return true;
            }

            functions = null;
            return false;
        }
    }
}
