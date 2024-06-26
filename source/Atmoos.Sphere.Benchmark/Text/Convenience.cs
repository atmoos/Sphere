using Atmoos.Sphere.Text;

namespace Atmoos.Sphere.Benchmark.Text;

internal static class Convenience
{
    public static IEnumerable<String> Section(String line, Int32 count)
    {
        for (Int32 lineNumber = 0; lineNumber < count; lineNumber++) {
            yield return line;
        }
    }

    public static IEnumerable<String> Sections(Int32 pre, LineTag tag, IEnumerable<String> section, Int32 post)
    {
        foreach (var line in Section("Pre", pre)) {
            yield return line;
        }
        yield return tag.Start;
        foreach (var line in section) {
            yield return line;
        }
        yield return tag.End;
        foreach (var line in Section("Post", post)) {
            yield return line;
        }
    }
}
