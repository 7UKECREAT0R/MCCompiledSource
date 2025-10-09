using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextCopy;

namespace mc_compiled.Json;

/// <summary>
///     A JSON rawtext sequence. Use the following types as entries to this object:
///     <ul>
///         <li>
///             <see cref="Text" />
///         </li>
///         <li>
///             <see cref="Score" />
///         </li>
///         <li>
///             <see cref="Selector" />
///         </li>
///         <li>
///             <see cref="Translate" />
///         </li>
///         <li>
///             <see cref="Variant" />
///         </li>
///     </ul>
/// </summary>
public class RawText
{
    private readonly List<RawTextEntry> terms;
    private string copiedString;

    public RawText() { this.terms = []; }
    public RawText(RawText copy)
    {
        this.terms = [];

        if (copy != null)
            this.terms.AddRange(copy.terms);
    }
    public void ClearTerms() { this.terms.Clear(); }
    public RawText AddTerm(RawTextEntry textEntry)
    {
        this.terms.Add(textEntry);
        return this;
    }
    public RawText AddTerms(IEnumerable<RawTextEntry> newTerms)
    {
        this.terms.AddRange(newTerms);
        return this;
    }
    public RawText AddTerms(params RawTextEntry[] newTerms)
    {
        this.terms.AddRange(newTerms);
        return this;
    }
    private string BuildPreviewString()
    {
        return string.Join(" ", from term in this.terms select term.PreviewString());
    }

    /// <summary>
    ///     Builds the terms inside this builder into the completed JSON.
    /// </summary>
    /// <returns></returns>
    public JObject Build()
    {
        JObject[] objects = (from term in this.terms select term.Build()).ToArray();

        return new JObject
        {
            ["rawtext"] = new JArray(objects.Cast<object>())
        };
    }
    /// <summary>
    ///     Returns <see cref="Build" />, but as a minimized JSON string.
    /// </summary>
    /// <returns></returns>
    public string BuildString() { return Build().ToString(Formatting.None); }

    /// <summary>
    ///     A pretty old module which lets users build this <see cref="RawText" /> interactively through the
    ///     console.
    /// </summary>
    public void ConsoleInterface()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(
                """
                ┌─ JSON RawText Builder ──────────┐
                │ {0}
                ├─────────────────────────────────┐
                │ [A] Add Item                    │
                │ [R] Remove Item                 │
                │                                 │
                │ [Q] Quit                        │
                │ [C] Copy Text                   │
                └─────────────────────────────────┘
                """, BuildPreviewString());

            if (this.copiedString != null)
            {
                Console.WriteLine("Copied: " + this.copiedString);
                this.copiedString = null;
            }

            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Q)
                return;
            if (key.Key == ConsoleKey.C)
            {
                this.copiedString = Build().ToString(Formatting.None);
                ClipboardService.SetText(this.copiedString);
                continue;
            }

            if (key.Key == ConsoleKey.A)
            {
                Console.WriteLine(
                    """
                    ┌─────────────────────────────────┬───────────────────────┐
                    │ Enter the item you want to add. │      Description      │
                    │                                 │                       │
                    │ TEXT <text>                     │ Regular text.         │
                    │ SCORE <objective> <selector>    │ A scoreboard value.   │
                    │ SELECTOR <selector>             │ The name of a target. │
                    │ TRANSLATE <key>                 │ A translation key.    │
                    └─────────────────────────────────┴───────────────────────┘
                    """);
                Console.Write("> ");
                string text = Console.ReadLine();
                string comp = text.ToUpper();
                if (comp.StartsWith("TEXT"))
                {
                    string str = text[5..];
                    str = Definitions.GLOBAL_DEFS.ReplaceDefinitions(str);
                    this.terms.Add(new Text(str));
                    continue;
                }

                if (comp.StartsWith("SCORE"))
                {
                    if (text.Length < 7)
                        continue;
                    string epic = text[6..];
                    int index = epic.IndexOf(' ');
                    string objective = epic[..index];
                    string selector = epic[(index + 1)..];
                    this.terms.Add(new Score(selector, objective));
                    continue;
                }

                if (comp.StartsWith("SELECTOR"))
                {
                    this.terms.Add(new Selector(text[9..]));
                    continue;
                }

                if (comp.StartsWith("TRANSLATE"))
                {
                    this.terms.Add(new Translate(text[10..]));
                    continue;
                }
            }

            if (key.Key == ConsoleKey.R)
            {
                int count = this.terms.Count;
                if (count < 1)
                    continue;
                this.terms.RemoveAt(count - 1);
            }
        }
    }
}