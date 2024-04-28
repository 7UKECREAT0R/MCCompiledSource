using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands.Native;

namespace mc_compiled.MCC.Compiler
{
    public sealed class StatementDirective : Statement, IExecutionSetPart
    {
        public readonly Directive directive;
        public override bool Skip => false;
        public override bool DoesAsyncSplit => HasAttribute(DirectiveAttribute.CAUSES_ASYNC_SPLIT);

        public StatementDirective(Directive directive, Token[] tokens) : base(tokens, true)
        {
            this.directive = directive;
            this.DecorateInSource = !directive.IsPreprocessor;
        }
        public override bool HasAttribute(DirectiveAttribute attribute)
        {
            if (this.directive == null)
                return false;
            return (this.directive.attributes & attribute) != 0;
        }
        public override string ToString()
        {
            if (this.directive == null)
                return $"[DIRECTIVE] [PARSING ERROR]";

            return $"[DIRECTIVE] {this.directive.description} -> {string.Join(" ", from t in this.tokens select t.DebugString())}";
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return this.directive.patterns;
        }
        protected override void Run(Executor runningExecutor)
        {
            this.directive.call(runningExecutor, this);
        }
    }
    public sealed class StatementComment : Statement
    {
        public readonly string comment;
        public override bool Skip => true;
        public override bool DoesAsyncSplit => false;

        public StatementComment(string comment) : base(Array.Empty<Token>(), true)
        {
            this.comment = comment;
            this.DecorateInSource = false;
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString() => $"[COMMENT] {this.comment}";
        protected override TypePattern[] GetValidPatterns() => Array.Empty<TypePattern>();

        protected override void Run(Executor runningExecutor)
        {
            if (!Program.DECORATE)
                return;

            // peek at the next to determine if this comment will be used as documentation or not.
            if (runningExecutor.HasNext)
            {
                Statement next = runningExecutor.Peek();
                if (next is StatementDirective statementDirective)
                {
                    if (statementDirective.HasAttribute(DirectiveAttribute.DOCUMENTABLE))
                        return; // this is a documentation string.
                }
            }

            string str = runningExecutor.ResolveString(this.comment);

            // find whether to add a newline or not
            CommandFile file = runningExecutor.CurrentFile;
            int length = file.Length;

            if (length > 0)
            {
                // get last line
                string lastLine = file.commands[length - 1];
                if (!lastLine.StartsWith("#")) // create newline if the last line was not a comment
                    file.Add("");
            }

            IEnumerable<string> chunks = str
                .Trim()
                .Split('\n')
                .Select(line => "# " + line.Trim());
            
            str = string.Join(Environment.NewLine, chunks);
            file.Add(str);
        }
    }
    /// <summary>
    /// Indicates opening a block.
    /// </summary>
    public sealed class StatementOpenBlock : Statement
    {
        /// <summary>
        /// If async state should be ignored for this block.
        /// </summary>
        public bool ignoreAsync;
        /// <summary>
        /// Pointer to the closing block.
        /// </summary>
        public StatementCloseBlock closer;

        public int statementsInside;
        public int meaningfulStatementsInside;
        public override bool Skip => false;
        public override bool DoesAsyncSplit => false;

        /// <summary>
        /// The action when this opening block is called.
        /// </summary>
        public Action<Executor> openAction;
        /// <summary>
        /// The action when the closing block connected to this opener is called.
        /// </summary>
        public Action<Executor> CloseAction
        {
            get => this.closer?.closeAction;
            set => this.closer.closeAction = value;
        }

        public StatementOpenBlock(int statementsInside, CommandFile file) : base(null)
        {
            this.statementsInside = statementsInside;
            this.openAction = null;
            this.DecorateInSource = false;
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
            
        public override string ToString()
        {
            return this.meaningfulStatementsInside != this.statementsInside ?
                $"[OPEN BLOCK: {this.meaningfulStatementsInside} STATEMENTS ({this.statementsInside})]" :
                $"[OPEN BLOCK: {this.statementsInside} STATEMENTS]";
        }

        protected override TypePattern[] GetValidPatterns() => Array.Empty<TypePattern>();
        protected override void Run(Executor runningExecutor)
        {
            // start a new group of async stages
            if (!this.ignoreAsync && runningExecutor.async.IsInAsync)
            {
                runningExecutor.async.CurrentFunction.StartNewGroup();
                runningExecutor.async.CurrentFunction.StartNewStage();
            }
            
            this.openAction?.Invoke(runningExecutor);
            
            // track depth
            runningExecutor.depth++;
            if (runningExecutor.depth > Executor.MAXIMUM_DEPTH)
                throw new Exception($"Surpassed maximum depth ({Executor.MAXIMUM_DEPTH}). Use the compile option: --maxdepth <amount>");
        }
    }
    /// <summary>
    /// Closes a block.
    /// </summary>
    public sealed class StatementCloseBlock : Statement
    {
        public override bool Skip => false;
        public override bool DoesAsyncSplit => false;
        
        public StatementCloseBlock() : base(null)
        {
            this.closeAction = null;
            this.DecorateInSource = false;
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[CLOSE BLOCK]";
        }

        /// <summary>
        /// If async state should be ignored for this block.
        /// </summary>
        public bool ignoreAsync;
        /// <summary>
        /// Pointer to the opening block.
        /// </summary>
        public StatementOpenBlock opener;

        /// <summary>
        /// The action when the opening block connected to this closer is called.
        /// </summary>
        public Action<Executor> OpenAction
        {
            get => this.opener?.openAction;
            set => this.opener.openAction = value;
        }
        /// <summary>
        /// The action when this closing block is called.
        /// </summary>
        public Action<Executor> closeAction;

        protected override TypePattern[] GetValidPatterns() => Array.Empty<TypePattern>();
        protected override void Run(Executor runningExecutor)
        {
            // start a new group of async stages
            if (!this.ignoreAsync && runningExecutor.async.IsInAsync)
            {
                runningExecutor.async.CurrentFunction.FinishStageImmediate();
                runningExecutor.async.CurrentFunction.StartNewGroup();
                runningExecutor.async.CurrentFunction.StartNewStage();
            }

            this.closeAction?.Invoke(runningExecutor);

            // track depth
            runningExecutor.depth--;
            if (runningExecutor.depth < 0)
                throw new Exception("Bracket depth was less than 0, this is likely a bug and should be reported.");
        }
    }
    /// <summary>
    /// Statement that assigns a scoreboard value and may also perform a set of arithmetic.<br />
    /// Examples:<br />
    ///     a *= b + (c - d) * e<br />
    ///     a += b<br />
    ///     d = b + f<br />
    ///     a = x<br />
    /// </summary>
    public sealed class StatementOperation : Statement, IExecutionSetPart
    {
        public override bool Skip => false;
        public override bool DoesAsyncSplit => false;

        public StatementOperation(Token[] tokens) : base(tokens) {}
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[OPERATION] {string.Join(" ", from t in this.tokens select t.AsString())}";
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return new[] {
                new TypePattern(
                    new NamedType(typeof(TokenIdentifierValue)),
                    new NamedType(typeof(IAssignment))
                )
            };
        }
        protected override void Run(Executor runningExecutor)
        {
            var value = Next<TokenIdentifierValue>("left");
            var assignment = Next<IAssignment>("assignment operator");

            if (!this.HasNext)
                throw new StatementException(this, "Nothing on right-hand side of assignment.");

            if (NextIs<TokenIdentifierValue>(false))
            {
                var next = Next<TokenIdentifierValue>(null);
                CommandFile file = runningExecutor.CurrentFile;

                if (assignment is TokenArithmetic arithmetic)
                {
                    TokenArithmetic.Type op = arithmetic.GetArithmeticType();
                    
                    // switch on 'op' and perform operation
                    IEnumerable<string> commands = value.value.Operation
                        (next.value, op, this);
                    
                    runningExecutor.AddCommands(commands,
                        "math_op",
                        $"Math operation from {file.CommandReference} line {runningExecutor.NextLineNumber}. Performs ({value.value.Name} {arithmetic.AsString()} {next.value.Name}).");
                } else
                    runningExecutor.AddCommands(value.value.Assign(next.value, this),
                        "set_op",
                        $"Set operation from {file.CommandReference} line {runningExecutor.NextLineNumber}. Performs ({value.value.Name} = {next.value.Name}).");
            }
            else if (NextIs<TokenLiteral>(false))
            {
                var next = Next<TokenLiteral>(null);
                CommandFile file = runningExecutor.CurrentFile;

                if (assignment is TokenArithmetic arithmetic)
                {
                    
                    TokenArithmetic.Type op = arithmetic.GetArithmeticType();
                    var commands = new List<string>();

                    switch (op)
                    {
                        case TokenArithmetic.Type.ADD:
                            commands.AddRange(value.value.AddLiteral(next, this));
                            break;
                        case TokenArithmetic.Type.SUBTRACT:
                            commands.AddRange(value.value.SubtractLiteral(next, this));
                            break;
                        case TokenArithmetic.Type.MULTIPLY:
                        case TokenArithmetic.Type.DIVIDE:
                        case TokenArithmetic.Type.MODULO:
                        case TokenArithmetic.Type.SWAP:
                        default:
                        {
                            ScoreboardValue temp = runningExecutor.scoreboard.temps.RequestGlobal(next, this);
                            commands.AddRange(temp.AssignLiteral(next, this));
                            commands.AddRange(value.value.Operation(temp, op, this));
                            break;
                        }
                    }

                    runningExecutor.AddCommands(commands, "math_op", $"Math operation from {file.CommandReference} line {runningExecutor.NextLineNumber}. Performs ({value.value.Name} {arithmetic.AsString()} {next.AsString()}).");
                }
                else
                    runningExecutor.AddCommands(value.value.AssignLiteral
                        (next, this), "set_op", $"Set operation from {file.CommandReference} line {runningExecutor.NextLineNumber}. Performs ({value.value.Name} = {next.AsString()}).");
            }
            else
                throw new StatementException(this, $"Cannot assign variable to type \"{Peek().GetType().Name}\"");
        }
    }
    /// <summary>
    /// Statement that calls a function without using its return value.
    /// </summary>
    public sealed class StatementFunctionCall : Statement, IExecutionSetPart
    {
        public override bool Skip => false;
        public override bool DoesAsyncSplit => false;

        public StatementFunctionCall(Token[] tokens) : base(tokens) { }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[CALL FUNCTION {this.tokens[0]} WITH {this.tokens.Length - 3} PARAMETERS]";
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return new[] {
                new TypePattern(
                    new NamedType(typeof(TokenIdentifier)),
                    new NamedType(typeof(TokenOpenParenthesis))
                )
            };
        }
        protected override void Run(Executor runningExecutor)
        {
            if (!NextIs<TokenIdentifierFunction>(false))
            {
                var id = Next<TokenIdentifier>("function name");
                throw new StatementException(this, $"Unresolved function name \"{id.word}\". Is it spelled right & defined somewhere above this line?");
            }

            var value = Next<TokenIdentifierFunction>(null);

            if (NextIs<TokenOpenParenthesis>(false))
                Next();

            var _passIn = new List<Token>();
            int level = 1;
            while(this.HasNext)
            {
                Token nextToken = Next();

                if(nextToken is TokenCloseParenthesis)
                {
                    level--;
                    if (level < 1)
                        break;
                }
                if (nextToken is TokenOpenParenthesis)
                    level++;

                _passIn.Add(nextToken);
            }

            Token[] passIn = _passIn.ToArray();
            Function[] functions = value.functions;

            // these are already sorted by importance, so now just find the best match.
            Function bestFunction = null;
            int bestFunctionScore = int.MinValue;

            bool foundValidMatch = false;
            string lastError = null;

            foreach (Function function in functions)
            {
                if (!function.MatchParameters(passIn,
                    out lastError, out int score))
                {
                    // the last error is stored, so it will be shown if no valid function is found.
                    continue;
                }

                if (score <= bestFunctionScore)
                    continue;
                
                foundValidMatch = true;
                bestFunction = function;
                bestFunctionScore = score;
            }

            if (!foundValidMatch)
                throw new StatementException(this, lastError);

            var parameterCommands = new List<string>();
            var callCommands = new List<string>();
            
            if (bestFunction is RuntimeFunction)
            {
                _ = bestFunction.CallFunction(callCommands, passIn, runningExecutor, this);
                bestFunction.ProcessParameters(passIn, parameterCommands, runningExecutor, this);
            }
            else
            {
                bestFunction.ProcessParameters(passIn, parameterCommands, runningExecutor, this);
                _ = bestFunction.CallFunction(callCommands, passIn, runningExecutor, this);
            }

            
            // finish with the commands.
            CommandFile current = runningExecutor.CurrentFile;

            // register the call for usage tree
            if (bestFunction is RuntimeFunction runtime)
                current.RegisterCall(runtime.file);

            string[] commands = parameterCommands.Concat(callCommands).ToArray();
            
            runningExecutor.AddCommands(commands, "call" + bestFunction.Keyword.Replace('.', '_'),
                $"From file {current.CommandReference} line {runningExecutor.NextLineNumber}: {bestFunction.Keyword}({string.Join(", ", passIn.Select(t => t.AsString()))})");
        }
    }

    /// <summary>
    /// An unknown or unidentifiable operation. This performs no operation,
    /// it's only here to function as potential input for other statements.
    ///                                 (e.g. as struct fields)
    /// </summary>
    public sealed class StatementUnknown : Statement
    {
        public override bool Skip => true;
        public override bool DoesAsyncSplit => false;

        public StatementUnknown(Token[] tokens) : base(tokens)
        {
            this.DecorateInSource = false;
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[UNKNOWN] {string.Join(" ", from t in this.tokens select t.AsString())}";
        }

        public Token[] GetTokens() => this.tokens;

        protected override TypePattern[] GetValidPatterns() { return Array.Empty<TypePattern>(); } // always valid
        protected override void Run(Executor runningExecutor) { } // no operation
    }

    /// <summary>
    /// Identifies a statement which *can* be part of an execution set.
    /// </summary>
    public interface IExecutionSetPart { }
}