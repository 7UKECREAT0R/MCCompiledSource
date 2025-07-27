# Values

<primary-label ref="runtime"/>

<link-summary>
Values are key to runtime logic, acting like variables, allowing math operations, type definitions, and more.
</link-summary>

Values are the backbone of all runtime logic. Values are just scoreboard objectives under the hood, but their
representation in MCCompiled is much different. There are a couple of key differences about values:
1. Values act identical to variables in other programming languages.
   - Regular math operations: `a + b`
   - Compound assignment: `a += b`
   - Declaration: `define int a`
2. Values can have [types](Types.md).
3. Values have no length restrictions on names.
4. Values are formatted when displayed to a player.

## Defining Values
Defining a value is done using the syntax <snippet id="define_with_type">`define [type] [name]`</snippet>.
The most common type is `int`, short for integer. An integer can be any whole number between -2,147,483,648 and 2,147,483,647.
It's versatile, and is the closest to a Minecraft scoreboard objective.
```%lang%
define int score
```

## Assigning Values
To assign something to a value, use the assignment `=` symbol. By default, the assignment always happens to the executing
entity, `@s`. If you want to change which entity gets the value assigned, see [clarification](#clarification).
```%lang%
score = 0
```
Values can be assigned to other values as well; notice that the difference between `scoreboard operation` and
`scoreboard set` is abstracted away entirely.
```%lang%
define bonus

bonus = 10
score = bonus
```

### Merging Define and Assignment (and type omission) {id="type-omission"}
If you wish to combine an assignment and a definition into one line, you can do that! Specify the assignment just after
the define command is completed. When merging definition/assignment, you can omit the type if you want, as it will
be inferred from the value you're assigning.

The following two examples have identical behavior. Note that in the second example, the type `int` is omitted because
the compiler can tell what type 'number' should be based on the input `21`.
```%lang%
define int example
example = 21
```
```%lang%
define example = 21
```

### Initializing Values
If you're familiar with scoreboard objectives in Minecraft, you may know that score-holders may not have any score at all.
Not zero, but nothing at all. You can use the `init` command to *initialize* a value so that if a score-holder doesn't hold
it, the value will be set to the default (usually 0).
```%lang%
init example
```
> You can use [clarifiers](#clarification) to choose *who* the value will be initialized for.
> {style="tip"}

## Simple Math
All the math operations in MCCompiled are listed here:

<deflist>
   <def title="No Compound Assignment">
      <tabs>
         <tab title="Addition">
            Adds the left and right values.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               define result
               %empty%
               result = a + b
               assert result == 14
            </code-block>
         </tab>
         <tab title="Subtraction">
            Subtracts the right value from the left value.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               define result
               %empty%
               result = a - b
               assert result == 6
            </code-block>
         </tab>
         <tab title="Multiplication">
            Multiplies the left and right values.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               define result
               %empty%
               result = a * b
               assert result == 40
            </code-block>
         </tab>
         <tab title="Division">
            Divides the left value by the right value.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               define result
               %empty%
               result = a / b
               assert result == 2
            </code-block>
            <tip>If you're dividing integers, the result is rounded to the highest integer that is less or equal to the
            result. (flooring)</tip>
         </tab>
         <tab title="Modulus">
            Divides the left value by the right value, but returns the remainder.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               define result
               %empty%
               result = a % b
               assert result == 2
            </code-block>
         </tab>
      </tabs>
   </def>
</deflist>

### Compound Assignment
When you need the left value to store the result of the operation, you can use what is called *compound assignment*.
Compound assignment is specified by using an equals-sign `=` after the operator you wish to use.

> In the simplest way possible, `A ?= B` is equivalent to `A = A ? B`, where `?` is your desired operator. Compound
> assignment is just useful shorthand to modify the left-hand variable, just like a regular assignment would.
> {style="note"}

<deflist>
   <def title="With Compound Assignment">
      <tabs>
         <tab title="Addition">
            Adds the right value to the left value.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               %empty%
               a += b
               assert a == 14
            </code-block>
         </tab>
         <tab title="Subtraction">
            Subtracts the right value from the left value.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               %empty%
               a -= b
               assert a == 6
            </code-block>
         </tab>
         <tab title="Multiplication">
            Multiplies the left value with the right value.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               %empty%
               a *= b
               assert a == 40
            </code-block>
         </tab>
         <tab title="Division">
            Divides the left value by the right value.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               %empty%
               a /= b
               assert a == 2
            </code-block>
            <tip>If you're dividing integers, the result is rounded to the highest integer that is less or equal to the
            result. (flooring)</tip>
         </tab>
         <tab title="Modulus">
            Divides the left value by the right value, but sets the left value to the remainder.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = 10
               define b = 4
               %empty%
               a %= b
               assert a == 2
            </code-block>
         </tab>
      </tabs>
   </def>
</deflist>

## Complex Statements {id="complex"}
MCCompiled fully supports the use of operations inline, as well as PEMDAS ordering. Using inline operations keeps
code size way down and communicates its purpose much more clearly than separating operations per-line.

The order that MCCompiled evaluates complex statements mostly conforms to PEMDAS along with some extra steps added for
the other features in the language:
1. Evaluate everything inside parentheses recursively. `()` `[]`
2. Run any [dereferences](Preprocessor.md#dereferencing). `$`
3. Run [function calls](Functions.md#calling-functions). `name(...)`
4. Apply any [indexers](Indexing.md). `[...]`
5. Evaluate multiplication/<tooltip term="modulo_extra">division</tooltip> operations. `a * b`
6. Evaluate addition/subtraction operations. `a - b`



The following example shows the use of a complex statement to calculate the amount of score remaining until a player
reaches the next level of a theoretical game:
```%lang%
define int untilNextLevel
define int betweenLevels = 1000

untilNextLevel = ((score / betweenLevels + 1) * betweenLevels) - score
```

## Clarification
By default, values point to `@s`, or the executing entity. This default works for a majority of cases; however, when you
need to change who the value points to, clarifiers are the answer.

A clarifier is defined as a selector encased in square brackets (indexers). For example, a clarifier pointing to all
cows would look like `[@e[type=cow]]`, and one pointing to the nearest non-self player would look like `[@p[rm=0.1]]`.

The following example shows adding one to `losses` for all players with the tag 'touched'
```%lang%
losses[@a[tag=touched]] += 1
```

### Other ways of doing the above example {collapsible="true" default-state="collapsed"}
<tip>
   There are some other ways to do this example, too, which look cleaner but may not be as efficient.
   <code-block lang="%lang%">
for @a {
   if @s[tag=touched]
      losses += 1
}
   </code-block>
   <code-block lang="%lang%">
execute as @a if @s[tag=touched] {
   losses += 1
}
   </code-block>
</tip>

## Miscellaneous Operations
Some operations don't follow the rules of the five main math operations, those of which are listed here. None of the
operations listed here are mentioned as assignment or not, they just exist as-is.
<deflist>
   <def title="Miscellaneous Operations">
      <tabs>
         <tab title="Swap">
            Swaps the left and right values.<br/>
            <code-block lang="%lang%" noinject="true">
               define a = true
               define b = false
               %empty%
               a %swap% b
               assert a == false
               assert b == true
            </code-block>
         </tab>
      </tabs>
   </def>
</deflist>

## Attributes
Attributes can change the way values behave. Any number of attributes can be applied pretty much anywhere in the
definition; before *or* after the type. See [attributes](Attributes.md) for all attributes across the language.

<include from="Attributes.md" element-id="value_attributes" />