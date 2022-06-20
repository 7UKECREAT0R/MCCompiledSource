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
            { "udl2", new UDL2() },
            { "monarch", new Monarch() },
            { "raw", new RawSyntax() }
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

        static Keyword[] KeywordsFromDirectives(IEnumerable<Compiler.Directive> directives) =>
            directives.Select(directive => new Keyword(directive.identifier, directive.documentation)).ToArray();

        public static readonly Keywords operators = new Keywords()
        {
            KeywordsUndocumented = new[] { "<", ">", "{", "}", "=", "(", ")", "+", "-", "*", "/", "%", "!" },
            style = new Highlight(224, 193, 255, HighlightStyle.NONE)
        };
        public static readonly Keywords selectors = new Keywords()
        {
            keywords = new[] {
                new Keyword("@e", "References all entities in the world."),
                new Keyword("@a", "References all players in the world."),
                new Keyword("@s", "References the executing entity/player."),
                new Keyword("@p", "References the nearest player.")
            },
            style = new Highlight(255, 79, 79, HighlightStyle.BOLD)
        };
        public static readonly Keywords preprocessor = new Keywords()
        {
            keywords = KeywordsFromDirectives(MCC.Compiler.Directives.PreprocessorDirectives),
            style = new Highlight(11, 164, 221, HighlightStyle.NONE)
        };
        public static readonly Keywords commands = new Keywords()
        {
            keywords = KeywordsFromDirectives(MCC.Compiler.Directives.RegularDirectives),
            style = new Highlight(238, 91, 175, HighlightStyle.NONE)
        };
        public static readonly Keywords literals = new Keywords()
        {
            keywords = new Keyword[]
            {
                new Keyword("true", "A boolean value representing true/yes."),
                new Keyword("false", "A boolean value representing false/no."),
                new Keyword("&", "Adds on another comparison."),
                new Keyword("~", "Relative to executor's position."),
                new Keyword("^", "Relative to executor's direction.")
            },
            style = new Highlight(224, 193, 255, HighlightStyle.NONE)
        };
        public static readonly Keywords types = new Keywords()
        {
            keywords = new Keyword[]
            {
                new Keyword("int", "An integer, representing any whole value between -2147483648 to 2147483647."),
                new Keyword("decimal", "A decimal number with a pre-specified level of precision."),
                new Keyword("bool", "A true or false value. Displayed as whatever is set in the '_true' and '_false' preprocessor variables respectively."),
                new Keyword("time", "A value representing a number of ticks. Displayed as MM:SS."),
                new Keyword("struct", "A user-defined structure of multiple variables.")
            },
            style = new Highlight(255, 128, 128, HighlightStyle.NONE)
        };
        public static readonly Keywords comparisons = new Keywords()
        {
            keywords = new[] {
                new Keyword("block", "Check for a block being present in the world."),
                new Keyword("type", "Check for a specific entity type."),
                new Keyword("family", "Check for a specific entity family."),
                new Keyword("mode", "Check for the player(s) in a specific gamemode."),
                new Keyword("near", "Check for entities being near a certain position. Relative coordinates are relative to the executing entity."),
                new Keyword("inside", "Check for entities inside a rectangular prism. Relative coordinates are relative to the executing entity."),
                new Keyword("not", "Invert the following condition."),
                new Keyword("level", "Compare player(s) XP level."),
                new Keyword("name", "Check for entities with a specific name."),
                new Keyword("rotation x", "Compare entity X rotation."), 
                new Keyword("rotation y", "Compare entity Y rotation."),
                new Keyword("any", "Check if any entity is matched by a selector."),
                new Keyword("count", "Compare the number of entities that match a selector."),
                new Keyword("item", "Check for players holding or containing a specific item/number of items in their inventory."),
                new Keyword("holding", "Check for player(s) holding a specific item/nummber of items."),
                new Keyword("offset", "Offset the execution of the next condition."),
                new Keyword("null", "Check for entities which are nulls, optionally with a specific name."),
                new Keyword("class", "Check for entities which are nulls and are under a specific class."),
                new Keyword("position", "Check for entities at a specific x, y, z, position. Relative coordinates are relative to the executing entity."),
                new Keyword("position x", "Compare entity X position. Relative coordinates are relative to the executing entity."),
                new Keyword("position y", "Compare entity Y position. Relative coordinates are relative to the executing entity."),
                new Keyword("position z", "Compare entity Z position. Relative coordinates are relative to the executing entity.")
            },
            style = new Highlight(255, 95, 66, HighlightStyle.NONE)
        };
        public static readonly Keywords options = new Keywords()
        {
            keywords = new[] {
                new Keyword("nulls", "Feature: Create null entity behavior/resource files and allow them to be spawned in the world."),
                new Keyword("gametest", "Feature: Gametest Integration"),
                new Keyword("exploders", "Feature: Create exploder entity behavior/resource files and allow them to be created through the 'explode' command."),
                new Keyword("uninstall", "Feature: Create an uninstall function to undo all effects of this project."),
                new Keyword("identify", "Feature: Give each player a unique ID, allowing them to be identified by the 'id' variable (integer)."),
                new Keyword("up", "Used with the 'move' command. Goes up relative to where the entity is looking."),
                new Keyword("down", "Used with the 'move' command. Goes down relative to where the entity is looking."),
                new Keyword("left", "Used with the 'move' command. Goes left relative to where the entity is looking."),
                new Keyword("right", "Used with the 'move' command. Goes right relative to where the entity is looking."),
                new Keyword("forward", "Used with the 'move' command. Goes forward relative to where the entity is looking."),
                new Keyword("backward", "Used with the 'move' command. Goes backward relative to where the entity is looking."),
                new Keyword("survival", "Survival mode. (0)"),
                new Keyword("creative", "Creative mode. (1)"),
                new Keyword("adventure", "Adventure mode. (2)"),
                new Keyword("times", "Specifies the fade-in/stay/fade-out times this text will show for."),
                new Keyword("subtitle", "Sets the subtitle for the next title shown."),
                new Keyword("destroy", "Destroy any existing blocks as if broken by a player."),
                new Keyword("replace", "Replace any existing blocks. Default option."),
                new Keyword("hollow", "Hollow the area, only filling the outer edges with the block. To keep inside contents, use 'outline'."),
                new Keyword("outline", "Outline the area, only filling the outer edges with the block. To remove inside contents, use 'hollow'."),
                new Keyword("keep", "Keep any existing blocks, and only fill where air is present."),
                new Keyword("lockinventory", "Lock the item in the player's inventory."),
                new Keyword("lockslot", "Lock the item in the slot which it is placed in."),
                new Keyword("canplaceon:", "Specifies a block the item can be placed on."),
                new Keyword("candestroy:", "Specifies a block the item can destroy."),
                new Keyword("enchant:", "Give a leveled enchantment to this item. No limits."),
                new Keyword("name:", "Give the item a display name."),
                new Keyword("lore:", "Give the item a line of lore. Multiple of these can be used to add more lines."),
                new Keyword("author:", "If this item is a 'written_book', set the name of the author."),
                new Keyword("title:", "If this item is a 'written_book', set its title."),
                new Keyword("page:", "If this item is a 'written_book', add a page to it.  Multiple of these can be used to add more pages."),
                new Keyword("dye:", "If this item is a piece of leather armor, set its color to an RGB value.")
            },
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
        internal Keyword[] keywords;

        internal string[] KeywordsUndocumented
        {
            set
            {
                keywords = value.Select(v => new Keyword()
                {
                    documentation = null,
                    name = v
                }).ToArray();
            }
        }
    }
    internal struct Keyword
    {
        internal string name;
        internal string documentation;

        internal Keyword(string name, string docs)
        {
            this.name = name;
            this.documentation = docs;
        }
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
