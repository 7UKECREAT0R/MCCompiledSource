# Optional Features

<primary-label ref="runtime"/>

<link-summary>
MCCompiled has optional features like Dummies, AutoInit, Exploders, Uninstall, Tests, and Audio Files.
</link-summary>

MCCompiled contains additional features which are disabled by default due to the way they affect compiled output.
Because these features being implicitly enabled would be off-putting and potentially unwanted for developers, we decided
to make them opt in only.

## Dummies {collapsible="true" default-state="collapsed"}
```%lang%
feature dummies
```
Dummy entities are a feature with an associated command that lets developers spawn, manipulate, and despawn invisible "marker"
entities in the world. Dummy entities cannot be seen by players or interacted with.

> Dummy entities also tick a small area of the world around them to keep it loaded at all times; thus, dummy entities
> cannot be lost to unloaded chunks.

### Spawning Dummies
Spawning a dummy is done using the `dummy create` command. Its syntax is:
- `dummy create <string: name> [string: tag] <coord: x> <coord: y> <coord: z>`
- `dummy single <string: name> [string: tag] <coord: x> <coord: y> <coord: z>`

If the tag parameter is specified, the dummy will be created pre-tagged, particularly useful since Minecraft doesn't
support summoning entities with tags on them.

#### When to use Single
The `single` subcommand ensures that all other dummies with the same name are despawned. You're guaranteed that if you
use the `single` subcommand, there will only ever be, at maximum, one dummy with the given name.

### Removing Dummies
The syntax to remove dummy(s) is:
- `dummy remove <string: name> [string: tag]`
- `dummy removeall [string: tag]`

If the tag parameter is specified in either case, only dummies with the matching tag will be removed.

### Using Dummies Elsewhere
#### By Name
Commands which require selectors also accept strings. If the string matches a known name of a dummy that has been
previously spawned, it will automatically be converted to the correct selector for that dummy.

#### ...But Manually
When dummies are enabled, the preprocessor variable `dummy` is set to the identifier of the entity type, allowing you
to use it in other parts of your code:
- `@e[type=$dummy,name=example]`
- `@e[type=$dummy,name=example,tag=some_tag]`

## AutoInit {collapsible="true" default-state="collapsed"}
```%lang%
feature autoinit
```
When you make a change to your code, which adds any new scoreboard objectives, you generally need to re-run the `init`
function in-game. This can become tedious over time, as well as being *downright unmaintainable* for dynamic worlds.

The AutoInit feature keeps track of your current build and re-runs `init` every time a new build is detected. This
feature is especially useful in Marketplace deployments or rapid iteration while code is being developed.

All you have to do is enable the feature, and it gets to work immediately. It's not enabled by default as it:
1. Tracks build number, which may be unwanted.
2. Adds a per-tick check which may slightly impact performance.
3. Adds an extra function to the root of the functions folder, `_autoinit.mcfunction`.

## Exploders {collapsible="true" default-state="collapsed"}
```%lang%
feature exploders
```
Enables the use of the `explode` command. Requires the creation of an entity
along with event code for all the different presets you might make for it.

This feature is disabled by default because it generates an entity file along with quite a few events/component groups.

### Creating Explosions
The syntax of the command is:
- `explode [coord: x] [coord: y] [coord: z] [int: power]`

    `[int: delay] [bool: causes fire] [bool: breaks blocks]`

And an explanation of each non-standard parameter is as follows:
<deflist type="narrow">
    <def title="power">
        The power of the explosion. For reference, a creeper's power is 3, primed TNT is 4, and a charged creeper is 6.
    </def>
    <def title="delay">
        The number of ticks to delay the explosion.
    </def>
    <def title="causes fire">
        If the explosion should spawn fire.
    </def>
    <def title="breaks blocks">
        If the explosion should break blocks.
    </def>
</deflist>

## Uninstall {collapsible="true" default-state="collapsed"}
```%lang%
feature uninstall
```
The "uninstall" feature creates a file called `uninstall.mcfunction` which contains everything needed to eject the addon
from the world as closely to perfect as possible. The following is performed when `uninstall` is run in-game:
<procedure title="Uninstall Procedure">
    <step>If <a anchor="dummies">dummies</a> are enabled, remove all of them from the world.</step>
    <step>Removes all temporary/compiler-generated scoreboard objectives.</step>
    <step>Removes all user-created scoreboard objectives.</step>
    <step>Removes all known tags from every loaded entity.</step>
</procedure>

<warning title="Leftover Data">
Unloaded entities will keep any tags created by the addon, and may have to be cleaned up manually. Additionally, if the
<a anchor="autoinit">AutoInit</a> feature is enabled, its data will remain in the world to prevent from everything
being automatically recreated.
</warning>

## Tests {collapsible="true" default-state="collapsed"}
```%lang%
feature tests
```
Enables the use of tests and the generation of all needed files.

[See the full page on how to write and use tests.](Testing.md)

## Audio Files {collapsible="true" default-state="collapsed"}
```%lang%
feature audiofiles
```
Enables the `playsound` command to accept audio files directly and generate the `sound_definitions.json` file
automatically. Will copy the audio files into the resource pack every compilation.

[See the full page on how to use audio files in MCCompiled.](Playsound.md)