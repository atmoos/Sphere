namespace Atmoos.Sphere.Functional;

public interface IFunctor<T>
{
    T Exit<TError>(Func<String, TError> onErrors)
        where TError : Exception;
    T Value(Func<T> fallback);
}
