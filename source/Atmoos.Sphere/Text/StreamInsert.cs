namespace Atmoos.Sphere.Text;

internal static class Insert
{
    public static IEnumerable<String> Text(IEnumerable<String> source, in LineMark tag, IEnumerable<String> lines)
    {
        return InsertTextImpl(tag.ToString(), tag.Tag, source, lines);

        static IEnumerable<String> InsertTextImpl(String start, String end, IEnumerable<String> source, IEnumerable<String> insert)
        {
            Boolean deleting = false;
            foreach (var line in source) {
                if (deleting && line.StartsWith(end)) {
                    foreach (var insertLine in insert) {
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
}
