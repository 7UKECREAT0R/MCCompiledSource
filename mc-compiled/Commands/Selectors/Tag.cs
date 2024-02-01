using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a selector option that limits from a tag.
    /// </summary>
    public readonly struct Tag
    {
        private readonly bool not;
        private readonly string tagName;  // Can be null

        [PublicAPI]
        public Tag(string tagName, bool not)
        {
            this.not = not;
            this.tagName = tagName;
        }
        [PublicAPI]
        public Tag(string tagName)
        {
            not = tagName.StartsWith("!");
            this.tagName = not ? tagName.Substring(1) : tagName;
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
                return new Tag("", false);

            if (!str.StartsWith("!"))
                return new Tag(str, false);
            if (str.Length == 1)
                return new Tag("", true);
            
            return new Tag(str.Substring(1), true);
        }

        public string GetSection()
        {
            string s = tagName ?? "";
            if (not)
                return "tag=!" + s;
            return "tag=" + s;
        }

        private bool Equals(Tag other)
        {
            return not == other.not && tagName == other.tagName;
        }
        public override bool Equals(object obj)
        {
            return obj is Tag other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (not.GetHashCode() * 397) ^ (tagName != null ? tagName.GetHashCode() : 0);
            }
        }
    }
}
