using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Text;

internal static class Insert
{
    public static IEnumerable<String> Section(IEnumerable<String> source, in LineTag tag, IEnumerable<String> section)
    {
        return InsertSectionImpl(tag.Start, tag.End, source, section);

        static IEnumerable<String> InsertSectionImpl(String start, String end, IEnumerable<String> source, IEnumerable<String> section)
        {
            Boolean deleting = false;
            foreach (var line in source) {
                if (deleting && line == end) {
                    foreach (var insertLine in section) {
                        yield return insertLine;
                    }
                    deleting = false;
                }
                if (deleting) {
                    continue;
                }
                if (!deleting && line == start) {
                    deleting = true;
                }
                yield return line;
            }
        }
    }

    public static IAsyncEnumerable<String> Section(IAsyncEnumerable<String> source, in LineTag tag, IEnumerable<String> section, CancellationToken token)
    {
        return InsertTextImpl(tag.Start, tag.End, source, section, token);

        static async IAsyncEnumerable<String> InsertTextImpl(String start, String end, IAsyncEnumerable<String> source, IEnumerable<String> section, [EnumeratorCancellation] CancellationToken token)
        {
            Boolean deleting = false;
            await foreach (var line in source.WithCancellation(token).ConfigureAwait(false)) {
                if (deleting && line == end) {
                    foreach (var insert in section) {
                        yield return insert;
                    }
                    deleting = false;
                }
                if (deleting) {
                    continue;
                }
                if (!deleting && line == start) {
                    deleting = true;
                }
                yield return line;
            }
        }
    }
}
