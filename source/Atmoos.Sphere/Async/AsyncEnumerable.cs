using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Atmoos.Sphere.Collections;

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
        => FromEnvelope<T>((u, _) => envelope(u), cancellationTimeout);

    /// <summary>
    /// Takes a cancellable <paramref name="envelope"/> task and transforms updates within the envelope into an asynchronous "stream" of items.
    /// When cancellation is triggered, the "stream" awaits successful cancellation to occur.
    /// </summary>
    /// <param name="envelope">The envelope to transform.</param>
    /// <typeparam name="T">The type of update items.</typeparam>
    public static async IAsyncEnumerable<T> FromEnvelope<T>(CancellableEnvelope<T> envelope, TimeSpan cancellationTimeout = default, [EnumeratorCancellation] CancellationToken token = default)
    {
        var queue = new ConcurrentQueue<T>();
        var incoming = new TaskCompletionSource<Int32>();
        var envelopeTask = envelope(Listen, token);
        while (!envelopeTask.IsCompleted && !token.IsCancellationRequested) {
            using (token.Register(() => incoming.TrySetCanceled())) {
                var some = await Task.WhenAny(envelopeTask, incoming.Task).ConfigureAwait(None);
                Interlocked.Exchange(ref incoming, new TaskCompletionSource<Int32>());
                await some.ConfigureAwait(None);
            }
            foreach (var item in queue.Consume()) {
                yield return item;
            }
        }
        if (token.IsCancellationRequested && !envelopeTask.IsCompleted) {
            await envelopeTask.With(cancellationTimeout, CancellationToken.None).ConfigureAwait(None);
            yield break;
        }
        foreach (var item in queue) {
            yield return item;
        }
        await envelopeTask.ConfigureAwait(None);

        void Listen(T item)
        {
            queue.Enqueue(item);
            incoming.TrySetResult(0);
        }
    }
}
