# Using Colors

<link-summary>
Learn to use color codes with the section symbol to decorate Minecraft command text and formatting.
</link-summary>

In writing any Minecraft commands that include text which will be shown to the player, you can always
use the section symbol `§` and a color code to decorate text with colors and formatting.

This formatting is not very informative, but you can use a feature in MCCompiled called `definitions` to improve the way
colors are laid out in text.

## definitions.def
In the MCCompiled installation directory, you can view the file called [`definitions.def`](%definitions_url%) to view all definitions shipped
with the compiler. The file is composed of `CATEGORY`s, each with a set of entries where `<key> IS <value>`. The entries
for the chat colors look like this:
```%lang%
CATEGORY COLOR AND CHATCOLOR AND CHAT COLOR
	BLACK IS §0
	DARKBLUE IS §1
	DARK BLUE IS §1
	DARKGREEN IS §2
	DARK GREEN IS §2
	DARKAQUA IS §3
	DARK AQUA IS §3
    DARKCYAN IS §3
    DARK CYAN IS §3
	DARKRED IS §4
	DARK RED IS §4
	DARKPURPLE IS §5
	DARK PURPLE IS §5
	GOLD IS §6
	GRAY IS §7
	GREY IS §7
	DARKGRAY IS §8
	DARKGREY IS §8
	DARK GRAY IS §8
	DARK GREY IS §8
	BLUE IS §9
	GREEN IS §a
	AQUA IS §b
	CYAN IS §b
	RED IS §c
	LIGHT PURPLE IS §d
	LIGHTPURPLE IS §d
	PINK IS §d
	YELLOW IS §e
	WHITE IS §f
	MINECOIN IS §g
	MINECOINGOLD IS §g
	MINECOIN GOLD IS §g
	MINECOINYELLOW IS §g
	MINECOIN YELLOW IS §g
	OBF IS §k
	OBFUSCATED IS §k
	BOLD IS §l
	STRIKE IS §m
	STRIKETHROUGH IS §m
	STRIKE THROUGH IS §m
	UNDERLINE IS §n
	UNDER LINE IS §n
	ITALIC IS §o
	ITALICS IS §o
	RESET IS §r
	NONE IS §r
	0 IS §r
```
{collapsible="true" default-state="collapsed"}

Multiple category names are defined, and most possible variants of color names are covered. To get the entry for black,
you could refer to it as `color: black`, `chatcolor: black`, `chat color: black`, etc.

## Specifying a Definition
Definitions are evaluated first, before every other process of the compiler. The text contained within a definition is
evaluated the same way raw code would be if it were written. Definitions just provide a convenient and verbose way to
specify code which has little meaning.

To specify a definition in your code, wrap it in square brackets and provide the category name as well as the entry name.
```%lang%
[category: entry]
```
> Formatting is very lenient, as well as case-insensitive. You don't have to specify a space between the colon if you
> don't want to. The following formats all work the same:
> - `[Category: Entry]`
> - `[CATEGORY:entry]`
> - `[category:Entry]`
> - etc.


### Example for Chat Coloring
This is an example of using the `color` category to style text.
```%lang%
// read by the compiler as "§cYou don't have permission!"
print "[color: red]You don't have permission!"
```

### Specifying Multiple Definitions
After specifying a category, you may specify multiple values under that category, separated by commas. The values will
be concatenated together, without a space in between them.
```%lang%
// read by the compiler as "§l§aYou don't have permission!"
print "[color: bold, green]Added {gold} gold!"
```

## Escaping Definitions
If you don't wish for a definition to be parsed, you can precede it with a backslash `\\` to cancel the definition on that
token. If two backslashes are specified, they'll cancel out, and you will end up with a single backslash before the
definition is parsed as usual.

The following table shows the results of different backslash counts to illustrate this idea:

| Backslash # | Input                  | Output           |
|-------------|------------------------|------------------|
| 0 (even)    | `[color: red]`         | `§c`             |
| 1 (odd)     | `\\[color: red]`       | `[color: red]`   |
| 2 (even)    | `\\\\[color: red]`     | `\\§c`           |
| 3 (odd)     | `\\\\\\[color: red]`   | `\\[color: red]` |
| 4 (even)    | `\\\\\\\\[color: red]` | `\\\\§c`         |
