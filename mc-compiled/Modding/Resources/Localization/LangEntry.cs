namespace mc_compiled.Modding.Resources.Localization;

/// <summary>
///     An entry in a .lang file, either a comment, empty line, or key/value pair. Create using the factory methods.
/// </summary>
public struct LangEntry
{
    internal bool isComment;
    internal bool isEmpty;

    internal readonly string key;
    internal readonly string value;

    private LangEntry(string key, string value, bool isComment, bool isEmpty)
    {
        this.key = key;
        this.value = value;
        this.isComment = isComment;
        this.isEmpty = isEmpty;
    }

    public static LangEntry Create(string key, string value)
    {
        return new LangEntry(key, value, false, false);
    }
    public static LangEntry Comment(string comment)
    {
        return new LangEntry(null, comment, true, false);
    }
    public static LangEntry Empty()
    {
        return new LangEntry(null, null, false, true);
    }

    public override int GetHashCode()
    {
        return this.key.GetHashCode();
    }
    public override string ToString()
    {
        return this.isEmpty ? "" :
            this.isComment ? $"## {this.value}" :
            $"{this.key}={this.value}";
    }
}