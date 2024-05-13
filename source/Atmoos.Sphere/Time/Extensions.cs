using System.Diagnostics;
using System.Runtime.CompilerServices;

using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Time;

public static class Extensions
{
    public static IEnumerable<TimeSpan> TimeStamps()
    {
        Stopwatch timer = Stopwatch.StartNew();
        while (true) {
            yield return timer.Elapsed;
        }
    }
    public static IEnumerable<(TimeSpan next, TimeSpan wallClock)> TimeSeries(this TimeSpan interval) => TimeSeries(interval, TimeStamps());
    internal static IEnumerable<(TimeSpan next, TimeSpan wallClock)> TimeSeries(this TimeSpan interval, IEnumerable<TimeSpan> wallClock)
    {
        return Series(interval > TimeSpan.Zero ? interval : throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval strictly greater zero required"), wallClock.GetEnumerator());

        static IEnumerable<(TimeSpan next, TimeSpan wallTime)> Series(TimeSpan interval, IEnumerator<TimeSpan> wallClock)
        {
            TimeSpan next = TimeSpan.Zero;
            while (wallClock.MoveNext()) {
                yield return (next, wallClock.Current);
                wallClock.MoveNext();
                next += interval * (((Int32)((wallClock.Current - next) / interval)) + 1);
            }
        }
    }
    public static IAsyncEnumerable<TimeSpan> AsyncTimeSeries(this TimeSpan interval, CancellationToken token = default) => AsyncTimeSeries(interval.TimeSeries(), token);
    internal static async IAsyncEnumerable<TimeSpan> AsyncTimeSeries(IEnumerable<(TimeSpan, TimeSpan)> intervals, [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var (next, wallTime) in intervals) {
            await Task.Delay(next - wallTime, token).ConfigureAwait(None);
            yield return next;
        }
    }
}
