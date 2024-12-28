using Newtonsoft.Json;

namespace mc_compiled.MCC.Language;

public struct LanguageSyntax
{
    [JsonProperty("extension")]
    public readonly string extension;

    [JsonProperty("ignore_case")]
    public readonly bool ignoreCase;

    [JsonProperty("comment_folding")]
    public readonly bool commentFolding;
    [JsonProperty("compact_folding")]
    public readonly bool compactFolding;

    [JsonProperty("special_characters")]
    public readonly LanguageSpecialCharacters specialCharacters;
}

public struct LanguageSpecialCharacters
{
    [JsonProperty("range_delimiter")]
    public readonly string rangeDelimiter;

    [JsonProperty("invert_delimiter")]
    public readonly string invertDelimiter;

    [JsonProperty("bracket_open")]
    public readonly string bracketOpen;

    [JsonProperty("bracket_close")]
    public readonly string bracketClose;

    [JsonProperty("block_open")]
    public readonly string blockOpen;

    [JsonProperty("block_close")]
    public readonly string blockClose;

    [JsonProperty("string_delimiters")]
    public readonly string[] stringDelimiters;

    [JsonProperty("escape")]
    public readonly string escape;

    [JsonProperty("line_comment")]
    public readonly string lineComment;

    [JsonProperty("multiline_comment_open")]
    public readonly string multilineCommentOpen;

    [JsonProperty("multiline_comment_close")]
    public readonly string multilineCommentClose;

    [JsonProperty("number_prefixes")]
    public readonly string[] numberPrefixes;

    [JsonProperty("number_suffixes")]
    public readonly string[] numberSuffixes;

    [JsonProperty("operators")]
    public readonly string[] operators;
}