using System;
using mc_compiled.Modding.Manifest.Dependencies;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest;

/// <summary>
///     Represents a dependency in a BP/RP manifest file.
/// </summary>
public abstract class Dependency
{
    internal ManifestVersion version;
    protected Dependency(ManifestVersion version)
    {
        this.version = version;
    }
    /// <summary>
    ///     Tries to parse a JSON object and create a Dependency from it.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="dependency">
    ///     When this method returns, contains the parsed Dependency object if the parsing was successful,
    ///     or null if the parsing failed.
    /// </param>
    /// <returns>
    ///     true if the parsing was successful and a Dependency was created; false otherwise.
    /// </returns>
    public static bool TryParse(JObject json, out Dependency dependency)
    {
        ManifestVersion version;
        if (json.TryGetValue("version", out JToken versionToken))
        {
            if (!ManifestVersion.TryParseToken(versionToken, out version))
            {
                // we can't just make up a dependency version
                dependency = null;
                return false;
            }
        }
        else
        {
            // we can't just make up a dependency version
            dependency = null;
            return false;
        }

        // uuid based dependency
        if (json.TryGetValue("uuid", out JToken uuidToken))
        {
            if (!Guid.TryParse(uuidToken.ToString(), out Guid uuid))
                uuid = Guid.NewGuid();
            dependency = new DependencyUUID(uuid, version);
            return true;
        }

        // module_name based dependency
        if (json.TryGetValue("module_name", out JToken moduleNameToken))
        {
            dependency = new DependencyModule(moduleNameToken.ToString(), version);
            return true;
        }

        dependency = null;
        return false;
    }

    /// <summary>
    ///     Convert this dependency to its associated JSON object.
    /// </summary>
    /// <returns></returns>
    public abstract JObject ToJSON();
}