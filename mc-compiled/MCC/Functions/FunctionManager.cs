using mc_compiled.MCC.Compiler.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Attributes.Implementations;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Functions
{
    /// <summary>
    /// Manages the user, system, and attribute functions in the project, as well as the parsing of them.
    /// </summary>
    public class FunctionManager
    {
        /// <summary>
        /// All of the functions currently in the project.
        /// </summary>
        private Dictionary<string, List<Function>> functionRegistry;
        private readonly List<TestFunction> tests;
        private readonly ScoreboardManager scoreboardManager;
        

        /// <summary>
        /// Contains all compiler-implemented providers that should be added on new instances of a FunctionManager.
        /// </summary>
        internal static IFunctionProvider[] DefaultCompilerProviders =
        {
            AttributeFunctions.PROVIDER,
            CompiletimeFunctions.PROVIDER,
        };

        /// <summary>
        /// Create a new FunctionManager with an empty registry.
        /// </summary>
        internal FunctionManager(ScoreboardManager manager)
        {
            this.functionRegistry = new Dictionary<string, List<Function>>(StringComparer.OrdinalIgnoreCase);
            this.scoreboardManager = manager;
            this.tests = new List<TestFunction>();
        }
        /// <summary>
        /// Registers the function providers included with the compiler.
        /// </summary>
        internal void RegisterDefaultProviders()
        {
            foreach (IFunctionProvider provider in DefaultCompilerProviders)
            {
                var functions = provider.ProvideFunctions(this.scoreboardManager);
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
            if (function == null)
                throw new NullReferenceException("Attempted to register a function that was null.");

            string key = function.Keyword;
            string[] aliases = function.Aliases;

            RegisterUnderName(key, function);

            if (aliases == null)
                return;
            
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
            if (!this.functionRegistry.TryGetValue(name, out List<Function> functions))
                functions = new List<Function>();

            functions.Add(function);
            this.functionRegistry[name] = functions;
        }
        /// <summary>
        /// Registers a test function.
        /// </summary>
        /// <param name="function">The test function to register.</param>
        internal void RegisterTest(TestFunction function)
        {
            this.tests.Add(function);
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
                from funcList in this.functionRegistry.Values
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
            return this.functionRegistry.Values.SelectMany(item => item);
        }
        
        /// <summary>
        /// Tries to fetch all functions that matched a keyword. Returned array is sorted by highest importance first.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="functions"></param>
        /// <returns></returns>
        public bool TryGetFunctions(string name, out Function[] functions)
        {
            if(this.functionRegistry.TryGetValue(name, out List<Function> results))
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
