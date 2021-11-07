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
        public string fileFolder;
        public readonly string[] content;

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(fileFolder))
                    return $"{fileName}{EXT}";
                else
                    return $"{fileFolder}{Path.DirectorySeparatorChar}{fileName}{EXT}";
            }
        }

        public MCFunction(string fileName, string fileFolder, List<string> lines)
        {
            this.fileName = fileName;
            this.fileFolder = fileFolder;
            content = lines.ToArray();
        }
        public void WriteFile(string folder)
        {
            string path, completeFolder;

            if (folder == null)
            {
                path = FullName;
                completeFolder = fileFolder;
            }
            else
            {
                path = Path.Combine(folder, FullName);
                completeFolder = Path.Combine(folder, fileFolder);
            }

            if(fileFolder != null && !Directory.Exists(completeFolder))
                Directory.CreateDirectory(completeFolder);

            File.WriteAllLines(path, content, Encoding.UTF8);
        }
    }
}
