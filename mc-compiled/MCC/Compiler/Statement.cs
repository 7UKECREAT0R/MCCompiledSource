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
        private readonly TypePattern[] patterns;
        public Statement(Token[] tokens)
        {
            this.tokens = tokens;
            patterns = GetValidPatterns();
        }

        protected Token[] tokens;
        protected int currentToken;

        public bool HasNext
        {
            get => currentToken < tokens.Length;
        }
        public Token Next()
        {
            return tokens[currentToken++];
        }
        public Token Peek()
        {
            return tokens[currentToken];
        }
        public T Next<T>() where T : class
        {
            Token token = tokens[currentToken++];
            if (!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type otherType = implicitToken.GetImplicitType();

                    if (token.GetType().IsAssignableFrom(otherType))
                        return implicitToken.Convert() as T;
                }
                throw new StatementException(this, $"Invalid token type. Expected {typeof(T)} but got {token.GetType()}");
            }
            else
                return token as T;
        }
        public T Peek<T>() where T : class
        {
            Token token = tokens[currentToken];
            if(!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type otherType = implicitToken.GetImplicitType();

                    if (token.GetType().IsAssignableFrom(otherType))
                        return implicitToken.Convert() as T;
                }
                throw new StatementException(this, $"Invalid token type. Expected {typeof(T)} but got {token.GetType()}");
            } else
                return token as T;
        }
        public bool NextIs<T>()
        {
            if (!HasNext)
                return false;
            return tokens[currentToken] is T;
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
            currentToken = 0;
            Run(executor);
        }
        /// <summary>
        /// Clone this statement and resolve its unidentified tokens based off the current executor's state.
        /// After this is finished, squash and process any intermediate math or functional operations.<br />
        /// <br />This function pushes the Scoreboard temp state once.
        /// </summary>
        /// <returns>A shallow clone of this Statement which has its tokens resolved.</returns>
        public Statement CloneResolve(Executor executor)
        {
            Statement statement = MemberwiseClone() as Statement;
            int length = statement.tokens.Length;
            Token[] allUnresolved = statement.tokens;
            Token[] allResolved = new Token[length];

            for(int i = 0; i < length; i++)
            {
                Token unresolved = allUnresolved[i];
                Token resolved = unresolved;
                int line = unresolved.lineNumber;

                if (unresolved is TokenStringLiteral)
                    resolved = new TokenStringLiteral(executor.ResolveString(unresolved as TokenStringLiteral), line);
                else if (unresolved is TokenUnresolvedPPV)
                    resolved = (executor.ResolvePPV(unresolved as TokenUnresolvedPPV) ?? unresolved);

                if(unresolved is TokenIdentifier)
                {
                    string word = (unresolved as TokenIdentifier).word;
                    if (executor.scoreboard.TryGetByAccessor(word, out ScoreboardValue value))
                        resolved = new TokenIdentifierValue(word, value, line);
                    else if (executor.scoreboard.TryGetStruct(word, out StructDefinition @struct))
                        resolved = new TokenIdentifierStruct(word, @struct, line);
                    else if (executor.TryLookupMacro(word, out Macro? macro))
                        resolved = new TokenIdentifierMacro(macro.Value, line);
                    else if (executor.TryLookupFunction(word, out Function function))
                        resolved = new TokenIdentifierFunction(function, line);
                }

                allResolved[i] = resolved;
            }

            // TODO squash intermediate operations
            executor.scoreboard.PushTempState();
            List<Token> tokens = new List<Token>(allResolved);
            Squash<TokenArithmaticFirst>(ref tokens, executor);
            Squash<TokenArithmaticSecond>(ref tokens, executor);
            SquashFunctions(ref tokens, executor);

            statement.tokens = tokens.ToArray();
            return statement;
        }
        public void Squash<T>(ref List<Token> tokens, Executor executor)
        {
            for (int i = 1; i < (tokens.Count() - 1); i++)
            {
                Token selected = tokens[i];
                if (!(selected is T))
                    continue;

                // this can be assumed due to how squash is meant to be called
                TokenArithmatic.Type op = (selected as TokenArithmatic).GetArithmaticType();
                Token squashedToken = null;
                string selector = executor.ActiveSelectorStr;

                Token _left = tokens[i - 1];
                Token _right = tokens[i + 1];

                bool leftIsLiteral = _left is TokenLiteral;
                bool rightIsLiteral = _right is TokenLiteral;
                bool leftIsValue = _left is TokenIdentifierValue;
                bool rightIsValue = _right is TokenIdentifierValue;

                if(leftIsLiteral & rightIsLiteral)
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
                else if(leftIsValue & rightIsValue)
                {
                    TokenIdentifierValue left = _left as TokenIdentifierValue;
                    TokenIdentifierValue right = _right as TokenIdentifierValue;
                    string leftAccessor = left.Accessor;
                    string rightAccessor = right.Accessor;
                    ScoreboardValue a = left.value;
                    ScoreboardValue b = right.value;

                    ScoreboardValue temp = executor.scoreboard.RequestTemp(a);
                    string accessorTemp = temp.baseName;
                    if(temp is ScoreboardValueStruct && left.word.Contains(':'))
                    {
                        StructDefinition structure = (temp as ScoreboardValueStruct).structure;
                        string field = left.word.Split(':')[1];
                        accessorTemp = structure.GetAccessor(accessorTemp, field);
                    }

                    executor.AddCommandsClean(temp.CommandsSet(selector, a, accessorTemp, right.word));
                    squashedToken = new TokenIdentifierValue(accessorTemp, temp, selected.lineNumber);

                    switch (op)
                    {
                        case TokenArithmatic.Type.ADD:
                            executor.AddCommandsClean(temp.CommandsAdd(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.SUBTRACT:
                            executor.AddCommandsClean(temp.CommandsSub(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.MULTIPLY:
                            executor.AddCommandsClean(temp.CommandsMul(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.DIVIDE:
                            executor.AddCommandsClean(temp.CommandsDiv(selector, b, accessorTemp, right.word));
                            break;
                        case TokenArithmatic.Type.MODULO:
                            executor.AddCommandsClean(temp.CommandsMod(selector, b, accessorTemp, right.word));
                            break;
                        default:
                            break;
                    }
                }
                else
                {

                    if(leftIsLiteral)
                    {
                        ScoreboardValue temp = executor.scoreboard.RequestTemp(_left as TokenLiteral, this);
                        executor.AddCommandsClean(temp.CommandsSetLiteral(temp.baseName, selector, _left as TokenLiteral));
                        TokenIdentifierValue b = _right as TokenIdentifierValue;
                        squashedToken = new TokenIdentifierValue(temp.baseName, temp, selected.lineNumber);

                        switch (op)
                        {
                            case TokenArithmatic.Type.ADD:
                                executor.AddCommandsClean(temp.CommandsAdd(selector, b.value, temp.baseName, b.word));
                                break;
                            case TokenArithmatic.Type.SUBTRACT:
                                executor.AddCommandsClean(temp.CommandsSub(selector, b.value, temp.baseName, b.word));
                                break;
                            case TokenArithmatic.Type.MULTIPLY:
                                executor.AddCommandsClean(temp.CommandsMul(selector, b.value, temp.baseName, b.word));
                                break;
                            case TokenArithmatic.Type.DIVIDE:
                                executor.AddCommandsClean(temp.CommandsDiv(selector, b.value, temp.baseName, b.word));
                                break;
                            case TokenArithmatic.Type.MODULO:
                                executor.AddCommandsClean(temp.CommandsMod(selector, b.value, temp.baseName, b.word));
                                break;
                            default:
                                break;
                        }
                    } else
                    {
                        TokenIdentifierValue a = _left as TokenIdentifierValue;
                        TokenLiteral b = _right as TokenLiteral;
                        ScoreboardValue temp = executor.scoreboard.RequestTemp(a.value);

                        ScoreboardValue bTemp = executor.scoreboard.RequestTemp(b, this);
                        executor.AddCommandsClean(bTemp.CommandsSetLiteral(bTemp.baseName, selector, b));

                        string accessorTemp = temp.baseName;
                        if (temp is ScoreboardValueStruct && a.word.Contains(':'))
                        {
                            StructDefinition structure = (temp as ScoreboardValueStruct).structure;
                            string field = a.word.Split(':')[1];
                            accessorTemp = structure.GetAccessor(accessorTemp, field);
                        }

                        executor.AddCommandsClean(temp.CommandsSet(selector, a.value, accessorTemp, a.word));
                        squashedToken = new TokenIdentifierValue(accessorTemp, temp, selected.lineNumber);

                        switch (op)
                        {
                            case TokenArithmatic.Type.ADD:
                                executor.AddCommandsClean(temp.CommandsAdd(selector, bTemp, accessorTemp, bTemp.baseName));
                                break;
                            case TokenArithmatic.Type.SUBTRACT:
                                executor.AddCommandsClean(temp.CommandsSub(selector, bTemp, accessorTemp, bTemp.baseName));
                                break;
                            case TokenArithmatic.Type.MULTIPLY:
                                executor.AddCommandsClean(temp.CommandsMul(selector, bTemp, accessorTemp, bTemp.baseName));
                                break;
                            case TokenArithmatic.Type.DIVIDE:
                                executor.AddCommandsClean(temp.CommandsDiv(selector, bTemp, accessorTemp, bTemp.baseName));
                                break;
                            case TokenArithmatic.Type.MODULO:
                                executor.AddCommandsClean(temp.CommandsMod(selector, bTemp, accessorTemp, bTemp.baseName));
                                break;
                            default:
                                break;
                        }
                    }
                }

                // replace those three tokens with the one squashed one
                tokens.RemoveRange(i - 1, 3);
                tokens.Insert(i - 1, squashedToken);

                // restart oop
                i = 0;
            }
        }
        public void SquashFunctions(ref List<Token> tokens, Executor executor)
        {
            for(int i = 0; i < (tokens.Count() - 2); i++)
            {
                Token selected = tokens[i];
                Token second = tokens[i + 1];
                Token third = tokens[i + 2];

                if (!(selected is TokenIdentifierFunction))
                    continue;
                if (!(second is TokenOpenParenthesis))
                    continue; // might just be regular identifier

                int o = i + 2;
                TokenIdentifierFunction func = selected as TokenIdentifierFunction;
                Function function = func.function;

                List<Token> passIn = new List<Token>();
                if (!(third is TokenCloseParenthesis))
                {
                    while (o < tokens.Count)
                    {
                        Token check = tokens[o];
                        if (check is TokenCloseParenthesis)
                        {
                            o++;
                            break;
                        }
                        o++;
                        passIn.Add(check);
                    }
                }

                if (passIn.Count < function.ParameterCount)
                    throw new StatementException(this, $"Missing parameters for function {function}");
                if(function.returnValue == null)
                    throw new StatementException(this, $"Cannot use function in statement since it doesn't return a value.");

                // call function
                string sel = executor.ActiveSelectorStr;
                executor.AddCommandsClean(function.CallFunction(sel, this, passIn.ToArray()));
                // store return value in temp
                ScoreboardValue clone = executor.scoreboard.RequestTemp(function.returnValue);
                executor.AddCommandsClean(clone.CommandsSet(sel, function.returnValue, null, null)); // ignore accessors

                int len = o - i;
                tokens.RemoveRange(i, len);
                tokens.Insert(i, new TokenIdentifierValue(clone.baseName, clone, selected.lineNumber));

                // gets incremented;
                i = -1;
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