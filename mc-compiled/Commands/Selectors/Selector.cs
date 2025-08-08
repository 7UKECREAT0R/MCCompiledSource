using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Selectors;

/// <summary>
///     Represents a target selector.
/// </summary>
public class Selector
{
    public enum Core
    {
        p, // Nearest player
        s, // Self
        a, // All players
        e, // All entities
        r, // Random entity
        initiator // as dialogue initiator - not usable in regular code
    }

    public static readonly Selector INVOKER = new(Core.initiator);
    public static readonly Selector NEAREST_PLAYER = new(Core.p);
    public static readonly Selector SELF = new(Core.s);
    public static readonly Selector ALL_PLAYERS = new(Core.a);
    public static readonly Selector ALL_ENTITIES = new(Core.e);

    public Area area; // The area where targets should be selected.

    public Core core;
    public Count count; // The limit of entities that can be selected.
    public Entity entity; // The entity/player's status (name, rotation, etc.)
    public HasItems hasItem; // The items which should be checked.
    private Coordinate offsetX = Coordinate.here;
    private Coordinate offsetY = Coordinate.here;
    private Coordinate offsetZ = Coordinate.here;
    public Player player; // The player's specific stats (level, gamemode, etc.)
    public Scores scores; // The scores that should be evaluated.
    public List<Tag> tags; // The tags this entity/player has. Can have multiple.

    public Selector()
    {
        this.scores = new Scores
        {
            checks = []
        };
        this.hasItem = new HasItems
        {
            entries = []
        };
        this.count = new Count
        {
            count = Count.NONE
        };
        this.area = new Area();
        this.entity = new Entity();
        this.player = new Player();
        this.tags = [];
    }
    public Selector(Core core)
    {
        this.core = core;
        this.scores = new Scores
        {
            checks = []
        };
        this.hasItem = new HasItems
        {
            entries = []
        };
        this.count = new Count
        {
            count = Count.NONE
        };
        this.area = new Area();
        this.entity = new Entity();
        this.player = new Player();
        this.tags = [];
    }
    public Selector(Selector copy)
    {
        this.core = copy.core;
        this.area = copy.area;
        this.scores = new Scores(new List<ScoresEntry>(copy.scores.checks));
        this.hasItem = new HasItems(new List<HasItemEntry>(copy.hasItem.entries));
        this.count = copy.count;
        this.entity = copy.entity;
        this.player = copy.player;
        this.tags = [..copy.tags];
    }

    /// <summary>
    ///     Returns if this selector targets multiple entities.
    /// </summary>
    public bool SelectsMultiple
    {
        get
        {
            if (this.count.count == 1)
                return false;
            return this.core != Core.s && this.core != Core.p && this.core != Core.r;
        }
    }

    /// <summary>
    ///     Returns if this selector needs to be aligned before executing locally on this entity.
    /// </summary>
    public bool NonSelf => this.core != Core.s;
    /// <summary>
    ///     Returns if this selector selects ANY entities that are not players.
    /// </summary>
    public bool AnyNonPlayers
    {
        get
        {
            if (this.core != Core.e && this.core != Core.r)
                return false;
            if (this.entity.type != null && this.entity.type.Contains("player"))
                return false;
            if (this.entity.families != null && this.entity.families.Any(f => f.Contains("player")))
                return false;
            return true;
        }
    }
    /// <summary>
    ///     Returns if this selector selects ONLY entities that are not players.
    /// </summary>
    public bool AllNonPlayers
    {
        get
        {
            if (this.core == Core.a || this.core == Core.s || this.core == Core.p)
                return false;
            if (this.entity.type != null && !this.entity.type.Contains("player"))
                return true;
            if (this.entity.families != null && this.entity.families.Any(f => !f.Contains("player")))
                return true;
            return false;
        }
    }

    /// <summary>
    ///     Returns if this selector has an offset that is not entirely relative-zero.
    /// </summary>
    public bool UsesOffset => this.offsetX.HasEffect || this.offsetY.HasEffect || this.offsetZ.HasEffect;

    private static Core? TryParseCore(string core)
    {
        string originalCore = core;
        if (core.StartsWith('@'))
            core = core[1..];
        return core.ToUpper() switch
        {
            "P" => Core.p,
            "S" => Core.s,
            "A" => Core.a,
            "E" => Core.e,
            "R" => Core.r,
            _ => null
        };
    }
    public static Core? TryParseCore(char core)
    {
        return char.ToUpper(core) switch
        {
            'P' => Core.p,
            'S' => Core.s,
            'A' => Core.a,
            'E' => Core.e,
            'R' => Core.r,
            _ => null
        };
    }
    private static Core ParseCore(string core)
    {
        string originalCore = core;
        if (core.StartsWith('@'))
            core = core[1..];
        return core.ToUpper() switch
        {
            "P" => Core.p,
            "S" => Core.s,
            "A" => Core.a,
            "E" => Core.e,
            "R" => Core.r,
            "INITIATOR" or "I" => throw new FormatException(
                "Selector @i (@initiator) is unsupported. @s refers to the initating player inside dialogue."),
            _ => throw new FormatException($"Cannot parse selector \"{originalCore}\"")
        };
    }
    public static Core ParseCore(char core)
    {
        return char.ToUpper(core) switch
        {
            'P' => Core.p,
            'S' => Core.s,
            'A' => Core.a,
            'E' => Core.e,
            'R' => Core.r,
            'I' => throw new FormatException(
                "Selector @i (@initiator) is unsupported. @s refers to the initating player inside dialogue."),
            _ => throw new FormatException($"Cannot parse selector \"{core}\"")
        };
    }
    public static bool TryParse(string str, out Selector selector)
    {
        int bracket = str.IndexOf('[');

        Core? core;
        if (bracket == -1)
        {
            core = TryParseCore(str);
            if (!core.HasValue)
            {
                selector = null;
                return false;
            }

            selector = new Selector(core.Value);
            return true;
        }

        string coreSection = str[..bracket];
        string bracketSection = str[bracket..];

        core = TryParseCore(coreSection);
        if (!core.HasValue)
        {
            selector = null;
            return false;
        }

        selector = Parse(core.Value, bracketSection);
        return true;
    }
    public static Selector Parse(string str)
    {
        Core core;

        int bracket = str.IndexOf('[');

        if (bracket == -1)
        {
            core = ParseCore(str);
            return new Selector(core);
        }

        string coreSection = str[..bracket];
        string bracketSection = str[bracket..];

        core = ParseCore(coreSection);
        return Parse(core, bracketSection);
    }
    public static Selector Parse(Core core, string str)
    {
        str = str.TrimStart('[').TrimEnd(']');
        string[] chunks = str.Split(',')
            .Select(c => c.Trim()).ToArray();

        var selector = new Selector
        {
            core = core,
            area = Area.Parse(chunks),
            scores = Scores.Parse(str),
            hasItem = HasItems.Parse(str),
            count = Count.Parse(chunks),
            entity = Entity.Parse(chunks),
            player = Player.Parse(chunks)
        };

        foreach (string chunk in chunks)
        {
            int index = chunk.IndexOf('=');
            if (index == -1)
                continue;
            string a = chunk[..index].Trim().ToUpper();

            if (a.Equals("TAG"))
            {
                string b = chunk[(index + 1)..].Trim();
                selector.tags.Add(Tag.Parse(b));
            }
        }

        return selector;
    }

    public void Validate(Statement callingStatement)
    {
        string stringRepresentation = ToString();

        if (this.count.HasCount && this.count.count == 0)
            throw new StatementException(callingStatement,
                $"Selector '{stringRepresentation}' does not select any entities.");
    }

    /// <summary>
    ///     Returns the fully qualified minecraft command selector that this represents.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        List<string> parts = [];

        string sScores = this.scores.GetSection(),
            sHasItem = this.hasItem.GetSection(),
            sCount = this.count.GetSection();

        if (sScores != null)
            parts.Add(sScores);
        if (sHasItem != null)
            parts.Add(sHasItem);
        if (sCount != null)
            parts.Add(sCount);

        parts.AddRange(this.area.GetSections());
        parts.AddRange(this.entity.GetSections());
        parts.AddRange(this.player.GetSections());
        parts.AddRange(from tag in this.tags select tag.GetSection());

        if (parts.Count > 0)
            return '@' + this.core.ToString() + '[' + string.Join(",", parts) + ']';
        return '@' + this.core.ToString();
    }

    private bool Equals(Selector other)
    {
        return this.core == other.core && this.offsetX.Equals(other.offsetX) && this.offsetY.Equals(other.offsetY) &&
               this.offsetZ.Equals(other.offsetZ) && this.area.Equals(other.area) && this.scores.Equals(other.scores) &&
               this.hasItem.Equals(other.hasItem) && this.count.Equals(other.count) &&
               this.entity.Equals(other.entity) && this.player.Equals(other.player) && Equals(this.tags, other.tags);
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Selector) obj);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (int) this.core;
            hashCode = (hashCode * 397) ^ this.offsetX.GetHashCode();
            hashCode = (hashCode * 397) ^ this.offsetY.GetHashCode();
            hashCode = (hashCode * 397) ^ this.offsetZ.GetHashCode();
            hashCode = (hashCode * 397) ^ this.area.GetHashCode();
            hashCode = (hashCode * 397) ^ this.scores.GetHashCode();
            hashCode = (hashCode * 397) ^ this.hasItem.GetHashCode();
            hashCode = (hashCode * 397) ^ this.count.GetHashCode();
            hashCode = (hashCode * 397) ^ this.entity.GetHashCode();
            hashCode = (hashCode * 397) ^ this.player.GetHashCode();
            hashCode = (hashCode * 397) ^ (this.tags != null ? this.tags.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static Selector operator +(Selector a, Selector b)
    {
        var clone = (Selector) a.MemberwiseClone();

        clone.offsetX += b.offsetX;
        clone.offsetY += b.offsetY;
        clone.offsetZ += b.offsetZ;
        clone.area += b.area;
        clone.scores += b.scores;
        clone.hasItem += b.hasItem;
        clone.count += b.count;
        clone.entity += b.entity;
        clone.player += b.player;

        clone.tags = new List<Tag>(a.tags.Count + b.tags.Count);
        clone.tags.AddRange(a.tags);
        clone.tags.AddRange(b.tags);

        return clone;
    }
}

public class UnresolvedSelector
{
    private readonly Selector.Core core;
    private string remainderString;

    public UnresolvedSelector(Selector.Core core, string remainderString)
    {
        this.core = core;
        this.remainderString = remainderString;
    }
    public override string ToString()
    {
        if (!this.remainderString.StartsWith("["))
            this.remainderString = '[' + this.remainderString;
        if (!this.remainderString.EndsWith("]"))
            this.remainderString += ']';

        return $"@{this.core}{this.remainderString}";
    }

    /// <summary>
    ///     Resolve this selector so that it can be used.
    /// </summary>
    /// <param name="executor"></param>
    /// <returns></returns>
    public Selector Resolve(Executor executor)
    {
        string resolvedString = executor.ResolveStringV2(this.remainderString);
        return Selector.Parse(this.core, resolvedString);
    }
}