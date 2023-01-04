using mc_compiled.MCC.Functions;
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

        public StatementDirective(Directive directive, Token[] tokens) : base(tokens, true)
        {
            this.directive = directive;
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

            string str = executor.ResolveString(comment);
            executor.CurrentFile.Add("# " + str);
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
        }
        public override bool HasAttribute(DirectiveAttribute attribute) => false;
        public override string ToString()
        {
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
        public StatementCloseBlock() : base(null)
        {
            this.closeAction = null;
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
            string selector = executor.ActiveSelectorStr;
            TokenIdentifierValue value = Next<TokenIdentifierValue>();
            IAssignment assignment = Next<IAssignment>();

            if (!HasNext)
                throw new StatementException(this, "Nothing on right-hand side of assignment.");

            if (NextIs<TokenIdentifierValue>())
            {
                TokenIdentifierValue next = Next<TokenIdentifierValue>();
                if (assignment is TokenArithmatic)
                {
                    TokenArithmatic.Type op = (assignment as TokenArithmatic).GetArithmaticType();
                    executor.AddCommands(value.value.CommandsFromOperation
                        (selector, next.value, value.Accessor, next.Accessor, op), "math_op");
                } else
                    executor.AddCommands(value.value.CommandsSet
                        (selector, next.value, value.Accessor, next.Accessor), "set_op");
            }
            else if (NextIs<TokenLiteral>())
            {
                TokenLiteral next = Next<TokenLiteral>();
                if (assignment is TokenArithmatic)
                {
                    
                    TokenArithmatic.Type op = (assignment as TokenArithmatic).GetArithmaticType();
                    List<string> commands = new List<string>();

                    if (op == TokenArithmatic.Type.ADD)
                        commands.AddRange(value.value.CommandsAddLiteral(selector, next, value.Accessor, this));
                    else if (op == TokenArithmatic.Type.SUBTRACT)
                        commands.AddRange(value.value.CommandsSubLiteral(selector, next, value.Accessor, this));
                    else
                    {
                        ScoreboardValue temp = executor.scoreboard.RequestTemp(next, false, this);
                        commands.AddRange(temp.CommandsSetLiteral(value.Accessor, selector, next));
                        commands.AddRange(value.value.CommandsFromOperation(selector, temp, value.Accessor, temp.Name, op));
                    }

                    executor.AddCommands(commands, "mathoperation");
                    executor.scoreboard.ReleaseTemp();
                }
                else
                    executor.AddCommands(value.value.CommandsSetLiteral
                        (value.Accessor, selector, next), "setoperation");
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
            string selector = executor.ActiveSelectorStr;

            if (!NextIs<TokenIdentifierFunction>())
            {
                TokenIdentifier id = Next<TokenIdentifier>();
                throw new StatementException(this, $"Unresolved function name \"{id.word}\". Is it spelled right & defined somewhere above this line?");
            }

            TokenIdentifierFunction value = Next<TokenIdentifierFunction>();

            if (NextIs<TokenOpenParenthesis>())
                Next();

            List<Token> passIn = new List<Token>();
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

                passIn.Add(nextToken);
            }
            UserFunction function = value.function;

            executor.PushSelectorExecute();
            executor.AddCommands(function.CallFunction(selector, this,
                executor.scoreboard, passIn.ToArray()), "call" + value.function.name);
            executor.PopSelector();
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
        public StatementUnknown(Token[] tokens) : base(tokens) { }
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