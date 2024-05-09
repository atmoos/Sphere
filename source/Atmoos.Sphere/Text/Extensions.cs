using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Text;

public static class Extensions
{
    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="file"/> at the line marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="file">The file in which <paramref name="section"/> is to be replaced.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    public static void InsertSection(this FileInfo file, in LineMark tag, IEnumerable<String> section)
    {
        String copy = Impl(file, in tag, section);
        File.Move(copy, file.FullName, overwrite: true);

        static String Impl(FileInfo file, in LineMark tag, IEnumerable<String> lines)
        {
            var copyName = $"{file.FullName}.tmp";
            using var source = file.OpenText();
            using var temporary = File.CreateText(copyName);
            source.InsertSection(temporary, in tag, lines);
            return copyName;
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="file"/> at the line marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="file">The file in which <paramref name="section"/> is to be replaced.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task InsertSectionAsync(this FileInfo file, LineMark tag, IEnumerable<String> section, CancellationToken token = default)
    {
        String copy = await Impl(file, tag, section, token).ConfigureAwait(false);
        File.Move(copy, file.FullName, overwrite: true);

        static async Task<String> Impl(FileInfo file, LineMark tag, IEnumerable<String> lines, CancellationToken token)
        {
            var copyName = $"{file.FullName}.tmp";
            using var source = file.OpenText();
            using var temporary = File.CreateText(copyName);
            await source.InsertSectionAsync(temporary, in tag, lines, token).ConfigureAwait(false);
            return copyName;
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the destination stream at the line marked by the tag.
    /// </summary>
    /// <param name="source">The current lines into which <paramref name="section"/> is to be inserted.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    public static void InsertSection(this TextReader source, TextWriter destination, in LineMark tag, IEnumerable<String> section)
        => destination.WriteLines(Insert.Section(source.ReadLines(), in tag, section));

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the destination stream at the line marked by the tag.
    /// </summary>
    /// <param name="source">The current lines into which <paramref name="section"/> is to be inserted.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static Task InsertSectionAsync(this TextReader source, TextWriter destination, in LineMark tag, IEnumerable<String> section, CancellationToken token = default)
        => destination.WriteLinesAsync(Insert.Section(source.ReadLinesAsync(token), in tag, section, token), token);

    public static void WriteLines(this TextWriter writer, IEnumerable<String> lines)
    {
        foreach (var line in lines) {
            writer.WriteLine(line);
        }
    }

    public static async Task WriteLinesAsync(this TextWriter writer, IAsyncEnumerable<String> lines, CancellationToken token = default)
    {
        Task write = Task.CompletedTask;
        await foreach (var line in lines.WithCancellation(token).ConfigureAwait(false)) {
            Task next = writer.WriteLineAsync(line.AsMemory(), token);
            await write.ConfigureAwait(false);
            write = next;
        }
        await write.ConfigureAwait(false);
    }

    /// <summary>
    /// Reads all lines from the stream in a memory efficient way.
    /// </summary>
    /// <param name="source">The source of the lines.</param>
    public static IEnumerable<String> ReadLines(this TextReader source)
    {
        String? line;
        while ((line = source.ReadLine()) != null) {
            yield return line;
        }
    }

    /// <summary>
    /// Reads all lines from the stream in a memory efficient way.
    /// </summary>
    /// <param name="source">The source of the lines.</param>
    /// <param name="token">The cancellation token.</param>
    public static async IAsyncEnumerable<String> ReadLinesAsync(this TextReader source, [EnumeratorCancellation] CancellationToken token = default)
    {
        String? line;
        ValueTask<String?> read = source.ReadLineAsync(token);
        while ((line = await read.ConfigureAwait(false)) != null) {
            read = source.ReadLineAsync(token);
            yield return line;
        }
    }
}
