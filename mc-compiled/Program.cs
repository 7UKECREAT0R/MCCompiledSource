using System;
using mc_compiled.CLI;

// ReSharper disable CommentTypo

namespace mc_compiled;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args) { CommandLineManager.Run(args); }
}