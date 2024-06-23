using Atmoos.Sphere.Text;

using static Atmoos.Sphere.Text.LineTags;

namespace Atmoos.Sphere.Test.Text;

public sealed class InsertExtensionsTest
{
    private static readonly LineTag tag = Markdown.Code("text");
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
        var source = new StringReader(TextScenario(tag, toReplace));

        source.InsertSection(destination, tag, toInsert.Split(Environment.NewLine));

        var expected = TextScenario(tag, toInsert);
        var actual = destination.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InsertSectionTextIoIsCompatibleWithMarkdownScenario()
    {
        var subheader = "My Subheader";
        var destination = new StringWriter();
        var headerTag = Markdown.SubHeader(subheader);
        var source = new StringReader(MarkdownScenario(subheader, toReplace));

        source.InsertSection(destination, headerTag, toInsert.Split(Environment.NewLine));

        var expected = MarkdownScenario(subheader, toInsert);
        var actual = destination.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task InsertSectionAsyncTextIoIsCompatibleWithMarkdownScenario()
    {
        var subheader = "My Subheader";
        var destination = new StringWriter();
        var headerTag = Markdown.SubHeader(subheader);
        var source = new StringReader(MarkdownScenario(subheader, toReplace));

        await source.InsertSectionAsync(destination, headerTag, toInsert.Split(Environment.NewLine));

        var expected = MarkdownScenario(subheader, toInsert);
        var actual = destination.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InsertSectionInFileReplacesTextAtSpecifiedLineMark()
    {
        var source = new FileInfo("Test.md");
        try {

            File.WriteAllText(source.FullName, TextScenario(tag, toReplace));

            source.InsertSection(tag, toInsert.Split(Environment.NewLine));

            var expected = TextScenario(tag, toInsert);
            var actual = File.ReadAllText(source.FullName);
            Assert.Equal(expected, actual);
        }
        finally {
            source.Delete();
        }
    }

    [Fact]
    public void InsertSectionInFilesAreCleanedUpOnError()
    {
        var source = new FileInfo("Test.md");
        var tempFileName = $"{source.FullName}.tmp";
        try {

            File.WriteAllText(source.FullName, TextScenario(tag, toReplace));
            IEnumerable<String> failingLines = FailingLines("This is a test.");

            Assert.Throws<InvalidOperationException>(() => source.InsertSection(tag, failingLines));

            Assert.False(File.Exists(tempFileName), "Temporary file was not cleaned up.");
        }
        finally {
            if (File.Exists(tempFileName)) {
                File.Delete(tempFileName);
            }
            source.Attributes = FileAttributes.Normal;
            source.Delete();
        }
    }

    [Fact]
    public async Task InsertSectionInFileAsyncReplacesTextAtSpecifiedLineMark()
    {
        var source = new FileInfo("AsyncTest.md");
        try {

            await File.WriteAllTextAsync(source.FullName, TextScenario(tag, toReplace));

            await source.InsertSectionAsync(tag, toInsert.Split(Environment.NewLine));

            var expected = TextScenario(tag, toInsert);
            var actual = await File.ReadAllTextAsync(source.FullName);
            Assert.Equal(expected, actual);
        }
        finally {
            source.Delete();
        }
    }

    [Fact]
    public async Task InsertSectionAsyncInFilesAreCleanedUpOnError()
    {
        var source = new FileInfo("Test.md");
        var tempFileName = $"{source.FullName}.tmp";
        try {

            File.WriteAllText(source.FullName, TextScenario(tag, toReplace));
            IEnumerable<String> failingLines = FailingLines("This is a test.");

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.InsertSectionAsync(tag, failingLines));

            Assert.False(File.Exists(tempFileName), "Temporary file was not cleaned up.");
        }
        finally {
            if (File.Exists(tempFileName)) {
                File.Delete(tempFileName);
            }
            source.Attributes = FileAttributes.Normal;
            source.Delete();
        }
    }

    [Fact]
    public async Task InsertSectionAsyncCanBeCancelled()
    {
        const Int32 cancelAtLine = 12;
        const String lineToInsert = "This will be inserted.";
        var source = new StringReader(TextScenario(tag, "This will be replaced."));
        using var cts = new CancellationTokenSource();
        var destination = new StringWriter();
        var lotsOfLines = TriggerCancellation(InfiniteLinesOf(lineToInsert), cancelAtLine, cts);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await source.InsertSectionAsync(destination, tag, lotsOfLines, cts.Token));

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

    static IEnumerable<String> FailingLines(String message)
    {
        yield return "Just some singular line.";
        throw new InvalidOperationException(message);
    }

    static String TextScenario(in LineTag tag, String insert)
    {
        return $"""
                # Sample
                Hello World!
                ## Sub Sample
                This be intro.
                {tag.Start}
                {insert}
                {tag.End}
                and this be outro.

                """;
    }

    static String MarkdownScenario(String subsectionTitle, String subsection)
    {
        return $"""
                # Header
                
                Hello Markdown!
                ## {subsectionTitle}
                {subsection}
                ## Some Sub-Header Title

                and this be some other section.

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
