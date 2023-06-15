using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static Core ParseCore(string core)
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
        public bool NonSelf
        {
            get => core != Core.s && core != Core.initiator;
        }

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
        public Coord offsetX = Coord.here;
        public Coord offsetY = Coord.here;
        public Coord offsetZ = Coord.here;

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

        public override bool Equals(object obj)
        {
            return obj is Selector selector &&
                   SelectsMultiple == selector.SelectsMultiple &&
                   NonSelf == selector.NonSelf &&
                   core == selector.core &&
                   EqualityComparer<Coord>.Default.Equals(offsetX, selector.offsetX) &&
                   EqualityComparer<Coord>.Default.Equals(offsetY, selector.offsetY) &&
                   EqualityComparer<Coord>.Default.Equals(offsetZ, selector.offsetZ) &&
                   EqualityComparer<Area>.Default.Equals(area, selector.area) &&
                   EqualityComparer<Scores>.Default.Equals(scores, selector.scores) &&
                   EqualityComparer<HasItems>.Default.Equals(hasItem, selector.hasItem) &&
                   EqualityComparer<Count>.Default.Equals(count, selector.count) &&
                   EqualityComparer<Entity>.Default.Equals(entity, selector.entity) &&
                   EqualityComparer<Player>.Default.Equals(player, selector.player) &&
                   EqualityComparer<List<Tag>>.Default.Equals(tags, selector.tags);
        }
        public override int GetHashCode()
        {
            int hashCode = 1214864800;
            hashCode = hashCode * -1521134295 + SelectsMultiple.GetHashCode();
            hashCode = hashCode * -1521134295 + NonSelf.GetHashCode();
            hashCode = hashCode * -1521134295 + core.GetHashCode();
            hashCode = hashCode * -1521134295 + offsetX.GetHashCode();
            hashCode = hashCode * -1521134295 + offsetY.GetHashCode();
            hashCode = hashCode * -1521134295 + offsetZ.GetHashCode();
            hashCode = hashCode * -1521134295 + area.GetHashCode();
            hashCode = hashCode * -1521134295 + scores.GetHashCode();
            hashCode = hashCode * -1521134295 + hasItem.GetHashCode();
            hashCode = hashCode * -1521134295 + count.GetHashCode();
            hashCode = hashCode * -1521134295 + entity.GetHashCode();
            hashCode = hashCode * -1521134295 + player.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Tag>>.Default.GetHashCode(tags);
            return hashCode;
        }

        public static Selector operator +(Selector a, Selector b)
        {
            Selector clone = (Selector)a.MemberwiseClone();

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
                remainderString = remainderString + ']';

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