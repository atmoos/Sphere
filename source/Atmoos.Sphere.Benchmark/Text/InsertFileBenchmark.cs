using Atmoos.Sphere.Text;
using BenchmarkDotNet.Attributes;

using static Atmoos.Sphere.Benchmark.Text.Convenience;

namespace Atmoos.Sphere.Benchmark.Text;

[MemoryDiagnoser]
[ShortRunJob, WarmupCount(5), IterationCount(11)]
public class InsertFileBenchmark
{
    private const String testFile = "TestFile.cs";
    private static readonly FileInfo file = new(testFile);
    private static readonly LineTag md = LineTags.Markdown.Code("csharp");
    private static readonly String[] section = Section("Some Section", 241).ToArray();

    [GlobalSetup]
    public void Setup()
    {
        File.WriteAllText(testFile, String.Join(Environment.NewLine, Sections(1154, md, section, 1965)));
    }

    [GlobalCleanup]
    public void Cleanup() => File.Delete(testFile);

    [Benchmark]
    public Int32 InsertSynchronously()
    {
        file.InsertSection(md, section);
        return 0;
    }

    [Benchmark(Baseline = true)]
    public async Task<Int32> InsertAsynchronously()
    {
        await file.InsertSectionAsync(md, section).ConfigureAwait(false);
        return 0;
    }
}

/* Summary

BenchmarkDotNet v0.15.3, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Max: 0.40GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.110
  [Host]   : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  ShortRun : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=11  LaunchCount=1  
WarmupCount=5  

| Method               | Mean       | Error   | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
|--------------------- |-----------:|--------:|------:|--------:|-------:|----------:|------------:|
| InsertSynchronously  |   573.2 μs | 0.83 μs |  0.32 | 30.2734 | 0.9766 | 126.28 KB |        0.34 |
| InsertAsynchronously | 1,772.0 μs | 5.50 μs |  1.00 | 89.8438 | 3.9063 | 367.35 KB |        1.00 |
Summary */
