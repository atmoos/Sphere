using Atmoos.Sphere.Time;

namespace Atmoos.Sphere.Test.Time.Extensions;

public class SynchronousTimeSeriesTest
{
    [Fact]
    public void UsingIntervalOfZeroThrowsAnArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TimeSpan.Zero.TimeSeries());
    }

    [Fact]
    public void UsingNegativeIntervalThrowsAnArgumentOutOfRangeException()
    {
        TimeSpan interval = TimeSpan.FromMilliseconds(-5);
        Assert.Throws<ArgumentOutOfRangeException>(() => interval.TimeSeries());
    }

    [Fact]
    public void TimeSeriesIsLinearDespiteNegligibleWallClockIncrement()
    {
        const Int32 count = 24;
        const Int32 wallClockCount = 2 * count;
        var interval = TimeSpan.FromSeconds(4);
        var constantWallClock = Enumerable.Repeat(TimeSpan.Zero, wallClockCount);
        var expected = Enumerable.Range(0, count).Select(i => (i * interval, TimeSpan.Zero)).ToList();

        var actual = interval.TimeSeries(constantWallClock).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TimeSeriesCompensatesClockFreezes()
    {
        var interval = TimeSpan.FromHours(1);
        var wallClock = ((Double[])[0, 0.5, 1, 1.5, /* freeze */ 4, 4.5, 5, 5.5, 6, 6.5]).Select(TimeSpan.FromHours).ToArray();
        var expected = ((Double[])[0, 1, 2, 5, 6]).Select(TimeSpan.FromHours).ToArray();

        var actualTimes = interval.TimeSeries(wallClock).Take(wallClock.Length / 2).ToList();

        var actual = actualTimes.Select(time => time.next).ToList();
        Assert.Equal(expected, actual);
    }


    [Fact]
    public void TimeSeriesEvensOutJitteryWallClock()
    {
        const Int32 count = 100;
        const Double minutes = 2;
        const Double noiseLevel = 2;
        var interval = TimeSpan.FromMinutes(minutes);
        var wallClock = AddNoise(Enumerable.Range(0, 2 * count).Select(i => i * minutes / 2d), noiseLevel).Select(TimeSpan.FromMinutes).ToArray();
        var expected = Enumerable.Range(0, count).Select(i => minutes * TimeSpan.FromMinutes(i)).ToArray();

        var actualTimes = interval.TimeSeries(wallClock).Take(count).ToList();

        var actual = actualTimes.Select(time => time.next).ToList();
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<Double> AddNoise(IEnumerable<Double> values, Double noise)
    {
        var random = new Random(42); // have repeatable noise
        return values.Select(value => value + (random.NextDouble() - 0.5) * noise);
    }
}
