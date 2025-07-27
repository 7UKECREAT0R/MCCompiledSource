# Changelog

## MCCompiled 1.19

**Additions**
- Added [`random()`](Built-In-Functions.md#math-functions) built-in function which supports range bounds, compile-time bounds, and runtime bounds.
- [Localized texts](Localization.md) now have proper contextual names which give you a hint as to where they're used.
  - Previously, they were just `mcc.rawtext.<HASH>` with the text's hash.
  - Now, you get something like: `mcc.func_exampleFunction.value3.actionbar_s`
- [](Localization.md) keys are now sorted alphabetically.
- [.lang files](Localization.md) are truncated at the end to only include one empty line at most.
- Preprocessor variables now support being empty!
  - `$var` now supports having no input to create an empty preprocessor variable.

**Bugfixes**
- Fixed `hasitem` selector property not supporting namespaced items.
- Fixed broken preprocessor commands inside [async](Async.md) contexts.
- Added preemptive error for [async](Async.md) code which will omit invalid output.
- Added explicit errors for use of [`await`](Async.md#awaiting) inside unsupported contexts, like loops.
- Some output names that involve indexes are now 1-indexed, and some exclude the 0.
  - Most prominent case might be: `name` -> `name1` -> `name2` -> etc...
- [](Macros.md) now actually (actually) have documentation support.
- [](Async.md) debug information now only shows if debugging is enabled.
- Fixed edge case in [`$strfriendly`](Advanced-Variable-Commands.md#string-manipulation) with short words at the start of phrases.
  - `the Place of Doom` --> `The Place of Doom`
- Gave item structure names contextual information, and they're now consistent between compilations.
- Gave [`scatter`](Scatter.md) structure names contextual information as well.
- Fixed an issue with [partial functions](Attributes.md#partial_examples) included under folders.
- Fixed a crash when using `execute` with a block inside async functions.
- Proud to announce: The official Web Editor now supports non-ASCII characters (accents, section symbol, etc.)

## MCCompiled 1.18.1
Hotfix for a ton of important production issues, and some new bindings stuff.

**Additions**
- Added new [binding](Attributes.md#bind_examples) type `custom_bool` which accepts a `string: condition` for determining the binding state.
- Added [binding](Attributes.md#bind_examples) `is_day` which checks `query.time_of_day` inside a threshold (unable to sleep).
- Added [binding](Attributes.md#bind_examples) `is_night` which checks `query.time_of_day` inside a threshold (able to sleep).
- Added compile option/editor property `IGNORE_MANIFESTS` which makes MCCompiled not touch manifest files.
    - This is just here for edge cases; most users will not need this.
- Added automatic updates to manifest `min_engine_version` when MCCompiled requires it.

**Bugfixes**
- Rewrote Manifest parser to support script modules and *many more* edge cases.
    - The new abstraction should also allow faster rollouts of fixes/additions for the parser.
- [Value definition](Values.md) duplicates are now ignored if they're identical. This allows you to `define` in macros.
- You are now able to create empty [macros](Macros.md); pointless limitation.
- Fixed wording of errors relating to selectors that should only target players.
- No longer crashes when trying to fetch JSON files from the default pack.
- [`$log`](Debugging.md#logging) no longer errors when linting (but not when compiling).
- Starting the `mc` command's input with `/` no longer causes it to compile incorrectly.
- Using `\n` in format strings with localization enabled no longer fails.

## MCCompiled 1.18
Another feature update, baby! Loops, loads of new exceptions, and async.

**Additions**
- Added [`while`](Loops.md#while) command.
- Added [`repeat`](Loops.md#repeat) command. Differs from the compile-time version as it is runtime-only.
- Added [`countEntities(selector)`](Built-In-Functions.md#utility-functions) function which counts the number of matching entities.
- Added support for checking for spectator mode in selectors.
- Added `--trace` compiler option for tracing the execution path in chat; used only for debugging.
- New bindings for [`bind`](Attributes.md#bind_examples) attribute:
- Added `query.is_baby`
- Added `query.is_carrying_block` (endermen)
- Added `query.is_sitting` (foxes, wolves/dogs, parrots)
- Added `query.is_sneezing` (llamas)
- Added `query.is_transforming` (zombies/husks)
- Added `query.is_tamed`
- Added `query.is_elder` (guardians)
- Added `summon` command; matches vanilla syntax.

**Additions: Errors**

Added loads of errors where previously the code was ignored. If some of your code is causing errors after these changes, it's most likely safe to remove the problematic parts because it wasn't valid; *probably not even being processed in the first place*.
- Added **7 new errors** for conditions.
- Added **3 new errors** for function definitions.
- Added **2 new errors** for [`give`](Giving-Items.md) command.
- Added an error for attempting to apply duplicate attributes to functions.
- Added an error for when tokens at the end of statements go unused (not including comments).

**Additions: [Async](Async.md)**
- [Async functions can now be created](Async.md#defining), allowing you to use the new [`await` command](Async.md#awaiting) inside them.
- `async(global)` makes the scheduler in a global context, not running on a specific entity.
- `async(local)` makes the scheduler run on the calling entity only.
- Does *not* support [return values](Functions.md#return-values); returns an "awaitable"
- Added [`await`](Async.md#awaiting) command. Can be used inside async functions.
- [`await <time>`](Async.md#await_time) waits for a specific amount of time.
- [`await until <condition>`](Async.md#await_condition) waits until a condition evaluates to true.
- [`await while <condition>`](Async.md#await_condition) waits while a condition remains true.
- [`await <awaitable>`](Async.md#await_function) await another async function.
- You cannot await a `local` function from a `global` function.
- Async isn't yet supported in [`repeat`](Loops.md#repeat) and [`while`](Loops.md#while), but support is coming.
- Async is supported with [`if`](Comparison.md) and [`else`](Comparison.md#else).
- [`halt`](Debugging.md#halting-code) and [`throw`](Debugging.md#throwing-errors) properly cancel execution of async functions.

**Additions: [Web Editor](%editor_url%)**

Planning on focusing on LSP for 1.19 and integrating into VSCode, but since the web editor is already VSCode based, these fixes are an essential step regardless.
- Updated to Monaco 0.47.0
- Sticky lines support.
- Enabled code mini-map only when hovered (right-side).
- Set default font to [Jetbrains Mono](https://www.jetbrains.com/lp/mono/)
- Added button to switch to Minecraft Font [(Monocraft)](https://github.com/IdreesInc/Monocraft)

**Changes**
- Changed exporter `udl2.1` to `notepadplusplus` for clarity.
- Overhauled `raw` exporter to output JSON and much more information.
- Added `raw-min` exporter, which is just the minimized version of `raw`.


**Bugfixes**
- Made [`throw`](Debugging.md#throwing-errors) show source code rather than compiled code (including location).
- MCCompiled now internally strips inline comments away to prevent unexpected errors.
- Full-line [comments](Syntax.md#comments) still remain for uses like [documentation](Syntax.md#documentation); just inline comments are stripped.
- Decimals multiplied using [time suffixes](Syntax.md#time-suffixes) are once again interpreted as integers (e.g., `0.5s`)
- [`playsound`](Playsound.md) command no longer requires a selector, defaulting to `@s` if unspecified.
- Fixed [function](Functions.md) re-definitions (e.g., [partials](Attributes.md#partial_examples) or overloads) being interpreted as a call if there were parentheses.
- (Web Editor) Autocomplete no longer triggers inside comments or strings.
- (Web Editor) Unclosed strings are now highlighted.
- All generated file names now use *consistent* camelCase formatting.

## MCCompiled 1.17.1
- Fixed new manifests not being created.
- Fixed GitHub "wiki" URL not directing to the new wiki.
> Both issues found by [Minato](https://www.youtube.com/@MinecraftBedrockArabic) on the [Discord](%discord_url%), thanks!

## MCCompiled 1.17
Dialogue, more marketplace stuff, and loads of bugfixes.

**Additions: [Dialogue Support](Dialogue.md)**
- `dialogue new <name> { ... }`
```%lang%
dialogue new "tradingDialogue" {
	name: "Trader"
	text: "Care to trade? \n§oHe squints at you."
	onClose: {
		print "You leave with purpose. Something wasn't quite right."
	}
	button "Common Pelt" {
		clear @s common_pelt 1
		print "'This will absolutely do.'\n\n+10 Gold"
		gold[@s] += 10
	}
	button "Rare Pelt" {
		clear @s rare_pelt 1
		print "'Exemplary quality.'\n\n+50 Gold"
		gold[@s] += 50
	}
}
```
- Supports code blocks and compiles right into the dialogue file.
- `dialogue open` syntax and `dialogue change` like vanilla.
- Includes compile-time selector validation.
- Support for [localization](Localization.md) natively.
- Support for using `\n` in text, if [localization](Localization.md) is enabled.

> This is because the method used to support newlines requires the JSON component `{ translate: "...", with: ["\n"] }`
{style="warning"}

**Additions**
- Added feature `audiofiles` which allows the [`playsound` command to accept audio file inputs](Playsound.md).
- Location of audio file in relation to project has effect on where it's copied to the RP!
- Audio file is placed in 'RP/sounds' root if no relative path is found.
- Added proper function overloading.
- Added [`partial`](Attributes.md#partial_examples) functions, which when re-defined, are appended to rather than overwritten.
- Added support for `@r`
- Added errors for defining a function with the same parameters.
- Added more compile-time checking to MANY commands/comparisons (especially regarding selectors).
- [Localization](Localization.md) now lets the BP/RP use `pack.name` and `pack.description`
- Updated [documentation](https://lukecreator.dev/mccompiled/docs/about.html)

**Changes**

- [Localization](Localization.md) no longer applies to whitespace strings or strings without letters.
- Removed support for `@i` selector, as it's not used in MCCompiled's [dialogue](Dialogue.md) implementation.
- Changed syntax of `fill` to match vanilla, because wow, it was awful before.
- Missing/unexpected parameters have more informative messages.

**Bugfixes**

- Fixed a bug which prevented more than one [function](Functions.md) from being resolved in a single statement.
- Fixed a bug which prevented more than one [dereference](Preprocessor.md#dereferencing) from being resolved in a single statement (sometimes).
- [Builder fields](Giving-Items.md#attributes) on their own lines now have tokens properly processed.
- [`$include`d](Including-Other-Files.md) files now execute relative to their location, not the caller's location.
- Fixed an issue with [dummy entities](Optional-Features.md#dummies) having incorrect `tick_world` component.
- Updated the `summon` command (exploders and dummy entities) to work with the Minecraft 1.20.51+.
- Declaring functions located in folders as [`auto`](Attributes.md#auto_examples) now works as intended.
- Removed `actionbar times` syntax because it's not a thing.
- Code created by generative functions like [`round()`](Built-In-Functions.md#rounding-functions) is now properly omitted if used in unused code.
- Code created by generative functions no longer generates more than once for the same configuration.
- You can now escape [definitions.def](Using-Colors.md) entries (e.g., `\[color: red]`)

## MCCompiled 1.16
Testing, compiler functions, and some heavily updated data-driving stuff!

**Additions: [Compiler/Runtime Functions](Built-In-Functions.md)**
- Added `glyphE0(x, y = 0)` function for getting a character on the E0 glyph at compile time.
- Added `glyphE1(x, y = 0)` function for getting a character on the E1 glyph at compile time.
- Added `min(a, b)` - takes lower of two values
- Added `max(a, b)` - takes higher of two values
- Added `sqrt(n)` - square root *(no runtime yet)*
- Added `sin(n)` - sine *(no runtime yet)*
- Added `cos(n)` - cosine *(no runtime yet)*
- Added `tan(n)` - tangent *(no runtime yet)*
- Added `arctan(n)` - angle which has a tangent of n *(no runtime yet)*
- Added `round(n)` - rounds to nearest int
- Added `floor(n)` - rounds down to nearest int
- Added `ceiling(n)` - rounds up to nearest int
- Added `getValue(str)` - gets a value with the given name; acts the same as a regular value.

**Additions: [Tests](Testing.md)**
- Added [`tests`](Testing.md) feature, which enables tests.
This is optional because it creates folders/files which would normally be unwanted.
- Added `test` command which will create a test, similarly to how a function is defined.
- Tests cannot be called, nor do they show up in the editor's autocomplete.
- Tests are automatically ALL run sequentially in `/function test`.
- Added [`assert`](Testing.md#writing-a-test), which checks the input condition and halts execution if it fails.
- Added `throw`, which is same as a guaranteed assertion fail with a custom message.

**Additions**
- Added `$unique` to reduce an array to its unique values. (a != b)
- Added `$assert`, which will throw a compile-time error with extra info if its condition evaluates to false.
- Added the ability to clarify variables using `*`, to select all registered score-holders.
For example, setting the score of ALL players, including offline ones: `score[*] = 0`
- Added feature `autoinit` which automatically calls `init` every time the project is compiled. :)
- Added `$append` which appends items to the end of a preprocessor variable.
- Added `$prepend` which prepends items to the start of a preprocessor variable.

> These new methods are fully optimized, as we've pulled away from the array-based implementation of preprocessor variables and fully implemented them as space-conscious lists instead. There's almost no memory overhead with the new implementation.

- Added `dummy removeall` syntax for removing all dummies (or filtered per-tag).
- Documentation now resolves [preprocessor variables](Preprocessor.md).
- Specifying a JSON array now works in directives that allow array inputs (e.g., `$add`, `$sub`).
- Added compile-time function [`getValue(str)`](Built-In-Functions.md) which returns a value that can be read/written to.
- Added support to [index](Indexing.md) preprocessor variables `variableName[index]`.

> The dereference is implicit. Reason being that these two snippets have different meanings:
> - `$variableName[...]` dereference `variableName` and then index result.
> - ` variableName[...]` index `variableName` and then dereference it.

**Changes: [`$iterate` Rework](Compile-Time-Loops.md)**

> Previously, the engine would iterate over any JSON arrays contained within the preprocessor variable. It was inconsistent and unexpected behavior. The new behavior has two types of input, and follows the same rules for both.
> - If the identifier of a preprocessor variable is specified, it will now **only iterate over its elements**.
> - Now iterates over JSON object property names if one is specified. Previously would error.
> - To iterate over a JSON array/object, dereference an element of a preprocessor variable.

**Changes**
- You no longer have to specify decimal precision when using [type inference](Values.md#defining-values) with a decimal literal.
- [`define`](Values.md#defining-values) now places its definition commands in the initialization file.
- Feature [`uninstall`](Optional-Features.md#uninstall) now emits its function in the root directory instead of the ./compiler folder.
- Operations `$add` `$sub` `$mul` `$div` `$mod` and `$pow`:
- Operands (command inputs) now loop to fill the number of items in the given PPV.

> This means that doing `$mul value 2` will multiply *all* values by 2, rather than just the first.

- Operations [`$strfriendly` `$strlower` and `$strupper`](Advanced-Variable-Commands.md#string-manipulation) now ignore non-string values.
- [`$strfriendly`](Advanced-Variable-Commands.md#string-manipulation) formatting improved.
- Changed [`init`](Hello-World.md#init) file to not include project name, reserved "init" function name.
- Greatly improved [`$if`](Comparison-compile-time.md#using-if) to produce better comparisons more consistently.
- Displaying [decimal values](Types.md#decimal) now appends missing 0s to the start of the number.

> Previously would show `0.1` instead of `0.001`, for example.

- Performing an `int ? decimal` operation no longer rounds the decimal, flooring instead.
- Moved codebase over to use *much more precise* decimal type for more consistent use.

**Bugfixes**
- Fixed `/setblock` invoking command(s) still using outdated block data format.
- Fixed parameter-less macros sometimes breaking the web editor.
- Fixed case where adding `.0` to the end of a number didn't convert it to a decimal.
- Fixed an issue where the initialization file would be emitted even if no commands were needed.
- Fixed an error when using two or more features at once. (yeah, seriously)
- `give` command no longer uses a structure when using properties which can be solved using JSON.
- `give` command now consistently loads structures at the right place.
- `give` command now generates stable structure names.
- Added guard to prevent `auto` attribute from being added to functions with parameters.
- Changed `dummies` feature to set `dummy` preprocessor variable on enable.

## MCCompiled 1.15.1
- Fixed Firefox support for web editor. Thanks cen0b

## MCCompiled 1.15
Much better control flow and API support, along with loads of important bugfixes. No huge features in this update in preparation for all the new types stuff coming in `1.16`. Types were completely reworked, so please report any bugs ASAP!

**Additions**
- Optimized output by only including functions that were used in the project
This allows users to use APIs created in MCCompiled in a more efficient way without bloating their project.
- Web IDE: Macros now show up in autocomplete and fully support templating.
- Added `bind('query.is_sleeping')` to possible bindings.
- Returning inconsistent types in functions now uses conversion instead of throwing an error.
- Reminder: the first instance of `return` always sets the function return type.
- Swapping `><` now converts types if they need to be converted.
- Conversion now only respects the left-hand side of the operation. (thanks SuperFluffyGame!)
- New `export` attribute which exports a function regardless if it's used or not.
- New `auto` attribute which makes a function run every tick/interval (if specified).
- Feature `uninstall` now also removes all temps and return values.

**Changes**
- (last update) Value names are no longer hashed unless they are longer than 256 characters.
- Function parameters are now global by default, making them work good when used with tick.json.
- Return values are now global by default, making them work good when used with tick.json.
- Removed unnecessary quotation marks in output to make Blockception happy (and me).
- Swapped arguments of `setblock` command to match vanilla.
- Much better handling of type-related errors.
- Re-evaluated and re-implemented where and how temps are used. More will be used now, but there is less chance of temps overwriting each other when they shouldn't.

**Bugfixes**
- Fixed `--help` formatting errors.
- Daemon detects file changes and handles inputs faster.
- 256 character limit for values now actually works.
- Function parameters that exceed the character limit no longer have bugged names.
- Function-call setup subfunctions no longer have `.` characters in their name.
- Global text commands now properly display values local to the player they are displaying to.
Consider the code `define int score` and `globalactionbar "Your score is {score}"`.
Previously, it would show all players the score of the player that executed the command. Now, it will show each individual player's score. This change only affects the `globalX` commands, not the regular ones in order to retain control with the programmer.
- Language server now provides information about macros. This field was accidentally left empty.
- Language server no longer crashes with big projects.
- And has better overall support for browsers.
- Added missing enchantments `mending` and `swift_sneak`
- Fixed grammatical issues with decoration of if-statements.
- `OPEN COM.MOJANG` button in the IDE works again.

## MCCompiled 1.14
Dialectization, dynamism, and decoration! Oh, and a dump-truck of bugfixes.

**Addition: Localization Support**
- New `lang <locale>` command for setting the active language.
- All strings specified in FString fields will automatically be placed in the active lang file.
- Translation components will automatically be used when an active language is set.
- Added preprocessor variable (boolean) `_lang_merge` to determine whether to merge equal language keys.

**Addition: Dynamic Calls**
- Functions can be defined using a string as the name of the function.
- A function can be called dynamically by using the new `$call` `<name>` `[parameters]` directive.
- Compiles to the exact same as calling a function regularly. i.e., `name(parameters)`

**Additions**
- `tp` command now has *full* vanilla parity with facing support and checking for blocks.
- Preprocessor variables can now be used in selectors.
Note that the implementation used is identical to the way strings are, so the support is still shallow. No inline operations are run, only raw `$dereferences` are.
- *Massively better* decoration (`-dc` compile option) to help people with reading/debugging output.
- All subfunctions now contains detailed description of what it is doing and where it is invoked.
- Unfortunately hits performance (even without decoration enabled), but only by a bit.
- Makes following the code tree much easier.
- No longer causes bugged code with multiline comments.
- More consistent newlines added to better separate ideas.
- Value and function documentation now shows up at the top of their respective locations.
- Added `DONT_DECORATE` attribute to language.json to indicate directives should not be displayed.
- Added `DOCUMENTABLE` attribute to language.json to indicate directives that can be documented.
- Function folders can now be done by using dots. For example, `math.abs` will create `abs` under the `math` folder.
This is a million times better than using a whole attribute just to set a folder. This also makes it so that your functions that *are* held behind a folder can be referenced using their fully qualified name rather than having many functions with the same name.
- Added the `extern` attribute that can be added to functions that are already in the BP without changing their contents.
- Added new support for project "properties" that allow you to change debug/decorate from the web editor. (gear icon)
- Language server now closes existing files when a new client connects to it to prevent from accidentally overwriting it.

**Changes**
- Renamed `minecraftversion` preprocessor variable to `_minecraft` for consistency with the others.
- Renamed `compilerversion` preprocessor variable to `_compiler` for consistency with the others.
- The syntax of `if` `<selector>` now only accepts @s selectors to dial in its meaning.
- Removed the `folder(...)` attribute in respect for the much nicer period support.
- The threshold for values to hash their names has been increased from 16 to 256 to fit the new 1.20 changes.

**Bugfixes**
- Fixed incorrect behavior/resource dependencies, in some cases.
- Prevented a crash if a manifest data module did not contain a description.
- Fixed `if` `not` `<boolean>` pattern throwing an error.
- Fixed loaded (existing) manifest files not having a proper output location.
- Fixed an issue that prevented Decoration from being enabled on the server.
- Save/load dialog boxes now always open on top of the IDE window. (WOO!)
- Comments no longer trip up if/else statements.
This was meant to be fixed in 1.12, but it still failed when no brackets were used.
- Return statements now properly terminate single line if/else statements.
- Disabled buttons in the Web IDE no longer change the cursor.

## MCCompiled 1.13
MoLang binding, bugfixes, and new IDE features!

**Changes**
- BREAKING: When using development folders, MCC now names folders like so: `name_BP` and `name_RP`.
- Removed size limit on `scatter` command, replaced with a warning.
- Scatter command now runs "shallowly" when linting, so it doesn't impact performance.
- Variable definitions now allow attributes before and after the name.
- Function definitions now allow attributes before and after the name.

**Additions**
- New attribute `bind(query)` that will bind a MoLang query to a value. Limited support.
See `language.json > bindings` for all of the current bindings.
- Including a comment behind a function definition will now count as documentation.
- Including a comment behind a value definition will now count as documentation.
- Including a comment behind a macro definition will now count as documentation.
- Documentation for all objects now show up in the IDE.
- Introduced server spec 5.7 with symbol documentation and version information.

**Bugfixes**
- Fixed attribute functions sometimes being broken and causing weird behavior.
- Fixed slight issue with terminal coloring.
- Scatter command works again.
- Fixed the encoding issues with definitions.def.

## MCCompiled 1.12
Bugfixes, bugfixes, bugfixes + internal comparison overhaul!

**Bugfixes**
- Reverted a small linting optimization that caused it to fail silently sometimes.
- Fixed an issue when trying to compare using aliased values (parameters or names longer than 16 chars)
- Fixed some comparison subcommands not properly sending out subcommands.
- `if` with compile-time comparisons now works when a single statement is given.
- `if`/`else` statements now tolerate having comments between them and their attached code.

**Comparison Bugfix**

Comparisons using if/else statements now properly stores the result of the first evaluation, meaning that the if clause and else clause cannot both be triggered if the parameters of the condition changes. The following code block had the following behavior in prior versions:
```%lang%
define int number = 105

if number > 100 {
    print "Number was greater than the maximum bound."
    number = 100
} else {
    print "Number was in bounds."
}
```
```
Number was greater than the maximum bound.
Number was in bounds.
```
This has been fixed for all versions 1.12+ as it now reuses the result of the first evaluation.
Bugs may happen as a result of this change, but ultimately it is the direction I want to go for future code
flow implementations (while loops, async file splitting, etc..)

## MCCompiled 1.11
Just a small set of hotfixes today with customizable time formatting and new internal features.

**Additions**
- Added the `_timeformat` preprocessor variable that indicates how times should be formatted.
The default is "m:ss". Valid letters include `h`, `m`, and `s`. The number of single characters indicate the minimum number of digits (padded with 0's if not filled).
- Added `_realtime` preprocessor variable, a string that shows the real-life time of compilation.
- Added `_realdate` preprocessor variable, a string that shows the real-life date of compilation.
- Introduced a warning when defining preprocessor variables starting with underscores.
- Added command categorization in language.json.
- Added new exporter `mc-compiled --syntax markdown` for the new cheat sheet on the wiki.

**Bugfixes**
- Fixed FStrings not working with global `time` variables.
- Setting a precision on decimals works again. It was defaulting to 2.
- Fixed bug causing globalprint, globalactionbar, and globaltitle to parse incorrectly.
- Fixed "exploders" feature being the wrong name.
- Users can no longer write `feature no_features`, resulting in a crash (?)

## MCCompiled 1.1

*Note that this is an update with MULTIPLE BREAKING CHANGES. I reconsidered a lot of parts of the language's design and landed on a result which better fits genuine use of the language. There is also a lot of new functionality for things I was really dreading doing and resorted previously to lazy implementations, such as a lack of indexing.*

**Changes**
- New branding!
- Improved performance in some areas, hit it in some other areas. All in the name of features.
- Definitions updated for 1.19.70, and potions have been completed.
- Much more informative pattern-mismatch errors. (missing arguments, wrong arguments, etc.)
- Commas are now *completely* ignored by the tokenizer, so they can be used to visually separate whatever you please.
- Heavy linting optimizations.
- Improved error handling with brackets.
- The `$log` command now accepts any input, including multiple arguments.
- Improved performance with strings utilizing preprocessor variables.
- Symbol `&` changed to keyword `and` to be more consistent with the rest of the language.
- Remove processing of multiple directives on the same line. This was dumb.
- `place` command changed to `setblock` to remove collisions with `if block`.
- `tag single` works with much full consistency now.
- Places generated structures in their own folder. (`give` and `scatter` commands)
- Swapped arguments around to match `$var` in the following directives:
  - `$len`
  - `$sum`
  - `$median`
  - `$mean`
  - `$strlower`
  - `$strupper`
  - `$strfriendly`
    > This isn't exactly a welcome change, but it was silly and unintentional to have them in a different order in the first place.

- "null" entities and their commands have been renamed to `dummy`. This was done to make way for the new `null` literal.
- Structs were removed since they served no real purpose yet.
- Implicit conversions (compiler-side) from decimal to int now *floors* instead of *rounds* for consistency.
- Removed dummy classes in favor of simply giving an option to tag them on spawn. New syntax:
- `dummy` `create` `<name>` `[tag]`
- `dummy` `single` `<name>` `[tag]`
- `dummy` `remove` `<name>` `[tag]`

**Additions**
- @initiator is now supported, @i for shorthand.
- Web IDE: Font can now be resized with Ctrl + Zoom
- New `uninstall` feature which creates a function to remove all this project's scoreboards/tags from the world.
- New `--maxdepth` command-line argument. Sets max allocated depth.
- New `--version` command-line argument. Prints out compiler version.
- New `--variable` command-line argument. Sets starting preprocessor variable.
- Completely new and improved if-statement format. See bottom of changelog for more information.
- Now properly sets up dependencies with manifest generation, additionally pulling UUIDs from existing manifests.
- Preprocessor variables now support storing range values.
- `$len` now accepts strings, returning the proper length.
- New `null` keyword which represents 0 under any type. Now the default return value for functions.
- All declaration statements (define, function parameters, etc.) now use type inference if no type is specified.
- FString "expressions" now fully parse and evaluate the code inside them.
- Strings can be declared using double *or* single quotes now, and they will only be closed by the same character.
- `$repeat` now accepts range values (inclusive).
- `$strfriendly`, `$strupper`, and `$strlower` now treat a single parameter as both input and output.
- Added a unit test library for testing every language feature. This should help catch bugs before release builds.
- Added `$sort` which sorts the values in a preprocessor variable either ascending or descending.
- Added `$reverse` which reverses the order of the values in a preprocessor variable.
- Added aliases to certain commands.

**Bugfixes**
- Re-added enchantments to definitions.def... oops.
- Range literals now correct themselves if min > max.
- Tokenizer now distinguishes between a negative relative coordinate and subtracting a value.
- Error output is now sent to standard error output, like it's supposed to. This should fix regolith's formatting as a result.
- Tokenizer no longer implodes when an @ is followed by the end of the file.
- $repeat can no longer repeat non-runnable statements like blocks/comments (standalone).
- Rotation comparisons no longer overlap each other.
- Only boolean values can be used in `if x {}` format.
- Fixed `damage` command not working when damage type was specified.
- Compile time measurements no longer include file read/write times.
- Decimal number interoperation now properly converts precision for all operations.
- Always converts to the left-hand side's precision in the operation.
- Compiler now gives a proper error when dereferencing a PPV that doesn't exist.
- Fixed a catastrophic issue when comparing a "constant and value" in that order.
- Two functions with shared parameter names will no longer cause issues. Now uses aliases/hashing.
- Macros now throw exceptions at both their source location and the caller location.

**Web IDE**
- Rewrote language server from the ground up and included complete documentation (now v5.6).
Now uses a WebSocket implementation for communication with the language server rather than HTTP. Accessible by third-party apps, and documentation will remain consistent across versions. We gain a lot of context-independent control with this new method... and better data transport overall.
- No longer errors when trying to display a fatal exception.
- Saving/loading projects has moved entirely to the server side, meaning **all browsers are now supported.**
- Now sets working directory to the file which is loaded/saved to, meaning 'includes' and 'JSON files' are usable.
- Added a new "extras" button with many new actions and links.
- Top-bar buttons are more consistent, and better reflect the state of the editor.
- Dimmed full-screen alerts to see better over any code, and made animations much more responsive.
- Much snappier autocomplete results.
- Debug improvements.
- Compiler-defined functions are now correctly identified in the symbol description.
- Implemented dynamic project metadata. Project name is now independent of file name.
This is just the first of many steps which will improve the IDE-server experience. Metadata is stored in the source file, but is handled entirely by the server so it doesn't have to be seen by the user.

**New IF**

If statements were completely reworked as they felt disconnected from the rest of the language. If-statements still support `and` and `not` keywords.

Additionally, the new if-statements now leverage the new execute format introduced in 1.19, meaning that they now work like actual if statements rather than glorified selectors. This comes with a massive runtime performance boost!

The new commands are as follows and are documented online:
- Same scoreboard comparisons stuff.
`if` `<boolean>` - checks if Boolean is true (or false if `not` was specified.)
`if` `<score>` `<operator>` `<value>` - checks a value.
- Different comparison method more focused on the dominant type, selectors.
`if` `<@selector>` - filters entities
`if` `count` `<@selector>` `<operator>` `<value>` - checks selector match count
`if` `any` `<@selector>` - checks selector for any matches
`if` `block` `<x>` `<y>` `<z>` `<block>` `[data]` - checks for block

**Indexing and Clarifiers**
- Added indexing/clarifier operator `[value]`  that can be used on various things.
- Indexing can access a component/item from an iterable value:
- `$get x n` has been removed in respect for the new `x[n]` syntax.
- Indexing a string will return a single character.
- Indexing a range can return the 0th/1st element from it.

- Clarifying is used with values to "clarify" the target.
- Values default to @s (executing entity)
- Values can be clarified to change who they point to.
*This is intended to be a good solution to cross-entity interaction. Using clarifiers, you can perform operations between entities rather than being forced to work on the executing entity.* Example:
`points = points[@e[type=cow,c=1]]` sets @s's points to the points of the nearest cow.
`total[@p] += total` adds @s's total to the nearest player's total.

**JSON Improvements**

*Hopefully these changes can make using JSON with MCCompiled much more accessible for actual code examples. Indexing JSON should feel much more like an actual language.*
- JSON values can be indexed using `[n]` and `[string]` now.
- `$json` still works as before, but can now load partial/incomplete values.
If you want, you can just load the root of the JSON and use indexing to do all the work!
- `$json` has some more lenient syntax now. Notice that 'path' is now optional.
- `$json` `<string: file>` `<id: result>` `[string: path]`
- `$json` `<json: json>` `<id: result>` `[string: path]`
- `$iterate` now works on JSON arrays.
- `$len` now can get the length of a JSON array.

## MCCompiled 1.04
**Changes**
- Builder fields can now be placed on proceeding lines after a statement.
- If a debugger is running, handled exceptions will pass-through to allow easier debugging.

**Additions**
- Detailed descriptions and a bunch of extra info in language.json.
- Syntax has been officially "documented" and is now able to be exported to three unique language targets using `mc-compiled --syntax`.
- Added an `explode` command with a new entity generator.
- Requires `exploder` feature to be enabled.
- Added `clear` command.
- Added `effect` and `effecth` command for shown/hidden particles.
- Capability of launching MCCompiled as a compiler/linting server which sends over code information.
- Created protocol for launching an MCCompiled server through URL: **mccompiled://server**

**Additions: Web IDE**

https://7ukecreat0r.github.io/mccompiled/editor.html
Initial release of the completely client-sided web IDE!
- Get live code errors as you type.
- Autocompletion for all language keywords.
- Autocompletion and details for variables/functions.
- Compile straight into development folders with the click of a button.
- Easy saving/loading of files, as well as project naming.
> NOTE: Files only work on supporting browsers: <https://caniuse.com/?search=Window.showSaveFilePicker>

**Bugfixes**
- Mistyped function names now have proper error messages.
- Fixed issue with if-statement commands not aligning to selected properly.
- Proper errors when if-statement (selector transformer) is given invalid option.
- Statements requiring following statements/blocks no longer throw unhandled exceptions.
- Parenthetical (squashable) statements being `$repeat`ed no longer causes unexpected errors.

## MCCompiled 1.03
Most of the features here are changes I wished existed when I was working on a larger project. I also hit a couple items off my bucket list and patched some important bugs. I planned for this update to be a hotfix, but uh... you see how that turned out

**Changes**
- Null command no longer has rotation parameters since it caused unexpected behavior.
- Changed null subcommands:
- `null create <string: name> [string: class] [pos: x, y, z]`
- `null single <string: name> [string: class] [pos: x, y, z]`
- `null remove`
- `null remove [name] [class]`
- `null class <class>`
- `null class remove`
- Changed compiler-generated functions folder name to "compiler" for clarity.
- `count` and `level` if-statement conditions use proper comparison arguments now. (\<, \>\=, \=\=, etc..)

**Additions**
- Nulls can now have "classes" which behave similar to tags.
- Added new if-statement clauses:
- `if null` - filter for all null entities
- `if null [name]` - filter for all null entities with a certain name.
- `if class <class>` - filter for all null entities with a certain class.
- `if position <x|y|z> <operator> <number>` - compare position
- `if position <x> <y> <z>` - check position
*These can be placed in tandem to select any combination of nulls.*
- Added `-ppv` compiler option to set variables from command line.
- Added new command `for` which acts as a way of natively executing over entities.
- Made position, rotation, count, and level if-statements use proper comparison arguments. It just feels SO much better.

**Bugfixes**
- Stop normal null creation from behaving like a singleton.
- Made selector offsets work more consistently outside if-statements.
- Fixed if-statements where relative coordinates were being aligned to individual entity rather than the executing one.
- If root file is empty, it will now contain project info to prevent MC compile errors.

## MCCompiled 1.02
**Changes**
- The damage command now additionally accepts three coordinates for the attacker parameter
- Selectors can now be specified as strings in the new formats outlined on the cheat sheet.
- Strings that are in selector format (e.g., "@e[name=Bogey]") will now be parsed during conversion to selector.
- Language commands (directives) are now held in a JSON file and parsed on startup.
- Changed how 'blocks' operate internally for better stability in the future.
- Enum parser was improved and holds more information.
- Integers using time notation (32m, 5s, etc...) now cache to ensure accurate scaling across commands.
- Removed pattern validation for if-statements.
- Internally reworked if-statements and made its processing system modular and portable.
- Moved "if limit..." to its own "limit" command since it didn't make a whole lot of sense.

**Additions**
- New "null" command for creating/removing dummy entities.
- `if any ...` will check if any entities match a selector.
- `if count ...` will check if the number of entities a selector matches is in a range.
- Managed entity names can now be used in `select`.
- You can now use multiline comments with \/\* and \*\/

- Range arguments such as 5.., 1..100, !..6, etc. are now parsed and proper language objects.
- Book content and leather armor support for give command.
- New "tag" command for interacting with entity tags.
- `hasitem=` is now parsed in selectors.
- `if item` and `if holding` for hasitem interaction.
- Following `select ...` by a block will push/pop the selector.

**Bugfixes**
- When else statements throw, they will show the correct source location.
- Commands no longer override internal enum values.
- Enum parser now accepts values using periods.
- Fixed an issue where selectors were sometimes improperly merged if a manual selection was performed.
- Limit now actually works when custom selectors are specified.

## MCCompiled 1.01
**Changes**
- Removed the `--basic` compile option.
- Reworked file system from the ground up--mostly internal changes.
- Regolith filter support
- Parentheses now affect squashing order (conforming to PEMDAS).

**Features**
- Added `offset` identifier in if-statements to allow execute offsets.
- Added `--search`, searches subfolders and compiles all .mcc files.
- Added `--outputbp`, sets behavior pack output folder.
- Added `--outputrp`, sets resource pack output folder.
- Added `--outputdevelopment`, outputs to com.mojang.
- Added `$iterate` which acts as a preprocessor for-loop. Allows for better JSON support.
- Functions can now have default selectors.
- `init` now can take multiple variable inputs.

**Bugfixes**
- Fixed small issue with fstrings unnecessarily aligning when they shouldn't.
- No longer generates resource pack unless necessary.
- Passing preprocessor variables (still denoted with $) into macro calls will now properly transfer array values.
- Fixed misalignment with stacked inline function calls.

## MCCompiled 1.0
-  Initial release
