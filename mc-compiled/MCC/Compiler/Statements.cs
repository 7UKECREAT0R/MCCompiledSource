﻿using mc_compiled.MCC.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    public sealed class StatementDirective : Statement, IExecutionSetPart
    {
        public readonly Directive directive;
        public override bool Skip => false;

        public StatementDirective(Directive directive, Token[] tokens) : base(tokens, true)
        {
            this.directive = directive;
            this.DecorateInSource = !directive.IsPreprocessor;
        }
        public override bool HasAttribute(DirectiveAttribute attribute)
        {
            if (directive == null)
                return false;
            return (directive.attributes & attribute) != 0;
        }
        public override string ToString()
        {
            if (directive == null)
                return $"[DIRECTIVE] [PARSING ERROR]";

            return $"[DIRECTIVE] {directive.description} -> {string.Join(" ", from t in tokens select t.DebugString())}";
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return directive.patterns;
        }
        protected override void Run(Executor executor)
        {
            directive.call(executor, this);
        }
    }
    public sealed class StatementComment : Statement
    {
        public readonly string comment;
        public override bool Skip => true;

        public StatementComment(string comment) : base(new Token[0], true)
        {
            this.comment = comment;
            DecorateInSource = false;
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[COMMENT] {comment}";
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return new TypePattern[0];
        }
        protected override void Run(Executor executor)
        {
            if (!Program.DECORATE)
                return;

            // peek at the next to determine if this comment will be used as documentation or not.
            Statement next = executor.Peek();
            if(next is StatementDirective sd)
            {
                if (sd.HasAttribute(DirectiveAttribute.DOCUMENTABLE))
                    return; // this is a documentation string.
            }

            string str = executor.ResolveString(comment);

            // find whether to add a newline or not
            CommandFile file = executor.CurrentFile;
            int length = file.Length;

            if (length > 0)
            {
                // get last line
                string lastLine = file.commands[length - 1];
                if (!lastLine.StartsWith("#")) // create newline if the last line was not a comment
                    file.Add("");
            }

            var chunks = str.Trim().Split('\n').Select(line => "# " + line.Trim());
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
        /// Pointer to the closing block.
        /// </summary>
        public StatementCloseBlock closer;

        public int statementsInside;
        public int meaningfulStatementsInside;
        public override bool Skip => false;

        /// <summary>
        /// The action when this opening block is called.
        /// </summary>
        public Action<Executor> openAction;
        /// <summary>
        /// The action when the closing block connected to this opener is called.
        /// </summary>
        public Action<Executor> CloseAction
        {
            get => closer?.closeAction;
            set {
                if (closer != null)
                    closer.closeAction = value;
            }
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
            if(meaningfulStatementsInside != statementsInside)
                return $"[OPEN BLOCK: {meaningfulStatementsInside} STATEMENTS ({statementsInside})]";
            else
                return $"[OPEN BLOCK: {statementsInside} STATEMENTS]";
        }

        protected override TypePattern[] GetValidPatterns()
            => new TypePattern[0];
        protected override void Run(Executor executor)
        {
            if (openAction != null)
                openAction(executor);
            executor.depth++;

            if (executor.depth > Executor.MAXIMUM_DEPTH)
                throw new Exception($"Surpassed maximum depth ({Executor.MAXIMUM_DEPTH}). Use the compile option: --maxdepth <amount>");
        }
    }
    /// <summary>
    /// Closes a block.
    /// </summary>
    public sealed class StatementCloseBlock : Statement
    {
        public override bool Skip => false;
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
        /// Pointer to the opening block.
        /// </summary>
        public StatementOpenBlock opener;

        /// <summary>
        /// The action when the opening block connected to this closer is called.
        /// </summary>
        public Action<Executor> OpenAction
        {
            get => opener?.openAction;
            set { if(opener != null) opener.openAction = value; }
        }
        /// <summary>
        /// The action when this closing block is called.
        /// </summary>
        public Action<Executor> closeAction;

        protected override TypePattern[] GetValidPatterns()
            => new TypePattern[0];
        protected override void Run(Executor executor)
        {
            if (closeAction != null)
                closeAction(executor);

            executor.depth--;

            if (executor.depth < 0)
                throw new Exception("Bracket depth was less than 0, this is likely a bug and should be reported.");
        }
    }
    /// <summary>
    /// Statement that assigns a scoreboard value and may also perform a set of arithmatic.<br />
    /// Examples:<br />
    ///     a *= b + (c - d) * e<br />
    ///     a += b<br />
    ///     d = b + f<br />
    ///     a = x<br />
    /// </summary>
    public sealed class StatementOperation : Statement, IExecutionSetPart
    {
        public override bool Skip => false;
        public StatementOperation(Token[] tokens) : base(tokens) {}
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[OPERATION] {string.Join(" ", from t in tokens select t.AsString())}";
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
        protected override void Run(Executor executor)
        {
            TokenIdentifierValue value = Next<TokenIdentifierValue>();
            IAssignment assignment = Next<IAssignment>();

            if (!HasNext)
                throw new StatementException(this, "Nothing on right-hand side of assignment.");

            if (NextIs<TokenIdentifierValue>())
            {
                TokenIdentifierValue next = Next<TokenIdentifierValue>();
                CommandFile file = executor.CurrentFile;

                if (assignment is TokenArithmatic arithmatic)
                {
                    TokenArithmatic.Type op = arithmatic.GetArithmaticType();
                    executor.AddCommands(value.value.CommandsFromOperation
                        (next.value, op), "math_op", $"Math operation from {file.CommandReference} line {executor.NextLineNumber}. Performs ({value.value.AliasName} {arithmatic.AsString()} {next.value.AliasName}).");
                } else
                    executor.AddCommands(value.value.CommandsSet
                        (next.value), "set_op", $"Set operation from {file.CommandReference} line {executor.NextLineNumber}. Performs ({value.value.AliasName} = {next.value.AliasName}).");
            }
            else if (NextIs<TokenLiteral>())
            {
                TokenLiteral next = Next<TokenLiteral>();
                CommandFile file = executor.CurrentFile;

                if (assignment is TokenArithmatic arithmatic)
                {
                    
                    TokenArithmatic.Type op = arithmatic.GetArithmaticType();
                    List<string> commands = new List<string>();

                    ScoreboardManager.ValueType type = ScoreboardManager.ValueType.INT;

                    if (op == TokenArithmatic.Type.ADD)
                        commands.AddRange(value.value.CommandsAddLiteral(next, this));
                    else if (op == TokenArithmatic.Type.SUBTRACT)
                        commands.AddRange(value.value.CommandsSubLiteral(next, this));
                    else
                    {
                        ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(next, this);
                        type = temp.valueType;
                        commands.AddRange(temp.CommandsSetLiteral(next));
                        commands.AddRange(value.value.CommandsFromOperation(temp, op));
                        executor.scoreboard.temps.ReleaseGlobal(type);
                    }

                    executor.AddCommands(commands, "math_op", $"Math operation from {file.CommandReference} line {executor.NextLineNumber}. Performs ({value.value.AliasName} {arithmatic.AsString()} {next.AsString()}).");
                }
                else
                    executor.AddCommands(value.value.CommandsSetLiteral
                        (next), "set_op", $"Set operation from {file.CommandReference} line {executor.NextLineNumber}. Performs ({value.value.AliasName} = {next.AsString()}).");
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
        public StatementFunctionCall(Token[] tokens) : base(tokens) { }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[CALL FUNCTION {tokens[0]} WITH {tokens.Length - 3} PARAMETERS]";
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
        protected override void Run(Executor executor)
        {
            if (!NextIs<TokenIdentifierFunction>())
            {
                TokenIdentifier id = Next<TokenIdentifier>();
                throw new StatementException(this, $"Unresolved function name \"{id.word}\". Is it spelled right & defined somewhere above this line?");
            }

            TokenIdentifierFunction value = Next<TokenIdentifierFunction>();

            if (NextIs<TokenOpenParenthesis>())
                Next();

            List<Token> _passIn = new List<Token>();
            int level = 1;
            while(HasNext)
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

                if (score > bestFunctionScore)
                {
                    foundValidMatch = true;
                    bestFunction = function;
                    bestFunctionScore = score;
                }
            }

            if (!foundValidMatch)
                throw new StatementException(this, lastError);

            List<string> commands = new List<string>();

            // process the parameters and get their commands.
            bestFunction.ProcessParameters(passIn, commands, executor, this);

            // call the function.
            Token replacement = bestFunction.CallFunction(commands, executor, this);

            // finish with the commands.
            CommandFile current = executor.CurrentFile;
            executor.AddCommands(commands, "call" + bestFunction.Keyword,
                $"From file {current.CommandReference} line {executor.NextLineNumber}: {bestFunction.Keyword}({string.Join(", ", passIn.Select(t => t.AsString()))})");
            commands.Clear();

            return;
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
        public StatementUnknown(Token[] tokens) : base(tokens)
        {
            this.DecorateInSource = false;
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
            return $"[UNKNOWN] {string.Join(" ", from t in tokens select t.AsString())}";
        }

        public Token[] GetTokens() => tokens;

        protected override TypePattern[] GetValidPatterns() { return new TypePattern[0]; } // always valid
        protected override void Run(Executor executor) { } // no operation
    }

    /// <summary>
    /// Identifies a statement which *can* be part of an execution set.
    /// </summary>
    public interface IExecutionSetPart { }
}