using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations
{
    internal readonly struct FixedDecimalData : ITypeStructure
    {
        internal readonly int precision;

        internal FixedDecimalData(int precision)
        {
            this.precision = precision;
        }
        public ITypeStructure DeepClone()
        {
            return new FixedDecimalData(precision);
        }
    }
    internal class TypedefFixedDecimal : Typedef<FixedDecimalData>
    {
        public const string SB_WHOLE = "_mcc_d_whole";
        public const string SB_PART = "_mcc_d_part";
        public const string SB_TEMP = "_mcc_d_temp";
        public const string SB_BASE = "_mcc_d_base";

        public override ScoreboardManager.ValueType TypeEnum => ScoreboardManager.ValueType.FIXEDDECIMAL;
        public override string TypeKeyword => "decimal";
        internal override string[] GetObjectives(ScoreboardValue input)
        {
            return new[] { input.Name };
        }

        public override bool CanCompareAlone => false;
        internal override Range CompareAlone(bool invert) => default;

        public override TypePattern SpecifyPattern => new TypePattern(new NamedType(typeof(TokenIntegerLiteral), "precision"));
        public override object AcceptPattern(TokenLiteral[] inputs)
        {
            int precision = (inputs[0] as TokenIntegerLiteral);
            return new FixedDecimalData(precision);
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
            ScoreboardManager manager = src.manager;
            
            int srcPrecision = ((FixedDecimalData)src.data).precision;
            int factor;
            
            ScoreboardValueInteger temp = manager.temps.RequestGlobal();
            manager.temps.ReleaseGlobal();
            
            switch (dstType)
            {
                case TypedefInteger _:
                {
                    factor = (int)Math.Pow(10, srcPrecision);
                    return new[]
                    {
                        Command.ScoreboardSet(temp, factor),
                        Command.ScoreboardOpSet(dst, src),
                        Command.ScoreboardOpDiv(dst, temp)
                    };
                }
                case TypedefFixedDecimal _:
                {
                    int dstPrecision = ((FixedDecimalData) dst.data).precision;
                    if (srcPrecision == dstPrecision)
                        return new[] { Command.ScoreboardOpSet(dst, src) };

                    int diff;
                    if (srcPrecision > dstPrecision)
                    {
                        diff = srcPrecision - dstPrecision;
                        factor = (int)Math.Pow(10, diff);
                        return new[]
                        {
                            Command.ScoreboardSet(temp, factor),
                            Command.ScoreboardOpSet(dst, src),
                            Command.ScoreboardOpMul(dst, temp)
                        };
                    }
                    else
                    {
                        diff = dstPrecision - srcPrecision;
                        factor = (int)Math.Pow(10, diff);
                        return new[]
                        {
                            Command.ScoreboardSet(temp, factor),
                            Command.ScoreboardOpSet(dst, src),
                            Command.ScoreboardOpDiv(dst, temp)
                        };
                    }
                }
                default:
                    return new[] { Command.ScoreboardOpSet(dst, src) };
            }
        }

        // Other
        internal override Tuple<string[], JSONRawTerm[]> ToRawText(ScoreboardValue value, ref int index)
        {
            ScoreboardManager manager = value.manager;
            Clarifier clarifier = value.clarifier;
            int precision = ((FixedDecimalData)value.data).precision;

            string _whole = SB_WHOLE + index;
            string _part = SB_PART + index;
            string _temporary = SB_TEMP + index;
            string _tempBase = SB_BASE + index;

            var whole = new ScoreboardValueInteger(_whole, false, manager);
            var part = new ScoreboardValueInteger(_part, false, manager);
            var temporary = new ScoreboardValueInteger(_temporary, true, manager);
            var tempBase = new ScoreboardValueInteger(_tempBase, true, manager);

            whole.clarifier.CopyFrom(clarifier);
            part.clarifier.CopyFrom(clarifier);

            manager.DefineMany(whole, part, temporary, tempBase);

            string[] commands = new string[]
            {
                Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpSet(temporary, value),
                Command.ScoreboardOpDiv(temporary, tempBase),
                Command.ScoreboardOpSet(whole, temporary),
                Command.ScoreboardOpMul(temporary, tempBase),
                Command.ScoreboardOpSet(part, value),
                Command.ScoreboardOpSub(part, temporary),
                Command.ScoreboardSet(temporary, -1),
                Command.Execute().IfScore(part, new Range(null, -1)).Run(Command.ScoreboardOpMul(part, temporary))
            };

            JSONRawTerm[] terms = new JSONRawTerm[]
            {
                new JSONScore(clarifier.CurrentString, whole),
                new JSONText("."),
                new JSONScore(clarifier.CurrentString, part)
            };

            return new Tuple<string[], JSONRawTerm[]>(commands, terms);
        }
        internal override Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal)
        {
            if(!(literal is TokenNumberLiteral numberLiteral))
                throw LiteralConversionError(self, literal);

            int precision = ((FixedDecimalData)self.data).precision;
            float _number = numberLiteral.GetNumber();
            int number = _number.ToFixedPoint(precision);

            return new Tuple<string[], ConditionalSubcommandScore[]>(
                null,
                new ConditionalSubcommandScore[]
                {
                    ConditionalSubcommandScore.New(self, comparisonType.AsRange(number)),
                }
            );
        }
        internal override string[] AssignLiteral(ScoreboardValue self, TokenLiteral literal)
        {
            if (literal is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(self, 0) };

            int precision = ((FixedDecimalData)self.data).precision;

            switch (literal)
            {
                case TokenIntegerLiteral integer:
                {
                    int i = integer.number;
                    return new string[]
                    {
                        Command.ScoreboardSet(self, i.ToFixedPoint(precision))
                    };
                }
                case TokenNumberLiteral number:
                {
                    float f = number.GetNumber();
                    return new string[]
                    {
                        Command.ScoreboardSet(self, f.ToFixedPoint(precision))
                    };
                }
                default:
                    throw LiteralConversionError(self, literal);
            }
        }
        internal override string[] AddLiteral(ScoreboardValue self, TokenLiteral literal)
        {
            if (literal is TokenNullLiteral)
                return Array.Empty<string>();

            int precision = ((FixedDecimalData)self.data).precision;

            switch (literal)
            {
                case TokenIntegerLiteral integer:
                {
                    int i = integer.number;
                    return new string[]
                    {
                        Command.ScoreboardAdd(self, i.ToFixedPoint(precision))
                    };
                }
                case TokenNumberLiteral number:
                {
                    float f = number.GetNumber();
                    return new string[]
                    {
                        Command.ScoreboardAdd(self, f.ToFixedPoint(precision))
                    };
                }
                default:
                    throw LiteralConversionError(self, literal);
            }
        }
        internal override string[] SubtractLiteral(ScoreboardValue self, TokenLiteral literal)
        {
            if (literal is TokenNullLiteral)
                return Array.Empty<string>();

            int precision = ((FixedDecimalData)self.data).precision;

            switch (literal)
            {
                case TokenIntegerLiteral integer:
                {
                    int i = integer.number;
                    return new string[]
                    {
                        Command.ScoreboardSubtract(self, i.ToFixedPoint(precision))
                    };
                }
                case TokenNumberLiteral number:
                {
                    float f = number.GetNumber();
                    return new string[]
                    {
                        Command.ScoreboardSubtract(self, f.ToFixedPoint(precision))
                    };
                }
                default:
                    throw LiteralConversionError(self, literal);
            }
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
            int precision = ((FixedDecimalData)self.data).precision;
            ScoreboardManager manager = self.manager;
            ScoreboardValue tempBase = manager.temps.RequestGlobal();
            manager.temps.ReleaseGlobal();

            return new[] {
                Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpMul(self, other),
                Command.ScoreboardOpDiv(self, tempBase)
            };
        }
        protected override string[] _Divide(ScoreboardValue self, ScoreboardValue other)
        {
            int precision = ((FixedDecimalData)self.data).precision;
            ScoreboardManager manager = self.manager;
            ScoreboardValue tempBase = manager.temps.RequestGlobal();
            manager.temps.ReleaseGlobal();

            return new[] {
                Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpMul(self, tempBase),
                Command.ScoreboardOpDiv(self, other)
            };
        }
        protected override string[] _Modulo(ScoreboardValue self, ScoreboardValue other)
        {
            return new[] { Command.ScoreboardOpMod(self, other) };
        }
    }
}
