using Atmoos.Sphere.Text;
using BenchmarkDotNet.Loggers;

namespace Atmoos.Sphere.BenchmarkDotNet;

public sealed record class ExportConfig
{
    internal static LineMark defaultTag = new() { Tag = "/*", Name = " Summary *" };
    public static ExportConfig Default => new();
    public LineMark Tag { get; init; } = defaultTag;
    public ILogger Logger { get; init; } = ConsoleLogger.Default;
}
