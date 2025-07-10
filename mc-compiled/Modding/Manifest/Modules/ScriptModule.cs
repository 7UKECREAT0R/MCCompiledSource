using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest.Modules;

/// <summary>
///     Represents a module which routes to/enables Script API in a project.
/// </summary>
public class ScriptModule(Guid? uuid, ScriptLanguage language, string entry)
    : Module(ModuleType.script, uuid, ManifestVersion.DEFAULT)
{
    public override JObject ToJSON()
    {
        return new JObject
        {
            ["type"] = nameof(ModuleType.script),
            ["language"] = language.ToString(),
            ["uuid"] = this.uuid.ToString(),
            ["entry"] = entry,
            ["version"] = this.moduleVersion.ToJSON()
        };
    }
}

/// <summary>
///     List of supported Script API languages for the 'language' field in a module.
/// </summary>
public enum ScriptLanguage
{
    javascript
}