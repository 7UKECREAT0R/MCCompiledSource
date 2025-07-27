# Comparison

<primary-label ref="runtime"/>

<link-summary>
Comparison syntax, value comparison, booleans, selectors, entities, and blocks. Examples provided.
</link-summary>

Comparison is the most important part of writing any logic, and a majority of MCCompiled code will contain comparisons
of some sort. Comparison is primarily done with two commands: `if` and `else`.

> The information on this page is for runtime comparison. If you're looking for comparison at compile time,
> see [here](Comparison-compile-time.md).
> {style="note"}

## Syntax
An `if` statement accepts as many comparisons at once as you would like. Each comparison must be separated by the `and`
keyword to indicate to MCCompiled that the last comparison ended.
```%lang%
if <comparison>
```
```%lang%
if <comparison> and <comparison> and /* ...continue... */
```

`if` statements must be followed by either a [block](Syntax.md#blocks) or [single statement](Syntax.md#omitting-brackets)
which will only be run if *all* the comparisons in the command evaluate true.

### Else
After an `if` statement ends (as well as its block/statement), you can choose to place an `else` command too. The else
command follows the same rules about how it needs to be followed by either a [block](Syntax.md#blocks) or [single statement](Syntax.md#omitting-brackets).
```%lang%
if <comparison>
{
    // code to run if true
}
else
{
    // code to run if false
}
```

### Inverting a Comparison
You can invert a comparison by specifying `not` before it. In doing so, you check if the comparison evaluates *false*
instead of true.
```%lang%
if not <comparison>
```

## Value Comparison
[Values](Values.md) can be compared to one another or compared to literal values. Comparisons automatically handle all
the necessary [type conversions](Types.md), ranges, setup, etc. for you so that you can focus more on the logic than
extraneous stuff.

When comparing values, the syntax of the comparison is `<a> <operator> <b>`. The parameter `a` can be any value or
literal, as well as `b`.

The operator can be any of the following, which decides what case A and B have to match in order for the comparison to
be true.

<tabs>
    <tab title="Equal">
        Checks if A and B are equal.<br />
        <code-block lang="%lang%">
    if a == b {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Not Equal">
        Checks if A and B are not equal.<br />
        <code-block lang="%lang%">
    if a != b {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Less">
        Checks if A is less than B.<br />
        <code-block lang="%lang%">
    if a %less% b {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Less or Equal">
        Checks if A is less than or equal to B.<br />
        <code-block lang="%lang%">
    if a %less%= b {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Greater">
        Checks if A is more (greater) than B.<br />
        <code-block lang="%lang%">
    if a %greater% b {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Greater or Equal">
        Checks if A is more (greater) than or equal to B.<br />
        <code-block lang="%lang%">
    if a %greater%= b {
        // ...
    }
        </code-block>
    </tab>
</tabs>

### Value Comparison Example {collapsible="true" default-state="collapsed"}
The following example checks if the player is in debt; if their balance is less than 0, then they are in debt.
```%lang%
define int balance

if balance < 0 {
    print "You are in debt!"
}
```

Let's expand on this example by adding an `else` statement and adding another `if` statement. If the player has enough
money to buy the item (indicated by `cost`), they'll receive the item and pay the cost. If the player doesn't have enough
money, they will be let know and nothing will happen.

```%lang%
define int balance
define int cost

cost = 40

if balance < 0
    print "You are in debt!"
else
{
    if balance >= cost
    {
        balance -= cost
        give @s diamond 1
    }
    else
        print "You don't have enough money! Missing {cost - balance}"
}
```

### Comparing Booleans
If you have a [boolean](Types.md#boolean) value, you can compare it as normal or completely omit the operator and B value.
When you do this, you are implicitly checking if the boolean is true. With the [`not`](#inverting-a-comparison) operator,
you can check if the boolean is false.

> If your boolean values are named adequately, this form reads extremely well.

```%lang%
define bool isJumping

if isJumping {
    // the player is jumping
}
```

## Selector Comparison
If you want to compare using a selector, specify the selector as-is. This form requires the selector be `@s` to keep
its meaning concise. The following example shows using this comparison to check if the executing player has a tag:
```%lang%
if @s[tag=safe] {
    // ...
}
```

## Entity Comparison
There are two different comparison methods of checking entities, besides selectors (seen above).

Any
: Check if *any* entity matches the given selector.
- `any <selector>`
```%lang%
if any @e[r=10,type=cow] {
    // found a cow somewhere nearby
}
```

Count
: Counts the given selector and checks it as if it was a value. Supports all the operators seen above and can be compared
to other values or literals.
- `count <selector> <operator> <b>`
```%lang%
if count @e[type=cow] > 10 {
    // cow overpopulation (more than 10)
}
```

## Block Comparison
Allows you to check for individual blocks or groups of blocks within the world. Like with all other comparisons, you can
invert these with [`not`](#inverting-a-comparison) for use-cases like checking if a block is not air.

Block
: Checks for a block at specific coordinates.
- `block <x, y, z> <block>`
```%lang%
if block ~ ~-1 ~ air {
    // player is in the air
}
```

Blocks
: Checks if a region of blocks matches another region.
- `blocks <region-start x, y, z> <region-end x, y, z> <dest x, y, z> all`
- `blocks <region-start x, y, z> <region-end x, y, z> <dest x, y, z> masked`
```%lang%
if blocks 0 0 0 10 10 10 30 0 30 {
    // region matched
}
```