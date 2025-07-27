# Comparison (compile-time)

<primary-label ref="compile_time"/>

<link-summary>
Logic of compile-time comparison using $if and $assert commands with six operators for equal, not equal, less, less or equal, greater, and greater or equal.
</link-summary>

Comparison is the backbone of logic in both runtime and compile-time code. This page will focus on the compile-time side
of comparison.

## Simple Comparison {id='comparison'}
Comparison is formatted universally the same. The primary way of performing a comparison is using `$if`, but the
[`$assert`](Debugging.md#assertions) command also shares the same syntax. Comparison compares the left and right side
using one of six different operators:
<tabs>
    <tab title="Equal">
        Checks if the left and right sides are equal to each other.<br />
        <code-block lang="%lang%">
    $if left == right {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Not Equal">
        Checks if the left and right sides are not equal to each other.<br />
        <code-block lang="%lang%">
    $if left != right {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Less">
        Checks if the left side is less than the right side.<br />
        <code-block lang="%lang%">
    $if left %less% right {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Less or Equal">
        Checks if the left side is less or equal to the right side.<br />
        <code-block lang="%lang%">
    $if left %less%= right {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Greater">
        Checks if the left side is more (greater) than the right side.<br />
        <code-block lang="%lang%">
    $if left %greater% right {
        // ...
    }
        </code-block>
    </tab>
    <tab title="Greater or Equal">
        Checks if the left side is more (greater) or equal to the right side.<br />
        <code-block lang="%lang%">
    $if left %greater%= right {
        // ...
    }
        </code-block>
    </tab>
</tabs>

### Using If
The `$if` command runs a comparison and performs the next [statement/block](Syntax.md#blocks) only if it evaluates true.

The example below shows a [macro](Macros.md) which runs `effect @s clear` if the input effect is equal to "clear."

```%lang%
$macro giveEffect effectName {
    $if effectName == "clear"
        effect @s clear
}
```

> This example macro could be used in automation, where someone has a list of effects to apply, represented as a list of
> strings:
> 
> `$var effects "clear" "speed" "jump_boost"`
> {style="note" title="Relevance of this example"}

### Using Else
The `$else` command looks at the result of the last `$if` command that was run in the same block, and only performs
the next [statement/block](Syntax.md#blocks) if that `$if` command didn't result with true. It's essentially the inverse.

This example expands and finishes the above example by adding `$else` to cover cases where the effect is *not* "clear."

<snippet id="macro_2">
```%lang%
$macro giveEffect effectName {
    $if effectName == "clear"
        effect @s clear
    $else
    {
        // give them the effect
        effect @s $effectName
%empty%
        // let them know what effect they've received
        $strfriendly effectName
        print "You've received the effect $effectName!"
    }
}
```
</snippet>