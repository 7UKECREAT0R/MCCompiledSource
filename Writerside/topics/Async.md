# Async

<primary-label ref="runtime"/>

<link-summary>
Async functions simplify async code with automatic state machines, await commands, and defining global/local execution.
</link-summary>

In a world of schedulers, state machines, and loads of nested functions, MCCompiled's async implementation is built to
remove ALL boilerplate and focus entirely on logic and structure.

Async functions generate state machines automatically, set up to either run locally on an entity or globally without
any executing context (directly from `tick.json`).

## Defining
To define an async function, the attribute `async` can be used. The attribute accepts one parameter, being the word
`global` or `local`, which defines if the async state machine should be run globally or on the executing entity only.
```%lang%
// does a countdown before starting the game.
function async(global) countdown() {

}

// gives the executing player a diamond after one second.
function async(local) giveDiamondLater() {

}
```

## Awaiting
The `await` command is only usable inside async functions. It allows you to wait for a specific amount of time, wait for
a condition to be met, or wait for another async function to finish executing.

### Awaiting an amount of time  {id="await_time"}
This is the most common use of async code. To await a certain amount of time, use the syntax `await <time>`, with `time`
being the number of ticks to wait. You can use [time suffixes](Syntax.md#time-suffixes) to make this number more concisely
defined.
```%lang%
function async(global) countdown() {
    globalprint "Three..."
    await 1s
    globalprint "Two..."
    await 1s
    globalprint "One..."
    await 1s
    globaltitle "GO!"
}
```

### Awaiting a condition {id="await_condition"}
To await a condition, use either syntax depending on what makes the most sense:
- `await until <condition>`
- `await while <condition>`

These will wait until the given condition is true/false respectively to continue execution. You can wait
_until_ a condition is met or wait _while_ a condition is met.

The conditions
that can be used are the same as what's available with [`if` statements](Comparison.md), [`while` loops](Loops.md#while), etc.
```%lang%
function async(local) forceGrabBow() {
    if not @s[hasitem={item=bow}}] {
        print "Grab the bow."
        await until @s[hasitem={item=bow}}]
    }
    
    // ...
}
```

### Awaiting another function {id="await_function"}
To wait for another async function to complete its execution, call it and pass the result as a parameter to `await`.
Async functions return an `awaitable`, and as such are not able to return any other type of value.

> You cannot await an `async(local)` function from an `async(global)` function, as there's no way to single out which
> entity to wait on.
> {style="warning"}

```%lang%
function async(global) countdown(int seconds) {
    repeat seconds i {
        print "Round starts in {seconds + 1}..."
        await 1s
    }
}

function async(global) startNextRound() {
   await countdown(10)
   print "GO!"
   round += 1
   running = true
}
```

## Warning as of 1.18

>As of MCCompiled 1.18, the use of `await` is not supported inside [runtime loops (repeat, while)](Loops.md); you will
>get a compiler error if this happens. Support is coming, but trying to port existing code over to work both with functions
>and scheduled stages is a monumentally complex task.
>
>No ETA on this right now, but it's in a similar priority to some other features like block states support, more commands, etc.
> {style="warning"}

