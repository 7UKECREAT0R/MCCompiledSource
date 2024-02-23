using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace mc_compiled.Modding.Resources
{
    public class EntityGeometry : IAddonFile
    {
        public string name;
        public string identifier;
        public int textureWidth;
        public int textureHeight;

        public string CommandReference => throw new NotImplementedException();

        public EntityGeometry(string name, string identifier)
        {
            this.name = name;
            this.identifier = identifier;
            this.textureWidth = 16;
            this.textureHeight = 16;
        }
        public JObject ToJSON()
        {
            return new JObject()
            {
                ["format_version"] = FormatVersion.r_MODEL.ToString(),
                ["minecraft:geometry"] = new JArray(new[] {
                    new JObject()
                    {
                        ["description"] = new JObject()
                        {
                            ["identifier"] = this.identifier,
                            ["texture_width"] = this.textureWidth,
                            ["texture_height"] = this.textureHeight
                        }
                    }
                })
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
            OutputLocation.r_MODELS__ENTITY;
    }
}