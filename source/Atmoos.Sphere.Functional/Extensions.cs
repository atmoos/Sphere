namespace Atmoos.Sphere.Functional;

public static class Extensions
{
    public static Result<T> Join<T>(this Result<Result<T>> composite)
        where T : notnull => composite.SelectMany(m => m);
    public static Result<TResult> SelectMany<T, U, TResult>(this Result<T> value, Func<T, Result<U>> selector, Func<T, U, TResult> join)
        where T : notnull where U : notnull where TResult : notnull
            => value.SelectMany(t => selector(t).Select(u => join(t, u)));

    extension<T, U>(Result<T>)
        where T : notnull
        where U : notnull
    {
        public static Result<U> operator |(Result<T> left, Func<T, U> operand) => left switch {
            Success<T> success => operand(success),
            Failure<T> failure => failure.Propagate<U>(),
            _ => throw new InvalidOperationException($"Unexpected result type: {left.GetType()}")
        };
        public static Result<U> operator |(Result<T> left, Func<T, Result<U>> operand) => left switch {
            Success<T> success => operand(success),
            Failure<T> failure => failure.Propagate<U>(),
            _ => throw new InvalidOperationException($"Unexpected result type: {left.GetType()}")
        };
    }
}

