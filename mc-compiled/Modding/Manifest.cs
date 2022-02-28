using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    public class Manifest : IAddonFile
    {
        public struct Module
        {
            public string description;
            public string type;
            public Guid uuid;
            public int[] version;

            public Module(string description, string type, Guid uuid, int[] version)
            {
                this.description = description;
                this.type = type;
                this.uuid = uuid;
                this.version = version;
            }
            public Module(JObject json)
            {
                description = json["description"].ToString();
                type = json["type"].ToString();
                uuid = Guid.Parse(json["uuid"].ToString());
                JArray array = (JArray)json["version"];
                version = array.ToObject<int[]>();
            }
            /// <summary>
            /// Create a Module that represents behavior pack data.
            /// </summary>
            /// <param name="projectName"></param>
            /// <returns></returns>
            public static Module BehaviorData(string projectName)
            {
                return new Module($"{projectName} Data", "data", Guid.NewGuid(), new[] { 1, 0, 0 });
            }
            /// <summary>
            /// Create a Module that represents resource pack data.
            /// </summary>
            /// <param name="projectName"></param>
            /// <returns></returns>
            public static Module ResourceData(string projectName)
            {
                return new Module($"{projectName} Resources", "resources", Guid.NewGuid(), new[] { 1, 0, 0 });
            }

            public JObject ToJson()
            {
                JObject module = new JObject();
                module["description"] = description;
                module["type"] = type;
                module["uuid"] = uuid.ToString();
                module["version"] = new JArray(version);
                return module;
            }
        }

        public int formatVersion;
        public List<Module> modules;

        public Guid? dependsOn;
        public string name;
        public string description;
        public Guid uuid;
        public int[] version;
        public int[] minEngineVersion;

        public Manifest(OutputLocation type, Guid uuid, string name = "Example Pack", string description = "Test Behavior Pack",
            int[] minEngineVersion = null, int formatVersion = 2, Guid? dependsOn = null)
        {
            manifestType = type;
            this.formatVersion = formatVersion;
            this.name = name;
            this.uuid = uuid;
            this.description = description;
            this.dependsOn = dependsOn;
            version = new int[] { 1, 0, 0 };
            modules = new List<Module>();

            if (minEngineVersion == null)
                this.minEngineVersion = new int[] { 1, 13, 0 };
            else this.minEngineVersion = minEngineVersion;
        }
        /// <summary>
        /// Parse an existing manifest.
        /// </summary>
        /// <param name="parse"></param>
        public Manifest(string parse)
        {
            JObject root = JObject.Parse(parse);
            JObject header = root["header"] as JObject;
            JArray modules = root["modules"] as JArray;
            formatVersion = root["format_version"].Value<int>();

            description = header["description"].ToString();
            name = header["name"].ToString();
            uuid = Guid.Parse(header["uuid"].ToString());
            version = header["version"].ToObject<int[]>();
            minEngineVersion = header["min_engine_version"].ToObject<int[]>();

            this.modules = new List<Module>();
            foreach (JObject module in modules)
                this.modules.Add(new Module(module));

            if(root.TryGetValue("dependencies", out JToken value))
            {
                JArray dependencies = value as JArray;
                if (dependencies.Count > 0)
                    dependsOn = Guid.Parse(dependencies[0]["uuid"].ToString());
            }
        }
        /// <summary>
        /// Include a module in this manifest.
        /// </summary>
        /// <param name="module"></param>
        /// <returns>this</returns>
        public Manifest WithModule(Module module)
        {
            modules.Add(module);
            return this;
        }

        public byte[] GetOutputData() =>
            Encoding.UTF8.GetBytes(ToString());
        public string GetOutputDirectory() =>
            null;
        public string GetOutputFile() =>
            "manifest.json";

        public override string ToString()
        {
            JObject header = new JObject();
            header["description"] = description;
            header["name"] = name;
            header["uuid"] = uuid.ToString();
            header["version"] = new JArray(version);
            header["min_engine_version"] = new JArray(minEngineVersion);

            JArray dependencies = null;
            if (dependsOn != null)
            {
                dependencies = new JArray();
                JObject primaryDependency = new JObject();
                primaryDependency["uuid"] = dependsOn.Value.ToString();
                primaryDependency["version"] = new JArray(new[] { 1, 0, 0 });
                dependencies.Add(primaryDependency);
            }

            JArray modules = new JArray();
            foreach (Module module in this.modules)
                modules.Add(module.ToJson());
            
            JObject main = new JObject();
            main["format_version"] = formatVersion;
            main["header"] = header;
            if (dependencies != null)
                main["dependencies"] = dependencies;
            main["modules"] = modules;

            return main.ToString();
        }

        public OutputLocation manifestType;
        public OutputLocation GetOutputRoot() =>
            manifestType;
    }
}
