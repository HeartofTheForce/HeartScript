# HeartScript

C-like IL compiled custom language

# Functionality

-   Control-flow (if-else, for loop, while loop, do while loop)
-   Primtive types int, double, bool
-   Arrays
-   Methods, Method Overloading
-   .NET Method injection
-   Arithmetic Operators

# Example

`dotnet run -p ./src/HeartScript.Cli ./src/demo.hs`

-   Quicksort implementation [demo.hs](./src/demo.hs)
-   Grammar used to parse the language [demo.hg](./src/demo.hg)

# How it works

-   Generate a Parse Tree using [Heart.Parsing](https://github.com/HeartofTheForce/Heart.Parsing)
-   Rewrite Parse Tree into an Abstract Syntax Tree
-   Emit IL while traversing Abstract Syntax Tree

# Missing things

-   User friendly error reporting, invalid operations will not provide the relevant location in the source file
-   [Definite assignment analysis](https://en.wikipedia.org/wiki/Definite_assignment_analysis) No error checking for definite assignment, variables are initialized to their default value
-   [Data-flow analysis](https://en.wikipedia.org/wiki/Data-flow_analysis) No error checking for returns values (returns default value of method return type)

# Further Reading

-   https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/how-to-define-a-generic-type-with-reflection-emit
-   https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes
