using System.Runtime.CompilerServices;

using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Async;

public static class Extensions
{
    public static async IAsyncEnumerable<T> AsAsync<T>(this IEnumerable<Task<T>> tasks, [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var task in tasks.OrderByCompletion()) {
            token.ThrowIfCancellationRequested();
            yield return await task.ConfigureAwait(None);
        }
    }

    public static async Task With(this Task task, TimeSpan timeout, CancellationToken token = default)
    {
        var any = await Task.WhenAny(task, Task.Delay(timeout, token)).ConfigureAwait(None);
        await any.ConfigureAwait(None);
    }

    public static IEnumerable<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> tasks)
    {
        var materialisedTasks = tasks.ToList();
        if (materialisedTasks.Count is 0 or 1) {
            return materialisedTasks;
        }
        return OrderByCompletion(materialisedTasks, materialisedTasks.Select(_ => new TaskCompletionSource<T>()).ToArray());

        static IEnumerable<Task<T>> OrderByCompletion(List<Task<T>> tasks, TaskCompletionSource<T>[] completions)
        {
            Int32 index = -1;
            foreach (var task in tasks) {
                Register(task);
            }
            return completions.Select(c => c.Task);

            void Set(T result) => completions[Interlocked.Increment(ref index)].TrySetResult(result);
            void SetEx(Exception result) => completions[Interlocked.Increment(ref index)].TrySetException(result);

            async void Register(Task<T> task)
            {
                try {
                    Set(await task.ConfigureAwait(false));
                }
                catch (Exception e) {
                    SetEx(e);
                }
            }
        }
    }
}
