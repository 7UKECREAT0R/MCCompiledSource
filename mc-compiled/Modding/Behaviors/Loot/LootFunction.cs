using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Function which will run on loot before being dropped.
/// </summary>
public abstract class LootFunction
{
    /// <summary>
    ///     Convert this function to JSON.
    /// </summary>
    /// <returns></returns>
    public JObject ToJSON()
    {
        var json = new JObject();
        json["function"] = GetFunctionName();
        foreach (JObject function in GetFunctionFields())
        foreach (JProperty property in function.Properties())
            json.Add(property);

        return json;
    }

    public abstract string GetFunctionName();
    public abstract JObject[] GetFunctionFields();
}