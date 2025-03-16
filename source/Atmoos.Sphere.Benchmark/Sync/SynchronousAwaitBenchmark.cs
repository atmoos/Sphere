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

BenchmarkDotNet v0.14.0, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.104
  [Host]   : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=9  LaunchCount=1  
WarmupCount=5  

| Method              | Mean     | Error    | Ratio | Allocated | Alloc Ratio |
|-------------------- |---------:|---------:|------:|----------:|------------:|
| AsyncAwaitTaskDelay | 63.92 ms | 0.272 ms |  1.00 |     464 B |        1.00 |
| SyncAwaitTaskDelay  | 63.92 ms | 0.186 ms |  1.00 |     360 B |        0.78 |
Summary */
