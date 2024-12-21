using mc_compiled.Commands.Selectors;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Serves as a clarification as to who is being referenced for a Value.
///     Example for all cows:
///     @e[type=cow]
///     Example for a fakeplayer 'neanderthal'
///     neanderthal
/// </summary>
public class Clarifier
{
    private const string DEFAULT_CLARIFIER = "@s";

    private bool global;

    /// <summary>
    ///     Creates a new Clarifier with the default string attached to the global state.
    /// </summary>
    /// <param name="global"></param>
    public Clarifier(bool global)
    {
        this.global = global;
        Reset();
    }
    public Clarifier(bool global, string currentString)
    {
        this.global = global;
        this.CurrentString = currentString;
    }

    /// <summary>
    ///     The string representing the current clarifier.<br />
    ///     <br />
    ///     To set, use <see cref="SetSelector(Selector)" /> or <see cref="SetString(string)" />
    /// </summary>
    public string CurrentString { get; private set; }

    /// <summary>
    ///     Returns if the clarifier was defined as global.
    /// </summary>
    public bool IsGlobal
    {
        get => this.global;
        set => SetGlobal(value);
    }
    /// <summary>
    ///     Return a deep clone of this clarifier.
    /// </summary>
    /// <returns></returns>
    public Clarifier Clone()
    {
        return new Clarifier(this.global, this.CurrentString);
    }

    public static Clarifier Local()
    {
        return new Clarifier(false);
    }
    public static Clarifier Global()
    {
        return new Clarifier(true);
    }
    public static Clarifier With(string selector)
    {
        return new Clarifier(false, selector);
    }

    /// <summary>
    ///     Sets this clarifier's global state. Makes call to <see cref="Reset" />.
    /// </summary>
    /// <param name="newGlobal">Whether to have global set or not.</param>
    /// <returns>This object for chaining.</returns>
    public void SetGlobal(bool newGlobal)
    {
        this.global = newGlobal;
        Reset();
    }
    public void CopyFrom(Clarifier other)
    {
        this.global = other.global;
        this.CurrentString = other.CurrentString;
    }
    /// <summary>
    ///     Reset this clarifier to its default value, changing depending on if it's <see cref="global" /> or not.
    /// </summary>
    private void Reset()
    {
        this.CurrentString = this.global ? Executor.FAKE_PLAYER_NAME : DEFAULT_CLARIFIER;
    }

    /// <summary>
    ///     Sets the clarifier to a specific selector.
    /// </summary>
    /// <param name="selector">The selector.</param>
    public void SetSelector(Selector selector)
    {
        string str = selector.ToString();
        this.CurrentString = str;
    }
    /// <summary>
    ///     Sets the clarifier to a specific string.
    /// </summary>
    /// <param name="str">The string.</param>
    public void SetString(string str)
    {
        this.CurrentString = str;
    }

    private bool Equals(Clarifier other)
    {
        return this.global == other.global && this.CurrentString == other.CurrentString;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Clarifier) obj);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            return (this.global.GetHashCode() * 397) ^
                   (this.CurrentString != null ? this.CurrentString.GetHashCode() : 0);
        }
    }
}