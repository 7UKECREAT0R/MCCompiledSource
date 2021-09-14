using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    public static class CommandLimits
    {
        public static readonly char[] SCOREBOARD_ALLOWED = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM-:._".ToCharArray();
        public const int SCOREBOARD_LIMIT = 16;
    }
}
