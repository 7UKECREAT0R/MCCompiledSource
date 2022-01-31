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
    public sealed class TokenNewline : Token
    {
        public override string AsString() => "\n";
        public TokenNewline(int lineNumber) : base(lineNumber) {}
    }
    /// <summary>
    /// Represents a directive call.
    /// </summary>
    public sealed class TokenDirective : Token
    {
        public readonly Directive directive;

        public override string AsString() => directive.identifier;
        public TokenDirective(Directive directive, int lineNumber) : base(lineNumber)
        {
            this.directive = directive;
        }
    }
    /// <summary>
    /// Represents a comment that was made using two slashes.
    /// </summary>
    public sealed class TokenComment : Token
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
    public class TokenIdentifier : Token, IObjectable, IImplicitToken
    {
        public readonly string word;

        public override string AsString() => word;
        public TokenIdentifier(string word, int lineNumber) : base(lineNumber)
        {
            this.word = word;
        }
        public object GetObject() => word;

        public Type GetImplicitType() =>
            typeof(TokenStringLiteral);
        public Token Convert() =>
            new TokenStringLiteral(word, lineNumber);
    }
    /// <summary>
    /// Represents a likely preprocessor variable that needs to be resolved.
    /// </summary>
    public sealed class TokenUnresolvedPPV : TokenIdentifier
    {
        public TokenUnresolvedPPV(string word, int lineNumber) : base(word, lineNumber) { }
    }
    /// <summary>
    /// Represents a reference to what is probably a directive's builder field.
    /// </summary>
    public sealed class TokenBuilderIdentifier : TokenIdentifier
    {
        public readonly string builderField;
        public TokenBuilderIdentifier(string fullWord, int lineNumber) : base(fullWord, lineNumber)
        {
            if(fullWord.EndsWith(":")) // it should 100% of the time
                builderField = fullWord.Substring(0, fullWord.Length - 1);
        }
    }
    /// <summary>
    /// Represents a likely preprocessor variable that needs to be resolved.
    /// </summary>
    public sealed class TokenIdentifierEnum : TokenIdentifier, IObjectable
    {
        public readonly object value;
        public TokenIdentifierEnum(string word, object value, int lineNumber) : base(word, lineNumber)
        {
            this.value = value;
        }
    }
    /// <summary>
    /// Represents a reference to a scoreboard value.
    /// </summary>
    public sealed class TokenIdentifierValue : TokenIdentifier
    {
        /// <summary>
        /// The value this identifier references.
        /// </summary>
        public readonly ScoreboardValue value;

        /// <summary>
        /// Get the full name used to access this value.
        /// </summary>
        public string Accessor { get => word; }

        public TokenIdentifierValue(string accessor, ScoreboardValue value, int lineNumber) : base(accessor, lineNumber)
        {
            this.value = value;
        }
    }
    /// <summary>
    /// Represents a reference to a user-defined struct.
    /// </summary>
    public sealed class TokenIdentifierStruct : TokenIdentifier
    {
        /// <summary>
        /// The value this identifier references.
        /// </summary>
        public readonly StructDefinition @struct;

        /// <summary>
        /// Get the full name used to access this value.
        /// </summary>
        public string Accessor { get => word; }

        public TokenIdentifierStruct(string word, StructDefinition @struct, int lineNumber) : base(word, lineNumber)
        {
            this.@struct = @struct;
        }
    }

    /// <summary>
    /// Allows this object to return an object that can go into a PPV.
    /// </summary>
    public interface IObjectable
    {
        object GetObject();
    }
}
