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
public readonly struct BlockState(BlockPropertyDefinition definition, object value, BlockPropertyType valueType)
{
    /// <summary>
    ///     The definition of the property this entry refers to.
    /// </summary>
    public readonly BlockPropertyDefinition definition = definition;

    /// <summary>
    ///     The value of the property set by this entry.
    /// </summary>
    public readonly object value = value;
    /// <summary>
    ///     The type of the <see cref="value" /> field.
    /// </summary>
    private readonly BlockPropertyType valueType = valueType;

    // constructors for convenience
    public BlockState(BlockPropertyDefinition definition, bool value) : this(definition, value,
        BlockPropertyType.@bool) { }
    public BlockState(BlockPropertyDefinition definition, int value) : this(definition, value,
        BlockPropertyType.@int) { }
    public BlockState(BlockPropertyDefinition definition, string value) : this(definition, value,
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
                return !blockProperty.IsValidValue(cBool)
                    ? ThrowInvalidValue(cBool)
                    : new BlockState(blockProperty, cBool);
            case TokenIntegerLiteral integerLiteral:
                int cInt = integerLiteral.number;
                return !blockProperty.IsValidValue(cInt)
                    ? ThrowInvalidValue(cInt)
                    : new BlockState(blockProperty, cInt);
            case TokenStringLiteral stringLiteral:
                string cString = stringLiteral.text;
                return !blockProperty.IsValidValue(cString)
                    ? ThrowInvalidValue(cString)
                    : new BlockState(blockProperty, cString);
            case TokenIdentifier stringButDifferent:
                string cIdentifier = stringButDifferent.word;
                return !blockProperty.IsValidValue(cIdentifier)
                    ? ThrowInvalidValue(cIdentifier)
                    : new BlockState(blockProperty, cIdentifier);
            default:
                return forExceptions != null
                    ? throw new StatementException(forExceptions,
                        $"Value '{c.AsString()}' is not a valid value for block property '{blockPropertyName}'. Available options include: {blockProperty.PossibleValuesFriendlyString}")
                    : null;
        }

        BlockState? ThrowInvalidValue(object value)
        {
            return forExceptions != null
                ? throw new StatementException(forExceptions,
                    $"Value {value} is not a valid value for block property '{blockPropertyName}'. Available options include: {blockProperty.PossibleValuesFriendlyString}")
                : null;
        }
    }

    /// <summary>
    ///     Returns if this block state entry is valid based on if a <see cref="value" /> is present and its type matches the
    ///     <see cref="definition" />.
    /// </summary>
    public bool IsValid => this.value != null && this.valueType == this.definition.Type;

    /// <summary>
    ///     Converts the current <see cref="BlockState" /> to an <see cref="NBTNode" />.
    /// </summary>
    /// <returns>
    ///     An <see cref="NBTNode" /> representing the state of the block.
    /// </returns>
    public NBTNode ToNBT() { return this.definition.CreateNBTNode(this.value); }
    public override string ToString()
    {
        return this.valueType switch
        {
            BlockPropertyType.@bool => $"\"{this.definition.Name}\"={this.value.ToString()!.ToLower()}",
            BlockPropertyType.@int => $"\"{this.definition.Name}\"={this.value}",
            BlockPropertyType.@string => $"\"{this.definition.Name}\"=\"{this.value}\"",
            _ => throw new Exception(
                $"Block property type \"{this.valueType}\" is not implemented for {nameof(BlockState)}#{nameof(ToString)}.")
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

        return states.Select(s => s.ToNBT()).ToArray();
    }
}