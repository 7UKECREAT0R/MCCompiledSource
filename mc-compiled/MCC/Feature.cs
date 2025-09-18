using System;

namespace mc_compiled.MCC;

[Flags]
internal enum Feature : uint
{
    /// <summary>
    ///     No features enabled.
    /// </summary>
    NO_FEATURES = 0,
    /// <summary>
    ///     Enables the use of `dummy` entities.
    /// </summary>
    DUMMIES = 1 << 0,
    /// <summary>
    ///     Automatically calls the `init` function in-game when a change is made.
    /// </summary>
    AUTOINIT = 1 << 1,
    /// <summary>
    ///     Enables the use of the `explode` command.
    /// </summary>
    EXPLODERS = 1 << 2,
    /// <summary>
    ///     Automatic generation of an uninstallation script.
    /// </summary>
    UNINSTALL = 1 << 3,
    /// <summary>
    ///     Allows tests to be created and associated files to be generated.
    /// </summary>
    TESTS = 1 << 4,
    /// <summary>
    ///     Support for specifying real audio files within the `playsound` command.
    /// </summary>
    AUDIOFILES = 1 << 5,
    /// <summary>
    ///     Support for `scatter` and oversized `fill` commands.
    /// </summary>
    STRUCTURES = 1 << 6
}