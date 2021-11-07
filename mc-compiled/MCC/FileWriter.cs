using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public struct FileWriter
    {
        public readonly string fileName;
        public readonly string fileFolder;
        public readonly List<string> lines;
        public StringBuilder addLineBuffer;


    }
}