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

BenchmarkDotNet v0.15.3, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Max: 0.40GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.110
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  Job-CNUJVU : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Gen0      | Allocated | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|----------:|----------:|------------:|
| Unordered               | 128   |   256.2 ms | 0.43 ms |  1.00 |         - |     336 B |        0.01 |
| OrderedByCompletion     | 128   |   256.4 ms | 0.47 ms |  1.00 |         - |   30848 B |        1.00 |
| NaiveCompletionOrdering | 128   |   256.0 ms | 0.24 ms |  1.00 |         - |  122568 B |        3.97 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 256   |   512.1 ms | 0.67 ms |  1.00 |         - |     336 B |       0.005 |
| OrderedByCompletion     | 256   |   512.3 ms | 0.55 ms |  1.00 |         - |   61568 B |       1.000 |
| NaiveCompletionOrdering | 256   |   512.1 ms | 0.57 ms |  1.00 |         - |  380880 B |       6.186 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 512   | 1,023.0 ms | 1.17 ms |  1.00 |         - |     336 B |       0.003 |
| OrderedByCompletion     | 512   | 1,023.4 ms | 0.99 ms |  1.00 |         - |  123520 B |       1.000 |
| NaiveCompletionOrdering | 512   | 1,023.1 ms | 0.95 ms |  1.00 |         - | 1289800 B |      10.442 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 1024  | 2,046.1 ms | 6.55 ms |  1.00 |         - |     336 B |       0.001 |
| OrderedByCompletion     | 1024  | 2,046.1 ms | 6.37 ms |  1.00 |         - |  246272 B |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,046.5 ms | 1.69 ms |  1.00 | 1000.0000 | 4743272 B |      19.260 |
Summary */
