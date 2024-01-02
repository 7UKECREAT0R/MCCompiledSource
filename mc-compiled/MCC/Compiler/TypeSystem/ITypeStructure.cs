namespace mc_compiled.MCC.Compiler.TypeSystem
{
    /// <summary>
    /// Includes traits necessary on a type's data structure.
    /// </summary>
    public interface ITypeStructure
    {
        /// <summary>
        /// Return an identical copy of the data structure, but with all of its members properly cloned.
        /// </summary>
        /// <returns></returns>
        ITypeStructure DeepClone();
        /// <summary>
        /// Returns the hashcode for this data structure. Used to enforce hashcode implementation for these types.
        /// </summary>
        /// <returns></returns>
        int TypeHashCode();
    }
}
