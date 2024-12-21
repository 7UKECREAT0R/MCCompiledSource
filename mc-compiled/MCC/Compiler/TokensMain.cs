using System;
using System.Diagnostics;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.Async;
using mc_compiled.MCC.Functions;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Represents a new line.
/// </summary>
public sealed class TokenNewline : Token, ITerminating
{
    public TokenNewline(int lineNumber) : base(lineNumber) { }
    public override string AsString()
    {
        return "\n";
    }
}

/// <summary>
///     Represents a directive call.
/// </summary>
public sealed class TokenDirective : Token, IImplicitToken
{
    public readonly Directive directive;

    public TokenDirective(Directive directive, int lineNumber) : base(lineNumber)
    {
        this.directive = directive;
    }

    public Type[] GetImplicitTypes()
    {
        if (this.directive.enumValue.HasValue)
            return [typeof(TokenIdentifier), typeof(TokenIdentifierEnum)];
        return [typeof(TokenIdentifier)];
    }
    public Token Convert(Executor executor, int index)
    {
        switch (index)
        {
            case 0:
                return new TokenIdentifier(this.directive.identifier, this.lineNumber);
            case 1:
                Debug.Assert(this.directive.enumValue != null, "directive.enumValue was null");
                return new TokenIdentifierEnum(this.directive.identifier, this.directive.enumValue.Value,
                    this.lineNumber);
        }

        return null;
    }

    public override string AsString()
    {
        return this.directive.identifier;
    }
}

/// <summary>
///     Represents a comment that was made using two slashes.
/// </summary>
public sealed class TokenComment : Token, IUselessInformation
{
    public readonly string contents;
    public TokenComment(string contents, int lineNumber) : base(lineNumber)
    {
        this.contents = contents;
    }

    public override string AsString()
    {
        return "// " + this.contents;
    }
}

/// <summary>
///     Represents the return value of calling an async function. It is possible to `await` this value.
/// </summary>
public sealed class TokenAwaitable : Token
{
    public readonly AsyncFunction function;
    public TokenAwaitable(AsyncFunction function, int lineNumber) : base(lineNumber)
    {
        this.function = function;
    }
    public override string AsString()
    {
        return $"[awaitable: {this.function.escapedFunctionName}]";
    }
}

/// <summary>
///     Represents a token which doesn't have any identifiable tokenization-time category,
///     but is probably an identifier. Should probably resolve when possible.
/// </summary>
public class TokenIdentifier : Token, IPreprocessor, IImplicitToken
{
    /// <summary>
    ///     Convert to STRING token.
    /// </summary>
    public const int CONVERT_STRING = 0;
    /// <summary>
    ///     Convert to BUILDER token.
    /// </summary>
    public const int CONVERT_BUILDER = 1;
    public readonly string word;
    public TokenIdentifier(string word, int lineNumber) : base(lineNumber)
    {
        this.word = word;
    }

    public Type[] GetImplicitTypes()
    {
        return
        [
            typeof(TokenStringLiteral),
            typeof(TokenBuilderIdentifier)
        ];
    }
    public Token Convert(Executor executor, int index)
    {
        return index switch
        {
            CONVERT_STRING => new TokenStringLiteral(this.word, this.lineNumber),
            CONVERT_BUILDER => new TokenBuilderIdentifier(this.word, this.lineNumber),
            _ => null
        };
    }
    public object GetValue()
    {
        return this.word;
    }

    public override string AsString()
    {
        return this.word;
    }
}

/// <summary>
///     Represents a selector that needs to be resolved within the active context, rather than statically.
/// </summary>
public sealed class TokenUnresolvedSelector : Token
{
    private readonly UnresolvedSelector unresolvedSelector;
    public TokenUnresolvedSelector(UnresolvedSelector unresolvedSelector, int lineNumber) : base(lineNumber)
    {
        this.unresolvedSelector = unresolvedSelector;
    }
    public override string AsString()
    {
        return $"{this.unresolvedSelector}";
    }

    public TokenSelectorLiteral Resolve(Executor executor)
    {
        return new TokenSelectorLiteral(this.unresolvedSelector.Resolve(executor), this.lineNumber);
    }
}

/// <summary>
///     Represents a reference to what is probably a directive's builder field.
/// </summary>
public sealed class TokenBuilderIdentifier : TokenIdentifier
{
    private readonly string builderField;
    public TokenBuilderIdentifier(string fullWord, int lineNumber) : base(fullWord, lineNumber)
    {
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (fullWord.EndsWith(":")) // it should as long as its not implicitly converted from identifier
            this.builderField = fullWord[..^1].Trim();
        else
            this.builderField = fullWord.Trim();
    }

    /// <summary>
    ///     This builder field converted to full upper-case, not containing the colon.
    /// </summary>
    public string BuilderField => this.builderField.ToUpper();
}

/// <summary>
///     Represents an enum constant defined by the compiler.
/// </summary>
public sealed class TokenIdentifierEnum : TokenIdentifier, IDocumented
{
    public readonly ParsedEnumValue value;
    internal TokenIdentifierEnum() : base(null, -1) { }
    public TokenIdentifierEnum(string word, ParsedEnumValue value, int lineNumber) : base(word, lineNumber)
    {
        this.value = value;
    }

    public string GetDocumentation()
    {
        return "Usually a specific keyword in a subset of possible keywords. This type is entirely context dependent.";
    }
    public override string AsString()
    {
        return this.value.value.ToString();
    }
}

/// <summary>
///     Represents a reference to a scoreboard value.
/// </summary>
public sealed class TokenIdentifierValue : TokenIdentifier, IIndexable, IDocumented
{
    /// <summary>
    ///     The value this identifier references.
    /// </summary>
    public readonly ScoreboardValue value;

    [UsedImplicitly]
    private TokenIdentifierValue() :
        base(null, -1) { } // if you remove this method the markdown exporter will blow up because it uses Activator and needs this

    public TokenIdentifierValue(string word, ScoreboardValue value, int lineNumber) : base(word, lineNumber)
    {
        this.value = value;
    }

    /// <summary>
    ///     Get the full name used to access this value.
    /// </summary>
    public string Accessor => this.word;

    /// <summary>
    ///     Shorthand for .value.clarifier.CurrentString();
    /// </summary>
    public string ClarifierStr => this.value.clarifier.CurrentString;
    public string GetDocumentation()
    {
        return "The name of a runtime value that was defined using the `define` command.";
    }

    public Token Index(TokenIndexer indexer, Statement forExceptions)
    {
        if (this.value.HasAttribute<AttributeGlobal>() || this.value.clarifier.IsGlobal)
            throw new StatementException(forExceptions, "Cannot clarify a value that is defined as global.");

        switch (indexer)
        {
            case TokenIndexerString @string:
            {
                ScoreboardValue clone = this.value.Clone(forExceptions);
                string fakePlayer = @string.token.text;
                clone.clarifier.SetString(fakePlayer);
                return new TokenIdentifierValue(this.word, clone, this.lineNumber);
            }
            case TokenIndexerAsterisk _:
            {
                ScoreboardValue clone = this.value.Clone(forExceptions);
                clone.clarifier.SetString("*");
                return new TokenIdentifierValue(this.word, clone, this.lineNumber);
            }
            case TokenIndexerSelector selector:
            {
                ScoreboardValue clone = this.value.Clone(forExceptions);
                clone.clarifier.SetSelector(selector.token.selector);
                return new TokenIdentifierValue(this.word, clone, this.lineNumber);
            }
            default:
                throw indexer.GetException(this, forExceptions);
        }
    }
}

public sealed class TokenIdentifierPreprocessor : TokenIdentifier, IIndexable, IDocumented
{
    internal readonly PreprocessorVariable variable;

    [UsedImplicitly]
    private TokenIdentifierPreprocessor() :
        base(null, -1) { } // if you remove this method the markdown exporter will blow up because it uses Activator and needs this

    public TokenIdentifierPreprocessor(string word, PreprocessorVariable variable, int lineNumber) : base(word,
        lineNumber)
    {
        this.variable = variable;
    }
    public string GetDocumentation()
    {
        return "The name of a preprocessor variable that was defined using the `$var`, `$json`, or other command.";
    }

    public Token Index(TokenIndexer indexer, Statement forExceptions)
    {
        if (indexer is not TokenIndexerInteger integer)
            throw indexer.GetException(this, forExceptions);

        int input = integer.token.number;
        int length = this.variable.Length;

        if (input < 0 || input >= length)
            throw integer.GetIndexOutOfBoundsException(0, length - 1, forExceptions);

        dynamic result = this.variable[input];

        if (result == null)
            throw new StatementException(forExceptions,
                "Preprocessor variable contained an unexpecteed null value. Report this as a github issue or in the Discord.");

        switch (result)
        {
            case TokenLiteral literal:
                return literal;
            case TokenIdentifier identifier:
                return identifier;
            default:
                TokenLiteral newLiteral = PreprocessorUtils.DynamicToLiteral(result, this.lineNumber);
                if (newLiteral == null)
                    throw new StatementException(forExceptions,
                        "Preprocessor variable contained an unexpected type that could not be converted to a TokenLiteral (sparse support?). Type: " +
                        result.GetType().FullName);
                return newLiteral;
        }
    }
}

/// <summary>
///     Represents a reference to a user-defined macro.
/// </summary>
public sealed class TokenIdentifierMacro : TokenIdentifier
{
    /// <summary>
    ///     The macro this identifier references.
    /// </summary>
    public readonly Macro macro;

    public TokenIdentifierMacro(Macro macro, int lineNumber) : base(macro.name, lineNumber)
    {
        this.macro = macro;
    }
}

/// <summary>
///     Represents a reference to multiple functions that fell under a keyword.
/// </summary>
public sealed class TokenIdentifierFunction : TokenIdentifier
{
    /// <summary>
    ///     The function this identifier references.
    /// </summary>
    public readonly Function[] functions;

    public TokenIdentifierFunction(string keyword, Function[] functions, int lineNumber) : base(keyword, lineNumber)
    {
        this.functions = functions;
    }
}

/// <summary>
///     Allows this object to return an object that can go into a PPV.
/// </summary>
public interface IPreprocessor
{
    object GetValue();
}

/// <summary>
///     Allows this object to be indexed through a set of values.
/// </summary>
public interface IIndexable
{
    /// <summary>
    ///     Index this object using an indexer.
    /// </summary>
    /// <param name="indexer">The indexer to use when accessing this object.</param>
    /// <param name="forExceptions">The statement to blame if something blows up.</param>
    /// <returns></returns>
    Token Index(TokenIndexer indexer, Statement forExceptions);
}