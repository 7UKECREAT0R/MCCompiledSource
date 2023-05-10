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
        public string id; // the ID of this dialogue. example: "npc3page2"

        public Dialogue(string id)
        {
            this.id = id;
            this.scenes = new List<Scene>();
        }

        public string CommandReference => throw new NotImplementedException();

        public string GetExtendedDirectory() => null;

        public byte[] GetOutputData()
        {
            throw new NotImplementedException();
        }
        public string GetOutputFile() => id + ".json";
        public OutputLocation GetOutputLocation() => OutputLocation.b_DIALOGUE;
    }
}
