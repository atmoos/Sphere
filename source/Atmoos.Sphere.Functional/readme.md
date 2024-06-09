# Atmoos.Sphere.Functional

A handful of types that support a more functional approach to C#.

[![nuget package](https://img.shields.io/nuget/v/Atmoos.Sphere.Functional.svg?logo=nuget)](https://www.nuget.org/packages/Atmoos.Sphere.Functional)

## Functional

C# is not a functional language. However, over time it is gaining more and more functional features. The continuous work on pattern matching capabilities is one such example.

This library contains a few types that lend themselves well to a functional style in C#.

## Types

### [Maybe](Maybe.cs)

A simple monadic maybe type, to indicate that a result may or may not occur without indicating an error as such.

Note that this type is almost homologous to a nullable type `T?`.

### [Result](Result.cs)

A slightly more involved type to represent a result. Again a monadic type, but this time a result is expected but errors may occur, they may even be expected. Contrary to the `Maybe` type not being able to compute a result value must be considered an error and should be appropriately handled.
