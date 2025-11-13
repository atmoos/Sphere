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

BenchmarkDotNet v0.15.7, Linux Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Max: 3.99GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.110
  [Host]   : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  ShortRun : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

| Method               | MsDelay | Mean       | Error    | Ratio | Allocated | Alloc Ratio |
|--------------------- |-------- |-----------:|---------:|------:|----------:|------------:|
| InsertSynchronously  | 1       |   495.5 ms | 16.91 ms |  1.44 |   6.66 KB |        0.13 |
| InsertAsynchronously | 1       |   344.3 ms | 27.26 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |          |       |           |             |
| InsertSynchronously  | 2       |   958.5 ms | 31.75 ms |  1.45 |   6.66 KB |        0.13 |
| InsertAsynchronously | 2       |   660.3 ms |  9.76 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |          |       |           |             |
| InsertSynchronously  | 4       | 1,884.0 ms | 16.50 ms |  1.46 |   6.66 KB |        0.13 |
| InsertAsynchronously | 4       | 1,291.3 ms |  7.29 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |          |       |           |             |
| InsertSynchronously  | 8       | 3,741.4 ms | 36.57 ms |  1.46 |   6.66 KB |        0.13 |
| InsertAsynchronously | 8       | 2,556.6 ms | 23.77 ms |  1.00 |  50.75 KB |        1.00 |
Summary */
