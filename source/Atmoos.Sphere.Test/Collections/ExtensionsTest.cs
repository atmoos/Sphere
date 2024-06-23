using System.Collections.Concurrent;
using Atmoos.Sphere.Collections;

namespace Atmoos.Sphere.Test.Collections;

public sealed class ExtensionsTest
{
    [Fact]
    public void TheDifferencesOnAnEmptyCollectionIsItselfEmpty()
    {
        var actual = Array.Empty<Double>().Differences().ToArray();

        Assert.Empty(actual);
    }

    [Fact]
    public void TheDifferencesOnCollectionWithOnlyOneElementIsEmpty()
    {
        var actual = new Double[] { Math.Tau }.Differences().ToArray();

        Assert.Empty(actual);
    }

    [Fact]
    public void TheDifferencesOnLinearFunctionAreConstant()
    {
        Int32 constant = 5;
        var linearFunction = Enumerable.Range(0, 10).Select(i => i * constant).ToArray();

        var actual = linearFunction.Differences().ToArray();

        Assert.All(actual, d => Assert.Equal(constant, d));
        Assert.Equal(linearFunction.Length - 1, actual.Length);
    }

    [Fact]
    public void TheDifferencesOnQuadraticFunctionAreLinearFunction()
    {
        const Int32 count = 20;
        Int32 constant = 2;
        var linearFunction = Enumerable.Range(0, count - 1).Select(i => 2 * i * constant + constant).ToArray();
        var quadraticFunction = Enumerable.Range(0, count).Select(i => i * i * constant).ToArray();

        var actual = quadraticFunction.Differences().ToArray();

        Assert.Equal(linearFunction, actual);
    }

    [Fact]
    public void TheSlidingWindowFunction()
    {
        const Int32 count = 20;
        var data = Enumerable.Range(0, count).ToArray();
        var expected = Enumerable.Range(0, count - 1).Select(i => $"{i}{i + 1}").ToArray();

        var actual = data.Window((a, b) => $"{a}{b}").ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConsumedQueueIsEmpty()
    {
        const Int32 count = 20;
        var queue = new Queue<Int32>(Enumerable.Range(0, count));

        var consumedElements = queue.Consume().ToArray();

        Assert.Empty(queue);
        Assert.Equal(count, consumedElements.Length);
    }

    [Fact]
    public void ConsumedProducerConsumerIsEmpty()
    {
        const Int32 count = 20;
        IProducerConsumerCollection<Int32> producerConsumer = new ConcurrentBag<Int32>(Enumerable.Range(0, count));

        var consumedElements = producerConsumer.Consume().ToArray();

        Assert.Empty(producerConsumer);
        Assert.Equal(count, consumedElements.Length);
    }
}
