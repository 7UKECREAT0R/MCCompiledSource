using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Native
{
    /// <summary>
    /// A list of BlockStates
    /// </summary>
    public class BlockStates: List<BlockState>
    {
        public override string ToString()
        {
            return $"[{string.Join(",", this.Select(b => b.ToString()))}]";
        }

        /// <summary>
        /// Adds a new BlockState to the list.
        /// </summary>
        /// <param name="fieldName">The name of the field in the BlockState object.</param>
        /// <param name="literal">The literal token to convert.</param>
        public void AddState(string fieldName, TokenLiteral literal)
        {
            Add(BlockState.FromLiteral(fieldName, literal));
        }
        /// <summary>
        /// Adds a new BlockState to the list.
        /// </summary>
        /// <param name="fieldName">The name of the field in the BlockState object.</param>
        /// <param name="valueAsBoolean">The boolean value to hold in the BlockState.</param>
        public void AddState(string fieldName, bool valueAsBoolean)
        {
            Add(new BlockState(fieldName, valueAsBoolean));
        }
        /// <summary>
        /// Adds a new BlockState to the list.
        /// </summary>
        /// <param name="fieldName">The name of the field in the BlockState object.</param>
        /// <param name="valueAsInteger">The integer value to hold in the BlockState.</param>
        public void AddState(string fieldName, int valueAsInteger)
        {
            Add(new BlockState(fieldName, valueAsInteger));
        }
        /// <summary>
        /// Adds a new BlockState to the list.
        /// </summary>
        /// <param name="fieldName">The name of the field in the BlockState object.</param>
        /// <param name="valueAsEnum">The string value to hold in the BlockState.</param>
        public void AddState(string fieldName, string valueAsEnum)
        {
            Add(new BlockState(fieldName, valueAsEnum));
        }
    }
}