using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.MCC;
using mc_compiled.NBT;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Commands;

/// <summary>
///     Static collection of properties for reading vanilla block-state information with MCCompiled.
/// </summary>
/// <remarks>
///     The files can be updated from https://github.com/Mojang/bedrock-samples/tree/main/metadata/vanilladata_modules,
///     OR you can run `vanilla-dependencies\update.ps1` to update it automatically.
/// </remarks>
public static class VanillaBlockProperties
{
    private static FrozenDictionary<string, BlockPropertyDefinition> _PROPERTIES;
    private static FrozenDictionary<string, BlockPropertyDefinition[]> _BLOCK_STATES;

    private static bool IS_LOADED;

    /// <summary>
    ///     A collection of block properties associated with vanilla block-state information.
    /// </summary>
    public static FrozenDictionary<string, BlockPropertyDefinition> Properties
    {
        get
        {
            EnsureLoaded();
            return _PROPERTIES;
        }
    }

    /// <summary>
    ///     Retrieves the block property with the specified name, if it exists.
    /// </summary>
    /// <param name="name">
    ///     The name of the block property to retrieve.
    /// </param>
    /// <returns>
    ///     The <see cref="BlockPropertyDefinition" /> for the specified <paramref name="name" />, or
    ///     <see langword="null" /> if the property is not found.
    /// </returns>
    [CanBeNull]
    public static BlockPropertyDefinition GetProperty(string name)
    {
        EnsureLoaded();
        return _PROPERTIES.GetValueOrDefault(name);
    }

    /// <summary>
    ///     Determines whether a block property with the specified name exists in the internal collection.
    /// </summary>
    /// <param name="name">
    ///     The name of the block property to check for.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the specified <paramref name="name" /> exists in the collection; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool PropertyExists(string name)
    {
        EnsureLoaded();
        return _PROPERTIES.ContainsKey(name);
    }
    /// <summary>
    ///     Retrieves the available block properties for a given block.
    /// </summary>
    /// <param name="blockIdentifier">
    ///     The identifier of the block for which to retrieve state definitions.
    /// </param>
    /// <returns>
    ///     An array of <see cref="BlockPropertyDefinition" /> objects representing the block's state
    ///     definitions, or <see langword="null" /> if the block identifier is not found.
    /// </returns>
    public static BlockPropertyDefinition[] GetBlockStates(string blockIdentifier)
    {
        EnsureLoaded();
        return _BLOCK_STATES.GetValueOrDefault(Command.Util.RequireNamespace(blockIdentifier));
    }

    private static void EnsureLoaded()
    {
        if (IS_LOADED)
            return;

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla block state information...");

        var properties = new Dictionary<string, BlockPropertyDefinition>();

        // load all block properties
        string file = VanillaData.BLOCKS_PATH;
        if (!File.Exists(file))
        {
            Console.Error.WriteLine($"Missing file '{file}'. Block properties cannot be validated.");
            IS_LOADED = true;
            _PROPERTIES = FrozenDictionary<string, BlockPropertyDefinition>.Empty;
            return;
        }

        JObject root = JObject.Parse(File.ReadAllText(file));
        JArray blockPropertiesRoot = root["block_properties"]?.Value<JArray>() ?? [];
        foreach (JToken _blockPropertyEntry in blockPropertiesRoot)
        {
            if (_blockPropertyEntry is not JObject blockPropertyEntry)
                continue;
            BlockPropertyDefinition property = BlockPropertyDefinition.Parse(blockPropertyEntry);
            properties[property.Name] = property;
        }

        // get which properties are attached to which blocks
        var blockStates = new Dictionary<string, BlockPropertyDefinition[]>();
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;
            string blockIdentifier = dataItem["name"]?.Value<string>();
            if (string.IsNullOrEmpty(blockIdentifier))
                continue;

            JArray propertiesArray = dataItem["properties"]?.Value<JArray>() ?? [];
            if (propertiesArray.Count == 0)
                continue;

            var linkedProperties = new BlockPropertyDefinition[propertiesArray.Count];
            bool gotAllProperties = true;
            for (int i = 0; i < propertiesArray.Count; i++)
            {
                JToken _propertyNameObject = propertiesArray[i];
                if (_propertyNameObject is not JObject propertyNameObject)
                {
                    gotAllProperties = false;
                    break;
                }

                string propertyName = propertyNameObject["name"]?.Value<string>();
                if (string.IsNullOrEmpty(propertyName))
                {
                    gotAllProperties = false;
                    break;
                }

                BlockPropertyDefinition property = properties[propertyName];
                linkedProperties[i] = property;
            }

            if (!gotAllProperties)
                continue; // something bad happened

            blockStates[Command.Util.RequireNamespace(blockIdentifier)] = linkedProperties;
        }

        _PROPERTIES = properties.ToFrozenDictionary();
        _BLOCK_STATES = blockStates.ToFrozenDictionary();
        IS_LOADED = true;
    }
}

/// <summary>
///     A definition of a block property.
/// </summary>
public record BlockPropertyDefinition
{
    private BlockPropertyDefinition(string name, BlockPropertyType type, object[] possibleValues)
    {
        this.Name = name;
        this.Type = type;
        this.PossibleValues = possibleValues;
    }

    /// <summary>
    ///     The name of this block property.
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    ///     The type of this block property.
    /// </summary>
    public BlockPropertyType Type { get; init; }

    /// <summary>
    ///     The possible values for this property, in their native type.
    /// </summary>
    public object[] PossibleValues { get; }
    /// <summary>
    ///     The possible values for this property, in their string representation.
    /// </summary>
    public string[] PossibleValuesAsStrings => this.PossibleValues.Select(v => v.ToString()).ToArray();
    /// <summary>
    ///     Gets the inclusive range of consecutive integer values, if all possible values are consecutive integers.
    ///     Returns <see langword="null" /> otherwise.
    /// </summary>
    private Range? PossibleValuesIntRange
    {
        get
        {
            if (this.Type != BlockPropertyType.@int)
                return null;

            // make sure all possible values are consecutive integers
            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (int value in this.IntValues)
            {
                if (value < min)
                    min = value;
                if (value > max)
                    max = value;
            }

            for (int i = min; i <= max; i++)
                if (!this.IntValues.Contains(i))
                    return null;

            return new Range(min, max);
        }
    }
    /// <summary>
    ///     Provides a human-readable representation of the possible values for this property.
    /// </summary>
    public string PossibleValuesFriendlyString
    {
        get
        {
            return this.Type switch
            {
                BlockPropertyType.@bool => "true, false",
                BlockPropertyType.@int => this.PossibleValuesIntRange?.ToString() ?? string.Join(", ", this.IntValues),
                BlockPropertyType.@string => string.Join(", ", this.StringValues),
                _ => throw new Exception(
                    $"Unimplemented block property type '{this.Type}' in {nameof(this.PossibleValuesFriendlyString)}.")
            };
        }
    }

    /// <summary>
    ///     Returns this block property's possible values in their native string type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     If the block property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@string" />.
    /// </exception>
    public string[] StringValues =>
        this.Type == BlockPropertyType.@string
            ? GetPossibleValues<string>()
            : throw new InvalidOperationException(
                $"Block property {this.Name} was not 'string' type. (got {this.Type})");
    /// <summary>
    ///     Returns this block property's possible values in their native integer type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     If the block property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@int" />.
    /// </exception>
    public int[] IntValues =>
        this.Type == BlockPropertyType.@int
            ? GetPossibleValues<int>()
            : throw new InvalidOperationException(
                $"Block property {this.Name} was not 'int' type. (got {this.Type})");
    /// <summary>
    ///     Returns this block property's possible values in their native boolean type.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     If the block property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@bool" />.
    /// </exception>
    public bool[] BoolValues =>
        this.Type == BlockPropertyType.@bool
            ? GetPossibleValues<bool>()
            : throw new InvalidOperationException(
                $"Block property {this.Name} was not 'bool' type. (got {this.Type})");
    /// <summary>
    ///     Returns the NBT tag type that corresponds to this block property's type.<br />
    ///     Use <see cref="CreateNBTNode" /> for an easier way to create an NBT node.
    /// </summary>
    /// <exception cref="Exception">Thrown if the property's <see cref="Type" /> is not implemented here.</exception>
    public TAG NBTTagType
    {
        get
        {
            return this.Type switch
            {
                BlockPropertyType.@bool => TAG.Byte,
                BlockPropertyType.@int => TAG.Int,
                BlockPropertyType.@string => TAG.String,
                _ => throw new Exception($"Block property '{this.Name}' has an unimplemented type '{this.Type}'.")
            };
        }
    }

    /// <summary>
    ///     Parse a <see cref="BlockPropertyDefinition" /> from its JSON representation inside <c>mojang-blocks.json</c>.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static BlockPropertyDefinition Parse(JObject json)
    {
        string name = json["name"].ToString();
        var type = Enum.Parse<BlockPropertyType>(json["type"].ToString());

        if (json["values"] is not JArray possibleValuesArray)
            throw new Exception($"Values array was not present inside block property definition '{name}'.");
        int count = possibleValuesArray.Count;
        object[] possibleValues = new object[count];

        for (int i = 0; i < count; i++)
        {
            JToken _possibleValueRaw = possibleValuesArray[i];
            if (_possibleValueRaw is not JObject _possibleValue)
                continue;
            JToken possibleValue = _possibleValue["value"];
            if (possibleValue == null)
                continue;

            switch (type)
            {
                case BlockPropertyType.@bool:
                    possibleValues[i] = possibleValue.Value<bool>();
                    break;
                case BlockPropertyType.@int:
                    possibleValues[i] = possibleValue.Value<int>();
                    break;
                case BlockPropertyType.@string:
                    possibleValues[i] = possibleValue.Value<string>();
                    break;
                default:
                    throw new Exception(
                        $"The type of the block property definition '{name}' is not valid. (got '{json["type"]}')");
            }
        }

        return new BlockPropertyDefinition(name, type, possibleValues);
    }

    /// <summary>
    ///     Creates an NBT node representing the value of this block property, appropriate to the property's type.
    /// </summary>
    /// <param name="value">
    ///     The value to be serialized into an NBT node. The specific type of <paramref name="value" /> must match
    ///     the <see cref="BlockPropertyDefinition.Type" /> of this block property. For example:
    ///     <list type="bullet">
    ///         <item>
    ///             If the type is <see cref="BlockPropertyType.@bool" />, <paramref name="value" /> must be of type
    ///             <see cref="bool" />.
    ///         </item>
    ///         <item>
    ///             If the type is <see cref="BlockPropertyType.@int" />, <paramref name="value" /> must be of type
    ///             <see cref="int" />.
    ///         </item>
    ///         <item>
    ///             If the type is <see cref="BlockPropertyType.@string" />, <paramref name="value" /> must be of type
    ///             <see cref="string" />.
    ///         </item>
    ///     </list>
    /// </param>
    /// <returns>
    ///     An <see cref="NBTNode" /> that encapsulates the provided <paramref name="value" /> in the appropriate type.
    ///     The specific returned instance will be a derived type of <see cref="NBTNode" />, such as
    ///     <see cref="NBTByte" />, <see cref="NBTInt" />, or <see cref="NBTString" />, depending on
    ///     the <see cref="BlockPropertyDefinition.Type" />.
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///     Thrown if <paramref name="value" /> does not match the type expected by the
    ///     <see cref="BlockPropertyDefinition.Type" />.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown if the property's <see cref="Type" /> is not implemented.
    /// </exception>
    public NBTNode CreateNBTNode(object value)
    {
        return this.Type switch
        {
            BlockPropertyType.@bool => CreateNBTBool((bool) value),
            BlockPropertyType.@int => CreateNBTInt((int) value),
            BlockPropertyType.@string => CreateNBTString((string) value),
            _ => throw new Exception($"Block property '{this.Name}' has an unimplemented type '{this.Type}'.")
        };
    }
    /// <summary>
    ///     Creates an NBT string based on this block property.
    /// </summary>
    /// <param name="value">The value to place inside the node.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     If this property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@string" />
    /// </exception>
    public NBTString CreateNBTString(string value)
    {
        if (this.Type != BlockPropertyType.@string)
            throw new InvalidOperationException(
                $"Passed a string to {nameof(CreateNBTString)} when its type is '{this.Type}'");

        return new NBTString
        {
            name = this.Name,
            value = value
        };
    }
    /// <summary>
    ///     Creates an NBT integer based on this block property.
    /// </summary>
    /// <param name="value">The value to place inside the node.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     If this property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@int" />
    /// </exception>
    public NBTInt CreateNBTInt(int value)
    {
        if (this.Type != BlockPropertyType.@int)
            throw new InvalidOperationException(
                $"Passed an integer to {nameof(CreateNBTInt)} when its type is '{this.Type}'");

        return new NBTInt
        {
            name = this.Name,
            value = value
        };
    }
    /// <summary>
    ///     Creates an NBT boolean based on this block property.
    /// </summary>
    /// <param name="value">The value to place inside the node.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     If this property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@bool" />
    /// </exception>
    public NBTByte CreateNBTBool(bool value)
    {
        if (this.Type != BlockPropertyType.@bool)
            throw new InvalidOperationException(
                $"Passed a boolean to {nameof(CreateNBTBool)} when its type is '{this.Type}'");

        return new NBTByte
        {
            name = this.Name,
            value = value ? (byte) 1 : (byte) 0
        };
    }

    /// <summary>
    ///     Returns <see cref="PossibleValues" /> but with each value cast to <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T[] GetPossibleValues<T>() { return this.PossibleValues.Cast<T>().ToArray(); }

    /// <summary>
    ///     Determines if the specified <paramref name="value" /> is a valid value for the block property.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    ///     <see langword="true" /> if the <paramref name="value" /> is valid for this block property;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValidValueGeneric(object value) { return this.PossibleValues.Contains(value); }

    /// <summary>
    ///     Determines if the specified <paramref name="value" /> is a valid integer value for the block property.
    /// </summary>
    /// <param name="value">
    ///     The integer value to validate.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the <paramref name="value" /> is valid for this block property;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValidValue(int value)
    {
        return this.Type == BlockPropertyType.@int && this.IntValues.Contains(value);
    }

    /// <summary>
    ///     Determines if the specified <paramref name="value" /> is a valid boolean value for the block property.
    /// </summary>
    /// <param name="value">
    ///     The boolean value to validate.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the <paramref name="value" /> is valid for this block property;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValidValue(bool value)
    {
        return this.Type == BlockPropertyType.@bool && this.BoolValues.Contains(value);
    }

    /// <summary>
    ///     Determines if the specified <paramref name="value" /> is a valid string value for the block property.
    /// </summary>
    /// <param name="value">
    ///     The string value to validate.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the <paramref name="value" /> is valid for this block property;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValidValue(string value)
    {
        return this.Type == BlockPropertyType.@string && this.StringValues.Contains(value);
    }
}

/// <summary>
///     A type associated with a block property.
/// </summary>
public enum BlockPropertyType
{
    @bool, @int, @string
}