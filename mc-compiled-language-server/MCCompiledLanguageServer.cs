using System.Collections.Concurrent;
using mc_compiled_language_server.MCC;
using OmniSharp.Extensions.LanguageServer.Server;

namespace mc_compiled_language_server;

internal static class MCCompiledLanguageServer
{
    /// <summary>
    ///     The unique identifier for the MCCompiled language to use across the language server.
    /// </summary>
    public const string LANGUAGE_ID = "mccompiled";
    /// <summary>
    ///     File pattern that matches MCCompiled source files.
    /// </summary>
    public const string EXTENSION_PATTERN = "*.mcc";
    /// <summary>
    ///     The file extension used by MCCompiled with a dot before it.
    /// </summary>
    public const string EXTENSION_WITH_DOT = ".mcc";
    /// <summary>
    ///     The file extension used by MCCompiled.
    /// </summary>
    public const string EXTENSION = "mcc";

    #region Server State

    /// <summary>
    ///     The currently opened project files, stored under their absolute file paths.
    /// </summary>
    internal static readonly ConcurrentDictionary<string, Project> PROJECTS = new();

    #endregion

    private static async Task Main(string[] args)
    {
        LanguageServer server = await LanguageServer.From(
            options => options
                .WithInput(Console.OpenStandardInput())
                .WithOutput(Console.OpenStandardOutput())
                .WithHandler<SyncHandler>()
        );
        await server.WaitForExit;
    }
}