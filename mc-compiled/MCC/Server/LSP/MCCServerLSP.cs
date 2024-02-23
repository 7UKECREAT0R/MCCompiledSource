using LanguageServer;
using System.IO;

namespace mc_compiled.MCC.Server.LSP
{
    internal class MCCServerLSP : ServiceConnection
    {
        public MCCServerLSP(Stream input, Stream output) : base(input, output)
        {

        }
    }
}
