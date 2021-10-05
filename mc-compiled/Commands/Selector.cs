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
            @p, @s, @a, @e
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

        public Selector()
        {
            scores = new Limits.Scores
            {
                checks = new List<Limits.ScoresEntry>()
            };
            count = new Limits.Count
            {
                count = Limits.Count.NONE
            };
            area = new Limits.Area();
            entity = new Limits.Entity();
            player = new Limits.Player();
            tags = new List<Limits.Tag>();
            blockCheck = BlockCheck.DISABLED;
        }
        public Selector(Selector copy)
        {
            core = copy.core;
            area = copy.area;
            scores = new Limits.Scores(new List<Limits.ScoresEntry>(copy.scores.checks));
            count = copy.count;
            entity = copy.entity;
            player = copy.player;
            tags = new List<Limits.Tag>(copy.tags);
            blockCheck = copy.blockCheck;
        }

        public Core core;              // Base selector.

        public Limits.Area area;       // The area where targets should be selected.
        public Limits.Scores scores;   // The scores that should be evaluated.
        public Limits.Count count;     // The limit of entities that can be selected.
        public Limits.Entity entity;   // The entity/player's status (name, rotation, etc.)
        public Limits.Player player;   // The player's specific stats (level, gamemode, etc.)
        public List<Limits.Tag> tags;   // The tags this entity/player has. Can have multiple.
        public BlockCheck blockCheck;

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