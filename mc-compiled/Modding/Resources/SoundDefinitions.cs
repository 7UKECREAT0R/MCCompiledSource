using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Resources
{
    /// <summary>
    /// Represents the `sounds/sound_definitions.json` file in the RP.
    /// </summary>
    public class SoundDefinitions : IAddonFile
    {
        public const string FILE = "sound_definitions.json";
        
        private readonly string formatVersion;
        private readonly Dictionary<string, SoundDefinition> soundDefinitions;

        public void AddSoundDefinition(SoundDefinition soundDefinition)
        {
            this.soundDefinitions[soundDefinition.CommandReference] = soundDefinition;
        }
        internal SoundDefinitions(string formatVersion, params SoundDefinition[] definitions)
        {
            this.formatVersion = formatVersion;
            this.soundDefinitions = new Dictionary<string, SoundDefinition>();

            foreach (SoundDefinition definition in definitions)
                AddSoundDefinition(definition);
        }
        public static SoundDefinitions Parse(string _json, Statement callingStatement)
        {
            JObject json = JObject.Parse(_json);
            return Parse(json, callingStatement);
        }

        public static SoundDefinitions Parse(JObject json, Statement callingStatement)
        {
            string formatVersion = (json["format_version"] ?? throw new StatementException(callingStatement,
                $"{FILE} file is missing 'format_version' field.")).ToString();
            JObject soundDefinitionsObject = json["sound_definitions"] as JObject ?? throw new StatementException(
                callingStatement, $"{FILE} file is missing 'sound_definitions' field.");

            List<SoundDefinition> soundDefinitions = (
                from property in soundDefinitionsObject.Properties()
                let name = property.Name
                select SoundDefinition.Parse(name, property.Value as JObject, callingStatement)
            ).ToList();

            return new SoundDefinitions(formatVersion, soundDefinitions.ToArray());
        }
        private JObject ToJSON()
        {
            var defs = new JObject();
            foreach (JProperty soundProperty in this.soundDefinitions
                         .Select(soundDefinition => soundDefinition.Value.ToJSON()))
            {
                defs.Add(soundProperty);
            }

            return new JObject
            {
                ["format_version"] = this.formatVersion,
                ["sound_definitions"] = defs
            };
        }

        /// <summary>
        /// Attempts to retrieve a sound definition from the `SoundDefinitions` class.
        /// </summary>
        /// <param name="key">The key of the sound definition to retrieve.</param>
        /// <param name="soundDefinition">When this method returns, contains the sound definition associated with the specified key, if the key was found; otherwise, null.</param>
        /// <returns>true if the sound definition was found; otherwise, false.</returns>
        public bool TryGetSoundDefinition(string key, out SoundDefinition soundDefinition)
        {
            return this.soundDefinitions.TryGetValue(key, out soundDefinition);
        }
        /// <summary>
        /// Retrieves a sound definition by its key.
        /// </summary>
        /// <param name="key">The key of the sound definition to retrieve.</param>
        /// <returns>The sound definition corresponding to the specified key.</returns>
        public SoundDefinition GetSoundDefinition(string key)
        {
            if (!this.soundDefinitions.ContainsKey(key))
                throw new KeyNotFoundException($"The key {key} does not exist in the sound definitions.");
            return this.soundDefinitions[key];
        }
        
        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;
        public string GetOutputFile() => FILE;
        public byte[] GetOutputData()
        {
            JObject output = ToJSON();
            return Encoding.UTF8.GetBytes(output.ToString());
        }
        public OutputLocation GetOutputLocation() => OutputLocation.r_SOUNDS;
    }

    /// <summary>
    /// Represents a sound definition.
    /// </summary>
    public class SoundDefinition
    {
        public string CommandReference => this.name;

        /// <summary>
        /// Represents a sound definition.
        /// </summary>
        internal SoundDefinition(string name, string fileName, SoundCategory category, params string[] sounds)
        {
            this.name = name;
            this.category = category;
            this.sounds = sounds.Select(file => file.Replace(Path.DirectorySeparatorChar, '/')).ToArray();
        }
        /// <summary>
        /// Parses a sound definition from a JSON object.
        /// </summary>
        /// <param name="name">The name of the sound definition.</param>
        /// <param name="json">The JSON object containing the sound definition.</param>
        /// <param name="callingStatement">The calling statement.</param>
        /// <returns>A new SoundDefinition instance.</returns>
        public static SoundDefinition Parse(string name, JObject json, Statement callingStatement)
        {
            var sounds = new List<string>();
            JArray soundsArray = json["sounds"] as JArray ?? throw new StatementException(callingStatement, $"Sound definition '{name}' missing 'sounds' array.");
            JToken categoryString = json["category"] ?? throw new StatementException(callingStatement, $"Sound definition '{name}' was missing 'category' string.");
            var category = (SoundCategory) Enum.Parse(typeof(SoundCategory), categoryString.ToString());
            
            foreach(JToken token in soundsArray)
            {
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (token.Type)
                {
                    case JTokenType.String:
                        sounds.Add(token.Value<string>());
                        break;
                    case JTokenType.Object:
                        var obj = token.Value<JObject>();
                        JToken nameField = obj["name"];
                        if (nameField == null)
                            throw new StatementException(callingStatement, $"Sound definition '{name}' ({SoundDefinitions.FILE}) sounds array contains foreign object.");
                        sounds.Add(nameField.ToString());
                        break;
                    default:
                        throw new StatementException(callingStatement,
                            $"Unexpected object in sound definition '{name}' ({SoundDefinitions.FILE}) sounds array: {token.Type}?");
                }
            }
            
            return new SoundDefinition(name, name, category, sounds.ToArray());
        }
        public JProperty ToJSON()
        {
            var jObject = new JObject
            {
                ["category"] = this.category.ToString(),
                ["sounds"] = new JArray(this.sounds.Cast<object>().ToArray())
            };

            if (this.category == SoundCategory.ui)
            {
                jObject["min_distance"] = 999999f;
            }
            
            return new JProperty(this.name, jObject);
        }

        private readonly string name;
        private readonly SoundCategory category;
        private readonly string[] sounds;
    }
    
    [EnumParsable(typeof(SoundCategory))]
    public enum SoundCategory
    {
        [UsedImplicitly] ambient,
        [UsedImplicitly] block,
        [UsedImplicitly] bottle,
        [UsedImplicitly] bucket,
        [UsedImplicitly] hostile,
        [UsedImplicitly] music,
        [UsedImplicitly] neutral,
        [UsedImplicitly] player,
        [UsedImplicitly] record,
        [UsedImplicitly] ui,
        [UsedImplicitly] weather
    }
}