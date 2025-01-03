﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC;

/// <summary>
///     definitions.defs file parser and manager.
/// </summary>
public class Definitions
{
    private const string FILE = "definitions.def";
    public static Definitions GLOBAL_DEFS;
    private static readonly Regex DEF_REGEX = new(@"(\\*)\[([\w ]+):\s*([\w ,]+)\]");

    internal readonly Dictionary<string, string> defs;
    /// <summary>
    ///     Initializes and loads a definitions instance, also setting GLOBAL_DEFS to it.
    /// </summary>
    /// <param name="debugInfo"></param>
    public Definitions(bool debugInfo)
    {
        this.defs = new Dictionary<string, string>();
        string assemblyDir = Path.GetDirectoryName(AppContext.BaseDirectory);
        string path = Path.Combine(assemblyDir, FILE);
        if (!File.Exists(path))
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WARNING: Missing definitions file at '{0}'. Expect everything to blow up.", path);
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

        GLOBAL_DEFS = this;
    }

    private static string BuildKey(string category, string query)
    {
        return (category + ':' + query).ToUpper();
    }

    /// <summary>
    ///     Replace all definition queries with their resulting values.<br />
    ///     [color: RED] -> §c<br />
    ///     [wool: BLUE] -> 11<br />
    ///     [misc: BELOW] -> ~ ~-1 ~<br />
    ///     [slot: MAIN HAND] -> slot.weapon.mainhand
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public string ReplaceDefinitions(string input)
    {
        MatchCollection matches = DEF_REGEX.Matches(input);

        foreach (Match match in matches)
        {
            var sb = new StringBuilder();
            int backslashes = match.Groups[1].Value.Length;

            if (backslashes % 2 == 1)
            {
                sb.Append(match.Value[(backslashes / 2)..]);
                goto no_changes_replace; // odd number of backslashes, it's escaped
            }

            sb.Append('\\', backslashes / 2);
            string category = match.Groups[2].Value;
            string fullQuery = match.Groups[3].Value;
            string[] multi = fullQuery.Split(',').Select(s => s.Trim()).ToArray();
            string[] replacements = new string[multi.Length];
            for (int i = 0; i < multi.Length; i++)
            {
                string key = BuildKey(category, multi[i]);
                if (this.defs.TryGetValue(key, out string replacement))
                    replacements[i] = replacement;
                else
                    goto no_changes; // fight me
            }

            foreach (string t in replacements)
                sb.Append(t);

            no_changes_replace:
            string replacementString = sb.ToString();
            input = input.Replace(match.Value, replacementString);

            no_changes: ;
        }

        return input;
    }
}