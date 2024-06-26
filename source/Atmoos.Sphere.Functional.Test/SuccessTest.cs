namespace Atmoos.Sphere.Functional.Test;

public sealed class SuccessTest
{
    [Fact]
    public void ToStringOnSuccessShowsTheValue()
    {
        String expected = "Oh what a success!";
        Result<String> success = expected;

        Assert.Contains(expected, success.ToString());
    }

    [Fact]
    public void SuccessComparesTrueWhenValuesAreEqual()
    {
        Result<Int32> left = 42;
        Result<Int32> right = 42;

        Assert.Equal(left, right);
    }

    [Fact]
    public void SuccessComparesFalseWhenValuesAreNotEqual()
    {
        Result<Int32> left = 42;
        Result<Int32> right = 21;

        Assert.NotEqual(left, right);
    }

    [Fact]
    public void SuccessComparesFalseWhenComparedTowardFailure()
    {
        Result<Int32> success = 42;
        Result<Int32> failure = Result.Failure<Int32>("Bang!");

        Assert.False(success.Equals(failure));
    }

    [Fact]
    public void SuccessCanImplicitlyAssignAndUnwrapStructTypes()
    {
        Result<CancellationToken> expected = CancellationToken.None;

        CancellationToken actual = Assert.IsType<Success<CancellationToken>>(expected);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SuccessCanImplicitlyAssignAndUnwrapClassTypes()
    {
        var expected = new List<Int32>();
        Result<List<Int32>> result = expected;

        List<Int32> actual = Assert.IsType<Success<List<Int32>>>(result);
        Assert.Same(expected, actual);
    }

    [Fact]
    public void SuccessCanImplicitlyAssignAndUnwrapToBaseTypes()
    {
        var expected = new Derived();
        Result<Derived> result = expected;

        Base actual = Assert.IsType<Success<Derived>>(result);
        Assert.Same(expected, actual);
    }

    [Fact]
    // See why we need Success() & Value() here
    // §15.10.4 https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#15104-conversion-operators
    public void SuccessCanAssignAndUnwrapInterfaceTypesUsingMethods()
    {
        IList<Int32> expected = [1, 2, 3];
        Result<IList<Int32>> result = Result.Success(expected);

        IList<Int32> actual = Assert.IsType<Success<IList<Int32>>>(result).Value();
        Assert.Same(expected, actual);
    }

    [Fact]
    public void SuccessOfTupleCanBeDeconstructed()
    {
        (Int32 left, String right) expected = (1, "World");
        Result<(Int32, String)> result = expected;

        Success<(Int32, String)> success = Assert.IsType<Success<(Int32, String)>>(result);
        var (actualLeft, actualRight) = success;
        Assert.Equal(expected, (actualLeft, actualRight));
    }

    [Fact]
    public void SuccessOfTripleCanBeDeconstructedWhenUsingValueMethod()
    {
        (Int32, Int16, Int64) expected = (3, 1, 2);
        Result<(Int32, Int16, Int64)> result = expected;

        Success<(Int32, Int16, Int64)> success = Assert.IsType<Success<(Int32, Int16, Int64)>>(result);
        var (actualLeft, actualCenter, actualRight) = success;
        Assert.Equal(expected, (actualLeft, actualCenter, actualRight));
    }

    [Fact]
    public void SuccessOfQuadrupleCanBeDeconstructedWhenUsingValueMethod()
    {
        (Single, Double, Byte, UInt16) expected = (1, 2, 3, 4);
        Result<(Single, Double, Byte, UInt16)> result = expected;

        Success<(Single, Double, Byte, UInt16)> success = Assert.IsType<Success<(Single, Double, Byte, UInt16)>>(result);
        var (a, b, c, d) = success;
        Assert.Equal(expected, (a, b, c, d));
    }

    [Fact]
    public void SuccessOfAnyTupleCanBeDeconstructedWhenUsingValueMethod()
    {
        (Int32, String, Int64, Boolean, Byte, Double) expected = (1, "ridiculous", 3, true, 4, 5.0);
        Result<(Int32, String, Int64, Boolean, Byte, Double)> result = expected;

        Success<(Int32, String, Int64, Boolean, Byte, Double)> success = Assert.IsType<Success<(Int32, String, Int64, Boolean, Byte, Double)>>(result);
        var (a, b, c, d, e, f) = success.Value();
        Assert.Equal(expected, (a, b, c, d, e, f));
    }

    [Fact]
    public void ValueFromSuccessIsNotTheFallbackValue()
    {
        Int32 expectedValue = 42;
        Int32 fallbackValue = expectedValue - 1;
        Result<Int32> result = expectedValue;

        Int32 actualValue = result.Value(() => fallbackValue);
        Assert.Equal(expectedValue, actualValue);
        Assert.NotEqual(fallbackValue, actualValue);
    }

    [Fact]
    public void ExitFromSuccessIsTheComputedValue()
    {
        Int32 expectedValue = -3;
        Result<Int32> result = expectedValue;

        Int32 actualValue = result.Exit(m => new InvalidDataException(m));
        Assert.Equal(expectedValue, actualValue);
    }

    private class Base { }
    private class Derived : Base { }
}
