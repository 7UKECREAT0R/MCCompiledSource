using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    public abstract class EntityComponent
    {
        public int? priority = null;

        /// <summary>
        /// The identifier of this component.
        /// </summary>
        public abstract string GetIdentifier();

        /// <summary>
        /// Get the object inside this component without priority.
        /// </summary>
        /// <returns>A non-null JObject that holds the data for this component.</returns>
        public abstract Newtonsoft.Json.Linq.JObject _GetValue();



        /// <summary>
        /// Get the object to be used inside this component.
        /// </summary>
        /// <returns></returns>
        public Newtonsoft.Json.Linq.JObject GetValue()
        {
            var value = _GetValue();
            if (priority.HasValue)
                value["priority"] = priority.Value;
            return value;
        }
    }
    public struct EntityEventHandler
    {
        public string eventID;
        public EventSubject target;

        public EntityEventHandler(string eventID, EventSubject target = EventSubject.self)
        {
            this.eventID = eventID;
            this.target = target;
        }
        public Newtonsoft.Json.Linq.JObject ToJSON() =>
            new Newtonsoft.Json.Linq.JObject()
            {
                ["event"] = eventID,
                ["target"] = target.ToString()
            };
    }
}
