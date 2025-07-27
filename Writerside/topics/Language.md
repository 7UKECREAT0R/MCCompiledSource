# Language

MCCompiled has a lot of features. Hopefully, most of them are documented here, along with guides that make them easy to
understand and begin using. MCCompiled features can be primarily split into two categories:
1. [Runtime](Runtime.md)
2. [Compile-time (preprocessor)](Preprocessor.md)

Runtime refers to the runtime features of the language, such as types, functions, variables (values), and so much
more. If you're studying the language for the first time, it may be more beneficial to start with the runtime
features of the language before diving into the preprocessor.

The Preprocessor refers to all types of code that can be run at compile time. There are many features that can be used
to generate runtime code, automate parts of the generation, and data-drive your projects using JSON.

## What's the Difference?
Compile-time actions *always* happen while the program is compiled, meaning nothing compile-time happens at runtime. This
principle is mutually exclusive; no runtime actions will run during compile-time, either.

As a result, there is a clear 
distinction made between which category all code falls into. Everything under [runtime](Runtime.md) will always run
in-game, and everything under [preprocessor](Preprocessor.md) will run during compile-time.