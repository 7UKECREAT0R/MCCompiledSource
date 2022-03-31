using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Resources
{
    /// <summary>
    /// Super rudimentary client-entity implementation. For now, only serves for null generation.
    /// </summary>
    public class EntityResource : IAddonFile
    {
        public string name;
        public ClientEntityDescription description;

        public JObject ToJSON()
        {
            return new JObject()
            {
                ["format_version"] = FormatVersion.r_ENTITY.ToString(),
                ["minecraft:client_entity"] = new JObject()
                {
                    ["description"] = description.ToJSON()
                }
            };
        }

        public string GetExtendedDirectory() =>
            null;
        public byte[] GetOutputData()
        {
            JObject full = ToJSON();
            string str = full.ToString();
            return Encoding.UTF8.GetBytes(str);
        }
        public string GetOutputFile() =>
            name + ".json";
        public OutputLocation GetOutputLocation() =>
            OutputLocation.r_ENTITY;
    }
    public struct ClientEntityDescription
    {
        public string identifier;
        public string material;
        public EntityGeometry geometry;

        public JObject ToJSON()
        {
            return new JObject()
            {
                ["identifier"] = identifier,
                ["materials"] = new JObject()
                {
                    ["default"] = material
                },
                ["geometry"] = new JObject() {
                    ["default"] = geometry.identifier
                }
            };
        }
    }
}