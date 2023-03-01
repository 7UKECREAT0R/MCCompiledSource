﻿using mc_compiled.Commands;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Linq;
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
    public abstract class Statement : TokenFeeder, ICloneable
    {
        /// <summary>
        /// If this statement should be skipped by the executor.
        /// </summary>
        public abstract bool Skip { get; }

        private TypePattern[] patterns;
        public Statement(Token[] tokens, bool waitForPatterns = false) : base(tokens)
        {
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

            // set executors
            this.executor = executor;
            statement.SetExecutor(executor);

            // e.g. close/open block
            if (statement.tokens == null)
            {
                // i hate the jank here but just trust this gets popped later
                // ignores contract
                executor.scoreboard.PushTempState();
                return statement;
            }

            bool resolvePPVsGlobal = !HasAttribute(DirectiveAttribute.DONT_EXPAND_PPV);
            int length = statement.tokens.Length;
            List<Token> allUnresolved = new List<Token>(statement.tokens);
            List<Token> allResolved = new List<Token>();

            // now resolve tokens forward
            int len = allUnresolved.Count;
            for(int i = 0; i < len; i++)
            {
                Token unresolved = allUnresolved[i];
                Token next = (i + 1 < len) ? allUnresolved[i + 1] : null;
                int line = unresolved.lineNumber;

                // define resolvePPVs
                bool resolvePPVs;

                // if the next token is an indexer, dont resolve the ppv since.
                // the resolve will be done by the indexer.
                if (next != null && next is TokenIndexer)
                    resolvePPVs = false;
                else
                    resolvePPVs = resolvePPVsGlobal;

                // resolve the token.
                if (unresolved is TokenStringLiteral)
                    allResolved.Add(new TokenStringLiteral(executor.ResolveString(unresolved as TokenStringLiteral), line));
                else if (resolvePPVs && unresolved is TokenUnresolvedPPV)
                    allResolved.AddRange(executor.ResolvePPV(unresolved as TokenUnresolvedPPV, statement) ?? new Token[] { unresolved });
                else if (resolvePPVs && unresolved is TokenIndexerUnresolvedPPV)
                    allResolved.Add((unresolved as TokenIndexerUnresolvedPPV).Resolve(executor, statement));
                else if (unresolved is TokenIdentifier)
                {
                    // identifier resolve requires a manual search.
                    TokenIdentifier identifier = unresolved as TokenIdentifier;
                    string word = identifier.word;
                    if (executor.scoreboard.TryGetByAccessor(word, out ScoreboardValue value, true))
                        allResolved.Add(new TokenIdentifierValue(word, value, line));
                    else if (executor.TryLookupMacro(word, out Macro? macro))
                        allResolved.Add(new TokenIdentifierMacro(macro.Value, line));
                    else if (executor.functions.TryGetFunctions(word, out Function[] functions))
                        allResolved.Add(new TokenIdentifierFunction(word, functions, line));
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

            executor.scoreboard.PushTempState(); // popped at call site, ignore contract
            SquashAll(allResolved, executor);
            SquashSpecial(allResolved); // ranges, indexers, etc.

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
                    continue;
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
                    continue;
                }

                // apply indexer to IIndexable
                if(token is TokenIndexer indexer)
                {
                    Stack<TokenIndexer> indexerBuffer = new Stack<TokenIndexer>();
                    indexerBuffer.Push(indexer);
                    Token current;
                    int indexerCount = 1;

                    // pull multiple indexers in reverse order
                    // e.g., someVariable["a"][2]["otherKey"]
                    while ((current = Previous(1)) is TokenIndexer tokenIndexer)
                    {
                        indexerCount++;
                        i--;
                        indexerBuffer.Push(tokenIndexer);
                    }

                    // if there's no valid token 
                    if (current == null)
                        throw new StatementException(this, $"Indexer '{indexer.AsString()}' is invalid here.");

                    // process token through all indexers
                    do
                    {
                        indexer = indexerBuffer.Pop();

                        if (!(current is IIndexable))
                            throw new StatementException(this, $"Cannot index token '{current.AsString()}'. (indexer: {indexer.AsString()})");

                        IIndexable indexable = current as IIndexable;
                        current = indexable.Index(indexer, this);
                    }
                    while(indexerBuffer.Count > 0);

                    // replace tokens
                    tokens.RemoveRange(i, indexerCount);
                    tokens[i - 1] = current;
                    
                    // 'i' is setup so that the next iteration here will run over 'current'.
                    // just incase it needs to be resolved further past this point.
                    continue;
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
                
                Token _left = tokens[i - 1];
                Token _right = tokens[i + 1];

                bool squashToGlobal = false;
                bool leftIsLiteral = _left is TokenLiteral;
                bool rightIsLiteral = _right is TokenLiteral;
                bool leftIsValue = _left is TokenIdentifierValue;
                bool rightIsValue = _right is TokenIdentifierValue;

                if (leftIsLiteral & rightIsLiteral)
                {
                    squashToGlobal = false;
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
                    ScoreboardValue a = left.value;
                    ScoreboardValue b = right.value;
                    squashToGlobal = a.clarifier.IsGlobal || b.clarifier.IsGlobal;

                    ScoreboardValue temp = executor.scoreboard.RequestTemp(a);
                    string accessorTemp = temp.Name;

                    commands.AddRange(temp.CommandsSet(b));
                    squashedToken = new TokenIdentifierValue(accessorTemp, temp, selected.lineNumber);

                    switch (op)
                    {
                        case TokenArithmatic.Type.ADD:
                            commands.AddRange(temp.CommandsAdd(b));
                            break;
                        case TokenArithmatic.Type.SUBTRACT:
                            commands.AddRange(temp.CommandsSub(b));
                            break;
                        case TokenArithmatic.Type.MULTIPLY:
                            commands.AddRange(temp.CommandsMul(b));
                            break;
                        case TokenArithmatic.Type.DIVIDE:
                            commands.AddRange(temp.CommandsDiv(b));
                            break;
                        case TokenArithmatic.Type.MODULO:
                            commands.AddRange(temp.CommandsMod(b));
                            break;
                        default:
                            break;
                    }
                }
                else if (leftIsValue | rightIsValue
                    && leftIsLiteral | rightIsLiteral)
                {
                    string aAccessor, bAccessor;
                    ScoreboardValue a, b;
                    if (leftIsLiteral)
                    {
                        b = (_right as TokenIdentifierValue).value;
                        squashToGlobal = b.clarifier.IsGlobal;

                        a = executor.scoreboard.RequestTemp(_left as TokenLiteral, squashToGlobal, this);
                        aAccessor = a.Name;
                        commands.AddRange(a.CommandsSetLiteral(_left as TokenLiteral));
                        bAccessor = (_right as TokenIdentifierValue).Accessor;
                    }
                    else
                    {
                        TokenIdentifierValue left = _left as TokenIdentifierValue;
                        squashToGlobal = left.value.clarifier.IsGlobal;

                        b = executor.scoreboard.RequestTemp(_right as TokenLiteral, squashToGlobal, this);
                        bAccessor = b.Name;
                        commands.AddRange(b.CommandsSetLiteral(_right as TokenLiteral));

                        // left is a value, so it needs to be put into a temp variable so that the source is not modified
                        a = executor.scoreboard.RequestTemp(left.value);
                        commands.AddRange(a.CommandsSet(left.value));
                        aAccessor = a.Name;
                    }

                    squashedToken = new TokenIdentifierValue(aAccessor, a, selected.lineNumber);

                    switch (op)
                    {
                        case TokenArithmatic.Type.ADD:
                            commands.AddRange(a.CommandsAdd(b));
                            break;
                        case TokenArithmatic.Type.SUBTRACT:
                            commands.AddRange(a.CommandsSub(b));
                            break;
                        case TokenArithmatic.Type.MULTIPLY:
                            commands.AddRange(a.CommandsMul(b));
                            break;
                        case TokenArithmatic.Type.DIVIDE:
                            commands.AddRange(a.CommandsDiv(b));
                            break;
                        case TokenArithmatic.Type.MODULO:
                            commands.AddRange(a.CommandsMod(b));
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

            for(int i = startAt; i < tokens.Count; i++)
            {
                Token selected = tokens[i];
                Token second = (tokens.Count > (i + 1)) ? tokens[i + 1] : null;
                Token third = (tokens.Count > (i + 2)) ? tokens[i + 2] : null;

                if (!(selected is TokenIdentifierFunction))
                    continue;

                TokenIdentifierFunction func = selected as TokenIdentifierFunction;
                Function[] functions = func.functions;

                // skip if there's no parenthesis and no functions can be implicitly called.
                bool useImplicit = false;
                if (!functions.Any(f => f.ImplicitCall))
                {
                    if (!(second is TokenOpenParenthesis))
                        continue; // might just be regular identifier
                }
                else
                {
                    if (!(second is TokenOpenParenthesis))
                        useImplicit = true; // call to an implicit function
                }

                int x = i + (useImplicit ? 0 : 2);

                // if its not parameterless() or implicit, fetch until level <= 0
                List<Token> _tokensInside = new List<Token>();
                if (!useImplicit && !(third is TokenCloseParenthesis))
                {
                    for(int z = x; z < tokens.Count; z++)
                    {
                        Token token = tokens[z];
                        if (token is TokenCloseParenthesis)
                            break;
                        _tokensInside.Add(tokens[z]);
                    }
                }
                Token[] tokensInside = _tokensInside.ToArray();

                // these are already sorted by importance, so now just find the best match.
                Function bestFunction = null;
                int bestFunctionScore = int.MinValue;
                string lastError = null;

                if (useImplicit)
                {
                    // simply use the first (most important) implicit function available.
                    bestFunction = functions.First(f => f.ImplicitCall);
                }
                else
                {
                    bool foundValidMatch = false;

                    foreach (Function function in functions)
                    {
                        if (!function.MatchParameters(tokensInside,
                            out lastError, out int score))
                        {
                            // the last error is stored, so it will be shown if no valid function is found.
                            continue;
                        }

                        if (score > bestFunctionScore)
                        {
                            foundValidMatch = true;
                            bestFunction = function;
                            bestFunctionScore = score;
                        }
                    }

                    if (!foundValidMatch)
                        throw new StatementException(this, lastError);
                }

                if (bestFunction == null)
                    throw new StatementException(this, $"Strange error, report to the devs. No best function was found for identifier: {func.word}. Implicit: {useImplicit}");

                List<string> commands = new List<string>();

                // process the parameters and get their commands.
                bestFunction.ProcessParameters(tokensInside, commands, executor, this);

                // call the function.
                Token replacement = bestFunction.CallFunction(commands, executor, this);

                // finish with the commands.
                executor.AddCommandsClean(commands, "call" + bestFunction.Keyword);
                commands.Clear();

                if (useImplicit)
                {
                    tokens.RemoveAt(i);
                    tokens.Insert(i, replacement);
                    i--;
                }
                else
                {
                    // substitute the returned value from the function.
                    int len = x - i + (1 + tokensInside.Length);
                    tokens.RemoveRange(i, len);
                    tokens.Insert(i, replacement);
                }

                // i gets incremented next iteration
                // NOTE: the reason it restarts from the beginning is just for stability. it doesn't have any real purpose
                i = startAt - 1;
                break;
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
    public class StatementException : FeederException
    {
        public readonly Statement statement;
        public StatementException(Statement statement, string message) : base(statement, message)
        {
            this.statement = statement;
        }
    }
    /// <summary>
    /// Indicates something has blown up while feeding tokens.
    /// </summary>
    public class FeederException : Exception
    {
        public readonly TokenFeeder feeder;
        public FeederException(TokenFeeder feeder, string message) : base(message)
        {
            this.feeder = feeder;
        }
    }
}