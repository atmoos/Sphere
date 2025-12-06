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
Intel Core i7-8565U CPU 1.80GHz (Max: 0.40GHz) (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]   : .NET 10.0.0 (10.0.0, 42.42.42.42424), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.0 (10.0.0, 42.42.42.42424), X64 RyuJIT x86-64-v3

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

| Method               | MsDelay | Mean       | Error     | Ratio | Allocated | Alloc Ratio |
|--------------------- |-------- |-----------:|----------:|------:|----------:|------------:|
| InsertSynchronously  | 1       |   495.9 ms |  11.57 ms |  1.42 |   6.66 KB |        0.13 |
| InsertAsynchronously | 1       |   348.2 ms |  21.83 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 2       |   976.8 ms |  20.06 ms |  1.44 |   6.66 KB |        0.13 |
| InsertAsynchronously | 2       |   676.7 ms |  29.65 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 4       | 1,898.8 ms |  10.56 ms |  1.45 |   6.66 KB |        0.13 |
| InsertAsynchronously | 4       | 1,306.2 ms |  18.65 ms |  1.00 |  50.75 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 8       | 4,071.8 ms | 257.00 ms |  1.46 |   6.66 KB |        0.13 |
| InsertAsynchronously | 8       | 2,793.0 ms | 203.01 ms |  1.00 |  50.75 KB |        1.00 |
Summary */
