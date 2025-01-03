﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace mc_compiled.Commands.Selectors;

/// <summary>
///     Represents a selector option that limits based off of score values.
/// </summary>
public struct Scores : IEquatable<Scores>
{
    private static readonly Regex MATCHER = new(@"scores={([\w\d=,.]+)}");
    public List<ScoresEntry> checks;

    public Scores(params ScoresEntry[] start)
    {
        this.checks = [..start];
    }
    public Scores(List<ScoresEntry> start)
    {
        this.checks = start;
    }

    public string GetSection()
    {
        if (this.checks == null || this.checks.Count < 1)
            return null;
        return "scores={" + string.Join(",", from i in this.checks select i.ToString()) + "}";
    }

    public static Scores Parse(string fullSelector)
    {
        if (!MATCHER.IsMatch(fullSelector))
            return new Scores(new List<ScoresEntry>());

        var scores = new Scores(new List<ScoresEntry>());

        Match match = MATCHER.Match(fullSelector);
        Group group = match.Groups[1];
        string str = group.Value;

        string[] parts = str.Split(',');
        foreach (string part in parts)
        {
            int index = part.IndexOf('=');
            if (index == -1)
                continue;

            string scoreName = part[..index].Trim();

            string _range = part[(index + 1)..].Trim();
            Range? range = Range.Parse(_range);

            if (range == null)
                continue; // failed parse

            var entry = new ScoresEntry(scoreName, range.Value);
            scores.checks.Add(entry);
        }

        return scores;
    }

    public bool Equals(Scores other)
    {
        return Equals(this.checks, other.checks);
    }
    public override bool Equals(object obj)
    {
        return obj is Scores other && Equals(other);
    }
    public override int GetHashCode()
    {
        return this.checks != null ? this.checks.GetHashCode() : 0;
    }

    public static Scores operator +(Scores a, Scores other)
    {
        var clone = (Scores) a.MemberwiseClone();
        clone.checks = [..a.checks];
        clone.checks.AddRange(other.checks);
        return clone;
    }

    public static bool operator ==(Scores left, Scores right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Scores left, Scores right)
    {
        return !(left == right);
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

    public bool Equals(ScoresEntry other)
    {
        return this.name == other.name && this.value.Equals(other.value);
    }
    public override bool Equals(object obj)
    {
        return obj is ScoresEntry other && Equals(other);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            return ((this.name != null ? this.name.GetHashCode() : 0) * 397) ^ this.value.GetHashCode();
        }
    }

    public override string ToString()
    {
        return this.name + "=" + this.value;
    }
}