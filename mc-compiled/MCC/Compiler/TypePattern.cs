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
        public TypePattern(params NamedType[] initial)
        {
            pattern = initial.Select(type => new MultiType(false, "unknown", type)).ToList();
        }
        /// <summary>
        /// Construct an empty TypePattern.
        /// </summary>
        /// <param name="initial"></param>
        public TypePattern()
        {
            pattern = new List<MultiType>();
        }
        public TypePattern And(NamedType type, string argName)
        {
            pattern.Add(new MultiType(false, argName, type));
            return this;
        }
        public TypePattern Optional(NamedType type, string argName)
        {
            pattern.Add(new MultiType(true, argName, type));
            return this;
        }

        public MatchResult Check(Token[] tokens)
        {
            int givenLength = tokens.Length;
            int minLength = pattern.Count(mt => !mt.IsOptional);
            if (tokens.Length < minLength)
            {
                // return the missing tokens.
                var missing = pattern.Skip(tokens.Length);
                return new MatchResult(false, givenLength / minLength, missing.ToArray());
            }

            int patternCount = pattern.Count;
            int self = 0;
            int external = 0;

            while(true)
            {
                if (self >= patternCount)
                    return new MatchResult(true); // reached end of pattern without any invalid tokens

                if (external >= tokens.Length)
                {
                    // not enough tokens to fit whole pattern, loop through and check if the rest are optional
                    for (; self < patternCount; self++)
                    {
                        MultiType cur = pattern[self];
                        if (!cur.IsOptional)
                        {
                            // this argument was not given and was not optional
                            int skip = self > 0 ? self - 1 : 0;
                            var missing = pattern.Skip(skip).TakeWhile(mt => !mt.IsOptional);
                            return new MatchResult(false, givenLength / minLength, missing.ToArray());
                        }
                    }

                    return new MatchResult(true); 
                }

                MultiType mtt = pattern[self];
                Token token = tokens[external];

                if (mtt.Check(token))
                {
                    self++;
                    external++;
                    continue;
                }
                else if (mtt.IsOptional)
                    self++;
                else
                {
                    // skipped/failed required parameter
                    int skip = self > 0 ? self - 1 : 0;
                    int missing = pattern.Skip(skip).Count(mt => !mt.IsOptional);
                    return new MatchResult(false, (patternCount - missing) / (float)patternCount, new[] { mtt });
                }
            }
        }
    }
    /// <summary>
    /// Information about a pattern match.
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// If this result returned true.
        /// </summary>
        public bool match;
        /// <summary>
        /// The about, between 0-1 that this did match. If (match == true), this will be 1.
        /// </summary>
        public float accuracy;
        /// <summary>
        /// The types that are missing from the given tokens.
        /// </summary>
        public MultiType[] missing;

        public MatchResult(bool match, float accuracy, MultiType[] missing)
        {
            this.match = match;
            this.accuracy = accuracy;
            this.missing = missing;
        }
        public MatchResult(bool match)
        {
            this.match = match;
            this.accuracy = match ? 1f : 0f;
            this.missing = null;
        }
    }
    /// <summary>
    /// A type with a pre-cached or otherwise set name.
    /// </summary>
    public class NamedType
    {
        public readonly Type type;
        public readonly string name;

        public NamedType(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }
        public NamedType(Type type)
        {
            this.type = type;
            this.name = type.Name;
        }

        public override string ToString() => this.name;
    }
    /// <summary>
    /// Represents multiple types OR'd together for the TypePattern.
    /// The OR'ing is currently unused, so only one type should be passed in per MultiType.
    /// </summary>
    public struct MultiType
    {
        internal readonly string argName;
        internal readonly bool optional;
        internal readonly NamedType[] types;

        public MultiType(bool optional, string argName, params NamedType[] types)
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
            if (types.Any(t => t.type.IsAssignableFrom(type)))
                return true;

            if(obj is IImplicitToken)
            {
                Type[] conversion = (obj as IImplicitToken).GetImplicitTypes();

                for (int i = 0; i < conversion.Length; i++)
                    if (types.Any(t => t.type.IsAssignableFrom(conversion[i])))
                        return true;
            }

            return false;
        }

        public override string ToString()
        {
            if(this.types.Length > 1)
                return '[' + string.Join("/", this.types.Select(t => t.name)) + ": " + this.argName + ']';

            return '[' + this.types[0].name + ": " + this.argName + ']';
        }
    }
}
