
# Cheat Sheet

This file is generated automatically with `mc-compiled --syntax wiki`

## Comments {id="comments"}

- `// <comment>` - Line comment. Lets you make a note for yourself on the current line.
- `/* <comment> */` - Multi-line comment. Lets you make a note for yourself on MULTIPLE LINES!

## Code Block {id="code-block"}

- Starts and ends with `{` brackets `}`, holding only code inside.

```%lang%
...
{
    // inside the code block
}
```


---


## Types {id="types"}

Descriptions of the upcoming types that will be present in the various command arguments.

identifier
: An identifier that either has meaning or doesn't. An identifier can be the name of anything defined in the language. It's usually self-explanatory when it's required.

integer
: Any integral number, like 5, 10, 5291, or -40. Use time suffixes to scale the integer accordingly, like with 4s -> 80.

string
: A block of text on a single line, surrounded with either 'single quotes' or "double quotes."

true/false
: A value that can be either 'true' or 'false.'

selector
: A Minecraft selector that targets a specific entity or set of entities. Example: `@e[type=cow]`

value
: The name of a runtime value that was defined using the `define` command.

preprocessor variable
: The name of a preprocessor variable that was defined using the `$var` command or similar, **without** the `$` symbol.

coordinate
: A Minecraft coordinate value that can optionally be both relative and facing offset, like ~10, 40, or ^5.

range
: A Minecraft number that specifies a range of integers (inclusive). Omitting a number from one side makes the number unbounded. `4..` means four and up. `1..5` means one through five.

JSON
: A JSON object achieved by $dereferencing a preprocessor variable holding one.


## Commands {id="commands-root"}

All the commands in the language (version 1.20). The command ID is the first word of the line, followed by the arguments it gives. Each command parameter includes the type it's allowed to be and its name. A required parameter is surrounded in `<angle brackets>`, and an optional parameter is surrounded in `[square brackets]`.

### Category: preprocessor {id="commands-preprocessor"}

Commands that allow the user to do things at compile time. Preprocessor commands generally start with a `$` and are highlighted differently than regular commands.
[Add to Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Adds two preprocessor variables/values together, changing only the first one. A += B

- `<preprocessor variable: variable>` `<1 or more object: values>`

[Append to Preprocessor Variable](Advanced-Variable-Commands.md#array-specific-manipulation)
: Adds the given item(s) to the end of the given preprocessor variable, or contents of another preprocessor variable if specified.

- in order:
	- `<preprocessor variable: array>`
	- one of:
		- `<any number of object: items>`
		- `<preprocessor variable: other>`

[Preprocessor Call Function](Metaprogramming.md#calling)
: Calls a function by name and passes in the given parameters. Because this is a preprocessor operation, it has the same error handling as a normal function call.

- `<string: function name>` `<any number of *: parameters>`

[Decrement Preprocessor Variable](Simple-Variable-Commands.md#inc-dec)
: Decrements the given preprocessor variable by one. If multiple values are held, they are all decremented.

- `<preprocessor variable: variable>`

[Divide Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Divides two preprocessor variables/values from each other, changing only the first one. A /= B

- `<preprocessor variable: variable>` `<1 or more object: values>`

[Preprocessor Else](Comparison-compile-time.md#using-else)
: Directly inverts the result of the last $if call at this level in scope.

- `[code block]`

[Preprocessor If](Comparison-compile-time.md#using-if)
: Compares a preprocessor variable and another value/variable. If the source variable contains multiple values, they all must match the condition.

- in order:
	- one of:
		- `<object: a>`
		- `<preprocessor variable: a>`
	- `<compare: comparison>`
	- one of:
		- `<object: a>`
		- `<preprocessor variable: a>`
	- `<code block>`

[Increment Preprocessor Variable](Simple-Variable-Commands.md#inc-dec)
: Increments the given preprocessor variable by one. If multiple values are held, they are all incremented.

- `<preprocessor variable: variable>`

[Include File](Including-Other-Files.md)
: Places the contents of the given file in replacement for this statement. Not intended for production use yet.

- `<string: file>`

Iterate Preprocessor Array
: Runs the following statement/code-block once for each value in the given preprocessor variable. The current iteration is held in the preprocessor variable given. If the target is a JSON array, the elements will be iterated upon.

- in order:
	- one of:
		- `<identifier: variable>`
		- `<JSON: json array or object>`
	- `<identifier: current>`
	- `<code block>`

[Preprocessor Load JSON Value](JSON-Processing.md)
: Load a JSON file (if not previously loaded) and retrieve a value from it, storing said value in a preprocessor variable.

- in order:
	- one of:
		- `<string: file name>`
		- `<JSON: existing json>`
	- `<identifier: result>`
	- `<string: path>`

[Preprocessor Length](Advanced-Variable-Commands.md#data-manipulation)
: If a preprocessor variable identifier or JSON array is specified, the number of elements it holds is gotten. If a string is given, its length is gotten.

- in order:
	- `<identifier: result>`
	- one of:
		- `<preprocessor variable: variable>`
		- `<JSON: json array>`
		- `<string: text>`

[Log to Console](Debugging.md#logging)
: Sends a message to stdout with a line terminator at the end.

- `<any number of *: message>`

[Define/Call Macro](Macros.md#defining-a-macro)
: If a code-block follows this directive, it is treated as a definition. Arguments are passed in as preprocessor variables. If no code-block follows this call, it will attempt to run the macro with any inputs parameters copied to their respective preprocessor variables.
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- :
	- one of:
		- `<identifier: macro name>` `<any number of identifier: arg names>` `<code block>`
		- `<identifier: macro name>` `<any number of *: arg values>`

[Preprocessor Array Mean](Advanced-Variable-Commands.md#data-manipulation)
: Averages all values in the given preprocessor variable together into one value and stores it in a result variable.

- :
	- one of:
		- `<preprocessor variable: variable to modify>`
		- `<identifier: result>` `<preprocessor variable: variable>`

[Preprocessor Array Median](Advanced-Variable-Commands.md#data-manipulation)
: Gets the middle value/average of the two middle values and stores it in a result variable.

- :
	- one of:
		- `<preprocessor variable: variable to modify>`
		- `<identifier: result>` `<preprocessor variable: variable>`

[Modulo Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Divides two preprocessor variables/values from each other, setting only the first one to the remainder of the operation. A %= B

- `<preprocessor variable: variable>` `<1 or more object: values>`

[Multiply with Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Multiplies two preprocessor variables/values together, changing only the first one. A *= B

- `<preprocessor variable: variable>` `<1 or more object: values>`

[Exponentiate Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Exponentiates two preprocessor variables/values with each other, changing only the first one. A = A^B

- `<preprocessor variable: variable>` `<1 or more object: values>`

[Prepend to Preprocessor Variable](Advanced-Variable-Commands.md#array-specific-manipulation)
: Adds the given item(s) to the start of the given preprocessor variable.

- in order:
	- `<preprocessor variable: array>`
	- one of:
		- `<any number of object: items>`
		- `<preprocessor variable: other>`

[Preprocessor Repeat](Compile-Time-Loops.md#repeat_number)
: Repeats the following statement/code-block a number of times. If a variable identifier is given, that variable will be set to the index of the current iteration. 0, 1, 2, etc.

- in order:
	- one of:
		- `<integer: repetitions>`
		- `<range: repetitions>`
	- `<identifier: indicator>`

[Preprocessor Reverse](Advanced-Variable-Commands.md#array-specific-manipulation)
: Reverses the order of the values in the given preprocessor variable.

- `<identifier: variable>`

[Preprocessor Array Sort](Advanced-Variable-Commands.md#array-specific-manipulation)
: Sorts the order of the values in the given preprocessor variable either 'ascending' or 'descending'. Values must be comparable.

- :
	- one of:
		- `ascending` Sort variables starting with the lowest first.
			- `<identifier: variable>`
		- `descending` Sort variables starting with the highest first.
			- `<identifier: variable>`

[Preprocessor String Friendly Name](Advanced-Variable-Commands.md#string-manipulation)
: Convert the given preprocessor variable value(s) to a string in 'Title Case'.

- :
	- one of:
		- `<preprocessor variable: variable to modify>`
		- `<identifier: result>` `<preprocessor variable: variable>`

[Preprocessor String Lowercase](Advanced-Variable-Commands.md#string-manipulation)
: Convert the given preprocessor variable value(s) to a string in 'lowercase'.

- :
	- one of:
		- `<preprocessor variable: variable to modify>`
		- `<identifier: result>` `<preprocessor variable: variable>`

[Preprocessor String Uppercase](Advanced-Variable-Commands.md#string-manipulation)
: Convert the given preprocessor variable value(s) to a string in 'UPPERCASE'.

- :
	- one of:
		- `<preprocessor variable: variable to modify>`
		- `<identifier: result>` `<preprocessor variable: variable>`

[Subtract from Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Subtracts two preprocessor variables/values from each other, changing only the first one. A -= B

- `<preprocessor variable: variable>` `<1 or more object: values>`

[Preprocessor Array Sum](Advanced-Variable-Commands.md#data-manipulation)
: Adds all values in the given preprocessor variable together into one value and stores it in a result variable.

- :
	- one of:
		- `<preprocessor variable: variable to modify>`
		- `<identifier: result>` `<preprocessor variable: variable>`

[Swap Preprocessor Variables](Simple-Variable-Commands.md#other-variable-operations)
: Swaps the values of two preprocessor variables

- `<preprocessor variable: a, b>`

[Preprocessor Array Unique](Advanced-Variable-Commands.md#array-specific-manipulation)
: Flattens the given preprocessor array to only unique values.

- `<identifier: variable>`

[Set Preprocessor Variable](Preprocessor.md)
: Sets a preprocessor variable to the value(s) provided.

- `<identifier: variable>` `<any number of object: values>`


### Category: text {id="commands-text"}

Commands which display text to players through format-strings or manipulate text otherwise.
[Show Actionbar](Text-Commands.md#commands)
: Displays an actionbar on the screen of the executing player, or to the given one if specified.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- `[selector: players]` `<string: text>`

[Define/Open Dialogue](Dialogue.md)
: If followed by a block, defines a new dialogue scene with the given name.

- :
	- one of:
		- `new` Creates a new dialogue scene.
			- `<string: scene tag>` `<code block>`
		- `open` Opens a dialogue scene for the given players.
			- `<selector: npc, players>` `[string: scene tag]`
		- `change` Change the dialogue for the given NPC, optionally only for specific players.
			- `<selector: npc>` `<string: scene tag>` `[selector: for players]`

[Show Actionbar to All Players](Text-Commands.md#commands)
: Displays an actionbar on the screen of all players in the game. Can also be used to set the timings of the actionbar.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- `<string: text>`

[Print to All Players](Text-Commands.md#commands)
: Prints a chat message to all players in the game.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- `<string: text>`

[Show Title to All Players](Text-Commands.md#commands)
: Displays a title on the screen of all players in the game. Can also be used to set the timings of the title.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- :
	- one of:
		- `times` Set the timings for the next title/future titles.
			- `<integer: fade in, stay, fade out>`
		- `subtitle` Set the subtitle for the next title displayed.
			- `<string: subtitle text>`
		- `<string: title text>`

[Set Active Language](Localization.md)
: Sets the active lang file (examples: en_US, pt_BR). Once set, all text will automatically be localized into that lang file; including format-strings.

- `<identifier: locale>`

[Print to Player](Text-Commands.md#commands)
: Prints a chat message to the executing player, or to the given one if specified.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- in order:
	- `<selector: entity>`
	- `<string: text>`

Say
: Send a plain-text message as the executing entity. Plain selectors can be used, but not variables.

- `<string: message>`

[Show Title](Text-Commands.md#commands)
: Displays a title on the screen of the executing player, or to the given one if specified. Can also be used to set the timings of the title.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- in order:
	- `<selector: players>`
	- one of:
		- `<integer: fade in, stay, fade out>`
		- `<string: subtitle text>`
		- `<string: title text>`


### Category: entities {id="commands-entities"}

Commands which manipulate, spawn, and transform entities in various ways.
Damage Entity
: Damages the given entities with a certain cause, optionally coming from a position or blaming an entity by a selector, or name of another managed entity (e.g., dummy entities).

- in order:
	- `<selector: targets>` `<integer: amount>`
	- `<damage cause: damage cause>`
	- optional, one of:
		- `<selector: blame>`
		- `<coordinate: from x, from y, from z>`

Give Effect to Entity
: Gives the given entities a potion effect. Time and amplifier can be specified to further customize the potion effect. All potion effects can be cleared using 'effect \<selector\> clear'.

- in order:
	- `<selector: entities>`
	- one of:
		- `clear` Clears all effects from the given entities.
		- `<minecraft effect: effect>` `[integer: seconds, amplifier]` `[true/false: hide particles]`

Face Towards...
: Faces the given entities towards a specific position, selector, or name of another managed entity (e.g., dummy entities).

- in order:
	- `<selector: entities>`
	- one of:
		- `<selector: target entity>`
		- `<coordinate: target x, target y, target z>`

Face Towards...
: Faces the given entities towards a specific position, selector, or name of another managed entity (e.g., dummy entities).

- in order:
	- `<selector: entities>`
	- one of:
		- `<selector: target entity>`
		- `<coordinate: target x, target y, target z>`

Set Gamemode
: Sets the gamemode of the executing player or other players if specified.

- `<gamemode: gamemode>` `[selector: players]`

Kill Entity
: Kills the given entities, causing the death animation, sounds, and particles to appear.

- `[selector: target]`

Move Entity
: Moves the specified entity in a direction (LEFT, RIGHT, UP, DOWN, FORWARD, BACKWARD) for a certain amount. Simpler alternative for teleporting using caret offsets.

- `<selector: entities>` `<move direction: direction>` `<number: amount>` `[true/false: check for blocks]`

Rotate Entity
: Rotates the given entities a certain number of degrees horizontally and vertically from their current rotation.

- `<selector: entities>` `<integer: rotation y, rotation x>`

Summon Entity
: Summons an entity; matches Minecraft vanilla syntax.

- in order:
	- `<minecraft entity: entity type>`
	- optional, one of:
		- `<string: name tag>` `[coordinate: x, y, z]`
		- in order:
			- `<coordinate: x, y, z>`
			- optional, one of:
				- `<coordinate: rotation y, rotation x>`
				- `facing` Spawn the entity facing a particular position or entity.
					- one of:
						- `<selector: face entity>`
						- `<coordinate: face x, face y, face z>`
			- `<string: spawn event>`
			- `<string: name tag>`

Tag Entity
: Add and remove tags from the given entity.

- in order:
	- `<selector: entities>`
	- one of:
		- `add` Add a tag to the given entities.
			- `<string: tag name>`
		- `remove` Remove a tag from the given entities.
			- `<string: tag name>`

Teleport Entity
: Teleports the executing/given entities to a specific position, selector, or name of another managed entity (e.g., dummy entities).

- in order:
	- `<selector: victim>`
	- one of:
		- `<selector: destination>`
		- `<coordinate: x, y, z>`
	- optional, one of:
		- `<coordinate: y rotation, x rotation>`
		- `facing` Set the position/entity the teleported entity will face towards.
			- one of:
				- `<selector: look at entity>`
				- `<coordinate: look at x, look at y, look at z>`
	- `<true/false: check for blocks>`

Teleport Entity
: Teleports the executing/given entities to a specific position, selector, or name of another managed entity (e.g., dummy entities).

- in order:
	- `<selector: victim>`
	- one of:
		- `<selector: destination>`
		- `<coordinate: x, y, z>`
	- optional, one of:
		- `<coordinate: y rotation, x rotation>`
		- `facing` Set the position/entity the teleported entity will face towards.
			- one of:
				- `<selector: look at entity>`
				- `<coordinate: look at x, look at y, look at z>`
	- `<true/false: check for blocks>`


### Category: blocks {id="commands-blocks"}

Commands which interact with the Minecraft world's blocks.
Fill Region
: Fills blocks in a specific region, optionally using a replace mode.

- `<coordinate: x1, y1, z1, x2, y2, z2>` `<minecraft block: block>` `[old handling: fill mode]` `[integer: data]`

Replace in Region
: Replaces all source blocks with a result block in a specific region.

- `<minecraft block: source block>` `[integer: source data]` `<coordinate: x1, y1, z1, x2, y2, z2>` `<minecraft block: result block>` `[integer: result data]`

[Scatter Blocks in Region](Scatter.md)
: Randomly scatters blocks throughout a region with a certain percentage.

- `<minecraft block: block>` `<integer: percent>` `<coordinate: x1, y1, z1, x2, y2, z2>` `[string: seed]`

Set Block
: Sets the block at a specific position, optionally using a replace mode.

- `<coordinate: x, y, z>` `<minecraft block: block>` `[integer: data]` `[old handling: replace mode]`


### Category: items {id="commands-items"}

Commands relating to entity/player items and inventories.
Clear Entity
: Clears the inventories of all given entities, optionally searching for a specific item and limiting the number of items to remove.

- `[selector: target]` `[minecraft item: item]` `[integer: data, max count]`

[Give Item](Giving-Items.md)
: Gives item(s) to the given entity. Runs either a 'give' or 'structure load' depending on requirements. Utilizes builder fields.

- in order:
	- `<selector: players>` `<minecraft item: item>` `[integer: count, data]`
	- optional, repeatable, one of:
		- `keep` Item will stay in the player's inventory even after death.
		- `lockinventory` Lock the item in the player's inventory.
		- `lockslot` Lock the item in the slot it's located in inside the player's inventory.
		- `canplaceon: ` Adds a block that this block can be placed on in adventure mode.
			- `<minecraft block: block>`
		- `candestroy: ` Adds a block that this tool/item can break in adventure mode.
			- `<minecraft block: block>`
		- `enchant: ` Adds an enchantment to the item.
			- `<minecraft enchantment: enchantment>` `<integer: level>`
		- `name: ` Sets the display name of the item.
			- `<string: display name>`
		- `lore: ` Adds a line of lore to the item.
			- `<string: lore line>`
		- `title: ` If the item is a written book, sets the title of the book.
			- `<string: book title>`
		- `author: ` If the item is a written book, sets the author of the book.
			- `<string: book author>`
		- `page: ` If the item is a written book, adds a page of text to the book.
			- `<string: page content>`
		- `dye: ` If the item is leather armor, sets the dye color of the armor.
			- `<integer: red, green, blue>`


### Category: cosmetic {id="commands-cosmetic"}

Commands that add visual and auditory appeal to the user's code.
Camera
: Modify the camera of the given players, identical to the vanilla command with much more fault-tolerance.

- in order:
	- `<selector: players>`
	- one of:
		- `clear` Clears the camera of the given players, setting it back to default.
		- `fade` Fade the camera in and out with a given color.
			- repeatable, one of:
				- `time` Specify the in/hold/out times of the fade, in seconds.
					- `<number: fade in seconds, hold seconds, fade out seconds>`
				- `color` Specify the color of the fade.
					- `<integer: red, green, blue>`
		- `set` Set the camera for the given players.
			- in order:
				- `<camera preset: preset>`
				- repeatable, one of:
					- `default` The default settings for the camera. Not necessary as it can be inferred.
					- `entity_offset` Offset the camera relative to its entity (world space).
						- `<number: offset x, offset y, offset z>`
					- `view_offset` Offset the camera relative to its entity (screen space).
						- `<number: offset x, offset y>`
					- `ease` Causes the camera to smoothly transition from its previous setting.
						- `<number: duration seconds>` `<easing: ease type>`
					- `facing` Rotate the camera to face either an entity, or a position in the world.
						- one of:
							- `<selector: face entity>`
							- `<coordinate: face x, face y, face z>`
					- `pos` Sets the camera's position. Only really relevant when using the 'minecraft:free' preset.
						- `<coordinate: x, y, z>`
					- `rot` Sets the camera's rotation. Only really relevant when using the 'minecraft:free' preset.
						- `<coordinate: x rotation, y rotation>`

Spawn Particle
: Spawns a particle effect in the world.

- `<string: effect>` `[coordinate: x, y, z]`

[Play Sound](Playsound.md)
: Plays a sound effect in the world, optionally with volume, pitch, and filtering specific players.

- `<string: sound>` `[selector: who]` `[coordinate: x, y, z]` `[number: volume, pitch, minimum volume]`


### Category: values {id="commands-values"}

Commands tied directly to values. Values can be used in if-statements, format-strings, and many other places.
[Define Variable](Values.md#defining-values)
: Defines a variable with a name and type, defaulting to int if unspecified. Can be assigned a value directly after defining.
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- in order:
	- `<any number of attribute: attributes>`
	- optional, one of:
		- `int` An integer, representing any whole value between -2147483648 to 2147483647.
		- `decimal` A decimal number with a pre-specified level of precision.
			- `<integer: precision>`
		- `bool` A true or false value.
		- `time` A value representing a number of ticks. Displayed as MM:SS by default.
	- `<any number of attribute: attributes>`
	- `<identifier: name>`
	- `<assignment operator: set>` `<object: default value>`

[Initialize Variable](Values.md#initializing-values)
: Ensures this variable has a value, defaulting to 0 if not. This ensures the executing entity(s) function as intended all the time. Use a clarifier to pick who the variable is initialized for: e.g., `variableName[@a]`
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- `<value: value>`

[Initialize Variable](Values.md#initializing-values)
: Ensures this variable has a value, defaulting to 0 if not. This ensures the executing entity(s) function as intended all the time. Use a clarifier to pick who the variable is initialized for: e.g., `variableName[@a]`
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- `<value: value>`


### Category: logic {id="commands-logic"}

Commands which handle logic and code flow. The butter for all the bread (code).
[Await (async)](Async.md#awaiting)
: Works in async functions. Awaits a certain amount of time, for a condition to be met, or another async function to complete executing.

- :
	- one of:
		- `<integer: ticks>`
		- `until` Wait until a certain condition is met. It will be checked once at the end of every tick.
			- repeatable, in order:
				- one of:
					- `<value: boolean value>`
					- one of:
						- `<value: a>` `<compare: comparison>` `<value: b>`
						- `<value: a>` `<compare: comparison>` `<object: b>`
					- `<selector: self selector>`
					- one of:
						- `<selector: selector>` `<compare: comparison>` `<value: b>`
						- `<selector: selector>` `<compare: comparison>` `<integer: b>`
					- `<selector: selector>`
					- `<coordinate: x, y, z>` `<minecraft block: block>`
					- `<coordinate: region start x, region start y, region start z, region end x, region end y, region end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`
		- `while` Wait as long as a certain condition is met. It will be checked once at the end of every tick.
			- repeatable, in order:
				- one of:
					- `<value: boolean value>`
					- one of:
						- `<value: a>` `<compare: comparison>` `<value: b>`
						- `<value: a>` `<compare: comparison>` `<object: b>`
					- `<selector: self selector>`
					- one of:
						- `<selector: selector>` `<compare: comparison>` `<value: b>`
						- `<selector: selector>` `<compare: comparison>` `<integer: b>`
					- `<selector: selector>`
					- `<coordinate: x, y, z>` `<minecraft block: block>`
					- `<coordinate: region start x, region start y, region start z, region end x, region end y, region end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`
		- `<async function call: awaitable>`

[Else Statement](Comparison.md#else)
: Inverts the comparison given by the previous if-statement at this scope level.

- `[code block]`

Execute
: Begins a vanilla Minecraft execute chain. Can be followed by a statement or code-block, but does not explicitly support the 'run' subcommand.

- in order:
	- repeatable:
		- one of:
			- `align` Aligns the current position of the command to the block grid.
				- `<grid alignment: alignment>`
			- `anchored` Execute at the location of a specific part of the executing entity; The eyes or the feet.
				- `<anchor position: anchor point>`
			- `as` Execute as the given entity/entities.
				- `<selector: entities>`
			- `at` Execute at the position of the given entity.
				- `<selector: entity>`
			- `facing` Execute facing another position/entity.
				- one of:
					- `<coordinate: facing x, facing y, facing z>`
					- `entity` Execute facing another entity.
						- `<selector: entity>`
			- `if` Execute if a certain condition passes.
				- one of:
					- `score` Execute if a value/scoreboard objective matches a condition.
						- in order:
							- `<value: a>`
							- one of:
								- `<compare: comparison>` `<value: b>`
								- `matches` Check if 'a' matches a certain number range.
									- `<range: range>`
					- `entity` Execute if a selector matches.
						- `<selector: pattern>`
					- `block` Execute if a block matches.
						- `<coordinate: x, y, z>` `<minecraft block: block>`
					- `blocks` Execute if two regions of blocks match.
						- `<coordinate: start x, start y, start z, end x, end y, end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`
			- `unless` Execute unless a certain condition passes.
				- one of:
					- `score` Execute unless a value/scoreboard objective matches a condition.
						- in order:
							- `<value: a>`
							- one of:
								- `<compare: comparison>` `<value: b>`
								- `matches` Check if 'a' doesn't match a certain number range.
									- `<range: range>`
					- `entity` Execute unless a selector matches.
						- `<selector: pattern>`
					- `block` Execute unless a block matches.
						- `<coordinate: x, y, z>` `<minecraft block: block>`
					- `blocks` Execute unless two regions of blocks match.
						- `<coordinate: start x, start y, start z, end x, end y, end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`
			- `in` Execute in a specific dimension.
				- `<dimension: dimension>`
			- `positioned` Change the execution position while keeping the current rotation.
				- one of:
					- `<selector: match entity>`
					- `<coordinate: x, y, z>`
			- `rotated` Change the execution rotation while keeping the current position.
				- one of:
					- `<selector: match entity>`
					- `<coordinate: yaw, pitch>`
	- `<code block>`

[For Each Entity](Loops.md#for)
: Runs the following statement or code-block once over every entity that matches a selector at its current position. Functionally equivalent to `execute as <selector> at @s run <code>`

- in order:
	- `<selector: entities>`
	- `at` Offset the execution position per entity.
		- `<coordinate: x, y, z>`
	- `<code block>`

[Define Function](Functions.md#defining-functions)
: Defines a function. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: `functionName(parameters)`
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- in order:
	- `<any number of attribute: attributes>`
	- `<identifier: function name>`
	- `<any number of attribute: attributes>`
	- `<open parenthesis: open parenthesis>`
	- optional, repeatable:
		- in order:
			- `<any number of attribute: attributes>`
			- optional, one of:
				- `<integer: precision>`
			- `<any number of attribute: attributes>`
			- `<identifier: name>`
			- `<assignment operator: set>` `<object: default value>`
	- `<close parenthesis: close parenthesis>`
	- `<code block>`

[Define Function](Functions.md#defining-functions)
: Defines a function. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: `functionName(parameters)`
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- in order:
	- `<any number of attribute: attributes>`
	- `<identifier: function name>`
	- `<any number of attribute: attributes>`
	- `<open parenthesis: open parenthesis>`
	- optional, repeatable:
		- in order:
			- `<any number of attribute: attributes>`
			- optional, one of:
				- `<integer: precision>`
			- `<any number of attribute: attributes>`
			- `<identifier: name>`
			- `<assignment operator: set>` `<object: default value>`
	- `<close parenthesis: close parenthesis>`
	- `<code block>`

[Halt Execution](Debugging.md#halting-code)
: Ends the execution of the code entirely by hitting the function command limit.


[If Statement](Comparison.md)
: Performs a comparison, only running the proceeding statement/code-block if the comparisons(s) are true. Multiple comparisons can be chained using the keyword 'and', and comparisons can be inverted using the keyword 'not'

- in order:
	- repeatable, in order:
		- `not` Invert the next comparison.
		- one of:
			- `<value: boolean value>`
			- one of:
				- `<value: a>` `<compare: comparison>` `<value: b>`
				- `<value: a>` `<compare: comparison>` `<object: b>`
			- `<selector: self selector>`
			- `count` Count the number of matching entities and compare the result.
				- one of:
					- `<selector: selector>` `<compare: comparison>` `<value: b>`
					- `<selector: selector>` `<compare: comparison>` `<integer: b>`
			- `any` Check if any entities match the given selector.
				- `<selector: selector>`
			- `block` Check if a block matches a given filter.
				- `<coordinate: x, y, z>` `<minecraft block: block>`
			- `blocks` Check if two regions of blocks are identical.
				- `<coordinate: region start x, region start y, region start z, region end x, region end y, region end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`
		- `and` Add another comparison.
	- `<code block>`

Repeat N Times
: Repeats the proceeding statement/code-block the given number of times. This command always runs at runtime.

- in order:
	- one of:
		- `<integer: repetitions>`
		- `<value: repetitions>`
	- `<identifier: current iteration value>`
	- `<code block>`

[Set Return Value](Functions.md#return-values)
: Set the value that will be returned from this function when it ends. The caller can use this value however it wishes.

- :
	- one of:
		- `<value: return value>`
		- `<object: return value>`

While Statement
: Repeats the proceeding statement/code-block as long as a condition remains true.  Multiple comparisons can be chained using the keyword 'and', and comparisons can be inverted using the keyword 'not'

- in order:
	- repeatable, in order:
		- one of:
			- `<value: boolean value>`
			- one of:
				- `<value: a>` `<compare: comparison>` `<value: b>`
				- `<value: a>` `<compare: comparison>` `<object: b>`
			- `<selector: self selector>`
			- one of:
				- `<selector: selector>` `<compare: comparison>` `<value: b>`
				- `<selector: selector>` `<compare: comparison>` `<integer: b>`
			- `<selector: selector>`
			- `<coordinate: x, y, z>` `<minecraft block: block>`
			- `<coordinate: region start x, region start y, region start z, region end x, region end y, region end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`
	- `<code block>`


### Category: debug {id="commands-debug"}

Commands related to testing, debugging, and all-around solidifying code.
[Preprocessor Assertion](Debugging.md#assertions)
: Asserts that the input comparison is true, and throws a compiler error if not. Allows a custom error message.

- in order:
	- one of:
		- `<object: a>`
		- `<preprocessor variable: a>`
	- `<compare: comparison>`
	- one of:
		- `<object: a>`
		- `<preprocessor variable: a>`
	- `<string: message>`

[Assert Statement](Testing.md#writing-a-test)
: Asserts that the given condition evaluates to true, at runtime. If the condition evaluates to false, the code is halted and info is displayed to the executing player(s).

- repeatable, in order:
	- one of:
		- `<value: boolean value>`
		- one of:
			- `<value: a>` `<compare: comparison>` `<value: b>`
			- `<value: a>` `<compare: comparison>` `<object: b>`
		- `<selector: self selector>`
		- one of:
			- `<selector: selector>` `<compare: comparison>` `<value: b>`
			- `<selector: selector>` `<compare: comparison>` `<integer: b>`
		- `<selector: selector>`
		- `<coordinate: x, y, z>` `<minecraft block: block>`
		- `<coordinate: region start x, region start y, region start z, region end x, region end y, region end z, destination x, destination y, destination z>` `<blocks scan mode: scan mode>`

[Define Test](Testing.md#writing-a-test)
: Defines a test; requires 'tests' feature. Must be followed by a code-block that contains the test contents.
<format color="MediumSeaGreen">Can be documented by writing a comment right before running this command.</format>

- `<identifier: name>` `<code block>`

[Throw Error](Debugging.md#throwing-errors)
: Throws an error, displaying it to the executing player(s). The code is halted immediately, so handle cleanup before calling throw.
<format color="CadetBlue">Supports [<format color="CadetBlue">format-strings.</format>](Text-Commands.md#format-strings)</format>

- `<string: error message>`


### Category: features {id="commands-features"}

Commands related to the optionally enable-able features in the language.
[Manage Dummy Entities](Optional-Features.md#dummies)
: Create a dummy entity, remove the selected ones, or manage the classes on the selected ones. Requires feature 'DUMMIES' to be enabled.

- :
	- one of:
		- `create` Create a new dummy entity.
			- `<string: name>` `[string: tag]` `[coordinate: x, y, z]`
		- `single` Create a new dummy entity, removing any others with a matching name/tag.
			- `<string: name>` `[string: tag]` `[coordinate: x, y, z]`
		- `removeall` Remove all dummies in the world, or only ones with a specific tag.
			- `[string: tag]`
		- `remove` Remove all dummies with the given name, and optionally tag.
			- `<string: name>` `[string: tag]`

[Create Explosion](Optional-Features.md#exploders)
: Create an explosion at a specific position with optional positioning, power, delay, fire, and block breaking settings. Requires feature 'EXPLODERS' to be enabled.

- `[coordinate: x, y, z]` `[integer: power, delay]` `[true/false: causes fire, breaks blocks]`

[Enable Feature](Optional-Features.md)
: Enables a feature to be used for this project, generating any of the necessary files.

- `<feature: feature>`


### Category: other {id="commands-other"}

The other commands that don't have a good designation.
Minecraft Command
: Places a plain command in the output file, used for when the language lacks a certain feature.

- `<string: command>`

Minecraft Command
: Places a plain command in the output file, used for when the language lacks a certain feature.

- `<string: command>`

Minecraft Command
: Places a plain command in the output file, used for when the language lacks a certain feature.

- `<string: command>`


