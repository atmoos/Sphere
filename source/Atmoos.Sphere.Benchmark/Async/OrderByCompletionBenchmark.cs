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

BenchmarkDotNet v0.15.2, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.109
  [Host]     : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Gen0      | Allocated | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|----------:|----------:|------------:|
| Unordered               | 128   |   256.4 ms | 0.59 ms |  1.00 |         - |     360 B |        0.01 |
| OrderedByCompletion     | 128   |   255.8 ms | 0.76 ms |  1.00 |         - |   31512 B |        1.00 |
| NaiveCompletionOrdering | 128   |   255.8 ms | 0.37 ms |  1.00 |         - |  122568 B |        3.89 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 256   |   512.3 ms | 0.89 ms |  1.00 |         - |     336 B |       0.005 |
| OrderedByCompletion     | 256   |   512.1 ms | 0.63 ms |  1.00 |         - |   61696 B |       1.000 |
| NaiveCompletionOrdering | 256   |   512.1 ms | 0.76 ms |  1.00 |         - |  362368 B |       5.873 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 512   | 1,023.3 ms | 1.45 ms |  1.00 |         - |     336 B |       0.003 |
| OrderedByCompletion     | 512   | 1,023.7 ms | 0.73 ms |  1.00 |         - |  123136 B |       1.000 |
| NaiveCompletionOrdering | 512   | 1,023.8 ms | 1.03 ms |  1.00 |         - | 1247088 B |      10.128 |
|                         |       |            |         |       |           |           |             |
| Unordered               | 1024  | 2,048.0 ms | 1.92 ms |  1.00 |         - |     336 B |       0.001 |
| OrderedByCompletion     | 1024  | 2,047.0 ms | 3.71 ms |  1.00 |         - |  246400 B |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,046.9 ms | 2.02 ms |  1.00 | 1000.0000 | 4679128 B |      18.990 |
Summary */
