using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.MCC.Compiler;
using mc_compiled.NBT;

namespace mc_compiled.Commands.Native;

/// <summary>
///     The state of a block. Basically just a value tied to a <see cref="BlockPropertyDefinition" />.
/// </summary>
public readonly struct BlockState(
    [CanBeNull] BlockPropertyDefinition definition,
    string propertyName,
    object value,
    BlockPropertyType valueType)
{
    /// <summary>
    ///     The definition of the property this entry refers to.
    ///     If null, there's no valid vanilla property this state refers to; however, it may be a custom property.
    ///     Validation should be done on an as-needed basis, like if this <see cref="BlockState" /> is paired with a
    ///     known vanilla block.
    /// </summary>
    [CanBeNull]
    public readonly BlockPropertyDefinition definition = definition;
    /// <summary>
    ///     A never-null name of the property set by this entry. The <see cref="definition" /> can be null, but this cannot.
    /// </summary>
    public readonly string propertyName = propertyName;

    /// <summary>
    ///     The value of the property set by this entry.
    /// </summary>
    public readonly object value = value;
    /// <summary>
    ///     The type of the <see cref="value" /> field.
    /// </summary>
    private readonly BlockPropertyType valueType = valueType;

    // constructors for convenience
    public BlockState(BlockPropertyDefinition definition, bool value) : this(definition, definition.Name, value,
        BlockPropertyType.@bool) { }
    public BlockState(BlockPropertyDefinition definition, int value) : this(definition, definition.Name, value,
        BlockPropertyType.@int) { }
    public BlockState(BlockPropertyDefinition definition, string value) : this(definition, definition.Name, value,
        BlockPropertyType.@string) { }
    public BlockState(string unknownPropertyName, bool value) : this(null, unknownPropertyName, value,
        BlockPropertyType.@bool) { }
    public BlockState(string unknownPropertyName, int value) : this(null, unknownPropertyName, value,
        BlockPropertyType.@int) { }
    public BlockState(string unknownPropertyName, string value) : this(null, unknownPropertyName, value,
        BlockPropertyType.@string) { }

    /// <summary>
    ///     Creates an enumerable of <see cref="BlockState" /> objects from an array of <see cref="Token" />s.
    /// </summary>
    /// <param name="tokens">
    ///     An array of <see cref="Token" /> objects representing the data to construct block states.
    /// </param>
    /// <param name="forExceptions">
    ///     An optional <see cref="Statement" /> providing context for exceptions, if any are thrown during processing.
    ///     This parameter can be <see langword="null" />, which will cause this method to return an empty enumerable
    ///     instead of throwing an exception.
    /// </param>
    /// <returns>
    ///     An enumerable of <see cref="BlockState" /> objects, or none if the input is invalid or empty and
    ///     <paramref name="forExceptions" /> was not supplied.
    /// </returns>
    public static IEnumerable<BlockState> FromTokens(Token[] tokens, Statement forExceptions = null)
    {
        int length = tokens.Length;

        if (length == 0)
        {
            if (forExceptions != null)
                throw new StatementException(forExceptions, "No tokens to make a block state entry.");
            yield break;
        }

        if (length % 3 != 0)
        {
            if (forExceptions != null)
                throw new StatementException(forExceptions,
                    length < 3
                        ? "Not enough tokens to make a block state entry."
                        : $"Not enough tokens to make {length / 3 + 1} block state entries.");
            yield break;
        }

        // take groups of three
        for (int i = 0; i < length; i += 3)
        {
            Token a = tokens[i];
            Token b = tokens[i + 1];
            Token c = tokens[i + 2];
            BlockState? state = FromTokens(a, b, c, forExceptions);
            if (state.HasValue)
                yield return state.Value;
        }
    }
    /// <summary>
    ///     Attempts to create a <see cref="BlockState" /> from the given tokens.
    /// </summary>
    /// <param name="a">
    ///     The first <see cref="Token" /> representing the block property name. Must be a string or identifier token.
    /// </param>
    /// <param name="b">
    ///     The second <see cref="Token" /> expected to be an equality sign token.
    /// </param>
    /// <param name="c">
    ///     The third <see cref="Token" /> representing the block property value. Must be a boolean, integer, or string
    ///     literal.
    /// </param>
    /// <param name="forExceptions">
    ///     Optional context for exceptions. If <see langword="null" />, the method will return <see langword="null" />
    ///     instead of throwing exceptions for invalid input.
    /// </param>
    /// <returns>
    ///     A <see cref="BlockState" /> representing the parsed block property, or <see langword="null" /> if the
    ///     provided tokens are invalid and <paramref name="forExceptions" /> was not supplied.
    /// </returns>
    public static BlockState? FromTokens(Token a, Token b, Token c, Statement forExceptions = null)
    {
        string blockPropertyName;
        if (a is TokenStringLiteral _stringLiteral)
            blockPropertyName = _stringLiteral.text;
        else if (a is TokenIdentifier _identifier)
            blockPropertyName = _identifier.word;
        else
            return forExceptions != null
                ? throw new StatementException(forExceptions, "Unexpected token for block property: " + a.DebugString())
                : null;

        BlockPropertyDefinition blockProperty = VanillaBlockProperties.GetProperty(blockPropertyName);

        if (b is not TokenAssignment)
            return forExceptions != null
                ? throw new StatementException(forExceptions,
                    "Expected an equals sign '=' after the block property name, got: " + a.DebugString())
                : null;

        // it's possible the user has defined a non-vanilla property.
        if (blockProperty == null)
            switch (c)
            {
                case TokenBooleanLiteral booleanLiteral:
                    blockProperty = BlockPropertyDefinition.Placeholder(blockPropertyName, BlockPropertyType.@bool,
                        booleanLiteral.boolean);
                    return new BlockState(blockProperty, booleanLiteral.boolean);
                case TokenIntegerLiteral integerLiteral:
                    blockProperty = BlockPropertyDefinition.Placeholder(blockPropertyName, BlockPropertyType.@int,
                        integerLiteral.number);
                    return new BlockState(blockProperty, integerLiteral.number);
                case TokenStringLiteral stringLiteral:
                    blockProperty = BlockPropertyDefinition.Placeholder(blockPropertyName, BlockPropertyType.@string,
                        stringLiteral.text);
                    return new BlockState(blockProperty, stringLiteral.text);
                case TokenIdentifier stringButDifferent:
                    blockProperty = BlockPropertyDefinition.Placeholder(blockPropertyName, BlockPropertyType.@string,
                        stringButDifferent.word);
                    return new BlockState(blockProperty, stringButDifferent.word);
                default:
                    return forExceptions != null
                        ? throw new StatementException(forExceptions,
                            $"Value '{c.AsString()}' is not a valid value for a block property.")
                        : null;
            }

        switch (c)
        {
            case TokenBooleanLiteral booleanLiteral:
                bool cBool = booleanLiteral.boolean;
                return new BlockState(blockProperty, cBool);
            case TokenIntegerLiteral integerLiteral:
                int cInt = integerLiteral.number;
                return new BlockState(blockProperty, cInt);
            case TokenStringLiteral stringLiteral:
                string cString = stringLiteral.text;
                return new BlockState(blockProperty, cString);
            case TokenIdentifier stringButDifferent:
                string cIdentifier = stringButDifferent.word;
                return new BlockState(blockProperty, cIdentifier);
            default:
                return forExceptions != null
                    ? throw new StatementException(forExceptions,
                        $"Value '{c.AsString()}' is not a valid value for block property '{blockPropertyName}'. Available options include: {blockProperty.PossibleValuesFriendlyString}")
                    : null;
        }
    }

    /// <summary>
    ///     Returns if this block state entry is valid based on if a <see cref="value" /> is present and its type matches the
    ///     <see cref="definition" />.
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (this.value == null)
                return false;
            if (this.definition == null)
                return true; // technically always valid if there are no restrictions?
            if (this.valueType != this.definition.Type)
                return false;

            return this.valueType switch
            {
                BlockPropertyType.@bool => this.value is bool boolValue && this.definition.IsValidValue(boolValue),
                BlockPropertyType.@int => this.value is int intValue && this.definition.IsValidValue(intValue),
                BlockPropertyType.@string => this.value is string strValue && this.definition.IsValidValue(strValue),
                _ => throw new Exception(
                    $"Block property type \"{this.valueType}\" is not implemented for {nameof(BlockState)}#{nameof(this.IsValid)}.")
            };
        }
    }

    public override string ToString()
    {
        return this.valueType switch
        {
            BlockPropertyType.@bool => $"\"{this.propertyName}\"={this.value.ToString()!.ToLower()}",
            BlockPropertyType.@int => $"\"{this.propertyName}\"={this.value}",
            BlockPropertyType.@string => $"\"{this.propertyName}\"=\"{this.value}\"",
            _ => throw new Exception(
                $"Block property type \"{this.valueType}\" is not implemented for {nameof(BlockState)}#{nameof(ToString)}.")
        };
    }

    /// <summary>
    ///     Creates an NBT node representing the value of this block state, appropriate to the property's type.
    /// </summary>
    /// <returns>
    ///     An <see cref="NBTNode" /> that encapsulates the provided <see cref="value" /> in the appropriate type.
    ///     The specific returned instance will be a derived type of <see cref="NBTNode" />, such as
    ///     <see cref="NBTByte" />, <see cref="NBTInt" />, or <see cref="NBTString" />, depending on
    ///     the <see cref="BlockPropertyDefinition.Type" />.
    /// </returns>
    /// <exception cref="InvalidCastException">
    ///     Thrown if <see cref="value" /> does not match the type expected by the
    ///     <see cref="BlockPropertyDefinition.Type" />.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown if the property's <see cref="valueType" /> is not implemented.
    /// </exception>
    public NBTNode CreateNBTNode()
    {
        return this.valueType switch
        {
            BlockPropertyType.@bool => CreateNBTBool((bool) this.value),
            BlockPropertyType.@int => CreateNBTInt((int) this.value),
            BlockPropertyType.@string => CreateNBTString((string) this.value),
            _ => throw new Exception($"Method '{nameof(CreateNBTNode)}' has an unimplemented type '{this.valueType}'.")
        };
    }
    /// <summary>
    ///     Creates an NBT string based on this block state.
    /// </summary>
    /// <param name="nbtValue">The value to place inside the node.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     If this property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@string" />
    /// </exception>
    private NBTString CreateNBTString(string nbtValue)
    {
        return new NBTString
        {
            name = this.propertyName,
            value = nbtValue
        };
    }
    /// <summary>
    ///     Creates an NBT integer based on this block state.
    /// </summary>
    /// <param name="nbtValue">The value to place inside the node.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     If this property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@int" />
    /// </exception>
    private NBTInt CreateNBTInt(int nbtValue)
    {
        return new NBTInt
        {
            name = this.propertyName,
            value = nbtValue
        };
    }
    /// <summary>
    ///     Creates an NBT boolean based on this block state.
    /// </summary>
    /// <param name="nbtValue">The value to place inside the node.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    ///     If this property's <see cref="Type" /> is not
    ///     <see cref="BlockPropertyType.@bool" />
    /// </exception>
    private NBTByte CreateNBTBool(bool nbtValue)
    {
        return new NBTByte
        {
            name = this.propertyName,
            value = nbtValue ? (byte) 1 : (byte) 0
        };
    }
}

public static class BlockStateExtensions
{
    /// <summary>
    ///     Converts an array of <see cref="BlockState" /> objects to a vanilla Minecraft syntax string representation;
    ///     e.g.:
    ///     <code>
    /// ["button_pressed_bit"=true,"facing"="north"]
    /// </code>
    /// </summary>
    /// <returns>
    ///     A string representing the block states in vanilla Minecraft syntax.
    /// </returns>
    public static string ToVanillaSyntax([CanBeNull] this BlockState[] states)
    {
        if (states == null || states.Length == 0)
            return "[]";
        return '[' + string.Join(",", states) + ']';
    }
    public static NBTNode[] ToNBT([CanBeNull] this BlockState[] states)
    {
        if (states == null || states.Length == 0)
            return [];

        return states.Select(s => s.CreateNBTNode()).ToArray();
    }

    /// <summary>
    ///     Validate this array of block states if the provided <paramref name="block" /> is a known vanilla block.
    /// </summary>
    /// <param name="states">The block states to validate.</param>
    /// <param name="block">The block identifier to look up. Doesn't require a namespace.</param>
    /// <param name="isForComparison">
    ///     If this validation is for comparison. If <see langword="true" />, an additional check is
    ///     performed to ensure all possible states for the block are accounted for.
    /// </param>
    /// <param name="callingStatement">The calling statement to attach to any thrown <see cref="StatementException" />.</param>
    /// <exception cref="StatementException">
    ///     If <paramref name="block" /> resolves to a known vanilla block and something is
    ///     wrong with the provided block states.
    /// </exception>
    public static void ValidateIfKnownVanillaBlock(this BlockState[] states,
        string block,
        bool isForComparison,
        Statement callingStatement)
    {
        if (states == null || states.Length == 0)
            return;

        // if the block is in the vanilla registry, we can validate the comparison
        BlockPropertyDefinition[] vanillaProperties = VanillaBlockProperties.GetBlockStates(block);

        if (vanillaProperties is {Length: > 0})
        {
            // - if a state is invalid
            foreach (BlockState state in states)
                if (!state.IsValid)
                    throw new StatementException(callingStatement, state.definition != null
                        ? $"The block property '{state.propertyName}' cannot accept the value '{state.value}'. Possible options include: {state.definition.PossibleValuesFriendlyString}"
                        : $"The block property '{state.propertyName}' cannot accept the value '{state.value}'."); // never occurs

            // - if a state is missing; a comparison will always fail
            if (isForComparison)
                foreach (BlockPropertyDefinition vanillaProperty in vanillaProperties)
                    if (!states.Any(s => s.propertyName.Equals(vanillaProperty.Name)))
                        throw new StatementException(callingStatement,
                            $"Missing check for the block property '{vanillaProperty.Name}'. Possible options include: {vanillaProperty.PossibleValuesFriendlyString}");

            // - if a state is present that will never be on the block; the comparison will always fail
            foreach (BlockState state in states)
                if (!vanillaProperties.Any(property => property.Name.Equals(state.propertyName)))
                    throw new StatementException(callingStatement, isForComparison
                        ? $"Block property '{state.propertyName}' will never be found on the block '{block}'."
                        : $"Block property '{state.propertyName}' can't be set for the block '{block}'.");
        }
    }
}