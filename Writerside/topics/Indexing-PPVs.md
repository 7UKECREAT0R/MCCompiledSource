# PPV Indexing Disambiguation

<primary-label ref="compile_time"/>

<link-summary>
Explains the nuances of using preprocessor variables as lists and using them with indexers.
</link-summary>

Preprocessor variables don't just store single values,
they work like lists out of the box! They usually only contain one item, but you can store as many as you'd like.
Indexers allow you to retrieve specific elements from a preprocessor variable,
but when it comes to the difference between including/not including the dereference `$`
operator, things can get a bit confusing at first.

## Basics
Indexers can be used on much more than just preprocessor variables.
See [the "Indexing" page](Indexing.md) on how to use indexers
in other ways.

## Simple Indexing
An important distinction to make is the difference between `name[i]` and `$name[i]`. In MCCompiled's order of operations,
the dereference `$` is run before the indexer.
This means that there's a clear and drastic difference with what happens depending on if you have the `$` or not.

If you're looking to get an element of a preprocessor variable, you just need to place _just_ the `[]` indexer.
The example code below shows fetching a name from a list of names:
```%lang%
$var names "Luke" "Lucas" "Lucy" "Lucario" "Luca"
%empty%
$assert names[2] == "Lucy"
```
The key takeaway is that there's no dereference `$` operator required when using an indexer. The indexer handles the dereferencing of just a single element of the variable.

Now, what would happen if we were to dereference `$` the variable before indexing it? *All* the values inside the PPV would be inserted, and then because of the order-of-operations, the *last* element in the list would be indexed instead!
This produces a result that would never be useful in this case:
```%lang%
$var names "Luke" "Lucas" "Lucy" "Lucario" "Luca"
%empty%
$assert "$names[2]" == "Luke Lucas Lucy Lucario c"
```

> We're wrapping `$names[2]` in quotes so that it gets dereferenced into a **single** string that we can compare with a **single** other string. The functionality is exactly the same, though.
{title="Note" style="note"}

As you can see, the character at index `2` of the last name is "c", which is what is present at the end of the result.
This is because the variable was dereferenced,
causing it to insert **all** of its elements, then the indexer ran on the **last** element only.

### When this behavior is useful {collapsible="true" default-state="collapsed"}
Even though in that particular example, indexing and dereferencing at the same time seem completely useless,
it's a very useful pattern when a preprocessor variable only contains one single element.
In this example, we use an indexer with a range to get a substring of the text:
```%lang%
$var example "Heck Yeah"
%empty%
// swear filter
$assert $example[5..] == "Yeah"
```

## Indexing in Strings/Selectors
Here's where we get a little bit unconventional because there's a clear difference between the presence of a dereference `$` operator or not.
It's the same as the example above, but the lack of a clear indicator may be a bit jarring at first.
```%lang%
$var spawn 400 106 240
%empty%
// iterate over all players not at spawn
for @a[x=spawn[0],y=spawn[1],z=spawn[2],rm=50] {
    print "Teleporting to spawn (spawn[0], spawn[1], spawn[2])..."
}
```
This example highlights using indexing to split apart a coordinate which we know the length of ahead-of-time.
Most importantly, there's no `$` dereference operator,
so it's not immediately clear what's going on until you really read over the code.
This is the tradeoff for how powerful preprocessor variables can be without any complex constructs or types though,
and once you fully get it, it's a nice choice to have.