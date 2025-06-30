using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Language;

/// <summary>
///     A list of <see cref="SyntaxParameter" /> arrays with which one can match a list of tokens.
/// </summary>
public class SyntaxPatterns : List<SyntaxParameter[]>, ICloneable
{
    /// <summary>
    ///     Create a new empty <see cref="SyntaxPatterns" />
    /// </summary>
    public SyntaxPatterns() { }
    /// <summary>
    ///     Create a new <see cref="SyntaxPatterns" /> with one valid pattern.
    /// </summary>
    /// <param name="parameters"></param>
    public SyntaxPatterns(params SyntaxParameter[] parameters) { Add(parameters); }
    /// <summary>
    ///     Create a new <see cref="SyntaxPatterns" /> with multiple valid patterns that can match it.
    /// </summary>
    /// <param name="parameters"></param>
    public SyntaxPatterns(IEnumerable<SyntaxParameter[]> parameters) :
        base(parameters) { }

    public object Clone() { return new SyntaxPatterns(this); }

    /// <summary>
    ///     Generates a formatted usage guide string for a single syntax pattern by processing
    ///     the specified array of <see cref="SyntaxParameter" /> objects, distinguishing between optional
    ///     and required parameters, and grouping similar parameters.
    /// </summary>
    /// <param name="pattern">
    ///     An array of <see cref="SyntaxParameter" /> objects that defines the syntax structure
    ///     to be represented in the usage guide. Each element in the array specifies whether a parameter
    ///     is optional, its functional equivalence, and its usage string.
    /// </param>
    /// <returns>
    ///     A formatted string that represents the usage guide for the given <paramref name="pattern" />.
    ///     The resulting string includes properly delimited placeholders with their respective names,
    ///     grouped appropriately for parameters that are functionally equivalent.
    /// </returns>
    /// <remarks>
    ///     The method processes the provided <paramref name="pattern" /> to construct a descriptive
    ///     usage guide by iterating over the array. Functionally equivalent parameters, as determined
    ///     by their type and characteristics, are grouped together in the output. Optional parameters
    ///     are wrapped with square brackets, and required parameters are wrapped with angle brackets.
    /// </remarks>
    private string BuildUsageGuideSingle(SyntaxParameter[] pattern)
    {
        int readIndex = 0;
        List<SyntaxParameter> collection = []; // reused
        StringBuilder sb = new();

        while (HasNext())
        {
            SyntaxParameter root = Next();
            collection.Clear();

            // collect parameters with the same type but different names.
            while (HasNext())
                if (root.FunctionallyEquals(Peek()))
                    collection.Add(Next());
                else
                    break;

            // build a string for these parameter(s).
            sb.Append('`');
            sb.Append(root.optional ? '[' : '<');
            sb.Append(root.AsUsageString);

            foreach (SyntaxParameter similarParameter in collection)
            {
                sb.Append(", ");
                sb.Append(similarParameter.name);
            }

            sb.Append(root.optional ? ']' : '>');
            sb.Append('`');

            if (HasNext())
                sb.Append(' ');
        }

        return sb.ToString();

        bool HasNext() { return readIndex < pattern.Length; }
        SyntaxParameter Peek() { return pattern[readIndex]; }
        SyntaxParameter Next() { return pattern[readIndex++]; }
    }
    /// <summary>
    ///     Builds a usage guide for this set of syntax patterns and outputs it into the <paramref name="lines" /> list.
    /// </summary>
    /// <param name="indentLevel">The base indent level to output on.</param>
    /// <param name="lines">The list to output into.</param>
    public void BuildUsageGuide(int indentLevel, List<(string content, int indentLevel)> lines)
    {
        if (this.Count == 0)
            return;
        if (this.Count == 1)
        {
            // the pattern can be placed on a single line
            lines.Add((BuildUsageGuideSingle(this[0]), indentLevel));
            return;
        }

        lines.Add(("one of:", indentLevel));
        lines.AddRange(this.Select(pattern =>
            (BuildUsageGuideSingle(pattern), indentLevel + 1))
        );
    }

    /// <summary>
    ///     Attempts to parse a single string array into a <see cref="SyntaxPatterns" /> object with a single pattern inside.
    /// </summary>
    /// <param name="parameters">
    ///     The input array of string arrays, where each string represents a single parameter to be
    ///     parsed.
    /// </param>
    /// <param name="patterns">
    ///     When the method returns, contains the resulting <see cref="SyntaxPatterns" /> object if parsing
    ///     was successful, or null if parsing failed.
    /// </param>
    /// <returns>True if the parsing was successful and the <see cref="SyntaxPatterns" /> object was created; otherwise, false.</returns>
    public static bool TryParse(string[] parameters, out SyntaxPatterns patterns)
    {
        List<SyntaxParameter> first = [];

        foreach (string parameter in parameters)
            if (SyntaxParameter.TryParse(parameter, out SyntaxParameter parsed))
            {
                first.Add(parsed);
            }
            else
            {
                patterns = null;
                return false;
            }

        patterns = new SyntaxPatterns(first.ToArray());
        return true;
    }
    /// <summary>
    ///     Attempts to parse an array of string arrays into a <see cref="SyntaxPatterns" /> object.
    /// </summary>
    /// <param name="patterns">The input array of string arrays, where each inner array represents a pattern to be parsed.</param>
    /// <param name="parsed">
    ///     When the method returns, contains the resulting <see cref="SyntaxPatterns" /> object if parsing
    ///     was successful, or null if parsing failed.
    /// </param>
    /// <returns>True if the parsing was successful and the <see cref="SyntaxPatterns" /> object was created; otherwise, false.</returns>
    public static bool TryParse(string[][] patterns, out SyntaxPatterns parsed)
    {
        var parsedPatterns = new SyntaxParameter[patterns.Length][];

        for (int i = 0; i < patterns.Length; i++)
        {
            string[] pattern = patterns[i];
            var parsedPattern = new SyntaxParameter[pattern.Length];
            for (int j = 0; j < pattern.Length; j++)
            {
                string parameter = pattern[j];
                if (SyntaxParameter.TryParse(parameter, out SyntaxParameter parsedParameter))
                {
                    parsedPattern[j] = parsedParameter;
                }
                else
                {
                    parsed = null;
                    return false;
                }
            }

            parsedPatterns[i] = parsedPattern;
        }

        parsed = new SyntaxPatterns(parsedPatterns);
        return true;
    }

    /// <summary>
    ///     Validates a given sequence of <see cref="Token" /> objects against a single pattern
    ///     specified by an array of <see cref="SyntaxParameter" />.
    /// </summary>
    /// <param name="executor">
    ///     The <see cref="Executor" /> instance used to execute and evaluate the details of the validation
    ///     process. This parameter provides context and tools necessary for the validation.
    /// </param>
    /// <param name="tokens">
    ///     An array of <see cref="Token" /> objects representing the sequence to be validated.
    ///     Each token in this array is checked against the provided syntax pattern.
    /// </param>
    /// <param name="pattern">
    ///     An array of <see cref="SyntaxParameter" /> objects that specifies the pattern to validate
    ///     the input tokens against. Each parameter in the pattern represents a specific aspect or
    ///     structure required for validation.
    /// </param>
    /// <returns>
    ///     A <see cref="bool" /> value indicating whether the sequence of <paramref name="tokens" />
    ///     matches the provided <paramref name="pattern" /> entirely. Returns <c>true</c> if the sequence
    ///     is valid, otherwise <c>false</c>.
    /// </returns>
    public static bool NewValidateSingle(Executor executor, Token[] tokens, params SyntaxParameter[] pattern)
    {
        var feeder = new TokenFeeder(tokens);
        return ValidateSinglePattern(executor, feeder, pattern, [], out _, out _);
    }

    /// <summary>
    ///     Validates the remaining tokens in <paramref name="feeder" /> and returns if they match this pattern.
    /// </summary>
    /// <param name="executor">The executor running this validation.</param>
    /// <param name="feeder">
    ///     The remaining tokens to sample. This will <b>not</b> be modified!
    /// </param>
    /// <param name="failReasons">
    ///     The parameters which are blocking this statement from validating, if returning <c>false</c>.
    /// </param>
    /// <param name="outputConfidence">The confidence in this match, even if returning <c>false</c>.</param>
    /// <param name="tokensConsumed">The number of tokens consumed by the match, if returning <c>true</c>.</param>
    /// <returns>True if a match was made.</returns>
    public bool Validate(Executor executor,
        TokenFeeder feeder,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence,
        out int tokensConsumed)
    {
        List<SyntaxValidationError> errors = [];
        failReasons = errors;
        int count = this.Count;
        tokensConsumed = 0;

        if (count == 0)
        {
            outputConfidence = 0;
            return true; // technically a match, as there are no constraints.
        }

        if (count == 1)
            return ValidateSinglePattern(executor, feeder, this[0], errors, out outputConfidence, out tokensConsumed);

        (bool result, int confidence, int tokensConsumed, List<SyntaxValidationError> temporaryErrors)[]
            allVariantMatches = this.Select(p =>
            {
                List<SyntaxValidationError> temporaryErrors = [];
                bool result = ValidateSinglePattern(executor, feeder, p, temporaryErrors, out int confidence,
                    out int tokensConsumed);
                return (result, confidence, tokensConsumed, temporaryErrors);
            }).ToArray();

        // choose the one with the highest confidence
        (bool result, int confidence, int tokensConsumed, List<SyntaxValidationError> temporaryErrors)
            highestConfidenceVariant =
                allVariantMatches.Any(v => v.result)
                    ? allVariantMatches.Where(v => v.result).MaxBy(v => v.confidence) // there was at least 1 match
                    : allVariantMatches.MaxBy(v => v.confidence); // there were no matches

        outputConfidence = highestConfidenceVariant.confidence;
        errors.AddRange(highestConfidenceVariant.temporaryErrors);
        tokensConsumed = highestConfidenceVariant.tokensConsumed;
        return highestConfidenceVariant.result;
    }
    private static bool ValidateSinglePattern(Executor executor,
        TokenFeeder feeder,
        SyntaxParameter[] pattern,
        List<SyntaxValidationError> errors,
        out int confidence,
        out int tokensConsumed,
        int startingIndex = 0)
    {
        int originalFeederLocation = feeder.Location;
        confidence = 0;
        tokensConsumed = 0;

        if (pattern.Length == 0)
            return true; // technically a match, as there are no constraints.

        int patternIndex = startingIndex - 1;

        while (feeder.HasNext)
        {
            patternIndex++;

            if (patternIndex >= pattern.Length)
            {
                feeder.Location = originalFeederLocation;
                return true; // matched all parameters in this pattern.
            }

            SyntaxParameter currentParameter = pattern[patternIndex];
            bool variadic = currentParameter.variadic;
            Range? variadicRange = currentParameter.variadicRange;
            bool optional = currentParameter.optional;
            if (optional == false && variadic && variadicRange.HasValue)
                // if the variadic accepts 0 parameters, this is technically optional as well.
                if (variadicRange.Value.IsInside(0))
                    optional = true;

            if (currentParameter.blockConstraint)
            {
                if (executor.NextIs<StatementOpenBlock>())
                {
                    confidence += 1;
                    continue;
                }

                if (!optional)
                {
                    errors.Add(new SyntaxValidationError("missing block {}", null, null));
                    feeder.Location = originalFeederLocation;
                    return false;
                }

                continue;
            }

            if (variadic)
            {
                int minInclusive = variadicRange?.min ?? 0;
                int maxInclusive = variadicRange?.max ?? int.MaxValue;
                SyntaxParameter? breakoutParameter =
                    patternIndex + 1 < pattern.Length ? pattern[patternIndex + 1] : null;
                int totalMatches = 0;

                // edge case: breakout parameter is specified immediately
                if (breakoutParameter.HasValue &&
                    feeder.NextMatchesParameter(breakoutParameter.Value, out int c, out _, false))
                {
                    tokensConsumed += c;

                    if (!optional)
                    {
                        errors.Add(new SyntaxValidationError($"minimum number of parameters is {minInclusive}", null,
                            breakoutParameter.Value));
                        feeder.Location = originalFeederLocation;
                        return false;
                    }

                    continue;
                }

                while (feeder.NextMatchesParameter(currentParameter, out c, out _))
                {
                    totalMatches++;
                    feeder.Next();
                    tokensConsumed++;
                    tokensConsumed += c; // the number of useless tokens consumed by the NextMatchesParameter method.

                    if (totalMatches >= maxInclusive)
                        break;
                    if (breakoutParameter.HasValue &&
                        feeder.NextMatchesParameter(breakoutParameter.Value, out c, out _, false))
                    {
                        tokensConsumed += c;
                        break;
                    }
                }

                if (totalMatches < minInclusive)
                {
                    errors.Add(new SyntaxValidationError($"minimum number of parameters is {minInclusive}",
                        null, currentParameter));
                    feeder.Location = originalFeederLocation;
                    return false;
                }

                if (totalMatches > maxInclusive)
                {
                    // technically can't happen, but you never know
                    errors.Add(new SyntaxValidationError($"maximum number of parameters is {maxInclusive}",
                        null, currentParameter));
                    feeder.Location = originalFeederLocation;
                    return false;
                }

                confidence += totalMatches;
                continue;
            }

            // basic match. does the next token in the feeder match this parameter?
            bool simpleMatch = feeder.NextMatchesParameter(currentParameter,
                out int uselessTokensConsumed,
                out Token theToken);
            tokensConsumed += uselessTokensConsumed;

            if (!simpleMatch)
            {
                // it's okay, but only if the parameter is optional.
                if (optional)
                    continue;

                string errorMessage = theToken == null ? "required" : "expected, but got " + theToken.FriendlyTypeName;
                errors.Add(new SyntaxValidationError(errorMessage, null, currentParameter));
                feeder.Location = originalFeederLocation;
                return false;
            }

            // match!
            feeder.Next();
            confidence++;
            tokensConsumed++;
        }

        // iterate over any remaining Syntax Parameters and
        // find out if they're not optional.
        for (int i = patternIndex + 1; i < pattern.Length; i++)
        {
            SyntaxParameter currentParameter = pattern[i];
            if (currentParameter.optional)
                continue;
            if (currentParameter.variadic)
            {
                if (currentParameter.variadicRange.HasValue)
                {
                    if (currentParameter.variadicRange.Value.IsInside(0))
                        continue;
                }
                else
                {
                    continue;
                }
            }

            // don't forget the block constraint!
            if (currentParameter.blockConstraint && executor.NextIs<StatementOpenBlock>())
                continue;

            errors.Add(new SyntaxValidationError("required", null, currentParameter));
            feeder.Location = originalFeederLocation;
            return false;
        }

        // got through the whole thing without returning false
        feeder.Location = originalFeederLocation;
        return true;
    }
}