using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a selector option that limits from a tag.
    /// </summary>
    public struct Tag
    {
        public bool not;
        public string tagName;  // Can be null

        public Tag(string tagName, bool not)
        {
            this.not = not;
            this.tagName = tagName;
        }
        public Tag(string tagName)
        {
            not = tagName.StartsWith("!");
            if (not)
                this.tagName = tagName.Substring(1);
            else this.tagName = tagName;
        }

        /// <summary>
        /// Parse something like "!is_waiting"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Tag Parse(string str)
        {
            if (str == null)
                throw new ArgumentNullException();

            str = str.Trim();

            if(str.Length == 0)
                return new Tag(null, false);

            if (str.StartsWith("!"))
                if (str.Length == 1)
                    return new Tag(null, true);
                else
                    return new Tag(str.Substring(1), true);
            else
                return new Tag(str, false);
        }

        public string GetSection()
        {
            string s = tagName ?? "";
            if (not)
                return "tag=!" + s;
            else return "tag=" + s;
        }
    }
}
