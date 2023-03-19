# BASIC Interpreter

This is an interpreter for a BASIC-like language written in C#.

This code follows the tutorial ["Make your own programming language in Python"](https://www.youtube.com/playlist?list=PLZQftyCk7_SdoVexSmwy_tBgs7P0b97yD) series on YouTube which is written in Python 3, and this code is rewritten in C#.

Note that there are two `Main` functions separated in two files ([`BasicInterpreter/Example.cs`](BasicInterpreter/Example.cs) and [`BasicInterpreter/Shell.cs`](BasicInterpreter/Shell.cs)). You may want to comment a file temporary before running another file.

To start with, you can run [`BasicInterpreter/Example.cs`](BasicInterpreter/Example.cs) which runs the [`BasicInterpreter/example.myopl`](BasicInterpreter/example.myopl) file. You may want to set the path to the `example.myopl` manually. You can also investigate the grammar rules (syntax) of the language in [`grammer.txt`](grammer.txt).

The other `Main` function is in a file called [`BasicInterpreter/Shell.cs`](BasicInterpreter/Shell.cs). You can try programming in the shell when you run the program.

For viewing all the built-in variables and functions that are avaliable, you can read [`BasicInterpreter/Basic.cs`](BasicInterpreter/Basic.cs).

## Progress Checklist

- [x] EP 1 - Lexer
- [x] EP 2 - Parser
- [x] EP 3 - Interpreter
- [x] Bonus - Power operator
- [x] EP 4 - Variables
- [x] EP 5 - Comparisons and logical operators
- [x] EP 6 - If statement
- [x] EP 7 - For and while statements
- [x] EP 8 - Function
- [x] EP 9 - Strings
- [x] EP 10 - Lists
- [x] EP 11 - Built-in functions
- [x] EP 12 - Multi-line statements
- [x] EP 13 - Return, continue, break
- [x] EP 14 - Run statements and comments (finale :tada:)

## Known Issues

1. Built-in functions `append()`, `pop()`, and `extend()` do not work because the built-in functions and the actual program use different `Context`. See `Execute()` in `BasicInterpreter/Runtime/BuiltInFunction.cs` and `Visit_CallNode()` in `BasicInterpreter/Interpreter.cs`.
