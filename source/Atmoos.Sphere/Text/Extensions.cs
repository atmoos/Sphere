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
    public static void InsertText(this TextWriter destination, TextReader source, in LineMark tag, IEnumerable<String> insert)
     => destination.WriteLines(Insert.Text(source.ReadLines(), in tag, insert));

    public static void WriteLines(this TextWriter writer, IEnumerable<String> lines)
    {
        foreach (var line in lines) {
            writer.WriteLine(line);
        }
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
}
