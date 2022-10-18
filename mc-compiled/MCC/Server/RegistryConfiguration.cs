using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Server
{
    /// <summary>
    /// Configures custom protocol to launch MCCompiled from the web.
    /// </summary>
    internal class RegistryConfiguration
    {
        const string SUBKEY = "mccompiled";

        public readonly bool hasBeenRegistered;

        /// <summary>
        /// Instantiate a RegistryConfiguration and run it. See <see cref="hasBeenRegistered"/> to identify if successful.
        /// </summary>
        internal RegistryConfiguration()
        {
            try
            {
                RegistryKey subkey = Registry.ClassesRoot.CreateSubKey(SUBKEY);
                subkey.SetValue("", "MCCompiled Server Protocol");
                subkey.SetValue("URL Protocol", "");

                subkey = subkey.CreateSubKey("shell");
                subkey = subkey.CreateSubKey("open");
                subkey = subkey.CreateSubKey("command");
                subkey.SetValue("", Assembly.GetExecutingAssembly().Location + " --fromProtocol");

                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Set up MCCompiled {Compiler.Executor.MCC_VERSION} language server for use. Please remember to run this command again if the location of this executable changes.");
                Console.ForegroundColor = old;
                hasBeenRegistered = true;
            } catch(Exception e)
            {
                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                if(e is UnauthorizedAccessException)
                    Console.WriteLine($"UnauthorizedAccessException: Please launch as administrator to install server features.");
                else
                {
                    Console.WriteLine($"Unknown Error: {e.GetType().Name}");
                    Console.WriteLine(e.ToString());
                }

                Console.ForegroundColor = old;
                hasBeenRegistered = false;
                return;
            }
        }
    }
}