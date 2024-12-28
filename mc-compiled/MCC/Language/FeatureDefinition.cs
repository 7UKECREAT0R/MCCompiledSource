using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Language;

public readonly struct FeatureDefinition(
    string name,
    string description,
    string details,
    decimal minVersion,
    uint value)
{
    /// Creates a new instance of the FeatureDefinition struct from a JSON property.
    /// <param name="property">
    ///     The JSON property representing a feature. The property's value must be a JSON object
    ///     containing keys for "description", "details", "minVersion", and "value".
    /// </param>
    /// <returns>
    ///     A FeatureDefinition instance initialized with data parsed from the provided JSON property.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the value of the provided JSON property is not a JSON object.
    /// </exception>
    public static FeatureDefinition FromJSONProperty(JProperty property)
    {
        string name = property.Name;

        if (property.Value is not JObject contents)
            throw new ArgumentException("Feature property contents must be an object.");

        return new FeatureDefinition(
            name,
            contents["description"]?.ToString() ?? "",
            contents["details"]?.ToString() ?? "",
            contents["minVersion"]?.ToObject<decimal>() ?? 0,
            contents["value"]?.ToObject<uint>() ?? 0
        );
    }
}