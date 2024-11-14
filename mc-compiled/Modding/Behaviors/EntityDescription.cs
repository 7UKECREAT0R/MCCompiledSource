using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors
{
    public struct EntityDescription
    {
        public readonly string identifier;
        public readonly bool isSummonable;
        public readonly bool isSpawnable;
        public readonly bool isExperimental;
        public string runtimeIdentifier;

        public EntityDescription(string identifier, bool isSummonable = true, bool isSpawnable = false,
            bool isExperimental = false, string runtimeIdentifier = null)
        {
            this.identifier = identifier;
            this.isSummonable = isSummonable;
            this.isSpawnable = isSpawnable;
            this.isExperimental = isExperimental;
            this.runtimeIdentifier = runtimeIdentifier;
        }
        public JObject ToJSON()
        {
            JObject json = new JObject
            {
                ["identifier"] = this.identifier,
                ["is_spawnable"] = this.isSpawnable,
                ["is_summonable"] = this.isSummonable,
                ["is_experimental"] = this.isExperimental
            };

            if (this.runtimeIdentifier != null)
                json["runtime_identifier"] = this.runtimeIdentifier;

            return json;
        }
        /// <summary>
        /// Get the entity name without namespace.
        /// </summary>
        /// <returns></returns>
        public string GetEntityName()
        {
            string stripped = this.identifier;

            if (stripped.EndsWith(".json"))
                stripped = stripped[..^5];

            int slash = stripped.LastIndexOf('/');
            if (slash != -1)
                stripped = stripped[(slash + 1)..];

            int i = stripped.IndexOf(':');

            if (i == -1)
                return stripped;

            return stripped[(i + 1)..];
        }
    }
    public abstract class EntityScript
    {
        public abstract JProperty AsProperty();
    }
    public sealed class EntityScriptAnimate : EntityScript
    {

        public override JProperty AsProperty()
        {
            throw new NotImplementedException();
        }
    }
}
