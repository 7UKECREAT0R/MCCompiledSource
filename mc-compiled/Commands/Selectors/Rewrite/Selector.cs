using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Rewrite
{
    /// <summary>
    /// A target selector.
    /// https://minecraft.fandom.com/wiki/Target_selectors
    /// </summary>
    public partial class Selector
    {
        public readonly SelectorCore core;

        public readonly SelectorField[] allFieldsList;
        public readonly Dictionary<string, SelectorField> allFields;



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



            this.allFieldsList = new SelectorField[]
            {
                // TODO put em here
            };
            this.allFields = new Dictionary<string, SelectorField>();
            foreach (SelectorField iteration in this.allFieldsList)
                this.allFields[iteration.name] = iteration;
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
        /// Parse a selector from a single string. If a core is already present, use <see cref="Parse(SelectorCore, string)"/>
        /// </summary>
        /// <param name="str">The full selector: "@e[name=Example]"</param>
        /// <returns></returns>
        public static Selector Parse(string str)
        {
            int opener = str.IndexOf('[');

            if(opener == -1)
                return new Selector(SelectorCoreUtils.Parse(str));

            string _core = str.Substring(0, opener);
            SelectorCore core = SelectorCoreUtils.Parse(_core);
            string body = str.Substring(opener + 1);

            return Parse(core, body);
        }
        /// <summary>
        /// Parse a selector from a core and a string.
        /// </summary>
        /// <param name="core">The core of the selector.</param>
        /// <param name="str">The bracketed part of the selector: "[name=Example]"</param>
        /// <returns></returns>
        public static Selector Parse(SelectorCore core, string str)
        {
            // get rid of brackets/whitespace
            str = str.Trim('[', ']', ' ', '\t');

            if (str.Length < 0)
                return new Selector(core);

            // get individual entries
            List<string> chunks = ParseChunks(str);

            Selector selector = new Selector(core);

            // process each entry into the selector

        }
        /// <summary>
        /// Gets individual "chunks" of selector from a string. e.g.:
        /// 
        /// <code>
        /// name="Luke",type=player,scores={winning=1,points=5..},r=10<br />
        /// name="Luke"   type=player   scores={winning=1,points=5..}   r=10
        /// </code>
        /// 
        /// The process isn't as simple as splitting by commas, it ignores
        /// commas where the bracket depth is greater than surface level.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static List<string> ParseChunks(string str)
        {
            List<string> chunks = new List<string>();

            // preallocate the maximum needed memory
            int length = str.Length;
            StringBuilder buffer = new StringBuilder(length);

            int depth = 0;

            for(int i = 0; i < length; i++)
            {
                char c = str[i];

                if (c == '[' || c == '{')
                    depth++;
                if (c == ']' || c == '}')
                    depth--;

                if(depth == 0 && c == ',')
                {
                    chunks.Add(buffer.ToString().Trim());
                    buffer.Clear();
                    continue;
                }

                buffer.Append(c);
            }

            if(buffer.Length > 0)
                chunks.Add(buffer.ToString().Trim());

            return chunks;
        }
        /// <summary>
        /// Parse a single chunk and apply it to a selector.
        /// </summary>
        /// <param name="chunk">The chunk to parse e.g., name="Luke"</param>
        /// <param name="modify">The selector to modify.</param>
        static void ParseChunk(string chunk, Selector modify)
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
