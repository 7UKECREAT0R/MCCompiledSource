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
                            return implicitToken.Convert(i) as T;    
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
                            return implicitToken.Convert(i) as T;
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
            if(patterns != null && patterns.Length > 0)
                if (!patterns.Any(pattern => pattern.Check(tokens)))
                    throw new StatementException(this, "Invalid call pattern. Make sure you included all arguments of the right type.");

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
            Token[] allUnresolved = statement.tokens;
            List<Token> allResolved = new List<Token>();

            for(int i = 0; i < length; i++)
            {
                Token unresolved = allUnresolved[i];
                int line = unresolved.lineNumber;

                if (unresolved is TokenStringLiteral)
                    allResolved.Add(new TokenStringLiteral(executor.ResolveString(unresolved as TokenStringLiteral), line));
                else if (resolvePPVs && unresolved is TokenUnresolvedPPV)
                    allResolved.AddRange(executor.ResolvePPV(unresolved as TokenUnresolvedPPV) ?? new Token[] { unresolved });
                else if(unresolved is TokenIdentifier)
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
                } else
                    allResolved.Add(unresolved);
            }

            executor.scoreboard.PushTempState(); // popped at call site
            SquashAll(ref allResolved, executor);

            statement.tokens = allResolved.ToArray();
            statement.patterns = statement.GetValidPatterns();
            return statement;
        }
        public void SquashAll(ref List<Token> tokens, Executor executor)
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

                if (this is StatementFunctionCall && i == 1)
                    startIndex += 1;
                else
                    removeLength += 2;

                // inside parentheses
                SquashAll(ref toSquash, executor);
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
                    string accessorTemp = temp.baseName;
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
                        aAccessor = a.baseName;
                        commands.AddRange(a.CommandsSetLiteral(a.baseName, selector, _left as TokenLiteral));
                        b = (_right as TokenIdentifierValue).value;
                        bAccessor = (_right as TokenIdentifierValue).Accessor;
                    }
                    else
                    {
                        b = executor.scoreboard.RequestTemp(_right as TokenLiteral, this);
                        bAccessor = b.baseName;
                        commands.AddRange(b.CommandsSetLiteral(b.baseName, selector, _right as TokenLiteral));

                        // left is a value, so it needs to be put into a temp variable so that the source is not modified
                        TokenIdentifierValue left = _left as TokenIdentifierValue;
                        a = executor.scoreboard.RequestTemp(left.value);
                        commands.AddRange(a.CommandsSet(selector, left.value, a.baseName, left.Accessor));
                        aAccessor = a.baseName;
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
                    int level = 1;
                    while (x < tokens.Count)
                    {
                        Token check = tokens[x];

                        if (check is TokenCloseParenthesis)
                        {
                            level--;
                            if (level <= 0)
                                break;
                        }
                        else if (check is TokenOpenParenthesis)
                            level++;

                        x++;
                        tokensInside.Add(check);
                    }
                    SquashFunctions(ref tokensInside, executor);
                    Squash<TokenArithmaticFirst>(ref tokensInside, executor);
                    Squash<TokenArithmaticSecond>(ref tokensInside, executor);
                    tokens.RemoveRange(i + 2, tokensInside.Count);
                    tokens.InsertRange(i + 2, tokensInside);
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

                int len = x - i + 1;
                tokens.RemoveRange(i, len);
                tokens.Insert(i, new TokenIdentifierValue(clone.baseName, clone, selected.lineNumber));

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
                return patterns.All(tp => tp.Check(tokens));
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