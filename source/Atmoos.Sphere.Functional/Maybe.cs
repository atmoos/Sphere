namespace Atmoos.Sphere.Functional;

public abstract class Maybe<T> : IFunctor<T>, IEquatable<Maybe<T>>
    where T : notnull
{
    private static readonly Nothing<T> nothing = new();
    private protected Maybe() { }
    public abstract Maybe<T> Where(Func<T, Boolean> predicate);
    public abstract Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
        where TResult : notnull;
    public abstract Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> selector)
        where TResult : notnull;
    public abstract T Exit<TError>(Func<String, TError> onErrors) where TError : Exception;
    public abstract T Value(Func<T> fallback);
    public abstract Boolean Equals(Maybe<T>? other);
    public abstract override Int32 GetHashCode();
    public override Boolean Equals(Object? other) => other is Maybe<T> maybe && Equals(maybe);

    // Monadic return
    public static implicit operator Maybe<T>(T value) => new Just<T>(value);
    // Monadic left identity
    public static Maybe<T> operator |(Maybe<T> left, Maybe<T> right) => (left, right) switch {
        (Just<T>, _) => left,
        (_, Just<T>) => right,
        _ => left
    };

    internal static Maybe<T> Nothing => nothing;
}

public sealed class Just<T> : Maybe<T>
    where T : notnull
{
    private readonly T value;
    internal Just(T value) => this.value = value;
    public override Maybe<T> Where(Func<T, Boolean> predicate) => predicate(this.value) ? this : Nothing;
    public override Maybe<TResult> Select<TResult>(Func<T, TResult> selector) => selector(this.value);
    public override Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> selector) => selector(this.value);
    public override T Value(Func<T> fallback) => this.value;
    public override T Exit<TError>(Func<String, TError> onFailure) => this.value;
    public override Boolean Equals(Maybe<T>? other) => other is Just<T> just && this.value.Equals(just.value);
    public override Int32 GetHashCode() => this.value.GetHashCode();
    public override String? ToString() => this.value.ToString();

    public static implicit operator T(Just<T> value) => value.value;
}

public sealed class Nothing<T> : Maybe<T>
    where T : notnull
{
    private static readonly Int32 hash = typeof(Nothing<T>).GetHashCode();
    private static readonly String nothing = $"No {typeof(T).Name}";
    internal Nothing() { }
    public override Maybe<T> Where(Func<T, Boolean> predicate) => this;
    public override Maybe<TResult> Select<TResult>(Func<T, TResult> selector) => Maybe<TResult>.Nothing;
    public override Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> selector) => Maybe<TResult>.Nothing;
    public override T Value(Func<T> fallback) => fallback();
    public override T Exit<TError>(Func<String, TError> onNothing) => throw onNothing(nameof(Nothing<T>));
    public override Boolean Equals(Maybe<T>? other) => other is Nothing<T>;
    public override Int32 GetHashCode() => hash;
    public override String? ToString() => nothing;
}
