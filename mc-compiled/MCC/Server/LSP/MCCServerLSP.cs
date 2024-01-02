using LanguageServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Server.LSP
{
    internal class MCCServerLSP : ServiceConnection
    {
        public MCCServerLSP(Stream input, Stream output) : base(input, output)
        {

        }
    }
}
