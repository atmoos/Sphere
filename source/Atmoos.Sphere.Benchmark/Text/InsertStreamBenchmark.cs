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

BenchmarkDotNet v0.15.2, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.109
  [Host]   : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

| Method               | MsDelay | Mean       | Error   | Ratio | Allocated | Alloc Ratio |
|--------------------- |-------- |-----------:|--------:|------:|----------:|------------:|
| InsertSynchronously  | 1       |   474.7 ms | 7.01 ms |  1.45 |   6.66 KB |        0.13 |
| InsertAsynchronously | 1       |   327.5 ms | 6.07 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |         |       |           |             |
| InsertSynchronously  | 2       |   945.4 ms | 5.30 ms |  1.46 |   6.66 KB |        0.13 |
| InsertAsynchronously | 2       |   647.5 ms | 9.14 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |         |       |           |             |
| InsertSynchronously  | 4       | 1,870.2 ms | 2.88 ms |  1.46 |   6.66 KB |        0.13 |
| InsertAsynchronously | 4       | 1,278.7 ms | 6.61 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |         |       |           |             |
| InsertSynchronously  | 8       | 3,718.6 ms | 4.08 ms |  1.46 |   6.66 KB |        0.13 |
| InsertAsynchronously | 8       | 2,539.1 ms | 4.32 ms |  1.00 |  50.75 KB |        1.00 |
Summary */
