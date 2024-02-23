using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace mc_compiled.Modding
{
    public class Manifest : IAddonFile
    {
        private static readonly int[] REQUIRED_ENGINE_VERSION = new[] { 1, 20, 60 };
        public readonly struct Module
        {
            private readonly ModuleType type;
            private readonly int[] version;
            private readonly Guid uuid;

            private Module(ModuleType type, Guid uuid, int[] version)
            {
                this.type = type;
                this.uuid = uuid;
                this.version = version;
            }
            public Module(JObject json)
            {
                if (!Enum.TryParse(json["type"].ToString(), false, out this.type))
                    throw new Exception($"Module was of type '{json["type"]}'. Valid types include 'data' or 'resources'.");
                if (!Guid.TryParse(json["uuid"].ToString(), out this.uuid))
                    throw new Exception($"Module (type {this.type})'s UUID was incorrectly formatted.");
                
                var array = (JArray)json["version"];
                this.version = array.ToObject<int[]>();
            }
            /// <summary>
            /// Create a Module that represents behavior pack data.
            /// </summary>
            /// <returns></returns>
            public static Module BehaviorData()
            {
                return new Module(ModuleType.data, Guid.NewGuid(), new[] { 1, 0, 0 });
            }
            /// <summary>
            /// Create a Module that represents resource pack data.
            /// </summary>
            /// <returns></returns>
            public static Module ResourceData()
            {
                return new Module(ModuleType.resources, Guid.NewGuid(), new[] { 1, 0, 0 });
            }

            public JObject ToJson()
            {
                var module = new JObject
                {
                    ["type"] = this.type.ToString(),
                    ["uuid"] = this.uuid.ToString(),
                    ["version"] = new JArray(this.version)
                };
                return module;
            }
        }

        public Guid? dependsOn;
        public Guid uuid;

        private readonly int formatVersion;
        private readonly List<Module> modules;
        private readonly string name;
        private readonly string description;
        private readonly int[] version;
        private readonly int[] minEngineVersion;

        public Manifest(OutputLocation type, Guid uuid, string name = "MCCompiled Pack", string description = "Example Description",
            int[] minEngineVersion = null, int formatVersion = 2, Guid? dependsOn = null)
        {
            this.location = type;
            this.formatVersion = formatVersion;
            this.name = name;
            this.uuid = uuid;
            this.description = description;
            this.dependsOn = dependsOn;
            this.version = new[] { 1, 0, 0 };
            this.modules = new List<Module>();
            this.minEngineVersion = minEngineVersion ?? REQUIRED_ENGINE_VERSION;
        }


        /// <summary>
        /// Represents a manifest file for an addon.
        /// </summary>
        public Manifest(string parse, OutputLocation type)
        {
            this.location = type;
            JObject root = JObject.Parse(parse);
            var header = root["header"] as JObject;
            var modules = root["modules"] as JArray;
            this.formatVersion = root["format_version"].Value<int>();

            this.description = header["description"].ToString();
            this.name = header["name"].ToString();
            this.uuid = Guid.Parse(header["uuid"].ToString());
            this.version = header["version"].ToObject<int[]>();
            this.minEngineVersion = header["min_engine_version"].ToObject<int[]>();

            this.modules = new List<Module>();
            foreach (JObject module in modules)
                this.modules.Add(new Module(module));

            if(root.TryGetValue("dependencies", out JToken value))
            {
                JArray dependencies = value as JArray;
                if (dependencies.Count > 0) this.dependsOn = Guid.Parse(dependencies[0]["uuid"].ToString());
            }
        }
        /// <summary>
        /// Include a module in this manifest.
        /// </summary>
        /// <param name="module"></param>
        /// <returns>this</returns>
        public Manifest WithModule(Module module)
        {
            this.modules.Add(module);
            return this;
        }

        public string CommandReference => throw new NotImplementedException();

        public byte[] GetOutputData() =>
            Encoding.UTF8.GetBytes(ToString());
        public string GetExtendedDirectory() =>
            null;
        public string GetOutputFile() =>
            "manifest.json";

        public override string ToString()
        {
            JObject header = new JObject();
            header["description"] = this.description;
            header["name"] = this.name;
            header["uuid"] = this.uuid.ToString();
            header["version"] = new JArray(this.version);
            header["min_engine_version"] = new JArray(this.minEngineVersion);

            JArray dependencies = null;
            if (this.dependsOn != null)
            {
                dependencies = new JArray();
                JObject primaryDependency = new JObject();
                primaryDependency["uuid"] = this.dependsOn.Value.ToString();
                primaryDependency["version"] = new JArray(new[] { 1, 0, 0 });
                dependencies.Add(primaryDependency);
            }

            JArray modules = new JArray();
            foreach (Module module in this.modules)
                modules.Add(module.ToJson());
            
            JObject main = new JObject();
            main["format_version"] = this.formatVersion;
            main["header"] = header;
            if (dependencies != null)
                main["dependencies"] = dependencies;
            main["modules"] = modules;

            return main.ToString();
        }

        public OutputLocation location;

        public OutputLocation GetOutputLocation() => this.location;
    }

    public enum ModuleType
    {
        data,
        resources
    }
}
