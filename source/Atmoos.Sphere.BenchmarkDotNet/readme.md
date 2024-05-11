# Atmoos.Sphere.BenchmarkDotNet

This library exports your benchmark results into the corresponding benchmark source files, which can be useful to keep track of the benchmarking results as work on the project progresses.

One scenario is to run all benchmarks before a release or major PR. Improvements and regressions will become visible in the diff.

For examples of this use case please see: [Atmoos.Sphere.Benchmark](https://github.com/atmoos/Sphere/tree/main/source/Atmoos.Sphere.Benchmark), where this library is used.

## Example Use

A simple Program.cs file might look something like this:

```csharp
using BenchmarkDotNet.Running;
using Atmoos.Sphere.BenchmarkDotNet;

var assembly = typeof(Program).Assembly;

var summary = BenchmarkSwitcher.FromAssembly(assembly).Run(args);
await assembly.Export(summary); // this exports your results
```
