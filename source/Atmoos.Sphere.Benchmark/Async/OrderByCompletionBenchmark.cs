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

/* Summary *

BenchmarkDotNet v0.13.12, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.104
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  Job-UFLBFX : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

| Method                  | Count | Mean       | Error   | Ratio | Allocated  | Alloc Ratio |
|------------------------ |------ |-----------:|--------:|------:|-----------:|------------:|
| Unordered               | 128   |   257.5 ms | 1.44 ms |  1.00 |    1.33 KB |        0.04 |
| OrderedByCompletion     | 128   |   257.5 ms | 1.40 ms |  1.00 |   32.29 KB |        1.00 |
| NaiveCompletionOrdering | 128   |   257.3 ms | 1.26 ms |  1.00 |  115.79 KB |        3.59 |
|                         |       |            |         |       |            |             |
| Unordered               | 256   |   513.9 ms | 0.68 ms |  1.00 |    1.33 KB |        0.02 |
| OrderedByCompletion     | 256   |   513.5 ms | 1.27 ms |  1.00 |   61.76 KB |        1.00 |
| NaiveCompletionOrdering | 256   |   513.4 ms | 1.26 ms |  1.00 |  344.52 KB |        5.58 |
|                         |       |            |         |       |            |             |
| Unordered               | 512   | 1,022.2 ms | 1.76 ms |  1.00 |    1.33 KB |        0.01 |
| OrderedByCompletion     | 512   | 1,023.0 ms | 1.04 ms |  1.00 |  121.63 KB |        1.00 |
| NaiveCompletionOrdering | 512   | 1,023.6 ms | 1.07 ms |  1.00 | 1231.64 KB |       10.13 |
|                         |       |            |         |       |            |             |
| Unordered               | 1024  | 2,046.7 ms | 1.81 ms |  1.00 |    1.33 KB |       0.005 |
| OrderedByCompletion     | 1024  | 2,045.5 ms | 3.22 ms |  1.00 |  241.63 KB |       1.000 |
| NaiveCompletionOrdering | 1024  | 2,047.0 ms | 1.88 ms |  1.00 | 4508.52 KB |      18.659 |
/* End */
