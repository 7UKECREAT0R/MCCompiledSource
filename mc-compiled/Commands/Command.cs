using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Native;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding.Behaviors;
using mc_compiled.Modding.Behaviors.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Commands;

/// <summary>
///     Utility for constructing minecraft commands.
/// </summary>
public static class Command
{
    public static readonly Util UTIL = new();
    /// <summary>
    ///     Returns a command as its literal, chat notation for JSON fields. Escapes any unescaped characters.
    /// </summary>
    public static string ForJSON(string command)
    {
        if (command.StartsWith("/"))
            return command.Replace("\"", "\\\"");
        return '/' + command.Replace("\"", "\\\"");
    }
    /// <summary>
    ///     Returns this string, but surrounded with quotation marks if it contains whitespace.
    /// </summary>
    public static string AsCommandParameterString(this string parameter)
    {
        if (parameter.Contains(' '))
            return '"' + parameter + '"';
        return parameter;
    }
    /// <summary>
    ///     Returns the commands as literal, chat notation for animation controller fields.
    /// </summary>
    public static string[] ForJSON(string[] commands)
    {
        for (int i = 0; i < commands.Length; i++)
            commands[i] = ForJSON(commands[i]);

        return commands;
    }

    public static void ResetState() { UTIL.tags.Clear(); }

    /// <summary>
    ///     Throws a <see cref="StatementException" /> if the provided string contains any whitespace characters.
    /// </summary>
    /// <param name="text">
    ///     The string to validate for the absence of whitespace.
    /// </param>
    /// <param name="parameterName">
    ///     The name of the parameter being validated, to include in the exception message if thrown.
    /// </param>
    /// <param name="callingStatement">
    ///     The statement that initiated the validation, to include in the exception if thrown.
    /// </param>
    public static void ThrowIfWhitespace(this string text, string parameterName, Statement callingStatement)
    {
        if (text.Any(char.IsWhiteSpace))
            throw new StatementException(callingStatement, $"The parameter {parameterName} cannot contain whitespace.");
    }

    public static string String(this ItemSlot slot) { return slot.ToString().Replace('_', '.'); }
    public static string String(this ScoreboardOp op)
    {
        return op switch
        {
            ScoreboardOp.SET => "=",
            ScoreboardOp.ADD => "+=",
            ScoreboardOp.SUB => "-=",
            ScoreboardOp.MUL => "*=",
            ScoreboardOp.DIV => "/=",
            ScoreboardOp.MOD => "%=",
            ScoreboardOp.SWAP => "><",
            ScoreboardOp.MIN => "<",
            ScoreboardOp.MAX => ">",
            _ => "???"
        };
    }
    public static string String(this StructureRotation rot) { return rot.ToString()[1..]; }

    public static string AlwaysDay(bool enabled) { return $"alwaysday {enabled.ToString().ToLower()}"; }

    public static string CameraClear(string players) { return $"camera {players} clear"; }
    public static string CameraFade(string players) { return $"camera {players} fade"; }
    public static string CameraFade(string players, int red, int green, int blue)
    {
        return $"camera {players} fade color {red} {green} {blue}";
    }
    public static string CameraFade(string players, decimal fadeInSeconds, decimal holdSeconds, decimal fadeOutSeconds)
    {
        return $"camera {players} fade time {fadeInSeconds} {holdSeconds} {fadeOutSeconds}";
    }
    public static string CameraFade(string players,
        decimal fadeInSeconds,
        decimal holdSeconds,
        decimal fadeOutSeconds,
        int red,
        int green,
        int blue)
    {
        return $"camera {players} fade time {fadeInSeconds} {holdSeconds} {fadeOutSeconds} color {red} {green} {blue}";
    }
    public static CameraBuilder Camera(string players, CameraPreset minecraftPreset)
    {
        return new CameraBuilder(players, "minecraft:" + minecraftPreset);
    }
    public static CameraBuilder Camera(string players, string namespacedPreset)
    {
        return new CameraBuilder(players, namespacedPreset);
    }

    public static string CameraShake(string target) { return $"camerashake add {target}"; }
    public static string CameraShake(string target, float intensity) { return $"camerashake add {target} {intensity}"; }
    public static string CameraShake(string target, float intensity, float seconds)
    {
        return $"camerashake add {target} {intensity} {seconds}";
    }
    public static string CameraShake(string target, float intensity, float seconds, CameraShakeType shakeType)
    {
        return $"camerashake add {target} {intensity} {seconds} {shakeType}";
    }

    public static string Clear() { return "clear"; }
    public static string Clear(string target) { return $"clear {target}"; }
    public static string Clear(string target, string item) { return $"clear {target} {item}"; }
    public static string Clear(string target, string item, int data) { return $"clear {target} {item} {data}"; }
    public static string Clear(string target, string item, int data, int maxCount)
    {
        return $"clear {target} {item} {data} {maxCount}";
    }

    public static string ClearSpawnPoint(string target) { return $"clearspawnpoint {target}"; }

    public static string Clone(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        Coordinate dstX,
        Coordinate dstY,
        Coordinate dstZ)
    {
        return $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ}";
    }
    public static string Clone(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        Coordinate dstX,
        Coordinate dstY,
        Coordinate dstZ,
        bool copyAir,
        CloneMode mode)
    {
        return $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} " + (copyAir ? "replace" : "masked") +
               $" {mode}";
    }

    public static string ConnectWSServer(string serverUri) { return $"wsserver {serverUri}"; }

    public static string Damage(string target, int amount) { return $"damage {target} {amount}"; }
    public static string Damage(string target, int amount, DamageCause cause)
    {
        return $"damage {target} {amount} {cause}";
    }
    public static string Damage(string target, int amount, DamageCause cause, string damager)
    {
        return $"damage {target} {amount} {cause} entity {damager}";
    }

    public static string Deop(string target) { return $"deop {target}"; }

    public static string DialogueChange(string npc, string sceneName)
    {
        return $"dialogue change {npc.AsCommandParameterString()} {sceneName}";
    }
    public static string DialogueChange(string npc, Scene scene)
    {
        return $"dialogue change {npc.AsCommandParameterString()} {scene.sceneTag}";
    }
    public static string DialogueChange(string npc, string sceneName, string players)
    {
        return $"dialogue change {npc.AsCommandParameterString()} {sceneName} {players.AsCommandParameterString()}";
    }
    public static string DialogueChange(string npc, Scene scene, string players)
    {
        return
            $"dialogue change {npc.AsCommandParameterString()} {scene.sceneTag} {players.AsCommandParameterString()}";
    }
    public static string DialogueOpen(string npc, string players)
    {
        return $"dialogue open {npc.AsCommandParameterString()} {players.AsCommandParameterString()}";
    }
    public static string DialogueOpen(string npc, string players, string sceneName)
    {
        return $"dialogue open {npc.AsCommandParameterString()} {players.AsCommandParameterString()} {sceneName}";
    }
    public static string DialogueOpen(string npc, string players, Scene scene)
    {
        return $"dialogue open {npc.AsCommandParameterString()} {players.AsCommandParameterString()} {scene.sceneTag}";
    }

    public static string Difficulty(DifficultyMode difficulty) { return $"difficulty {difficulty}"; }

    public static string EffectClear(string target) { return $"effect {target} clear"; }
    public static string Effect(string target, PotionEffect effect) { return $"effect {target} {effect}"; }
    public static string Effect(string target, PotionEffect effect, int seconds)
    {
        return $"effect {target} {effect} {seconds}";
    }
    public static string Effect(string target, PotionEffect effect, int seconds, int amplifier)
    {
        return $"effect {target} {effect} {seconds} {amplifier}";
    }
    public static string Effect(string target, PotionEffect effect, int seconds, int amplifier, bool hideParticles)
    {
        return $"effect {target} {effect} {seconds} {amplifier} {hideParticles.ToString().ToLower()}";
    }

    public static string Enchant(string target, Enchantment enchantment) { return $"effect {target} {enchantment}"; }
    public static string Enchant(string target, Enchantment enchantment, int level)
    {
        return $"effect {target} {enchantment} {level}";
    }

    public static string Event(string target, string eventName) { return $"event entity {target} {eventName}"; }

    public static ExecuteBuilder Execute() { return new ExecuteBuilder(); }

    public static string Fill(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        string block)
    {
        return $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()}";
    }
    public static string Fill(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        string block,
        [CanBeNull] BlockState[] blockStates)
    {
        return $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} {blockStates.ToVanillaSyntax()}";
    }
    public static string Fill(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        string block,
        [CanBeNull] BlockState[] blockStates,
        OldHandling fillMode)
    {
        return $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} {blockStates} {fillMode}";
    }
    public static string Fill(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        string block,
        [CanBeNull] BlockState[] blockStates,
        string replaceBlock)
    {
        return
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} {blockStates.ToVanillaSyntax()} replace {replaceBlock.AsCommandParameterString()} -1";
    }
    public static string Fill(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        string block,
        [CanBeNull] BlockState[] blockStates,
        string replaceBlock,
        [CanBeNull] BlockState[] replaceBlockStates)
    {
        return
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} {blockStates.ToVanillaSyntax()} replace {replaceBlock.AsCommandParameterString()} {replaceBlockStates.ToVanillaSyntax()}";
    }

    public static string FogPush(string target, string fogId, string userProvidedId)
    {
        return $"fog {target} push {fogId} {userProvidedId.AsCommandParameterString()}";
    }
    public static string FogPop(string target, string userProvidedId)
    {
        return $"fog {target} pop {userProvidedId.AsCommandParameterString()}";
    }
    public static string FogRemove(string target, string userProvidedId)
    {
        return $"fog {target} remove {userProvidedId.AsCommandParameterString()}";
    }

    public static string Function(string name) { return $"function {name}"; }
    public static string Function(CommandFile function) { return $"function {function.CommandReference}"; }

    public static string Gamemode(string target, GameMode mode) { return $"gamemode {mode} {target}"; }
    public static string Gamemode(string target, int mode) { return $"gamemode {mode} {target}"; }

    public static string Gamerule(GameRule rule, string value) { return $"gamerule {rule} {value}"; }
    public static string Gamerule(string rule, string value) { return $"gamerule {rule} {value}"; }

    public static string Give(string target, string item) { return $"give {target} {item}"; }
    public static string Give(string target, string item, int amount) { return $"give {target} {item} {amount}"; }
    public static string Give(string target, string item, int amount, int data)
    {
        return $"give {target} {item} {amount} {data}";
    }
    public static string Give(string target, string item, int amount, int data, string json)
    {
        return $"give {target} {item} {amount} {data} {json}";
    }

    public static string Help() { return "help"; }

    public static string ImmutableWorld(bool immutable) { return $"immutableworld {immutable.ToString().ToLower()}"; }

    public static string Kick(string target) { return $"kick {target}"; }
    public static string Kick(string target, string reason) { return $"kick {target} {reason}"; }

    public static string Kill() { return "kill"; }
    public static string Kill(string target) { return $"kill {target}"; }

    public static string List() { return "list"; }

    public static string Locate(StructureType type) { return $"locate {type}"; }
    public static string Locate(string structureType) { return $"locate {structureType}"; }

    public static string LootTable(Coordinate x, Coordinate y, Coordinate z, LootTable table)
    {
        return $"loot spawn {x} {y} {z} loot {table.CommandPath}";
    }
    public static string LootTable(Coordinate x, Coordinate y, Coordinate z, string table)
    {
        return $"loot spawn {x} {y} {z} loot {table}";
    }
    public static string LootEntity(Coordinate x, Coordinate y, Coordinate z, string entity)
    {
        return $"loot spawn {x} {y} {z} kill {entity}";
    }

    public static string Me(string text) { return $"me {text}"; }

    public static string MobEvent(MobEventType @event, bool value)
    {
        return $"mobevent minecraft:{@event} {value.ToString().ToLower()}";
    }
    public static string MobEvent(string @event, bool value)
    {
        return $"mobevent {@event} {value.ToString().ToLower()}";
    }
    public static string MobEvent(MobEventType @event) { return $"mobevent minecraft:{@event}"; }
    public static string MobEvent(string @event) { return $"mobevent {@event}"; }
    public static string MobEventsDisable() { return "mobevent events_enabled false"; }
    public static string MobEventsEnable() { return "mobevent events_enabled true"; }

    public static string Message(string target, string message) { return $"w {target} {message}"; }

    public static string MusicPlay(string track,
        float volume = 1f,
        float fadeSeconds = 0f,
        MusicRepeatMode repeatMode = MusicRepeatMode.play_once)
    {
        return $"music play {track} {volume} {fadeSeconds} {repeatMode}";
    }
    public static string MusicQueue(string track,
        float volume = 1f,
        float fadeSeconds = 0f,
        MusicRepeatMode repeatMode = MusicRepeatMode.play_once)
    {
        return $"music queue {track} {volume} {fadeSeconds} {repeatMode}";
    }
    public static string MusicStop(float fadeSeconds) { return $"music stop {fadeSeconds}"; }
    public static string MusicVolume(float volume) { return $"music volume {volume}"; }

    public static string Op(string target) { return $"op {target}"; }

    public static string Particle(string effect, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"particle {effect} {x} {y} {z}";
    }

    public static string PlayAnimation(string target, string animation)
    {
        return $"playanimation {target} {animation}";
    }
    public static string PlayAnimation(string target, string animation, string nextState)
    {
        return $"playanimation {target} {animation} {nextState}";
    }
    public static string PlayAnimation(string target, string animation, string nextState, float blendOutTime)
    {
        return $"playanimation {target} {animation} {nextState} {blendOutTime}";
    }
    public static string PlayAnimation(string target,
        string animation,
        string nextState,
        float blendOutTime,
        string molangStopCondition)
    {
        return $"playanimation {target} {animation} {nextState} {blendOutTime} {molangStopCondition}";
    }
    public static string PlayAnimation(string target,
        string animation,
        string nextState,
        float blendOutTime,
        string molangStopCondition,
        string controller)
    {
        return $"playanimation {target} {animation} {nextState} {blendOutTime} {molangStopCondition} {controller}";
    }

    public static string PlaySound(string sound) { return $"playsound {sound}"; }
    public static string PlaySound(string sound, string target) { return $"playsound {sound} {target}"; }
    public static string PlaySound(string sound, string target, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"playsound {sound} {target} {x} {y} {z}";
    }
    public static string PlaySound(string sound, string target, Coordinate x, Coordinate y, Coordinate z, float volume)
    {
        return $"playsound {sound} {target} {x} {y} {z} {volume}";
    }
    public static string PlaySound(string sound,
        string target,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        float volume,
        float pitch)
    {
        return $"playsound {sound} {target} {x} {y} {z} {volume} {pitch}";
    }
    public static string PlaySound(string sound,
        string target,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        float volume,
        float pitch,
        float minVolume)
    {
        return $"playsound {sound} {target} {x} {y} {z} {volume} {pitch} {minVolume}";
    }
    public static string PlaySound(string sound, string target, float volume)
    {
        return $"playsound {sound} {target} ~ ~ ~ {volume}";
    }
    public static string PlaySound(string sound, string target, float volume, float pitch)
    {
        return $"playsound {sound} {target} ~ ~ ~ {volume} {pitch}";
    }
    public static string PlaySound(string sound, string target, float volume, float pitch, float minVolume)
    {
        return $"playsound {sound} {target} ~ ~ ~ {volume} {pitch} {minVolume}";
    }

    public static string Reload() { return "reload"; }

    public static string ReplaceItemBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        int slot,
        OldHandling handling,
        string item)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item}";
    }
    public static string ReplaceItemBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        int slot,
        OldHandling handling,
        string item,
        int amount)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount}";
    }
    public static string ReplaceItemBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        int slot,
        OldHandling handling,
        string item,
        int amount,
        int data)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount} {data}";
    }
    public static string ReplaceItemBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        int slot,
        OldHandling handling,
        string item,
        int amount,
        int data,
        string json)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount} {data} {json}";
    }

    public static string ReplaceItemEntity(string target,
        ItemSlot slotType,
        int slot,
        OldHandling handling,
        string item)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item}";
    }
    public static string ReplaceItemEntity(string target,
        ItemSlot slotType,
        int slot,
        OldHandling handling,
        string item,
        int amount)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount}";
    }
    public static string ReplaceItemEntity(string target,
        ItemSlot slotType,
        int slot,
        OldHandling handling,
        string item,
        int amount,
        int data)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount} {data}";
    }
    public static string ReplaceItemEntity(string target,
        ItemSlot slotType,
        int slot,
        OldHandling handling,
        string item,
        int amount,
        int data,
        string json)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount} {data} {json}";
    }

    public static string Ride(string sources,
        string targets,
        RideTeleportRules tpRules = RideTeleportRules.teleport_rider,
        RideFillType fillType = RideFillType.until_full)
    {
        return $"ride {sources} start_riding {targets} {tpRules} {fillType}";
    }
    public static string RideDismount(string riders) { return $"ride {riders} stop_riding"; }
    public static string RideEvictRiders(string rides) { return $"ride {rides} evict_riders"; }
    public static string RideSummonRider(string rides, string entity) { return $"ride {rides} summon_rider {entity}"; }
    public static string RideSummonRider(string rides, string entity, string nameTag)
    {
        return $"ride {rides} summon_rider {entity} none {nameTag.AsCommandParameterString()}";
    }
    public static string RideSummonRide(string riders, string entity) { return $"ride {riders} summon_ride {entity}"; }
    public static string RideSummonRide(string riders, string entity, RideSummonRules summonRules)
    {
        return $"ride {riders} summon_ride {entity} {summonRules}";
    }
    public static string RideSummonRide(string riders, string entity, RideSummonRules summonRules, string nameTag)
    {
        return $"ride {riders} summon_ride {entity} {summonRules} none {nameTag.AsCommandParameterString()}";
    }

    public static string SaveHold() { return "save hold"; }
    public static string SaveQuery() { return "save query"; }
    public static string SaveResume() { return "save resume"; }

    public static string Say(string message) { return $"say {message}"; }

    public static string ScheduleAreaLoaded(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        string function)
    {
        return $"schedule on_area_loaded add {x1} {y1} {z1} {x2} {y2} {z2}";
    }
    public static string ScheduleAreaLoaded(Coordinate x, Coordinate y, Coordinate z, int radius, string function)
    {
        return $"schedule on_area_loaded add circle {x} {y} {z} {radius} {function}";
    }
    public static string ScheduleAreaLoaded(string tickingArea, string function)
    {
        return $"schedule on_area_loaded add tickingarea {tickingArea} {function}";
    }

    public static string ScoreboardCreateObjective(string name) { return $"scoreboard objectives add {name} dummy"; }
    public static string ScoreboardCreateObjective(string name, string display)
    {
        return
            $"scoreboard objectives add {name.AsCommandParameterString()} dummy {display.AsCommandParameterString()}";
    }
    public static string ScoreboardRemoveObjective(string name)
    {
        return $"scoreboard objectives remove {name.AsCommandParameterString()}";
    }
    public static string ScoreboardDisplayList(string name)
    {
        return $"scoreboard objectives setdisplay list {name.AsCommandParameterString()}";
    }
    public static string ScoreboardDisplayList(string name, ScoreboardOrdering ordering)
    {
        return $"scoreboard objectives setdisplay list {name.AsCommandParameterString()} {ordering}";
    }
    public static string ScoreboardDisplaySidebar(string name)
    {
        return $"scoreboard objectives setdisplay sidebar {name.AsCommandParameterString()}";
    }
    public static string ScoreboardDisplaySidebar(string name, ScoreboardOrdering ordering)
    {
        return $"scoreboard objectives setdisplay sidebar {name.AsCommandParameterString()} {ordering}";
    }
    public static string ScoreboardDisplayBelowName(string name)
    {
        return $"scoreboard objectives setdisplay belowname {name.AsCommandParameterString()}";
    }
    public static string ScoreboardList(string target) { return $"scoreboard players list {target}"; }

    public static string ScoreboardSet(string target, string objective, int value)
    {
        return $"scoreboard players set {target} {objective.AsCommandParameterString()} {value}";
    }
    public static string ScoreboardAdd(string target, string objective, int amount)
    {
        return $"scoreboard players add {target} {objective.AsCommandParameterString()} {amount}";
    }
    public static string ScoreboardSubtract(string target, string objective, int amount)
    {
        return $"scoreboard players remove {target} {objective.AsCommandParameterString()} {amount}";
    }
    public static string ScoreboardRandom(string target, string objective, int minInclusive, int maxInclusive)
    {
        return
            $"scoreboard players random {target} {objective.AsCommandParameterString()} {minInclusive} {maxInclusive}";
    }
    public static string ScoreboardReset(string target, string objective)
    {
        return $"scoreboard players reset {target} {objective.AsCommandParameterString()}";
    }
    public static string ScoreboardOpRaw(string targetA, string a, ScoreboardOp op, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} {op.String()} {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpSet(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} = {targetB} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpAdd(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} += {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpSub(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} -= {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMul(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} *= {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpDiv(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} /= {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMod(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} %= {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpSwap(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} >< {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMin(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} < {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMax(string targetA, string a, string targetB, string b)
    {
        return
            $"scoreboard players operation {targetA.AsCommandParameterString()} {a.AsCommandParameterString()} > {targetB.AsCommandParameterString()} {b.AsCommandParameterString()}";
    }

    public static string ScoreboardSet(ScoreboardValue objective, int value)
    {
        return
            $"scoreboard players set {objective.clarifier.CurrentString} {objective.InternalName.AsCommandParameterString()} {value}";
    }
    public static string ScoreboardAdd(ScoreboardValue objective, int value)
    {
        return
            $"scoreboard players add {objective.clarifier.CurrentString} {objective.InternalName.AsCommandParameterString()} {value}";
    }
    public static string ScoreboardSubtract(ScoreboardValue objective, int amount)
    {
        return
            $"scoreboard players remove {objective.clarifier.CurrentString} {objective.InternalName.AsCommandParameterString()} {amount}";
    }
    public static string ScoreboardRandom(ScoreboardValue objective, int minInclusive, int maxInclusive)
    {
        return
            $"scoreboard players random {objective.clarifier.CurrentString} {objective.InternalName.AsCommandParameterString()} {minInclusive} {maxInclusive}";
    }
    public static string ScoreboardReset(ScoreboardValue objective)
    {
        return
            $"scoreboard players reset {objective.clarifier.CurrentString} {objective.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardReset(string selector, ScoreboardValue objective)
    {
        return
            $"scoreboard players reset {selector.AsCommandParameterString()} {objective.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpRaw(ScoreboardValue a, ScoreboardOp op, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} {op.String()} {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpSet(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} = {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpAdd(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} += {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpSub(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} -= {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMul(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} *= {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpDiv(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} /= {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMod(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} %= {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpSwap(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} >< {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMin(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} < {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }
    public static string ScoreboardOpMax(ScoreboardValue a, ScoreboardValue b)
    {
        return
            $"scoreboard players operation {a.clarifier.CurrentString} {a.InternalName.AsCommandParameterString()} > {b.clarifier.CurrentString} {b.InternalName.AsCommandParameterString()}";
    }

    public static string SetBlock(Coordinate x, Coordinate y, Coordinate z, string block)
    {
        return $"setblock {x} {y} {z} {block}";
    }
    public static string SetBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        string block,
        [CanBeNull] BlockState[] blockStates)
    {
        return $"setblock {x} {y} {z} {block} {blockStates.ToVanillaSyntax()}";
    }
    public static string SetBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        string block,
        [CanBeNull] BlockState[] blockStates,
        OldHandling handling)
    {
        return $"setblock {x} {y} {z} {block} {blockStates.ToVanillaSyntax()} {handling}";
    }

    public static string SetMaxPlayers(int max) { return $"setmaxplayers {max}"; }

    public static string SetWorldSpawn(Coordinate x, Coordinate y, Coordinate z)
    {
        return $"setworldspawn {x} {y} {z}";
    }

    public static string Spawnpoint() { return "spawnpoint"; }
    public static string Spawnpoint(string target) { return $"spawnpoint {target}"; }
    public static string Spawnpoint(string target, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"spawnpoint {target} {x} {y} {z}";
    }

    public static string SpreadPlayers(Coordinate x, Coordinate z, float spreadDistance, float maxRange, string targets)
    {
        return $"spreadplayers {x} {z} {spreadDistance} {maxRange} {targets.AsCommandParameterString()}";
    }

    public static string Stop() { return "stop"; }

    public static string StopSound(string target) { return $"stopsound {target}"; }
    public static string StopSound(string target, string sound) { return $"stopsound {target} {sound}"; }

    public static string StructureSaveDisk(string name,
        Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2)
    {
        return $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} disk";
    }
    public static string StructureSaveMemory(string name,
        Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2)
    {
        return $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} memory";
    }
    public static string StructureSaveDisk(string name,
        Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        bool includeEntities,
        bool includeBlocks = true)
    {
        return
            $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} {includeEntities} disk {includeBlocks.ToString().ToLower()}";
    }
    public static string StructureSaveMemory(string name,
        Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        bool includeEntities,
        bool includeBlocks = true)
    {
        return
            $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} {includeEntities} memory {includeBlocks.ToString().ToLower()}";
    }
    public static string StructureLoad(string name,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        StructureRotation rotation = StructureRotation._0_degrees,
        StructureMirror flip = StructureMirror.none,
        bool includeEntities = true,
        bool includeBlocks = true,
        bool waterLogged = false,
        decimal integrity = 100,
        string seed = null)
    {
        return integrity.Equals(100)
            ? $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()}"
            : seed == null
                ? $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity}"
                : $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity} {seed}";
    }
    public static string StructureLoad(string name,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        StructureRotation rotation = StructureRotation._0_degrees,
        StructureMirror flip = StructureMirror.none,
        StructureAnimationMode animation = StructureAnimationMode.layer_by_layer,
        float animationSeconds = 0,
        bool includeEntities = true,
        bool includeBlocks = true,
        bool waterLogged = false,
        decimal integrity = 100,
        string seed = null)
    {
        if (seed == null)
            return
                $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {animation} {animationSeconds} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity}";

        return
            $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {animation} {animationSeconds} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity} {seed}";
    }
    public static string StructureDelete(string name) { return $"structure delete {name.AsCommandParameterString()}"; }

    public static string Summon(string entity) { return $"summon {entity}"; }
    public static string Summon(string entity, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"summon {entity} {x} {y} {z}";
    }
    public static string Summon(string entity, string nameTag, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"summon {entity} {nameTag.AsCommandParameterString()} {x} {y} {z}";
    }
    public static string Summon(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate yRot,
        Coordinate xRot)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot}";
    }
    public static string Summon(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate yRot,
        Coordinate xRot,
        string nameTag)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot} \"\" {nameTag.AsCommandParameterString()}";
    }
    public static string Summon(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate yRot,
        Coordinate xRot,
        string nameTag,
        string spawnEvent)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot} {spawnEvent} {nameTag.AsCommandParameterString()}";
    }
    public static string SummonWithEvent(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate yRot,
        Coordinate xRot,
        string spawnEvent)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot} {spawnEvent}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, string facingSelector)
    {
        return $"summon {entity} {x} {y} {z} facing {facingSelector}";
    }
    public static string SummonFacing(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate faceX,
        Coordinate faceY,
        Coordinate faceZ)
    {
        return $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ}";
    }
    public static string SummonFacing(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        string facingSelector,
        string nameTag)
    {
        return $"summon {entity} {x} {y} {z} facing {facingSelector} \"\" {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacing(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate faceX,
        Coordinate faceY,
        Coordinate faceZ,
        string nameTag)
    {
        return $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ} \"\" {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacing(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        string facingSelector,
        string nameTag,
        string spawnEvent)
    {
        return
            $"summon {entity} {x} {y} {z} facing {facingSelector} {spawnEvent.AsCommandParameterString()} {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacing(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate faceX,
        Coordinate faceY,
        Coordinate faceZ,
        string nameTag,
        string spawnEvent)
    {
        return
            $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ} {spawnEvent.AsCommandParameterString()} {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacingWithEvent(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        string facingSelector,
        string eventName)
    {
        return $"summon {entity} {x} {y} {z} facing {facingSelector} {eventName.AsCommandParameterString()}";
    }
    public static string SummonFacingWithEvent(string entity,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate faceX,
        Coordinate faceY,
        Coordinate faceZ,
        string eventName)
    {
        return $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ} {eventName.AsCommandParameterString()}";
    }

    public static string Tag(string targets, string tag)
    {
        return $"tag {targets.AsCommandParameterString()} add {tag.AsCommandParameterString()}";
    }
    public static string TagRemove(string targets, string tag)
    {
        return $"tag {targets.AsCommandParameterString()} remove {tag.AsCommandParameterString()}";
    }
    public static string TagList(string targets) { return $"tag {targets.AsCommandParameterString()} list"; }

    public static string Teleport(string otherEntity, bool checkForBlocks = false)
    {
        return $"tp @s {otherEntity} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(string target, string otherEntity, bool checkForBlocks = false)
    {
        return $"tp {target} {otherEntity} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(Coordinate x, Coordinate y, Coordinate z, bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate yRot,
        Coordinate xRot,
        bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} {yRot} {xRot} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(string target, Coordinate x, Coordinate y, Coordinate z, bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(string target,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate yRot,
        Coordinate xRot,
        bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} {yRot} {xRot} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate facingX,
        Coordinate facingY,
        Coordinate facingZ,
        bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} facing {facingX} {facingY} {facingZ} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(Coordinate x,
        Coordinate y,
        Coordinate z,
        string facingEntity,
        bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} facing {facingEntity} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(string target,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        Coordinate facingX,
        Coordinate facingY,
        Coordinate facingZ,
        bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} facing {facingX} {facingY} {facingZ} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(string target,
        Coordinate x,
        Coordinate y,
        Coordinate z,
        string facingEntity,
        bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} facing {facingEntity} {checkForBlocks.ToString().ToLower()}";
    }

    public static string Tellraw(string jsonMessage) { return $"tellraw @a {jsonMessage}"; }
    public static string Tellraw(string targets, string jsonMessage) { return $"tellraw {targets} {jsonMessage}"; }
    public static string Tellraw(JObject jsonMessage) { return $"tellraw @a {jsonMessage.ToString(Formatting.None)}"; }
    public static string Tellraw(string targets, JObject jsonMessage)
    {
        return $"tellraw {targets} {jsonMessage.ToString(Formatting.None)}";
    }

    public static string TestFor(string targets) { return $"testfor {targets}"; }

    public static string TestForBlock(Coordinate x, Coordinate y, Coordinate z, string block)
    {
        return $"testforblock {x} {y} {z} {block}";
    }
    public static string TestForBlock(Coordinate x,
        Coordinate y,
        Coordinate z,
        string block,
        [CanBeNull] BlockState[] blockStates)
    {
        return $"testforblock {x} {y} {z} {block} {blockStates.ToVanillaSyntax()}";
    }

    public static string TestForBlocks(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        Coordinate dstX,
        Coordinate dstY,
        Coordinate dstZ)
    {
        return $"testforblocks {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ}";
    }
    public static string TestForBlocksMasked(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2,
        Coordinate dstX,
        Coordinate dstY,
        Coordinate dstZ)
    {
        return $"testforblocks {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} masked";
    }

    public static string TickingAreaAdd(string name,
        Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2)
    {
        return $"tickingarea add {x1} {y1} {z1} {x2} {y2} {z2} {name.AsCommandParameterString()}";
    }
    public static string TickingAreaAdd(string name, Coordinate x1, Coordinate y1, Coordinate z1, int radius)
    {
        return $"tickingarea add circle {x1} {y1} {z1} {radius} {name.AsCommandParameterString()}";
    }
    public static string TickingAreaRemove(string name)
    {
        return $"tickingarea remove {name.AsCommandParameterString()}";
    }
    public static string TickingAreaRemove(Coordinate x1, Coordinate y1, Coordinate z1)
    {
        return $"tickingarea remove {x1} {y1} {z1}";
    }
    public static string TickingAreaRemoveAll() { return "tickingarea remove_all"; }
    public static string TickingAreaList() { return "tickingarea list"; }
    public static string TickingAreaListAll() { return "tickingarea list all-dimensions"; }

    public static string TimeAdd(int ticks) { return $"time add {ticks}"; }
    public static string TimeSet(int ticks) { return $"time set {ticks}"; }
    public static string TimeSet(TimeSpec spec) { return $"time set {spec}"; }
    public static string TimeGet(TimeQuery query) { return $"time query {query}"; }

    public static string TitleClear(string target) { return $"titleraw {target} clear"; }
    public static string TitleReset(string target) { return $"titleraw {target} reset"; }
    public static string TitleSubtitle(string target, string json) { return $"titleraw {target} subtitle {json}"; }
    public static string TitleActionBar(string target, string json) { return $"titleraw {target} actionbar {json}"; }
    public static string Title(string target, string json) { return $"titleraw {target} title {json}"; }
    public static string TitleTimes(string target, int fadeIn, int stay, int fadeOut)
    {
        return $"titleraw {target} times {fadeIn} {stay} {fadeOut}";
    }

    public static string ToggleDownfall() { return "toggledownfall"; }

    public static string Weather(WeatherState state) { return $"weather {state}"; }
    public static string Weather(WeatherState state, int durationTicks) { return $"weather {state} {durationTicks}"; }
    public static string WeatherQuery() { return "weather query"; }

    public static string Whitelist(string player) { return $"whitelist add {player.AsCommandParameterString()}"; }
    public static string WhitelistRemove(string player)
    {
        return $"whitelist remove {player.AsCommandParameterString()}";
    }
    public static string WhitelistList() { return "whitelist list"; }
    public static string WhitelistOff() { return "whitelist off"; }
    public static string WhitelistOn() { return "whitelist on"; }
    public static string WhitelistReload() { return "whitelist reload"; }

    public static string Xp(int amount) { return $"xp {amount}"; }
    public static string XpL(int amount) { return $"xp {amount}L"; }
    public static string Xp(int amount, string target) { return $"xp {amount} {target}"; }
    public static string XpL(int amount, string target) { return $"xp {amount}L {target}"; }

    /// <summary>
    ///     Command utils that require instance stuff.
    /// </summary>
    public class Util
    {
        internal readonly Dictionary<int, int> tags = new();

        /// <summary>
        ///     Get a unique tag incase used somewhere else in-scope.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public IndexedTag PushTag(string tag)
        {
            int hash = tag.GetHashCode();
            if (this.tags.TryGetValue(hash, out int index))
            {
                var ret = new IndexedTag
                {
                    name = tag,
                    index = index
                };

                index++;
                this.tags[hash] = index;
                return ret;
            }

            this.tags[hash] = 1;
            return new IndexedTag
            {
                name = tag,
                index = 0
            };
        }
        /// <summary>
        ///     Pop a tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public void PopTag(IndexedTag tag)
        {
            int hash = tag.name.GetHashCode();
            int index = this.tags[hash];
            this.tags[hash] = index - 1;
        }

        /// <summary>
        ///     Make an entity invisible via a potion effect.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string MakeInvisible(string entity)
        {
            return Effect(entity, PotionEffect.invisibility, 99999999, 0, true);
        }

        /// <summary>
        ///     Prepends "minecraft:" if no namespace is identifier.
        /// </summary>
        /// <returns></returns>
        public static string RequireNamespace(string identifier)
        {
            if (identifier.Contains(':'))
                return identifier;
            return "minecraft:" + identifier;
        }
        /// <summary>
        ///     Strips the namespace off of the given identifier.
        ///     <code>
        /// minecraft:diamond → diamond
        /// </code>
        /// </summary>
        /// <returns></returns>
        public static string StripNamespace(string identifier)
        {
            if (identifier.Contains(':'))
            {
                int lastColon = identifier.LastIndexOf(':');
                return lastColon + 1 >= identifier.Length ? identifier.Trim(':') : identifier[(lastColon + 1)..];
            }

            return identifier;
        }

        /// <summary>
        ///     Makes a string inverted/not depending on a boolean. "!thing" or "thing"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        public static string MakeInvertedString(string str, bool invert)
        {
            bool isInverted = str.StartsWith('!');

            if (isInverted == invert)
                return str;

            if (isInverted)
                return str[1..];
            return '!' + str;
        }
        /// <summary>
        ///     Makes a string inverted/not depending on a boolean. "!thing" or "thing"
        /// </summary>
        /// <param name="str">The string to invert.</param>
        /// <returns>The inverted string. Will never return the same <c>string</c> instance.</returns>
        public string ToggleInversion(string str)
        {
            bool isInverted = str.StartsWith("!");

            if (isInverted)
                return str[1..];
            return '!' + str;
        }
    }
}

/// <summary>
///     A tag that contains an index. Used with Command.UTIL.[Push/Pop]Tag
/// </summary>
public struct IndexedTag
{
    public string name;
    public int index;

    public string Tag => this.name + this.index;
    public static implicit operator string(IndexedTag tag) { return tag.Tag; }
}

public enum CameraShakeType
{
    /// <summary>
    ///     The shake effect will only affect the position of the camera.
    /// </summary>
    positional,
    /// <summary>
    ///     The shake effect will only affect the rotation of the camera.
    /// </summary>
    rotational
}

/// <summary>
///     A camera preset.
/// </summary>
public enum CameraPreset
{
    /// <summary>
    ///     The default camera preset.
    /// </summary>
    first_person,
    /// <summary>
    ///     The camera is free to move wherever and is not attached to any particular player.
    /// </summary>
    free,
    /// <summary>
    ///     The default third-person camera preset.
    /// </summary>
    third_person,
    /// <summary>
    ///     The default alternate third-person camera preset.
    /// </summary>
    third_person_front,
    control_scheme_camera,
    fixed_boom,
    follow_orbit
}

/// <summary>
///     A type of command block.
/// </summary>
[UsableInMCC]
public enum CommandBlockType
{
    /// <summary>
    ///     The command block will run once when powered by redstone.
    /// </summary>
    impulse,
    /// <summary>
    ///     The command block will run when activated by another command block pointing at it.
    /// </summary>
    chain,
    /// <summary>
    ///     The command block will run either continuously or when powered by redstone. Supports a tick delay.
    /// </summary>
    repeating
}

/// <summary>
///     Easing function used for camera movements.
/// </summary>
[UsableInMCC]
public enum Easing
{
    linear,
    spring,
    in_quad,
    out_quad,
    in_out_quad,
    in_cubic,
    out_cubic,
    in_out_cubic,
    in_quart,
    out_quart,
    in_out_quart,
    in_quint,
    out_quint,
    in_out_quint,
    in_sine,
    out_sine,
    in_out_sine,
    in_expo,
    out_expo,
    in_out_expo,
    in_circ,
    out_circ,
    in_out_circ,
    in_bounce,
    out_bounce,
    in_out_bounce,
    in_back,
    out_back,
    in_out_back,
    in_elastic,
    out_elastic,
    in_out_elastic
}

public enum CloneMode
{
    /// <summary>
    ///     Force the clone even if the source and destination regions overlap
    /// </summary>
    force,
    /// <summary>
    ///     Clone the source region to the destination region, then replace the source region with air.
    ///     When used in filtered mask mode, only the cloned blocks are replaced with air.
    /// </summary>
    move,
    /// <summary>
    ///     Don't move or force.
    /// </summary>
    normal
}

[UsableInMCC]
public enum DifficultyMode
{
    /// <summary>
    ///     The 'peaceful' difficulty.
    /// </summary>
    peaceful = 0,
    /// <summary>
    ///     The 'easy' difficulty.
    /// </summary>
    easy = 1,
    /// <summary>
    ///     The 'normal' difficulty.
    /// </summary>
    normal = 2,
    /// <summary>
    ///     The 'hard' difficulty.
    /// </summary>
    hard = 3
}

[UsableInMCC]
public enum OldHandling
{
    /// <summary>
    ///     Destroys any previously existing blocks.
    /// </summary>
    destroy,
    /// <summary>
    ///     Keeps any previously existing blocks.
    /// </summary>
    keep,
    /// <summary>
    ///     Hollows out the area by filling the inside with air.
    /// </summary>
    hollow,
    /// <summary>
    ///     Fills only the edges of the area. Keeps the interior blocks.
    /// </summary>
    outline,
    /// <summary>
    ///     Replaces all previously existing blocks. (default)
    /// </summary>
    replace
}

[UsableInMCC]
public enum PotionEffect
{
    /// <summary>
    ///     Adds temporary bonus hearts to absorb damage.
    /// </summary>
    absorption,
    /// <summary>
    ///     Causes an ominous event upon entering a village or trial chamber.
    /// </summary>
    bad_omen,
    /// <summary>
    ///     Adds a thick black fog to obscure vision and prevents players from sprinting.
    /// </summary>
    blindness,
    /// <summary>
    ///     Gives vision underwater and prevents drowning.
    /// </summary>
    conduit_power,
    /// <summary>
    ///     Pulsating blindness-like effect.
    /// </summary>
    darkness,
    /// <summary>
    ///     Deals damage to the entity over time, but unlike regular poison, can kill it.
    /// </summary>
    fatal_poison,
    /// <summary>
    ///     Prevents the entity from taking fire/lava damage.
    /// </summary>
    fire_resistance,
    /// <summary>
    ///     Allows players to break blocks faster and attack faster.
    /// </summary>
    haste,
    /// <summary>
    ///     Increases the maximum health of the entity temporarily.
    ///     Different from absorption in that the hearts can be regenerated.
    /// </summary>
    health_boost,
    /// <summary>
    ///     Causes players to lose hunger much faster.
    /// </summary>
    hunger,
    /// <summary>
    ///     Makes the entity have a chance to spawn silverfish when attacked.
    /// </summary>
    infested,
    /// <summary>
    ///     Instantly deals damage to the entity, but heals undead mobs.
    /// </summary>
    instant_damage,
    /// <summary>
    ///     Instantly heals the entity, but deals damage to undead mobs.
    /// </summary>
    instant_health,
    /// <summary>
    ///     Makes the entity invisible (but not armor, held item, etc...) and makes it harder for mobs to detect it.
    /// </summary>
    invisibility,
    /// <summary>
    ///     Makes the entity jump higher.
    /// </summary>
    jump_boost,
    /// <summary>
    ///     Makes the entity levitate into the air against their will.
    /// </summary>
    levitation,
    /// <summary>
    ///     Makes players break blocks and attack more slowly.
    /// </summary>
    mining_fatigue,
    /// <summary>
    ///     Creates a warping, dizzy effect on screen for players.
    /// </summary>
    nausea,
    /// <summary>
    ///     Allows players to see in the dark.
    /// </summary>
    night_vision,
    /// <summary>
    ///     Makes the entity spawn 2 slimes on death.
    /// </summary>
    oozing,
    /// <summary>
    ///     Deals damage to the entity over time. Cannot directly kill the entity.
    /// </summary>
    poison,
    /// <summary>
    ///     Forcefully starts a raid once the effect expires.
    /// </summary>
    raid_omen,
    /// <summary>
    ///     Makes the entity's health regenerate more quickly.
    /// </summary>
    regeneration,
    /// <summary>
    ///     Makes the entity take less damage from all sources.
    /// </summary>
    resistance,
    /// <summary>
    ///     Refills food level and saturation of players.
    /// </summary>
    saturation,
    /// <summary>
    ///     Makes the entity fall more slowly.
    /// </summary>
    slow_falling,
    /// <summary>
    ///     Makes the entity move more slowly.
    /// </summary>
    slowness,
    /// <summary>
    ///     Makes the entity move faster.
    /// </summary>
    speed,
    /// <summary>
    ///     Makes the entity deal more damage with melee attacks.
    /// </summary>
    strength,
    /// <summary>
    ///     Turns any nearby trial spawners into ominous trial spawners.
    /// </summary>
    trial_omen,
    /// <summary>
    ///     Gives players a discount on villager trades, and causes some villagers to drop free items.
    /// </summary>
    village_hero,
    /// <summary>
    ///     Allows the entity to breathe underwater.
    /// </summary>
    water_breathing,
    /// <summary>
    ///     Makes the entity deal less damage with melee attacks.
    /// </summary>
    weakness,
    /// <summary>
    ///     Entity can move faster in cobweb blocks, and will place cobweb blocks upon death.
    /// </summary>
    weaving,
    /// <summary>
    ///     Entity will emit a wind burst upon death, similar to what a wind charge does.
    /// </summary>
    wind_charged,
    /// <summary>
    ///     Deals wither damage to the entity over time, and can kill it.
    /// </summary>
    wither
}

public enum Enchantment
{
    /// <summary>
    ///     Intended for tools; Removes the mining speed penalty underwater.
    /// </summary>
    aqua_affinity,
    /// <summary>
    ///     Intended for weapons; Deals more damage to arthropod mobs and applies slowness to them.
    /// </summary>
    bane_of_arthropods,
    /// <summary>
    ///     Curse; Intended for any armor; Prevents the item from being removed from its armor slot once equipped.
    /// </summary>
    binding,
    /// <summary>
    ///     Intended for any armor; Reduces the damage and knockback received from explosions.
    /// </summary>
    blast_protection,
    /// <summary>
    ///     Intended for bows; Makes a bow not consume standard arrows.
    /// </summary>
    infinity,
    /// <summary>
    ///     Intended for weapons; Makes a weapon more effective against armor.
    /// </summary>
    breach,
    /// <summary>
    ///     Intended for tridents; makes it summon lighting if thrown during a thunderstorm.
    /// </summary>
    channeling,
    /// <summary>
    ///     Intended for maces; Increases the amount of damage the smash attack does.
    /// </summary>
    density,
    /// <summary>
    ///     Intended for boots; Allows the wearer to move faster underwater.
    /// </summary>
    depth_strider,
    /// <summary>
    ///     Intended for tools; Makes the tool dig/mine faster.
    /// </summary>
    efficiency,
    /// <summary>
    ///     Intended for boots; Lowers the amount of fall damage taken.
    /// </summary>
    feather_falling,
    /// <summary>
    ///     Intended for weapons; Sets the target on fire, additionally cooking any drops.
    /// </summary>
    fire_aspect,
    /// <summary>
    ///     Intended for armor; Reduces the damage received from fire and lava.
    /// </summary>
    fire_protection,
    /// <summary>
    ///     Intended for bows; Makes fired arrows set their targets on fire.
    /// </summary>
    flame,
    /// <summary>
    ///     Intended for pickaxes; Increases the resource yield from mining.
    /// </summary>
    fortune,
    /// <summary>
    ///     Intended for boots; Makes water freeze under the wearer's feet as they walk.
    /// </summary>
    frost_walker,
    /// <summary>
    ///     Intended for tridents; Increases damage while in water/rain.
    /// </summary>
    impaling,
    /// <summary>
    ///     Intended for weapons; Increases the knockback of the weapon.
    /// </summary>
    knockback,
    /// <summary>
    ///     Intended for weapons; Increases the drop yield when killing a mob.
    /// </summary>
    looting,
    /// <summary>
    ///     Intended for tridents; Will return to the thrower automatically after landing.
    /// </summary>
    loyalty,
    /// <summary>
    ///     Intended for fishing rods; Increases the amount of treasure fished up.
    /// </summary>
    luck_of_the_sea,
    /// <summary>
    ///     Intended for fishing rods; Decreases the amount of time it takes to get a bite when fishing.
    /// </summary>
    lure,
    /// <summary>
    ///     Repairs the item when the wearer/equipper receives experience.
    /// </summary>
    mending,
    /// <summary>
    ///     Intended for crossbows; Fires three shots instead of one.
    /// </summary>
    multishot,
    /// <summary>
    ///     Intended for crossbows; Allows arrows to hit multiple mobs and be picked up after landing.
    /// </summary>
    piercing,
    /// <summary>
    ///     Intended for bows; Increases damage of fired arrows.
    /// </summary>
    power,
    /// <summary>
    ///     Intended for armor; Reduces the damage received from projectiles.
    /// </summary>
    projectile_protection,
    /// <summary>
    ///     Intended for armor; Reduces damage received overall.
    /// </summary>
    protection,
    /// <summary>
    ///     Intended for bows; Increases the amount of knockback dealt by fired arrows.
    /// </summary>
    punch,
    /// <summary>
    ///     Intended for crossbows; Decreases the amount of time required to charge the crossbow.
    /// </summary>
    quick_charge,
    /// <summary>
    ///     Intended for helmets; Increases the amount of time the wearer can breathe underwater.
    /// </summary>
    respiration,
    /// <summary>
    ///     Intended for tridents; When in the water/rain, changes the throw mechanic to launch the thrower forward instead.
    /// </summary>
    riptide,
    /// <summary>
    ///     Intended for weapons; Increases damage overall.
    /// </summary>
    sharpness,
    /// <summary>
    ///     Intended for tools; Certain blocks drop themselves rather than their typical drops (e.g., coal ore will literally
    ///     drop coal ore).
    /// </summary>
    silk_touch,
    /// <summary>
    ///     Intended for weapons; Increases damage against undead mobs.
    /// </summary>
    smite,
    /// <summary>
    ///     Intended for boots; Increases movement speed while walking on soul sand/soil.
    /// </summary>
    soul_speed,
    /// <summary>
    ///     Intended for leggings; Increases movement speed while sneaking.
    /// </summary>
    swift_sneak,
    /// <summary>
    ///     Intended for armor; Attacks entities back that attack the wearer.
    /// </summary>
    thorns,
    /// <summary>
    ///     Increases the item's durability.
    /// </summary>
    unbreaking,
    /// <summary>
    ///     Curse; Makes the item completely disappear if a player dies with it in their inventory.
    /// </summary>
    vanishing,
    /// <summary>
    ///     Intended for maces; Launches the player upwards after using the smash attack.
    /// </summary>
    wind_burst
}

[UsableInMCC]
public enum GameMode
{
    /// <summary>
    ///     Survival mode. Players have health, hunger, can break blocks, etc...
    /// </summary>
    survival = 0,
    /// <summary>
    ///     Creative mode. Players have no health or hunger, access to unlimited resources, can break blocks, etc...
    /// </summary>
    creative = 1,
    /// <summary>
    ///     Adventure mode. Players can only break/place blocks allowed by the mapmakers.
    /// </summary>
    adventure = 2,
    /// <summary>
    ///     Spectator mode. Players can fly around freely but can’t interact with the world in any way.
    /// </summary>
    spectator = 6
}

/// <summary>
///     A cause of damage.
/// </summary>
[UsableInMCC]
public enum DamageCause
{
    /// <summary>
    ///     Generic damage cause with no effect.
    /// </summary>
    all,
    fatal,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit with a falling anvil.
    /// </summary>
    anvil,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit with an exploding block, like TNT.
    /// </summary>
    block_explosion,
    charging,
    contact,
    /// <summary>
    ///     Damage cause that's raised when the entity is out of breath and underwater.
    /// </summary>
    drowning,
    /// <summary>
    ///     Damage cause that's raised when an entity is attacked directly by another entity.
    /// </summary>
    entity_attack,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit with an exploding entity, like a creeper.
    /// </summary>
    entity_explosion,
    /// <summary>
    ///     Damage cause that's raised when an entity hits the ground too fast.
    /// </summary>
    fall,
    falling_block,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit by any fire except being on fire actively (see fire_tick).
    /// </summary>
    fire,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit by actively being on fire.
    /// </summary>
    fire_tick,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit by exploding fireworks.
    /// </summary>
    fireworks,
    /// <summary>
    ///     Damage cause that's raised when an entity hits a wall while flying with an elytra.
    /// </summary>
    fly_into_wall,
    /// <summary>
    ///     Damage cause that's raised by being in powdered snow too long.
    /// </summary>
    freezing,
    /// <summary>
    ///     Damage cause that's raised by swimming in lava.
    /// </summary>
    lava,
    /// <summary>
    ///     Damage cause that's raised by being hit by lightning.
    /// </summary>
    lightning,
    magic,
    /// <summary>
    ///     Damage cause that's raised when an entity stands on magma without boots or sneaking.
    /// </summary>
    magma,
    /// <summary>
    ///     No specific damage cause.
    /// </summary>
    none,
    @override,
    piston,
    /// <summary>
    ///     Damage cause that's raised when an entity is hit with a projectile.
    /// </summary>
    projectile,
    stalactite,
    stalagmite,
    /// <summary>
    ///     Damage cause that's raised when a player is starving.
    /// </summary>
    starve,
    /// <summary>
    ///     Damage cause that's raised when an entity is stuck in a wall (or settled falling blocks) and can't breath.
    /// </summary>
    suffocation,
    suicide,
    temperature,
    /// <summary>
    ///     Damage cause that's raised by the "thorns" enchantment.
    /// </summary>
    thorns,
    @void,
    wither
}

[UsableInMCC]
public enum GameRule
{
    /// <summary>
    ///     Whether command blocks are enabled/runnable in-game. Default true
    /// </summary>
    commandBlocksEnabled,
    /// <summary>
    ///     Whether command blocks should output their results to chat. This is generally preferred off. Default true
    /// </summary>
    commandBlockOutput,
    /// <summary>
    ///     Whether the daylight cycle is enabled. Default true
    /// </summary>
    doDaylightCycle,
    /// <summary>
    ///     Whether entities (not mobs) should drop items when killed/destroyed. Default true
    /// </summary>
    doEntityDrops,
    /// <summary>
    ///     Whether fire should tick and spread. Default true
    /// </summary>
    doFireTick,
    /// <summary>
    ///     Whether players should immediately respawn on death. Default false
    /// </summary>
    doImmediateRespawn,
    /// <summary>
    ///     Whether phantoms spawn around players who haven't slept in a while. Default true
    /// </summary>
    doInsomnia,
    /// <summary>
    ///     Whether players should be barred from crafting items they haven't unlocked the recipes to yet. Default false
    /// </summary>
    doLimitedCrafting,
    /// <summary>
    ///     Whether mobs should drop loot/experience when killed. Default true
    /// </summary>
    doMobLoot,
    /// <summary>
    ///     Whether mobs should naturally spawn. Default true
    /// </summary>
    doMobSpawning,
    /// <summary>
    ///     Whether blocks should drop items when broken. Default true
    /// </summary>
    doTileDrops,
    /// <summary>
    ///     Whether the weather should change. Default true
    /// </summary>
    doWeatherCycle,
    /// <summary>
    ///     Whether players should take damage from drowning. Default true
    /// </summary>
    drowningDamage,
    /// <summary>
    ///     Whether players should take fall damage. Default true
    /// </summary>
    fallDamage,
    /// <summary>
    ///     Whether players should take fire damage. Default true
    /// </summary>
    fireDamage,
    /// <summary>
    ///     Whether players should take damage from freezing (in powdered snow). Default true
    /// </summary>
    freezeDamage,
    /// <summary>
    ///     The number of commands which a single function is allowed to run at one time. Default 10,000
    /// </summary>
    functionCommandLimit,
    /// <summary>
    ///     Whether players shouldn't drop their inventories on death. Default false
    /// </summary>
    keepInventory,
    /// <summary>
    ///     Whether the player locator bar should be shown. Default true
    /// </summary>
    locatorBar,
    /// <summary>
    ///     The maximum length of a chain of command blocks in a single tick. Default 65536
    /// </summary>
    maxCommandChainLength,
    /// <summary>
    ///     Whether mobs can modify the world. Default true
    /// </summary>
    mobGriefing,
    /// <summary>
    ///     Whether player health naturally regenerates (without effects). Default true
    /// </summary>
    naturalRegeneration,
    /// <summary>
    ///     The percentage of players that need to sleep to progress to the next day. Default 100
    /// </summary>
    playersSleepingPercentage,
    /// <summary>
    ///     Whether projectiles can break breakable blocks (like decorated pots). Default true
    /// </summary>
    projectilesCanBreakBlocks,
    /// <summary>
    ///     Whether players can attack each other. Default true
    /// </summary>
    pvp,
    /// <summary>
    ///     The speed at which random ticks occur. Default 1
    /// </summary>
    randomTickSpeed,
    /// <summary>
    ///     Whether recipes can be unlocked by having players collect all the needed items. Default true
    /// </summary>
    recipesUnlock,
    /// <summary>
    ///     Whether beds/respawn anchors explode in the incorrect dimensions. Default true
    /// </summary>
    respawnBlocksExplode,
    /// <summary>
    ///     Whether players should get chat feedback when running commands. Default true
    /// </summary>
    sendCommandFeedback,
    /// <summary>
    ///     Whether the effect specific to border blocks shows to players. Default true
    /// </summary>
    showBorderEffect,
    /// <summary>
    ///     Whether the players' coordinates are shown to them on-screen. Default true
    /// </summary>
    showCoordinates,
    /// <summary>
    ///     Whether the players' number of days played are shown to them on-screen. Default false
    /// </summary>
    showDaysPlayed,
    /// <summary>
    ///     Whether messages should be sent in chat when a player or named entity dies.
    /// </summary>
    showDeathMessages,
    /// <summary>
    ///     Whether recipe unlock messages are displayed. Default true
    /// </summary>
    showRecipeMessages,
    /// <summary>
    ///     Whether the "Can place on", "Can destroy", and item lock tags are hidden. Default true
    /// </summary>
    showTags,
    /// <summary>
    ///     The number of blocks away from the world spawn-point players might spawn randomly in. Default 10
    /// </summary>
    spawnRadius,
    /// <summary>
    ///     Whether TNT actually explodes.
    /// </summary>
    tntExplodes,
    /// <summary>
    ///     Whether blocks are dropped by all blocks (false) or randomly (true) depending on how far away the block is from a
    ///     TNT explosion.
    /// </summary>
    tntExplosionDropDecay
}

/// <summary>
///     A pre-defined structure type.
/// </summary>
[UsableInMCC]
public enum StructureType
{
    bastionremnant,
    buriedtreasure,
    endcity,
    fortress,
    mansion,
    mineshaft,
    monument,
    ruins,
    pillageroutpost,
    ruinedportal,
    shipwreck,
    stronghold,
    temple,
    village
}

[UsableInMCC]
public enum MobEventType
{
    /// <summary>
    ///     Controls the event that spawns the ender dragon.
    /// </summary>
    ender_dragon_event,
    /// <summary>
    ///     Controls the event that spawns pillager patrols.
    /// </summary>
    pillager_patrols_event,
    /// <summary>
    ///     Controls the event that spawns wandering traders.
    /// </summary>
    wandering_trader_event
}

public enum MusicRepeatMode
{
    /// <summary>
    ///     The music will play once and then end.
    /// </summary>
    play_once,
    /// <summary>
    ///     The music will repeat until manually stopped.
    /// </summary>
    loop
}

/// <summary>
///     An item slot identifier.
/// </summary>
[UsableInMCC]
public enum ItemSlot
{
    slot_armor,
    slot_armor_head,
    slot_armor_chest,
    slot_armor_legs,
    slot_armor_feet,
    slot_weapon_mainhand,
    slot_weapon_offhand,
    slot_container,
    slot_chest,
    slot_enderchest,
    slot_saddle,
    slot_hotbar,
    slot_inventory
}

public enum RideFillType
{
    /// <summary>
    ///     The command will only go through if all riders will fit on the mount.
    /// </summary>
    if_group_fits,
    /// <summary>
    ///     The command will ride as many riders as possible until the mount is full.
    /// </summary>
    until_full
}

public enum RideSummonRules
{
    /// <summary>
    ///     Summons entities only for riders that aren't riding on another entity and not being ridden.
    /// </summary>
    no_ride_change,
    /// <summary>
    ///     Default value. Makes riders dismount if they're riding, then summons an entity for all of them.
    /// </summary>
    reassign_rides,
    /// <summary>
    ///     Summons entities only for riders that aren't already riding on another entity.
    /// </summary>
    skip_riders
}

public enum RideTeleportRules
{
    /// <summary>
    ///     Teleport the entity being ridden.
    /// </summary>
    teleport_ride,
    /// <summary>
    ///     Teleport the rider of the entity.
    /// </summary>
    teleport_rider
}

public enum ScoreboardOrdering
{
    /// <summary>
    ///     Scoreboard entries will be shown starting with the lowest first.
    /// </summary>
    ascending,
    /// <summary>
    ///     Scoreboard entries will be shown starting with the highest first.
    /// </summary>
    descending
}

public enum ScoreboardOp
{
    SET,
    ADD,
    SUB,
    MUL,
    DIV,
    MOD,
    SWAP,
    MIN,
    MAX
}

[UsableInMCC]
public enum StructureRotation
{
    /// <summary>
    ///     Do not rotate the structure.
    /// </summary>
    _0_degrees,
    /// <summary>
    ///     Rotate the structure 90 degrees around the Y axis.
    /// </summary>
    _90_degrees,
    /// <summary>
    ///     Rotate the structure 180 degrees around the Y axis.
    /// </summary>
    _180_degrees,
    /// <summary>
    ///     Rotate the structure 270 degrees around the Y axis.
    /// </summary>
    _270_degrees
}

public enum StructureMirror
{
    /// <summary>
    ///     Do not mirror the structure.
    /// </summary>
    none,
    /// <summary>
    ///     Flip the structure around the X axis.
    /// </summary>
    x,
    /// <summary>
    ///     Flip the structure around the Z axis.
    /// </summary>
    z,
    /// <summary>
    ///     Flip the structure around the X and Z axes.
    /// </summary>
    xz
}

public enum StructureAnimationMode
{
    /// <summary>
    ///     Animate the loading of the structure by building it block-by-block.
    /// </summary>
    block_by_block,
    /// <summary>
    ///     Animate the loading of the structure by building it layer-by-layer vertically.
    /// </summary>
    layer_by_layer
}

public enum TimeQuery
{
    /// <summary>
    ///     Query the number of ticks relative to the current day.
    /// </summary>
    daytime,
    /// <summary>
    ///     Query the number of ticks total.
    /// </summary>
    gametime,
    /// <summary>
    ///     Query the number of days played.
    /// </summary>
    day
}

[UsableInMCC]
public enum TimeSpec
{
    /// <summary>
    ///     Day (1000)
    /// </summary>
    day = 1000,
    /// <summary>
    ///     Night (13000)
    /// </summary>
    night = 13000,
    /// <summary>
    ///     Noon (6000)
    /// </summary>
    noon = 6000,
    /// <summary>
    ///     Midnight (18000)
    /// </summary>
    midnight = 18000,
    /// <summary>
    ///     Sunrise (23000)
    /// </summary>
    sunrise = 23000,
    /// <summary>
    ///     Sunset (12000)
    /// </summary>
    sunset = 12000
}

public enum WeatherState
{
    /// <summary>
    ///     Clear weather (default).
    /// </summary>
    clear,
    /// <summary>
    ///     Raining.
    /// </summary>
    rain,
    /// <summary>
    ///     Raining + random thunder strikes.
    /// </summary>
    thunder
}

[UsableInMCC]
public enum AnchorPosition
{
    /// <summary>
    ///     The command execution will be anchored to the entity's eyes, both in position and rotation.
    /// </summary>
    eyes,
    /// <summary>
    ///     The command execution will be anchored to the entity's feet.
    /// </summary>
    feet
}

[UsableInMCC]
public enum Dimension
{
    /// <summary>
    ///     The overworld dimension.
    /// </summary>
    overworld,
    /// <summary>
    ///     The nether dimension. 8x smaller than the overworld.
    /// </summary>
    nether,
    /// <summary>
    ///     The end dimension.
    /// </summary>
    the_end
}

[UsableInMCC]
public enum BlocksScanMode
{
    /// <summary>
    ///     Check for all blocks matching.
    /// </summary>
    all,
    /// <summary>
    ///     Check only non-air blocks.
    /// </summary>
    masked
}