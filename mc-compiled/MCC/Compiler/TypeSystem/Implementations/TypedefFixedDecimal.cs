using System;
using System.Collections.Generic;
using System.Text;
using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Json;
using mc_compiled.MCC.Language;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations;

public readonly struct FixedDecimalData : ITypeStructure
{
    public readonly byte precision;

    internal FixedDecimalData(byte precision) { this.precision = precision; }
    public ITypeStructure DeepClone() { return new FixedDecimalData(this.precision); }

    /// <summary>
    ///     Throws an exception if the given precision is out of bounds.
    /// </summary>
    /// <param name="precision">The precision to check.</param>
    /// <param name="callingStatement">The calling statement.</param>
    /// <exception cref="StatementException">Thrown if the precision is too high or too low.</exception>
    internal static void ThrowIfOutOfBounds(int precision, Statement callingStatement)
    {
        if (precision > byte.MaxValue)
            throw new StatementException(callingStatement,
                $"Precision {precision} was too high to store internally. Is this intentional?");
        if (precision < byte.MinValue)
            throw new StatementException(callingStatement,
                $"Precision {precision} was too low to store internally. Is this intentional?");
    }

    public int TypeHashCode() { return this.precision.GetHashCode(); }
    public bool Equals(FixedDecimalData other) { return this.precision == other.precision; }
    public override bool Equals(object obj) { return obj is FixedDecimalData other && Equals(other); }
    public override int GetHashCode() { return this.precision.GetHashCode(); }
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

    public override bool CanCompareAlone => false;

    public override SyntaxGroup SpecifyPattern =>
        SyntaxGroup.WrapPattern(false, SyntaxParameter.Simple<TokenIntegerLiteral>("precision"));
    internal override string[] GetObjectives(ScoreboardValue input) { return [input.InternalName]; }
    internal override ConditionalSubcommandScore[] CompareAlone(bool invert, ScoreboardValue value) { return null; }
    public override ITypeStructure AcceptPattern(Statement statement)
    {
        int precision = statement.Next<TokenNumberLiteral>("precision").GetNumberInt();
        return new FixedDecimalData((byte) precision);
    }

    public override bool CanAcceptLiteralForData(TokenLiteral literal) { return literal is TokenDecimalLiteral; }
    public override ITypeStructure AcceptLiteral(TokenLiteral literal)
    {
        return new FixedDecimalData(((TokenDecimalLiteral) literal).number.GetPrecision());
    }

    // Conversion
    internal override bool CanConvertTo(Typedef type)
    {
        return type switch
        {
            TypedefInteger _ or TypedefFixedDecimal _ => true,
            _ => false
        };
    }
    internal override IEnumerable<string> ConvertTo(ScoreboardValue src, ScoreboardValue dst)
    {
        Typedef dstType = dst.type;
        ScoreboardManager manager = src.manager;

        int srcPrecision = ((FixedDecimalData) src.data).precision;
        int factor;

        ScoreboardValue temp = manager.temps.RequestGlobal();

        switch (dstType)
        {
            case TypedefInteger _:
            {
                factor = (int) Math.Pow(10, srcPrecision);
                return
                [
                    Command.ScoreboardSet(temp, factor),
                    Command.ScoreboardOpSet(dst, src),
                    Command.ScoreboardOpDiv(dst, temp)
                ];
            }
            case TypedefFixedDecimal _:
            {
                int dstPrecision = ((FixedDecimalData) dst.data).precision;
                if (srcPrecision == dstPrecision)
                    return [Command.ScoreboardOpSet(dst, src)];

                int diff;
                if (srcPrecision > dstPrecision)
                {
                    diff = srcPrecision - dstPrecision;
                    factor = (int) Math.Pow(10, diff);
                    return
                    [
                        Command.ScoreboardSet(temp, factor),
                        Command.ScoreboardOpSet(dst, src),
                        Command.ScoreboardOpDiv(dst, temp)
                    ];
                }

                diff = dstPrecision - srcPrecision;
                factor = (int) Math.Pow(10, diff);
                return
                [
                    Command.ScoreboardSet(temp, factor),
                    Command.ScoreboardOpSet(dst, src),
                    Command.ScoreboardOpMul(dst, temp)
                ];
            }
            default:
                return [Command.ScoreboardOpSet(dst, src)];
        }
    }
    internal override bool NeedsToBeConvertedTo(ScoreboardValue src, ScoreboardValue dst)
    {
        if (dst.type is not TypedefFixedDecimal)
            return base.NeedsToBeConvertedTo(src, dst);

        var dataA = (FixedDecimalData) src.data;
        var dataB = (FixedDecimalData) dst.data;
        return dataA.precision != dataB.precision;
    }

    // Other
    internal override Tuple<string[], RawTextEntry[]> ToRawText(ScoreboardValue value, ref int index)
    {
        ScoreboardManager manager = value.manager;
        Clarifier clarifier = value.clarifier;
        int precision = ((FixedDecimalData) value.data).precision;

        string _whole = SB_WHOLE + index;
        string _part = SB_PART + index;
        string _temporary = SB_TEMP + index;
        string _tempBase = SB_BASE + index;

        var whole = new ScoreboardValue(_whole, false, INTEGER, manager);
        var part = new ScoreboardValue(_part, false, INTEGER, manager);
        var temporary = new ScoreboardValue(_temporary, true, INTEGER, manager);
        var tempBase = new ScoreboardValue(_tempBase, true, INTEGER, manager);

        whole.clarifier.CopyFrom(clarifier);
        part.clarifier.CopyFrom(clarifier);

        manager.DefineMany(whole, part, temporary, tempBase);

        string[] commands =
        [
            Command.ScoreboardSet(tempBase, (int) Math.Pow(10, precision)),
            Command.ScoreboardOpSet(temporary, value),
            Command.ScoreboardOpDiv(temporary, tempBase),
            Command.ScoreboardOpSet(whole, temporary),
            Command.ScoreboardOpMul(temporary, tempBase),
            Command.ScoreboardOpSet(part, value),
            Command.ScoreboardOpSub(part, temporary),
            Command.ScoreboardSet(temporary, -1),
            Command.Execute().IfScore(part, new Range(null, -1))
                .Run(Command.ScoreboardOpMul(part, temporary)) // whole already is negative, no need to change it.
        ];

        // precision is two or more, need to create conditional terms for the 0's.
        var conditionalTerms = new List<ConditionalTerm>();
        var zeroBuilder = new StringBuilder();
        int lowerBound = 0;
        int upperBound = 9;

        // i represents the number of zeros to add.
        // as the lower/upper bounds increase, the number of zeros needed decrease.
        for (int i = precision - 1; i >= 0; i--)
        {
            ConditionalTerm term;
            if (i == 0)
            {
                var range = new Range(lowerBound, null);

                // include no zeros
                term = new ConditionalTerm([
                    new Score(clarifier.CurrentString, whole.InternalName),
                    new Text("."),
                    new Score(clarifier.CurrentString, part.InternalName)
                ], ConditionalSubcommandScore.New(clarifier.CurrentString, part.InternalName, range), false);
            }
            else
            {
                var range = new Range(lowerBound, upperBound);

                // include i number of zeros
                zeroBuilder.Append('0', i);
                term = new ConditionalTerm([
                    new Score(clarifier.CurrentString, whole.InternalName),
                    new Text("." + zeroBuilder),
                    new Score(clarifier.CurrentString, part.InternalName)
                ], ConditionalSubcommandScore.New(clarifier.CurrentString, part.InternalName, range), false);
                zeroBuilder.Clear();
            }

            conditionalTerms.Add(term);

            // raise bound to requiring one less 0
            if (lowerBound == 0)
                lowerBound = 1;
            lowerBound *= 10;
            upperBound *= 10;
        }

        return new Tuple<string[], RawTextEntry[]>(commands, [
            new Variant(conditionalTerms)
        ]);
    }
    internal override Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(
        TokenCompare.Type comparisonType,
        ScoreboardValue self,
        TokenLiteral literal,
        Statement callingStatement)
    {
        if (literal is not TokenNumberLiteral numberLiteral)
            throw LiteralConversionError(self, literal, callingStatement);

        decimal _number = numberLiteral.GetNumber();
        byte precision = ((FixedDecimalData) self.data).precision;
        int number = _number.ToFixedPoint(precision);

        return new Tuple<string[], ConditionalSubcommandScore[]>(
            null,
            [
                ConditionalSubcommandScore.New(self, comparisonType.AsRange(number))
            ]
        );
    }
    internal override IEnumerable<string> AssignLiteral(ScoreboardValue self,
        TokenLiteral literal,
        Statement callingStatement)
    {
        if (literal is TokenNullLiteral)
            return [Command.ScoreboardSet(self, 0)];

        byte precision = ((FixedDecimalData) self.data).precision;

        switch (literal)
        {
            case TokenIntegerLiteral integer:
            {
                int i = integer.number;
                return
                [
                    Command.ScoreboardSet(self, i.ToFixedPoint(precision))
                ];
            }
            case TokenNumberLiteral number:
            {
                decimal f = number.GetNumber();
                return
                [
                    Command.ScoreboardSet(self, f.ToFixedPoint(precision))
                ];
            }
            default:
                throw LiteralConversionError(self, literal, callingStatement);
        }
    }
    internal override IEnumerable<string> AddLiteral(ScoreboardValue self,
        TokenLiteral literal,
        Statement callingStatement)
    {
        if (literal is TokenNullLiteral)
            return [];

        byte precision = ((FixedDecimalData) self.data).precision;

        switch (literal)
        {
            case TokenIntegerLiteral integer:
            {
                int i = integer.number;
                return
                [
                    Command.ScoreboardAdd(self, i.ToFixedPoint(precision))
                ];
            }
            case TokenNumberLiteral number:
            {
                decimal f = number.GetNumber();
                return
                [
                    Command.ScoreboardAdd(self, f.ToFixedPoint(precision))
                ];
            }
            default:
                throw LiteralConversionError(self, literal, callingStatement);
        }
    }
    internal override IEnumerable<string> SubtractLiteral(ScoreboardValue self,
        TokenLiteral literal,
        Statement callingStatement)
    {
        if (literal is TokenNullLiteral)
            return [];

        byte precision = ((FixedDecimalData) self.data).precision;

        switch (literal)
        {
            case TokenIntegerLiteral integer:
            {
                int i = integer.number;
                return
                [
                    Command.ScoreboardSubtract(self, i.ToFixedPoint(precision))
                ];
            }
            case TokenNumberLiteral number:
            {
                decimal f = number.GetNumber();
                return
                [
                    Command.ScoreboardSubtract(self, f.ToFixedPoint(precision))
                ];
            }
            default:
                throw LiteralConversionError(self, literal, callingStatement);
        }
    }

    // Methods
    internal override IEnumerable<string> _Assign(ScoreboardValue self,
        ScoreboardValue other,
        Statement callingStatement)
    {
        return [Command.ScoreboardOpSet(self, other)];
    }
    internal override IEnumerable<string> _Add(ScoreboardValue self,
        ScoreboardValue other,
        Statement callingStatement)
    {
        return [Command.ScoreboardOpAdd(self, other)];
    }
    internal override IEnumerable<string> _Subtract(ScoreboardValue self,
        ScoreboardValue other,
        Statement callingStatement)
    {
        return [Command.ScoreboardOpSub(self, other)];
    }
    internal override IEnumerable<string> _Multiply(ScoreboardValue self,
        ScoreboardValue other,
        Statement callingStatement)
    {
        int precision = ((FixedDecimalData) self.data).precision;
        ScoreboardManager manager = self.manager;
        ScoreboardValue tempBase = manager.temps.RequestGlobal();

        return
        [
            Command.ScoreboardSet(tempBase, (int) Math.Pow(10, precision)),
            Command.ScoreboardOpMul(self, other),
            Command.ScoreboardOpDiv(self, tempBase)
        ];
    }
    internal override IEnumerable<string> _Divide(ScoreboardValue self,
        ScoreboardValue other,
        Statement callingStatement)
    {
        int precision = ((FixedDecimalData) self.data).precision;
        ScoreboardManager manager = self.manager;
        ScoreboardValue tempBase = manager.temps.RequestGlobal();

        return
        [
            Command.ScoreboardSet(tempBase, (int) Math.Pow(10, precision)),
            Command.ScoreboardOpMul(self, tempBase),
            Command.ScoreboardOpDiv(self, other)
        ];
    }
    internal override IEnumerable<string> _Modulo(ScoreboardValue self,
        ScoreboardValue other,
        Statement callingStatement)
    {
        return [Command.ScoreboardOpMod(self, other)];
    }
}