namespace Atmoos.Sphere.Functional;

public interface IUnit<T> {    /* marker interface*/}

public interface IUnit<TReturn, T> : IUnit<T>
    where TReturn : notnull, IUnit<TReturn, T>
    where T : notnull
{
    static abstract implicit operator TReturn(T value);
}
