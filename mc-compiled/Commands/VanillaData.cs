using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using mc_compiled.MCC;
using mc_compiled.MCC.Language;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Commands;

/// <summary>
///     Static collection of properties for reading the bundled vanilla data with MCCompiled.
///     <br /><br />
///     Some methods here are expensive, so reserve their use for cases where MCCompiled will remain open and can reuse the
///     results of the first, expensive load.
/// </summary>
/// <remarks>
///     The files can be updated from https://github.com/Mojang/bedrock-samples/tree/main/metadata/vanilladata_modules,
///     OR you can run `vanilla-dependencies\update.ps1` to update it automatically.
/// </remarks>
public static class VanillaData
{
    private const bool KEEP_MINECRAFT_IDENTIFIER = false;

    private const string _FOLDER = "vanilla-dependencies";
    private const string BLOCKS_FILE = "mojang-blocks.json";
    private const string CAMERA_PRESETS_FILE = "mojang-camera-presets.json";
    private const string MOB_EFFECTS_FILE = "mojang-effects.json";
    private const string ENCHANTMENTS_FILE = "mojang-enchantments.json";
    private const string ENTITIES_FILE = "mojang-entities.json";
    private const string ITEMS_FILE = "mojang-items.json";

    private static readonly string FOLDER = Path.Combine(AppContext.BaseDirectory, _FOLDER);
    public static readonly string BLOCKS_PATH = Path.Combine(FOLDER, BLOCKS_FILE);
    public static readonly string CAMERA_PRESETS_PATH = Path.Combine(FOLDER, CAMERA_PRESETS_FILE);
    public static readonly string MOB_EFFECTS_PATH = Path.Combine(FOLDER, MOB_EFFECTS_FILE);
    public static readonly string ENCHANTMENTS_PATH = Path.Combine(FOLDER, ENCHANTMENTS_FILE);
    public static readonly string ENTITIES_PATH = Path.Combine(FOLDER, ENTITIES_FILE);
    public static readonly string ITEMS_PATH = Path.Combine(FOLDER, ITEMS_FILE);

    private static bool BLOCKS_LOADED;
    private static bool CAMERA_PRESETS_LOADED;
    private static bool MOB_EFFECTS_LOADED;
    private static bool ENCHANTMENTS_LOADED;
    private static bool ENTITIES_LOADED;
    private static bool ITEMS_LOADED;

    private static readonly List<EnumerationKeyword> _blocks = [];
    private static readonly List<EnumerationKeyword> _cameraPresets = [];
    private static readonly List<EnumerationKeyword> _mobEffects = [];
    private static readonly List<EnumerationKeyword> _enchantments = [];
    private static readonly List<EnumerationKeyword> _entities = [];
    private static readonly List<EnumerationKeyword> _items = [];

    /// <summary>
    ///     Returns the input string, but with the <c>minecraft:</c> identifier stripped off.
    /// </summary>
    /// <param name="identifier">The input identifier.</param>
    private static string StripMinecraftIdentifier(string identifier)
    {
        return identifier.StartsWith("minecraft:") ? identifier[10..] : identifier;
    }

    /// <summary>
    ///     Warning: This is a VERY expensive method on the first call. Use sparingly.<br />
    ///     <br />
    ///     Returns all block identifiers in Minecraft Vanilla.
    /// </summary>
    [PublicAPI]
    public static IReadOnlyList<EnumerationKeyword> Blocks()
    {
        EnsureBlocksLoaded();
        return _blocks;
    }
    /// <summary>
    ///     Returns all camera presets in Minecraft Vanilla.
    /// </summary>
    [PublicAPI]
    public static IReadOnlyList<EnumerationKeyword> CameraPresets()
    {
        EnsureCameraPresetsLoaded();
        return _cameraPresets;
    }
    /// <summary>
    ///     Returns all effects (like what's available with <c>/effect</c>) in Minecraft Vanilla.
    /// </summary>
    [PublicAPI]
    public static IReadOnlyList<EnumerationKeyword> MobEffects()
    {
        EnsureMobEffectsLoaded();
        return _mobEffects;
    }
    /// <summary>
    ///     Returns all enchantment identifiers in Minecraft Vanilla.
    /// </summary>
    [PublicAPI]
    public static IReadOnlyList<EnumerationKeyword> Enchantments()
    {
        EnsureEnchantmentsLoaded();
        return _enchantments;
    }
    /// <summary>
    ///     Returns all entity identifiers in Minecraft Vanilla.
    /// </summary>
    [PublicAPI]
    public static IReadOnlyList<EnumerationKeyword> Entities()
    {
        EnsureEntitiesLoaded();
        return _entities;
    }
    /// <summary>
    ///     Warning: This is a VERY expensive method on the first call. Use sparingly.<br />
    ///     <br />
    ///     Returns all item identifiers in Minecraft Vanilla.
    /// </summary>
    [PublicAPI]
    public static IReadOnlyList<EnumerationKeyword> Items()
    {
        EnsureItemsLoaded();
        return _items;
    }

    private static void EnsureBlocksLoaded()
    {
        if (BLOCKS_LOADED)
            return;
        if (!File.Exists(BLOCKS_PATH))
        {
            BLOCKS_LOADED = true;
            Console.Error.WriteLine($"Missing file './{FOLDER}/{BLOCKS_FILE}'");
            return;
        }

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla block identifiers...");

        JObject root = JObject.Parse(File.ReadAllText(BLOCKS_PATH));
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;
            string name = dataItem["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
                continue;
            if (!KEEP_MINECRAFT_IDENTIFIER)
                name = StripMinecraftIdentifier(name);
            _blocks.Add(new EnumerationKeyword(name));
        }

        BLOCKS_LOADED = true;
    }
    private static void EnsureCameraPresetsLoaded()
    {
        if (CAMERA_PRESETS_LOADED)
            return;
        if (!File.Exists(CAMERA_PRESETS_PATH))
        {
            CAMERA_PRESETS_LOADED = true;
            Console.Error.WriteLine($"Missing file './{FOLDER}/{CAMERA_PRESETS_FILE}'");
            return;
        }

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla camera preset identifiers...");

        JObject root = JObject.Parse(File.ReadAllText(CAMERA_PRESETS_PATH));
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;
            string name = dataItem["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
                continue;
            if (!KEEP_MINECRAFT_IDENTIFIER)
                name = StripMinecraftIdentifier(name);
            _cameraPresets.Add(new EnumerationKeyword(name));
        }

        CAMERA_PRESETS_LOADED = true;
    }
    private static void EnsureMobEffectsLoaded()
    {
        if (MOB_EFFECTS_LOADED)
            return;
        if (!File.Exists(MOB_EFFECTS_PATH))
        {
            MOB_EFFECTS_LOADED = true;
            Console.Error.WriteLine($"Missing file './{FOLDER}/{MOB_EFFECTS_FILE}'");
            return;
        }

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla mob effect identifiers...");

        JObject root = JObject.Parse(File.ReadAllText(MOB_EFFECTS_PATH));
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;
            string name = dataItem["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
                continue;
            if (!KEEP_MINECRAFT_IDENTIFIER)
                name = StripMinecraftIdentifier(name);
            _mobEffects.Add(new EnumerationKeyword(name));
        }

        MOB_EFFECTS_LOADED = true;
    }
    private static void EnsureEnchantmentsLoaded()
    {
        if (ENCHANTMENTS_LOADED)
            return;
        if (!File.Exists(ENCHANTMENTS_PATH))
        {
            ENCHANTMENTS_LOADED = true;
            Console.Error.WriteLine($"Missing file './{FOLDER}/{ENCHANTMENTS_FILE}'");
            return;
        }

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla enchantment identifiers...");

        JObject root = JObject.Parse(File.ReadAllText(ENCHANTMENTS_PATH));
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;

            string name = dataItem["value"]?.Value<string>() ??
                          dataItem["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
                continue;
            if (!KEEP_MINECRAFT_IDENTIFIER)
                name = StripMinecraftIdentifier(name);
            _enchantments.Add(new EnumerationKeyword(name));
        }

        ENCHANTMENTS_LOADED = true;
    }
    private static void EnsureEntitiesLoaded()
    {
        if (ENTITIES_LOADED)
            return;
        if (!File.Exists(ENTITIES_PATH))
        {
            ENTITIES_LOADED = true;
            Console.Error.WriteLine($"Missing file './{FOLDER}/{ENTITIES_FILE}'");
            return;
        }

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla entity identifiers...");

        JObject root = JObject.Parse(File.ReadAllText(ENTITIES_PATH));
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;
            string name = dataItem["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
                continue;
            if (!KEEP_MINECRAFT_IDENTIFIER)
                name = StripMinecraftIdentifier(name);
            _entities.Add(new EnumerationKeyword(name));
        }

        ENTITIES_LOADED = true;
    }
    private static void EnsureItemsLoaded()
    {
        if (ITEMS_LOADED)
            return;
        if (!File.Exists(ITEMS_PATH))
        {
            ITEMS_LOADED = true;
            Console.Error.WriteLine($"Missing file './{FOLDER}/{ITEMS_FILE}'");
            return;
        }

        if (GlobalContext.Debug)
            Console.WriteLine("[VANILLA] Loading vanilla item identifiers...");

        JObject root = JObject.Parse(File.ReadAllText(ITEMS_PATH));
        JArray dataItems = root["data_items"]?.Value<JArray>() ?? [];

        foreach (JToken _dataItem in dataItems)
        {
            if (_dataItem is not JObject dataItem)
                continue;
            string name = dataItem["command_name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
                continue;
            if (!KEEP_MINECRAFT_IDENTIFIER)
                name = StripMinecraftIdentifier(name);
            _items.Add(new EnumerationKeyword(name));
        }

        ITEMS_LOADED = true;
    }
}