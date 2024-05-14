using System.Numerics;

namespace Atmoos.Sphere.Collections;

public delegate R Window<in T, out R>(T antecedent, T current);

public static class Extensions
{
    // ToDo: This could very well be moved to Atmoos.Sphere.Math...
    public static IEnumerable<T> Differences<T>(this IEnumerable<T> source)
        where T : ISubtractionOperators<T, T, T> => source.Window((a, b) => b - a);

    public static IEnumerable<R> Window<T, R>(this IEnumerable<T> source, Window<T, R> function)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext()) {
            yield break;
        }
        T antecedent = enumerator.Current;
        while (enumerator.MoveNext()) {
            yield return function(antecedent, antecedent = enumerator.Current);
        }
    }
}
