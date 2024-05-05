using System.Collections;

namespace Atmoos.Sphere.Async;

internal sealed class OrderByCompletion<T>(Task<T>[] unorderedTasks) : IEnumerable<Task<T>>
{
    public IEnumerator<Task<T>> GetEnumerator()
    {
        if (unorderedTasks.Length is 0 or 1) {
            return ((IEnumerable<Task<T>>)unorderedTasks).GetEnumerator();
        }
        return new Completion(unorderedTasks).Tasks;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Completion
    {
        private Int32 index = -1;
        private readonly TaskCompletionSource<T>[] completions;
        public Completion(Task<T>[] tasks)
        {
            this.completions = Enumerable.Range(0, tasks.Length).Select(_ => new TaskCompletionSource<T>()).ToArray();
            foreach (var task in tasks) {
                Register(task);
            }
        }
        public IEnumerator<Task<T>> Tasks => this.completions.Select(c => c.Task).GetEnumerator();
        private void Set(T result) => this.completions[Interlocked.Increment(ref this.index)].TrySetResult(result);
        private void Set(Exception result) => this.completions[Interlocked.Increment(ref this.index)].TrySetException(result);
        private async void Register(Task<T> task)
        {
            try {
                Set(await task.ConfigureAwait(false));
            }
            catch (Exception e) {
                Set(e);
            }
        }
    }
}
