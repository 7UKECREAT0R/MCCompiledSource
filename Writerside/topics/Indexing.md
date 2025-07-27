# Indexing

<primary-label ref="runtime"/>

<link-summary>
Indexing explains how to use square brackets to access elements in arrays or strings and change value pointers.
</link-summary>

Indexers are sets of tokens placed inside square brackets `[]`. Its purpose is to pull an element from an array or to
change who (or what) a value points to, called "clarifying."

## Simple Indexing
Various types of objects support indexing. Examples are provided under each.

String Literals
: String [literals](Syntax.md#strings) can be indexed by number, resulting in the character at the given index.
If you index the string with a range, a substring will be returned (inclusive).<br /><br />
Unbounded ranges will return the rest of the string, allowing
you to get substrings without having to calculate any lengths.
```%lang%
$var text "Hello, World!"
$assert $text[4] == 'o'
$assert $text[7..11] == 'World'
$assert $text[7..] == 'World!'
$assert $text[..5] == 'Hello,'
```

Ranges
: You can index a range by either using (`0`, `min` or `minimum`) or (`1`, `max` or `maximum`) to get either element of it
respectively.<br /><br />
Specifying (`2` or `inverted`) returns a boolean identifying if the range is inverted.
```%lang%
$var zone 5..10
$assert $zone['min'] == 5
$assert $zone['max'] == 10
$assert $zone['inverted'] == false
```

JSON
: Indexing JSON is handled differently depending on what the JSON object is. If it's an object (with properties), it can
only be indexed using a string, being the property name. If it's a JSON array, it can only be indexed using an integer,
being the index of the item to retrieve.<br /><br />
[See examples, and more about indexing JSON here.](JSON-Processing.md#indexing)

Preprocessor Variables
: An alternative way to [dereference](Preprocessor.md#dereferencing) a preprocessor variable is using an indexer. When
indexing a preprocessor variable, you pull an object at a specific index and dereference it.
```%lang%
$var names "pig" "cow" "sheep"
$assert names[0] == "pig"
$assert names[2] == "sheep"
$assert names[1] == "cow"
```

## Clarifying
In some cases, you might need to assign a value to specific entities, rather than the executing entity `@s`. In this
case, you can use *clarification* to change who the assignment targets.

The gist of clarification is that you can index a value using a selector to change who it points to.
See the following example, which sets the value of all players to 60:
```%lang%
define time timeRemaining

timeRemaining[@a] = 60s
```

[A full section is dedicated to how to use clarification here.](Values.md#clarification)