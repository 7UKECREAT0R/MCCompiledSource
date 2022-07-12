using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Scores field in selector.
    /// https://minecraft.fandom.com/wiki/Target_selectors#Selecting_targets_by_scores
    /// </summary>
    public class HasScoreCheck
    {
        public string objective;
        public Range value;

        public HasScoreCheck(string objective, Range value)
        {
            this.objective = objective;
            this.value = value;
        }
        public override string ToString()
        {
            return objective + '=' + value.ToString();
        }

        /// <summary>
        /// Parse a HasScoreCheck. Expected format: <code>objectiveName=12..34</code>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HasScoreCheck Parse(string str)
        {
            int equals = str.IndexOf('=');

            if (equals == -1)
                throw new MCC.Compiler.TokenizerException("Expected equals sign in score check: " + str);

            string objective = str.Substring(0, equals);
            string _value = str.Substring(equals + 1);
            Range? value = Range.Parse(_value);

            if(value.HasValue)
                return new HasScoreCheck(objective, value.Value);
            else
                throw new MCC.Compiler.TokenizerException("Invalid scoreboard value to check for: " + _value);
        }
    }
}
