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
  public void InsertSynchronously() => file.InsertSection(md, section);

  [Benchmark(Baseline = true)]
  public async Task InsertAsynchronously() => await file.InsertSectionAsync(md, section).ConfigureAwait(false);
}

/* Summary *

BenchmarkDotNet v0.13.12, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.104
  [Host]   : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=11  LaunchCount=1  
WarmupCount=5  

 Method               | Mean       | Error     | Ratio | Gen0   | Allocated | Alloc Ratio |
--------------------- |-----------:|----------:|------:|-------:|----------:|------------:|
 InsertSynchronously  |   239.9 μs |   4.31 μs |  0.19 | 1.4648 |  126.3 KB |        0.34 |
 InsertAsynchronously | 1,296.4 μs | 339.26 μs |  1.00 | 3.9063 | 367.42 KB |        1.00 |
/* End */
