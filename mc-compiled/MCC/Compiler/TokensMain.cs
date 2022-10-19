using mc_compiled.Commands.Selectors;
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
    public sealed class TokenUnresolvedPPV : TokenIdentifier
    {
        public TokenUnresolvedPPV(string word, int lineNumber) : base(word, lineNumber) { }
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
    public sealed class TokenIdentifierEnum : TokenIdentifier, IPreprocessor
    {
        public readonly Commands.ParsedEnumValue value;
        public TokenIdentifierEnum(string word, Commands.ParsedEnumValue value, int lineNumber) : base(word, lineNumber)
        {
            this.value = value;
        }
        public override string AsString() => value.value.ToString();
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
        /// The owning entity this identifier references. Defaults to @s.
        /// </summary>
        public Selector references;

        /// <summary>
        /// Get the full name used to access this value.
        /// </summary>
        public string Accessor { get => word; }
        /// <summary>
        /// Shorthand for .references.ToString();
        /// </summary>
        public string RefStr { get => references.ToString(); }

        public TokenIdentifierValue(string accessor, ScoreboardValue value, int lineNumber) : base(accessor, lineNumber)
        {
            this.value = value;
            this.references = new Selector(Selector.Core.s);
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
    /// Represents a reference to a function.
    /// </summary>
    public sealed class TokenIdentifierFunction : TokenIdentifier
    {
        /// <summary>
        /// The function this identifier references.
        /// </summary>
        public readonly Function function;

        public TokenIdentifierFunction(Function function, int lineNumber) : base(function.name, lineNumber)
        {
            this.function = function;
        }
    }

    /// <summary>
    /// Allows this object to return an object that can go into a PPV.
    /// </summary>
    public interface IPreprocessor
    {
        object GetValue();
    }
}
