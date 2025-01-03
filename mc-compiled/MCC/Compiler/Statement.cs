using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Linq;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     A fully qualified statement which can be run.
/// </summary>
public abstract class Statement : TokenFeeder, ICloneable
{
    private TypePattern[] patterns;

    protected Statement(Token[] tokens, bool waitForPatterns = false) : base(tokens)
    {
        if (!waitForPatterns)
            // ReSharper disable once VirtualMemberCallInConstructor
            // DNC
            this.patterns = GetValidPatterns();
        this.DecorateInSource = true;
    }
    /// <summary>
    ///     If this statement should be skipped by the executor.
    /// </summary>
    public abstract bool Skip { get; }
    /// <summary>
    ///     If this statement will split async context. (grouping or stages)
    /// </summary>
    public abstract bool DoesAsyncSplit { get; }

    public object Clone() { return MemberwiseClone(); }
    /// <summary>
    ///     Returns if this statement type is a directive and it has this attribute.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public abstract bool HasAttribute(DirectiveAttribute attribute);

    protected abstract TypePattern[] GetValidPatterns();
    /// <summary>
    ///     Run this statement/continue where it left off.
    /// </summary>
    protected abstract void Run(Executor runningExecutor);

    /// <summary>
    ///     Clone this statement and resolve its unidentified tokens based off the current executor's state.
    ///     After this is finished, squash and process any intermediate math or functional operations.<br />
    ///     <br />This function pushes the Scoreboard temp state once.
    /// </summary>
    /// <returns>A shallow clone of this Statement which has its tokens resolved.</returns>
    public Statement ClonePrepare(Executor activeExecutor)
    {
        if (MemberwiseClone() is not Statement statement)
            return null;

        statement.PrepareThis(activeExecutor);
        return statement;
    }
    /// <summary>
    ///     Resolve any unidentified tokens based off the current executor's state on this instance.
    ///     After this is finished, squash and process any intermediate math or functional operations.
    /// </summary>
    /// <returns>A shallow clone of this Statement which has its tokens resolved.</returns>
    public void PrepareThis(Executor activeExecutor)
    {
        // set executors
        SetExecutor(activeExecutor);

        // e.g. close/open block
        if (this.tokens == null)
            return;

        bool lastTokenWasDeref = false;
        bool shouldDereference = !HasAttribute(DirectiveAttribute.DONT_DEREFERENCE);
        bool resolveStrings = !HasAttribute(DirectiveAttribute.DONT_RESOLVE_STRINGS);
        var allUnresolved = new List<Token>(this.tokens);
        var allResolved = new List<Token>();

        // now resolve tokens forward
        int len = allUnresolved.Count;
        for (int i = 0; i < len; i++)
        {
            Token unresolved = allUnresolved[i];
            int line = unresolved.lineNumber;

            if (unresolved is TokenDeref)
            {
                if (shouldDereference)
                {
                    allResolved.Add(unresolved);
                    lastTokenWasDeref = true;
                }

                continue;
            }

            // resolve the token.
            if (resolveStrings && unresolved is TokenStringLiteral literal)
                allResolved.Add(new TokenStringLiteral(activeExecutor.ResolveString(literal), line));
            else if (unresolved is TokenUnresolvedSelector unresolvedSelector)
                allResolved.Add(unresolvedSelector.Resolve(activeExecutor).Validate(this));
            else if (unresolved is TokenSelectorLiteral resolvedSelector)
                allResolved.Add(resolvedSelector.Validate(this));
            else
                switch (unresolved)
                {
                    case TokenIdentifier tokenIdentifier:
                    {
                        // identifier resolve requires a manual search.
                        string word = tokenIdentifier.word;

                        if (lastTokenWasDeref)
                        {
                            // prioritize preprocessor variables first -- just in case.
                            if (activeExecutor.TryGetPPV(word, out PreprocessorVariable ppv))
                                allResolved.Add(new TokenIdentifierPreprocessor(word, ppv, line));
                            break;
                        }

                        if (activeExecutor.scoreboard.TryGetByUserFacingName(word, out ScoreboardValue value))
                            allResolved.Add(new TokenIdentifierValue(word, value, line));
                        else if (activeExecutor.TryLookupMacro(word, out Macro? macro))
                            allResolved.Add(new TokenIdentifierMacro(macro.GetValueOrDefault(), line));
                        else if (activeExecutor.functions.TryGetFunctions(word, out Function[] functions))
                            allResolved.Add(new TokenIdentifierFunction(word, functions, line));
                        else if (activeExecutor.TryGetPPV(word, out PreprocessorVariable ppv))
                            allResolved.Add(new TokenIdentifierPreprocessor(word, ppv, line));
                        else
                            allResolved.Add(tokenIdentifier);
                        break;
                    }
                    case TokenOpenGroupingBracket grouper:
                        grouper.hasBeenSquashed = false;
                        allResolved.Add(grouper);
                        break;
                    default:
                        allResolved.Add(unresolved);
                        break;
                }

            lastTokenWasDeref = false;
        }

        SquashAll(allResolved, activeExecutor);

        this.tokens = allResolved.ToArray();
        this.patterns = GetValidPatterns();
    }

    /// <summary>
    ///     Decorate the active file with this statement's source if decoration is enabled.
    /// </summary>
    public void Decorate(Executor activeExecutor)
    {
        if (!GlobalContext.Decorate || !this.DecorateInSource || this.Source == null)
            return;
        if (this is StatementDirective std)
        {
            if (std.directive == null)
                return;
            bool dontDecorate = (std.directive.attributes & DirectiveAttribute.DONT_DECORATE) != 0;
            if (dontDecorate)
                return;
        }

        // find whether to add a newline or not
        CommandFile file = activeExecutor.CurrentFile;
        int length = file.Length;

        if (length > 0)
        {
            string lastLine = file.commands[length - 1];
            if (!lastLine.StartsWith("#") && lastLine.Length > 0 &&
                !lastLine.All(c =>
                    char.IsWhiteSpace(c))) // create newline if the last line was not a comment and not whitespace
                file.Add("");
        }

        file.Add("# " + this.Source);
    }
    /// <summary>
    ///     Run this statement from square one.
    /// </summary>
    public void Run0(Executor activeExecutor)
    {
        if (this.patterns != null && this.patterns.Length > 0)
        {
            IEnumerable<MatchResult> results = this.patterns.Select(
                pattern => pattern.Check(this.tokens));
            IEnumerable<MatchResult> matchResults = results as MatchResult[] ?? results.ToArray();

            if (matchResults.All(result => !result.match))
            {
                // get the closest matched pattern
                MatchResult closest = matchResults.Aggregate((a, b) => a.accuracy > b.accuracy ? a : b);
                IEnumerable<string> missingArgs = closest.missing.Select(m => m.ToString());
                throw new StatementException(this, "Missing argument(s): " + string.Join(", ", missingArgs));
            }
        }

        this.currentToken = 0;
        Run(activeExecutor);

        // if there's tokens left in this statement, then the user likely expected them to be used.
        // throwing a helpful exception if this is the case. I would emit a warning if I could (do this when LSP is implemented pls)

        if (this.RemainingTokens == 0)
            return;

        Token[] remainingUsefulTokens = GetRemainingTokens().Where(t => t is not IUselessInformation).ToArray();

        if (remainingUsefulTokens.Length == 0)
            return;

        string plural = remainingUsefulTokens.Length == 1 ? "token" : "tokens";
        string joined = string.Join(", ", remainingUsefulTokens.Select(t => t.AsString()));
        throw new StatementException(this, $"Trailing unused {plural}: {joined}");
    }
    private void SquashAll(List<Token> squashTokens, Executor activeExecutor)
    {
        // recursively call parenthesis first
        for (int i = 0; i < squashTokens.Count; i++)
        {
            Token token = squashTokens[i];
            if (token is not TokenOpenGroupingBracket opener)
                continue;
            if (opener.hasBeenSquashed)
                continue;

            int level = 1;
            var toSquash = new List<Token>();
            for (int x = i + 1; x < squashTokens.Count; x++)
            {
                token = squashTokens[x];
                switch (token)
                {
                    case TokenOpenGroupingBracket opener2 when opener.IsAssociated(opener2):
                        level++;
                        break;
                    case TokenCloseGroupingBracket closer when opener.IsAssociated(closer):
                    {
                        level--;
                        if (level < 1)
                            goto properlyClosed;
                        break;
                    }
                }

                toSquash.Add(token);
            }

            throw new StatementException(this, "Unexpected end-of-line inside parenthesis.");

            properlyClosed:
            int startIndex = i;
            int removeLength = toSquash.Count;

            // check if this is a function call.
            bool isFunction = this is StatementFunctionCall && i == 1;
            if (!isFunction && i > 0)
                isFunction |= squashTokens[i - 1] is TokenIdentifierFunction;

            // only remove parentheses, NOT indexers, if they're used for grouping
            if (isFunction || opener is TokenOpenIndexer)
                startIndex += 1;
            else
                removeLength += 2;

            // inside parentheses
            SquashAll(toSquash, activeExecutor);
            squashTokens.RemoveRange(startIndex, removeLength);
            squashTokens.InsertRange(startIndex, toSquash);
            opener.hasBeenSquashed = true;
            i = -1; // reset back to the start
        }

        // squash any range pieces into a complete range, now that the numbers have been evaluated
        SquashSpecial(squashTokens);
        // squash any indexing brackets into single semantically evaluated tokens.
        SquashIndexers(squashTokens);

        // run $dereferences
        SquashDereferences(squashTokens);
        // run functions(...)
        SquashFunctions(squashTokens, activeExecutor);
        // run indexers[...]
        SquashIndexing(squashTokens);
        // run multiplication/division/modulo
        Squash<TokenArithmeticFirst>(squashTokens, activeExecutor);
        // run addition/subtraction
        Squash<TokenArithmeticSecond>(squashTokens, activeExecutor);
    }

    private void SquashSpecial(List<Token> squashTokens)
    {
        // going over it backwards for merging any particular tokens
        for (int i = squashTokens.Count - 1; i >= 0; i--)
        {
            Token token = squashTokens[i];

            switch (token)
            {
                // ridiculously complex range stuff
                case TokenRangeDots _:
                {
                    TokenRangeLiteral replacement;
                    int replacementLocation;
                    int replacementLength;

                    Token back1 = Previous(1);
                    Token next1 = After(1);

                    if (back1 is TokenIntegerLiteral back1Literal)
                    {
                        int? numberMax;
                        if (next1 is TokenIntegerLiteral)
                            numberMax = next1 as TokenIntegerLiteral;
                        else
                            numberMax = null;

                        replacementLocation = -1;
                        replacementLength = numberMax.HasValue ? 3 : 2;
                        int numberMin = back1Literal;
                        var range = new Range(numberMin, numberMax);
                        replacement = new TokenRangeLiteral(range, token.lineNumber);
                    }
                    else
                    {
                        if (next1 is not TokenIntegerLiteral next1Literal)
                            throw new TokenizerException("Range argument only accepts integers.");
                        replacementLocation = 0;
                        replacementLength = 2;
                        int number = next1Literal;
                        var range = new Range(null, number);
                        replacement = new TokenRangeLiteral(range, token.lineNumber);
                    }

                    i += replacementLocation;
                    if (Previous(1) is TokenRangeInvert)
                    {
                        i--;
                        replacementLength += 1;
                        replacement.range.invert = true;
                    }

                    squashTokens.RemoveRange(i, replacementLength);
                    squashTokens.Insert(i, replacement);
                    continue;
                }
                case TokenRangeInvert _:
                {
                    Token after = After(1);
                    if (after is not TokenIntegerLiteral afterLiteral)
                        throw new TokenizerException("You can only invert integers.");
                    squashTokens.RemoveRange(i, 2);
                    int number = afterLiteral;
                    var range = new Range(number, true);
                    squashTokens.Insert(i, new TokenRangeLiteral(range, token.lineNumber));
                    continue;
                }
            }

            // flatten JArrays, as long as !DONT_FLATTEN_ARRAYS
            if (!HasAttribute(DirectiveAttribute.DONT_FLATTEN_ARRAYS) && token is TokenJSONLiteral _json)
            {
                JToken json = _json.token;
                if (json is JArray jsonArray)
                {
                    var replace = new List<TokenLiteral>(jsonArray.Count);
                    int line = this.Lines[0];

                    foreach (JToken jsonToken in jsonArray)
                        if (PreprocessorUtils.TryGetLiteral(jsonToken, line, out TokenLiteral unwrapped))
                            replace.Add(unwrapped);

                    squashTokens.RemoveAt(i);
                    squashTokens.InsertRange(i, replace);
                    i += replace.Count;
                }
            }

            continue;

            Token Previous(int amount)
            {
                if (i - amount < 0)
                    return null;
                return squashTokens[i - amount];
            }

            Token After(int amount)
            {
                if (i + amount >= squashTokens.Count)
                    return null;
                return squashTokens[i + amount];
            }
        }
    }
    private void Squash<T>(List<Token> squashTokens, Executor activeExecutor)
    {
        for (int i = 1; i < squashTokens.Count - 1; i++)
        {
            Token selected = squashTokens[i];
            if (selected is not T)
                continue;
            if (selected is IAssignment)
                continue; // dont squash assignments

            // this can be assumed due to how squash is meant to be called
            var arithmetic = (TokenArithmetic) selected;
            TokenArithmetic.Type op = arithmetic.GetArithmeticType();
            var commands = new List<string>();
            Token squashedToken = null;

            Token _left = squashTokens[i - 1];
            Token _right = squashTokens[i + 1];

            bool squashToGlobal;
            bool leftIsLiteral = _left is TokenLiteral;
            bool rightIsLiteral = _right is TokenLiteral;
            bool leftIsValue = _left is TokenIdentifierValue;
            bool rightIsValue = _right is TokenIdentifierValue;

            if (leftIsLiteral & rightIsLiteral)
            {
                var left = _left as TokenLiteral;
                var right = _right as TokenLiteral;

                squashedToken = op switch
                {
                    TokenArithmetic.Type.ADD => left.AddWithOther(right),
                    TokenArithmetic.Type.SUBTRACT => left.SubWithOther(right),
                    TokenArithmetic.Type.MULTIPLY => left.MulWithOther(right),
                    TokenArithmetic.Type.DIVIDE => left.DivWithOther(right),
                    TokenArithmetic.Type.MODULO => left.ModWithOther(right),
                    TokenArithmetic.Type.SWAP => throw new StatementException(this, "Attempting to swap literals"),
                    _ => null
                };
            }
            else if (leftIsValue & rightIsValue)
            {
                var left = _left as TokenIdentifierValue;
                var right = _right as TokenIdentifierValue;
                ScoreboardValue a = left.value;
                ScoreboardValue b = right.value;
                squashToGlobal = a.clarifier.IsGlobal || b.clarifier.IsGlobal;

                ScoreboardValue temp = activeExecutor.scoreboard.temps.RequestCopy(a, squashToGlobal);
                string accessorTemp = temp.Name;

                commands.AddRange(temp.Assign(a, this));
                squashedToken = new TokenIdentifierValue(accessorTemp, temp, selected.lineNumber);

                switch (op)
                {
                    case TokenArithmetic.Type.ADD:
                        commands.AddRange(temp.Add(b, this));
                        break;
                    case TokenArithmetic.Type.SUBTRACT:
                        commands.AddRange(temp.Subtract(b, this));
                        break;
                    case TokenArithmetic.Type.MULTIPLY:
                        commands.AddRange(temp.Multiply(b, this));
                        break;
                    case TokenArithmetic.Type.DIVIDE:
                        commands.AddRange(temp.Divide(b, this));
                        break;
                    case TokenArithmetic.Type.MODULO:
                        commands.AddRange(temp.Modulo(b, this));
                        break;
                    case TokenArithmetic.Type.SWAP:
                        commands.AddRange(temp.Swap(b, this));
                        break;
                    default:
                        throw new Exception($"Unknown arithmetic type '{op}'.");
                }
            }
            else if (leftIsValue | rightIsValue
                     && leftIsLiteral | rightIsLiteral)
            {
                string aAccessor;
                ScoreboardValue a, b;
                if (leftIsLiteral)
                {
                    b = (_right as TokenIdentifierValue).value;
                    a = activeExecutor.scoreboard.temps.Request(_left as TokenLiteral, this, true);
                    aAccessor = a.Name;
                    commands.AddRange(a.AssignLiteral(_left as TokenLiteral, this));
                }
                else
                {
                    var left = _left as TokenIdentifierValue;
                    squashToGlobal = left.value.clarifier.IsGlobal;

                    b = activeExecutor.scoreboard.temps.Request(_right as TokenLiteral, this, true);
                    commands.AddRange(b.AssignLiteral(_right as TokenLiteral, this));

                    // left is a value, so it needs to be put into a temp variable so that the source is not modified
                    a = activeExecutor.scoreboard.temps.RequestCopy(left.value, squashToGlobal);
                    commands.AddRange(a.Assign(left.value, this));
                    aAccessor = a.Name;
                }

                squashedToken = new TokenIdentifierValue(aAccessor, a, selected.lineNumber);

                switch (op)
                {
                    case TokenArithmetic.Type.ADD:
                        commands.AddRange(a.Add(b, this));
                        break;
                    case TokenArithmetic.Type.SUBTRACT:
                        commands.AddRange(a.Subtract(b, this));
                        break;
                    case TokenArithmetic.Type.MULTIPLY:
                        commands.AddRange(a.Multiply(b, this));
                        break;
                    case TokenArithmetic.Type.DIVIDE:
                        commands.AddRange(a.Divide(b, this));
                        break;
                    case TokenArithmetic.Type.MODULO:
                        commands.AddRange(a.Modulo(b, this));
                        break;
                    case TokenArithmetic.Type.SWAP:
                        commands.AddRange(a.Swap(b, this));
                        break;
                    default:
                        throw new Exception($"Unknown arithmetic type '{op}'.");
                }
            }
            else
            {
                throw new StatementException(this,
                    $"No valid data given in tokens '{_left}' and '{_right}'; was there a misspelling?");
            }

            CommandFile file = activeExecutor.CurrentFile;
            int nextLineNumber = activeExecutor.NextLineNumber;
            activeExecutor.AddCommandsClean(commands, "inlineOp",
                $"Part of an inline math operation from {file.CommandReference} line {nextLineNumber}. Performs ({_left.AsString()} {arithmetic} {_right.AsString()}).");

            // replace those three tokens with the one squashed one
            squashTokens.RemoveRange(i - 1, 3);
            squashTokens.Insert(i - 1, squashedToken);

            // restart order-of-operations
            i = -1;
        }
    }
    private void SquashFunctions(List<Token> squashTokens, Executor activeExecutor)
    {
        // be careful if this is a function definition, might be in the same format.
        bool isFunctionDefinition = this is StatementDirective directive &&
                                    directive.directive.call == DirectiveImplementations.function;

        int startAt = 0;

        // ignore first function call since that's part of the statement
        if (this is StatementFunctionCall)
            startAt = 2;

        for (int i = startAt; i < squashTokens.Count; i++)
        {
            Token selected = squashTokens[i];
            Token second = squashTokens.Count > i + 1 ? squashTokens[i + 1] : null;
            Token third = squashTokens.Count > i + 2 ? squashTokens[i + 2] : null;

            if (selected is not TokenIdentifierFunction identifierFunction)
                continue;

            Function[] functions = identifierFunction.functions;

            // if a function is being defined and there's an existing function (but it's user defined)
            // then this is almost 100% unintentional.
            if (isFunctionDefinition && functions.Any(f => f is RuntimeFunction))
                continue;

            // skip if there's no parenthesis and no functions can be implicitly called.
            bool useImplicit = false;
            if (!functions.Any(f => f.ImplicitCall))
            {
                if (second is not TokenOpenParenthesis)
                    continue; // might just be regular identifier
            }
            else
            {
                if (second is not TokenOpenParenthesis)
                    useImplicit = true; // call to an implicit function
            }

            int x = i + (useImplicit ? 0 : 2);

            // if its not parameterless() or implicit, fetch until level <= 0
            var _tokensInside = new List<Token>();
            if (!useImplicit && third is not TokenCloseParenthesis)
                for (int z = x; z < squashTokens.Count; z++)
                {
                    Token token = squashTokens[z];
                    if (token is TokenCloseParenthesis)
                        break;
                    _tokensInside.Add(squashTokens[z]);
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
                        // the last error is stored, so it will be shown if no valid function is found.
                        continue;

                    if (score <= bestFunctionScore)
                        continue;

                    foundValidMatch = true;
                    bestFunction = function;
                    bestFunctionScore = score;
                }

                if (!foundValidMatch)
                    throw new StatementException(this, lastError);
            }

            if (bestFunction == null)
                throw new StatementException(this,
                    $"No best function was found for identifier: {identifierFunction.word}. Implicit: {useImplicit}. Please submit this as an issue on GitHub, or contact lukecreator on Discord about this.");

            var commands = new List<string>();

            // process the parameters and get their commands.
            bestFunction.ProcessParameters(tokensInside, commands, activeExecutor, this);

            // call the function.
            Token replacement = bestFunction.CallFunction(commands, tokensInside, activeExecutor, this);

            // finish with the commands.
            CommandFile current = activeExecutor.CurrentFile;

            // register the call for usage tree
            if (bestFunction is RuntimeFunction runtime)
                current.RegisterCall(runtime.file);

            activeExecutor.AddCommandsClean(commands, "call" + bestFunction.Keyword.Replace('.', '_'),
                $"From file {current.CommandReference} line {activeExecutor.NextLineNumber}: {bestFunction.Keyword}({string.Join(", ", tokensInside.Select(t => t.AsString()))})");
            commands.Clear();

            if (useImplicit)
            {
                squashTokens.RemoveAt(i);
                squashTokens.Insert(i, replacement);
            }
            else
            {
                // substitute the returned value from the function.
                int len = x - i + 1 + tokensInside.Length;
                squashTokens.RemoveRange(i, len);
                squashTokens.Insert(i, replacement);
            }

            // i gets incremented next iteration
            // NOTE: the reason it restarts from the beginning is just for stability. it doesn't have any real purpose
            i = startAt - 1;
        }
    }
    private void SquashDereferences(List<Token> squashTokens)
    {
        int tokensLength = squashTokens.Count;
        int i;

        for (i = 0; i < tokensLength - 1; i++)
        {
            Token iToken = squashTokens[i];

            if (iToken is not TokenDeref)
                continue;

            // get the next token and see if it's a preprocessor variable
            Token nextToken = squashTokens[i + 1];
            if (nextToken is not TokenIdentifierPreprocessor preprocessorToken)
                throw new StatementException(this, $"Cannot dereference token: {nextToken.AsString()}");

            PreprocessorVariable ppv = preprocessorToken.variable;
            int insertCount = ppv.Length - 1;
            int line = this.Lines?[0] ?? 0;

            // check to see if every item in the ppv can be dereferenced, and wrap it in a literal
            IEnumerable<TokenLiteral> wrappedLiterals = ppv
                .Select(d =>
                {
                    TokenLiteral literal = PreprocessorUtils.DynamicToLiteral(d, line);
                    if (literal == null)
                        throw new StatementException(this, $"Value {d} could not be dereferenced into code.");
                    return literal;
                });

            // remove i and i+1 from the tokens
            squashTokens.RemoveRange(i, 2);
            tokensLength -= 1 - insertCount; // length reduced by one (deref removed), but went up by insertCount

            // insert the tokens and step forward past them, if needed.
            squashTokens.InsertRange(i, wrappedLiterals);
            i += insertCount;
        }
    }
    private void SquashIndexers(List<Token> squashTokens)
    {
        for (int i = squashTokens.Count - 1; i >= 0; i--)
        {
            Token token = squashTokens[i];

            if (token is not TokenCloseIndexer closer)
                continue;

            // pull tokens until we find an opener
            var openerTokens = new Stack<Token>();
            for (int j = i - 1; j >= 0; j--)
            {
                Token previousToken = squashTokens[j];
                if (previousToken is TokenOpenIndexer)
                {
                    i = j;
                    goto foundOpener;
                }

                openerTokens.Push(previousToken);
            }

            throw new StatementException(this, $"Couldn't find opening bracket for closer '{closer.AsString()}'.");

            foundOpener:
            // create an indexer out of them
            Token[] insideTokens = openerTokens
                .Reverse()
                .ToArray();
            var indexer = TokenIndexer.CreateIndexer(insideTokens, this);
            squashTokens.RemoveRange(i, insideTokens.Length + 2); // account for open/close brackets
            squashTokens.Insert(i, indexer);
        }
    }
    private void SquashIndexing(List<Token> squashTokens)
    {
        for (int i = squashTokens.Count - 1; i >= 0; i--)
        {
            Token token = squashTokens[i];

            // apply indexer to IIndexable
            if (token is TokenIndexer indexer)
            {
                var indexerBuffer = new Stack<TokenIndexer>();
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

                    if (current is not IIndexable indexable)
                        throw new StatementException(this,
                            $"Cannot index token '{current.AsString()}'. (indexer: {indexer.AsString()})");

                    current = indexable.Index(indexer, this);
                } while (indexerBuffer.Count > 0);

                // replace tokens
                squashTokens.RemoveRange(i, indexerCount);
                squashTokens[i - 1] = current;
            }

            continue;

            Token Previous(int amount)
            {
                if (i - amount < 0)
                    return null;
                return squashTokens[i - amount];
            }
        }
    }
}

/// <summary>
///     A statement with no syntactic meaning, just holds tokens. Can only be created by the compiler.
/// </summary>
public sealed class StatementHusk : Statement
{
    public StatementHusk(Token[] tokens) : base(tokens, true) { }
    public override bool Skip => true;
    public override bool DoesAsyncSplit => false;
    public override string ToString()
    {
        return $"[HUSK] {string.Join(" ", from t in this.tokens select t.DebugString())}";
    }
    protected override void Run(Executor runningExecutor)
    {
        throw new StatementException(this, "Compiler tried to run a Husk statement. Have a dev look at this.");
    }
    protected override TypePattern[] GetValidPatterns() { return null; }
    public override bool HasAttribute(DirectiveAttribute attribute) { return false; }
}

/// <summary>
///     Indicates something has blown up while executing a statement.
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
///     Indicates something has blown up while feeding tokens.
/// </summary>
public class FeederException : Exception
{
    public readonly TokenFeeder feeder;
    public FeederException(TokenFeeder feeder, string message) : base(message) { this.feeder = feeder; }
}