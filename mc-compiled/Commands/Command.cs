using System.Collections.Generic;
using mc_compiled.Commands.Execute;
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

    public static void ResetState()
    {
        UTIL.tags.Clear();
    }

    public static string String(this ItemSlot slot)
    {
        return slot.ToString().Replace('_', '.');
    }
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
    public static string String(this StructureRotation rot)
    {
        return rot.ToString()[1..];
    }

    public static string AlwaysDay(bool enabled)
    {
        return $"alwaysday {enabled.ToString().ToLower()}";
    }

    public static string CameraShake(string target)
    {
        return $"camerashake add {target}";
    }
    public static string CameraShake(string target, float intensity)
    {
        return $"camerashake add {target} {intensity}";
    }
    public static string CameraShake(string target, float intensity, float seconds)
    {
        return $"camerashake add {target} {intensity} {seconds}";
    }
    public static string CameraShake(string target, float intensity, float seconds, CameraShakeType shakeType)
    {
        return $"camerashake add {target} {intensity} {seconds} {shakeType}";
    }

    public static string Clear()
    {
        return "clear";
    }
    public static string Clear(string target)
    {
        return $"clear {target}";
    }
    public static string Clear(string target, string item)
    {
        return $"clear {target} {item}";
    }
    public static string Clear(string target, string item, int data)
    {
        return $"clear {target} {item} {data}";
    }
    public static string Clear(string target, string item, int data, int maxCount)
    {
        return $"clear {target} {item} {data} {maxCount}";
    }

    public static string ClearSpawnPoint(string target)
    {
        return $"clearspawnpoint {target}";
    }

    public static string Clone(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        Coordinate dstX, Coordinate dstY, Coordinate dstZ)
    {
        return $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ}";
    }
    public static string Clone(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        Coordinate dstX, Coordinate dstY, Coordinate dstZ, bool copyAir, CloneMode mode)
    {
        return $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} " + (copyAir ? "replace" : "masked") +
               $" {mode}";
    }

    public static string ConnectWSServer(string serverUri)
    {
        return $"wsserver {serverUri}";
    }

    public static string Damage(string target, int amount)
    {
        return $"damage {target} {amount}";
    }
    public static string Damage(string target, int amount, DamageCause cause)
    {
        return $"damage {target} {amount} {cause}";
    }
    public static string Damage(string target, int amount, DamageCause cause, string damager)
    {
        return $"damage {target} {amount} {cause} entity {damager}";
    }

    public static string Deop(string target)
    {
        return $"deop {target}";
    }

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

    public static string Difficulty(DifficultyMode difficulty)
    {
        return $"difficulty {difficulty}";
    }

    public static string EffectClear(string target)
    {
        return $"effect {target} clear";
    }
    public static string Effect(string target, PotionEffect effect)
    {
        return $"effect {target} {effect}";
    }
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

    public static string Enchant(string target, Enchantment enchantment)
    {
        return $"effect {target} {enchantment}";
    }
    public static string Enchant(string target, Enchantment enchantment, int level)
    {
        return $"effect {target} {enchantment} {level}";
    }

    public static string Event(string target, string eventName)
    {
        return $"event entity {target} {eventName}";
    }

    public static ExecuteBuilder Execute()
    {
        return new ExecuteBuilder();
    }

    public static string Fill(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        string block)
    {
        return $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()}";
    }
    public static string Fill(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        string block, int data)
    {
        return $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} []";
    }
    public static string Fill(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        string block, int data, OldHandling fillMode)
    {
        return $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} [] {fillMode}";
    }
    public static string Fill(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        string block, int data, string replaceBlock)
    {
        return
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} [] replace {replaceBlock.AsCommandParameterString()} -1";
    }
    public static string Fill(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2, Coordinate z2,
        string block, int data, string replaceBlock, int replaceData)
    {
        return
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block.AsCommandParameterString()} [] replace {replaceBlock.AsCommandParameterString()} {replaceData}";
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

    public static string Function(string name)
    {
        return $"function {name}";
    }
    public static string Function(CommandFile function)
    {
        return $"function {function.CommandReference}";
    }

    public static string Gamemode(string target, GameMode mode)
    {
        return $"gamemode {mode} {target}";
    }
    public static string Gamemode(string target, int mode)
    {
        return $"gamemode {mode} {target}";
    }

    public static string Gamerule(GameRule rule, string value)
    {
        return $"gamerule {rule} {value}";
    }
    public static string Gamerule(string rule, string value)
    {
        return $"gamerule {rule} {value}";
    }

    public static string Give(string target, string item)
    {
        return $"give {target} {item}";
    }
    public static string Give(string target, string item, int amount)
    {
        return $"give {target} {item} {amount}";
    }
    public static string Give(string target, string item, int amount, int data)
    {
        return $"give {target} {item} {amount} {data}";
    }
    public static string Give(string target, string item, int amount, int data, string json)
    {
        return $"give {target} {item} {amount} {data} {json}";
    }

    public static string Help()
    {
        return "help";
    }

    public static string ImmutableWorld(bool immutable)
    {
        return $"immutableworld {immutable.ToString().ToLower()}";
    }

    public static string Kick(string target)
    {
        return $"kick {target}";
    }
    public static string Kick(string target, string reason)
    {
        return $"kick {target} {reason}";
    }

    public static string Kill()
    {
        return "kill";
    }
    public static string Kill(string target)
    {
        return $"kill {target}";
    }

    public static string List()
    {
        return "list";
    }

    public static string Locate(StructureType type)
    {
        return $"locate {type}";
    }
    public static string Locate(string structureType)
    {
        return $"locate {structureType}";
    }

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

    public static string Me(string text)
    {
        return $"me {text}";
    }

    public static string MobEvent(MobEventType @event, bool value)
    {
        return $"mobevent minecraft:{@event} {value.ToString().ToLower()}";
    }
    public static string MobEvent(string @event, bool value)
    {
        return $"mobevent {@event} {value.ToString().ToLower()}";
    }
    public static string MobEvent(MobEventType @event)
    {
        return $"mobevent minecraft:{@event}";
    }
    public static string MobEvent(string @event)
    {
        return $"mobevent {@event}";
    }
    public static string MobEventsDisable()
    {
        return "mobevent events_enabled false";
    }
    public static string MobEventsEnable()
    {
        return "mobevent events_enabled true";
    }

    public static string Message(string target, string message)
    {
        return $"w {target} {message}";
    }

    public static string MusicPlay(string track, float volume = 1f, float fadeSeconds = 0f,
        MusicRepeatMode repeatMode = MusicRepeatMode.play_once)
    {
        return $"music play {track} {volume} {fadeSeconds} {repeatMode}";
    }
    public static string MusicQueue(string track, float volume = 1f, float fadeSeconds = 0f,
        MusicRepeatMode repeatMode = MusicRepeatMode.play_once)
    {
        return $"music queue {track} {volume} {fadeSeconds} {repeatMode}";
    }
    public static string MusicStop(float fadeSeconds)
    {
        return $"music stop {fadeSeconds}";
    }
    public static string MusicVolume(float volume)
    {
        return $"music volume {volume}";
    }

    public static string Op(string target)
    {
        return $"op {target}";
    }

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
    public static string PlayAnimation(string target, string animation, string nextState, float blendOutTime,
        string molangStopCondition)
    {
        return $"playanimation {target} {animation} {nextState} {blendOutTime} {molangStopCondition}";
    }
    public static string PlayAnimation(string target, string animation, string nextState, float blendOutTime,
        string molangStopCondition, string controller)
    {
        return $"playanimation {target} {animation} {nextState} {blendOutTime} {molangStopCondition} {controller}";
    }

    public static string PlaySound(string sound)
    {
        return $"playsound {sound}";
    }
    public static string PlaySound(string sound, string target)
    {
        return $"playsound {sound} {target}";
    }
    public static string PlaySound(string sound, string target, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"playsound {sound} {target} {x} {y} {z}";
    }
    public static string PlaySound(string sound, string target, Coordinate x, Coordinate y, Coordinate z, float volume)
    {
        return $"playsound {sound} {target} {x} {y} {z} {volume}";
    }
    public static string PlaySound(string sound, string target, Coordinate x, Coordinate y, Coordinate z, float volume,
        float pitch)
    {
        return $"playsound {sound} {target} {x} {y} {z} {volume} {pitch}";
    }
    public static string PlaySound(string sound, string target, Coordinate x, Coordinate y, Coordinate z, float volume,
        float pitch, float minVolume)
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

    public static string Reload()
    {
        return "reload";
    }

    public static string ReplaceItemBlock(Coordinate x, Coordinate y, Coordinate z, int slot, OldHandling handling,
        string item)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item}";
    }
    public static string ReplaceItemBlock(Coordinate x, Coordinate y, Coordinate z, int slot, OldHandling handling,
        string item, int amount)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount}";
    }
    public static string ReplaceItemBlock(Coordinate x, Coordinate y, Coordinate z, int slot, OldHandling handling,
        string item, int amount, int data)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount} {data}";
    }
    public static string ReplaceItemBlock(Coordinate x, Coordinate y, Coordinate z, int slot, OldHandling handling,
        string item, int amount, int data, string json)
    {
        return $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount} {data} {json}";
    }

    public static string ReplaceItemEntity(string target, ItemSlot slotType, int slot, OldHandling handling,
        string item)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item}";
    }
    public static string ReplaceItemEntity(string target, ItemSlot slotType, int slot, OldHandling handling,
        string item, int amount)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount}";
    }
    public static string ReplaceItemEntity(string target, ItemSlot slotType, int slot, OldHandling handling,
        string item, int amount, int data)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount} {data}";
    }
    public static string ReplaceItemEntity(string target, ItemSlot slotType, int slot, OldHandling handling,
        string item, int amount, int data, string json)
    {
        return $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount} {data} {json}";
    }

    public static string Ride(string sources, string targets, TeleportRules tpRules = TeleportRules.teleport_rider,
        RideFillType fillType = RideFillType.until_full)
    {
        return $"ride {sources} start_riding {targets} {tpRules} {fillType}";
    }
    public static string RideDismount(string riders)
    {
        return $"ride {riders} stop_riding";
    }
    public static string RideEvictRiders(string rides)
    {
        return $"ride {rides} evict_riders";
    }
    public static string RideSummonRider(string rides, string entity)
    {
        return $"ride {rides} summon_rider {entity}";
    }
    public static string RideSummonRider(string rides, string entity, string nameTag)
    {
        return $"ride {rides} summon_rider {entity} none {nameTag.AsCommandParameterString()}";
    }
    public static string RideSummonRide(string riders, string entity)
    {
        return $"ride {riders} summon_ride {entity}";
    }
    public static string RideSummonRide(string riders, string entity, RideRules rules)
    {
        return $"ride {riders} summon_ride {entity} {rules}";
    }
    public static string RideSummonRide(string riders, string entity, RideRules rules, string nameTag)
    {
        return $"ride {riders} summon_ride {entity} {rules} none {nameTag.AsCommandParameterString()}";
    }

    public static string SaveHold()
    {
        return "save hold";
    }
    public static string SaveQuery()
    {
        return "save query";
    }
    public static string SaveResume()
    {
        return "save resume";
    }

    public static string Say(string message)
    {
        return $"say {message}";
    }

    public static string ScheduleAreaLoaded(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2,
        Coordinate z2, string function)
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

    public static string ScoreboardCreateObjective(string name)
    {
        return $"scoreboard objectives add {name} dummy";
    }
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
    public static string ScoreboardList(string target)
    {
        return $"scoreboard players list {target}";
    }

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
    public static string SetBlock(Coordinate x, Coordinate y, Coordinate z, string block, int data)
    {
        return $"setblock {x} {y} {z} {block} []";
    }
    public static string SetBlock(Coordinate x, Coordinate y, Coordinate z, string block, int data,
        OldHandling handling)
    {
        return $"setblock {x} {y} {z} {block} [] {handling}";
    }

    public static string SetMaxPlayers(int max)
    {
        return $"setmaxplayers {max}";
    }

    public static string SetWorldSpawn(Coordinate x, Coordinate y, Coordinate z)
    {
        return $"setworldspawn {x} {y} {z}";
    }

    public static string Spawnpoint()
    {
        return "spawnpoint";
    }
    public static string Spawnpoint(string target)
    {
        return $"spawnpoint {target}";
    }
    public static string Spawnpoint(string target, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"spawnpoint {target} {x} {y} {z}";
    }

    public static string SpreadPlayers(Coordinate x, Coordinate z, float spreadDistance, float maxRange, string targets)
    {
        return $"spreadplayers {x} {z} {spreadDistance} {maxRange} {targets.AsCommandParameterString()}";
    }

    public static string Stop()
    {
        return "stop";
    }

    public static string StopSound(string target)
    {
        return $"stopsound {target}";
    }
    public static string StopSound(string target, string sound)
    {
        return $"stopsound {target} {sound}";
    }

    public static string StructureSaveDisk(string name, Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2,
        Coordinate y2, Coordinate z2)
    {
        return $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} disk";
    }
    public static string StructureSaveMemory(string name, Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2,
        Coordinate y2, Coordinate z2)
    {
        return $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} memory";
    }
    public static string StructureSaveDisk(string name, Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2,
        Coordinate y2, Coordinate z2, bool includeEntities, bool includeBlocks = true)
    {
        return
            $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} {includeEntities} disk {includeBlocks.ToString().ToLower()}";
    }
    public static string StructureSaveMemory(string name, Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2,
        Coordinate y2, Coordinate z2, bool includeEntities, bool includeBlocks = true)
    {
        return
            $"structure save {name.AsCommandParameterString()} {x1} {y1} {z1} {x2} {y2} {z2} {includeEntities} memory {includeBlocks.ToString().ToLower()}";
    }
    public static string StructureLoad(string name, Coordinate x, Coordinate y, Coordinate z,
        StructureRotation rotation = StructureRotation._0_degrees,
        StructureMirror flip = StructureMirror.none, bool includeEntities = true, bool includeBlocks = true,
        bool waterLogged = false, float integrity = 100, string seed = null)
    {
        if (seed == null)
            return
                $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity}";

        return
            $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity} {seed}";
    }
    public static string StructureLoad(string name, Coordinate x, Coordinate y, Coordinate z,
        StructureRotation rotation = StructureRotation._0_degrees,
        StructureMirror flip = StructureMirror.none,
        StructureAnimationMode animation = StructureAnimationMode.layer_by_layer,
        float animationSeconds = 0, bool includeEntities = true, bool includeBlocks = true, bool waterLogged = false,
        float integrity = 100, string seed = null)
    {
        if (seed == null)
            return
                $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {animation} {animationSeconds} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity}";

        return
            $"structure load {name.AsCommandParameterString()} {x} {y} {z} {rotation.String()} {flip} {animation} {animationSeconds} {includeEntities.ToString().ToLower()} {includeBlocks.ToString().ToLower()} {waterLogged.ToString().ToLower()} {integrity} {seed}";
    }
    public static string StructureDelete(string name)
    {
        return $"structure delete {name.AsCommandParameterString()}";
    }

    public static string Summon(string entity)
    {
        return $"summon {entity}";
    }
    public static string Summon(string entity, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"summon {entity} {x} {y} {z}";
    }
    public static string Summon(string entity, string nameTag, Coordinate x, Coordinate y, Coordinate z)
    {
        return $"summon {entity} {nameTag.AsCommandParameterString()} {x} {y} {z}";
    }
    public static string Summon(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate yRot,
        Coordinate xRot)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot}";
    }
    public static string Summon(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate yRot,
        Coordinate xRot, string nameTag)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot} \"\" {nameTag.AsCommandParameterString()}";
    }
    public static string Summon(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate yRot,
        Coordinate xRot, string nameTag, string spawnEvent)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot} {spawnEvent} {nameTag.AsCommandParameterString()}";
    }
    public static string SummonWithEvent(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate yRot,
        Coordinate xRot, string spawnEvent)
    {
        return $"summon {entity} {x} {y} {z} {yRot} {xRot} {spawnEvent}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, string facingSelector)
    {
        return $"summon {entity} {x} {y} {z} facing {facingSelector}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate faceX,
        Coordinate faceY, Coordinate faceZ)
    {
        return $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, string facingSelector,
        string nameTag)
    {
        return $"summon {entity} {x} {y} {z} facing {facingSelector} \"\" {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate faceX,
        Coordinate faceY, Coordinate faceZ, string nameTag)
    {
        return $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ} \"\" {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, string facingSelector,
        string nameTag, string spawnEvent)
    {
        return
            $"summon {entity} {x} {y} {z} facing {facingSelector} {spawnEvent.AsCommandParameterString()} {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacing(string entity, Coordinate x, Coordinate y, Coordinate z, Coordinate faceX,
        Coordinate faceY, Coordinate faceZ, string nameTag, string spawnEvent)
    {
        return
            $"summon {entity} {x} {y} {z} facing {faceX} {faceY} {faceZ} {spawnEvent.AsCommandParameterString()} {nameTag.AsCommandParameterString()}";
    }
    public static string SummonFacingWithEvent(string entity, Coordinate x, Coordinate y, Coordinate z,
        string facingSelector, string eventName)
    {
        return $"summon {entity} {x} {y} {z} facing {facingSelector} {eventName.AsCommandParameterString()}";
    }
    public static string SummonFacingWithEvent(string entity, Coordinate x, Coordinate y, Coordinate z,
        Coordinate faceX, Coordinate faceY, Coordinate faceZ, string eventName)
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
    public static string TagList(string targets)
    {
        return $"tag {targets.AsCommandParameterString()} list";
    }

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
    public static string Teleport(Coordinate x, Coordinate y, Coordinate z, Coordinate yRot, Coordinate xRot,
        bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} {yRot} {xRot} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(string target, Coordinate x, Coordinate y, Coordinate z, bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} {checkForBlocks.ToString().ToLower()}";
    }
    public static string Teleport(string target, Coordinate x, Coordinate y, Coordinate z, Coordinate yRot,
        Coordinate xRot, bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} {yRot} {xRot} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(Coordinate x, Coordinate y, Coordinate z, Coordinate facingX,
        Coordinate facingY, Coordinate facingZ, bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} facing {facingX} {facingY} {facingZ} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(Coordinate x, Coordinate y, Coordinate z, string facingEntity,
        bool checkForBlocks = false)
    {
        return $"tp {x} {y} {z} facing {facingEntity} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(string target, Coordinate x, Coordinate y, Coordinate z, Coordinate facingX,
        Coordinate facingY, Coordinate facingZ, bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} facing {facingX} {facingY} {facingZ} {checkForBlocks.ToString().ToLower()}";
    }
    public static string TeleportFacing(string target, Coordinate x, Coordinate y, Coordinate z, string facingEntity,
        bool checkForBlocks = false)
    {
        return $"tp {target} {x} {y} {z} facing {facingEntity} {checkForBlocks.ToString().ToLower()}";
    }

    public static string Tellraw(string jsonMessage)
    {
        return $"tellraw @a {jsonMessage}";
    }
    public static string Tellraw(string targets, string jsonMessage)
    {
        return $"tellraw {targets} {jsonMessage}";
    }
    public static string Tellraw(JObject jsonMessage)
    {
        return $"tellraw @a {jsonMessage.ToString(Formatting.None)}";
    }
    public static string Tellraw(string targets, JObject jsonMessage)
    {
        return $"tellraw {targets} {jsonMessage.ToString(Formatting.None)}";
    }

    public static string TestFor(string targets)
    {
        return $"testfor {targets}";
    }

    public static string TestForBlock(Coordinate x, Coordinate y, Coordinate z, string block)
    {
        return $"testforblock {x} {y} {z} {block}";
    }
    public static string TestForBlock(Coordinate x, Coordinate y, Coordinate z, string block, int data)
    {
        return $"testforblock {x} {y} {z} {block} []";
    }

    public static string TestForBlocks(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2,
        Coordinate z2, Coordinate dstX, Coordinate dstY, Coordinate dstZ)
    {
        return $"testforblocks {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ}";
    }
    public static string TestForBlocksMasked(Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2, Coordinate y2,
        Coordinate z2, Coordinate dstX, Coordinate dstY, Coordinate dstZ)
    {
        return $"testforblocks {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} masked";
    }

    public static string TickingAreaAdd(string name, Coordinate x1, Coordinate y1, Coordinate z1, Coordinate x2,
        Coordinate y2, Coordinate z2)
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
    public static string TickingAreaRemoveAll()
    {
        return "tickingarea remove_all";
    }
    public static string TickingAreaList()
    {
        return "tickingarea list";
    }
    public static string TickingAreaListAll()
    {
        return "tickingarea list all-dimensions";
    }

    public static string TimeAdd(int ticks)
    {
        return $"time add {ticks}";
    }
    public static string TimeSet(int ticks)
    {
        return $"time set {ticks}";
    }
    public static string TimeSet(TimeSpec spec)
    {
        return $"time set {spec}";
    }
    public static string TimeGet(TimeQuery query)
    {
        return $"time query {query}";
    }

    public static string TitleClear(string target)
    {
        return $"titleraw {target} clear";
    }
    public static string TitleReset(string target)
    {
        return $"titleraw {target} reset";
    }
    public static string TitleSubtitle(string target, string json)
    {
        return $"titleraw {target} subtitle {json}";
    }
    public static string TitleActionBar(string target, string json)
    {
        return $"titleraw {target} actionbar {json}";
    }
    public static string Title(string target, string json)
    {
        return $"titleraw {target} title {json}";
    }
    public static string TitleTimes(string target, int fadeIn, int stay, int fadeOut)
    {
        return $"titleraw {target} times {fadeIn} {stay} {fadeOut}";
    }

    public static string ToggleDownfall()
    {
        return "toggledownfall";
    }

    public static string Weather(WeatherState state)
    {
        return $"weather {state}";
    }
    public static string Weather(WeatherState state, int durationTicks)
    {
        return $"weather {state} {durationTicks}";
    }
    public static string WeatherQuery()
    {
        return "weather query";
    }

    public static string Whitelist(string player)
    {
        return $"whitelist add {player.AsCommandParameterString()}";
    }
    public static string WhitelistRemove(string player)
    {
        return $"whitelist remove {player.AsCommandParameterString()}";
    }
    public static string WhitelistList()
    {
        return "whitelist list";
    }
    public static string WhitelistOff()
    {
        return "whitelist off";
    }
    public static string WhitelistOn()
    {
        return "whitelist on";
    }
    public static string WhitelistReload()
    {
        return "whitelist reload";
    }

    public static string Xp(int amount)
    {
        return $"xp {amount}";
    }
    public static string XpL(int amount)
    {
        return $"xp {amount}L";
    }
    public static string Xp(int amount, string target)
    {
        return $"xp {amount} {target}";
    }
    public static string XpL(int amount, string target)
    {
        return $"xp {amount}L {target}";
    }

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
    public static implicit operator string(IndexedTag tag)
    {
        return tag.Tag;
    }
}

public enum CameraShakeType
{
    positional,
    rotational
}

public enum CloneMode
{
    force,
    move,
    normal
}

[EnumParsable(typeof(DifficultyMode))]
public enum DifficultyMode
{
    peaceful = 0,
    easy = 1,
    normal = 2,
    hard = 3
}

[EnumParsable(typeof(OldHandling))]
public enum OldHandling
{
    destroy,
    keep,
    hollow,
    outline,
    replace
}

[EnumParsable(typeof(PotionEffect))]
public enum PotionEffect
{
    speed = 1,
    slowness = 2,
    haste = 3,
    mining_fatigue = 4,
    strength = 5,
    instant_health = 6,
    instant_damage = 7,
    jump_boost = 8,
    nausea = 9,
    regeneration = 10,
    resistance = 11,
    fire_resistance = 12,
    water_breathing = 13,
    invisibility = 14,
    blindness = 15,
    night_vision = 16,
    hunger = 17,
    weakness = 18,
    poison = 19,
    wither = 20,
    health_boost = 21,
    absorption = 22,
    saturation = 23,
    levitation = 24,
    fatal_poison = 25,
    conduit_power = 26,
    slow_falling = 27,
    bad_omen = 28,
    hero_of_the_village = 29,
    darkness = -1
}

[EnumParsable(typeof(Enchantment))]
public enum Enchantment
{
    protection,
    fire_protection,
    feather_falling,
    blast_protection,
    projectile_protection,
    respiration,
    aqua_affinity,
    thorns,
    depth_strider,
    frost_walker,
    binding_curse,
    soul_speed,
    sharpness,
    smite,
    bane_of_arthropods,
    knockback,
    fire_aspect,
    looting,
    sweeping,
    power,
    punch,
    flame,
    infinity,
    efficiency,
    silk_touch,
    unbreaking,
    fortune,
    luck_of_the_sea,
    lure,
    channeling,
    impaling,
    loyalty,
    riptide,
    multishot,
    piercing,
    quick_charge,
    mending,
    swift_sneak
}

[EnumParsable(typeof(GameMode))]
public enum GameMode
{
    survival = 0,
    creative = 1,
    adventure = 2,
    spectator = 6
}

[EnumParsable(typeof(DamageCause))]
public enum DamageCause
{
    all,
    fatal,
    anvil,
    block_explosion,
    charging,
    contact,
    drowning,
    entity_attack,
    entity_explosion,
    fall,
    falling_block,
    fire,
    fire_tick,
    fireworks,
    fly_into_wall,
    freezing,
    lava,
    lightning,
    magic,
    magma,
    none,
    @override,
    piston,
    projectile,
    stalactite,
    stalagmite,
    starve,
    suffocation,
    suicide,
    temperature,
    thorns,
    @void,
    wither
}

[EnumParsable(typeof(GameRule))]
public enum GameRule
{
    commandBlocksEnabled,
    commandBlockOutput,
    doDaylightCycle,
    doEntityDrops,
    doFireTick,
    doInsomnia,
    doImmediateRespawn,
    doMobLoot,
    doMobSpawning,
    doTileDrops,
    doWeatherCycle,
    drowningDamage,
    fallDamage,
    fireDamage,
    freezeDamage,
    functionCommandLimit,
    keepInventory,
    maxCommandChainLength,
    mobGriefing,
    naturalRegeneration,
    pvp,
    randomTickSpeed,
    respawnBlocksExplode,
    sendCommandFeedback,
    showCoordinates,
    showDeathMessages,
    spawnRadius,
    tntExplodes,
    showTags
}

[EnumParsable(typeof(StructureType))]
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

[EnumParsable(typeof(MobEventType))]
public enum MobEventType
{
    pillager_patrols_event,
    wandering_trader_event
}

public enum MusicRepeatMode
{
    play_once,
    loop
}

[EnumParsable(typeof(ItemSlot))]
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
    if_group_fits,
    until_full
}

public enum RideRules
{
    no_ride_change,
    reassign_rides,
    skip_riders
}

public enum TeleportRules
{
    teleport_ride,
    teleport_rider
}

public enum ScoreboardOrdering
{
    ascending,
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

public enum StructureRotation
{
    _0_degrees,
    _90_degrees,
    _180_degrees,
    _270_degrees
}

public enum StructureMirror
{
    none,
    x,
    z,
    xz
}

public enum StructureAnimationMode
{
    block_by_block,
    layer_by_layer
}

public enum TimeQuery
{
    daytime,
    gametime,
    day
}

[EnumParsable(typeof(TimeSpec))]
public enum TimeSpec
{
    day = 1000,
    night = 13000,
    noon = 6000,
    midnight = 18000,
    sunrise = 23000,
    sunset = 12000
}

public enum WeatherState
{
    clear,
    rain,
    thunder
}

[EnumParsable(typeof(AnchorPosition))]
public enum AnchorPosition
{
    eyes,
    feet
}

[EnumParsable(typeof(Dimension))]
public enum Dimension
{
    nether,
    overworld,
    the_end
}

[EnumParsable(typeof(BlocksScanMode))]
public enum BlocksScanMode
{
    all,
    masked
}