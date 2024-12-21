using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest.Modules;

/// <summary>
///     Represents a simple module which contains data about the pack it's contained within.
///     Coverage of 'data' and 'resource' type modules.
/// </summary>
public class BasicModule : Module
{
    public BasicModule(ModuleType type, Guid? guid = null, ManifestVersion version = null) : base(type, guid, version)
    {
        if (type == ModuleType.script)
            throw new FormatException(
                "Attempted to create a BasicModule with type 'script'. This is a bug with the compiler itself.");
    }

    public override JObject ToJSON()
    {
        return new JObject
        {
            ["type"] = this.type.ToString(),
            ["uuid"] = this.uuid.ToString(),
            ["version"] = this.moduleVersion.ToJSON()
        };
    }
}