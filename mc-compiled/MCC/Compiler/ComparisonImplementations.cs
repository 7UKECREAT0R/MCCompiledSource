using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Compares if a boolean value is true.
/// </summary>
public class ComparisonAlone : Comparison
{
    private readonly ScoreboardValue score;
    public ComparisonAlone(ScoreboardValue score, bool invert) : base(invert) { this.score = score; }
    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;
        return null;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;
        return this.score.CompareAlone(this.inverted)
            .Select(check => new SubcommandIf(check))
            .Cast<Subcommand>()
            .ToArray();
    }

    public override string GetDescription() { return this.inverted ? $"not {this.score.Name}" : $"{this.score.Name}"; }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets() { return [this.score]; }
}

/// <summary>
///     Compares two values.
/// </summary>
public class ComparisonValue : Comparison
{
    private readonly Token a;
    private readonly Token b;
    private readonly TokenCompare.Type comparison;
    private SideType aType;
    private SideType bType;

    private Tuple<string[], ConditionalSubcommandScore[]> entries;
    private ScoreboardValue temp;
    public ComparisonValue(Token a, TokenCompare.Type comparison, Token b, bool invert) : base(invert)
    {
        this.a = a;
        this.b = b;
        this.comparison = comparison;

        // get types of each side
        FindAType();
        FindBType();

        if (!this.RequiresSwap)
            return;

        // swap tokens for simplicity
        (this.bType, this.aType) = (this.aType, this.bType);
        this.a = b;
        this.b = a;
        this.comparison = comparison.InvertDirection();
    }

    /// <summary>
    ///     Returns if both sides of the comparison are values.
    /// </summary>
    private bool BothValues => this.aType == SideType.Variable && this.bType == SideType.Variable;
    /// <summary>
    ///     Returns if the sides of the comparison need to be swapped.
    /// </summary>
    private bool RequiresSwap => this.aType == SideType.Constant && this.bType == SideType.Variable;

    private void FindAType()
    {
        this.aType = this.a switch
        {
            TokenIdentifierValue _ => SideType.Variable,
            TokenNumberLiteral _ => SideType.Constant,
            _ => SideType.Unknown
        };
    }

    private void FindBType()
    {
        this.bType = this.b switch
        {
            TokenIdentifierValue _ => SideType.Variable,
            TokenNumberLiteral _ => SideType.Constant,
            _ => SideType.Unknown
        };
    }

    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;

        if (this.aType == SideType.Unknown)
            throw new StatementException(callingStatement,
                $"Unexpected token on left-hand side of condition: {this.a.AsString()}");
        if (this.bType == SideType.Unknown)
            throw new StatementException(callingStatement,
                $"Unexpected token on right-hand side of condition: {this.b.AsString()}");

        TokenCompare.Type localComparison = this.comparison;
        if (this.inverted) // invert if needed, but only in the local scope
            localComparison = localComparison.InvertComparison();

        if (this.aType == SideType.Variable && this.bType == SideType.Variable)
        {
            ScoreboardValue aValue = ((TokenIdentifierValue) this.a).value;
            ScoreboardValue bValue = ((TokenIdentifierValue) this.b).value;

            if (aValue.InternalName.Equals(bValue.InternalName) && aValue.clarifier.Equals(bValue.clarifier))
                switch (localComparison)
                {
                    case TokenCompare.Type.NOT_EQUAL:
                    case TokenCompare.Type.LESS:
                    case TokenCompare.Type.GREATER:
                        throw new StatementException(callingStatement, "Expression will always be false.");
                    case TokenCompare.Type.EQUAL:
                    case TokenCompare.Type.LESS_OR_EQUAL:
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                    default:
                        throw new StatementException(callingStatement, "Expression will always be true.");
                }

            var commands = new List<string>();

            if (willBeInverted)
            {
                this.temp = executor.scoreboard.temps.Request(true);
                commands.Add(Command.ScoreboardSet(this.temp, 0));

                if (localComparison == TokenCompare.Type.NOT_EQUAL)
                    commands.Add(Command.Execute()
                        .UnlessScore(aValue, TokenCompare.Type.EQUAL, bValue)
                        .Run(Command.ScoreboardSet(this.temp, 1)));
                else
                    commands.Add(Command.Execute()
                        .IfScore(aValue, localComparison, bValue)
                        .Run(Command.ScoreboardSet(this.temp, 1)));

                return commands;
            }
        }

        if (this.aType == SideType.Variable && this.bType == SideType.Constant)
        {
            ScoreboardValue aValue = ((TokenIdentifierValue) this.a).value;
            var bValue = (TokenNumberLiteral) this.b;
            this.entries = aValue.CompareToLiteral(localComparison, bValue, callingStatement);

            if (willBeInverted)
            {
                var commands = new List<string>(this.entries.Item1 ?? []);
                this.temp = executor.scoreboard.temps.Request(true);
                commands.Add(Command.ScoreboardSet(this.temp, 0));

                ExecuteBuilder builder = Command.Execute();
                foreach (ConditionalSubcommandScore entry in this.entries.Item2)
                    // retrieve the scoreboard value
                    if (entry.comparesRange)
                    {
                        string scoreName = entry.sourceValue;

                        if (!executor.scoreboard.TryGetByInternalName(scoreName, out ScoreboardValue score))
                            throw new StatementException(callingStatement,
                                $"Unknown value: '{scoreName}'. Is it defined above this line?");

                        score = score.Clone(callingStatement, newClarifier: entry.SourceClarifier);
                        builder.IfScore(score, entry.range);
                    }
                    else
                    {
                        string sourceName = entry.sourceValue;
                        string otherName = entry.otherValue;

                        if (!executor.scoreboard.TryGetByInternalName(sourceName, out ScoreboardValue source))
                            throw new StatementException(callingStatement,
                                $"Unknown value: '{sourceName}'. Is it defined above this line?");
                        if (!executor.scoreboard.TryGetByInternalName(otherName, out ScoreboardValue other))
                            throw new StatementException(callingStatement,
                                $"Unknown value: '{otherName}'. Is it defined above this line?");

                        source = source.Clone(callingStatement, newClarifier: entry.SourceClarifier);
                        other = other.Clone(callingStatement, newClarifier: entry.OtherClarifier);
                        builder.IfScore(source, entry.comparisonType, other);
                    }

                commands.Add(builder.Run(Command.ScoreboardSet(this.temp, 1)));
                return commands;
            }

            return this.entries.Item1;
        }

        return null;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        TokenCompare.Type localComparison = this.comparison;
        if (this.inverted)
            localComparison = localComparison.InvertComparison();

        cancel = false;

        if (this.BothValues)
        {
            // "value value" comparison
            ScoreboardValue aValue = ((TokenIdentifierValue) this.a).value;
            ScoreboardValue bValue = ((TokenIdentifierValue) this.b).value;

            if (willBeInverted)
                return
                [
                    new SubcommandIf(ConditionalSubcommandScore.New(this.temp, Range.Of(1)))
                ];

            if (localComparison == TokenCompare.Type.NOT_EQUAL)
                return
                [
                    // mojang doesn't support != so invert the root of the comparison
                    new SubcommandUnless(ConditionalSubcommandScore.New(aValue, TokenCompare.Type.EQUAL, bValue))
                ];

            return
            [
                new SubcommandIf(ConditionalSubcommandScore.New(aValue, localComparison, bValue))
            ];
        }

        if (this.aType == SideType.Variable)
        {
            if (willBeInverted)
                return
                [
                    new SubcommandIf(ConditionalSubcommandScore.New(this.temp, Range.Of(1)))
                ];

            // "value constant" comparison
            return this.entries.Item2
                .Select(condition => new SubcommandIf(condition))
                .Cast<Subcommand>().ToArray();
        }

        // "constant constant" comparison.
        var numberA = this.a as TokenNumberLiteral;
        var numberB = this.b as TokenNumberLiteral;

        // ReSharper disable once PossibleNullReferenceException
        bool result = numberA.CompareWithOther(localComparison, numberB);
        cancel = !result;
        return null;
    }

    public override string GetDescription()
    {
        return this.inverted
            ? $"{this.a.AsString()} is not {this.comparison.ToString().Replace("_", "").ToLower()} to {this.b.AsString()}"
            : $"{this.a.AsString()} is {this.comparison.ToString().Replace("_", "").ToLower()} to {this.b.AsString()}";
    }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets()
    {
        if (this.aType == SideType.Variable && this.bType != SideType.Variable)
            return [((TokenIdentifierValue) this.a).value];
        if (this.aType != SideType.Variable && this.bType == SideType.Variable)
            return [((TokenIdentifierValue) this.b).value];
        if (this.aType == SideType.Variable && this.bType == SideType.Variable)
            return [((TokenIdentifierValue) this.a).value, ((TokenIdentifierValue) this.b).value];

        return Array.Empty<ScoreboardValue>();
    }
}

public class ComparisonSelector : Comparison
{
    private const string ERROR_MESSAGE =
        "'if <selector>' format should only use @s selectors. Use 'if any <selector>' to more concisely show checking for any selector match.";
    private readonly Selector selector;
    private ScoreboardValue temp;

    public ComparisonSelector(Selector selector, bool invert) : base(invert) { this.selector = selector; }

    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        if (this.selector.core != Selector.Core.s)
            throw new StatementException(callingStatement, ERROR_MESSAGE);

        if (!willBeInverted && !this.inverted)
        {
            cancel = false;
            return null;
        }

        List<string> commands = [];

        this.temp = executor.scoreboard.temps.Request(true);
        commands.Add(Command.ScoreboardSet(this.temp, 0));
        commands.Add(Command.Execute()
            .As(this.selector)
            .Run(Command.ScoreboardSet(this.temp, 1)));

        cancel = false;
        return commands;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        if (this.selector.core != Selector.Core.s)
            throw new StatementException(callingStatement, ERROR_MESSAGE);

        cancel = false;

        if (willBeInverted || this.inverted)
        {
            var range = new Range(1, this.inverted);
            return [new SubcommandIf(ConditionalSubcommandScore.New(this.temp, range))];
        }

        return [new SubcommandAs(this.selector)];
    }

    public override string GetDescription()
    {
        return this.inverted
            ? $"{this.selector} does not match the executing entity"
            : $"{this.selector} matches the executing entity";
    }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets() { return Array.Empty<ScoreboardValue>(); }
}

public class ComparisonCount : Comparison
{
    private readonly TokenCompare.Type comparison;

    private readonly Token goalCount;
    private readonly Selector selector;
    private SideType goalType;

    private ScoreboardValue temp;
    public ComparisonCount(Selector selector, TokenCompare.Type comparison, Token goalCount, bool invert) : base(invert)
    {
        this.selector = selector;
        this.comparison = comparison;
        this.goalCount = goalCount;

        FindGoalType();
    }

    public bool RequiresSubtraction => this.goalType == SideType.Variable;

    private void FindGoalType()
    {
        if (this.goalCount is TokenIdentifierValue)
            this.goalType = SideType.Variable;
        else if (this.goalCount is TokenNumberLiteral)
            this.goalType = SideType.Constant;
        else
            this.goalType = SideType.Unknown;
    }
    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        if (this.goalType == SideType.Unknown)
            throw new StatementException(callingStatement,
                $"Unexpected token on right-hand side of condition: {this.goalCount.AsString()}");

        // compile-time validation
        if (this.goalType == SideType.Constant)
        {
            int constant = ((TokenNumberLiteral) this.goalCount).GetNumberInt();

            if (constant == 0 && this.comparison == TokenCompare.Type.LESS)
                throw new StatementException(callingStatement, "Expression will always be false.");
            if (constant < 0)
                // we know that the count will always be 0 or greater, so use that for inference
                switch (this.comparison)
                {
                    case TokenCompare.Type.EQUAL:
                    case TokenCompare.Type.NOT_EQUAL:
                    case TokenCompare.Type.LESS:
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        throw new StatementException(callingStatement, "Expression will always be false.");
                    case TokenCompare.Type.GREATER:
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                    default:
                        throw new StatementException(callingStatement, "Expression will always be true.");
                }

            if (this.selector.count.HasCount)
            {
                // we know the MAX selector count and target count and can perform a static comparison
                int maxSelected = this.selector.count.count;

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (this.comparison)
                {
                    case TokenCompare.Type.EQUAL:
                        if (constant > maxSelected)
                            throw new StatementException(callingStatement, "Expression will always be false.");
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        if (constant > maxSelected)
                            throw new StatementException(callingStatement, "Expression will always be true.");
                        break;
                    case TokenCompare.Type.LESS:
                        if (constant > maxSelected)
                            throw new StatementException(callingStatement, "Expression will always be true.");
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        if (constant >= maxSelected)
                            throw new StatementException(callingStatement, "Expression will always be true.");
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        if (constant > maxSelected)
                            throw new StatementException(callingStatement, "Expression will always be false.");
                        break;
                    case TokenCompare.Type.GREATER:
                        if (constant >= maxSelected)
                            throw new StatementException(callingStatement, "Expression will always be false.");
                        break;
                }
            }
        }

        // count number of entities
        var commands = new List<string>();

        this.temp = executor.scoreboard.temps.Request(true);
        commands.Add(Command.ScoreboardSet(this.temp, 0));
        commands.Add(Command.Execute()
            .As(this.selector)
            .Run(Command.ScoreboardAdd(this.temp, 1)));

        cancel = false;
        return commands;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;

        TokenCompare.Type localComparison = this.comparison;
        if (this.inverted)
            localComparison = localComparison.InvertComparison();

        if (this.goalType == SideType.Variable)
        {
            ScoreboardValue b = ((TokenIdentifierValue) this.goalCount).value;
            return [new SubcommandIf(ConditionalSubcommandScore.New(this.temp, localComparison, b))];
        }
        else
        {
            var _b = this.goalCount as TokenNumberLiteral;
            int b = _b.GetNumberInt();

            Range range = localComparison switch
            {
                TokenCompare.Type.EQUAL => new Range(b, false),
                TokenCompare.Type.NOT_EQUAL => new Range(b, true),
                TokenCompare.Type.LESS => new Range(null, b - 1),
                TokenCompare.Type.LESS_OR_EQUAL => new Range(null, b),
                TokenCompare.Type.GREATER => new Range(b + 1, null),
                TokenCompare.Type.GREATER_OR_EQUAL => new Range(b, null),
                _ => throw new StatementException(callingStatement,
                    $"Unexpected comparison type: {localComparison}.")
            };

            return [new SubcommandIf(ConditionalSubcommandScore.New(this.temp, range))];
        }
    }

    public override string GetDescription()
    {
        return this.inverted
            ? $"count of {this.selector} is not {this.comparison.ToString().Replace("_", "").ToLower()} to {this.goalCount.AsString()}"
            : $"count of {this.selector} is {this.comparison.ToString().Replace("_", "").ToLower()} to {this.goalCount.AsString()}";
    }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets()
    {
        if (this.goalType == SideType.Variable)
            return [(this.goalCount as TokenIdentifierValue).value];
        return Array.Empty<ScoreboardValue>();
    }
}

public class ComparisonAny : Comparison
{
    public readonly Selector selector;

    private ScoreboardValue temp;

    public ComparisonAny(Selector selector, bool invert) : base(invert)
    {
        if (selector.count.count == 1)
        {
            this.selector = selector;
        }
        else
        {
            this.selector = new Selector(selector);
            this.selector.count.count = 1;
        }
    }
    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        if (!willBeInverted)
        {
            cancel = false;
            return null;
        }

        List<string> commands = [];

        this.temp = executor.scoreboard.temps.Request(true);
        commands.Add(Command.ScoreboardSet(this.temp, 0));
        commands.Add(Command.Execute()
            .IfEntity(this.selector)
            .Run(Command.ScoreboardSet(this.temp, 1)));

        cancel = false;
        return commands;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;

        if (willBeInverted)
        {
            var range = new Range(1, this.inverted);
            return [new SubcommandIf(ConditionalSubcommandScore.New(this.temp, range))];
        }

        if (this.inverted)
            return [new SubcommandUnless(ConditionalSubcommandEntity.New(this.selector))];
        return [new SubcommandIf(ConditionalSubcommandEntity.New(this.selector))];
    }

    public override string GetDescription()
    {
        return this.inverted ? $"{this.selector} does not match any entity" : $"{this.selector} matches any entity";
    }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets() { return Array.Empty<ScoreboardValue>(); }
}

public class ComparisonBlock : Comparison
{
    private readonly string block;
    [CanBeNull]
    private readonly BlockState[] blockStates;
    private readonly Coordinate x;
    private readonly Coordinate y;
    private readonly Coordinate z;

    private ScoreboardValue temp;

    public ComparisonBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        string block,
        [CanBeNull] BlockState[] blockStates,
        bool invert) :
        base(invert)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.block = block;
        this.blockStates = blockStates;
    }
    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        if (!willBeInverted)
        {
            cancel = false;
            return null;
        }

        List<string> commands = [];

        this.temp = executor.scoreboard.temps.Request(true);
        commands.Add(Command.ScoreboardSet(this.temp, 0));
        commands.Add(Command.Execute()
            .IfBlock(this.x, this.y, this.z, this.block, this.blockStates)
            .Run(Command.ScoreboardSet(this.temp, 1)));

        cancel = false;
        return commands;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;

        if (willBeInverted)
        {
            var range = new Range(1, this.inverted);
            return [new SubcommandIf(ConditionalSubcommandScore.New(this.temp, range))];
        }

        if (this.inverted)
            return
            [
                new SubcommandUnless(ConditionalSubcommandBlock.New(this.x, this.y, this.z, this.block,
                    this.blockStates))
            ];
        return
        [
            new SubcommandIf(ConditionalSubcommandBlock.New(this.x, this.y, this.z, this.block, this.blockStates))
        ];
    }

    public override string GetDescription()
    {
        return this.inverted
            ? $"block at ({this.x} {this.y} {this.z}) is not {this.block}{this.blockStates.ToVanillaSyntax()}"
            : $"block at ({this.x} {this.y} {this.z}) is {this.block}{this.blockStates.ToVanillaSyntax()}";
    }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets() { return []; }
}

public class ComparisonBlocks : Comparison
{
    private readonly Coordinate beginX, beginY, beginZ;
    private readonly Coordinate destX, destY, destZ;
    private readonly Coordinate endX, endY, endZ;
    private readonly BlocksScanMode scanMode;

    private ScoreboardValue temp;

    public ComparisonBlocks(Coordinate beginX,
        Coordinate beginY,
        Coordinate beginZ,
        Coordinate endX,
        Coordinate endY,
        Coordinate endZ,
        Coordinate destX,
        Coordinate destY,
        Coordinate destZ,
        BlocksScanMode scanMode,
        bool invert) : base(invert)
    {
        this.beginX = beginX;
        this.beginY = beginY;
        this.beginZ = beginZ;
        this.endX = endX;
        this.endY = endY;
        this.endZ = endZ;
        this.destX = destX;
        this.destY = destY;
        this.destZ = destZ;
        this.scanMode = scanMode;
    }
    public override IEnumerable<string> GetCommands(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        if (!willBeInverted)
        {
            cancel = false;
            return null;
        }

        List<string> commands = [];

        this.temp = executor.scoreboard.temps.Request(true);
        commands.Add(Command.ScoreboardSet(this.temp, 0));
        commands.Add(Command.Execute()
            .IfBlocks(this.beginX, this.beginY, this.beginZ, this.endX, this.endY, this.endZ, this.destX, this.destY,
                this.destZ, this.scanMode)
            .Run(Command.ScoreboardSet(this.temp, 1)));

        cancel = false;
        return commands;
    }
    public override Subcommand[] GetExecuteChunks(Executor executor,
        Statement callingStatement,
        bool willBeInverted,
        out bool cancel)
    {
        cancel = false;

        if (willBeInverted)
        {
            var range = new Range(1, this.inverted);
            return [new SubcommandIf(ConditionalSubcommandScore.New(this.temp, range))];
        }

        if (this.inverted)
            return
            [
                new SubcommandUnless(ConditionalSubcommandBlocks.New(this.beginX, this.beginY, this.beginZ, this.endX,
                    this.endY, this.endZ, this.destX, this.destY, this.destZ, this.scanMode))
            ];
        return
        [
            new SubcommandIf(ConditionalSubcommandBlocks.New(this.beginX, this.beginY, this.beginZ, this.endX,
                this.endY, this.endZ, this.destX, this.destY, this.destZ, this.scanMode))
        ];
    }

    public override string GetDescription()
    {
        return this.inverted
            ? $"{this.scanMode} blocks between ({this.beginX} {this.beginY} {this.beginZ}) and ({this.endX} {this.endY} {this.endZ}) do not match the blocks at ({this.destX} {this.destY} {this.destZ})"
            : $"{this.scanMode} blocks between ({this.beginX} {this.beginY} {this.beginZ}) and ({this.endX} {this.endY} {this.endZ}) match the blocks at ({this.destX} {this.destY} {this.destZ})";
    }
    public override IEnumerable<ScoreboardValue> GetAssertionTargets() { return Array.Empty<ScoreboardValue>(); }
}

/// <summary>
///     The type of an operand in a comparison-based statement
/// </summary>
public enum SideType
{
    Unknown, Constant, Variable
}