using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using mc_compiled.Commands.Native;
using mc_compiled.Commands;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Parses text into tokens.
    /// </summary>
    public class Tokenizer
    {
        public const string DEFS_FILE = "definitions.def";
        public readonly Definitions defs;
        public static int CURRENT_LINE = 1;

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
        static readonly char[] IDENTIFIER_CHARS = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM#$_:".ToCharArray();
        static readonly char[] ARITHMATIC_CHARS = "+-*/%".ToCharArray();
        static bool IsWhiteSpace(char c) => c == ' ' | c == '\t';

        readonly char[] content;
        readonly StringBuilder sb;
        private int index; // the index of the reader

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
            this.content = content
                .Replace("\r", "")
                .ToCharArray();
            sb = new StringBuilder();
        }

        /// <summary>
        /// Tokenize a file after reading it by path.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Token[] TokenizeFile(string file)
        {
            // Load file
            if (!File.Exists(file))
                throw new TokenizerException("File specified could not be found.");

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
            while (HasNext && IsWhiteSpace(Peek()))
                NextChar();
        }

        /// <summary>
        /// Tokenize the contents of this object.
        /// </summary>
        /// <returns></returns>
        public Token[] Tokenize()
        {
            CURRENT_LINE = 1;
            index = 0;

            List<Token> all = new List<Token>();

            Token identifier;
            bool lastWasNewline = false;

            while ((identifier = NextIdentifier()) != null)
            {
                if (identifier is TokenNewline)
                {
                    if (lastWasNewline)
                        continue;
                    lastWasNewline = true;
                }
                else
                    lastWasNewline = false;

                all.Add(identifier);
            }

            return all.ToArray();
        }
        /// <summary>
        /// Read the next valid identifier.
        /// </summary>
        /// <returns></returns>
        public Token NextIdentifier()
        {
            FlushWhitespace();
            sb.Clear();

            if (!HasNext)
                return null; // EOF

            char firstChar = NextChar();
            char secondChar = HasNext ? Peek() : '\0';

            switch(firstChar)
            {
                case '\n':
                    return new TokenNewline(CURRENT_LINE++);
                case '(':
                    return new TokenOpenParenthesis(CURRENT_LINE);
                case ')':
                    return new TokenCloseParenthesis(CURRENT_LINE);
                case '{':
                    return new TokenOpenBlock(CURRENT_LINE);
                case '}':
                    return new TokenCloseBlock(CURRENT_LINE);
                case '&':
                    return new TokenAnd(CURRENT_LINE);
                default:
                    break;
            }

            if(firstChar == '@')
            {
                Selector.Core core;

                switch (char.ToUpper(secondChar))
                {
                    case 'P':
                        NextChar();
                        core = Selector.Core.p;
                        break;
                    case 'S':
                        NextChar();
                        core = Selector.Core.s;
                        break;
                    case 'A':
                        NextChar();
                        core = Selector.Core.a;
                        break;
                    case 'E':
                        NextChar();
                        core = Selector.Core.e;
                        break;
                    default:
                        throw new TokenizerException("Invalid selector. '" +
                            secondChar + "'. Valid options: @p, @s, @a, or @e");
                }
                if (HasNext && Peek() == '[')
                    return NextSelectorLiteral(core);
                else
                    return new TokenSelectorLiteral(core, CURRENT_LINE);
            }
            
            // comment, read to EOL
            if(firstChar == '/' && secondChar == '/')
            {
                NextChar();
                while (HasNext && Peek() != '\n')
                    sb.Append(NextChar());

                string str = sb.ToString().Trim();
                return new TokenComment(str, CURRENT_LINE);
            }
            
            // equality/assignment
            if(firstChar == '=')
            {
                if(secondChar == '=')
                {
                    NextChar();
                    return new TokenEquality(CURRENT_LINE);
                }
                return new TokenAssignment(CURRENT_LINE);
            }

            // inequality
            if(firstChar == '!' && secondChar == '=')
            {
                NextChar();
                return new TokenInequality(CURRENT_LINE);
            }

            // check for number literal
            if (char.IsDigit(firstChar) || (firstChar == '-' && char.IsDigit(secondChar)))
                return NextNumberIdentifier(firstChar);

            // check for math token
            if (firstChar == '>' && secondChar == '<')
            {
                NextChar();
                return new TokenSwapAssignment(CURRENT_LINE);
            }
            else if (ARITHMATIC_CHARS.Contains(firstChar))
            {
                bool assignment = secondChar == '=';
                if (assignment) NextChar();
                return ArithmaticIdentifier(firstChar, assignment);
            }

            // comparison
            bool lessThan;
            if((lessThan = firstChar == '<') || firstChar == '>')
            {
                bool orEqual = secondChar == '=';
                if (orEqual) NextChar();
                return CompareIdentifier(lessThan, orEqual);
            }

            // coordinate literals
            if(firstChar == '~')
            {
                if (char.IsDigit(secondChar) || secondChar == '-')
                {
                    NextChar();
                    TokenNumberLiteral number = NextNumberIdentifier(secondChar);
                    string str = '~' + number.AsString();
                    return new TokenCoordinateLiteral(Coord.Parse(str).Value, CURRENT_LINE);
                }
                return new TokenCoordinateLiteral(new Coord(0, false, true, false), CURRENT_LINE);
            }
            if (firstChar == '^')
            {
                if (char.IsDigit(secondChar) || secondChar == '-')
                {
                    NextChar();
                    TokenNumberLiteral number = NextNumberIdentifier(secondChar);
                    string str = '^' + number.AsString();
                    return new TokenCoordinateLiteral(Coord.Parse(str).Value, CURRENT_LINE);
                }
                return new TokenCoordinateLiteral(new Coord(0, false, false, true), CURRENT_LINE);
            }

            // check for string literal
            if (firstChar == '"')
            {
                // empty string
                if(secondChar == '"')
                {
                    NextChar();
                    return new TokenStringLiteral("", CURRENT_LINE);
                }
                return NextStringIdentifier();
            }

            // not any hardcoded known symbols.
            // just read a full word.
            sb.Append(firstChar);
            while (HasNext)
            {
                char next = Peek();
                if (IDENTIFIER_CHARS.Contains(next))
                    sb.Append(NextChar());
                else
                    break;
            }

            string word = sb.ToString();
            string wordCI = word.ToUpper();

            switch (wordCI)
            {
                case "TRUE":
                    return new TokenBooleanLiteral(true, CURRENT_LINE);
                case "FALSE":
                    return new TokenBooleanLiteral(false, CURRENT_LINE);
                default:
                    break;
            }

            // check for probable builder field
            if (word.EndsWith(":"))
                return new TokenBuilderIdentifier(word, CURRENT_LINE);

            // check for directive
            Directive directive = Directives.Query(word);
            if (directive != null)
                return new TokenDirective(directive, CURRENT_LINE);

            // check for enum constant
            if (CommandEnumParser.TryParse(word, out object enumValue))
                return new TokenIdentifierEnum(word, enumValue, CURRENT_LINE);

            // unresolved
            if (word.StartsWith("$"))
                return new TokenUnresolvedPPV(word, CURRENT_LINE);

            return new TokenIdentifier(word, CURRENT_LINE);
        }

        public TokenNumberLiteral NextNumberIdentifier(char first)
        {
            sb.Append(first);
            bool isFloat = false;

            char c;
            while (HasNext)
            {
                c = Peek();

                if (c == '.')
                {
                    isFloat = true;
                    sb.Append(NextChar());
                    continue;
                }

                if (!char.IsDigit(c))
                    break;

                sb.Append(NextChar());
            }

            string str = sb.ToString();
            if(isFloat)
            {
                if (float.TryParse(str, out float f))
                    return new TokenDecimalLiteral(f, CURRENT_LINE);
                else
                    throw new TokenizerException("Couldn't parse decimal literal: " + str);
            } else
            {
                if (int.TryParse(str, out int i))
                    return new TokenIntegerLiteral(i, CURRENT_LINE);
                else
                    throw new TokenizerException("Couldn't parse integer literal: " + str);
            }
        }
        public TokenStringLiteral NextStringIdentifier()
        {
            bool escaped = false;

            while (HasNext)
            {
                char c = NextChar();

                if (c == '\\')
                    escaped = !escaped;
                else if (c == '"' && !escaped)
                    break;
                else if (escaped)
                    escaped = false;

                sb.Append(c);
            }

            return new TokenStringLiteral(sb.ToString(), CURRENT_LINE);
        }
        public TokenSelectorLiteral NextSelectorLiteral(Selector.Core core)
        {
            if (Peek() == '[')
                NextChar();

            int level = 0;
            bool escaped = false;
            bool inQuotes = false;

            while (HasNext)
            {
                char c = NextChar();
                sb.Append(c);

                if (c == '\\')
                    escaped = !escaped;

                if(!escaped)
                {
                    if (!inQuotes)
                    {
                        if (c == '[')
                            level++;
                        if (c == ']')
                        {
                            level--;
                            if (level < 0)
                                break;
                        }
                    }
                    else if (c == '"')
                        inQuotes = !inQuotes;
                }
                else if (c != '\\')
                    escaped = false;
            }

            string str = sb.ToString();
            Selector selector = Selector.Parse(core, str);
            return new TokenSelectorLiteral(selector, CURRENT_LINE);
        }
        public TokenArithmatic ArithmaticIdentifier(char a, bool assignment)
        {
            switch (a)
            {
                case '+':
                    return assignment ?
                        (TokenArithmatic)new TokenAddAssignment(CURRENT_LINE) :
                        (TokenArithmatic)new TokenAdd(CURRENT_LINE);
                case '-':
                    return assignment ?
                        (TokenArithmatic)new TokenSubtractAssignment(CURRENT_LINE) :
                        (TokenArithmatic)new TokenSubtract(CURRENT_LINE);
                case '*':
                    return assignment ?
                        (TokenArithmatic)new TokenMultiplyAssignment(CURRENT_LINE) :
                        (TokenArithmatic)new TokenMultiply(CURRENT_LINE);
                case '/':
                    return assignment ?
                        (TokenArithmatic)new TokenDivideAssignment(CURRENT_LINE) :
                        (TokenArithmatic)new TokenDivide(CURRENT_LINE);
                case '%':
                    return assignment ?
                        (TokenArithmatic)new TokenModuloAssignment(CURRENT_LINE) :
                        (TokenArithmatic)new TokenModulo(CURRENT_LINE);
                default:
                    throw new TokenizerException("Couldn't parse identifier?");
            }
        }
        public TokenCompare CompareIdentifier(bool lessThan, bool orEqual)
        {
            if (lessThan)
            {
                return orEqual ?
                    (TokenCompare)new TokenLessThanEqual(CURRENT_LINE) :
                    (TokenCompare)new TokenLessThan(CURRENT_LINE);
            }
            else
            {
                return orEqual ?
                    (TokenCompare)new TokenGreaterThanEqual(CURRENT_LINE) :
                    (TokenCompare)new TokenGreaterThan(CURRENT_LINE);
            }
        }
    }
    
    /// <summary>
    /// Indicates that the tokenizer has encountered an issue with parsing.
    /// </summary>
    public class TokenizerException : Exception
    {
        public int line;
        public TokenizerException(string message) : base(message)
        {
            line = Tokenizer.CURRENT_LINE;
        }
    }
}
