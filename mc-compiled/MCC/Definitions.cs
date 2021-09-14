using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// definitions.defs file parser and manager.
    /// </summary>
    public class Definitions
    {
        public static Definitions GLOBAL_DEFS;

        public const string FILE = "definitions.def";
        public static readonly Regex DEF_REGEX = new Regex("\\[([\\w ]+):\\s*([\\w ]+)\\]");

        internal readonly Dictionary<string, string> defs;

        string BuildKey(string category, string query)
        {
            return (category + ':' + query).ToUpper();
        }
        public Definitions(bool debugInfo)
        {
            defs = new Dictionary<string, string>();
            if (!File.Exists(FILE))
            {
                ConsoleColor errprevious = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARN: No '{0}' file found. Constants will not be valid.", FILE);
                Console.ForegroundColor = errprevious;
                return;
            }
            string[] lines = File.ReadAllLines(FILE, Encoding.UTF8);
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
                    Executor.MINECRAFT_VERSION = line.Substring(8);
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
        /// Replace all definition objects with its definition.<br />
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
                string query = match.Groups[2].Value;
                string key = BuildKey(category, query);

                if(defs.TryGetValue(key, out string replacement))
                    input = input.Replace(match.Value, replacement);
            }

            return input;
        }
    }
}
