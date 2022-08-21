using mc_compiled.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A fully qualified statement which can be run.
    /// </summary>
    public abstract class Statement : ICloneable
    {
        private TypePattern[] patterns;
        internal Executor executor;
        public Statement(Token[] tokens, bool waitForPatterns = false)
        {
            this.tokens = tokens;
            if (!waitForPatterns)
                patterns = GetValidPatterns();
            DecorateInSource = true;
        }
        /// <summary>
        /// Returns if this statement type is a directive and it has this attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public abstract bool HasAttribute(DirectiveAttribute attribute);

        /// <summary>
        /// Set the line of source this statement relates to. Used in "errors."
        /// </summary>
        /// <param name="line"></param>
        /// <param name="code"></param>
        public void SetSource(int line, string code)
        {
            Line = line;
            Source = code;
        }
        /// <summary>
        /// Set the executor of this statement.
        /// </summary>
        /// <param name="executor"></param>
        public void SetExecutor(Executor executor)
        {
            this.executor = executor;
        }

        public int Line
        {
            get; private set;
        }
        public bool DecorateInSource
        {
            get; protected set;
        }
        public string Source
        {
            get; private set;
        }

        protected Token[] tokens;
        protected int currentToken;

        public bool HasNext
        {
            get => currentToken < tokens.Length;
        }
        public Token Next()
        {
            if (currentToken >= tokens.Length)
                throw new StatementException(this, $"Token expected at end of line.");
            return tokens[currentToken++];
        }
        public Token Peek()
        {
            if (currentToken >= tokens.Length)
                throw new StatementException(this, $"Token expected at end of line.");
            return tokens[currentToken];
        }
        public T Next<T>() where T : class
        {
            if (currentToken >= tokens.Length)
                throw new StatementException(this, $"Token expected at end of line, type {typeof(T).Name}");

            Token token = tokens[currentToken++];
            if (!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type[] otherTypes = implicitToken.GetImplicitTypes();

                    for(int i = 0; i < otherTypes.Length; i++)
                        if(typeof(T).IsAssignableFrom(otherTypes[i]))
                            return implicitToken.Convert(executor, i) as T;    
                }
                throw new StatementException(this, $"Invalid token type. Expected {typeof(T).Name} but got {token.GetType().Name}");
            }
            else
                return token as T;
        }
        public T Peek<T>() where T : class
        {
            if (currentToken >= tokens.Length)
                throw new StatementException(this, $"Token expected at end of line, type {typeof(T).Name}");
            Token token = tokens[currentToken];
            if(!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type[] otherTypes = implicitToken.GetImplicitTypes();

                    for (int i = 0; i < otherTypes.Length; i++)
                        if (typeof(T).IsAssignableFrom(otherTypes[i]))
                            return implicitToken.Convert(executor, i) as T;
                }
                throw new StatementException(this, $"Invalid token type. Expected {typeof(T)} but got {token.GetType()}");
            } else
                return token as T;
        }
        public bool NextIs<T>()
        {
            if (!HasNext)
                return false;

            Token token = tokens[currentToken];

            if (token is T)
                return true;

            if(token is IImplicitToken)
            {
                IImplicitToken implicitToken = token as IImplicitToken;
                Type[] otherTypes = implicitToken.GetImplicitTypes();

                for (int i = 0; i < otherTypes.Length; i++)
                    if (typeof(T).IsAssignableFrom(otherTypes[i]))
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Return the remaining tokens in this statement.
        /// </summary>
        /// <returns></returns>
        public Token[] GetRemainingTokens()
        {
            Token[] ret = new Token[tokens.Length - currentToken];
            for (int i = currentToken; i < tokens.Length; i++)
                ret[i] = tokens[i];
            return ret;
        }
        protected abstract TypePattern[] GetValidPatterns();
        /// <summary>
        /// Run this statement/continue where it left off.
        /// </summary>
        protected abstract void Run(Executor executor);

        /// <summary>
        /// Run this statement from square one.
        /// </summary>
        public void Run0(Executor executor)
        {
            if (patterns != null && patterns.Length > 0)
            {
                IEnumerable<MatchResult> results = patterns.Select(pattern => pattern.Check(tokens));

                if(results.All(result => !result.match))
                {
                    // get the closest matched pattern
                    MatchResult closest = results.Aggregate((a, b) => a.accuracy > b.accuracy ? a : b);
                    var missingArgs = closest.missing.Select(m => m.ToString());
                    throw new StatementException(this, "Missing argument(s): " + string.Join(", ", missingArgs));
                }
            }

            currentToken = 0;
            Run(executor);
        }
        /// <summary>
        /// Clone this statement and resolve its unidentified tokens based off the current executor's state.
        /// After this is finished, squash and process any intermediate math or functional operations.<br />
        /// <br />This function pushes the Scoreboard temp state once.
        /// </summary>
        /// <returns>A shallow clone of this Statement which has its tokens resolved.</returns>
        public Statement ClonePrepare(Executor executor)
        {
            // decorator
            if (Program.DECORATE && DecorateInSource)
            {
                if(Source != null)
                    executor.CurrentFile.Add("# " + Source);
            }

            Statement statement = MemberwiseClone() as Statement;
            
            // e.g. close/open block
            if (statement.tokens == null)
            {
                executor.scoreboard.PushTempState();
                return statement;
            }

            bool resolvePPVs = !HasAttribute(DirectiveAttribute.DONT_EXPAND_PPV);
            int length = statement.tokens.Length;
            List<Token> allUnresolved = new List<Token>(statement.tokens);
            List<Token> allResolved = new List<Token>();

            // now resolve tokens forward
            for(int i = 0; i < allUnresolved.Count; i++)
            {
                Token unresolved = allUnresolved[i];
                int line = unresolved.lineNumber;

                if (unresolved is TokenStringLiteral)
                    allResolved.Add(new TokenStringLiteral(executor.ResolveString(unresolved as TokenStringLiteral), line));
                else if (resolvePPVs && unresolved is TokenUnresolvedPPV)
                    allResolved.AddRange(executor.ResolvePPV(unresolved as TokenUnresolvedPPV) ?? new Token[] { unresolved });
                else if (unresolved is TokenIdentifier)
                {
                    TokenIdentifier identifier = unresolved as TokenIdentifier;
                    string word = identifier.word;
                    if (executor.scoreboard.TryGetByAccessor(word, out ScoreboardValue value, true))
                        allResolved.Add(new TokenIdentifierValue(word, value, line));
                    else if (executor.scoreboard.TryGetStruct(word, out StructDefinition @struct))
                        allResolved.Add(new TokenIdentifierStruct(word, @struct, line));
                    else if (executor.TryLookupMacro(word, out Macro? macro))
                        allResolved.Add(new TokenIdentifierMacro(macro.Value, line));
                    else if (executor.TryLookupFunction(word, out Function function))
                        allResolved.Add(new TokenIdentifierFunction(function, line));
                    else
                        allResolved.Add(unresolved);
                }
                else if (unresolved is TokenOpenParenthesis)
                {
                    (unresolved as TokenOpenParenthesis).hasBeenSquashed = false;
                    allResolved.Add(unresolved);
                }
                else
                    allResolved.Add(unresolved);
            }

            executor.scoreboard.PushTempState(); // popped at call site
            SquashAll(allResolved, executor);
            SquashSpecial(allResolved); // ranges, etc..

            statement.tokens = allResolved.ToArray();
            statement.patterns = statement.GetValidPatterns();
            return statement;
        }
        public void SquashSpecial(List<Token> tokens)
        {
            // going over it backwards for merging any particular tokens
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                Token Previous(int amount)
                {
                    if (i - amount < 0)
                        return null;
                    return tokens[i - amount];
                };
                Token After(int amount)
                {
                    if (i + amount >= tokens.Count)
                        return null;
                    return tokens[i + amount];
                };

                Token token = tokens[i];

                // ridiculously complex range stuff
                if (token is TokenRangeDots)
                {
                    TokenRangeLiteral replacement;
                    int replacementLocation;
                    int replacementLength;

                    Token back2 = Previous(2);
                    Token back1 = Previous(1);
                    Token next1 = After(1);

                    if (back1 is TokenIntegerLiteral)
                    {
                        int? numberMax;
                        if (next1 is TokenIntegerLiteral)
                            numberMax = next1 as TokenIntegerLiteral;
                        else
                            numberMax = null;

                        replacementLocation = -1;
                        replacementLength = numberMax.HasValue ? 3 : 2;
                        int numberMin = back1 as TokenIntegerLiteral;
                        Range range = new Range(numberMin, numberMax, false);
                        replacement = new TokenRangeLiteral(range, token.lineNumber);
                    }
                    else
                    {
                        if (!(next1 is TokenIntegerLiteral))
                            throw new TokenizerException("Range argument only accepts integers.");
                        replacementLocation = 0;
                        replacementLength = 2;
                        int number = next1 as TokenIntegerLiteral;
                        Range range = new Range(null, number, false);
                        replacement = new TokenRangeLiteral(range, token.lineNumber);
                    }

                    i += replacementLocation;
                    if (Previous(1) is TokenRangeInvert)
                    {
                        i--;
                        replacementLength += 1;
                        replacement.range.invert = true;
                    }

                    tokens.RemoveRange(i, replacementLength);
                    tokens.Insert(i, replacement);
                }
                if (token is TokenRangeInvert)
                {
                    Token after = After(1);
                    if (!(after is TokenIntegerLiteral))
                        throw new TokenizerException("You can only invert integers.");
                    tokens.RemoveRange(i, 2);
                    int number = after as TokenIntegerLiteral;
                    Range range = new Range(number, true);
                    tokens.Insert(i, new TokenRangeLiteral(range, token.lineNumber));
                }
            }
        }
        public void SquashAll(List<Token> tokens, Executor executor)
        {
            // recursively call parenthesis first
            for(int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                if (!(token is TokenOpenParenthesis parenthesis))
                    continue;
                else if (parenthesis.hasBeenSquashed)
                    continue;

                int level = 1;
                List<Token> toSquash = new List<Token>();
                for(int x = i + 1; x < tokens.Count; x++)
                {
                    token = tokens[x];
                    if (token is TokenOpenParenthesis)
                        level++;
                    else if(token is TokenCloseParenthesis)
                    {
                        level--;
                        if (level < 1)
                            goto properlyClosed;
                    }
                    toSquash.Add(token);
                }
                throw new StatementException(this, "Unexpected end-of-line inside parenthesis.");

            properlyClosed:
                int startIndex = i;
                int removeLength = toSquash.Count;

                // check if this is a function call.
                bool isFunction = this is StatementFunctionCall && i == 1;
                if(!isFunction && i > 0)
                    isFunction |= tokens[i - 1] is TokenIdentifierFunction;

                // only remove parentheses if they're used for grouping
                if (isFunction)
                    startIndex += 1;
                else
                    removeLength += 2;

                // inside parentheses
                SquashAll(toSquash, executor);
                tokens.RemoveRange(startIndex, removeLength);
                tokens.InsertRange(startIndex, toSquash);
                parenthesis.hasBeenSquashed = true;
                i = -1; // reset back to the start
            }

            // root of the statement
            SquashFunctions(ref tokens, executor);
            Squash<TokenArithmaticFirst>(ref tokens, executor);
            Squash<TokenArithmaticSecond>(ref tokens, executor);
        }
        public void Squash<T>(ref List<Token> tokens, Executor executor)
        {
            for (int i = 1; i < (tokens.Count() - 1); i++)
            {
                Token selected = tokens[i];
                if (!(selected is T))
                    continue;
                if (selected is IAssignment)
                    continue; // dont squash assignments

                // this can be assumed due to how squash is meant to be called
                TokenArithmatic.Type op = (selected as TokenArithmatic).GetArithmaticType();
                List<string> commands = new List<string>();
                Token squashedToken = null;
                string selector = executor.ActiveSelectorStr;

                Token _left = tokens[i - 1];
                Token _right = tokens[i + 1];

                bool leftIsLiteral = _left is TokenLiteral;
                bool rightIsLiteral = _right is TokenLiteral;
                bool leftIsValue = _left is TokenIdentifierValue;
                bool rightIsValue = _right is TokenIdentifierValue;

                if (leftIsLiteral & rightIsLiteral)
                {
                    TokenLiteral left = _left as TokenLiteral;
                    TokenLiteral right = _right as TokenLiteral;

                    switch (op)
                    {
                        case TokenArithmatic.Type.ADD:
                            squashedToken = left.AddWithOther(right);
                            break;
                        case TokenArithmatic.Type.SUBTRACT:
                            squashedToken = left.SubWithOther(right);
                            break;
                        case TokenArithmatic.Type.MULTIPLY:
                            squashedToken = left.MulWithOther(right);
                            break;
                        case TokenArithmatic.Type.DIVIDE:
                            squashedToken = left.DivWithOther(right);
                            break;
                        case TokenArithmatic.Type.MODULO:
                            squashedToken = left.ModWithOther(right);
                            break;
                        default:
                            break;
                    }
                }
                else if (leftIsValue & rightIsValue)
                {
                    TokenIdentifierValue left = _left as TokenIdentifierValue;
                    TokenIdentifierValue right = _right as TokenIdentifierValue;
                    string leftAccessor = left.Accessor;
                    string rightAccessor = right.Accessor;
                    ScoreboardValue a = left.value;
                    ScoreboardValue b = right.value;

                    ScoreboardValue temp = executor.scoreboard.RequestTemp(a);
                    string accessorTemp = temp.Name;
                    if (temp is ScoreboardValueStruct && left.word.Contains(':'))
                    {
                        StructDefinition structure = (temp as ScoreboardValueStruct).structure;
                        string field = left.word.Split(':')[1];
                        accessorTemp = structure.GetAccessor(accessorTemp, field);
                    }

                    commands.AddRange(temp.CommandsSet(selector, a, accessorTemp, right.word));
                    squashedToken = new TokenIdentifierValue(accessorTemp, temp, selected.lineNumber);

                    switch (op)
                    {
                        case TokenArithmatic.Type.ADD:
                            commands.AddRange(temp.CommandsAdd(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.SUBTRACT:
                            commands.AddRange(temp.CommandsSub(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.MULTIPLY:
                            commands.AddRange(temp.CommandsMul(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.DIVIDE:
                            commands.AddRange(temp.CommandsDiv(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.MODULO:
                            commands.AddRange(temp.CommandsMod(selector, b, accessorTemp, right.word));
                            break;
                        default:
                            break;
                    }
                }
                else if (leftIsValue | rightIsValue && leftIsLiteral | rightIsLiteral)
                {
                    string aAccessor, bAccessor;
                    ScoreboardValue a, b;
                    if (leftIsLiteral)
                    {
                        a = executor.scoreboard.RequestTemp(_left as TokenLiteral, this);
                        aAccessor = a.Name;
                        commands.AddRange(a.CommandsSetLiteral(a.Name, selector, _left as TokenLiteral));
                        b = (_right as TokenIdentifierValue).value;
                        bAccessor = (_right as TokenIdentifierValue).Accessor;
                    }
                    else
                    {
                        b = executor.scoreboard.RequestTemp(_right as TokenLiteral, this);
                        bAccessor = b.Name;
                        commands.AddRange(b.CommandsSetLiteral(b.Name, selector, _right as TokenLiteral));

                        // left is a value, so it needs to be put into a temp variable so that the source is not modified
                        TokenIdentifierValue left = _left as TokenIdentifierValue;
                        a = executor.scoreboard.RequestTemp(left.value);
                        commands.AddRange(a.CommandsSet(selector, left.value, a.Name, left.Accessor));
                        aAccessor = a.Name;
                    }

                    squashedToken = new TokenIdentifierValue(aAccessor, a, selected.lineNumber);

                    switch (op)
                    {
                        case TokenArithmatic.Type.ADD:
                            commands.AddRange(a.CommandsAdd(selector, b, aAccessor, bAccessor));
                            break;
                        case TokenArithmatic.Type.SUBTRACT:
                            commands.AddRange(a.CommandsSub(selector, b, aAccessor, bAccessor));
                            break;
                        case TokenArithmatic.Type.MULTIPLY:
                            commands.AddRange(a.CommandsMul(selector, b, aAccessor, bAccessor));
                            break;
                        case TokenArithmatic.Type.DIVIDE:
                            commands.AddRange(a.CommandsDiv(selector, b, aAccessor, bAccessor));
                            break;
                        case TokenArithmatic.Type.MODULO:
                            commands.AddRange(a.CommandsMod(selector, b, aAccessor, bAccessor));
                            break;
                        default:
                            break;
                    }
                }
                else
                    throw new StatementException(this, $"No valid data given in tokens '{_left}' and '{_right}'; was there a misspelling?");

                executor.AddCommandsClean(commands, "operation");

                // replace those three tokens with the one squashed one
                tokens.RemoveRange(i - 1, 3);
                tokens.Insert(i - 1, squashedToken);

                // restart order-of-operations
                i = -1;
            }
        }
        public void SquashFunctions(ref List<Token> tokens, Executor executor)
        {
            int startAt = 0;

            // ignore first function call since thats part of the statement
            if (this is StatementFunctionCall)
                startAt = 2;

            for(int i = startAt; i < (tokens.Count() - 2); i++)
            {
                Token selected = tokens[i];
                Token second = tokens[i + 1];
                Token third = tokens[i + 2];

                if (!(selected is TokenIdentifierFunction))
                    continue;
                if (!(second is TokenOpenParenthesis))
                    continue; // might just be regular identifier

                int x = i + 2;
                TokenIdentifierFunction func = selected as TokenIdentifierFunction;
                Function function = func.function;

                // if its not parameterless() then fetch until level <= 0
                List<Token> tokensInside = new List<Token>();
                if (!(third is TokenCloseParenthesis))
                {
                    for(int z = x; z < tokens.Count; z++)
                    {
                        Token token = tokens[z];
                        if (token is TokenCloseParenthesis)
                            break;
                        tokensInside.Add(tokens[z]);
                    }
                }

                if (tokensInside.Count < function.ParameterCount)
                    throw new StatementException(this, $"Missing parameters for function {function}");
                if(function.returnValue == null)
                    throw new StatementException(this, $"Function does not have a return value.");

                // call function
                string sel = executor.ActiveSelectorStr;
                executor.AddCommandsClean(function.CallFunction(sel, this, executor.scoreboard, tokensInside.ToArray()), "call" + function.name);

                // store return value in temp
                ScoreboardValue clone = executor.scoreboard.RequestTemp(function.returnValue);
                executor.AddCommandsClean(clone.CommandsSet(sel, function.returnValue, null, null), "store" + function.name); // ignore accessors

                int len = x - i + (1 + tokensInside.Count);
                tokens.RemoveRange(i, len);
                tokens.Insert(i, new TokenIdentifierValue(clone.AliasName, clone, selected.lineNumber));

                // gets incremented;
                i = startAt - 1;
            }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// If the tokens inside this statement match its pattern, if any.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (patterns.Length < 1)
                    return true;
                return patterns.Any(tp => tp.Check(tokens).match);
            }
        }
    }

    /// <summary>
    /// Indicates something has blown up while executing a statement.
    /// </summary>
    public class StatementException : Exception
    {
        public readonly Statement statement;
        public StatementException(Statement statement, string message) : base(message)
        {
            this.statement = statement;
        }
    }
}