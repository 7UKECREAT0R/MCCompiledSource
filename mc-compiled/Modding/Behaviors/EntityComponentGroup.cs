using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    public sealed class EntityComponentGroup : List<EntityComponent>
    {
        public string name;
        public EntityComponentGroup(string name) : base()
        {
            this.name = name;
        }
        public EntityComponentGroup(string name, params EntityComponent[] initial) : base(initial)
        {
            this.name = name;
        }

        public JProperty ToJSON()
        {
            JObject json = new JObject();
            foreach (EntityComponent component in this)
                json[component.GetIdentifier()] = json[component.GetValue()];
            return new JProperty(name, json);
        }
    }
}
