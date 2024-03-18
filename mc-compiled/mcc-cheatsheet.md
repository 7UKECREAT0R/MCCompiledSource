# Cheat Sheet

## Comments
`// <text>` - Line comment. Must be at the start of the line and extends to the very end.<br />
`/* <text> */` - Multiline comment. Only ends when specified, not at the end of the line.

## Code Block
Starts and ends with brackets, holding code inside:
```%lang%
...
{
    // inside code block
}
```

---

## Types
Descriptions of the upcoming types that will be present in the various command arguments.

id
: An identifier that either has meaning or doesn't. An identifier can be the name of anything defined in the language, and is usually context dependent.
int
: Any integral number, like 5, 10, 5291, or -40. Use time suffixes to scale the integer accordingly, like with 4s -> 80.

string
: A block of text on a single line, surrounded with either 'single quotes' or "double quotes."

bool
: A value that can be either 'true' or 'false.'

selector
: A Minecraft selector that targets a specific entity or set of entities. Example: `@e[type=cow]`

value
: The name of a runtime value that was defined using the `define` command.

coordinate
: A Minecraft coordinate value that can optionally be both relative and facing offset, like ~10, 40, or ^5.

enum
: Usually a specific keyword in a subset of possible keywords. This type is entirely context dependent.

range
: A Minecraft number that specifies a range of integers (inclusive). Omitting a number from one side makes the number unbounded. 4.. means four and up. 1..5 means one through five.

json
: A JSON object achieved by $dereferencing a preprocessor variable holding one.

## Commands
All the commands in the language (version 1.17). The command ID is the first word of the line, followed by the arguments it gives. Each command parameter includes the type it's allowed to be and its name. A required parameter is surrounded in `<angle brackets>`, and an optional parameter is surrounded in `[square brackets]`.


### Category: preprocessor
Commands that allow the user to do things at compile time. Preprocessor commands generally start with a `$` and are highlighted differently than regular commands.

[Add to Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Adds two preprocessor variables/values together, changing only the first one. A += B
- `$add <id: variable> <object: values>`

[Append to Preprocessor Variable](Advanced-Variable-Commands.md#array-specific-manipulation)
: Adds the given item(s) to the end of the given preprocessor variable, or contents of another preprocessor variable if specified.
- `$append <id: to modify> <object: items>`
- `$append <id: to modify> <id: other>`

[Decrement Preprocessor Variable](Simple-Variable-Commands.md#inc-dec)
: Decrements the given preprocessor variable by one. If multiple values are held, they are all decremented.
- `$dec <id: variable>`

[Define/Call Macro](Macros.md#defining-a-macro)
: If a code-block follows this call, it is treated as a definition. Arguments are passed in as preprocessor variables. If no code-block follows this call, it will attempt to run the macro with any inputs parameters copied to their respective preprocessor variables.
- `$macro <id: name>`

[Divide Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Divides two preprocessor variables/values from each other, changing only the first one. A /= B
- `$div <id: variable> <object: values>`

[Exponentiate Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Exponentiates two preprocessor variables/values with each other, changing only the first one. A = A^B
- `$pow <id: variable> <object: values>`

[Include File](Including-Other-Files.md)
: Places the contents of the given file in replacement for this statement. Not intended for production use yet.
- `$include <string: file>`

[Increment Preprocessor Variable](Simple-Variable-Commands.md#inc-dec)
: Increments the given preprocessor variable by one. If multiple values are held, they are all incremented.
- `$inc <id: variable>`

[Log to Console](Debugging.md#logging)
: Sends a message to stdout with a line terminator at the end.
- `$log <*: message>`

[Modulo Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Divides two preprocessor variables/values from each other, setting only the first one to the remainder of the operation. A %= B
- `$mod <id: variable> <object: values>`

[Multiply with Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Multiplies two preprocessor variables/values together, changing only the first one. A *= B
- `$mul <id: variable> <object: values>`

[Prepend to Preprocessor Variable](Advanced-Variable-Commands.md#array-specific-manipulation)
: Adds the given item(s) to the start of the given preprocessor variable.
- `$prepend <id: to modify> <object: items>`
- `$prepend <id: to modify> <id: other>`

[Preprocessor Array Mean](Advanced-Variable-Commands.md#data-manipulation)
: Averages all values in the given preprocessor variable together into one value and stores it in a result variable.
- `$mean <id: result> [id: variable]`

[Preprocessor Array Median](Advanced-Variable-Commands.md#data-manipulation)
: Gets the middle value/average of the two middle values and stores it in a result variable.
- `$median <id: result> [id: variable]`

[Preprocessor Array Sort](Advanced-Variable-Commands.md#array-specific-manipulation)
: Sorts the order of the values in the given preprocessor variable either 'ascending' or 'descending'. Values must be comparable.
- `$sort <id: ascending or descending> <id: variable>`

[Preprocessor Array Sum](Advanced-Variable-Commands.md#data-manipulation)
: Adds all values in the given preprocessor variable together into one value and stores it in a result variable.
- `$sum <id: result> [id: variable]`

[Preprocessor Array Unique](Advanced-Variable-Commands.md#array-specific-manipulation)
: Flattens the given preprocessor array to only unique values.
- `$unique <id: variable>`

[Preprocessor Call Function](Metaprogramming.md#calling)
: Calls a function by name and passes in the given parameters. Because this is a preprocessor operation, it has the same error handling as a normal function call.
- `$call <string: function name> [*: parameters]`

[Preprocessor Else](Comparison-compile-time.md#using-else)
: Directly inverts the result of the last $if call at this level in scope.

[Preprocessor If](Comparison-compile-time.md#using-if)
: Compares a preprocessor variable and another value/variable. If the source variable contains multiple values, they all must match the condition.
- `$if <object: a> <compare: comparison> <object: b>`
- `$if <id: a> <compare: comparison> <object: b>`
- `$if <object: a> <compare: comparison> <id: b>`
- `$if <id: a> <compare: comparison> <id: b>`

[Preprocessor Length](Advanced-Variable-Commands.md#data-manipulation)
: If a preprocessor variable identifier or JSON array is specified, the number of elements it holds is gotten. If a string is given, its length is gotten.
- `$len <id: result> <id: variable>`
- `$len <id: result> <json: array>`
- `$len <id: result> <string: text>`

[Preprocessor Load JSON Value](JSON-Processing.md)
: Load a JSON file (if not previously loaded) and retrieve a value from it, storing said value in a preprocessor variable.
- `$json <string: file name> <id: result> [string: path]`
- `$json <json: existing json> <id: result> [string: path]`

[Preprocessor Repeat](Compile-Time-Repeating-and-Iteration.md#repeat_number)
: Repeats the following statement/code-block a number of times. If a variable identifier is given, that variable will be set to the index of the current iteration. 0, 1, 2, etc.
- `$repeat <int: amount> [id: indicator]`
- `$repeat <range: amount> [id: indicator]`

[Preprocessor Reverse](Advanced-Variable-Commands.md#array-specific-manipulation)
: Reverses the order of the values in the given preprocessor variable.
- `$reverse <id: variable>`

[Preprocessor String Friendly Name](Advanced-Variable-Commands.md#string-manipulation)
: Convert the given preprocessor variable value(s) to a string in 'Title Case'.
- `$strfriendly <id: result> [id: variable]`

[Preprocessor String Lowercase](Advanced-Variable-Commands.md#string-manipulation)
: Convert the given preprocessor variable value(s) to a string in 'lowercase'.
- `$strlower <id: result> [id: variable]`

[Preprocessor String Uppercase](Advanced-Variable-Commands.md#string-manipulation)
: Convert the given preprocessor variable value(s) to a string in 'UPPERCASE'.
- `$strupper <id: result> [id: variable]`

[Set Preprocessor Variable](Preprocessor.md)
: Sets a preprocessor variable to the value(s) provided.
- `$var <id: variable> <object: values>`

[Subtract from Preprocessor Variable](Simple-Variable-Commands.md#all-of-them)
: Subtracts two preprocessor variables/values from each other, changing only the first one. A -= B
- `$sub <id: variable> <object: values>`

[Swap Preprocessor Variables](Simple-Variable-Commands.md#other-variable-operations)
: Swaps the values of two preprocessor variables
- `$swap <id: a> <id: b>`

Iterate Preprocessor Array
: Runs the following statement/code-block once for each value in the given preprocessor variable. The current iteration is held in the preprocessor variable given. If the target is a JSON array, the elements will be iterated upon.
- `$iterate <id: variable> <id: current>`
- `$iterate <json: array or object> <id: current>`


### Category: text
Commands which display text to players through format-strings, or manipulate text otherwise.

[Define/Open Dialogue](Dialogue.md)
: If followed by a block, defines a new dialogue scene with the given name.
- `dialogue <id: new> <string: scene tag>`
- `dialogue <id: open> <selector: npc> <selector: player> [string: scene tag]`
- `dialogue <id: change> <selector: npc> <string: scene tag> [selector: player]`

[Print to All Players](Text-Commands.md#commands)
: Prints a chat message to all players in the game. Supports format-strings.
- `globalprint <string: text>`

[Print to Player](Text-Commands.md#commands)
: Prints a chat message to the executing player, or to the given one if specified. Supports format-strings.
- `print <selector: entity> <string: text>`
- `print <string: text>`

[Set Active Language](Localization.md)
: Sets the active lang file (examples: en_US, pt_BR). Once set, all text will automatically be localized into that lang file; including format-strings.
- `lang <id: locale>`

[Show Actionbar to All Players](Text-Commands.md#commands)
: Displays an actionbar on the screen of all players in the game. Can also be used to set the timings of the actionbar. Supports format-strings.
- `globalactionbar <string: text>`

[Show Actionbar](Text-Commands.md#commands)
: Displays an actionbar on the screen of the executing player, or to the given one if specified. Supports format-strings.
- `actionbar <string: text>`

[Show Title to All Players](Text-Commands.md#commands)
: Displays a title on the screen of all players in the game. Can also be used to set the timings of the title. Supports format-strings.
- `globaltitle <id: times> <int: fade in> <int: stay> <int: fade out>`
- `globaltitle <id: subtitle> <string: text>`
- `globaltitle <string: text>`

[Show Title](Text-Commands.md#commands)
: Displays a title on the screen of the executing player, or to the given one if specified. Can also be used to set the timings of the title. Supports format-strings.
- `title <selector: target> <id: times> <int: fade in> <int: stay> <int: fade out>`
- `title <selector: target> <id: subtitle> <string: text>`
- `title <selector: target> <string: text>`
- `title <id: times> <int: fade in> <int: stay> <int: fade out>`
- `title <id: subtitle> <string: text>`
- `title <string: text>`

Say
: Send a plain-text message as the executing entity. Plain selectors can be used, but not variables.
- `say <string: message>`


### Category: entities
Commands which manipulate, spawn, and transform entities in various ways.

Damage Entity
: Damages the given entities with a certain cause, optionally coming from a position or blaming an entity by a selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).
- `damage <selector: target> <int: amount> [enum: damage cause]`
- `damage <selector: target> <int: amount> [enum: damage cause] <selector: blame>`
- `damage <selector: target> <int: amount> [enum: damage cause] <string: blame>`
- `damage <selector: target> <int: amount> [enum: damage cause] <coordinate: from x> <coordinate: from y> <coordinate:  from z>`

Face Towards...
: Faces the given entities towards a specific position, selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).
- `face <selector: source> <coordinate: x> <coordinate: y> <coordinate: z>`
- `face <selector: source> <selector: other>`
- `face <selector: source> <string: other>`

Give Effect to Entity
: Gives the given entities a potion effect. Time and amplifier can be specified to further customize the potion effect. All potion effects can be cleared using 'effect \<selector\> clear'.
- `effect <selector: target> <id: clear>`
- `effect <selector: target> <enum: effect> [int: seconds] [int: amplifier] [bool: hide]`

Kill Entity
: Kills the given entities, causing the death animation, sounds, and particles to appear.
- `kill [selector: target]`

Move Entity
: Moves the specified entity in a direction (LEFT, RIGHT, UP, DOWN, FORWARD, BACKWARD) for a certain amount. Simpler alternative for teleporting using caret offsets.
- `move <selector: source> <id: direction> <number: amount> [bool: check for blocks]`

Remove Entity
: Teleports the given entities deep into the void, causing a silent death. Looking to rewrite this in the future to generate entity code for real removal.
- `remove [selector: target]`

Rotate Entity
: Rotates the given entities a certain number of degrees horizontally and vertically from their current rotation.
- `rotate <selector: source> <int: y> <int: x>`

Tag Entity
: Add and remove tags from the given entity.
- `tag <selector: target> <id: mode> <string: name>`

Teleport Entity
: Teleports the executing/given entities to a specific position, selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).
- `tp <coordinate: x> <coordinate: y> <coordinate: z> [id: facing] [selector: face entity] [bool: check for blocks]`
- `tp <coordinate: x> <coordinate: y> <coordinate: z> [id: facing] [coordinate: facing x] [coordinate: facing y] [coordinate: facing z] [bool: check for blocks]`
- `tp <selector: source> <coordinate: x> <coordinate: y> <coordinate: z> [id: facing] [selector: face entity] [bool: check for blocks]`
- `tp <selector: source> <coordinate: x> <coordinate: y> <coordinate: z> [id: facing] [coordinate: facing x] [coordinate: facing y] [coordinate: facing z] [bool: check for blocks]`
- `tp <selector: source> <selector: other> [id: facing] [selector: face entity] [bool: check for blocks]`
- `tp <selector: source> <selector: other> [id: facing] [coordinate: facing x] [coordinate: facing y] [coordinate: facing z] [bool: check for blocks]`
- `tp <selector: source> <string: other> [id: facing] [selector: face entity] [bool: check for blocks]`
- `tp <selector: source> <string: other> [id: facing] [coordinate: facing x] [coordinate: facing y] [coordinate: facing z] [bool: check for blocks]`


### Category: blocks
Commands which interact with the Minecraft world's blocks.

[Scatter Blocks in Region](Scatter.md)
: Randomly scatters blocks throughout a region with a certain percentage.
- `scatter <string: block> <int: percent> <coordinate: x1> <coordinate: y1> <coordinate: z1> <coordinate: x2> <coordinate: y2> <coordinate: z2> [string: seed]`

Fill Region
: Fills blocks in a specific region, optionally using a replace mode.
- `fill <coordinate: x1> <coordinate: y1> <coordinate: z1> <coordinate: x2> <coordinate: y2> <coordinate: z2> <string: block> [enum: fill mode] [int: data]`

Replace in Region
: Replaces all source blocks with a result block in a specific region.
- `replace <string: source block> [int: data] <coordinate: x1> <coordinate: y1> <coordinate: z1> <coordinate: x2> <coordinate: y2> <coordinate: z2> <string: result block> [int: data]`

Set Block
: Sets the block at a specific position, optionally using a replace mode.
- `setblock <coordinate: x> <coordinate: y> <coordinate: z> <string: block> [int: data] [id: replace mode]`


### Category: items
Commands relating to entity/player items and inventories.

[Give Item](Giving-Items.md)
: Gives item(s) to the given entity. Runs either a 'give' or 'structure load' depending on requirements. Utilizes builder fields.
- `give <selector: entity> <string: item> [int: amount] [int: data]`

Clear Entity
: Clears the inventories of all given entities, optionally searching for a specific item and limiting the number of items to remove.
- `clear [selector: target] [string: item] [int: data] [int: max count]`


### Category: cosmetic
Commands that add visual and auditory appeal to the user's code.

[Play Sound](Playsound.md)
: Plays a sound effect in the world, optionally with volume, pitch, and filtering specific players.
- `playsound <string: sound> <selector: who> [coordinate: x] [coordinate: y] [coordinate: z] [number: volume] [number: pitch] [number: minimum volume]`

Spawn Particle
: Spawns a particle effect in the world.
- `particle <string: effect> [coordinate: x] [coordinate: y] [coordinate: z]`


### Category: values
Commands tied directly to values. Values can be used in if-statements, format-strings, and many other places.

[Define Variable](Values.md#defining-values)
: Defines a variable with a name and type, defaulting to int if unspecified. Can be assigned a value directly after defining.
- `define <*: args>`

[Initialize Variable](Values.md#initializing-values)
: Ensures this variable has a value, defaulting to 0 if not. This ensures the executing entity(s) function as intended all the time. Use clarifiers to pick who the variable is initialized for: e.g., `variableName[@a]`
- `init <value: value>`


### Category: logic
Commands which handle logic and code flow. The butter for all the bread (code).

[Define Function](Functions.md#defining-functions)
: Defines a function. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: `functionName(parameters)`
- `function <*: args>`

[Else Statement](Comparison.md#else)
: Inverts the comparison given by the previous if-statement at this scope level.

[Halt Execution](Debugging.md#halting-code)
: Ends the execution of the code entirely by hitting the function command limit.

[If Statement](Comparison.md)
: Performs a comparison, only running the proceeding statement/code-block if the comparisons(s) are true. Multiple comparisons can be chained using the keyword 'and', and comparisons can be inverted using the keyword 'not'

[Set Return Value](Functions.md#return-values)
: Set the value that will be returned from this function when it ends. The caller can use this value however it wishes.
- `return <value: variable>`
- `return <any: return value>`

Execute
: Begins a vanilla minecraft 1.19.70+ execute chain. Can be followed by a statement or code-block, but does not explicitly support the 'run' subcommand.
- `execute <id: subcommand> <*: subcommand arguments>`

For Each Entity
: Runs the following statement or code-block once over every entity that matches a selector at its current position. Functionally equivalent to `execute as <selector> at @s run <code>`
- `for <selector: entities> <id: at> <coordinate: x> <coordinate: y> <coordinate: z>`
- `for <selector: entities>`

Repeat N Times
: Repeats the proceeding statement/code-block the given number of times. This command always runs at runtime.
- `repeat <int: repetitions> [id: current]`
- `repeat <value: repetitions> [id: current]`

While Statement
: Repeats the proceeding statement/code-block as long as a condition remains true.  Multiple comparisons can be chained using the keyword 'and', and comparisons can be inverted using the keyword 'not'


### Category: debug
Commands related to testing, debugging and all-around solidifying code.

[Assert Statement](Testing.md#writing-a-test)
: Asserts that the given condition evaluates to true, at runtime. If the condition evaluates to false, the code is halted and info is displayed to the executing player(s).

[Define Test](Testing.md#writing-a-test)
: Defines a test; requires 'tests' feature. Must be followed by a code-block that contains the test contents.
- `test <id: test name>`

[Preprocessor Assertion](Debugging.md#assertions)
: Asserts that the input comparison is true, and throws a compiler error if not.
- `$assert <object: a> <compare: comparison> <object: b>`
- `$assert <id: a> <compare: comparison> <object: b>`
- `$assert <object: a> <compare: comparison> <id: b>`
- `$assert <id: a> <compare: comparison> <id: b>`

[Throw Error](Debugging.md#throwing-errors)
: Throws an error, displaying it to the executing player(s). The code is halted immediately, so handle cleanup before calling throw. Supports format-strings.
- `throw <string: error>`


### Category: features
Commands related to the optionally enable-able features in the language.

[Create Explosion](Optional-Features.md#exploders)
: Create an explosion at a specific position with optional positioning, power, delay, fire, and block breaking settings. Requires feature 'EXPLODERS' to be enabled.
- `explode [coordinate: x] [coordinate: y] [coordinate: z] [int: power] [int: delay] [bool: causes fire] [bool: breaks blocks]`

[Enable Feature](Optional-Features.md)
: Enables a feature to be used for this project, generating any of the necessary files.
- `feature <id: feature name>`

[Manage Dummy Entities](Optional-Features.md#dummies)
: Create a dummy entity, remove the selected ones, or manage the classes on the selected ones. Requires feature 'DUMMIES' to be enabled.
- `dummy <id: create> <string: name> [string: tag] [coordinate: x] [coordinate: y] [coordinate: x]`
- `dummy <id: single> <string: name> [string: tag] [coordinate: x] [coordinate: y] [coordinate: x]`
- `dummy <id: removeall> [string: tag]`
- `dummy <id: remove> <string: name> [string: tag]`


### Category: other
The other commands that don't have a good designation.

Minecraft Command
: Places a plain command in the output file, used for when the language lacks a certain feature.
- `mc <string: command>`


