using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors;

public abstract class Filter
{
    public FilterOperator check;

    public EventSubject subject;
    /// <summary>
    ///     Get the test that occurs.
    /// </summary>
    /// <returns></returns>
    public abstract string GetTest();
    /// <summary>
    ///     Get the value to test with.
    /// </summary>
    /// <returns></returns>
    public abstract object GetValue();

    /// <summary>
    ///     Get any extra properties that go with this filter.
    /// </summary>
    /// <returns></returns>
    public abstract JProperty[] GetExtraProperties();

    public JObject ToJSON()
    {
        var json = new JObject();
        json["test"] = GetTest();
        json["subject"] = this.subject.ToString();

        JProperty[] properties = GetExtraProperties();
        if (properties != null)
            foreach (JProperty property in properties)
                json[property.Name] = property.Value;

        json["operator"] = this.check.String();
        json["value"] = GetValue().ToString();
        return json;
    }
}

/// <summary>
///     Indicates a filter which checks a boolean value.
/// </summary>
public abstract class BooleanFilter : Filter
{
    public bool checkValue;
    public override object GetValue()
    {
        return this.checkValue;
    }
}

/// <summary>
///     Operators used in filter tests.
/// </summary>
public enum FilterOperator
{
    EQUAL,
    UNEQUAL,
    LESS,
    GREATER,
    LESS_EQUAL,
    GREATER_EQUAL
}

public static class FilterOperatorExtensions
{
    public static string String(this FilterOperator @operator)
    {
        return @operator switch
        {
            FilterOperator.EQUAL => "equals",
            FilterOperator.UNEQUAL => "not",
            FilterOperator.LESS => "<",
            FilterOperator.GREATER => ">",
            FilterOperator.LESS_EQUAL => "<=",
            FilterOperator.GREATER_EQUAL => ">=",
            _ => "??"
        };
    }
}