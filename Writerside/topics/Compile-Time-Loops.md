# Loops

<primary-label ref="compile_time"/>

<link-summary>
Loops in MCCompiled for repeating statements, tracking current iteration, iterating over ranges, and arrays.
</link-summary>

Sometimes you need to do things multiple times; an especially powerful tool at compile time. There are two primary types
of iteration in MCCompiled:
1. [Repeating a certain number of times.](#repeat_number)
2. [Iterating over a given value.](#repeat_iteration)

## Repeating N Times {id="repeat_number"}
Repeating a statement/set of statements some number of times is useful in lots of cases. As such, MCCompiled's `$repeat`
command is a powerful and simple choice for doing this. This page discusses the preprocessor version of the command,
which runs exclusively at compile time. [See runtime repeat here.](Loops.md#repeat)

An example is shown here summoning 10 zombies at the given coordinates.
```%lang%
$repeat 10 {
    summon zombie 304 -391 1047
}
```

### Current Iteration
Sometimes it's also necessary to know what iteration the repeat is currently on. For this, you can specify the second
parameter, which is the identifier of the preprocessor variable to store the current iteration in.

When iterating a single digit like this, the variable always begins at zero and ends at the ending number minus one. In
the following example, the identifier `xOffset` is used, and then its value is used to offset where each zombie is spawned.
`xOffset` begins at 0, and ends at 9, covering 10 total digits.

```%lang%
$repeat 10 xOffset {
    summon zombie (304 + $xOffset) -391 1047
}
```

### Iterating Over Ranges
When you want an iteration to have an upper and lower bound, you can use a *range* as an input rather than a single number.
Like in regular Minecraft, ranges are always inclusive; so `2..5` includes the indices `2, 3, 4, 5`.
```%lang%
$repeat 50..100 i {
    setblock ~ (~ + $i) (~ + $i) stone
    print "Stairway created."
}
```

## Iterating over Arrays {id="repeat_iteration"}
When you want to iterate over the elements in an object but don't care about the location it's at, the `$iterate` command
is the best choice. The syntax of the iterate command is `$iterate [object] [id: current]`. The argument 'current' will
be the name of the preprocessor variable that stores the current element in the iteration.

### Preprocessor Variable Elements
The most simple iteration you can do is over the elements in a preprocessor variable. To specify a preprocessor variable,
enter its identifier as the input for "object." Regardless of what's stored *in* the preprocessor variable, its elements
are guaranteed to be iterated over (even if it contains one element).
```%lang%
$var colors "red" "orange" "yellow" "green" "blue" "purple"

$iterate colors color {
    give @s "$color_wool" 16
}
```
