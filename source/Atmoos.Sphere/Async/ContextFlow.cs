using System.Runtime.CompilerServices;
using SyncContext = System.Threading.SynchronizationContext;

namespace Atmoos.Sphere.Async;

public sealed class ContextFlow : INotifyCompletion
{
    private readonly SyncContext? context;
    public Boolean IsCompleted => SyncContext.Current == this.context;
    private ContextFlow(SyncContext? context) => this.context = context;
    public static ContextFlow On(SyncContext? context) => new(context);
    public static ContextFlow Current() => new(SyncContext.Current);
    public ContextFlow GetAwaiter() => this;
    public void OnCompleted(Action continuation)
    {
        SyncContext? ctx;
        if ((ctx = this.context ?? SyncContext.Current) is not null) {
            ctx.Post(ContinueWith, continuation);
            return;
        }
        Task.Run(continuation);

        static void ContinueWith(Object? continuation) => (continuation as Action)?.Invoke();
    }
    public void GetResult() => SyncContext.SetSynchronizationContext(this.context);
}
