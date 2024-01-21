using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Functions;
using System;
using System.Diagnostics;
using JetBrains.Annotations;
using mc_compiled.MCC.Attributes;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Represents a new line.
    /// </summary>
    public sealed class TokenNewline : Token, ITerminating
    {
        public override string AsString() => "\n";
        public TokenNewline(int lineNumber) : base(lineNumber) {}
    }
    /// <summary>
    /// Represents a directive call.
    /// </summary>
    public sealed class TokenDirective : Token, IImplicitToken
    {
        public readonly Directive directive;

        public override string AsString() => directive.identifier;

        public Type[] GetImplicitTypes()
        {
            if(directive.enumValue.HasValue)
                return new[] { typeof(TokenIdentifier), typeof(TokenIdentifierEnum) };
            else
                return new[] { typeof(TokenIdentifier) };
        }
        public Token Convert(Executor executor, int index)
        {
            switch(index)
            {
                case 0:
                    return new TokenIdentifier(directive.identifier, lineNumber);
                case 1:
                    Debug.Assert(directive.enumValue != null, "directive.enumValue was null");
                    return new TokenIdentifierEnum(directive.identifier, directive.enumValue.Value, lineNumber);
            }
            return null;
        }

        public TokenDirective(Directive directive, int lineNumber) : base(lineNumber)
        {
            this.directive = directive;
        }
    }
    /// <summary>
    /// Represents a comment that was made using two slashes.
    /// </summary>
    public sealed class TokenComment : Token, IInformationless
    {
        public readonly string contents;

        public override string AsString() => "// " + contents;
        public TokenComment(string contents, int lineNumber) : base(lineNumber)
        {
            this.contents = contents;
        }
    }

    /// <summary>
    /// Represents a token which doesn't have any identifiable tokenization-time category,
    /// but is probably an identifier. Should probably resolve when possible.
    /// </summary>
    public class TokenIdentifier : Token, IPreprocessor, IImplicitToken
    {
        public readonly string word;

        /// <summary>
        /// Convert to STRING token.
        /// </summary>
        public const int CONVERT_STRING = 0;
        /// <summary>
        /// Convert to BUILDER token.
        /// </summary>
        public const int CONVERT_BUILDER = 1;

        public override string AsString() => word;
        public TokenIdentifier(string word, int lineNumber) : base(lineNumber)
        {
            this.word = word;
        }
        public object GetValue() => word;

        public Type[] GetImplicitTypes() =>
            new[]
            {
                typeof(TokenStringLiteral),
                typeof(TokenBuilderIdentifier),
            };
        public Token Convert(Executor executor, int index)
        {
            switch (index)
            {
                case CONVERT_STRING:
                    return new TokenStringLiteral(word, lineNumber);
                case CONVERT_BUILDER:
                    return new TokenBuilderIdentifier(word, lineNumber);
                default:
                    return null;
            }
        }
    }
    /// <summary>
    /// Represents a selector that needs to be resolved within the active context, rather than statically.
    /// </summary>
    public sealed class TokenUnresolvedSelector : Token
    {
        public override string AsString() => $"{unresolvedSelector}";

        public UnresolvedSelector unresolvedSelector;
        public TokenUnresolvedSelector(UnresolvedSelector unresolvedSelector, int lineNumber) : base(lineNumber)
        {
            this.unresolvedSelector = unresolvedSelector;
        }

        public TokenSelectorLiteral Resolve(Executor executor)
        {
            return new TokenSelectorLiteral(unresolvedSelector.Resolve(executor), lineNumber);
        }
    }
    /// <summary>
    /// Represents a reference to what is probably a directive's builder field.
    /// </summary>
    public sealed class TokenBuilderIdentifier : TokenIdentifier
    {
        private string builderField;

        /// <summary>
        /// This builder field converted to full upper-case.
        /// </summary>
        public string BuilderField
        {
            get => builderField.ToUpper();
        }
        public TokenBuilderIdentifier(string fullWord, int lineNumber) : base(fullWord, lineNumber)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (fullWord.EndsWith(":")) // it should as long as its not implicitly converted from identifier
                builderField = fullWord.Substring(0, fullWord.Length - 1).Trim();
            else
                builderField = fullWord.Trim();
        }
    }
    /// <summary>
    /// Represents an enum constant defined by the compiler.
    /// </summary>
    public sealed class TokenIdentifierEnum : TokenIdentifier, IDocumented
    {
        public readonly Commands.ParsedEnumValue value;
        internal TokenIdentifierEnum() : base(null, -1) { }
        public TokenIdentifierEnum(string word, Commands.ParsedEnumValue value, int lineNumber) : base(word, lineNumber)
        {
            this.value = value;
        }
        public override string AsString() => value.value.ToString();

        public string GetDocumentation() => "Usually a specific keyword in a subset of possible keywords. This type is entirely context dependent.";
    }
    /// <summary>
    /// Represents a reference to a scoreboard value.
    /// </summary>
    public sealed class TokenIdentifierValue : TokenIdentifier, IIndexable, IDocumented
    {
        /// <summary>
        /// The value this identifier references.
        /// </summary>
        public readonly ScoreboardValue value;

        /// <summary>
        /// Get the full name used to access this value.
        /// </summary>
        public string Accessor => word;

        /// <summary>
        /// Shorthand for .value.clarifier.CurrentString();
        /// </summary>
        public string ClarifierStr => value.clarifier.CurrentString;

        [UsedImplicitly]
        private TokenIdentifierValue() : base(null, -1) {} // if you remove this method the markdown exporter will blow up because it uses Activator and needs this
        
        public TokenIdentifierValue(string word, ScoreboardValue value, int lineNumber) : base(word, lineNumber)
        {
            this.value = value;
        }

        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            if (value.HasAttribute<AttributeGlobal>() || value.clarifier.IsGlobal)
                throw new StatementException(forExceptions, "Cannot clarify a value that is defined as global.");
            
            switch (indexer)
            {
                case TokenIndexerString @string:
                {
                    ScoreboardValue clone = value.Clone(forExceptions);
                    string fakePlayer = @string.token.text;
                    clone.clarifier.SetString(fakePlayer);
                    return new TokenIdentifierValue(word, clone, lineNumber);
                }
                case TokenIndexerAsterisk _:
                {
                    ScoreboardValue clone = value.Clone(forExceptions);
                    clone.clarifier.SetString("*");
                    return new TokenIdentifierValue(word, clone, lineNumber);
                }
                case TokenIndexerSelector selector:
                {
                    ScoreboardValue clone = value.Clone(forExceptions);
                    clone.clarifier.SetSelector(selector.token.selector);
                    return new TokenIdentifierValue(word, clone, lineNumber);
                }
                default:
                    throw indexer.GetException(this, forExceptions);
            }
        }
        public string GetDocumentation() => "The name of a runtime value that was defined using the `define` command.";
    }

    public sealed class TokenIdentifierPreprocessor : TokenIdentifier, IIndexable, IDocumented
    {
        public readonly PreprocessorVariable variable;

        [UsedImplicitly]
        private TokenIdentifierPreprocessor() : base(null, -1) {} // if you remove this method the markdown exporter will blow up because it uses Activator and needs this

        public TokenIdentifierPreprocessor(string word, PreprocessorVariable variable, int lineNumber) : base(word,
            lineNumber)
        {
            this.variable = variable;
        }

        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            if (!(indexer is TokenIndexerInteger integer))
                throw indexer.GetException(this, forExceptions);

            int input = integer.token.number;
            int length = variable.Length;

            if (input < 0 || input >= length)
                throw integer.GetIndexOutOfBoundsException(0, length - 1, forExceptions);
            
            return variable[input];
        }
        public string GetDocumentation() => "The name of a preprocessor variable that was defined using the `$var`, `$json`, or other command.";
    }
    /// <summary>
    /// Represents a reference to a user-defined macro.
    /// </summary>
    public sealed class TokenIdentifierMacro : TokenIdentifier
    {
        /// <summary>
        /// The macro this identifier references.
        /// </summary>
        public readonly Macro macro;

        public TokenIdentifierMacro(Macro macro, int lineNumber) : base(macro.name, lineNumber)
        {
            this.macro = macro;
        }
    }
    /// <summary>
    /// Represents a reference to multiple functions that fell under a keyword.
    /// </summary>
    public sealed class TokenIdentifierFunction : TokenIdentifier
    {
        /// <summary>
        /// The function this identifier references.
        /// </summary>
        public readonly Function[] functions;

        public TokenIdentifierFunction(string keyword, Function[] functions, int lineNumber) : base(keyword, lineNumber)
        {
            this.functions = functions;
        }
    }


    /// <summary>
    /// Allows this object to return an object that can go into a PPV.
    /// </summary>
    public interface IPreprocessor
    {
        object GetValue();
    }
    /// <summary>
    /// Allows this object to be indexed through a set of values.
    /// </summary>
    public interface IIndexable
    {
        /// <summary>
        /// Index this object using an indexer.
        /// </summary>
        /// <param name="indexer">The indexer to use when accessing this object.</param>
        /// <param name="forExceptions">The statement to blame if something blows up.</param>
        /// <returns></returns>
        Token Index(TokenIndexer indexer, Statement forExceptions);
    }
}
