using mc_compiled.Commands.Execute;
using mc_compiled.Commands;
using mc_compiled.Json;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations
{
    internal sealed class TypedefBoolean : Typedef
    {
        internal TypedefBoolean() {}
        
        // Data
        public override ScoreboardManager.ValueType TypeEnum => ScoreboardManager.ValueType.BOOL;
        public override string TypeShortcode => "BLN";
        public override string TypeKeyword => "BOOL";
        public override object CloneData(object data) => null; // no data to clone anyways
        public override bool CanCompareAlone => true;

        internal override ConditionalSubcommandScore[] CompareAlone(bool invert, ScoreboardValue value)
        {
            return new ConditionalSubcommandScore[]
            {
                ConditionalSubcommandScore.New(value, new Range(1, invert))
            };
        }
        internal override string[] GetObjectives(ScoreboardValue input)
        {
            return new[] { input.InternalName };
        }

        // Conversion
        internal override bool CanConvertTo(Typedef type) => false;
        internal override IEnumerable<string> ConvertTo(ScoreboardValue src, ScoreboardValue dst) => Array.Empty<string>();

        // Other
        internal override Tuple<string[], JSONRawTerm[]> ToRawText(ScoreboardValue value, ref int index)
        {
            value.manager.executor.TryGetPPV("_true", out dynamic[] trueValues);
            value.manager.executor.TryGetPPV("_false", out dynamic[] falseValues);

            Range check = new Range(1, false);
            
            JSONRawTerm[] terms = new JSONRawTerm[] {
                new JSONVariant(
                    new ConditionalTerm(new JSONRawTerm[] { new JSONText(trueValues[0]) }, ConditionalSubcommandScore.New(value, check), false),
                    new ConditionalTerm(new JSONRawTerm[] { new JSONText(falseValues[0]) }, ConditionalSubcommandScore.New(value, check), true)
                )
            };

            return new Tuple<string[], JSONRawTerm[]>(null, terms);
        }
        internal override Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(
            TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal, Statement callingStatement)
        {
            if (!(literal is TokenBooleanLiteral booleanLiteral))
                throw LiteralConversionError(self, literal, callingStatement);

            int src = booleanLiteral.boolean ? 1 : 0;

            Range range;
            switch (comparisonType)
            {
                case TokenCompare.Type.EQUAL:
                    range = new Range(src, false);
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    range = new Range(src, true);
                    break;
                case TokenCompare.Type.LESS:
                case TokenCompare.Type.LESS_OR_EQUAL:
                case TokenCompare.Type.GREATER:
                case TokenCompare.Type.GREATER_OR_EQUAL:
                default:
                    throw new Exception("Boolean values only support == and !=");
            }

            return new Tuple<string[], ConditionalSubcommandScore[]>(
                null,
                new ConditionalSubcommandScore[]
                {
                    ConditionalSubcommandScore.New(self, range)
                }
            );
        }
        internal override IEnumerable<string> AssignLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            if (!(literal is TokenBooleanLiteral booleanLiteral))
                throw LiteralConversionError(self, literal, callingStatement);

            return new[] { Command.ScoreboardSet(self, ((bool)booleanLiteral) ? 1 : 0) };
        }
        internal override IEnumerable<string> AddLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            throw UnsupportedOperationError(self, UnsupportedOperationType.AddLiteral, callingStatement);
        }
        internal override IEnumerable<string> SubtractLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            throw UnsupportedOperationError(self, UnsupportedOperationType.SubtractLiteral, callingStatement);
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
            throw UnsupportedOperationError(self, UnsupportedOperationType.Add, callingStatement);
        }
        internal override IEnumerable<string> _Subtract(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            throw UnsupportedOperationError(self, UnsupportedOperationType.Subtract, callingStatement);
        }
        internal override IEnumerable<string> _Multiply(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            throw UnsupportedOperationError(self, UnsupportedOperationType.Multiply, callingStatement);
        }
        internal override IEnumerable<string> _Divide(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            throw UnsupportedOperationError(self, UnsupportedOperationType.Divide, callingStatement);
        }
        internal override IEnumerable<string> _Modulo(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            throw UnsupportedOperationError(self, UnsupportedOperationType.Modulo, callingStatement);
        }
    }
}
