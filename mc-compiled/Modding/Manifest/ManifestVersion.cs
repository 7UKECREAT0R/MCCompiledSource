using System;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest
{
    /// <summary>
    /// Represents a semantic version array with three integers (release, major, minor)
    /// </summary>
    public class ManifestVersion
    {
        // https://wiki.bedrock.dev/guide/format-version.html#format-versions-per-asset-type
        public static readonly ManifestVersion r_ENTITY = Parse("1.10.0");
        public static readonly ManifestVersion r_ANIMATION = Parse("1.10.0");
        public static readonly ManifestVersion r_ATTACHABLE = Parse("1.10.0");
        public static readonly ManifestVersion r_MODEL = Parse("1.12.0");
        public static readonly ManifestVersion r_RENDER_CONTROLLER = Parse("1.10.0");
        public static readonly ManifestVersion r_PARTICLES = Parse("1.10.0");
        public static readonly ManifestVersion r_SOUND_DEFINITIONS = Parse("1.20.20");
        public static readonly ManifestVersion b_ANIMATION_CONTROLLER = Parse("1.10.0");
        public static readonly ManifestVersion b_ENTITY = Parse("1.16.0");
        public static readonly ManifestVersion b_ITEM = Parse("1.10");
        public static readonly ManifestVersion b_RECIPE = Parse("1.16");
        public static readonly ManifestVersion b_SPAWN_RULE = Parse("1.8.0");
        public static readonly ManifestVersion b_DIALOGUE = Parse("1.17.0"); // no clue where I pulled this number from, but it works

        /// <summary>
        /// The current `min_engine_version` required by MCCompiled's feature-set.
        /// </summary>
        public static readonly ManifestVersion MIN_ENGINE_VERSION = Parse("1.20.80");
        /// <summary>
        /// The default 1.0 manifest version. Used for new un-versioned manifests being generated.
        /// </summary>
        public static readonly ManifestVersion DEFAULT = Parse("1.0.0");
        
        private readonly int release;
        private readonly int major;
        private readonly int? minor;
        
        /// <summary>
        /// Tries to parse an array of integers as a VersionArray object.
        /// </summary>
        /// <param name="versions">An array of integers representing the version numbers.</param>
        /// <param name="result">When this method returns, contains the parsed VersionArray if successful, or null if failed to parse.</param>
        /// <returns>true if the parsing was successful; otherwise, false.</returns>
        public static bool TryParse(int[] versions, out ManifestVersion result)
        {
            if (versions.Length < 2)
            {
                result = null;
                return false;
            }
            
            if (versions.Length == 2)
            {
                result = new ManifestVersion(
                    versions[0],
                    versions[1]
                );
            }
            else
            {
                result = new ManifestVersion(
                    versions[0],
                    versions[1],
                    versions[2]
                );
            }
            
            return true;
        }
        /// <summary>
        /// Tries to parse the input as a ManifestVersion and returns a boolean indicating whether the parsing was successful.
        /// </summary>
        /// <param name="input">JSON array of integers representing the verison numbers.</param>
        /// <param name="output">When this method returns, contains the parsed ManifestVersion if the parsing was successful, or null if the parsing failed.</param>
        /// <returns>
        /// true if the input was successfully parsed; otherwise, false.
        /// </returns>
        public static bool TryParse(JArray input, out ManifestVersion output)
        {
            if (input.Count < 2)
            {
                output = null;
                return false;
            }

            if (input.Count > 2)
            {
                output = new ManifestVersion(
                    input[0].Value<int>(),
                    input[1].Value<int>(),
                    input[2].Value<int>()
                );
            }
            else
            {
                output = new ManifestVersion(
                    input[0].Value<int>(),
                    input[1].Value<int>()
                );
            }

            return true;
        }
        /// <summary>
        /// Tries to parse a string representation of a manifest version and returns a value indicating whether the parsing was successful.
        /// </summary>
        /// <param name="input">The string representation of a manifest version.</param>
        /// <param name="output">When this method returns, contains the parsed ManifestVersion object if the parsing was successful, or null if the parsing failed.</param>
        /// <returns>true if the parsing was successful; otherwise, false.</returns>
        public static bool TryParse(string input, out ManifestVersion output)
        {
            string[] parts = input.Split('.');
            if (parts.Length < 2)
            {
                output = null;
                return false;
            }

            if (parts.Length == 2)
            {
                if (!int.TryParse(parts[0], out int release) ||
                    !int.TryParse(parts[1], out int major))
                {
                    output = null;
                    return false;
                }

                output = new ManifestVersion(release, major);
            }
            else
            {
                if (!int.TryParse(parts[0], out int release) ||
                    !int.TryParse(parts[1], out int major) ||
                    !int.TryParse(parts[2], out int minor))
                {
                    output = null;
                    return false;
                }

                output = new ManifestVersion(release, major, minor);
            }
            
            return true;
        }
        /// <summary>
        /// Tries to parse a JSON token as a ManifestVersion object, whether it be a string or array internally.
        /// </summary>
        /// <param name="input">The JSON token to parse.</param>
        /// <param name="output">When this method returns, contains the parsed ManifestVersion if successful, or null if failed to parse.</param>
        /// <returns>true if the parsing was successful; otherwise, false.</returns>
        public static bool TryParseToken(JToken input, out ManifestVersion output)
        {
            switch (input.Type)
            {
                case JTokenType.String:
                    return TryParse(input.ToString(), out output);
                case JTokenType.Array:
                    return TryParse((JArray)input, out output);
                case JTokenType.None:
                case JTokenType.Object:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                default:
                    output = null;
                    return false;
            }
        }
        
        /// <summary>
        /// Parses a string representation of a manifest version.
        /// </summary>
        /// <param name="input">The string representation of a manifest version.</param>
        /// <returns>A new ManifestVersion object if the parsing was successful.</returns>
        private static ManifestVersion Parse(string input)
        {
            string[] parts = input.Split('.');

            if (parts.Length < 2)
            {
                throw new FormatException("Invalid manifest version format.");
            }

            if (parts.Length == 2)
            {
                if (!int.TryParse(parts[0], out int release) ||
                    !int.TryParse(parts[1], out int major))
                {
                    throw new FormatException("Invalid manifest version format.");
                }

                return new ManifestVersion(release, major);
            }
            else
            {
                if (!int.TryParse(parts[0], out int release) ||
                    !int.TryParse(parts[1], out int major) ||
                    !int.TryParse(parts[2], out int minor))
                {
                    throw new FormatException("Invalid manifest version format.");
                }

                return new ManifestVersion(release, major, minor);
            }
        }

        public ManifestVersion(ManifestVersion copy)
        {
            this.release = copy.release;
            this.major = copy.major;
            this.minor = copy.minor;
        }
        private ManifestVersion(int release, int major, int? minor = null)
        {
            this.release = release;
            this.major = major;
            this.minor = minor;
        }
        
        /// <summary>
        /// Converts this ManifestVersion to a JSON array containing the release, major and minor (if applicable) versions.
        /// </summary>
        /// <returns>A JSON array representing the ManifestVersion object.</returns>
        public JArray ToJSON()
        {
            return this.minor.HasValue ?
                new JArray(this.release, this.major, this.minor.Value) :
                new JArray(this.release, this.major);
        }
        /// <summary>
        /// Converts this ManifestVersion to a string representing it as a semantic version separated by periods. e.g., "1.10.0"
        /// </summary>
        /// <returns></returns>
        public string ToVersionString()
        {
            return this.minor.HasValue ?
                $"{this.release}.{this.major}.{this.minor.Value}" :
                $"{this.release}.{this.major}";
        }

        private bool Equals(ManifestVersion other)
        {
            return this.release == other.release &&
                   this.major == other.major &&
                   Nullable.Equals(this.minor, other.minor);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ManifestVersion)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.release;
                hashCode = (hashCode * 397) ^ this.major;
                hashCode = (hashCode * 397) ^ this.minor.GetHashCode();
                return hashCode;
            }
        }
        
        public static bool operator <(ManifestVersion a, ManifestVersion b)
        {
            if (a.release < b.release)
                return true;
            if (a.release > b.release)
                return false;

            if (a.major < b.major)
                return true;
            if (a.major > b.major)
                return false;

            return a.minor < b.minor;
        }

        public static bool operator >(ManifestVersion a, ManifestVersion b)
        {
            if (a.release > b.release)
                return true;
            if (a.release < b.release)
                return false;

            if (a.major > b.major)
                return true;
            if (a.major < b.major)
                return false;

            return a.minor > b.minor;
        }
        public static bool operator <=(ManifestVersion a, ManifestVersion b)
        {
            return a < b || a.Equals(b);
        }
        public static bool operator >=(ManifestVersion a, ManifestVersion b)
        {
            return a > b || a.Equals(b);
        }
    }
}