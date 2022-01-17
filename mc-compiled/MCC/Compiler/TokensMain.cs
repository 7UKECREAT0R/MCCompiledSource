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
    /// Represents a token which doesn't have any identifiable tokenization-time category.
    /// This shouldn't be ignored, since it could be a PPV or variable name, it just needs to be resolved at compile time.
    /// </summary>
    public sealed class TokenUnresolved : Token
    {
        public readonly string word;

        public override string AsString() => word;
        public TokenUnresolved(string word, int lineNumber) : base(lineNumber)
        {
            this.word = word;
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
}
