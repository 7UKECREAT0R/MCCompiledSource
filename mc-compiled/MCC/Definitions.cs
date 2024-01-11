using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace mc_compiled.MCC
{
    /// <summary>
    /// definitions.defs file parser and manager.
    /// </summary>
    public class Definitions
    {
        public static Definitions GLOBAL_DEFS;

        public const string FILE = "definitions.def";
        public static readonly Regex DEF_REGEX = new Regex(@"\[([\w ]+):\s*([\w ,]+)\]");

        internal readonly Dictionary<string, string> defs;

        string BuildKey(string category, string query)
        {
            return (category + ':' + query).ToUpper();
        }
        /// <summary>
        /// Initializes and loads a definitions instance, also setting GLOBAL_DEFS to it.
        /// </summary>
        /// <param name="debugInfo"></param>
        public Definitions(bool debugInfo)
        {
            defs = new Dictionary<string, string>();
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(assemblyDir, FILE);
            if (!File.Exists(path))
            {
                ConsoleColor errprevious = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: Missing definitions file at '{0}'. Expect everything to blow up.", path);
                Console.ForegroundColor = errprevious;
                return;
            }
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            string category = null;
            int catEntries = 0;
            string[] categoryAliases = new string[0];

            ConsoleColor previous = ConsoleColor.White;
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
                    string input = line.Substring(9).ToUpper();

                    if (debugInfo)
                    {
                        Console.WriteLine("Define: Category {0} with {1} entries and {2} alias(es).", category, catEntries, categoryAliases.Length);
                        catEntries = 0;
                    }

                    if (input.Contains(" AND "))
                    {
                        string[] aliases = input.Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
                        category = aliases[0];
                        categoryAliases = new string[aliases.Length - 1];
                        for (int i = 0; i < categoryAliases.Length; i++)
                            categoryAliases[i] = aliases[i + 1].Trim();
                    }
                    else
                    {
                        categoryAliases = new string[0];
                        category = input;
                    }
                    continue;
                }
                else if (line.StartsWith("VERSION"))
                {
                    Compiler.Executor.MINECRAFT_VERSION = line.Substring(8);
                    continue;
                }

                string[] assignParts = line.Split(new string[] { " IS " },
                    StringSplitOptions.RemoveEmptyEntries);
                if (assignParts.Length == 2)
                {
                    if (category == null)
                        continue;
                    string name = assignParts[0].Trim().ToUpper();
                    string value = assignParts[1].Trim();

                    string key;
                    
                    key = BuildKey(category, name);
                    defs[key] = value;

                    if (debugInfo)
                        catEntries++;

                    foreach (string alias in categoryAliases)
                    {
                        key = BuildKey(alias, name);
                        defs[key] = value;
                    }
                }
            }

            if (debugInfo)
                Console.ForegroundColor = previous;

            GLOBAL_DEFS = this;
        }

        /// <summary>
        /// Replace all definition queries with their resulting values.<br />
        /// [color: RED] -> §c<br />
        /// [wool: BLUE] -> 11<br />
        /// [misc: BELOW] -> ~ ~-1 ~<br />
        /// [slot: MAIN HAND] -> slot.weapon.mainhand
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ReplaceDefinitions(string input)
        {
            int totalLines = input.Count(c => c == '\n') + 1;

            MatchCollection matches = DEF_REGEX.Matches(input);

            foreach(Match match in matches)
            {
                string category = match.Groups[1].Value;
                string fullQuery = match.Groups[2].Value;
                string[] multi = fullQuery.Split(',').Select(s => s.Trim()).ToArray();
                string[] replacements = new string[multi.Length];
                for(int i = 0; i < multi.Length; i++)
                {
                    string key = BuildKey(category, multi[i]);
                    if (defs.TryGetValue(key, out string replacement))
                        replacements[i] = replacement;
                    else
                        goto no_changes; // fight me
                }
                input = input.Replace(match.Value, string.Join("", replacements));
                no_changes:
                continue;
            }

            return input;
        }
    }
}
