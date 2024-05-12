using System.Text;
using Atmoos.Sphere.Text;
using BenchmarkDotNet.Attributes;

using static Atmoos.Sphere.Benchmark.Text.Convenience;

namespace Atmoos.Sphere.Benchmark.Text;

[MemoryDiagnoser]
[ShortRunJob]
public class InsertStreamBenchmark
{
    private static readonly LineMark md = LineMarks.Markdown.Code("csharp");
    private static readonly String longText = String.Join(Environment.NewLine, Sections(43, md, Section("Old Section", 9), 97));
    private static readonly String[] section = Section("New Section", 13).ToArray();

    private TextReader reader;
    private TextWriter writer;

    [Params(2, 4, 8)]
    public Int32 MsDelay { get; set; }

    [IterationSetup]
    public void Setup()
    {
        var delay = TimeSpan.FromMilliseconds(MsDelay);
        this.reader = new SlowReader(longText, delay);
        this.writer = new SlowWriter(new StringBuilder(longText.Length), delay);
    }

    [Benchmark]
    public Int32 InsertSynchronously()
    {
        this.reader.InsertSection(this.writer, md, section);
        return 0;
    }

    [Benchmark(Baseline = true)]
    public async Task<Int32> InsertAsynchronously()
    {
        await this.reader.InsertSectionAsync(this.writer, md, section).ConfigureAwait(false);
        return 0;
    }


}

file sealed class SlowReader(String s, TimeSpan delay) : StringReader(s)
{
    public override String ReadLine()
    {
        Thread.Sleep(delay);
        return base.ReadLine();
    }

    public override async ValueTask<String> ReadLineAsync(CancellationToken token)
    {
        await Task.Yield();
        Thread.Sleep(delay);
        return await base.ReadLineAsync(token).ConfigureAwait(false);
    }
}

file sealed class SlowWriter(StringBuilder sb, TimeSpan delay) : StringWriter(sb)
{
    public override void Write(String value)
    {
        Thread.Sleep(delay);
        base.Write(value);
    }

    public override async Task WriteLineAsync(ReadOnlyMemory<Char> value, CancellationToken token = default)
    {
        await Task.Yield();
        Thread.Sleep(delay);
        await base.WriteLineAsync(value, token).ConfigureAwait(false);
    }
}

/* Summary *

BenchmarkDotNet v0.13.12, Arch Linux
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.104
  [Host]   : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

| Method               | MsDelay | Mean       | Error     | Ratio | Allocated | Alloc Ratio |
|--------------------- |-------- |-----------:|----------:|------:|----------:|------------:|
| InsertSynchronously  | 2       | 1,038.8 ms | 210.85 ms |  1.45 |   7.42 KB |        0.14 |
| InsertAsynchronously | 2       |   715.3 ms | 113.33 ms |  1.00 |  51.64 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 4       | 1,970.1 ms |  32.81 ms |  1.46 |   7.42 KB |        0.14 |
| InsertAsynchronously | 4       | 1,349.0 ms |  97.26 ms |  1.00 |  51.64 KB |        1.00 |
|                      |         |            |           |       |           |             |
| InsertSynchronously  | 8       | 4,055.1 ms | 129.72 ms |  1.46 |   7.42 KB |        0.14 |
| InsertAsynchronously | 8       | 2,772.1 ms | 349.39 ms |  1.00 |  51.64 KB |        1.00 |
/* End */
