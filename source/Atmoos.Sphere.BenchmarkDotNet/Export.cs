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
        var sourceFiles = FindSourceFiles(assembly);
        foreach (var summary in summaries) {
            await Export(summary, sourceFiles).ConfigureAwait(false);
        }
    }

    public static Task Export(this Assembly assembly, Summary summary) => Export(summary, FindSourceFiles(assembly));
    private static Task Export(Summary summary, List<FileInfo> allFiles)
        => Export(summary, MarkdownExporter.Console, allFiles);

    private static async Task Export(Summary summary, IExporter exporter, List<FileInfo> allFiles)
    {
        Task update = Task.CompletedTask;
        foreach (var file in exporter.ExportToFiles(summary, logger).Select(f => new FileInfo(f))) {
            var name = BenchmarkName(file.Name);
            var fileName = $"{name}.cs";
            logger.WriteLine($"Exporting: {name}");
            var sourceFile = allFiles.Single(f => f.Name.EndsWith(fileName));
            await update.ConfigureAwait(false);
            update = UpdateSourceFile(sourceFile, file);
        }
        await update.ConfigureAwait(false);

        static async Task UpdateSourceFile(FileInfo sourceFile, FileInfo reportFile)
        {
            using var section = reportFile.OpenText();
            await sourceFile.InsertSectionAsync(mark, section.ReadLines()).ConfigureAwait(false);
            reportFile.Delete();
        }
    }

    private static List<FileInfo> FindSourceFiles(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name ?? throw new ArgumentNullException(nameof(assembly));
        DirectoryInfo? dir = new(assembly.Location);
        while (dir.Name.EndsWith(assemblyName) == false) {
            var prev = dir.FullName;
            if ((dir = dir?.Parent) is null || prev == dir.FullName) {
                throw new Exception($"Failed finding source dir @ {prev}");
            }
        }
        return dir.EnumerateFiles("*.cs", SearchOption.AllDirectories).ToList();
    }

    private static String BenchmarkName(ReadOnlySpan<Char> reportPath)
    {
        var index = reportPath.IndexOf('-');
        var sourceName = reportPath[..index];
        index = sourceName.LastIndexOf('.') + 1;
        return new String(sourceName[index..]);
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
