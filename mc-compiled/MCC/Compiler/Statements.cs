using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    public sealed class StatementDirective : Statement
    {
        public readonly Directive directive;

        public StatementDirective(Directive directive, Token[] tokens) : base(tokens)
        {
            this.directive = directive;
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
    /// <summary>
    /// Indicates opening a block.
    /// </summary>
    public sealed class StatementOpenBlock : Statement
    {
        private static int branchIndex;
        public static CommandFile GetNextBranchFile() =>
            new CommandFile("branch" + (branchIndex++), "_branching");

        public int statementsInside;
        public bool shouldRun = false;
        public bool aligns = false;
        private CommandFile file;

        public StatementOpenBlock(int statementsInside, CommandFile file) : base(null)
        {
            this.statementsInside = statementsInside;
            this.file = file;
        }
        public bool HasTargetFile
        {
            get => file != null;
        }
        public CommandFile TargetFile
        {
            set => file = value;
        }


        protected override TypePattern[] GetValidPatterns()
            => new TypePattern[0];
        protected override void Run(Executor executor)
        {
            // get the closer and tell it whether to pop file or not.
            StatementCloseBlock closer = executor.Peek<StatementCloseBlock>(statementsInside);

            if (shouldRun)
            {
                closer.popFile = file != null;
                if (file != null)
                    executor.PushFile(file);
                executor.PushSelector(aligns); // push a level up
            } else
            {
                closer.popFile = false;
                for (int i = 0; i < statementsInside; i++)
                    executor.Next();
            }
        }
    }
    /// <summary>
    /// Closes a block.
    /// </summary>
    public sealed class StatementCloseBlock : Statement
    {
        public bool popFile;
        public StatementCloseBlock() : base(null)
        {
            this.popFile = false;
        }

        protected override TypePattern[] GetValidPatterns()
            => new TypePattern[0];
        protected override void Run(Executor executor)
        {
            if (popFile)
                executor.PopFile();

            executor.PopSelector();
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
    public sealed class StatementOperation : Statement
    {
        public StatementOperation(Token[] tokens) : base(tokens) {}

        protected override TypePattern[] GetValidPatterns()
        {
            return new[] {
                new TypePattern(typeof(TokenIdentifierValue), typeof(IAssignment))
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
                        (selector, next.value, value.Accessor, next.Accessor, op));
                } else
                    executor.AddCommands(value.value.CommandsSet
                        (selector, next.value, value.Accessor, next.Accessor));
            }
            else if (NextIs<TokenLiteral>())
            {
                TokenLiteral next = Next<TokenLiteral>();
                if (assignment is TokenArithmatic)
                {
                    ScoreboardValue temp = executor.scoreboard.RequestTemp(next, this);
                    TokenArithmatic.Type op = (assignment as TokenArithmatic).GetArithmaticType();
                    executor.AddCommands(value.value.CommandsFromOperation
                        (selector, temp, value.Accessor, temp.baseName, op));
                    executor.scoreboard.ReleaseTemp();
                }
                else
                    executor.AddCommands(value.value.CommandsSetLiteral
                        (value.Accessor, selector, next));
            }
            else
                throw new StatementException(this, $"Cannot assign variable to type \"{Peek().GetType()}\"");
        }
    }
    /// <summary>
    /// Statement that calls a function without using its return value.
    /// </summary>
    public sealed class StatementFunctionCall : Statement
    {
        public StatementFunctionCall(Token[] tokens) : base(tokens) { }

        protected override TypePattern[] GetValidPatterns()
        {
            return new[] {
                new TypePattern(typeof(TokenIdentifierFunction), typeof(TokenOpenParenthesis))
            };
        }
        protected override void Run(Executor executor)
        {
            string selector = executor.ActiveSelectorStr;
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
                {
                    level++;
                }

                passIn.Add(nextToken);
            }
        }
    }
}
