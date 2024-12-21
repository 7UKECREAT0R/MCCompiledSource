namespace mc_compiled.Commands.Selectors;

/// <summary>
///     Represents selector option which limits based on count.
/// </summary>
public struct Count
{
    public const int NONE = -1;
    public int count;

    public Count(int count)
    {
        this.count = count;
    }
    public static Count Parse(string[] chunks)
    {
        int endCount = NONE;

        foreach (string chunk in chunks)
        {
            int index = chunk.IndexOf('=');
            if (index == -1)
                continue;
            string a = chunk[..index].Trim().ToUpper();
            string b = chunk[(index + 1)..].Trim();

            if (!a.Equals("C"))
                continue;
            if (string.IsNullOrWhiteSpace(b))
                continue;
            endCount = int.Parse(b);
            if (endCount < 0)
                endCount = 0;
            break;
        }

        return new Count(endCount);
    }

    public bool Equals(Count other)
    {
        return this.count == other.count;
    }
    public override bool Equals(object obj)
    {
        return obj is Count other && Equals(other);
    }
    public override int GetHashCode()
    {
        return this.count;
    }

    public bool HasCount => this.count != NONE;
    public string GetSection()
    {
        if (this.count == NONE)
            return null;
        return "c=" + this.count;
    }
    public static Count operator +(Count a, Count other)
    {
        if (a.count == NONE)
            a.count = other.count;
        return a;
    }

    public static bool operator ==(Count left, Count right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Count left, Count right)
    {
        return !(left == right);
    }
}