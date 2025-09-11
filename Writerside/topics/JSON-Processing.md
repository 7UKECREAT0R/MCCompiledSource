# JSON Processing

<primary-label ref="compile_time"/>

<link-summary>
JSON Processing: load, traverse, index JSON data in MCCompiled with $json command and path option.
</link-summary>

When you need to access bigger or more complex sets of data that don't make sense to store in the source code, or
co-processing with the output from other applications, JSON has you covered.
MCCompiled features support for simple JSON processing and data traversal using a simple feature-set.

## Loading JSON
Getting the JSON file into your project requires using the `$json` command. The simplest syntax available is
`$json [file] [result]`, where `file` is the file to load, and `result` is the preprocessor variable identifier to store
the JSON in.
```%lang%
$json "resources/data.json" stuff
```

> Files are loaded relative to the active project's file, i.e.: Specifying `file.json` would mean it needs to be in the
> same folder as the project's `%ext%` file.

### Specifying Load Path {id="load_path"}
There is an optional parameter at the end of the `$json` command called "path." The path tells MCCompiled where to
traverse the JSON before loading it into the preprocessor variable. As an example, take the following JSON file:
```json
{
  "version": 4,
  "data": {
    "coordinates": [5, 130, -53],
    "name": "Home",
    "owners": [
      {
        "name": "Jeremiah",
        "age": 16
      },
      {
        "name": "Jackson",
        "age": 23
      },
      {
        "name": "Jonah",
        "age": 19
      }
    ]
  }
}
```
{collapsible="true" default-state="collapsed"}

If you didn't care about the version and just wanted to access the object "data," then using a path would make sense.

#### Path Format
> Note that this is **not** the main way to use JSON in MCCompiled. Paths simply serve as a convenience method of
> ignoring data that is not going to be used in your project.
> {style="warning"}

Paths are basic and lax by design. A path is a string made up of "elements," separated by either a period `.` or
either type of slash: `/` or `\`. Each element in a path denotes where next to traverse the JSON. If you encounter a
JSON array, you need to specify an integer rather than a string.

With the example text above, to fetch the element "data" you would specify the path:
```%lang%
$json "resources/data.json" stuff "data"
```

To get the first element of "owners":
```%lang%
$json "resources/data.json" stuff "data/owners/0"
```

## Indexing JSON {id="indexing"}
Now that you have loaded the JSON, you have a preprocessor variable containing either a JSON object or array. From here,
you are free to *index* this JSON further or dereference the preprocessor variable to inlay the data in your code, just
like a regular preprocessor variable.

> JSON arrays are different from preprocessor arrays. As an example, you could have a preprocessor variable containing
> multiple JSON arrays. Preprocessor variables are always able to hold as many values in them as you would like.

Indexing is done using the regular index operator. Most importantly, an indexing operation must start with a `$` to
indicate that you wish to dereference and *then* index the result. Without a dereference, [indexing a preprocessor
variable](Preprocessor.md#dereferencing-using-indexer) will just return an element of it.
You can read more about this [in this page dedicated to disambiguating the topic.](Indexing-PPVs.md)

```%lang%
$json "resources/data.json" stuff "data"

tp @a $stuff["coordinates"]
```

### Chaining Indexers
You can also chain indexers together (still only requiring dereferencing at the start) to traverse the JSON more than
one step at a time. Consider the example above, but without using a [path](#load_path) when loading it.
```%lang%
$json "resources/data.json" stuff

tp @a $stuff["data"]["coordinates"]
```

### Indexing Arrays
When indexing a JSON *array*, it must be indexed using an integer, since there are no named keys to go off. Integers
must be specified without quotation marks, as with the rest of the language.
```%lang%
$json "resources/data.json" stuff "data"

$var name $stuff["owners"][2]["name"]
print "$name has joined!"
```