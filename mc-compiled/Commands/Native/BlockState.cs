using System;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Native;

public readonly struct BlockState
{
    internal BlockState(string fieldName, string valueAsEnum)
    {
        this.FieldName = fieldName;
        this.ValueAsEnum = valueAsEnum;
        this.ValueType = BlockStateType.Enum;
        this.ValueAsBoolean = default;
        this.ValueAsInteger = default;
    }
    internal BlockState(string fieldName, bool valueAsBoolean)
    {
        this.FieldName = fieldName;
        this.ValueAsBoolean = valueAsBoolean;
        this.ValueType = BlockStateType.Boolean;
        this.ValueAsEnum = null;
        this.ValueAsInteger = default;
    }
    internal BlockState(string fieldName, int valueAsInteger)
    {
        this.FieldName = fieldName;
        this.ValueAsInteger = valueAsInteger;
        this.ValueType = BlockStateType.Integer;
        this.ValueAsBoolean = default;
        this.ValueAsEnum = null;
    }

    /// <summary>
    ///     Converts a literal token to a BlockState object.
    /// </summary>
    /// <param name="fieldName">The name of the field in the BlockState object.</param>
    /// <param name="literal">The literal token to convert.</param>
    /// <returns>A BlockState object created from the literal token, or null if the conversion fails.</returns>
    public static BlockState FromLiteral(string fieldName, TokenLiteral literal)
    {
        return literal switch
        {
            TokenStringLiteral str => new BlockState(fieldName, str.text),
            TokenBooleanLiteral boolean => new BlockState(fieldName, boolean.boolean),
            TokenNumberLiteral num => new BlockState(fieldName, num.GetNumberInt()),
            _ => new BlockState(fieldName, literal.AsString())
        };
    }

    private string FieldName { get; }
    private BlockStateType ValueType { get; }

    private string ValueAsEnum { get; }
    private bool ValueAsBoolean { get; }
    private int ValueAsInteger { get; }

    public override string ToString()
    {
        return this.ValueType switch
        {
            BlockStateType.Enum => $@"""{this.FieldName}""=""{this.ValueAsEnum}""",
            BlockStateType.Boolean => $@"""{this.FieldName}""={this.ValueAsBoolean}",
            BlockStateType.Integer => $@"""{this.FieldName}""={this.ValueAsInteger}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum BlockStateType
{
    Enum,
    Boolean,
    Integer
}