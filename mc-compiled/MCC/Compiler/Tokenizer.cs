using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using mc_compiled.Commands.Native;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;

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
        static readonly char[] BP_RP_IDENTIFIER_CHARS = "1234567890qwertyuiopasdfghjklzxcvbnm".ToCharArray();
        static readonly char[] LETTERS = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
        static readonly char[] IDENTIFIER_CHARS = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM#$_:.".ToCharArray();
        static readonly char[] ARITHMATIC_CHARS = "+-*/%".ToCharArray();
        public static bool IsWhiteSpace(char c) => c == ' ' | c == '\t';
        public static string StripForPack(string str)
        {
            return new string(str.ToLower().Where(c => BP_RP_IDENTIFIER_CHARS.Contains(c)).ToArray());
        }

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
        char Peek(int amount)
        {
            if (index + amount >= content.Length)
                return '\0';
            return content[index + amount];
        }
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
                case ',':
                case '\r':
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
                    case 'I':
                        NextChar();
                        while (HasNext && LETTERS.Contains(Peek()))
                            NextChar();
                        core = Selector.Core.initiator;
                        break;
                    default:
                        if(HasNext)
                            throw new TokenizerException("Invalid selector '" + secondChar + "'. Valid options: @p, @s, @a, @e, or @i/@initiator");
                        else
                            throw new TokenizerException("Invalid selector '(EOF)'. Valid options: @p, @s, @a, @e, or @i/@initiator");
                }
                if (HasNext && Peek() == '[')
                    return NextSelectorLiteral(core);
                else
                    return new TokenSelectorLiteral(core, CURRENT_LINE);
            }
            
            // comment
            if(firstChar == '/' && secondChar == '/')
            {
                NextChar();
                while (HasNext && Peek() != '\n')
                    sb.Append(NextChar());

                string str = sb.ToString().Trim();
                return new TokenComment(str, CURRENT_LINE);
            }
            /* multiline comment, just like this */
            if (firstChar == '/' && secondChar == '*')
            {
                int startLine = CURRENT_LINE;

                NextChar();
                while (HasNext)
                {
                    char next = NextChar();
                    if (next == '\n')
                        CURRENT_LINE++;
                    if (next == '*' && Peek() == '/')
                    {
                        NextChar();
                        break;
                    }
                    sb.Append(next);
                }

                string str = sb.ToString();
                return new TokenComment(str, startLine);
            }

            // equality/assignment
            if (firstChar == '=')
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

            // range operator
            if(firstChar == '.' && secondChar == '.')
            {
                NextChar();
                return new TokenRangeDots(CURRENT_LINE);
            }
            // range inverter (probably)
            if(firstChar == '!')
            {
                if(secondChar == '.' || char.IsDigit(secondChar))
                    return new TokenRangeInvert(CURRENT_LINE);
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
                if (char.IsDigit(secondChar) || (secondChar == '-' & char.IsDigit(Peek(1))))
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
                if (char.IsDigit(secondChar) || (secondChar == '-' & char.IsDigit(Peek(1))))
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

            switch (word)
            {
                case "true":
                case "True":
                case "TRUE":
                    return new TokenBooleanLiteral(true, CURRENT_LINE);
                case "false":
                case "False":
                case "FALSE":
                    return new TokenBooleanLiteral(false, CURRENT_LINE);
                case "and":
                case "And":
                case "AND":
                    return new TokenAnd(CURRENT_LINE);
                case "or":
                case "Or":
                case "OR":
                    return new TokenOr(CURRENT_LINE);
                case "not":
                case "Not":
                case "NOT":
                    return new TokenNot(CURRENT_LINE);

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
            if (CommandEnumParser.TryParse(word, out ParsedEnumValue enumValue))
                return new TokenIdentifierEnum(word, enumValue, CURRENT_LINE);

            // unresolved
            if (word.StartsWith("$"))
                return new TokenUnresolvedPPV(word, CURRENT_LINE);

            return new TokenIdentifier(word, CURRENT_LINE);
        }

        public TokenNumberLiteral NextNumberIdentifier(char first, bool rangeSecondArg = false)
        {
            sb.Append(first);
            IntMultiplier multiplier = IntMultiplier.none;

            char c;
            while (HasNext)
            {
                c = Peek();

                if (c == '.' && char.IsDigit(Peek(1)))
                {
                    sb.Append(NextChar());
                    continue;
                }

                if (!char.IsDigit(c))
                {
                    foreach (IntMultiplier im in TokenIntegerLiteral.ALL_MULTIPLIERS)
                    {
                        if (c == im.ToString()[0])
                        {
                            NextChar();
                            multiplier = im;
                            break;
                        }
                    }
                    break;
                }

                sb.Append(NextChar());
            }

            string str = sb.ToString();

            if (int.TryParse(str, out int i))
                return new TokenIntegerLiteral(i * (int)multiplier, multiplier, CURRENT_LINE);
            else
            {
                if (float.TryParse(str, out float f))
                {
                    f *= (int)multiplier;
                    int converted = (int)f;
                    if(f == (float)converted)
                        return new TokenIntegerLiteral(converted, multiplier, CURRENT_LINE);
                    return new TokenDecimalLiteral(f, CURRENT_LINE);
                }
                else
                    throw new TokenizerException("Couldn't parse literal: " + str);
            }
        }
        public TokenStringLiteral NextStringIdentifier()
        {
            bool escaped = false;
            sb.Clear();

            while (HasNext)
            {
                char c = NextChar();

                if(escaped)
                {
                    escaped = false;
                    sb.Append(c);
                    continue;
                }
                if(c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"')
                    break;

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
                
                if(escaped)
                {
                    escaped = false;
                    sb.Append(c);
                    continue;
                }
                if (c == '\\')
                    escaped = true;

                sb.Append(c);

                // bracket handling
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
        public TokenizerException(string message, int line) : base(message)
        {
            this.line = line;
        }
        public TokenizerException(string message) : base(message)
        {
            this.line = Tokenizer.CURRENT_LINE;
        }
    }
}
