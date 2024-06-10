namespace Atmoos.Sphere.Functional.Test;

// See: https://wiki.haskell.org/Typeclassopedia#Laws
public sealed class ResultTest : IFunctorLaws<String>
{
    private const String errorOnAverage = "Sequence contains no elements";
    [Fact]
    public void TheIdentityFunctionHasNoEffect()
    {
        Result<String> result = "Hello, World!";

        Assert.Equal(result, result.Select(Identity));
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void CompositionIsPreserved(String value)
    {
        Result<String> result = value;

        Assert.Equal(result.Select(Length).Select(IsEven), result.Select(s => IsEven(Length(s))));
    }

    [Fact]
    public void FailureHasHigherPrecedenceThanSuccess()
    {
        var errorMessage = "An error occurred";
        Result<Int32> success = 42;
        Result<Int32> result = success + errorMessage;

        var failure = Assert.IsType<Failure<Int32>>(result);
        String[] expectedErrors = [errorMessage];
        Assert.Equal(expectedErrors, failure);
    }

    [Theory]
    [MemberData(nameof(PositiveNumbers))]
    public void ResultIsCompatibleWithQuerySyntax(Int32[] values)
    {
        var result = from average in Average(values)
                     from sqrt in Sqrt(average)
                     select sqrt + average;

        Double numericResult = Assert.IsType<Success<Double>>(result);
        Assert.Equal(Math.Sqrt(values.Average()) + values.Average(), numericResult, 2e-12);
    }

    [Theory]
    [MemberData(nameof(FailingExamples))]
    public void ResultCapturesFailure(Int32[] values, String expectedError)
    {
        var result = from average in Average(values)
                     from sqrt in Sqrt(average)
                     select sqrt + average;

        String[] expectedErrors = [expectedError];
        IEnumerable<String> actualErrors = Assert.IsType<Failure<Double>>(result);
        Assert.Equal(expectedErrors, actualErrors);
    }

    [Fact]
    public void ErrorsAreEmittedInLiFoOrder()
    {
        const String firstError = "first error";
        const String secondError = "second error";
        const String thirdError = "third error";
        String[] expectedErrors = [thirdError, secondError, firstError];

        Result<Int32> result = Result.Failure<Int32>(firstError) + secondError + thirdError;

        IEnumerable<String> actualErrors = Assert.IsType<Failure<Int32>>(result);
        Assert.Equal(expectedErrors, actualErrors);
    }

    [Fact]
    public void ResultOrIsLeftAssociativeOnSuccesses()
    {
        Int32 expectedValue = 42;
        Result<Int32> left = expectedValue;
        Result<Int32> right = 24;

        Int32 actualValue = Assert.IsType<Success<Int32>>(left | right);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ResultOrSelectsSuccessRegardlessOfOrder()
    {
        Int32 expectedValue = -12;
        Result<Int32> failure = Result.Failure<Int32>("As long as one succeeds, we're good.");
        Result<Int32> success = expectedValue;

        Int32 actualFailSuccess = Assert.IsType<Success<Int32>>(failure | success);
        Int32 actualSuccessFail = Assert.IsType<Success<Int32>>(success | failure);
        Assert.Equal(actualFailSuccess, actualSuccessFail);
        Assert.Equal(expectedValue, actualFailSuccess);
    }

    [Fact]
    public void ResultOrCombinesFailures()
    {
        String firstError = "something went wrong";
        String secondError = "oh my!";

        Result<Int32> left = Result.Failure<Int32>(firstError);
        Result<Int32> right = Result.Failure<Int32>(secondError);

        String[] expectedMessage = [firstError, "-- + --", secondError];
        IEnumerable<String> actualError = Assert.IsType<Failure<Int32>>(left | right);
        Assert.Equal(expectedMessage, actualError);
    }

    [Fact]
    public void ResultAndCombinesResultIntoTuplesOnSuccess()
    {
        Int32 leftValue = 42;
        Int32 rightValue = 24;
        Result<Int32> left = leftValue;
        Result<Int32> right = rightValue;
        Result<(Int32, Int32)> sum = left & right;

        (Int32 left, Int32 right) actual = Assert.IsType<Success<(Int32, Int32)>>(sum);
        Assert.Equal((leftValue, rightValue), actual);
    }

    [Fact]
    public void ResultAndSelectsFailureRegardlessOfOrder()
    {
        String expectedError = "Can't cope with any error.";
        Result<Int32> failure = Result.Failure<Int32>(expectedError);
        Result<Int32> success = -32;

        String[] expectedErrors = [expectedError];
        IEnumerable<String> actualFailSuccess = Assert.IsType<Failure<(Int32, Int32)>>(failure & success);
        IEnumerable<String> actualSuccessFail = Assert.IsType<Failure<(Int32, Int32)>>(success & failure);
        Assert.Equal(actualFailSuccess, actualSuccessFail);
        Assert.Equal(expectedErrors, actualFailSuccess);
    }

    [Fact]
    public void ResultAndCombinesFailures()
    {
        String firstError = "something went wrong";
        String secondError = "oh my!";

        Result<Int32> left = Result.Failure<Int32>(firstError);
        Result<Int32> right = Result.Failure<Int32>(secondError);

        String[] expectedMessage = [firstError, "-- + --", secondError];
        IEnumerable<String> actualError = Assert.IsType<Failure<Int32>>(left | right);
        Assert.Equal(expectedMessage, actualError);
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
    public void ValueFromFailureIsTheFallbackValue()
    {
        Int32 fallbackValue = 42;
        Result<Int32> result = Result.Failure<Int32>("Nope.");

        Int32 actualValue = result.Value(() => fallbackValue);
        Assert.Equal(fallbackValue, actualValue);
    }

    [Fact]
    public void ExitFromSuccessIsTheComputedValue()
    {
        Int32 expectedValue = -3;
        Result<Int32> result = expectedValue;

        Int32 actualValue = result.Exit(m => new InvalidDataException(m));
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ExitFromFailureThrows()
    {
        String expectedError = "No, not today.";
        Result<Int32> result = Result.Failure<Int32>(expectedError);

        var exception = Assert.Throws<InvalidDataException>(() => result.Exit(m => new InvalidDataException(m)));
        Assert.Contains(expectedError, exception.Message);
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
    public void SuccessOfHomogenousTupleCanBeDeconstructed()
    {
        (String left, String right) expected = ("Hello", "World");
        Result<(String, String)> result = expected;

        Success<(String, String)> success = Assert.IsType<Success<(String, String)>>(result);
        var (actualLeft, actualRight) = success;
        Assert.Equal(expected, (actualLeft, actualRight));
    }

    [Fact]
    public void SuccessOfHomogenousTripleCanBeDeconstructedWhenUsingValueMethod()
    {
        (Int32, Int32, Int32) expected = (3, 1, 2);
        Result<(Int32, Int32, Int32)> result = expected;

        Success<(Int32, Int32, Int32)> success = Assert.IsType<Success<(Int32, Int32, Int32)>>(result);
        var (actualLeft, actualCenter, actualRight) = success;
        Assert.Equal(expected, (actualLeft, actualCenter, actualRight));
    }

    [Fact]
    public void SuccessOfHomogenousQuadrupleCanBeDeconstructedWhenUsingValueMethod()
    {
        (Single, Single, Single, Single) expected = (1, 2, 3, 4);
        Result<(Single, Single, Single, Single)> result = expected;

        Success<(Single, Single, Single, Single)> success = Assert.IsType<Success<(Single, Single, Single, Single)>>(result);
        var (a, b, c, d) = success;
        Assert.Equal(expected, (a, b, c, d));
    }

    [Fact]
    public void SuccessOfAnyTupleCanBeDeconstructedWhenUsingValueMethod()
    {
        (Int32 left, String right) expected = (1, "Hello");
        Result<(Int32, String)> result = expected;

        Success<(Int32, String)> success = Assert.IsType<Success<(Int32, String)>>(result);
        var (actualLeft, actualRight) = success.Value();
        Assert.Equal(expected, (actualLeft, actualRight));
    }

    private static Result<Double> Average(IEnumerable<Int32> values)
        => Result.From<Double, InvalidOperationException>(values.Average, _ => errorOnAverage);
    private static Result<Double> Sqrt(Double value) => value switch {
        < 0 => Result.Failure<Double>($"Cannot calculate the square root of a negative number: {value}"),
        Double.NaN => Result.Failure<Double>("Cannot calculate the square root of NaN"),
        Double.PositiveInfinity => Result.Failure<Double>("Cannot calculate the square root of Infinity"),
        _ => Math.Sqrt(value),
    };

    public static TheoryData<String> Values() => new()
    {
         "uneven string",
         "An even string",
    };
    public static TheoryData<Int32[]> PositiveNumbers() => new() {
        new Int32[] {1,  3, 5, 22, 42, 100, 21},
        new Int32[] {1, 2, 3, 5, 22, 42, 100, 21},
    };
    public static TheoryData<Int32[], String> FailingExamples() => new() {
        {Array.Empty<Int32>(), errorOnAverage},
        {new Int32[] {0, 3, -12, 4,-231}, "Cannot calculate the square root of a negative number: -47.2"},
    };

    private class Base { }
    private class Derived : Base { }
}

