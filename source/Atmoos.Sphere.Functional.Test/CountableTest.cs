namespace Atmoos.Sphere.Functional;

public sealed class CountableTest
{
    [Fact]
    public void CountableInterfaceUsesTheGenericGetEnumerableMethod()
    {
        var countable = new Countable(1, 2, 3);

        Assert.NotEmpty(countable); // This uses IEnumerable.GetEnumerator(), which is implemented on ICountable itself.
    }
}

file sealed class Countable(params Int32[] values) : ICountable<Int32>
{
    public Int32 Count => values.Length;
    public IEnumerator<Int32> GetEnumerator() => ((IEnumerable<Int32>)values).GetEnumerator();
}
