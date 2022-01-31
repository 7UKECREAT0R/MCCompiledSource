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
    /// Performs a multiple level order-of-operations math statement on scoreboard values.
    /// </summary>
    public sealed class StatementMath : Statement
    {
        ScoreboardValue finalResult;

        public StatementMath(ScoreboardValue finalResult, Token[] tokens) : base(tokens)
        {
            this.finalResult = finalResult;
        }

        protected override TypePattern[] GetValidPatterns()
        {
            return new[] {
                new TypePattern(typeof(CompoundAssignment))
            };
        }
        protected override void Run(Executor executor)
        {
            throw new NotImplementedException();
        }
    }
}
