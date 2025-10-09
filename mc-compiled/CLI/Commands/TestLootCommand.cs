using System;
using System.IO;
using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding.Behaviors.Loot;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class TestLootCommand() : CommandLineOption([])
{
    public override string LongName => "testloot";
    public override string ShortName => null;
    public override string Description => "Writes a loot table to test the internal library.";
    public override bool IsRunnable => true;
    public override bool IsHiddenFromHelp => true; // not helpful to most users
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new TestLootCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        files = null; // prevent further execution
        Definitions.TryInitialize(context.debug);
        var table = new LootTable("test");
        table.pools.Add(new LootPool(6, new LootEntry(LootEntry.EntryType.item, "minecraft:iron_sword")
                .WithFunction(new LootFunctionEnchant(new EnchantmentEntry(Enchantment.sharpness, 20)))
                .WithFunction(new LootFunctionDurability(0.5f))
                .WithFunction(new LootFunctionName("§lSuper Sword"))
                .WithFunction(new LootFunctionLore(
                    "§cHi! This is a line of lore.",
                    "§6Here's another line.")), new LootEntry(LootEntry.EntryType.item, "minecraft:book")
                .WithFunction(new LootFunctionBook("Test Book", "lukecreator",
                    "yo welcome to the first page!\nSecond line.",
                    "Second page!")), new LootEntry(LootEntry.EntryType.item, "minecraft:leather_chestplate")
                .WithFunction(new LootFunctionName("Random Enchant"))
                .WithFunction(new LootFunctionRandomEnchant(true))
                .WithFunction(new LootFunctionRandomDye()),
            new LootEntry(LootEntry.EntryType.item, "minecraft:leather_leggings")
                .WithFunction(new LootFunctionName("Simulated Enchant"))
                .WithFunction(new LootFunctionSimulateEnchant(20, 40)),
            new LootEntry(LootEntry.EntryType.item, "minecraft:leather_boots")
                .WithFunction(new LootFunctionName("Gear Enchant"))
                .WithFunction(new LootFunctionRandomEnchantGear(1.0f)),
            new LootEntry(LootEntry.EntryType.item, "minecraft:cooked_beef")
                .WithFunction(new LootFunctionCount(2, 64))));

        File.WriteAllBytes("testloot.json", table.GetOutputData());
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Written test table to 'testloot.json'");
        Console.ForegroundColor = oldColor;
    }
}