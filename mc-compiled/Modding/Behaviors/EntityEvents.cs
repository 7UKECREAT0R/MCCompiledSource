using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors;

public class EntityEventHandler
{
    public EntityEventAction action;
    public string eventID;
    public EventSubject target;

    /// <summary>
    ///     Creates a new unregistered event handler.
    /// </summary>
    /// <param name="eventID"></param>
    /// <param name="target"></param>
    /// <param name="action"></param>
    internal EntityEventHandler(string eventID, EventSubject target = EventSubject.self,
        EntityEventAction action = null)
    {
        this.eventID = eventID;
        this.target = target;
        this.action = action;
    }
    public JObject ToComponentJSON()
    {
        return new JObject
        {
            ["event"] = this.eventID,
            ["target"] = this.target.ToString()
        };
    }
    public JProperty ToDefinitionJSON()
    {
        JProperty item = this.action.ToJSON();
        var json = new JObject();
        json.Add(item);

        return new JProperty(this.eventID, json);
    }
}

public abstract class EntityEventAction
{
    public abstract JProperty ToJSON();
}

public class EventActionAddGroup : EntityEventAction
{
    public List<string> groups;

    public EventActionAddGroup(params string[] groups)
    {
        this.groups = [..groups];
    }
    public EventActionAddGroup(params EntityComponentGroup[] groups)
    {
        this.groups = groups.Select(group => group.name).ToList();
    }
    public override JProperty ToJSON()
    {
        return new JProperty("add", new JObject
        {
            ["component_groups"] = new JArray(this.groups)
        });
    }
}

public class EventActionRemoveGroup : EntityEventAction
{
    public List<string> groups;

    public EventActionRemoveGroup(params string[] groups)
    {
        this.groups = [..groups];
    }
    public EventActionRemoveGroup(params EntityComponentGroup[] groups)
    {
        this.groups = groups.Select(group => group.name).ToList();
    }
    public override JProperty ToJSON()
    {
        return new JProperty("remove", new JObject
        {
            ["component_groups"] = new JArray(this.groups)
        });
    }
}

public class EventActionSequence : EntityEventAction
{
    public List<EntityEventAction> actions;

    public EventActionSequence(params EntityEventAction[] actions)
    {
        this.actions = [..actions];
    }
    public override JProperty ToJSON()
    {
        object[] objects = new object[this.actions.Count];
        for (int i = 0; i < this.actions.Count; i++)
        {
            var json = new JObject {this.actions[i].ToJSON()};
            objects[i] = json;
        }

        return new JProperty("sequence", new JArray(objects));
    }
}