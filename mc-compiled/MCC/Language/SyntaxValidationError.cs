﻿using JetBrains.Annotations;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Represents an error related to a <see cref="SyntaxParameter" />. Used for LSP linting.
/// </summary>
/// <param name="error">A description of the error to show to the user.</param>
/// <param name="keywordFix">If not null, the keyword which can be used to fix this validation error further.</param>
/// <param name="parameter">If present, the <see cref="SyntaxParameter" /> which failed.</param>
public readonly struct SyntaxValidationError(string error, [CanBeNull] string keywordFix, SyntaxParameter? parameter)
{
    public readonly string error = error;
    [CanBeNull]
    public readonly string keywordFix = keywordFix;
    public readonly SyntaxParameter? parameter = parameter;

    public string ParameterString => this.parameter.HasValue ? $"{this.parameter.Value} {this.error}" : this.error;
}