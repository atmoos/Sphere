using Atmoos.Sphere.Async;
using BenchmarkDotNet.Attributes;

namespace Atmoos.Sphere.Benchmark;

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

BenchmarkDotNet v0.13.12, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  Job-QJKLMT : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Allocated  | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|-----------:|------------:|
| Unordered               | 128   |   257.4 ms | 1.55 ms |  1.00 |    1.33 KB |        0.04 |
| OrderedByCompletion     | 128   |   257.3 ms | 1.42 ms |  1.00 |   31.63 KB |        1.00 |
| NaiveCompletionOrdering | 128   |   257.3 ms | 1.25 ms |  1.00 |  115.42 KB |        3.65 |
|                         |       |            |         |       |            |             |
| Unordered               | 256   |   513.9 ms | 1.25 ms |  1.00 |    1.33 KB |        0.02 |
| OrderedByCompletion     | 256   |   513.7 ms | 1.16 ms |  1.00 |   61.38 KB |        1.00 |
| NaiveCompletionOrdering | 256   |   513.8 ms | 1.03 ms |  1.00 |  361.45 KB |        5.89 |
|                         |       |            |         |       |            |             |
| Unordered               | 512   | 1,023.3 ms | 0.81 ms |  1.00 |    1.33 KB |        0.01 |
| OrderedByCompletion     | 512   | 1,023.4 ms | 0.62 ms |  1.00 |  121.13 KB |        1.00 |
| NaiveCompletionOrdering | 512   | 1,023.7 ms | 1.42 ms |  1.00 | 1233.38 KB |       10.18 |
|                         |       |            |         |       |            |             |
| Unordered               | 1024  | 2,045.2 ms | 0.83 ms |  1.00 |    1.33 KB |       0.005 |
| OrderedByCompletion     | 1024  | 2,045.5 ms | 2.98 ms |  1.00 |  241.76 KB |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,047.0 ms | 1.83 ms |  1.00 |  4512.4 KB |      18.665 |
Summary */
