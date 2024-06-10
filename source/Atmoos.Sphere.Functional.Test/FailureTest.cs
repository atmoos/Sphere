namespace Atmoos.Sphere.Functional.Test;

public sealed class FailureTest
{
    [Fact]
    public void ToStringOnFailureShowsTheErrorMessage()
    {
        String expected = "Oh my, what a pity!";
        Result<Int32[]> failure = Result.Failure<Int32[]>(expected);

        Assert.Contains(expected, failure.ToString());
    }

    [Fact]
    public void FailureCompareFalseWhenSeparateInstances()
    {
        String sameErrorMessage = "Oh no!";
        Result<Int32> left = Result.Failure<Int32>(sameErrorMessage);
        Result<Int32> right = Result.Failure<Int32>(sameErrorMessage);

        Assert.NotEqual(left, right);
    }

    [Fact]
    public void FailureComparesFalseWhenComparedTowardSuccess()
    {
        Result<Int32> success = 42;
        Result<Int32> failure = Result.Failure<Int32>("Bang!");

        Assert.False(failure.Equals(success));
    }

    [Fact]
    public void FailureComparesEqualOnlyWhenTheErrorMessagesArePropagated()
    {
        Result<Int32> failure = Result.Failure<Int32>("Bang!");
        Result<Int32> otherFailureSameMessages = failure.Select(_ => 42); // Propagates the error message.

        Assert.True(failure.Equals(otherFailureSameMessages));
        Assert.NotSame(failure, otherFailureSameMessages);
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
    public void ExitFromFailureThrows()
    {
        String expectedError = "No, not today.";
        Result<Int32> result = Result.Failure<Int32>(expectedError);

        var exception = Assert.Throws<InvalidDataException>(() => result.Exit(m => new InvalidDataException(m)));
        Assert.Contains(expectedError, exception.Message);
    }

    [Fact]
    public void CountIndicatesCorrectNumberOfErrors()
    {

        Result<Int32> result = Result.Failure<Int32>("First") + "Second" + "Third" + "Fourth";

        Failure<Int32> actualErrors = Assert.IsType<Failure<Int32>>(result);
        Assert.Equal(4, actualErrors.Count);
    }

    [Fact]
    public void ErrorsAreEmittedInLiFoOrder()
    {
        const String firstError = "first error";
        const String secondError = "second error";
        const String thirdError = "third error";
        String[] expectedErrors = [thirdError, secondError, firstError];

        Result<Int32> result = Result.Failure<Int32>(firstError) + secondError + thirdError;

        Failure<Int32> actualErrors = Assert.IsType<Failure<Int32>>(result);
        Assert.Equal(expectedErrors, actualErrors);
    }
}
