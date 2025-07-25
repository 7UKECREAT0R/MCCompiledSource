using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Language;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Parses text into tokens.
/// </summary>
public class Tokenizer
{
    public static string CURRENT_FILE;
    public static int CURRENT_LINE = 1;
    private static readonly char[] TOKENIZER_IGNORE_CHARS = [' ', '\t', ','];
    private static readonly char[] BP_RP_IDENTIFIER_CHARS = "1234567890qwertyuiopasdfghjklzxcvbnm".ToCharArray();
    private static readonly char[] LETTERS = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
    private static readonly char[] IDENTIFIER_CHARS =
        "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM#$_:.".ToCharArray();
    private static readonly char[] ARITHMETIC_CHARS = "+-*/%".ToCharArray();

    private readonly char[] content;
    private readonly StringBuilder sb;
    private int index; // the index of the reader

    /// <summary>
    ///     Create a new Tokenizer, optionally running cleanups and the definitions.def parser over it.
    /// </summary>
    /// <param name="content">The content to tokenize.</param>
    /// <param name="stripCarriageReturns">Whether to strip carriage return characters from the input.</param>
    /// <param name="useDefinitions">Whether to check and replace definitions.def references in the content.</param>
    public Tokenizer(string content, bool stripCarriageReturns = true, bool useDefinitions = true)
    {
        if (useDefinitions)
        {
            Definitions defs = Definitions.GLOBAL_DEFS;
            content = defs.ReplaceDefinitions(content);
        }

        if (stripCarriageReturns)
            this.content = content
                .Replace("\r", "")
                .ToCharArray();
        else
            this.content = content
                .ToCharArray();

        this.sb = new StringBuilder();
    }

    private bool HasNext => this.index < this.content.Length;
    private static bool IsIgnored(char c)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        // LINQ and closures here can cause a bit of performance loss and loads of stuff allocated on the SOH
        foreach (char tokenizerIgnoreChar in TOKENIZER_IGNORE_CHARS)
            if (tokenizerIgnoreChar == c)
                return true;

        return false;
    }

    /// <summary>
    ///     Tokenize a file after reading it by path.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static Token[] TokenizeFile(string file)
    {
        // Load file
        if (!File.Exists(file))
            throw new TokenizerException("File specified could not be found.");

        string content = File.ReadAllText(file);
        return new Tokenizer(content).Tokenize( /* only passed for errors: */ file);
    }
    private char Peek() { return this.content[this.index]; }
    private char Peek(int amount)
    {
        if (this.index + amount >= this.content.Length)
            return '\0';
        return this.content[this.index + amount];
    }
    private char NextChar() { return this.content[this.index++]; }
    private void FlushIgnoredCharacters()
    {
        while (this.HasNext && IsIgnored(Peek()))
            NextChar();
    }

    /// <summary>
    ///     Tokenize the contents of this object.
    /// </summary>
    /// <param name="fileNameForErrors">The file name that will be embedded into errors, if any.</param>
    /// <returns>The tokenized result.</returns>
    public Token[] Tokenize(string fileNameForErrors)
    {
        CURRENT_FILE = fileNameForErrors;
        CURRENT_LINE = 1;
        this.index = 0;

        var all = new List<Token>();

        Token token;
        bool lastWasNewline = false;

        while ((token = NextToken()) != null)
        {
            switch (token)
            {
                case TokenComment _:
                {
                    // strip all comments except ones at the start of the line
                    if (lastWasNewline || all.Count == 0)
                        all.Add(token);
                    break;
                }
                case TokenIdentifier id:
                {
                    // split the deref token; it wasn't a directive
                    string word = id.word;
                    if (word[0] == '$')
                    {
                        if (word.Length == 1)
                        {
                            token = new TokenDeref(CURRENT_LINE);
                        }
                        else
                        {
                            all.Add(new TokenDeref(CURRENT_LINE));
                            token = new TokenIdentifier(word[1..], CURRENT_LINE);
                        }
                    }

                    break;
                }
            }

            if (token is TokenNewline)
            {
                if (lastWasNewline)
                    continue;
                lastWasNewline = true;
            }
            else
            {
                lastWasNewline = false;
            }

            all.Add(token);
        }

        return all.ToArray();
    }
    /// <summary>
    ///     Read the next valid identifier.
    /// </summary>
    private Token NextToken()
    {
        FlushIgnoredCharacters();
        this.sb.Clear();

        if (!this.HasNext)
            return null; // EOF

        char firstChar = NextChar();
        char secondChar = this.HasNext ? Peek() : '\0';

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
                    case 'R':
                        NextChar();
                        core = Selector.Core.r;
                        break;
                    case 'I':
                        throw new TokenizerException(
                            "Selector @i (@initiator) is unsupported. @s refers to the initating player inside dialogue.");
                    default:
                        if (this.HasNext)
                            throw new TokenizerException("Invalid selector '" + secondChar +
                                                         "'. Valid options: @p, @s, @a, @e, or @r");
                        throw new TokenizerException(
                            "Invalid selector '(end-of-file)'. Valid options: @p, @s, @a, @e, or @r");
                }

                if (this.HasNext && Peek() == '[')
                    return NextSelectorLiteral(core);

                return new TokenSelectorLiteral(core, CURRENT_LINE);
            }
            // comment
            case '/' when secondChar == '/':
            {
                NextChar();
                while (this.HasNext && Peek() != '\n')
                    this.sb.Append(NextChar());

                string str = this.sb.ToString().Trim();
                return new TokenComment(str, CURRENT_LINE);
            }
            /* multiline comment, just like this */
            case '/' when secondChar == '*':
            {
                int startLine = CURRENT_LINE;

                NextChar();
                while (this.HasNext)
                {
                    char next = NextChar();
                    if (next == '\n')
                        CURRENT_LINE++;
                    if (next == '*' && this.HasNext && Peek() == '/')
                    {
                        NextChar(); // skip the '/'
                        break;
                    }

                    this.sb.Append(next);
                }

                string str = this.sb.ToString();
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

        if (ARITHMETIC_CHARS.Contains(firstChar))
        {
            bool assignment = secondChar == '=';
            if (assignment)
                NextChar();
            return ArithmeticIdentifier(firstChar, assignment);
        }

        // comparison
        bool lessThan;
        if ((lessThan = firstChar == '<') || firstChar == '>')
        {
            bool orEqual = secondChar == '=';
            if (orEqual)
                NextChar();
            return CompareIdentifier(lessThan, orEqual);
        }

        switch (firstChar)
        {
            // coordinate literals
            case '~' when char.IsDigit(secondChar) || (secondChar == '-') & char.IsDigit(Peek(1)):
            {
                NextChar();
                TokenNumberLiteral number = NextNumberIdentifier(secondChar);
                string str = '~' + number.AsString();
                return new TokenCoordinateLiteral(Coordinate.Parse(str) ?? Coordinate.zero, CURRENT_LINE);
            }
            case '~':
                return new TokenCoordinateLiteral(new Coordinate(0, false, true, false), CURRENT_LINE);
            case '^' when char.IsDigit(secondChar) || (secondChar == '-') & char.IsDigit(Peek(1)):
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
        this.sb.Append(firstChar);
        while (this.HasNext)
        {
            char next = Peek();
            if (IDENTIFIER_CHARS.Contains(next))
                this.sb.Append(NextChar());
            else
                break;
        }

        string word = this.sb.ToString();

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
        Directive directive = Language.Language.QueryDirective(word);
        if (directive != null)
            return new TokenDirective(directive, CURRENT_LINE);

        // check for enum constant
        if (CommandEnumParser.TryParse(word, out RecognizedEnumValue enumValue))
            return new TokenIdentifierEnum(word, enumValue, CURRENT_LINE);

        return new TokenIdentifier(word, CURRENT_LINE);
    }
    private TokenNumberLiteral NextNumberIdentifier(char first)
    {
        bool inDecimalPart = false;
        this.sb.Append(first);
        var multiplier = IntMultiplier.none;

        while (this.HasNext)
        {
            char c = Peek();

            if (c == '.' && char.IsDigit(Peek(1)))
            {
                if (inDecimalPart)
                    break;
                this.sb.Append(NextChar());
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

            this.sb.Append(NextChar());
        }

        string str = this.sb.ToString();

        if (int.TryParse(str, out int i))
            return new TokenIntegerLiteral(i * (int) multiplier, multiplier, CURRENT_LINE);
        if (!decimal.TryParse(str, out decimal d))
            throw new TokenizerException("Couldn't parse literal: " + str);

        d *= (int) multiplier;

        // ReSharper disable once InvertIf
        if (multiplier != IntMultiplier.none)
        {
            // if it's not an integer after being multiplied, then throw a special exception for this case.
            int rounded;

            if (d == (int) d)
                rounded = (int) d;
            else
                rounded = (int) Math.Round(d);

            // throw new TokenizerException("Time suffix did not multiply evenly to ticks.", new[] {CURRENT_LINE});
            return new TokenIntegerLiteral(rounded, multiplier, CURRENT_LINE);
        }

        return new TokenDecimalLiteral(d, CURRENT_LINE);
    }
    /// <summary>
    ///     Returns the next string literal, ending with the given closing character. Backslashes are only omitted
    ///     when one's used to escape the closer character.
    /// </summary>
    /// <param name="closer"></param>
    /// <returns></returns>
    private TokenStringLiteral NextStringIdentifier(char closer)
    {
        this.sb.Clear();

        while (this.HasNext)
        {
            char c = NextChar();

            if (c == '\\')
            {
                char next = Peek();
                if (next == closer)
                {
                    next = NextChar();
                    this.sb.Append(next);
                    continue;
                }

                this.sb.Append(c);
                continue;
            }

            if (c == closer)
                break;

            this.sb.Append(c);
        }

        return new TokenStringLiteral(this.sb.ToString(), CURRENT_LINE);
    }
    private Token NextSelectorLiteral(Selector.Core core)
    {
        if (Peek() == '[')
            NextChar();

        int level = 0;
        bool escaped = false;
        bool inQuotes = false;

        while (this.HasNext)
        {
            char c = NextChar();

            if (escaped)
            {
                escaped = false;
                this.sb.Append(c);
                continue;
            }

            if (c == '\\')
                escaped = true;
            else
                this.sb.Append(c);

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

        string str = this.sb.ToString();

        if (str.Contains("$")) // selector probably contains preprocessor stuff. use the non-optimized version.
            return new TokenUnresolvedSelector(new UnresolvedSelector(core, str), CURRENT_LINE);

        Selector selector = Selector.Parse(core, str);
        return new TokenSelectorLiteral(selector, CURRENT_LINE);
    }

    [PublicAPI]
    public static TokenCompare CompareIdentifier(bool lessThan, bool orEqual)
    {
        if (lessThan)
            return orEqual ? new TokenLessThanEqual(CURRENT_LINE) : new TokenLessThan(CURRENT_LINE);

        return orEqual ? new TokenGreaterThanEqual(CURRENT_LINE) : new TokenGreaterThan(CURRENT_LINE);
    }
    [PublicAPI]
    public static TokenArithmetic ArithmeticIdentifier(char a, bool assignment)
    {
        return a switch
        {
            '+' => assignment ? new TokenAddAssignment(CURRENT_LINE) : new TokenAdd(CURRENT_LINE),
            '-' => assignment
                ? new TokenSubtractAssignment(CURRENT_LINE)
                : new TokenSubtract(CURRENT_LINE),
            '*' => assignment
                ? new TokenMultiplyAssignment(CURRENT_LINE)
                : new TokenMultiply(CURRENT_LINE),
            '/' => assignment
                ? new TokenDivideAssignment(CURRENT_LINE)
                : new TokenDivide(CURRENT_LINE),
            '%' => assignment
                ? new TokenModuloAssignment(CURRENT_LINE)
                : new TokenModulo(CURRENT_LINE),
            _ => throw new TokenizerException("Couldn't parse identifier?")
        };
    }
}

/// <summary>
///     Indicates that the tokenizer has encountered an issue with parsing.
/// </summary>
public class TokenizerException : Exception
{
    public readonly string file;
    public readonly int[] lines;
    public TokenizerException(string message, int[] lines) : base(message)
    {
        this.lines = lines;
        this.file = Tokenizer.CURRENT_FILE;
    }
    public TokenizerException(string message) : base(message)
    {
        this.lines = [Tokenizer.CURRENT_LINE];
        this.file = Tokenizer.CURRENT_FILE;
    }
}