using System;

namespace mc_compiled.MCC
{
    [Flags]
    internal enum Feature : uint
    {
        NO_FEATURES = 0,    // No features enabled.

        DUMMIES = 1 << 0,       // Dummy Entities
        AUTOINIT = 1 << 1,      // Automatic Init Call
        EXPLODERS = 1 << 2,     // Exploder Entities
        UNINSTALL = 1 << 3,     // Uninstall Script,
        TESTS = 1 << 4,         // Automated Tests
    }
}
