# Testing

<primary-label ref="runtime"/>

<link-summary>
Writing and running tests to ensure code correctness in projects.
</link-summary>

When designing larger projects, it becomes increasingly more important to ensure every little piece is not thrown off
balance. This is where testing comes in.

This page focuses more on runtime testing. If you're interested in catching errors at compile-time, [see here](Debugging.md#assertions)

## Enabling Tests
Because tests create extra files in your output, they are guarded behind the `tests` feature. To enable, specify that
you wish to use tests at the top of your file:
```%lang%
feature tests
```

## Writing a Test
Tests are defined like functions without parameters, except using the keyword `test`. Inside a test, you can include
code that will be run as the player running the test (not global context).

A test requires at least one assertion using the `assert` command to determine if it passed or failed. Writing assertions
is the same as writing a [runtime comparison](Comparison.md); in fact, it uses the same underlying code.

The following example shows writing a test to make sure that the `abs()` function (defined at the top) is working as
intended.
```%lang%
function abs(int n) {
    if n < 0
        return n * -1
    else
        return n
}

test absoluteValue {
    assert abs(5) == 5
    assert abs(0) == 0
    assert abs(-3) == 3
    assert abs(-8329) == 8329
}
```
Each assertion is testing a different case. In projects that include pure functions or important state machines, testing
saves time by automating as much of the validation process as possible. It creates a list of guarantees that don't have
to be manually validated by quality assurance.

## Running Tests
You can't run tests like functions; they are automatically packed into a function called `test` in the root of your
behavior pack's functions. When a player runs `test`, every test defined in your project is automatically run from top
to bottom.

If a test fails, execution stops and debug information is displayed to alert you as to which test failed, which assertion
in that test, and what the values were at the time of assertion.

![animation_tests_failed.gif](animation_tests_failed.gif)

If all assertions in the test pass, it is displayed to the user as "passed."

![animation_tests_succeeded.gif](animation_tests_succeeded.gif)