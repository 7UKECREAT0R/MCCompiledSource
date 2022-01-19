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
        private static short nextIndex = 0;
        internal Directive(string identifier, string fullName)
        {
            index = nextIndex++;
            this.identifier = identifier;
            this.fullName = fullName;

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

        public override int GetHashCode() => identifier.GetHashCode();
    }
    public static class Directives
    {
        public static Directive[] REGISTRY =
        {
            new Directive("$var", "Set Preprocessor Variable"),
            new Directive("$inc", "Increment Preprocessor Variable"),
            new Directive("$dec", "Decrement Preprocessor Variable"),
            new Directive("$add", "Add to Preprocessor Variable"),
            new Directive("$sub", "Subtract from Preprocessor Variable"),
            new Directive("$mul", "Multiply with Preprocessor Variable"),
            new Directive("$div", "Divide Preprocessor Variable"),
            new Directive("$mod", "Modulo Preprocessor Variable"),
            new Directive("$if", "Preprocessor If"),
            new Directive("$else", "Preprocessor Else"),
            new Directive("$repeat", "Preprocessor Repeat"),
            new Directive("$log", "Preprocessor Log to Console"),
            new Directive("$macro", "Define/Call Preprocessor Macro"),
            new Directive("$include", "Include other File"),
            new Directive("$strfriendly", "Preprocessor String Friendly Name"),
            new Directive("$strupper", "Preprocessor String Uppercase"),
            new Directive("$strlower", "Preprocessor String Lowercase"),

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
