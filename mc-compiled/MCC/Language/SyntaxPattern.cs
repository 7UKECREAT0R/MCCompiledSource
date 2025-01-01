using System.Collections.Generic;

namespace mc_compiled.MCC.Language;

/// <summary>
///     A list of <see cref="SyntaxParameter" /> arrays with which one can match a list of tokens.
/// </summary>
public class SyntaxPatterns : List<SyntaxParameter[]>
{
    /// <summary>
    ///     Create a new empty <see cref="SyntaxPatterns" />
    /// </summary>
    public SyntaxPatterns() { }
    /// <summary>
    ///     Create a new <see cref="SyntaxPatterns" /> with one valid pattern.
    /// </summary>
    /// <param name="parameters"></param>
    public SyntaxPatterns(params SyntaxParameter[] parameters)
    {
        Add(parameters);
    }
    /// <summary>
    ///     Create a new <see cref="SyntaxPatterns" /> with multiple valid patterns that can match it.
    /// </summary>
    /// <param name="parameters"></param>
    public SyntaxPatterns(IEnumerable<SyntaxParameter[]> parameters) :
        base(parameters) { }

    /// <summary>
    ///     Attempts to parse a single string array into a <see cref="SyntaxPatterns" /> object with a single pattern inside.
    /// </summary>
    /// <param name="parameters">
    ///     The input array of string arrays, where each string represents a single parameter to be
    ///     parsed.
    /// </param>
    /// <param name="patterns">
    ///     When the method returns, contains the resulting <see cref="SyntaxPatterns" /> object if parsing
    ///     was successful, or null if parsing failed.
    /// </param>
    /// <returns>True if the parsing was successful and the <see cref="SyntaxPatterns" /> object was created; otherwise, false.</returns>
    public static bool TryParse(string[] parameters, out SyntaxPatterns patterns)
    {
        List<SyntaxParameter> first = [];

        foreach (string parameter in parameters)
            if (SyntaxParameter.TryParse(parameter, out SyntaxParameter parsed))
            {
                first.Add(parsed);
            }
            else
            {
                patterns = null;
                return false;
            }

        patterns = new SyntaxPatterns(first.ToArray());
        return true;
    }
    /// <summary>
    ///     Attempts to parse an array of string arrays into a <see cref="SyntaxPatterns" /> object.
    /// </summary>
    /// <param name="patterns">The input array of string arrays, where each inner array represents a pattern to be parsed.</param>
    /// <param name="parsed">
    ///     When the method returns, contains the resulting <see cref="SyntaxPatterns" /> object if parsing
    ///     was successful, or null if parsing failed.
    /// </param>
    /// <returns>True if the parsing was successful and the <see cref="SyntaxPatterns" /> object was created; otherwise, false.</returns>
    public static bool TryParse(string[][] patterns, out SyntaxPatterns parsed)
    {
        var parsedPatterns = new SyntaxParameter[patterns.Length][];

        for (int i = 0; i < patterns.Length; i++)
        {
            string[] pattern = patterns[i];
            var parsedPattern = new SyntaxParameter[pattern.Length];
            for (int j = 0; j < pattern.Length; j++)
            {
                string parameter = pattern[j];
                if (SyntaxParameter.TryParse(parameter, out SyntaxParameter parsedParameter))
                {
                    parsedPattern[j] = parsedParameter;
                }
                else
                {
                    parsed = null;
                    return false;
                }
            }

            parsedPatterns[i] = parsedPattern;
        }

        parsed = new SyntaxPatterns(parsedPatterns);
        return true;
    }
}