using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Time;

public sealed class ExponentialDecay(in TimeSpan timeout, Double decayFactor = ExponentialDecay.defaultDecay)
{
    const Double defaultDecay = 2d;
    private readonly TimeSpan timeout = Guard(timeout);
    private readonly Double decayFactor = Guard(decayFactor);

    public static Decay StartNew(in TimeSpan timeout, Double decayFactor = defaultDecay, ConfigureAwaitOptions awaitOption = ConfigureAwaitOptions.None, CancellationToken token = default)
        => new(Guard(timeout), Guard(decayFactor), awaitOption, token);
    public Decay Start(ConfigureAwaitOptions awaitOption = ConfigureAwaitOptions.None, CancellationToken cancellation = default)
        => new(in this.timeout, this.decayFactor, awaitOption, cancellation);

    public struct Decay
    {
        private Int32 exponent = -1;
        private readonly Double decay;
        private readonly TimeSpan timeout;
        private readonly CancellationToken token;
        private readonly ConfigureAwaitOptions awaitOption;
        public readonly Int32 Iteration => this.exponent;
        internal Decay(in TimeSpan timeout, Double decayFactor, ConfigureAwaitOptions awaitOptions, CancellationToken token)
        {
            this.token = token;
            this.timeout = timeout;
            this.decay = decayFactor;
            this.awaitOption = awaitOptions;
        }

        public ConfiguredTaskAwaitable<TimeSpan>.ConfiguredTaskAwaiter GetAwaiter()
        {
            return Delay(this.timeout * Math.Pow(this.decay, Interlocked.Increment(ref this.exponent)), this.token).ConfigureAwait(this.awaitOption).GetAwaiter();

            static async Task<TimeSpan> Delay(TimeSpan timeout, CancellationToken token)
            {
                await Task.Delay(timeout, token).ConfigureAwait(false);
                return timeout;
            }
        }
    }

    private static Double Guard(Double decayFactor) => Guard(decayFactor, 1d, nameof(decayFactor));

    private static TimeSpan Guard(TimeSpan timeout) => Guard(timeout, TimeSpan.Zero);

    private static T Guard<T>(T parameter, T lowerLimit, [CallerArgumentExpression(nameof(parameter))] String? name = default)
        where T : IComparable<T>
    {
        if (parameter.CompareTo(lowerLimit) > 0) {
            return parameter;
        }
        String msg = $"Exponential decay only works for a {name} strictly greater than {lowerLimit}. Received: {parameter}";
        throw new ArgumentOutOfRangeException(name, parameter, msg);
    }
}
