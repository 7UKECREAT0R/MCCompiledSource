# Playsound

<primary-label ref="runtime"/>

<link-summary>
Playsound feature in MCCompiled allows importing audio files for projects.
Enabling feature, using .wav, .ogg, or .fsb files and sub-folders.
</link-summary>

MCCompiled supports the native syntax of the `playsound` command but also has a convenience method for easily importing
audio files and setting them up for use in your projects.

## Enabling the Feature
You must have the `audiofiles` feature enabled. See [here](Optional-Features.md#audio-files) on how to enable features.
This functionality is not enabled by default because it creates entries in the `sound_definitions.json` file and copies
audio files every compilation.

## Using Audio Files
Once the feature is enabled, the `playsound` command will accept an audio file input of either `.wav, .ogg, or .fsb`.

> As of now, audio files defined this way are defined as `ui` sounds, and there's no syntactic way to change it. If you
> have suggestions, post about it on the [Discord](%discord_url%) or open an issue on [GitHub](%github_url%)!
> {style="warning"}

```%lang%
feature audiofiles
playsound "resources/explosion.wav" @a ~ ~ ~
```

### Sub-folders
In the example above, the `explosion.ogg` audio file is located in the subfolder `resources`. As such, the file will be
copied into the subfolder `sounds/resources` in the output. This rule applies to any file that's relative to the
location of the project file.