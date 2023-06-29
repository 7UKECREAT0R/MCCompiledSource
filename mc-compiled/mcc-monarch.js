const mccompiled = {
	operators: [ `<`, `>`, `{`, `}`, `=`, `(`, `)`, `+`, `-`, `*`, `/`, `%`, `!` ],
	selectors: [ `@e`, `@a`, `@s`, `@p`, `@i`, `@initiator` ],
	preprocessor: [ `$var`, `$inc`, `$dec`, `$add`, `$sub`, `$mul`, `$div`, `$mod`, `$pow`, `$swap`, `$if`, `$else`, `$repeat`, `$log`, `$macro`, `$include`, `$strfriendly`, `$strupper`, `$strlower`, `$sum`, `$median`, `$mean`, `$sort`, `$reverse`, `$iterate`, `$len`, `$json`, `$call` ],
	commands: [ `mc`, `command`, `cmd`, `globalprint`, `print`, `lang`, `define`, `init`, `initialize`, `if`, `else`, `give`, `tp`, `teleport`, `move`, `face`, `lookat`, `rotate`, `setblock`, `fill`, `scatter`, `replace`, `kill`, `remove`, `clear`, `globaltitle`, `title`, `globalactionbar`, `actionbar`, `say`, `halt`, `damage`, `effect`, `playsound`, `particle`, `dummy`, `tag`, `explode`, `feature`, `function`, `fn`, `return`, `for`, `execute` ],
	literals: [ `true`, `false`, `not`, `and`, `null`, `~`, `^` ],
	types: [ `int`, `decimal`, `bool`, `time`, `struct`, `ppv`, `global`, `extern`, `bind` ],
	comparisons: [ `count`, `any`, `block`, `blocks`, `positioned` ],
	options: [ `dummies`, `gametest`, `exploders`, `uninstall`, `up`, `down`, `left`, `right`, `forward`, `backward`, `ascending`, `descending`, `survival`, `creative`, `adventure`, `spectator`, `times`, `subtitle`, `destroy`, `replace`, `hollow`, `outline`, `keep`, `lockinventory`, `lockslot`, `canplaceon:`, `candestroy:`, `enchant:`, `name:`, `lore:`, `author:`, `title:`, `page:`, `dye:`, `align`, `anchored`, `as`, `at`, `facing`, `facing entity`, `in`, `positioned`, `positioned as`, `rotated`, `rotated as` ],
    tokenizer: {
        root: [
            [ /@?[a-zA-Z$][\w]*/, {
                cases: {
                    '@selectors': 'selectors',
                    '@preprocessor': 'preprocessor',
                    '@commands': 'commands',
                    '@literals': 'literals',
                    '@types': 'types',
                    '@comparisons': 'comparisons',
                    '@options': 'options'
                }
            }],
			
			{ include: '@handler' },
			
			[ /[<>{}=()+\-*/%!]+/, 'operators' ],
            [ /"(?:[^"\\]|\\.)*"/, 'strings' ],
            [ /'(?:[^'\\]|\\.)*'/, 'strings' ],
            [ /\[.+\]/, 'selectors.properties' ],
            [ /!?(?:\.\.)?\d+(?:\.\.)?\.?\d*[hms]?/, 'numbers' ]
        ],
		comment: [
            [/[^\/*]+/, 'comment' ],
			[/\/\*/, 'comment', '@push' ],
			["\\*/", 'comment', '@pop'  ],
			[/[\/*]/, 'comment' ]
		],
		handler: [
			[/[ \t\r\n]+/, 'white' ],
			[/\/\*/, 'comment', '@comment' ],
			[/\/\/.*$/, 'comment' ],
		]
    }
}
const mcc_operators = [
	{
		word: `<`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `>`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `{`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `}`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `=`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `(`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `)`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `+`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `-`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `*`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `/`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `%`,
		docs: 'No documentation available for v1.14.'
	},
	{
		word: `!`,
		docs: 'No documentation available for v1.14.'
	},
]
const mcc_selectors = [
	{
		word: `@e`,
		docs: `References all entities in the world.`
	},
	{
		word: `@a`,
		docs: `References all players in the world.`
	},
	{
		word: `@s`,
		docs: `References the executing entity/player.`
	},
	{
		word: `@p`,
		docs: `References the nearest player.`
	},
	{
		word: `@i`,
		docs: `References the initiator, if this was run from dialogue.`
	},
	{
		word: `@initiator`,
		docs: `References the initiator, if this was run from dialogue.`
	},
]
const mcc_preprocessor = [
	{
		word: `$var`,
		docs: `Sets a preprocessor variable to the value(s) provided.`
	},
	{
		word: `$inc`,
		docs: `Increments the given preprocessor variable by one. If multiple values are held, they are all incremented.`
	},
	{
		word: `$dec`,
		docs: `Decrements the given preprocessor variable by one. If multiple values are held, they are all decremented.`
	},
	{
		word: `$add`,
		docs: `Adds two preprocessor variables/values together, changing only the first one. A += B`
	},
	{
		word: `$sub`,
		docs: `Subtracts two preprocessor variables/values from each other, changing only the first one. A -= B`
	},
	{
		word: `$mul`,
		docs: `Multiplies two preprocessor variables/values together, changing only the first one. A *= B`
	},
	{
		word: `$div`,
		docs: `Divides two preprocessor variables/values from each other, changing only the first one. A /= B`
	},
	{
		word: `$mod`,
		docs: `Divides two preprocessor variables/values from each other, setting only the first one to the remainder of the operation. A %= B`
	},
	{
		word: `$pow`,
		docs: `Exponentiates two preprocessor variables/values with each other, changing only the first one. A = A^B`
	},
	{
		word: `$swap`,
		docs: `Swaps the values of two preprocessor variables`
	},
	{
		word: `$if`,
		docs: `Compares a preprocessor variable and another value/variable. If the source variable contains multiple values, they all must match the condition.`
	},
	{
		word: `$else`,
		docs: `Directly inverts the result of the last $if call at this level in scope.`
	},
	{
		word: `$repeat`,
		docs: `Repeats the following statement/code-block a number of times. If a variable identifier is given, that variable will be set to the index of the current iteration. 0, 1, 2, etc.`
	},
	{
		word: `$log`,
		docs: `Sends a message to stdout with a line terminator at the end.`
	},
	{
		word: `$macro`,
		docs: `If a code-block follows this call, it is treated as a definition. Arguments are passed in as preprocessor variables. If no code-block follows this call, it will attempt to run the macro with any inputs parameters copied to their respective preprocessor variables.`
	},
	{
		word: `$include`,
		docs: `Places the contents of the given file in replacement for this statement. Not intended for production use yet.`
	},
	{
		word: `$strfriendly`,
		docs: `Convert the given preprocessor variable value(s) to a string in 'Title Case'.`
	},
	{
		word: `$strupper`,
		docs: `Convert the given preprocessor variable value(s) to a string in 'UPPERCASE'.`
	},
	{
		word: `$strlower`,
		docs: `Convert the given preprocessor variable value(s) to a string in 'lowercase'.`
	},
	{
		word: `$sum`,
		docs: `Adds all values in the given preprocessor variable together into one value and stores it in a result variable.`
	},
	{
		word: `$median`,
		docs: `Gets the middle value/average of the two middle values and stores it in a result variable.`
	},
	{
		word: `$mean`,
		docs: `Averages all values in the given preprocessor variable together into one value and stores it in a result variable.`
	},
	{
		word: `$sort`,
		docs: `Sorts the order of the values in the given preprocessor variable either 'ascending' or 'descending'. Values must be comparable.`
	},
	{
		word: `$reverse`,
		docs: `Reverses the order of the values in the given preprocessor variable.`
	},
	{
		word: `$iterate`,
		docs: `Runs the following statement/code-block once for each value in the given preprocessor variable. The current iteration is held in the preprocessor variable given. If the target is a JSON array, the elements will be iterated upon.`
	},
	{
		word: `$len`,
		docs: `If a preprocessor variable ID is given, the number of elements it holds is gotten. If a JSON array is given, the number of elements is gotten. If a string is given, the number of characters is gotten.`
	},
	{
		word: `$json`,
		docs: `Load a JSON file (if not previously loaded) and retrieve a value from it, storing said value in a preprocessor variable.`
	},
	{
		word: `$call`,
		docs: `Calls a function by name and passes in the given parameters. Because this is a preprocessor operation, it has the same error handling as a normal function call.`
	},
]
const mcc_commands = [
	{
		word: `mc`,
		docs: `Places a plain command in the output file, used for when the language lacks a certain feature.`
	},
	{
		word: `command`,
		docs: `Alias of 'mc'. Places a plain command in the output file, used for when the language lacks a certain feature.`
	},
	{
		word: `cmd`,
		docs: `Alias of 'mc'. Places a plain command in the output file, used for when the language lacks a certain feature.`
	},
	{
		word: `globalprint`,
		docs: `Prints a chat message to all players in the game. Supports format strings.`
	},
	{
		word: `print`,
		docs: `Prints a chat message to the executing player, or to the given one if specified. Supports format strings.`
	},
	{
		word: `lang`,
		docs: `Sets the active lang file (examples: en_US, pt_BR). Once set, all text will automatically be localized into that lang file; including FStrings.`
	},
	{
		word: `define`,
		docs: `Defines a variable with a name and type, defaulting to int if unspecified. Can be assigned a value directly after defining.`
	},
	{
		word: `init`,
		docs: `Ensures the given entities have a value for the given variable, defaulting to 0 if not. This ensures the given entities function as intended all the time.`
	},
	{
		word: `initialize`,
		docs: `Alias of 'init'. Ensures the given entities have a value for the given variable, defaulting to 0 if not. This ensures the given entities function as intended all the time.`
	},
	{
		word: `if`,
		docs: `Allows comparison of variables, along with a huge collection of other criteria. Can be chained together by the keyword 'and' and inverted by the keyword 'not'. Only runs the proceeding statement/code-block for entities where the condition returns true.`
	},
	{
		word: `else`,
		docs: `Inverts the comparison given by the previous if-statement at this scope level.`
	},
	{
		word: `give`,
		docs: `Gives item(s) to the given entity. Runs either a 'give' or 'structure load' depending on requirements. Utilizes builder fields.`
	},
	{
		word: `tp`,
		docs: `Teleports the executing/given entities to a specific position, selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).`
	},
	{
		word: `teleport`,
		docs: `Alias of 'tp'. Teleports the executing/given entities to a specific position, selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).`
	},
	{
		word: `move`,
		docs: `Moves the specified entity in a direction (LEFT, RIGHT, UP, DOWN, FORWARD, BACKWARD) for a certain amount. Simpler alternative for teleporting using caret offsets.`
	},
	{
		word: `face`,
		docs: `Faces the given entities towards a specific position, selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).`
	},
	{
		word: `lookat`,
		docs: `Alias of 'face'. Faces the given entities towards a specific position, selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).`
	},
	{
		word: `rotate`,
		docs: `Rotates the given entities a certain number of degrees horizontally and vertically from their current rotation.`
	},
	{
		word: `setblock`,
		docs: `Sets the block at a specific position, optionally using a replace mode.`
	},
	{
		word: `fill`,
		docs: `Fills blocks in a specific region, optionally using a replace mode.`
	},
	{
		word: `scatter`,
		docs: `Randomly scatters blocks throughout a region with a certain percentage.`
	},
	{
		word: `replace`,
		docs: `Replaces all source blocks with a result block in a specific region.`
	},
	{
		word: `kill`,
		docs: `Kills the given entities, causing the death animation, sounds, and particles to appear.`
	},
	{
		word: `remove`,
		docs: `Teleports the given entities deep into the void, causing a silent death.`
	},
	{
		word: `clear`,
		docs: `Clears the inventories of all given entities, optionally searching for a specific item and limiting the number of items to remove.`
	},
	{
		word: `globaltitle`,
		docs: `Displays a title on the screen of all players in the game. Can also be used to set the timings of the title. Supports format strings.`
	},
	{
		word: `title`,
		docs: `Displays a title on the screen of the executing player, or to the given one if specified. Can also be used to set the timings of the title. Supports format strings.`
	},
	{
		word: `globalactionbar`,
		docs: `Displays an actionbar on the screen of all players in the game. Can also be used to set the timings of the actionbar. Supports format strings.`
	},
	{
		word: `actionbar`,
		docs: `Displays an actionbar on the screen of the executing player, or to the given one if specified. Supports format strings.`
	},
	{
		word: `say`,
		docs: `Send a plain-text message as the executing entity. Plain selectors can be used, but not variables.`
	},
	{
		word: `halt`,
		docs: `Ends the execution of the code entirely by hitting the function command limit.`
	},
	{
		word: `damage`,
		docs: `Damages the given entities with a certain cause, optionally coming from a position or blaming an entity by a selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).`
	},
	{
		word: `effect`,
		docs: `Gives the given entities a potion effect. Time and amplifier can be specified to further customize the potion effect. All potion effects can be cleared using 'effect \<selector\> clear'.`
	},
	{
		word: `playsound`,
		docs: `Plays a sound effect in the world, optionally with volume, pitch, and filtering specific players.`
	},
	{
		word: `particle`,
		docs: `Spawns a particle effect in the world.`
	},
	{
		word: `dummy`,
		docs: `Create a dummy entity, remove the selected ones, or manage the classes on the selected ones. Requires feature 'DUMMIES' to be enabled.`
	},
	{
		word: `tag`,
		docs: `Add and remove tags from the given entity.`
	},
	{
		word: `explode`,
		docs: `Create an explosion at a specific position with optional positioning, power, delay, fire, and block breaking settings. Requires feature 'EXPLODERS' to be enabled.`
	},
	{
		word: `feature`,
		docs: `Enables a feature to be used for this project, generating any of the necessary files.`
	},
	{
		word: `function`,
		docs: `Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: functionName(parameters)`
	},
	{
		word: `fn`,
		docs: `Alias of 'function'. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: functionName(parameters)`
	},
	{
		word: `return`,
		docs: `Set the value that will be returned from this function when it ends. The caller can use this value however it wishes.`
	},
	{
		word: `for`,
		docs: `Runs the following statement or code-block once over every entity that matches a selector at its current position. Functionally equivalent to 'execute as \<selector\> at @s run \<code\>'`
	},
	{
		word: `execute`,
		docs: `Begins a vanilla minecraft 1.19.70+ execute chain. Can be followed by a statement or code-block, but does not explicitly support the 'run' subcommand.`
	},
]
const mcc_literals = [
	{
		word: `true`,
		docs: `A boolean value representing true/yes.`
	},
	{
		word: `false`,
		docs: `A boolean value representing false/no.`
	},
	{
		word: `not`,
		docs: `Invert the following comparison.`
	},
	{
		word: `and`,
		docs: `Includes on another comparison.`
	},
	{
		word: `null`,
		docs: `No value. Goes to 0/false under all types.`
	},
	{
		word: `~`,
		docs: `Relative to executor's position.`
	},
	{
		word: `^`,
		docs: `Relative to executor's direction.`
	},
]
const mcc_types = [
	{
		word: `int`,
		docs: `An integer, representing any whole value between -2147483648 to 2147483647.`
	},
	{
		word: `decimal`,
		docs: `A decimal number with a pre-specified level of precision.`
	},
	{
		word: `bool`,
		docs: `A true or false value. Displayed as whatever is set in the '_true' and '_false' preprocessor variables respectively.`
	},
	{
		word: `time`,
		docs: `A value representing a number of ticks. Displayed as MM:SS.`
	},
	{
		word: `struct`,
		docs: `A user-defined structure of multiple variables.`
	},
	{
		word: `ppv`,
		docs: `A preprocessor variable that will be set on function call. Not currently supported as a variable/struct type.`
	},
	{
		word: `global`,
		docs: `Makes a value global, meaning it will only be accessed in the context of the global fakeplayer, '_'.`
	},
	{
		word: `extern`,
		docs: `Makes a function extern, meaning it was written outside of MCCompiled and can now be called as any other function.`
	},
	{
		word: `bind`,
		docs: `Binds a value to a pre-defined MoLang query. See bindings.json.`
	},
]
const mcc_comparisons = [
	{
		word: `count`,
		docs: `Compare the number of entities that match a selector.`
	},
	{
		word: `any`,
		docs: `Check if any entities match a selector.`
	},
	{
		word: `block`,
		docs: `Check for a block.`
	},
	{
		word: `blocks`,
		docs: `Check for an area of blocks matching another.`
	},
	{
		word: `positioned`,
		docs: `Position the next comparison.`
	},
]
const mcc_options = [
	{
		word: `dummies`,
		docs: `Feature: Create dummy entity behavior/resource files and allow them to be spawned in the world.`
	},
	{
		word: `gametest`,
		docs: `Feature: Gametest Integration (not implemented)`
	},
	{
		word: `exploders`,
		docs: `Feature: Create exploder entity behavior/resource files and allow them to be created through the 'explode' command.`
	},
	{
		word: `uninstall`,
		docs: `Feature: Create an function named 'uninstall' to remove all tags/scoreboards/etc., made by this project.`
	},
	{
		word: `up`,
		docs: `Used with the 'move' command. Goes up relative to where the entity is looking.`
	},
	{
		word: `down`,
		docs: `Used with the 'move' command. Goes down relative to where the entity is looking.`
	},
	{
		word: `left`,
		docs: `Used with the 'move' command. Goes left relative to where the entity is looking.`
	},
	{
		word: `right`,
		docs: `Used with the 'move' command. Goes right relative to where the entity is looking.`
	},
	{
		word: `forward`,
		docs: `Used with the 'move' command. Goes forward relative to where the entity is looking.`
	},
	{
		word: `backward`,
		docs: `Used with the 'move' command. Goes backward relative to where the entity is looking.`
	},
	{
		word: `ascending`,
		docs: `Used with the '$sort' command. Sorts with the lowest value first.`
	},
	{
		word: `descending`,
		docs: `Used with the '$sort' command. Sorts with the highest value first.`
	},
	{
		word: `survival`,
		docs: `Survival mode. (0)`
	},
	{
		word: `creative`,
		docs: `Creative mode. (1)`
	},
	{
		word: `adventure`,
		docs: `Adventure mode. (2)`
	},
	{
		word: `spectator`,
		docs: `Spectator mode. (spectator)`
	},
	{
		word: `times`,
		docs: `Specifies the fade-in/stay/fade-out times this text will show for.`
	},
	{
		word: `subtitle`,
		docs: `Sets the subtitle for the next title shown.`
	},
	{
		word: `destroy`,
		docs: `Destroy any existing blocks as if broken by a player.`
	},
	{
		word: `replace`,
		docs: `Replace any existing blocks. Default option.`
	},
	{
		word: `hollow`,
		docs: `Hollow the area, only filling the outer edges with the block. To keep inside contents, use 'outline'.`
	},
	{
		word: `outline`,
		docs: `Outline the area, only filling the outer edges with the block. To remove inside contents, use 'hollow'.`
	},
	{
		word: `keep`,
		docs: `Keep any existing blocks/items, and only fill where air is present.`
	},
	{
		word: `lockinventory`,
		docs: `Lock the item in the player's inventory.`
	},
	{
		word: `lockslot`,
		docs: `Lock the item in the slot which it is placed in.`
	},
	{
		word: `canplaceon:`,
		docs: `Specifies a block the item can be placed on.`
	},
	{
		word: `candestroy:`,
		docs: `Specifies a block the item can destroy.`
	},
	{
		word: `enchant:`,
		docs: `Give a leveled enchantment to this item. No limits.`
	},
	{
		word: `name:`,
		docs: `Give the item a display name.`
	},
	{
		word: `lore:`,
		docs: `Give the item a line of lore. Multiple of these can be used to add more lines.`
	},
	{
		word: `author:`,
		docs: `If this item is a 'written_book', set the name of the author.`
	},
	{
		word: `title:`,
		docs: `If this item is a 'written_book', set its title.`
	},
	{
		word: `page:`,
		docs: `If this item is a 'written_book', add a page to it.  Multiple of these can be used to add more pages.`
	},
	{
		word: `dye:`,
		docs: `If this item is a piece of leather armor, set its color to an RGB value.`
	},
	{
		word: `align`,
		docs: `Execute subcommand: Runs aligned to the given axes.`
	},
	{
		word: `anchored`,
		docs: `Execute subcommand: Runs anchored to the executing entities eyes or feet.`
	},
	{
		word: `as`,
		docs: `Execute subcommand: Runs as the given entity(s).`
	},
	{
		word: `at`,
		docs: `Execute subcommand: Runs at the given location or entity.`
	},
	{
		word: `facing`,
		docs: `Teleport & Execute subcommand: Runs facing a certain position.`
	},
	{
		word: `facing entity`,
		docs: `Execute subcommand: Runs facing a certain entity.`
	},
	{
		word: `in`,
		docs: `Execute subcommand: Runs in a given dimension.`
	},
	{
		word: `positioned`,
		docs: `Execute subcommand: Runs at a given position.`
	},
	{
		word: `positioned as`,
		docs: `Execute subcommand: Runs at the position of the given entity.`
	},
	{
		word: `rotated`,
		docs: `Execute subcommand: Runs at the given rotation.`
	},
	{
		word: `rotated as`,
		docs: `Execute subcommand: Runs at the rotation of the given entity.`
	},
]
