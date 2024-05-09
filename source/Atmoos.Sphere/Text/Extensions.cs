using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Text;

public static class Extensions
{
    /// <summary>
    /// Inserts the lines into the destination stream at the line marked by the tag.
    /// </summary>
    /// <param name="destination">The destination stream.</param>
    /// <param name="source">The current lines into which <paramref name="insert"/> is to be inserted.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="insert">The text that is to be inserted.</param>
    public static void InsertText(this FileInfo file, in LineMark tag, IEnumerable<String> insert)
    {
        String copy = Impl(file, in tag, insert);
        File.Move(copy, file.FullName, overwrite: true);

        static String Impl(FileInfo file, in LineMark tag, IEnumerable<String> lines)
        {
            var copyName = $"{file.FullName}.tmp";
            using var source = file.OpenText();
            using var temporary = File.CreateText(copyName);
            temporary.InsertText(source, in tag, lines);
            return copyName;
        }
    }
    /// <summary>
    /// Inserts the lines into the destination stream at the line marked by the tag.
    /// </summary>
    /// <param name="destination">The destination stream.</param>
    /// <param name="source">The current lines into which <paramref name="insert"/> is to be inserted.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="insert">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task InsertTextAsync(this FileInfo file, LineMark tag, IEnumerable<String> insert, CancellationToken token = default)
    {
        String copy = await Impl(file, tag, insert, token).ConfigureAwait(false);
        File.Move(copy, file.FullName, overwrite: true);

        static async Task<String> Impl(FileInfo file, LineMark tag, IEnumerable<String> lines, CancellationToken token)
        {
            var copyName = $"{file.FullName}.tmp";
            using var source = file.OpenText();
            using var temporary = File.CreateText(copyName);
            await temporary.InsertTextAsync(source, in tag, lines, token).ConfigureAwait(false);
            return copyName;
        }
    }

    /// <summary>
    /// Inserts the lines into the destination stream at the line marked by the tag.
    /// </summary>
    /// <param name="destination">The destination stream.</param>
    /// <param name="source">The current lines into which <paramref name="insert"/> is to be inserted.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="insert">The text that is to be inserted.</param>
    public static void InsertText(this TextWriter destination, TextReader source, in LineMark tag, IEnumerable<String> insert)
     => destination.WriteLines(Insert.Text(source.ReadLines(), in tag, insert));

    /// <summary>
    /// Inserts the lines into the destination stream at the line marked by the tag.
    /// </summary>
    /// <param name="destination">The destination stream.</param>
    /// <param name="source">The current lines into which <paramref name="insert"/> is to be inserted.</param>
    /// <param name="tag">The line mark indicating where to insert the text.</param>
    /// <param name="insert">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static Task InsertTextAsync(this TextWriter destination, TextReader source, in LineMark tag, IEnumerable<String> insert, CancellationToken token = default)
    {
        return destination.WriteLinesAsync(Insert.Text(source.ReadLinesAsync(token), in tag, insert, token), token);
    }

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
