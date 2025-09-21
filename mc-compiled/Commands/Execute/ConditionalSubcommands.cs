using System;
using JetBrains.Annotations;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Execute;

internal class ConditionalSubcommandBlock : ConditionalSubcommand
{
    private string block;
    [CanBeNull]
    private BlockState[] states;
    private Coordinate x, y, z;

    public ConditionalSubcommandBlock() { }
    private ConditionalSubcommandBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        string block,
        [CanBeNull] BlockState[] states)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.block = block;
        this.states = states;
    }

    public override string Keyword => "block";

    /// <summary>
    ///     Create a ConditionalSubcommandBlock with the given parameters.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="block"></param>
    /// <param name="states"></param>
    /// <returns></returns>
    internal static ConditionalSubcommandBlock New(Coordinate x,
        Coordinate y,
        Coordinate z,
        string block,
        BlockState[] states = null)
    {
        return new ConditionalSubcommandBlock(x, y, z, block, states);
    }

    public override void FromTokens(Statement tokens)
    {
        this.x = tokens.Next<TokenCoordinateLiteral>("x");
        this.y = tokens.Next<TokenCoordinateLiteral>("y");
        this.z = tokens.Next<TokenCoordinateLiteral>("z");
        this.block = tokens.Next<TokenStringLiteral>("block");

        if (tokens.NextIs<TokenBlockStatesLiteral>(false))
        {
            this.states = tokens.Next<TokenBlockStatesLiteral>("block states");
            this.states.ValidateIfKnownVanillaBlock(this.block, true, tokens);
        }
    }
    public override string ToMinecraft()
    {
        return this.states is {Length: > 0}
            ? $"block {this.x} {this.y} {this.z} {this.block} {this.states.ToVanillaSyntax()}"
            : $"block {this.x} {this.y} {this.z} {this.block}";
    }
}

internal class ConditionalSubcommandBlocks : ConditionalSubcommand
{
    private Coordinate beginX, beginY, beginZ;
    private Coordinate destX, destY, destZ;
    private Coordinate endX, endY, endZ;
    private BlocksScanMode scanMode;

    public ConditionalSubcommandBlocks() { }
    private ConditionalSubcommandBlocks(
        Coordinate beginX,
        Coordinate beginY,
        Coordinate beginZ,
        Coordinate endX,
        Coordinate endY,
        Coordinate endZ,
        Coordinate destX,
        Coordinate destY,
        Coordinate destZ,
        BlocksScanMode scanMode)
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

    public override string Keyword => "block";

    /// <summary>
    ///     Create a ConditionalSubcommandBlocks with the given parameters.
    /// </summary>
    /// <param name="beginX"></param>
    /// <param name="beginY"></param>
    /// <param name="beginZ"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <param name="endZ"></param>
    /// <param name="destX"></param>
    /// <param name="destY"></param>
    /// <param name="destZ"></param>
    /// <param name="scanMode"></param>
    /// <returns></returns>
    internal static ConditionalSubcommandBlocks New(
        Coordinate beginX,
        Coordinate beginY,
        Coordinate beginZ,
        Coordinate endX,
        Coordinate endY,
        Coordinate endZ,
        Coordinate destX,
        Coordinate destY,
        Coordinate destZ,
        BlocksScanMode scanMode)
    {
        return new ConditionalSubcommandBlocks(beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode);
    }

    public override void FromTokens(Statement tokens)
    {
        this.beginX = tokens.Next<TokenCoordinateLiteral>("start X");
        this.beginY = tokens.Next<TokenCoordinateLiteral>("start Y");
        this.beginZ = tokens.Next<TokenCoordinateLiteral>("start Z");

        this.endX = tokens.Next<TokenCoordinateLiteral>("end X");
        this.endY = tokens.Next<TokenCoordinateLiteral>("end Y");
        this.endZ = tokens.Next<TokenCoordinateLiteral>("end Z");

        this.destX = tokens.Next<TokenCoordinateLiteral>("destination X");
        this.destY = tokens.Next<TokenCoordinateLiteral>("destination Y");
        this.destZ = tokens.Next<TokenCoordinateLiteral>("destination Z");

        RecognizedEnumValue recognizedEnum = tokens.Next<TokenIdentifierEnum>("scan mode").value;
        recognizedEnum.RequireType<BlocksScanMode>(tokens);
        this.scanMode = (BlocksScanMode) recognizedEnum.value;
    }
    public override string ToMinecraft()
    {
        return
            $"blocks {this.beginX} {this.beginY} {this.beginZ} {this.endX} {this.endY} {this.endZ} {this.destX} {this.destY} {this.destZ} {this.scanMode}";
    }
}

internal class ConditionalSubcommandEntity : ConditionalSubcommand
{
    internal Selector entity;

    public ConditionalSubcommandEntity() { }
    private ConditionalSubcommandEntity(Selector entity) { this.entity = entity; }

    public override string Keyword => "entity";
    public override bool TerminatesChain => false;

    /// <summary>
    ///     Create a ConditionalSubcommandEntity that contains the given entity.
    /// </summary>
    /// <param name="entity">The entity(ies) to check for existing.</param>
    /// <returns></returns>
    internal static ConditionalSubcommandEntity New(Selector entity) { return new ConditionalSubcommandEntity(entity); }

    public override void FromTokens(Statement tokens) { this.entity = tokens.Next<TokenSelectorLiteral>("entity"); }
    public override string ToMinecraft() { return $"entity {this.entity}"; }
}

internal class ConditionalSubcommandScore : ConditionalSubcommand
{
    private readonly string otherSelector;

    // The reason this isn't a ScoreboardValue is that sometimes the user
    // uses a different selector, and it's not worth cloning it.
    private readonly string sourceSelector;
    internal bool comparesRange;

    // if !comparesRange
    internal TokenCompare.Type comparisonType;
    internal string otherValue;

    // if comparesRange
    internal Range range;
    internal string sourceValue;

    public ConditionalSubcommandScore() { }
    private ConditionalSubcommandScore(bool comparesRange,
        ScoreboardValue sourceValue,
        Range range,
        TokenCompare.Type comparisonType,
        ScoreboardValue otherValue)
    {
        this.comparesRange = comparesRange;

        this.sourceSelector = sourceValue.clarifier.CurrentString;
        this.sourceValue = sourceValue.InternalName;

        this.range = range;
        this.comparisonType = comparisonType;

        if (otherValue != null)
        {
            this.otherSelector = otherValue.clarifier.CurrentString;
            this.otherValue = otherValue.InternalName;
        }
        else
        {
            this.otherSelector = null;
            this.otherValue = null;
        }
    }
    private ConditionalSubcommandScore(bool comparesRange,
        string sourceSelector,
        string sourceValue,
        Range range,
        TokenCompare.Type comparisonType,
        string otherSelector,
        string otherValue)
    {
        this.comparesRange = comparesRange;

        this.sourceSelector = sourceSelector;
        this.sourceValue = sourceValue;

        this.range = range;
        this.comparisonType = comparisonType;

        this.otherSelector = otherSelector;
        this.otherValue = otherValue;
    }

    private bool SourceIsGlobal => this.sourceSelector.Equals(Executor.FAKE_PLAYER_NAME);
    internal Clarifier SourceClarifier => new(this.SourceIsGlobal, this.sourceSelector);

    private bool OtherIsGlobal => this.otherSelector.Equals(Executor.FAKE_PLAYER_NAME);
    internal Clarifier OtherClarifier => new(this.OtherIsGlobal, this.otherSelector);

    public override string Keyword => "score";
    public override bool TerminatesChain => false;

    /// <summary>
    ///     Create a ConditionalSubcommandScore that compares a scoreboard value with a range.
    /// </summary>
    /// <param name="source">The first value.</param>
    /// <param name="range">The range to compare against.</param>
    /// <returns></returns>
    internal static ConditionalSubcommandScore New(ScoreboardValue source, Range range)
    {
        return new ConditionalSubcommandScore(true, source, range, 0, null);
    }
    /// <summary>
    ///     Create a ConditionalSubcommandScore that compares a scoreboard value with another scoreboard value.
    /// </summary>
    /// <param name="source">The first value.</param>
    /// <param name="type">The comparison type/operator used.</param>
    /// <param name="other">The other value.</param>
    /// <returns></returns>
    internal static ConditionalSubcommandScore New(ScoreboardValue source,
        TokenCompare.Type type,
        ScoreboardValue other)
    {
        return new ConditionalSubcommandScore(false, source, Range.zero, type, other);
    }

    /// <summary>
    ///     Create a ConditionalSubcommandScore that compares a scoreboard value with a range.
    /// </summary>
    /// <param name="selector">The selector for the objective to compare.</param>
    /// <param name="objective">The objective name to compare.</param>
    /// <param name="range">The range to compare against.</param>
    /// <returns></returns>
    internal static ConditionalSubcommandScore New(string selector, string objective, Range range)
    {
        return new ConditionalSubcommandScore(true, selector, objective, range, 0, null, null);
    }

    /// <summary>
    ///     Create a ConditionalSubcommandScore that compares a scoreboard value with another scoreboard value.
    /// </summary>
    /// <param name="sourceSelector">The selector to use for the first value.</param>
    /// <param name="sourceObjective">The first value.</param>
    /// <param name="type">The comparison type/operator used.</param>
    /// <param name="otherSelector">The selector to use for the other value.</param>
    /// <param name="otherObjective">The other value.</param>
    /// <returns></returns>
    internal static ConditionalSubcommandScore New(string sourceSelector,
        string sourceObjective,
        TokenCompare.Type type,
        string otherSelector,
        string otherObjective)
    {
        return new ConditionalSubcommandScore(false, sourceSelector, sourceObjective, Range.zero, type, otherSelector,
            otherObjective);
    }

    public override void FromTokens(Statement tokens)
    {
        this.sourceValue = tokens.Next<TokenIdentifierValue>("source").value.InternalName;

        // thisScore == otherScore
        if (tokens.NextIs<TokenCompare>(false, false))
        {
            this.comparesRange = false;
            this.comparisonType = tokens.Next<TokenCompare>("comparison operator").GetCompareType();
            this.otherValue = tokens.Next<TokenIdentifierValue>("other").value.InternalName;
            return;
        }

        // not a valid comparison operator, check it.
        string middleWord = tokens.Next<TokenIdentifier>("comparison operator").word;
        if (!middleWord.Equals("matches", StringComparison.OrdinalIgnoreCase))
        {
            if (middleWord.StartsWith("m", StringComparison.OrdinalIgnoreCase))
                throw new StatementException(tokens,
                    $"Unknown comparison operator: {middleWord}. Did you mean 'matches'?");
            throw new StatementException(tokens, $"Unknown comparison operator: {middleWord}.");
        }

        // thisScore matches 1..10
        this.comparesRange = true;
        this.range = tokens.Next<TokenRangeLiteral>("range").range;
    }
    public override string ToMinecraft()
    {
        if (this.comparesRange)
            return $"score {this.sourceSelector} {this.sourceValue} matches {this.range.ToString()}";

        string operatorString = TokenCompare.GetMinecraftOperator(this.comparisonType);
        return
            $"score {this.sourceSelector} {this.sourceValue} {operatorString} {this.otherSelector} {this.otherValue}";
    }
}