using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            JObject json = new JObject()
            {
                ["identifier"] = identifier,
                ["is_spawnable"] = isSpawnable,
                ["is_summonable"] = isSpawnable,
                ["is_experimental"] = isExperimental
            };

            if (runtimeIdentifier != null)
                json["runtime_identifier"] = runtimeIdentifier;

            return json;
        }
        /// <summary>
        /// Get the entity name without namespace.
        /// </summary>
        /// <returns></returns>
        public string GetEntityName()
        {
            int i = identifier.IndexOf(':');
            if (i == -1) return identifier;
            return identifier.Substring(i + 1);
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
