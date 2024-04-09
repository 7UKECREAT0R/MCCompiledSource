using System.Collections.Generic;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents player specific selector options.
    /// </summary>
    public struct Player
    {
        private GameMode? gamemode;
        private bool gamemodeNot;
        private int? levelMin;
        private int? levelMax;

        private Player(GameMode? gamemode, int? levelMin = null, int? levelMax = null)
        {
            this.gamemodeNot = false;
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
                    case "MODE":
                        gamemode = ParseGameMode(b.Trim('\"'));
                        break;
                    case "LM":
                        if (int.TryParse(b, out int lm))
                            levelMin = lm;
                        break;
                    case "L":
                        if (int.TryParse(b, out int l))
                            levelMax = l;
                        break;
                }
            }

            return new Player(gamemode, levelMin, levelMax);
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
                case "SPECTATOR":
                case "6":
                    return GameMode.spectator;
                default:
                    return null;
            }
        }

        public string[] GetSections()
        {
            var strings = new List<string>();
            if (this.gamemode.HasValue)
                strings.Add("m=" + (this.gamemodeNot?"!":"") + ((int) this.gamemode.Value));
            if (this.levelMin.HasValue)
                strings.Add("lm=" + this.levelMin.Value);
            if (this.levelMax.HasValue)
                strings.Add("l=" + this.levelMax.Value);
            return strings.ToArray();
        }

        public string AsStoreIn(string selector, string objective)
        {
            IEnumerable<string> parts = GetSections();
            string tags = string.Join(",", parts);

            return $"execute {selector}[{tags}] ~~~ scoreboard players set @s {objective} 1";
        }

        public bool Equals(Player other)
        {
            return this.gamemode == other.gamemode && this.gamemodeNot == other.gamemodeNot && this.levelMin == other.levelMin && this.levelMax == other.levelMax;
        }
        public override bool Equals(object obj)
        {
            return obj is Player other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.gamemode.GetHashCode();
                hashCode = (hashCode * 397) ^ this.gamemodeNot.GetHashCode();
                hashCode = (hashCode * 397) ^ this.levelMin.GetHashCode();
                hashCode = (hashCode * 397) ^ this.levelMax.GetHashCode();
                return hashCode;
            }
        }

        public static Player operator +(Player a, Player other)
        {
            if (a.gamemode == null)
                a.gamemode = other.gamemode;
            if (a.levelMin == null)
                a.levelMin = other.levelMin;
            if (a.levelMax == null)
                a.levelMax = other.levelMax;
            return a;
        }
    }

}
