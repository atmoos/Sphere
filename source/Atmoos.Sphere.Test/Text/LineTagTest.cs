using Atmoos.Sphere.Text;

using static Atmoos.Sphere.Text.LineTags;

namespace Atmoos.Sphere.Test.Text;

public sealed class LineTagTest
{
    [Fact]
    public void MarkdownHeaderIsCorrect()
    {
        var header = Markdown.Header("SomeName");
        var expected = new LineTag { Start = "# SomeName", End = "#" };
        Assert.Equal(expected, header);
    }

    [Fact]
    public void MarkdownSubHeaderIsCorrect()
    {
        var subHeader = Markdown.SubHeader("SomeName");
        var expected = new LineTag { Start = "## SomeName", End = "##" };
        Assert.Equal(expected, subHeader);
    }

    [Fact]
    public void MarkdownSubSubHeaderIsCorrect()
    {
        var subSubHeader = Markdown.SubSubHeader("SomeName");
        var expected = new LineTag { Start = "### SomeName", End = "###" };
        Assert.Equal(expected, subSubHeader);
    }

    [Fact]
    public void MarkdownCodeIsCorrect()
    {
        var code = Markdown.Code("zsh SomeScript");
        var expected = new LineTag { Start = "```zsh SomeScript", End = "```" };
        Assert.Equal(expected, code);
    }

    [Fact]
    public void CSharpLineCommentIsCorrect()
    {
        var code = CSharp.LineComment("SomeName");
        var expected = new LineTag { Start = "// SomeName", End = "// SomeName" };
        Assert.Equal(expected, code);
    }

    [Fact]
    public void CSharpBlockCommentIsCorrect()
    {
        var code = CSharp.BlockComment("SomeName");
        var expected = new LineTag { Start = "/* SomeName", End = "SomeName */" };
        Assert.Equal(expected, code);
    }

    [Fact]
    public void CSharpRegionIsCorrect()
    {
        var code = CSharp.Region("SomeName");
        var expected = new LineTag { Start = "#region SomeName", End = "#endregion" };
        Assert.Equal(expected, code);
    }

    [Fact]
    public void DeconstructingLineTagsDeconstructsIntoConstituents()
    {
        var expected = (start: "Start", end: "End");
        var mark = new LineTag { Start = expected.start, End = expected.end };

        var (actualStart, actualEnd) = mark;

        Assert.Equal(expected.start, actualStart);
        Assert.Equal(expected.end, actualEnd);
    }
}
