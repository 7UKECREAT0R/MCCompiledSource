# Localization

<primary-label ref="compile_time"/>

<link-summary>
Localization allows for easy translations in multi-language projects using .lang files and keys/values.
</link-summary>

When writing code that is going to be deployed to more than one language, it's necessary to use a `.lang` file to
hold the keys/values for all pieces of text that show up in the project. MCCompiled offers support for automatically
generating `.lang` files and insetting keys into the compiled result. Lang file keys are named after their context and
sorted alphabetically.

## Setting Up
By default, strings are inlined everywhere in the compiled output. To set up localization, use the `lang` command.
The command accepts an identifier or string, being the locale to default to. If your project is written in English,
use `en_US`, for example.

> See [here](https://www.ibm.com/docs/en/rational-soft-arch/9.7.0?topic=overview-locales-code-pages-supported)
> for a list of all country codes with their attached languages.

It's recommended to place the command at the *top* of your file so that everything applies. If you're creating a library
file for others to use, don't use `lang` at all, since it would override their setting.

```%lang%
lang en_US
```

## What Uses Localization?
Once localization is enabled for a given language, you need an expectation of what will be in the `.lang` file once
the project compiles. Anything that supports raw-text will use translation; take commands like `print` or `actionbar`.

When a value is displayed that uses text, such as with [booleans](Types.md#boolean), its entries will also be added
to the lang file.

## Merging Keys
When two or more `lang` file entries contain the same text, it's possible to merge them into one. By default, this
behavior is enabled. Whether to merge or not is controlled by the `_lang_merge` [preprocessor variable](Preprocessor.md),
containing a boolean true/false value. If `_lang_merge` is `false`, language entries will not be merged.
```%lang%
// enable lang file merging (default)
$var _lang_merge true

// disable lang file merging
$var _lang_merge false
```