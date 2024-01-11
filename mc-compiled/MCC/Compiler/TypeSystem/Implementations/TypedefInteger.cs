using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Json;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations
{
    internal class TypedefInteger : Typedef
    {
        // Data
        public override ScoreboardManager.ValueType TypeEnum => ScoreboardManager.ValueType.INT;
        public override string TypeShortcode => "INT";
        public override string TypeKeyword => "INT";
        public override ITypeStructure CloneData(ITypeStructure data) => null; // no data to clone
        public override bool CanCompareAlone => false;
        internal override ConditionalSubcommandScore[] CompareAlone(bool invert, ScoreboardValue value) => default;
        internal override string[] GetObjectives(ScoreboardValue input)
        {
            return new[] { input.InternalName };
        }

        // Conversion
        internal override bool CanConvertTo(Typedef type)
        {
            switch (type)
            {
                case TypedefInteger _:
                case TypedefFixedDecimal _:
                    return true;
                default:
                    return false;
            }
        }
        internal override IEnumerable<string> ConvertTo(ScoreboardValue src, ScoreboardValue dst)
        {
            Typedef dstType = dst.type;

            switch (dstType)
            {
                case TypedefInteger _:
                {
                    return new[] { Command.ScoreboardOpSet(dst, src) };
                }
                case TypedefFixedDecimal _:
                {
                    int precision = ((FixedDecimalData) dst.data).precision;
                    int factor = (int) Math.Pow(10, precision);
                    
                    ScoreboardManager manager = src.manager;
                    ScoreboardValue temp = manager.temps.RequestGlobal();
                    
                    return new[]
                    {
                        Command.ScoreboardSet(temp, factor),
                        Command.ScoreboardOpSet(dst, src),
                        Command.ScoreboardOpMul(dst, temp)
                    };
                }
            }

            return Array.Empty<string>();
        }

        // Other
        internal override Tuple<string[], JSONRawTerm[]> ToRawText(ScoreboardValue value, ref int index)
        {
            var terms = new JSONRawTerm[] { new JSONScore(value) };
            return new Tuple<string[], JSONRawTerm[]>(null, terms);
        }
        internal override Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(
            TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal, Statement callingStatement)
        {
            int value;

            switch (literal)
            {
                case TokenNullLiteral _:
                    value = 0;
                    break;
                case TokenNumberLiteral number:
                    value = number.GetNumberInt();
                    break;
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }

            return new Tuple<string[], ConditionalSubcommandScore[]>(
                null,
                new[]
                {
                    ConditionalSubcommandScore.New(self, comparisonType.AsRange(value))
                }
            );
        }
        internal override IEnumerable<string> AssignLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            int value;

            switch (literal)
            {
                case TokenNullLiteral _:
                    value = 0;
                    break;
                case TokenNumberLiteral number:
                    value = number.GetNumberInt();
                    break;
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }

            return new[] { Command.ScoreboardSet(self, value) };
        }
        internal override IEnumerable<string> AddLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            int value;

            switch (literal)
            {
                case TokenNullLiteral _:
                    value = 0;
                    break;
                case TokenNumberLiteral number:
                    value = number.GetNumberInt();
                    break;
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }

            return new[] { Command.ScoreboardAdd(self, value) };
        }
        internal override IEnumerable<string> SubtractLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            int value;

            switch (literal)
            {
                case TokenNullLiteral _:
                    value = 0;
                    break;
                case TokenNumberLiteral number:
                    value = number.GetNumberInt();
                    break;
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }

            return new[] { Command.ScoreboardSubtract(self, value) };
        }

        // Methods
        internal override IEnumerable<string> _Assign(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpSet(self, other) };
        }
        internal override IEnumerable<string> _Add(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpAdd(self, other) };
        }
        internal override IEnumerable<string> _Subtract(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpSub(self, other) };
        }
        internal override IEnumerable<string> _Multiply(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpMul(self, other) };
        }
        internal override IEnumerable<string> _Divide(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpDiv(self, other) };
        }
        internal override IEnumerable<string> _Modulo(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpMod(self, other) };
        }
    }
}
