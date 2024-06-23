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
        Boolean previousIsLower = true;
        for (Int32 i = 0; i < value.Length; i++) {
            Boolean isUpper = Char.IsUpper(value[i]);
            if (isUpper && previous != i && previousIsLower) {
                yield return value[previous..i];
                previous = i;
            }
            previousIsLower = !isUpper;
        }
        if (previous < value.Length) {
            yield return value[previous..];
        }
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
