namespace Atmoos.Sphere.Functional;

public interface IUnit<T> { /* marker interface*/ }

/// <summary>
/// Monadic return or unit that wraps a value of <typeparamref name="T"/> in a monadic type <typeparamref name="TReturn"/> of itself.
/// </summary>
public interface IUnit<TReturn, T> : IUnit<T>
    where TReturn : notnull, IUnit<TReturn, T>
    where T : notnull
{
    static abstract implicit operator TReturn(T value);
}
