using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    public struct Manifest
    {
        public int formatVersion;

        public string name;
        public string description;
        public Guid uuid1, uuid2;
        public int[] version;
        public int[] minEngineVersion;

        public Manifest(string name = "Example Pack", string description = "Test Behavior Pack",
            int[] minEngineVersion = null, int formatVersion = 2)
        {
            this.formatVersion = formatVersion;
            this.name = name;
            uuid1 = Guid.NewGuid();
            uuid2 = Guid.NewGuid();
            this.description = description;
            version = new int[] { 0, 0, 1 };

            if (minEngineVersion == null)
                this.minEngineVersion = new int[] { 1, 13, 0 };
            else this.minEngineVersion = minEngineVersion;
        }
        public override string ToString()
        {
            JObject header = new JObject();
            header["description"] = description;
            header["name"] = name;
            header["uuid"] = uuid1.ToString();
            header["version"] = new JArray(version);
            header["min_engine_version"] = new JArray(minEngineVersion);

            JArray modules = new JArray();
            JObject module = new JObject();
            module["description"] = description;
            module["type"] = "data";
            module["uuid"] = uuid2.ToString();
            module["version"] = new JArray(version);
            modules.Add(module);
            
            JObject token = new JObject();
            token["format_version"] = formatVersion;
            token["header"] = header;
            token["modules"] = modules;

            return token.ToString();
        }
    }
}
