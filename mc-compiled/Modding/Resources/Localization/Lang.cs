﻿using mc_compiled.MCC.Compiler;
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
        private const string HEADER = "MCCompiled Language Entries";
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
            string[] lines = file.Split(new char[] { '\r', '\n' });
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

                    string comment = new string(line.Substring(2).SkipWhile(c => char.IsWhiteSpace(c)).ToArray());
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
            file = file.Replace(Environment.NewLine, "\n");
            string[] lines = file.Split('\n');
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

                    string comment = new string(line.Substring(2).SkipWhile(c => char.IsWhiteSpace(c)).ToArray());
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

                if (key.StartsWith(Executor.MCC_TRANSLATE_PREFIX))
                    continue; // don't load generated keys, they need to be thrown out for this build.

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

            if (this.headerIndex == -1)
            {
                lines.Add(LangEntry.Empty());
                lines.Add(LangEntry.Comment(HEADER));
                this.headerIndex = lines.Count; // start inserting from the bottom of the file, under the header.
                lines.Add(LangEntry.Empty()); // to prevent List::Insert() from throwing, since it can't insert at the end.
            } else
            {
                headerIndex++; // after the header, not before

                // if EOL is right there, add an empty line.
                if(lines.Count <= headerIndex)
                    lines.Add(LangEntry.Empty());
            }
        }

        /// <summary>
        /// Adds a new LangEntry to this Lang file under the MCCompiled header.
        /// </summary>
        /// <param name="langEntry">The entry to add.</param>
        /// <param name="overwrite">If entries with conflicting keys should be overwritten.</param>
        /// <param name="merge">Whether to merge with language keys that share identical contents. Changes return value sometimes.</param>
        /// <returns>The passed in LangEntry unless the key was merged with an existing one.</returns>
        internal LangEntry Add(LangEntry langEntry, bool overwrite, bool merge)
        {
            // attempt to find an entry that matches the key.
            if(!langEntry.isEmpty && !langEntry.isComment)
            {
                string keyToFind = langEntry.key;
                int indexOfKey = lines
                    .FindIndex(e =>
                        !e.isComment &&
                        !e.isEmpty &&
                        e.key.Equals(keyToFind, StringComparison.OrdinalIgnoreCase));

                if(merge)
                {
                    string valueToFind = langEntry.value;
                    int indexOfValue = lines
                        .FindIndex(e =>
                            !e.isComment &&
                            !e.isEmpty &&
                            e.value.Equals(valueToFind));

                    if(indexOfValue != -1)
                    {
                        // key with that value already exists, so use it instead.
                        return lines[indexOfValue];
                    }
                }

                if (indexOfKey != -1)
                {
                    // found duplicate, overwrite if the option is set.
                    if (!overwrite)
                        return lines[indexOfKey];

                    lines[indexOfKey] = langEntry;
                    return langEntry;
                }
            }

            // insert it under the header.
            this.lines.Insert(this.headerIndex, langEntry);
            return langEntry;
        }
        /// <summary>
        /// Adds a collection of LangEntries to this Lang file under the MCCompiled header.
        /// </summary>
        /// <param name="langEntry">The entry to add.</param>
        /// <param name="overwrite">If entries with conflicting keys should be overwritten.</param>
        internal LangEntry[] AddRange(IEnumerable<LangEntry> langEntry, bool overwrite, bool merge)
        {
            LangEntry[] entries = new LangEntry[langEntry.Count()];
            int i = 0;

            foreach (LangEntry entry in langEntry)
                entries[i++] = Add(entry, overwrite, merge);

            return entries;
        }

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;
        public byte[] GetOutputData()
        {
            string[] lines = (from line in this.lines select line.ToString()).ToArray();
            string fullString = string.Join(Environment.NewLine, lines);
            return Encoding.UTF8.GetBytes(fullString);
        }
        public string GetOutputFile() => localeName + ".lang";
        public OutputLocation GetOutputLocation() => OutputLocation.r_TEXTS;
    }
}