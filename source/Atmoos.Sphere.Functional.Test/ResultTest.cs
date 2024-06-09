using System.Collections;

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

        Result<Int32> result = Result<Int32>.Failure(firstError) + secondError + thirdError;

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
        Result<Int32> failure = Result<Int32>.Failure("As long as one succeeds, we're good.");
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

        Result<Int32> left = Result<Int32>.Failure(firstError);
        Result<Int32> right = Result<Int32>.Failure(secondError);

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
        Result<Int32> failure = Result<Int32>.Failure(expectedError);
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

        Result<Int32> left = Result<Int32>.Failure(firstError);
        Result<Int32> right = Result<Int32>.Failure(secondError);

        String[] expectedMessage = [firstError, "-- + --", secondError];
        IEnumerable<String> actualError = Assert.IsType<Failure<Int32>>(left | right);
        Assert.Equal(expectedMessage, actualError);
    }

    private static Result<Double> Average(IEnumerable<Int32> values)
        => Result<Double>.From<InvalidOperationException>(values.Average, _ => errorOnAverage);
    private static Result<Double> Sqrt(Double value) => value switch {
        < 0 => Result<Double>.Failure($"Cannot calculate the square root of a negative number: {value}"),
        Double.NaN => Result<Double>.Failure("Cannot calculate the square root of NaN"),
        Double.PositiveInfinity => Result<Double>.Failure("Cannot calculate the square root of Infinity"),
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
}

