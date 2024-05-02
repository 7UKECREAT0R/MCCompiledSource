using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest.Modules
{
    /// <summary>
    /// Represents a module which routes to/enables Script API in a project.
    /// </summary>
    public class ScriptModule : Module
    {
        private readonly ScriptLanguage language;
        private readonly string entry;

        public ScriptModule(Guid? uuid, ScriptLanguage language, string entry) : 
            base(ModuleType.script, uuid, ManifestVersion.DEFAULT)
        {
            this.language = language;
            this.entry = entry;
        }

        public override JObject ToJSON()
        {
            return new JObject
            {
                ["type"] = ModuleType.script.ToString(),
                ["language"] = this.language.ToString(),
                ["uuid"] = this.uuid.ToString(),
                ["entry"] = this.entry,
                ["version"] = this.moduleVersion.ToJSON()
            };
        }
    }
    /// <summary>
    /// List of supported Script API languages for the 'language' field in a module.
    /// </summary>
    public enum ScriptLanguage
    {
        javascript
    }
}