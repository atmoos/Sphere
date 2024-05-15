using Atmoos.Sphere.Async;

namespace Atmoos.Sphere.Test.Async;

// These tests are copied over from an old project of mine.
// They are ok for now, but could do with a refactoring :-)
public class ExtensionsTest
{
    [Fact]
    public async Task InCompletionOrder_OnEmptyTasks_ReturnsEmpty()
    {
        List<Int32> actualIds = [];
        List<Task<Int32>> unorderedTasks = [];
        foreach (Task<Int32> orderedTask in unorderedTasks.OrderByCompletion()) {
            actualIds.Add(await orderedTask);
        }
        Assert.Empty(actualIds);
    }

    [Fact]
    public async Task InCompletionOrder_OnSingularTasks_ReturnsSameSingularTask()
    {
        const Int32 singularId = 3;
        List<Int32> actualIds = [];
        List<Task<Int32>> actualTasks = [];
        List<Task<Int32>> expected = [IdentifiableDelay(singularId)];
        foreach (Task<Int32> orderedTask in expected.OrderByCompletion()) {
            actualIds.Add(await orderedTask);
            actualTasks.Add(orderedTask);
        }
        Assert.Equal([singularId], actualIds);
        Assert.Same(expected[0], actualTasks[0]);
    }

    [Fact]
    public async Task InCompletionOrder_OnTimeOrderedTasks_ReturnsTasksInCompletionOrder()
    {
        const Int32 count = 9;
        List<Int32> actualIds = new List<Int32>(count);
        List<Int32> expectedIds = new List<Int32>(count);
        List<Task<Int32>> unorderedTasks = new List<Task<Int32>>(count);
        for (Int32 id = 0; id < count; ++id) {
            expectedIds.Add(id);
            unorderedTasks.Add(IdentifiableDelay(id));
        }
        unorderedTasks.Reverse();
        foreach (Task<Int32> orderedTask in unorderedTasks.OrderByCompletion()) {
            actualIds.Add(await orderedTask);
        }
        Assert.Equal(expectedIds, actualIds);
    }

    [Fact]
    public async Task InCompletionOrder_OnTimeOrderedTasks_PropagatesExceptions()
    {
        const Int32 count = 7;
        const Int32 delayScaling = 30;
        const Int32 indexOfFaultyTask = 2 * count / 3 + 1;
        var (throws, noThrows) = ("Throw", "No Throw");
        var unorderedTasks = Enumerable.Range(0, count).Select(id => IdentifiableDelay(id, delayScaling)).ToArray();
        unorderedTasks[indexOfFaultyTask] = DelayedFailingTask(delayScaling * (indexOfFaultyTask + 1));

        List<String> actualNews = [];
        List<String> expectedNews = [];
        foreach (Task<Int32> orderedTask in unorderedTasks.Shuffle().OrderByCompletion()) {
            expectedNews.Add(noThrows);
            try {
                await orderedTask;
                actualNews.Add(noThrows);
            }
            catch (InvalidOperationException) {
                actualNews.Add(throws);
            }
        }
        expectedNews[indexOfFaultyTask] = throws;
        Assert.Equal(expectedNews, actualNews);
    }

    [Fact]
    public async Task InCompletionOrder_OnTimeOrderedTasks_PropagatesCancellation()
    {
        const Int32 count = 11;
        const Int32 delayScaling = 20;
        const Int32 indexOfCancelingTask = 3 * count / 5 + 1;
        var (cancels, completes) = ("cancelled", "completes");
        using var cancellation = new CancellationTokenSource();
        var unorderedTasks = Enumerable.Range(0, count).Select(id => IdentifiableDelay(id, delayScaling)).ToArray();
        unorderedTasks[indexOfCancelingTask] = DelayedCancellingTask(300, cancellation.Token);

        List<String> actualNews = [];
        List<String> expectedNews = [];
        foreach (var (index, orderedTask) in unorderedTasks.Shuffle().OrderByCompletion().Select((t, i) => (i, t))) {
            if (index == indexOfCancelingTask) {
                cancellation.Cancel();
            }
            expectedNews.Add(completes);
            try {
                await orderedTask;
                actualNews.Add(completes);
            }
            catch (TaskCanceledException) {
                actualNews.Add(cancels);
            }
        }
        expectedNews[indexOfCancelingTask] = cancels;
        Assert.Equal(expectedNews, actualNews);
    }

    [Fact]
    public async Task EnumerableTasksAsAsyncEnumerable_AreOrderedByCompletion()
    {
        const Int32 count = 9;
        var random = new Random();
        var unorderedTasks = Enumerable.Range(0, count).OrderBy(_ => random.Next()).Select(id => IdentifiableDelay(id, scaling: 24));
        var actualIds = new List<Int32>(count);

        await foreach (var orderedTask in unorderedTasks.AsAsync()) {
            actualIds.Add(orderedTask);
        }

        Assert.Equal(Enumerable.Range(0, count), actualIds);
    }

    private static async Task<Int32> IdentifiableDelay(Int32 id, Int32 scaling = 16)
    {
        await Task.Delay(scaling * (id + 1)).ConfigureAwait(false);
        return id;
    }

    private static async Task<Int32> DelayedFailingTask(Int32 delayMs)
    {
        await Task.Delay(delayMs).ConfigureAwait(false);
        throw new InvalidOperationException("Foo!");
    }
    private static async Task<Int32> DelayedCancellingTask(Int32 delayMs, CancellationToken cancellationToken)
    {
        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
        return delayMs;
    }
}

