using System;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest
{
    /// <summary>
    /// Represents the header of a BP/RP manifest file.
    /// </summary>
    public class ManifestHeader
    {
        public string name;
        public string description;
        public Guid uuid;
        
        /// <summary>
        /// The version of this pack.
        /// </summary>
        public ManifestVersion version;
        /// <summary>
        /// The minimum engine version required to run this pack; i.e., allowed features.
        /// </summary>
        public ManifestVersion minEngineVersion;

        /// <summary>
        /// Returns a ManifestHeader with default values that reflect the base input values (BP/RP and project name).
        /// </summary>
        /// <param name="type">The type of the manifest; BP or RP.</param>
        /// <param name="projectName">The name of the project being compiled.</param>
        /// <param name="uuid">If included, the UUID that will be assigned to the header.</param>
        /// <returns></returns>
        public static ManifestHeader Default(ManifestType type, string projectName, Guid? uuid = null)
        {
            return new ManifestHeader(
                $"{projectName} {type}",
                $"MCCompiled {Executor.MCC_VERSION} Project. ({type})",
                uuid);
        }
        public ManifestHeader(string name, string description,
            Guid? uuid = null,
            ManifestVersion version = null,
            ManifestVersion minEngineVersion = null)
        {
            this.name = name;
            this.description = description;
            this.uuid = uuid ?? Guid.NewGuid();
            this.version = version ?? ManifestVersion.DEFAULT;

            if (minEngineVersion != null)
            {
                // make sure it's at least the one supported by MCCompiled
                if (minEngineVersion < ManifestVersion.MIN_ENGINE_VERSION)
                    minEngineVersion = ManifestVersion.MIN_ENGINE_VERSION;
            } else
                minEngineVersion = ManifestVersion.MIN_ENGINE_VERSION;
            
            this.minEngineVersion = minEngineVersion;
        }
        
        /// <summary>
        /// Parses a Manifest header from the given JSON input. This method cannot fail, but
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static ManifestHeader Parse(JObject json)
        {
            string name = json["name"]?.ToString() ?? "pack.name";
            string description = json["description"]?.ToString() ?? "pack.description";
            
            Guid? uuid = null;
            if (json.TryGetValue("uuid", out JToken uuidJToken))
                if (Guid.TryParse(uuidJToken.ToString(), out Guid _uuid))
                    uuid = _uuid;
            
            ManifestVersion version = null;
            if (json.TryGetValue("version", out JToken versionJToken))
                ManifestVersion.TryParse(versionJToken.ToString(), out version);
            
            ManifestVersion minEngineVersion = null;
            if (json.TryGetValue("version", out JToken minEngineVersionJToken))
                ManifestVersion.TryParse(minEngineVersionJToken.ToString(), out minEngineVersion);

            return new ManifestHeader(name, description, uuid, version, minEngineVersion);
        }
        /// <summary>
        /// Converts the ManifestHeader object to a JSON representation.
        /// </summary>
        /// <returns>A JObject representing the ManifestHeader object in JSON format.</returns>
        public JObject ToJSON()
        {
            return new JObject
            {
                ["name"] = this.name,
                ["description"] = this.description,
                ["uuid"] = this.uuid.ToString(),
                ["version"] = this.version.ToJSON(),
                ["min_engine_version"] = this.minEngineVersion.ToJSON()
            };
        }
    }
}