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
            initiator   // Initiator of Dialogue Button
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
                case "INITIATOR":
                case "I":
                    return Core.e;
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
                case 'I':
                    return Core.e;
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
                if (count.count == 1)
                    return false;
                return core != Core.s && core != Core.p && core != Core.initiator;
            }
        }

        /// <summary>
        /// Returns if this selector needs to be aligned before executing locally on this entity.
        /// </summary>
        public bool NonSelf => core != Core.s && core != Core.initiator;

        public static readonly Selector NEAREST_PLAYER = new Selector(Core.p);
        public static readonly Selector SELF = new Selector(Core.s);
        public static readonly Selector ALL_PLAYERS = new Selector(Core.a);
        public static readonly Selector ALL_ENTITIES = new Selector(Core.e);
        public static readonly Selector INITIATOR = new Selector(Core.initiator);

        public Selector()
        {
            scores = new Selectors.Scores
            {
                checks = new List<Selectors.ScoresEntry>()
            };
            hasItem = new Selectors.HasItems
            {
                entries = new List<Selectors.HasItemEntry>()
            };
            count = new Selectors.Count
            {
                count = Selectors.Count.NONE
            };
            area = new Selectors.Area();
            entity = new Selectors.Entity();
            player = new Selectors.Player();
            tags = new List<Selectors.Tag>();
        }
        public Selector(Core core)
        {
            this.core = core;
            scores = new Selectors.Scores
            {
                checks = new List<Selectors.ScoresEntry>()
            };
            hasItem = new Selectors.HasItems
            {
                entries = new List<Selectors.HasItemEntry>()
            };
            count = new Selectors.Count
            {
                count = Selectors.Count.NONE
            };
            area = new Selectors.Area();
            entity = new Selectors.Entity();
            player = new Selectors.Player();
            tags = new List<Selectors.Tag>();
        }
        public Selector(Selector copy)
        {
            core = copy.core;
            area = copy.area;
            scores = new Selectors.Scores(new List<Selectors.ScoresEntry>(copy.scores.checks));
            hasItem = new Selectors.HasItems(new List<Selectors.HasItemEntry>(copy.hasItem.entries));
            count = copy.count;
            entity = copy.entity;
            player = copy.player;
            tags = new List<Selectors.Tag>(copy.tags);
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
                area = Selectors.Area.Parse(chunks),
                scores = Selectors.Scores.Parse(str),
                hasItem = Selectors.HasItems.Parse(str),
                count = Selectors.Count.Parse(chunks),
                entity = Selectors.Entity.Parse(chunks),
                player = Selectors.Player.Parse(chunks)
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
                    selector.tags.Add(Selectors.Tag.Parse(b));
                }
            }

            return selector;
        }

        public Core core;
        public Coordinate offsetX = Coordinate.here;
        public Coordinate offsetY = Coordinate.here;
        public Coordinate offsetZ = Coordinate.here;

        /// <summary>
        /// Returns if this selector has an offset that is not entirely relative-zero.
        /// </summary>
        public bool UsesOffset
        {
            get => offsetX.HasEffect || offsetY.HasEffect || offsetZ.HasEffect;
        }

        public Selectors.Area area;         // The area where targets should be selected.
        public Selectors.Scores scores;     // The scores that should be evaluated.
        public Selectors.HasItems hasItem;  // The items which should be checked.
        public Selectors.Count count;       // The limit of entities that can be selected.
        public Selectors.Entity entity;     // The entity/player's status (name, rotation, etc.)
        public Selectors.Player player;     // The player's specific stats (level, gamemode, etc.)
        public List<Selectors.Tag> tags;    // The tags this entity/player has. Can have multiple.

        /// <summary>
        /// Returns the fully qualified minecraft command selector that this represents.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> parts = new List<string>();

            string sScores = scores.GetSection(),
                sHasItem = hasItem.GetSection(),
                sCount = count.GetSection();

            if (sScores != null)
                parts.Add(sScores);
            if (sHasItem != null)
                parts.Add(sHasItem);
            if (sCount != null)
                parts.Add(sCount);

            parts.AddRange(area.GetSections());
            parts.AddRange(entity.GetSections());
            parts.AddRange(player.GetSections());
            parts.AddRange(from tag in tags select tag.GetSection());

            if (parts.Count > 0)
                return '@' + core.ToString() + '[' + string.Join(",", parts) + ']';
            else return '@' + core.ToString();
        }

        private bool Equals(Selector other)
        {
            return core == other.core && offsetX.Equals(other.offsetX) && offsetY.Equals(other.offsetY) &&
                   offsetZ.Equals(other.offsetZ) && area.Equals(other.area) && scores.Equals(other.scores) &&
                   hasItem.Equals(other.hasItem) && count.Equals(other.count) && entity.Equals(other.entity) &&
                   player.Equals(other.player) && Equals(tags, other.tags);
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
                int hashCode = (int) core;
                hashCode = (hashCode * 397) ^ offsetX.GetHashCode();
                hashCode = (hashCode * 397) ^ offsetY.GetHashCode();
                hashCode = (hashCode * 397) ^ offsetZ.GetHashCode();
                hashCode = (hashCode * 397) ^ area.GetHashCode();
                hashCode = (hashCode * 397) ^ scores.GetHashCode();
                hashCode = (hashCode * 397) ^ hasItem.GetHashCode();
                hashCode = (hashCode * 397) ^ count.GetHashCode();
                hashCode = (hashCode * 397) ^ entity.GetHashCode();
                hashCode = (hashCode * 397) ^ player.GetHashCode();
                hashCode = (hashCode * 397) ^ (tags != null ? tags.GetHashCode() : 0);
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

            clone.tags = new List<Selectors.Tag>(a.tags.Count + b.tags.Count);
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
            if (!remainderString.StartsWith("["))
                remainderString = '[' + remainderString;
            if (!remainderString.EndsWith("]"))
                remainderString += ']';

            return $"@{core}{remainderString}";
        }

        /// <summary>
        /// Resolve this selector so that it can be used.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public Selector Resolve(Executor executor)
        {
            string resolvedString = executor.ResolveString(remainderString);
            return Selector.Parse(core, resolvedString);
        }
    }
}