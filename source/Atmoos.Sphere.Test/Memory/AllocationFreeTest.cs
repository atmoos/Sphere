using Atmoos.Sphere.Memory;

namespace Atmoos.Sphere.Test.Memory;

public sealed class AllocationFreeTest
{
    [Fact]
    public void Item_ReturnsSameInstanceForReferenceType()
    {
        var first = AllocationFree<Sample>.Item;
        var second = AllocationFree<Sample>.Item;

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.IsType<Sample>(first);
    }

    [Fact]
    public void Item_ReturnsSameInstanceForInterfaceImplementation()
    {
        var first = AllocationFree<IShape, Circle>.Item;
        var second = AllocationFree<IShape, Circle>.Item;

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.IsType<IShape>(first, exactMatch: false);
        Assert.IsType<Circle>(first);
    }

    private sealed class Sample;

    private interface IShape;

    private sealed class Circle : IShape;
}
