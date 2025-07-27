# Simple Variable Commands

<primary-label ref="compile_time"/>

<link-summary>
Learn how to create and manipulate variables, perform math operations, and swap variable contents.
</link-summary>

> This page assumes you have read the introduction to the preprocessor [here](Preprocessor.md).
> {style="warning"}

Variables are the main way of dealing with data during the preprocessing phase. If you want to store, keep track of,
or modify data in some way, you'll need to use variables. The easiest way to create a variable is using the syntax:
```%lang%
$var <name> [values...]
```
This will create a variable with a name and value. Preprocessor variables can hold as many values as you would like,
sort of like how lists work in other languages; however, their behavior remains consistent whether they have one value,
multiple, or even zero. This is expanded upon [later](#simple-math-operations).

## Incrementation (and how to specify 'id') {id="inc-dec"}
You can increment and decrement variables using `$inc` and `$dec` respectively. These commands will increment *all values*
in the given preprocessor variable at once.

> Notice how these commands begin with a dollar sign, identifying them as preprocessor operations.

These commands accept one parameter, being of the type `id`. It is not looking for a value, but rather the identifier
of a preprocessor variable. Consider the following example, which **would not be valid**:
```%lang%
// this example is intentionally incorrect
$var example 15
$inc $example
```
By dereferencing the variable "example," we place *its contents* into the command, making it effectively `$inc 15`.
Of course, this doesn't make any sense. You can't do anything by incrementing a literal number, you want to increment
the variable itself.

The proper way to do this is simply to specify the *identifier* of the preprocessor variable, or in this case, `example`.
The command knows it will be given an identifier (evidenced by its parameter, `<id: variable>`), and thus it knows what
to do with this identifier.

> At this point, you know all the rules of the preprocessor. The distinction between identifiers and dereferencing is
> confusing to some new users, but as long as you have that down, you're set.
> {style="note"}

## Simple Math Operations
MCCompiled has all the necessary commands for doing math with preprocessor variables. Don't let this be a red flag, as
there are still inline operations you can perform. The main advantage of using these commands is that
1. It's clear that a change is happening to the input variables.
2. These commands loop the operands to match the number of inputs.

### Looping Operands
When one of these operations is run without enough operands to match the number of entries in the preprocessor
variable, the operands are looped. If you had a preprocessor variable containing `2 3 4 5 6`, and ran `$add variable 10 1`,
you would get the result `12 4 14 6 16`. This is because the operands you specify are looped to match the input length (5).

> This feature is new as of 1.16. In previous versions, the behavior was to ignore all values beyond where the operands
> stop.

### All of Them
Here's a list of all the math operations in MCCompiled which follow the rules described in this section.  
<tabs>
    <tab title="$add">
        <deflist>
            <def title="Add to Preprocessor Variable">
                <p>Performs addition. <b>A += B</b> for each item in the variable.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$sub">
        <deflist>
            <def title="Subtract from Preprocessor Variable">
                <p>Performs subtraction. <b>A -= B</b> for each item in the variable.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$mul">
        <deflist>
            <def title="Multiply with Preprocessor Variable">
                <p>Performs multiplication. <b>A *= B</b> for each item in the variable.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$div">
        <deflist>
            <def title="Divide Preprocessor Variable">
                <p>Performs division. <b>A /= B</b> for each item in the variable.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$mod">
        <deflist>
            <def title="Modulo Preprocessor Variable">
                <p>Performs modulus. <b>A %= B</b> for each item in the variable.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$pow">
        <deflist>
            <def title="Exponentiate Preprocessor Variable">
                <p>Performs exponentiation (power). <b>A to the power of B</b> for each item in the variable.</p>
            </def>
        </deflist>
    </tab>
</tabs>

## Other Variable Operations
There are some other less widely used operations that deserve their own list... just away from everything else.

### Swap
Swap will swap the contents of two given preprocessor variables, regardless of length. In the following example, the
variable `two` ends up containing `"hello"` `"world"` and vice versa.
```%lang%
$var one "hello" "world"
$var two 9 8 7 6 5 4 3 2 1

$swap one two
```
