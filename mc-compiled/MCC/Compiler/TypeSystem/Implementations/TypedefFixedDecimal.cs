using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations
{
    public readonly struct FixedDecimalData : ITypeStructure
    {
        public readonly byte precision;

        internal FixedDecimalData(byte precision)
        {
            this.precision = precision;
        }
        public ITypeStructure DeepClone()
        {
            return new FixedDecimalData(precision);
        }


        /// <summary>
        /// Throws an exception if the given precision is out of bounds.
        /// </summary>
        /// <param name="precision">The precision to check.</param>
        /// <param name="callingStatement">The calling statement.</param>
        /// <exception cref="StatementException">Thrown if the precision is too high or too low.</exception>
        internal static void ThrowIfOutOfBounds(int precision, Statement callingStatement)
        {
            if (precision > byte.MaxValue)
                throw new StatementException(callingStatement, $"Precision {precision} was too high to store internally. Is this intentional?");
            if(precision < byte.MinValue)
                throw new StatementException(callingStatement, $"Precision {precision} was too low to store internally. Is this intentional?");
        }
        
        public int TypeHashCode()
        {
            return precision.GetHashCode();
        }
    }
    internal class TypedefFixedDecimal : Typedef<FixedDecimalData>
    {
        private const string SB_WHOLE = "_mcc_d_whole";
        private const string SB_PART = "_mcc_d_part";
        private const string SB_TEMP = "_mcc_d_temp";
        private const string SB_BASE = "_mcc_d_base";

        public override ScoreboardManager.ValueType TypeEnum => ScoreboardManager.ValueType.FIXED_DECIMAL;
        public override string TypeShortcode => "FDC";
        public override string TypeKeyword => "DECIMAL";
        internal override string[] GetObjectives(ScoreboardValue input)
        {
            return new[] { input.InternalName };
        }

        public override bool CanCompareAlone => false;
        internal override ConditionalSubcommandScore[] CompareAlone(bool invert, ScoreboardValue value) => default;

        public override TypePattern SpecifyPattern => new TypePattern(new NamedType(typeof(TokenIntegerLiteral), "precision"));
        public override ITypeStructure AcceptPattern(Statement statement)
        {
            int precision = statement.Next<TokenNumberLiteral>().GetNumberInt();
            return new FixedDecimalData((byte)precision);
        }

        public override bool CanAcceptLiteralForData(TokenLiteral literal)
        {
            return literal is TokenDecimalLiteral;
        }
        public override ITypeStructure AcceptLiteral(TokenLiteral literal)
        {
            return new FixedDecimalData(((TokenDecimalLiteral)literal).number.GetPrecision());
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
            ScoreboardManager manager = src.manager;
            
            int srcPrecision = ((FixedDecimalData)src.data).precision;
            int factor;
            
            ScoreboardValue temp = manager.temps.RequestGlobal();
            
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
                            Command.ScoreboardOpDiv(dst, temp)
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
                            Command.ScoreboardOpMul(dst, temp)
                        };
                    }
                }
                default:
                    return new[] { Command.ScoreboardOpSet(dst, src) };
            }
        }
        internal override bool NeedsToBeConvertedTo(ScoreboardValue src, ScoreboardValue dst)
        {
            if (!(dst.type is TypedefFixedDecimal))
                return base.NeedsToBeConvertedTo(src, dst);
            
            var dataA = (FixedDecimalData)src.data;
            var dataB = (FixedDecimalData)dst.data;
            return dataA.precision != dataB.precision;
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

            var whole = new ScoreboardValue(_whole, false, Typedef.INTEGER, manager);
            var part = new ScoreboardValue(_part, false, Typedef.INTEGER, manager);
            var temporary = new ScoreboardValue(_temporary, true, Typedef.INTEGER, manager);
            var tempBase = new ScoreboardValue(_tempBase, true, Typedef.INTEGER, manager);

            whole.clarifier.CopyFrom(clarifier);
            part.clarifier.CopyFrom(clarifier);

            manager.DefineMany(whole, part, temporary, tempBase);

            string[] commands =
            {
                Command.ScoreboardSet(tempBase, (int) Math.Pow(10, precision)),
                Command.ScoreboardOpSet(temporary, value),
                Command.ScoreboardOpDiv(temporary, tempBase),
                Command.ScoreboardOpSet(whole, temporary),
                Command.ScoreboardOpMul(temporary, tempBase),
                Command.ScoreboardOpSet(part, value),
                Command.ScoreboardOpSub(part, temporary),
                Command.ScoreboardSet(temporary, -1),
                Command.Execute().IfScore(part, new Range(null, -1)).Run(Command.ScoreboardOpMul(part, temporary)) // whole already is negative, no need to change it.
            };
            
            // precision is two or more, need to create conditional terms for the 0's.
            var conditionalTerms = new List<ConditionalTerm>();
            var zeroBuilder = new StringBuilder();
            int lowerBound = 1;
            int upperBound = 9;
            
            // create case for part == 0
            conditionalTerms.Add(
                new ConditionalTerm(
                    new JSONRawTerm[] { new JSONScore(clarifier.CurrentString, part.InternalName) },
                    ConditionalSubcommandScore.New(clarifier.CurrentString, part.InternalName, Range.zero),
                    false)
            );
            
            // i represents the number of zeros to add.
            // as the lower/upper bounds increase, the number of zeros needed decrease.
            for (int i = precision - 1; i >= 0; i--)
            {
                var range = new Range(lowerBound, upperBound);
                ConditionalTerm term;
                if (i == 0)
                {
                    // include i number of zeros
                    zeroBuilder.Append('0', i);
                    term = new ConditionalTerm(new JSONRawTerm[]
                    {
                        new JSONScore(clarifier.CurrentString, whole.InternalName),
                        new JSONText("." + zeroBuilder),
                        new JSONScore(clarifier.CurrentString, part.InternalName)
                    }, ConditionalSubcommandScore.New(clarifier.CurrentString, part.InternalName, range), false);
                    zeroBuilder.Clear();
                }
                else
                {
                    // include no zeros
                    term = new ConditionalTerm(new JSONRawTerm[]
                    {
                        new JSONScore(clarifier.CurrentString, whole.InternalName),
                        new JSONText("."),
                        new JSONScore(clarifier.CurrentString, part.InternalName)
                    }, ConditionalSubcommandScore.New(clarifier.CurrentString, part.InternalName, range), false);
                }
                conditionalTerms.Add(term);
                
                // raise bound to requiring one less 0
                lowerBound *= 10;
                upperBound *= 10;
            }

            return new Tuple<string[], JSONRawTerm[]>(commands, new JSONRawTerm[]
            {
                new JSONVariant(conditionalTerms)
            });
        }
        internal override Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(
            TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal, Statement callingStatement)
        {
            if(!(literal is TokenNumberLiteral numberLiteral))
                throw LiteralConversionError(self, literal, callingStatement);

            decimal _number = numberLiteral.GetNumber();
            byte precision = ((FixedDecimalData)self.data).precision;
            int number = _number.ToFixedPoint(precision);

            return new Tuple<string[], ConditionalSubcommandScore[]>(
                null,
                new[]
                {
                    ConditionalSubcommandScore.New(self, comparisonType.AsRange(number)),
                }
            );
        }
        internal override IEnumerable<string> AssignLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            if (literal is TokenNullLiteral)
                return new[] { Command.ScoreboardSet(self, 0) };

            byte precision = ((FixedDecimalData)self.data).precision;

            switch (literal)
            {
                case TokenIntegerLiteral integer:
                {
                    int i = integer.number;
                    return new[]
                    {
                        Command.ScoreboardSet(self, i.ToFixedPoint(precision))
                    };
                }
                case TokenNumberLiteral number:
                {
                    decimal f = number.GetNumber();
                    return new[]
                    {
                        Command.ScoreboardSet(self, f.ToFixedPoint(precision))
                    };
                }
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }
        }
        internal override IEnumerable<string> AddLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            if (literal is TokenNullLiteral)
                return Array.Empty<string>();

            byte precision = ((FixedDecimalData)self.data).precision;

            switch (literal)
            {
                case TokenIntegerLiteral integer:
                {
                    int i = integer.number;
                    return new[]
                    {
                        Command.ScoreboardAdd(self, i.ToFixedPoint(precision))
                    };
                }
                case TokenNumberLiteral number:
                {
                    decimal f = number.GetNumber();
                    return new[]
                    {
                        Command.ScoreboardAdd(self, f.ToFixedPoint(precision))
                    };
                }
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }
        }
        internal override IEnumerable<string> SubtractLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement)
        {
            if (literal is TokenNullLiteral)
                return Array.Empty<string>();

            byte precision = ((FixedDecimalData)self.data).precision;

            switch (literal)
            {
                case TokenIntegerLiteral integer:
                {
                    int i = integer.number;
                    return new[]
                    {
                        Command.ScoreboardSubtract(self, i.ToFixedPoint(precision))
                    };
                }
                case TokenNumberLiteral number:
                {
                    decimal f = number.GetNumber();
                    return new[]
                    {
                        Command.ScoreboardSubtract(self, f.ToFixedPoint(precision))
                    };
                }
                default:
                    throw LiteralConversionError(self, literal, callingStatement);
            }
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
            int precision = ((FixedDecimalData)self.data).precision;
            ScoreboardManager manager = self.manager;
            ScoreboardValue tempBase = manager.temps.RequestGlobal();

            return new[] {
                Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpMul(self, other),
                Command.ScoreboardOpDiv(self, tempBase)
            };
        }
        internal override IEnumerable<string> _Divide(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            int precision = ((FixedDecimalData)self.data).precision;
            ScoreboardManager manager = self.manager;
            ScoreboardValue tempBase = manager.temps.RequestGlobal();

            return new[] {
                Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpMul(self, tempBase),
                Command.ScoreboardOpDiv(self, other)
            };
        }
        internal override IEnumerable<string> _Modulo(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement)
        {
            return new[] { Command.ScoreboardOpMod(self, other) };
        }
    }
}
