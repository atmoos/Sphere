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
    public static void Deconstruct<T>(this Success<(T, T)> value, out T left, out T right) => (left, right) = ((T, T))value;
    public static void Deconstruct<T>(this Success<(T, T, T)> value, out T a, out T b, out T c) => (a, b, c) = ((T, T, T))value;
    public static void Deconstruct<T>(this Success<(T, T, T, T)> value, out T a, out T b, out T c, out T d) => (a, b, c, d) = ((T, T, T, T))value;
}

