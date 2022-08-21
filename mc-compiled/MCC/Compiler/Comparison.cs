using mc_compiled.Commands.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A set of comparisons.
    /// </summary>
    public class ComparisonSet : List<Comparison>
    {
        public void InvertAll(bool invert)
        {
            foreach (Comparison item in this)
                item.SetInversion(invert);
        }
        
        /// <summary>
        /// Get the setup commands for this entire comparison set.
        /// </summary>
        /// <param name="executor">The executor driving this call.</param>
        /// <returns></returns>
        public List<string> GetCommands(Executor executor)
        {
            List<string> commands = new List<string>();
            foreach (Comparison item in this)
            {
                IEnumerable<string> setupCommands = item.GetCommands(executor);
                if(setupCommands != null)
                    commands.AddRange(setupCommands);
            }

            return commands;
        }
        /// <summary>
        /// Transforms a selector for the entire comparison set.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="callingStatement"></param>
        public IEnumerable<Selector> GetSelectors(Executor executor, Statement callingStatement)
        {
            List<Selector> selectors = new List<Selector>();
            foreach (Comparison item in this)
            {
                Selector selector = item.GetSelector(executor, callingStatement);
                if (selector != null)
                    selectors.Add(selector);
            }

            return selectors;
        }
    }

    /// <summary>
    /// Represents a generic comparison in an if-statement.
    /// </summary>
    public abstract class Comparison
    {
        readonly bool originallyInverted;

        public Comparison(bool invert)
        {
            originallyInverted = invert;
            inverted = invert;
        }

        /// <summary>
        /// If this comparison is inverted.
        /// </summary>
        public bool inverted;
        /// <summary>
        /// Toggles the inversion of this comparison.
        /// </summary>
        public void SetInversion(bool invert) => inverted = invert ? !originallyInverted : originallyInverted;

        /// <summary>
        /// Get the commands needed, if any, to set up this comparison. May return null if no commands are needed.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetCommands(Executor executor);
        /// <summary>
        /// Gets the selector for this stage of the if-statement.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="callingStatement"></param>
        public abstract Selector GetSelector(Executor executor, Statement callingStatement);
    }
}
