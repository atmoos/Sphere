namespace Atmoos.Sphere.Functional;

public static class Extensions
{
    public static Maybe<T> Join<T>(this Maybe<Maybe<T>> composite)
        where T : notnull => composite.SelectMany(m => m);
    public static Result<T> Join<T>(this Result<Result<T>> composite)
        where T : notnull => composite.SelectMany(m => m);
    public static Maybe<TResult> SelectMany<T, U, TResult>(this Maybe<T> value, Func<T, Maybe<U>> selector, Func<T, U, TResult> join)
        where T : notnull where U : notnull where TResult : notnull
            => value.SelectMany(t => selector(t).Select(u => join(t, u)));
    public static Result<TResult> SelectMany<T, U, TResult>(this Result<T> value, Func<T, Result<U>> selector, Func<T, U, TResult> join)
        where T : notnull where U : notnull where TResult : notnull
            => value.SelectMany(t => selector(t).Select(u => join(t, u)));
}

