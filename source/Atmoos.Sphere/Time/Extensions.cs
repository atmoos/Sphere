using System.Diagnostics;
using System.Runtime.CompilerServices;

using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Time;

public static class Extensions
{
    public static IAsyncEnumerable<TimeSpan> AsStream(this TimeSpan interval, CancellationToken token = default)
    {
        return Stream(interval > TimeSpan.Zero ? interval : throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval strictly greater zero required"), token);

        static async IAsyncEnumerable<TimeSpan> Stream(TimeSpan interval, [EnumeratorCancellation] CancellationToken token)
        {
            TimeSpan next = TimeSpan.Zero;
            Stopwatch timer = Stopwatch.StartNew();

            while (!token.IsCancellationRequested) {
                yield return next;
                var elapsed = timer.Elapsed;
                next += interval * (1 + (Int32)((elapsed - next) / interval));
                await Task.Delay(next - elapsed, token).ConfigureAwait(None);
            }
        }
    }
}
