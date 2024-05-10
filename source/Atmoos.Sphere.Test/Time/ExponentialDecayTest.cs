using System.Diagnostics;
using Atmoos.Sphere.Time;

namespace Atmoos.Sphere.Test.Time;

public class ExponentialDecayTest
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CannotCreateDecayWithIntervalSmallerOrEqualZero(Int32 msInterval)
    {
        var interval = TimeSpan.FromMilliseconds(msInterval);

        Assert.Throws<ArgumentOutOfRangeException>(() => ExponentialDecay.StartNew(interval));
    }
    [Theory]
    [InlineData(1)]
    [InlineData(1.0 - 3e-15)]
    public void CannotCreateDecayWithDecayFactorSmallerOrEqualOne(Double decayFactor)
    {
        var interval = TimeSpan.FromMilliseconds(4);

        Assert.Throws<ArgumentOutOfRangeException>(() => ExponentialDecay.StartNew(interval, decayFactor));
    }

    [Fact]
    public async Task ExponentialDecayDecaysExponentially()
    {
        const Double factor = 2;
        const Int32 iterations = 5;
        var timeout = TimeSpan.FromMilliseconds(2);
        var actualStates = new List<TimeSpan>(iterations);
        var expectedStates = new List<TimeSpan>(iterations);
        var decay = ExponentialDecay.StartNew(timeout, decayFactor: factor);
        for (Int32 exponent = 0; exponent < iterations; ++exponent) {
            actualStates.Add(await decay);
            expectedStates.Add(Math.Pow(factor, exponent) * timeout);
        }

        Assert.Equal(expectedStates, actualStates);
    }

    [Fact]
    public async Task ExponentialDecayAwaitsApproximatelyAccurately()
    {
        const Int64 expected = 80; // ms
        var decay = ExponentialDecay.StartNew(TimeSpan.FromMilliseconds(expected));
        var timer = Stopwatch.StartNew();

        await decay;

        var actual = timer.ElapsedMilliseconds;

        Assert.InRange(actual, 0.9 * expected, 1.8 * expected);
    }

    [Fact]
    public async Task ExponentialDecayCanBeCancelled()
    {
        var decay = new ExponentialDecay(TimeSpan.FromSeconds(2));
        using var cts = new CancellationTokenSource(millisecondsDelay: 40);
        var timeout = decay.Start(cancellation: cts.Token);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await timeout);
    }
}
