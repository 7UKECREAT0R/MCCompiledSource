using System;

namespace mc_compiled.Modding
{
    public struct FormatVersion
    {
        // https://wiki.bedrock.dev/guide/format-version.html#format-versions-per-asset-type
        public static readonly FormatVersion r_ENTITY = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion r_ANIMATION = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion r_ATTACHABLE = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion r_MODEL = new FormatVersion(1, 12, 0);
        public static readonly FormatVersion r_PARTICLE = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion r_RENDER_CONTROLLER = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion r_PARTICLES = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion r_SOUNDS = new FormatVersion(1, 18, 10);
        public static readonly FormatVersion b_ANIMATION_CONTROLLER = new FormatVersion(1, 10, 0);
        public static readonly FormatVersion b_ENTITY = new FormatVersion(1, 16, 0);
        public static readonly FormatVersion b_ITEM = new FormatVersion(1, 10);
        public static readonly FormatVersion b_RECIPE = new FormatVersion(1, 16);
        public static readonly FormatVersion b_SPAWNRULE = new FormatVersion(1, 8, 0);
        public static readonly FormatVersion b_DIALOGUE = new FormatVersion(1, 17, 0);

        public readonly int release, major;
        public readonly int? minor;
        public FormatVersion(int release, int major, int? minor = null)
        {
            this.release = release;
            this.major = major;
            this.minor = minor;
        }
        public FormatVersion(string version)
        {
            string[] parts = version.Split('.');

            if (parts.Length < 2)
                throw new Exception("Format version was missing information.");

            if (parts.Length == 2)
            {
                release = int.Parse(parts[0]);
                major = int.Parse(parts[1]);
                minor = null;
            } else
            {
                release = int.Parse(parts[0]);
                major = int.Parse(parts[1]);
                minor = int.Parse(parts[2]);
            }
        }

        /// <summary>
        /// Convert this format version to a proper string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (minor.HasValue)
                return $"{release}.{major}.{minor.Value}";

            return $"{release}.{major}";
        }
    }
}
