# Compiling Manually

When you need to control how your project is compiled, it's best to do it from the command line. There are lots of
parameters that can adjust how your code is compiled and emitted, beyond what you can do with just the editor.

> [List of all compiler options not listed here](Extra-Compile-Options.md)

## Simple Compilation
Compiling a `%ext%` file is as simple as running the command:
```text
mc-compiled [file] [args...]
```
Doing this will open the file, compile its contents, and then write the output to the current directory.
If the result of a previous compilation is in the directory, then MCCompiled will always amend to that output rather
than create a new one.

For example, if you exported a file named `egg%ext%`, it would not repeatedly create new `egg` Behavior Packs every time
you compiled. MCCompiled would instead only create the Behavior Pack the first time, then amend it each time after.

## Changing Output Location
You have control over where both the behavior pack and resource pack end up, as well as an option for targeting the
Minecraft Bedrock development folders if needed.

### Manual Location
Specifying manual location can be done using input parameters; one for behavior pack location, and one for resource pack location.
<deflist type="medium" sorted="none">
    <def title="--outputbp (-obp)">
        Sets the output location of the behavior pack.
    </def>
    <def title="--outputrp (-orp)">
        Sets the output location of the resource pack.
    </def>
</deflist>

When using output location, you can use `%project%` to denote the name of the project being exported.

## Development Location {id="dev_folders"}
If you want the behavior and resource pack to be written into the `development_behavior_packs` and `development_resource_packs`
folders respectively, there is a single compiler parameter that *does not take an input.*
<deflist type="medium" sorted="none">
    <def title="--outputdevelopment (-od)">
        Sets the output location of both the behavior and resource packs to the Minecraft <tooltip term="development_packs">development</tooltip> folders.
    </def>
</deflist>

> The location is based on the stable (retail) version of the game, not preview or developer builds. If this is needed,
> please open an issue on [GitHub](https://github.com/7UKECREAT0R/MCCompiled/issues) along with the folder path to these
> versions of the game from `%localappdata%/Packages`.

{ignore-vars="true" style="warning"}

## Project Name {id="project_name"}
The "Project Name" is what defines how the code is compiled to the various files; specifically, the paths of the
output folders, and the name of the project in the `manifest.json` files. Outputting to the <tooltip term="development_packs">development folders</tooltip>
uses the fairly standard format `%project%_BP` and `%project%_RP`.
When manually setting output, try to always use `%project%` to denote the file name in case you ever change it.

By default, the project name is a stripped/lowercase version of the root file name; as in, the one passed into the compiler.
You can set it manually, however.

<deflist type="medium" sorted="none">
    <def title="--project (-p)">
        Sets the name of the project being compiled, instead of the stripped file name.
    </def>
</deflist>

## Extra Compile Options
There are also some less useful options you may consider looking at to see if they fit your use case. The goal is
eventually to be able to fully cover most use cases that niche users would have, so if there's something missing, open
an [issue](https://github.com/7UKECREAT0R/MCCompiled/issues) or even submit [your own PR](https://github.com/7UKECREAT0R/MCCompiled/pulls)
in the GitHub.
> [List of all compiler options not listed here](Extra-Compile-Options.md)