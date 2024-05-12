using Atmoos.Sphere.Text;
using BenchmarkDotNet.Loggers;

namespace Atmoos.Sphere.BenchmarkDotNet;

public sealed record class ExportConfig
{
    internal static LineTag defaultTag = LineTags.CSharp.BlockComment("Summary");
    public static ExportConfig Default => new();
    public LineTag Tag { get; init; } = defaultTag;
    public ILogger Logger { get; init; } = ConsoleLogger.Default;
}
