# Debugging

<primary-label ref="compile_time"/>

<link-summary>
Debugging with logging, assertions, errors, and halting code in MCCompiled to prevent bugs and issues.
</link-summary>

Sometimes, when working on a project, things don't work as you expect. You may also wish to add guards which make sure
that the project can only be compiled if certain things *are* working as you expect.

MCCompiled has two commands made for debugging and guarding against bugs.

## Logging
The `$log` command accepts *any inputs of any amount*. It will print the input tokens to the console when compiled, separated
by spaces. The log command works just as well with a single string specified, too.

Logging is useful to get context about what's happening to preprocessor variable(s) at a certain place in the code.
When something unexpected is happening, logging is the first step to figuring out why.
```%lang%
$var numbers 4 6 8
$add numbers 10

$log $numbers
```

## Assertions
> This section talks about assertions at __compile time__. [See here for information about testing and/or asserting at
> __runtime__.](Testing.md)
> {style="note"}

When you make an assumption about what a preprocessor variable will contain in your code, you can reinforce that
assumption using assertions. The `$assert` command takes a [compile-time condition, the same as the `$if` command](Comparison-compile-time.md#comparison),
with the addition of an optional 'message' parameter.

If the condition doesn't evaluate to true, an error is thrown by the command and your project *won't compile*.

Using assertions is particularly useful in places where it's vital for code not to break if you make a change.
Code can break for many reasons, but it's almost always unintentional as a side effect of some other change. Assertions
let you catch it before it reaches runtime.

```%lang%
$var numbers 4 6 8
$add numbers 10

$assert numbers == 14 16 18
```
Based on the code, you would expect the results to be `14, 16, 18`, so an assertion is made to reinforce that.
Uses of `$assert` in real code would likely be a little different; restricting numbers to certain bounds, making sure
that strings aren't too long, etc...

### Larger Assertion Example {id="larger_example"}
The following example shows a snippet of code which makes sure that the input doesn't move the player downwards under
any circumstance. By creating this assertion, you guarantee that the player can’t be moved down using this macro.

<snippet id="macro_1">
```%lang%
$macro movePlayer x y z {
    $assert y >= 0
    tp @s (~ + $x) (~ + $y) (~ + $z)
}
```
</snippet>

### Using Custom Messages {id="custom_assertion_message"}
You can provide a custom message as a [string literal](Syntax.md#strings) at the end of your assertion. If present,
it will be what shows up in the compiler error, rather than the generic "assertion failed, ..."
```%lang%
$macro movePlayer x y z {
    $assert y >= 0 "Height must be above 0."
    tp @s (~ + $x) (~ + $y) (~ + $z)
}
```
![assertion-message.png](assertion-message.png)

## Throwing Errors
Throwing an error can be useful when something goes terribly wrong, and you need to figure out where it happened.
This is done using the `throw` command, which runs at runtime and fully halts execution. `throw` supports
[format-strings](Text-Commands.md#format-strings).
```%lang%
throw "Player {@s} didn't have an id. ({id})"
```

## Halting Code
If you need your code to stop executing immediately, you can run the `halt` command. Doing this will generate an
infinite loop and force the "function command limit" to kick in and stop execution.