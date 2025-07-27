# Syntax

<link-summary>
MCCompiled's syntax similar to Minecraft commands, with added features. Covers comments, tokens, statements, code blocks.
</link-summary>

MCCompiled's syntax is similar to regular Minecraft commands, with a little bit of c-style sprinkled in for extra features.
This page goes over some miscellaneous syntax that will be used/referred to throughout the documentation.

## Comments {id="comments"}
A comment is a note to leave to yourself in the source code. You might add one to explain why you wrote code a certain way,
or maybe to explain a complicated section of code. It's also useful to leave comments so that other developers can more
easily understand what your code is doing.

There are two different kinds of comments; single-line and multi-line. They use slightly different syntax for each.
```%lang%
// This is a comment that only spans a single line.

/*
    This is a multi-line comment.
    You can add whatever you want anywhere between
        the opening and closing markers.
*/
```

### Documentation
Some items can be documented, such as [macros](Macros.md), [functions](Functions.md), or [values](Values.md). Documenting can be done
by leaving a comment before defining the item, done using `$macro`, `define`, and `function` respectively. Documented
items will show up under the item's keyword in any editors/IDEs that support MCCompiled.

The example below shows documenting a function:
```%lang%
// %documenting_example_text%
function reset {
    score[*] = 0
    inGame[*] = false
    timeLeft[*] = 0
}
```
![Example of what the documentation looks like in the editor.](documenting_example.png)

## Common Tokens
A "token" is just a unit of some value, whether it be an integer, decimal number, boolean, or even text. All of these
qualify as a token. Text is commonly referred to as a string, being that it's a *string* of characters. The following
sections will touch on exactly how to specify these different tokens.

### Strings {collapsible="true" default-state="collapsed"}
A string is just another word for a piece of text. They can be specified using either `'` or `"`, depending on preference.
The string must be opened and closed using the same character. An example string looks like: `"Hello World!"`.

#### Inlaying Preprocessor Variables
The value(s) of a [preprocessor variable](Preprocessor.md) can be placed into a string using the dereference operator. `$`
If you wanted to place the variable `example` into a string, it would look like the following: `'The example is $example'`.

#### Escaping
When you have something in your string that you don't wish to be counted as code, you can escape `\\` it using a backslash.
This also works for the opening/closing character itself. This is particularly useful if you wish to use a quotation mark
in your string without closing it. `"Mark's \\"things\\" are pretty old."` or try the following example
`'Collected \\$money!'` which won't inlay the preprocessor variable `$money`.

### Integers {collapsible="true" default-state="collapsed"}
An integer is written as a whole number, with or without a negative sign `-`.

### Decimals {collapsible="true" default-state="collapsed"}
When you need to represent a number that is part of a whole, the decimal point `.` denotes the start of the partial
part of the number; e.g., `4.65` or `12.1`

### Coordinates {collapsible="true" default-state="collapsed"}
Works exactly the same as coordinates in Minecraft commands. Coordinates can be prefixed as positional relative `~` or
directional relative `^`, and they can be positive or negative; same as integers, e.g., `42`,`-101`, `~30`, or `^-5`

### Ranges {collapsible="true" default-state="collapsed"}
Works exactly the same as ranges in Minecraft commands. Ranges describe a set of numbers, consisting of a left/right side
separated by the range operator `..`. When both sides are specified, the range includes all integers between the two
numbers inclusively (meaning that `5..10` will include 5, 6, 7, 8, 9, and 10).

When a side is omitted, the range continues on forever in that direction. `..7` includes 7 and every integer below 7.
Likewise, `10..` includes 10 and every possible number after 10.

Ranges can additionally be inverted using the "not" operator `!`. This causes the range to include every number *except*
the ones described in the range. `!0..50` includes every number except those between 0 and 50 inclusive.

### Selectors {collapsible="true" default-state="collapsed"}
Works exactly the same as selectors in Minecraft commands. A selector selects a specific set of entities using special
filters.

[Learn about target selectors here](https://wiki.bedrock.dev/commands/selectors.html)

### Time Suffixes {collapsible="true" default-state="collapsed"}
Numbers support suffixing to better represent time. A simple example of this is `4s`, which when represented as ticks,
evaluates to `80`. Time suffixes offer the advantage of being context-aware. If a command requires an input in seconds,
the seconds suffix `s` will not apply any change to its attached number. Likewise, if a command requires a tick input,
the input will be multiplied according to its need.

The list of available time suffixes is here:
- `t` ticks, multiply the number by 1 by default.
- `s` seconds, multiply the number by 20 by default.
- `m` minutes, multiply the number by 1200 by default.
- `h` hours, multiply the number by 72,000 by default.

## Statements {id="statements"}
A statement is just a line of code that does something. A statement could be a command, an operation to a variable,
or a call to a [function](Functions.md). 

## Code Blocks {id="blocks"}
MCCompiled introduces a new concept to commands called "blocks." Blocks are just chunks of statements surrounded with
curly braces `{}`. The opening curly brace can be included on its own line or at the end of a statement; it's down to
preference.

Blocks are usually preceded by a statement which tells you how the code in the block is different from other code.
The code might be part of a different function; maybe it only runs under a certain condition; or maybe it runs multiple
times instead of just once. The preceding statement is always the one that tells you that.
```%lang%
if timeLeft <= 0
{
    globalprint "Time is up!"
    tp @a 40 100 50
}
```
In the example above, there are two statements contained within the block:
```%lang%
%empty%
%empty%
    globalprint "Time is up!"
    tp @a 40 100 50
%empty%
```
This block only runs under the condition placed right before it, `if timeLeft <= 0`. The statement that preceded the
block modified how the block runs. This concept is used all throughout the language, and you'll find that there are
many kinds of commands which modify the way blocks run.

### Omitting Brackets
Commands which require blocks can also accept a single statement if you wish. This is just a syntax sugar thing,
it doesn't impact the way the code runs. It's just important to remember that when using this syntax, only the first
statement will fall under the rules of the command in question. The two following examples are identical:
```%lang%
if deadPlayerCount == allPlayerCount {
    endGame()
}
```
```%lang%
if deadPlayerCount == allPlayerCount
    endGame()
```

> The indentation of the statement doesn't matter, use whatever you think communicates the statement's rule best; i.e.,
> indenting a statement would make it clearer to developers.
