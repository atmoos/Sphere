using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Text;

internal static class Insert
{
    public static IEnumerable<String> Section(IEnumerable<String> source, LineTag tag, IEnumerable<String> section)
    {
        Boolean deleting = false;
        foreach (var line in source) {
            if (deleting && line.StartsWith(tag.End)) {
                foreach (var insertLine in section) {
                    yield return insertLine;
                }
                deleting = false;
            }
            if (deleting) {
                continue;
            }
            if (!deleting && line == tag.Start) {
                deleting = true;
            }
            yield return line;
        }
    }

    public static async IAsyncEnumerable<String> Section(IAsyncEnumerable<String> source, LineTag tag, IEnumerable<String> section, [EnumeratorCancellation] CancellationToken token)
    {
        Boolean deleting = false;
        await foreach (var line in source.WithCancellation(token).ConfigureAwait(false)) {
            if (deleting && line.StartsWith(tag.End)) {
                foreach (var insert in section) {
                    yield return insert;
                }
                deleting = false;
            }
            if (deleting) {
                continue;
            }
            if (!deleting && line == tag.Start) {
                deleting = true;
            }
            yield return line;
        }
    }
}
