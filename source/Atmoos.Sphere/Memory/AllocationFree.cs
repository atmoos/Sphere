using System.Runtime.CompilerServices;

namespace Atmoos.Sphere.Memory;

public static class AllocationFree<TItem>
    where TItem : class, new()
{
    private static readonly TItem item = new();
    public static ref readonly TItem Item => ref item;
}

public static class AllocationFree<TReference, TItem>
    where TReference : class
    where TItem : class, TReference, new()
{
    private static readonly TReference item = new TItem();
    public static ref readonly TReference Item => ref item;
}
