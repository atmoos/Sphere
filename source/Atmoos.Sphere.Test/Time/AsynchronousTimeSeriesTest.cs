using Atmoos.Sphere.Time;
using Atmoos.Sphere.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using static System.TimeSpan;
using static Atmoos.Sphere.Time.Extensions;

namespace Atmoos.Sphere.Test.Time.Extensions;

public class AsynchronousTimeSeriesTest
{
    [Fact]
    public void PassingANegativeInterval_ToTheConstructor_ThrowsAnArgumentOutOfRangeException()
    {
        TimeSpan negativeInterval = FromMilliseconds(-5);
        Assert.Throws<ArgumentOutOfRangeException>(() => negativeInterval.AsyncTimeSeries());
    }

    [Fact]
    public void PassingAnIntervalOfZero_ToTheConstructor_ThrowsAnArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Zero.AsyncTimeSeries());
    }

    [Fact]
    public async Task Cancellation_ThrowsTaskCanceledException()
    {
        var count = 0;
        const Int32 throwAt = 3;
        var interval = FromMilliseconds(2);
        using var source = new CancellationTokenSource(200);
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await foreach (TimeSpan timeStamp in interval.AsyncTimeSeries(source.Token)) {
                if (count++ > throwAt) {
                    source.Cancel();
                }
                if (count > 3 * throwAt) {
                    throw new InvalidOperationException("Cancellation did not occur.");
                }
            }
        });
    }

    [Fact]
    public async Task CompletionsOccurAtExpectedIntervals()
    {
        var tol = new RelativeTolerance(0.05);
        TimeSpan interval = FromMilliseconds(60);
        var expectedIntervals = new Double[] { 3, 1.5, 1, 2 }.Select(d => interval * d).ToArray();
        var wallClock = expectedIntervals.Select((d, i) => (next: i * interval + d, wall: i * interval)).ToArray();

        var timeStamps = await RunAgainstWallClock(wallClock);

        var actualIntervals = timeStamps.Prepend((Zero, wallClock: Zero)).Window((a, b) => b.wallClock - a.wallClock).ToArray();
        Assert.Equal(expectedIntervals, actualIntervals, tol);
    }

    [Fact]
    public async Task TimeStampsAreAccurateEnough()
    {
        TimeSpan interval = FromMilliseconds(60);
        var tol = new AbsoluteTolerance(FromMilliseconds(8));

        var timeStamps = await RunAgainstWallClock(interval.TimeSeries().Take(6));

        var actualTimestamps = timeStamps.Select(t => t.wallClock).ToArray();
        var expectedTimestamps = timeStamps.Select(t => t.timestamp).ToArray();
        Assert.Equal(expectedTimestamps, actualTimestamps, tol);
    }

    [Fact]
    public async Task FirstTimeStampIsZero()
    {
        TimeSpan interval = FromMilliseconds(2);
        await foreach (TimeSpan timeStamp in interval.AsyncTimeSeries()) {
            Assert.Equal(Zero, timeStamp);
            return;
        }
    }

    [Fact]
    public async Task LongExternalDelaysDoNotAffectValidityOfTimeStamp()
    {
        List<TimeSpan> actualTimeStamps = [];
        TimeSpan interval = FromMilliseconds(16);
        TimeSpan longDelay = FromMilliseconds(80);
        await foreach (TimeSpan timeStamp in AsyncTimeSeries(interval.TimeSeries().Take(2), CancellationToken.None)) {
            await Task.Delay(longDelay - interval / 2);
            actualTimeStamps.Add(timeStamp);
        }

        Assert.Equal([Zero, longDelay], actualTimeStamps);
    }

    private static async Task<List<(TimeSpan timestamp, TimeSpan wallClock)>> RunAgainstWallClock(IEnumerable<(TimeSpan next, TimeSpan wall)> clock)
    {
        var timer = Stopwatch.StartNew();
        var actual = new List<(TimeSpan, TimeSpan)>();
        await foreach (TimeSpan timestamp in AsyncTimeSeries(clock, CancellationToken.None)) {
            actual.Add((timestamp, timer.Elapsed));
        }
        return actual;
    }
}

file sealed class AbsoluteTolerance(TimeSpan tol) : IEqualityComparer<TimeSpan>
{
    public Boolean Equals(TimeSpan expected, TimeSpan actual)
    {
        var difference = expected >= actual ? (expected - actual) : (actual - expected);
        return difference <= tol;
    }

    public Int32 GetHashCode([DisallowNull] TimeSpan obj) => obj.GetHashCode();
}
file sealed class RelativeTolerance(Double tol) : IEqualityComparer<TimeSpan>
{
    public Boolean Equals(TimeSpan expected, TimeSpan actual)
    {
        var difference = expected >= actual ? (expected - actual) : (actual - expected);
        return difference / expected <= tol;
    }

    public Int32 GetHashCode([DisallowNull] TimeSpan obj) => obj.GetHashCode();
}
