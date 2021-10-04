using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A .mcfunction file waiting to be written.
    /// </summary>
    public class MCFunction
    {
        public const string EXT = ".mcfunction";
        public string fileName;
        public string fileOffset;
        public readonly string[] content;

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(fileOffset))
                    return $"{fileName}{EXT}";
                else
                    return $"{fileName}-{fileOffset}{EXT}";
            }
        }

        public MCFunction(string fileName, string fileOffset, List<string> lines)
        {
            this.fileName = fileName;
            this.fileOffset = fileOffset;
            content = lines.ToArray();
        }
        public void WriteFile(string folder)
        {
            string path;

            if (folder == null)
                path = FullName;
            else
                path = Path.Combine(folder, FullName);

            File.WriteAllLines(path, content);
        }
    }
}
