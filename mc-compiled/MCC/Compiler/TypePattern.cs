using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A pattern that can be matched with a set of tokens.
    /// </summary>
    public class TypePattern
    {
        private readonly List<MultiType> pattern;

        /// <summary>
        /// Construct a new TypePattern that starts with a base required token.
        /// </summary>
        /// <param name="initial"></param>
        public TypePattern(params Type[] initial)
        {
            pattern = initial.Select(type => new MultiType(false, type)).ToList();
        }
        public TypePattern And<A>()
        {
            pattern.Add(new MultiType(false, typeof(A)));
            return this;
        }

        public bool Check(Token[] tokens)
        {
            if (tokens.Length < pattern.Count(mt => !mt.IsOptional))
                return false;

            int self = 0;
            int external = 0;

            while(true)
            {
                if (self >= pattern.Count)
                    return true; // reached end of pattern without any invalid tokens

                if (external >= tokens.Length)
                {
                    // not enough tokens to fit whole pattern, see if rest are optional
                    for (; self < pattern.Count; self++)
                        if (!pattern[self].IsOptional)
                            return false;
                    return true; 
                }

                MultiType mtt = pattern[self];
                Type type = tokens[external].GetType();

                if(mtt.Check(type))
                {
                    self++;
                    external++;
                    continue;
                } else if(mtt.IsOptional)
                    self++;
                else
                    return false; // skipped/missed required parameter
            }
        }
    }
    /// <summary>
    /// Represents multiple types OR'd together for the TypePattern.
    /// </summary>
    public struct MultiType
    {
        readonly bool optional;
        readonly Type[] types;

        public MultiType(bool optional, params Type[] types)
        {
            this.optional = optional;
            this.types = types;
        }

        public bool IsOptional
        {
            get => optional;
        }

        /// <summary>
        /// Check this type to see if it fits into this MultiType's template.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Check(Type type)
        {
            return types.Any(t => t.IsAssignableFrom(type));
        }
    }
}
