using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    /// <summary>
    /// States that this class can be output to a behavior file.
    /// </summary>
    public interface IBehaviorFile
    {
        /// <summary>
        /// Get the directory that this output file will go in.
        /// </summary>
        /// <returns></returns>
        string GetOutputDirectory();
        /// <summary>
        /// Get the file name and extension for this output.
        /// </summary>
        /// <returns></returns>
        string GetOutputFile();

        /// <summary>
        /// Get the data to write into the file.
        /// </summary>
        /// <returns></returns>
        byte[] GetOutputData();

        /// <summary>
        /// Indicates base location of the output file.
        /// </summary>
        /// <returns>The output location of the file.</returns>
        OutputLocation GetOutputRoot();
    }
    public enum OutputLocation
    {
        BEHAVIORS,
        RESOURCES
    }
}
