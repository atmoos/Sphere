using System.Runtime.CompilerServices;

using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Text;

public static class Extensions
{
    public static (Int32 value, String text) ToString(this Int32 count, String singular, String plural) => (count, count switch {
        0 => String.Empty,
        1 => $"one {singular}",
        _ => $"{count} {plural}"
    });

    public static String Combine(this (Int32 count, String text) left, (Int32 count, String text) right) => (left.count, right.count) switch {
        (0, 0) => String.Empty,
        (_, 0) => left.text,
        (0, _) => right.text,
        _ => $"{left.text} and {right.text}"
    };

    public static IEnumerable<String> SplitByCase(this String value)
    {
        Int32 previous = 0;
        for (Int32 i = 0; i < value.Length; i++) {
            if (Char.IsUpper(value[i])) {
                if (previous != i) {
                    yield return value[previous..i];
                }
                previous = i;
            }
        }
        if (previous < value.Length) {
            yield return value[previous..];
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="file"/> in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="file">The file in which <paramref name="section"/> is to be replaced.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    public static void InsertSection(this FileInfo file, in LineTag tag, IEnumerable<String> section)
    {
        String temporaryCopy = CreateTemporaryCopy(file, in tag, section);
        File.Move(temporaryCopy, file.FullName, overwrite: true);

        static String CreateTemporaryCopy(FileInfo file, in LineTag tag, IEnumerable<String> lines)
        {
            var temporaryFile = $"{file.FullName}.tmp";
            try {
                using var source = file.OpenText();
                using var temporary = File.CreateText(temporaryFile);
                source.InsertSection(temporary, in tag, lines);
            }
            catch {
                File.Delete(temporaryFile);
                throw;
            }
            return temporaryFile;
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="file"/> in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="file">The file in which <paramref name="section"/> is to be replaced.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task InsertSectionAsync(this FileInfo file, LineTag tag, IEnumerable<String> section, CancellationToken token = default)
    {
        String temporaryCopy = await CreateTemporaryCopy(file, tag, section, token).ConfigureAwait(None);
        File.Move(temporaryCopy, file.FullName, overwrite: true);

        static async Task<String> CreateTemporaryCopy(FileInfo file, LineTag tag, IEnumerable<String> lines, CancellationToken token)
        {
            var temporaryFile = $"{file.FullName}.tmp";
            try {
                using var source = file.OpenText();
                using var temporary = File.CreateText(temporaryFile);
                await source.InsertSectionAsync(temporary, tag, lines, token).ConfigureAwait(None);
            }
            catch {
                File.Delete(temporaryFile);
                throw;
            }
            return temporaryFile;
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="destination"/> stream in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="source">The current lines into which <paramref name="section"/> is to be inserted.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    public static void InsertSection(this TextReader source, TextWriter destination, in LineTag tag, IEnumerable<String> section)
        => destination.WriteLines(Insert.Section(source.ReadLines(), tag, section));

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="destination"/> stream in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="source">The current lines into which <paramref name="section"/> is to be inserted.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task InsertSectionAsync(this TextReader source, TextWriter destination, LineTag tag, IEnumerable<String> section, CancellationToken token = default)
        => await destination.WriteLinesAsync(Insert.Section(source.ReadLinesAsync(token), tag, section, token), token).ConfigureAwait(false);

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
            await write.ConfigureAwait(None);
            write = writer.WriteLineAsync(line.AsMemory(), token);
        }
        await write.ConfigureAwait(None);
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
