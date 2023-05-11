using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mc_compiled.Modding.Resources.Localization
{
    /// <summary>
    /// A .lang file.
    /// </summary>
    internal class Lang : IAddonFile
    {
        private const string HEADER = "MCCompiled Entries";
        private int headerIndex = -1; // index of the MCCompiled header in the lang file.

        private readonly string localeName;
        private LocaleDefinition locale;
        private List<LangEntry> lines;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localeName"></param>
        /// <param name="lines"></param>
        internal Lang(string localeName, LangEntry[] lines = null)
        {
            this.localeName = localeName;
            this.locale = null;
            this.lines = new List<LangEntry>();

            if (lines != null)
                this.lines.AddRange(lines);

            FindHeader();
        }
        /// <summary>
        /// Create a new Lang file with the given locale.
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="lines"></param>
        internal Lang(LocaleDefinition locale, LangEntry[] lines = null)
        {
            this.localeName = locale.locale;
            this.locale = locale;
            this.locale.file = this;
            this.lines = new List<LangEntry>();

            if(lines != null)
                this.lines.AddRange(lines);

            FindHeader();
        }

        /// <summary>
        /// Returns this Lang's LocaleDefinition, or creates a new
        /// one from the lang information if it doesn't exist.
        /// </summary>
        /// <returns></returns>
        internal LocaleDefinition GetOrCreateLocaleDefinition()
        {
            if(this.locale != null)
                return this.locale;

            this.locale = new LocaleDefinition(localeName, this);
            return this.locale;
        }

        /// <summary>
        /// Parse a .lang file under the given locale.
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static Lang Parse(LocaleDefinition locale, string file)
        {
            string[] lines = file.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<LangEntry> entries = new List<LangEntry>(lines.Length);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    entries.Add(LangEntry.Empty());
                    continue;
                }

                if (line.StartsWith("##"))
                {
                    if (line.Length == 2)
                    {
                        entries.Add(LangEntry.Comment(""));
                        continue;
                    }

                    string comment = line.Substring(2).Trim();
                    entries.Add(LangEntry.Comment(comment));
                    continue;
                }

                int equals = line.IndexOf('=');

                if (equals == -1)
                {
                    entries.Add(LangEntry.Empty());
                    continue;
                }

                string key = line.Substring(0, equals);
                string value = line.Substring(equals + 1);

                LangEntry entry = LangEntry.Create(key, value);
                entries.Add(entry);
                continue;
            }

            Lang lang = new Lang(locale, entries.ToArray());
            return lang;
        }
        /// <summary>
        /// Parse a .lang file under the given locale string.
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static Lang Parse(string locale, string file)
        {
            string[] lines = file.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<LangEntry> entries = new List<LangEntry>(lines.Length);

            foreach (string line in lines)
            {
                if(string.IsNullOrWhiteSpace(line))
                {
                    entries.Add(LangEntry.Empty());
                    continue;
                }

                if (line.StartsWith("##"))
                {
                    if(line.Length == 2)
                    {
                        entries.Add(LangEntry.Comment(""));
                        continue;
                    }

                    string comment = line.Substring(2).Trim();
                    entries.Add(LangEntry.Comment(comment));
                    continue;
                }

                int equals = line.IndexOf('=');

                if(equals == -1)
                {
                    entries.Add(LangEntry.Empty());
                    continue;
                }

                string key = line.Substring(0, equals);
                string value = line.Substring(equals + 1);

                LangEntry entry = LangEntry.Create(key, value);
                entries.Add(entry);
                continue;
            }

            Lang lang = new Lang(locale, entries.ToArray());
            return lang;
        }

        /// <summary>
        /// Attempts to locate the <see cref="HEADER"/> somewhere in the file and assigns <see cref="headerIndex"/>.
        /// If it doesn't exist, it will be created.
        /// </summary>
        private void FindHeader()
        {
            if (this.headerIndex != -1)
                return;

            this.headerIndex = lines.FindIndex(entry => entry.isComment && entry.value.Equals(HEADER));

            if (this.headerIndex != -1)
            {
                lines.Add(LangEntry.Comment(HEADER));
                this.headerIndex = lines.Count - 1; // start inserting from the bottom of the file, under the header.
            }
        }

        /// <summary>
        /// Adds a new LangEntry to this Lang file under the MCCompiled header.
        /// </summary>
        /// <param name="langEntry">The entry to add.</param>
        /// <param name="overwrite">If entries with conflicting keys should be overwritten.</param>
        internal void Add(LangEntry langEntry, bool overwrite = true)
        {
            // attempt to find an entry that matches the key.
            if(!langEntry.isEmpty && !langEntry.isComment)
            {
                string keyToFind = langEntry.key;
                int indexOfKey = lines.FindIndex(e => e.key.Equals(keyToFind, StringComparison.OrdinalIgnoreCase));

                if (indexOfKey != -1)
                {
                    // found duplicate, overwrite if the option is set.
                    if (!overwrite)
                        return;

                    lines[indexOfKey] = langEntry;
                    return;
                }
            }

            // insert it under the header.
            this.lines.Insert(this.headerIndex, langEntry);
        }
        /// <summary>
        /// Adds a collection of LangEntries to this Lang file under the MCCompiled header.
        /// </summary>
        /// <param name="langEntry">The entry to add.</param>
        /// <param name="overwrite">If entries with conflicting keys should be overwritten.</param>
        internal void AddRange(IEnumerable<LangEntry> langEntry, bool overwrite = true)
        {
            foreach(LangEntry entry in langEntry)
                Add(entry, overwrite);
        }

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;
        public byte[] GetOutputData()
        {
            throw new NotImplementedException();
        }
        public string GetOutputFile() => localeName + ".lang";
        public OutputLocation GetOutputLocation() => OutputLocation.r_TEXTS;
    }
}
