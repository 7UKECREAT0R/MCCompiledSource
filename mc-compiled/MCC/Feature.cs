using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    [Flags]
    internal enum Feature : uint
    {
        NO_FEATURES = 0,     // No features enabled.

        NULLS = 1 << 0,     // Null Entities
        GAMETEST = 1 << 1,  // GameTest Framework
        EXPLODERS = 1 << 2, // Exploder Entities
        UNINSTALL = 1 << 3, // Uninstall Script
    }
}
