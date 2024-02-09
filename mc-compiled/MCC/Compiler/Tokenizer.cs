using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Parses text into tokens.
    /// </summary>
    public class Tokenizer
    {
        public static int CURRENT_LINE = 1;
        private static readonly char[] TOKENIZER_IGNORE_CHARS = { ' ', '\t', ',' };
        private static readonly char[] BP_RP_IDENTIFIER_CHARS = "1234567890qwertyuiopasdfghjklzxcvbnm".ToCharArray();
        private static readonly char[] LETTERS = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
        private static readonly char[] IDENTIFIER_CHARS = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM#$_:.".ToCharArray();
        private static readonly char[] ARITHMATIC_CHARS = "+-*/%".ToCharArray();
        private static bool IsIgnored(char c) => TOKENIZER_IGNORE_CHARS.Any(test => test == c);

        private readonly char[] content;
        private readonly StringBuilder sb;
        private int index; // the index of the reader

        /// <summary>
        /// Create a new Tokenizer, optionally running cleanups and the definitions.def parser over it.
        /// </summary>
        /// <param name="content">The content to tokenize.</param>
        /// <param name="stripCarriageReturns">Whether or not to strip carriage return characters from the input.</param>
        /// <param name="useDefinitions">Whether or not to check and replace definitions.def references in the content.</param>
        public Tokenizer(string content, bool stripCarriageReturns = true, bool useDefinitions = true)
        {
            if (useDefinitions)
            {
                Definitions defs = Definitions.GLOBAL_DEFS;
                content = defs.ReplaceDefinitions(content);
            }

            if (stripCarriageReturns)
            {
                this.content = content
                    .Replace("\r", "")
                    .ToCharArray();
            } else
            {
                this.content = content
                    .ToCharArray();
            }

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

        private bool HasNext
        {
            get => index < content.Length;
        }
        private char Peek() => content[index];
        private char Peek(int amount)
        {
            if (index + amount >= content.Length)
                return '\0';
            return content[index + amount];
        }
        private char NextChar() => content[index++];
        private void FlushIgnoredCharacters()
        {
            while (HasNext && IsIgnored(Peek()))
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

            var all = new List<Token>();

            Token token;
            bool lastWasNewline = false;

            while ((token = NextToken()) != null)
            {
                if (token is TokenIdentifier id)
                {
                    // split the deref token, it wasn't a directive
                    string word = id.word;
                    if (word[0] == '$')
                    {
                        if(word.Length == 1)
                            token = new TokenDeref(CURRENT_LINE);
                        else
                        {
                            all.Add(new TokenDeref(CURRENT_LINE));
                            token = new TokenIdentifier(word.Substring(1), CURRENT_LINE);
                        }
                    }
                }
                
                if (token is TokenNewline)
                {
                    if (lastWasNewline)
                        continue;
                    lastWasNewline = true;
                }
                else
                    lastWasNewline = false;

                all.Add(token);
            }

            return all.ToArray();
        }
        /// <summary>
        /// Read the next valid identifier.
        /// </summary>
        /// <returns></returns>
        private Token NextToken()
        {
            FlushIgnoredCharacters();
            sb.Clear();

            if (!HasNext)
                return null; // EOF

            char firstChar = NextChar();
            char secondChar = HasNext ? Peek() : '\0';

            switch (firstChar)
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
                case '[':
                    return new TokenOpenIndexer(CURRENT_LINE);
                case ']':
                    return new TokenCloseIndexer(CURRENT_LINE);
            }

            switch (firstChar)
            {
                case '@':
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
                            throw new TokenizerException("Invalid selector '(end-of-file)'. Valid options: @p, @s, @a, @e, or @i/@initiator");
                    }
                    if (HasNext && Peek() == '[')
                        return NextSelectorLiteral(core);
                    
                    return new TokenSelectorLiteral(core, CURRENT_LINE);
                }
                // comment
                case '/' when secondChar == '/':
                {
                    NextChar();
                    while (HasNext && Peek() != '\n')
                        sb.Append(NextChar());

                    string str = sb.ToString().Trim();
                    return new TokenComment(str, CURRENT_LINE);
                }
                /* multiline comment, just like this */
                case '/' when secondChar == '*':
                {
                    int startLine = CURRENT_LINE;

                    NextChar();
                    while (HasNext)
                    {
                        char next = NextChar();
                        if (next == '\n')
                            CURRENT_LINE++;
                        if (next == '*' && HasNext && Peek() == '/')
                        {
                            NextChar(); // skip the '/'
                            break;
                        }
                        sb.Append(next);
                    }

                    string str = sb.ToString();
                    return new TokenComment(str, startLine);
                }
                // equality/assignment
                case '=' when secondChar == '=':
                    NextChar();
                    return new TokenEquality(CURRENT_LINE);
                case '=':
                    return new TokenAssignment(CURRENT_LINE);
                // inequality
                case '!' when secondChar == '=':
                    NextChar();
                    return new TokenInequality(CURRENT_LINE);
                // range operator
                case '.' when secondChar == '.':
                    NextChar();
                    return new TokenRangeDots(CURRENT_LINE);
                // range inverter (probably)
                case '!' when secondChar == '.' || char.IsDigit(secondChar):
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

            if (ARITHMATIC_CHARS.Contains(firstChar))
            {
                bool assignment = secondChar == '=';
                if (assignment) NextChar();
                return ArithmeticIdentifier(firstChar, assignment);
            }

            // comparison
            bool lessThan;
            if((lessThan = firstChar == '<') || firstChar == '>')
            {
                bool orEqual = secondChar == '=';
                if (orEqual) NextChar();
                return CompareIdentifier(lessThan, orEqual);
            }

            switch (firstChar)
            {
                // coordinate literals
                case '~' when char.IsDigit(secondChar) || (secondChar == '-' & char.IsDigit(Peek(1))):
                {
                    NextChar();
                    TokenNumberLiteral number = NextNumberIdentifier(secondChar);
                    string str = '~' + number.AsString();
                    return new TokenCoordinateLiteral(Coordinate.Parse(str) ?? Coordinate.zero, CURRENT_LINE);
                }
                case '~':
                    return new TokenCoordinateLiteral(new Coordinate(0, false, true, false), CURRENT_LINE);
                case '^' when char.IsDigit(secondChar) || (secondChar == '-' & char.IsDigit(Peek(1))):
                {
                    NextChar();
                    TokenNumberLiteral number = NextNumberIdentifier(secondChar);
                    string str = '^' + number.AsString();
                    return new TokenCoordinateLiteral(Coordinate.Parse(str) ?? Coordinate.zero, CURRENT_LINE);
                }
                case '^':
                    return new TokenCoordinateLiteral(new Coordinate(0, false, false, true), CURRENT_LINE);
                // check for string literal
                case '"':
                case '\'':
                {
                    if (secondChar != firstChar)
                        return NextStringIdentifier(firstChar);
                    // empty string
                    NextChar();
                    return new TokenStringLiteral("", CURRENT_LINE);
                }
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

            switch (word.ToUpper())
            {
                case "TRUE":
                    return new TokenBooleanLiteral(true, CURRENT_LINE);
                case "FALSE":
                    return new TokenBooleanLiteral(false, CURRENT_LINE);
                case "NULL":
                    return new TokenNullLiteral(CURRENT_LINE);
                case "AND":
                    return new TokenAnd(CURRENT_LINE);
                case "OR":
                    return new TokenOr(CURRENT_LINE);
                case "NOT":
                    return new TokenNot(CURRENT_LINE);
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
            
            return new TokenIdentifier(word, CURRENT_LINE);
        }
        private TokenNumberLiteral NextNumberIdentifier(char first)
        {
            bool inDecimalPart = false;
            sb.Append(first);
            var multiplier = IntMultiplier.none;

            while (HasNext)
            {
                char c = Peek();

                if (c == '.' && char.IsDigit(Peek(1)))
                {
                    if (inDecimalPart)
                        break;
                    sb.Append(NextChar());
                    inDecimalPart = true;
                    continue;
                }

                if (!char.IsDigit(c))
                {
                    foreach (IntMultiplier im in TokenIntegerLiteral.ALL_MULTIPLIERS)
                    {
                        if (c != im.ToString()[0])
                            continue;
                        
                        NextChar();
                        multiplier = im;
                        break;
                    }
                    break;
                }

                sb.Append(NextChar());
            }

            string str = sb.ToString();

            if (int.TryParse(str, out int i))
                return new TokenIntegerLiteral(i * (int)multiplier, multiplier, CURRENT_LINE);
            if (!decimal.TryParse(str, out decimal d))
                throw new TokenizerException("Couldn't parse literal: " + str);
            
            d *= (int)multiplier;
            return new TokenDecimalLiteral(d, CURRENT_LINE);
        }
        /// <summary>
        /// Returns the next string literal, ending with the given closing character. Backslashes are only omitted
        /// when one's used to escape the closer character.
        /// </summary>
        /// <param name="closer"></param>
        /// <returns></returns>
        private TokenStringLiteral NextStringIdentifier(char closer)
        {
            sb.Clear();

            while (HasNext)
            {
                char c = NextChar();
                
                if(c == '\\')
                {
                    char next = Peek();
                    if (next == closer)
                    {
                        next = NextChar();
                        sb.Append(next);
                        continue;
                    }
                    sb.Append(c);
                    continue;
                }

                if (c == closer)
                    break;

                sb.Append(c);
            }

            return new TokenStringLiteral(sb.ToString(), CURRENT_LINE);
        }
        private Token NextSelectorLiteral(Selector.Core core)
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
                else
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
                if (c == '"')
                    inQuotes = !inQuotes;
            }

            string str = sb.ToString();

            if (str.Contains("$")) // selector probably contains preprocessor stuff. use the non-optimized version.
            {
                return new TokenUnresolvedSelector(new UnresolvedSelector(core, str), CURRENT_LINE);
            }

            Selector selector = Selector.Parse(core, str);
            return new TokenSelectorLiteral(selector, CURRENT_LINE);
        }
        
        [PublicAPI]
        public static TokenCompare CompareIdentifier(bool lessThan, bool orEqual)
        {
            if (lessThan)
            {
                return orEqual ?
                    new TokenLessThanEqual(CURRENT_LINE) :
                    (TokenCompare)new TokenLessThan(CURRENT_LINE);
            }

            return orEqual ?
                new TokenGreaterThanEqual(CURRENT_LINE) :
                (TokenCompare)new TokenGreaterThan(CURRENT_LINE);
        }
        [PublicAPI]
        public static TokenArithmetic ArithmeticIdentifier(char a, bool assignment)
        {
            switch (a)
            {
                case '+':
                    return assignment ?
                        new TokenAddAssignment(CURRENT_LINE) :
                        (TokenArithmetic)new TokenAdd(CURRENT_LINE);
                case '-':
                    return assignment ?
                        new TokenSubtractAssignment(CURRENT_LINE) :
                        (TokenArithmetic)new TokenSubtract(CURRENT_LINE);
                case '*':
                    return assignment ?
                        new TokenMultiplyAssignment(CURRENT_LINE) :
                        (TokenArithmetic)new TokenMultiply(CURRENT_LINE);
                case '/':
                    return assignment ?
                        new TokenDivideAssignment(CURRENT_LINE) :
                        (TokenArithmetic)new TokenDivide(CURRENT_LINE);
                case '%':
                    return assignment ?
                        new TokenModuloAssignment(CURRENT_LINE) :
                        (TokenArithmetic)new TokenModulo(CURRENT_LINE);
                default:
                    throw new TokenizerException("Couldn't parse identifier?");
            }
        }
    }
    
    /// <summary>
    /// Indicates that the tokenizer has encountered an issue with parsing.
    /// </summary>
    public class TokenizerException : Exception
    {
        public readonly int[] lines;
        public TokenizerException(string message, int[] lines) : base(message)
        {
            this.lines = lines;
        }
        public TokenizerException(string message) : base(message)
        {
            this.lines = new[] { Tokenizer.CURRENT_LINE };
        }
    }
}
