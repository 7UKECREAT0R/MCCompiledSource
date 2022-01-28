﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a selector option that limits based off of score values.
    /// </summary>
    public struct Scores
    {
        public static readonly Regex MATCHER = new Regex(@"scores={([\w\D=]+)}");
        public List<ScoresEntry> checks;

        public Scores(params ScoresEntry[] start)
        {
            checks = new List<ScoresEntry>(start);
        }
        public Scores(List<ScoresEntry> start)
        {
            checks = start;
        }

        public string GetSection()
        {
            if (checks == null || checks.Count < 1)
                return null;
            return "scores={" + string.Join(",", (from i in checks select i.ToString())) + "}";
        }

        public static Scores Parse(string fullSelector)
        {
            if (!MATCHER.IsMatch(fullSelector))
                return new Scores(new ScoresEntry[0]);

            Scores scores = new Scores(new List<ScoresEntry>());

            Match match = MATCHER.Match(fullSelector);
            Group group = match.Groups[1];
            string str = group.Value;

            string[] parts = str.Split(',');
            foreach(string part in parts)
            {
                int index = part.IndexOf('=');
                if (index == -1)
                    continue;

                string scoreName = part.Substring(0, index);
                string _range = part.Substring(index + 1);
                Range range = Range.Parse(_range).Value;
                ScoresEntry entry = new ScoresEntry(scoreName, range);
                scores.checks.Add(entry);
            }

            return scores;
        }
    }
    public struct ScoresEntry
    {
        public string name;
        public Range value;

        public ScoresEntry(string name, Range value)
        {
            this.name = name;
            this.value = value;
        }
        public override string ToString()
        {
            return name + "=" + value.ToString();
        }
    }
}
