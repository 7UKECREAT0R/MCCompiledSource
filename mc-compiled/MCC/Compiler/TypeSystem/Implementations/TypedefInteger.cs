using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations
{
    internal class TypedefInteger : Typedef
    {
        // Data
        public override ScoreboardManager.ValueType TypeEnum => ScoreboardManager.ValueType.INT;
        public override string TypeKeyword => "int";
        protected override object CloneData(object data) => null; // no data to clone anyways
        public override bool CanCompareAlone => false;
        internal override Range CompareAlone(bool invert) => default;
        internal override string[] GetObjectives(ScoreboardValue input)
        {
            return new[] { input.Name };
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
        internal override string[] ConvertTo(ScoreboardValue src, ScoreboardValue dst)
        {
            Typedef dstType = dst.type;

            switch (dstType)
            {
                case TypedefInteger _:
                {
                    return new string[] { Command.ScoreboardOpSet(dst, src) };
                }
                case TypedefFixedDecimal _:
                {
                    int precision = ((FixedDecimalData) dst.data).precision;
                    int factor = (int) Math.Pow(10, precision);
                    
                    ScoreboardManager manager = src.manager;
                    ScoreboardValueInteger temp = manager.temps.RequestGlobal();
                    manager.temps.ReleaseGlobal();
                    
                    return new string[]
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
            string[] commands = null;
            JSONRawTerm[] terms = new JSONRawTerm[] { new JSONScore(value) };

            return new Tuple<string[], JSONRawTerm[]>(commands, terms);
        }
        internal override Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal)
        {
            int value;

            if (literal is TokenNullLiteral)
                value = 0;
            else if (literal is TokenNumberLiteral number)
                value = number.GetNumberInt();
            else
                throw LiteralConversionError(self, literal);

            return new Tuple<string[], ConditionalSubcommandScore[]>(
                null,
                new ConditionalSubcommandScore[]
                {
                    ConditionalSubcommandScore.New(self, comparisonType.AsRange(value))
                }
            );
        }
        internal override string[] AssignLiteral(ScoreboardValue self, TokenLiteral literal)
        {
            int value;

            if(literal is TokenNullLiteral)
                value = 0;
            else if(literal is TokenNumberLiteral number)
                value = number.GetNumberInt();
            else
                throw LiteralConversionError(self, literal);

            return new[] { Command.ScoreboardSet(self, value) };
        }
        internal override string[] AddLiteral(ScoreboardValue self, TokenLiteral literal)
        {
            int value;

            if (literal is TokenNullLiteral)
                value = 0;
            else if (literal is TokenNumberLiteral number)
                value = number.GetNumberInt();
            else
                throw LiteralConversionError(self, literal);

            return new[] { Command.ScoreboardAdd(self, value) };
        }
        internal override string[] SubtractLiteral(ScoreboardValue self, TokenLiteral literal)
        {
            int value;

            if (literal is TokenNullLiteral)
                value = 0;
            else if (literal is TokenNumberLiteral number)
                value = number.GetNumberInt();
            else
                throw LiteralConversionError(self, literal);

            return new[] { Command.ScoreboardSubtract(self, value) };
        }

        // Methods
        protected override string[] _Assign(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpSet(self, other) };
        }
        protected override string[] _Add(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpAdd(self, other) };
        }
        protected override string[] _Subtract(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpSub(self, other) };
        }
        protected override string[] _Multiply(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpMul(self, other) };
        }
        protected override string[] _Divide(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpDiv(self, other) };
        }
        protected override string[] _Modulo(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpMod(self, other) };
        }
    }
}
