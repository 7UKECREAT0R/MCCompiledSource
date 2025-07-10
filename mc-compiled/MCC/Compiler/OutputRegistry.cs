using System.Collections.Generic;
using System.IO;
using mc_compiled.Modding;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Generates and holds a "registry" for directing file outputs.
/// </summary>
internal readonly struct OutputRegistry
{
    internal readonly string bpBase; // e.g: development_behavior_packs/project_name/
    internal readonly string rpBase; // e.g: development_resource_packs/project_name/
    private readonly Dictionary<OutputLocation, string> registry;

    internal OutputRegistry(string bpBase, string rpBase)
    {
        this.bpBase = bpBase;
        this.rpBase = rpBase;
        this.registry = new Dictionary<OutputLocation, string>();

        // None
        this.registry[OutputLocation.NONE] = "";

        // BP Folders
        this.registry[OutputLocation.b_ROOT] = bpBase;
        this.registry[OutputLocation.b_ANIMATIONS] = Path.Combine(bpBase, "animations");
        this.registry[OutputLocation.b_ANIMATION_CONTROLLERS] = Path.Combine(bpBase, "animation_controllers");
        this.registry[OutputLocation.b_BLOCKS] = Path.Combine(bpBase, "blocks");
        this.registry[OutputLocation.b_BIOMES] = Path.Combine(bpBase, "biomes");
        this.registry[OutputLocation.b_DIALOGUE] = Path.Combine(bpBase, "dialogue");
        this.registry[OutputLocation.b_ENTITIES] = Path.Combine(bpBase, "entities");
        this.registry[OutputLocation.b_FEATURES] = Path.Combine(bpBase, "features");
        this.registry[OutputLocation.b_FEATURE_RULES] = Path.Combine(bpBase, "feature_rules");
        this.registry[OutputLocation.b_FUNCTIONS] = Path.Combine(bpBase, "functions");
        this.registry[OutputLocation.b_ITEMS] = Path.Combine(bpBase, "items");
        this.registry[OutputLocation.b_LOOT_TABLES] = Path.Combine(bpBase, "loot_tables");
        this.registry[OutputLocation.b_RECIPES] = Path.Combine(bpBase, "recipes");
        this.registry[OutputLocation.b_SCRIPTS__CLIENT] = Path.Combine(bpBase, "scripts", "client");
        this.registry[OutputLocation.b_SCRIPTS__SERVER] = Path.Combine(bpBase, "scripts", "server");
        this.registry[OutputLocation.b_SCRIPTS__GAMETESTS] = Path.Combine(bpBase, "scripts", "gametests");
        this.registry[OutputLocation.b_SPAWN_RULES] = Path.Combine(bpBase, "spawn_rules");
        this.registry[OutputLocation.b_TEXTS] = Path.Combine(bpBase, "texts");
        this.registry[OutputLocation.b_TRADING] = Path.Combine(bpBase, "trading");
        this.registry[OutputLocation.b_STRUCTURES] = Path.Combine(bpBase, "structures");

        // RP Folders
        this.registry[OutputLocation.r_ROOT] = rpBase;
        this.registry[OutputLocation.r_ANIMATION_CONTROLLERS] = Path.Combine(rpBase, "animation_controllers");
        this.registry[OutputLocation.r_ANIMATIONS] = Path.Combine(rpBase, "animations");
        this.registry[OutputLocation.r_ATTACHABLES] = Path.Combine(rpBase, "attachables");
        this.registry[OutputLocation.r_ENTITY] = Path.Combine(rpBase, "entity");
        this.registry[OutputLocation.r_FOGS] = Path.Combine(rpBase, "fogs");
        this.registry[OutputLocation.r_MODELS__ENTITY] = Path.Combine(rpBase, "models", "entity");
        this.registry[OutputLocation.r_MODELS__BLOCKS] = Path.Combine(rpBase, "models", "blocks");
        this.registry[OutputLocation.r_PARTICLES] = Path.Combine(rpBase, "particles");
        this.registry[OutputLocation.r_ITEMS] = Path.Combine(rpBase, "items");
        this.registry[OutputLocation.r_RENDER_CONTROLLERS] = Path.Combine(rpBase, "render_controllers");
        this.registry[OutputLocation.r_SOUNDS] = Path.Combine(rpBase, "sounds");
        this.registry[OutputLocation.r_TEXTS] = Path.Combine(rpBase, "texts");
        this.registry[OutputLocation.r_TEXTURES__ENVIRONMENT] = Path.Combine(rpBase, "textures", "environment");
        this.registry[OutputLocation.r_TEXTURES__BLOCKS] = Path.Combine(rpBase, "textures", "blocks");
        this.registry[OutputLocation.r_TEXTURES__ENTITY] = Path.Combine(rpBase, "textures", "entity");
        this.registry[OutputLocation.r_TEXTURES__ITEMS] = Path.Combine(rpBase, "textures", "items");
        this.registry[OutputLocation.r_TEXTURES__PARTICLE] = Path.Combine(rpBase, "textures", "particle");
        this.registry[OutputLocation.r_UI] = Path.Combine(rpBase, "ui");
    }
    internal string this[OutputLocation location] => this.registry[location];
}