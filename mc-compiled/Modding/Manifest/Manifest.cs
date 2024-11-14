using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mc_compiled.Modding.Manifest.Dependencies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest
{
    public class Manifest : IAddonFile
    {
        public const int FORMAT_VERSION = 2;
        public readonly ManifestType type;
        public readonly ManifestHeader header;
        public readonly List<Dependency> dependencies;
        public readonly List<Module> modules;

        /// <summary>
        /// Construct a manifest with the given header and empty lists for dependencies and modules.
        /// Use <see cref="WithDependency"/> and <see cref="WithModule"/> methods to add on to this.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="header"></param>
        private Manifest(ManifestType type, ManifestHeader header)
        {
            this.type = type;
            this.header = header;
            this.dependencies = [];
            this.modules = [];
        }
        /// <summary>
        /// Construct a default manifest based on some basic input information.
        /// </summary>
        /// <param name="type">The type of manifest; BP or RP.</param>
        /// <param name="projectName">The name of the project that the manifest is being generated for.</param>
        /// <param name="uuid">If included, the UUID that will be assigned to the header.</param>
        public Manifest(ManifestType type, string projectName, Guid? uuid = null)
        {
            this.type = type;
            this.header = ManifestHeader.Default(type, projectName, uuid);
            this.dependencies = [];
            this.modules = [];
        }
        /// <summary>
        /// Appends a dependency to the manifest.
        /// </summary>
        /// <param name="dependency">The dependency to be added.</param>
        /// <returns>The modified manifest object with the added dependency.</returns>
        public Manifest WithDependency(Dependency dependency)
        {
            this.dependencies.Add(dependency);
            return this;
        }
        /// <summary>
        /// Adds a module to the manifest.
        /// </summary>
        /// <param name="module">The module to be added.</param>
        /// <returns>The updated manifest with the added module.</returns>
        public Manifest WithModule(Module module)
        {
            this.modules.Add(module);
            return this;
        }

        /// <summary>
        /// Adds a dependency with the specified UUID and version to the manifest.
        /// If there is already a dependency with the same UUID, the target version will be updated if necessary.
        /// </summary>
        /// <param name="uuid">The UUID of the dependency to add or update.</param>
        /// <param name="version">The target version of the dependency.</param>
        public void DependOn(Guid uuid, ManifestVersion version)
        {
            if (this.dependencies.Any(d => d is DependencyUUID other && other.dependsOnUUID.Equals(uuid)))
            {
                IEnumerable<DependencyUUID> dependenciesToUpdate = from d in this.dependencies
                    where d is DependencyUUID other && other.dependsOnUUID.Equals(uuid)
                    select (DependencyUUID) d;
                
                // update the target version of the dependency(s) if needed
                foreach (DependencyUUID dependencyToUpdate in dependenciesToUpdate)
                    if (dependencyToUpdate.version < version)
                        dependencyToUpdate.version = version;
                return;
            }

            this.dependencies.Add(new DependencyUUID(uuid, version));
        }
        public void DependOn(Manifest otherManifest)
        {
            DependOn(otherManifest.header.uuid, otherManifest.header.version);
        }
        
        /// <summary>
        /// Parses a manifest from JSON.
        /// </summary>
        /// <param name="json">The JSON to parse the manifest from.</param>
        /// <param name="type">The type of manifest this is; BP or RP.</param>
        /// <param name="projectName">The project name to default to if the manifest is missing a header.</param>
        /// <returns>The parsed manifest. This method cannot throw, but will discard any invalid manifest data, so beware.</returns>
        public static Manifest Parse(JObject json, ManifestType type, string projectName)
        {
            ManifestHeader header = json.TryGetValue("header", out JToken headerToken) ?
                ManifestHeader.Parse((JObject)headerToken) :
                ManifestHeader.Default(type, projectName);

            var manifest = new Manifest(type, header);

            if (json.TryGetValue("dependencies", out JToken dependenciesToken) && dependenciesToken.Type == JTokenType.Array)
            {
                var dependencies = (JArray)dependenciesToken;
                foreach (JToken jToken in dependencies)
                {
                    if (jToken.Type != JTokenType.Object)
                        continue;
                    var dependency = (JObject)jToken;
                    if (Dependency.TryParse(dependency, out Dependency parsedDependency))
                        manifest.WithDependency(parsedDependency);
                }
            }
            if (json.TryGetValue("modules", out JToken modulesToken) && modulesToken.Type == JTokenType.Array)
            {
                var modules = (JArray)modulesToken;
                foreach (JToken jToken in modules)
                {
                    if (jToken.Type != JTokenType.Object)
                        continue;
                    var module = (JObject)jToken;
                    if (Module.TryParse(module, out Module parsedModule))
                        manifest.WithModule(parsedModule);
                }
            }

            return manifest;
        }
        
        public string CommandReference => throw new NotImplementedException();

        public byte[] GetOutputData() =>
            Encoding.UTF8.GetBytes(ToString());
        public string GetExtendedDirectory() =>
            null;
        public string GetOutputFile() =>
            "manifest.json";

        /// <summary>
        /// Outputs the JSON text for this manifest file.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToJSON().ToString(Formatting.Indented);
        
        /// <summary>
        /// Export this manifest as its proper JSON format.
        /// </summary>
        /// <returns></returns>
        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["format_version"] = FORMAT_VERSION,
                ["header"] = this.header.ToJSON(),
            };

            if (this.dependencies.Any())
                json["dependencies"] = new JArray(this.dependencies.Select(d => d.ToJSON()));
            if (this.modules.Any())
                json["modules"] = new JArray(this.modules.Select(m => m.ToJSON()));

            return json;
        }
        
        public OutputLocation GetOutputLocation() => this.type == ManifestType.BP ?
            OutputLocation.b_ROOT :
            OutputLocation.r_ROOT;
    }

    public enum ManifestType
    {
        BP,
        RP
    }
}