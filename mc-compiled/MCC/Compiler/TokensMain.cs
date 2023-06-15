using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(index == CONVERT_STRING)
                return new TokenStringLiteral(word, lineNumber);
            else if(index == CONVERT_BUILDER)
                return new TokenBuilderIdentifier(word, lineNumber);

            return null;
        }
    }
    /// <summary>
    /// Represents a likely preprocessor variable that needs to be resolved.
    /// </summary>
    public sealed class TokenUnresolvedPPV : TokenIdentifier, IIndexable
    {
        public override string AsString() => $"{word}";

        public TokenUnresolvedPPV(string word, int lineNumber) : base(word, lineNumber) { }

        /// <summary>
        /// Index this unresolved PPV, essentially resolving it in a different way.
        /// The squasher internally detects when 1+ indexers proceed an unresolved PPV, ignoring it for this purpose.
        /// </summary>
        /// <param name="indexer">The indexer to resolve this PPV.</param>
        /// <param name="forExceptions">The statement to blame when everything goes wrong. Also holds the executor.</param>
        /// <returns></returns>
        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            Executor executor = forExceptions.executor;
            Token resolved = executor.ResolvePPVIndex(this, indexer, forExceptions);

            if (resolved == null)
                throw new StatementException(forExceptions, $"Couldn't index PPV '{word}' using indexer {indexer.AsString()}.");

            return resolved;
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
            if (fullWord.EndsWith(":")) // it should as long as its not implicitly converted from identifier
                builderField = fullWord.Substring(0, fullWord.Length - 1).Trim();
            else
                builderField = fullWord.Trim();
        }
    }
    /// <summary>
    /// Represents an enum constant defined by the compiler.
    /// </summary>
    public sealed class TokenIdentifierEnum : TokenIdentifier, IPreprocessor, IDocumented
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
        public string Accessor { get => word; }
        /// <summary>
        /// Shorthand for .value.clarifier.CurrentString();
        /// </summary>
        public string ClarifierStr { get => value.clarifier.CurrentString; }

        internal TokenIdentifierValue() : base(null, -1) { } 
        public TokenIdentifierValue(string word, ScoreboardValue value, int lineNumber) : base(word, lineNumber)
        {
            this.value = value;
        }

        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            if(indexer is TokenIndexerString @string)
            {
                ScoreboardValue clone = value.Clone() as ScoreboardValue;
                string fakePlayer = @string.token.text;
                clone.clarifier.SetString(fakePlayer, forExceptions);
                return new TokenIdentifierValue(word, clone, lineNumber);
            }

            if(indexer is TokenIndexerSelector selector)
            {
                ScoreboardValue clone = value.Clone() as ScoreboardValue;
                clone.clarifier.SetSelector(selector.token.selector, forExceptions);
                return new TokenIdentifierValue(word, clone, lineNumber);
            }

            throw indexer.GetException(this, forExceptions);
        }

        public string GetDocumentation() => "The name of a runtime value that was defined using the `define` command.";
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
