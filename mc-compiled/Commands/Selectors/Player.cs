using mc_compiled.MCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static mc_compiled.Commands.Command;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents player specific selector options.
    /// </summary>
    public struct Player
    {
        public GameMode?
            gamemode;
        public bool
            gamemodeNot;
        public int?
            levelMin,
            levelMax;

        public Player(GameMode? gamemode, int? levelMin = null, int? levelMax = null)
        {
            gamemodeNot = false;
            this.gamemode = gamemode;
            this.levelMin = levelMin;
            this.levelMax = levelMax;
        }
        public static Player Parse(string[] chunks)
        {
            GameMode? gamemode = null;
            int? levelMin = null;
            int? levelMax = null;

            foreach (string chunk in chunks)
            {
                int index = chunk.IndexOf('=');
                if (index == -1)
                    continue;
                string a = chunk.Substring(0, index).Trim().ToUpper();
                string b = chunk.Substring(index + 1).Trim();

                switch (a)
                {
                    case "M":
                        gamemode = ParseGameMode(b.Trim('\"'));
                        break;
                    case "LM":
                        levelMin = int.Parse(b);
                        break;
                    case "L":
                        levelMax = int.Parse(b);
                        break;
                }
            }

            return new Player(gamemode, levelMin, levelMax);
        }

        public void InsertGameMode(GameMode? mode, bool not)
        {
            gamemodeNot = not;
            gamemode = mode;
        }
        public static GameMode? ParseGameMode(string str)
        {
            switch (str.ToUpper())
            {
                case "S":
                case "SURVIVAL":
                case "0":
                    return GameMode.survival;
                case "C":
                case "CREATIVE":
                case "1":
                    return GameMode.creative;
                case "A":
                case "ADVENTURE":
                case "2":
                    return GameMode.adventure;
                default:
                    return null;
            }
        }

        public string[] GetSections()
        {
            List<string> strings = new List<string>();
            if (gamemode.HasValue)
                strings.Add("m=" + (gamemodeNot?"!":"") + ((int)gamemode.Value));
            if (levelMin.HasValue)
                strings.Add("l=" + levelMin.Value);
            if (levelMax.HasValue)
                strings.Add("lm=" + levelMax.Value);
            return strings.ToArray();
        }

        public string AsStoreIn(string selector, string objective)
        {
            IEnumerable<string> parts = GetSections();
            string tags = string.Join(",", parts);

            return $"execute {selector}[{tags}] ~~~ scoreboard players set @s {objective} 1";
        }
    }

}
