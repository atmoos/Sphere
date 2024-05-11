using System.Reflection;
using Atmoos.Sphere.Text;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace Atmoos.Sphere.BenchmarkDotNet;

public static class Exporter
{
    private static readonly ILogger logger = new Logger();
    private static readonly LineMark mark = new() { Tag = "/*", Name = " Summary *" };

    public static async Task Export(this Assembly assembly, IEnumerable<Summary> summaries)
    {
        var sourceFiles = ExtractSources(assembly);
        foreach (var summary in summaries) {
            await Export(summary, sourceFiles).ConfigureAwait(false);
        }
    }

    public static Task Export(this Assembly assembly, Summary summary) => Export(summary, ExtractSources(assembly));
    private static Task Export(Summary summary, List<FileInfo> allFiles)
        => Export(summary, MarkdownExporter.Console, allFiles);

    private static async Task Export(Summary summary, IExporter exporter, List<FileInfo> allFiles)
    {
        Task update = Task.CompletedTask;
        foreach (var file in exporter.ExportToFiles(summary, logger).Select(f => new FileInfo(f))) {
            var name = BenchmarkName(file.Name);
            var fileName = $"{name}.cs";
            logger.WriteLine($"Exporting: {name}");
            // ToDo: This is not safe, but good enough for now.
            var sourceFile = allFiles.Single(f => f.Name.EndsWith(fileName));
            await update.ConfigureAwait(false);
            update = UpdateSourceFile(sourceFile, file);
        }
        await update.ConfigureAwait(false);

        static async Task UpdateSourceFile(FileInfo source, FileInfo benchmarkReport)
        {
            try {
                using var report = benchmarkReport.OpenText();
                await source.InsertSectionAsync(mark, report.ReadLines()).ConfigureAwait(false);
            }
            finally {
                benchmarkReport.Delete();
            }
        }
    }

    private static List<FileInfo> ExtractSources(this Assembly assembly)
        => FindSourceFilesIn(FindSourceDir(assembly)).Select(f => new FileInfo(f)).ToList();

    private static DirectoryInfo FindSourceDir(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name ?? throw new ArgumentNullException(nameof(assembly));
        DirectoryInfo? dir = new(assembly.Location);
        while (dir.Name.EndsWith(assemblyName) == false) {
            var prev = dir.FullName;
            if ((dir = dir?.Parent) is null || prev == dir.FullName) {
                throw new Exception($"Failed finding source dir @ {prev}");
            }
        }
        return dir;
    }

    private static IEnumerable<String> FindSourceFilesIn(DirectoryInfo dir, String sourceType = "*.cs")
        => dir.EnumerateFiles(sourceType, SearchOption.AllDirectories).Select(f => f.FullName);

    private static String BenchmarkName(String reportPath)
    {
        // path is:  Namespace.ClassName-report-console.md
        var index = reportPath.IndexOf('-');
        var sourceName = reportPath[..index];
        index = sourceName.LastIndexOf('.') + 1;
        return sourceName[index..];
    }
}

file sealed class Logger : ILogger
{
    public String Id => "ConsoleLogger";
    public Int32 Priority => 1;
    public void WriteLine() => Console.WriteLine();
    public void Write(LogKind logKind, String text) => Console.Write(text);
    public void WriteLine(LogKind logKind, String text) => Console.WriteLine(text);
    public void Flush() { }
}
