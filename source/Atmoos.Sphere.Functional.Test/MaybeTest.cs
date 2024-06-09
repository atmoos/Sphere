using Atmoos.Sphere.Functional;

namespace Atmoos.Sphere.Test.Functional;

public sealed class MaybeTest
{
    public static Maybe<Int32> Add(Maybe<Int32> left, Int32 right) => left switch {
        Just<Int32> value => value + right,
        _ => left
    };
}
