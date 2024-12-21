using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors;

public sealed class EntityComponentGroup : List<EntityComponent>
{
    public string name;
    public EntityComponentGroup(string name)
    {
        this.name = name;
    }
    public EntityComponentGroup(string name, params EntityComponent[] initial) : base(initial)
    {
        this.name = name;
    }

    public JProperty ToJSON()
    {
        var json = new JObject();
        foreach (EntityComponent component in this)
            json[component.GetIdentifier()] = component.GetValue();
        return new JProperty(this.name, json);
    }
}