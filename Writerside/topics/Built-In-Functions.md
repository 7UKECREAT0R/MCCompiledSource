# Built-In Functions

<primary-label ref="runtime"/>

<link-summary>
Pre-implemented functions for compile-time, runtime, or both, including utility, math, and rounding functions.
</link-summary>

MCCompiled ships with a list of built-in functions which are already implemented for your use. Functions can either
support <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format>, or both.

When a function supports runtime, it follows the [same rules as functions](Functions.md#exports), as in, it is only
included in your project's output when it's used somewhere in your code.

> Some runtime functions need to create more than one variant of themselves for when different types are passed in, too.
> 
> For example, the `round(...)` function supports inputting decimals of different precisions and integers, so it needs
> to generate code for each variant.

## Utility Functions
These functions are miscellaneous useful utilities that can help with writing MCCompiled code. Metaprogramming, math,
and other various functions allow you to write better code. 

<deflist>
<def title="Get Value by Name">
<code>getValue(name)</code> <format color='lightskyblue'>compile-time</format><br />
Fetches a value by the given name and returns it to be used as if its identifier was specified. An example of this
in action looks like the following:
<code-block lang="%lang%">
define int exampleValue
%empty%
getValue("exampleValue") = 40
</code-block>
</def>

<def title="Fetch E0 Glyph">
<code>glyphE0(x, y = 0)</code> <format color='lightskyblue'>compile-time</format><br />
Fetches the character at a coordinate in <code>glyph_E0.png</code> and returns it as a <a href="Syntax.md" anchor="strings"> string literal.</a> The coordinate <code>0, 0</code> is the top left.
<note><code>x</code> wraps around and moves down if it crosses the right edge.</note>
</def>

<def title="Fetch E1 Glyph">
<code>glyphE1(x, y = 0)</code> <format color='lightskyblue'>compile-time</format><br />
Fetches the character at a coordinate in <code>glyph_E1.png</code> and returns it as a <a href="Syntax.md" anchor="strings"> string literal.</a> The coordinate <code>0, 0</code> is the top left.
<note><code>x</code> wraps around and moves down if it crosses the right edge.</note>
</def>

<def title="Minimum">
<code>min(a, b)</code> <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format><br />
Returns the smaller of the two inputs.
</def>

<def title="Maximum">
<code>max(a, b)</code> <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format><br />
Returns the larger of the two inputs.
</def>

<def title="Count Entities">
<code>countEntities(selector)</code> <format color='hotpink'>runtime</format><br />
Returns the number of entities which match the input selector as an integer.
<code-block lang="%lang%">
define int players
%empty%
players = countEntities(@a)
</code-block>
</def>

</deflist>

## Math Functions
These functions work with numbers and computation beyond what the [operators](Values.md#simple-math) can do.

<deflist>
<def title="Random">
<code>random(range)</code> <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format><br />
Returns a random number. Supports input of either a single value or a range.
When a range is specified, the minimum and maximum values will be used (inclusive).
When a single value is specified, the range (0..n-1) is used.
<code-block lang="%lang%">
define int choice
%empty%
// both cases can be 0, 1, 2, or 3
choice = random(0..3)
choice = random(4)
</code-block>
</def>

<def title="Square Root">
<code>sqrt(n)</code> <format color='lightskyblue'>compile-time</format><br />
Returns the square root of <code>n</code>. Runtime support is planned eventually.
</def>

<def title="Sine">
<code>sin(n)</code> <format color='lightskyblue'>compile-time</format><br />
Returns the sine of <code>n</code>. Runtime support is planned eventually.
</def>

<def title="Cosine">
<code>cos(n)</code> <format color='lightskyblue'>compile-time</format><br />
Returns the co-sine of <code>n</code>. Runtime support is planned eventually.
</def>

<def title="Tangent">
<code>tan(n)</code> <format color='lightskyblue'>compile-time</format><br />
Returns the tangent of <code>n</code>. Runtime support is planned eventually.
</def>

<def title="Arc-tangent">
<code>arctan(n)</code> <format color='lightskyblue'>compile-time</format><br />
Returns the angle that has a tangent of <code>n</code>. Runtime support is planned eventually.
</def>
</deflist>

## Rounding Functions
These functions focus on decimal numbers; specifically, rounding them to integers in different ways.

<deflist>
<def title="Round">
<code>round(n)</code> <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format><br />
Rounds the given number to the nearest integer. If the number is a midpoint (e.g., 1.5), the number is rounded up.
</def>

<def title="Floor">
<code>floor(n)</code> <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format><br />
Rounds the given number to the nearest integer that is less or equal to it. Generally referred to as "rounding down."
</def>

<def title="Ceiling">
<code>ceiling(n)</code> <format color='lightskyblue'>compile-time</format>, <format color='hotpink'>runtime</format><br />
Rounds the given number to the nearest integer that is greater or equal to it. Generally referred to as "rounding up."
</def>
</deflist>

