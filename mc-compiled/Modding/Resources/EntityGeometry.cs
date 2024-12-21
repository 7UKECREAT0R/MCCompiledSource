using System;
using System.Text;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Resources;

public class EntityGeometry : IAddonFile
{
    public string identifier;
    public string name;
    public int textureHeight;
    public int textureWidth;

    public EntityGeometry(string name, string identifier)
    {
        this.name = name;
        this.identifier = identifier;
        this.textureWidth = 16;
        this.textureHeight = 16;
    }

    public string CommandReference => throw new NotImplementedException();

    public string GetExtendedDirectory()
    {
        return null;
    }
    public byte[] GetOutputData()
    {
        JObject full = ToJSON();
        string str = full.ToString();
        return Encoding.UTF8.GetBytes(str);
    }
    public string GetOutputFile()
    {
        return this.name + ".json";
    }
    public OutputLocation GetOutputLocation()
    {
        return OutputLocation.r_MODELS__ENTITY;
    }
    public JObject ToJSON()
    {
        return new JObject
        {
            ["format_version"] = FormatVersion.r_MODEL.ToString(),
            ["minecraft:geometry"] = new JArray(new object[]
            {
                new JObject
                {
                    ["description"] = new JObject
                    {
                        ["identifier"] = this.identifier,
                        ["texture_width"] = this.textureWidth,
                        ["texture_height"] = this.textureHeight
                    }
                }
            })
        };
    }
}