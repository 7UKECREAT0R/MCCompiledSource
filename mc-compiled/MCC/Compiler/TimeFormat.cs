using System;
using System.Text;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// An MCCompiled time format parsed from a string. e.g., "hh:mm:ss", "m:sss"
    /// </summary>
    public struct TimeFormat
    {
        public static readonly TimeFormat Default = new TimeFormat(0, 1, 2, TimeOption.m | TimeOption.s); // m:ss

        public TimeOption flags;
        public int minimumHours;
        public int minimumMinutes;
        public int minimumSeconds;

        public static TimeFormat Parse(string str)
        {
            TimeFormat format = new TimeFormat(0, 0, 0, TimeOption.none);

            foreach(char c in str.ToLower())
            {
                switch (c)
                {
                    case 'h':
                        format.flags |= TimeOption.h;
                        format.minimumHours++;
                        break;
                    case 'm':
                        format.flags |= TimeOption.m;
                        format.minimumMinutes++;
                        break;
                    case 's':
                        format.flags |= TimeOption.s;
                        format.minimumSeconds++;
                        break;
                    default:
                        continue;
                }
            }

            return format;
        }
        public TimeFormat(int minimumHours, int minimumMinutes, int minimumSeconds, TimeOption flags = TimeOption.none)
        {
            this.flags = flags;
            this.minimumHours = minimumHours;
            this.minimumMinutes = minimumMinutes;
            this.minimumSeconds = minimumSeconds;
        }
        public TimeFormat WithOption(TimeOption option)
        {
            this.flags |= option;
            return this;
        }
        public bool HasOption(TimeOption option)
        {
            return (this.flags & option) == option;
        }

        /// <summary>
        /// Regenerate the time string used to create this TimeFormat.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if ((this.flags & TimeOption.h) != 0)
            {
                sb.Append('h', this.minimumHours);
                sb.Append(':');
            }

            if ((this.flags & TimeOption.m) != 0)
            {
                sb.Append('m', this.minimumMinutes);
                sb.Append(":");
            }

            if ((this.flags & TimeOption.s) != 0)
                sb.Append('s', this.minimumSeconds);

            return sb.ToString();
        }
    }

    /// <summary>
    /// [Flags] A flags field representing the various options for time formatting: h, m, and s.
    /// </summary>
    [Flags]
    public enum TimeOption : byte
    {
        none = 0,
        h = 1 << 0,
        m = 1 << 1,
        s = 1 << 2
    }
}
