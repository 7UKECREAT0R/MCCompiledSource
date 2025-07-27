# About
MCCompiled %latest_version% is an open source programming language designed for anyone and everyone who writes Minecraft commands.
It compiles code down to a behavior pack based on a context-aware compiler. MCCompiled code is intentionally built to
mirror Minecraft commands but with many extended features.

---

### Why?
Commands are hard. Not just because there are so many nuances to remember or that basically everything you make is a
workaround of something else;
There's just a lot of code to get a little bit done. *Automation* is the natural next step, but the way to approach it
isn't immediately obvious given the technical hurdles of writing Minecraft commands in the first place.

MCCompiled takes a C-style approach to code, trying to abstract away some parts of Minecraft's command system while
leaving others which are more desirable. Primarily, things like Scoreboard Objectives (values), Rawtext, Translation Keys,
Control Flow, and Functions are focused on.
Everything else is left untouched to prevent from needing to learn more than necessary.

```%lang%
define global int highScore
define int score

function reset {
    // loop over all players, set high score if they hit it.
    for @a {
        if score > highScore
            highScore = score
    }
    // reset the score of all players
    score[@a] = 0
}
```
{validate="false"}