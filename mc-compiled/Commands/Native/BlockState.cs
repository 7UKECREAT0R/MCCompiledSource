using System;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Native
{
    public readonly struct BlockState
    {
        internal BlockState(string fieldName, string valueAsEnum)
        {
            FieldName = fieldName;
            ValueAsEnum = valueAsEnum;
            ValueType = BlockStateType.Enum;
            ValueAsBoolean = default;
            ValueAsInteger = default;
        }
        internal BlockState(string fieldName, bool valueAsBoolean)
        {
            FieldName = fieldName;
            ValueAsBoolean = valueAsBoolean;
            ValueType = BlockStateType.Boolean;
            ValueAsEnum = null;
            ValueAsInteger = default;
        }
        internal BlockState(string fieldName, int valueAsInteger)
        {
            FieldName = fieldName;
            ValueAsInteger = valueAsInteger;
            ValueType = BlockStateType.Integer;
            ValueAsBoolean = default;
            ValueAsEnum = null;
        }

        /// <summary>
        /// Converts a literal token to a BlockState object.
        /// </summary>
        /// <param name="fieldName">The name of the field in the BlockState object.</param>
        /// <param name="literal">The literal token to convert.</param>
        /// <returns>A BlockState object created from the literal token, or null if the conversion fails.</returns>
        public static BlockState FromLiteral(string fieldName, TokenLiteral literal)
        {
            switch (literal)
            {
                case TokenStringLiteral str:
                    return new BlockState(fieldName, str.text);
                case TokenBooleanLiteral boolean:
                    return new BlockState(fieldName, boolean.boolean);
                case TokenNumberLiteral num:
                    return new BlockState(fieldName, num.GetNumberInt());
                default:
                    return new BlockState(fieldName, literal.AsString());
            }
        }
        
        private string FieldName { get; }
        private BlockStateType ValueType { get; }
        
        private string ValueAsEnum { get; }
        private bool ValueAsBoolean { get; }
        private int ValueAsInteger { get; }
        
        public override string ToString()
        {
            switch (ValueType)
            {
                case BlockStateType.Enum:
                    return $@"""{FieldName}""=""{ValueAsEnum}""";
                case BlockStateType.Boolean:
                    return $@"""{FieldName}""={ValueAsBoolean}";
                case BlockStateType.Integer:
                    return $@"""{FieldName}""={ValueAsInteger}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public enum BlockStateType
    {
        Enum,
        Boolean,
        Integer
    }
}