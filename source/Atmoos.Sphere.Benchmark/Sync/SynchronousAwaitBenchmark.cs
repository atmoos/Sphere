using Atmoos.Sphere.Sync;
using BenchmarkDotNet.Attributes;

namespace Atmoos.Sphere.Benchmark.Sync;

[MemoryDiagnoser]
[ShortRunJob, WarmupCount(5), IterationCount(9)]
public class SynchronousAwaitBenchmark
{
    const ConfigureAwaitOptions continueOptions = ConfigureAwaitOptions.None;
    private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(63);

    [Benchmark(Baseline = true)]
    public async Task AsyncAwaitTaskDelay() => await Task.Delay(delay).ConfigureAwait(continueOptions);

#pragma warning disable CS0618 // Type or member is obsolete
    [Benchmark]
    public void SyncAwaitTaskDelay() => Task.Delay(delay).Await(continueOptions);
#pragma warning restore CS0618 // Type or member is obsolete
}

/* Summary

BenchmarkDotNet v0.15.4, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Max: 3.50GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.110
  [Host]   : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  ShortRun : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=9  LaunchCount=1  
WarmupCount=5  

| Method              | Mean     | Error    | Ratio | Allocated | Alloc Ratio |
|-------------------- |---------:|---------:|------:|----------:|------------:|
| AsyncAwaitTaskDelay | 63.15 ms | 0.084 ms |  1.00 |     336 B |        1.00 |
| SyncAwaitTaskDelay  | 63.10 ms | 0.123 ms |  1.00 |     232 B |        0.69 |
Summary */
