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

BenchmarkDotNet v0.14.0, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.104
  [Host]     : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2
  Job-MFPBZR : .NET 9.0.3 (9.0.325.11113), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Gen0      | Gen1      | Allocated  | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|----------:|----------:|-----------:|------------:|
| Unordered               | 128   |   256.6 ms | 0.50 ms |  1.00 |         - |         - |    1.33 KB |        0.04 |
| OrderedByCompletion     | 128   |   256.4 ms | 0.65 ms |  1.00 |         - |         - |   31.63 KB |        1.00 |
| NaiveCompletionOrdering | 128   |   256.5 ms | 0.80 ms |  1.00 |         - |         - |  116.22 KB |        3.67 |
|                         |       |            |         |       |           |           |            |             |
| Unordered               | 256   |   512.4 ms | 0.24 ms |  1.00 |         - |         - |    1.33 KB |        0.02 |
| OrderedByCompletion     | 256   |   512.5 ms | 0.62 ms |  1.00 |         - |         - |    61.5 KB |        1.00 |
| NaiveCompletionOrdering | 256   |   512.7 ms | 0.44 ms |  1.00 |         - |         - |  360.01 KB |        5.85 |
|                         |       |            |         |       |           |           |            |             |
| Unordered               | 512   | 1,023.2 ms | 0.84 ms |  1.00 |         - |         - |    1.33 KB |        0.01 |
| OrderedByCompletion     | 512   | 1,024.0 ms | 0.59 ms |  1.00 |         - |         - |  121.63 KB |        1.00 |
| NaiveCompletionOrdering | 512   | 1,023.8 ms | 0.52 ms |  1.00 |         - |         - | 1229.13 KB |       10.11 |
|                         |       |            |         |       |           |           |            |             |
| Unordered               | 1024  | 2,048.0 ms | 1.40 ms |  1.00 |         - |         - |    1.33 KB |       0.005 |
| OrderedByCompletion     | 1024  | 2,046.1 ms | 4.17 ms |  1.00 |         - |         - |   241.5 KB |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,047.4 ms | 1.28 ms |  1.00 | 1000.0000 | 1000.0000 |  4536.6 KB |      18.785 |
Summary */
