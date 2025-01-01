﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Language;

public class SyntaxGroup
{
    /// <summary>
    ///     A syntax group with no constraints; will match no tokens and always pass validation.
    /// </summary>
    public static readonly SyntaxGroup NONE = new(SyntaxGroupBehavior.OneOf, null, false, null, false, false,
        new SyntaxPatterns());

    /// <summary>
    ///     The behavior of this group's children/patterns.
    /// </summary>
    public readonly SyntaxGroupBehavior behavior;
    /// <summary>
    ///     Doubles as specifying <see cref="optional" />.
    ///     Requires this group to be specified before continuing.
    ///     Only really relevant inside <see cref="SyntaxGroupBehavior.Sequential" /> groups.
    /// </summary>
    public readonly bool blocking;
    public readonly SyntaxGroup[] children;
    /// <summary>
    ///     An optional description for this group.
    ///     Mainly used if the group is tied to a specific keyword as to provide documentation for the keyword.
    ///     It's also sometimes available for documentation or making definitions easier to read.
    /// </summary>
    public readonly string description;

    public readonly bool hasChildren;
    public readonly bool hasPatterns;
    /// <summary>
    ///     The unique identifier (locally) of this group. It's the property name used while specifying it.
    ///     If the identifier is null, it was likely created implicitly.
    /// </summary>
    [CanBeNull]
    public readonly string identifier;
    /// <summary>
    ///     This group is optional.
    /// </summary>
    public readonly bool optional;
    public readonly SyntaxPatterns patterns;
    /// <summary>
    ///     If this group's tokens are repeatable, or can only be specified once.
    /// </summary>
    public readonly bool repeatable;

    /// <summary>
    ///     Create a new group that uses <see cref="SyntaxPatterns" /> as its content.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="patterns" /> is null.</exception>
    private SyntaxGroup(SyntaxGroupBehavior behavior, [CanBeNull] string identifier, bool blocking,
        [CanBeNull] string description,
        bool optional, bool repeatable,
        [NotNull] SyntaxPatterns patterns)
    {
        this.patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
        this.hasChildren = false;
        this.hasPatterns = true;

        this.behavior = behavior;
        this.blocking = blocking;
        this.description = description;
        this.optional = optional;
        this.repeatable = repeatable;
        this.children = null;
        this.identifier = identifier;
    }
    /// <summary>
    ///     Create a new group that uses another <see cref="SyntaxGroup" /> as its content.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="children" /> is null.</exception>
    private SyntaxGroup(SyntaxGroupBehavior behavior, [CanBeNull] string identifier, bool blocking,
        [CanBeNull] string description,
        bool optional, bool repeatable,
        [NotNull] SyntaxGroup[] children)
    {
        this.children = children ?? throw new ArgumentNullException(nameof(children));
        this.hasChildren = true;
        this.hasPatterns = false;

        this.behavior = behavior;
        this.blocking = blocking;
        this.description = description;
        this.optional = optional;
        this.repeatable = repeatable;
        this.patterns = null;
        this.identifier = identifier;
    }

    /// <summary>
    ///     If present, the keyword required to reference this group. Keyword is documented by the group's
    ///     <see cref="description" />.
    /// </summary>
    public string Keyword { get; private set; }

    /// <summary>
    ///     Returns if this group is <see cref="optional" />, or optional <see cref="blocking" />
    /// </summary>
    public bool IsOptional => this.optional || this.blocking;

    private static SyntaxPatterns ParseSimplePattern(string[] parameters,
        SyntaxGroupBehavior behavior = SyntaxGroupBehavior.OneOf)
    {
        if (SyntaxPatterns.TryParse(parameters, out SyntaxPatterns parsed))
            return parsed;

        throw new FormatException(
            $"Couldn't parse syntax group from the following patterns: {string.Join(", ", parameters)}");
    }
    private static SyntaxPatterns ParseSimplePatterns(IEnumerable<string[]> _patterns,
        SyntaxGroupBehavior behavior = SyntaxGroupBehavior.OneOf)
    {
        string[][] patterns = _patterns.ToArray(); // prevent multiple-enumeration
        if (SyntaxPatterns.TryParse(patterns, out SyntaxPatterns parsed))
            return parsed;

        throw new FormatException(
            $"Couldn't parse syntax group from the following patterns: [{string.Join("], [", patterns.Select(p => string.Join(", ", p)))}]");
    }
    private static SyntaxGroup ParseSingleComplex(JProperty _json)
    {
        string identifier = _json.Name;
        JObject json = _json.Value as JObject ??
                       throw new FormatException($"Syntax group must be an object at '{_json.Path}'");

        bool isOptional = json["optional"]?.Value<bool>() ?? false;
        bool isBlocking = json["blocking"]?.Value<bool>() ?? false;
        string keyword = json["keyword"]?.Value<string>();
        string description = json["description"]?.Value<string>();

        bool hasPatterns = json.TryGetValue("patterns", out JToken _patternsToken);
        bool hasChildren = json.TryGetValue("children", out JToken _childrenToken);
        bool hasRef = json.TryGetValue("ref", out JToken _refToken);

        if (!hasPatterns && !hasChildren && !hasRef)
            throw new FormatException($"Syntax group was missing `patterns`, `children`, or `ref` at '{json.Path}'");
        if ((hasPatterns && hasChildren) || (hasPatterns && hasRef) || (hasChildren && hasRef))
            throw new FormatException(
                $"Syntax group had multiple of `patterns`, `children`, or `ref` at '{json.Path}'");

        SyntaxGroup group = null;

        // patterns case, terminating
        if (hasPatterns)
        {
            JArray patternsJSON = _patternsToken as JArray ??
                                  throw new FormatException($"'patterns' must be an array at '{_patternsToken.Path}'");

            bool isMultiplePatterns = patternsJSON[0].Type == JTokenType.Array;
            SyntaxPatterns patterns;

            if (isMultiplePatterns)
            {
                string[][] patternsArray = patternsJSON.ToObject<string[][]>() ??
                                           throw new FormatException(
                                               $"Expected array of string arrays in pattern at '{_patternsToken.Path}'");
                patterns = ParseSimplePatterns(patternsArray);
            }
            else
            {
                string[] patternsArray = patternsJSON.ToObject<string[]>() ??
                                         throw new FormatException(
                                             $"Expected array of strings in pattern at '{_patternsToken.Path}'");
                patterns = ParseSimplePattern(patternsArray);
            }

            group = new SyntaxGroup(SyntaxGroupBehavior.OneOf, identifier,
                isBlocking,
                description,
                isOptional,
                false,
                patterns);
        }

        // children case, recursive
        if (hasChildren)
        {
            JObject childrenJSON = _childrenToken as JObject ??
                                   throw new FormatException(
                                       $"'children' must be an object at '{_childrenToken.Path}'");
            if (childrenJSON.Count == 0)
                throw new FormatException(
                    $"'children' had no groups inside. Must have at least one at '{_childrenToken.Path}'");

            (SyntaxGroup[] children, bool isSequential, bool isRepeatable) = ParseManyComplex(childrenJSON);

            group = new SyntaxGroup(isSequential ? SyntaxGroupBehavior.Sequential : SyntaxGroupBehavior.OneOf,
                identifier,
                isBlocking,
                description,
                isOptional,
                isRepeatable,
                children);
        }

        // reference case, terminating
        if (hasRef)
        {
            string refName = _refToken.Value<string>() ??
                             throw new FormatException($"'ref' must be a string at '{_refToken.Path}'");
            group = Language.QuerySyntaxGroup(refName) ??
                    throw new FormatException(
                        $"Syntax group by reference '{refName}' could not be found. At '{_refToken.Path}'");
        }

        if (keyword != null)
            group.Keyword = keyword;
        return group;
    }
    /// <summary>
    ///     Parses an object containing multiple sub-objects representing subgroups.
    ///     Returns all the groups in order,
    ///     as well as if <c>sequential</c> or <c>repeatable</c> were specified respectively.
    /// </summary>
    /// <param name="json"></param>
    /// <param name="behaviorOverride"></param>
    /// <returns>(An array of the parsed groups --- if they're sequential --- if they're repeatable.)</returns>
    private static (SyntaxGroup[], bool, bool) ParseManyComplex(JObject json,
        SyntaxGroupBehavior? behaviorOverride = null)
    {
        bool isSequential = json["sequential"]?.Value<bool>() ?? false;
        bool isRepeatable = json["repeatable"]?.Value<bool>() ?? false;

        SyntaxGroupBehavior behavior = behaviorOverride ??
                                       (isSequential ? SyntaxGroupBehavior.Sequential : SyntaxGroupBehavior.OneOf);

        List<SyntaxGroup> children = [];
        children.AddRange(json.Properties()
            .Select(property => property.Name is "sequential" or "repeatable" ? null : ParseSingleComplex(property))
            .Where(g => g != null));

        return (children.ToArray(), isSequential, isRepeatable);
    }
    /// <summary>
    ///     Parses a new <see cref="SyntaxGroup" /> from the given JSON token.
    ///     Works with a simple string array or an object containing multiple groups.
    /// </summary>
    /// <param name="token">The token to parse from.</param>
    /// <returns></returns>
    public static SyntaxGroup Parse(JToken token)
    {
        if (token.Type == JTokenType.Array)
        {
            var array = token as JArray;
            if (array!.Count == 0)
                return new SyntaxGroup(SyntaxGroupBehavior.OneOf, null,
                    false, null, true, false, new SyntaxPatterns());
            string[] patterns = array!.ToObject<string[]>() ??
                                throw new FormatException($"Syntax group was array of non-strings at '{token.Path}'");
            SyntaxPatterns parsedPatterns = ParseSimplePattern(patterns);
            return new SyntaxGroup(SyntaxGroupBehavior.OneOf, null, false, null, false, false, parsedPatterns);
        }

        if (token.Type == JTokenType.Object)
        {
            var obj = token as JObject;
            (SyntaxGroup[] groups, bool isSequential, bool isRepeatable) = ParseManyComplex(obj);
            return new SyntaxGroup(isSequential ? SyntaxGroupBehavior.Sequential : SyntaxGroupBehavior.OneOf, null,
                false, null, false, isRepeatable, groups);
        }

        throw new FormatException($"Syntax group was not an array or object at '{token.Path}'");
    }
    /// <summary>
    ///     Searches for a child <see cref="SyntaxGroup" /> with the specified identifier within the current group.
    /// </summary>
    /// <param name="childName">The identifier of the child <see cref="SyntaxGroup" /> to search for.</param>
    /// <returns>
    ///     The matching child <see cref="SyntaxGroup" /> if found; otherwise, null.
    /// </returns>
    public SyntaxGroup QueryChild(string childName)
    {
        if (!this.hasChildren)
            return null;

        foreach (SyntaxGroup child in this.children)
            if (child.identifier != null && child.identifier.Equals(childName, StringComparison.OrdinalIgnoreCase))
                return child;

        return (from child in this.children
                where child.identifier == null
                select child.QueryChild(childName))
            .FirstOrDefault(subgroup => subgroup != null);
    }
}

public enum SyntaxGroupBehavior
{
    /// <summary>
    ///     Children/patterns will be sampled sequentially, in order from top to bottom.
    /// </summary>
    Sequential,
    /// <summary>
    ///     Only one of the children/patterns will be chosen as a viable path.
    /// </summary>
    OneOf
}