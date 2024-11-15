using System.Text;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Resources
{
    /// <summary>
    /// Super rudimentary client-entity implementation. For now, only serves for null generation.
    /// </summary>
    public class EntityResource : IAddonFile
    {
        public string name;
        public ClientEntityDescription description;

        public string CommandReference
        {
            get => this.description.identifier;
        }

        public JObject ToJSON()
        {
            return new JObject
            {
                ["format_version"] = FormatVersion.r_ENTITY.ToString(),
                ["minecraft:client_entity"] = new JObject
                {
                    ["description"] = this.description.ToJSON()
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
        public string GetOutputFile() => this.name + ".json";
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
            return new JObject
            {
                ["identifier"] = this.identifier,
                ["materials"] = new JObject
                {
                    ["default"] = this.material
                },
                ["geometry"] = new JObject {
                    ["default"] = this.geometry.identifier
                }
            };
        }
    }
}