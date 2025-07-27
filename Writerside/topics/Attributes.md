# Attributes

<primary-label ref="runtime"/>

<link-summary>
Attributes modify values and functions. Learn about value and function attributes, with examples and use-cases.
</link-summary>

Attributes are modifiers that change the way values or functions work. They can be specified in most places in
definition commands ([`define`](Values.md#defining-values) or [`function`](Functions.md#defining-functions)).

## Value Attributes
The following contains all attributes that can be applied to values, as well as examples and use-cases where applicable.

<snippet id="value_attributes">

<deflist>
    <def title="global">
        Makes the attached value global, meaning that the value's scoreboard objectives are <emphasis>always</emphasis>
        tied to the global fakeplayer <code>%fakeplayer%</code>.<br />
        Any attempts to use <a href="Values.md" anchor="clarification">clarifiers</a> on the value will result in a
        compile-time error. The value is guaranteed to never be attached to an entity.
        <h3 id="global_examples">Examples & Use-case</h3>
        Making values global is useful for anything that applies to the world, and not a specific player/entity. If
        you were designing a mini-game, for example, you would want the game-related code to be global, along with the
        compile-time guarantees that it will remain global.
        <code-block lang="%lang%" validate="false">
            define global int playersPassed
            define global time timeRemaining
            %empty%
            function display {
                globalprint "PASSED: {playersPassed} | TIME LEFT: {timeRemaining}"
            }
        </code-block>
    </def>
    <def title="bind">
        Binds the value to a given MoLang query. Requires one parameter, being a string that contains the query to use.
        <a href="https://github.com/7UKECREAT0R/MCCompiledSource/blob/master/mc-compiled/bindings.json">The current
        list of supported bindings is here.</a><br/>
        Most bindings come with pre-defined entities they attach to, but in cases where they don't, the entities can be
        manually specified by adding more string parameters to the end of the function, i.e., 
        <code>bind("query.is_sleeping", "fox")</code> to bind specifically to foxes.
        <h3 id="bind_examples">Examples & Use-case</h3>
        Binding replaces all cases where animation controllers were needed to access extra information about entities.
        <code-block lang="%lang%" validate="false">
            define bool bind("query.is_sleeping") isSleeping
            %empty%
            if not isSleeping {
                summon lightning_bolt
                print @a "{@s} IS RUINING THE SMP!!!"
            }
        </code-block>
    </def>
</deflist>

</snippet>

## Function Attributes
The following contains all attributes that can be applied to functions, as well as examples and use-cases where applicable.

<snippet id="function_attributes">

<deflist>
    <def title="extern">
        Makes a function external. External functions cannot have code in them, and their parameter names are interpreted
        verbatim. These functions will call the <code>.mcfunction</code> that matches their name and folder, making them
        great for combining MCCompiled with optimized handwritten functions or code from non-MCCompiled users.
        <h3 id="extern_examples">Examples & Use-case</h3>
        In cases where you have an <code>.mcfunction</code> file that cannot be ported or is not necessary to port over,
        it's better to declare an external function; making it possible to call the <code>.mcfunction</code> directly.
        <code-block lang="%lang%" validate="false">
            // BP/functions/library/example.mcfunction
            function extern library.example
            %empty%
            library.example()
        </code-block>
    </def>
    <def title="export">
        Marks a function for export. The function and any functions it calls will always be exported whether they're
        in use or not. See more about usage/exports <a href="Functions.md" anchor="exports">here.</a>
        <h3 id="export_examples">Examples & Use-case</h3>
        Excluding unused files is beneficial for many projects, but sometimes you'll have functions you run in-game through
        a command block, the <code>/function</code> command, or other methods MCCompiled doesn't know about. The export
        attribute covers those edge cases. In this example, the user wants to run <code>/function reset</code> in game.
        <code-block lang="%lang%" validate="false">
            function export reset {
                kill @e[type=item]
                itemsCollected[*] = 0
                print "Reset everything!"
            }
        </code-block>
    </def>
    <def title="auto">
        Makes the function automatically run every tick; or, if specified, at an interval. If the attribute is specified
        as-is, the function will be marked as "in use" and added to <code>tick.json</code>. If a parameter is given in
        ticks, the function will run on that interval using an auto-generated timer.
        <tip>The auto attribute cannot be applied to functions with parameters.</tip>
        <h3 id="auto_examples">Examples & Use-case</h3>
        Anywhere <code>tick.json</code> is needed, or something needs to happen on a timer, the auto attribute saves
        lots of boilerplate code and clearly communicates which functions automatically run. In the example below,
        <a href="Syntax.md" anchor="time-suffixes">time suffixes</a> are used instead of tick count.
        <code-block lang="%lang%" validate="false">
            function auto everyTick {
                // logic to run every tick.
            }
            function auto(1s) everySecond {
                // logic to run once a second.
            }
        </code-block>
    </def>
    <def title="partial">
        Makes a function <emphasis>partially implemented</emphasis>. When a function is partial, you can re-define it
        later as many times as you want, with each one appending its contents to the original function.
        <h3 id="partial_examples">Examples & Use-case</h3>
        Partial functions are useful with <a href="Metaprogramming.md">metaprogramming</a> or forward declaration, where
        you may want to use a function before its actual implementation. The following example shows how a function
        can be defined in two different places with their results merged.
        <code-block lang="%lang%" validate="false">
            function partial example {
                print "One!"
            }
            function partial example {
                print "Two!"
            }
            %empty%
            example()
            // One!
            // Two!
        </code-block>
    </def>
</deflist>

</snippet>