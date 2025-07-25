using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC;

/// <summary>
///     definitions.defs file parser and manager.
/// </summary>
public partial class Definitions
{
    private const string FILE = "definitions.def";
    public static Definitions GLOBAL_DEFS;
    private static readonly Regex DEF_REGEX = DefinitionRegex();

    internal readonly Dictionary<string, string> defs;

    private Definitions(bool debugInfo)
    {
        this.defs = new Dictionary<string, string>();
        string applicationBaseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
        if (applicationBaseDirectory == null)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(
                "WARNING: Couldn't figure out where the application is located. Expect everything to blow up.");
            Console.ForegroundColor = previousColor;
            return;
        }

        string path = Path.Combine(applicationBaseDirectory, FILE);
        if (!File.Exists(path))
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("WARNING: Missing definitions file at '{0}'. Expect everything to blow up.", path);
            Console.ForegroundColor = previousColor;
            return;
        }

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        string category = null;
        int catEntries = 0;
        string[] categoryAliases = [];

        var previous = ConsoleColor.White;
        if (debugInfo)
        {
            previous = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
        }

        foreach (string _line in lines)
        {
            string line = _line.Trim();
            if (line.StartsWith("CATEGORY"))
            {
                string input = line[9..].ToUpper();

                if (debugInfo)
                {
                    Console.WriteLine("Define: Category {0} with {1} entries and {2} alias(es).", category, catEntries,
                        categoryAliases.Length);
                    catEntries = 0;
                }

                if (input.Contains(" AND "))
                {
                    string[] aliases = input.Split([" AND "], StringSplitOptions.RemoveEmptyEntries);
                    category = aliases[0];
                    categoryAliases = new string[aliases.Length - 1];
                    for (int i = 0; i < categoryAliases.Length; i++)
                        categoryAliases[i] = aliases[i + 1].Trim();
                }
                else
                {
                    categoryAliases = [];
                    category = input;
                }

                continue;
            }

            if (line.StartsWith("VERSION"))
            {
                Executor.MINECRAFT_VERSION = line[8..];
                continue;
            }

            string[] assignParts = line.Split([" IS "],
                StringSplitOptions.RemoveEmptyEntries);

            if (assignParts.Length != 2)
                continue;
            if (category == null)
                continue;

            string name = assignParts[0].Trim().ToUpper();
            string value = assignParts[1].Trim();

            string key = BuildKey(category, name);
            this.defs[key] = value;

            if (debugInfo)
                catEntries++;

            foreach (string alias in categoryAliases)
            {
                key = BuildKey(alias, name);
                this.defs[key] = value;
            }
        }

        if (debugInfo)
            Console.ForegroundColor = previous;
    }

    /// <summary>
    ///     Initialize the <see cref="GLOBAL_DEFS" /> singleton if it's not yet initialized.
    /// </summary>
    /// <param name="debugInfo"></param>
    public static void TryInitialize(bool debugInfo)
    {
        if (GLOBAL_DEFS != null)
            return;
        GLOBAL_DEFS = new Definitions(debugInfo);
    }

    private static string BuildKey(string category, string query) { return (category + ':' + query).ToUpper(); }

    /// <summary>
    ///     Replace all definition queries with their resulting values.
    ///     <ul>
    ///         <li>[color: RED] -> <c>§c</c></li>
    ///         <li>[wool: blue] -> <c>11</c></li>
    ///         <li>[misc: below] -> <c>~ ~-1 ~</c></li>
    ///         <li>[slot: Main Hand] -> <c>slot.weapon.mainhand</c></li>
    ///         <li>[color: reset, bold, red] -> <c>§r§l§c</c></li>
    ///     </ul>
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns></returns>
    public string ReplaceDefinitions(string input)
    {
        MatchCollection matches = DEF_REGEX.Matches(input);
        if (matches.Count == 0)
            return input;

        var result = new StringBuilder(input.Length); // the result will likely be shorter/same as the input
        int lastIndex = 0;

        foreach (Match match in matches)
        {
            result.Append(input, lastIndex, match.Index - lastIndex); // append everything before the match

            int numberOfBackslashes = match.Groups[1].Value.Length;
            bool escaped = numberOfBackslashes % 2 == 1;
            result.Append('\\', numberOfBackslashes / 2);

            if (escaped)
            {
                result.Append(match.Value[(numberOfBackslashes / 2)..]);
            }
            else
            {
                // this is a definition string
                string category = match.Groups[2].Value;
                string fullQuery = match.Groups[3].Value;

                if (TryResolveDefinitions(category, fullQuery, out string replacement))
                    result.Append(replacement);
                else
                    result.Append(match.Value);
            }

            lastIndex = match.Index + match.Length;
        }

        // everything else
        result.Append(input, lastIndex, input.Length - lastIndex);
        return result.ToString();
    }
    private bool TryResolveDefinitions(string category, string _fullQuery, out string result)
    {
        result = null;
        var sb = new StringBuilder();

        ReadOnlySpan<char> fullQuery = _fullQuery.AsSpan();

        while (!fullQuery.IsEmpty)
        {
            int comma = fullQuery.IndexOf(',');
            ReadOnlySpan<char> currentQuery = comma != -1 ? fullQuery[..comma].Trim() : fullQuery.Trim();

            string key = BuildKey(category, currentQuery.ToString());
            if (!this.defs.TryGetValue(key, out string value))
                return false; // any failure means this is should act as a no-op

            sb.Append(value);
            if (comma == -1)
                break;
            fullQuery = fullQuery[(comma + 1)..];
        }

        result = sb.ToString();
        return true;
    }

    [GeneratedRegex(@"(\\*)\[([\w ]+):\s*([\w ,]+)\]")]
    private static partial Regex DefinitionRegex();
}