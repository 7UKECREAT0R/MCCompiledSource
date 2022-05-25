using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Gives information about the result of a transformation.
    /// </summary>
    public struct TransformResult
    {
        public readonly Token[] tokensUsed;

        /// <summary>
        /// Represents if this transformation requires the positioning of the executor, rather than the individual entity.
        /// </summary>
        public readonly bool runAtExecutor;
        /// <summary>
        /// Represents if this transformation requires running at each selected entity indivudually.
        /// </summary>
        public readonly bool runAtSelf;
    }
}
