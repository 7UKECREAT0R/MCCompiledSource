using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A directive that is the root of a statement.
    /// </summary>
    public class Directive
    {
        /// <summary>
        /// An implementation of a directive call.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="tokens"></param>
        public delegate void DirectiveImpl(Executor executor, Token[] tokens);


        private static short nextIndex = 0;
        internal Directive(DirectiveImpl call, string identifier,
            string fullName, params TypePattern[] patterns)
        {
            index = nextIndex++;
            this.call = call;
            this.identifier = identifier;
            this.fullName = fullName;
            this.patterns = patterns;
        }

        /// <summary>
        /// Get the key that should be used in a dictionary.
        /// </summary>
        public string DictValue
        {
            get
            {
                return identifier.ToUpper();
            }
        }

        public readonly short index;
        public readonly string identifier;
        public readonly string fullName;
        public readonly DirectiveImpl call;
        public readonly TypePattern[] patterns;

        public override int GetHashCode() => identifier.GetHashCode();
    }
    public static class Directives
    {
        public static Directive[] REGISTRY =
        {
            new Directive(DirectiveImplementations._var, "$var", "Set Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenLiteral))),
            new Directive(DirectiveImplementations._inc, "$inc", "Increment Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._dec, "$dec", "Decrement Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._add, "$add", "Add to Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenLiteral))),
            new Directive(DirectiveImplementations._sub, "$sub", "Subtract from Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenLiteral))),
            new Directive(DirectiveImplementations._mul, "$mul", "Multiply with Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenLiteral))),
            new Directive(DirectiveImplementations._div, "$div", "Divide Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenLiteral))),
            new Directive(DirectiveImplementations._mod, "$mod", "Modulo Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenLiteral))),
            new Directive(DirectiveImplementations._if, "$if", "Preprocessor If"),
            new Directive(DirectiveImplementations._else, "$else", "Preprocessor Else"),
            new Directive(DirectiveImplementations._repeat, "$repeat", "Preprocessor Repeat"),
            new Directive(DirectiveImplementations._log, "$log", "Preprocessor Log to Console"),
            new Directive(DirectiveImplementations._macro, "$macro", "Define/Call Preprocessor Macro"),
            new Directive(DirectiveImplementations._include, "$include", "Include other File"),
            new Directive(DirectiveImplementations._strfriendly, "$strfriendly", "Preprocessor String Friendly Name"),
            new Directive(DirectiveImplementations._strupper, "$strupper", "Preprocessor String Uppercase"),
            new Directive(DirectiveImplementations._strlower, "$strlower", "Preprocessor String Lowercase"),

            new Directive("mc", "Minecraft Command"),
            new Directive("select", "Select Target"),
            new Directive("print", "Print to All Chat"),
            new Directive("printp", "Print to Selected Entity"),
            new Directive("define", "Define Variable"),
            new Directive("init", "Initialize Variable to 0"),
            new Directive("if", "If Directive"),
            new Directive("else", "Else Directive"),
            new Directive("give", "Give Item to Selected"),
            new Directive("tp", "Teleport Selected Entity"),
            new Directive("face", "Face Selected Entity in Direction"),
            new Directive("place", "Place Block"),
            new Directive("fill", "Fill Region of Blocks"),
            new Directive("replace", "Replace Region of Blocks"),
            new Directive("kill", "Kill Selected Entity"),
            new Directive("title", "Show Title"),
            new Directive("halt", "Halt Execution"),
        };

        static readonly Dictionary<string, Directive> directiveLookup = new Dictionary<string, Directive>();
        static Directives()
        {
            foreach (Directive directive in REGISTRY)
                directiveLookup.Add(directive.DictValue, directive);
        }

        /// <summary>
        /// Query for a directive that matches this token contents. Case insensitive.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Directive Query(string token)
        {
            if (directiveLookup.TryGetValue(token.ToUpper(), out Directive directive))
                return directive;
            return null;
        }
    }
}
