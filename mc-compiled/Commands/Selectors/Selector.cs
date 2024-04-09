using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a target selector.
    /// </summary>
    public class Selector
    {
        public enum Core
        {
            p,          // Nearest player
            s,          // Self
            a,          // All players
            e,          // All entities
            r,          // Random entity
            initiator,  // as dialogue initiator - not usable in regular code
        }

        private static Core ParseCore(string core)
        {
            string originalCore = core;
            if (core.StartsWith("@"))
                core = core.Substring(1);
            switch (core.ToUpper())
            {
                case "P":
                    return Core.p;
                case "S":
                    return Core.s;
                case "A":
                    return Core.a;
                case "E":
                    return Core.e;
                case "R":
                    return Core.r;
                case "INITIATOR":
                case "I":
                    throw new FormatException($"Selector @i (@initiator) is unsupported. @s refers to the initating player inside dialogue.");
                default:
                    throw new FormatException($"Cannot parse selector \"{originalCore}\"");
            }
        }
        public static Core ParseCore(char core)
        {
            switch (char.ToUpper(core))
            {
                case 'P':
                    return Core.p;
                case 'S':
                    return Core.s;
                case 'A':
                    return Core.a;
                case 'E':
                    return Core.e;
                case 'R':
                    return Core.r;
                case 'I':
                    throw new FormatException($"Selector @i (@initiator) is unsupported. @s refers to the initating player inside dialogue.");
                default:
                    throw new FormatException($"Cannot parse selector \"{core}\"");
            }
        }

        /// <summary>
        /// Returns if this selector targets multiple entities.
        /// </summary>
        public bool SelectsMultiple
        {
            get {
                if (this.count.count == 1)
                    return false;
                return this.core != Core.s && this.core != Core.p && this.core != Core.r;
            }
        }

        /// <summary>
        /// Returns if this selector needs to be aligned before executing locally on this entity.
        /// </summary>
        public bool NonSelf => this.core != Core.s;
        /// <summary>
        /// Returns if this selector selects ANY entities that are not players.
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
        /// Returns if this selector selects ONLY entities that are not players.
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
        
        public static readonly Selector INVOKER = new Selector(Core.initiator);
        public static readonly Selector NEAREST_PLAYER = new Selector(Core.p);
        public static readonly Selector SELF = new Selector(Core.s);
        public static readonly Selector ALL_PLAYERS = new Selector(Core.a);
        public static readonly Selector ALL_ENTITIES = new Selector(Core.e);

        public Selector()
        {
            this.scores = new Scores
            {
                checks = new List<ScoresEntry>()
            };
            this.hasItem = new HasItems
            {
                entries = new List<HasItemEntry>()
            };
            this.count = new Count
            {
                count = Count.NONE
            };
            this.area = new Area();
            this.entity = new Entity();
            this.player = new Player();
            this.tags = new List<Tag>();
        }
        public Selector(Core core)
        {
            this.core = core;
            this.scores = new Scores
            {
                checks = new List<ScoresEntry>()
            };
            this.hasItem = new HasItems
            {
                entries = new List<HasItemEntry>()
            };
            this.count = new Count
            {
                count = Count.NONE
            };
            this.area = new Area();
            this.entity = new Entity();
            this.player = new Player();
            this.tags = new List<Tag>();
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
            this.tags = new List<Tag>(copy.tags);
        }
        public static Selector Parse(string str)
        {
            Core core;

            int bracket = str.IndexOf('[');

            if(bracket == -1)
            {
                core = ParseCore(str);
                return new Selector(core);
            }

            string coreSection = str.Substring(0, bracket);
            string bracketSection = str.Substring(bracket);

            core = ParseCore(coreSection);
            return Parse(core, bracketSection);
        }
        public static Selector Parse(Core core, string str)
        {
            str = str.TrimStart('[').TrimEnd(']');
            string[] chunks = str.Split(',')
                .Select(c => c.Trim()).ToArray();

            Selector selector = new Selector()
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
                string a = chunk.Substring(0, index).Trim().ToUpper();

                if (a.Equals("TAG"))
                {
                    string b = chunk.Substring(index + 1).Trim();
                    selector.tags.Add(Tag.Parse(b));
                }
            }

            return selector;
        }

        public void Validate(Statement callingStatement)
        {
            string stringRepresentation = ToString();
            
            if (this.count.HasCount && this.count.count == 0)
                throw new StatementException(callingStatement, $"Selector '{stringRepresentation}' does not select any entities.");
            
        }
        
        public Core core;
        private Coordinate offsetX = Coordinate.here;
        private Coordinate offsetY = Coordinate.here;
        private Coordinate offsetZ = Coordinate.here;

        /// <summary>
        /// Returns if this selector has an offset that is not entirely relative-zero.
        /// </summary>
        public bool UsesOffset
        {
            get => this.offsetX.HasEffect || this.offsetY.HasEffect || this.offsetZ.HasEffect;
        }

        public Area area;         // The area where targets should be selected.
        public Scores scores;     // The scores that should be evaluated.
        public HasItems hasItem;  // The items which should be checked.
        public Count count;       // The limit of entities that can be selected.
        public Entity entity;     // The entity/player's status (name, rotation, etc.)
        public Player player;     // The player's specific stats (level, gamemode, etc.)
        public List<Tag> tags;    // The tags this entity/player has. Can have multiple.

        /// <summary>
        /// Returns the fully qualified minecraft command selector that this represents.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> parts = new List<string>();

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
            else return '@' + this.core.ToString();
        }

        private bool Equals(Selector other)
        {
            return this.core == other.core && this.offsetX.Equals(other.offsetX) && this.offsetY.Equals(other.offsetY) && this.offsetZ.Equals(other.offsetZ) && this.area.Equals(other.area) && this.scores.Equals(other.scores) && this.hasItem.Equals(other.hasItem) && this.count.Equals(other.count) && this.entity.Equals(other.entity) && this.player.Equals(other.player) && Equals(this.tags, other.tags);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Selector) obj);
        }

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
            var clone = (Selector)a.MemberwiseClone();

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
        public Selector.Core core;
        public string remainderString;

        public UnresolvedSelector(Selector.Core core, string remainderString)
        {
            this.core = core;
            this.remainderString = remainderString;
        }
        public override string ToString()
        {
            if (!this.remainderString.StartsWith("[")) this.remainderString = '[' + this.remainderString;
            if (!this.remainderString.EndsWith("]")) this.remainderString += ']';

            return $"@{this.core}{this.remainderString}";
        }

        /// <summary>
        /// Resolve this selector so that it can be used.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public Selector Resolve(Executor executor)
        {
            string resolvedString = executor.ResolveString(this.remainderString);
            return Selector.Parse(this.core, resolvedString);
        }
    }
}