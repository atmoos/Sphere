namespace Atmoos.Sphere.Functional;

/// <summary>
/// Conditionally unwrap a value.
/// </summary>
public interface IUnwrap<TResult>
    where TResult : notnull
{
    /// <summary>   
    /// Unwrap by throwing an exception when there is no value to exit with.
    /// </summary>
    TResult Exit<TError>(Func<String, TError> onErrors)
        where TError : Exception;

    /// <summary>   
    /// Unwrap by providing a fallback value should there be no value.
    /// </summary>
    TResult Value(Func<TResult> fallback);
}

/// <summary>
/// Is guaranteed to unwrap a value.
/// </summary>
public interface IUnwrap<TUnit, T>
    where TUnit : notnull, IUnwrap<TUnit, T>, IUnit<T>
    where T : notnull
{
    /// <summary>   
    /// There is guaranteed a value to obtain.
    /// </summary>
    static abstract implicit operator T(TUnit value);
}
