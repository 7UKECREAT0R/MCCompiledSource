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

        protected override TypePattern[] GetValidPatterns() => directive.patterns;
        protected override void Run(Executor executor)
        {
            directive.call(executor, this);
        }
    }
    /// <summary>
    /// Indicates opening a block.
    /// </summary>
    public sealed class StatementBlock : Statement
    {
        public readonly int statementsInside;
        public readonly CommandFile file;

        public StatementBlock(int toSkip, CommandFile file) : base(null)
        {
            this.statementsInside = toSkip;
            this.file = file;
        }

        protected override TypePattern[] GetValidPatterns()
            => new TypePattern[0];
        protected override void Run(Executor executor)
        {
            // no action
        }
    }
}
