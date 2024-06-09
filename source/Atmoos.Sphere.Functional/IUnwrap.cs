namespace Atmoos.Sphere.Functional;

public interface IUnwrap<TResult>
    where TResult : notnull
{
    TResult Exit<TError>(Func<String, TError> onErrors)
        where TError : Exception;
    TResult Value(Func<TResult> fallback);
}

public interface IUnwrap<TUnit, T>
    where TUnit : notnull, IUnwrap<TUnit, T>, IUnit<T>
    where T : notnull
{
    static abstract implicit operator T(TUnit value);
}
