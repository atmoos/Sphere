using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Async;

public static class Extensions
{
    public static IEnumerable<Task<T>> OrderByCompletion<T>(this IEnumerable<Task<T>> tasks) => new OrderByCompletion<T>(tasks.ToArray());
    public static async IAsyncEnumerable<T> AsAsync<T>(this IEnumerable<Task<T>> tasks, [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var task in tasks.OrderByCompletion()) {
            token.ThrowIfCancellationRequested();
            yield return await task.ConfigureAwait(false);
        }
    }
}
