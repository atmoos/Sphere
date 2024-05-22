<!-- markdownlint-disable MD033 MD041 -->
<div align="center">
 <img src="./assets/images/atmoos.sphere.svg" height="160" alt="Logo">
</div>
<!-- markdownlint-enable MD033 MD041 -->

# Atmoos.Sphere

A collection of .Net mechanisms to simplify daily work as a .Net developer.

[![main status](https://github.com/atmoos/Sphere/actions/workflows/dotnet.yml/badge.svg)](https://github.com/atmoos/Sphere/actions/workflows/dotnet.yml)
[![nuget package](https://img.shields.io/nuget/v/Atmoos.Sphere.svg?logo=nuget)](https://www.nuget.org/packages/Atmoos.Sphere)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/atmoos/Sphere/blob/master/LICENSE)

Currently this is mostly a collection of patterns that I frequently encountered and which I was able to convert into an easy to use mechanism without introducing any dependencies to other libraries.

As such, this library contains very few abstractions, but many extension methods.

Please see the individual project's readme files for a list of mechanisms and the corresponding test projects on how to best use these mechanisms.

## Libraries

- [Atmoos.Sphere](source/Atmoos.Sphere)
  - A collection of mechanisms predominately exposed as extension methods.
- [Atmoos.Sphere.BenchmarkDotNet](source/Atmoos.Sphere.BenchmarkDotNet)
  - A mechanism to export benchmark results into the corresponding benchmark source files.

## High Level Focus & Goals

We focus on commonly re-occurring programming patterns and offer a re-usable and *tested* version of it as a mechanism.

- Wrapping "old-style" patterns into current .Net types.
- Dealing with patterns that are easy to understand but hard to get right when taking all edge cases into account.
- Providing reliable patterns covered extensive by high quality tests.
- Improving performance by creating high performance mechanisms.

Essentially, an attempt to make your life easier by providing performant and rigorously tested mechanisms for commonly occurring patterns.
