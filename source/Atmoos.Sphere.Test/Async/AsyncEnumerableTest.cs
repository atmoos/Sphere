using Atmoos.Sphere.Async;
using AsyncEnumerable = Atmoos.Sphere.Async.AsyncEnumerable;

namespace Atmoos.Sphere.Test.Async;


public sealed class AsyncEnumerableTest
{
    const String errorMessage = "This is some error message";
    const Int32 timeoutMs = 500;

    [Fact(Timeout = timeoutMs)]
    public async Task SequenceOrderIsPreserved()
    {
        var actual = new List<Int32>();
        Int32[] expected = [9, 1, 3, 2, -3, 5];

        await foreach (var value in AsyncEnumerable.FromEnvelope<Int32>((u, ct) => Envelope(expected, u, ct))) {
            actual.Add(value);
        }

        Assert.Equal(expected, actual);
    }

    [Fact(Timeout = timeoutMs)]
    public async Task FromEnvelopeIsCompatibleWithSynchronouslyImplementedEnvelopes()
    {
        var actual = new List<Int32>();
        Int32[] expected = [3, 1, 4, 5];

        await foreach (var value in AsyncEnumerable.FromEnvelope<Int32>((u, ct) => SynchronousEnvelopeAsVeryBadExample(expected, u, ct))) {
            actual.Add(value);
        }

        Assert.Equal(expected, actual);

        static Task SynchronousEnvelopeAsVeryBadExample<T>(IEnumerable<T> values, Action<T> update, CancellationToken _)
        {
            foreach (var value in values) {
                update(value);
            }
            return Task.CompletedTask;
        }
    }

    [Fact(Timeout = timeoutMs)]
    public async Task EmptyEnvelope_ResultsInEmptyAsyncSequence()
    {
        var actual = new List<Int32>();

        await foreach (var value in AsyncEnumerable.FromEnvelope<Int32>(EmptyEnvelope)) {
            actual.Add(value);
        }

        Assert.Empty(actual);

        static Task EmptyEnvelope(Action<Int32> _, CancellationToken __) => Task.FromResult(0);
    }

    [Fact(Timeout = timeoutMs)]
    public async Task EnumerationCanBeCancelled()
    {
        const Int32 cancelAfter = 4;
        const Int32 maxValues = 123;
        var actual = new List<Int32>();
        var values = Enumerable.Range(0, maxValues);
        using var cts = new CancellationTokenSource();
        await foreach (var value in AsyncEnumerable.FromEnvelope<Int32>((u, ct) => Envelope(values, u, ct)).WithCancellation(cts.Token)) {
            actual.Add(value);
            if (actual.Count == cancelAfter) {
                cts.Cancel();
            }
        }

        // As the Envelope method does not make use of the token, we expect no exception to occur!
        // and the enumeration to stop somewhere in-between cancelAfter & maxValues.
        Assert.InRange(actual.Count, cancelAfter, maxValues);
    }

    [Fact(Timeout = timeoutMs)]
    public async Task EnumerationCanBeCancelledOnUnCancellableEnvelope()
    {

        await Assert.ThrowsAsync<TaskCanceledException>(CancelsWithTaskCancelException);
        Assert.True(true);

        static async Task CancelsWithTaskCancelException()
        {
            var cancellationTimeout = TimeSpan.FromMilliseconds(21);
            using var cts = new CancellationTokenSource(cancellationTimeout);
            await foreach (var value in AsyncEnumerable.FromEnvelope<Int32>(UnCancellableNonEndingEnvelope, cancellationTimeout).WithCancellation(cts.Token)) {
                // We shouldn't even get here...
                Assert.Fail("Failed the sanity check.");
            }
        }

        static async Task UnCancellableNonEndingEnvelope(Action<Int32> _)
        { // Also, this envelope is empty, as the provided action is never invoked...
            var nonEndingTask = new TaskCompletionSource().Task;
            await nonEndingTask.ConfigureAwait(false);
        }
    }

    [Fact(Timeout = timeoutMs)]
    public async Task EnumerationPropagatesErrorsFromEnvelopeThatFailsImmediately()
    {
        var e = await Assert.ThrowsAsync<InvalidOperationException>(() => ConsumeEnvelope(ImmediatelyFailingEnvelope));
        Assert.Contains(errorMessage, e.Message);

        static Task ImmediatelyFailingEnvelope(Action<Int32> _, CancellationToken __)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    [Fact(Timeout = timeoutMs)]
    public async Task EnumerationPropagatesErrorsFromMiddleOfRunningEnvelope()
    {
        var e = await Assert.ThrowsAsync<InvalidOperationException>(() => ConsumeEnvelope(FailsInTheMiddle));
        Assert.Contains(errorMessage, e.Message);

        static async Task FailsInTheMiddle(Action<Int32> update, CancellationToken token)
        {
            const Int32 failAfter = 3;
            foreach (var item in Enumerable.Range(0, 2 * failAfter)) {
                if (item > failAfter) {
                    throw new InvalidOperationException(errorMessage);
                }
                await Task.Delay(item, token).ConfigureAwait(false);
                update(item);
            }
        }
    }

    [Fact(Timeout = timeoutMs)]
    public async Task EnumerationPropagatesErrorsFromTailEndOfLongRunningEnvelope()
    {
        var e = await Assert.ThrowsAsync<InvalidOperationException>(() => ConsumeEnvelope(FailsAtTheEnd));
        Assert.Contains(errorMessage, e.Message);

        static async Task FailsAtTheEnd(Action<Int32> update, CancellationToken token)
        {
            const Int32 failAfter = 12;
            foreach (var item in Enumerable.Range(0, 2 * failAfter)) {
                await Task.Delay(item, token).ConfigureAwait(false);
                update(item);
            }

            throw new InvalidOperationException(errorMessage);
        }
    }

    private static async Task Envelope<T>(IEnumerable<T> values, Action<T> update, CancellationToken _)
    {
        foreach (var value in values) {
            await Task.Yield();
            update(value);
        }
    }

    private static async Task ConsumeEnvelope(CancellableEnvelope<Int32> envelope)
    {
        await foreach (var value in AsyncEnumerable.FromEnvelope(envelope)) {
            GC.KeepAlive(value);
        }
    }
}
