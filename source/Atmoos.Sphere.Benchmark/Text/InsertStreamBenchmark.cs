using System.Text;
using Atmoos.Sphere.Text;
using BenchmarkDotNet.Attributes;

using static Atmoos.Sphere.Benchmark.Text.Convenience;
using static Atmoos.Sphere.Benchmark.Text.InsertStreamBenchmark;

namespace Atmoos.Sphere.Benchmark.Text;

[MemoryDiagnoser]
[ShortRunJob]
public class InsertStreamBenchmark
{
    private static readonly LineTag md = LineTags.Markdown.Code("csharp");
    private static readonly String longText = String.Join(Environment.NewLine, Sections(43, md, Section("Old Section", 9), 97));
    private static readonly String[] section = Section("New Section", 13).ToArray();

    private TextReader reader;
    private TextWriter writer;

    [Params(1, 2, 4, 8)]
    public Int32 MsDelay { get; set; }

    [IterationSetup]
    public void Setup()
    {
        var delay = TimeSpan.FromMilliseconds(MsDelay);
        this.reader = new SlowReader(longText, delay);
        this.writer = new SlowWriter(new StringBuilder(longText.Length), delay);
    }

    [Benchmark]
    public void InsertSynchronously()
    {
        this.reader.InsertSection(this.writer, md, section);
    }

    [Benchmark(Baseline = true)]
    public async Task InsertAsynchronously()
    {
        await this.reader.InsertSectionAsync(this.writer, md, section).ConfigureAwait(false);
    }

    internal static void SimulateIo(TimeSpan delay) => Thread.Sleep(delay);
}

file sealed class SlowReader(String s, TimeSpan delay) : StringReader(s)
{
    public override String ReadLine()
    {
        SimulateIo(delay);
        return base.ReadLine();
    }

    public override async ValueTask<String> ReadLineAsync(CancellationToken token)
    {
        await Task.Yield();
        SimulateIo(delay);
        return await base.ReadLineAsync(token).ConfigureAwait(false);
    }
}

file sealed class SlowWriter(StringBuilder sb, TimeSpan delay) : StringWriter(sb)
{
    public override void Write(String value)
    {
        SimulateIo(delay);
        base.Write(value);
    }

    public override async Task WriteLineAsync(ReadOnlyMemory<Char> value, CancellationToken token = default)
    {
        await Task.Yield();
        SimulateIo(delay);
        await base.WriteLineAsync(value, token).ConfigureAwait(false);
    }
}

/* Summary

BenchmarkDotNet v0.13.12, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.104
  [Host]   : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

| Method               | MsDelay | Mean       | Error     | Ratio | Allocated | Alloc Ratio |
|--------------------- |-------- |-----------:|----------:|------:|----------:|------------:|
| InsertSynchronously  | 1       |   534.3 ms | 139.41 ms |  1.45 |   7.42 KB |        0.14 |
| InsertAsynchronously | 1       |   367.6 ms |  58.39 ms |  1.00 |  51.64 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 2       | 1,008.6 ms |  79.98 ms |  1.45 |   7.42 KB |        0.14 |
| InsertAsynchronously | 2       |   696.4 ms |  31.52 ms |  1.00 |  51.64 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 4       | 1,933.7 ms |  17.54 ms |  1.45 |   7.42 KB |        0.14 |
| InsertAsynchronously | 4       | 1,329.4 ms |  45.18 ms |  1.00 |  51.64 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 8       | 3,795.9 ms |  89.24 ms |  1.46 |   7.42 KB |        0.14 |
| InsertAsynchronously | 8       | 2,602.9 ms |  63.58 ms |  1.00 |  51.64 KB |        1.00 |
Summary */
