using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mc_compiled.MCC.Language.SyntaxExporter;

public class ExporterMonarch() : SyntaxExporter("mcc-monarch.js", "monarch", "Monarch/Monaco Language File")
{
    private readonly StringBuilder buffer = new();
    private int indentLevel;

    /// <summary>
    ///     Appends spaces to the internal buffer based on the current indentation level.
    /// </summary>
    private void AppendIndent() { this.buffer.Append(new string(' ', this.indentLevel * 4)); }

    /// <summary>
    ///     Opens a new block by appending an opening brace ("{") to the internal buffer,
    ///     increases the indentation level, and returns a <see cref="BlockContract" /> object
    ///     that ensures proper closure of the block.
    /// </summary>
    /// <param name="finishWithSemicolon">
    ///     A boolean value indicating whether the block should be closed with a semicolon (';')
    ///     when the block is terminated.
    /// </param>
    /// <param name="finishWithComma">
    ///     A boolean value indicating whether the block should be closed with a comma (',')
    ///     when the block is terminated.
    /// </param>
    /// <returns>
    ///     A <see cref="BlockContract" /> instance that manages the lifecycle of the block
    ///     and ensures proper closure using the specified <paramref name="finishWithSemicolon" /> behavior.
    /// </returns>
    private BlockContract OpenBlock(bool finishWithSemicolon, bool finishWithComma)
    {
        AppendIndent();
        this.buffer.AppendLine("{");
        this.indentLevel++;
        return new BlockContract(this, finishWithSemicolon, finishWithComma, false);
    }
    /// <summary>
    ///     Opens a new block for an array in the internal buffer, appending an opening bracket ('[') and increasing
    ///     the indentation level.
    /// </summary>
    /// <param name="finishWithSemicolon">
    ///     Indicates whether the block, when closed, should end with a semicolon.
    ///     If true, the block will be closed with '];', otherwise it will be closed with ']'.
    /// </param>
    /// <param name="finishWithComma">
    ///     Indicates whether the block, when closed, should end with a comma.
    ///     If true, the block will be closed with '],', otherwise it will be closed with ']'.
    /// </param>
    /// <returns>
    ///     Returns an instance of <see cref="BlockContract" /> which manages the scope of the array block.
    ///     Disposing this instance will automatically close the block.
    /// </returns>
    private BlockContract OpenArrayBlock(bool finishWithSemicolon, bool finishWithComma)
    {
        AppendIndent();
        this.buffer.AppendLine("[");
        this.indentLevel++;
        return new BlockContract(this, finishWithSemicolon, finishWithComma, true);
    }
    /// <summary>
    ///     Declares a constant object in the internal buffer with the specified <paramref name="name" />
    ///     and initializes a new block for its definition. The method appends the declaration
    ///     to the internal buffer, increases the indentation level, and returns a <see cref="BlockContract" />
    ///     to manage the block's lifecycle.
    /// </summary>
    /// <param name="name">
    ///     The name of the constant object to be declared. This value is appended to the buffer as part
    ///     of the constant declaration.
    /// </param>
    /// <param name="endWithSemicolon">End this object with a semicolon?</param>
    /// <param name="endWithComma">End this object with a comma?</param>
    /// <returns>
    ///     A <see cref="BlockContract" /> instance that ensures the proper closure of the constant
    ///     object's block, including appending the necessary closing brace and semicolon.
    /// </returns>
    private BlockContract OpenConstObject(string name, bool endWithSemicolon, bool endWithComma)
    {
        AppendIndent();
        this.buffer.Append("const ")
            .Append(name)
            .AppendLine(" = {");
        this.indentLevel++;
        return new BlockContract(this, endWithSemicolon, endWithComma, false);
    }
    /// <summary>
    ///     Opens a new constant array declaration in the internal buffer, appending the declaration syntax,
    ///     starting a new array block, and setting it to be automatically closed with a semicolon.
    /// </summary>
    /// <param name="name">
    ///     The name of the constant array to be declared. This value is included in the declaration
    ///     statement appended to the internal buffer.
    /// </param>
    /// <param name="endWithSemicolon">End this array with a semicolon?</param>
    /// <param name="endWithComma">End this array with a comma?</param>
    /// <returns>
    ///     Returns an instance of <see cref="BlockContract" /> that manages the scope of the array block.
    ///     Disposing of this instance will close the array block.
    /// </returns>
    private BlockContract OpenConstArray(string name, bool endWithSemicolon, bool endWithComma)
    {
        AppendIndent();
        this.buffer.Append("const ")
            .Append(name)
            .AppendLine(" = [");
        this.indentLevel++;
        return new BlockContract(this, endWithSemicolon, endWithComma, true);
    }
    /// <summary>
    ///     Opens a new property object block by appending the specified property name to the internal buffer
    ///     and increasing the indentation level. Returns a <see cref="BlockContract" /> for managing the block lifecycle.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property to be added as the key for the object block. This value is written to the internal buffer
    ///     followed by a colon and an opening brace, representing the start of the property object.
    /// </param>
    /// <param name="endWithSemicolon">End this object with a semicolon?</param>
    /// <param name="endWithComma">End this object with a comma?</param>
    /// <returns>
    ///     A <see cref="BlockContract" /> instance that can be used to ensure proper handling of
    ///     the property's object block, including closing it after usage.
    /// </returns>
    private BlockContract OpenPropertyObject(string propertyName, bool endWithSemicolon, bool endWithComma)
    {
        AppendIndent();
        this.buffer.Append(propertyName)
            .AppendLine(": {");
        this.indentLevel++;
        return new BlockContract(this, endWithSemicolon, endWithComma, false);
    }
    /// <summary>
    ///     Opens a new property array definition in the internal buffer for constructing object-like structures.
    ///     This method writes the property name and begins an array declaration, increasing the indentation level.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property to define in the object structure. This will be followed by the array declaration
    ///     (`[`) in the generated syntax.
    /// </param>
    /// <param name="endWithSemicolon">End this array with a semicolon?</param>
    /// <param name="endWithComma">End this array with a comma?</param>
    /// <returns>
    ///     A <see cref="BlockContract" /> instance that provides a way to ensure proper cleanup by closing the array block
    ///     upon disposal.
    /// </returns>
    private BlockContract OpenPropertyArray(string propertyName, bool endWithSemicolon, bool endWithComma)
    {
        AppendIndent();
        this.buffer.Append(propertyName)
            .AppendLine(": [");
        this.indentLevel++;
        return new BlockContract(this, endWithSemicolon, endWithComma, true);
    }
    /// <summary>
    ///     Closes a block, but you should probably use <see cref="BlockContract.Dispose" /> instead for easier consistency.
    /// </summary>
    /// <param name="addSemicolon">Add a semicolon at the end?</param>
    /// <param name="addComma">Add a comma at the end?</param>
    private void CloseBlock(bool addSemicolon, bool addComma)
    {
        this.indentLevel--;
        AppendIndent();
        this.buffer.AppendLine(addComma ? "}," : addSemicolon ? "};" : "}");
    }
    /// <summary>
    ///     Closes an array block, but you should probably use <see cref="BlockContract.Dispose" /> instead for easier
    ///     consistency.
    /// </summary>
    /// <param name="addSemicolon">Add a semicolon at the end?</param>
    /// <param name="addComma">Add a comma at the end?</param>
    private void CloseArrayBlock(bool addSemicolon, bool addComma)
    {
        this.indentLevel--;
        AppendIndent();
        this.buffer.AppendLine(addComma ? "]," : addSemicolon ? "];" : "]");
    }

    /// <summary>
    ///     Adds a JavaScript array definition to the internal buffer with the specified name and values.
    /// </summary>
    /// <param name="name">
    ///     The name of the array. If the name contains whitespace, it will be escaped and enclosed in double quotes.
    /// </param>
    /// <param name="values">
    ///     An IEnumerable of strings representing the values to include in the array. Each value will be wrapped in
    ///     backticks and any backticks within the values will be escaped.
    /// </param>
    /// <param name="endWithComma">
    ///     A boolean indicating whether a comma should be appended to the end of the array definition.
    /// </param>
    /// <remarks>
    ///     This method formats and appends a JavaScript object property definition to the internal buffer in the form of
    ///     <c>name: [value1, value2, ...]</c>. If <paramref name="endWithComma" /> is true, the output will end with a
    ///     comma. The method also ensures proper indentation is applied using the current indentation level.
    /// </remarks>
    private void AddSimpleArray(string name, IEnumerable<string> values, bool endWithComma = true)
    {
        if (name.Any(char.IsWhiteSpace))
            name = '"' + name.Replace("\"", "\\\"") + '"';

        AppendIndent();
        IEnumerable<string> stringedValues = values
            .Select(v => '`' + v.Replace("`", "\\`") + '`');
        string joinedValues = string.Join(", ", stringedValues);
        this.buffer.Append($"{name}: [{joinedValues}]");

        if (endWithComma)
            this.buffer.Append(',');

        this.buffer.AppendLine();
    }
    /// <summary>
    ///     Adds a simple array of string values to the internal buffer in a JavaScript array format.
    /// </summary>
    /// <param name="values">
    ///     An IEnumerable of <see cref="string" /> values to be formatted as a JavaScript array. Each value in the array
    ///     will be enclosed in backticks (`) with escape handling for existing backticks.
    /// </param>
    /// <param name="endWithComma">
    ///     A <see cref="bool" /> indicating whether a comma should be appended after the array in the buffer.
    ///     If <paramref name="endWithComma" /> is <c>true</c>, a comma will be appended after the array. Otherwise, it will
    ///     not.
    /// </param>
    private void AddSimpleArray(IEnumerable<string> values, bool endWithComma = true)
    {
        AppendIndent();
        IEnumerable<string> stringedValues = values
            .Select(v => '`' + v.Replace("`", "\\`") + '`');
        string joinedValues = string.Join(", ", stringedValues);
        this.buffer.Append($"[{joinedValues}]");

        if (endWithComma)
            this.buffer.Append(',');

        this.buffer.AppendLine();
    }
    /// <summary>
    ///     Writes a simple node value to the internal buffer, optionally adding a trailing comma.
    /// </summary>
    /// <param name="value">
    ///     The text content of the node to be appended to the internal buffer.
    /// </param>
    /// <param name="endWithComma">
    ///     A boolean indicating whether a trailing comma should be added after the node value.
    /// </param>
    /// <remarks>
    ///     This method appends the current indentation level to the internal buffer,
    ///     followed by the specified <paramref name="value" />. If <paramref name="endWithComma" />
    ///     is <see langword="true" />, a comma will be appended after the node value. A line break
    ///     is appended at the end.
    /// </remarks>
    private void AddSimpleNodeRaw(string value, bool endWithComma = true)
    {
        AppendIndent();
        this.buffer.Append(value);
        if (endWithComma)
            this.buffer.Append(',');
        this.buffer.AppendLine();
    }
    /// <summary>
    ///     Adds a simple property definition to the internal buffer in the format `name: value`.
    /// </summary>
    /// <param name="name">
    ///     The name of the property to be added. If the property name contains any symbol or whitespace
    ///     characters, it will be enclosed in single quotes.
    /// </param>
    /// <param name="value">
    ///     The value of the property to be added. It will be formatted as a string enclosed in single
    ///     quotes. Any single quotes in the value will be escaped.
    /// </param>
    /// <param name="endWithComma">
    ///     Indicates whether a trailing comma is appended after the property definition. If
    ///     <paramref name="endWithComma" /> is <see langword="true" />, a comma will be added after the value.
    ///     The default value is <see langword="true" />.
    /// </param>
    private void AddSimpleProperty(string name, string value, bool endWithComma = true)
    {
        AppendIndent();

        if (!char.IsLetter(name[0]) || name.Any(char.IsSymbol) || name.Any(char.IsWhiteSpace))
            name = '\'' + name + '\'';

        this.buffer.Append($"{name}: '{value.Replace("'", "\\'")}'");
        if (endWithComma)
            this.buffer.Append(',');
        this.buffer.AppendLine();
    }
    /// <summary>
    ///     Adds a named constant array of keywords to the internal buffer.
    /// </summary>
    /// <param name="listName">
    ///     The name of the constant array to define in the exported syntax file.
    /// </param>
    /// <param name="keywords">
    ///     A collection of <see cref="LanguageKeyword" /> items that represent the keywords to include in the array.
    ///     Each keyword contains an identifier and optional documentation.
    /// </param>
    /// <remarks>
    ///     This method serializes the provided keywords into a constant array definition in the internal buffer.
    ///     Each keyword is defined as a block with properties for the identifier and documentation.
    ///     The array is concluded with proper syntax formatting.
    /// </remarks>
    private void AddKeywordList(string listName, IEnumerable<LanguageKeyword> keywords)
    {
        using (OpenConstArray(listName, true, false))
        {
            LanguageKeyword[] keywordArray = keywords.ToArray();
            for (int i = 0; i < keywordArray.Length; i++)
            {
                LanguageKeyword keyword = keywordArray[i];
                bool last = i == keywordArray.Length - 1;
                using (OpenBlock(false, !last))
                {
                    AddSimpleProperty("word", keyword.identifier);
                    AddSimpleProperty("docs", keyword.docs);
                }
            }
        }
    }

    public override string Export()
    {
        this.buffer.Clear();
        this.indentLevel = 0;

        // tokenizer definition
        using (OpenConstObject("mccompiled", true, false))
        {
            AddSimpleArray("operators", Language.KEYWORDS_OPERATORS.Select(k => k.identifier));
            AddSimpleArray("selectors", Language.KEYWORDS_SELECTORS.Select(k => k.identifier));

            string[] preprocessorNames = Language.AllPreprocessorDirectives.Select(d => d.name).ToArray();
            string[] runtimeNames = Language.AllRuntimeDirectives.Select(d => d.name).ToArray();
            AddSimpleArray("preprocessor", preprocessorNames);
            AddSimpleArray("commands", runtimeNames);

            AddSimpleArray("literals", Language.KEYWORDS_LITERALS.Select(k => k.identifier));
            AddSimpleArray("types", Language.KEYWORDS_TYPES.Select(k => k.identifier));
            AddSimpleArray("comparisons", Language.KEYWORDS_COMPARISONS.Select(k => k.identifier));
            AddSimpleArray("options", Language.KEYWORDS_COMMAND_OPTIONS.Select(k => k.identifier));

            // tokenizer has a lot of static stuff that won't change, hold your horses.
            using (OpenPropertyObject("tokenizer", false, false))
            {
                using (OpenPropertyArray("root", false, true))
                {
                    // the root contains a bunch of arrays which link color themes to regex patterns.
                    // it also has some other logical features built in.
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("/@?[a-zA-Z$]\\w*/");

                        using (OpenBlock(false, false))
                        {
                            using (OpenPropertyObject("cases", false, false))
                            {
                                AddSimpleProperty("@selectors", "selectors");
                                AddSimpleProperty("@preprocessor", "preprocessor");
                                AddSimpleProperty("@commands", "commands");
                                AddSimpleProperty("@literals", "literals");
                                AddSimpleProperty("@types", "types");
                                AddSimpleProperty("@comparisons", "comparisons");
                                AddSimpleProperty("@options", "options");
                            }
                        }
                    }

                    using (OpenBlock(false, true))
                    {
                        AddSimpleProperty("include", "@handler");
                    }

                    // define how operators are identified
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/[<>{}=()+\-*/%!]+/""");
                        AddSimpleNodeRaw("operators", false);
                    }

                    // terminated strings
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/"(?:[^"\\]|\\.)*"/""");
                        AddSimpleNodeRaw("string", false);
                    }

                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/'(?:[^"\\]|\\.)*'/""");
                        AddSimpleNodeRaw("string", false);
                    }

                    // unterminated strings
                    // (not syntactically correct, but it's a nice-to-have)
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/"(?:[^"\\]|\\.)*$/""");
                        AddSimpleNodeRaw("string", false);
                    }

                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/'(?:[^'\\]|\\.)*$/""");
                        AddSimpleNodeRaw("string", false);
                    }

                    // no advanced parsing for selector properties
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/\[.+]/""");
                        AddSimpleNodeRaw("selectors.properties", false);
                    }

                    // parse numbers
                    using (OpenArrayBlock(false, false))
                    {
                        AddSimpleNodeRaw("""/!?(?:\.\.)?\d+(?:\.\.)?\.?\d*[hms]?/""");
                        AddSimpleNodeRaw("numbers", false);
                    }
                }

                // add entries for comment support
                using (OpenPropertyArray("comment", false, true))
                {
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/[^\/*]+/""");
                        AddSimpleNodeRaw("comment", false);
                    }

                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/\/\*/""");
                        AddSimpleNodeRaw("comment");
                        AddSimpleNodeRaw("@push", false);
                    }

                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""
                                         "\\*/"
                                         """);
                        AddSimpleNodeRaw("comment");
                        AddSimpleNodeRaw("@pop", false);
                    }

                    using (OpenArrayBlock(false, false))
                    {
                        AddSimpleNodeRaw("""/[\/*]/""");
                        AddSimpleNodeRaw("comment", false);
                    }
                }

                // comment handler
                using (OpenPropertyArray("handler", false, false))
                {
                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/[ \t\r\n]+/""");
                        AddSimpleNodeRaw("white", false);
                    }

                    using (OpenArrayBlock(false, true))
                    {
                        AddSimpleNodeRaw("""/\/\*/""");
                        AddSimpleNodeRaw("comment");
                        AddSimpleNodeRaw("@comment", false);
                    }

                    using (OpenArrayBlock(false, false))
                    {
                        AddSimpleNodeRaw("""/\/\/.*$/""");
                        AddSimpleNodeRaw("comment", false);
                    }
                }
            }
        }

        AddKeywordList("mcc_operators", Language.KEYWORDS_OPERATORS);
        AddKeywordList("mcc_selectors", Language.KEYWORDS_SELECTORS);
        AddKeywordList("mcc_literals", Language.KEYWORDS_LITERALS);
        AddKeywordList("mcc_types", Language.KEYWORDS_TYPES);
        AddKeywordList("mcc_options", Language.KEYWORDS_COMMAND_OPTIONS);
        AddKeywordList("mcc_preprocessor", Language.AllPreprocessorDirectives.Select(d => d.AsKeyword));
        AddKeywordList("mcc_commands", Language.AllRuntimeDirectives.Select(d => d.AsKeyword));
        return this.buffer.ToString();
    }

    /// <summary>
    ///     RAII object for closing a block/array.
    /// </summary>
    /// <param name="parent">The exporter to close this block in.</param>
    /// <param name="finishWithSemicolon">Should the block finish with a semicolon at the end?</param>
    /// <param name="isArray">Should the block finish with a comma at the end?</param>
    private class BlockContract(ExporterMonarch parent, bool finishWithSemicolon, bool finishWithComma, bool isArray)
        : IDisposable
    {
        private bool isClosed;

        public void Dispose()
        {
            if (this.isClosed)
                return;
            this.isClosed = true;
            if (isArray)
                parent.CloseArrayBlock(finishWithSemicolon, finishWithComma);
            else
                parent.CloseBlock(finishWithSemicolon, finishWithComma);
        }
    }
}