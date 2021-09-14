using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Limits
{
    /// <summary>
    /// Represents a selector option that limits based off of score values.
    /// </summary>
    public struct Scores
    {
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
