const mccompiled = {
    operators: [`<`, `>`, `{`, `}`, `=`, `(`, `)`, `+`, `-`, `*`, `/`, `%`, `!`],
    selectors: [`@e`, `@a`, `@s`, `@p`, `@r`],
    preprocessor: [`$add`, `$append`, `$assert`, `$call`, `$dec`, `$div`, `$else`, `$if`, `$inc`, `$include`, `$iterate`, `$json`, `$len`, `$log`, `$macro`, `$mean`, `$median`, `$mod`, `$mul`, `$pow`, `$prepend`, `$repeat`, `$reverse`, `$sort`, `$strfriendly`, `$strlower`, `$strupper`, `$sub`, `$sum`, `$swap`, `$unique`, `$var`],
    commands: [`actionbar`, `assert`, `await`, `clear`, `damage`, `define`, `dialogue`, `dummy`, `effect`, `else`, `execute`, `explode`, `face`, `lookat`, `feature`, `fill`, `for`, `function`, `fn`, `give`, `globalactionbar`, `globalprint`, `globaltitle`, `halt`, `if`, `init`, `initialize`, `kill`, `lang`, `mc`, `command`, `cmd`, `move`, `particle`, `playsound`, `print`, `remove`, `repeat`, `replace`, `return`, `rotate`, `say`, `scatter`, `setblock`, `summon`, `tag`, `test`, `throw`, `title`, `tp`, `teleport`, `while`],
    literals: [`true`, `false`, `not`, `and`, `null`, `~`, `^`],
    types: [`int`, `decimal`, `bool`, `time`, `struct`, `ppv`, `global`, `local`, `extern`, `export`, `bind`, `auto`, `partial`, `async`],
    comparisons: [`until`, `count`, `any`, `block`, `blocks`, `positioned`],
    options: [`dummies`, `autoinit`, `exploders`, `uninstall`, `tests`, `audiofiles`, `up`, `down`, `left`, `right`, `forward`, `backward`, `ascending`, `descending`, `survival`, `creative`, `adventure`, `spectator`, `removeall`, `times`, `subtitle`, `destroy`, `replace`, `hollow`, `outline`, `keep`, `new`, `open`, `change`, `lockinventory`, `lockslot`, `canplaceon:`, `candestroy:`, `enchant:`, `name:`, `lore:`, `author:`, `title:`, `page:`, `dye:`, `text:`, `button:`, `onOpen:`, `onClose:`, `align`, `anchored`, `as`, `at`, `facing`, `facing entity`, `in`, `positioned`, `positioned as`, `rotated`, `rotated as`],
    tokenizer: {
        root: [
            [
                /@?[a-zA-Z$]\w*/,

                {
                    cases: {
                        '@selectors': 'selectors',
                        '@preprocessor': 'preprocessor',
                        '@commands': 'commands',
                        '@literals': 'literals',
                        '@types': 'types',
                        '@comparisons': 'comparisons',
                        '@options': 'options'
                    }
                }
            ],

            {include: '@handler'},

            [/[<>{}=()+\-*/%!]+/, 'operators'],

            // terminated strings
            [/"(?:[^"\\]|\\.)*"/, 'string'],
            [/'(?:[^'\\]|\\.)*'/, 'string'],

            // unterminated strings
            [/"(?:[^"\\]|\\.)*$/, 'string'],
            [/'(?:[^'\\]|\\.)*$/, 'string'],

            [/\[.+]/, 'selectors.properties'],
            [/!?(?:\.\.)?\d+(?:\.\.)?\.?\d*[hms]?/, 'numbers']
        ],
        comment: [
            [/[^\/*]+/, 'comment'],
            [/\/\*/, 'comment', '@push'],
            ["\\*/", 'comment', '@pop'],
            [/[\/*]/, 'comment']
        ],
        handler: [
            [/[ \t\r\n]+/, 'white'],
            [/\/\*/, 'comment', '@comment'],
            [/\/\/.*$/, 'comment'],
        ]
    }
}
const mcc_operators = [
    {
        word: `<`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `>`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `{`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `}`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `=`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `(`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `)`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `+`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `-`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `*`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `/`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `%`,
        docs: 'No documentation available for v1.19.'
    },
    {
        word: `!`,
        docs: 'No documentation available for v1.19.'
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
        word: `@r`,
        docs: `References a random entity.`
    },
]
const mcc_preprocessor = [
    {
        word: `$add`,
        docs: `Adds two preprocessor variables/values together, changing only the first one. A += B`
    },
    {
        word: `$append`,
        docs: `Adds the given item(s) to the end of the given preprocessor variable, or contents of another preprocessor variable if specified.`
    },
    {
        word: `$assert`,
        docs: `Asserts that the input comparison is true, and throws a compiler error if not.`
    },
    {
        word: `$call`,
        docs: `Calls a function by name and passes in the given parameters. Because this is a preprocessor operation, it has the same error handling as a normal function call.`
    },
    {
        word: `$dec`,
        docs: `Decrements the given preprocessor variable by one. If multiple values are held, they are all decremented.`
    },
    {
        word: `$div`,
        docs: `Divides two preprocessor variables/values from each other, changing only the first one. A /= B`
    },
    {
        word: `$else`,
        docs: `Directly inverts the result of the last $if call at this level in scope.`
    },
    {
        word: `$if`,
        docs: `Compares a preprocessor variable and another value/variable. If the source variable contains multiple values, they all must match the condition.`
    },
    {
        word: `$inc`,
        docs: `Increments the given preprocessor variable by one. If multiple values are held, they are all incremented.`
    },
    {
        word: `$include`,
        docs: `Places the contents of the given file in replacement for this statement. Not intended for production use yet.`
    },
    {
        word: `$iterate`,
        docs: `Runs the following statement/code-block once for each value in the given preprocessor variable. The current iteration is held in the preprocessor variable given. If the target is a JSON array, the elements will be iterated upon.`
    },
    {
        word: `$json`,
        docs: `Load a JSON file (if not previously loaded) and retrieve a value from it, storing said value in a preprocessor variable.`
    },
    {
        word: `$len`,
        docs: `If a preprocessor variable identifier or JSON array is specified, the number of elements it holds is gotten. If a string is given, its length is gotten.`
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
        word: `$mean`,
        docs: `Averages all values in the given preprocessor variable together into one value and stores it in a result variable.`
    },
    {
        word: `$median`,
        docs: `Gets the middle value/average of the two middle values and stores it in a result variable.`
    },
    {
        word: `$mod`,
        docs: `Divides two preprocessor variables/values from each other, setting only the first one to the remainder of the operation. A %= B`
    },
    {
        word: `$mul`,
        docs: `Multiplies two preprocessor variables/values together, changing only the first one. A *= B`
    },
    {
        word: `$pow`,
        docs: `Exponentiates two preprocessor variables/values with each other, changing only the first one. A = A^B`
    },
    {
        word: `$prepend`,
        docs: `Adds the given item(s) to the start of the given preprocessor variable.`
    },
    {
        word: `$repeat`,
        docs: `Repeats the following statement/code-block a number of times. If a variable identifier is given, that variable will be set to the index of the current iteration. 0, 1, 2, etc.`
    },
    {
        word: `$reverse`,
        docs: `Reverses the order of the values in the given preprocessor variable.`
    },
    {
        word: `$sort`,
        docs: `Sorts the order of the values in the given preprocessor variable either 'ascending' or 'descending'. Values must be comparable.`
    },
    {
        word: `$strfriendly`,
        docs: `Convert the given preprocessor variable value(s) to a string in 'Title Case'.`
    },
    {
        word: `$strlower`,
        docs: `Convert the given preprocessor variable value(s) to a string in 'lowercase'.`
    },
    {
        word: `$strupper`,
        docs: `Convert the given preprocessor variable value(s) to a string in 'UPPERCASE'.`
    },
    {
        word: `$sub`,
        docs: `Subtracts two preprocessor variables/values from each other, changing only the first one. A -= B`
    },
    {
        word: `$sum`,
        docs: `Adds all values in the given preprocessor variable together into one value and stores it in a result variable.`
    },
    {
        word: `$swap`,
        docs: `Swaps the values of two preprocessor variables`
    },
    {
        word: `$unique`,
        docs: `Flattens the given preprocessor array to only unique values.`
    },
    {
        word: `$var`,
        docs: `Sets a preprocessor variable to the value(s) provided.`
    },
]
const mcc_commands = [
    {
        word: `actionbar`,
        docs: `Displays an actionbar on the screen of the executing player, or to the given one if specified.`
    },
    {
        word: `assert`,
        docs: `Asserts that the given condition evaluates to true, at runtime. If the condition evaluates to false, the code is halted and info is displayed to the executing player(s).`
    },
    {
        word: `await`,
        docs: `Works in async functions. Awaits a certain amount of time, for a condition to be met, or another async function to complete executing.`
    },
    {
        word: `clear`,
        docs: `Clears the inventories of all given entities, optionally searching for a specific item and limiting the number of items to remove.`
    },
    {
        word: `damage`,
        docs: `Damages the given entities with a certain cause, optionally coming from a position or blaming an entity by a selector, "name:type" of entity, or name of another managed entity (e.g., dummy entities).`
    },
    {
        word: `define`,
        docs: `Defines a variable with a name and type, defaulting to int if unspecified. Can be assigned a value directly after defining.`
    },
    {
        word: `dialogue`,
        docs: `If followed by a block, defines a new dialogue scene with the given name.`
    },
    {
        word: `dummy`,
        docs: `Create a dummy entity, remove the selected ones, or manage the classes on the selected ones. Requires feature 'DUMMIES' to be enabled.`
    },
    {
        word: `effect`,
        docs: `Gives the given entities a potion effect. Time and amplifier can be specified to further customize the potion effect. All potion effects can be cleared using 'effect \<selector\> clear'.`
    },
    {
        word: `else`,
        docs: `Inverts the comparison given by the previous if-statement at this scope level.`
    },
    {
        word: `execute`,
        docs: `Begins a vanilla Minecraft execute chain. Can be followed by a statement or code-block, but does not explicitly support the 'run' subcommand.`
    },
    {
        word: `explode`,
        docs: `Create an explosion at a specific position with optional positioning, power, delay, fire, and block breaking settings. Requires feature 'EXPLODERS' to be enabled.`
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
        word: `feature`,
        docs: `Enables a feature to be used for this project, generating any of the necessary files.`
    },
    {
        word: `fill`,
        docs: `Fills blocks in a specific region, optionally using a replace mode.`
    },
    {
        word: `for`,
        docs: `Runs the following statement or code-block once over every entity that matches a selector at its current position. Functionally equivalent to \`execute as <selector> at @s run <code>\``
    },
    {
        word: `function`,
        docs: `Defines a function. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: \`functionName(parameters)\``
    },
    {
        word: `fn`,
        docs: `Alias of 'function'. Defines a function. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: \`functionName(parameters)\``
    },
    {
        word: `give`,
        docs: `Gives item(s) to the given entity. Runs either a 'give' or 'structure load' depending on requirements. Utilizes builder fields.`
    },
    {
        word: `globalactionbar`,
        docs: `Displays an actionbar on the screen of all players in the game. Can also be used to set the timings of the actionbar.`
    },
    {
        word: `globalprint`,
        docs: `Prints a chat message to all players in the game.`
    },
    {
        word: `globaltitle`,
        docs: `Displays a title on the screen of all players in the game. Can also be used to set the timings of the title.`
    },
    {
        word: `halt`,
        docs: `Ends the execution of the code entirely by hitting the function command limit.`
    },
    {
        word: `if`,
        docs: `Performs a comparison, only running the proceeding statement/code-block if the comparisons(s) are true. Multiple comparisons can be chained using the keyword 'and', and comparisons can be inverted using the keyword 'not'`
    },
    {
        word: `init`,
        docs: `Ensures this variable has a value, defaulting to 0 if not. This ensures the executing entity(s) function as intended all the time. Use clarifiers to pick who the variable is initialized for: e.g., \`variableName[@a]\``
    },
    {
        word: `initialize`,
        docs: `Alias of 'init'. Ensures this variable has a value, defaulting to 0 if not. This ensures the executing entity(s) function as intended all the time. Use clarifiers to pick who the variable is initialized for: e.g., \`variableName[@a]\``
    },
    {
        word: `kill`,
        docs: `Kills the given entities, causing the death animation, sounds, and particles to appear.`
    },
    {
        word: `lang`,
        docs: `Sets the active lang file (examples: en_US, pt_BR). Once set, all text will automatically be localized into that lang file; including format-strings.`
    },
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
        word: `move`,
        docs: `Moves the specified entity in a direction (LEFT, RIGHT, UP, DOWN, FORWARD, BACKWARD) for a certain amount. Simpler alternative for teleporting using caret offsets.`
    },
    {
        word: `particle`,
        docs: `Spawns a particle effect in the world.`
    },
    {
        word: `playsound`,
        docs: `Plays a sound effect in the world, optionally with volume, pitch, and filtering specific players.`
    },
    {
        word: `print`,
        docs: `Prints a chat message to the executing player, or to the given one if specified.`
    },
    {
        word: `remove`,
        docs: `Teleports the given entities deep into the void, causing a silent death. Looking to rewrite this in the future to generate entity code for real removal.`
    },
    {
        word: `repeat`,
        docs: `Repeats the proceeding statement/code-block the given number of times. This command always runs at runtime.`
    },
    {
        word: `replace`,
        docs: `Replaces all source blocks with a result block in a specific region.`
    },
    {
        word: `return`,
        docs: `Set the value that will be returned from this function when it ends. The caller can use this value however it wishes.`
    },
    {
        word: `rotate`,
        docs: `Rotates the given entities a certain number of degrees horizontally and vertically from their current rotation.`
    },
    {
        word: `say`,
        docs: `Send a plain-text message as the executing entity. Plain selectors can be used, but not variables.`
    },
    {
        word: `scatter`,
        docs: `Randomly scatters blocks throughout a region with a certain percentage.`
    },
    {
        word: `setblock`,
        docs: `Sets the block at a specific position, optionally using a replace mode.`
    },
    {
        word: `summon`,
        docs: `Summons an entity; matches Minecraft vanilla syntax.`
    },
    {
        word: `tag`,
        docs: `Add and remove tags from the given entity.`
    },
    {
        word: `test`,
        docs: `Defines a test; requires 'tests' feature. Must be followed by a code-block that contains the test contents.`
    },
    {
        word: `throw`,
        docs: `Throws an error, displaying it to the executing player(s). The code is halted immediately, so handle cleanup before calling throw.`
    },
    {
        word: `title`,
        docs: `Displays a title on the screen of the executing player, or to the given one if specified. Can also be used to set the timings of the title.`
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
        word: `while`,
        docs: `Repeats the proceeding statement/code-block as long as a condition remains true.  Multiple comparisons can be chained using the keyword 'and', and comparisons can be inverted using the keyword 'not'`
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
        docs: `Makes a value global, never being assigned on an entity. Alternately used as a parameter for the 'async' attribute.`
    },
    {
        word: `local`,
        docs: `Makes a value local (default). Alternately used as a parameter for the 'async' attribute.`
    },
    {
        word: `extern`,
        docs: `Makes a function extern, meaning it was written outside of MCCompiled and can now be called as any other function.`
    },
    {
        word: `export`,
        docs: `Marks a function for export, meaning it will be outputted regardless of if it is used or not.`
    },
    {
        word: `bind`,
        docs: `Binds a value to a pre-defined MoLang query. See bindings.json.`
    },
    {
        word: `auto`,
        docs: `Makes a function run every tick (via tick.json), or if specified, some other interval.`
    },
    {
        word: `partial`,
        docs: `Makes a function partial, allowing it to be re-defined , appending to any previous code in it. When re-declaring a function, the partial attribute must be used in both.`
    },
    {
        word: `async`,
        docs: `Makes the given function asynchronous, either locally (state is attached to an entity) or globally (state is global only).`
    },
]
const mcc_comparisons = [
    {
        word: `until`,
        docs: `Used in the 'await' command to wait UNTIL a condition is true.`
    },
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
        word: `autoinit`,
        docs: `Feature: Runs the initialization file automatically in new worlds, and every time a new build is compiled. Requires a check-function to be run every tick.`
    },
    {
        word: `exploders`,
        docs: `Feature: Create exploder entity behavior/resource files and allow them to be created through the 'explode' command.`
    },
    {
        word: `uninstall`,
        docs: `Feature: Create an function named 'uninstall' to remove all tags/scoreboards/etc made by this project.`
    },
    {
        word: `tests`,
        docs: `Feature: Enables the ability to use the 'test' command, which creates tests that are run on '/function test'. Use the 'assert' command to test various parts of your code.`
    },
    {
        word: `audiofiles`,
        docs: `Feature: Enables support for 'playsound' command to accept audio files.`
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
        word: `removeall`,
        docs: `Used with the 'dummy' command. Subcommand, removes all dummies, optionally with the given tag.`
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
        word: `new`,
        docs: `Create a new dialogue scene with the given name.`
    },
    {
        word: `open`,
        docs: `Open an existing dialogue through the given NPC, for the given player(s).`
    },
    {
        word: `change`,
        docs: `Change the dialogue that shows up when an NPC is interacted with (for specific players if specified)`
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
        docs: `Item display name OR dialogue NPC name.`
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
        word: `text:`,
        docs: `The text, aka the contents of the dialogue.`
    },
    {
        word: `button:`,
        docs: `Adds a button to the dialogue which runs code when clicked.`
    },
    {
        word: `onOpen:`,
        docs: `Specifies the code to run every time this dialogue is opened.`
    },
    {
        word: `onClose:`,
        docs: `Specifies the code to run every time this dialogue is closed.`
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
