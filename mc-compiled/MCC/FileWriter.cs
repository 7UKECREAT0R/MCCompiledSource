using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public class FileWriter
    {
        public readonly string fileName;
        public readonly string fileFolder;
        public readonly List<string> lines;
        public StringBuilder addLineBuffer;

        public FileWriter(string name, string folder = null)
        {
            fileName = name;
            fileFolder = folder;
            lines = new List<string>();
            addLineBuffer = new StringBuilder();
        }

        /// <summary>
        /// Insert a line at a specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="line"></param>
        public void InsertLine(int index, string line)
        {
            lines.Insert(index, line);
        }

        /// <summary>
        /// Write the contents of the buffer into a line
        /// and clear it. Can optionally specify to keep
        /// the buffer or append an additional string.
        /// </summary>
        /// <param name="end">The string to concat to the end. Null by default.</param>
        /// <param name="modify">Whether to wipe the buffer at the end or preserve its contents.</param>
        public void ApplyBuffer(string end = null, bool modify = true)
        {
            // Write line
            if(end == null)
                lines.Add(addLineBuffer.ToString());
            else
                lines.Add(end + addLineBuffer.ToString());

            // Wipe buffer
            if (modify)
                addLineBuffer.Clear();
        }
        /// <summary>
        /// Set the buffer. This will be prepended to the next ApplyBuffer() call.
        /// </summary>
        /// <param name="text">The text to set the buffer to to.</param>
        public void SetBuffer(string text)
        {
            addLineBuffer.Clear();
            addLineBuffer.Append(text);
        }

        /// <summary>
        /// Finalize this writer by converting it to an MCFunction.
        /// </summary>
        /// <returns></returns>
        public MCFunction Finalize()
        {
            return new MCFunction(fileName, fileFolder, lines);
        }
    }
}