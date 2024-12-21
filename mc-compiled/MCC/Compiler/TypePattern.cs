using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     A pattern that can be matched with a set of tokens.
/// </summary>
public class TypePattern
{
    private readonly List<MultiType> pattern;

    /// <summary>
    ///     Construct a new TypePattern that starts with some required token(s).
    /// </summary>
    /// <param name="initial">The initial tokens.</param>
    public TypePattern(params NamedType[] initial)
    {
        this.pattern = initial.Select(type => new MultiType(false, "unknown", type)).ToList();
    }
    /// <summary>
    ///     Construct an empty TypePattern.
    /// </summary>
    public TypePattern()
    {
        this.pattern = [];
    }

    public int Count => this.pattern.Count;

    public TypePattern And(NamedType type, string argName)
    {
        this.pattern.Add(new MultiType(false, argName, type));
        return this;
    }
    public TypePattern And(NamedType type)
    {
        this.pattern.Add(new MultiType(false, type.name, type));
        return this;
    }
    public TypePattern Optional(NamedType type, string argName)
    {
        this.pattern.Add(new MultiType(true, argName, type));
        return this;
    }
    public TypePattern Optional(NamedType type)
    {
        this.pattern.Add(new MultiType(true, type.name, type));
        return this;
    }

    public TypePattern PrependAnd(NamedType type, string argName)
    {
        _ = this.pattern.Prepend(new MultiType(false, argName, type));
        return this;
    }
    public TypePattern PrependAnd(NamedType type)
    {
        _ = this.pattern.Prepend(new MultiType(false, type.name, type));
        return this;
    }
    public TypePattern PrependOptional(NamedType type, string argName)
    {
        _ = this.pattern.Prepend(new MultiType(true, argName, type));
        return this;
    }
    public TypePattern PrependOptional(NamedType type)
    {
        _ = this.pattern.Prepend(new MultiType(true, type.name, type));
        return this;
    }

    /// <summary>
    ///     Checks tokens straight from the given statement, without consuming them. Shorthand for
    ///     Check(tokens.GetRemainingTokens());
    /// </summary>
    /// <param name="_tokens"></param>
    /// <returns></returns>
    public MatchResult Check(Statement _tokens)
    {
        IEnumerable<Token> tokens = _tokens.GetRemainingTokens();
        return Check(tokens.ToArray());
    }
    /// <summary>
    ///     Checks the given array of tokens against this pattern, returning information about how much they match, if it
    ///     matched, and which tokens are missing.
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public MatchResult Check(Token[] tokens)
    {
        float givenLength = tokens.Length;
        float minLength = this.pattern.Count(mt => !mt.IsOptional);
        if (tokens.Length < minLength)
        {
            // return the missing tokens.
            IEnumerable<MultiType> missing = this.pattern.Skip(tokens.Length);
            return new MatchResult(false, givenLength / minLength, missing.ToArray());
        }

        int patternCount = this.pattern.Count;
        int self = 0;
        int external = 0;

        while (true)
        {
            if (self >= patternCount)
                return new MatchResult(true); // reached end of pattern without any invalid tokens

            if (external >= tokens.Length)
            {
                // not enough tokens to fit whole pattern, loop through and check if the rest are optional
                for (; self < patternCount; self++)
                {
                    MultiType cur = this.pattern[self];
                    if (!cur.IsOptional)
                    {
                        // this argument was not given and was not optional
                        int skip = self > 0 ? self - 1 : 0;
                        IEnumerable<MultiType> missing = this.pattern.Skip(skip).TakeWhile(mt => !mt.IsOptional);
                        return new MatchResult(false, givenLength / minLength, missing.ToArray());
                    }
                }

                return new MatchResult(true);
            }

            MultiType mtt = this.pattern[self];
            Token token = tokens[external];

            if (mtt.Check(token))
            {
                self++;
                external++;
            }
            else if (mtt.IsOptional)
            {
                self++;
            }
            else
            {
                // skipped/failed required parameter
                int skip = self > 0 ? self - 1 : 0;
                int missing = this.pattern.Skip(skip).Count(mt => !mt.IsOptional);
                return new MatchResult(false, (patternCount - missing) / (float) patternCount, [mtt]);
            }
        }
    }

    /// <summary>
    ///     Returns this TypePattern converted to a markdown string controlling how it will look on the Wiki Cheatsheet.
    /// </summary>
    /// <returns></returns>
    public string ToMarkdownDocumentation()
    {
        return string.Join(" ", this.pattern.Select(multiType => multiType.ToString()));
    }
}

/// <summary>
///     Information about a pattern match.
/// </summary>
public class MatchResult
{
    /// <summary>
    ///     The about, between 0-1 that this did match. If (match == true), this will be 1.
    /// </summary>
    public float accuracy;
    /// <summary>
    ///     If this result returned true.
    /// </summary>
    public bool match;
    /// <summary>
    ///     The types that are missing from the given tokens.
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
///     A type with a pre-cached or otherwise set name.
/// </summary>
public class NamedType
{
    public readonly string name;
    public readonly Type type;

    /// <summary>
    ///     A type with a pre-cached or otherwise set name.
    /// </summary>
    public NamedType(Type type, string name)
    {
        this.type = type;
        this.name = name;
    }
    /// <summary>
    ///     A type with a pre-cached or otherwise set name.
    /// </summary>
    public NamedType(Type type)
    {
        this.type = type;
        this.name = type.Name;
    }

    public override string ToString()
    {
        return this.name;
    }
}

/// <summary>
///     Represents multiple types OR'd together for the TypePattern.
///     The OR'ing is currently unused, so only one type should be passed in per MultiType.
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

    public bool IsOptional => this.optional;

    /// <summary>
    ///     Check this type to see if it fits into this MultiType's template.
    /// </summary>
    /// <param name="obj">The object to check the type of.</param>
    /// <returns></returns>
    public bool Check(object obj)
    {
        Type type = obj.GetType();
        if (this.types.Any(t => t.type.IsAssignableFrom(type)))
            return true;

        if (obj is IImplicitToken)
        {
            Type[] conversion = (obj as IImplicitToken).GetImplicitTypes();

            for (int i = 0; i < conversion.Length; i++)
                if (this.types.Any(t => t.type.IsAssignableFrom(conversion[i])))
                    return true;
        }

        return false;
    }

    /// <summary>
    ///     Displays this multitype as a parameter string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        char bOpen = this.optional ? '[' : '<';
        char bClose = this.optional ? ']' : '>';
        return bOpen + this.types[0].name + ": " + this.argName + bClose;
    }
}