using System.Runtime.CompilerServices;

using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Async;

public static class Extensions
{
    public static IEnumerable<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> tasks) => new OrderByCompletion<T>(tasks.ToArray());
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
}
