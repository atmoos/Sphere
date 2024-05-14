using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Async;

/// <summary>
/// An envelope function that supports cancellation.
/// </summary>
/// <typeparam name="T">The type of update items.</typeparam>
/// <param name="updates">The delegate with which updates are emitted.</param>
/// <param name="token">The cancellation token.</param>
/// <returns>The enveloping task of all the updates.</returns>
public delegate Task CancellableEnvelope<out T>(Action<T> updates, CancellationToken token);

/// <summary>
/// An envelope function that does not support cancellation.
/// </summary>
/// <typeparam name="T">The type of update items.</typeparam>
/// <param name="updates">The delegate with which updates are emitted.</param>
/// <returns>The enveloping task of all the updates.</returns>
public delegate Task UnCancellableEnvelope<out T>(Action<T> updates);

/// <summary>
/// Transforms some envelop task represented by a long running single task which produces incremental updates of items.
/// </summary>
public static class AsyncEnumerable
{
    /// <summary>
    /// Takes a cancellable <paramref name="envelope"/> task and transforms updates within the envelope into an asynchronous "stream" of items.
    /// When cancellation is triggered, the "stream" awaits successful cancellation to occur.
    /// </summary>
    /// <param name="envelope">The envelope to transform.</param>
    /// <typeparam name="T">The type of update items.</typeparam>
    public static IAsyncEnumerable<T> FromEnvelope<T>(CancellableEnvelope<T> envelope)
        => new AsyncGenerator<T>(envelope, TimeSpan.Zero);

    /// <summary>
    /// Takes an un-cancellable <paramref name="envelope"/> task and transforms updates within the envelope into an asynchronous "stream" of items.
    /// When cancellation is triggered, will await the envelop to complete with the given <paramref name="cancellationTimeout"/>.
    /// </summary>
    /// <param name="envelope">The un-cancellable envelope to transform.</param>
    /// <param name="cancellationTimeout">Use this optional timeout to provide a timeout to wait on the envelope. Not providing a value can lead to weird behaviour.</param>
    /// <typeparam name="T">The type of update items.</typeparam>
    /// <remarks>
    /// A word of caution: Although the async enumerable will respect cancellation, the resulting envelop may stay still run silently in the background for an indeterminate duration.
    /// </remarks>
    public static IAsyncEnumerable<T> FromEnvelope<T>(UnCancellableEnvelope<T> envelope, TimeSpan cancellationTimeout = default)
        => new AsyncGenerator<T>((u, _) => envelope(u), cancellationTimeout);

    private sealed class AsyncGenerator<T> : IAsyncEnumerable<T>
    {
        private readonly TimeSpan cancellationTimeout;
        private readonly CancellableEnvelope<T> envelope;
        internal AsyncGenerator(CancellableEnvelope<T> envelope, TimeSpan cancellationTimeout)
            => (this.envelope, this.cancellationTimeout) = (envelope, cancellationTimeout);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this.envelope, this.cancellationTimeout, cancellationToken);

        private sealed class Enumerator : IAsyncEnumerator<T>
        {
            private readonly Task envelope;
            private readonly TimeSpan cancellationTimeout;
            private readonly ConcurrentQueue<T> queue;
            private readonly CancellationToken token;
            private TaskCompletionSource<Int32> next;
            public T Current
            {
                get {
                    if (this.queue.TryDequeue(out var current)) {
                        return current;
                    }
                    throw new InvalidOperationException("There are no more elements.");
                }
            }

            public Enumerator(CancellableEnvelope<T> envelope, TimeSpan cancellationTimeout, CancellationToken token)
            {
                this.token = token;
                this.queue = new ConcurrentQueue<T>();
                this.next = new TaskCompletionSource<Int32>();
                this.cancellationTimeout = cancellationTimeout;
                this.envelope = MonitorEnvelope(envelope, token);
            }

            public async ValueTask DisposeAsync()
            {
                this.next.TrySetResult(0);
                if (this.token.IsCancellationRequested && this.cancellationTimeout > TimeSpan.Zero) {
                    await (await Task.WhenAny(this.envelope, Task.Delay(this.cancellationTimeout)).ConfigureAwait(None)).ConfigureAwait(None);
                    return;
                }
                await this.envelope.ConfigureAwait(None);
            }

            public async ValueTask<Boolean> MoveNextAsync()
            {
                if (this.token.IsCancellationRequested) {
                    return false;
                }
                if (!this.queue.IsEmpty) {
                    return true;
                }
                if (this.envelope.IsCompleted || this.envelope.IsCanceled || this.envelope.IsFaulted) {
                    return false;
                }
                var current = this.next;
                using (this.token.Register(() => current.TrySetCanceled())) {
                    await current.Task.ConfigureAwait(None);
                }
                Interlocked.Exchange(ref this.next, new TaskCompletionSource<Int32>());
                return !this.queue.IsEmpty;
            }

            private async Task MonitorEnvelope(CancellableEnvelope<T> envelope, CancellationToken cancellation)
            {
                try {
                    await envelope(Listen, cancellation).ConfigureAwait(None);
                }
                catch (Exception e) {
                    this.next.TrySetResult(0);
                    ExceptionDispatchInfo.Capture(e).Throw();
                }

                void Listen(T item)
                {
                    this.queue.Enqueue(item);
                    this.next.TrySetResult(0);
                }
            }
        }
    }
}
