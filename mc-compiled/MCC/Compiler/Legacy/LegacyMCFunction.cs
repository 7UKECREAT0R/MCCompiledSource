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
    public class LegacyMCFunction
    {
        public const string EXT = ".mcfunction";
        public string fileName;
        public string fileFolder;
        public readonly string[] content;

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(fileFolder))
                    return $"{fileName}{EXT}";
                else
                    return Path.Combine(fileFolder, $"{fileName}{EXT}");
            }
        }

        public LegacyMCFunction(string fileName, string fileFolder, List<string> lines)
        {
            this.fileName = fileName;
            this.fileFolder = fileFolder;
            content = lines.ToArray();
        }
        public void WriteFile(string baseFolder)
        {
            // Create Directories if they don't exist.
            string completeDirectory;

            if(fileFolder != null)
                completeDirectory = Path.Combine(baseFolder, fileFolder);
            else
                completeDirectory = baseFolder;

            if (!Directory.Exists(completeDirectory))
                Directory.CreateDirectory(completeDirectory);

            // Write the file.
            string file = Path.Combine(baseFolder, FullName);
            File.WriteAllLines(file, content, Encoding.UTF8);
        }
    }
}
