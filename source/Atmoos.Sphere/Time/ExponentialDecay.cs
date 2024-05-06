using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Time;

public sealed class ExponentialDecay
{
    const Double defaultDecay = 2d;
    private readonly Double decayFactor;
    private readonly TimeSpan timeout;
    public ExponentialDecay(in TimeSpan timeout, Double decayFactor = defaultDecay)
    {
        this.timeout = timeout;
        this.decayFactor = DecayFactorGuard(decayFactor);
    }
    public static Decay StartNew(in TimeSpan timeout, CancellationToken token = default, Double decayFactor = defaultDecay, Boolean continueOnCapturedContext = true)
        => new(timeout, DecayFactorGuard(decayFactor), token, continueOnCapturedContext);
    public Decay Start(CancellationToken token = default, Boolean continueOnCapturedContext = true)
        => new(this.timeout, this.decayFactor, token, continueOnCapturedContext);

    public struct Decay
    {
        private Int32 exponent = 0;
        private readonly Double decay;
        private readonly TimeSpan timeout;
        private readonly Boolean captureContext;
        private readonly CancellationToken token;
        public readonly Int32 Iteration => this.exponent;
        public readonly TimeSpan Current => this.timeout * Math.Pow(this.decay, this.exponent);
        internal Decay(in TimeSpan timeout, Double decayFactor, CancellationToken token, Boolean captureContext)
        {
            this.token = token;
            this.timeout = timeout;
            this.decay = decayFactor;
            this.captureContext = captureContext;
        }

        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            // Exponential decay, means exponentially growing timeouts.
            var timeout = this.timeout * Math.Pow(this.decay, this.exponent++);
            return Task.Delay(timeout, this.token).ConfigureAwait(this.captureContext).GetAwaiter();
        }
    }

    static Double DecayFactorGuard(Double decayFactor)
    {
        if (decayFactor > 1) {
            return decayFactor;
        }
        String msg = $"Exponential decay only works for decay factors strictly greater than one. Received: {decayFactor:g4}";
        throw new ArgumentOutOfRangeException(nameof(decayFactor), msg);
    }
}
