using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace mc_compiled.Modding.Behaviors.Dialogue
{
    /// <summary>
    /// Represents a set of NPC dialogue scenes.
    /// </summary>
    public class DialogueManager : IAddonFile
    {
        public readonly Dictionary<string, Scene> scenes;
        private readonly string fileName;

        /// <summary>
        /// Represents a set of NPC dialogue scenes.
        /// </summary>
        public DialogueManager(string id)
        {
            this.fileName = id;
            this.scenes = new Dictionary<string, Scene>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a new scene to the Dialogue.
        /// </summary>
        /// <param name="scene">The scene to add.</param>
        public void AddScene(Scene scene)
        {
            this.scenes.Add(scene.sceneTag, scene);
        }
        /// <summary>
        /// Tries to get a specific scene from the Dialogue.
        /// </summary>
        /// <param name="sceneTag">The tag of the scene to retrieve.</param>
        /// <param name="scene">When this method returns, contains the specified scene if found, otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the scene is found, <c>false</c> otherwise.</returns>
        public bool TryGetScene(string sceneTag, out Scene scene)
        {
            return this.scenes.TryGetValue(sceneTag, out scene);
        }
        /// <summary>
        /// Retrieves a specific scene from the Dialogue.
        /// </summary>
        /// <param name="sceneTag">The tag of the scene to retrieve.</param>
        /// <returns>The specified scene if found, otherwise null.</returns>
        [CanBeNull]
        public Scene GetScene(string sceneTag)
        {
            return this.scenes.TryGetValue(sceneTag, out Scene scene) ? scene : null;
        }
        
        private JObject ToJSON()
        {
            return new JObject()
            {
                ["format_version"] = FormatVersion.b_DIALOGUE.ToString(),
                ["minecraft:npc_dialogue"] = new JObject
                {
                    ["scenes"] = new JArray(from scene in this.scenes select scene.Value.ToJSON())
                }
            };
        }

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;
        public byte[] GetOutputData()
        {
            JObject built = ToJSON();
            string dataString = built.ToString(Newtonsoft.Json.Formatting.Indented);
            return Encoding.UTF8.GetBytes(dataString);
        }
        public string GetOutputFile() => this.fileName + ".json";
        public OutputLocation GetOutputLocation() => OutputLocation.b_DIALOGUE;
    }
}
