using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Language.SyntaxExporter;

/// <summary>
///     Handles exporting of language information to various file types.
/// </summary>
public static class SyntaxExporters
{
    private static readonly SyntaxExporter[] ALL_EXPORTERS = [];
    private static readonly Dictionary<string, SyntaxExporter> EXPORTERS_BY_IDENTIFIER =
        new(StringComparer.OrdinalIgnoreCase);

    static SyntaxExporters()
    {
        // load if it's not already loaded.
        Language.TryLoad();

        foreach (SyntaxExporter exporter in ALL_EXPORTERS)
        {
            string identifier = exporter.Identifier;
            EXPORTERS_BY_IDENTIFIER[identifier] = exporter;
        }
    }

    /// <summary>
    ///     A read-only set of all registered exporters.
    /// </summary>
    public static IReadOnlyCollection<SyntaxExporter> AllExporters => ALL_EXPORTERS;
    /// <summary>
    ///     Gets an exporter by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier to look up.</param>
    /// <returns><c>null</c> if the exporter couldn't be found.</returns>
    public static SyntaxExporter GetExporter(string identifier)
    {
        return EXPORTERS_BY_IDENTIFIER.GetValueOrDefault(identifier);
    }
    /// <summary>
    ///     Attempts to get an exporter by the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier to look up.</param>
    /// <param name="exporter">If the method returns <c>true</c>, the exporter that was retrieved.</param>
    /// <returns><c>true</c> if an exporter was found, <c>false</c> otherwise.</returns>
    public static bool TryGetExporter(string identifier, out SyntaxExporter exporter)
    {
        return EXPORTERS_BY_IDENTIFIER.TryGetValue(identifier, out exporter);
    }
}