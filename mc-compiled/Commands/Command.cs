using mc_compiled.Commands.Native;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    /// <summary>
    /// Utility for constructing minecraft commands.
    /// </summary>
    public static class Command
    {
        /// <summary>
        /// Command utils that require instance stuff.
        /// </summary>
        public class Util
        {
            internal readonly Dictionary<int, int> tags = new Dictionary<int, int>();

            /// <summary>
            /// Get a unique tag incase used somewhere else in-scope.
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public IndexedTag PushTag(string tag)
            {
                int hash = tag.GetHashCode();
                if(tags.TryGetValue(hash, out int index))
                {
                    IndexedTag ret = new IndexedTag()
                    {
                        name = tag,
                        index = index
                    };

                    index++;
                    tags[hash] = index;
                    return ret;
                } else
                {
                    tags[hash] = 1;
                    return new IndexedTag()
                    {
                        name = tag,
                        index = 0
                    };
                }
            }
            /// <summary>
            /// Pop a tag.
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public void PopTag(IndexedTag tag)
            {
                int hash = tag.name.GetHashCode();
                int index = tags[hash];
                tags[hash] = index - 1;
            }

            /// <summary>
            /// Make an entity invisible via a potion effect.
            /// </summary>
            /// <param name="entity"></param>
            /// <returns></returns>
            public string MakeInvisible(string entity) =>
                Command.Effect(entity, PotionEffect.invisibility, 99999999, 0, true);

            /// <summary>
            /// Prepends "minecraft:" if no namespace is given.
            /// </summary>
            /// <param name="identifier"></param>
            /// <returns></returns>
            public string RequireNamespace(string identifier)
            {
                if (identifier.Contains(':'))
                    return identifier;
                else
                    return "minecraft:" + identifier;
            }
        }
        
        public static void ResetState()
        {
            UTIL.tags.Clear();
        }
        public static readonly Util UTIL = new Util();

        public static string String(this EquipmentSlotType slot) => slot.ToString().Replace('_', '.');
        public static string String(this ScoreboardOp op)
        {
            switch (op)
            {
                case ScoreboardOp.SET:
                    return "=";
                case ScoreboardOp.ADD:
                    return "+=";
                case ScoreboardOp.SUB:
                    return "-=";
                case ScoreboardOp.MUL:
                    return "*=";
                case ScoreboardOp.DIV:
                    return "/=";
                case ScoreboardOp.MOD:
                    return "%=";
                case ScoreboardOp.SWAP:
                    return "><";
                case ScoreboardOp.MIN:
                    return "<";
                case ScoreboardOp.MAX:
                    return ">";
                default:
                    return "???";
            }
        }
        public static string String(this StructureRotation rot) => rot.ToString().Substring(1);

        public static string AlwaysDay(bool enabled) =>
            $"alwaysday {enabled.ToString().ToLower()}";

        public static string CameraShake(string target) =>
            $"camerashake add {target}";
        public static string CameraShake(string target, float intensity) =>
            $"camerashake add {target} {intensity}";
        public static string CameraShake(string target, float intensity, float seconds) =>
            $"camerashake add {target} {intensity} {seconds}";
        public static string CameraShake(string target, float intensity, float seconds, CameraShakeType shakeType) =>
            $"camerashake add {target} {intensity} {seconds} {shakeType}";

        public static string Clear(string target) =>
            $"clear {target}";
        public static string Clear(string target, string item) =>
            $"clear {target} {item}";
        public static string Clear(string target, string item, int data) =>
            $"clear {target} {item} {data}";
        public static string Clear(string target, string item, int data, int maxCount) =>
            $"clear {target} {item} {data} {maxCount}";

        public static string ClearSpawnPoint(string target) =>
            $"clearspawnpoint {target}";

        public static string Clone(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, Coord dstX, Coord dstY, Coord dstZ) =>
            $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ}";
        public static string Clone(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, Coord dstX, Coord dstY, Coord dstZ, bool copyAir, CloneMode mode) =>
            $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} " + (copyAir ? "replace" : "masked") + $" {mode}";
        public static string Clone(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, Coord dstX, Coord dstY, Coord dstZ, CloneMode mode, string filterBlock, int filterData) =>
            $"clone {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} filtered {mode} {filterBlock} {filterData}";

        public static string ConnectWSServer(string serverUri) =>
            $"wsserver {serverUri}";

        public static string Damage(string target, int amount) =>
            $"damage {target} {amount}";
        public static string Damage(string target, int amount, DamageCause cause) =>
            $"damage {target} {amount} {cause}";
        public static string Damage(string target, int amount, DamageCause cause, string damager) =>
            $"damage {target} {amount} {cause} entity {damager}";

        public static string Deop(string target) =>
            $"deop {target}";

        public static string DialogueShow(string target, string npc, string sceneName) =>
            $"dialogue open {npc} {target} {sceneName}";
        public static string DialogueChange(string target, string npc, string sceneName) =>
            $"dialogue change {npc} {sceneName} {target}";

        public static string Difficulty(DifficultyMode difficulty) =>
            $"difficulty {difficulty}";

        public static string EffectClear(string target) =>
            $"effect {target} clear";
        public static string Effect(string target, PotionEffect effect) =>
            $"effect {target} {effect}";
        public static string Effect(string target, PotionEffect effect, int seconds) =>
            $"effect {target} {effect} {seconds}";
        public static string Effect(string target, PotionEffect effect, int seconds, int amplifier) =>
            $"effect {target} {effect} {seconds} {amplifier}";
        public static string Effect(string target, PotionEffect effect, int seconds, int amplifier, bool hideParticles) =>
            $"effect {target} {effect} {seconds} {amplifier} {hideParticles.ToString().ToLower()}";

        public static string Enchant(string target, Enchantment enchantment) =>
            $"effect {target} {enchantment}";
        public static string Enchant(string target, Enchantment enchantment, int level) =>
            $"effect {target} {enchantment} {level}";

        public static string Event(string target, string eventName) =>
            $"event entity {target} {eventName}";

        public static string Execute(string target, Coord x, Coord y, Coord z, string command) =>
            $"execute {target} {x} {y} {z} {command}";
        public static string Execute(string target, Coord x, Coord y, Coord z, Coord detectX, Coord detectY, Coord detectZ, string block, string command) =>
            $"execute {target} {x} {y} {z} detect {detectX} {detectY} {detectZ} {block} -1 {command}";
        public static string Execute(string target, Coord x, Coord y, Coord z, Coord detectX, Coord detectY, Coord detectZ, string block, int data, string command) =>
            $"execute {target} {x} {y} {z} detect {detectX} {detectY} {detectZ} {block} {data} {command}";

        public static string Fill(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, string block) =>
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block}";
        public static string Fill(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, string block, int data) =>
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block} {data}";
        public static string Fill(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, string block, int data, OldObjectHandling fillMode) =>
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block} {data} {fillMode}";
        public static string Fill(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, string block, int data, string replaceBlock) =>
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block} {data} replace {replaceBlock} -1";
        public static string Fill(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, string block, int data, string replaceBlock, int replaceData) =>
            $"fill {x1} {y1} {z1} {x2} {y2} {z2} {block} {data} replace {replaceBlock} {replaceData}";

        public static string FogPush(string target, string fogId, string userProvidedId) =>
            $"fog {target} push {fogId} {userProvidedId}";
        public static string FogPop(string target, string userProvidedId) =>
            $"fog {target} pop {userProvidedId}";
        public static string FogRemove(string target, string userProvidedId) =>
            $"fog {target} remove {userProvidedId}";

        public static string Function(string name) =>
            $"function {name}";
        public static string Function(CommandFile function) =>
            $"function {function.QualifiedName}";

        public static string Gamemode(string target, GameMode mode) =>
            $"gamemode {mode} {target}";
        public static string Gamemode(string target, int mode) =>
            $"gamemode {mode} {target}";

        public static string Gamerule(GameRule rule, string value) =>
            $"gamerule {rule} {value}";
        public static string Gamerule(string rule, string value) =>
            $"gamerule {rule} {value}";

        public static string Give(string target, string item) =>
            $"give {target} {item}";
        public static string Give(string target, string item, int amount) =>
            $"give {target} {item} {amount}";
        public static string Give(string target, string item, int amount, int data) =>
            $"give {target} {item} {amount} {data}";
        public static string Give(string target, string item, int amount, int data, string json) =>
            $"give {target} {item} {amount} {data} {json}";

        public static string Help() =>
            $"help";

        public static string ImmutableWorld(bool immutable) =>
            $"immutableworld {immutable}";

        public static string Kick(string target) =>
            $"kick {target}";
        public static string Kick(string target, string reason) =>
            $"kick {target} {reason}";

        public static string Kill() =>
            $"kill";
        public static string Kill(string target) =>
            $"kill {target}";

        public static string List() =>
            $"list";

        public static string Locate(StructureType type) =>
            $"locate {type}";
        public static string Locate(string structureType) =>
            $"locate {structureType}";

        public static string LootTable(Coord x, Coord y, Coord z, LootTable table) =>
            $"loot spawn {x} {y} {z} loot {table.CommandPath}";
        public static string LootTable(Coord x, Coord y, Coord z, string table) =>
            $"loot spawn {x} {y} {z} loot {table}";
        public static string LootEntity(Coord x, Coord y, Coord z, string entity) =>
            $"loot spawn {x} {y} {z} kill {entity}";

        public static string Me(string text) =>
            $"me {text}";

        public static string MobEvent(MobEventType @event, bool value) =>
            $"mobevent minecraft:{@event} {value}";
        public static string MobEvent(string @event, bool value) =>
            $"mobevent {@event} {value}";
        public static string MobEvent(MobEventType @event) =>
            $"mobevent minecraft:{@event}";
        public static string MobEvent(string @event) =>
            $"mobevent {@event}";
        public static string MobEventsDisable() =>
            $"mobevent events_enabled false";
        public static string MobEventsEnable() =>
            $"mobevent events_enabled true";

        public static string Message(string target, string message) =>
            $"w {target} {message}";

        public static string MusicPlay(string track, float volume = 1f, float fadeSeconds = 0f, MusicRepeatMode repeatMode = MusicRepeatMode.play_once) =>
            $"music play {track} {volume} {fadeSeconds} {repeatMode}";
        public static string MusicQueue(string track, float volume = 1f, float fadeSeconds = 0f, MusicRepeatMode repeatMode = MusicRepeatMode.play_once) =>
            $"music queue {track} {volume} {fadeSeconds} {repeatMode}";
        public static string MusicStop(float fadeSeconds) =>
            $"music stop {fadeSeconds}";
        public static string MusicVolume(float volume) =>
            $"music volume {volume}";

        public static string Op(string target) =>
            $"op {target}";

        public static string Particle(string effect, Coord x, Coord y, Coord z) =>
            $"particle {effect} {x} {y} {z}";

        public static string PlayAnimation(string target, string animation) =>
            $"playanimation {target} {animation}";
        public static string PlayAnimation(string target, string animation, string nextState) =>
            $"playanimation {target} {animation} {nextState}";
        public static string PlayAnimation(string target, string animation, string nextState, float blendOutTime) =>
            $"playanimation {target} {animation} {nextState} {blendOutTime}";
        public static string PlayAnimation(string target, string animation, string nextState, float blendOutTime, string molangStopCondition) =>
            $"playanimation {target} {animation} {nextState} {blendOutTime} {molangStopCondition}";
        public static string PlayAnimation(string target, string animation, string nextState, float blendOutTime, string molangStopCondition, string controller) =>
            $"playanimation {target} {animation} {nextState} {blendOutTime} {molangStopCondition} {controller}";

        public static string PlaySound(string sound) =>
            $"playsound {sound}";
        public static string PlaySound(string sound, string target) =>
            $"playsound {sound} {target}";
        public static string PlaySound(string sound, string target, Coord x, Coord y, Coord z) =>
            $"playsound {sound} {target} {x} {y} {z}";
        public static string PlaySound(string sound, string target, Coord x, Coord y, Coord z, float volume) =>
            $"playsound {sound} {target} {x} {y} {z} {volume}";
        public static string PlaySound(string sound, string target, Coord x, Coord y, Coord z, float volume, float pitch) =>
            $"playsound {sound} {target} {x} {y} {z} {volume} {pitch}";
        public static string PlaySound(string sound, string target, Coord x, Coord y, Coord z, float volume, float pitch, float minVolume) =>
            $"playsound {sound} {target} {x} {y} {z} {volume} {pitch} {minVolume}";
        public static string PlaySound(string sound, string target, float volume) =>
            $"playsound {sound} {target} ~ ~ ~ {volume}";
        public static string PlaySound(string sound, string target, float volume, float pitch) =>
            $"playsound {sound} {target} ~ ~ ~ {volume} {pitch}";
        public static string PlaySound(string sound, string target, float volume, float pitch, float minVolume) =>
            $"playsound {sound} {target} ~ ~ ~ {volume} {pitch} {minVolume}";

        public static string Reload() =>
            $"reload";

        public static string ReplaceItemBlock(Coord x, Coord y, Coord z, int slot, OldObjectHandling handling, string item) =>
            $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item}";
        public static string ReplaceItemBlock(Coord x, Coord y, Coord z, int slot, OldObjectHandling handling, string item, int amount) =>
            $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount}";
        public static string ReplaceItemBlock(Coord x, Coord y, Coord z, int slot, OldObjectHandling handling, string item, int amount, int data) =>
            $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount} {data}";
        public static string ReplaceItemBlock(Coord x, Coord y, Coord z, int slot, OldObjectHandling handling, string item, int amount, int data, string json) =>
            $"replaceitem block {x} {y} {z} slot.container {slot} {handling} {item} {amount} {data} {json}";

        public static string ReplaceItemEntity(string target, EquipmentSlotType slotType, int slot, OldObjectHandling handling, string item) =>
            $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item}";
        public static string ReplaceItemEntity(string target, EquipmentSlotType slotType, int slot, OldObjectHandling handling, string item, int amount) =>
            $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount}";
        public static string ReplaceItemEntity(string target, EquipmentSlotType slotType, int slot, OldObjectHandling handling, string item, int amount, int data) =>
            $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount} {data}";
        public static string ReplaceItemEntity(string target, EquipmentSlotType slotType, int slot, OldObjectHandling handling, string item, int amount, int data, string json) =>
            $"replaceitem entity {target} {slotType.String()} {slot} {handling} {item} {amount} {data} {json}";

        public static string Ride(string sources, string targets, TeleportRules tpRules = TeleportRules.teleport_rider, RideFillType fillType = RideFillType.until_full) =>
            $"ride {sources} start_riding {targets} {tpRules} {fillType}";
        public static string RideDismount(string riders) =>
            $"ride {riders} stop_riding";
        public static string RideEvictRiders(string rides) =>
            $"ride {rides} evict_riders";
        public static string RideSummonRider(string rides, string entity) =>
            $"ride {rides} summon_rider {entity}";
        public static string RideSummonRider(string rides, string entity, string nameTag) =>
            $"ride {rides} summon_rider {entity} none \"{nameTag}\"";
        public static string RideSummonRide(string riders, string entity) =>
            $"ride {riders} summon_ride {entity}";
        public static string RideSummonRide(string riders, string entity, RideRules rules) =>
            $"ride {riders} summon_ride {entity} {rules}";
        public static string RideSummonRide(string riders, string entity, RideRules rules, string nameTag) =>
            $"ride {riders} summon_ride {entity} {rules} none \"{nameTag}\"";

        public static string SaveHold() =>
            $"save hold";
        public static string SaveQuery() =>
            $"save query";
        public static string SaveResume() =>
            $"save resume";

        public static string Say(string message) =>
            $"say {message}";

        public static string ScheduleAreaLoaded(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, string function) =>
            $"schedule on_area_loaded add {x1} {y1} {z1} {x2} {y2} {z2}";
        public static string ScheduleAreaLoaded(Coord x, Coord y, Coord z, int radius, string function) =>
            $"schedule on_area_loaded add circle {x} {y} {z} {radius} {function}";
        public static string ScheduleAreaLoaded(string tickingArea, string function) =>
            $"schedule on_area_loaded add tickingarea {tickingArea} {function}";

        public static string ScoreboardCreateObjective(string name) =>
            $"scoreboard objectives add \"{name}\" dummy";
        public static string ScoreboardRemoveObjective(string name) =>
            $"scoreboard objectives remove \"{name}\"";
        public static string ScoreboardDisplayList(string name) =>
            $"scoreboard objectives setdisplay list \"{name}\"";
        public static string ScoreboardDisplayList(string name, ScoreboardOrdering ordering) =>
            $"scoreboard objectives setdisplay list \"{name}\" {ordering}";
        public static string ScoreboardDisplaySidebar(string name) =>
            $"scoreboard objectives setdisplay sidebar \"{name}\"";
        public static string ScoreboardDisplaySidebar(string name, ScoreboardOrdering ordering) =>
            $"scoreboard objectives setdisplay sidebar \"{name}\" {ordering}";
        public static string ScoreboardDisplayBelowName(string name) =>
            $"scoreboard objectives setdisplay belowname \"{name}\"";
        public static string ScoreboardList(string target) =>
            $"scoreboard players list {target}";
        public static string ScoreboardSet(string target, string objective, int value) =>
            $"scoreboard players set {target} \"{objective}\" {value}";
        public static string ScoreboardAdd(string target, string objective, int amount) =>
            $"scoreboard players add {target} \"{objective}\" {amount}";
        public static string ScoreboardSubtract(string target, string objective, int amount) =>
            $"scoreboard players remove {target} \"{objective}\" {amount}";
        public static string ScoreboardRandom(string target, string objective, int min, int max) =>
            $"scoreboard players random {target} \"{objective}\" {min} {max}"; // both inclusive
        public static string ScoreboardRemove(string target, string objective) =>
            $"scoreboard players reset {target} \"{objective}\"";
        public static string ScoreboardOpRaw(string target, string a, ScoreboardOp op, string b) =>
            $"scoreboard players operation {target} \"{a}\" {op.String()} {target} \"{b}\"";
        public static string ScoreboardOpRaw(string a, ScoreboardOp op, string b) =>
            $"scoreboard players operation * \"{a}\" {op.String()} @a \"{b}\"";
        public static string ScoreboardOpSet(string a, string b) =>
            $"scoreboard players operation * \"{a}\" = @a \"{b}\"";
        public static string ScoreboardOpAdd(string a, string b) =>
            $"scoreboard players operation * \"{a}\" += @a \"{b}\"";
        public static string ScoreboardOpSub(string a, string b) =>
            $"scoreboard players operation * \"{a}\" -= @a \"{b}\"";
        public static string ScoreboardOpMul(string a, string b) =>
            $"scoreboard players operation * \"{a}\" *= @a \"{b}\"";
        public static string ScoreboardOpDiv(string a, string b) =>
            $"scoreboard players operation * \"{a}\" /= @a \"{b}\"";
        public static string ScoreboardOpMod(string a, string b) =>
            $"scoreboard players operation * \"{a}\" %= @a \"{b}\"";
        public static string ScoreboardOpSwap(string a, string b) =>
            $"scoreboard players operation * \"{a}\" >< @a \"{b}\"";
        public static string ScoreboardOpMin(string a, string b) =>
            $"scoreboard players operation * \"{a}\" < @a \"{b}\"";
        public static string ScoreboardOpMax(string a, string b) =>
            $"scoreboard players operation * \"{a}\" > @a \"{b}\"";
        public static string ScoreboardOpSet(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" = {target} \"{b}\"";
        public static string ScoreboardOpAdd(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" += {target} \"{b}\"";
        public static string ScoreboardOpSub(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" -= {target} \"{b}\"";
        public static string ScoreboardOpMul(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" *= {target} \"{b}\"";
        public static string ScoreboardOpDiv(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" /= {target} \"{b}\"";
        public static string ScoreboardOpMod(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" %= {target} \"{b}\"";
        public static string ScoreboardOpSwap(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" >< {target} \"{b}\"";
        public static string ScoreboardOpMin(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" < {target} \"{b}\"";
        public static string ScoreboardOpMax(string target, string a, string b) =>
            $"scoreboard players operation {target} \"{a}\" > {target} \"{b}\"";

        public static string SetBlock(Coord x, Coord y, Coord z, string block) =>
            $"setblock {x} {y} {z} {block}";
        public static string SetBlock(Coord x, Coord y, Coord z, string block, int data) =>
            $"setblock {x} {y} {z} {block} {data}";
        public static string SetBlock(Coord x, Coord y, Coord z, string block, int data, OldObjectHandling handling) =>
            $"setblock {x} {y} {z} {block} {data} {handling}";

        public static string SetMaxPlayers(int max) =>
            $"setmaxplayers {max}";

        public static string SetWorldSpawn(Coord x, Coord y, Coord z) =>
            $"setworldspawn {x} {y} {z}";

        public static string Spawnpoint() =>
            $"spawnpoint";
        public static string Spawnpoint(string target) =>
            $"spawnpoint {target}";
        public static string Spawnpoint(string target, Coord x, Coord y, Coord z) =>
            $"spawnpoint {target} {x} {y} {z}";

        public static string SpreadPlayers(Coord x, Coord z, float spreadDistance, float maxRange, string targets) =>
            $"spreadplayers {x} {z} {spreadDistance} {maxRange} {targets}";

        public static string Stop() =>
            $"stop";

        public static string StopSound(string target) =>
            $"stopsound {target}";
        public static string StopSound(string target, string sound) =>
            $"stopsound {target} {sound}";

        public static string StructureSaveDisk(string name, Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2) =>
            $"structure save {name} {x1} {y1} {z1} {x2} {y2} {z2} disk";
        public static string StructureSaveMemory(string name, Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2) =>
            $"structure save {name} {x1} {y1} {z1} {x2} {y2} {z2} memory";
        public static string StructureSaveDisk(string name, Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, bool includeEntities, bool includeBlocks = true) =>
            $"structure save {name} {x1} {y1} {z1} {x2} {y2} {z2} {includeEntities} disk {includeBlocks}";
        public static string StructureSaveMemory(string name, Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, bool includeEntities, bool includeBlocks = true) =>
            $"structure save {name} {x1} {y1} {z1} {x2} {y2} {z2} {includeEntities} memory {includeBlocks}";
        public static string StructureLoad(string name, Coord x, Coord y, Coord z, StructureRotation rotation = StructureRotation._0_degrees,
                StructureMirror flip = StructureMirror.none, bool includeEntities = true, bool includeBlocks = true, float integrity = 100, string seed = null) =>
            seed == null ? $"structure load {name} {x} {y} {z} {rotation.String()} {flip} {includeEntities} {includeBlocks} {integrity}"
            : $"structure load {name} {x} {y} {z} {rotation.String()} {flip} {includeEntities} {includeBlocks} {integrity} {seed}";
        public static string StructureLoad(string name, Coord x, Coord y, Coord z, StructureRotation rotation = StructureRotation._0_degrees,
                StructureMirror flip = StructureMirror.none, StructureAnimationMode animation = StructureAnimationMode.layer_by_layer,
                float animationSeconds = 0, bool includeEntities = true, bool includeBlocks = true, float integrity = 100, string seed = null) =>
            seed == null ? $"structure load {name} {x} {y} {z} {rotation.String()} {flip} {animation} {animationSeconds} {includeEntities} {includeBlocks} {integrity}"
            : $"structure load {name} {x} {y} {z} {rotation.String()} {flip} {animation} {animationSeconds} {includeEntities} {includeBlocks} {integrity} {seed}";
        public static string StructureDelete(string name) =>
            $"structure delete {name}";

        public static string Summon(string entity) =>
            $"summon {entity}";
        public static string Summon(string entity, Coord x, Coord y, Coord z) =>
            $"summon {entity} {x} {y} {z}";
        public static string Summon(string entity, Coord x, Coord y, Coord z, string nameTag) =>
            $"summon {entity} {x} {y} {z} named \"{nameTag}\"";

        public static string Tag(string targets, string tag) =>
            $"tag {targets} add {tag}";
        public static string TagRemove(string targets, string tag) =>
            $"tag {targets} remove {tag}";
        public static string TagList(string targets) =>
            $"tag {targets} list";

        public static string Teleport(string otherEntity, bool checkForBlocks = false) =>
            $"tp {otherEntity} {checkForBlocks}";
        public static string Teleport(string target, string otherEntity, bool checkForBlocks = false) =>
            $"tp {target} {otherEntity} {checkForBlocks}";
        public static string Teleport(Coord x, Coord y, Coord z, bool checkForBlocks = false) =>
            $"tp {x} {y} {z} {checkForBlocks}";
        public static string Teleport(Coord x, Coord y, Coord z, Coord yRot, Coord xRot, bool checkForBlocks = false) =>
            $"tp {x} {y} {z} {yRot} {xRot} {checkForBlocks}";
        public static string Teleport(string target, Coord x, Coord y, Coord z, bool checkForBlocks = false) =>
            $"tp {target} {x} {y} {z} {checkForBlocks}";
        public static string Teleport(string target, Coord x, Coord y, Coord z, Coord yRot, Coord xRot, bool checkForBlocks = false) =>
            $"tp {target} {x} {y} {z} {yRot} {xRot} {checkForBlocks}";
        public static string TeleportFacing(Coord x, Coord y, Coord z, Coord facingX, Coord facingY, Coord facingZ, bool checkForBlocks = false) =>
            $"tp {x} {y} {z} facing {facingX} {facingY} {facingZ} {checkForBlocks}";
        public static string TeleportFacing(Coord x, Coord y, Coord z, string facingEntity, bool checkForBlocks = false) =>
            $"tp {x} {y} {z} facing {facingEntity} {checkForBlocks}";
        public static string TeleportFacing(string target, Coord x, Coord y, Coord z, Coord facingX, Coord facingY, Coord facingZ, bool checkForBlocks = false) =>
            $"tp {target} {x} {y} {z} facing {facingX} {facingY} {facingZ} {checkForBlocks}";
        public static string TeleportFacing(string target, Coord x, Coord y, Coord z, string facingEntity, bool checkForBlocks = false) =>
            $"tp {target} {x} {y} {z} facing {facingEntity} {checkForBlocks}";

        public static string Tellraw(string jsonMessage) =>
            $"tellraw @a {jsonMessage}";
        public static string Tellraw(string targets, string jsonMessage) =>
            $"tellraw {targets} {jsonMessage}";

        public static string TestFor(string targets) =>
            $"testfor {targets}";

        public static string TestForBlock(Coord x, Coord y, Coord z, string block) =>
            $"testforblock {x} {y} {z} {block}";
        public static string TestForBlock(Coord x, Coord y, Coord z, string block, int data) =>
            $"testforblock {x} {y} {z} {block} {data}";

        public static string TestForBlocks(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, Coord dstX, Coord dstY, Coord dstZ) =>
            $"testforblocks {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ}";
        public static string TestForBlocksMasked(Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2, Coord dstX, Coord dstY, Coord dstZ) =>
            $"testforblocks {x1} {y1} {z1} {x2} {y2} {z2} {dstX} {dstY} {dstZ} masked";

        public static string TickingAreaAdd(string name, Coord x1, Coord y1, Coord z1, Coord x2, Coord y2, Coord z2) =>
            $"tickingarea add {x1} {y1} {z1} {x2} {y2} {z2} \"{name}\"";
        public static string TickingAreaAdd(string name, Coord x1, Coord y1, Coord z1, int radius) =>
            $"tickingarea add circle {x1} {y1} {z1} {radius} \"{name}\"";
        public static string TickingAreaRemove(string name) =>
            $"tickingarea remove \"{name}\"";
        public static string TickingAreaRemove(Coord x1, Coord y1, Coord z1) =>
            $"tickingarea remove {x1} {y1} {z1}";
        public static string TickingAreaRemoveAll() =>
            $"tickingarea remove_all";
        public static string TickingAreaList() =>
            $"tickingarea list";
        public static string TickingAreaListAll() =>
            $"tickingarea list all-dimensions";

        public static string TimeAdd(int ticks) =>
            $"time add {ticks}";
        public static string TimeSet(int ticks) =>
            $"time set {ticks}";
        public static string TimeSet(TimeSpec spec) =>
            $"time set {spec}";
        public static string TimeGet(TimeQuery query) =>
            $"time query {query}";

        public static string TitleClear(string target) =>
            $"titleraw {target} clear";
        public static string TitleReset(string target) =>
            $"titleraw {target} reset";
        public static string TitleSubtitle(string target, string json) =>
            $"titleraw {target} subtitle {json}";
        public static string TitleActionBar(string target, string json) =>
            $"titleraw {target} actionbar {json}";
        public static string Title(string target, string json) =>
            $"titleraw {target} title {json}";
        public static string TitleTimes(string target, int fadeIn, int stay, int fadeOut) =>
            $"titleraw {target} times {fadeIn} {stay} {fadeOut}";

        public static string ToggleDownfall() =>
            $"toggledownfall";

        public static string Weather(WeatherState state) =>
            $"weather {state}";
        public static string Weather(WeatherState state, int durationTicks) =>
            $"weather {state} {durationTicks}";
        public static string WeatherQuery() =>
            $"weather query";

        public static string Whitelist(string player) =>
            $"whitelist add {player}";
        public static string WhitelistRemove(string player) =>
            $"whitelist remove {player}";
        public static string WhitelistList() =>
            $"whitelist list";
        public static string WhitelistOff() =>
            $"whitelist off";
        public static string WhitelistOn() =>
            $"whitelist on";
        public static string WhitelistReload() =>
            $"whitelist reload";

        public static string Xp(int amount) =>
            $"xp {amount}";
        public static string XpL(int amount) =>
            $"xp {amount}L";
        public static string Xp(int amount, string target) =>
            $"xp {amount} {target}";
        public static string XpL(int amount, string target) =>
            $"xp {amount}L {target}";
    }

    /// <summary>
    /// A tag that contains an index. Used with Command.UTIL.[Push/Pop]Tag
    /// </summary>
    public struct IndexedTag
    {
        public string name;
        public int index;

        public string Tag { get => name + index; }
        public static implicit operator string(IndexedTag tag) => tag.Tag;
    }
    public enum CameraShakeType
    {
        positional, rotational
    }
    public enum CloneMode
    {
        force, move, normal
    }
    [EnumParsable(typeof(DifficultyMode))]
    public enum DifficultyMode : int
    {
        peaceful = 0,
        easy = 1,
        normal = 2,
        hard = 3
    }
    [EnumParsable(typeof(OldObjectHandling))]
    public enum OldObjectHandling
    {
        destroy, keep, hollow, outline, replace
    }
    [EnumParsable(typeof(PotionEffect))]
    public enum PotionEffect : int
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
        quick_charge
    }
    [EnumParsable(typeof(GameMode))]
    public enum GameMode : int
    {
        survival = 0,
        creative = 1,
        adventure = 2
    }
    [EnumParsable(typeof(DamageCause))]
    public enum DamageCause
    {
        all,
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
    [EnumParsable(typeof(EquipmentSlotType))]
    public enum EquipmentSlotType
    {
        slot_armor_head,
        slot_armor_chest,
        slot_armor_legs,
        slot_armor_feet,
        slot_weapon_mainhand,
        slot_weapon_offhand,
        slot_container,
        slot_enderchest,
        slot_hotbar,
        slot_inventory,
        slot_saddle,
        slot_armor,
        slot_chest
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
        ascending, descending
    }
    public enum ScoreboardOp
    {
        SET, ADD, SUB, MUL, DIV, MOD, SWAP, MIN, MAX
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
        none, x, z, xz
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
    public enum TimeSpec : int
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
}