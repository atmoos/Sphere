using Atmoos.Sphere.Text;

namespace Atmoos.Sphere.Test.Text;

public sealed class ExtensionsTest
{
    private static readonly (Int32, String) zero = (0, "Anything");

    [Fact]
    public void ToPluralStringOnZeroReturnsAnEmptyString()
    {
        var none = 0.ToString("apple", "apples");

        Assert.Equal((0, String.Empty), none);
    }

    [Fact]
    public void ToPluralStringOnOneReturnsTheSingularForm()
    {
        const Int32 one = 1;
        const String singular = "apple";
        var none = one.ToString(singular, "apples");

        Assert.Equal((one, $"one {singular}"), none);
    }

    [Fact]
    public void ToPluralStringOnMoreThanOneReturnsThePluralForm()
    {
        const Int32 many = 2;
        const String plural = "apples";
        var none = many.ToString("apple", plural);

        Assert.Equal((many, $"{many} {plural}"), none);
    }

    [Fact]
    public void CombineTwoZerosReturnsAnEmptyString()
    {
        var actual = zero.Combine(zero);

        Assert.Equal(String.Empty, actual);
    }

    [Fact]
    public void CombineZeroWithNoneZeroReturnsNoneZeroText()
    {
        const String expected = "Some right hand text";
        var nonZero = (-3, expected);

        var actual = zero.Combine(nonZero);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CombineNoneZeroWithZeroReturnsNoneZeroText()
    {
        const String expected = "Some left hand text";
        var nonZero = (-3, expected);

        var actual = nonZero.Combine(zero);

        Assert.Equal(expected, actual);
    }


    [Fact]
    public void CombineTwoNonZerosAndCombinesTheTexts()
    {
        const String leftText = "some left hand text";
        const String rightText = "some right hand text";
        var left = (2, leftText);
        var right = (-3, rightText);

        var actual = left.Combine(right);

        Assert.Equal($"{leftText} and {rightText}", actual);
    }
}

