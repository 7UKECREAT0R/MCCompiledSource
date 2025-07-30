# Extra Compile Options

There are extra compile options that can control how the project is compiled even further than just "where the files
are emitted and how?" The various extra compiler parameters are briefly shown here and expanded upon further down:

<deflist type="medium" sorted="none">
    <def title="--daemon (-dm)">
        <a href="#compile_option_daemon">Watch the input file and recompile it every time it's written to: generally, a save from a text editor.</a>
    </def>
    <def title="--debug (-db)">
        <a href="#compile_option_debug">Output detailed information about the compilation process.</a>
    </def>
    <def title="--decorate (-dc)">
        <a href="#compile_option_decorate">Emits files with "decoration"; extra useful information that helps make the compiled code much more human-readable.</a>
    </def>
    <def title="--exportall (-ea)">
        <a href="#compile_option_export_all">Exports all functions, regardless if they are used or not.</a>
    </def>
    <def title="--nopause (-np)">
        <a href="#compile_option_nopause">Don't pause at the end of compilation, close the application immediately.</a>
    </def>
    <def title="--variable (-ppv)">
        <a href="#compile_option_variable">Accepts a [name] and [value]. Sets a preprocessor variable.</a>
    </def>
    <def title="--clearcache (-cc)">
        <a href="#compile_option_clear_cache">Clears MCCompiled's temporary cache.</a>
    </def>
    <def title="--server">
        <a href="#compile_option_server">Opens as a language server for the <tooltip term="editor">editor</tooltip> to connect to.</a>
    </def>
    <def title="--search">
        <a href="#compile_option_search">Searches for and compiles all files individually in the current and children directories.</a>
    </def>
    <def title="--ignoremanifests">
        <a href="#compile_option_ignore_manifests">Disables the writing of manifest files.</a>
    </def>
</deflist>

## Daemon {id="compile_option_daemon"}
<deflist type="medium" sorted="none">
    <def title="--daemon (-dm)">
        Watch the input file and recompile it every time it's written to: generally, a save from a text editor.
    </def>
</deflist>

The compiler will remain open after each compilation and watch the input file for changes.
Every time the file is done being written to, it will re-run the compilation with the same settings. This is useful
for getting bare-bones linting with a regular text editor, along with instantly being able to see your changes reflected
in the game without having to press any buttons.

## Debug {id="compile_option_debug"}
<deflist type="medium" sorted="none">
    <def title="--debug (-db)">
        Output detailed information about the compilation process.
    </def>
</deflist>

This isn't generally useful for the end-user unless you like seeing all the inner workings of the compiler as it goes,
or are trying to figure out why a compilation is taking a long time. Debug mode will print:
- All incoming and outgoing WebSocket traffic, if running as a server
- Project name
- Results (tokens) after lexical analysis
    - Reconstruction of code based on those tokens
- All 'statements' are assembled from the tokens
- All 'statements' as they execute and are processed down
    - Such as after compression of an `a + b` operation to `c`
- Compilation time
- All files are written after compilation
- Success status (True or False)

## Decorate {id="compile_option_decorate"}
<deflist type="medium" sorted="none">
    <def title="--decorate (-dc)">
        Emits files with "decoration"; extra useful information that helps make the compiled code much more human-readable.
    </def>
</deflist>

<snippet id="decoration">

Decoration is an infinitely useful option if your final commands are going to be reviewed or debugged by a human manually.
Each command is given a comment with the original code (as if it was pseudocode) to better explain what it's doing,
and all subfunctions are given comments marking where they were called from and why. If statements are given descriptive
comments about what they are checking for, and overall, the entire project is annotated with readability in mind.

<img src="decoration0.png" alt="Original code, depicting a += b * c"/>

<img src="decoration1.png" alt="Compiled code decorated to illustrate the subfunction it creates for the b * c operation."/>

</snippet>

## Export All {id="compile_option_export_all"}
<deflist type="medium" sorted="none">
    <def title="--exportall (-ea)">
        Exports all functions, regardless if they are used or not.
    </def>
</deflist>

In MCCompiled, functions are only emitted if they are used in the source. This makes it possible to include things like
libraries without having to incur the overhead of the library's unused functions making it into the final build.

You can use the [`export` attribute](Attributes.md#function-attributes) to export a single function that needs to be
in the final build. "Export All" is just for cases where you want all files no matter what.

## No Pause {id="compile_option_nopause"}
<deflist type="medium" sorted="none">
    <def title="--nopause (-np)">
        Don't pause at the end of compilation, close the application immediately.
    </def>
</deflist>

Tells the application to exit immediately once done compiling, without requiring an extra key press. Recommended if
running as part of a script, but not recommended if it is the main command of a script.

## Preprocessor Variable {id="compile_option_variable"}
<deflist type="medium" sorted="none">
    <def title="--variable (-ppv)">
        Accepts a [name] and [value]. Sets a preprocessor variable.
    </def>
</deflist>

Set a [preprocessor variable](Preprocessor.md) to a value before compilation starts. This serves as a way of
passing in information through external application, batch script, etc. to the preprocessor. The `--variable` argument
can be specified as many times as needed and follows the format of `--variable [name] [value]`

Values are only partially parsed and don't support things like selectors, dereferencing other preprocessor variables,
or suffixed integers. Here is a list of supported types as of MCCompiled 1.16, in order of checks:
- Booleans (case-insensitive)
- Decimal numbers
- Integers
- Strings (fallback)

> Because of this in-between parsing stage, it's recommended that you sanitize any input going in if it originates from
> a user. It may cause issues if your input is interpreted as a type other than what you expect in your implementation.
{style="note"}

## Clear Cache {id="compile_option_clear_cache"}
<deflist type="medium" sorted="none">
    <def title="--clearcache (-cc)">
        Clears MCCompiled's temporary cache.
    </def>
</deflist>

The cache is located at `%temp_location%`. It contains files that track compilation counts, vanilla files,
etc... If MCCompiled is outputting files for bindings which are too old, you may consider clearing the cache this way.

## Server {id="compile_option_server"}
<deflist type="medium" sorted="none">
    <def title="--server">
        Opens as a language server for the <tooltip term="editor">editor</tooltip> to connect to.
    </def>
</deflist>

MCCompiled will open as a language server for communication with the <tooltip term="editor">editor</tooltip>. It begins
by actively listening for incoming WebSocket connections and then will host the first one that connects. Documentation
about the server's protocol is unpublished as of now; The plan eventually is to move over to an LSP implementation
instead, for easier integration into existing editors like [VSCode](https://code.visualstudio.com/) or [Fleet](https://www.jetbrains.com/fleet/).

The "server" option does not require a file name, as it completely changes the flow of the application.

## Search {id="compile_option_search"}
<deflist type="medium" sorted="none">
    <def title="--search">
        Searches for and compiles all files individually in the current and children directories.
    </def>
</deflist>

The compiler searches for every `%ext%` file in the current and children directories. Each file is compiled individually,
and it is not recommended to try setting the project name manually as it may cause every file to collide. Setting location
still works normally, as the `%project%` variable is subbed out for each file's project name.

> See [here](Compiling-Manually.md#project_name) on how the compiler decides project names when unspecified.

When used along with the [Daemon](#compile_option_daemon) compile option, the compiler will watch for changes in any
`%ext%` file in the current or children directories, rather than the specified one.

The "search" option does not require a file name, as it completely changes the flow of the application.

## Ignore Manifests {id="compile_option_ignore_manifests"}
If MCCompiled doesn't support your particular manifest configuration, you can disable the writing of manifests entirely
while a fix is worked on. By passing this flag, no manifests (including RP regardless of if it exists) will be written
at the end of compilation.