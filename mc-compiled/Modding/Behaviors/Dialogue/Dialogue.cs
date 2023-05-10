using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors.Dialogue
{
    /// <summary>
    /// Represents an NPC dialogue with buttons, 
    /// </summary>
    public class Dialogue : IAddonFile
    {
        public readonly List<Scene> scenes;
        public string fileName;

        public Dialogue(string id)
        {
            this.fileName = id;
            this.scenes = new List<Scene>();
        }
        public JObject Build()
        {
            return new JObject()
            {
                ["format_version"] = FormatVersion.b_DIALOGUE.ToString(),
                ["minecraft:npc_dialogue"] = new JObject()
                {
                    ["scenes"] = new JArray(from scene in scenes select scene.Build())
                }
            };
        }

        public string CommandReference => null;
        public string GetExtendedDirectory() => null;
        public byte[] GetOutputData()
        {
            JObject built = this.Build();
            string dataString = built.ToString(Newtonsoft.Json.Formatting.Indented);
            return Encoding.UTF8.GetBytes(dataString);
        }
        public string GetOutputFile() => fileName + ".json";
        public OutputLocation GetOutputLocation() => OutputLocation.b_DIALOGUE;
    }
}
