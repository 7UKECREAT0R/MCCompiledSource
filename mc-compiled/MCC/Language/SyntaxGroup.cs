using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Language;

public class SyntaxGroup : ICloneable
{
    /// <summary>
    ///     A syntax group with no constraints; will match no tokens and always pass validation.
    /// </summary>
    public static readonly SyntaxGroup EMPTY = new(SyntaxGroupBehavior.OneOf, null, false, null, false, false,
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
    private SyntaxGroup(SyntaxGroupBehavior behavior,
        [CanBeNull] string identifier,
        bool blocking,
        [CanBeNull] string description,
        bool optional,
        bool repeatable,
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
    private SyntaxGroup(SyntaxGroupBehavior behavior,
        [CanBeNull] string identifier,
        bool blocking,
        [CanBeNull] string description,
        bool optional,
        bool repeatable,
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
    /// <summary>
    ///     Returns if this group has no validation constraints, and thus should always match.
    /// </summary>
    public bool AlwaysMatches =>
        string.IsNullOrEmpty(this.Keyword) &&
        (this.hasChildren ? this.children.Length == 0 : this.patterns.Count == 0);

    public object Clone()
    {
        if (this.hasChildren)
        {
            SyntaxGroup[] clonedChildren = this.children.Select(child => (SyntaxGroup) child.Clone()).ToArray();
            return new SyntaxGroup(this.behavior, this.identifier, this.blocking, this.description, this.optional,
                this.repeatable, clonedChildren);
        }

        if (this.hasPatterns)
        {
            var clonedPatterns = (SyntaxPatterns) this.patterns.Clone();
            return new SyntaxGroup(this.behavior, this.identifier, this.blocking, this.description, this.optional,
                this.repeatable, clonedPatterns);
        }

        throw new InvalidOperationException(
            "Attempted to clone a syntax group that has no children or patterns. Identifier: " +
            (this.identifier ?? "unknown"));
    }

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

            SyntaxPatterns patterns;

            if (patternsJSON.Count == 0)
            {
                patterns = [];
            }
            else
            {
                bool isMultiplePatterns = patternsJSON[0].Type == JTokenType.Array;

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

            (SyntaxGroup[] children, bool isSequential, bool isRepeatable) =
                ParseManyComplex(childrenJSON, SyntaxGroupBehavior.OneOf);

            group = new SyntaxGroup(isSequential ? SyntaxGroupBehavior.Sequential : SyntaxGroupBehavior.OneOf,
                identifier,
                isBlocking,
                description,
                isOptional,
                isRepeatable,
                children);
        }

        // IMPORTANT: change this if you modify the method to change any more of the referenced SyntaxGroups.
        // in this case, the `Keyword` property is changed if keyword != null, so it's cloned if that's going to happen.
        bool shouldCloneRefGroups = keyword != null;

        // reference case, terminating
        if (hasRef)
        {
            string refName = _refToken.Value<string>() ??
                             throw new FormatException($"'ref' must be a string at '{_refToken.Path}'");
            group = Language.QuerySyntaxGroup(refName, shouldCloneRefGroups) ??
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
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="defaultBehavior">The default behavior if unspecified.</param>
    /// <returns>A tuple of values representing the parsed SyntaxGroup.</returns>
    private static (SyntaxGroup[] parsedGroups, bool isSequential, bool isRepeatable) ParseManyComplex(JObject json,
        SyntaxGroupBehavior defaultBehavior)
    {
        // the behavior override kicks in only if it's not overridden in the JSON source
        bool isSequential = json.ContainsKey("sequential")
            ? json["sequential"].Value<bool>()
            : defaultBehavior == SyntaxGroupBehavior.Sequential;
        bool isRepeatable = json["repeatable"]?.Value<bool>() ?? false;

        if (isRepeatable && !json.ContainsKey("sequential"))
            throw new FormatException(
                $"Definition at {json.Path} is marked as repeatable, but no 'sequential' property was specified. Please add one to be explicit in purpose.");

        List<SyntaxGroup> children = [];
        children.AddRange(json.Properties()
            .Select(property => property.Name is "sequential" or "repeatable" ? null : ParseSingleComplex(property))
            .Where(g => g != null));

        // validation: if repeatable, all subgroups must not always match.
        // there needs to only ever be one branch that fully matches. 
        if (isRepeatable)
            foreach (SyntaxGroup child in children)
                if (child.AlwaysMatches)
                    throw new FormatException(
                        $"Syntax group {child.identifier ?? "(unknown)"}'s parent was marked as repeatable, but always matches. This is an issue with the syntax in language.json, not user error.");

        return (children.ToArray(), isSequential, isRepeatable);
    }
    /// <summary>
    ///     Parses a new <see cref="SyntaxGroup" /> from the given JSON token.
    ///     Works with a simple string array or an object containing multiple groups.
    /// </summary>
    /// <param name="token">The JSON token to parse a <see cref="SyntaxGroup" /> from.</param>
    /// <param name="defaultBehavior">The default behavior for the syntax group, if unspecified by the JSON.</param>
    /// <returns></returns>
    public static SyntaxGroup Parse(JToken token, SyntaxGroupBehavior defaultBehavior)
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
            (SyntaxGroup[] groups, bool isSequential, bool isRepeatable) =
                ParseManyComplex(obj, defaultBehavior);
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
    /// <summary>
    ///     Creates a new <see cref="SyntaxGroup" /> containing the specified patterns.
    ///     The group will choose only one of the given patterns that best fit.
    /// </summary>
    public static SyntaxGroup WrapPatterns(bool groupOptional, params SyntaxParameter[][] groupPatterns)
    {
        return new SyntaxGroup(SyntaxGroupBehavior.OneOf, null, false, null, groupOptional, false,
            new SyntaxPatterns(groupPatterns));
    }
    /// <summary>
    ///     Creates a new <see cref="SyntaxGroup" /> containing one pattern.
    /// </summary>
    public static SyntaxGroup WrapPattern(bool groupOptional, params SyntaxParameter[] groupPattern)
    {
        return new SyntaxGroup(SyntaxGroupBehavior.OneOf, null, false, null, groupOptional, false,
            new SyntaxPatterns(groupPattern));
    }

    /// <summary>
    ///     Recursively count the number of possible paths this <see cref="SyntaxGroup" /> expands into.
    /// </summary>
    /// <param name="runningTotal">The running total, used for recursion.</param>
    /// <returns>The number of paths.</returns>
    public int CountPaths(int runningTotal = 0)
    {
        if (this.hasChildren)
            return runningTotal + this.children.Sum(child => child.CountPaths());
        if (this.hasPatterns)
            return runningTotal + this.patterns.Count;

        return runningTotal; // should never happen but here we are
    }

    /// <summary>
    ///     Returns a <see cref="HashSet{T}" /> of possible parameters which could be specified immediately after the given
    ///     <paramref name="feeder" /> in order to be valid.
    /// </summary>
    /// <param name="feeder">The remaining tokens to read from.</param>
    /// <param name="possibleNextParameters">The set to be compounded to. If null, a new one will be made.</param>
    /// <returns></returns>
    public HashSet<PossibleParameter> GetPossibleNextParameters(TokenFeeder feeder,
        HashSet<PossibleParameter> possibleNextParameters = null)
    {
        possibleNextParameters ??= [];

        return possibleNextParameters;
    }

    private (bool, IEnumerable<SyntaxValidationError>) ValidatePatterns(Executor executor,
        TokenFeeder tokens,
        out int confidence,
        out int tokensConsumed)
    {
        bool result = this.patterns.Validate(
            executor,
            tokens,
            out IEnumerable<SyntaxValidationError> failReasons,
            out confidence,
            out tokensConsumed);

        return (result, failReasons);
    }
    /// <summary>
    ///     Validate this group against an input <see cref="TokenFeeder" />.
    /// </summary>
    /// <param name="executor">The <see cref="Executor" />, for context.</param>
    /// <param name="tokens">
    ///     The tokens to pull from to match against this <see cref="SyntaxGroup" />.
    ///     This will never be modified.
    /// </param>
    /// <param name="failReasons">If the validation fails (returned false), a list of reasons that it failed.</param>
    /// <param name="outputConfidence">If the method returns <c>true</c>, the confidence in this match.</param>
    /// <param name="tokensConsumed">If the method returns <c>true</c>, the number of tokens it should consume from the feeder.</param>
    /// <returns>
    ///     True if the tokens were validated, false if the validation failed for whatever reason.
    /// </returns>
    /// <remarks>
    ///     If two or more <see cref="SyntaxGroup" />s match, you can always choose the one with the higher confidence output
    ///     as the determining factor. This method does NOT take the group's <see cref="optional" /> or <see cref="blocking" />
    ///     value into account.
    /// </remarks>
    public bool Validate(
        Executor executor,
        TokenFeeder tokens,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence,
        out int tokensConsumed)
    {
        if (this.AlwaysMatches)
        {
            outputConfidence = 0;
            tokensConsumed = 0;
            failReasons = null;
            return true;
        }

        int initialTokenFeederLocation = tokens.Location;
        int keywordTokensConsmed = 0;

        // check for keyword token first, since that always takes precedent
        bool matchedKeyword = false;
        if (!string.IsNullOrEmpty(this.Keyword))
        {
            if (tokens.NextMatchesKeyword(this.Keyword, out keywordTokensConsmed))
            {
                keywordTokensConsmed += 1; // include the keyword token itself
                tokens.Location += keywordTokensConsmed; // move the TokenFeeder forward as well
                matchedKeyword = true;
            }
            else
            {
                outputConfidence = 0;
                tokensConsumed = 0;
                failReasons = [new SyntaxValidationError($"missing keyword '{this.Keyword}'", this.Keyword, null)];
                tokens.Location = initialTokenFeederLocation;
                return false;
            }
        }

        if (this.hasPatterns)
        {
            bool patternsResult = this.patterns.Validate(
                executor,
                tokens,
                out failReasons,
                out outputConfidence,
                out tokensConsumed);

            if (matchedKeyword)
            {
                outputConfidence += 100;
                tokensConsumed += keywordTokensConsmed;
                tokens.Location = initialTokenFeederLocation;
            }

            return patternsResult;
        }

        if (!this.hasChildren)
            throw new FormatException(
                $"Syntax group '{this.identifier ?? "(unknown)"}', brought up by source `{tokens.Source}`, had no patterns or children. This is an issue with the syntax in language.json, not user error.");

        switch (this.behavior)
        {
            case SyntaxGroupBehavior.Sequential:
            {
                bool result = this.repeatable
                    ? ValidateSequentialWithRepetition(executor, tokens,
                        out failReasons, out outputConfidence, out tokensConsumed)
                    : ValidateSequentialWithoutRepetition(executor, tokens,
                        out failReasons, out outputConfidence, out tokensConsumed);
                if (matchedKeyword)
                {
                    outputConfidence += 100;
                    tokensConsumed += keywordTokensConsmed;
                    tokens.Location = initialTokenFeederLocation;
                }

                return result;
            }
            case SyntaxGroupBehavior.OneOf:
            {
                bool result = this.repeatable
                    ? ValidateOneOfWithRepetition(executor, tokens,
                        out failReasons, out outputConfidence, out tokensConsumed)
                    : ValidateOneOfWithoutRepetition(executor, tokens,
                        out failReasons, out outputConfidence, out tokensConsumed);
                if (matchedKeyword)
                {
                    outputConfidence += 100;
                    tokensConsumed += keywordTokensConsmed;
                    tokens.Location = initialTokenFeederLocation;
                }

                return result;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(this.behavior),
                    $"SyntaxGroupBehavior not implemented: `{this.behavior}`.");
        }
    }

    /// <summary>
    ///     Validate this group sequentially, repeatedly.
    ///     This method does not take <see cref="optional" /> into account, only for the <see cref="children" />.
    /// </summary>
    private bool ValidateSequentialWithRepetition(Executor executor,
        TokenFeeder tokens,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence,
        out int tokensConsumed)
    {
        int initialTokenFeederLocation = tokens.Location;
        failReasons = [];
        outputConfidence = 0;
        tokensConsumed = 0;
        int successfulPasses = 0;

        do
        {
            bool result = ValidateSequentialWithoutRepetition(executor, tokens,
                out IEnumerable<SyntaxValidationError> currentFailReasons,
                out int currentConfidence,
                out int currentTokensConsumed);

            if (!result)
            {
                // we only want to return a syntax error if it's clear the user's actually trying to specify a repetition of this group.
                // let's assume this if (currentTokensConsumed > 0).
                if (successfulPasses == 0 || (successfulPasses > 0 && currentTokensConsumed > 0))
                {
                    failReasons = currentFailReasons;
                    outputConfidence += currentConfidence;
                    tokensConsumed += currentTokensConsumed;
                    return false;
                }

                break;
            }

            successfulPasses += 1;
            outputConfidence += currentConfidence;
            tokensConsumed += currentTokensConsumed;

            // advance the feeder temporarily
            tokens.Location += currentTokensConsumed;
        } while (tokens.HasNext);

        tokens.Location = initialTokenFeederLocation;
        return successfulPasses > 0;
    }
    /// <summary>
    ///     Validate this group sequentially once, without repetition.
    ///     This method does not take <see cref="optional" /> into account, only for the <see cref="children" />.
    /// </summary>
    private bool ValidateSequentialWithoutRepetition(Executor executor,
        TokenFeeder tokens,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence,
        out int tokensConsumed)
    {
        int initialTokenFeederLocation = tokens.Location;
        List<SyntaxValidationError> errors = [];
        failReasons = errors;
        outputConfidence = 0;
        tokensConsumed = 0;

        int numChildren = this.children.Length;

        for (int childIndex = 0; childIndex < numChildren; childIndex++)
        {
            SyntaxGroup child = this.children[childIndex];
            bool childIsOptional = child.optional;

            if (child.Validate(executor, tokens,
                    out IEnumerable<SyntaxValidationError> childFailReasons,
                    out int childConfidence,
                    out int childTokensConsumed)
               )
            {
                tokensConsumed += childTokensConsumed;
                tokens.Location += childTokensConsumed;
                outputConfidence += childConfidence;
                continue;
            }

            tokensConsumed += childTokensConsumed;
            outputConfidence += childConfidence;

            // failed to validate
            if (child.blocking)
            {
                // check if there are any non-optional parameters after this
                if (this.children.Skip(childIndex + 1).Any(g => !g.IsOptional))
                {
                    errors.AddRange(childFailReasons);
                    tokens.Location = initialTokenFeederLocation;
                    return false;
                }

                // there are no more required parameters after this, so it's okay, this group matches.
                tokens.Location = initialTokenFeederLocation;
                return true;
            }

            if (childIsOptional)
                continue;

            // this child is not optional, so the validation fails
            errors.AddRange(childFailReasons);
            tokens.Location = initialTokenFeederLocation;
            return false;
        }

        // got through without returning `false`, so this group matches.
        tokens.Location = initialTokenFeederLocation;
        errors.Clear();
        return true;
    }
    /// <summary>
    ///     Validate this group by repeatedly accepting any of its subgroups until there isn't a valid match.
    ///     This method does not take <see cref="optional" /> into account.
    /// </summary>
    private bool ValidateOneOfWithRepetition(Executor executor,
        TokenFeeder tokens,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence,
        out int tokensConsumed)
    {
        int initialTokenFeederLocation = tokens.Location;
        failReasons = [];
        outputConfidence = 0;
        tokensConsumed = 0;
        int successfulPasses = 0;

        do
        {
            bool result = ValidateOneOfWithoutRepetition(executor, tokens,
                out IEnumerable<SyntaxValidationError> currentFailReasons,
                out int currentConfidence,
                out int currentTokensConsumed);

            if (!result)
            {
                if (successfulPasses == 0 || (successfulPasses > 0 && currentTokensConsumed > 0))
                {
                    failReasons = currentFailReasons;
                    outputConfidence += currentConfidence;
                    tokensConsumed += currentTokensConsumed;
                    return false;
                }

                break;
            }

            successfulPasses += 1;
            outputConfidence += currentConfidence;
            tokensConsumed += currentTokensConsumed;

            // advance the feeder temporarily
            tokens.Location += currentTokensConsumed;
        } while (tokens.HasNext);

        tokens.Location = initialTokenFeederLocation;
        return successfulPasses > 0;
    }
    /// <summary>
    ///     Validate this group by accepting the best match of its subgroups.
    ///     This method does not take <see cref="optional" /> into account.
    /// </summary>
    private bool ValidateOneOfWithoutRepetition(Executor executor,
        TokenFeeder tokens,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence,
        out int tokensConsumed)
    {
        (bool result, IEnumerable<SyntaxValidationError> failReasons, int outputConfidence, int tokensConsumed)[]
            childrenMatches = this.children.Select(c =>
            {
                bool result = c.Validate(executor, tokens,
                    out IEnumerable<SyntaxValidationError> childFailReasons,
                    out int childOutputConfidence,
                    out int childTokensConsumed);
                return (result, childFailReasons, childOutputConfidence, childTokensConsumed);
            }).ToArray();

        // no matches
        if (!childrenMatches.Any(m => m.result))
        {
            // edge case: all children have keywords, but none of them matched.
            // it's more helpful to display all possible keywords to the user instead of just one.
            if (childrenMatches.All(c => c.failReasons.All(f => f.keywordFix != null)))
            {
                string errorMessage = "possible keywords here: " + string.Join(", ", childrenMatches
                    .SelectMany(c => c.failReasons)
                    .Select(sve => sve.keywordFix));
                failReasons = [new SyntaxValidationError(errorMessage, null, null)];
                outputConfidence = 0;
                tokensConsumed = 0;
                return false;
            }

            (bool result, IEnumerable<SyntaxValidationError> failReasons, int outputConfidence, int tokensConsumed)
                bestFailedMatch = childrenMatches.MaxBy(c => c.outputConfidence);
            outputConfidence = bestFailedMatch.outputConfidence;
            tokensConsumed = bestFailedMatch.tokensConsumed;
            failReasons = bestFailedMatch.failReasons;
            return false;
        }

        // there's one or more matches.
        (bool result, IEnumerable<SyntaxValidationError> failReasons, int outputConfidence, int tokensConsumed)
            bestMatch = childrenMatches.Where(c => c.result).MaxBy(c => c.outputConfidence);
        outputConfidence = bestMatch.outputConfidence;
        tokensConsumed = bestMatch.tokensConsumed;
        failReasons = bestMatch.failReasons;
        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder("Group ");

        if (!string.IsNullOrEmpty(this.identifier))
        {
            sb.Append('\'');
            sb.Append(this.identifier);
            sb.Append("' ");
        }

        if (!string.IsNullOrEmpty(this.Keyword))
        {
            sb.Append("keyword \"");
            sb.Append(this.Keyword);
            sb.Append('"');
        }

        sb.Append(" - ");

        if (this.AlwaysMatches)
        {
            sb.Append("always matches");
            return sb.ToString();
        }

        if (this.hasChildren)
        {
            sb.Append(this.children.Length);
            sb.Append(" subgroups");
        }
        else
        {
            sb.Append(this.patterns.Count);
            sb.Append(" patterns");
        }

        if (this.optional)
            sb.Append(" (optional)");
        if (this.blocking)
            sb.Append(" (blocking)");
        if (this.repeatable)
            sb.Append(" (repeatable)");

        return sb.ToString();
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