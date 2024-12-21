using System;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding.Manifest.Modules;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Manifest;

/// <summary>
///     Represents a module inside a manifest.
/// </summary>
public abstract class Module
{
    /// <summary>
    ///     The version of this module.
    /// </summary>
    protected readonly ManifestVersion moduleVersion;
    /// <summary>
    ///     The type of this module.
    /// </summary>
    protected readonly ModuleType type;
    /// <summary>
    ///     The unique identifier for this module.
    /// </summary>
    protected readonly Guid uuid;

    protected Module(ModuleType type, Guid? uuid, ManifestVersion moduleVersion)
    {
        this.type = type;
        this.uuid = uuid ?? Guid.NewGuid();
        this.moduleVersion = moduleVersion ?? ManifestVersion.DEFAULT;
    }

    /// <summary>
    ///     Tries to parse a JSON object and create a Module from it.
    /// </summary>
    /// <param name="json">The JSON object to parse.</param>
    /// <param name="module">
    ///     When this method returns, contains the parsed Module object if the parsing was successful, or null
    ///     if the parsing failed.
    /// </param>
    /// <returns>
    ///     true if the parsing was successful and a Module was created; false otherwise.
    /// </returns>
    public static bool TryParse(JObject json, out Module module)
    {
        if (!json.TryGetValue("type", out JToken _type))
        {
            module = null;
            return false;
        }

        if (!Enum.TryParse(_type.ToString(), true, out ModuleType type))
        {
            module = null;
            return false;
        }

        Guid? uuid = null;
        if (json.TryGetValue("uuid", out JToken uuidToken))
            if (Guid.TryParse(uuidToken.ToString(), out Guid _uuid))
                uuid = _uuid;

        ManifestVersion version = null;
        if (json.TryGetValue("version", out JToken versionToken))
            ManifestVersion.TryParseToken(versionToken, out version);

        switch (type)
        {
            case ModuleType.data:
            case ModuleType.resources:
                module = new BasicModule(type, uuid, version);
                return true;
            case ModuleType.script:
                // this is barely readable just trust me it gets the 'language' and 'entry' fields and parses them
                var language = ScriptLanguage.javascript;
                if (json.TryGetValue("language", out JToken languageToken))
                    Enum.TryParse(languageToken.ToString(), out language);
                string entry;
                if (json.TryGetValue("entry", out JToken entryToken))
                {
                    entry = entryToken.ToString();
                }
                else
                {
                    const string DEFAULT_ENTRY_POINT = "scripts/main.js";
                    Executor.Warn(
                        $"No entry point specified in 'script' module. Defaulting to '{DEFAULT_ENTRY_POINT}'.");
                    entry = DEFAULT_ENTRY_POINT;
                }

                module = new ScriptModule(uuid, language, entry);
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Converts this Module to its appropriate JSON format for output.
    /// </summary>
    /// <returns></returns>
    public abstract JObject ToJSON();
}

public enum ModuleType
{
    /// <summary>
    ///     Behavior pack data.
    /// </summary>
    data,
    /// <summary>
    ///     Resource pack data.
    /// </summary>
    resources,
    /// <summary>
    ///     Scripting API data.
    /// </summary>
    script
}