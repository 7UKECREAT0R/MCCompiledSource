# Structure Generation

<primary-label ref="compile_time"/>

<link-summary>
Generating structure files at compile-time with loads of features that extend beyond vanilla commands.
</link-summary>

MCCompiled has reasonable support for generating `.mcstructure` files at compile time. There's a whole host of reasons
you would want to do this. Some examples include:
- Setting many blocks at once with fast runtime performance (since the structure is pre-baked)
- Algorithmically generating structures using math, data, or other logic.
- Placing blocks with data that goes beyond vanilla commands, such as pre-filled chests, furnaces, command blocks, or signs.

## Getting Started

### Enabling the Feature
To start, make sure you have the `structures` feature enabled. You can do this by adding the line `feature structures` to the top of your file.
```%lang%
// allows the compiler to output .mcstructure files
feature structures
```

### Creating a new Structure
Every structure starts with the `structure new <name: string>` command. The command must always be followed by a code block `{}`,
which will contain the code which defines what blocks will go inside it.
```%lang%
structure new "example" {
    // blocks go in here!
}
``` 

> The size of the structure is automatically however big it needs to be to fit all defined blocks inside.
{style="tip"}

### Setting Blocks
When you're inside the block of code related to your new structure, the behavior of any block-related commands change
so that they affect the structure instead of the world. This means you can use `setblock`, `fill`, [`scatter`](Scatter.md), `replace`, 
etc... and they will affect the structure you're defining.

An admittedly strange example is shown here which creates a hunk of iron ore and stone, with the top edges smoothed out:
```%lang%
feature structures

structure new "thing" {
    // set the stone and then scatter iron ore randomly inside it
    fill 0 0 0 10 10 10 stone
    scatter 0 0 0 10 10 10 iron_ore 40

    // remove the top edges
    fill 0 10 0 10 10 0 structure_void
    fill 0 10 0 0 10 10 structure_void
    fill 10 10 0 10 10 10 structure_void
    fill 0 10 10 10 10 10 structure_void
}
```
and it looks like this when loaded with `/structure load "thing" ~~~`:

![What the structure looks like when loaded.](thing.jpg){border-effect="rounded" width="720"}

### Structure Voids
The `minecraft:structure_void` block is what all new structures are filled with by default. When the structure is loaded, any
structure void blocks will leave any existing blocks completely unaffected. They're effectively no-ops, and work like
erasers when used in commands.

MCCompiled understands when a `structure_void` is used and has compatibility built in for them!

## Structure-Specific Commands
MCCompiled 1.20 introduces new commands designed specifically for use inside structures. While support is not extremely
robust, they're still useful for covering a majority of use cases. The best part is that if you ever need a niche new
feature, the project is easily extensible and can be forked and modified to add it.

### Containers with Items {collapsible="true" default-state="collapsed"}
The `container` command lets you place a container inside a structure containing any number of items. It's always followed
by a code block, which lets you use the associated `item` command to place items inside the container.
- `container <int: x, y, z> <block> <facing direction> {}`

#### Facing Direction
The "facing direction" parameter is the direction the container will face. Some containers only support cardinal directions,
such as chests and furnaces, while others support all directions, such as dispensers or barrels. They all use the same
syntax, so you can use any of the following:
- `down`
- `up`
- `north` or `negativez`
- `east` or `positivex`
- `west` or `negativex`
- `south` or `positivez`

#### Filling with Items
While inside a container, the `item` command can be run to place an item inside the container. Its syntax is very similar
to the [`give`](Giving-Items.md) command, but it doesn't allow you to specify a player.
- `item <item> [int: count] [int: data] [attributes]`

See [`give attributes`](Giving-Items.md#attributes) for a list of all attributes that can be used with the `item` command.
Additionally, you can use the following attribute(s) which are exclusive to the `item` command:

Place Item At
: Select the slot in the container which the item will be placed at.
- `at: <int: slot>`

#### Example {id="container_example"}
This example shows how you could place a pre-filled chest with some basic starter items scattered randomly inside it:
```%lang%
container 0 0 0 "chest" north {
    item "bread" 5 at: 7
    item "leather_helmet" 1 at: 2
    item "leather_leggings" 1 at: 18
    item "stone_pickaxe" 1 at: 25
}
```

![Example showing the chest that's produced by the example code above.](structure_chest_example.png){border-effect="rounded" width="480"}

### Signs with Custom Text {collapsible="true" default-state="collapsed"}
The `sign` command lets you place a sign with text inside the structure. It's relatively barebones right now, only supporting
front text, and it has no abstracted-away rotation logic. You'll need to specify the block states yourself for orienting the sign.
- `sign <int: x y z> <block> <states> [bool: isEditable] <string: text>`

#### Text Lines
When specifying the text for a sign, you can use `\n` or `~LINEBREAK~` to indicate a new line.

#### Rotation Guide
There are three prominent types of signs. `<wood>_standing_sign`, `<wood>_hanging_sign`, and `<wood>_wall_sign`. If you
want to use an oak wood standing/wall sign, then you can omit the `<wood>` part entirely. Hanging sign identifiers are consistent across the three types.

![Image showing how each different sign variant looks in-game.](sign_identifiers_example.png){border-effect="rounded" width="480"}

Standing Signs
: Supports the `"ground_sign_direction"` property, which accepts an integer 0–15 inclusive. The integer represents
the 16 possible directions that a sign standing on the ground can be placed at. 
: *Example: `["ground_sign_direction"=4]`*

Hanging Signs
: Supports two different properties.
- `hanging` can be `true` or `false` to indicate if the sign is "hanging" from a block. If false, a board is attached to the top of the sign.
- `facing_direction` can be 2–5 inclusive, mapping to north (2), south (3), west (4), and east (5).
: *Example: `["hanging"=true,"facing_direction"=4]`*

Wall Signs
: Supports the `facing_direction` property, which accepts an integer 2–5 inclusive. The integer represents
the four possible cardinal directions, mapping to north (2), south (3), west (4), and east (5).
: *Example: `["facing_direction"=4]`*

#### Example {id="sign_example"}
This example shows you how to place a non-editable wall sign facing south that says "Hey there":
```%lang%
sign 0 0 0 "wall_sign" [facing_direction=3] false "Hey there"
```

![Image showing the wiki_sign structure loaded.](sign_example.png){border-effect="rounded" width="480"}

### Command Blocks {collapsible="true" default-state="collapsed"}
The `commandblock` command lets you place a command block inside a structure with a multitude of configurable options.
It doesn't cover *every* single possible use case, but it covers most of the common ones. There are two different variants
of the command, one of which is for the `repeating_command_block`, and one for the others.

- `commandblock <int: x, y, z> impulse/chain [int: delay] <direction> [bool: conditional] <string: command>`
- `commandblock <int: x, y, z> repeating <bool: always active> [int: delay] <direction> [bool: conditional] <string: command>`

#### Direction
The "direction" parameter is the direction the command block will face towards. It can be one of the following:
- `down`
- `up`
- `north` or `negativez`
- `east` or `positivex`
- `west` or `negativex`
- `south` or `positivez`

#### Example {id="commandblock_example"}
This example shows you how to create a command block which gives you an emerald every time you press the button on top of it.

```%lang%
// place the command block
commandblock 0 0 0 impulse up "give @p emerald 1"

// add a button for the player to press
setblock 0 1 0 wooden_button [facing_direction=1]
```

![emerald_button.png](emerald_button.png){border-effect="rounded" width="480" style="inline"}
![emerald_button_2.png](emerald_button_2.png){border-effect="rounded" width="480" style="inline"}