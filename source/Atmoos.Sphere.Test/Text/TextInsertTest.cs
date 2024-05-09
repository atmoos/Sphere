using Atmoos.Sphere.Text;

using static Atmoos.Sphere.Text.LineMarks;

namespace Atmoos.Sphere.Test.Text;

public class TextInsertTest
{
    private static readonly LineMark mark = Markdown.Code("text");
    private const String toReplace = """
                                       Hi there!
                                       This should be replaced.
                                      """;
    private const String toInsert = """
                                     This is new text!
                                     And more lines
                                     than before :-)
                                    """;

    [Fact]
    public void InsertSectionWithTextIoReplacesTextAtSpecifiedLineMark()
    {
        var destination = new StringWriter();
        var source = new StringReader(Text(mark, toReplace));

        source.InsertSection(destination, mark, toInsert.Split(Environment.NewLine));

        var expected = Text(mark, toInsert);
        var actual = destination.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InsertSectionInFileReplacesTextAtSpecifiedLineMark()
    {
        var source = new FileInfo("Test.md");
        try {

            File.WriteAllText(source.FullName, Text(mark, toReplace));

            source.InsertSection(mark, toInsert.Split(Environment.NewLine));

            var expected = Text(mark, toInsert);
            var actual = File.ReadAllText(source.FullName);
            Assert.Equal(expected, actual);
        }
        finally {
            source.Delete();
        }
    }

    [Fact]
    public async Task InsertSectionInFileAsyncReplacesTextAtSpecifiedLineMark()
    {
        var source = new FileInfo("AsyncTest.md");
        try {

            await File.WriteAllTextAsync(source.FullName, Text(mark, toReplace));

            await source.InsertSectionAsync(mark, toInsert.Split(Environment.NewLine));

            var expected = Text(mark, toInsert);
            var actual = await File.ReadAllTextAsync(source.FullName);
            Assert.Equal(expected, actual);
        }
        finally {
            source.Delete();
        }
    }

    [Fact]
    public async Task InsertSectionAsyncCanBeCancelled()
    {
        const Int32 cancelAtLine = 12;
        const String lineToInsert = "This will be inserted.";
        var source = new StringReader(Text(mark, "This will be replaced."));
        using var cts = new CancellationTokenSource();
        var destination = new StringWriter();
        var lotsOfLines = TriggerCancellation(InfiniteLinesOf(lineToInsert), cancelAtLine, cts);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await source.InsertSectionAsync(destination, mark, lotsOfLines, cts.Token));

        var actual = destination.ToString();

        Assert.EndsWith($"{cancelAtLine}: {lineToInsert}{Environment.NewLine}", actual);

        static IEnumerable<String> TriggerCancellation(IEnumerable<String> lines, Int32 cancelAt, CancellationTokenSource cts)
        {
            foreach (var (line, count) in lines.Select((l, c) => (l, c))) {
                yield return line;
                if (count == cancelAt) {
                    cts.Cancel();
                }
            }
        }
    }

    static String Text(in LineMark tag, String insert)
    {
        return $"""
                # Sample
                Hello World!
                ## Sub Sample
                This be intro.
                {tag}
                {insert}
                {tag.Tag}
                and this be outro.

                """;
    }

    static IEnumerable<String> InfiniteLinesOf(String line)
    {
        Int64 count = 0;
        while (true) {
            unchecked {
                yield return $"{count++}: {line}";
            }
        }
    }
}
