using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// A target selector.
    /// https://minecraft.fandom.com/wiki/Target_selectors
    /// </summary>
    public partial class Selector
    {
        public readonly SelectorCore core;

        public readonly SelectorField[] allFields;

        // Position
        public readonly SelectorField<Coord> x;
        public readonly SelectorField<Coord> y;
        public readonly SelectorField<Coord> z;
        public readonly SelectorField<float> radiusMin;
        public readonly SelectorField<float> radiusMax;
        public readonly SelectorField<int> volumeX;
        public readonly SelectorField<int> volumeY;
        public readonly SelectorField<int> volumeZ;

        // Values
        public readonly SelectorField<List<HasScoreCheck>> scores;
        public readonly SelectorField<List<string>> tags;

        // Entity Species
        public readonly SelectorField<string> name;
        public readonly SelectorField<string> type;
        public readonly SelectorField<List<string>> families;

        // Entity Data
        public readonly SelectorField<int> rotationXMin;
        public readonly SelectorField<int> rotationXMax;
        public readonly SelectorField<int> rotationYMin;
        public readonly SelectorField<int> rotationYMax;
        public readonly SelectorField<List<HasItemCheck>> hasitem;

        // Player Data
        public readonly SelectorField<int> levelMin;
        public readonly SelectorField<int> levelMax;
        public readonly SelectorField<GameMode> gamemode;

        /// <summary>
        /// Limit to the maximum number of entities this selector can target, sorted from closest to furthest.
        /// </summary>
        public int? limit;
        /// <summary>
        /// The block to check for, if any.
        /// </summary>
        public BlockCheck blockCheck;

        public Coord offsetX, offsetY, offsetZ;

        public Selector(SelectorCore core)
        {
            this.core = core;
            this.blockCheck = BlockCheck.DISABLED;
            this.limit = null;

            this.offsetX = Coord.here;
            this.offsetY = Coord.here;
            this.offsetZ = Coord.here;

            this.x = new SelectorField<Coord>("x", this);
            this.y = new SelectorField<Coord>("y", this);
            this.z = new SelectorField<Coord>("z", this);
            this.radiusMin = new SelectorField<float>("rm", this);
            this.radiusMax = new SelectorField<float>("r", this);
            this.volumeX = new SelectorField<int>("dx", this);
            this.volumeY = new SelectorField<int>("dy", this);
            this.volumeZ = new SelectorField<int>("dz", this);

            this.scores = new SelectorField<List<HasScoreCheck>>("scores", this).WithResultProvider(field =>
            {
                if (field.Value.Count == 0)
                    return new string[0];

                string inner = string.Join(",", field.Value);
                return new[] { "scores={" + inner + '}' };
            });

            this.tags = new SelectorField<List<string>>("tag", this).WithResultProvider(field =>
            {
                if (field.Value.Count == 0)
                    return new string[0];

                return field.Value.Select(tag => "tag=\"" + tag + '\"').ToArray();
            });
            this.scores.SetValue(new List<HasScoreCheck>());
            this.tags.SetValue(new List<string>());

            this.name = new SelectorField<string>("name", this);
            this.type = new SelectorField<string>("type", this);
            this.families = new SelectorField<List<string>>("family", this).WithResultProvider(field =>
            {
                if (field.Value.Count == 0)
                    return new string[0];

                return field.Value.Select(family => "family=" + family).ToArray();
            });
            this.families.SetValue(new List<string>());

            this.rotationXMin = new SelectorField<int>("rxm", this);
            this.rotationXMax = new SelectorField<int>("rx", this);
            this.rotationYMin = new SelectorField<int>("rym", this);
            this.rotationYMax = new SelectorField<int>("ry", this);
            this.hasitem = new SelectorField<List<HasItemCheck>>("hasitem", this).WithResultProvider(field =>
            {
                if (field.Value.Count == 0)
                    return new string[0];
                if (field.Value.Count == 1)
                    return new[] { "hasitem={" + field.Value[0].ToString() + '}' };

                string inner = '{' + string.Join("},{", field.Value) + '}';
                return new[] { "hasitem=[" + inner + ']' };
            });
            this.hasitem.SetValue(new List<HasItemCheck>());

            this.levelMin = new SelectorField<int>("lm", this);
            this.levelMax = new SelectorField<int>("l", this);
            this.gamemode = new SelectorField<GameMode>("m", this).WithResultProvider(field =>
            {
                return new[] { ((int)field.Value).ToString() };
            });
        }

        /// <summary>
        /// Returns if this selector selects multiple entities, or just one.
        /// </summary>
        public bool SelectsMultipleEntities
        {
            get
            {
                if (limit.HasValue && limit.Value == 1)
                    return false;
                return core.SelectsMultiple();
            }
        }
        /// <summary>
        /// Returns if this selector needs to be aligned before being used properly.
        /// </summary>
        public bool NeedsAlignment
        {
            get => core.IsMisaligned();
        }

        /// <summary>
        /// Parse a selector from a core and a string.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Selector Parse(SelectorCore core, string str)
        {

        }
        static List<string> GetParts(string str)
        {

        }
    }

    public static class SelectorCoreUtils
    {
        /// <summary>
        /// Parse a core from a string: @e, @initiator, p, etc...
        /// </summary>
        /// <param name="str">A string with/without an @ symbol.</param>
        /// <returns></returns>
        public static SelectorCore Parse(string str)
        {
            if (str.StartsWith("@"))
                str = str.Substring(1);

            switch (str)
            {
                case "a":
                    return SelectorCore.all_players;
                case "e":
                    return SelectorCore.all_entities;
                case "i":
                case "initiator":
                    return SelectorCore.initiator;
                case "p":
                    return SelectorCore.nearest_player;
                case "s":
                default:
                    return SelectorCore.self;
            }
        }
        /// <summary>
        /// Convert this core to its appropriate string, including the @ symbol.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string ToString(this SelectorCore core)
        {
            switch (core)
            {
                case SelectorCore.self:
                    return "@s";
                case SelectorCore.nearest_player:
                    return "@p";
                case SelectorCore.all_players:
                    return "@a";
                case SelectorCore.all_entities:
                    return "@e";
                case SelectorCore.initiator:
                    return "@initiator";
                default:
                    return "???";
            }
        }
        /// <summary>
        /// Create a new selector from this 
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Selector CreateFrom(this SelectorCore core) =>
            new Selector(core);
        /// <summary>
        /// Returns if this core selects multiple entities, or just one.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool SelectsMultiple(this SelectorCore core)
        {
            switch (core)
            {
                case SelectorCore.self:
                    return false;
                case SelectorCore.nearest_player:
                    return false;
                case SelectorCore.all_players:
                    return true; 
                case SelectorCore.all_entities:
                    return true;
                case SelectorCore.initiator:
                    return false;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Returns if this core needs to be aligned using /execute before being used.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool IsMisaligned(this SelectorCore core)
        {
            switch (core)
            {
                case SelectorCore.self:
                    return false;
                case SelectorCore.nearest_player:
                    return true;
                case SelectorCore.all_players:
                    return true;
                case SelectorCore.all_entities:
                    return true;
                case SelectorCore.initiator:
                    return false;
                default:
                    return false;
            }
        }
    }
    public enum SelectorCore
    {
        self,           // @s
        nearest_player, // @p
        all_players,    // @a
        all_entities,   // @e
        initiator       // @i
    }
}
