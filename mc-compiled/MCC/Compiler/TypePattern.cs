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
            pattern = initial.Select(type => new MultiType(false, type.Name, type)).ToList();
        }
        /// <summary>
        /// Construct an empty TypePattern.
        /// </summary>
        /// <param name="initial"></param>
        public TypePattern()
        {
            pattern = new List<MultiType>();
        }
        /*public TypePattern Prepend<A>()
        {
            pattern.Insert(0, new MultiType(false, typeof(A)));
            return this;
        }
        public TypePattern PrependOptional<A>()
        {
            pattern.Insert(0, new MultiType(true, typeof(A)));
            return this;
        }
        public TypePattern And<A>()
        {
            pattern.Add(new MultiType(false, typeof(A)));
            return this;
        }
        public TypePattern Optional<A>()
        {
            pattern.Add(new MultiType(true, typeof(A)));
            return this;
        }*/
        public TypePattern And(Type type, string argName)
        {
            pattern.Add(new MultiType(false, argName, type));
            return this;
        }
        public TypePattern Optional(Type type, string argName)
        {
            pattern.Add(new MultiType(true, argName, type));
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
                Token token = tokens[external];

                if(mtt.Check(token))
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
    /// The OR'ing is currently unused, so only one type should be passed in per MultiType.
    /// </summary>
    public struct MultiType
    {
        internal readonly string argName;
        internal readonly bool optional;
        internal readonly Type[] types;

        public MultiType(bool optional, string argName, params Type[] types)
        {
            this.optional = optional;
            this.argName = argName;
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
        public bool Check(object obj)
        {
            Type type = obj.GetType();
            if (types.Any(t => t.IsAssignableFrom(type)))
                return true;

            if(obj is IImplicitToken)
            {
                Type[] conversion = (obj as IImplicitToken).GetImplicitTypes();

                for (int i = 0; i < conversion.Length; i++)
                    if (types.Any(t => t.IsAssignableFrom(conversion[i])))
                        return true;
            }

            return false;
        }
    }
}
