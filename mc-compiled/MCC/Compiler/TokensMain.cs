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
    public class TokenIdentifier : Token
    {
        public readonly string word;

        public override string AsString() => word;
        public TokenIdentifier(string word, int lineNumber) : base(lineNumber)
        {
            this.word = word;
        }
    }
    /// <summary>
    /// Represents a likely preprocessor variable that needs to be resolved.
    /// </summary>
    public sealed class TokenUnresolvedPPV : TokenIdentifier
    {
        public TokenUnresolvedPPV(string word, int lineNumber) : base(word, lineNumber) { }
    }
    /// <summary>
    /// Represents a likely preprocessor variable that needs to be resolved.
    /// </summary>
    public sealed class TokenEnumIdentifier : TokenIdentifier
    {
        public readonly Enum @enum;

        public TokenEnumIdentifier(string word, Enum @enum, int lineNumber) : base(word, lineNumber)
        {
            this.@enum = @enum;
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
