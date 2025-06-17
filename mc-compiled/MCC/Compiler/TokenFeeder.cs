using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.MCC.Language;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     A class which allows the "feeding" of tokens and quick checking/casting of their types.
/// </summary>
public class TokenFeeder(Token[] tokens) : ICloneable
{
    protected int currentToken;
    public Executor executor;
    protected Token[] tokens = tokens;

    public int[] Lines { get; private set; }
    public bool DecorateInSource { get; protected set; }
    public string Source { get; private set; }

    /// <summary>
    ///     Returns if this feeder has another available token.
    /// </summary>
    public bool HasNext => this.currentToken < this.tokens.Length;

    /// <summary>
    ///     Returns the number of remaining tokens in this feeder, excluding comments.
    /// </summary>
    public int RemainingTokens
    {
        get
        {
            if (this.tokens == null)
                return 0;
            return this.tokens.Skip(this.currentToken)
                .Count(t => t is not IUselessInformation);
        }
    }
    /// <summary>
    ///     Gets or sets the current position in the token stream managed by the <see cref="TokenFeeder" />.
    ///     Modifying this property directly changes the index of the current token
    ///     being processed within the internal token array. This allows for manual adjustment of the token
    ///     processing position, such as rewinding or skipping ahead.
    /// </summary>
    /// <remarks>
    ///     The value is zero-based and must be within the bounds of the tokens array.
    ///     Invalid values (e.g., negative indices or indices exceeding the token array bounds) may lead to undesired
    ///     behavior or exceptions during subsequent operations on the <see cref="TokenFeeder" />.
    /// </remarks>
    /// <value>
    ///     An integer representing the zero-based index of the current token in the managed token stream.
    /// </value>
    /// <seealso cref="HasNext" />
    /// <seealso cref="RemainingTokens" />
    public int Location
    {
        get => this.currentToken;
        set => this.currentToken = value;
    }
    /// <summary>
    ///     Properly clones this TokenFeeder. Has a minor bit of overhead.
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
        return new TokenFeeder(this.tokens)
        {
            executor = this.executor,
            currentToken = this.currentToken,
            Lines = this.Lines,
            Source = this.Source,
            DecorateInSource = this.DecorateInSource
        };
    }

    /// <summary>
    ///     Copies the current token index from another <see cref="TokenFeeder" /> instance.
    /// </summary>
    /// <param name="other">The source <see cref="TokenFeeder" /> whose location should be copied.</param>
    public void CopyLocationOf(TokenFeeder other) { this.currentToken = other.currentToken; }

    /// <summary>
    ///     Set the executor of this TokenFeeder.
    /// </summary>
    /// <param name="newExecutor"></param>
    public void SetExecutor(Executor newExecutor) { this.executor = newExecutor; }

    /// <summary>
    ///     Set the line of source this feeder relates to. Used in "errors."
    /// </summary>
    /// <param name="lines">An array of integers representing the line numbers associated with the line.</param>
    /// <param name="code">A string representing the source code</param>
    public void SetSource(int[] lines, string code)
    {
        this.Lines = lines;
        this.Source = code;
    }
    /// <summary>
    ///     Pulls the next token in the feeder.
    /// </summary>
    /// <exception cref="FeederException"></exception>
    public Token Next()
    {
        if (this.currentToken >= this.tokens.Length)
            throw new FeederException(this, "Expected token at end of line.");
        return this.tokens[this.currentToken++];
    }
    /// <summary>
    ///     Peeks at the next token in the <see cref="TokenFeeder" />, but doesn't skip it yet.
    ///     Use <see cref="Next" /> and its derivatives to skip the token.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FeederException">If there are no more tokens in the feeder.</exception>
    public Token Peek()
    {
        if (this.currentToken >= this.tokens.Length)
            throw new FeederException(this, "Token expected at end of line.");
        return this.tokens[this.currentToken];
    }
    /// <summary>
    ///     Pulls the next token in the feeder, casting it to the given type. Implements MCCompiled implicit conversions.
    /// </summary>
    /// <typeparam name="T">The type to cast.</typeparam>
    /// <param name="parameterHint">
    ///     The name of the parameter that this token will fill. Errors will display this name as a hint to the user.
    ///     You may pass null to this parameter if you’ve checked it beforehand via <see cref="NextIs{T}" />.
    /// </param>
    /// <returns></returns>
    /// <exception cref="FeederException"></exception>
    public T Next<T>(string parameterHint) where T : class
    {
        if (this.currentToken >= this.tokens.Length)
            throw new FeederException(this,
                $"expected parameter '{parameterHint}' at end of line, type ");

        Token token = this.tokens[this.currentToken++];

        if (token is T castedToken)
            return castedToken;
        if (token is not IImplicitToken implicitToken)
            throw new FeederException(this,
                $"invalid token type for parameter '{parameterHint}'. expected {typeof(T).GetFriendlyTokenName()} but got {token.FriendlyTypeName}");

        Type[] otherTypes = implicitToken.GetImplicitTypes();

        for (int i = 0; i < otherTypes.Length; i++)
            if (typeof(T).IsAssignableFrom(otherTypes[i]))
                return implicitToken.Convert(this.executor, i) as T;

        throw new FeederException(this,
            $"invalid token type for parameter '{parameterHint}'. expected {typeof(T).GetFriendlyTokenName()} but got {implicitToken.GetType().GetFriendlyTokenName()}");
    }
    /// <summary>
    ///     Peeks at the next token in the feeder, casting it to the given type. Implements MCCompiled implicit conversions.
    /// </summary>
    /// <param name="allowImplicit"></param>
    /// <typeparam name="T">The type to cast.</typeparam>
    /// <returns></returns>
    /// <exception cref="FeederException"></exception>
    public T Peek<T>(bool allowImplicit = true) where T : class
    {
        if (this.currentToken >= this.tokens.Length)
            throw new FeederException(this, $"Token expected at end of line, type {typeof(T).GetFriendlyTokenName()}");
        Token token = this.tokens[this.currentToken];

        if (token is T validToken)
            return validToken;

        if (!allowImplicit)
            throw new FeederException(this,
                $"invalid token type. expected {typeof(T).GetFriendlyTokenName()} but got {token.FriendlyTypeName}");

        if (token is not IImplicitToken implicitToken)
            throw new FeederException(this,
                $"invalid token type. expected {typeof(T).GetFriendlyTokenName()} but got {token.FriendlyTypeName}");

        Type[] otherTypes = implicitToken.GetImplicitTypes();

        for (int i = 0; i < otherTypes.Length; i++)
            if (typeof(T).IsAssignableFrom(otherTypes[i]))
                return implicitToken.Convert(this.executor, i) as T;
        throw new FeederException(this,
            $"invalid token type. expected {typeof(T).GetFriendlyTokenName()} but got {token.FriendlyTypeName}");
    }
    /// <summary>
    ///     Returns if the next parameter (if any) is able to be casted to a certain type. Implements MCCompiled implicit
    ///     conversions.
    /// </summary>
    /// <typeparam name="T">
    ///     The type to check for. If <paramref name="enforceType" /> is true, the token (if any) must match
    ///     the type or it will throw an error.
    /// </typeparam>
    /// <param name="enforceType">
    ///     If true and there is a token at the end of the statement, an exception will be thrown if the
    ///     type doesn't match.
    /// </param>
    /// <param name="allowImplicit">Allow implicit conversion of tokens.</param>
    /// <returns></returns>
    public bool NextIs<T>(bool enforceType, bool allowImplicit = true)
    {
        if (!this.HasNext)
            return false;

        Token token = this.tokens[this.currentToken];

        if (token is T)
            return true;
        if (token is IUselessInformation)
            return false; // don't throw

        if (!allowImplicit || token is not IImplicitToken implicitToken)
        {
            TryEnforceType();
            return false;
        }

        Type[] otherTypes = implicitToken.GetImplicitTypes();
        bool canDoImplicitConversion = otherTypes.Any(t => typeof(T).IsAssignableFrom(t));

        if (!canDoImplicitConversion)
            TryEnforceType();

        return canDoImplicitConversion;

        // throws if enforceType is enabled.
        void TryEnforceType()
        {
            if (enforceType)
                throw new FeederException(this,
                    $"parameter here must be '{typeof(T).GetFriendlyTokenName()}', but got '{token.FriendlyTypeName}'.");
        }
    }
    /// <summary>
    ///     Checks whether the next token is considered "useless" information, such as a comment.
    /// </summary>
    /// <returns>True if the next token is of a type that implements IUselessInformation; otherwise, false.</returns>
    public bool NextIsUseless()
    {
        if (!this.HasNext)
            return false;
        return this.tokens[this.currentToken] is IUselessInformation;
    }

    /// <summary>
    ///     Returns if the next token matches the given <see cref="SyntaxParameter" />.
    /// </summary>
    /// <param name="parameter">The parameter to match against.</param>
    /// <param name="uselessTokensConsumed">
    ///     The number of tokens consumed by this method.
    ///     This method doesn't generally mutate the <see cref="TokenFeeder" />, but it does skip over useless tokens (such as
    ///     comments.)
    /// </param>
    /// <param name="theToken">If there was a token to check, that token. Null if the feeder was out of tokens.</param>
    /// <param name="allowImplicit">Allow implicit conversions for the validation.</param>
    public bool NextMatchesParameter(SyntaxParameter parameter,
        out int uselessTokensConsumed,
        [CanBeNull] out Token theToken,
        bool allowImplicit = true)
    {
        uselessTokensConsumed = 0;
        while (NextIsUseless())
        {
            this.currentToken++;
            uselessTokensConsumed++;
        }

        if (!this.HasNext)
        {
            theToken = null;
            return false;
        }

        theToken = Peek();

        return parameter.blockConstraint
            ? this.executor.NextIs<StatementOpenBlock>()
            : theToken.MatchesParameter(parameter, allowImplicit);
    }
    /// <summary>
    ///     Determines whether the next meaningful token in the sequence matches the specified keyword. Case-insensitive.
    /// </summary>
    /// <param name="keyword">
    ///     The keyword to compare against the next meaningful token. The comparison is case-insensitive.
    /// </param>
    /// <param name="uselessTokensConsumed">
    ///     The number of tokens consumed by this method.
    ///     This method doesn't generally mutate the <see cref="TokenFeeder" />, but it does skip over useless tokens (such as
    ///     comments.)
    /// </param>
    /// <returns>
    ///     True if the next meaningful token matches the specified <paramref name="keyword" />; otherwise, false.
    /// </returns>
    /// <remarks>
    ///     The method skips over any tokens that implement <see cref="IUselessInformation" /> before
    ///     performing the comparison.
    ///     If no meaningful tokens remain in the sequence or the next meaningful
    ///     token is not a <see cref="TokenIdentifier" />, the method returns false.
    /// </remarks>
    public bool NextMatchesKeyword(string keyword, out int uselessTokensConsumed)
    {
        uselessTokensConsumed = 0;
        while (NextIsUseless())
        {
            this.currentToken++;
            uselessTokensConsumed++;
        }

        if (!this.HasNext)
            return false;

        string nextTokenWord = Peek().AsString();
        return nextTokenWord.Equals(keyword, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Return the remaining tokens in this feeder, excluding comments. Doesn’t modify the reader's location.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Token> GetRemainingTokens()
    {
        if (this.tokens == null)
            return Array.Empty<Token>();
        if (this.tokens.Length <= this.currentToken)
            return Array.Empty<Token>();

        return this.tokens.Skip(this.currentToken)
            .Where(t => t is not IUselessInformation);
    }
}