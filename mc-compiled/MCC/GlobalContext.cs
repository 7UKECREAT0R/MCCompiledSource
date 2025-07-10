using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace mc_compiled.MCC;

/// <summary>
///     Static class for managing compiler options/context.
/// </summary>
public static class GlobalContext
{
    private static readonly Stack<Context> contexts = new();
    /// <summary>
    ///     Get the current context. Never null, but may be invalid.
    /// </summary>
    [NotNull]
    public static Context Current { get; private set; } = new();

    /// <summary>
    ///     Shorthand for <c>GlobalContext.Current.debug</c>
    /// </summary>
    public static bool Debug => Current.debug;
    /// <summary>
    ///     Shorthand for <c>GlobalContext.Current.decorate</c>
    /// </summary>
    public static bool Decorate => Current.decorate;

    internal static void Pop()
    {
        contexts.Pop();
        Current = contexts.TryPeek(out Context currentTop) ? currentTop : new Context();
    }

    /// <summary>
    ///     Creates a new <see cref="Context" /> and pushes it to the top of the stack. Has default values.
    ///     Access using <see cref="GlobalContext.Current" />
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public static ContextContract NewDefault()
    {
        Current = new Context();
        contexts.Push(Current);
        return new ContextContract(Current);
    }
    /// <summary>
    ///     Creates a new <see cref="Context" /> cloned from the existing one and pushes it to the top of the stack.
    ///     Access using <see cref="GlobalContext.Current" />
    /// </summary>
    /// <returns></returns>
    public static ContextContract NewInherit()
    {
        Current = (Context) Current.Clone();
        contexts.Push(Current);
        return new ContextContract(Current);
    }
}

/// <summary>
///     A contract for a context that agrees to release itself once it's done being used.
/// </summary>
public class ContextContract : IDisposable
{
    public readonly Context heldContext;

    internal ContextContract(Context context) { this.heldContext = context; }
    public void Dispose() { GlobalContext.Pop(); }
}

/// <summary>
///     Compilation context.
/// </summary>
public class Context : ICloneable
{
    public const string PROJECT_BP = "?project_BP";
    public const string PROJECT_RP = "?project_RP";
    public const string PROJECT_REPLACER = "?project";
    private const string APP_ID = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

    /// <summary>
    ///     The path to output the behavior pack at.
    ///     Use <see cref="PROJECT_BP" /> to denote where the name of the compiled project should be inserted at.
    /// </summary>
    public string behaviorPackOutputPath = PROJECT_BP;

    /// <summary>
    ///     The compiler will stay open and continue to compile files as they change.
    /// </summary>
    internal bool daemon;
    /// <summary>
    ///     Emit extra debug information in the console.
    /// </summary>
    public bool debug;
    /// <summary>
    ///     Decorate the compiled files with comments for human readers.
    /// </summary>
    public bool decorate;
    /// <summary>
    ///     Export all functions regardless if they're used or not.
    /// </summary>
    public bool exportAll;
    /// <summary>
    ///     Ignore manifests and leave them as-is. Used in special cases where the compiler doesn't support manifests.
    /// </summary>
    public bool ignoreManifests;
    /// <summary>
    ///     A list of PPVs to set at the start of a compilation.
    /// </summary>
    internal List<InputPPV> inputPPVs = [];
    /// <summary>
    ///     May be null. The name of the project that will be executed under this context.
    /// </summary>
    public string projectName;
    /// <summary>
    ///     Special Regolith compilation settings.
    /// </summary>
    internal bool regolith;
    /// <summary>
    ///     The path to output the resource pack at.
    ///     Use <see cref="PROJECT_RP" /> to denote where the name of the compiled project should be inserted at.
    /// </summary>
    public string resourcePackOutputPath = PROJECT_RP;
    /// <summary>
    ///     Compiler will search and compile multiple files.
    /// </summary>
    internal bool search;
    /// <summary>
    ///     Add debug print statements to trace the execution of the application. Only really used for debugging.
    /// </summary>
    public bool trace;

    public object Clone()
    {
        return new Context
        {
            behaviorPackOutputPath = this.behaviorPackOutputPath,
            resourcePackOutputPath = this.resourcePackOutputPath,
            projectName = this.projectName,
            daemon = this.daemon,
            debug = this.debug,
            decorate = this.decorate,
            exportAll = this.exportAll,
            ignoreManifests = this.ignoreManifests,
            inputPPVs = [..this.inputPPVs],
            regolith = this.regolith,
            search = this.search,
            trace = this.trace
        };
    }

    /// <summary>
    ///     Configures the output paths for the behavior pack and resource pack to be within the development folders
    ///     used by Minecraft's local storage, specifically under "development_behavior_packs" and
    ///     "development_resource_packs".
    /// </summary>
    /// <remarks>
    ///     This method modifies the <see cref="Context.behaviorPackOutputPath" /> and
    ///     <see cref="Context.resourcePackOutputPath" />
    ///     properties to point to development folders. The paths are formulated using the current user's
    ///     Local AppData directory and are based on the <see cref="APP_ID" /> for the Minecraft UWP application.
    /// </remarks>
    public void SetOutputToDevelopmentFolders()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string comMojang = Path.Combine(localAppData, "Packages", APP_ID, "LocalState", "games",
            "com.mojang");
        this.behaviorPackOutputPath = Path.Combine(comMojang, "development_behavior_packs") + '\\' + PROJECT_BP;
        this.resourcePackOutputPath = Path.Combine(comMojang, "development_resource_packs") + '\\' + PROJECT_RP;
    }

    /// <summary>
    ///     A preprocessor variable which is to be set upon the start of each compilation.
    /// </summary>
    public struct InputPPV
    {
        public readonly string name;
        public readonly object value;

        public InputPPV(string name, string value)
        {
            this.name = name;

            switch (value.ToUpper())
            {
                case "TRUE":
                    this.value = true;
                    return;
                case "FALSE":
                    this.value = false;
                    return;
            }

            if (int.TryParse(value, out int integer))
            {
                this.value = integer;
                return;
            }

            if (float.TryParse(value, out float floating))
            {
                this.value = floating;
                return;
            }

            this.value = value;
        }
    }
}