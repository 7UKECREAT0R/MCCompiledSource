# Dialogue

<primary-label ref="runtime"/>

<link-summary>
NPC dialogue is automated, defined as scenes, and can run code; dialogues can be localized.
</link-summary>

NPC dialogue is always a pain to write; localizing text, creating naming conventions, and then having to connect it with
commands elsewhere in your project takes an unusually long amount of time.

MCCompiled contains features for defining, displaying, and running code right out of dialogue,
which as a result can be automated using [metaprogramming](Metaprogramming.md), or automatically localized with
[localization.](Localization.md)

## Syntax
The `dialogue` command is slightly different from the vanilla version of the command; most notably being the addition of
`dialogue new`. This syntax allows you to define a scene.

- `dialogue new <string: scene name> {...}`
- `dialogue open <npc> <player> [string: scene name]`
- `dialogue change <npc> <string: scene name> [player]`

## Defining Dialogue Scenes
A page of dialogue is called a "scene" in Minecraft. If you're familiar with writing dialogue, this will immediately
ring a bell. Defining dialogue is made to be as easy as possible and as closely integrated with the language as possible.

Let's begin with the `dialogue new` subcommand, which defines a new scene. The command must immediately be followed
by a block; however, the block does not contain regular statements. Each line in the block must be a valid field
followed by a comma.
```%lang%
dialogue new "exampleDialogue" {
    name: "NPC Name"
    text: "Why, hello there traveller."
    button: "Hello!" {
        print "The man gives you six emeralds to begin trading with."
        give @s gold_nugget 6
    }
    button: "Goodbye!" {}
}
```

### Forward Declaration...?
Since MCCompiled doesn't yet have two-pass compilation, it was easier to **disable validation entirely for dialogue
names**. This means it's possible to misspell the names of defined dialogue, but it's also possible to use dialogue
defined externally or create circular dialogues.

> This concept doesn't carry over to other parts of the language. For functions, you have to use
> [`partial`](Attributes.md#partial_examples) attribute to forward-declare them.

### Available Fields
All texts in dialogue fields support [localization](Localization.md), if enabled.
The fields that can be specified in dialogue definitions are as follows:

NPC Name
: Sets the name of the NPC showing this dialogue.
- `name: <string: name>`

Text
: Sets the text in the dialogue. This field supports placing `\n` to represent a new-line.
- `text: <string: text>`

Event: On Open
: The code to run when the dialogue is opened. Supports full MCCompiled code. The selector `@s` will refer to the
player who ran the dialogue.
- `onOpen: { ... }`

Event: On Close
: The code to run when the dialogue is closed. Supports full MCCompiled code. The selector `@s` will refer to the
player who ran the dialogue.
- `onClose: { ... }`

Button
: Adds a button to the dialogue with a name and action to perform when clicked. Supports all MCCompiled code. The
selector `@s` will refer to the player who ran the dialogue.
- `button: <string: text> { ... }`

> When designing the language, there were lots of problems that arose when trying to combine the programmer's intuition
> *("@i should be valid here, right?")* with the compiler's various optimizations.
>
> The selector fell out-of-scope way more than what was reasonable, so it ended up being best to wrap all the code in
> an `execute as @i at @s ...` internally.
> {title="Why @s instead of @i?" style="note"}

## Opening/Changing Dialogue
Once dialogue has been defined, it can be applied to an NPC so that it can be displayed when the NPC is interacted with 
or shown to a player manually. This is done using the `dialogue change` and `dialogue open` subcommands, respectively.

```%lang%
dialogue new "abc123" { ... }

// same as vanilla!
dialogue open @e[type=npc,name=Milton] @s "abc123"
```

> This is identical to the behavior of the vanilla command; no additional learning is needed.
> {style="tip"}
