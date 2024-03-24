namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Contains data about a block.
    /// </summary>
    public struct BlockMetadata
    {
        /// <summary>
        /// If this block runs only if a condition returns true, this is the condition required to run it.
        /// </summary>
        public ComparisonSet conditional;
        /// <summary>
        /// Returns if this block only runs if a condition is true.
        /// </summary>
        public bool IsConditional => this.conditional != null;
        
        /// <summary>
        /// Is this block part of an async function?
        /// </summary>
        public bool isAsync;
        /// <summary>
        /// Is this block part of a loop? i.e., <see cref="DirectiveImplementations.repeat"/>, <see cref="DirectiveImplementations.whileLoop"/>
        /// </summary>
        public bool isLoop;
        /// <summary>
        /// Does this block change the executing entity?
        /// </summary>
        public bool changesExecutingEntity;

        /// <summary>
        /// Contains data about a block.
        /// </summary>
        /// <param name="isAsync">Is this block part of an async function?</param>
        /// <param name="isLoop">Is this block part of a loop? i.e., <see cref="DirectiveImplementations.repeat"/>, <see cref="DirectiveImplementations.whileLoop"/></param>
        /// <param name="changesExecutingEntity">Does this block change the executing entity?</param>
        /// <param name="conditional">Does this block only run under a comparison?</param>
        public BlockMetadata(bool isAsync = false,
            bool isLoop = false,
            bool changesExecutingEntity = false,
            ComparisonSet conditional = null)
        {
            this.isAsync = isAsync;
            this.isLoop = isLoop;
            this.conditional = conditional;
            this.changesExecutingEntity = changesExecutingEntity;
        }
    }
}