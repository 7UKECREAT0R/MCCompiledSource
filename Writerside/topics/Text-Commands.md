# Text Commands

<primary-label ref="runtime"/>

<link-summary>
Format strings in MCCompiled simplify JSON raw-text usage, enabling easier and more efficient command writing.
</link-summary>

MCCompiled's support for *format strings* entirely abstracts away the use of JSON raw-text. There are a couple of
reasons why JSON was phased out, but most importantly:
- Writing raw-text is hard and unnecessarily verbose.
- More opportunities for MCCompiled to take the wheel, especially with [types](Types.md).
- Better language support without having to extend rawtext's already convoluted syntax.

The commands in MCCompiled mirror the commands in regular Minecraft. With any command in Minecraft that accepts raw-text,
its MCCompiled version accepts a format-string.

> The commands on this page support variants of themselves called their "global" variants. Calling the `global` version
> of a command will display the text to all players, as that player. It's functionally equivalent to:
> ```text
> execute as @a at @s run <command>
> ```

## Syntax {id="format-strings"}
Format strings use the same syntax as regular strings, but their contents are what dictate what should be shown. To insert
an item inside a format-string, use curly braces `{}` surrounding the content to insert. You can insert values, expressions,
or selectors.

### Inserting Values
To insert a value, place it inside curly braces anywhere in the string you want it to show.
```%lang%
print "You finished with {score} score!"
```

### Inserting Expressions
If you want to evaluate an expression inside the curly braces, you can do that. The result will be stored in an
 automatically generated temporary value and displayed as expected!
```%lang%
actionbar "Time left: {endTime - getCurrentTime()}"
```

### Inserting Selectors
When inserting a selector, the name(s) of the entity it selects will be displayed. If multiple entities match the
selector, their names will be separated by commas. The order in which the entities are displayed is not defined.
```%lang%
print "{cowCount} remain: {@e[type=cow,tag=game]}"
```

### Escaping
If you don't want an item in curly braces to be evaluated, use a backslash `\` to escape it, making it not be evaluated.
Using two backslashes will *escape* the backslash, making it show as one backslash and not affect the proceeding
insert.
```%lang%
print "This \{text} shows ingame as-is, without the backslash."
print "This \\{text} shows as the value of 'text' with a backslash."
```

## Commands
The commands which support format-strings are [listed in the cheat cheet here](Cheat-Sheet.md#commands-text). If marked
with the text "<format color="CadetBlue">Supports format-strings</format>," then the command will properly process
format-string inputs.