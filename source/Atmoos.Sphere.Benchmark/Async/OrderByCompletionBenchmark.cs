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
Intel Core i7-8565U CPU 1.80GHz (Max: 3.99GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.110
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  Job-CNUJVU : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Gen0      | Allocated | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|----------:|----------:|------------:|
| Unordered               | 128   |   256.3 ms | 0.30 ms |  1.00 |         - |     336 B |        0.01 |
| OrderedByCompletion     | 128   |   256.4 ms | 0.52 ms |  1.00 |         - |   31104 B |        1.00 |
| NaiveCompletionOrdering | 128   |   256.6 ms | 0.39 ms |  1.00 |         - |  119392 B |        3.84 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 256   |   511.9 ms | 0.54 ms |  1.00 |         - |     336 B |       0.005 |
| OrderedByCompletion     | 256   |   512.4 ms | 0.37 ms |  1.00 |         - |   61568 B |       1.000 |
| NaiveCompletionOrdering | 256   |   512.4 ms | 0.50 ms |  1.00 |         - |  377616 B |       6.133 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 512   | 1,023.3 ms | 0.59 ms |  1.00 |         - |    1384 B |        0.01 |
| OrderedByCompletion     | 512   | 1,023.8 ms | 0.58 ms |  1.00 |         - |  123008 B |        1.00 |
| NaiveCompletionOrdering | 512   | 1,023.8 ms | 0.62 ms |  1.00 |         - | 1257208 B |       10.22 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 1024  | 2,045.6 ms | 6.07 ms |  1.00 |         - |     336 B |       0.001 |
| OrderedByCompletion     | 1024  | 2,047.2 ms | 0.97 ms |  1.00 |         - |  246144 B |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,045.5 ms | 5.49 ms |  1.00 | 1000.0000 | 4614024 B |      18.745 |
Summary */
