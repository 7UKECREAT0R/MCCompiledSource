# Functions

<primary-label ref="runtime"/>

<link-summary>
Explains how to define, use, and export functions in .mcfunction files for custom Minecraft code.
</link-summary>

Functions work similarly to regular `.mcfunction`s, but contain extra utility to make them work more like programming
functions. At its most basic level, a function can be used to create a new `.mcfunction` file and place code in it.

## Defining Functions
Functions are defined using the `function` command. The command itself is
very dynamic and accepts lots of different kinds of inputs; however, it always requires a function name.

A [code block](Syntax.md#blocks) must always follow function definitions. The code in this block is what will go into
the defined function.
```%lang%
function example {
    print "Heyo, what's up?"
}
```

> Upon testing these examples, you may notice that there is no function outputted. This is because functions which are
> unused aren't exported unless called by another exported function or marked with the [`export` attribute](Attributes.md#export_examples).
> See more about this topic [below](#exports).


### Parameters
Functions can be defined with parameters, which must be specified whenever the function is called. Parameter definitions
use the same syntax as [values](Values.md#defining-values), but without the keyword `define`. Functions are very flexible 
with how you can specify parameters, but the most widely accepted way is by placing them inside parentheses `()` and
separating them by commas `,`.

The following example shows defining a function called "awardPoints" with a single parameter, being the number of
points to award.
```%lang%
function awardPoints(int amount) {
    points += amount
}
```

> None of the syntax shown here is necessary for the code to compile, but it helps show readers which parts are the
> parameters, and the boundaries between them.
> {style="note"}

#### Optional Parameters
If you want to make a parameter optional, you need to give it a default value that will be specified automatically if
it's omitted. This is done using the assignment operator `=`, and the default value. Once you specify an optional
parameter, every parameter that follows must also be optional.

The following example shows the "awardPoints" function with its "amount" parameter optional such that if the caller
doesn't specify an amount, it will default to 1.
```%lang%
function awardPoints(int amount = 1) {
    points += amount
}
```

### Using Folders
When defining a function, the dots `.` in its name act as path separators for the folder it will go in. When the function
is exported to an `.mcfunction` file, it will be placed in the folder. The following example shows creating a function
which will be placed in a subfolder:
```%lang%
function scoring.reset() {
    points[*] = 0
}
```
The compiled function is then located at `scoring/reset.mcfunction`. Using folders is a good way to group functions
based on what they do, increasing clarity for both the writer of the code and the reader.

## Calling Functions
Now that you have functions, you need to be able to use them. Running a function is also known as "calling" it.
The syntax of calling a function is `functionName(parameters)`.

If the function doesn't have any parameters (or its parameters are all [optional](#optional-parameters)), you still need
the parentheses to tell MCCompiled that you want to call the function: `functionName()`

The following example calls the awardPoints defined in the earlier examples:
```%lang%
// award the player with five points
awardPoints(5)

// award the player with a single point
awardPoints()
```

## Return Values
Functions are able to send back a value after they're done running, called a "return value." Return values are
particularly useful in cases where the function is a utility for other code to use.

### Returning a Value
To return a value, use the `return` command. This will set the value that
will be returned when the function ends. The return command, as of 1.16, does not stop the function immediately, but
this is planned for a future version.

This example shows a more complicated function which computes the highest score of any player in the game and returns
it to the caller:
```%lang%
function getHighestPoints {
    // keep track of the highest number of points so far
    define global int highestPoints = 0
    
    // loop over each player, seeing if their points are higher
    for @a {
        if points > highestPoints
            highestPoints = points
    }
    
    // return the value to the caller
    return highestPoints
}
```

#### Type Consistency
If you have a `return` command somewhere in the function, every `return` command in that same function must return the
same type. If you return a boolean somewhere in the function, you can't return an integer somewhere else.

### Using Return Values
Using a return value is done by using the function call as if it were a value. The following example builds on the
`getHighestPoints` example shown above by using its return value. If the function returns more than 100, it's
considered an "ultra-high score" and a title is shown to all players.

The title then calls the function again and displays its returned value in the message.
```%lang%
if getHighestPoints() > 100 {
    title @a "Achieved an ultra-high score of {getHighestPoints()}!"
}
```

## Exports
When a function is not used anywhere in your code, it is not included in the compiled output unless given the [`export`
attribute](Attributes.md#export_examples). So, what constitutes a function being "used"?

Any code in the top-level of your file, as in, not inside any block, is *always* marked as used. Any function marked for
export will propagate that to any of the functions it calls. Any functions marked with the [`auto` attribute](Attributes.md#auto_examples)
are also marked as used.

```%lang%
function a() {}
function b() {
    a()
}
function c() {
    b()
}

// top level call
a()
```
- In this example, only the function `a` is exported because it's the only one in use.
- If the top level call is changed to `b()`, then the functions `a` and `b` are both exported.
- If the top level call is changed to `c()`, then the functions `a`, `b` and `c` are exported.

## Attributes
Attributes can change the way functions behave. Any number of attributes can be applied anywhere before the name.
See [attributes](Attributes.md) for all attributes across the language.

<include from="Attributes.md" element-id="function_attributes" />