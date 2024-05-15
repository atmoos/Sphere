using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Atmoos.Sphere.Collections;

public interface IEnumerableConvenience<T> : IEnumerable<T>
{
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
