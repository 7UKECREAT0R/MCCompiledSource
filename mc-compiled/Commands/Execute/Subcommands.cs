using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Execute;

/// <summary>
///     [Flags] Represents an alignment to x/y/z.
/// </summary>
[Flags]
public enum Axes : byte
{
    none = 0,
    x = 1 << 0,
    y = 1 << 1,
    z = 1 << 2
}

internal class SubcommandAlign : Subcommand
{
    internal SubcommandAlign() { }
    internal SubcommandAlign(Axes axes)
    {
        this.axes = axes;
    }

    private Axes axes { get; set; }

    public override TypePattern[] Patterns =>
    [
        new(
            new NamedType(typeof(TokenStringLiteral), "axes")
        )
    ];
    public override string Keyword => "align";
    public override bool TerminatesChain => false;
    /// <summary>
    ///     Parse a set of axes based on a string input.
    /// </summary>
    /// <returns></returns>
    internal static Axes ParseAxes(string input)
    {
        char[] characters = input.ToCharArray();
        var axes = Axes.none;

        foreach (char c in characters)
            switch (c)
            {
                case 'x':
                    axes |= Axes.x;
                    break;
                case 'y':
                    axes |= Axes.y;
                    break;
                case 'z':
                    axes |= Axes.z;
                    break;
            }

        return axes;
    }
    /// <summary>
    ///     Convert a set of axes flags to
    /// </summary>
    /// <param name="axes"></param>
    /// <returns></returns>
    private static string FromAxes(Axes axes)
    {
        var sb = new StringBuilder();

        if ((axes & Axes.x) != 0)
            sb.Append("x");
        if ((axes & Axes.y) != 0)
            sb.Append("y");
        if ((axes & Axes.z) != 0)
            sb.Append("z");

        return sb.ToString();
    }

    public override void FromTokens(Statement tokens)
    {
        var literal = tokens.Next<TokenStringLiteral>("axes");
        string axesString = literal.text;
        this.axes = ParseAxes(axesString);
    }
    public override string ToMinecraft()
    {
        return $"align {FromAxes(this.axes)}";
    }
}

internal class SubcommandAnchored : Subcommand
{
    private AnchorPosition anchor;

    internal SubcommandAnchored() { }
    internal SubcommandAnchored(AnchorPosition anchor)
    {
        this.anchor = anchor;
    }

    public override TypePattern[] Patterns =>
    [
        new(
            new NamedType(typeof(TokenIdentifierEnum), "anchor")
        )
    ];
    public override string Keyword => "anchored";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        var @enum = tokens.Next<TokenIdentifierEnum>("anchor position");
        ParsedEnumValue parsedEnum = @enum.value;

        parsedEnum.RequireType<AnchorPosition>(tokens);

        this.anchor = (AnchorPosition) parsedEnum.value;
    }
    public override string ToMinecraft()
    {
        return $"anchored {this.anchor}";
    }
}

internal class SubcommandAs : Subcommand
{
    internal Selector entity;

    internal SubcommandAs() { }
    internal SubcommandAs(Selector entity)
    {
        this.entity = entity;
    }

    public override TypePattern[] Patterns =>
    [
        new(
            new NamedType(typeof(TokenSelectorLiteral), "target")
        )
    ];
    public override string Keyword => "as";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        var selector = tokens.Next<TokenSelectorLiteral>("entity");
        this.entity = selector.selector;
    }
    public override string ToMinecraft()
    {
        return $"as {this.entity}";
    }
}

internal class SubcommandAt : Subcommand
{
    private Selector entity;

    internal SubcommandAt() { }
    internal SubcommandAt(Selector entity)
    {
        this.entity = entity;
    }

    public override TypePattern[] Patterns =>
    [
        new(
            new NamedType(typeof(TokenSelectorLiteral), "target")
        )
    ];
    public override string Keyword => "at";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        var selector = tokens.Next<TokenSelectorLiteral>("entity");
        this.entity = selector.selector;
    }
    public override string ToMinecraft()
    {
        return $"at {this.entity}";
    }
}

internal class SubcommandFacing : Subcommand
{
    private AnchorPosition anchor;

    private Selector entity;
    private bool isEntity;
    private Coordinate x;
    private Coordinate y;
    private Coordinate z;

    internal SubcommandFacing() { }
    internal SubcommandFacing(bool isEntity, Selector entity, AnchorPosition anchor, Coordinate x, Coordinate y,
        Coordinate z)
    {
        this.isEntity = isEntity;
        this.entity = entity;
        this.anchor = anchor;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override TypePattern[] Patterns =>
    [
        // coordinate
        new(
            new NamedType(typeof(TokenCoordinateLiteral), "x"),
            new NamedType(typeof(TokenCoordinateLiteral), "y"),
            new NamedType(typeof(TokenCoordinateLiteral), "z")
        ),
        // entity
        new(
            new NamedType(typeof(TokenSelectorLiteral), "entity"),
            new NamedType(typeof(TokenIdentifierEnum), "anchor")
        )
    ];
    public override string Keyword => "facing";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        // entity
        if (tokens.NextIs<TokenSelectorLiteral>(false))
        {
            this.isEntity = true;

            this.entity = tokens.Next<TokenSelectorLiteral>("entity");

            ParsedEnumValue parsedEnum = tokens.Next<TokenIdentifierEnum>("anchor").value;
            parsedEnum.RequireType<AnchorPosition>(tokens);
            this.anchor = (AnchorPosition) parsedEnum.value;
            return;
        }

        // coordinate
        this.isEntity = false;

        this.x = tokens.Next<TokenCoordinateLiteral>("x");
        this.y = tokens.Next<TokenCoordinateLiteral>("y");
        this.z = tokens.Next<TokenCoordinateLiteral>("z");
    }
    public override string ToMinecraft()
    {
        if (this.isEntity)
            return $"facing entity {this.entity} {this.anchor}";

        return $"facing {this.x} {this.y} {this.z}";
    }
}

internal class SubcommandIn : Subcommand
{
    private Dimension dimension;

    internal SubcommandIn() { }
    internal SubcommandIn(Dimension dimension)
    {
        this.dimension = dimension;
    }

    public override TypePattern[] Patterns =>
    [
        new(
            new NamedType(typeof(TokenIdentifierEnum), "dimension")
        )
    ];
    public override string Keyword => "in";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        var @enum = tokens.Next<TokenIdentifierEnum>("dimension");
        ParsedEnumValue parsedEnum = @enum.value;

        parsedEnum.RequireType<Dimension>(tokens);

        this.dimension = (Dimension) parsedEnum.value;
    }
    public override string ToMinecraft()
    {
        return $"in {this.dimension}";
    }
}

internal class SubcommandPositioned : Subcommand
{
    private bool asEntity;
    private Selector entity;
    private Coordinate x, y, z;

    internal SubcommandPositioned() { }
    internal SubcommandPositioned(bool asEntity, Selector entity, Coordinate x, Coordinate y, Coordinate z)
    {
        this.asEntity = asEntity;
        this.entity = entity;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override TypePattern[] Patterns =>
    [
        // coordinate
        new(
            new NamedType(typeof(TokenCoordinateLiteral), "x"),
            new NamedType(typeof(TokenCoordinateLiteral), "y"),
            new NamedType(typeof(TokenCoordinateLiteral), "z")
        ),
        // entity
        new(
            new NamedType(typeof(TokenSelectorLiteral), "entity")
        )
    ];
    public override string Keyword => "positioned";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        // entity
        if (tokens.NextIs<TokenSelectorLiteral>(false))
        {
            this.asEntity = true;
            this.entity = tokens.Next<TokenSelectorLiteral>("entity");
            return;
        }

        // coords
        this.x = tokens.Next<TokenCoordinateLiteral>("x");
        this.y = tokens.Next<TokenCoordinateLiteral>("y");
        this.z = tokens.Next<TokenCoordinateLiteral>("z");
    }
    public override string ToMinecraft()
    {
        if (this.asEntity)
            return $"positioned as {this.entity}";

        return $"positioned {this.x} {this.y} {this.z}";
    }
}

internal class SubcommandRotated : Subcommand
{
    private bool asEntity;
    private Selector entity;
    private Coordinate yaw, pitch;

    internal SubcommandRotated() { }
    internal SubcommandRotated(bool asEntity, Selector entity, Coordinate yaw, Coordinate pitch)
    {
        this.asEntity = asEntity;
        this.entity = entity;
        this.yaw = yaw;
        this.pitch = pitch;
    }

    public override TypePattern[] Patterns =>
    [
        // coordinate
        new(
            new NamedType(typeof(TokenCoordinateLiteral), "yaw"),
            new NamedType(typeof(TokenCoordinateLiteral), "pitch")
        ),
        // entity
        new(
            new NamedType(typeof(TokenSelectorLiteral), "entity")
        )
    ];
    public override string Keyword => "rotated";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        // entity
        if (tokens.NextIs<TokenSelectorLiteral>(false))
        {
            this.asEntity = true;
            this.entity = tokens.Next<TokenSelectorLiteral>("entity");
            return;
        }

        // coords
        this.yaw = tokens.Next<TokenCoordinateLiteral>("yaw");
        this.pitch = tokens.Next<TokenCoordinateLiteral>("pitch");
    }
    public override string ToMinecraft()
    {
        if (this.asEntity)
            return $"rotated as {this.entity}";

        return $"rotated {this.yaw} {this.pitch}";
    }
}

internal class SubcommandRun : Subcommand
{
    private string command;

    /// <summary>
    ///     Create a SubcommandRun with a command given.
    /// </summary>
    /// <param name="command"></param>
    internal SubcommandRun(string command)
    {
        this.command = command;
    }
    /// <summary>
    ///     Create a SubcommandRun that leaves the command field empty.
    /// </summary>
    internal SubcommandRun()
    {
        this.command = "";
    }

    public override TypePattern[] Patterns =>
    [
        new(
            new NamedType(typeof(TokenStringLiteral), "command")
        )
    ];
    public override string Keyword => "run";
    public override bool TerminatesChain => true;

    public override void FromTokens(Statement tokens)
    {
        this.command = string.Join(" ", tokens.GetRemainingTokens().Select(tok => tok.ToString()));
    }
    public override string ToMinecraft()
    {
        return $"run {this.command}";
    }
}

internal class SubcommandIf : Subcommand
{
    internal ConditionalSubcommand condition;

    internal SubcommandIf() { }
    internal SubcommandIf(ConditionalSubcommand condition)
    {
        this.condition = condition;
    }

    public override TypePattern[] Patterns
    {
        get
        {
            IEnumerable<TypePattern> _validPatterns =
                ConditionalSubcommand.CONDITIONAL_EXAMPLES.SelectMany(c => c.Patterns);
            TypePattern[] validPatterns = _validPatterns as TypePattern[] ?? _validPatterns.ToArray();

            foreach (TypePattern pattern in validPatterns)
                pattern.PrependAnd(new NamedType(typeof(TokenIdentifier)), "subcommand");

            return validPatterns.ToArray();
        }
    }
    public override string Keyword => "if";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        string word = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();

        // load condition
        this.condition = ConditionalSubcommand.GetConditionalSubcommandForKeyword(word, tokens);

        // load the statement's parameters based on the following input
        this.condition.FromTokens(tokens);
    }
    public override string ToMinecraft()
    {
        return $"if {this.condition.ToMinecraft()}";
    }
}

internal class SubcommandUnless : Subcommand
{
    internal ConditionalSubcommand condition;

    internal SubcommandUnless() { }
    internal SubcommandUnless(ConditionalSubcommand condition)
    {
        this.condition = condition;
    }

    public override TypePattern[] Patterns
    {
        get
        {
            IEnumerable<TypePattern> _validPatterns =
                ConditionalSubcommand.CONDITIONAL_EXAMPLES.SelectMany(c => c.Patterns);
            TypePattern[] validPatterns = _validPatterns as TypePattern[] ?? _validPatterns.ToArray();

            foreach (TypePattern pattern in validPatterns)
                pattern.PrependAnd(new NamedType(typeof(TokenIdentifier)), "subcommand");

            return validPatterns.ToArray();
        }
    }
    public override string Keyword => "unless";
    public override bool TerminatesChain => false;

    public override void FromTokens(Statement tokens)
    {
        string word = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();

        // load condition
        this.condition = ConditionalSubcommand.GetConditionalSubcommandForKeyword(word, tokens);

        // load the statement's parameters based on the following input
        this.condition.FromTokens(tokens);
    }
    public override string ToMinecraft()
    {
        return $"unless {this.condition.ToMinecraft()}";
    }
}