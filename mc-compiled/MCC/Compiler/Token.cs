using System;
using System.Linq;
using mc_compiled.MCC.Language;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     The most basic processed unit in the file.
///     Gives basic parsed information and generally assists in the assembly of a Statement
/// </summary>
public abstract class Token
{
    public int lineNumber;
    public Token(int lineNumber) { this.lineNumber = lineNumber; }
    /// <summary>
    ///     Get this token represented as it might look in the source file.
    /// </summary>
    /// <returns></returns>
    public abstract string AsString();

    public override string ToString() { return AsString(); }

    /// <summary>
    ///     Get a string that provides more information about the token given.
    /// </summary>
    /// <returns></returns>
    public string DebugString()
    {
        if (this is TokenNewline)
            return "[Newline]\n";

        string typeName = GetType().Name[5..];
        return '[' + typeName + ' ' + AsString() + ']';
    }

    /// <summary>
    ///     Returns true if this token can match the given <see cref="SyntaxParameter" />
    /// </summary>
    /// <param name="parameter">The parameter to check against.</param>
    /// <param name="allowImplicit">Allow implicit conversions in the validation. Defaults to true.</param>
    /// <returns>True if the token matches the parameter.</returns>
    public bool MatchesParameter(SyntaxParameter parameter, bool allowImplicit = true)
    {
        Type nextType = GetType();
        Type typeConstraint = parameter.typeConstraint!;

        if (typeConstraint.IsAssignableFrom(nextType))
            return true;

        // not assignable, but may be able to convert implicitly
        if (allowImplicit && this is IImplicitToken implicitToken)
        {
            Type[] otherTypes = implicitToken.GetImplicitTypes();
            return otherTypes.Any(t => typeConstraint.IsAssignableFrom(t));
        }

        return false;
    }
}

public struct TokenDefinition
{
    public readonly Type type;
    public readonly string keyword;

    public TokenDefinition(Type type, string keyword)
    {
        this.type = type;
        this.keyword = keyword;
    }
}

/// <summary>
///     Decorates a token which can be implicitly converted to another type if needed.
/// </summary>
public interface IImplicitToken
{
    /// <summary>
    ///     Get the types this token can be implicitly converted to.
    /// </summary>
    /// <returns></returns>
    Type[] GetImplicitTypes();

    /// <summary>
    ///     Convert this token to an alternate type by index of its valid types.
    /// </summary>
    /// <param name="executor">The executor running this conversion, for project context/errors.</param>
    /// <param name="index">The index of GetImplicitTypes() to convert to.</param>
    /// <returns></returns>
    Token Convert(Executor executor, int index);
}

/// <summary>
///     Decorates a token so that it can contain user-friendly documentation on how to input it in the language.
///     This documentation should be shown only when used in language.json type mapping.
/// </summary>
public interface IDocumented
{
    string GetDocumentation();
}

/// <summary>
///     Indicates that a token had some kind of unknown error.
/// </summary>
public class TokenException : Exception
{
    public string desc;
    public Token token;

    public TokenException(Token token, string desc)
    {
        this.token = token;
        this.desc = desc;
    }
}