# Preprocessor

<primary-label ref="compile_time"/>

<link-summary>
Preprocessor offers compile-time code generation, data driving, macros, testing, and more with dereferencing examples.
</link-summary>

The preprocessor is a powerful collection of features aimed at doing as much work during compile time as possible.
It contains features for code generation, data driving via. JSON files, macros, testing, and so much more. The
preprocessor offers a guarantee that any code using it will **always** run at compile time, and that you will **only**
get compile-time errors.

## Defining {id="defining"}
To define a preprocessor variable, use the `$var` keyword followed by the variable name and its value(s). Here's the
simplest example:
```%lang%
$var name "Luke"
$var time 120
```

### Defining with Multiple Values (arrays)
A preprocessor variable can hold more than one value, making it an **array**. Each value should be specified on the
same line, separated by a space.
```%lang%
$var names "Luke" "John" "Matt"
```

> There are also specific cases where you may want to define a preprocessor variable with *no values*.
> 
> The `$var` command supports having no inputs other than the variable name, if needed.
> {style="note" title="Defining with No Values"}

## Introduction to Dereferencing {id="dereferencing"}
Generally, anything marked with a dollar sign `$` is part of preprocessor code. It is going to affect the compiled
result in some way, but it doesn’t directly contribute to its contents. The simplest example of how the preprocessor
could be used is the following example:
```%lang%
$var text "This is some reusable text!"

print "Need some text? $text"
```
In this example, a preprocessor variable is defined using `$var`—it's given the name "text" and the value
"This is some reusable text!" Then, the variable is dereferenced (inserted) into a print command's contents. There are no
runtime changes happening here, the preprocessor variable is baked into the string at *compile time*. It works like a
constant here.

Notice how dereferencing a preprocessor variable uses the dollar sign `$`. Preprocessor variables aren’t just
limited to strings, but many types of items. Anywhere it is dereferenced, its contents will be placed as if they were
in the original code. Dereferencing is always the first step taken by MCCompiled when analyzing a statement.

### Example Using Arrays
```%lang%
$var spawnLocation 651 -5291 102

tp @a $spawnLocation
```
This example is much more interesting. We store three coordinates in the preprocessor variable "spawnLocation." We were
then able to dereference it to fulfill *all three arguments of the `tp` command at once*. You can think of this
dereferencing as a copying of the contents from "spawnLocation" directly into the source code.

You can likely see how this concept can be utilized to quickly and easily create code at compile time, as well as drive
code using data.

## Dereferencing using Indexer
You can also use an indexer to dereference a specific part of the preprocessor variable. When using an indexer, the
variable is *implicitly* dereferenced. The reason for this is that there is a semantic difference between `$a[0]` and `a[0]`.

As defined in the [order-of-operations](Values.md#complex), MCCompiled *always* evaluates a dereference operation `$`
before it evaluates an indexing operation. Consider the following example:
```%lang%
$var text "Example"
```
We've defined a variable called "text" which contains one string, being "Example." If you dereference the preprocessor
variable before indexing, you're just indexing the string because dereferences come before indexing:

<procedure title="Steps (dereferencing)">
<step><code>$text[0]</code></step>
<step><code>"Example"[0]</code></step>
<step><code>"E"</code></step>
</procedure>

Without the dereference, you're indexing the preprocessor variable directly, which fetches an element from it and
dereferences just that element.

<procedure title="Steps (not dereferencing)">
<step><code>text[0]</code></step>
<step><code>"Example"</code></step>
</procedure>

> You can imagine this is much more useful if you have a preprocessor variable with multiple elements in it, rather
> than just one.