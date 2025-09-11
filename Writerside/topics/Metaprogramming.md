# Metaprogramming

<primary-label ref="compile_time"/>

<link-summary>
Metaprogramming: how to use preprocessor to generate and use functions/values with dynamic names.
</link-summary>

The preprocessor is an incredibly powerful tool, so this page goes over how to use it in a more advanced way to both generate
and use [functions](Functions.md) and [values](Values.md) with fully dynamic names.

> This is an advanced topic, so make sure you *really* understand the [preprocessor](Preprocessor.md) before diving into
> metaprogramming. It looks complicated (and can be), but it's the most powerful tool available when dealing with
> repetitious code.
> {style="warning"}

## Introduction
To introduce this topic, we're going to begin with a set of example data to use across the page. It consists of a list
of mob names that we wish to create two utility functions for; `count_(mob name)` and `killAll_(mob name)`.
```%lang%
$var mobs "pig" "cow" "sheep" "chicken" "llama"
```

## Iterating
The first step to accomplish this example is to iterate over all the items in the `mobs` preprocessor variable. This
can be done using [`$iterate`](Compile-Time-Loops.md#preprocessor-variable-elements). For every item
in the list of mobs, we want to create a function which will kill all of that specific mob.

This is possible because the `function` command accepts a [string](Syntax.md#strings) input as a name, not just an identifier.
This allows you to [use preprocessor variables](Syntax.md#inlaying-preprocessor-variables) in the name, thus becoming a
foothold into metaprogramming.
```%lang%
// loop over every mob in the "mobs" preprocessor variable.
$iterate mobs mob {
    // kills all of the given mob.
    function export "killAll_$mob" {
        kill @e[type=$mob]
    }
}
```

> When you want to iterate over a JSON array, make sure to dereference the preprocessor variable (`$`), or you'll end up
> iterating the preprocessor variable's elements instead.
> 
> Think of it like this; all preprocessor variables *are* arrays and can contain as many items as you'd like. It's just that
> the ones you usually use only contain one item.
> So if you `$iterate` over a PPV's name alone, you iterate over its elements.
> If you dereference it, you'll iterate over the element it contains. 
{style="warning" title="If you've got a JSON array..."}

Without doing anything else, you now have five functions available, each with the correct code inside:

![Example showing all five newly created functions.](killall_example.png)

## Calling
Next, let's create a function called `killAll` which runs all the created killAll functions we've created using
metaprogramming. You could do it the obvious way:
```%lang%
function killAll {
    killAll_pig()
    killAll_cow()
    killAll_sheep()
    killAll_chicken()
    killAll_llama()
}
```

Or you could use the `$call` command, which calls a function based on
a string name. The command accepts parameters the same as a regular function call does, and it even compiles exactly the same.
```%lang%
function killAll {
    $iterate mobs mob
        $call "killAll_$mob"
}
```
As your "mobs" list expands, so will their functions and the `killAll` function. This lowers the time cost of making
changes, as well as guarding against user error. If the code works, it will continue to work as the data changes/expands.

## Values
Next, we're going to expand the example to include a `count_(mob name)` function which stores the mob count in a value
created specifically for that mob.

Identical to the `function` command, the [`define`](Values.md#defining-values) command also can accept a string as the
name input.
```%lang%
// loop over every mob in the "mobs" preprocessor variable.
$iterate mobs mob {

    // kills all of the given mob.
    function export "killAll_$mob" {
        kill @e[type=$mob]
    }
    
    // counts the number of this mob.
    function export "count_$mob" {
        // define a global integer and set it to 0.
        define global int "amountOf_$mob" = 0
    }
}
```

### Using the Values
Using a value defined by a string is a little more challenging than with functions. The `getValueByName` function is
 built-in to the compiler, see [built-in functions](Built-In-Functions.md) for more information about these. The function
accepts one string parameter, being the name of the value to get. If the function successfully finds a value with the
given name, it will be replaced with that value.
```%lang%
// add one to this mob's amountOf value that we defined earlier
getValueByName("amountOf_$mob") += 1
```

### Completed Example
With all of these concepts combined, here is the final file, containing:
- A `killAll_$mob` function for every mob in the list.
- A `count_$mob` function for every mob in the list.
- A value `amountOf_$mob` for every mob in the list.
- A function `killAll` which calls all defined "killAll" functions.
- A function `countAll` which calls all defined "countAll" functions.

```%lang%
$var mobs "pig" "cow" "sheep" "chicken" "llama"

$iterate mobs mob {
    function export "killAll_$mob" {
        kill @e[type=$mob]
    }
    
    function export "count_$mob" {
        define global int "amountOf_$mob" = 0
        
        for @e[type=$mob]
            getValueByName("amountOf_$mob") += 1
    }
}

function killAll {
    $iterate mobs mob
        $call "killAll_$mob"
}
function countAll {
    $iterate mobs mob
        $call "count_$mob"
}
```

### Expanded Example
Without metaprogramming, the code would look like this; it's simpler but harder to maintain the larger the data gets,
as well as being more tedious when trying to make changes to the logic of the code.
```%lang%
function export killAll_pig {
    kill @e[type=pig]
}
function export killAll_cow {
    kill @e[type=cow]
}
function export killAll_sheep {
    kill @e[type=sheep]
}
function export killAll_chicken {
    kill @e[type=chicken]
}
function export killAll_llama {
    kill @e[type=llama]
}

function export count_pig {
    define global int amountOf_pig = 0
    
    for @e[type=pig]
        amountOf_pig += 1
}
function export count_cow {
    define global int amountOf_cow = 0
    
    for @e[type=cow]
        amountOf_cow += 1
}
function export count_sheep {
    define global int amountOf_sheep = 0
    
    for @e[type=sheep]
        amountOf_sheep += 1
}
function export count_chicken {
    define global int amountOf_chicken = 0
    
    for @e[type=chicken]
        amountOf_chicken += 1
}
function export count_llama {
    define global int amountOf_llama = 0
    
    for @e[type=llama]
        amountOf_llama += 1
}

function killAll {
    killAll_pig()
    killAll_cow()
    killAll_sheep()
    killAll_chicken()
    killAll_llama()
}
function countAll {
    count_pig()
    count_cow()
    count_sheep()
    count_chicken()
    count_llama()
}
```