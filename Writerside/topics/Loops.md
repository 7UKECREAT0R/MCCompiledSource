# Loops

<primary-label ref="runtime"/>

<link-summary>
Loops in MCCompiled allow conditional code repetition without function recursion, including repeat N times and while commands.
</link-summary>

When you need to do something multiple times in commands, the traditional option is to create a function and call it
recursively.  MCComplied offers three run-time commands that enable you to repeat code conditionally without having to
touch function recursion, counters, etc.

All of these commands should be followed by either a [code block](Syntax.md#blocks) or a single [statement](Syntax.md#statements).
That will be the code that runs for each iteration of the loop.

## Repeating N times {id="repeat"}
To repeat code a given number of times, use the `repeat` command. The repeat command takes in either an integer or
another [value](Values.md), indicating how many times to repeat the code.

In the following example, the repeat command is used to kill 10 random cows in the world.
```%lang%
repeat 10 {
    kill @r[type=cow]
}
```

### Repeating on a value {id="repeat-on-value"}
You can also specify an existing runtime value as the `repeat` command's argument. The value will be converted to an
[`int`](Types.md#int) if it's not already. This example is the same as above but uses the value 'prune' as the count instead.
```%lang%
define prune = 10

repeat prune {
    kill @r[type=cow]
}
```

### Accessing the current iteration {id="repeat-current-iteration"}
Either variant of the `repeat` command supports an additional argument, being the name of the value that should hold
the current iteration. If one doesn't yet exist, a new [`global`](Attributes.md#global_examples) [`int`](Types.md#int) value is created
with the given name.

> If the name is already in use by another value, that value must be an [`int`](Types.md#int) so that it can be reused. If it does not
> match the constraints, you will encounter an error.
> {style="warning"}

#### Current iteration bounds {id="repeat-bounds-details" collapsible="true" default-state="collapsed"}
When using the 'current iteration' value, it will begin at `i - 1` where `i` is the number of repetitions. It will then
count down to zero (inclusive). If the current iteration is 0, you know it is the last iteration in the loop. To illustrate,
this is the output of using:
```%lang%
repeat 10 val {
    print "Number: {val}"
}
```
```
Number: 9
Number: 8
Number: 7
Number: 6
Number: 5
Number: 4
Number: 3
Number: 2
Number: 1
Number: 0
```

## Repeating while a condition is true {id="while"}
The `while` command allows you to repeat code as long as a condition remains true. The conditions supported in the `while`
command are exactly the same as with regular [if-statements](Comparison.md).

The following kills random cows while there are more than 3 remaining:
```%lang%
while count @e[type=cow] > 3 {
    kill @r[type=cow]
}
```

## Repeating over a selector {id="for"}
The `for` command allows you to concisely iterate over every entity that matches a selector.

```%lang%
for @e[type=cow] {
    say "Moo"
}
```

> It's literally just shorthand for `execute as <selector> at @s { ... }`.

### Offsetting position
If you wish to offset the position of the execution (similarly to using the `positioned` subcommand in `execute`), you
can use the `at` keyword after specifying the selector. This will allow you to offset the execution position.

The position is applied _after_ being aligned to the executing entity, so facing coordinates like `^1` will be aligned properly.

```%lang%
for @e[type=cow] at ~ ~10 ~ {
    summon tnt
}
```