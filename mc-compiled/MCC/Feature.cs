using System;

namespace mc_compiled.MCC
{
    [Flags]
    internal enum Feature : uint
    {
        NO_FEATURES = 0,    // No features enabled.

        DUMMIES = 1 << 0,     // Dummy Entities
        GAMETEST = 1 << 1,  // GameTest Framework
        EXPLODERS = 1 << 2,   // Exploder Entities
        UNINSTALL = 1 << 3, // Uninstall Script
    }
}
