using System.Reflection;
using Atmoos.Sphere.Text;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using static System.Environment;

namespace Atmoos.Sphere.BenchmarkDotNet;

public static class Exporter
{
    private static readonly ExportConfig defaultConfig = ExportConfig.Default;
    public static async Task Export(this Assembly assembly, IEnumerable<Summary> summaries)
        => await assembly.Export(summaries, defaultConfig).ConfigureAwait(false);
    public static async Task Export(this Assembly assembly, IEnumerable<Summary> summaries, ExportConfig config)
    {
        var export = Task.CompletedTask;
        var sourceFiles = ExtractSources(assembly);
        foreach (var summary in summaries) {
            await export.ConfigureAwait(false);
            export = Export(summary, sourceFiles, config);
        }
        await export.ConfigureAwait(false);
    }

    public static async Task Export(this Assembly assembly, Summary summary)
        => await assembly.Export(summary, defaultConfig).ConfigureAwait(false);
    public static async Task Export(this Assembly assembly, Summary summary, ExportConfig config)
        => await Export(summary, ExtractSources(assembly), config).ConfigureAwait(false);

    private static Task Export(Summary summary, List<FileInfo> allFiles, ExportConfig config)
        => Export(summary, MarkdownExporter.Console, allFiles, config);
    private static async Task Export(Summary summary, IExporter exporter, List<FileInfo> allFiles, ExportConfig config)
    {
        Task update = Task.CompletedTask;
        foreach (var file in exporter.ExportToFiles(summary, config.Logger).Select(f => new FileInfo(f))) {
            var name = BenchmarkName(file.Name);
            var fileName = $"{name}.cs";
            config.Logger.WriteInfo($"Exporting: {name}{NewLine}");
            // ToDo: This is not safe, but good enough for now.
            var sourceFile = allFiles.Single(f => f.Name.EndsWith(fileName));
            await update.ConfigureAwait(false);
            update = UpdateSourceFile(sourceFile, config.Tag, file);
        }
        await update.ConfigureAwait(false);

        static async Task UpdateSourceFile(FileInfo source, LineMark tag, FileInfo benchmarkReport)
        {
            try {
                using var report = benchmarkReport.OpenText();
                await source.InsertSectionAsync(tag, report.ReadLines()).ConfigureAwait(false);
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
        var end = reportPath.IndexOf('-');
        var start = reportPath.LastIndexOf('.', end, end) + 1;
        return reportPath[start..end];
    }
}
