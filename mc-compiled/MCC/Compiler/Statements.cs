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

        public readonly int statementsInside;
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
        bool isResolved;

        TokenIdentifier aUnresovled;
        TokenIdentifierValue a = null;

        IAssignment assignmentOperator;

        Token[] tokensUnresolved;

        public StatementOperation(TokenIdentifier a, IAssignment assignment, Token[] tokens) : base(null)
        {
            isResolved = false;
            aUnresovled = a;
            assignmentOperator = assignment;
            tokensUnresolved = tokens;
        }
        /// <summary>
        /// Resolve all scoreboard identifiers.
        /// </summary>
        /// <param name="executor"></param>
        public void ResolveAll(Executor executor)
        {
            int length = tokensUnresolved.Length;
            tokens = new Token[length];

            for(int i = 0; i < length; i++)
            {
                Token token = tokensUnresolved[i];
                tokens[i] = token;
                if (!(token is TokenIdentifier))
                    continue;
                if (token is TokenIdentifierValue)
                    continue;

                string accessor = (token as TokenIdentifier).word;
                if(executor.scoreboard.TryGetByAccessor(accessor, out ScoreboardValue output)) {
                    tokens[i] = new TokenIdentifierValue(accessor, output, token.lineNumber);
                    continue;
                }
            }
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return new[] {
                new TypePattern(typeof(TokenIdentifier))
            };
        }
        protected override void Run(Executor executor)
        {
            throw new NotImplementedException();
        }
    }
}
