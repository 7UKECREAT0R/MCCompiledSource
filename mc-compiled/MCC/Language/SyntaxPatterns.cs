using System;
using System.Collections.Generic;
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
        return ValidateSinglePattern(executor, feeder, pattern, [], out _);
    }

    /// <summary>
    ///     Validates the remaining tokens in <paramref name="feeder" /> and returns if they match this pattern.
    /// </summary>
    /// <param name="executor">The executor running this validation.</param>
    /// <param name="feeder">
    ///     The remaining tokens to sample.
    ///     This <b>will</b> mutate the <see cref="TokenFeeder" />.
    /// </param>
    /// <param name="failReasons">
    ///     The parameters which are blocking this statement from validating,
    ///     if returning <c>false</c>.
    /// </param>
    /// <param name="outputConfidence">The confidence in this match, even if returning false.</param>
    /// <returns>True if a match was made.</returns>
    public bool Validate(Executor executor,
        TokenFeeder feeder,
        out IEnumerable<SyntaxValidationError> failReasons,
        out int outputConfidence)
    {
        List<SyntaxValidationError> errors = [];
        int highestConfidence = -1;
        int count = this.Count;

        if (count == 0)
        {
            failReasons = [];
            outputConfidence = 1;
            return true; // technically a match, as there are no constraints.
        }

        if (count == 1)
        {
            failReasons = errors;
            return ValidateSinglePattern(executor, feeder, this[0], errors, out outputConfidence);
        }

        for (int i = 0; i < count; i++)
        {
            errors.Clear();
            bool last = i == count - 1; // if this is the last iteration
            TokenFeeder temporaryFeeder = last ? feeder : (TokenFeeder) feeder.Clone();
            List<SyntaxValidationError> temporaryErrors = [];

            if (ValidateSinglePattern(executor, temporaryFeeder, this[i], temporaryErrors, out int confidence))
            {
                failReasons = [];
                outputConfidence = confidence;
                return true;
            }

            if (confidence > highestConfidence)
            {
                errors.Clear();
                errors.AddRange(temporaryErrors);
                highestConfidence = confidence;
            }
        }

        failReasons = errors;
        outputConfidence = highestConfidence;
        return false;
    }
    private static bool ValidateSinglePattern(Executor executor,
        TokenFeeder feeder,
        SyntaxParameter[] pattern,
        List<SyntaxValidationError> errors,
        out int confidence,
        int startingIndex = 0)
    {
        confidence = 0;

        if (pattern.Length == 0)
            return true; // technically a match, as there are no constraints.

        int patternIndex = startingIndex - 1;

        while (feeder.HasNext)
        {
            patternIndex++;

            if (patternIndex >= pattern.Length)
                return true; // matched all parameters in this pattern.

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

                errors.Add(new SyntaxValidationError("missing block {}", null));
                return false;
            }

            if (variadic)
            {
                int minInclusive = variadicRange?.min ?? 0;
                int maxInclusive = variadicRange?.max ?? int.MaxValue;
                SyntaxParameter? breakoutParameter =
                    patternIndex + 1 < pattern.Length ? pattern[patternIndex + 1] : null;
                int totalMatches = 0;

                // edge case: breakout parameter is specified immediately
                if (breakoutParameter.HasValue && feeder.NextMatchesParameter(breakoutParameter.Value, false))
                {
                    if (!optional)
                    {
                        errors.Add(new SyntaxValidationError($"minimum number of parameters is {minInclusive}",
                            breakoutParameter.Value));
                        return false;
                    }

                    continue;
                }

                while (feeder.NextMatchesParameter(currentParameter))
                {
                    totalMatches++;
                    feeder.Next();

                    if (totalMatches >= maxInclusive)
                        break;
                    if (breakoutParameter.HasValue && feeder.NextMatchesParameter(breakoutParameter.Value, false))
                        break;
                }

                if (totalMatches < minInclusive)
                {
                    errors.Add(new SyntaxValidationError($"minimum number of parameters is {minInclusive}",
                        currentParameter));
                    return false;
                }

                if (totalMatches > maxInclusive)
                {
                    // technically can't happen, but you never know
                    errors.Add(new SyntaxValidationError($"maximum number of parameters is {maxInclusive}",
                        currentParameter));
                    return false;
                }

                confidence += totalMatches;
                continue;
            }

            // basic match. does the next token in the feeder match this parameter?
            bool simpleMatch = feeder.NextMatchesParameter(currentParameter);

            if (!simpleMatch)
            {
                // it's okay, but only if the parameter is optional.
                if (optional)
                    continue;

                errors.Add(new SyntaxValidationError("required", currentParameter));
                return false;
            }

            // match!
            confidence += 1;
            feeder.Next();
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

            // don't forget the block constraint
            if (currentParameter.blockConstraint && executor.NextIs<StatementOpenBlock>())
                continue;

            errors.Add(new SyntaxValidationError("required", currentParameter));
            return false;
        }

        // got through the whole thing without returning false
        return true;
    }
}