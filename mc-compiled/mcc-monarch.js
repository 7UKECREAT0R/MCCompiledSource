const mccompiled = {
    operators: [`<`, `>`, `<=`, `>=`, `{`, `}`, `=`, `==`, `!=`, `(`, `)`, `+`, `-`, `*`, `/`, `%`, `+=`, `-=`, `*=`, `/=`, `%=`, `~`, `^`],
    selectors: [`@e`, `@a`, `@s`, `@p`, `@r`],
    preprocessor: [`$var`, `$inc`, `$dec`, `$add`, `$sub`, `$mul`, `$div`, `$mod`, `$pow`, `$swap`, `$append`, `$prepend`, `$if`, `$else`, `$assert`, `$repeat`, `$log`, `$macro`, `$include`, `$strfriendly`, `$strupper`, `$strlower`, `$sum`, `$median`, `$mean`, `$sort`, `$reverse`, `$unique`, `$iterate`, `$len`, `$json`, `$call`],
    commands: [`mc`, `globalprint`, `print`, `lang`, `define`, `init`, `if`, `else`, `while`, `repeat`, `assert`, `throw`, `give`, `tp`, `move`, `face`, `rotate`, `setblock`, `fill`, `scatter`, `replace`, `kill`, `clear`, `globaltitle`, `title`, `globalactionbar`, `actionbar`, `say`, `camera`, `halt`, `summon`, `damage`, `effect`, `playsound`, `particle`, `gamemode`, `dummy`, `tag`, `explode`, `feature`, `function`, `test`, `return`, `dialogue`, `for`, `execute`, `await`],
    literals: [`true`, `false`, `null`],
    types: [`global`, `local`, `extern`, `export`, `bind`, `auto`, `partial`, `async`, `int`, `decimal`, `bool`, `time`],
    comparisons: [`not`, `count`, `any`, `block`, `blocks`, `and`],
    options: [`ascending`, `descending`, `keep`, `lockinventory`, `lockslot`, `canplaceon: `, `candestroy: `, `enchant: `, `name: `, `lore: `, `title: `, `author: `, `page: `, `dye: `, `facing`, `facing`, `times`, `subtitle`, `clear`, `fade`, `time`, `color`, `set`, `default`, `entity_offset`, `view_offset`, `ease`, `facing`, `pos`, `rot`, `facing`, `clear`, `create`, `single`, `removeall`, `remove`, `add`, `remove`, `new`, `open`, `change`, `at`, `align`, `anchored`, `as`, `at`, `facing`, `entity`, `if`, `score`, `matches`, `entity`, `block`, `blocks`, `unless`, `score`, `matches`, `entity`, `block`, `blocks`, `in`, `positioned`, `rotated`],
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
                        '@options': 'options',
                    }
                }
            ],
            {
                include: '@handler',
            },
            [
                /[<>{}=()+\-*/%!]+/,
                "operators"
            ],
            [
                /"(?:[^"\\]|\\.)*"/,
                "string"
            ],
            [
                /'(?:[^"\\]|\\.)*'/,
                "string"
            ],
            [
                /"(?:[^"\\]|\\.)*$/,
                "string"
            ],
            [
                /'(?:[^'\\]|\\.)*$/,
                "string"
            ],
            [
                /\[.+]/,
                "selectors.properties"
            ],
            [
                /!?(?:\.\.)?\d+(?:\.\.)?\.?\d*[hms]?/,
                "numbers"
            ]
        ],
        comment: [
            [
                /[^\/*]+/,
                "comment"
            ],
            [
                /\/\*/,
                "comment",
                "@push"
            ],
            [
                "\\*/",
                "comment",
                "@pop"
            ],
            [
                /[\/*]/,
                "comment"
            ]
        ],
        handler: [
            [
                /[ \t\r\n]+/,
                "white"
            ],
            [
                /\/\*/,
                "comment",
                "@comment"
            ],
            [
                /\/\/.*$/,
                "comment"
            ]
        ]
    }
};
const mcc_operators = [
    {
        word: '<',
        docs: 'Checks if the current value is less than the next one.',
    },
    {
        word: '>',
        docs: 'Checks if the current value is greater than the next one.',
    },
    {
        word: '<=',
        docs: 'Checks if the current value is less than or equal to the next one.',
    },
    {
        word: '>=',
        docs: 'Checks if the current value is greater than or equal to the next one.',
    },
    {
        word: '{',
        docs: 'Opens a code block.',
    },
    {
        word: '}',
        docs: 'Closes a code block.',
    },
    {
        word: '=',
        docs: 'Assigns a value to whatever\'s on the left-hand side.',
    },
    {
        word: '==',
        docs: 'Checks if the current value is equal to the next one.',
    },
    {
        word: '!=',
        docs: 'Checks if the current value is not equal to the next one.',
    },
    {
        word: '(',
        docs: 'Open parenthesis.',
    },
    {
        word: ')',
        docs: 'Close parenthesis.',
    },
    {
        word: '+',
        docs: 'Adds the left and right values.',
    },
    {
        word: '-',
        docs: 'Subtracts the right value from the left value.',
    },
    {
        word: '*',
        docs: 'Multiplies the left and right values.',
    },
    {
        word: '/',
        docs: 'Divides the left value by the right value.',
    },
    {
        word: '%',
        docs: 'Divides the left value by the right value and returns the remainder.',
    },
    {
        word: '+=',
        docs: 'Adds the left and right values. Assigns the result to the left value.',
    },
    {
        word: '-=',
        docs: 'Subtracts the right value from the left value. Assigns the result to the left value.',
    },
    {
        word: '*=',
        docs: 'Multiplies the left and right values. Assigns the result to the left value.',
    },
    {
        word: '/=',
        docs: 'Divides the left value by the right value. Assigns the result to the left value.',
    },
    {
        word: '%=',
        docs: 'Divides the left value by the right value and returns the remainder. Assigns the result to the left value.',
    },
    {
        word: '~',
        docs: 'Coordinate relative to the executing position.',
    },
    {
        word: '^',
        docs: 'Coordinate relative to where the executor is facing.',
    }
];
const mcc_selectors = [
    {
        word: '@e',
        docs: 'Reference all entities.',
    },
    {
        word: '@a',
        docs: 'Reference all players.',
    },
    {
        word: '@s',
        docs: 'Reference the executing entity/player.',
    },
    {
        word: '@p',
        docs: 'Reference the nearest player.',
    },
    {
        word: '@r',
        docs: 'Reference a random entity.',
    }
];
const mcc_literals = [
    {
        word: 'true',
        docs: 'The boolean value \'true\'.',
    },
    {
        word: 'false',
        docs: 'The boolean value \'false\'.',
    },
    {
        word: 'null',
        docs: 'Defaults to 0, false, or null depending on the context. Represents nothing generically.',
    }
];
const mcc_types = [
    {
        word: 'global',
        docs: 'Defines something that\'s global and not attached to any specific entity.',
    },
    {
        word: 'local',
        docs: 'Defines something that\'s local to an entity.',
    },
    {
        word: 'extern',
        docs: 'Function attribute which makes it use an existing .mcfunction file as its source. Parameters will be passed verbatim.',
    },
    {
        word: 'export',
        docs: 'Function attribute which forces it to be exported whether it\'s in use or not.',
    },
    {
        word: 'bind',
        docs: 'Value attribute which binds a MoLang query to the value. The value will be updated automatically whenever the query result changes.',
    },
    {
        word: 'auto',
        docs: 'Function attribute which makes it automatically run every tick; or, if specified, every N ticks.',
    },
    {
        word: 'partial',
        docs: 'Function attribute which makes it able to be defined more than once, with each definition appending commands to it instead of overwriting it.',
    },
    {
        word: 'async',
        docs: 'Function attribute which makes it run asynchronously. Allows the use of the \'await\' command for sequences which don\'t finish in a single tick.',
    },
    {
        word: 'int',
        docs: 'An integer, representing any whole value between -2147483648 to 2147483647.',
    },
    {
        word: 'decimal',
        docs: 'A decimal number with a pre-specified level of precision.',
    },
    {
        word: 'bool',
        docs: 'A true or false value.',
    },
    {
        word: 'time',
        docs: 'A value representing a number of ticks. Displayed as MM:SS by default.',
    }
];
const mcc_options = [
    {
        word: 'ascending',
        docs: 'Sort variables starting with the lowest first.',
    },
    {
        word: 'descending',
        docs: 'Sort variables starting with the highest first.',
    },
    {
        word: 'keep',
        docs: 'Item will stay in the player\'s inventory even after death.',
    },
    {
        word: 'lockinventory',
        docs: 'Lock the item in the player\'s inventory.',
    },
    {
        word: 'lockslot',
        docs: 'Lock the item in the slot it\'s located in inside the player\'s inventory.',
    },
    {
        word: 'canplaceon: ',
        docs: 'Adds a block that this block can be placed on in adventure mode.',
    },
    {
        word: 'candestroy: ',
        docs: 'Adds a block that this tool/item can break in adventure mode.',
    },
    {
        word: 'enchant: ',
        docs: 'Adds an enchantment to the item.',
    },
    {
        word: 'name: ',
        docs: 'Sets the display name of the item.',
    },
    {
        word: 'lore: ',
        docs: 'Adds a line of lore to the item.',
    },
    {
        word: 'title: ',
        docs: 'If the item is a written book, sets the title of the book.',
    },
    {
        word: 'author: ',
        docs: 'If the item is a written book, sets the author of the book.',
    },
    {
        word: 'page: ',
        docs: 'If the item is a written book, adds a page of text to the book.',
    },
    {
        word: 'dye: ',
        docs: 'If the item is leather armor, sets the dye color of the armor.',
    },
    {
        word: 'facing',
        docs: 'Set the position/entity the teleported entity will face towards.',
    },
    {
        word: 'facing',
        docs: 'Set the position/entity the teleported entity will face towards.',
    },
    {
        word: 'times',
        docs: 'Set the timings for the next title/future titles.',
    },
    {
        word: 'subtitle',
        docs: 'Set the subtitle for the next title displayed.',
    },
    {
        word: 'clear',
        docs: 'Clears the camera of the given players, setting it back to default.',
    },
    {
        word: 'fade',
        docs: 'Fade the camera in and out with a given color.',
    },
    {
        word: 'time',
        docs: 'Specify the in/hold/out times of the fade, in seconds.',
    },
    {
        word: 'color',
        docs: 'Specify the color of the fade.',
    },
    {
        word: 'set',
        docs: 'Set the camera for the given players.',
    },
    {
        word: 'default',
        docs: 'The default settings for the camera. Not necessary as it can be inferred.',
    },
    {
        word: 'entity_offset',
        docs: 'Offset the camera relative to its entity (world space).',
    },
    {
        word: 'view_offset',
        docs: 'Offset the camera relative to its entity (screen space).',
    },
    {
        word: 'ease',
        docs: 'Causes the camera to smoothly transition from its previous setting.',
    },
    {
        word: 'facing',
        docs: 'Rotate the camera to face either an entity, or a position in the world.',
    },
    {
        word: 'pos',
        docs: 'Sets the camera\'s position. Only really relevant when using the \'minecraft:free\' preset.',
    },
    {
        word: 'rot',
        docs: 'Sets the camera\'s rotation. Only really relevant when using the \'minecraft:free\' preset.',
    },
    {
        word: 'facing',
        docs: 'Spawn the entity facing a particular position or entity.',
    },
    {
        word: 'clear',
        docs: 'Clears all effects from the given entities.',
    },
    {
        word: 'create',
        docs: 'Create a new dummy entity.',
    },
    {
        word: 'single',
        docs: 'Create a new dummy entity, removing any others with a matching name/tag.',
    },
    {
        word: 'removeall',
        docs: 'Remove all dummies in the world, or only ones with a specific tag.',
    },
    {
        word: 'remove',
        docs: 'Remove all dummies with the given name, and optionally tag.',
    },
    {
        word: 'add',
        docs: 'Add a tag to the given entities.',
    },
    {
        word: 'remove',
        docs: 'Remove a tag from the given entities.',
    },
    {
        word: 'new',
        docs: 'Creates a new dialogue scene.',
    },
    {
        word: 'open',
        docs: 'Opens a dialogue scene for the given players.',
    },
    {
        word: 'change',
        docs: 'Change the dialogue for the given NPC, optionally only for specific players.',
    },
    {
        word: 'at',
        docs: 'Offset the execution position per entity.',
    },
    {
        word: 'align',
        docs: 'Aligns the current position of the command to the block grid.',
    },
    {
        word: 'anchored',
        docs: 'Execute at the location of a specific part of the executing entity; The eyes or the feet.',
    },
    {
        word: 'as',
        docs: 'Execute as the given entity/entities.',
    },
    {
        word: 'at',
        docs: 'Execute at the position of the given entity.',
    },
    {
        word: 'facing',
        docs: 'Execute facing another position/entity.',
    },
    {
        word: 'entity',
        docs: 'Execute facing another entity.',
    },
    {
        word: 'if',
        docs: 'Execute if a certain condition passes.',
    },
    {
        word: 'score',
        docs: 'Execute if a value/scoreboard objective matches a condition.',
    },
    {
        word: 'matches',
        docs: 'Check if \'a\' matches a certain number range.',
    },
    {
        word: 'entity',
        docs: 'Execute if a selector matches.',
    },
    {
        word: 'block',
        docs: 'Execute if a block matches.',
    },
    {
        word: 'blocks',
        docs: 'Execute if two regions of blocks match.',
    },
    {
        word: 'unless',
        docs: 'Execute unless a certain condition passes.',
    },
    {
        word: 'score',
        docs: 'Execute unless a value/scoreboard objective matches a condition.',
    },
    {
        word: 'matches',
        docs: 'Check if \'a\' doesn\'t match a certain number range.',
    },
    {
        word: 'entity',
        docs: 'Execute unless a selector matches.',
    },
    {
        word: 'block',
        docs: 'Execute unless a block matches.',
    },
    {
        word: 'blocks',
        docs: 'Execute unless two regions of blocks match.',
    },
    {
        word: 'in',
        docs: 'Execute in a specific dimension.',
    },
    {
        word: 'positioned',
        docs: 'Change the execution position while keeping the current rotation.',
    },
    {
        word: 'rotated',
        docs: 'Change the execution rotation while keeping the current position.',
    },
    {
        word: 'not',
        docs: 'Invert the next comparison.',
    },
    {
        word: 'count',
        docs: 'Count the number of matching entities and compare the result.',
    },
    {
        word: 'any',
        docs: 'Check if any entities match the given selector.',
    },
    {
        word: 'block',
        docs: 'Check if a block matches a given filter.',
    },
    {
        word: 'blocks',
        docs: 'Check if two regions of blocks are identical.',
    },
    {
        word: 'and',
        docs: 'Add another comparison.',
    }
];
const mcc_preprocessor = [
    {
        word: '$var',
        docs: 'Sets a preprocessor variable to the value(s) provided. Can be empty.',
    },
    {
        word: '$inc',
        docs: 'Increments the given preprocessor variable by one. If multiple values are held, they are all incremented.',
    },
    {
        word: '$dec',
        docs: 'Decrements the given preprocessor variable by one. If multiple values are held, they are all decremented.',
    },
    {
        word: '$add',
        docs: 'Adds two preprocessor variables/values together, changing only the first one. A += B',
    },
    {
        word: '$sub',
        docs: 'Subtracts two preprocessor variables/values from each other, changing only the first one. A -= B',
    },
    {
        word: '$mul',
        docs: 'Multiplies two preprocessor variables/values together, changing only the first one. A *= B',
    },
    {
        word: '$div',
        docs: 'Divides two preprocessor variables/values from each other, changing only the first one. A /= B',
    },
    {
        word: '$mod',
        docs: 'Divides two preprocessor variables/values from each other, setting only the first one to the remainder of the operation. A %= B',
    },
    {
        word: '$pow',
        docs: 'Exponentiates two preprocessor variables/values with each other, changing only the first one. A = A^B',
    },
    {
        word: '$swap',
        docs: 'Swaps the values of two preprocessor variables',
    },
    {
        word: '$append',
        docs: 'Adds the given item(s) to the end of the given preprocessor variable, or contents of another preprocessor variable if specified.',
    },
    {
        word: '$prepend',
        docs: 'Adds the given item(s) to the start of the given preprocessor variable.',
    },
    {
        word: '$if',
        docs: 'Compares a preprocessor variable and another value/variable. If the source variable contains multiple values, they all must match the condition.',
    },
    {
        word: '$else',
        docs: 'Directly inverts the result of the last $if call at this level in scope.',
    },
    {
        word: '$assert',
        docs: 'Asserts that the input comparison is true, and throws a compiler error if not. Allows a custom error message.',
    },
    {
        word: '$repeat',
        docs: 'Repeats the following statement/code-block a number of times. If a variable identifier is given, that variable will be set to the index of the current iteration. 0, 1, 2, etc.',
    },
    {
        word: '$log',
        docs: 'Sends a message to stdout with a line terminator at the end.',
    },
    {
        word: '$macro',
        docs: 'If a code-block follows this directive, it is treated as a definition. Arguments are passed in as preprocessor variables. If no code-block follows this call, it will attempt to run the macro with any inputs parameters copied to their respective preprocessor variables.',
    },
    {
        word: '$include',
        docs: 'Places the contents of the given file in replacement for this statement.',
    },
    {
        word: '$strfriendly',
        docs: 'Convert the given preprocessor variable value(s) to a string in \'Title Case\'.',
    },
    {
        word: '$strupper',
        docs: 'Convert the given preprocessor variable value(s) to a string in \'UPPERCASE\'.',
    },
    {
        word: '$strlower',
        docs: 'Convert the given preprocessor variable value(s) to a string in \'lowercase\'.',
    },
    {
        word: '$sum',
        docs: 'Adds all values in the given preprocessor variable together into one value and stores it in a result variable.',
    },
    {
        word: '$median',
        docs: 'Gets the middle value/average of the two middle values and stores it in a result variable.',
    },
    {
        word: '$mean',
        docs: 'Averages all values in the given preprocessor variable together into one value and stores it in a result variable.',
    },
    {
        word: '$sort',
        docs: 'Sorts the order of the values in the given preprocessor variable either \'ascending\' or \'descending\'. Values must be comparable.',
    },
    {
        word: '$reverse',
        docs: 'Reverses the order of the values in the given preprocessor variable.',
    },
    {
        word: '$unique',
        docs: 'Flattens the given preprocessor array to only unique values.',
    },
    {
        word: '$iterate',
        docs: 'Runs the following statement/code-block once for each value in the given preprocessor variable. The current iteration is held in the preprocessor variable given. If the target is a JSON array, the elements will be iterated upon.',
    },
    {
        word: '$len',
        docs: 'If a preprocessor variable identifier or JSON array is specified, the number of elements it holds is gotten. If a string is given, its length is gotten.',
    },
    {
        word: '$json',
        docs: 'Load a JSON file (if not previously loaded) and retrieve a value from it, storing said value in a preprocessor variable.',
    },
    {
        word: '$call',
        docs: 'Calls a function by name and passes in the given parameters. Because this is a preprocessor operation, it has the same error handling as a normal function call.',
    }
];
const mcc_commands = [
    {
        word: 'mc',
        docs: 'Places a plain command in the output file, used for when the language lacks a certain feature.',
    },
    {
        word: 'globalprint',
        docs: 'Prints a chat message to all players in the game.',
    },
    {
        word: 'print',
        docs: 'Prints a chat message to the executing player, or to the given one if specified.',
    },
    {
        word: 'lang',
        docs: 'Sets the active lang file (examples: en_US, pt_BR). Once set, all text in the project will automatically be localized into that lang file.',
    },
    {
        word: 'define',
        docs: 'Defines a value with a name and type, defaulting to int if unspecified. Can be assigned a value directly after defining.',
    },
    {
        word: 'init',
        docs: 'Ensures this value has a value, defaulting to 0 if not. This ensures the executing entity(s) function as intended all the time. Use a clarifier to pick who the variable is initialized for: e.g., `variableName[@a]`',
    },
    {
        word: 'if',
        docs: 'Performs a comparison, only running the proceeding statement/code-block if the comparisons(s) are true. Multiple comparisons can be chained using the keyword \'and\', and comparisons can be inverted using the keyword \'not\'',
    },
    {
        word: 'else',
        docs: 'Inverts the comparison given by the previous if-statement at this scope level.',
    },
    {
        word: 'while',
        docs: 'Repeats the proceeding statement/code-block as long as a condition remains true.  Multiple comparisons can be chained using the keyword \'and\', and comparisons can be inverted using the keyword \'not\'',
    },
    {
        word: 'repeat',
        docs: 'Repeats the proceeding statement/code-block the given number of times. This command always runs at runtime.',
    },
    {
        word: 'assert',
        docs: 'Asserts that the given condition evaluates to true, at runtime. If the condition evaluates to false, the code is halted and info is displayed to the executing player(s).',
    },
    {
        word: 'throw',
        docs: 'Throws an error, displaying it to the executing player(s). The code is halted immediately, so handle cleanup before calling throw.',
    },
    {
        word: 'give',
        docs: 'Gives item(s) to the given entity. Runs either a \'give\' or \'structure load\' depending on requirements. Utilizes builder fields.',
    },
    {
        word: 'tp',
        docs: 'Teleports the executing/given entities to a specific position, selector, or name of another managed entity (e.g., dummy entities).',
    },
    {
        word: 'move',
        docs: 'Moves the specified entity in a direction (LEFT, RIGHT, UP, DOWN, FORWARD, BACKWARD) for a certain amount. Simpler alternative for teleporting using caret offsets.',
    },
    {
        word: 'face',
        docs: 'Faces the given entities towards a specific position, selector, or name of another managed entity (e.g., dummy entities).',
    },
    {
        word: 'rotate',
        docs: 'Rotates the given entities a certain number of degrees horizontally and vertically from their current rotation.',
    },
    {
        word: 'setblock',
        docs: 'Sets the block at a specific position, optionally using a replace mode.',
    },
    {
        word: 'fill',
        docs: 'Fills blocks in a specific region, optionally using a replace mode.',
    },
    {
        word: 'scatter',
        docs: 'Randomly scatters blocks throughout a region with a certain percentage.',
    },
    {
        word: 'replace',
        docs: 'Replaces all source blocks with a result block in a specific region.',
    },
    {
        word: 'kill',
        docs: 'Kills the given entities, causing the death animation, sounds, and particles to appear.',
    },
    {
        word: 'clear',
        docs: 'Clears the inventories of all given entities, optionally searching for a specific item and limiting the number of items to remove.',
    },
    {
        word: 'globaltitle',
        docs: 'Displays a title on the screen of all players in the game. Can also be used to set the timings of the title.',
    },
    {
        word: 'title',
        docs: 'Displays a title on the screen of the executing player, or to the given one if specified. Can also be used to set the timings of the title.',
    },
    {
        word: 'globalactionbar',
        docs: 'Displays an actionbar on the screen of all players in the game. Can also be used to set the timings of the actionbar.',
    },
    {
        word: 'actionbar',
        docs: 'Displays an actionbar on the screen of the executing player, or to the given one if specified.',
    },
    {
        word: 'say',
        docs: 'Send a plain-text message as the executing entity. Plain selectors can be used, but not variables.',
    },
    {
        word: 'camera',
        docs: 'Modify the camera of the given players, identical to the vanilla command with much more fault-tolerance.',
    },
    {
        word: 'halt',
        docs: 'Ends the execution of the code entirely by hitting the function command limit.',
    },
    {
        word: 'summon',
        docs: 'Summons an entity; matches Minecraft vanilla syntax.',
    },
    {
        word: 'damage',
        docs: 'Damages the given entities with a certain cause, optionally coming from a position or blaming an entity by a selector, or name of another managed entity (e.g., dummy entities).',
    },
    {
        word: 'effect',
        docs: 'Gives the given entities a potion effect. Time and amplifier can be specified to further customize the potion effect. All potion effects can be cleared using `effect <selector> clear`.',
    },
    {
        word: 'playsound',
        docs: 'Plays a sound effect in the world, optionally with volume, pitch, and filtering specific players.',
    },
    {
        word: 'particle',
        docs: 'Spawns a particle effect in the world.',
    },
    {
        word: 'gamemode',
        docs: 'Sets the gamemode of the executing player or other players if specified.',
    },
    {
        word: 'dummy',
        docs: 'Create a dummy entity, remove the selected ones, or manage the classes on the selected ones. Requires feature \'DUMMIES\' to be enabled.',
    },
    {
        word: 'tag',
        docs: 'Add and remove tags from the given entity.',
    },
    {
        word: 'explode',
        docs: 'Create an explosion at a specific position with optional positioning, power, delay, fire, and block breaking settings. Requires feature \'EXPLODERS\' to be enabled.',
    },
    {
        word: 'feature',
        docs: 'Enables a feature to be used for this project, generating any of the necessary files.',
    },
    {
        word: 'function',
        docs: 'Defines a function. Must be followed by a code-block. Parameters must have types, optionally having default values. Function calls look like this: `functionName(parameters)`',
    },
    {
        word: 'test',
        docs: 'Defines a test; requires \'tests\' feature. Must be followed by a code-block that contains the test contents.',
    },
    {
        word: 'return',
        docs: 'Set the value that will be returned from this function when it ends. The caller can use this value however it wishes.',
    },
    {
        word: 'dialogue',
        docs: 'If followed by a block, defines a new dialogue scene with the given name.',
    },
    {
        word: 'for',
        docs: 'Runs the following statement or code-block once over every entity that matches a selector at its current position. Functionally equivalent to `execute as <selector> at @s run <code>`',
    },
    {
        word: 'execute',
        docs: 'Begins a vanilla Minecraft execute chain. Can be followed by a statement or code-block, but does not explicitly support the \'run\' subcommand.',
    },
    {
        word: 'await',
        docs: 'Works in async functions. Awaits a certain amount of time, for a condition to be met, or another async function to complete executing.',
    }
];
