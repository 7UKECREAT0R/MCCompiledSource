# Types

<primary-label ref="runtime"/>

<link-summary>
Types in MCCompiled define how values interact, display, and convert; examples include int, decimal, boolean, and time.
</link-summary>

Values can have their own *types*, a way of changing how they interact, add, display to users, convert, etc...
You can think of types as presets that change everything about the value in a way that is unified and makes sense.

The greatest thing about types in MCCompiled is that they are very back-seat. They work as you would expect and don't
require you to think about conversion anywhere. If it makes sense, then everything just works.

## Defining Value with a Type
When defining a value, you optionally can specify a type when using <include from="Values.md" element-id="define_with_type"/>.
The most common type is the [`int`](#int)

## Built-In Types

<tabs>
<tab title="Integer" id="int">
    <p>The <code>int</code> is identical to a Minecraft scoreboard objective.</p>
    <h3 id="int_limits">Limits</h3>
    <p>
        Integers are limited to <code>-2^31</code> to <code>2^31 - 1</code>. If you overflow an integer by making it
        exceed these limits, it will wrap back around into the negatives/positives depending on which way you overflowed.
    </p>
    <h3 id="int_display">Display</h3>
    <p>
        When displayed to a player, integers are shown as-is with no formatting applied.
    </p>
    <h3 id="int_features">Language Features</h3>
    <p>
        Integers support all default language features.
    </p>
    <h3 id="int_example">Example</h3>
    <code-block lang="%lang%">
        define int exampleValue
        exampleValue = 42
        exampleValue += 10
        print "The value is: {exampleValue}"
        // The value is: 52
    </code-block>
</tab>
<tab title="Decimal" id="decimal">
    The <code>decimal</code> can be a decimal number. They're represented as a single
    scoreboard objective as a <a href="https://en.wikipedia.org/wiki/Fixed-point_arithmetic">fixed point number</a>.
    As such, decimal numbers need to have their precision specified at the time of definition: <code>decimal &lt;precision&gt;</code>
    <h3 id="decimal_limits">Limits</h3>
    <p>
        Decimals are limited based on their precision. Where P=Precision, <code>-2^31 * (0.1^P)</code> to <code>(2^31 - 1)  * (0.1^P)</code>
        if you overflow an integer by making it exceed these limits, it will wrap back around into the negatives/positives
        depending on which way you overflowed.
    </p>
    <warning title="Multiplication/Division Limitations">
        With multiplication and division operations specifically, they hit the limit of their representations more quickly than others.
        When two numbers of the same precision are multiplied/divided, their precision temporarily doubles before being rounded
        back down to the intended precision. This may cause overflow much faster than you may expect.
    </warning>
    <p>
        To prevent from overflowing prematurely, use precisions as low as you're comfortable with and avoid multiplication
        or division with numbers that are of precision 3 or higher (as a general rule; not applicable everywhere).
    </p>
    <h3 id="decimal_display">Display</h3>
    <p>
        When displayed to a player, decimal numbers are displayed as expected, using a <code>.</code> to denote the
        decimal part. Zeros will pad the left side of the decimal part, if needed.
    </p>
    <h3 id="decimal_features">Language Features</h3>
    <p>
        Decimals support all default language features.
        When an operation is performed between two decimal numbers and their precisions don't match, the right-hand
        value is <emphasis>always</emphasis> scaled to match the precision of the land-hand value automatically.<br />
        <br />
        Additionally, if you define a <a href="Values.md" anchor="type-omission">value without a type</a>, opting to
        specify a decimal number instead, the precision of the defined value will accurately reflect the input.
        <code>3.14</code> would be given a precision of two, <code>1.5</code> would be given a precision of one, etc...
    </p>
    <h3 id="decimal_example">Example</h3>
    <code-block lang="%lang%">
        define decimal 3 exampleValue
        exampleValue = 42.521
        exampleValue += 10.1
        print "The value is: {exampleValue}"
        // The value is: 52.621
    </code-block>
</tab>
<tab title="Boolean" id="boolean">
    The <code>boolean</code> can be either true or false. They're represented as a single scoreboard objective that is
    either one (true) or zero (false).
    <h3 id="boolean_limits">Limits</h3>
    <p>
        Booleans are limited to only two values by design, as they represent a limited state. They can be set to the
        literal <code>true</code> or <code>false</code>, nothing else.
    </p>
    <h3 id="boolean_display">Display</h3>
    <p>
        When displayed to a player, booleans are represented as pieces of text; by default, this is "true" or "false."
        This can be changed by setting the preprocessor variables <code>_true</code> and <code>_false</code> respectively to the string you
        wish to be displayed. When <a href="Localization.md">localization</a> is enabled, entries are automatically created as with
        any other strings.
    </p>
    <h3 id="boolean_features">Language Features</h3>
    <p>
        Booleans do not support arithmetic, only assignment, comparison, and display.
        The <a href="Comparison.md" anchor="comparing-booleans"><code>if</code></a> command supports placing only the
        boolean variable on its own without needing any comparison operators. When this is the case, the boolean value
        will be checked to see if it's true.
    </p>
    <h3 id="boolean_example">Example</h3>
    <code-block lang="%lang%">
        define boolean isGameRunning
        isGameRunning = true
        %empty%
        // Setting how the boolean will be printed to players
        $var _true "Running"
        $var _false "Paused"
        %empty%
        print "Game Status: {isGameRunning}"
        // Game Status: Running
    </code-block>
</tab>
<tab title="Time" id="time">
    The <code>time</code> acts the same as a regular integer, but with extra features for display.
    <h3 id="time_limits">Limits</h3>
    <p>
        Time values have the same limits as integers; limited to <code>-2^31</code> to <code>2^31 - 1</code>.
        If you overflow a time by making it exceed these limits, it will wrap back around into the negatives/positives
        depending on which way you overflowed.
    </p>
    <h3 id="time_display">Display</h3>
    <p>
        When displayed to a player, times are formatted based on the <code>_timeformat</code> preprocessor variable.
        This variable is a string using lettering and colons to describe how the time should be divided, and where zeros
        should be inserted.
    </p>
    <h4 id="time_display_detail">Time Format</h4>
    <p>
        The default time format is <code>m:ss</code>. This implies that the number of minutes should be displayed (m),
        and the number of seconds should be displayed after it (s). The two s's indicate that they should be padded to
        always have a width of 2, so if the number of seconds is, for example, <code>6</code>, then it should display
        as <code>06</code>.<br />
        <br />Let's try with <code>h:mm:ss</code>. It will display the number of hours padded to a width of 1, the
        number of minutes padded to a width of 2, and the number of seconds padded to a width of 2; example: "0:12:05"
    </p>
    <h3 id="time_features">Language Features</h3>
    <p>
        Times support all default language features, including <a href="Syntax.md" anchor="time-suffixes">time suffixes</a>.
    </p>
    <h3 id="time_example">Example</h3>
    <code-block lang="%lang%">
        define time timeRemaining
        timeRemaining = 1m + 30s
        timeRemaining -= 7s
        %empty%
        // Setting how the time will be printed to players
        $var _timeformat "mm:ss"
        %empty%
        print "Time Remaining: {timeRemaining}"
        // Time Remaining: 01:23
    </code-block>
</tab>
</tabs>
