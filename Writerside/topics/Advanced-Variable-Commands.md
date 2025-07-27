# Advanced Variable Commands

<primary-label ref="compile_time"/>

<link-summary>
String and data manipulation commands to modify text, extract info from arrays, and manipulate arrays.
</link-summary>

Beyond simple addition, subtraction, and the likes of it, there are commands dedicated to some more advanced usages
that will be touched on in detail here.

## String Manipulation
There are three commands dedicated to modifying the way strings look. These are useful when fine-tuning some text that
will be displayed to a user. These commands are good because MCCompiled doesn't feature a good way to manipulate strings
character by character. Each command takes in two parameters; first, the identifier of the preprocessor variable that
will hold the result. Second, the identifier of the input data.

If only one identifier is specified, the language assumes you want to *destructively modify* the input variable.
It's better to do this *only if you're no longer going to use the original data*.

All of these commands perform their operation on *all* values in the preprocessor variable. So, running `$strupper`
on a set of `hello` `world` would result in `HELLO` `WORLD`. If one of the values in the input is not a string,
it is left untouched.

<tabs id="string_ops">
    <tab title="$strupper">
        <deflist>
            <def title="Preprocessor String Uppercase">
                <p>Transforms the given input string(s) to <code>UPPERCASE</code>.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$strlower">
        <deflist>
            <def title="Preprocessor String Lowercase">
                <p>Transforms the given input string(s) to <code>lowercase</code>.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$strfriendly">
        <deflist>
            <def title="Preprocessor String Friendly Name">
                <p>
                    Transforms the given input string(s) to <code>Title Case</code>, which is more user-friendly.
                    Additionally, converts any characters like <code>_</code> or <code>-</code> to spaces for user display.
                </p>
            </def>
        </deflist>
    </tab>
</tabs>

## Data Manipulation
These commands focus on extrapolating info from data (generally numbers) inside an array of objects. Each of them takes
in two parameters; first, the identifier of the preprocessor variable that will hold the result. Second, the identifier
of the input data.

If only one identifier is specified, the language assumes you want to *destructively modify* the input variable.
It's better to do this *only if you're no longer going to use the original data*.

<tabs>
    <tab title="$sum">
        <deflist>
            <def title="Preprocessor Array Sum">
                <p>Retrieve the sum of all elements in the given array.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$median">
        <deflist>
            <def title="Preprocessor Array Median">
                <p>Retrieve the middle element of the given data, or the average of the two middle values if even.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$mean">
        <deflist>
            <def title="Preprocessor Array Mean">
                <p>Retrieve the mean (average) of all the elements in the given array.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$len">
        <deflist>
            <def title="Preprocessor Array Length">
                <p>Retrieve the number of elements (length) in the given array.</p>
            </def>
        </deflist>
    </tab>
</tabs>

## Array-Specific Manipulation
These commands are dedicated to manipulating preprocessor arrays and don't really serve a purpose on variables with
single values. All of these commands destructively modify a single parameter, which is the identifier of the variable
to modify.

<tabs>
    <tab title="$sort">
        <deflist>
            <def title="Preprocessor Array Sort">
                <p>
                    Sorts the values in the given array either ascending or descending.
                    <br /><br />
                    Use the syntax: <code>$sort [ascending|descending] [id: variable]</code>
                </p>
            </def>
        </deflist>
    </tab>
    <tab title="$reverse">
        <deflist>
            <def title="Preprocessor Array Reverse">
                <p>Reverse the order of the elements in the given array.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$unique">
        <deflist>
            <def title="Preprocessor Array Unique Values">
                <p>Prune all non-unique values from the given array; as in, any value which == another value in the array.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$append">
        <deflist>
            <def title="Preprocessor Array Append">
                <p>Adds the given item(s) to the end of the given preprocessor variable, or contents of another preprocessor variable if specified.</p>
            </def>
        </deflist>
    </tab>
    <tab title="$prepend">
        <deflist>
            <def title="Preprocessor Array Prepend">
                <p>Adds the given item(s) to the start of the given preprocessor variable, or contents of another preprocessor variable if specified.</p>
            </def>
        </deflist>
    </tab>
</tabs>