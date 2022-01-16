using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using mc_compiled.Commands.Native;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Compile and tokenize a script.
    /// </summary>
    public class Tokenizer
    {
        public const string DEFS_FILE = "definitions.def";
        public readonly Definitions defs;
        public static int CURRENT_LINE = 0;

        public static Dictionary<string, TokenDefinition> keywordLookup = new Dictionary<string, TokenDefinition>();
        public static List<string> guessedValues = new List<string>();
        public static List<string> guessedPPValues = new List<string>();

        struct TokenCharCategory
        {
            readonly string name;
            readonly char[] chars;
            
            public TokenCharCategory(string name, string chars)
            {
                this.name = name;
                this.chars = chars.ToCharArray();
            }
        }
        /// <summary>
        /// Characters which form a variable name, directive name, or other. Does not include string literals.
        /// </summary>
        static TokenCharCategory[] characterCategories = new TokenCharCategory[]
        {
            new TokenCharCategory("numbers", "-0123456789.0"),
            new TokenCharCategory("identifier", "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM#$_1234567890"),
            new TokenCharCategory("operators", "=+-*/%()")
        };
        static readonly char[] IDENTIFIER_CHARS = 
            "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM#$_".ToCharArray();
        static readonly char[] NUMBER_CHARS =
            "-.0123456789".ToCharArray();

        readonly char[] content;
        readonly StringBuilder sb;

        private int index;          // the index of the reader
        private bool stringLiteral; // if inside string literal
        private bool escape;        // if escaping using backslash

        /// <summary>
        /// Splits by space but preserves arguments encapsulated with quotation marks.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] GetArguments(string input)
        {
            return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>().Select(match => match.Value).ToArray();
        }

        public Tokenizer(string content)
        {
            defs = Definitions.GLOBAL_DEFS;
            content = defs.ReplaceDefinitions(content);
            this.content = content.ToCharArray();
            sb = new StringBuilder();
            index = 0;
        }

        public static Token[] TokenizeFile(string file)
        {
            // Load file
            if (!File.Exists(file))
                throw new FileNotFoundException("File specified could not be found.");

            string content = File.ReadAllText(file);
            return new Tokenizer(content).Tokenize();
        }

        bool HasNext
        {
            get => index < content.Length;
        }
        char Peek() => content[index];
        char NextChar() => content[index++];
        void FlushWhitespace()
        {
            while (HasNext && char.IsWhiteSpace(Peek()))
                NextChar();
        }


        public Token[] Tokenize()
        {
            List<Token> all = new List<Token>();

            string identifier;
            while((identifier = NextIdentifier()) != null)
            {

            }

            return all.ToArray();
        }
        /// <summary>
        /// Get the next identifier
        /// </summary>
        /// <returns></returns>
        public Token NextIdentifier()
        {
            if (!HasNext)
                return null;

            FlushWhitespace();
            sb.Clear();

            char c = NextChar();
            char c2 = HasNext ? Peek() : '\0';

            // check for number literal
            if (int.TryParse(c.ToString(), out _))
                return NextNumberIdentifier(c);
            else if(c == '-' && int.TryParse())

        }
        public Token NextNumberIdentifier(char first)
        {
            if (!HasNext)
                return null;

            FlushWhitespace();
            sb.Clear();

            char c = NextChar();
            char c2 = HasNext ? Peek() : '\0';

            if (int.Parse(c.ToString()))

                return ParseToken(sb.ToString());
        }
    }
}
