using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest.Dependencies;

/// <summary>
///     Dependency that references a built-in module, such as Scripting APIs.
/// </summary>
public sealed class DependencyModule : Dependency
{
    /// <summary>
    ///     The Guid of the pack this dependency references.
    /// </summary>
    private readonly string moduleName;

    public DependencyModule(string moduleName, ManifestVersion version) : base(version)
    {
        this.moduleName = moduleName;
    }

    public override JObject ToJSON()
    {
        return new JObject
        {
            ["module_name"] = this.moduleName,
            ["version"] = this.version.ToVersionString()
        };
    }
}