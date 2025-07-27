# Macros

<primary-label ref="compile_time"/>

<link-summary>
Macros automate tasks by defining and calling code with parameters, saving time and space.
</link-summary>

Macros are the biggest driver of automation in MCCompiled by parameterizing repeated code. It sounds complicated but
is implemented in a way that makes it easy.

The `$macro` command is used for both defining and calling them, and the syntax is the same for both cases:
`$macro` `[name]` `[args...]`

## Defining a Macro
If the macro command is followed up with a [block](Syntax.md#blocks), it will be interpreted as a definition. The
simplest macro definition, without any arguments, looks like this:
```%lang%
$macro exampleMacro {
    // ...code here...
}
```

### Using Arguments {id="last_example"}
If you define your macro with arguments, the names of those arguments will be used as preprocessor variable names.
Anything that calls the macro will be required to specify a value that will be assigned to its correlated variable.

The following example shows the use of a macro argument `message` in its contents:
```%lang%
$macro showWarning message {
    print "[!] Warning: $message"
}
```

## Calling a Macro
After a macro is defined, it can be re-used however many times it's needed. The syntax for calling a macro is the same
as defining one, except that it's not followed by a block and the input arguments are the values for the parameters
rather than their names.

When you call a macro, its contents are run essentially "pasted" with its arguments set to whatever the inputs you gave
were. Adding onto the [last example](#last_example), let's add two statements which call the macro with different texts.
```%lang%
$macro showWarning message {
    print "[!] Warning: $message"
}

$macro showWarning "Winds are high!"
$macro showWarning "Get inside!"
```
The two macro calls are at the bottom. A summary of what happens is as follows:
- Set preprocessor variable `message` to `"Winds are high!"`
  - Create a `print` command containing `"[!] Warning: $message"`
- Set preprocessor variable `message` to `"Get inside!"`
    - Create a `print` command containing `"[!] Warning: $message"`

These two macro calls run exactly equivalent to:
```%lang%
print "[!] Warning: Winds are high!"
print "[!] Warning: Get inside!"
```

## Why use Macros?
Macros can contain as much code as you want, so while there isn't too much benefit to using them in this example, there
are unlimited cases where macros save lots of code space and time. See the following two examples from other places
in this documentation where macros are illustrated:

This example uses [`$assert`](Debugging.md#assertions) to add bounds-checking to the input `y`.

<include from="Debugging.md" element-id="macro_1" />

This next example is more complicated and could be used when you have a list of effect strings that need to be applied.
When the string "clear" is passed in, the `effect @s clear` syntax is used automatically. Additionally, a message is
sent to the player telling them that they've received an effect using a [user-friendly display string](Advanced-Variable-Commands.md#string_ops)

<include from="Comparison-compile-time.md" element-id="macro_2" />
