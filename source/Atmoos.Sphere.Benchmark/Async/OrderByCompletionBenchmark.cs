using Atmoos.Sphere.Async;
using BenchmarkDotNet.Attributes;

namespace Atmoos.Sphere.Benchmark.Async;

[MemoryDiagnoser]
public class OrderByCompletionBenchmark
{
    private List<Task<Int32>> tasks;

    [Params(128, 256, 512, 1024)]
    public Int32 Count { get; set; }

    [IterationSetup]
    public void Setup()
    {
        const Int32 delay = 2; // ms
        var random = new Random(87);
        var delays = Enumerable.Range(1, Count + 1).Select(d => d * delay).OrderBy(d => random.Next());
        this.tasks = delays.Select(d => Task.Delay(d).ContinueWith(t => d)).ToList();
    }

    [Benchmark]
    public async Task<Int32> Unordered() => await Sum(this.tasks).ConfigureAwait(false);

    [Benchmark(Baseline = true)]
    public async Task<Int32> OrderedByCompletion() => await Sum(this.tasks.OrderByCompletion()).ConfigureAwait(false);

    [Benchmark]
    public async Task<Int32> NaiveCompletionOrdering() => await Sum(NaiveOrdering(this.tasks)).ConfigureAwait(false);

    private static async Task<Int32> Sum(IEnumerable<Task<Int32>> tasks)
    {
        var sum = 0;
        foreach (var task in tasks) {
            sum += await task.ConfigureAwait(false);
        }
        return sum;
    }

    private static IEnumerable<Task<T>> NaiveOrdering<T>(IEnumerable<Task<T>> tasks)
    {
        var set = tasks.ToHashSet();
        while (set.Count > 0) {
            yield return Next(set);
        }

        static async Task<T> Next(HashSet<Task<T>> set)
        {
            var completed = await Task.WhenAny(set).ConfigureAwait(false);
            set.Remove(completed);
            return await completed.ConfigureAwait(false);
        }
    }
}

/* Summary

BenchmarkDotNet v0.15.7, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Max: 0.40GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 42.42.42.42424), X64 RyuJIT x86-64-v3
  Job-CNUJVU : .NET 10.0.0 (10.0.0, 42.42.42.42424), X64 RyuJIT x86-64-v3

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Gen0      | Allocated | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|----------:|----------:|------------:|
| Unordered               | 128   |   256.3 ms | 0.38 ms |  1.00 |         - |     336 B |        0.01 |
| OrderedByCompletion     | 128   |   256.6 ms | 0.50 ms |  1.00 |         - |   31360 B |        1.00 |
| NaiveCompletionOrdering | 128   |   256.4 ms | 0.54 ms |  1.00 |         - |  115512 B |        3.68 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 256   |   512.5 ms | 0.64 ms |  1.00 |         - |     336 B |       0.005 |
| OrderedByCompletion     | 256   |   512.1 ms | 0.72 ms |  1.00 |         - |   61440 B |       1.000 |
| NaiveCompletionOrdering | 256   |   512.9 ms | 0.55 ms |  1.00 |         - |  384440 B |       6.257 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 512   | 1,023.3 ms | 0.44 ms |  1.00 |         - |     336 B |       0.003 |
| OrderedByCompletion     | 512   | 1,023.9 ms | 0.59 ms |  1.00 |         - |  123392 B |       1.000 |
| NaiveCompletionOrdering | 512   | 1,023.7 ms | 0.71 ms |  1.00 |         - | 1257656 B |      10.192 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 1024  | 2,046.8 ms | 1.33 ms |  1.00 |         - |     336 B |       0.001 |
| OrderedByCompletion     | 1024  | 2,048.0 ms | 1.57 ms |  1.00 |         - |  246272 B |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,047.4 ms | 1.62 ms |  1.00 | 1000.0000 | 4591816 B |      18.645 |
Summary */
