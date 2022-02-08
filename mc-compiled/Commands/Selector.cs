using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    /// <summary>
    /// Represents a target selector.
    /// </summary>
    public class Selector
    {
        public enum Core
        {
            p,  // Nearest player
            s,  // Self
            a,  // All players
            e   // All entities
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
                default:
                    throw new FormatException($"Cannot parse selector \"{originalCore}\"");
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
                return core != Core.s && core != Core.p;
            }
        }
        /// <summary>
        /// Returns if this selector needs to be aligned before executing locally on this entity.
        /// </summary>
        public bool NeedsAlign
        {
            get => core != Core.s;
        }

        public Selector()
        {
            scores = new Selectors.Scores
            {
                checks = new List<Selectors.ScoresEntry>()
            };
            count = new Selectors.Count
            {
                count = Selectors.Count.NONE
            };
            area = new Selectors.Area();
            entity = new Selectors.Entity();
            player = new Selectors.Player();
            tags = new List<Selectors.Tag>();
            blockCheck = BlockCheck.DISABLED;
        }
        public Selector(Selector copy)
        {
            core = copy.core;
            area = copy.area;
            scores = new Selectors.Scores(new List<Selectors.ScoresEntry>(copy.scores.checks));
            count = copy.count;
            entity = copy.entity;
            player = copy.player;
            tags = new List<Selectors.Tag>(copy.tags);
            blockCheck = copy.blockCheck;
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

        public Core core;               // Base selector.

        public Selectors.Area area;     // The area where targets should be selected.
        public Selectors.Scores scores; // The scores that should be evaluated.
        public Selectors.Count count;   // The limit of entities that can be selected.
        public Selectors.Entity entity; // The entity/player's status (name, rotation, etc.)
        public Selectors.Player player; // The player's specific stats (level, gamemode, etc.)
        public List<Selectors.Tag> tags;// The tags this entity/player has. Can have multiple.
        public BlockCheck blockCheck;   // The block to check.

        /// <summary>
        /// Returns the fully qualified minecraft command selector that this represents.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> parts = new List<string>();

            string sScores = scores.GetSection(),
                   sCount = count.GetSection();
            if (sScores != null)
                parts.Add(sScores);
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
        public string GetAsPrefix()
        {
            if (blockCheck.present)
            {
                if (core == Core.p)
                {
                    core = Core.s;
                    string ret = $"execute @p ~~~ execute {ToString()} ~~~ {blockCheck} ";
                    core = Core.p;
                    return ret;
                }
                return $"execute {ToString()} ~~~ {blockCheck} ";
            }
            else
            {
                if (core == Core.p)
                {
                    core = Core.s;
                    string ret = $"execute @p ~~~ execute {ToString()} ~~~ ";
                    core = Core.p;
                    return ret;
                }
                return $"execute {ToString()} ~~~ ";
            }
        }
    }
}