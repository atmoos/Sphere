using System.Collections;

namespace Atmoos.Sphere.Functional;

public interface ICountable<T> : IEnumerable<T>
{
    Int32 Count { get; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
