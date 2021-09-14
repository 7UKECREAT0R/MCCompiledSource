using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Limits
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

        public void ParseGamemode(string parse, bool not)
        {
            gamemodeNot = not;
            switch (parse.ToUpper())
            {
                case "S":
                case "SURVIVAL":
                case "0":
                    gamemode = GameMode.SURVIVAL;
                    break;
                case "C":
                case "CREATIVE":
                case "1":
                    gamemode = GameMode.CREATIVE;
                    break;
                case "A":
                case "ADVENTURE":
                case "2":
                    gamemode = GameMode.ADVENTURE;
                    break;
                default:
                    throw new FormatException("Not a valid gamemode.");
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
    }
    public enum GameMode : int
    {
        SURVIVAL = 0,
        CREATIVE = 1,
        ADVENTURE = 2
    }
}
