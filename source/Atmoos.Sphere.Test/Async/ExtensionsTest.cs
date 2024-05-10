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
        const Int32 count = 11;
        const Int32 indexOfFaultyTask = count / 2 + 1;
        List<Task<Int32>> unorderedTasks = new List<Task<Int32>>(count);
        Int32 index;
        for (index = 0; index < indexOfFaultyTask; ++index) {
            unorderedTasks.Add(IdentifiableDelay(30 + index, 1));
        }
        unorderedTasks.Add(DelayedFailingTask(50));
        for (index = indexOfFaultyTask + 1; index < count; ++index) {
            unorderedTasks.Add(IdentifiableDelay(70 + index, 1));
        }
        unorderedTasks.Reverse();
        index = 0;
        String throws = "Throw";
        String noThrows = "No Throw";
        List<String> actualNews = [];
        List<String> expectedNews = [];
        foreach (Task<Int32> orderedTask in unorderedTasks.OrderByCompletion()) {
            expectedNews.Add(noThrows);
            try {
                await orderedTask;
                actualNews.Add(noThrows);
            }
            catch (InvalidOperationException) {
                actualNews.Add(throws);
            }
            index++;
        }
        expectedNews[indexOfFaultyTask] = throws;
        Assert.Equal(expectedNews, actualNews);
    }

    [Fact]
    public async Task InCompletionOrder_OnTimeOrderedTasks_PropagatesCancellation()
    {
        const Int32 count = 11;
        const Int32 indexOfCancelingTask = count / 2 + 1;
        using (CancellationTokenSource cancellation = new CancellationTokenSource()) {
            List<Task<Int32>> unorderedTasks = new List<Task<Int32>>(count);
            Int32 index;
            for (index = 0; index < indexOfCancelingTask; ++index) {
                unorderedTasks.Add(IdentifiableDelay(30 + index, 1));
            }
            unorderedTasks.Add(DelayedCancellingTask(200, cancellation.Token));
            for (index = indexOfCancelingTask + 1; index < count; ++index) {
                unorderedTasks.Add(IdentifiableDelay(70 + index, 1));
            }
            unorderedTasks.Reverse();
            index = 0;
            cancellation.CancelAfter(50);
            String cancels = "cancelled";
            String completes = "completed";
            List<String> actualNews = [];
            List<String> expectedNews = [];
            foreach (Task<Int32> orderedTask in unorderedTasks.OrderByCompletion()) {
                expectedNews.Add(completes);
                try {
                    await orderedTask;
                    actualNews.Add(completes);
                }
                catch (TaskCanceledException) {
                    actualNews.Add(cancels);
                }
                index++;
            }
            expectedNews[indexOfCancelingTask] = cancels;
            Assert.Equal(expectedNews, actualNews);
        }
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

