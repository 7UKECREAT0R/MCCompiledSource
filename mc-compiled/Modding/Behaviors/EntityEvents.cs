using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    public struct EntityEventHandler
    {
        public string eventID;
        public EventSubject target;
        public EntityEventAction action;

        /// <summary>
        /// Creates a new unregistered event handler.
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="target"></param>
        /// <param name="action"></param>
        internal EntityEventHandler(string eventID, EventSubject target = EventSubject.self, EntityEventAction action = null)
        {
            this.eventID = eventID;
            this.target = target;
            this.action = action;
        }
        public JObject ToComponentJSON() =>
            new JObject()
            {
                ["event"] = eventID,
                ["target"] = target.ToString()
            };
        public JProperty ToDefinitionJSON()
        {
            JProperty item = action.ToJSON();
            JObject json = new JObject();
            json.Add(item);

            return new JProperty(eventID, json);
        }
    }
    public abstract class EntityEventAction
    {
        public abstract JProperty ToJSON();
    }
    public class EventActionAddGroup : EntityEventAction
    {
        public string[] groups;

        public EventActionAddGroup(params string[] groups)
        {
            this.groups = groups;
        }
        public EventActionAddGroup(params EntityComponentGroup[] groups)
        {
            this.groups = groups.Select(group => group.name).ToArray();
        }
        public override JProperty ToJSON()
        {
            return new JProperty("add", new JObject()
            {
                ["component_groups"] = new JArray(groups)
            });
        }
    }
    public class EventActionRemoveGroup : EntityEventAction
    {
        public string[] groups;

        public EventActionRemoveGroup(params string[] groups)
        {
            this.groups = groups;
        }
        public EventActionRemoveGroup(params EntityComponentGroup[] groups)
        {
            this.groups = groups.Select(group => group.name).ToArray();
        }
        public override JProperty ToJSON()
        {
            return new JProperty("remove", new JObject()
            {
                ["component_groups"] = new JArray(groups)
            });
        }
    }
    public class EventActionSequence : EntityEventAction
    {
        public EntityEventAction[] actions;

        public EventActionSequence(params EntityEventAction[] actions)
        {
            this.actions = actions;
        }
        public override JProperty ToJSON()
        {
            JObject[] objects = new JObject[actions.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                JObject json = new JObject();
                json.Add(actions[i].ToJSON());
                objects[i] = json;
            }

            return new JProperty("sequence", new JArray(objects));
        }
    }
}
