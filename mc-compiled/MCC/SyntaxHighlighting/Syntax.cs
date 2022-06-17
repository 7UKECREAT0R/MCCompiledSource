using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.SyntaxHighlighting
{
    /// <summary>
    /// Global definitions for syntax highlighters.
    /// </summary>
    internal static class Syntax
    {
        static Syntax() { }

        public const string EXTENSION = "mcc";
        public const bool IGNORE_CASE = true;
        public const bool COMMENT_FOLDING = true;
        public const bool COMPACT_FOLDING = false;
        public const string NUMBER_RANGE = "..";

        internal static readonly Dictionary<string, SyntaxTarget> syntaxTargets = new Dictionary<string, SyntaxTarget>()
        {
            { "udl2", new UDL2() }
        };

        public const string bracketOpen = "[";
        public const string bracketClose = "]";
        public const string blockOpen = "{";
        public const string blockClose = "}";
        public const string stringDelimiter = "\"";
        public const string escape = "\\";
        public const string lineComment = "//";
        public const string multilineOpen = "/*";
        public const string multilineClose = "*/";
        
        public static readonly string[] numberPrefixes = new string[] { "~", "^", "!", ".." };
        public static readonly string[] numberSuffixes = new string[] { "h", "m", "s", "t" };

        public static readonly Highlight commentColor = new Highlight(62, 140, 66, HighlightStyle.NONE);
        public static readonly Highlight numberColor = new Highlight(224, 193, 255, HighlightStyle.NONE);
        public static readonly Highlight stringColor = new Highlight(221, 179, 255, HighlightStyle.NONE);
        public static readonly Highlight selectorColor = new Highlight(192, 192, 192, HighlightStyle.NONE);

        public static readonly Keywords operators = new Keywords()
        {
            keywords = new[] { "<", ">", "{", "}", "=", "(", ")", "+", "-", "*", "/", "%", "!" },
            style = new Highlight(224, 193, 255, HighlightStyle.NONE)
        };
        public static readonly Keywords selectors = new Keywords()
        {
            keywords = new[] { "@e", "@a", "@s", "@p" },
            style = new Highlight(255, 79, 79, HighlightStyle.BOLD)
        };
        public static readonly Keywords preprocessor = new Keywords()
        {
            keywords = MCC.Compiler.Directives.PreprocessorKeywords.ToArray(),
            style = new Highlight(11, 164, 221, HighlightStyle.NONE)
        };
        public static readonly Keywords commands = new Keywords()
        {
            keywords = MCC.Compiler.Directives.RegularKeywords.ToArray(),
            style = new Highlight(238, 91, 175, HighlightStyle.NONE)
        };
        public static readonly Keywords literals = new Keywords()
        {
            keywords = new[] { "true", "false", "&", "~", "^" },
            style = new Highlight(224, 193, 255, HighlightStyle.NONE)
        };
        public static readonly Keywords types = new Keywords()
        {
            keywords = new[] { "int", "decimal", "bool", "time", "struct", "function", "$macro" },
            style = new Highlight(255, 128, 128, HighlightStyle.NONE)
        };
        public static readonly Keywords comparisons = new Keywords()
        {
            keywords = new[] { "block", "type", "family", "mode", "near", "inside", "not", "level", "name", "rotation x", "rotation y", "any", "count", "item", "holding", "offset", "null", "class", "position", "position x", "position y", "position z" },
            style = new Highlight(255, 95, 66, HighlightStyle.NONE)
        };
        public static readonly Keywords options = new Keywords()
        {
            keywords = new[] { "nulls", "gametest", "exploders", "uninstall", "identify", "up", "down", "left", "right", "forward", "backward", "survival", "creative", "adventure", "times", "subtitle", "destroy", "replace", "hollow", "outline", "keep", "lockinventory", "lockslot", "canplaceon:", "candestroy:", "enchant:", "name:", "lore:", "author:", "title:", "page:", "dye:" },
            style = new Highlight(215, 174, 255, HighlightStyle.NONE)
        };

    }
    internal interface SyntaxTarget
    {
        void Write(System.IO.TextWriter writer);
        string Describe();
        string GetFile();
    }

    internal struct Comments
    {
        internal readonly Highlight style;
        internal readonly string lineComment;
        internal readonly string multilineOpen;
        internal readonly string multilineClose;

        public Comments(string lineComment, string multilineOpen, string multilineClose, Highlight style)
        {
            this.lineComment = lineComment;
            this.multilineOpen = multilineOpen;
            this.multilineClose = multilineClose;
            this.style = style;
        }
    }
    internal struct Keywords
    {
        internal Highlight style;
        internal string[] keywords;
    }

    [Flags]
    internal enum HighlightStyle : int
    {
        NONE = 0,
        BOLD = 1 << 0,
        ITALIC = 1 << 1,
        UNDERLINE = 1 << 2,
        STRIKE = 1 << 3
    }
    internal struct Highlight
    {
        public readonly int r, g, b;
        public readonly HighlightStyle style;

        public string HexWithoutHash
        {
            get => string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
        }
        public string HexWithHash
        {
            get => string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }


        internal Highlight(int r, int g, int b, HighlightStyle style)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.style = style;
        }
    }
}
