using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Represents a directive in MCCompiled, as defined in <c>language.json</c>.
///     Commonly referred to as a command in the user-facing implementation.
/// </summary>
public class Directive
{
    private static readonly Type DIRECTIVE_IMPLEMENTATIONS = typeof(DirectiveImplementations);

    /// <summary>
    ///     The aliases for this directive; i.e., alternate names the user can use to specify it.
    /// </summary>
    public readonly string[] aliases;
    /// <summary>
    ///     The attributes applied to this directive that modify how it works.
    /// </summary>
    public readonly DirectiveAttribute attributes;

    /// <summary>
    ///     If present, the name of the category this directive lies under.
    /// </summary>
    [CanBeNull]
    public readonly string category;
    /// <summary>
    ///     A short description of the directive. More descriptive than the name, less descriptive than the
    ///     <see cref="details" />.
    /// </summary>
    public readonly string description;
    /// <summary>
    ///     A detailed description of what the directive does.
    /// </summary>
    public readonly string details;

    /// <summary>
    ///     The C# implementation of this directive.
    /// </summary>
    internal readonly DirectiveImpl implementation;

    /// <summary>
    ///     The name/identifier of this directive.
    /// </summary>
    public readonly string name;

    // Directive might overlap an enum value.
    // In the case this happens, it can use this field to help convert itself.
    /// <summary>
    ///     This directive might overlap a <see cref="ParsedEnumValue" />. In the case
    ///     this happens, it can use this field to help convert itself over.
    /// </summary>
    public readonly ParsedEnumValue? overlappingEnumValue;

    /// <summary>
    ///     If present, the link to the wiki page which details this directive and how the user should use it.
    ///     For example: <c>Debugging.md#assertions</c>
    /// </summary>
    [CanBeNull]
    public readonly string wikiLink;

    /// <summary>
    ///     The syntax of this directive in MCCompiled. <see cref="Syntax" /> is the public API.
    /// </summary>
    internal SyntaxGroup _syntax;

    private Directive(string[] aliases,
        DirectiveAttribute attributes,
        [CanBeNull] string category,
        string description,
        string details,
        DirectiveImpl implementation,
        string name,
        [CanBeNull] string wikiLink,
        [NotNull] SyntaxGroup syntax)
    {
        this.aliases = aliases;
        this.attributes = attributes;
        this.category = category;
        this.description = description;
        this.details = details;
        this.implementation = implementation;
        this.name = name;
        this.wikiLink = wikiLink;
        this._syntax = syntax;

        if (CommandEnumParser.TryParse(name, out ParsedEnumValue result))
            this.overlappingEnumValue = result;
    }

    /// <summary>
    ///     Returns if this directive is a preprocessor directive based on whether it begins with a dollar sign ($).
    /// </summary>
    public bool IsPreprocessor => !string.IsNullOrEmpty(this.name) && this.name[0].Equals('$');
    /// <summary>
    ///     Retrieves a reference to the syntax of this directive in MCCompiled.
    /// </summary>
    public SyntaxGroup Syntax => this._syntax;

    /// <summary>
    ///     Parses a directive from a JSON property.
    /// </summary>
    /// <param name="property">The JSON property representing the directive, containing its name and contents.</param>
    /// <returns>The newly parsed directive.</returns>
    /// <exception cref="ArgumentException">Thrown if the property contents are not a valid JSON object.</exception>
    [PublicAPI]
    public static Directive Parse(JProperty property)
    {
        if (property.Value is not JObject json)
            throw new ArgumentException("Directive property contents must be a JSON object.");
        return Parse(property.Name, json);
    }
    /// <summary>
    ///     Parses a directive from a JSON property's name and its contained object.
    /// </summary>
    /// <param name="name">The name of the directive; the property name generally.</param>
    /// <param name="json">The JSON object containing the directive's contents.</param>
    /// <returns>The newly parsed directive.</returns>
    /// <exception cref="ArgumentException">If the directive is missing a required field.</exception>
    [NotNull]
    [PublicAPI]
    public static Directive Parse(string name, JObject json)
    {
        string[] aliases = json["aliases"]?.ToObject<string[]>() ?? [];
        DirectiveAttribute[] _attributes = json["attributes"]?.ToObject<DirectiveAttribute[]>() ?? [];
        DirectiveAttribute attributes = _attributes is {Length: > 0} ? _attributes.Aggregate((a, b) => a | b) : 0;
        string category = json.Value<string>("category");
        string description = json.Value<string>("description") ??
                             throw new ArgumentException($"Directive {name} must include `description`.");
        string details = json.Value<string>("details") ??
                         throw new ArgumentException($"Directive {name} must include `details`.");
        string functionNameOverride = json.Value<string>("function");
        string wikiLink = json.Value<string>("wiki_link");

        DirectiveImpl implementation = GetImplementation(functionNameOverride ?? name);
        if (implementation == null)
            throw new ArgumentException($"Couldn't find implementation for directive {name}.");

        SyntaxGroup syntax;
        JToken syntaxToken = json["syntax"];
        JToken syntaxRefToken = json["syntax_ref"];
        if (syntaxToken != null && syntaxRefToken != null)
            throw new ArgumentException(
                $"Directive {name} has both `syntax` and `syntax_ref` specified. Only one is allowed.");
        if (syntaxToken != null)
        {
            syntax = SyntaxGroup.Parse(syntaxToken);
        }
        else if (syntaxRefToken != null)
        {
            string query = syntaxRefToken.Value<string>() ??
                           throw new ArgumentException($"Syntax reference for {name} must be a string.");
            syntax = Language.QuerySyntaxGroup(query);
            if (syntax == null)
                throw new ArgumentException($"Couldn't resolve syntax reference '{query}'.");
        }
        else
        {
            syntax = SyntaxGroup.EMPTY;
        }

        return new Directive(aliases, attributes, category, description,
            details, implementation, name, wikiLink, syntax);
    }

    /// <summary>
    ///     Attempts to get a directive implementation from the <see cref="DIRECTIVE_IMPLEMENTATIONS" /> type. The
    ///     function must match the delegate: <see cref="DirectiveImpl" />.
    /// </summary>
    /// <param name="functionName">The name of the function.</param>
    /// <returns><c>null</c> if no function could be found, or the incoming function name was bad.</returns>
    [CanBeNull]
    private static DirectiveImpl GetImplementation(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
            return null;

        MethodInfo info = DIRECTIVE_IMPLEMENTATIONS.GetMethod(functionName, [
            typeof(Executor),
            typeof(Statement)
        ]);

        if (info == null)
            return null;

        return (DirectiveImpl) Delegate
            .CreateDelegate(typeof(DirectiveImpl), info);
    }

    /// <summary>
    ///     An implementation of a directive call.
    /// </summary>
    /// <param name="executor">The executor (context) running this directive.</param>
    /// <param name="tokens">The statement that this directive was initiated by.</param>
    internal delegate void DirectiveImpl(Executor executor, Statement tokens);
}