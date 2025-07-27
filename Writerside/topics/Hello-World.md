# Hello, World!

Once you have [installed](Installation.md) MCCompiled, it's time to get something compiled and running.

## Creating a Project
Projects in MCCompiled are just `%ext%` files. These files can [include](Including-Other-Files.md) other files, and
that determines the project's structure. When compiling, you always want to compile the root file.

To begin, create a file called `first_project%ext%`

## Opening in the Web Editor
[**Open the Web Editor in your browser**](%editor_url%)

The first step is to get an instance of the server running on your system. Press "CONNECT" to attempt to connect to one.
If there is no server running, you will be prompted if you would like to open one. Pressing "YES" will open a new
MCCompiled server on your system. Now, when you press "CONNECT," the <tooltip term="editor">editor</tooltip> should
connect to it. *Congratulations, you now have live errors and compilation!*

![Video showing how to launch and connect to a language server in the editor.](animation_editor_connect.gif)

## Opening Project
You can press the "OPEN" button in the <tooltip term="editor">editor</tooltip> to open an existing `%ext%` file as a
project. Try pressing the button and choosing your newly created `first_project%ext%` file. It will open in the editor,
but it's currently empty. At this point, you can now do a couple of things:
1. Change the project name in an editable field under "PROJECT NAME."
2. Change the project settings by pressing the gear icon.

Your project name and settings are saved automatically inside the `%ext%` file as comments at the top of the file. 
Because of this, your settings can be restored later if you re-open the same project.

> NOTE: If you don't see a file open window upon pressing "OPEN," minimize all windows and make sure it's not below them.
> Sometimes Windows likes to send the window to the back for no apparent reason.
{style="warning"}

## Writing the Code
Let's make a simple `/tellraw` statement that says hello to the player that runs it. MCCompiled has the command `print`
which builds a tellraw to display to @s. The `print` command accepts a string as an input, which is denoted using
"quotation marks" or 'single quotes.' It's down to preference.
```%lang%
print "Hello, {@s}!"
```

This particular example is using *string interpolation* to insert the name of @s, or the executing player. Interpolation
can include things like selectors, variable names, or even full expressions. An interpolated item is surrounded with \{curly braces.}

As seen in the example, the selector `@s` is surrounded with `{}` to denote that it should be interpolated: `{@s}`.
You could also do something more complicated, like `{@e[type=cow]}`

> Commands which compile to raw-text are the only ones that support this kind of interpolation (such as `print` in this case).

## Compiling the Code
Compiling the code in the <tooltip term="editor">editor</tooltip> is as simple as pressing the big "COMPILE" button at
the top. Compilation in MCCompiled is extremely fast, so in most cases you will see "Compilation completed" in the top-right
immediately. Upon compiling, there will be a new behavior pack created in your Minecraft
<tooltip term="development_packs">development</tooltip> folders in the format
`[project name]_BP`.
> Because your code doesn't use anything with resource packs yet, no resource pack is created.

### ... and running it
At this point, you are clear to open Minecraft, add the pack into your world, and run `/function [project name]`. By default,
any top-level statements are placed in a file named after the project.

### What's the 'init' function about? {id="init"}
While looking through the emitted files, You may also notice an `init` function in the output.
This file will contain any initialization code that should only run once per world and thus is pulled away from the
rest of the regular code. Things like defining scoreboard objectives are placed there rather than in the source files, as
it wastes part of the precious 10,000-command limit.

A pitfall of this optimization is that sometimes this function needs to be re-run when the project expands. The project doesn't
only expand when new variables are added, either. MCCompiled also creates and manages temporary variables to assist with
inline operations, function parameters, return values, timers, and preparing objectives to be displayed in rawtext.
If you are experiencing an unexpected issue with your code, re-running the `init` file may be the solution.
> Tip: The [`autoinit`](Optional-Features.md#autoinit) feature lets you run `init` automatically for every new build.

## Done!
You've successfully compiled and run your first project in MCCompiled using the <tooltip term="editor">editor</tooltip>!
The next logical step would be to continue exploring language features and commands.

Head into the "[fundamentals](Language.md)" category to begin exploring the language's features or go to the
[cheat sheet](Cheat-Sheet.md) to see everything all in one place.