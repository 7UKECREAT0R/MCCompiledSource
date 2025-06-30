using System;
using mc_compiled.MCC.Compiler;
using Microsoft.Win32;

namespace mc_compiled.MCC.ServerWebSocket;

/// <summary>
///     Configures custom protocol to launch MCCompiled from the web.
/// </summary>
internal class RegistryConfiguration
{
    internal const string SUBKEY = "mccompiled";
    internal const string PROGRAM_ARG = "--fromProtocol";
    internal const string NAME = "MCCompiled Server Protocol";
    internal const string URL_PROTOCOL = "URL Protocol";

    public bool hasBeenRegistered;

    /// <summary>
    ///     Instantiate a RegistryConfiguration. Sets <see cref="hasBeenRegistered" />.
    /// </summary>
    internal RegistryConfiguration()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("This feature is only supported on Windows.");

        try
        {
            RegistryKey root = Registry.ClassesRoot.OpenSubKey(SUBKEY);
            this.hasBeenRegistered = root != null;
        }
        catch (Exception e)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            if (e is UnauthorizedAccessException)
            {
                Console.WriteLine(
                    "UnauthorizedAccessException: Please launch as administrator to install server features.");
            }
            else
            {
                Console.WriteLine($"Unknown Error: {e.GetType().Name}");
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = old;
            this.hasBeenRegistered = false;
        }
    }

    /// <summary>
    ///     Installs this version of MCCompiled into registry as a protocol url.
    /// </summary>
    internal void Install()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("This feature is only supported on Windows.");

        try
        {
            RegistryKey subkey = Registry.ClassesRoot.CreateSubKey(SUBKEY);
            subkey.SetValue("", NAME);
            subkey.SetValue(URL_PROTOCOL, "");

            subkey = subkey.CreateSubKey("shell");
            subkey = subkey.CreateSubKey("open");
            subkey = subkey.CreateSubKey("command");
            subkey.SetValue("", AppContext.BaseDirectory + ' ' + PROGRAM_ARG);

            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"Set up MCCompiled 1.{Executor.MCC_VERSION} language server for use. Remember to run this command again if the location of this executable changes.");
            Console.ForegroundColor = old;
            this.hasBeenRegistered = true;
        }
        catch (Exception e)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            if (e is UnauthorizedAccessException)
            {
                Console.WriteLine(
                    "UnauthorizedAccessException: Please launch as administrator to install server features.");
            }
            else
            {
                Console.WriteLine($"Unknown Error: {e.GetType().Name}");
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = old;
        }
    }
    /// <summary>
    ///     Removes the keys associated with any installation of MCCompiled so protocol urls no longer work.
    /// </summary>
    internal void Uninstall()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("This feature is only supported on Windows.");

        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(SUBKEY);

            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Uninstalled MCCompiled 1.{Executor.MCC_VERSION} language server for all versions.");
            Console.ForegroundColor = old;
            this.hasBeenRegistered = true;
        }
        catch (Exception e)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            if (e is UnauthorizedAccessException)
            {
                Console.WriteLine(
                    "UnauthorizedAccessException: Please launch as administrator to uninstall server features.");
            }
            else
            {
                Console.WriteLine($"Unknown Error: {e.GetType().Name}");
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = old;
        }
    }
}